//   OData .NET Libraries ver. 5.6.3
//   Copyright (c) Microsoft Corporation
//   All rights reserved. 
//   MIT License
//   Permission is hereby granted, free of charge, to any person obtaining a copy of
//   this software and associated documentation files (the "Software"), to deal in
//   the Software without restriction, including without limitation the rights to use,
//   copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
//   Software, and to permit persons to whom the Software is furnished to do so,
//   subject to the following conditions:

//   The above copyright notice and this permission notice shall be included in all
//   copies or substantial portions of the Software.

//   THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//   FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//   COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//   IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//   CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace System.Data.Services.Client.Materialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Diagnostics;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using DSClient = System.Data.Services.Client;

    /// <summary>
    /// Use this class to materialize objects provided from an <see cref="ODataMessageReader"/>.
    /// </summary>
    internal class CollectionValueMaterializationPolicy : MaterializationPolicy
    {
        /// <summary> The materializer context. </summary>
        private readonly IODataMaterializerContext materializerContext;

        /// <summary> The complex value materialization policy. </summary>
        private ComplexValueMaterializationPolicy complexValueMaterializationPolicy;

        /// <summary> The primitive value materialization policy. </summary>
        private PrimitiveValueMaterializationPolicy primitiveValueMaterializationPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValueMaterializationPolicy" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="primitivePolicy">The primitive policy.</param>
        internal CollectionValueMaterializationPolicy(IODataMaterializerContext context, PrimitiveValueMaterializationPolicy primitivePolicy)
        {
            this.materializerContext = context;
            this.primitiveValueMaterializationPolicy = primitivePolicy;
        }

        /// <summary>
        /// Gets the complex value materialization policy.
        /// </summary>
        /// <value>
        /// The complex value materialization policy.
        /// </value>
        internal ComplexValueMaterializationPolicy ComplexValueMaterializationPolicy
        {
            get 
            {
                Debug.Assert(this.complexValueMaterializationPolicy != null, "complexValueMaterializationPolicy!= null");
                return this.complexValueMaterializationPolicy;
            }

            set 
            { 
                this.complexValueMaterializationPolicy = value; 
            }
        }

        /// <summary>
        /// Creates Collection instance of store Collection items.
        /// </summary>
        /// <param name="collectionProperty">ODataProperty instance representing the Collection as seen in the atom payload.</param>
        /// <param name="userCollectionType">CLR type of the Collection as defined by the user.</param>
        /// <returns>Newly created Collection instance. Never null.</returns>
        internal object CreateCollectionPropertyInstance(ODataProperty collectionProperty, Type userCollectionType)
        {
            Debug.Assert(collectionProperty != null, "collectionProperty != null");
            Debug.Assert(collectionProperty.Value != null, "Collection should have already been checked for nullness");
            Debug.Assert(userCollectionType != null, "userCollectionType != null");
            Debug.Assert(ClientTypeUtil.GetImplementationType(userCollectionType, typeof(ICollection<>)) != null, "Not a Collection - Collection types must implement ICollection<> interface.");
            Debug.Assert(
                !ClientTypeUtil.TypeIsEntity(ClientTypeUtil.GetImplementationType(userCollectionType, typeof(ICollection<>)).GetGenericArguments()[0], this.materializerContext.Model),
                "Not a Collection - Collections cannot contain entities");
            Debug.Assert(!(collectionProperty.Value is ODataFeed) && !(collectionProperty.Value is ODataEntry), "Collection properties should never materialized from entry or feed payload");

            ODataCollectionValue collectionValue = collectionProperty.Value as ODataCollectionValue;

            // get a ClientType instance for the Collection property. This determines what type will be used later when creating the actual Collection instance
            ClientTypeAnnotation collectionClientType = this.materializerContext.ResolveTypeForMaterialization(userCollectionType, collectionValue.TypeName);

            return this.CreateCollectionInstance(collectionClientType.EdmTypeReference as IEdmCollectionTypeReference, collectionClientType.ElementType, () => DSClient.Strings.AtomMaterializer_NoParameterlessCtorForCollectionProperty(collectionProperty.Name, collectionClientType.ElementTypeName));
        }

        /// <summary>
        /// Creates the collection instance.
        /// </summary>
        /// <param name="edmCollectionTypeReference">The edm collection type reference.</param>
        /// <param name="clientCollectionType">Type of the client collection.</param>
        /// <returns>New Collection Instance.</returns>
        internal object CreateCollectionInstance(IEdmCollectionTypeReference edmCollectionTypeReference, Type clientCollectionType)
        {
            Debug.Assert(edmCollectionTypeReference != null, "edmCollectionTypeReference!=null");
            Debug.Assert(clientCollectionType != null, "clientCollectionType!=null");
            return CreateCollectionInstance(edmCollectionTypeReference, clientCollectionType, () => DSClient.Strings.AtomMaterializer_MaterializationTypeError(clientCollectionType.FullName));
        }

        /// <summary>
        /// Applies collectionValue item to the provided <paramref name="collectionInstance"/>. 
        /// </summary>
        /// <param name="collectionProperty">Atom property containing materialized Collection items.</param>
        /// <param name="collectionInstance">Collection instance. Must implement ICollection&lt;T&gt; where T is either primitive or complex type (not an entity).</param>
        /// <param name="collectionItemType">Type of items in the Collection. Note: this could be calculated from collectionInstance but we already have it in upstream methods.</param>
        /// <param name="addValueToBackingICollectionInstance">Action called actually add a Collection item to <paramref name="collectionInstance" /></param>
        internal void ApplyCollectionDataValues(ODataProperty collectionProperty, object collectionInstance, Type collectionItemType, Action<object, object> addValueToBackingICollectionInstance)
        {
            Debug.Assert(collectionProperty != null, "property != null");
            Debug.Assert(collectionProperty.Value != null, "Collection should have already been checked for nullness");
            Debug.Assert(collectionInstance != null, "collectionInstance != null");
            Debug.Assert(WebUtil.IsCLRTypeCollection(collectionInstance.GetType(), this.materializerContext.Model), "collectionInstance must be a CollectionValue");
            Debug.Assert(
                ClientTypeUtil.GetImplementationType(collectionInstance.GetType(), typeof(ICollection<>)).GetGenericArguments()[0] == collectionItemType,
                "collectionItemType has to match the collectionInstance generic type.");
            Debug.Assert(!ClientTypeUtil.TypeIsEntity(collectionItemType, this.materializerContext.Model), "CollectionValues cannot contain entities");
            Debug.Assert(addValueToBackingICollectionInstance != null, "AddValueToBackingICollectionInstance != null");

            ODataCollectionValue collectionValue = collectionProperty.Value as ODataCollectionValue;
            this.ApplyCollectionDataValues(collectionValue.Items, collectionValue.TypeName, collectionInstance, collectionItemType, addValueToBackingICollectionInstance);

            collectionProperty.SetMaterializedValue(collectionInstance);
        }

        /// <summary>
        /// Applies the collection data values to a collection instance.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="wireTypeName">Name of the wire type.</param>
        /// <param name="collectionInstance">The collection instance.</param>
        /// <param name="collectionItemType">Type of the collection item.</param>
        /// <param name="addValueToBackingICollectionInstance">The add value to backing I collection instance.</param>
        internal void ApplyCollectionDataValues(IEnumerable items, string wireTypeName, object collectionInstance, Type collectionItemType, Action<object, object> addValueToBackingICollectionInstance)
        {
            Debug.Assert(collectionInstance != null, "collectionInstance != null");
            Debug.Assert(WebUtil.IsCLRTypeCollection(collectionInstance.GetType(), this.materializerContext.Model), "collectionInstance must be a CollectionValue");
            Debug.Assert(
                ClientTypeUtil.GetImplementationType(collectionInstance.GetType(), typeof(ICollection<>)).GetGenericArguments()[0] == collectionItemType,
                "collectionItemType has to match the collectionInstance generic type.");
            Debug.Assert(!ClientTypeUtil.TypeIsEntity(collectionItemType, this.materializerContext.Model), "CollectionValues cannot contain entities");
            Debug.Assert(addValueToBackingICollectionInstance != null, "AddValueToBackingICollectionInstance != null");

            // is the Collection not empty ?
            if (items != null)
            {
                bool isCollectionItemTypePrimitive = PrimitiveType.IsKnownNullableType(collectionItemType);

                var edmModel = this.materializerContext.Model;

                foreach (object item in items)
                {
                    if (item == null)
                    {
                        throw DSClient.Error.InvalidOperation(DSClient.Strings.Collection_NullCollectionItemsNotSupported);
                    }

                    ODataComplexValue complexValue = item as ODataComplexValue;

                    // Is it a Collection of primitive types?
                    if (isCollectionItemTypePrimitive)
                    {
                        // verify that the Collection does not contain complex type items
                        if (complexValue != null || item is ODataCollectionValue)
                        {
                            throw DSClient.Error.InvalidOperation(DSClient.Strings.Collection_ComplexTypesInCollectionOfPrimitiveTypesNotAllowed);
                        }

                        object materializedValue = this.primitiveValueMaterializationPolicy.MaterializePrimitiveDataValueCollectionElement(collectionItemType, wireTypeName, item);

                        addValueToBackingICollectionInstance(collectionInstance, materializedValue);
                    }
                    else
                    {
                        // verify that the Collection does not contain primitive values
                        if (complexValue == null)
                        {
                            throw DSClient.Error.InvalidOperation(DSClient.Strings.Collection_PrimitiveTypesInCollectionOfComplexTypesNotAllowed);
                        }

                        // Derived complex types are not supported. We create the base type here and we will fail later when trying to materialize the value of a
                        // property that is on the derived type but not on the base type (unless IgnoreMissingProperty setting is set to true). This is by design.
                        ClientTypeAnnotation complexType = edmModel.GetClientTypeAnnotation(edmModel.GetOrCreateEdmType(collectionItemType));
                        object complexInstance = this.CreateNewInstance(complexType.EdmTypeReference, complexType.ElementType);

                        // set properties with metarialized data values if there are any (note that for a payload that looks as follows <element xmlns="http://schemas.microsoft.com/ado/2007/08/dataservices"/> 
                        // and represents an item that is a complex type there are no properties to be set)
                        this.ComplexValueMaterializationPolicy.ApplyDataValues(complexType, complexValue.Properties, complexInstance);

                        addValueToBackingICollectionInstance(collectionInstance, complexInstance);
                    }
                }
            }
        }

        /// <summary>
        /// Creates Collection instance of store Collection items.
        /// </summary>
        /// <param name="edmCollectionTypeReference">The edm collection type reference.</param>
        /// <param name="clientCollectionType">Type of the client collection.</param>
        /// <param name="error">Error to throw.</param>
        /// <returns>
        /// Newly created Collection instance. Never null.
        /// </returns>
        private object CreateCollectionInstance(IEdmCollectionTypeReference edmCollectionTypeReference, Type clientCollectionType, Func<string> error)
        {
            Debug.Assert(clientCollectionType != null, "clientCollectionType != null");
            Debug.Assert(edmCollectionTypeReference != null, "edmCollectionTypeReference != null");
            Debug.Assert(error != null, "error != null");

            // DataServiceCollection cannot track non-entity types so it should not be used for storing primitive or complex types
            if (ClientTypeUtil.IsDataServiceCollection(clientCollectionType))
            {
                throw DSClient.Error.InvalidOperation(DSClient.Strings.AtomMaterializer_DataServiceCollectionNotSupportedForNonEntities);
            }

            try
            {
                return this.CreateNewInstance(edmCollectionTypeReference, clientCollectionType);
            }
#if PORTABLELIB
            catch (MissingMemberException ex)
#else
            catch (MissingMethodException ex)
#endif
            {
                throw DSClient.Error.InvalidOperation(error(), ex);
            }
        }
    }
}

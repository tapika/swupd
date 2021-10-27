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
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Diagnostics;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using DSClient = System.Data.Services.Client;

    /// <summary>
    /// Contains logic on how to materialize properties into an instance
    /// </summary>
    internal abstract class StructuralValueMaterializationPolicy : MaterializationPolicy
    {
        /// <summary> The collection value materialization policy. </summary>
        private CollectionValueMaterializationPolicy collectionValueMaterializationPolicy;

        /// <summary> The primitive property converter. </summary>
        private DSClient.SimpleLazy<PrimitivePropertyConverter> lazyPrimitivePropertyConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexValueMaterializationPolicy" /> class.
        /// </summary>
        /// <param name="materializerContext">The materializer context.</param>
        /// <param name="lazyPrimitivePropertyConverter">The lazy primitive property converter.</param>
        protected StructuralValueMaterializationPolicy(IODataMaterializerContext materializerContext, DSClient.SimpleLazy<PrimitivePropertyConverter> lazyPrimitivePropertyConverter)
        {
            Debug.Assert(materializerContext != null, "materializer!=null");
            this.MaterializerContext = materializerContext;
            this.lazyPrimitivePropertyConverter = lazyPrimitivePropertyConverter;
        }

        /// <summary>
        /// Gets the collection value materialization policy.
        /// </summary>
        /// <value>
        /// The collection value materialization policy.
        /// </value>
        protected internal CollectionValueMaterializationPolicy CollectionValueMaterializationPolicy
        {
            get
            {
                Debug.Assert(this.collectionValueMaterializationPolicy != null, "collectionValueMaterializationPolicy != null");
                return this.collectionValueMaterializationPolicy;
            }

            set
            {
                this.collectionValueMaterializationPolicy = value;
            }
        }

        /// <summary>
        /// Gets the primitive property converter.
        /// </summary>
        /// <value>
        /// The primitive property converter.
        /// </value>
        protected PrimitivePropertyConverter PrimitivePropertyConverter
        {
            get
            {
                Debug.Assert(this.lazyPrimitivePropertyConverter != null, "lazyPrimitivePropertyConverter != null");
                return this.lazyPrimitivePropertyConverter.Value;
            }
        }

        /// <summary>
        /// Gets the materializer context.
        /// </summary>
        /// <value>
        /// The materializer context.
        /// </value>
        protected IODataMaterializerContext MaterializerContext { get; private set; }

        /// <summary>Materializes a primitive value. No op for non-primitive values.</summary>
        /// <param name="type">Type of value to set.</param>
        /// <param name="property">Property holding value.</param>
        internal void MaterializePrimitiveDataValue(Type type, ODataProperty property)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(property != null, "atomProperty != null");

            if (!property.HasMaterializedValue())
            {
                object value = property.Value;
                object materializedValue = this.PrimitivePropertyConverter.ConvertPrimitiveValue(value, type);
                property.SetMaterializedValue(materializedValue);
            }
        }

        /// <summary>
        /// Applies the values of the specified <paramref name="properties"/> to a given <paramref name="instance"/>.
        /// </summary>
        /// <param name="type">Type to which properties will be applied.</param>
        /// <param name="properties">Properties to assign to the specified <paramref name="instance"/>.</param>
        /// <param name="instance">Instance on which values will be applied.</param>
        internal void ApplyDataValues(ClientTypeAnnotation type, IEnumerable<ODataProperty> properties, object instance)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(properties != null, "properties != null");
            Debug.Assert(instance != null, "instance != context");

            foreach (var p in properties)
            {
                this.ApplyDataValue(type, p, instance);
            }
        }

        /// <summary>Applies a data value to the specified <paramref name="instance"/>.</summary>
        /// <param name="type">Type to which a property value will be applied.</param>
        /// <param name="property">Property with value to apply.</param>
        /// <param name="instance">Instance on which value will be applied.</param>
        internal void ApplyDataValue(ClientTypeAnnotation type, ODataProperty property, object instance)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(property != null, "property != null");
            Debug.Assert(instance != null, "instance != null");

            var prop = type.GetProperty(property.Name, this.MaterializerContext.IgnoreMissingProperties);
            if (prop == null)
            {
                return;
            }

            // Is it a collection? (note: property.Properties will be null if the Collection is empty (contains no elements))
            if (prop.IsPrimitiveOrComplexCollection)
            {
                // Collections must not be null
                if (property.Value == null)
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.Collection_NullCollectionNotSupported(property.Name));
                }

                // This happens if the payload contain just primitive value for a Collection property
                if (property.Value is string)
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.Deserialize_MixedTextWithComment);
                }

                if (property.Value is ODataComplexValue)
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.AtomMaterializer_InvalidCollectionItem(property.Name));
                }

                // ODataLib already parsed the data and materialized all the primitive types. There is nothing more to materialize
                // anymore. Only complex type instance and collection instances need to be materialized, but those will be
                // materialized later on.
                // We need to materialize items before we change collectionInstance since this may throw. If we tried materializing
                // after the Collection is wiped or created we would leave the object in half constructed state.
                object collectionInstance = prop.GetValue(instance);
                if (collectionInstance == null)
                {
                    collectionInstance = this.CollectionValueMaterializationPolicy.CreateCollectionPropertyInstance(property, prop.PropertyType);

                    // allowAdd is false - we need to assign instance as the new property value
                    prop.SetValue(instance, collectionInstance, property.Name, false /* allowAdd? */);
                }
                else
                {
                    // Clear existing Collection
                    prop.ClearBackingICollectionInstance(collectionInstance);
                }

                this.CollectionValueMaterializationPolicy.ApplyCollectionDataValues(property, collectionInstance, prop.PrimitiveOrComplexCollectionItemType, prop.AddValueToBackingICollectionInstance);
            }
            else
            {
                object propertyValue = property.Value;
                ODataComplexValue complexValue = propertyValue as ODataComplexValue;
                if (propertyValue != null && complexValue != null)
                {
                    if (!prop.EdmProperty.Type.IsComplex())
                    {
                        // The error message is a bit odd, but it's compatible with V1.
                        throw DSClient.Error.InvalidOperation(DSClient.Strings.Deserialize_ExpectingSimpleValue);
                    }

                    // Complex type.
                    bool needToSet = false;

                    // Derived complex types are not supported. We create the base type here and we will fail later when trying to materialize the value of a
                    // property that is on the derived type but not on the base type (unless IgnoreMissingProperty setting is set to true). This is by design.
                    ClientEdmModel edmModel = this.MaterializerContext.Model;
                    ClientTypeAnnotation complexType = edmModel.GetClientTypeAnnotation(edmModel.GetOrCreateEdmType(prop.PropertyType));
                    object complexInstance = prop.GetValue(instance);
                    if (complexInstance == null)
                    {
                        complexInstance = this.CreateNewInstance(complexType.EdmTypeReference, complexType.ElementType);
                        needToSet = true;
                    }

                    this.MaterializeDataValues(complexType, complexValue.Properties, this.MaterializerContext.IgnoreMissingProperties);
                    this.ApplyDataValues(complexType, complexValue.Properties, complexInstance);

                    if (needToSet)
                    {
                        prop.SetValue(instance, complexInstance, property.Name, true /* allowAdd? */);
                    }
                }
                else
                {
                    this.MaterializePrimitiveDataValue(prop.NullablePropertyType, property);
                    prop.SetValue(instance, property.GetMaterializedValue(), property.Name, true /* allowAdd? */);
                }
            }
        }

        /// <summary>
        /// Materializes the primitive data values in the given list of <paramref name="values"/>.
        /// </summary>
        /// <param name="actualType">Actual type for properties being materialized.</param>
        /// <param name="values">List of values to materialize.</param>
        /// <param name="ignoreMissingProperties">
        /// Whether properties missing from the client types should be ignored.
        /// </param>
        /// <remarks>
        /// Values are materialized in-place withi each <see cref="ODataProperty"/>
        /// instance.
        /// </remarks>
        internal void MaterializeDataValues(ClientTypeAnnotation actualType, IEnumerable<ODataProperty> values, bool ignoreMissingProperties)
        {
            Debug.Assert(actualType != null, "actualType != null");
            Debug.Assert(values != null, "values != null");

            foreach (var odataProperty in values)
            {
                if (odataProperty.Value is ODataStreamReferenceValue)
                {
                    continue;
                }

                string propertyName = odataProperty.Name;

                var property = actualType.GetProperty(propertyName, ignoreMissingProperties); // may throw
                if (property == null)
                {
                    continue;
                }

                // we should throw if the property type on the client does not match with the property type in the server
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(property.PropertyType))
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.AtomMaterializer_InvalidEntityType(property.EntityCollectionItemType ?? property.PropertyType));
                }

                if (property.IsKnownType)
                {
                    // Note: MaterializeDataValue materializes only properties of primitive types. Materialization specific 
                    // to complex types and collections is done later. 
                    this.MaterializePrimitiveDataValue(property.NullablePropertyType, odataProperty);
                }
            }
        }
    }
}

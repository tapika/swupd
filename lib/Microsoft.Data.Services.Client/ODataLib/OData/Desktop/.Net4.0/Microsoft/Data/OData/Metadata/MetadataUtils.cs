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

namespace Microsoft.Data.OData.Metadata
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Annotations;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Library.Values;
    using Microsoft.Data.Edm.Values;
    using Microsoft.Data.OData.Atom;
    using Strings = Microsoft.Data.OData.Strings;
    #endregion Namespaces

    /// <summary>
    /// Class with utility methods for dealing with OData metadata.
    /// </summary>
    internal static class MetadataUtils
    {
        /// <summary>
        /// Returns the annotation in the OData metadata namespace with the specified <paramref name="localName" />.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the annotation.</param>
        /// <param name="annotatable">The <see cref="IEdmElement"/> to get the annotation from.</param>
        /// <param name="localName">The local name of the annotation to find.</param>
        /// <param name="value">The value of the annotation in the OData metadata namespace and with the specified <paramref name="localName"/>.</param>
        /// <returns>true if an annotation with the specified local name was found; otherwise false.</returns>
        internal static bool TryGetODataAnnotation(this IEdmModel model, IEdmElement annotatable, string localName, out string value)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(model != null, "model != null"); 
            Debug.Assert(annotatable != null, "annotatable != null");
            Debug.Assert(!string.IsNullOrEmpty(localName), "!string.IsNullOrEmpty(localName)");

            object annotationValue = model.GetAnnotationValue(annotatable, AtomConstants.ODataMetadataNamespace, localName);
            if (annotationValue == null)
            {
                value = null;
                return false;
            }

            IEdmStringValue annotationStringValue = annotationValue as IEdmStringValue;
            if (annotationStringValue == null)
            {
                // invalid annotation type found
                throw new ODataException(Strings.ODataAtomWriterMetadataUtils_InvalidAnnotationValue(localName, annotationValue.GetType().FullName));
            }

            value = annotationStringValue.Value;
            return true;
        }

        /// <summary>
        /// Sets the annotation with the OData metadata namespace and the specified <paramref name="localName" /> on the <paramref name="annotatable"/>.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the annotations."/></param>
        /// <param name="annotatable">The <see cref="IEdmElement"/> to set the annotation on.</param>
        /// <param name="localName">The local name of the annotation to set.</param>
        /// <param name="value">The value of the annotation to set.</param>
        internal static void SetODataAnnotation(this IEdmModel model, IEdmElement annotatable, string localName, string value)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(model != null, "model != null"); 
            Debug.Assert(annotatable != null, "annotatable != null");
            Debug.Assert(!string.IsNullOrEmpty(localName), "!string.IsNullOrEmpty(localName)");

            IEdmStringValue stringValue = null;
            if (value != null)
            {
                IEdmStringTypeReference typeReference = EdmCoreModel.Instance.GetString(/*nullable*/true);
                stringValue = new EdmStringConstant(typeReference, value);
            }

            model.SetAnnotationValue(annotatable, AtomConstants.ODataMetadataNamespace, localName, stringValue);
        }

        /// <summary>
        /// Gets all the serializable annotations in the OData metadata namespace on the <paramref name="annotatable"/>.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the annotations."/></param>
        /// <param name="annotatable">The <see cref="IEdmElement"/> to get the annotations from.</param>
        /// <returns>All annotations in the OData metadata namespace; or null if no annotations are found.</returns>
        internal static IEnumerable<IEdmDirectValueAnnotation> GetODataAnnotations(this IEdmModel model, IEdmElement annotatable)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(model != null, "model != null");
            Debug.Assert(annotatable != null, "annotatable != null");
            
            IEnumerable<IEdmDirectValueAnnotation> annotations = model.DirectValueAnnotations(annotatable);
            if (annotations == null)
            {
                return null;
            }

            return annotations.Where(a => a.NamespaceUri == AtomConstants.ODataMetadataNamespace);
        }

        /// <summary>
        /// Gets the EDM type of an OData instance from the <see cref="ODataTypeAnnotation"/> of the instance (if available).
        /// </summary>
        /// <param name="annotatable">The OData instance to get the EDM type for.</param>
        /// <returns>The EDM type of the <paramref name="annotatable"/> if available in the <see cref="ODataTypeAnnotation"/> annotation.</returns>
        internal static IEdmTypeReference GetEdmType(this ODataAnnotatable annotatable)
        {
            DebugUtils.CheckNoExternalCallers();

            if (annotatable == null)
            {
                return null;
            }

            ODataTypeAnnotation typeAnnotation = annotatable.GetAnnotation<ODataTypeAnnotation>();
            return typeAnnotation == null ? null : typeAnnotation.Type;
        }

        /// <summary>
        /// Resolves the name of a primitive, complex, entity or collection type to the respective type. Uses the semantics used by writers.
        /// Thus it implements the strict speced behavior.
        /// </summary>
        /// <param name="model">The model to use.</param>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <returns>The <see cref="IEdmType"/> representing the type specified by the <paramref name="typeName"/>;
        /// or null if no such type could be found.</returns>
        internal static IEdmType ResolveTypeNameForWrite(IEdmModel model, string typeName)
        {
            DebugUtils.CheckNoExternalCallers();
            EdmTypeKind typeKind;

            // Writers should use the highest recognized version for type resolution since they need to verify
            // that the type being used is allowed in the given version. So pass the max here
            // so that we recognize all types and writers can fail later on if the type doesn't fit into the payload.
            return ResolveTypeName(model, /*expectedType*/ null, typeName, /*customTypeResolved*/ null, /*version*/ ODataConstants.MaxODataVersion, out typeKind);
        }

        /// <summary>
        /// Resolves the name of a primitive, complex, entity or collection type to the respective type. Uses the semantics used be readers.
        /// Thus it can be a bit looser.
        /// </summary>
        /// <param name="model">The model to use.</param>
        /// <param name="expectedType">The expected type for the type name being resolved, or null if none is available.</param>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <param name="readerBehavior">Reader behavior if the caller is a reader, null if no reader behavior is available.</param>
        /// <param name="version">The version of the payload being read.</param>
        /// <param name="typeKind">The type kind of the type, if it could be determined. This will be None if we couldn't tell. It might be filled
        /// even if the method returns null, for example for Collection types with item types which are not recognized.</param>
        /// <returns>The <see cref="IEdmType"/> representing the type specified by the <paramref name="typeName"/>;
        /// or null if no such type could be found.</returns>
        internal static IEdmType ResolveTypeNameForRead(
            IEdmModel model,
            IEdmType expectedType,
            string typeName,
            ODataReaderBehavior readerBehavior,
            ODataVersion version,
            out EdmTypeKind typeKind)
        {
            DebugUtils.CheckNoExternalCallers();
            Func<IEdmType, string, IEdmType> customTypeResolver = readerBehavior == null ? null : readerBehavior.TypeResolver;
            Debug.Assert(
                customTypeResolver == null || readerBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesClient,
                "Custom type resolver can only be specified in WCF DS Client behavior.");

            return ResolveTypeName(model, expectedType, typeName, customTypeResolver, version, out typeKind);
        }

        /// <summary>
        /// Resolves the name of a primitive, complex, entity or collection type to the respective type.
        /// </summary>
        /// <param name="model">The model to use.</param>
        /// <param name="expectedType">The expected type for the type name being resolved, or null if none is available.</param>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <param name="customTypeResolver">Custom type resolver to use, if null the model is used directly.</param>
        /// <param name="version">The version to use when resolving the type name.</param>
        /// <param name="typeKind">The type kind of the type, if it could be determined. This will be None if we couldn't tell. It might be filled
        /// even if the method returns null, for example for Collection types with item types which are not recognized.</param>
        /// <returns>The <see cref="IEdmType"/> representing the type specified by the <paramref name="typeName"/>;
        /// or null if no such type could be found.</returns>
        [SuppressMessage("DataWeb.Usage", "AC0003:MethodCallNotAllowed", Justification = "IEdmModel.FindType is allowed here and all other places should call this method to get to the type.")]
        internal static IEdmType ResolveTypeName(
            IEdmModel model,
            IEdmType expectedType,
            string typeName,
            Func<IEdmType, string, IEdmType> customTypeResolver,
            ODataVersion version,
            out EdmTypeKind typeKind)
        {
            DebugUtils.CheckNoExternalCallers();

            Debug.Assert(model != null, "model != null");
            Debug.Assert(typeName != null, "typeName != null");
            IEdmType resolvedType = null;

            // Collection types should only be recognized in V3 and higher.
            string itemTypeName = version >= ODataVersion.V3 ? EdmLibraryExtensions.GetCollectionItemTypeName(typeName) : null;
            if (itemTypeName == null)
            {
                // Note: we require the type resolver or the model to also resolve
                //       primitive types.
                if (customTypeResolver != null && model.IsUserModel())
                {
                    resolvedType = customTypeResolver(expectedType, typeName);
                    if (resolvedType == null)
                    {
                        // If a type resolver is specified it must never return null.
                        throw new ODataException(Strings.MetadataUtils_ResolveTypeName(typeName));
                    }
                }
                else
                {
                    resolvedType = model.FindType(typeName);
                }

                // Spatial types are only recognized in V3 and higher.
                if (version < ODataVersion.V3 && resolvedType != null && resolvedType.IsSpatial())
                {
                    resolvedType = null;
                }

                typeKind = resolvedType == null ? EdmTypeKind.None : resolvedType.TypeKind;
            }
            else
            {
                // Collection
                typeKind = EdmTypeKind.Collection;
                EdmTypeKind itemTypeKind;

                IEdmType expectedItemType = null;
                if (customTypeResolver != null && expectedType != null && expectedType.TypeKind == EdmTypeKind.Collection)
                {
                    expectedItemType = ((IEdmCollectionType)expectedType).ElementType.Definition;
                }

                IEdmType itemType = ResolveTypeName(model, expectedItemType, itemTypeName, customTypeResolver, version, out itemTypeKind);
                if (itemType != null)
                {
                    resolvedType = EdmLibraryExtensions.GetCollectionType(itemType);
                }
            }

            return resolvedType;
        }

        /// <summary>
        /// Calculates the operations that are always bindable to the given type.
        /// </summary>
        /// <param name="bindingType">The binding type in question.</param>
        /// <param name="model">The model to search for operations.</param>
        /// <param name="edmTypeResolver">The edm type resolver to get the parameter type.</param>
        /// <returns>An enumeration of operations that are always bindable to the given type.</returns>
        internal static IEdmFunctionImport[] CalculateAlwaysBindableOperationsForType(IEdmType bindingType, IEdmModel model, EdmTypeResolver edmTypeResolver)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(model != null, "model != null");
            Debug.Assert(edmTypeResolver != null, "edmTypeResolver != null");

            List<IEdmFunctionImport> operations = new List<IEdmFunctionImport>();
            foreach (IEdmEntityContainer container in model.EntityContainers())
            {
                foreach (IEdmFunctionImport functionImport in container.FunctionImports())
                {
                    if (!functionImport.IsBindable || !model.IsAlwaysBindable(functionImport))
                    {
                        continue;
                    }

                    IEdmFunctionParameter bindingParameter = functionImport.Parameters.FirstOrDefault();
                    if (bindingParameter == null)
                    {
                        continue;
                    }

                    IEdmType resolvedBindingType = edmTypeResolver.GetParameterType(bindingParameter).Definition;
                    if (resolvedBindingType.IsAssignableFrom(bindingType))
                    {
                        operations.Add(functionImport);
                    }
                }
            }

            return operations.ToArray();
        }

        /// <summary>
        /// Looks up the given term name in the given model, and returns the term's type if a matching term was found.
        /// </summary>
        /// <param name="qualifiedTermName">The name of the term to lookup, including the namespace.</param>
        /// <param name="model">The model to look in.</param>
        /// <returns>The type of the term in the model, or null if no matching term was found.</returns>
        internal static IEdmTypeReference LookupTypeOfValueTerm(string qualifiedTermName, IEdmModel model)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(model != null, "model != null");

            IEdmTypeReference typeFromModel = null;
            IEdmValueTerm termFromModel = model.FindValueTerm(qualifiedTermName);
            if (termFromModel != null)
            {
                typeFromModel = termFromModel.Type;
            }

            return typeFromModel;
        }

        /// <summary>
        /// Gets the nullable type reference for a payload type; if the payload type is null, uses Edm.String.
        /// </summary>
        /// <param name="payloadType">The payload type to get the type reference for.</param>
        /// <returns>The nullable <see cref="IEdmTypeReference"/> for the <paramref name="payloadType"/>.</returns>
        internal static IEdmTypeReference GetNullablePayloadTypeReference(IEdmType payloadType)
        {
            DebugUtils.CheckNoExternalCallers();
            return payloadType == null ? null : payloadType.ToTypeReference(true);
        }
    }
}

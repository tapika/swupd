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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.Edm.Validation;
    #endregion Namespaces

    /// <summary>
    /// Class with utility methods for dealing with OData metadata that are shared with the OData.Query project.
    /// </summary>
    internal static class MetadataUtilsCommon
    {
        /// <summary>
        /// Checks whether a type reference refers to an OData primitive type (i.e., a primitive, non-stream type).
        /// </summary>
        /// <param name="typeReference">The (non-null) <see cref="IEdmTypeReference"/> to check.</param>
        /// <returns>true if the <paramref name="typeReference"/> is an OData primitive type reference; otherwise false.</returns>
        internal static bool IsODataPrimitiveTypeKind(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull(typeReference.Definition, "typeReference.Definition");

            return typeReference.Definition.IsODataPrimitiveTypeKind();
        }

        /// <summary>
        /// Checks whether a type refers to an OData primitive type (i.e., a primitive, non-stream type).
        /// </summary>
        /// <param name="type">The (non-null) <see cref="IEdmType"/> to check.</param>
        /// <returns>true if the <paramref name="type"/> is an OData primitive type; otherwise false.</returns>
        internal static bool IsODataPrimitiveTypeKind(this IEdmType type)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(type, "type");

            EdmTypeKind typeKind = type.TypeKind;
            if (typeKind != EdmTypeKind.Primitive)
            {
                return false;
            }

            // also make sure it is not a stream
            return !type.IsStream();
        }

        /// <summary>
        /// Checks whether a type reference refers to an OData complex type.
        /// </summary>
        /// <param name="typeReference">The (non-null) <see cref="IEdmTypeReference"/> to check.</param>
        /// <returns>true if the <paramref name="typeReference"/> is an OData complex type reference; otherwise false.</returns>
        internal static bool IsODataComplexTypeKind(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull(typeReference.Definition, "typeReference.Definition");

            return typeReference.Definition.IsODataComplexTypeKind();
        }

        /// <summary>
        /// Checks whether a type refers to an OData complex type.
        /// </summary>
        /// <param name="type">The (non-null) <see cref="IEdmType"/> to check.</param>
        /// <returns>true if the <paramref name="type"/> is an OData complex type; otherwise false.</returns>
        internal static bool IsODataComplexTypeKind(this IEdmType type)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(type, "type");

            return type.TypeKind == EdmTypeKind.Complex;
        }

        /// <summary>
        /// Checks whether a type reference refers to an OData entity type.
        /// </summary>
        /// <param name="typeReference">The (non-null) <see cref="IEdmTypeReference"/> to check.</param>
        /// <returns>true if the <paramref name="typeReference"/> is an OData entity type reference; otherwise false.</returns>
        internal static bool IsODataEntityTypeKind(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull(typeReference.Definition, "typeReference.Definition");

            return typeReference.Definition.IsODataEntityTypeKind();
        }

        /// <summary>
        /// Checks whether a type refers to an OData entity type.
        /// </summary>
        /// <param name="type">The (non-null) <see cref="IEdmType"/> to check.</param>
        /// <returns>true if the <paramref name="type"/> is an OData entity type; otherwise false.</returns>
        internal static bool IsODataEntityTypeKind(this IEdmType type)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(type, "type");

            return type.TypeKind == EdmTypeKind.Entity;
        }

        /// <summary>
        /// Checks whether a type reference is considered a value type in OData.
        /// </summary>
        /// <param name="typeReference">The <see cref="IEdmTypeReference"/> to check.</param>
        /// <returns>true if the <paramref name="typeReference"/> is considered a value type; otherwise false.</returns>
        /// <remarks>
        /// The notion of value type in the OData space is driven by the IDSMP requirements where 
        /// Clr types denote the primitive types.
        /// </remarks>
        internal static bool IsODataValueType(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();

            IEdmPrimitiveTypeReference primitiveTypeReference = typeReference.AsPrimitiveOrNull();
            if (primitiveTypeReference == null)
            {
                return false;
            }

            switch (primitiveTypeReference.PrimitiveKind())
            {
                case EdmPrimitiveTypeKind.Boolean:
                case EdmPrimitiveTypeKind.Byte:
                case EdmPrimitiveTypeKind.DateTime:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Decimal:
                case EdmPrimitiveTypeKind.Double:
                case EdmPrimitiveTypeKind.Guid:
                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                case EdmPrimitiveTypeKind.Int64:
                case EdmPrimitiveTypeKind.SByte:
                case EdmPrimitiveTypeKind.Single:
                case EdmPrimitiveTypeKind.Time:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks whether a type reference refers to a OData collection value type of non-entity elements.
        /// </summary>
        /// <param name="typeReference">The (non-null) <see cref="IEdmType"/> to check.</param>
        /// <returns>true if the <paramref name="typeReference"/> is a non-entity OData collection value type; otherwise false.</returns>
        internal static bool IsNonEntityCollectionType(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(typeReference, "typeReference");
            ExceptionUtils.CheckArgumentNotNull(typeReference.Definition, "typeReference.Definition");

            return typeReference.Definition.IsNonEntityCollectionType();
        }

        /// <summary>
        /// Checks whether a type refers to a OData collection value type of non-entity elements.
        /// </summary>
        /// <param name="type">The (non-null) <see cref="IEdmType"/> to check.</param>
        /// <returns>true if the <paramref name="type"/> is a non-entity OData collection value type; otherwise false.</returns>
        internal static bool IsNonEntityCollectionType(this IEdmType type)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(type != null, "type != null");

            IEdmCollectionType collectionType = type as IEdmCollectionType;

            // Return false if this is not a collection type, or if it's a collection of entity types (i.e., a navigation property)
            if (collectionType == null || (collectionType.ElementType != null && collectionType.ElementType.TypeKind() == EdmTypeKind.Entity))
            {
                return false;
            }

            Debug.Assert(collectionType.TypeKind == EdmTypeKind.Collection, "Expected collection type kind.");
            return true;
        }

        /// <summary>
        /// Casts an <see cref="IEdmTypeReference"/> to a <see cref="IEdmPrimitiveTypeReference"/> or returns null if this is not supported.
        /// </summary>
        /// <param name="typeReference">The type reference to convert.</param>
        /// <returns>An <see cref="IEdmPrimitiveTypeReference"/> instance or null if the <paramref name="typeReference"/> cannot be converted.</returns>
        internal static IEdmPrimitiveTypeReference AsPrimitiveOrNull(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();

            if (typeReference == null)
            {
                return null;
            }

            return typeReference.TypeKind() == EdmTypeKind.Primitive ? typeReference.AsPrimitive() : null;
        }

        /// <summary>
        /// Casts an <see cref="IEdmTypeReference"/> to a <see cref="IEdmEntityTypeReference"/> or returns null if this is not supported.
        /// </summary>
        /// <param name="typeReference">The type reference to convert.</param>
        /// <returns>An <see cref="IEdmComplexTypeReference"/> instance or null if the <paramref name="typeReference"/> cannot be converted.</returns>
        internal static IEdmEntityTypeReference AsEntityOrNull(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();

            if (typeReference == null)
            {
                return null;
            }

            return typeReference.TypeKind() == EdmTypeKind.Entity ? typeReference.AsEntity() : null;
        }

        /// <summary>
        /// Casts an <see cref="IEdmTypeReference"/> to a <see cref="IEdmStructuredTypeReference"/> or returns null if this is not supported.
        /// </summary>
        /// <param name="typeReference">The type reference to convert.</param>
        /// <returns>An <see cref="IEdmStructuredTypeReference"/> instance or null if the <paramref name="typeReference"/> cannot be converted.</returns>
        internal static IEdmStructuredTypeReference AsStructuredOrNull(this IEdmTypeReference typeReference)
        {
            DebugUtils.CheckNoExternalCallers();

            if (typeReference == null)
            {
                return null;
            }

            return typeReference.IsStructured() ? typeReference.AsStructured() : null;
        }

        /// <summary>
        /// Determines if a <paramref name="sourcePrimitiveType"/> is convertibale according to OData rules to the
        /// <paramref name="targetPrimitiveType"/>.
        /// </summary>
        /// <param name="sourcePrimitiveType">The type which is to be converted.</param>
        /// <param name="targetPrimitiveType">The type to which we want to convert.</param>
        /// <returns>true if the source type is convertible to the target type; otherwise false.</returns>
        internal static bool CanConvertPrimitiveTypeTo(
            IEdmPrimitiveType sourcePrimitiveType,
            IEdmPrimitiveType targetPrimitiveType)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(sourcePrimitiveType != null, "sourcePrimitiveType != null");
            Debug.Assert(targetPrimitiveType != null, "targetPrimitiveType != null");

            EdmPrimitiveTypeKind sourcePrimitiveKind = sourcePrimitiveType.PrimitiveKind;
            EdmPrimitiveTypeKind targetPrimitiveKind = targetPrimitiveType.PrimitiveKind;
            switch (sourcePrimitiveKind)
            {
                case EdmPrimitiveTypeKind.SByte:
                    switch (targetPrimitiveKind)
                    {
                        case EdmPrimitiveTypeKind.SByte:
                        case EdmPrimitiveTypeKind.Int16:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Decimal:
                            return true;
                    }

                    break;
                case EdmPrimitiveTypeKind.Byte:
                    switch (targetPrimitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Byte:
                        case EdmPrimitiveTypeKind.Int16:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Decimal:
                            return true;
                    }

                    break;
                case EdmPrimitiveTypeKind.Int16:
                    switch (targetPrimitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Int16:
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Decimal:
                            return true;
                    }

                    break;
                case EdmPrimitiveTypeKind.Int32:
                    switch (targetPrimitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Int32:
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Decimal:
                            return true;
                    }

                    break;
                case EdmPrimitiveTypeKind.Int64:
                    switch (targetPrimitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Int64:
                        case EdmPrimitiveTypeKind.Single:
                        case EdmPrimitiveTypeKind.Double:
                        case EdmPrimitiveTypeKind.Decimal:
                            return true;
                    }

                    break;
                case EdmPrimitiveTypeKind.Single:
                    switch (targetPrimitiveKind)
                    {
                        case EdmPrimitiveTypeKind.Single:
                        case EdmPrimitiveTypeKind.Double:
                            return true;
                    }

                    break;
                default:
                    return sourcePrimitiveKind == targetPrimitiveKind || targetPrimitiveType.IsAssignableFrom(sourcePrimitiveType);
            }

            return false;
        }
    }
}

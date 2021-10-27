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

#if ASTORIA_CLIENT
namespace System.Data.Services.Client
#else
namespace Microsoft.Data.OData.Evaluation
#endif
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Values;
    #endregion

    /// <summary>
    /// Extensibility point for customizing how OData uri's are built.
    /// </summary>
    internal abstract class ODataUriBuilder
    {
        /// <summary>
        /// Builds the base URI for the entity container.
        /// </summary>
        /// <returns>
        /// The base URI for the entity container.
        /// This can be either an absolute URI,
        /// or relative URI which will be combined with the URI of the metadata document for the service.
        /// null if the model doesn't have the service base URI annotation.
        /// </returns>
        internal virtual Uri BuildBaseUri()
        {
#if !ASTORIA_CLIENT
            DebugUtils.CheckNoExternalCallers();
#endif
            return null;
        }

        /// <summary>
        /// Builds the URI for an entity set.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="entitySetName">The entity set name.</param>
        /// <returns>The entity set URI.</returns>
        internal virtual Uri BuildEntitySetUri(Uri baseUri, string entitySetName)
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNullAndEmpty(entitySetName, "entitySetName");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(entitySetName, "entitySetName");
#endif
            return null;
        }

#if ASTORIA_CLIENT
        /// <summary>
        /// Appends to create the entity instance URI for the specified <paramref name="entityInstance"/>.
        /// </summary>
        /// <param name="baseUri">The URI to append to</param>
        /// <param name="entityInstance">The entity instance to use.</param>
        /// <returns>
        /// The entity instance URI.
        /// </returns>
        internal virtual Uri BuildEntityInstanceUri(Uri baseUri, IEdmStructuredValue entityInstance)
#else
        /// <summary>
        /// Builds the entity instance URI with the given key property values.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="keyProperties">The list of name value pair for key properties.</param>
        /// <param name="entityTypeName">The full name of the entity type we are building the key expression for.</param>
        /// <returns>The entity instance URI.</returns>
        internal virtual Uri BuildEntityInstanceUri(Uri baseUri, ICollection<KeyValuePair<string, object>> keyProperties, string entityTypeName)
#endif
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNull(entityInstance, "entityInstance");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(keyProperties, "keyProperties");
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(entityTypeName, "entityTypeName");
#endif
            return null;
        }

        /// <summary>
        /// Builds the edit link for a stream property.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="streamPropertyName">
        /// The name of the stream property the link is computed for; 
        /// or null for the default media resource.
        /// </param>
        /// <returns>The edit link for the stream.</returns>
        internal virtual Uri BuildStreamEditLinkUri(Uri baseUri, string streamPropertyName)
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNotEmpty(streamPropertyName, "streamPropertyName");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentStringNotEmpty(streamPropertyName, "streamPropertyName");
#endif
            return null;
        }

        /// <summary>
        /// Builds the read link for a stream property.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="streamPropertyName">
        /// The name of the stream property the link is computed for; 
        /// or null for the default media resource.
        /// </param>
        /// <returns>The read link for the stream.</returns>
        internal virtual Uri BuildStreamReadLinkUri(Uri baseUri, string streamPropertyName)
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNotEmpty(streamPropertyName, "streamPropertyName");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentStringNotEmpty(streamPropertyName, "streamPropertyName");
#endif
            return null;
        }

        /// <summary>
        /// Builds the navigation link for the navigation property.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="navigationPropertyName">The name of the navigation property to get the navigation link URI for.</param>
        /// <returns>The navigation link URI for the navigation property.</returns>
        internal virtual Uri BuildNavigationLinkUri(Uri baseUri, string navigationPropertyName)
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNullAndEmpty(navigationPropertyName, "navigationPropertyName");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(navigationPropertyName, "navigationPropertyName");
#endif
            return null;
        }

        /// <summary>
        /// Builds the association link for the navigation property.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="navigationPropertyName">The name of the navigation property to get the association link URI for.</param>
        /// <returns>The association link URI for the navigation property.</returns>
        internal virtual Uri BuildAssociationLinkUri(Uri baseUri, string navigationPropertyName)
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNullAndEmpty(navigationPropertyName, "navigationPropertyName");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(navigationPropertyName, "navigationPropertyName");
#endif
            return null;
        }

        /// <summary>
        /// Builds the operation target URI for the specified <paramref name="operationName"/>.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="operationName">The fully qualified name of the operation for which to get the target URI.</param>
        /// <param name="bindingParameterTypeName">The binding parameter type name to include in the target, or null/empty if there is none.</param>
        /// <returns>The target URI for the operation.</returns>
        internal virtual Uri BuildOperationTargetUri(Uri baseUri, string operationName, string bindingParameterTypeName)
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNullAndEmpty(operationName, "operationName");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(operationName, "operationName");
#endif
            return null;
        }

        /// <summary>
        /// Builds a URI with the given type name appended as a new segment on the base URI.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="typeName">The fully qualified type name to append.</param>
        /// <returns>The URI with the type segment appended.</returns>
        internal virtual Uri AppendTypeSegment(Uri baseUri, string typeName)
        {
#if ASTORIA_CLIENT
            Util.CheckArgumentNullAndEmpty(typeName, "typeName");
#else
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(typeName, "typeName");
#endif
            return null;
        }
    }
}

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

namespace Microsoft.Data.OData.Evaluation
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Values;
    using Microsoft.Data.OData.Metadata;
    using Microsoft.Data.OData.Query;
    #endregion

    /// <summary>
    /// Implementation of OData URI builder based on OData protocol conventions.
    /// </summary>
    internal sealed class ODataConventionalUriBuilder : ODataUriBuilder
    {
        /// <summary>The base URI of the service. This will be used as the base URI for all entity containers.</summary>
        private readonly Uri serviceBaseUri;

        /// <summary>The specific url-convention to use.</summary>
        private readonly UrlConvention urlConvention;

        /// <summary>The specific key-serializer to use based on the convention.</summary>
        private readonly KeySerializer keySerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceBaseUri">The base URI of the service. This will be used as the base URI for all entity containers.</param>
        /// <param name="urlConvention">The specific url convention to use.</param>
        internal ODataConventionalUriBuilder(Uri serviceBaseUri, UrlConvention urlConvention)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(serviceBaseUri != null && serviceBaseUri.IsAbsoluteUri, "serviceBaseUri != null && serviceBaseUri.IsAbsoluteUri");
            Debug.Assert(urlConvention != null, "urlConvention != null");

            this.serviceBaseUri = serviceBaseUri;
            this.urlConvention = urlConvention;
            this.keySerializer = KeySerializer.Create(this.urlConvention);
        }

        /// <summary>
        /// Builds the base URI for the entity container.
        /// </summary>
        /// <returns>
        /// The base URI for the entity container.
        /// This can be either an absolute URI,
        /// or relative URI which will be combined with the URI of the metadata document for the service.
        /// null if the model doesn't have the service base URI annotation.
        /// </returns>
        internal override Uri BuildBaseUri()
        {
            DebugUtils.CheckNoExternalCallers();

            return this.serviceBaseUri;
        }

        /// <summary>
        /// Builds the URI for an entity set.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="entitySetName">The entity set name.</param>
        /// <returns>The entity set URI.</returns>
        internal override Uri BuildEntitySetUri(Uri baseUri, string entitySetName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(entitySetName, "entitySetName");

            return AppendSegment(baseUri, entitySetName, true /*escapeSegment*/);
        }

        /// <summary>
        /// Builds the entity instance URI with the given key property values.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="keyProperties">The list of name value pair for key properties.</param>
        /// <param name="entityTypeName">The full name of the entity type we are building the key expression for.</param>
        /// <returns>The entity instance URI.</returns>
        internal override Uri BuildEntityInstanceUri(Uri baseUri, ICollection<KeyValuePair<string, object>> keyProperties, string entityTypeName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            Debug.Assert(keyProperties != null, "keyProperties != null");
            Debug.Assert(!string.IsNullOrEmpty(entityTypeName), "!string.IsNullOrEmpty(entityTypeName)");

            StringBuilder builder = new StringBuilder(UriUtilsCommon.UriToString(baseUri));

            // TODO: What should be done about escaping the values.
            // TODO: What should happen if the URL does end with a slash?
            this.AppendKeyExpression(builder, keyProperties, entityTypeName);
            return new Uri(builder.ToString(), UriKind.Absolute);
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
        internal override Uri BuildStreamEditLinkUri(Uri baseUri, string streamPropertyName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            ExceptionUtils.CheckArgumentStringNotEmpty(streamPropertyName, "streamPropertyName");

            if (streamPropertyName == null)
            {
                return AppendSegment(baseUri, ODataConstants.DefaultStreamSegmentName, false /*escapeSegment*/);
            }
            else
            {
                return AppendSegment(baseUri, streamPropertyName, true /*escapeSegment*/);
            }
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
        internal override Uri BuildStreamReadLinkUri(Uri baseUri, string streamPropertyName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            ExceptionUtils.CheckArgumentStringNotEmpty(streamPropertyName, "streamPropertyName");

            if (streamPropertyName == null)
            {
                return AppendSegment(baseUri, ODataConstants.DefaultStreamSegmentName, false /*escapeSegment*/);
            }
            else
            {
                return AppendSegment(baseUri, streamPropertyName, true /*escapeSegment*/);
            }
        }

        /// <summary>
        /// Builds the navigation link for the navigation property.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="navigationPropertyName">The name of the navigation property to get the navigation link URI for.</param>
        /// <returns>The navigation link URI for the navigation property.</returns>
        internal override Uri BuildNavigationLinkUri(Uri baseUri, string navigationPropertyName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(navigationPropertyName, "navigationPropertyName");

            return AppendSegment(baseUri, navigationPropertyName, true /*escapeSegment*/);
        }

        /// <summary>
        /// Builds the association link for the navigation property.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="navigationPropertyName">The name of the navigation property to get the association link URI for.</param>
        /// <returns>The association link URI for the navigation property.</returns>
        internal override Uri BuildAssociationLinkUri(Uri baseUri, string navigationPropertyName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(navigationPropertyName, "navigationPropertyName");

            // We don't want the $links segment to be escaped, so append that first without escaping, then append the property name with escaping
            Uri baseUriWithLinksSegment = AppendSegment(baseUri, ODataConstants.AssociationLinkSegmentName + ODataConstants.UriSegmentSeparator, false /*escapeSegment*/);
            return AppendSegment(baseUriWithLinksSegment, navigationPropertyName, true /*escapeSegment*/);
        }

        /// <summary>
        /// Builds the operation target URI for the specified <paramref name="operationName"/>.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="operationName">The fully qualified name of the operation for which to get the target URI.</param>
        /// <param name="bindingParameterTypeName">The binding parameter type name to include in the target, or null/empty if there is none.</param>
        /// <returns>The target URI for the operation.</returns>
        internal override Uri BuildOperationTargetUri(Uri baseUri, string operationName, string bindingParameterTypeName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(operationName, "operationName");

            if (!string.IsNullOrEmpty(bindingParameterTypeName))
            {
                Uri withBindingParameter = AppendSegment(baseUri, bindingParameterTypeName, true /*escapeSegment*/);
                return AppendSegment(withBindingParameter, operationName, true /*escapeSegment*/);
            }

            return AppendSegment(baseUri, operationName, true /*escapeSegment*/);
        }

        /// <summary>
        /// Builds a URI with the given type name appended as a new segment on the base URI.
        /// </summary>
        /// <param name="baseUri">The URI to append to.</param>
        /// <param name="typeName">The fully qualified type name to append.</param>
        /// <returns>The URI with the type segment appended.</returns>
        internal override Uri AppendTypeSegment(Uri baseUri, string typeName)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidateBaseUri(baseUri);
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(typeName, "typeName");

            return AppendSegment(baseUri, typeName, true /*escapeSegment*/);
        }

        /// <summary>
        /// Validates the base URI parameter to be a non-null absolute URI.
        /// </summary>
        /// <param name="baseUri">The base URI parameter to validate.</param>
        [Conditional("DEBUG")]
        private static void ValidateBaseUri(Uri baseUri)
        {
            Debug.Assert(baseUri != null && baseUri.IsAbsoluteUri, "baseUri != null && baseUri.IsAbsoluteUri");
        }

        /// <summary>
        /// Appends a segment to the specified base URI.
        /// </summary>
        /// <param name="baseUri">The base Uri to append the segment to.</param>
        /// <param name="segment">The segment to append.</param>
        /// <param name="escapeSegment">True if the new segment should be escaped, otherwise False.</param>
        /// <returns>New URI with the appended segment and no trailing slash added.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("DataWeb.Usage", "AC0018:SystemUriEscapeDataStringRule", Justification = "Values passed to this method are model elements like property names or keywords.")]
        private static Uri AppendSegment(Uri baseUri, string segment, bool escapeSegment)
        {
            string baseUriString = UriUtilsCommon.UriToString(baseUri);

            if (escapeSegment)
            {
                segment = Uri.EscapeDataString(segment);
            }

            if (baseUriString[baseUriString.Length - 1] != ODataConstants.UriSegmentSeparatorChar)
            {
                return new Uri(baseUriString + ODataConstants.UriSegmentSeparator + segment, UriKind.Absolute);
            }
            else
            {
                return new Uri(baseUri, segment);
            }
        }

        /// <summary>
        /// Gets the CLR value of a primitive key property.
        /// </summary>
        /// <param name="keyPropertyName">The key property name.</param>
        /// <param name="keyPropertyValue">The key property value.</param>
        /// <param name="entityTypeName">The entity type name we are validating the key value for.</param>
        /// <returns>The primitive value of the key property.</returns>
        private static object ValidateKeyValue(string keyPropertyName, object keyPropertyValue, string entityTypeName)
        {
            if (keyPropertyValue == null)
            {
                throw new ODataException(OData.Strings.ODataConventionalUriBuilder_NullKeyValue(keyPropertyName, entityTypeName));
            }

            return keyPropertyValue;
        }

        /// <summary>
        /// Appends the key expression for the given entity to the given <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="builder">The builder to append onto.</param>
        /// <param name="keyProperties">The list of name value pair for key properties.</param>
        /// <param name="entityTypeName">The full name of the entity type we are building the key expression for.</param>
        private void AppendKeyExpression(StringBuilder builder, ICollection<KeyValuePair<string, object>> keyProperties, string entityTypeName)
        {
            Debug.Assert(builder != null, "builder != null");
            Debug.Assert(keyProperties != null, "keyProperties != null");

            if (!keyProperties.Any())
            {
                throw new ODataException(OData.Strings.ODataConventionalUriBuilder_EntityTypeWithNoKeyProperties(entityTypeName));
            }

            this.keySerializer.AppendKeyExpression(builder, keyProperties, p => p.Key, p => ValidateKeyValue(p.Key, p.Value, entityTypeName));
        }
    }
}

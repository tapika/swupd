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

namespace Microsoft.Data.OData.JsonLight
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Json;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// OData JsonLight serializer for entity reference links.
    /// </summary>
    internal sealed class ODataJsonLightEntityReferenceLinkSerializer : ODataJsonLightSerializer
    {
        /// <summary>The metadata uri builder to use.</summary>
        private readonly ODataJsonLightMetadataUriBuilder metadataUriBuilder;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLightOutputContext">The output context to write to.</param>
        internal ODataJsonLightEntityReferenceLinkSerializer(ODataJsonLightOutputContext jsonLightOutputContext)
            : base(jsonLightOutputContext)
        {
            DebugUtils.CheckNoExternalCallers();

            // DEVNOTE: grab this early so that any validation errors are thrown at creation time rather than when Write___ is called.
            this.metadataUriBuilder = jsonLightOutputContext.CreateMetadataUriBuilder();
        }

        /// <summary>
        /// Writes a single top-level Uri in response to a $links query.
        /// </summary>
        /// <param name="link">The entity reference link to write out.</param>
        /// <param name="entitySet">The entity set of the navigation property</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference link is being written, or null if none is available.</param>
        internal void WriteEntityReferenceLink(ODataEntityReferenceLink link, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(link != null, "link != null");

            this.WriteTopLevelPayload(
                () => this.WriteEntityReferenceLinkImplementation(link, entitySet, navigationProperty, /* isTopLevel */ true));
        }

        /// <summary>
        /// Writes a set of links (Uris) in response to a $links query; includes optional count and next-page-link information.
        /// </summary>
        /// <param name="entityReferenceLinks">The set of entity reference links to write out.</param>
        /// <param name="entitySet">The entity set of the navigation property</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference links are being written, or null if none is available.</param>
        internal void WriteEntityReferenceLinks(ODataEntityReferenceLinks entityReferenceLinks, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityReferenceLinks != null, "entityReferenceLinks != null");

            this.WriteTopLevelPayload(
                () => this.WriteEntityReferenceLinksImplementation(entityReferenceLinks, entitySet, navigationProperty));
        }

        /// <summary>
        /// Writes a single Uri in response to a $links query.
        /// </summary>
        /// <param name="entityReferenceLink">The entity reference link to write out.</param>
        /// <param name="entitySet">The entity set of the navigation property</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference link is being written, or null if none is available.</param>
        /// <param name="isTopLevel">true if the entity reference link being written is at the top level of the payload.</param>
        private void WriteEntityReferenceLinkImplementation(ODataEntityReferenceLink entityReferenceLink, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty, bool isTopLevel)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityReferenceLink != null, "entityReferenceLink != null");

            WriterValidationUtils.ValidateEntityReferenceLink(entityReferenceLink);

            this.JsonWriter.StartObjectScope();
     
            if (isTopLevel)
            {
                Uri metadataUri;
                if (this.metadataUriBuilder.TryBuildEntityReferenceLinkMetadataUri(entityReferenceLink.SerializationInfo, entitySet, navigationProperty, out metadataUri))
                {
                    this.WriteMetadataUriProperty(metadataUri);
                }
            }

            this.JsonWriter.WriteName(JsonLightConstants.ODataEntityReferenceLinkUrlName);
            this.JsonWriter.WriteValue(this.UriToString(entityReferenceLink.Url));
            this.JsonWriter.EndObjectScope();
        }

        /// <summary>
        /// Writes a set of links (Uris) in response to a $links query; includes optional count and next-page-link information.
        /// </summary>
        /// <param name="entityReferenceLinks">The set of entity reference links to write out.</param>
        /// <param name="entitySet">The entity set of the navigation property</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference links are being written, or null if none is available.</param>
        private void WriteEntityReferenceLinksImplementation(ODataEntityReferenceLinks entityReferenceLinks, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityReferenceLinks != null, "entityReferenceLinks != null");

            bool wroteNextLink = false;
            bool wroteCount = false;

            // {
            this.JsonWriter.StartObjectScope();

            Uri metadataUri;
            if (this.metadataUriBuilder.TryBuildEntityReferenceLinksMetadataUri(entityReferenceLinks.SerializationInfo, entitySet, navigationProperty, out metadataUri))
            {
                this.WriteMetadataUriProperty(metadataUri);
            }

            if (entityReferenceLinks.Count.HasValue)
            {
                // We try to write the count property at the top of the payload if one is available. If not, we try again at the end.
                wroteCount = true;

                // "odata.count": ...
                this.WriteCountAnnotation(entityReferenceLinks.Count.Value);
            }

            if (entityReferenceLinks.NextPageLink != null)
            {
                // We try to write the next link at the top of the payload if one is available. If not, we try again at the end.
                wroteNextLink = true;

                // "odata.next": ...
                this.WriteNextLinkAnnotation(entityReferenceLinks.NextPageLink);
            }

            // "value":
            this.JsonWriter.WriteValuePropertyName();
            
            // "[":
            this.JsonWriter.StartArrayScope();

            IEnumerable<ODataEntityReferenceLink> entityReferenceLinksEnumerable = entityReferenceLinks.Links;
            if (entityReferenceLinksEnumerable != null)
            {
                foreach (ODataEntityReferenceLink entityReferenceLink in entityReferenceLinksEnumerable)
                {
                    WriterValidationUtils.ValidateEntityReferenceLinkNotNull(entityReferenceLink);
                    this.WriteEntityReferenceLinkImplementation(entityReferenceLink, entitySet, /* navigationProperty */ null, /* isTopLevel */ false);
                }
            }

            // "]"
            this.JsonWriter.EndArrayScope();

            if (!wroteCount && entityReferenceLinks.Count.HasValue)
            {
                // "odata.count": ...
                this.WriteCountAnnotation(entityReferenceLinks.Count.Value);
            }

            if (!wroteNextLink && entityReferenceLinks.NextPageLink != null)
            {
                // "odata.next": ...
                this.WriteNextLinkAnnotation(entityReferenceLinks.NextPageLink);
            }

            // "}"
            this.JsonWriter.EndObjectScope();
        }

        /// <summary>
        /// Writes the next link property, which consists of the property name and value.
        /// </summary>
        /// <param name="nextPageLink">The non-null value of the next link to write.</param>
        private void WriteNextLinkAnnotation(Uri nextPageLink)
        {
            Debug.Assert(nextPageLink != null, "Expected non-null next link.");

            this.JsonWriter.WriteName(ODataAnnotationNames.ODataNextLink);
            this.JsonWriter.WriteValue(this.UriToString(nextPageLink));
        }

        /// <summary>
        /// Writes the odata.count property, which consists of the property name and value.
        /// </summary>
        /// <param name="countValue">The value of the count property to write.</param>
        private void WriteCountAnnotation(long countValue)
        {
            this.JsonWriter.WriteName(ODataAnnotationNames.ODataCount);
            this.JsonWriter.WriteValue(countValue);
        }
    }
}

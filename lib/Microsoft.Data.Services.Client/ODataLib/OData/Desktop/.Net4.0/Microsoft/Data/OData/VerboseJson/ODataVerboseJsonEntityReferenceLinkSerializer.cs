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

namespace Microsoft.Data.OData.VerboseJson
{
    #region Namespaces
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.OData.Json;
    #endregion Namespaces

    /// <summary>
    /// OData Verbose JSON serializer for entity reference links.
    /// </summary>
    internal sealed class ODataVerboseJsonEntityReferenceLinkSerializer : ODataVerboseJsonSerializer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="verboseJsonOutputContext">The output context to write to.</param>
        internal ODataVerboseJsonEntityReferenceLinkSerializer(ODataVerboseJsonOutputContext verboseJsonOutputContext)
            : base(verboseJsonOutputContext)
        {
            DebugUtils.CheckNoExternalCallers();
        }

        /// <summary>
        /// Writes a single top-level Uri in response to a $links query.
        /// </summary>
        /// <param name="link">The entity reference link to write out.</param>
        internal void WriteEntityReferenceLink(ODataEntityReferenceLink link)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(link != null, "link != null");

            this.WriteTopLevelPayload(
                () => this.WriteEntityReferenceLinkImplementation(link));
        }

        /// <summary>
        /// Writes a set of links (Uris) in response to a $links query; includes optional count and next-page-link information.
        /// </summary>
        /// <param name="entityReferenceLinks">The set of entity reference links to write out.</param>
        internal void WriteEntityReferenceLinks(ODataEntityReferenceLinks entityReferenceLinks)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityReferenceLinks != null, "entityReferenceLinks != null");

            this.WriteTopLevelPayload(
                () => this.WriteEntityReferenceLinksImplementation(entityReferenceLinks, this.Version >= ODataVersion.V2 && this.WritingResponse));
        }

        /// <summary>
        /// Writes a single Uri in response to a $links query.
        /// </summary>
        /// <param name="entityReferenceLink">The entity reference link to write out.</param>
        private void WriteEntityReferenceLinkImplementation(ODataEntityReferenceLink entityReferenceLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityReferenceLink != null, "entityReferenceLink != null");

            WriterValidationUtils.ValidateEntityReferenceLink(entityReferenceLink);

            this.JsonWriter.StartObjectScope();
            this.JsonWriter.WriteName(JsonConstants.ODataUriName);
            this.JsonWriter.WriteValue(this.UriToAbsoluteUriString(entityReferenceLink.Url));
            this.JsonWriter.EndObjectScope();
        }

        /// <summary>
        /// Writes a set of links (Uris) in response to a $links query; includes optional count and next-page-link information.
        /// </summary>
        /// <param name="entityReferenceLinks">The set of entity reference links to write out.</param>
        /// <param name="includeResultsWrapper">true if the 'results' wrapper should be included into the payload; otherwise false.</param>
        private void WriteEntityReferenceLinksImplementation(ODataEntityReferenceLinks entityReferenceLinks, bool includeResultsWrapper)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityReferenceLinks != null, "entityReferenceLinks != null");

            if (includeResultsWrapper)
            {
                // {
                this.JsonWriter.StartObjectScope();
            }

            if (entityReferenceLinks.Count.HasValue)
            {
                Debug.Assert(includeResultsWrapper, "Expected 'includeResultsWrapper' to be true if a count is specified.");
                this.JsonWriter.WriteName(JsonConstants.ODataCountName);

                this.JsonWriter.WriteValue(entityReferenceLinks.Count.Value);
            }

            if (includeResultsWrapper)
            {
                // "results":
                this.JsonWriter.WriteDataArrayName();
            }

            this.JsonWriter.StartArrayScope();

            IEnumerable<ODataEntityReferenceLink> entityReferenceLinksEnumerable = entityReferenceLinks.Links;
            if (entityReferenceLinksEnumerable != null)
            {
                foreach (ODataEntityReferenceLink entityReferenceLink in entityReferenceLinksEnumerable)
                {
                    WriterValidationUtils.ValidateEntityReferenceLinkNotNull(entityReferenceLink);
                    this.WriteEntityReferenceLinkImplementation(entityReferenceLink);
                }
            }

            this.JsonWriter.EndArrayScope();

            if (entityReferenceLinks.NextPageLink != null)
            {
                // "__next": ...
                Debug.Assert(includeResultsWrapper, "Expected 'includeResultsWrapper' to be true if a next page link is specified.");
                this.JsonWriter.WriteName(JsonConstants.ODataNextLinkName);
                this.JsonWriter.WriteValue(this.UriToAbsoluteUriString(entityReferenceLinks.NextPageLink));
            }

            if (includeResultsWrapper)
            {
                this.JsonWriter.EndObjectScope();
            }
        }
    }
}

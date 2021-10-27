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

namespace Microsoft.Data.OData.Atom
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    using ODataErrorStrings = Microsoft.Data.OData.Strings;
    #endregion Namespaces

    /// <summary>
    /// Base class for all OData ATOM Metadata serializers.
    /// </summary>
    internal abstract class ODataAtomMetadataSerializer : ODataAtomSerializer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomOutputContext">The output context to write to.</param>
        internal ODataAtomMetadataSerializer(ODataAtomOutputContext atomOutputContext)
            : base(atomOutputContext)
        {
            DebugUtils.CheckNoExternalCallers();
        }

        /// <summary>
        /// Writes an Xml element with the specified primitive value as content.
        /// </summary>
        /// <param name="prefix">The prefix for the element's namespace.</param>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="ns">The namespace of the element.</param>
        /// <param name="textConstruct">The <see cref="AtomTextConstruct"/> value to be used as element content.</param>
        internal void WriteTextConstruct(string prefix, string localName, string ns, AtomTextConstruct textConstruct)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(prefix != null, "prefix != null");
            Debug.Assert(!string.IsNullOrEmpty(localName), "!string.IsNullOrEmpty(localName)");
            Debug.Assert(!string.IsNullOrEmpty(ns), "!string.IsNullOrEmpty(ns)");

            this.XmlWriter.WriteStartElement(prefix, localName, ns);

            if (textConstruct != null)
            {
                AtomTextConstructKind textKind = textConstruct.Kind;

                this.XmlWriter.WriteAttributeString(AtomConstants.AtomTypeAttributeName, AtomValueUtils.ToString(textConstruct.Kind));

                string textValue = textConstruct.Text;
                if (textValue == null)
                {
                    textValue = String.Empty;
                }

                if (textKind == AtomTextConstructKind.Xhtml)
                {
                    ODataAtomWriterUtils.WriteRaw(this.XmlWriter, textValue);
                }
                else
                {
                    ODataAtomWriterUtils.WriteString(this.XmlWriter, textValue);
                }
            }

            this.XmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes the 'atom:category' element given category metadata.
        /// </summary>
        /// <param name="category">The category information to write.</param>
        internal void WriteCategory(AtomCategoryMetadata category)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(category != null, "Category must not be null.");

            this.WriteCategory(AtomConstants.AtomNamespacePrefix, category.Term, category.Scheme, category.Label);
        }

        /// <summary>
        /// Writes the 'atom:category' element with the specified attributes.
        /// </summary>
        /// <param name="atomPrefix">The prefix to use for the 'category' element.</param>
        /// <param name="term">The value for the 'term' attribute (required).</param>
        /// <param name="scheme">The value for the 'scheme' attribute (optional).</param>
        /// <param name="label">The value for the 'label' attribute (optional).</param>
        internal void WriteCategory(string atomPrefix, string term, string scheme, string label)
        {
            DebugUtils.CheckNoExternalCallers();

            this.XmlWriter.WriteStartElement(
                atomPrefix,
                AtomConstants.AtomCategoryElementName,
                AtomConstants.AtomNamespace);

            if (term == null)
            {
                throw new ODataException(ODataErrorStrings.ODataAtomWriterMetadataUtils_CategoryMustSpecifyTerm);
            }

            this.XmlWriter.WriteAttributeString(
                AtomConstants.AtomCategoryTermAttributeName,
                term);

            if (scheme != null)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.AtomCategorySchemeAttributeName, scheme);
            }

            if (label != null)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.AtomCategoryLabelAttributeName, label);
            }

            this.XmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Write an empty author element that has the required name element
        /// </summary>
        internal void WriteEmptyAuthor()
        {
            DebugUtils.CheckNoExternalCallers();

            // <atom:author>
            this.XmlWriter.WriteStartElement(AtomConstants.AtomNamespacePrefix, AtomConstants.AtomAuthorElementName, AtomConstants.AtomNamespace);

            // <atom:Name></atom:Name>
            this.WriteEmptyElement(
                AtomConstants.AtomNamespacePrefix,
                AtomConstants.AtomAuthorNameElementName,
                AtomConstants.AtomNamespace);

            // </atom:author>
            this.XmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes the specified start/end tags and the specified person metadata as content
        /// </summary>
        /// <param name="personMetadata">The person metadata to write.</param>
        internal void WritePersonMetadata(AtomPersonMetadata personMetadata)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(personMetadata != null, "Person metadata must not be null.");

            // <atom:name>name of person</atom:name>
            // NOTE: write an empty element if no name is specified because the element is required.
            this.WriteElementWithTextContent(
                AtomConstants.AtomNamespacePrefix,
                AtomConstants.AtomPersonNameElementName,
                AtomConstants.AtomNamespace,
                personMetadata.Name);

            string uriString = personMetadata.UriFromEpm;
            if (uriString != null)
            {
                Debug.Assert(
                    personMetadata.Uri == null,
                    "If the internal UriFromEpm was used, then the Uri property must be left null. The merge between custom and EPM is probably wrong.");
            }
            else
            {
                Uri uri = personMetadata.Uri;
                if (uri != null)
                {
                    uriString = this.UriToUrlAttributeValue(uri);
                }
            }

            if (uriString != null)
            {
                this.WriteElementWithTextContent(
                    AtomConstants.AtomNamespacePrefix,
                    AtomConstants.AtomPersonUriElementName,
                    AtomConstants.AtomNamespace,
                    uriString);
            }

            string email = personMetadata.Email;
            if (email != null)
            {
                this.WriteElementWithTextContent(
                    AtomConstants.AtomNamespacePrefix,
                    AtomConstants.AtomPersonEmailElementName,
                    AtomConstants.AtomNamespace,
                    email);
            }
        }

        /// <summary>
        /// Write the metadata of a link in ATOM format
        /// </summary>
        /// <param name="linkMetadata">The link metadata to write.</param>
        /// <param name="etag">The (optional) ETag for a link.</param>
        internal void WriteAtomLink(AtomLinkMetadata linkMetadata, string etag)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(linkMetadata != null, "Link metadata must not be null.");

            // <atom:link ...
            this.XmlWriter.WriteStartElement(
                AtomConstants.AtomNamespacePrefix,
                AtomConstants.AtomLinkElementName,
                AtomConstants.AtomNamespace);

            // write the attributes of the link
            this.WriteAtomLinkAttributes(linkMetadata, etag);

            // </atom:link>
            this.XmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Write the metadata of a link in ATOM format
        /// </summary>
        /// <param name="linkMetadata">The link metadata to write.</param>
        /// <param name="etag">The (optional) ETag for a link.</param>
        internal void WriteAtomLinkAttributes(AtomLinkMetadata linkMetadata, string etag)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(linkMetadata != null, "Link metadata must not be null.");

            string linkHref = linkMetadata.Href == null ? null : this.UriToUrlAttributeValue(linkMetadata.Href);

            this.WriteAtomLinkMetadataAttributes(linkMetadata.Relation, linkHref, linkMetadata.HrefLang, linkMetadata.Title, linkMetadata.MediaType, linkMetadata.Length);

            if (etag != null)
            {
                ODataAtomWriterUtils.WriteETag(this.XmlWriter, etag);
            }
        }

        /// <summary>
        /// Write the metadata attributes of a link in ATOM format
        /// </summary>
        /// <param name="relation">The value for the 'rel' attribute.</param>
        /// <param name="href">The value for the 'href' attribute.</param>
        /// <param name="hrefLang">The value for the 'hreflang' attribute.</param>
        /// <param name="title">The value for the 'title' attribute.</param>
        /// <param name="mediaType">The value for the 'type' attribute.</param>
        /// <param name="length">The value for the 'length' attribute.</param>
        private void WriteAtomLinkMetadataAttributes(string relation, string href, string hrefLang, string title, string mediaType, int? length)
        {
            // rel="..."
            if (relation != null)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.AtomLinkRelationAttributeName, relation);
            }

            // type="..."
            if (mediaType != null)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.AtomLinkTypeAttributeName, mediaType);
            }

            // title="..."
            if (title != null)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.AtomLinkTitleAttributeName, title);
            }

            // href="..."
            if (href == null)
            {
                throw new ODataException(ODataErrorStrings.ODataAtomWriterMetadataUtils_LinkMustSpecifyHref);
            }

            this.XmlWriter.WriteAttributeString(AtomConstants.AtomHRefAttributeName, href);

            // hreflang="..."
            if (hrefLang != null)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.AtomLinkHrefLangAttributeName, hrefLang);
            }

            // length="..."
            if (length.HasValue)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.AtomLinkLengthAttributeName, ODataAtomConvert.ToString(length.Value));
            }
        }
    }
}

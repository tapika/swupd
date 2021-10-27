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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml;
    using Microsoft.Data.Edm.Library;
    #endregion Namespaces

    /// <summary>
    /// OData ATOM deserializer for entity reference links.
    /// </summary>
    internal sealed class ODataAtomEntityReferenceLinkDeserializer : ODataAtomDeserializer
    {
        #region Atomized strings
        /// <summary>OData element name for the 'links' element</summary>
        private readonly string ODataLinksElementName;

        /// <summary>OData element name for the 'count' element</summary>
        private readonly string ODataCountElementName;

        /// <summary>OData element name for the 'next' element</summary>
        private readonly string ODataNextElementName;

        /// <summary>OData element name for the 'uri' element</summary>
        private readonly string ODataUriElementName;
        #endregion Atomized strings

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomInputContext">The ATOM input context to read from.</param>
        internal ODataAtomEntityReferenceLinkDeserializer(ODataAtomInputContext atomInputContext)
            : base(atomInputContext)
        {
            DebugUtils.CheckNoExternalCallers();

            XmlNameTable nameTable = this.XmlReader.NameTable;
            this.ODataLinksElementName = nameTable.Add(AtomConstants.ODataLinksElementName);
            this.ODataCountElementName = nameTable.Add(AtomConstants.ODataCountElementName);
            this.ODataNextElementName = nameTable.Add(AtomConstants.ODataNextLinkElementName);
            this.ODataUriElementName = nameTable.Add(AtomConstants.ODataUriElementName);
        }

        /// <summary>
        /// An enumeration of the various kinds of properties on an entity reference link collection.
        /// </summary>
        [Flags]
        private enum DuplicateEntityReferenceLinksElementBitMask
        {
            /// <summary>No duplicates.</summary>
            None = 0,

            /// <summary>The 'm:count' element of the 'links' element.</summary>
            Count = 1,

            /// <summary>The 'd:next' element of the 'links' element.</summary>
            NextLink = 2,
        }

        /// <summary>
        /// Read a set of top-level entity reference links.
        /// </summary>
        /// <returns>An <see cref="ODataEntityReferenceLinks"/> representing the read links.</returns>
        /// <remarks>
        /// Pre-Condition:  PayloadStart        - assumes that the XML reader has not been used yet.
        /// Post-Condtion:  XmlNodeType.None    - The reader must be at the end of the input.
        /// </remarks>
        internal ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.XmlReader != null, "this.XmlReader != null");
            this.XmlReader.AssertNotBuffering();

            // Read the start of the payload up to the first element
            this.ReadPayloadStart();
            this.AssertXmlCondition(XmlNodeType.Element);

            if (!this.XmlReader.NamespaceEquals(this.XmlReader.ODataNamespace) || !this.XmlReader.LocalNameEquals(this.ODataLinksElementName))
            {
                throw new ODataException(
                    Strings.ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinksStartElement(this.XmlReader.LocalName, this.XmlReader.NamespaceURI));
            }

            ODataEntityReferenceLinks entityReferenceLinks = this.ReadLinksElement();

            // Read the payload end
            this.ReadPayloadEnd();
            this.XmlReader.AssertNotBuffering();

            return entityReferenceLinks;
        }

        /// <summary>
        /// Reads a top-level entity reference link.
        /// </summary>
        /// <returns>An <see cref="ODataEntityReferenceLink"/> instance representing the read entity reference link.</returns>
        /// <remarks>
        /// Pre-Condition:  PayloadStart        - assumes that the XML reader has not been used yet.
        /// Post-Condtion:  XmlNodeType.None    - The reader must be at the end of the input.
        /// </remarks>
        internal ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.XmlReader != null, "this.XmlReader != null");

            // Read the start of the payload up to the first element
            this.ReadPayloadStart();
            this.AssertXmlCondition(XmlNodeType.Element);

            // We need to accept both OData and OData metadata namespace for the "uri" element due to backward compatibility.
            // Per spec the element should be in OData namespace, by WCF DS client was using the metadata namespace, so it's easier to accept that here as well.
            if ((!this.XmlReader.NamespaceEquals(this.XmlReader.ODataNamespace) && !this.XmlReader.NamespaceEquals(this.XmlReader.ODataMetadataNamespace)) ||
                !this.XmlReader.LocalNameEquals(this.ODataUriElementName))
            {
                throw new ODataException(
                    Strings.ODataAtomEntityReferenceLinkDeserializer_InvalidEntityReferenceLinkStartElement(this.XmlReader.LocalName, this.XmlReader.NamespaceURI));
            }

            ODataEntityReferenceLink entityReferenceLink = this.ReadUriElement();

            // Read the payload end
            this.ReadPayloadEnd();
            this.XmlReader.AssertNotBuffering();

            return entityReferenceLink;
        }

        /// <summary>
        /// Verifies that the specified element was not yet found in the entity reference links element.
        /// </summary>
        /// <param name="elementsFoundBitField">The bit field which stores which elements of an inner error were found so far.</param>
        /// <param name="elementFoundBitMask">The bit mask for the element to check.</param>
        /// <param name="elementNamespace">The namespace name of the element ot check (used for error reporting).</param>
        /// <param name="elementName">The name of the element to check (used for error reporting).</param>
        private static void VerifyEntityReferenceLinksElementNotFound(
            ref DuplicateEntityReferenceLinksElementBitMask elementsFoundBitField,
            DuplicateEntityReferenceLinksElementBitMask elementFoundBitMask,
            string elementNamespace,
            string elementName)
        {
            Debug.Assert(((int)elementFoundBitMask & (((int)elementFoundBitMask) - 1)) == 0, "elementFoundBitMask is not a power of 2.");
            Debug.Assert(!string.IsNullOrEmpty(elementName), "!string.IsNullOrEmpty(elementName)");

            if ((elementsFoundBitField & elementFoundBitMask) == elementFoundBitMask)
            {
                throw new ODataException(Strings.ODataAtomEntityReferenceLinkDeserializer_MultipleEntityReferenceLinksElementsWithSameName(elementNamespace, elementName));
            }

            elementsFoundBitField |= elementFoundBitMask;
        }

        /// <summary>
        /// Reads all top-level entity reference links and the (optional) inline count and next link elements.
        /// </summary>
        /// <returns>An <see cref="ODataEntityReferenceLinks"/> instance representing the read entity reference links.</returns>
        /// <remarks>
        /// Pre-Condition:  XmlNodeType.Element - The 'd:links' element.
        /// Post-Condtion:  any                 - The node after the 'd:links' end element (or empty 'd:links' element).
        /// </remarks>
        private ODataEntityReferenceLinks ReadLinksElement()
        {
            Debug.Assert(this.XmlReader != null, "this.XmlReader != null");
            this.AssertXmlCondition(XmlNodeType.Element);
            Debug.Assert(this.XmlReader.NamespaceURI == this.XmlReader.ODataNamespace, "this.XmlReader.NamespaceURI == this.XmlReader.ODataNamespace");
            Debug.Assert(this.XmlReader.LocalName == AtomConstants.ODataLinksElementName, "this.XmlReader.LocalName == AtomConstants.ODataLinksElementName");

            ODataEntityReferenceLinks links = new ODataEntityReferenceLinks();
            List<ODataEntityReferenceLink> linkList = new List<ODataEntityReferenceLink>();
            DuplicateEntityReferenceLinksElementBitMask elementsReadBitmask = DuplicateEntityReferenceLinksElementBitMask.None;

            if (!this.XmlReader.IsEmptyElement)
            {
                // Move to the first child node of the element.
                this.XmlReader.Read();

                do
                {
                    switch (this.XmlReader.NodeType)
                    {
                        case XmlNodeType.EndElement:
                            // end of the <links> element
                            continue;

                        case XmlNodeType.Element:
                            // <m:count>
                            if (this.XmlReader.NamespaceEquals(this.XmlReader.ODataMetadataNamespace) &&
                                this.XmlReader.LocalNameEquals(this.ODataCountElementName) &&
                                this.Version >= ODataVersion.V2)
                            {
                                VerifyEntityReferenceLinksElementNotFound(
                                    ref elementsReadBitmask,
                                    DuplicateEntityReferenceLinksElementBitMask.Count,
                                    this.XmlReader.ODataMetadataNamespace,
                                    AtomConstants.ODataCountElementName);

                                // Note that we allow negative values to be read.
                                long countValue = (long)AtomValueUtils.ReadPrimitiveValue(this.XmlReader, EdmCoreModel.Instance.GetInt64(false));
                                links.Count = countValue;

                                // Read over the end element of the <m:count> element
                                this.XmlReader.Read();

                                continue;
                            }

                            if (this.XmlReader.NamespaceEquals(this.XmlReader.ODataNamespace))
                            {
                                // <d:uri>
                                if (this.XmlReader.LocalNameEquals(this.ODataUriElementName))
                                {
                                    ODataEntityReferenceLink link = this.ReadUriElement();
                                    linkList.Add(link);

                                    continue;
                                }

                                // <d:next>
                                if (this.XmlReader.LocalNameEquals(this.ODataNextElementName) && this.Version >= ODataVersion.V2)
                                {
                                    VerifyEntityReferenceLinksElementNotFound(
                                        ref elementsReadBitmask,
                                        DuplicateEntityReferenceLinksElementBitMask.NextLink,
                                        this.XmlReader.ODataNamespace,
                                        AtomConstants.ODataNextLinkElementName);

                                    // NOTE: get the base URI here before we read the content as string; reading the content as string will move the 
                                    //       reader to the end element and thus we lose the xml:base definition on the element.
                                    Uri xmlBaseUri = this.XmlReader.XmlBaseUri;
                                    string uriString = this.XmlReader.ReadElementValue();
                                    links.NextPageLink = this.ProcessUriFromPayload(uriString, xmlBaseUri);

                                    continue;
                                }
                            }

                            break;
                        default:
                            break;
                    }

                    this.XmlReader.Skip();
                }
                while (this.XmlReader.NodeType != XmlNodeType.EndElement);
            }

            // Read over the end element, or empty start element.
            this.XmlReader.Read();

            links.Links = new ReadOnlyEnumerable<ODataEntityReferenceLink>(linkList);
            return links;
        }

        /// <summary>
        /// Read an entity reference link.
        /// </summary>
        /// <returns>An instance of <see cref="ODataEntityReferenceLink"/> which was read.</returns>
        /// <remarks>
        /// Pre-Condition:  XmlNodeType.Element - the 'd:uri' element to read.
        /// Post-Condition: Any                 - the node after the 'd:uri' element which was read.
        /// </remarks>
        private ODataEntityReferenceLink ReadUriElement()
        {
            Debug.Assert(this.XmlReader != null, "this.XmlReader != null");
            this.AssertXmlCondition(XmlNodeType.Element);
            Debug.Assert(
                this.XmlReader.NamespaceURI == this.XmlReader.ODataNamespace || this.XmlReader.NamespaceURI == this.XmlReader.ODataMetadataNamespace,
                "this.XmlReader.NamespaceURI == this.XmlReader.ODataNamespace  || this.XmlReader.NamespaceURI == this.XmlReader.ODataMetadataNamespace");
            Debug.Assert(this.XmlReader.LocalName == this.ODataUriElementName, "this.XmlReader.LocalName == this.ODataUriElementName");

            ODataEntityReferenceLink link = new ODataEntityReferenceLink();

            // NOTE: get the base URI here before we read the content as string; reading the content as string will move the 
            //       reader to the end element and thus we lose the xml:base definition on the element.
            Uri xmlBaseUri = this.XmlReader.XmlBaseUri;
            string uriString = this.XmlReader.ReadElementValue();
            Debug.Assert(uriString != null, "In ATOM a URI element can never represent a null value.");
            Uri uri = this.ProcessUriFromPayload(uriString, xmlBaseUri);
            link.Url = uri;

            ReaderValidationUtils.ValidateEntityReferenceLink(link);
            return link;
        }
    }
}

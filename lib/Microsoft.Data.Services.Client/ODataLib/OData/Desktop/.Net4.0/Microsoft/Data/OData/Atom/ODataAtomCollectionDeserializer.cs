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
    using System.Diagnostics;
    using System.Xml;
    using Microsoft.Data.Edm;
    using ODataErrorStrings = Microsoft.Data.OData.Strings;
    #endregion Namespaces

    /// <summary>
    /// OData ATOM deserializer for collections.
    /// </summary>
    internal sealed class ODataAtomCollectionDeserializer : ODataAtomPropertyAndValueDeserializer
    {
        /// <summary>Cached duplicate property names checker to use if the items are complex values.</summary>
        private readonly DuplicatePropertyNamesChecker duplicatePropertyNamesChecker;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomInputContext">The ATOM input context to read from.</param>
        internal ODataAtomCollectionDeserializer(ODataAtomInputContext atomInputContext)
            : base(atomInputContext)
        {
            DebugUtils.CheckNoExternalCallers();

            this.duplicatePropertyNamesChecker = this.CreateDuplicatePropertyNamesChecker();
        }

        /// <summary>
        /// Reads the start element of a collection.
        /// </summary>
        /// <param name="isCollectionElementEmpty">true, if the collection element is empty; false otherwise.</param>
        /// <returns>An <see cref="ODataCollectionStart"/> representing the collection-level information. Currently this only contains 
        /// the name of the collection.</returns>
        /// <remarks>
        /// Pre-Condition:   XmlNodeType.Element - The start element of the collection.
        /// Post-Condition:  Any                 - The next node after the start element node of the collection or the 
        ///                                        empty collection element node.
        /// </remarks>
        internal ODataCollectionStart ReadCollectionStart(out bool isCollectionElementEmpty)
        {
            DebugUtils.CheckNoExternalCallers();
            this.XmlReader.AssertNotBuffering();
            this.AssertXmlCondition(XmlNodeType.Element);

            if (!this.XmlReader.NamespaceEquals(this.XmlReader.ODataNamespace))
            {
                throw new ODataException(ODataErrorStrings.ODataAtomCollectionDeserializer_TopLevelCollectionElementWrongNamespace(this.XmlReader.NamespaceURI, this.XmlReader.ODataNamespace));
            }
         
            while (this.XmlReader.MoveToNextAttribute())
            {
                if (this.XmlReader.NamespaceEquals(this.XmlReader.ODataMetadataNamespace) &&
                   (this.XmlReader.LocalNameEquals(this.AtomTypeAttributeName) ||
                   (this.XmlReader.LocalNameEquals(this.ODataNullAttributeName))))
                {
                    // make sure that m:type or m:null attributes are not present in the root element of the collection.
                    throw new ODataException(ODataErrorStrings.ODataAtomCollectionDeserializer_TypeOrNullAttributeNotAllowed);
                }
            }

            // ignore all other attributes.
            this.XmlReader.MoveToElement();

            ODataCollectionStart collectionStart = new ODataCollectionStart();

            // we don't need to validate the collection name because all valid XML local names 
            // are also valid collection names and the XML validity is checked by the XmlReader.
            collectionStart.Name = this.XmlReader.LocalName;

            isCollectionElementEmpty = this.XmlReader.IsEmptyElement;

            if (!isCollectionElementEmpty)
            {
                // if the collection start element is not an empty element than read over the 
                // start element.
                this.XmlReader.Read();
            }

            return collectionStart;
        }

        /// <summary>
        /// Reads the end of a collection.
        /// </summary>
        /// <remarks>
        /// Pre-condition:  XmlNodeType.EndElement - The end element of the collection.
        ///                 XmlNodeType.Element    - The start element of the collection, if the element is empty.
        /// Post-condition: Any                    - Next node after the end element of the collection.
        /// </remarks>
        internal void ReadCollectionEnd()
        {
            DebugUtils.CheckNoExternalCallers();
            this.XmlReader.AssertNotBuffering();
            this.AssertXmlCondition(true, XmlNodeType.EndElement);

            // read over the end tag of the collection or the start tag if the collection is empty.
            this.XmlReader.Read();

            this.XmlReader.AssertNotBuffering();
        }

        /// <summary>
        /// Reads an item in the collection. 
        /// </summary>
        /// <param name="expectedItemType">The expected type of the item to read.</param>
        /// <param name="collectionValidator">The collection validator instance if no expected item type has been specified; otherwise null.</param>
        /// <returns>The value of the collection item that was read; this can be an ODataComplexValue, a primitive value or 'null'.</returns>
        /// <remarks>
        /// Pre-Condition:  XmlNodeType.Element    - The start element node of the item in the collection.
        /// Post-Condition: Any                    - The next node after the end tag of the item.
        /// </remarks>
        internal object ReadCollectionItem(IEdmTypeReference expectedItemType, CollectionWithoutExpectedTypeValidator collectionValidator)
        {
            DebugUtils.CheckNoExternalCallers();
            this.XmlReader.AssertNotBuffering();
            this.AssertXmlCondition(XmlNodeType.Element);

            // the caller should guarantee that we are reading elements in the OData namespace or the custom namespace specified through the reader settings.
            Debug.Assert(this.XmlReader.NamespaceEquals(this.XmlReader.ODataNamespace), "The 'element' node should be in the OData Namespace or in the user specified Namespace");

            // make sure that the item is named as 'element'.
            if (!this.XmlReader.LocalNameEquals(this.ODataCollectionItemElementName))
            {
                throw new ODataException(ODataErrorStrings.ODataAtomCollectionDeserializer_WrongCollectionItemElementName(this.XmlReader.LocalName, this.XmlReader.ODataNamespace));
            }

            // We don't support EPM for collections so it is fine to say that EPM is not present
            object item = this.ReadNonEntityValue(expectedItemType, this.duplicatePropertyNamesChecker, collectionValidator, /*validateNullValue*/ true, /* epmPresent */ false);

            // read over the end tag of the element or the start tag if the element was empty.
            this.XmlReader.Read();

            this.XmlReader.AssertNotBuffering();

            return item;
        }

        /// <summary>
        /// Reads from the Xml reader skipping all nodes until an Element or an EndElement in the OData namespace  
        /// is found or the reader.EOF is reached.
        /// </summary>
        internal void SkipToElementInODataNamespace()
        {
            DebugUtils.CheckNoExternalCallers();
            this.XmlReader.AssertNotBuffering();

            do
            {
                switch (this.XmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (this.XmlReader.NamespaceEquals(this.XmlReader.ODataNamespace))
                        {
                            return;
                        }

                        // skip anything which is not in the OData Namespace.
                        this.XmlReader.Skip();

                        break;

                    case XmlNodeType.EndElement:
                        return;

                    default:
                        this.XmlReader.Skip();
                        break;
                }
            }
            while (!this.XmlReader.EOF);
        }
    }
}

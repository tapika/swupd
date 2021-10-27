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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using System.Xml;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using ODataErrorStrings = Microsoft.Data.OData.Strings;
    #endregion Namespaces

    /// <summary>
    /// ODataCollectionWriter for the ATOM format.
    /// </summary>
    internal sealed class ODataAtomCollectionWriter : ODataCollectionWriterCore
    {
        /// <summary>The output context to write to.</summary>
        private readonly ODataAtomOutputContext atomOutputContext;

        /// <summary>The collection serializer to use for writing.</summary>
        private readonly ODataAtomCollectionSerializer atomCollectionSerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomOutputContext">The output context to write to.</param>
        /// <param name="itemTypeReference">The item type of the collection being written or null if no metadata is available.</param>
        internal ODataAtomCollectionWriter(ODataAtomOutputContext atomOutputContext, IEdmTypeReference itemTypeReference)
            : base(atomOutputContext, itemTypeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(atomOutputContext != null, "atomOutputContext != null");

            this.atomOutputContext = atomOutputContext;
            this.atomCollectionSerializer = new ODataAtomCollectionSerializer(atomOutputContext);
        }

        /// <summary>
        /// Check if the object has been disposed; called from all public API methods. Throws an ObjectDisposedException if the object
        /// has already been disposed.
        /// </summary>
        protected override void VerifyNotDisposed()
        {
            this.atomOutputContext.VerifyNotDisposed();
        }

        /// <summary>
        /// Flush the output.
        /// </summary>
        protected override void FlushSynchronously()
        {
            this.atomOutputContext.Flush();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Flush the output.
        /// </summary>
        /// <returns>Task representing the pending flush operation.</returns>
        protected override Task FlushAsynchronously()
        {
            return this.atomOutputContext.FlushAsync();
        }
#endif

        /// <summary>
        /// Start writing an OData payload.
        /// </summary>
        protected override void StartPayload()
        {
            this.atomCollectionSerializer.WritePayloadStart();
        }

        /// <summary>
        /// Finish writing an OData payload.
        /// </summary>
        protected override void EndPayload()
        {
            // This method is only called if no error has been written so it is safe to
            // call WriteEndDocument() here (since it closes all open elements which we don't want in error state)
            this.atomCollectionSerializer.WritePayloadEnd();
        }

        /// <summary>
        /// Start writing a collection.
        /// </summary>
        /// <param name="collectionStart">The <see cref="ODataCollectionStart"/> representing the collection.</param>
        protected override void StartCollection(ODataCollectionStart collectionStart)
        {
            Debug.Assert(collectionStart != null, "collection != null");

            string collectionName = collectionStart.Name;
            if (collectionName == null)
            {
                // null collection names are not allowed in ATOM
                throw new ODataException(ODataErrorStrings.ODataAtomCollectionWriter_CollectionNameMustNotBeNull);
            }

            // Note that we don't perform metadata validation of the name of the collection.
            // This is because there are multiple possibilities (service operation, action, function, top-level property)
            // and without more information we can't know which one to look for.

            // <collectionName>
            this.atomOutputContext.XmlWriter.WriteStartElement(collectionName, this.atomCollectionSerializer.MessageWriterSettings.WriterBehavior.ODataNamespace);

            // xmlns:="ODataNamespace"
            this.atomOutputContext.XmlWriter.WriteAttributeString(
                AtomConstants.XmlnsNamespacePrefix,
                AtomConstants.XmlNamespacesNamespace,
                this.atomCollectionSerializer.MessageWriterSettings.WriterBehavior.ODataNamespace);

            this.atomCollectionSerializer.WriteDefaultNamespaceAttributes(
                ODataAtomSerializer.DefaultNamespaceFlags.ODataMetadata |
                ODataAtomSerializer.DefaultNamespaceFlags.Gml |
                ODataAtomSerializer.DefaultNamespaceFlags.GeoRss);
        }

        /// <summary>
        /// Finish writing a collection.
        /// </summary>
        protected override void EndCollection()
        {
            // </collectionName>
            this.atomOutputContext.XmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes a collection item (either primitive or complex)
        /// </summary>
        /// <param name="item">The collection item to write.</param>
        /// <param name="expectedItemType">The expected type of the collection item or null if no expected item type exists.</param>
        protected override void WriteCollectionItem(object item, IEdmTypeReference expectedItemType)
        {
            // <d:element>
            this.atomOutputContext.XmlWriter.WriteStartElement(AtomConstants.ODataCollectionItemElementName, this.atomCollectionSerializer.MessageWriterSettings.WriterBehavior.ODataNamespace);

            if (item == null)
            {
                ValidationUtils.ValidateNullCollectionItem(expectedItemType, this.atomOutputContext.MessageWriterSettings.WriterBehavior);

                // NOTE can't use ODataAtomWriterUtils.WriteNullAttribute because that method assumes the
                //      default 'm' prefix for the metadata namespace.
                this.atomOutputContext.XmlWriter.WriteAttributeString(
                    AtomConstants.ODataNullAttributeName,
                    AtomConstants.ODataMetadataNamespace,
                    AtomConstants.AtomTrueLiteral);
            }
            else
            {
                ODataComplexValue complexValue = item as ODataComplexValue;
                if (complexValue != null)
                {
                    this.atomCollectionSerializer.AssertRecursionDepthIsZero();
                    this.atomCollectionSerializer.WriteComplexValue(
                        complexValue,
                        expectedItemType,
                        false /* isOpenPropertyType */,
                        true  /* isWritingCollection */,
                        null  /* beforePropertiesAction */,
                        null  /* afterPropertiesAction */,
                        this.DuplicatePropertyNamesChecker,
                        this.CollectionValidator,
                        null  /* epmValueCache */,
                        null  /* epmSourcePathSegment */,
                        null  /* projectedProperties */);
                    this.atomCollectionSerializer.AssertRecursionDepthIsZero();
                    this.DuplicatePropertyNamesChecker.Clear();
                }
                else
                {
                    Debug.Assert(!(item is ODataCollectionValue), "!(item is ODataCollectionValue)");
                    Debug.Assert(!(item is ODataStreamReferenceValue), "!(item is ODataStreamReferenceValue)");

                    // Note: Currently there is no way for a user to control primitive type information when the primitive values are part of a collection.
                    this.atomCollectionSerializer.WritePrimitiveValue(item, this.CollectionValidator, expectedItemType, /*serializationTypeNameAnnotation*/ null);
                }
            }

            // </d:element>
            this.atomOutputContext.XmlWriter.WriteEndElement();
        }
    }
}

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
    using System.Linq;
    using System.Xml;
    using Microsoft.Data.Edm;
    #endregion Namespaces

    /// <summary>
    /// OData ATOM serializer for entries and feeds.
    /// </summary>
    internal sealed class ODataAtomEntryAndFeedSerializer : ODataAtomPropertyAndValueSerializer
    {
        /// <summary>
        /// The serializer for writing ATOM metadata for entries.
        /// </summary>
        private readonly ODataAtomEntryMetadataSerializer atomEntryMetadataSerializer;

        /// <summary>
        /// The serializer for writing ATOM metadata for feeds.
        /// </summary>
        private readonly ODataAtomFeedMetadataSerializer atomFeedMetadataSerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomOutputContext">The output context to write to.</param>
        internal ODataAtomEntryAndFeedSerializer(ODataAtomOutputContext atomOutputContext)
            : base(atomOutputContext)
        {
            DebugUtils.CheckNoExternalCallers();

            this.atomEntryMetadataSerializer = new ODataAtomEntryMetadataSerializer(atomOutputContext);
            this.atomFeedMetadataSerializer = new ODataAtomFeedMetadataSerializer(atomOutputContext);
        }

        /// <summary>
        /// Writes the start element for the m:properties element on the entry.
        /// </summary>
        internal void WriteEntryPropertiesStart()
        {
            DebugUtils.CheckNoExternalCallers();

            // <m:properties> if required
            this.XmlWriter.WriteStartElement(
                AtomConstants.ODataMetadataNamespacePrefix,
                AtomConstants.AtomPropertiesElementName,
                AtomConstants.ODataMetadataNamespace);
        }

        /// <summary>
        /// Writes the end element for the m:properties element on the entry.
        /// </summary>
        internal void WriteEntryPropertiesEnd()
        {
            DebugUtils.CheckNoExternalCallers();

            // </m:properties>
            this.XmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes the type name category element for the entry.
        /// </summary>
        /// <param name="typeName">The type name to write.</param>
        /// <param name="entryMetadata">The entry metadata if available.</param>
        internal void WriteEntryTypeName(string typeName, AtomEntryMetadata entryMetadata)
        {
            DebugUtils.CheckNoExternalCallers();

            if (typeName != null)
            {
                AtomCategoryMetadata mergedCategoryMetadata = ODataAtomWriterMetadataUtils.MergeCategoryMetadata(
                    entryMetadata == null ? null : entryMetadata.CategoryWithTypeName,
                    typeName,
                    this.MessageWriterSettings.WriterBehavior.ODataTypeScheme);
                this.atomEntryMetadataSerializer.WriteCategory(mergedCategoryMetadata);
            }
        }

        /// <summary>
        /// Write the ATOM metadata for an entry
        /// </summary>
        /// <param name="entryMetadata">The entry metadata to write.</param>
        /// <param name="epmEntryMetadata">The ATOM metadata for the entry which came from EPM.</param>
        /// <param name="updatedTime">Value for the atom:updated element.</param>
        internal void WriteEntryMetadata(AtomEntryMetadata entryMetadata, AtomEntryMetadata epmEntryMetadata, string updatedTime)
        {
            DebugUtils.CheckNoExternalCallers();

            this.atomEntryMetadataSerializer.WriteEntryMetadata(entryMetadata, epmEntryMetadata, updatedTime);
        }

        /// <summary>
        /// Writes the entry atom:id element.
        /// </summary>
        /// <param name="entryId">The value of the ODataEntry.Id property to write.</param>
        internal void WriteEntryId(string entryId)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entryId == null || entryId.Length > 0, "We should have validated that the Id is not empty by now.");

            // <atom:id>idValue</atom:id>
            // NOTE: do not generate a relative Uri for the ID; it is independent of xml:base
            this.WriteElementWithTextContent(
                AtomConstants.AtomNamespacePrefix,
                AtomConstants.AtomIdElementName,
                AtomConstants.AtomNamespace,
                entryId);
        }

        /// <summary>
        /// Writes the read link element for an entry.
        /// </summary>
        /// <param name="readLink">The read link URL.</param>
        /// <param name="entryMetadata">The ATOM entry metatadata for the current entry.</param>
        internal void WriteEntryReadLink(Uri readLink, AtomEntryMetadata entryMetadata)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(readLink != null, "readLink != null");

            // we allow additional link metadata to specify the title, type, hreflang or length of the link
            AtomLinkMetadata readLinkMetadata = entryMetadata == null ? null : entryMetadata.SelfLink;

            // <link rel="self" href="LinkHRef" ... />
            this.WriteReadOrEditLink(readLink, readLinkMetadata, AtomConstants.AtomSelfRelationAttributeValue);
        }

        /// <summary>
        /// Writes the edit link element for an entry.
        /// </summary>
        /// <param name="editLink">The edit link URL.</param>
        /// <param name="entryMetadata">The ATOM entry metatadata for the current entry.</param>
        internal void WriteEntryEditLink(Uri editLink, AtomEntryMetadata entryMetadata)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(editLink != null, "editLink != null");

            // we allow additional link metadata to specify the title, type, hreflang or length of the link
            AtomLinkMetadata editLinkMetadata = entryMetadata == null ? null : entryMetadata.EditLink;

            // <link rel="edit" href="LinkHRef" .../>
            this.WriteReadOrEditLink(editLink, editLinkMetadata, AtomConstants.AtomEditRelationAttributeValue);
        }

        /// <summary>
        /// Writes the edit-media link for an entry.
        /// </summary>
        /// <param name="mediaResource">The media resource representing the MR of the entry to write.</param>
        internal void WriteEntryMediaEditLink(ODataStreamReferenceValue mediaResource)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(mediaResource != null, "mediaResource != null");

            Uri mediaEditLink = mediaResource.EditLink;
            Debug.Assert(mediaEditLink != null || mediaResource.ETag == null, "The default stream edit link and etag should have been validated by now.");
            if (mediaEditLink != null)
            {
                AtomStreamReferenceMetadata streamReferenceMetadata = mediaResource.GetAnnotation<AtomStreamReferenceMetadata>();
                AtomLinkMetadata mediaEditMetadata = streamReferenceMetadata == null ? null : streamReferenceMetadata.EditLink;
                AtomLinkMetadata mergedLinkMetadata =
                    ODataAtomWriterMetadataUtils.MergeLinkMetadata(
                        mediaEditMetadata,
                        AtomConstants.AtomEditMediaRelationAttributeValue,
                        mediaEditLink,
                        null /* title */,
                        null /* mediaType */);

                this.atomEntryMetadataSerializer.WriteAtomLink(mergedLinkMetadata, mediaResource.ETag);
            }
        }

        /// <summary>
        /// Write the metadata for an OData association link; makes sure any duplicate of the link's values duplicated in metadata are equal.
        /// </summary>
        /// <param name="associationLink">The association link for which to write the metadata.</param>
        /// <param name="owningType">The <see cref="IEdmEntityType"/> instance the association link is defined on.</param>
        /// <param name="duplicatePropertyNamesChecker">The checker instance for duplicate property names.</param>
        /// <param name="projectedProperties">Set of projected properties, or null if all properties should be written.</param>
        internal void WriteAssociationLink(
            ODataAssociationLink associationLink,
            IEdmEntityType owningType,
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker,
            ProjectedPropertiesAnnotation projectedProperties)
        {
            DebugUtils.CheckNoExternalCallers();

            ValidationUtils.ValidateAssociationLinkNotNull(associationLink);
            string associationLinkName = associationLink.Name;
            if (projectedProperties.ShouldSkipProperty(associationLinkName))
            {
                return;
            }

            this.ValidateAssociationLink(associationLink, owningType);
            duplicatePropertyNamesChecker.CheckForDuplicateAssociationLinkNames(associationLink);

            AtomLinkMetadata linkMetadata = associationLink.GetAnnotation<AtomLinkMetadata>();
            string linkRelation = AtomUtils.ComputeODataAssociationLinkRelation(associationLink);
            AtomLinkMetadata mergedLinkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(linkMetadata, linkRelation, associationLink.Url, associationLinkName, MimeConstants.MimeApplicationXml);
            this.atomEntryMetadataSerializer.WriteAtomLink(mergedLinkMetadata, null /* etag*/);
        }

        /// <summary>
        /// Writes the navigation link's start element and atom metadata.
        /// </summary>
        /// <param name="navigationLink">The navigation link to write.</param>
        /// <param name="navigationLinkUrlOverride">Url to use for the navigation link. If this is specified the Url property on the <paramref name="navigationLink"/>
        /// will be ignored. If this parameter is null, the Url from the navigation link is used.</param>
        internal void WriteNavigationLinkStart(ODataNavigationLink navigationLink, Uri navigationLinkUrlOverride)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(navigationLink != null, "navigationLink != null");
            Debug.Assert(!string.IsNullOrEmpty(navigationLink.Name), "The navigation link name was not verified yet.");
            Debug.Assert(navigationLink.Url != null, "The navigation link Url was not verified yet.");
            Debug.Assert(navigationLink.IsCollection.HasValue, "navigationLink.IsCollection.HasValue");

            // <atom:link>
            this.XmlWriter.WriteStartElement(AtomConstants.AtomNamespacePrefix, AtomConstants.AtomLinkElementName, AtomConstants.AtomNamespace);

            string linkRelation = AtomUtils.ComputeODataNavigationLinkRelation(navigationLink);
            string linkType = AtomUtils.ComputeODataNavigationLinkType(navigationLink);
            string linkTitle = navigationLink.Name;

            Uri navigationLinkUrl = navigationLinkUrlOverride ?? navigationLink.Url;
            AtomLinkMetadata linkMetadata = navigationLink.GetAnnotation<AtomLinkMetadata>();
            AtomLinkMetadata mergedMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(linkMetadata, linkRelation, navigationLinkUrl, linkTitle, linkType);
            this.atomEntryMetadataSerializer.WriteAtomLinkAttributes(mergedMetadata, null /* etag */);
        }

        /// <summary>
        /// Write the given feed metadata in atom format
        /// </summary>
        /// <param name="feed">The feed for which to write the meadata or null if it is the metadata of an atom:source element.</param>
        /// <param name="updatedTime">Value for the atom:updated element.</param>
        /// <param name="authorWritten">Set to true if the author element was written, false otherwise.</param>
        internal void WriteFeedMetadata(ODataFeed feed, string updatedTime, out bool authorWritten)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(feed != null, "feed != null");
            Debug.Assert(!string.IsNullOrEmpty(updatedTime), "!string.IsNullOrEmpty(updatedTime)");
#if DEBUG
            DateTimeOffset tempDateTimeOffset;
            Debug.Assert(DateTimeOffset.TryParse(updatedTime, out tempDateTimeOffset), "DateTimeOffset.TryParse(updatedTime, out tempDateTimeOffset)");
#endif

            AtomFeedMetadata feedMetadata = feed.GetAnnotation<AtomFeedMetadata>();

            if (feedMetadata == null)
            {
                // create the required metadata elements with default values.

                // <atom:id>idValue</atom:id>
                Debug.Assert(!string.IsNullOrEmpty(feed.Id), "The feed Id should have been validated by now.");
                this.WriteElementWithTextContent(
                    AtomConstants.AtomNamespacePrefix,
                    AtomConstants.AtomIdElementName,
                    AtomConstants.AtomNamespace,
                    feed.Id);

                // <atom:title></atom:title>
                this.WriteEmptyElement(
                    AtomConstants.AtomNamespacePrefix,
                    AtomConstants.AtomTitleElementName,
                    AtomConstants.AtomNamespace);

                // <atom:updated>dateTimeOffset</atom:updated>
                this.WriteElementWithTextContent(
                    AtomConstants.AtomNamespacePrefix,
                    AtomConstants.AtomUpdatedElementName,
                    AtomConstants.AtomNamespace,
                    updatedTime);

                authorWritten = false;
            }
            else
            {
                this.atomFeedMetadataSerializer.WriteFeedMetadata(feedMetadata, feed, updatedTime, out authorWritten);
            }
        }

        /// <summary>
        /// Writes the default empty author for a feed.
        /// </summary>
        internal void WriteFeedDefaultAuthor()
        {
            DebugUtils.CheckNoExternalCallers();

            this.atomFeedMetadataSerializer.WriteEmptyAuthor();
        }

        /// <summary>
        /// Writes the next page link for a feed.
        /// </summary>
        /// <param name="feed">The feed to write the next page link for.</param>
        internal void WriteFeedNextPageLink(ODataFeed feed)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(feed != null, "feed != null");

            Uri nextPageLink = feed.NextPageLink;
            if (nextPageLink != null)
            {
                // <atom:link rel="next" href="next-page-link" />
                this.WriteFeedLink(
                    feed,
                    AtomConstants.AtomNextRelationAttributeValue,
                    nextPageLink, 
                    (feedMetadata) => feedMetadata == null ? null : feedMetadata.NextPageLink);
            }
        }

        /// <summary>
        /// Writes the delta link for a feed.
        /// </summary>
        /// <param name="feed">The feed to write the delta link for.</param>
        internal void WriteFeedDeltaLink(ODataFeed feed)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(feed != null, "feed != null");

            Uri deltaLink = feed.DeltaLink;
            if (deltaLink != null)
            {
                // <atom:link rel="http://docs.oasis-open.org/odata/ns/delta" href="delta-link" />
                this.WriteFeedLink(
                    feed, 
                    AtomConstants.AtomDeltaRelationAttributeValue, 
                    deltaLink, 
                    (feedMetadata) => feedMetadata == null ? null : feedMetadata.Links.FirstOrDefault(link => link.Relation == AtomConstants.AtomDeltaRelationAttributeValue));
            }
        }

        /// <summary>
        /// Writes a feed link.
        /// </summary>
        /// <param name="feed">The feed that contains the link.</param>
        /// <param name="relation">Relation attribute of the link.</param>
        /// <param name="href">href attribute of the link.</param>
        /// <param name="getLinkMetadata">Function to get the AtomLinkMetadata for the feed link.</param>
        internal void WriteFeedLink(ODataFeed feed, string relation, Uri href, Func<AtomFeedMetadata, AtomLinkMetadata> getLinkMetadata)
        {
            DebugUtils.CheckNoExternalCallers();
            AtomFeedMetadata feedMetadata = feed.GetAnnotation<AtomFeedMetadata>();
            AtomLinkMetadata mergedLink = ODataAtomWriterMetadataUtils.MergeLinkMetadata(
                    getLinkMetadata(feedMetadata),
                    relation,
                    href,
                    null, /*title*/
                    null /*mediaType*/);
            this.atomFeedMetadataSerializer.WriteAtomLink(mergedLink, null);
        }

        /// <summary>
        /// Writes a stream property to the ATOM payload
        /// </summary>
        /// <param name="streamProperty">The stream property to create the payload for.</param>
        /// <param name="owningType">The <see cref="IEdmEntityType"/> instance for which the stream property defined on.</param>
        /// <param name="duplicatePropertyNamesChecker">The checker instance for duplicate property names.</param>
        /// <param name="projectedProperties">Set of projected properties, or null if all properties should be written.</param>
        internal void WriteStreamProperty(
            ODataProperty streamProperty,
            IEdmEntityType owningType,
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker,
            ProjectedPropertiesAnnotation projectedProperties)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(streamProperty != null, "Stream property must not be null.");
            Debug.Assert(streamProperty.Value != null, "The media resource of the stream property must not be null.");

            WriterValidationUtils.ValidatePropertyNotNull(streamProperty);
            string propertyName = streamProperty.Name;
            if (projectedProperties.ShouldSkipProperty(propertyName))
            {
                return;
            }

            WriterValidationUtils.ValidatePropertyName(propertyName);
            duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(streamProperty);
            IEdmProperty edmProperty = WriterValidationUtils.ValidatePropertyDefined(streamProperty.Name, owningType, this.MessageWriterSettings.UndeclaredPropertyBehaviorKinds);
            WriterValidationUtils.ValidateStreamReferenceProperty(streamProperty, edmProperty, this.Version, this.WritingResponse);
            ODataStreamReferenceValue streamReferenceValue = (ODataStreamReferenceValue)streamProperty.Value;
            WriterValidationUtils.ValidateStreamReferenceValue(streamReferenceValue, false /*isDefaultStream*/);
            if (owningType != null && owningType.IsOpen && edmProperty == null)
            {
                ValidationUtils.ValidateOpenPropertyValue(streamProperty.Name, streamReferenceValue, this.MessageWriterSettings.UndeclaredPropertyBehaviorKinds);
            }

            AtomStreamReferenceMetadata streamReferenceMetadata = streamReferenceValue.GetAnnotation<AtomStreamReferenceMetadata>();
            string contentType = streamReferenceValue.ContentType;
            string linkTitle = streamProperty.Name;

            Uri readLink = streamReferenceValue.ReadLink;
            if (readLink != null)
            {
                string readLinkRelation = AtomUtils.ComputeStreamPropertyRelation(streamProperty, false);

                AtomLinkMetadata readLinkMetadata = streamReferenceMetadata == null ? null : streamReferenceMetadata.SelfLink;
                AtomLinkMetadata mergedMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(readLinkMetadata, readLinkRelation, readLink, linkTitle, contentType);
                this.atomEntryMetadataSerializer.WriteAtomLink(mergedMetadata, null /* etag */);
            }

            Uri editLink = streamReferenceValue.EditLink;
            if (editLink != null)
            {
                string editLinkRelation = AtomUtils.ComputeStreamPropertyRelation(streamProperty, true);

                AtomLinkMetadata editLinkMetadata = streamReferenceMetadata == null ? null : streamReferenceMetadata.EditLink;
                AtomLinkMetadata mergedMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(editLinkMetadata, editLinkRelation, editLink, linkTitle, contentType);
                this.atomEntryMetadataSerializer.WriteAtomLink(mergedMetadata, streamReferenceValue.ETag);
            }
        }

        /// <summary>
        /// Writes an operation (an action or a function).
        /// </summary>
        /// <param name="operation">The association link to write.</param>
        internal void WriteOperation(ODataOperation operation)
        {
            DebugUtils.CheckNoExternalCallers();

            // checks for null and validates its properties
            WriterValidationUtils.ValidateCanWriteOperation(operation, this.WritingResponse);
            ValidationUtils.ValidateOperationMetadataNotNull(operation);
            ValidationUtils.ValidateOperationTargetNotNull(operation);

            string elementName;
            if (operation is ODataAction)
            {
                elementName = AtomConstants.ODataActionElementName;
            }
            else
            {
                Debug.Assert(operation is ODataFunction, "operation is either an ODataAction or an ODataFunction");
                elementName = AtomConstants.ODataFunctionElementName;
            }

            // <m:action ... or <m:function ...
            this.XmlWriter.WriteStartElement(
                AtomConstants.ODataMetadataNamespacePrefix,
                elementName,
                AtomConstants.ODataMetadataNamespace);

            // write the attributes of the action/function

            // The metadata URI of an ODataOperation can be relative.
            string metadataAttributeValue = this.UriToUrlAttributeValue(operation.Metadata, /*failOnRelativeUriWithoutBaseUri*/ false);

            this.XmlWriter.WriteAttributeString(AtomConstants.ODataOperationMetadataAttribute, metadataAttributeValue);

            if (operation.Title != null)
            {
                this.XmlWriter.WriteAttributeString(AtomConstants.ODataOperationTitleAttribute, operation.Title);
            }

            string targetAttribute = this.UriToUrlAttributeValue(operation.Target);
            this.XmlWriter.WriteAttributeString(AtomConstants.ODataOperationTargetAttribute, targetAttribute);

            // </m:action> or </m:function>
            this.XmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes the self or edit link.
        /// </summary>
        /// <param name="link">Uri object for the link.</param>
        /// <param name="linkMetadata">The atom link metadata for the link to specify title, type, hreflang and length of the link.</param>
        /// <param name="linkRelation">Relationship value. Either "edit" or "self".</param>
        private void WriteReadOrEditLink(
            Uri link,
            AtomLinkMetadata linkMetadata,
            string linkRelation)
        {
            if (link != null)
            {
                AtomLinkMetadata mergedLinkMetadata = ODataAtomWriterMetadataUtils.MergeLinkMetadata(
                    linkMetadata,
                    linkRelation,
                    link,
                    null /* title */,
                    null /* media type */);

                this.atomEntryMetadataSerializer.WriteAtomLink(mergedLinkMetadata, null /* etag */);
            }
        }
    }
}

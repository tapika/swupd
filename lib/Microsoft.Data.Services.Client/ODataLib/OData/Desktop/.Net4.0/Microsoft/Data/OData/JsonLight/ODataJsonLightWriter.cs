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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Evaluation;
    using Microsoft.Data.OData.Json;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Implementation of the ODataWriter for the JsonLight format.
    /// </summary>
    internal sealed class ODataJsonLightWriter : ODataWriterCore
    {
        /// <summary>
        /// The output context to write to.
        /// </summary>
        private readonly ODataJsonLightOutputContext jsonLightOutputContext;

        /// <summary>
        /// The JsonLight entry and feed serializer to use.
        /// </summary>
        private readonly ODataJsonLightEntryAndFeedSerializer jsonLightEntryAndFeedSerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLightOutputContext">The output context to write to.</param>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <param name="writingFeed">true if the writer is created for writing a feed; false when it is created for writing an entry.</param>
        internal ODataJsonLightWriter(
            ODataJsonLightOutputContext jsonLightOutputContext,
            IEdmEntitySet entitySet,
            IEdmEntityType entityType,
            bool writingFeed)
            : base(jsonLightOutputContext, entitySet, entityType, writingFeed)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(jsonLightOutputContext != null, "jsonLightOutputContext != null");

            this.jsonLightOutputContext = jsonLightOutputContext;
            this.jsonLightEntryAndFeedSerializer = new ODataJsonLightEntryAndFeedSerializer(this.jsonLightOutputContext);
        }

        /// <summary>
        /// Returns the current JsonLightEntryScope.
        /// </summary>
        private JsonLightEntryScope CurrentEntryScope
        {
            get
            {
                JsonLightEntryScope currentJsonLightEntryScope = this.CurrentScope as JsonLightEntryScope;
                Debug.Assert(currentJsonLightEntryScope != null, "Asking for JsonLightEntryScope when the current scope is not an JsonLightEntryScope.");
                return currentJsonLightEntryScope;
            }
        }

        /// <summary>
        /// Returns the current JsonLightFeedScope.
        /// </summary>
        private JsonLightFeedScope CurrentFeedScope
        {
            get
            {
                JsonLightFeedScope currentJsonLightFeedScope = this.CurrentScope as JsonLightFeedScope;
                Debug.Assert(currentJsonLightFeedScope != null, "Asking for JsonFeedScope when the current scope is not a JsonFeedScope.");
                return currentJsonLightFeedScope;
            }
        }

        /// <summary>
        /// Check if the object has been disposed; called from all public API methods. Throws an ObjectDisposedException if the object
        /// has already been disposed.
        /// </summary>
        protected override void VerifyNotDisposed()
        {
            this.jsonLightOutputContext.VerifyNotDisposed();
        }

        /// <summary>
        /// Flush the output.
        /// </summary>
        protected override void FlushSynchronously()
        {
            this.jsonLightOutputContext.Flush();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Flush the output.
        /// </summary>
        /// <returns>Task representing the pending flush operation.</returns>
        protected override Task FlushAsynchronously()
        {
            return this.jsonLightOutputContext.FlushAsync();
        }
#endif

        /// <summary>
        /// Starts writing a payload (called exactly once before anything else)
        /// </summary>
        protected override void StartPayload()
        {
            this.jsonLightEntryAndFeedSerializer.WritePayloadStart();
        }

        /// <summary>
        /// Ends writing a payload (called exactly once after everything else in case of success)
        /// </summary>
        protected override void EndPayload()
        {
            this.jsonLightEntryAndFeedSerializer.WritePayloadEnd();
        }

        /// <summary>
        /// Place where derived writers can perform custom steps before the entry is writen, at the begining of WriteStartEntryImplementation.
        /// </summary>
        /// <param name="entry">Entry to write.</param>
        /// <param name="typeContext">The context object to answer basic questions regarding the type of the entry or feed.</param>
        /// <param name="selectedProperties">The selected properties of this scope.</param>
        protected override void PrepareEntryForWriteStart(ODataEntry entry, ODataFeedAndEntryTypeContext typeContext, SelectedPropertiesNode selectedProperties)
        {
            if (this.jsonLightOutputContext.MessageWriterSettings.AutoComputePayloadMetadataInJson)
            {
                EntryScope entryScope = (EntryScope)this.CurrentScope;
                Debug.Assert(entryScope != null, "entryScope != null");

                ODataEntityMetadataBuilder builder = this.jsonLightOutputContext.MetadataLevel.CreateEntityMetadataBuilder(
                    entry, 
                    typeContext, 
                    entryScope.SerializationInfo, 
                    entryScope.EntityType, 
                    selectedProperties, 
                    this.jsonLightOutputContext.WritingResponse,
                    this.jsonLightOutputContext.MessageWriterSettings.AutoGeneratedUrlsShouldPutKeyValueInDedicatedSegment);
                this.jsonLightOutputContext.MetadataLevel.InjectMetadataBuilder(entry, builder);
            }
        }

        /// <summary>
        /// Validates the media resource on the entry.
        /// </summary>
        /// <param name="entry">The entry to validate.</param>
        /// <param name="entityType">The entity type of the entry.</param>
        protected override void ValidateEntryMediaResource(ODataEntry entry, IEdmEntityType entityType)
        {
            if (this.jsonLightOutputContext.MessageWriterSettings.AutoComputePayloadMetadataInJson && this.jsonLightOutputContext.MetadataLevel is JsonNoMetadataLevel)
            {
                // entry.MediaResource is always null for NoMetadata mode. Skip the media resource validation.
            }
            else
            {
                base.ValidateEntryMediaResource(entry, entityType);
            }
        }

        /// <summary>
        /// Start writing an entry.
        /// </summary>
        /// <param name="entry">The entry to write.</param>
        protected override void StartEntry(ODataEntry entry)
        {
            ODataNavigationLink parentNavLink = this.ParentNavigationLink;
            if (parentNavLink != null)
            {
                // Write the property name of an expanded navigation property to start the value. 
                this.jsonLightOutputContext.JsonWriter.WriteName(parentNavLink.Name);
            }

            if (entry == null)
            {
                Debug.Assert(
                    parentNavLink != null && !parentNavLink.IsCollection.Value,
                        "when entry == null, it has to be and expanded single entry navigation");

                // this is a null expanded single entry and it is null, so write a JSON null as value.
                this.jsonLightOutputContext.JsonWriter.WriteValue(null);
                return;
            }

            // Write just the object start, nothing else, since we might not have complete information yet
            this.jsonLightOutputContext.JsonWriter.StartObjectScope();

            JsonLightEntryScope entryScope = this.CurrentEntryScope;
            if (this.IsTopLevel)
            {
                // Write odata.metadata
                this.jsonLightEntryAndFeedSerializer.TryWriteEntryMetadataUri(entryScope.GetOrCreateTypeContext(this.jsonLightOutputContext.Model, this.jsonLightOutputContext.WritingResponse));
            }

            // Write the annotation group in responses (if any)
            this.jsonLightEntryAndFeedSerializer.WriteAnnotationGroup(entry);

            // Write the metadata
            this.jsonLightEntryAndFeedSerializer.WriteEntryStartMetadataProperties(entryScope);
            this.jsonLightEntryAndFeedSerializer.WriteEntryMetadataProperties(entryScope);

            // Write custom instance annotations
            this.jsonLightEntryAndFeedSerializer.InstanceAnnotationWriter.WriteInstanceAnnotations(entry.InstanceAnnotations, entryScope.InstanceAnnotationWriteTracker);

        }

        /// <summary>
        /// Finish writing an entry.
        /// </summary>
        /// <param name="entry">The entry to write.</param>
        protected override void EndEntry(ODataEntry entry)
        {
            if (entry == null)
            {
                Debug.Assert(
                    this.ParentNavigationLink != null && this.ParentNavigationLink.IsCollection.HasValue && !this.ParentNavigationLink.IsCollection.Value,
                        "when entry == null, it has to be and expanded single entry navigation");

                // this is a null expanded single entry and it is null, JSON null should be written as value in StartEntry()
                return;
            }

            // Get the projected properties
            JsonLightEntryScope entryScope = this.CurrentEntryScope;
            ProjectedPropertiesAnnotation projectedProperties = GetProjectedPropertiesAnnotation(entryScope);

            this.jsonLightEntryAndFeedSerializer.WriteEntryMetadataProperties(entryScope);

            // Write custom instance annotations
            this.jsonLightEntryAndFeedSerializer.InstanceAnnotationWriter.WriteInstanceAnnotations(entry.InstanceAnnotations, entryScope.InstanceAnnotationWriteTracker);

            this.jsonLightEntryAndFeedSerializer.WriteEntryEndMetadataProperties(entryScope, entryScope.DuplicatePropertyNamesChecker);

            // Write the properties
            this.jsonLightEntryAndFeedSerializer.JsonLightValueSerializer.AssertRecursionDepthIsZero();
            this.jsonLightEntryAndFeedSerializer.WriteProperties(
                this.EntryEntityType,
                entry.Properties,
                false /* isComplexValue */,
                this.DuplicatePropertyNamesChecker,
                projectedProperties);
            this.jsonLightEntryAndFeedSerializer.JsonLightValueSerializer.AssertRecursionDepthIsZero();

            // Close the object scope
            this.jsonLightOutputContext.JsonWriter.EndObjectScope();
        }

        /// <summary>
        /// Start writing a feed.
        /// </summary>
        /// <param name="feed">The feed to write.</param>
        protected override void StartFeed(ODataFeed feed)
        {
            Debug.Assert(feed != null, "feed != null");

            IJsonWriter jsonWriter = this.jsonLightOutputContext.JsonWriter;
            if (this.ParentNavigationLink == null)
            {
                // Top-level feed.
                // "{"
                jsonWriter.StartObjectScope();

                // odata.metadata
                this.jsonLightEntryAndFeedSerializer.TryWriteFeedMetadataUri(this.CurrentFeedScope.GetOrCreateTypeContext(this.jsonLightOutputContext.Model, this.jsonLightOutputContext.WritingResponse));

                if (this.jsonLightOutputContext.WritingResponse)
                {
                    // Write the inline count if it's available.
                    this.WriteFeedCount(feed, /*propertyName*/null);

                    // Write the next link if it's available.
                    this.WriteFeedNextLink(feed, /*propertyName*/null);

                    // Write the delta link if it's available.
                    this.WriteFeedDeltaLink(feed);
                }

                // Write custom instance annotations
                this.jsonLightEntryAndFeedSerializer.InstanceAnnotationWriter.WriteInstanceAnnotations(feed.InstanceAnnotations, this.CurrentFeedScope.InstanceAnnotationWriteTracker);

                // "value":
                jsonWriter.WriteValuePropertyName();

                // Start array which will hold the entries in the feed.
                jsonWriter.StartArrayScope();
            }
            else
            {
                // Expanded feed.
                Debug.Assert(
                    this.ParentNavigationLink != null && this.ParentNavigationLink.IsCollection.HasValue && this.ParentNavigationLink.IsCollection.Value,
                    "We should have verified that feeds can only be written into IsCollection = true links in requests.");
                string propertyName = this.ParentNavigationLink.Name;

                this.ValidateNoDeltaLinkForExpandedFeed(feed);
                this.ValidateNoCustomInstanceAnnotationsForExpandedFeed(feed);

                if (this.jsonLightOutputContext.WritingResponse)
                {
                    // Write the inline count if it's available.
                    this.WriteFeedCount(feed, propertyName);

                    // Write the next link if it's available.
                    this.WriteFeedNextLink(feed, propertyName);

                    // And then write the property name to start the value. 
                    jsonWriter.WriteName(propertyName);

                    // Start array which will hold the entries in the feed.
                    jsonWriter.StartArrayScope();
                }
                else
                {
                    JsonLightNavigationLinkScope navigationLinkScope = (JsonLightNavigationLinkScope)this.ParentNavigationLinkScope;
                    if (!navigationLinkScope.FeedWritten)
                    {
                        // Close the entity reference link array (if written)
                        if (navigationLinkScope.EntityReferenceLinkWritten)
                        {
                            jsonWriter.EndArrayScope();
                        }

                        // And then write the property name to start the value. 
                        jsonWriter.WriteName(propertyName);

                        // Start array which will hold the entries in the feed.
                        jsonWriter.StartArrayScope();

                        navigationLinkScope.FeedWritten = true;
                    }
                }
            }
        }

        /// <summary>
        /// Finish writing a feed.
        /// </summary>
        /// <param name="feed">The feed to write.</param>
        protected override void EndFeed(ODataFeed feed)
        {
            Debug.Assert(feed != null, "feed != null");

            bool isTopLevel = this.ParentNavigationLink == null;
            if (isTopLevel)
            {
                // End the array which holds the entries in the feed.
                this.jsonLightOutputContext.JsonWriter.EndArrayScope();

                // Write custom instance annotations
                this.jsonLightEntryAndFeedSerializer.InstanceAnnotationWriter.WriteInstanceAnnotations(feed.InstanceAnnotations, this.CurrentFeedScope.InstanceAnnotationWriteTracker);

                if (this.jsonLightOutputContext.WritingResponse)
                {
                    // Write the inline count if it's available.
                    this.WriteFeedCount(feed, /*propertyName*/null);

                    // Write the next link if it's available.
                    this.WriteFeedNextLink(feed, /*propertyName*/null);

                    // Write the delta link if it's available.
                    this.WriteFeedDeltaLink(feed);
                }

                // Close the object wrapper.
                this.jsonLightOutputContext.JsonWriter.EndObjectScope();
            }
            else
            {
                Debug.Assert(
                    this.ParentNavigationLink != null && this.ParentNavigationLink.IsCollection.HasValue && this.ParentNavigationLink.IsCollection.Value,
                    "We should have verified that feeds can only be written into IsCollection = true links in requests.");
                string propertyName = this.ParentNavigationLink.Name;

                this.ValidateNoDeltaLinkForExpandedFeed(feed);
                this.ValidateNoCustomInstanceAnnotationsForExpandedFeed(feed);

                if (this.jsonLightOutputContext.WritingResponse)
                {
                    // End the array which holds the entries in the feed.
                    // NOTE: in requests we will only write the EndArray of a feed 
                    //       when we hit the navigation link end since a navigation link
                    //       can contain multiple feeds that get collapesed into a single array value.
                    this.jsonLightOutputContext.JsonWriter.EndArrayScope();

                    // Write the inline count if it's available.
                    this.WriteFeedCount(feed, propertyName);

                    // Write the next link if it's available.
                    this.WriteFeedNextLink(feed, propertyName);
                }
            }
        }

        /// <summary>
        /// Start writing a deferred (non-expanded) navigation link.
        /// </summary>
        /// <param name="navigationLink">The navigation link to write.</param>
        protected override void WriteDeferredNavigationLink(ODataNavigationLink navigationLink)
        {
            Debug.Assert(navigationLink != null, "navigationLink != null");
            Debug.Assert(this.jsonLightOutputContext.WritingResponse, "Deferred links are only supported in response, we should have verified this already.");

            // A deferred navigation link is just the link metadata, no value.
            this.jsonLightEntryAndFeedSerializer.WriteNavigationLinkMetadata(navigationLink, this.DuplicatePropertyNamesChecker);
        }

        /// <summary>
        /// Start writing a navigation link with content.
        /// </summary>
        /// <param name="navigationLink">The navigation link to write.</param>
        protected override void StartNavigationLinkWithContent(ODataNavigationLink navigationLink)
        {
            Debug.Assert(navigationLink != null, "navigationLink != null");
            Debug.Assert(!string.IsNullOrEmpty(navigationLink.Name), "The navigation link name should have been verified by now.");

            if (this.jsonLightOutputContext.WritingResponse)
            {
                // Write the navigation link metadata first. The rest is written by the content entry or feed.
                this.jsonLightEntryAndFeedSerializer.WriteNavigationLinkMetadata(navigationLink, this.DuplicatePropertyNamesChecker);
            }
            else
            {
                WriterValidationUtils.ValidateNavigationLinkHasCardinality(navigationLink);
            }
        }

        /// <summary>
        /// Finish writing a navigation link with content.
        /// </summary>
        /// <param name="navigationLink">The navigation link to write.</param>
        protected override void EndNavigationLinkWithContent(ODataNavigationLink navigationLink)
        {
            Debug.Assert(navigationLink != null, "navigationLink != null");

            if (!this.jsonLightOutputContext.WritingResponse)
            {
                JsonLightNavigationLinkScope navigationLinkScope = (JsonLightNavigationLinkScope)this.CurrentScope;

                // If we wrote entity reference links for a collection navigation property but no 
                // feed afterwards, we have to now close the array of links.
                if (navigationLinkScope.EntityReferenceLinkWritten && !navigationLinkScope.FeedWritten && navigationLink.IsCollection.Value)
                {
                    this.jsonLightOutputContext.JsonWriter.EndArrayScope();
                }

                // In requests, the navigation link may have multiple entries in multiple feeds in it; if we 
                // wrote at least one feed, close the resulting array here.
                if (navigationLinkScope.FeedWritten)
                {
                    Debug.Assert(navigationLink.IsCollection.Value, "navigationLink.IsCollection.Value");
                    this.jsonLightOutputContext.JsonWriter.EndArrayScope();
                }
            }
        }

        /// <summary>
        /// Write an entity reference link.
        /// </summary>
        /// <param name="parentNavigationLink">The parent navigation link which is being written around the entity reference link.</param>
        /// <param name="entityReferenceLink">The entity reference link to write.</param>
        protected override void WriteEntityReferenceInNavigationLinkContent(ODataNavigationLink parentNavigationLink, ODataEntityReferenceLink entityReferenceLink)
        {
            Debug.Assert(parentNavigationLink != null, "parentNavigationLink != null");
            Debug.Assert(entityReferenceLink != null, "entityReferenceLink != null");
            Debug.Assert(!this.jsonLightOutputContext.WritingResponse, "Entity reference links are only supported in request, we should have verified this already.");

            // In JSON Light, we can only write entity reference links at the beginning of a navigation link in requests;
            // once we wrote a feed, entity reference links are not allowed anymore (we require all the entity reference
            // link to come first because of the grouping in the JSON Light wire format).
            JsonLightNavigationLinkScope navigationLinkScope = (JsonLightNavigationLinkScope)this.CurrentScope;
            if (navigationLinkScope.FeedWritten)
            {
                throw new ODataException(OData.Strings.ODataJsonLightWriter_EntityReferenceLinkAfterFeedInRequest);
            }

            if (!navigationLinkScope.EntityReferenceLinkWritten)
            {
                // Write the property annotation for the entity reference link(s)
                this.jsonLightOutputContext.JsonWriter.WritePropertyAnnotationName(parentNavigationLink.Name, ODataAnnotationNames.ODataBind);
                Debug.Assert(parentNavigationLink.IsCollection.HasValue, "parentNavigationLink.IsCollection.HasValue");
                if (parentNavigationLink.IsCollection.Value)
                {
                    this.jsonLightOutputContext.JsonWriter.StartArrayScope();
                }

                navigationLinkScope.EntityReferenceLinkWritten = true;
            }

            Debug.Assert(entityReferenceLink.Url != null, "The entity reference link Url should have been validated by now.");
            this.jsonLightOutputContext.JsonWriter.WriteValue(this.jsonLightEntryAndFeedSerializer.UriToString(entityReferenceLink.Url));
        }

        /// <summary>
        /// Create a new feed scope.
        /// </summary>
        /// <param name="feed">The feed for the new scope.</param>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <param name="skipWriting">true if the content of the scope to create should not be written.</param>
        /// <param name="selectedProperties">The selected properties of this scope.</param>
        /// <returns>The newly create scope.</returns>
        protected override FeedScope CreateFeedScope(ODataFeed feed, IEdmEntitySet entitySet, IEdmEntityType entityType, bool skipWriting, SelectedPropertiesNode selectedProperties)
        {
            return new JsonLightFeedScope(feed, entitySet, entityType, skipWriting, selectedProperties);
        }

        /// <summary>
        /// Create a new entry scope.
        /// </summary>
        /// <param name="entry">The entry for the new scope.</param>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <param name="skipWriting">true if the content of the scope to create should not be written.</param>
        /// <param name="selectedProperties">The selected properties of this scope.</param>
        /// <returns>The newly create scope.</returns>
        protected override EntryScope CreateEntryScope(ODataEntry entry, IEdmEntitySet entitySet, IEdmEntityType entityType, bool skipWriting, SelectedPropertiesNode selectedProperties)
        {
            return new JsonLightEntryScope(
                entry,
                this.GetEntrySerializationInfo(entry),
                entitySet,
                entityType,
                skipWriting,
                this.jsonLightOutputContext.WritingResponse,
                this.jsonLightOutputContext.MessageWriterSettings.WriterBehavior,
                selectedProperties);
        }

        /// <summary>
        /// Creates a new JSON Light navigation link scope.
        /// </summary>
        /// <param name="writerState">The writer state for the new scope.</param>
        /// <param name="navLink">The navigation link for the new scope.</param>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <param name="skipWriting">true if the content of the scope to create should not be written.</param>
        /// <param name="selectedProperties">The selected properties of this scope.</param>
        /// <returns>The newly created JSON Light  navigation link scope.</returns>
        protected override NavigationLinkScope CreateNavigationLinkScope(WriterState writerState, ODataNavigationLink navLink, IEdmEntitySet entitySet, IEdmEntityType entityType, bool skipWriting, SelectedPropertiesNode selectedProperties)
        {
            return new JsonLightNavigationLinkScope(writerState, navLink, entitySet, entityType, skipWriting, selectedProperties);
        }

        /// <summary>
        /// Writes the odata.count annotation for a feed if it has not been written yet (and the count is specified on the feed).
        /// </summary>
        /// <param name="feed">The feed to write the count for.</param>
        /// <param name="propertyName">The name of the expanded nav property or null for a top-level feed.</param>
        private void WriteFeedCount(ODataFeed feed, string propertyName)
        {
            Debug.Assert(feed != null, "feed != null");

            // If we haven't written the count yet and it's available, write it.
            long? count = feed.Count;
            if (count.HasValue && !this.CurrentFeedScope.CountWritten)
            {
                if (propertyName == null)
                {
                    this.jsonLightOutputContext.JsonWriter.WriteName(ODataAnnotationNames.ODataCount);
                }
                else
                {
                    this.jsonLightOutputContext.JsonWriter.WritePropertyAnnotationName(propertyName, ODataAnnotationNames.ODataCount);
                }

                this.jsonLightOutputContext.JsonWriter.WriteValue(count.Value);
                this.CurrentFeedScope.CountWritten = true;
            }
        }

        /// <summary>
        /// Writes the odata.nextLink annotation for a feed if it has not been written yet (and the next link is specified on the feed).
        /// </summary>
        /// <param name="feed">The feed to write the next link for.</param>
        /// <param name="propertyName">The name of the expanded nav property or null for a top-level feed.</param>
        private void WriteFeedNextLink(ODataFeed feed, string propertyName)
        {
            Debug.Assert(feed != null, "feed != null");

            // If we haven't written the next link yet and it's available, write it.
            Uri nextPageLink = feed.NextPageLink;
            if (nextPageLink != null && !this.CurrentFeedScope.NextPageLinkWritten)
            {
                if (propertyName == null)
                {
                    this.jsonLightOutputContext.JsonWriter.WriteName(ODataAnnotationNames.ODataNextLink);
                }
                else
                {
                    this.jsonLightOutputContext.JsonWriter.WritePropertyAnnotationName(propertyName, ODataAnnotationNames.ODataNextLink);
                }

                this.jsonLightOutputContext.JsonWriter.WriteValue(this.jsonLightEntryAndFeedSerializer.UriToString(nextPageLink));
                this.CurrentFeedScope.NextPageLinkWritten = true;
            }
        }

        /// <summary>
        /// Writes the odata.deltaLink annotation for a feed if it has not been written yet (and the delta link is specified on the feed).
        /// </summary>
        /// <param name="feed">The feed to write the delta link for.</param>
        private void WriteFeedDeltaLink(ODataFeed feed)
        {
            Debug.Assert(feed != null, "feed != null");

            // If we haven't written the delta link yet and it's available, write it.
            Uri deltaLink = feed.DeltaLink;
            if (deltaLink != null && !this.CurrentFeedScope.DeltaLinkWritten)
            {
                this.jsonLightOutputContext.JsonWriter.WriteName(ODataAnnotationNames.ODataDeltaLink);
                this.jsonLightOutputContext.JsonWriter.WriteValue(this.jsonLightEntryAndFeedSerializer.UriToString(deltaLink));
                this.CurrentFeedScope.DeltaLinkWritten = true;
            }
        }

        /// <summary>
        /// Validates that the ODataFeed.InstanceAnnotations collection is empty for the given expanded feed.
        /// </summary>
        /// <param name="feed">The expanded feed in question.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "An instance field is used in a debug assert.")]
        private void ValidateNoCustomInstanceAnnotationsForExpandedFeed(ODataFeed feed)
        {
            Debug.Assert(feed != null, "feed != null");
            Debug.Assert(
                this.ParentNavigationLink != null && this.ParentNavigationLink.IsCollection.HasValue && this.ParentNavigationLink.IsCollection.Value == true,
                "This should only be called when writing an expanded feed.");

            if (feed.InstanceAnnotations.Count > 0)
            {
                throw new ODataException(OData.Strings.ODataJsonLightWriter_InstanceAnnotationNotSupportedOnExpandedFeed);
            }
        }

        /// <summary>
        /// A scope for a JSON lite feed.
        /// </summary>
        private sealed class JsonLightFeedScope : FeedScope
        {
            /// <summary>true if the odata.count was already written, false otherwise.</summary>
            private bool countWritten;

            /// <summary>true if the odata.nextLink was already written, false otherwise.</summary>
            private bool nextLinkWritten;

            /// <summary>true if the odata.deltaLink was already written, false otherwise.</summary>
            private bool deltaLinkWritten;

            /// <summary>
            /// Constructor to create a new feed scope.
            /// </summary>
            /// <param name="feed">The feed for the new scope.</param>
            /// <param name="entitySet">The entity set we are going to write entities for.</param>
            /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
            /// <param name="skipWriting">true if the content of the scope to create should not be written.</param>
            /// <param name="selectedProperties">The selected properties of this scope.</param>
            internal JsonLightFeedScope(ODataFeed feed, IEdmEntitySet entitySet, IEdmEntityType entityType, bool skipWriting, SelectedPropertiesNode selectedProperties)
                : base(feed, entitySet, entityType, skipWriting, selectedProperties)
            {
                DebugUtils.CheckNoExternalCallers();
            }

            /// <summary>
            /// true if the odata.count annotation was already written, false otherwise.
            /// </summary>
            internal bool CountWritten
            {
                get
                {
                    DebugUtils.CheckNoExternalCallers();
                    return this.countWritten;
                }

                set
                {
                    DebugUtils.CheckNoExternalCallers();
                    this.countWritten = value;
                }
            }

            /// <summary>
            /// true if the odata.nextLink annotation was already written, false otherwise.
            /// </summary>
            internal bool NextPageLinkWritten
            {
                get
                {
                    DebugUtils.CheckNoExternalCallers();
                    return this.nextLinkWritten;
                }

                set
                {
                    DebugUtils.CheckNoExternalCallers();
                    this.nextLinkWritten = value;
                }
            }

            /// <summary>
            /// true if the odata.deltaLink annotation was already written, false otherwise.
            /// </summary>
            internal bool DeltaLinkWritten
            {
                get
                {
                    DebugUtils.CheckNoExternalCallers();
                    return this.deltaLinkWritten;
                }

                set
                {
                    DebugUtils.CheckNoExternalCallers();
                    this.deltaLinkWritten = value;
                }
            }
        }

        /// <summary>
        /// A scope for an entry in JSON Light writer.
        /// </summary>
        private sealed class JsonLightEntryScope : EntryScope, IODataJsonLightWriterEntryState
        {
            /// <summary>Bit field of the JSON Light metadata properties written so far.</summary>
            private int alreadyWrittenMetadataProperties;

            /// <summary>
            /// Constructor to create a new entry scope.
            /// </summary>
            /// <param name="entry">The entry for the new scope.</param>
            /// <param name="serializationInfo">The serialization info for the current entry.</param>
            /// <param name="entitySet">The entity set we are going to write entities for.</param>
            /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
            /// <param name="skipWriting">true if the content of the scope to create should not be written.</param>
            /// <param name="writingResponse">true if we are writing a response, false if it's a request.</param>
            /// <param name="writerBehavior">The <see cref="ODataWriterBehavior"/> instance controlling the behavior of the writer.</param>
            /// <param name="selectedProperties">The selected properties of this scope.</param>
            internal JsonLightEntryScope(ODataEntry entry, ODataFeedAndEntrySerializationInfo serializationInfo, IEdmEntitySet entitySet, IEdmEntityType entityType, bool skipWriting, bool writingResponse, ODataWriterBehavior writerBehavior, SelectedPropertiesNode selectedProperties)
                : base(entry, serializationInfo, entitySet, entityType, skipWriting, writingResponse, writerBehavior, selectedProperties)
            {
                DebugUtils.CheckNoExternalCallers();
            }

            /// <summary>
            /// Enumeration of JSON Light metadata property flags, used to keep track of which properties were already written.
            /// </summary>
            [Flags]
            private enum JsonLightEntryMetadataProperty
            {
                /// <summary>The odata.editLink property.</summary>
                EditLink = 0x1,

                /// <summary>The odata.readLink property.</summary>
                ReadLink = 0x2,

                /// <summary>The odata.mediaEditLink property.</summary>
                MediaEditLink = 0x4,

                /// <summary>The odata.mediaReadLink property.</summary>
                MediaReadLink = 0x8,

                /// <summary>The odata.mediaContentType property.</summary>
                MediaContentType = 0x10,

                /// <summary>The odata.mediaETag property.</summary>
                MediaETag = 0x20,
            }

            /// <summary>
            /// The entry being written.
            /// </summary>
            public ODataEntry Entry
            {
                get { return (ODataEntry)this.Item; }
            }

            /// <summary>
            /// Flag which indicates that the odata.editLink metadata property has been written.
            /// </summary>
            public bool EditLinkWritten
            {
                get
                {
                    return this.IsMetadataPropertyWritten(JsonLightEntryMetadataProperty.EditLink);
                }

                set
                {
                    Debug.Assert(value == true, "The flag that a metadata property has been written should only ever be set from false to true.");
                    this.SetWrittenMetadataProperty(JsonLightEntryMetadataProperty.EditLink);
                }
            }

            /// <summary>
            /// Flag which indicates that the odata.readLink metadata property has been written.
            /// </summary>
            public bool ReadLinkWritten
            {
                get
                {
                    return this.IsMetadataPropertyWritten(JsonLightEntryMetadataProperty.ReadLink);
                }

                set
                {
                    Debug.Assert(value == true, "The flag that a metadata property has been written should only ever be set from false to true.");
                    this.SetWrittenMetadataProperty(JsonLightEntryMetadataProperty.ReadLink);
                }
            }

            /// <summary>
            /// Flag which indicates that the odata.mediaEditLink metadata property has been written.
            /// </summary>
            public bool MediaEditLinkWritten
            {
                get
                {
                    return this.IsMetadataPropertyWritten(JsonLightEntryMetadataProperty.MediaEditLink);
                }

                set
                {
                    Debug.Assert(value == true, "The flag that a metadata property has been written should only ever be set from false to true.");
                    this.SetWrittenMetadataProperty(JsonLightEntryMetadataProperty.MediaEditLink);
                }
            }

            /// <summary>
            /// Flag which indicates that the odata.mediaReadLink metadata property has been written.
            /// </summary>
            public bool MediaReadLinkWritten
            {
                get
                {
                    return this.IsMetadataPropertyWritten(JsonLightEntryMetadataProperty.MediaReadLink);
                }

                set
                {
                    Debug.Assert(value == true, "The flag that a metadata property has been written should only ever be set from false to true.");
                    this.SetWrittenMetadataProperty(JsonLightEntryMetadataProperty.MediaReadLink);
                }
            }

            /// <summary>
            /// Flag which indicates that the odata.mediaContentType metadata property has been written.
            /// </summary>
            public bool MediaContentTypeWritten
            {
                get
                {
                    return this.IsMetadataPropertyWritten(JsonLightEntryMetadataProperty.MediaContentType);
                }

                set
                {
                    Debug.Assert(value == true, "The flag that a metadata property has been written should only ever be set from false to true.");
                    this.SetWrittenMetadataProperty(JsonLightEntryMetadataProperty.MediaContentType);
                }
            }

            /// <summary>
            /// Flag which indicates that the odata.mediaETag metadata property has been written.
            /// </summary>
            public bool MediaETagWritten
            {
                get
                {
                    return this.IsMetadataPropertyWritten(JsonLightEntryMetadataProperty.MediaETag);
                }

                set
                {
                    Debug.Assert(value == true, "The flag that a metadata property has been written should only ever be set from false to true.");
                    this.SetWrittenMetadataProperty(JsonLightEntryMetadataProperty.MediaETag);
                }
            }

            /// <summary>
            /// Marks the <paramref name="jsonLightMetadataProperty"/> as written in this entry scope.
            /// </summary>
            /// <param name="jsonLightMetadataProperty">The metadta property which was written.</param>
            private void SetWrittenMetadataProperty(JsonLightEntryMetadataProperty jsonLightMetadataProperty)
            {
                DebugUtils.CheckNoExternalCallers();
                Debug.Assert(!this.IsMetadataPropertyWritten(jsonLightMetadataProperty), "Can't write the same metadata property twice.");
                this.alreadyWrittenMetadataProperties |= (int)jsonLightMetadataProperty;
            }

            /// <summary>
            /// Determines if the <paramref name="jsonLightMetadataProperty"/> was already written for this entry scope.
            /// </summary>
            /// <param name="jsonLightMetadataProperty">The metadata property to test for.</param>
            /// <returns>true if the <paramref name="jsonLightMetadataProperty"/> was already written for this entry scope; false otherwise.</returns>
            private bool IsMetadataPropertyWritten(JsonLightEntryMetadataProperty jsonLightMetadataProperty)
            {
                DebugUtils.CheckNoExternalCallers();
                return (this.alreadyWrittenMetadataProperties & (int)jsonLightMetadataProperty) == (int)jsonLightMetadataProperty;
            }
        }

        /// <summary>
        /// A scope for a JSON Light navigation link.
        /// </summary>
        private sealed class JsonLightNavigationLinkScope : NavigationLinkScope
        {
            /// <summary>true if we have already written an entity reference link for this navigation link in requests; otherwise false.</summary>
            private bool entityReferenceLinkWritten;

            /// <summary>true if we have written at least one feed for this navigation link in requests; otherwise false.</summary>
            private bool feedWritten;

            /// <summary>
            /// Constructor to create a new JSON Light navigation link scope.
            /// </summary>
            /// <param name="writerState">The writer state for the new scope.</param>
            /// <param name="navLink">The navigation link for the new scope.</param>
            /// <param name="entitySet">The entity set we are going to write entities for.</param>
            /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
            /// <param name="skipWriting">true if the content of the scope to create should not be written.</param>
            /// <param name="selectedProperties">The selected properties of this scope.</param>
            internal JsonLightNavigationLinkScope(WriterState writerState, ODataNavigationLink navLink, IEdmEntitySet entitySet, IEdmEntityType entityType, bool skipWriting, SelectedPropertiesNode selectedProperties)
                : base(writerState, navLink, entitySet, entityType, skipWriting, selectedProperties)
            {
                DebugUtils.CheckNoExternalCallers();
            }

            /// <summary>
            /// true if we have already written an entity reference link for this navigation link in requests; otherwise false.
            /// </summary>
            internal bool EntityReferenceLinkWritten
            {
                get
                {
                    DebugUtils.CheckNoExternalCallers();
                    return this.entityReferenceLinkWritten;
                }

                set
                {
                    DebugUtils.CheckNoExternalCallers();
                    this.entityReferenceLinkWritten = value;
                }
            }

            /// <summary>
            /// true if we have written at least one feed for this navigation link in requests; otherwise false.
            /// </summary>
            internal bool FeedWritten
            {
                get
                {
                    DebugUtils.CheckNoExternalCallers();
                    return this.feedWritten;
                }

                set
                {
                    DebugUtils.CheckNoExternalCallers();
                    this.feedWritten = value;
                }
            }

            /// <summary>
            /// Clones this JSON Light navigation link scope and sets a new writer state.
            /// </summary>
            /// <param name="newWriterState">The writer state to set.</param>
            /// <returns>The cloned navigation link scope with the specified writer state.</returns>
            internal override NavigationLinkScope Clone(WriterState newWriterState)
            {
                return new JsonLightNavigationLinkScope(newWriterState, (ODataNavigationLink)this.Item, this.EntitySet, this.EntityType, this.SkipWriting, this.SelectedProperties)
                {
                    EntityReferenceLinkWritten = this.entityReferenceLinkWritten,
                    FeedWritten = this.feedWritten,
                };
            }
        }
    }
}

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

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Json;
    using Microsoft.Data.OData.Metadata;

    #endregion Namespaces

    /// <summary>
    /// OData collection reader for the JsonLight format.
    /// </summary>
    internal sealed class ODataJsonLightCollectionReader : ODataCollectionReaderCoreAsync
    {
        /// <summary>The input to read the payload from.</summary>
        private readonly ODataJsonLightInputContext jsonLightInputContext;

        /// <summary>The collection deserializer to use to read from the input.</summary>
        private readonly ODataJsonLightCollectionDeserializer jsonLightCollectionDeserializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLightInputContext">The input to read the payload from.</param>
        /// <param name="expectedItemTypeReference">The expected type for the items in the collection.</param>
        /// <param name="listener">If not null, the reader will notify the implementer of the interface of relevant state changes in the reader.</param>
        internal ODataJsonLightCollectionReader(
            ODataJsonLightInputContext jsonLightInputContext,
            IEdmTypeReference expectedItemTypeReference, 
            IODataReaderWriterListener listener)
            : base(jsonLightInputContext, expectedItemTypeReference, listener)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(jsonLightInputContext != null, "jsonLightInputContext != null");

            this.jsonLightInputContext = jsonLightInputContext;
            this.jsonLightCollectionDeserializer = new ODataJsonLightCollectionDeserializer(jsonLightInputContext);
        }

        /// <summary>
        /// Implementation of the collection reader logic when in state 'Start'.
        /// </summary>
        /// <returns>true if more items can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.None:      assumes that the JSON reader has not been used yet when not reading a nested payload.
        /// Post-Condition: The reader is positioned on the first node of the first item or the EndArray node of an empty item array
        /// </remarks>
        protected override bool ReadAtStartImplementation()
        {
            Debug.Assert(this.State == ODataCollectionReaderState.Start, "this.State == ODataCollectionReaderState.Start");
            Debug.Assert(this.IsReadingNestedPayload || this.jsonLightCollectionDeserializer.JsonReader.NodeType == JsonNodeType.None, "Pre-Condition: expected JsonNodeType.None when not reading a nested payload.");

            // We use this to store annotations and check for duplicate annotation names, but we don't really store properties in it.
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = this.jsonLightInputContext.CreateDuplicatePropertyNamesChecker();

            // Position the reader on the first node depending on whether we are reading a nested payload or not
            this.jsonLightCollectionDeserializer.ReadPayloadStart(
                ODataPayloadKind.Collection, 
                duplicatePropertyNamesChecker, 
                this.IsReadingNestedPayload,
                /*allowEmptyPayload*/false);

            return this.ReadAtStartImplementationSynchronously(duplicatePropertyNamesChecker);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Implementation of the collection reader logic when in state 'Start'.
        /// </summary>
        /// <returns>Task which returns true if more items can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.None:      assumes that the JSON reader has not been used yet when not reading a nested payload.
        /// Post-Condition: The reader is positioned on the first node of the first item or the EndArray node of an empty item array
        /// </remarks>
        [SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "API design calls for a bool being returned from the task here.")]
        protected override Task<bool> ReadAtStartImplementationAsync()
        {
            Debug.Assert(this.State == ODataCollectionReaderState.Start, "this.State == ODataCollectionReaderState.Start");
            Debug.Assert(this.IsReadingNestedPayload || this.jsonLightCollectionDeserializer.JsonReader.NodeType == JsonNodeType.None, "Pre-Condition: expected JsonNodeType.None when not reading a nested payload.");

            // We use this to store annotations and check for duplicate annotation names, but we don't really store properties in it.
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = this.jsonLightInputContext.CreateDuplicatePropertyNamesChecker();

            // Position the reader on the first node depending on whether we are reading a nested payload or not
            return this.jsonLightCollectionDeserializer.ReadPayloadStartAsync(
                ODataPayloadKind.Collection,
                duplicatePropertyNamesChecker,
                this.IsReadingNestedPayload,
                /*allowEmptyPayload*/false)

                .FollowOnSuccessWith(t =>
                    this.ReadAtStartImplementationSynchronously(duplicatePropertyNamesChecker));
        }
#endif

        /// <summary>
        /// Implementation of the reader logic when in state 'CollectionStart'.
        /// </summary>
        /// <returns>true if more nodes can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  The first node of the first item in the collection or the EndArray node of the (empty) item array
        ///                 NOTE: this method will throw if the node is not
        ///                 JsonNodeType.EndArray:       for an empty item array of the collection
        ///                 JsonNodeType.StartObject:    for a complex value as first item
        ///                 JsonNodeType.PrimitiveValue: for a primitive value as first item
        /// Post-Condition: The reader is positioned on the first node of the second item or an EndArray node if there are no items in the collection
        /// </remarks>
        protected override bool ReadAtCollectionStartImplementation()
        {
            return this.ReadAtCollectionStartImplementationSynchronously();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Implementation of the reader logic when in state 'CollectionStart'.
        /// </summary>
        /// <returns>Task which returns true if more nodes can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  The first node of the first item in the collection or the EndArray node of the (empty) item array
        ///                 NOTE: this method will throw if the node is not
        ///                 JsonNodeType.EndArray:       for an empty item array of the collection
        ///                 JsonNodeType.StartObject:    for a complex value as first item
        ///                 JsonNodeType.PrimitiveValue: for a primitive value as first item
        /// Post-Condition: The reader is positioned on the first node of the second item or an EndArray node if there are no items in the collection
        /// </remarks>
        [SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "API design calls for a bool being returned from the task here.")]
        protected override Task<bool> ReadAtCollectionStartImplementationAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<bool>(this.ReadAtCollectionStartImplementationSynchronously);
        }
#endif

        /// <summary>
        /// Implementation of the reader logic when in state 'Value'.
        /// </summary>
        /// <returns>true if more nodes can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  The first node of the next item in the collection or the EndArray node of the item array
        ///                 NOTE: this method will throw if the node is not
        ///                 JsonNodeType.EndArray:       for the end of the item array of the collection
        ///                 JsonNodeType.StartObject:    for a complex item
        ///                 JsonNodeType.PrimitiveValue: for a primitive item
        /// Post-Condition: The reader is positioned on the first node of the next item or an EndArray node if there are no items in the collection
        /// </remarks>
        protected override bool ReadAtValueImplementation()
        {
            return this.ReadAtValueImplementationSynchronously();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Implementation of the reader logic when in state 'Value'.
        /// </summary>
        /// <returns>Task which returns true if more nodes can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  The first node of the next item in the collection or the EndArray node of the item array
        ///                 NOTE: this method will throw if the node is not
        ///                 JsonNodeType.EndArray:       for the end of the item array of the collection
        ///                 JsonNodeType.StartObject:    for a complex item
        ///                 JsonNodeType.PrimitiveValue: for a primitive item
        /// Post-Condition: The reader is positioned on the first node of the next item or an EndArray node if there are no items in the collection
        /// </remarks>
        [SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "API design calls for a bool being returned from the task here.")]
        protected override Task<bool> ReadAtValueImplementationAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<bool>(this.ReadAtValueImplementationSynchronously);
        }
#endif

        /// <summary>
        /// Implementation of the reader logic when in state 'CollectionEnd'.
        /// </summary>
        /// <returns>false since no more nodes can be read from the reader after the collection ended.</returns>
        /// <remarks>
        /// Pre-Condition: JsonNodeType.EndArray        the end of the item array of the collection
        /// Post-Condition: JsonNodeType.EndOfInput     nothing else to read when not reading a nested payload
        /// </remarks>
        protected override bool ReadAtCollectionEndImplementation()
        {
            return this.ReadAtCollectionEndImplementationSynchronously();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Implementation of the reader logic when in state 'CollectionEnd'.
        /// </summary>
        /// <returns>Task which should return false since no more nodes can be read from the reader after the collection ends.</returns>
        /// <remarks>
        /// Pre-Condition: JsonNodeType.EndArray        the end of the item array of the collection
        /// Post-Condition: JsonNodeType.EndOfInput     nothing else to read when not reading a nested payload
        /// </remarks>
        [SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "API design calls for a bool being returned from the task here.")]
        protected override Task<bool> ReadAtCollectionEndImplementationAsync()
        {
            return TaskUtils.GetTaskForSynchronousOperation<bool>(this.ReadAtCollectionEndImplementationSynchronously);
        }
#endif

        /// <summary>
        /// Implementation of the collection reader logic when in state 'Start'.
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker for the top-level scope.</param>
        /// <returns>true if more items can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.None:      assumes that the JSON reader has not been used yet when not reading a nested payload.
        /// Post-Condition: The reader is positioned on the first node of the first item or the EndArray node of an empty item array
        /// </remarks>
        private bool ReadAtStartImplementationSynchronously(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker)
        {
            Debug.Assert(duplicatePropertyNamesChecker != null, "duplicatePropertyNamesChecker != null");

            IEdmTypeReference actualItemTypeReference;
            this.ExpectedItemTypeReference = ReaderValidationUtils.ValidateCollectionMetadataUriAndGetPayloadItemTypeReference(
                this.jsonLightCollectionDeserializer.MetadataUriParseResult,
                this.ExpectedItemTypeReference);

            // read the start of the collection until we find the content array for top-level collections
            ODataCollectionStart collectionStart = this.jsonLightCollectionDeserializer.ReadCollectionStart(
                duplicatePropertyNamesChecker,
                this.IsReadingNestedPayload,
                this.ExpectedItemTypeReference,
                out actualItemTypeReference);

            if (actualItemTypeReference != null)
            {
                this.ExpectedItemTypeReference = actualItemTypeReference;
            }

            this.jsonLightCollectionDeserializer.JsonReader.ReadStartArray();

            this.EnterScope(ODataCollectionReaderState.CollectionStart, collectionStart);

            return true;
        }

        /// <summary>
        /// Implementation of the reader logic when in state 'CollectionStart'.
        /// </summary>
        /// <returns>true if more nodes can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  The first node of the first item in the collection or the EndArray node of the (empty) item array
        ///                 NOTE: this method will throw if the node is not
        ///                 JsonNodeType.EndArray:       for an empty item array of the collection
        ///                 JsonNodeType.StartObject:    for a complex value as first item
        ///                 JsonNodeType.PrimitiveValue: for a primitive value as first item
        /// Post-Condition: The reader is positioned on the first node of the second item or an EndArray node if there are no items in the collection
        /// </remarks>
        private bool ReadAtCollectionStartImplementationSynchronously()
        {
            Debug.Assert(this.State == ODataCollectionReaderState.CollectionStart, "this.State == ODataCollectionReaderState.CollectionStart");

            if (this.jsonLightCollectionDeserializer.JsonReader.NodeType == JsonNodeType.EndArray)
            {
                // empty collection
                this.ReplaceScope(ODataCollectionReaderState.CollectionEnd, this.Item);
            }
            else
            {
                object item = this.jsonLightCollectionDeserializer.ReadCollectionItem(this.ExpectedItemTypeReference, this.CollectionValidator);
                this.EnterScope(ODataCollectionReaderState.Value, item);
            }

            return true;
        }

        /// <summary>
        /// Implementation of the reader logic when in state 'Value'.
        /// </summary>
        /// <returns>true if more nodes can be read from the reader; otherwise false.</returns>
        /// <remarks>
        /// Pre-Condition:  The first node of the next item in the collection or the EndArray node of the item array
        ///                 NOTE: this method will throw if the node is not
        ///                 JsonNodeType.EndArray:       for the end of the item array of the collection
        ///                 JsonNodeType.StartObject:    for a complex item
        ///                 JsonNodeType.PrimitiveValue: for a primitive item
        /// Post-Condition: The reader is positioned on the first node of the next item or an EndArray node if there are no items in the collection
        /// </remarks>
        private bool ReadAtValueImplementationSynchronously()
        {
            Debug.Assert(this.State == ODataCollectionReaderState.Value, "this.State == ODataCollectionReaderState.Value");

            if (this.jsonLightCollectionDeserializer.JsonReader.NodeType == JsonNodeType.EndArray)
            {
                // end of collection reached
                this.PopScope(ODataCollectionReaderState.Value);
                this.ReplaceScope(ODataCollectionReaderState.CollectionEnd, this.Item);
            }
            else
            {
                object item = this.jsonLightCollectionDeserializer.ReadCollectionItem(this.ExpectedItemTypeReference, this.CollectionValidator);
                this.ReplaceScope(ODataCollectionReaderState.Value, item);
            }

            return true;
        }

        /// <summary>
        /// Implementation of the reader logic when in state 'CollectionEnd'.
        /// </summary>
        /// <returns>false since no more nodes can be read from the reader after the collection ended.</returns>
        /// <remarks>
        /// Pre-Condition: JsonNodeType.EndArray        the end of the item array of the collection
        /// Post-Condition: JsonNodeType.EndOfInput     nothing else to read when not reading a nested payload
        /// </remarks>
        private bool ReadAtCollectionEndImplementationSynchronously()
        {
            Debug.Assert(this.State == ODataCollectionReaderState.CollectionEnd, "this.State == ODataCollectionReaderState.CollectionEnd");
            Debug.Assert(this.jsonLightCollectionDeserializer.JsonReader.NodeType == JsonNodeType.EndArray, "Pre-Condition: expected JsonNodeType.EndArray");

            this.PopScope(ODataCollectionReaderState.CollectionEnd);
            Debug.Assert(this.State == ODataCollectionReaderState.Start, "this.State == ODataCollectionReaderState.Start");

            this.jsonLightCollectionDeserializer.ReadCollectionEnd(this.IsReadingNestedPayload);
            this.jsonLightCollectionDeserializer.ReadPayloadEnd(this.IsReadingNestedPayload);

            // replace the 'Start' scope with the 'Completed' scope
            this.ReplaceScope(ODataCollectionReaderState.Completed, null);

            return false;
        }
    }
}

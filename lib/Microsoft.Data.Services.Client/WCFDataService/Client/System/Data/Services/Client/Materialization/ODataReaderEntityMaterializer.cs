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

namespace System.Data.Services.Client.Materialization
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Diagnostics;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using DSClient = System.Data.Services.Client;

    /// <summary>
    /// Materializes feeds and entities from an ODataReader
    /// </summary>
    internal class ODataReaderEntityMaterializer : ODataEntityMaterializer
    {
        /// <summary>The enty or feed reader.</summary>
        private FeedAndEntryMaterializerAdapter feedEntryAdapter;

        /// <summary>The message reader.</summary>
        private ODataMessageReader messageReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataReaderEntityMaterializer" /> class.
        /// </summary>
        /// <param name="odataMessageReader">The odata message reader.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="materializerContext">The materializer context.</param>
        /// <param name="entityTrackingAdapter">The entity tracking adapter.</param>
        /// <param name="queryComponents">The query components.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="materializeEntryPlan">The materialize entry plan.</param>
        public ODataReaderEntityMaterializer(
            ODataMessageReader odataMessageReader,
            ODataReaderWrapper reader,
            IODataMaterializerContext materializerContext,
            EntityTrackingAdapter entityTrackingAdapter,
            QueryComponents queryComponents,
            Type expectedType,
            ProjectionPlan materializeEntryPlan)
            : base(materializerContext, entityTrackingAdapter, queryComponents, expectedType, materializeEntryPlan)
        {
            this.messageReader = odataMessageReader;
            this.feedEntryAdapter = new FeedAndEntryMaterializerAdapter(odataMessageReader, reader, materializerContext.Model, entityTrackingAdapter.MergeOption);
        }

        /// <summary>
        /// Feed being materialized; possibly null.
        /// </summary>
        internal override ODataFeed CurrentFeed
        {
            get { return this.feedEntryAdapter.CurrentFeed; }
        }

        /// <summary>
        /// Entry being materialized; possibly null.
        /// </summary>
        internal override ODataEntry CurrentEntry
        {
            get { return this.feedEntryAdapter.CurrentEntry; }
        }

        /// <summary>
        /// Whether we have finished processing the current data stream.
        /// </summary>
        internal override bool IsEndOfStream
        {
            get { return this.IsDisposed || this.feedEntryAdapter.IsEndOfStream; }
        }

        /// <summary>
        /// The count tag's value, if requested
        /// </summary>
        /// <returns>The count value returned from the server</returns>
        internal override long CountValue
        {
            get
            {
                return this.feedEntryAdapter.GetCountValue(!this.IsDisposed);
            }
        }

        /// <summary>
        /// Returns true if the underlying object used for counting is available
        /// </summary>
        internal override bool IsCountable
        {
            // TODO: is this correct?
            get { return true; }
        }

        /// <summary>
        /// Returns true if the materializer has been disposed
        /// </summary>
        protected override bool IsDisposed
        {
            get { return this.messageReader == null; }
        }

        /// <summary>
        /// The format of the response being materialized.
        /// </summary>
        protected override ODataFormat Format
        {
            get { return ODataUtils.GetReadFormat(this.messageReader); }
        }

        /// <summary>
        /// This method is for parsing CUD operation payloads which should contain
        /// 1 a single entry
        /// 2 An Error
        /// </summary>
        /// <param name="message">the message for the payload</param>
        /// <param name="responseInfo">The current ResponseInfo object</param>
        /// <param name="expectedType">The expected type</param>
        /// <returns>the MaterializerEntry that was read</returns>
        internal static MaterializerEntry ParseSingleEntityPayload(IODataResponseMessage message, ResponseInfo responseInfo, Type expectedType)
        {
            ODataPayloadKind messageType = ODataPayloadKind.Entry;
            using (ODataMessageReader messageReader = CreateODataMessageReader(message, responseInfo, ref messageType))
            {
                IEdmType edmType = responseInfo.TypeResolver.ResolveExpectedTypeForReading(expectedType);
                ODataReaderWrapper reader = ODataReaderWrapper.Create(messageReader, messageType, edmType, responseInfo.ResponsePipeline);

                FeedAndEntryMaterializerAdapter parser = new FeedAndEntryMaterializerAdapter(messageReader, reader, responseInfo.Model, responseInfo.MergeOption);

                ODataEntry entry = null;
                bool readFeed = false;
                while (parser.Read())
                {
                    readFeed |= parser.CurrentFeed != null;
                    if (parser.CurrentEntry != null)
                    {
                        if (entry != null)
                        {
                            throw new InvalidOperationException(DSClient.Strings.AtomParser_SingleEntry_MultipleFound);
                        }

                        entry = parser.CurrentEntry;
                    }
                }

                if (entry == null)
                {
                    if (readFeed)
                    {
                        throw new InvalidOperationException(DSClient.Strings.AtomParser_SingleEntry_NoneFound);
                    }
                    else
                    {
                        throw new InvalidOperationException(DSClient.Strings.AtomParser_SingleEntry_ExpectedFeedOrEntry);
                    }
                }

                return MaterializerEntry.GetEntry(entry);
            }
        }

        /// <summary>
        /// Called when IDisposable.Dispose is called.
        /// </summary>
        protected override void OnDispose()
        {
            if (this.messageReader != null)
            {
                this.messageReader.Dispose();
                this.messageReader = null;
            }

            this.feedEntryAdapter.Dispose();
        }

        /// <summary>
        /// Reads the next feed or entry.
        /// </summary>
        /// <returns>
        /// True if an entry was read, otherwise false
        /// </returns>
        protected override bool ReadNextFeedOrEntry()
        {
            return this.feedEntryAdapter.Read();
        }
    }
}

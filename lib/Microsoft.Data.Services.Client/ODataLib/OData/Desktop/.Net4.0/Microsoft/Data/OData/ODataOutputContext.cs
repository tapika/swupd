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

namespace Microsoft.Data.OData
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Base class for all output contexts, defines the interface 
    /// to be implemented by the specific formats.
    /// </summary>
    internal abstract class ODataOutputContext : IDisposable
    {
        /// <summary>The format for this output context.</summary>
        private readonly ODataFormat format;

        /// <summary>The message writer settings to be used for writing.</summary>
        private readonly ODataMessageWriterSettings messageWriterSettings;

        /// <summary>Set to true if this context is writing a response payload.</summary>
        private readonly bool writingResponse;

        /// <summary>true if the input should be written synchronously; false if it should be written asynchronously.</summary>
        private readonly bool synchronous;

        /// <summary>The model to use.</summary>
        private readonly IEdmModel model;

        /// <summary>The optional URL resolver to perform custom URL resolution for URLs written to the payload.</summary>
        private readonly IODataUrlResolver urlResolver;

        /// <summary>The type resolver to use.</summary>
        private readonly EdmTypeResolver edmTypeResolver;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="format">The format for this output context.</param>
        /// <param name="messageWriterSettings">Configuration settings of the OData writer.</param>
        /// <param name="writingResponse">true if writing a response message; otherwise false.</param>
        /// <param name="synchronous">true if the output should be written synchronously; false if it should be written asynchronously.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs written to the payload.</param>
        protected ODataOutputContext(
            ODataFormat format,
            ODataMessageWriterSettings messageWriterSettings,
            bool writingResponse,
            bool synchronous,
            IEdmModel model,
            IODataUrlResolver urlResolver)
        {
            ExceptionUtils.CheckArgumentNotNull(format, "format");
            ExceptionUtils.CheckArgumentNotNull(messageWriterSettings, "messageWriterSettings");

            this.format = format;
            this.messageWriterSettings = messageWriterSettings;
            this.writingResponse = writingResponse;
            this.synchronous = synchronous;
            this.model = model ?? EdmCoreModel.Instance;
            this.urlResolver = urlResolver;
            this.edmTypeResolver = EdmTypeWriterResolver.Instance;
        }

        /// <summary>
        /// The message writer settings to be used for writing.
        /// </summary>
        internal ODataMessageWriterSettings MessageWriterSettings
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.messageWriterSettings;
            }
        }

        /// <summary>
        /// The version of the OData protocol to use.
        /// </summary>
        internal ODataVersion Version
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.messageWriterSettings.Version.Value;
            }
        }

        /// <summary>
        /// Set to true if a response is being written.
        /// </summary>
        internal bool WritingResponse
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.writingResponse;
            }
        }

        /// <summary>
        /// true if the output should be written synchronously; false if it should be written asynchronously.
        /// </summary>
        internal bool Synchronous
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.synchronous;
            }
        }

        /// <summary>
        /// The model to use or null if no metadata is available.
        /// </summary>
        internal IEdmModel Model
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                Debug.Assert(this.model != null, "this.model != null");
                return this.model;
            }
        }

        /// <summary>
        /// The optional URL resolver to perform custom URL resolution for URLs written to the payload.
        /// </summary>
        internal IODataUrlResolver UrlResolver
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.urlResolver;
            }
        }

        /// <summary>
        /// The type resolver to use.
        /// </summary>
        internal EdmTypeResolver EdmTypeResolver
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.edmTypeResolver;
            }
        }

        /// <summary>
        /// true if the WCF DS client compatibility format behavior should be used; otherwise false.
        /// </summary>
        protected internal bool UseClientFormatBehavior
        {
            get
            {
                return this.messageWriterSettings.WriterBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesClient;
            }
        }

        /// <summary>
        /// true if the WCF DS server compatibility format behavior should be used; otherwise false.
        /// </summary>
        protected internal bool UseServerFormatBehavior
        {
            get
            {
                return this.messageWriterSettings.WriterBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesServer;
            }
        }

        /// <summary>
        /// true if the default format behavior should be used; otherwise false.
        /// </summary>
        protected internal bool UseDefaultFormatBehavior
        {
            get
            {
                return this.messageWriterSettings.WriterBehavior.FormatBehaviorKind == ODataBehaviorKind.Default;
            }
        }

        /// <summary>
        /// true if the WCF DS server compatibility API behavior should be used; otherwise false.
        /// </summary>
        protected internal bool UseServerApiBehavior
        {
            get
            {
                return this.messageWriterSettings.WriterBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesServer;
            }
        }

        /// <summary>
        /// IDisposable.Dispose() implementation to cleanup unmanaged resources of the context.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes an <see cref="ODataError"/> into the message payload.
        /// </summary>
        /// <param name="error">The error to write.</param>
        /// <param name="includeDebugInformation">
        /// A flag indicating whether debug information (e.g., the inner error from the <paramref name="error"/>) should 
        /// be included in the payload. This should only be used in debug scenarios.
        /// </param>
        /// <remarks>
        /// This method is called if the ODataMessageWriter.WriteError is called once some other
        /// write operation has already started.
        /// The method should write the in-stream error representation for the specific format into the current payload.
        /// Before the method is called no flush is performed on the output context or any active writer.
        /// It is the responsibility of this method to flush the output before the method returns.
        /// </remarks>
        internal virtual void WriteInStreamError(ODataError error, bool includeDebugInformation)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Writes an <see cref="ODataError"/> into the message payload.
        /// </summary>
        /// <param name="error">The error to write.</param>
        /// <param name="includeDebugInformation">
        /// A flag indicating whether debug information (e.g., the inner error from the <paramref name="error"/>) should 
        /// be included in the payload. This should only be used in debug scenarios.
        /// </param>
        /// <returns>Task which represents the pending write operation.</returns>
        /// <remarks>
        /// This method is called if the ODataMessageWriter.WriteError is called once some other
        /// write operation has already started.
        /// The method should write the in-stream error representation for the specific format into the current payload.
        /// Before the method is called no flush is performed on the output context or any active writer.
        /// It is the responsibility of this method to make sure that all the data up to this point are written before
        /// the in-stream error is written.
        /// It is the responsibility of this method to flush the output before the task finishes.
        /// </remarks>
        internal virtual Task WriteInStreamErrorAsync(ODataError error, bool includeDebugInformation)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }
#endif

        /// <summary>
        /// Creates an <see cref="ODataWriter" /> to write a feed.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <returns>The created writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual ODataWriter CreateODataFeedWriter(IEdmEntitySet entitySet, IEdmEntityType entityType)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Feed);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously creates an <see cref="ODataWriter" /> to write a feed.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <returns>A running task for the created writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual Task<ODataWriter> CreateODataFeedWriterAsync(IEdmEntitySet entitySet, IEdmEntityType entityType)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Feed);
        }
#endif

        /// <summary>
        /// Creates an <see cref="ODataWriter" /> to write an entry.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <returns>The created writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual ODataWriter CreateODataEntryWriter(IEdmEntitySet entitySet, IEdmEntityType entityType)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Entry);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously creates an <see cref="ODataWriter" /> to write an entry.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to write entities for.</param>
        /// <param name="entityType">The entity type for the entries in the feed to be written (or null if the entity set base type should be used).</param>
        /// <returns>A running task for the created writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual Task<ODataWriter> CreateODataEntryWriterAsync(IEdmEntitySet entitySet, IEdmEntityType entityType)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Entry);
        }
#endif

        /// <summary>
        /// Creates an <see cref="ODataCollectionWriter" /> to write a collection of primitive or complex values (as result of a service operation invocation).
        /// </summary>
        /// <param name="itemTypeReference">The item type of the collection being written or null if no metadata is available.</param>
        /// <returns>The created collection writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual ODataCollectionWriter CreateODataCollectionWriter(IEdmTypeReference itemTypeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Collection);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously creates an <see cref="ODataCollectionWriter" /> to write a collection of primitive or complex values (as result of a service operation invocation).
        /// </summary>
        /// <param name="itemTypeReference">The item type of the collection being written or null if no metadata is available.</param>
        /// <returns>A running task for the created collection writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual Task<ODataCollectionWriter> CreateODataCollectionWriterAsync(IEdmTypeReference itemTypeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Collection);
        }
#endif

        /// <summary>
        /// Creates an <see cref="ODataBatchWriter" /> to write a batch of requests or responses.
        /// </summary>
        /// <param name="batchBoundary">The boundary string for the batch structure itself.</param>
        /// <returns>The created batch writer.</returns>
        /// <remarks>We don't plan to make this public!</remarks>
        /// <remarks>
        /// The write must flush the output when it's finished (inside the last Write call).
        /// Since we don't want to support batch format extensibility (at least not yet) this method should remain internal.
        /// </remarks>
        internal virtual ODataBatchWriter CreateODataBatchWriter(string batchBoundary)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Batch);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously creates an <see cref="ODataBatchWriter" /> to write a batch of requests or responses.
        /// </summary>
        /// <param name="batchBoundary">The boundary string for the batch structure itself.</param>
        /// <returns>A running task for the created batch writer.</returns>
        /// <remarks>We don't plan to make this public!</remarks>
        /// <remarks>
        /// The write must flush the output when it's finished (inside the last Write call).
        /// Since we don't want to support batch format extensibility (at least not yet) this method should remain internal.
        /// </remarks>
        internal virtual Task<ODataBatchWriter> CreateODataBatchWriterAsync(string batchBoundary)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Batch);
        }
#endif

        /// <summary>
        /// Creates an <see cref="ODataParameterWriter" /> to write a parameter payload.
        /// </summary>
        /// <param name="functionImport">The function import whose parameters will be written.</param>
        /// <returns>The created parameter writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual ODataParameterWriter CreateODataParameterWriter(IEdmFunctionImport functionImport)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously creates an <see cref="ODataParameterWriter" /> to write a parameter payload.
        /// </summary>
        /// <param name="functionImport">The function import whose parameters will be written.</param>
        /// <returns>A running task for the created parameter writer.</returns>
        /// <remarks>The write must flush the output when it's finished (inside the last Write call).</remarks>
        internal virtual Task<ODataParameterWriter> CreateODataParameterWriterAsync(IEdmFunctionImport functionImport)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }
#endif

        /// <summary>
        /// Writes a service document with the specified <paramref name="defaultWorkspace"/> 
        /// as message payload.
        /// </summary>
        /// <param name="defaultWorkspace">The default workspace to write in the service document.</param>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal virtual void WriteServiceDocument(ODataWorkspace defaultWorkspace)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.ServiceDocument);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously writes a service document with the specified <paramref name="defaultWorkspace"/> 
        /// as message payload.
        /// </summary>
        /// <param name="defaultWorkspace">The default workspace to write in the service document.</param>
        /// <returns>A task representing the asynchronous operation of writing the service document.</returns>
        /// <remarks>It is the responsibility of this method to flush the output before the task finishes.</remarks>
        internal virtual Task WriteServiceDocumentAsync(ODataWorkspace defaultWorkspace)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.ServiceDocument);
        }
#endif

        /// <summary>
        /// Writes an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="property">The property to write.</param>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal virtual void WriteProperty(ODataProperty property)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Property);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously writes an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="property">The property to write</param>
        /// <returns>A task representing the asynchronous operation of writing the property.</returns>
        /// <remarks>It is the responsibility of this method to flush the output before the task finishes.</remarks>
        internal virtual Task WritePropertyAsync(ODataProperty property)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Property);
        }
#endif

        /// <summary>
        /// Writes an <see cref="ODataError"/> as the message payload.
        /// </summary>
        /// <param name="error">The error to write.</param>
        /// <param name="includeDebugInformation">
        /// A flag indicating whether debug information (e.g., the inner error from the <paramref name="error"/>) should 
        /// be included in the payload. This should only be used in debug scenarios.
        /// </param>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal virtual void WriteError(ODataError error, bool includeDebugInformation)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously writes an <see cref="ODataError"/> as the message payload.
        /// </summary>
        /// <param name="error">The error to write.</param>
        /// <param name="includeDebugInformation">
        /// A flag indicating whether debug information (e.g., the inner error from the <paramref name="error"/>) should 
        /// be included in the payload. This should only be used in debug scenarios.
        /// </param>
        /// <returns>A task representing the asynchronous operation of writing the error.</returns>
        /// <remarks>It is the responsibility of this method to flush the output before the task finishes.</remarks>
        internal virtual Task WriteErrorAsync(ODataError error, bool includeDebugInformation)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Error);
        }
#endif

        /// <summary>
        /// Writes the result of a $links query as the message payload.
        /// </summary>
        /// <param name="links">The entity reference links to write as message payload.</param>
        /// <param name="entitySet">The entity set of the navigation property.</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference links are being written, or null if none is available.</param>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal virtual void WriteEntityReferenceLinks(ODataEntityReferenceLinks links, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLinks);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously writes the result of a $links query as the message payload.
        /// </summary>
        /// <param name="links">The entity reference links to write as message payload.</param>
        /// <param name="entitySet">The entity set of the navigation property.</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference links are being written, or null if none is available.</param>
        /// <returns>A task representing the asynchronous writing of the entity reference links.</returns>
        /// <remarks>It is the responsibility of this method to flush the output before the task finishes.</remarks>
        internal virtual Task WriteEntityReferenceLinksAsync(ODataEntityReferenceLinks links, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLinks);
        }
#endif

        /// <summary>
        /// Writes a singleton result of a $links query as the message payload.
        /// </summary>
        /// <param name="link">The entity reference link to write as message payload.</param>
        /// <param name="entitySet">The entity set of the navigation property.</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference link is being written, or null if none is available.</param>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal virtual void WriteEntityReferenceLink(ODataEntityReferenceLink link, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLink);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously writes a singleton result of a $links query as the message payload.
        /// </summary>
        /// <param name="link">The link result to write as message payload.</param>
        /// <param name="entitySet">The entity set of the navigation property.</param>
        /// <param name="navigationProperty">The navigation property for which the entity reference link is being written, or null if none is available.</param>
        /// <returns>A running task representing the writing of the link.</returns>
        /// <remarks>It is the responsibility of this method to flush the output before the task finishes.</remarks>
        internal virtual Task WriteEntityReferenceLinkAsync(ODataEntityReferenceLink link, IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.EntityReferenceLink);
        }
#endif

        /// <summary>
        /// Writes a single value as the message body.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal virtual void WriteValue(object value)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Value);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously writes a single value as the message body.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <returns>A running task representing the writing of the value.</returns>
        /// <remarks>It is the responsibility of this method to flush the output before the task finishes.</remarks>
        internal virtual Task WriteValueAsync(object value)
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.Value);
        }
#endif

        /// <summary>
        /// Writes the metadata document as the message body.
        /// </summary>
        /// <remarks>It is the responsibility of this method to flush the output before the method returns.</remarks>
        internal virtual void WriteMetadataDocument()
        {
            DebugUtils.CheckNoExternalCallers();
            throw this.CreatePayloadKindNotSupportedException(ODataPayloadKind.MetadataDocument);
        }

        /// <summary>
        /// Asserts that the input context was created for synchronous operation.
        /// </summary>
        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Needs to access this in debug only.")]
        internal void AssertSynchronous()
        {
            DebugUtils.CheckNoExternalCallers();

#if DEBUG
            Debug.Assert(this.synchronous, "The method should only be called on a synchronous output context.");
#endif
        }

        /// <summary>
        /// Asserts that the input context was created for asynchronous operation.
        /// </summary>
        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Needs to access this in debug only.")]
        internal void AssertAsynchronous()
        {
            DebugUtils.CheckNoExternalCallers();

#if DEBUG
            Debug.Assert(!this.synchronous, "The method should only be called on an asynchronous output context.");
#endif
        }

        /// <summary>
        /// Perform the actual cleanup work.
        /// </summary>
        /// <param name="disposing">If 'true' this method is called from user code; if 'false' it is called by the runtime.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Creates an exception which reports that the specified payload kind if not support by this format.
        /// </summary>
        /// <param name="payloadKind">The payload kind which is not supported.</param>
        /// <returns>An exception to throw.</returns>
        private ODataException CreatePayloadKindNotSupportedException(ODataPayloadKind payloadKind)
        {
            return new ODataException(Strings.ODataOutputContext_UnsupportedPayloadKindForFormat(this.format.ToString(), payloadKind.ToString()));
        }
    }
}

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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Reader class used to read all OData payloads (entries, feeds, metadata documents, service documents, etc.).
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Main entry point for reader functionality")]
    public sealed class ODataMessageReader : IDisposable
    {
        /// <summary>The message for which the message reader was created.</summary>
        private readonly ODataMessage message;

        /// <summary>A flag indicating whether we are reading a request or a response message.</summary>
        private readonly bool readingResponse;

        /// <summary>The message reader settings to use when reading the message payload.</summary>
        private readonly ODataMessageReaderSettings settings;

        /// <summary>The model. Non-null if we do have metadata available.</summary>
        private readonly IEdmModel model;

        /// <summary>The <see cref="ODataVersion"/> to be used for reading the payload.</summary>
        private readonly ODataVersion version;

        /// <summary>The optional URL resolver to perform custom URL resolution for URLs read from the payload.</summary>
        private readonly IODataUrlResolver urlResolver;

        /// <summary>The resolver to use when determining an entity set's element type.</summary>
        private readonly EdmTypeResolver edmTypeResolver;

        /// <summary>Flag to ensure that only a single read method is called on the message reader.</summary>
        private bool readMethodCalled;

        /// <summary>true if Dispose() has been called on this message reader, false otherwise.</summary>
        private bool isDisposed;

        /// <summary>The input context used to read the message content.</summary>
        private ODataInputContext inputContext;

        /// <summary>The payload kind of the payload to be read with this reader.</summary>
        /// <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        private ODataPayloadKind readerPayloadKind = ODataPayloadKind.Unsupported;

        /// <summary>The <see cref="ODataFormat"/> of the payload to be read with this reader.</summary>
        /// <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        private ODataFormat format;

        /// <summary>The <see cref="MediaType"/> parsed from the content type header.</summary>
        /// <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        private MediaType contentType;

        /// <summary>The <see cref="Encoding"/> of the payload to be read with this reader.</summary>
        /// <remarks>This field is set implicitly when one of the read (or reader creation) methods is called.</remarks>
        private Encoding encoding;

        /// <summary>The batch boundary string if the payload to be read is a batch request or response.</summary>
        /// <remarks>This is set implicitly when the CreateBatchReader method is called.</remarks>
        private string batchBoundary;

        /// <summary>The media type resolver to use when interpreting the incoming content type.</summary>
        private MediaTypeResolver mediaTypeResolver;

        /// <summary>Storage for format specific states from payload kind detection.</summary>
        private Dictionary<ODataFormat, object> payloadKindDetectionFormatStates;

        /// <summary>Creates a new <see cref="T:Microsoft.Data.OData.ODataMessageReader" /> for the given request message.</summary>
        /// <param name="requestMessage">The request message for which to create the reader.</param>
        public ODataMessageReader(IODataRequestMessage requestMessage)
            : this(requestMessage, new ODataMessageReaderSettings())
        {
        }

        /// <summary>Creates a new <see cref="T:Microsoft.Data.OData.ODataMessageReader" /> for the given request message and message reader settings.</summary>
        /// <param name="requestMessage">The request message for which to create the reader.</param>
        /// <param name="settings">The message reader settings to use for reading the message payload.</param>
        public ODataMessageReader(IODataRequestMessage requestMessage, ODataMessageReaderSettings settings)
            : this(requestMessage, settings, null)
        {
        }

        /// <summary>
        /// Creates a new ODataMessageReader for the given request message and message reader settings.
        /// </summary>
        /// <param name="requestMessage">The request message for which to create the reader.</param>
        /// <param name="settings">The message reader settings to use for reading the message payload.</param>
        /// <param name="model">The model to use.</param>
        public ODataMessageReader(IODataRequestMessage requestMessage, ODataMessageReaderSettings settings, IEdmModel model)
        {
            ExceptionUtils.CheckArgumentNotNull(requestMessage, "requestMessage");

            // Clone the settings here so we can later modify them without changing the settings passed to us by the user
            this.settings = settings == null ? new ODataMessageReaderSettings() : new ODataMessageReaderSettings(settings);
            ReaderValidationUtils.ValidateMessageReaderSettings(this.settings, /*readingResponse*/ false);

            this.readingResponse = false;
            this.message = new ODataRequestMessage(requestMessage, /*writing*/ false, this.settings.DisableMessageStreamDisposal, this.settings.MessageQuotas.MaxReceivedMessageSize);
            this.urlResolver = requestMessage as IODataUrlResolver;
            this.version = ODataUtilsInternal.GetDataServiceVersion(this.message, this.settings.MaxProtocolVersion);
            this.model = model ?? EdmCoreModel.Instance;
            this.edmTypeResolver = new EdmTypeReaderResolver(this.model, this.settings.ReaderBehavior, this.version);

            ODataVersionChecker.CheckVersionSupported(this.version, this.settings);
        }

        /// <summary>Creates a new <see cref="T:System.Data.OData.ODataMessageReader" /> for the given response message.</summary>
        /// <param name="responseMessage">The response message for which to create the reader.</param>
        public ODataMessageReader(IODataResponseMessage responseMessage)
            : this(responseMessage, new ODataMessageReaderSettings())
        {
        }

        /// <summary>Creates a new <see cref="T:Microsoft.Data.OData.ODataMessageReader" /> for the given response message and message reader settings.</summary>
        /// <param name="responseMessage">The response message for which to create the reader.</param>
        /// <param name="settings">The message reader settings to use for reading the message payload.</param>
        public ODataMessageReader(IODataResponseMessage responseMessage, ODataMessageReaderSettings settings)
            : this(responseMessage, settings, null)
        {
        }

        /// <summary>
        /// Creates a new ODataMessageReader for the given response message and message reader settings.
        /// </summary>
        /// <param name="responseMessage">The response message for which to create the reader.</param>
        /// <param name="settings">The message reader settings to use for reading the message payload.</param>
        /// <param name="model">The model to use.</param>
        public ODataMessageReader(IODataResponseMessage responseMessage, ODataMessageReaderSettings settings, IEdmModel model)
        {
            ExceptionUtils.CheckArgumentNotNull(responseMessage, "responseMessage");

            // Clone the settings here so we can later modify them without changing the settings passed to us by the user
            this.settings = settings == null ? new ODataMessageReaderSettings() : new ODataMessageReaderSettings(settings);
            ReaderValidationUtils.ValidateMessageReaderSettings(this.settings, /*readingResponse*/ true);

            this.readingResponse = true;
            this.message = new ODataResponseMessage(responseMessage, /*writing*/ false, this.settings.DisableMessageStreamDisposal, this.settings.MessageQuotas.MaxReceivedMessageSize);
            this.urlResolver = responseMessage as IODataUrlResolver;
            this.version = ODataUtilsInternal.GetDataServiceVersion(this.message, this.settings.MaxProtocolVersion);
            this.model = model ?? EdmCoreModel.Instance;
            this.edmTypeResolver = new EdmTypeReaderResolver(this.model, this.settings.ReaderBehavior, this.version);

            // If the Preference-Applied header on the response message contains an annotation filter, we set the filter
            // to the reader settings if it's not already set, so that we would only read annotations that satisfy the filter.
            string annotationFilter = responseMessage.PreferenceAppliedHeader().AnnotationFilter;
            if (this.settings.ShouldIncludeAnnotation == null && !string.IsNullOrEmpty(annotationFilter))
            {
                this.settings.ShouldIncludeAnnotation = ODataUtils.CreateAnnotationFilter(annotationFilter);
            }

            ODataVersionChecker.CheckVersionSupported(this.version, this.settings);
        }

        /// <summary>
        /// The message reader settings to use when reading the message payload.
        /// </summary>
        internal ODataMessageReaderSettings Settings
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.settings;
            }
        }

        /// <summary>
        /// The media type resolver to use when interpreting the incoming content type.
        /// </summary>
        private MediaTypeResolver MediaTypeResolver
        {
            get
            {
                if (this.mediaTypeResolver == null)
                {
                    this.mediaTypeResolver = MediaTypeResolver.CreateReaderMediaTypeResolver(this.version, this.settings.ReaderBehavior.FormatBehaviorKind);
                }

                return this.mediaTypeResolver;
            }
        }

        /// <summary>Determines the potential payload kinds and formats of the payload being read and returns it.</summary>
        /// <returns>The set of potential payload kinds and formats for the payload being read by this reader.</returns>
        /// <remarks>When this method is called it first analyzes the content type and determines whether there
        /// are multiple matching payload kinds registered for the message's content type. If there are, it then
        /// runs the payload kind detection on all formats that have a matching payload kind registered.
        /// Note that this method can return multiple results if a payload is valid for multiple payload kinds but
        /// will always at most return a single result per payload kind.
        /// </remarks>
        public IEnumerable<ODataPayloadKindDetectionResult> DetectPayloadKind()
        {
            // We don't support payload kind detection in WCF DS server mode
            if (this.settings.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesServer)
            {
                throw new ODataException(Strings.ODataMessageReader_PayloadKindDetectionInServerMode);
            }

            IEnumerable<ODataPayloadKindDetectionResult> payloadKindsFromContentType;
            if (this.TryGetSinglePayloadKindResultFromContentType(out payloadKindsFromContentType))
            {
                return payloadKindsFromContentType;
            }

            this.payloadKindDetectionFormatStates = new Dictionary<ODataFormat, object>(ReferenceEqualityComparer<ODataFormat>.Instance);

            // Otherwise we have to do sniffing
            List<ODataPayloadKindDetectionResult> detectedPayloadKinds = new List<ODataPayloadKindDetectionResult>();
            try
            {
                // Group the payload kinds by format so we call the payload kind detection method only
                // once per format.
                IEnumerable<IGrouping<ODataFormat, ODataPayloadKindDetectionResult>> payloadKindFromContentTypeGroups = 
                    payloadKindsFromContentType.GroupBy(kvp => kvp.Format);

                foreach (IGrouping<ODataFormat, ODataPayloadKindDetectionResult> payloadKindGroup in payloadKindFromContentTypeGroups)
                {
                    ODataPayloadKindDetectionInfo detectionInfo = new ODataPayloadKindDetectionInfo(
                        this.contentType,
                        this.encoding,
                        this.settings, 
                        this.model,
                        payloadKindGroup.Select(pkg => pkg.PayloadKind));

                    // Call the payload kind detection code on the format
                    IEnumerable<ODataPayloadKind> detectionResult = this.readingResponse
                        ? payloadKindGroup.Key.DetectPayloadKind((IODataResponseMessage)this.message, detectionInfo)
                        : payloadKindGroup.Key.DetectPayloadKind((IODataRequestMessage)this.message, detectionInfo);

                    if (detectionResult != null)
                    {
                        foreach (ODataPayloadKind kind in detectionResult)
                        {
                            // Only include the payload kinds that we expect
                            if (payloadKindsFromContentType.Any(pk => pk.PayloadKind == kind))
                            {
                                Debug.Assert(!detectedPayloadKinds.Any(dpk => dpk.PayloadKind == kind), "Each kind must appear at most once.");
                                detectedPayloadKinds.Add(new ODataPayloadKindDetectionResult(kind, payloadKindGroup.Key));
                            }
                        }
                    }

                    this.payloadKindDetectionFormatStates.Add(payloadKindGroup.Key, detectionInfo.PayloadKindDetectionFormatState);
                }
            }
            finally
            {
                // We are done sniffing; stop buffering
                this.message.UseBufferingReadStream = false;
                this.message.BufferingReadStream.StopBuffering();
            }

            // Always sort by payload kind to guarantee stable order of results in case clients rely on it
            detectedPayloadKinds.Sort(this.ComparePayloadKindDetectionResult);

            return detectedPayloadKinds;
        }

#if ODATALIB_ASYNC
        /// <summary>Determines the potential payload kinds and formats of the payload being read and returns it.</summary>
        /// <returns>The set of potential payload kinds and formats for the payload being read by this reader.</returns>
        /// <remarks>When this method is called it first analyzes the content type and determines whether there
        /// are multiple matching payload kinds registered for the message's content type. If there are, it then
        /// runs the payload kind detection on all formats that have a matching payload kind registered.
        /// Note that this method can return multiple results if a payload is valid for multiple payload kinds but
        /// will always at most return a single result per payload kind.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Need to a return a task of an enumerable.")]
        public Task<IEnumerable<ODataPayloadKindDetectionResult>> DetectPayloadKindAsync()
        {
            // We don't support payload kind detection in WCF DS server mode
            if (this.settings.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesServer)
            {
                throw new ODataException(Strings.ODataMessageReader_PayloadKindDetectionInServerMode);
            }

            IEnumerable<ODataPayloadKindDetectionResult> payloadKindsFromContentType;
            if (this.TryGetSinglePayloadKindResultFromContentType(out payloadKindsFromContentType))
            {
                return TaskUtils.GetCompletedTask(payloadKindsFromContentType);
            }

            // The dictionary is filled by the GetPayloadKindDetectionTasks below.
            this.payloadKindDetectionFormatStates = new Dictionary<ODataFormat, object>(ReferenceEqualityComparer<ODataFormat>.Instance);

            // Otherwise we have to do sniffing
            List<ODataPayloadKindDetectionResult> detectedPayloadKinds = new List<ODataPayloadKindDetectionResult>();

            // NOTE: this relies on the lazy eval of the enumerator
            return Task.Factory.Iterate(this.GetPayloadKindDetectionTasks(payloadKindsFromContentType, detectedPayloadKinds))
                .FollowAlwaysWith(
                    t =>
                    {
                        // We are done sniffing; stop buffering.
                        this.message.UseBufferingReadStream = false;
                        this.message.BufferingReadStream.StopBuffering();
                    })
                .FollowOnSuccessWith(
                    t =>
                    {
                        // Always sort by payload kind to guarantee stable order of results in case clients rely on it
                        detectedPayloadKinds.Sort(this.ComparePayloadKindDetectionResult);

                        return (IEnumerable<ODataPayloadKindDetectionResult>)detectedPayloadKinds;
                    });
        }
#endif

        /// <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataReader" /> to read a feed.</summary>
        /// <returns>The created reader.</returns>
        public ODataReader CreateODataFeedReader()
        {
            return this.CreateODataFeedReader(/*entitySet*/null, /*expectedBaseEntityType*/null);
        }

        /// <summary>
        /// Creates an <see cref="ODataReader" /> to read a feed.
        /// </summary>
        /// <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
        /// <returns>The created reader.</returns>
        public ODataReader CreateODataFeedReader(IEdmEntityType expectedBaseEntityType)
        {
            return this.CreateODataFeedReader(/*entitySet*/null, expectedBaseEntityType);
        }

        /// <summary>
        /// Creates an <see cref="ODataReader" /> to read a feed.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to read entities for.</param>
        /// <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
        /// <returns>The created reader.</returns>
        public ODataReader CreateODataFeedReader(IEdmEntitySet entitySet, IEdmEntityType expectedBaseEntityType)
        {
            this.VerifyCanCreateODataFeedReader(entitySet, expectedBaseEntityType);
            expectedBaseEntityType = expectedBaseEntityType ?? this.edmTypeResolver.GetElementType(entitySet);
            return this.ReadFromInput(
                (context) => context.CreateFeedReader(entitySet, expectedBaseEntityType),
                ODataPayloadKind.Feed);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataReader" /> to read a feed.</summary>
        /// <returns>A running task for the created reader.</returns>
        public Task<ODataReader> CreateODataFeedReaderAsync()
        {
            return this.CreateODataFeedReaderAsync(/*entitySet*/null, /*entityType*/null);
        }

        /// <summary>
        /// Asynchronously creates an <see cref="ODataReader" /> to read a feed.
        /// </summary>
        /// <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
        /// <returns>A running task for the created reader.</returns>
        public Task<ODataReader> CreateODataFeedReaderAsync(IEdmEntityType expectedBaseEntityType)
        {
            return this.CreateODataFeedReaderAsync(/*entitySet*/null, expectedBaseEntityType);
        }

        /// <summary>
        /// Asynchronously creates an <see cref="ODataReader" /> to read a feed.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to read entities for.</param>
        /// <param name="expectedBaseEntityType">The expected base type for the entities in the feed.</param>
        /// <returns>A running task for the created reader.</returns>
        public Task<ODataReader> CreateODataFeedReaderAsync(IEdmEntitySet entitySet, IEdmEntityType expectedBaseEntityType)
        {
            this.VerifyCanCreateODataFeedReader(entitySet, expectedBaseEntityType);
            expectedBaseEntityType = expectedBaseEntityType ?? this.edmTypeResolver.GetElementType(entitySet);
            return this.ReadFromInputAsync(
                (context) => context.CreateFeedReaderAsync(entitySet, expectedBaseEntityType),
                ODataPayloadKind.Feed);
        }
#endif

        /// <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataReader" /> to read an entry.</summary>
        /// <returns>The created reader.</returns>
        public ODataReader CreateODataEntryReader()
        {
            return this.CreateODataEntryReader(/*entitySet*/null, /*entityType*/null);
        }

        /// <summary>
        /// Creates an <see cref="ODataReader" /> to read an entry.
        /// </summary>
        /// <param name="entityType">The expected entity type for the entry to be read.</param>
        /// <returns>The created reader.</returns>
        public ODataReader CreateODataEntryReader(IEdmEntityType entityType)
        {
            return this.CreateODataEntryReader(/*entitySet*/null, entityType);
        }

        /// <summary>
        /// Creates an <see cref="ODataReader" /> to read an entry.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to read entities for.</param>
        /// <param name="entityType">The expected entity type for the entry to be read.</param>
        /// <returns>The created reader.</returns>
        public ODataReader CreateODataEntryReader(IEdmEntitySet entitySet, IEdmEntityType entityType)
        {
            this.VerifyCanCreateODataEntryReader(entitySet, entityType);
            entityType = entityType ?? this.edmTypeResolver.GetElementType(entitySet);
            return this.ReadFromInput(
                (context) => context.CreateEntryReader(entitySet, entityType),
                ODataPayloadKind.Entry);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously creates an <see cref="T:System.Data.OData.ODataReader" /> to read an entry.</summary>
        /// <returns>A running task for the created reader.</returns>
        public Task<ODataReader> CreateODataEntryReaderAsync()
        {
            return this.CreateODataEntryReaderAsync(/*entitySet*/null, /*entityType*/null);
        }

        /// <summary>
        /// Asynchronously creates an <see cref="ODataReader" /> to read an entry.
        /// </summary>
        /// <param name="entityType">The expected entity type for the entry to be read.</param>
        /// <returns>A running task for the created reader.</returns>
        public Task<ODataReader> CreateODataEntryReaderAsync(IEdmEntityType entityType)
        {
            return this.CreateODataEntryReaderAsync(/*entitySet*/null, entityType);
        }

        /// <summary>
        /// Asynchronously creates an <see cref="ODataReader" /> to read an entry.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to read entities for.</param>
        /// <param name="entityType">The expected entity type for the entry to be read.</param>
        /// <returns>A running task for the created reader.</returns>
        public Task<ODataReader> CreateODataEntryReaderAsync(IEdmEntitySet entitySet, IEdmEntityType entityType)
        {
            this.VerifyCanCreateODataEntryReader(entitySet, entityType);
            entityType = entityType ?? this.edmTypeResolver.GetElementType(entitySet);
            return this.ReadFromInputAsync(
                (context) => context.CreateEntryReaderAsync(entitySet, entityType),
                ODataPayloadKind.Entry);
        }
#endif

        /// <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataCollectionReader" /> to read a collection of primitive or complex values (as result of a service operation invocation).</summary>
        /// <returns>The created collection reader.</returns>
        public ODataCollectionReader CreateODataCollectionReader()
        {
            return this.CreateODataCollectionReader(null /*expectedItemTypeReference*/);
        }

        /// <summary>
        /// Creates an <see cref="ODataCollectionReader" /> to read a collection of primitive or complex values (as result of a service operation invocation).
        /// </summary>
        /// <param name="expectedItemTypeReference">The expected type reference for the items in the collection.</param>
        /// <returns>The created collection reader.</returns>
        public ODataCollectionReader CreateODataCollectionReader(IEdmTypeReference expectedItemTypeReference)
        {
            this.VerifyCanCreateODataCollectionReader(expectedItemTypeReference);
            return this.ReadFromInput(
                (context) => context.CreateCollectionReader(expectedItemTypeReference),
                ODataPayloadKind.Collection);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataCollectionReader" /> to read a collection of primitive or complex values (as result of a service operation invocation).</summary>
        /// <returns>A running task for the created collection reader.</returns>
        public Task<ODataCollectionReader> CreateODataCollectionReaderAsync()
        {
            return this.CreateODataCollectionReaderAsync(null /*expectedItemTypeReference*/);
        }

        /// <summary>
        /// Asynchronously creates an <see cref="ODataCollectionReader" /> to read a collection of primitive or complex values (as result of a service operation invocation).
        /// </summary>
        /// <param name="expectedItemTypeReference">The expected type reference for the items in the collection.</param>
        /// <returns>A running task for the created collection reader.</returns>
        public Task<ODataCollectionReader> CreateODataCollectionReaderAsync(IEdmTypeReference expectedItemTypeReference)
        {
            this.VerifyCanCreateODataCollectionReader(expectedItemTypeReference);
            return this.ReadFromInputAsync(
                (context) => context.CreateCollectionReaderAsync(expectedItemTypeReference),
                ODataPayloadKind.Collection);
        }

#endif

        /// <summary>Creates an <see cref="T:Microsoft.Data.OData.ODataBatchReader" /> to read a batch of requests or responses.</summary>
        /// <returns>The created batch reader.</returns>
        public ODataBatchReader CreateODataBatchReader()
        {
            this.VerifyCanCreateODataBatchReader();
            return this.ReadFromInput(
                (context) => context.CreateBatchReader(this.batchBoundary),
                ODataPayloadKind.Batch);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously creates an <see cref="T:Microsoft.Data.OData.ODataBatchReader" /> to read a batch of requests or responses.</summary>
        /// <returns>A running task for the created batch reader.</returns>
        public Task<ODataBatchReader> CreateODataBatchReaderAsync()
        {
            this.VerifyCanCreateODataBatchReader();
            return this.ReadFromInputAsync(
                (context) => context.CreateBatchReaderAsync(this.batchBoundary),
                ODataPayloadKind.Batch);
        }
#endif

        /// <summary>
        /// Creates an <see cref="ODataParameterReader" /> to read the parameters for <paramref name="functionImport"/>.
        /// </summary>
        /// <param name="functionImport">The function import whose parameters are being read.</param>
        /// <returns>The created parameter reader.</returns>
        public ODataParameterReader CreateODataParameterReader(IEdmFunctionImport functionImport)
        {
            this.VerifyCanCreateODataParameterReader(functionImport);
            return this.ReadFromInput(
                (context) => context.CreateParameterReader(functionImport),
                ODataPayloadKind.Parameter);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously creates an <see cref="ODataParameterReader" /> to read the parameters for <paramref name="functionImport"/>.
        /// </summary>
        /// <param name="functionImport">The function import whose parameters are being read.</param>
        /// <returns>A running task for the created parameter reader.</returns>
        public Task<ODataParameterReader> CreateODataParameterReaderAsync(IEdmFunctionImport functionImport)
        {
            this.VerifyCanCreateODataParameterReader(functionImport);
            return this.ReadFromInputAsync(
                (context) => context.CreateParameterReaderAsync(functionImport),
                ODataPayloadKind.Parameter);
        }
#endif

        /// <summary>Reads a service document payload.</summary>
        /// <returns>The service document read.</returns>
        public ODataWorkspace ReadServiceDocument()
        {
            this.VerifyCanReadServiceDocument();
            return this.ReadFromInput(
                (context) => context.ReadServiceDocument(),
                ODataPayloadKind.ServiceDocument);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously reads a service document payload.</summary>
        /// <returns>A task representing the asynchronous operation of reading the service document.</returns>
        public Task<ODataWorkspace> ReadServiceDocumentAsync()
        {
            this.VerifyCanReadServiceDocument();
            return this.ReadFromInputAsync(
                (context) => context.ReadServiceDocumentAsync(),
                ODataPayloadKind.ServiceDocument);
        }
#endif

        /// <summary>Reads an <see cref="T:Microsoft.Data.OData.ODataProperty" /> as message payload.</summary>
        /// <returns>The property read from the payload.</returns>
        public ODataProperty ReadProperty()
        {
            return this.ReadProperty((IEdmTypeReference)null);
        }

        /// <summary>
        /// Reads an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        /// <returns>The property read from the payload.</returns>
        public ODataProperty ReadProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            this.VerifyCanReadProperty(expectedPropertyTypeReference);
            return this.ReadFromInput(
                (context) => context.ReadProperty(/*property*/null, expectedPropertyTypeReference),
                ODataPayloadKind.Property);
        }

        /// <summary>
        /// Reads an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="property">The metadata of the property to read.</param>
        /// <returns>The property read from the payload.</returns>
        public ODataProperty ReadProperty(IEdmStructuralProperty property)
        {
            this.VerifyCanReadProperty(property);
            return this.ReadFromInput(
                (context) => context.ReadProperty(property, property.Type),
                ODataPayloadKind.Property);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously reads an <see cref="T:Microsoft.Data.OData.ODataProperty" /> as message payload.</summary>
        /// <returns>A task representing the asynchronous operation of reading the property.</returns>
        public Task<ODataProperty> ReadPropertyAsync()
        {
            return this.ReadPropertyAsync((IEdmTypeReference)null);
        }

        /// <summary>
        /// Asynchronously reads an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        /// <returns>A task representing the asynchronous operation of reading the property.</returns>
        public Task<ODataProperty> ReadPropertyAsync(IEdmTypeReference expectedPropertyTypeReference)
        {
            this.VerifyCanReadProperty(expectedPropertyTypeReference);
            return this.ReadFromInputAsync(
                (context) => context.ReadPropertyAsync(/*propertyOrFunctionImport*/null, expectedPropertyTypeReference),
                ODataPayloadKind.Property);
        }

        /// <summary>
        /// Asynchronously reads an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="property">The metadata of the property to read.</param>
        /// <returns>A task representing the asynchronous operation of reading the property.</returns>
        public Task<ODataProperty> ReadPropertyAsync(IEdmStructuralProperty property)
        {
            this.VerifyCanReadProperty(property);
            return this.ReadFromInputAsync(
                (context) => context.ReadPropertyAsync(property, property.Type),
                ODataPayloadKind.Property);
        }

#endif

        /// <summary>Reads an <see cref="T:Microsoft.Data.OData.ODataError" /> as the message payload.</summary>
        /// <returns>The <see cref="T:Microsoft.Data.OData.ODataError" /> read from the message payload.</returns>
        public ODataError ReadError()
        {
            this.VerifyCanReadError();
            return this.ReadFromInput(
                (context) => context.ReadError(),
                ODataPayloadKind.Error);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously reads an <see cref="T:Microsoft.Data.OData.ODataError" /> as the message payload.</summary>
        /// <returns>A task representing the asynchronous operation of reading the error.</returns>
        public Task<ODataError> ReadErrorAsync()
        {
            this.VerifyCanReadError();
            return this.ReadFromInputAsync(
                (context) => context.ReadErrorAsync(),
                ODataPayloadKind.Error);
        }
#endif

        /// <summary>Reads the result of a $links query (entity reference links) as the message payload.</summary>
        /// <returns>The entity reference links read as message payload.</returns>
        public ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            return this.ReadEntityReferenceLinks(/*navigationProperty*/null);
        }

        /// <summary>
        /// Reads the result of a $links query (entity reference links) as the message payload.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which to read the entity reference links.</param>
        /// <returns>The entity reference links read as message payload.</returns>
        public ODataEntityReferenceLinks ReadEntityReferenceLinks(IEdmNavigationProperty navigationProperty)
        {
            this.VerifyCanReadEntityReferenceLinks(navigationProperty);
            return this.ReadFromInput(
                (context) => context.ReadEntityReferenceLinks(navigationProperty),
                ODataPayloadKind.EntityReferenceLinks);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously reads the result of a $links query as the message payload.</summary>
        /// <returns>A task representing the asynchronous reading of the entity reference links.</returns>
        public Task<ODataEntityReferenceLinks> ReadEntityReferenceLinksAsync()
        {
            return this.ReadEntityReferenceLinksAsync(/*navigationProperty*/null);
        }

        /// <summary>
        /// Asynchronously reads the result of a $links query as the message payload.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which to read the entity reference links.</param>
        /// <returns>A task representing the asynchronous reading of the entity reference links.</returns>
        public Task<ODataEntityReferenceLinks> ReadEntityReferenceLinksAsync(IEdmNavigationProperty navigationProperty)
        {
            this.VerifyCanReadEntityReferenceLinks(navigationProperty);
            return this.ReadFromInputAsync(
                (context) => context.ReadEntityReferenceLinksAsync(navigationProperty),
                ODataPayloadKind.EntityReferenceLinks);
        }
#endif

        /// <summary>Reads a singleton result of a $links query (entity reference link) as the message payload.</summary>
        /// <returns>The entity reference link read from the message payload.</returns>
        public ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            return this.ReadEntityReferenceLink(/*navigationProperty*/null);
        }

        /// <summary>
        /// Reads a singleton result of a $links query (entity reference link) as the message payload.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which to read the entity reference link.</param>
        /// <returns>The entity reference link read from the message payload.</returns>
        public ODataEntityReferenceLink ReadEntityReferenceLink(IEdmNavigationProperty navigationProperty)
        {
            this.VerifyCanReadEntityReferenceLink();
            return this.ReadFromInput(
                (context) => context.ReadEntityReferenceLink(navigationProperty),
                ODataPayloadKind.EntityReferenceLink);
        }

#if ODATALIB_ASYNC
        /// <summary>Asynchronously reads a singleton result of a $links query (entity reference link) as the message payload.</summary>
        /// <returns>A running task representing the reading of the entity reference link.</returns>
        public Task<ODataEntityReferenceLink> ReadEntityReferenceLinkAsync()
        {
            return this.ReadEntityReferenceLinkAsync(/*navigationProperty*/null);
        }

        /// <summary>
        /// Asynchronously reads a singleton result of a $links query (entity reference link) as the message payload.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which to read the entity reference link.</param>
        /// <returns>A running task representing the reading of the entity reference link.</returns>
        public Task<ODataEntityReferenceLink> ReadEntityReferenceLinkAsync(IEdmNavigationProperty navigationProperty)
        {
            this.VerifyCanReadEntityReferenceLink();
            return this.ReadFromInputAsync(
                (context) => context.ReadEntityReferenceLinkAsync(navigationProperty),
                ODataPayloadKind.EntityReferenceLink);
        }
#endif

        /// <summary>
        /// Reads a single value as the message body.
        /// </summary>
        /// <param name="expectedTypeReference">The expected type reference for the value to be read; null if no expected type is available.</param>
        /// <returns>The read value.</returns>
        public object ReadValue(IEdmTypeReference expectedTypeReference)
        {
            ODataPayloadKind[] supportedPayloadKinds = this.VerifyCanReadValue(expectedTypeReference);

            return this.ReadFromInput(
                (context) => context.ReadValue((IEdmPrimitiveTypeReference)expectedTypeReference),
                supportedPayloadKinds);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously reads a single value as the message body.
        /// </summary>
        /// <param name="expectedTypeReference">The expected type reference for the value to be read; null if no expected type is available.</param>
        /// <returns>A running task representing the reading of the value.</returns>
        public Task<object> ReadValueAsync(IEdmTypeReference expectedTypeReference)
        {
            ODataPayloadKind[] supportedPayloadKinds = this.VerifyCanReadValue(expectedTypeReference);

            return this.ReadFromInputAsync(
                (context) => context.ReadValueAsync((IEdmPrimitiveTypeReference)expectedTypeReference),
                supportedPayloadKinds);
        }
#endif

        /// <summary>Reads the message body as metadata document.</summary>
        /// <returns>Returns <see cref="T:Microsoft.Data.Edm.IEdmModel" />.</returns>
        public IEdmModel ReadMetadataDocument()
        {
            this.VerifyCanReadMetadataDocument();
            return this.ReadFromInput(
                (context) => context.ReadMetadataDocument(),
                ODataPayloadKind.MetadataDocument);
        }

        /// <summary><see cref="M:System.IDisposable.Dispose()" /> implementation to cleanup unmanaged resources of the reader. </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines the format of the payload being read and returns it.
        /// </summary>
        /// <returns>The format of the payload being read by this reader.</returns>
        /// <remarks>
        /// The format of the payload is determined when starting to read the message; 
        /// if this method is called before reading has started it will throw.
        /// </remarks>
        internal ODataFormat GetFormat()
        {
            DebugUtils.CheckNoExternalCallers();
            if (this.format == null)
            {
                throw new ODataException(Strings.ODataMessageReader_GetFormatCalledBeforeReadingStarted);
            }

            return this.format;
        }

        /// <summary>
        /// Processes the content type header of the message to determine the format of the payload, the encoding, and the payload kind.
        /// </summary>
        /// <param name="payloadKinds">All possible kinds of payload to be read with this message reader; must not include ODataPayloadKind.Unsupported.</param>
        private void ProcessContentType(params ODataPayloadKind[] payloadKinds)
        {
            Debug.Assert(!payloadKinds.Contains(ODataPayloadKind.Unsupported), "!payloadKinds.Contains(ODataPayloadKind.Unsupported)");
            Debug.Assert(this.format == null, "this.format == null");
            Debug.Assert(this.readerPayloadKind == ODataPayloadKind.Unsupported, "this.readerPayloadKind == ODataPayloadKind.Unsupported");
            
            // Set the format, encoding and payload kind.
            string contentTypeHeader = this.GetContentTypeHeader();
            this.format = MediaTypeUtils.GetFormatFromContentType(contentTypeHeader, payloadKinds, this.MediaTypeResolver, out this.contentType, out this.encoding, out this.readerPayloadKind, out this.batchBoundary);
        }

        /// <summary>
        /// Gets the content type header of the message and validates that it is present and not empty.
        /// </summary>
        /// <returns>The content type header of the message.</returns>
        private string GetContentTypeHeader()
        {
            string contentTypeHeader = this.message.GetHeader(ODataConstants.ContentTypeHeader);
            contentTypeHeader = contentTypeHeader == null ? null : contentTypeHeader.Trim();
            if (string.IsNullOrEmpty(contentTypeHeader))
            {
                throw new ODataContentTypeException(Strings.ODataMessageReader_NoneOrEmptyContentTypeHeader);
            }

            return contentTypeHeader;
        }

        /// <summary>
        /// Verify arguments for creation of an <see cref="ODataReader" /> to read a feed.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to read entities for.</param>
        /// <param name="expectedBaseEntityType">The expected base entity type for the entities in the feed.</param>
        private void VerifyCanCreateODataFeedReader(IEdmEntitySet entitySet, IEdmEntityType expectedBaseEntityType)
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (!this.model.IsUserModel())
            {
                if (entitySet != null)
                {
                    throw new ArgumentException(Strings.ODataMessageReader_EntitySetSpecifiedWithoutMetadata("entitySet"), "entitySet");
                }

                if (expectedBaseEntityType != null)
                {
                    throw new ArgumentException(Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("expectedBaseEntityType"), "expectedBaseEntityType");
                }
            }
        }

        /// <summary>
        /// Verify arguments for creation of an <see cref="ODataReader" /> to read an entry.
        /// </summary>
        /// <param name="entitySet">The entity set we are going to read entities for.</param>
        /// <param name="entityType">The expected entity type for the entry to be read.</param>
        private void VerifyCanCreateODataEntryReader(IEdmEntitySet entitySet, IEdmEntityType entityType)
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (!this.model.IsUserModel())
            {
                if (entitySet != null)
                {
                    throw new ArgumentException(Strings.ODataMessageReader_EntitySetSpecifiedWithoutMetadata("entitySet"), "entitySet");
                }

                if (entityType != null)
                {
                    throw new ArgumentException(Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("entityType"), "entityType");
                }
            }
        }

        /// <summary>
        /// Verify arguments for creation of an <see cref="ODataCollectionReader" /> to read a collection of primitive or complex values 
        /// (as result of a service operation invocation).
        /// </summary>
        /// <param name="expectedItemTypeReference">The expected type for the items in the collection.</param>
        private void VerifyCanCreateODataCollectionReader(IEdmTypeReference expectedItemTypeReference)
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (expectedItemTypeReference != null)
            {
                if (!this.model.IsUserModel())
                {
                    throw new ArgumentException(Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("expectedItemTypeReference"), "expectedItemTypeReference");
                }

                if (!expectedItemTypeReference.IsODataPrimitiveTypeKind() && expectedItemTypeReference.TypeKind() != EdmTypeKind.Complex)
                {
                    throw new ArgumentException(
                        Strings.ODataMessageReader_ExpectedCollectionTypeWrongKind(expectedItemTypeReference.TypeKind().ToString()),
                        "expectedItemTypeReference");
                }
            }
        }

        /// <summary>
        /// Verify arguments for creation of a batch as the message body.
        /// </summary>
        private void VerifyCanCreateODataBatchReader()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
        }

        /// <summary>
        /// Verify arguments for creation of an <see cref="ODataParameterReader" /> to read the parameters for <paramref name="functionImport"/>.
        /// </summary>
        /// <param name="functionImport">The function import whose parameters are being read.</param>
        private void VerifyCanCreateODataParameterReader(IEdmFunctionImport functionImport)
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (this.readingResponse)
            {
                throw new ODataException(Strings.ODataMessageReader_ParameterPayloadInResponse);
            }

            ODataVersionChecker.CheckParameterPayload(this.version);

            if (functionImport != null && !this.model.IsUserModel())
            {
                throw new ArgumentException(Strings.ODataMessageReader_FunctionImportSpecifiedWithoutMetadata("functionImport"), "functionImport");
            }
        }

        /// <summary>
        /// Verify arguments for reading of a service document payload.
        /// </summary>
        private void VerifyCanReadServiceDocument()
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (!this.readingResponse)
            {
                throw new ODataException(Strings.ODataMessageReader_ServiceDocumentInRequest);
            }
        }

        /// <summary>
        /// Verify arguments for reading of a metadata document payload.
        /// </summary>
        private void VerifyCanReadMetadataDocument()
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (!this.readingResponse)
            {
                throw new ODataException(Strings.ODataMessageReader_MetadataDocumentInRequest);
            }
        }

        /// <summary>
        /// Verify arguments for reading of an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="property">The metadata of the property to read.</param>
        private void VerifyCanReadProperty(IEdmStructuralProperty property)
        {
            if (property == null)
            {
                return;
            }

            this.VerifyCanReadProperty(property.Type);
        }

        /// <summary>
        /// Verify arguments for reading of an <see cref="ODataProperty"/> as message payload.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        private void VerifyCanReadProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (expectedPropertyTypeReference != null)
            {
                if (!this.model.IsUserModel())
                {
                    throw new ArgumentException(Strings.ODataMessageReader_ExpectedTypeSpecifiedWithoutMetadata("expectedPropertyTypeReference"), "expectedPropertyTypeReference");
                }

                IEdmCollectionType collectionType = expectedPropertyTypeReference.Definition as IEdmCollectionType;
                if (collectionType != null && collectionType.ElementType.IsODataEntityTypeKind())
                {
                    throw new ArgumentException(Strings.ODataMessageReader_ExpectedPropertyTypeEntityCollectionKind, "expectedPropertyTypeReference");
                }

                if (expectedPropertyTypeReference.IsODataEntityTypeKind())
                {
                    throw new ArgumentException(Strings.ODataMessageReader_ExpectedPropertyTypeEntityKind, "expectedPropertyTypeReference");
                }
                else if (expectedPropertyTypeReference.IsStream())
                {
                    throw new ArgumentException(Strings.ODataMessageReader_ExpectedPropertyTypeStream, "expectedPropertyTypeReference");
                }
            }
        }

        /// <summary>
        /// Verify arguments for reading of an <see cref="ODataError"/> as the message payload.
        /// </summary>
        private void VerifyCanReadError()
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (!this.readingResponse)
            {
                // top-level errors can only be read for response messages
                throw new ODataException(Strings.ODataMessageReader_ErrorPayloadInRequest);
            }
        }

        /// <summary>
        /// Verify arguments for reading of the result of a $links query (entity reference links) as the message payload.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for which to read the entity reference links.</param>
        private void VerifyCanReadEntityReferenceLinks(IEdmNavigationProperty navigationProperty)
        {
            // NOTE: we decided to not stream links for now but only make reading them async.
            this.VerifyReaderNotDisposedAndNotUsed();

            if (!this.readingResponse)
            {
                throw new ODataException(Strings.ODataMessageReader_EntityReferenceLinksInRequestNotAllowed);
            }

            // We require a collection navigation property in order to read entity reference links
            if (navigationProperty != null && !navigationProperty.Type.IsCollection())
            {
                throw new ODataException(Strings.ODataMessageReader_SingletonNavigationPropertyForEntityReferenceLinks(
                    navigationProperty.Name, 
                    navigationProperty.DeclaringEntityType().FullName()));
            }
        }

        /// <summary>
        /// Verify arguments for reading of a singleton result of a $links query (entity reference link) as the message payload.
        /// </summary>
        private void VerifyCanReadEntityReferenceLink()
        {
            this.VerifyReaderNotDisposedAndNotUsed();
        }

        /// <summary>
        /// Verify arguments for reading of a single value as the message body.
        /// </summary>
        /// <param name="expectedTypeReference">The expected type reference for the value to be read; null if no expected type is available.</param>
        /// <returns>The payload kinds allowed for the given expected type.</returns>
        private ODataPayloadKind[] VerifyCanReadValue(IEdmTypeReference expectedTypeReference)
        {
            this.VerifyReaderNotDisposedAndNotUsed();

            if (expectedTypeReference != null)
            {
                if (!expectedTypeReference.IsODataPrimitiveTypeKind())
                {
                    throw new ArgumentException(
                        Strings.ODataMessageReader_ExpectedValueTypeWrongKind(expectedTypeReference.TypeKind().ToString()),
                        "expectedTypeReference");
                }

                if (expectedTypeReference.IsBinary())
                {
                    return new ODataPayloadKind[] { ODataPayloadKind.BinaryValue };
                }
                else
                {
                    return new ODataPayloadKind[] { ODataPayloadKind.Value };
                }
            }

            return new ODataPayloadKind[] { ODataPayloadKind.Value, ODataPayloadKind.BinaryValue };
        }

        /// <summary>
        /// Verifies that the ODataMessageReader has not been used before; an ODataMessageReader can only be used to
        /// read a single message payload but cannot be reused later.
        /// </summary>
        private void VerifyReaderNotDisposedAndNotUsed()
        {
            this.VerifyNotDisposed();
            if (this.readMethodCalled)
            {
                throw new ODataException(Strings.ODataMessageReader_ReaderAlreadyUsed);
            }

            if (this.message.BufferingReadStream != null && this.message.BufferingReadStream.IsBuffering)
            {
                throw new ODataException(Strings.ODataMessageReader_PayloadKindDetectionRunning);
            }

            this.readMethodCalled = true;
        }

        /// <summary>
        /// Check if the object has been disposed. Throws an ObjectDisposedException if the object has already been disposed.
        /// </summary>
        private void VerifyNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        /// <summary>
        /// Perform the actual cleanup work.
        /// </summary>
        /// <param name="disposing">If 'true' this method is called from user code; if 'false' it is called by the runtime.</param>
        private void Dispose(bool disposing)
        {
            this.isDisposed = true;
            if (disposing)
            {
                try
                {
                    if (this.inputContext != null)
                    {
                        this.inputContext.Dispose();
                    }
                }
                finally
                {
                    this.inputContext = null;
                }

                // If we still have a buffering read stream only the payload kind detection was triggered but
                // the actual reading never started. Dispose the stream now (if disposal is not disabled).
                if (!this.settings.DisableMessageStreamDisposal && this.message.BufferingReadStream != null)
                {
                    this.message.BufferingReadStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Method which creates an input context around the input message and calls a func to read the input.
        /// </summary>
        /// <typeparam name="T">The type returned by the read method.</typeparam>
        /// <param name="readFunc">The read function which will be called over the created input context.</param>
        /// <param name="payloadKinds">All possible kinds of payload to read.</param>
        /// <returns>The read value from the input.</returns>
        private T ReadFromInput<T>(Func<ODataInputContext, T> readFunc, params ODataPayloadKind[] payloadKinds) where T : class
        {
            this.ProcessContentType(payloadKinds);
            Debug.Assert(this.format != null, "By now we should have figured out which format to use.");

            object payloadKindDetectionFormatState = null;
            if (this.payloadKindDetectionFormatStates != null)
            {
                this.payloadKindDetectionFormatStates.TryGetValue(this.format, out payloadKindDetectionFormatState);
            }

            this.inputContext = this.format.CreateInputContext(
                this.readerPayloadKind,
                this.message,
                this.contentType,
                this.encoding,
                this.settings,
                this.version,
                this.readingResponse,
                this.model,
                this.urlResolver,
                payloadKindDetectionFormatState);

            return readFunc(this.inputContext);
        }

        /// <summary>
        /// Gets all the supported payload kinds for a given content type across all formats and returns them.
        /// </summary>
        /// <param name="payloadKindResults">The set of supported payload kinds for the content type of the message.</param>
        /// <returns>true if no or a single payload kind was found for the content type; false if more than one payload kind was found.</returns>
        private bool TryGetSinglePayloadKindResultFromContentType(out IEnumerable<ODataPayloadKindDetectionResult> payloadKindResults)
        {
            if (this.message.UseBufferingReadStream == true)
            {
                // This method must be called at most once and not after the actual reading has started.
                throw new ODataException(Strings.ODataMessageReader_DetectPayloadKindMultipleTimes);
            }

            string contentTypeHeader = this.GetContentTypeHeader();
            IList<ODataPayloadKindDetectionResult> payloadKindsFromContentType = MediaTypeUtils.GetPayloadKindsForContentType(contentTypeHeader, this.MediaTypeResolver, out this.contentType, out this.encoding);
            payloadKindResults = payloadKindsFromContentType.Where(r => ODataUtilsInternal.IsPayloadKindSupported(r.PayloadKind, !this.readingResponse));

            if (payloadKindResults.Count() > 1)
            {
                // Set UseBufferingReadStream to 'true' to use the buffering read stream when
                // being asked for the message stream.
                this.message.UseBufferingReadStream = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares two payload kind detection results.
        /// </summary>
        /// <param name="first">The first <see cref="ODataPayloadKindDetectionResult"/>.</param>
        /// <param name="second">The second <see cref="ODataPayloadKindDetectionResult"/>.</param>
        /// <returns>-1 if <paramref name="first"/> is considered less than <paramref name="second"/>,
        /// 0 if the kinds are considered equal, 1 if <paramref name="first"/> is considered greater than <paramref name="second"/>.</returns>
        private int ComparePayloadKindDetectionResult(ODataPayloadKindDetectionResult first, ODataPayloadKindDetectionResult second)
        {
            ODataPayloadKind firstKind = first.PayloadKind;
            ODataPayloadKind secondKind = second.PayloadKind;

            if (firstKind == secondKind)
            {
                return 0;
            }

            return first.PayloadKind < second.PayloadKind ? -1 : 1;
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Get an enumerable of tasks to get the supported payload kinds for all formats.
        /// </summary>
        /// <param name="payloadKindsFromContentType">All payload kinds for which we found matches in some format based on the content type.</param>
        /// <param name="detectionResults">The list of combined detection results after sniffing.</param>
        /// <returns>A lazy enumerable of tasks to get the supported payload kinds for all formats.</returns>
        private IEnumerable<Task> GetPayloadKindDetectionTasks(
            IEnumerable<ODataPayloadKindDetectionResult> payloadKindsFromContentType,
            List<ODataPayloadKindDetectionResult> detectionResults)
        {
            // Group the payload kinds by format so we call the payload kind detection method only
            // once per format.
            IEnumerable<IGrouping<ODataFormat, ODataPayloadKindDetectionResult>> payloadKindFromContentTypeGroups =
                payloadKindsFromContentType.GroupBy(kvp => kvp.Format);

            foreach (IGrouping<ODataFormat, ODataPayloadKindDetectionResult> payloadKindGroup in payloadKindFromContentTypeGroups)
            {
                ODataPayloadKindDetectionInfo detectionInfo = new ODataPayloadKindDetectionInfo(
                    this.contentType,
                    this.encoding,
                    this.settings, 
                    this.model, 
                    payloadKindGroup.Select(pkg => pkg.PayloadKind));

                // Call the payload kind detection code on the format
                Task<IEnumerable<ODataPayloadKind>> detectionResult = this.readingResponse
                    ? payloadKindGroup.Key.DetectPayloadKindAsync((IODataResponseMessageAsync)this.message, detectionInfo)
                    : payloadKindGroup.Key.DetectPayloadKindAsync((IODataRequestMessageAsync)this.message, detectionInfo);

                yield return detectionResult
                    .FollowOnSuccessWith(
                    t =>
                    {
                        IEnumerable<ODataPayloadKind> result = t.Result;
                        if (result != null)
                        {
                            foreach (ODataPayloadKind kind in result)
                            {
                                // Only include the payload kinds that we expect
                                if (payloadKindsFromContentType.Any(pk => pk.PayloadKind == kind))
                                {
                                    Debug.Assert(!detectionResults.Any(dpk => dpk.PayloadKind == kind), "Each kind must appear at most once.");
                                    detectionResults.Add(new ODataPayloadKindDetectionResult(kind, payloadKindGroup.Key));
                                }
                            }
                        }

                        this.payloadKindDetectionFormatStates.Add(payloadKindGroup.Key, detectionInfo.PayloadKindDetectionFormatState);
                    });
            }
        }

        /// <summary>
        /// Method which asynchronously creates an input context around the input message and calls a func to read the input.
        /// </summary>
        /// <typeparam name="T">The type returned by the read method.</typeparam>
        /// <param name="readFunc">The read function which will be called over the created input context.</param>
        /// <param name="payloadKinds">All possible kinds of payload to read.</param>
        /// <returns>A task which when completed return the read value from the input.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller.")]
        private Task<T> ReadFromInputAsync<T>(Func<ODataInputContext, Task<T>> readFunc, params ODataPayloadKind[] payloadKinds) where T : class
        {
            this.ProcessContentType(payloadKinds);
            Debug.Assert(this.format != null, "By now we should have figured out which format to use.");

            object payloadKindDetectionFormatState = null;
            if (this.payloadKindDetectionFormatStates != null)
            {
                this.payloadKindDetectionFormatStates.TryGetValue(this.format, out payloadKindDetectionFormatState);
            }

            return this.format.CreateInputContextAsync(
                this.readerPayloadKind,
                this.message,
                this.contentType,
                this.encoding,
                this.settings,
                this.version,
                this.readingResponse,
                this.model,
                this.urlResolver,
                payloadKindDetectionFormatState)

                .FollowOnSuccessWithTask(
                (createInputContextTask) =>
                {
                    this.inputContext = createInputContextTask.Result;
                    return readFunc(this.inputContext);
                });
        }
#endif
    }
}

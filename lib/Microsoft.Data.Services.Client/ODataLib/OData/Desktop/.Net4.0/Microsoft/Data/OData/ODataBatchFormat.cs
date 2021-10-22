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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    #endregion Namespaces

    /// <summary>
    /// The $batch OData format.
    /// </summary>
    internal sealed class ODataBatchFormat : ODataFormat
    {
        /// <summary>
        /// The text representation - the name of the format.
        /// </summary>
        /// <returns>The name of the format.</returns>
        public override string ToString()
        {
            return "Batch";
        }

        /// <summary>
        /// Detects the payload kinds supported by this format for the specified message payload.
        /// </summary>
        /// <param name="responseMessage">The response message with the payload stream.</param>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>The set of <see cref="ODataPayloadKind"/>s that are supported with the specified payload.</returns>
        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(
            IODataResponseMessage responseMessage,
            ODataPayloadKindDetectionInfo detectionInfo)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull(detectionInfo, "detectionInfo");

            return DetectPayloadKindImplementation(detectionInfo.ContentType);
        }

        /// <summary>
        /// Detects the payload kinds supported by this format for the specified message payload.
        /// </summary>
        /// <param name="requestMessage">The request message with the payload stream.</param>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>The set of <see cref="ODataPayloadKind"/>s that are supported with the specified payload.</returns>
        internal override IEnumerable<ODataPayloadKind> DetectPayloadKind(
            IODataRequestMessage requestMessage,
            ODataPayloadKindDetectionInfo detectionInfo)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull(detectionInfo, "detectionInfo");

            return DetectPayloadKindImplementation(detectionInfo.ContentType);
        }

        /// <summary>
        /// Creates an instance of the input context for this format.
        /// </summary>
        /// <param name="readerPayloadKind">The <see cref="ODataPayloadKind"/> to read.</param>
        /// <param name="message">The message to use.</param>
        /// <param name="contentType">The content type of the message to read.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="messageReaderSettings">Configuration settings of the OData reader.</param>
        /// <param name="version">The OData protocol version to be used for reading the payload.</param>
        /// <param name="readingResponse">true if reading a response message; otherwise false.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs read from the payload.</param>
        /// <param name="payloadKindDetectionFormatState">Format specific state stored during payload kind detection
        /// using the <see cref="ODataPayloadKindDetectionInfo.SetPayloadKindDetectionFormatState"/>.</param>
        /// <returns>The newly created input context.</returns>
        internal override ODataInputContext CreateInputContext(
            ODataPayloadKind readerPayloadKind,
            ODataMessage message,
            MediaType contentType,
            Encoding encoding,
            ODataMessageReaderSettings messageReaderSettings,
            ODataVersion version,
            bool readingResponse,
            IEdmModel model,
            IODataUrlResolver urlResolver,
            object payloadKindDetectionFormatState)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(message, "message");
            ExceptionUtils.CheckArgumentNotNull(messageReaderSettings, "messageReaderSettings");

            Stream messageStream = message.GetStream();
            return new ODataRawInputContext(
                this,
                messageStream,
                encoding,
                messageReaderSettings,
                version,
                readingResponse,
                /*synchronous*/ true,
                model,
                urlResolver,
                readerPayloadKind);
        }

        /// <summary>
        /// Creates an instance of the output context for this format.
        /// </summary>
        /// <param name="message">The message to use.</param>
        /// <param name="mediaType">The specific media type being written.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="messageWriterSettings">Configuration settings of the OData writer.</param>
        /// <param name="writingResponse">true if writing a response message; otherwise false.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs written to the payload.</param>
        /// <returns>The newly created output context.</returns>
        internal override ODataOutputContext CreateOutputContext(
            ODataMessage message, 
            MediaType mediaType, 
            Encoding encoding, 
            ODataMessageWriterSettings messageWriterSettings, 
            bool writingResponse,
            IEdmModel model, 
            IODataUrlResolver urlResolver)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(message, "message");
            ExceptionUtils.CheckArgumentNotNull(messageWriterSettings, "messageWriterSettings");

            Stream messageStream = message.GetStream();
            return new ODataRawOutputContext(
                this,
                messageStream,
                encoding,
                messageWriterSettings,
                writingResponse,
                /*synchronous*/ true,
                model,
                urlResolver);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously detects the payload kinds supported by this format for the specified message payload.
        /// </summary>
        /// <param name="responseMessage">The response message with the payload stream.</param>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>A task that when completed returns the set of <see cref="ODataPayloadKind"/>s 
        /// that are supported with the specified payload.</returns>
        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(
            IODataResponseMessageAsync responseMessage, 
            ODataPayloadKindDetectionInfo detectionInfo)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(responseMessage, "responseMessage");
            ExceptionUtils.CheckArgumentNotNull(detectionInfo, "detectionInfo");

            return TaskUtils.GetTaskForSynchronousOperation(() => DetectPayloadKindImplementation(detectionInfo.ContentType));
        }

        /// <summary>
        /// Asynchronously detects the payload kinds supported by this format for the specified message payload.
        /// </summary>
        /// <param name="requestMessage">The request message with the payload stream.</param>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>A task that when completed returns the set of <see cref="ODataPayloadKind"/>s 
        /// that are supported with the specified payload.</returns>
        internal override Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(
            IODataRequestMessageAsync requestMessage,
            ODataPayloadKindDetectionInfo detectionInfo)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(requestMessage, "requestMessage");
            ExceptionUtils.CheckArgumentNotNull(detectionInfo, "detectionInfo");

            return TaskUtils.GetTaskForSynchronousOperation(() => DetectPayloadKindImplementation(detectionInfo.ContentType));
        }

        /// <summary>
        /// Asynchronously creates an instance of the input context for this format.
        /// </summary>
        /// <param name="readerPayloadKind">The <see cref="ODataPayloadKind"/> to read.</param>
        /// <param name="message">The message to use.</param>
        /// <param name="contentType">The content type of the message to read.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="messageReaderSettings">Configuration settings of the OData reader.</param>
        /// <param name="version">The OData protocol version to be used for reading the payload.</param>
        /// <param name="readingResponse">true if reading a response message; otherwise false.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs read from the payload.</param>
        /// <param name="payloadKindDetectionFormatState">Format specific state stored during payload kind detection
        /// using the <see cref="ODataPayloadKindDetectionInfo.SetPayloadKindDetectionFormatState"/>.</param>
        /// <returns>Task which when completed returned the newly created input context.</returns>
        internal override Task<ODataInputContext> CreateInputContextAsync(
            ODataPayloadKind readerPayloadKind,
            ODataMessage message,
            MediaType contentType,
            Encoding encoding,
            ODataMessageReaderSettings messageReaderSettings,
            ODataVersion version,
            bool readingResponse,
            IEdmModel model,
            IODataUrlResolver urlResolver,
            object payloadKindDetectionFormatState)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(message, "message");
            ExceptionUtils.CheckArgumentNotNull(messageReaderSettings, "messageReaderSettings");

            return message.GetStreamAsync()
                .FollowOnSuccessWith(
                    (streamTask) => (ODataInputContext)new ODataRawInputContext(
                        this,
                        streamTask.Result,
                        encoding,
                        messageReaderSettings,
                        version,
                        readingResponse,
                        /*synchronous*/ false,
                        model,
                        urlResolver,
                        readerPayloadKind));
        }

        /// <summary>
        /// Creates an instance of the output context for this format.
        /// </summary>
        /// <param name="message">The message to use.</param>
        /// <param name="mediaType">The specific media type being written.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="messageWriterSettings">Configuration settings of the OData writer.</param>
        /// <param name="writingResponse">true if writing a response message; otherwise false.</param>
        /// <param name="model">The model to use.</param>
        /// <param name="urlResolver">The optional URL resolver to perform custom URL resolution for URLs written to the payload.</param>
        /// <returns>Task which represents the pending create operation.</returns>
        internal override Task<ODataOutputContext> CreateOutputContextAsync(
            ODataMessage message,
            MediaType mediaType, 
            Encoding encoding, 
            ODataMessageWriterSettings messageWriterSettings, 
            bool writingResponse,
            IEdmModel model, 
            IODataUrlResolver urlResolver)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(message, "message");
            ExceptionUtils.CheckArgumentNotNull(messageWriterSettings, "messageWriterSettings");

            return message.GetStreamAsync()
                .FollowOnSuccessWith(
                    (streamTask) => (ODataOutputContext)new ODataRawOutputContext(
                        this,
                        streamTask.Result,
                        encoding,
                        messageWriterSettings,
                        writingResponse,
                        /*synchronous*/ false,
                        model,
                        urlResolver));
        }
#endif

        /// <summary>
        /// Detects the payload kind(s) from the message stream.
        /// </summary>
        /// <param name="contentType">The content type of the message.</param>
        /// <returns>An enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        private static IEnumerable<ODataPayloadKind> DetectPayloadKindImplementation(MediaType contentType)
        {
            // NOTE: for batch payloads we only use the content type header of the message to detect the payload kind.
            //       We assume a valid batch payload if the content type is multipart/mixed and a boundary parameter exists
            // Require 'multipart/mixed' content type with a boundary parameter to be considered batch.
            if (HttpUtils.CompareMediaTypeNames(MimeConstants.MimeMultipartType, contentType.TypeName) &&
                HttpUtils.CompareMediaTypeNames(MimeConstants.MimeMixedSubType, contentType.SubTypeName) &&
                contentType.Parameters != null &&
                contentType.Parameters.Any(kvp => HttpUtils.CompareMediaTypeParameterNames(ODataConstants.HttpMultipartBoundary, kvp.Key)))
            {
                return new ODataPayloadKind[] { ODataPayloadKind.Batch };
            }

            return Enumerable.Empty<ODataPayloadKind>();
        }
    }
}

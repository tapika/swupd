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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    #endregion Namespaces

    /// <summary>
    /// The JsonLight OData format.
    /// </summary>
    internal sealed class ODataJsonLightFormat : ODataFormat
    {
        /// <summary>
        /// The text representation - the name of the format.
        /// </summary>
        /// <returns>The name of the format.</returns>
        public override string ToString()
        {
            return "JsonLight";
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

            ODataMessage message = (ODataMessage)responseMessage;
            Stream messageStream = message.GetStream();
            return this.DetectPayloadKindImplementation(messageStream, message, /*readingResponse*/ true, detectionInfo);
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

            ODataMessage message = (ODataMessage)requestMessage;
            Stream messageStream = message.GetStream();
            return this.DetectPayloadKindImplementation(messageStream, message, /*readingResponse*/ false, detectionInfo);
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
            return new ODataJsonLightInputContext(
                this,
                messageStream,
                contentType,
                encoding,
                messageReaderSettings,
                version,
                readingResponse,
                /*synchronous*/ true,
                model,
                urlResolver,
                (ODataJsonLightPayloadKindDetectionState)payloadKindDetectionFormatState);
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
            return new ODataJsonLightOutputContext(
                this,
                messageStream,
                mediaType,
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

            // NOTE: After getting the message stream we already (asynchronously) buffered the whole stream in memory (in the BufferedReadStream).
            //       Until we get Task-based async stream APIs and retire the BufferedReadStream, we call the synchronous method on the buffered stream.
            ODataMessage message = (ODataMessage)responseMessage;
            return message.GetStreamAsync()
                .FollowOnSuccessWithTask(streamTask => this.DetectPayloadKindImplementationAsync(
                    streamTask.Result,
                    message,
                    /*readingResponse*/ true,
                    detectionInfo));
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

            // NOTE: After getting the message stream we already (asynchronously) buffered the whole stream in memory (in the BufferedReadStream).
            //       Until we get Task-based async stream APIs and retire the BufferedReadStream, we call the synchronous method on the buffered stream.
            ODataMessage message = (ODataMessage)requestMessage;
            return message.GetStreamAsync()
                .FollowOnSuccessWithTask(streamTask => this.DetectPayloadKindImplementationAsync(
                    streamTask.Result,
                    message,
                    /*readingResponse*/ false,
                    detectionInfo));
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
                    (streamTask) => (ODataInputContext)new ODataJsonLightInputContext(
                        this,
                        streamTask.Result,
                        contentType,
                        encoding,
                        messageReaderSettings,
                        version,
                        readingResponse,
                        /*synchronous*/ false,
                        model,
                        urlResolver,
                        (ODataJsonLightPayloadKindDetectionState)payloadKindDetectionFormatState));
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
                    (streamTask) => (ODataOutputContext)new ODataJsonLightOutputContext(
                        this,
                        streamTask.Result, 
                        mediaType,
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
        /// <param name="messageStream">The message stream to read from for payload kind detection.</param>
        /// <param name="message">The message being read.</param>
        /// <param name="readingResponse">true if reading a response message; otherwise false.</param>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>An enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        private IEnumerable<ODataPayloadKind> DetectPayloadKindImplementation(
            Stream messageStream,
            ODataMessage message,
            bool readingResponse,
            ODataPayloadKindDetectionInfo detectionInfo)
        {
            using (ODataJsonLightInputContext jsonLightInputContext = new ODataJsonLightInputContext(
                this,
                messageStream,
                detectionInfo.ContentType,
                detectionInfo.GetEncoding(),
                detectionInfo.MessageReaderSettings,
                ODataVersion.V3,    // NOTE: we don't rely on the version for payload kind detection; taking the latest.
                readingResponse,
                /*synchronous*/ true,
                detectionInfo.Model,
                /*urlResolver*/ null,
                /*payloadKindDetectionState*/ null))
            {
                return jsonLightInputContext.DetectPayloadKind(detectionInfo);
            }
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Detects the payload kind(s) from the message stream.
        /// </summary>
        /// <param name="messageStream">The message stream to read from for payload kind detection.</param>
        /// <param name="message">The message being read.</param>
        /// <param name="readingResponse">true if reading a response message; otherwise false.</param>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>An enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        private Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindImplementationAsync(
            Stream messageStream,
            ODataMessage message,
            bool readingResponse,
            ODataPayloadKindDetectionInfo detectionInfo)
        {
            ODataJsonLightInputContext jsonLightInputContext = new ODataJsonLightInputContext(
                this,
                messageStream,
                detectionInfo.ContentType,
                detectionInfo.GetEncoding(),
                detectionInfo.MessageReaderSettings,
                ODataVersion.V3,    // NOTE: we don't rely on the version for payload kind detection; taking the latest.
                readingResponse,
                /*synchronous*/ false,
                detectionInfo.Model,
                /*urlResolver*/ null,
                /*payloadKindDetectionState*/ null);

            return jsonLightInputContext.DetectPayloadKindAsync(detectionInfo)
                .FollowAlwaysWith(t =>
                    {
                        jsonLightInputContext.Dispose();
                    });
        }
#endif
    }
}

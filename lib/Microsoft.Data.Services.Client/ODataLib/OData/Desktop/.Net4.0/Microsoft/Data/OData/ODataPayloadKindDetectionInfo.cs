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
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Microsoft.Data.Edm;
    #endregion Namespaces

    /// <summary>
    /// Represents the set of information available for payload kind detection.
    /// </summary>
    /// <remarks>This class is used to represent the input to run payload kind detection using
    /// <see cref="ODataMessageReader.DetectPayloadKind"/>. See the documentation of that method for more 
    /// information.</remarks>
    internal sealed class ODataPayloadKindDetectionInfo
    {
        /// <summary>The parsed content type as <see cref="MediaType"/>.</summary>
        private readonly MediaType contentType;

        /// <summary>The encoding specified in the charset parameter of contentType or the default encoding from MediaType.</summary>
        private readonly Encoding encoding;

        /// <summary>The <see cref="ODataMessageReaderSettings"/> being used for reading the message.</summary>
        private readonly ODataMessageReaderSettings messageReaderSettings;

        /// <summary>The <see cref="IEdmModel"/> for the payload.</summary>
        private readonly IEdmModel model;

        /// <summary>The possible payload kinds based on content type negotiation.</summary>
        private readonly IEnumerable<ODataPayloadKind> possiblePayloadKinds;

        /// <summary>Format specific state created during payload kind detection for that format.</summary>
        /// <remarks>
        /// This instance will be stored on the message reader and passed to the format if it will be used
        /// for actually reading the payload.
        /// Format can store information which was already extracted from the payload during payload kind detection
        /// and which it wants to avoid to recompute again during actual reading.
        /// </remarks>
        private object payloadKindDetectionFormatState;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contentType">The parsed content type as <see cref="MediaType"/>.</param>
        /// <param name="encoding">The encoding from the content type or the default encoding from <see cref="MediaType" />.</param>
        /// <param name="messageReaderSettings">The <see cref="ODataMessageReaderSettings"/> being used for reading the message.</param>
        /// <param name="model">The <see cref="IEdmModel"/> for the payload.</param>
        /// <param name="possiblePayloadKinds">The possible payload kinds based on content type negotiation.</param>
        internal ODataPayloadKindDetectionInfo(
            MediaType contentType,
            Encoding encoding,
            ODataMessageReaderSettings messageReaderSettings, 
            IEdmModel model, 
            IEnumerable<ODataPayloadKind> possiblePayloadKinds)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(contentType, "contentType");
            ExceptionUtils.CheckArgumentNotNull(messageReaderSettings, "readerSettings");
            ExceptionUtils.CheckArgumentNotNull(possiblePayloadKinds, "possiblePayloadKinds");

            this.contentType = contentType;
            this.encoding = encoding;
            this.messageReaderSettings = messageReaderSettings;
            this.model = model;
            this.possiblePayloadKinds = possiblePayloadKinds;
        }

        /// <summary>
        /// The <see cref="ODataMessageReaderSettings"/> being used for reading the message.
        /// </summary>
        public ODataMessageReaderSettings MessageReaderSettings
        {
            get { return this.messageReaderSettings; }
        }

        /// <summary>
        /// The <see cref="IEdmModel"/> for the payload.
        /// </summary>
        public IEdmModel Model
        {
            get { return this.model; }
        }

        /// <summary>
        /// The possible payload kinds based on content type negotiation.
        /// </summary>
        public IEnumerable<ODataPayloadKind> PossiblePayloadKinds
        {
            get { return this.possiblePayloadKinds; }
        }

        /// <summary>
        /// The <see cref="ODataMessageReaderSettings"/> being used for reading the message.
        /// </summary>
        internal MediaType ContentType
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.contentType;
            }
        }

        /// <summary>
        /// The format specific payload kind detection state.
        /// </summary>
        internal object PayloadKindDetectionFormatState
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.payloadKindDetectionFormatState;
            }
        }

        /// <summary>
        /// The encoding derived from the content type or the default encoding.
        /// </summary>
        /// <returns>The encoding derived from the content type or the default encoding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "There is computation needed to get the encoding from the content type; thus a method.")]
        public Encoding GetEncoding()
        {
            return this.encoding ?? this.contentType.SelectEncoding();
        }

        /// <summary>
        /// Sets a format specific state created during payload kind detection.
        /// </summary>
        /// <param name="state">A format specific state, the value is opaque to the message reader, it only stores the reference.</param>
        /// <remarks>
        /// The state will be stored on the message reader and passed to the format if it will be used
        /// for actually reading the payload.
        /// Format can store information which was already extracted from the payload during payload kind detection
        /// and which it wants to avoid to recompute again during actual reading.
        /// </remarks>
        public void SetPayloadKindDetectionFormatState(object state)
        {
            this.payloadKindDetectionFormatState = state;
        }
    }
}

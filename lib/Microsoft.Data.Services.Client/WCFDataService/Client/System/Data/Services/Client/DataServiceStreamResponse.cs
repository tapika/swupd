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

namespace System.Data.Services.Client
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.Data.OData;

    /// <summary>Represents a response from WCF Data Services that contains binary data as a stream.</summary>
    public sealed class DataServiceStreamResponse : IDisposable
    {
        /// <summary>IODataResponseMessage containing all the response information.</summary>
        private IODataResponseMessage responseMessage;

        /// <summary>Lazy initialized cached response headers.</summary>
        private Dictionary<string, string> headers;

        /// <summary>
        /// Response stream. Caching the response stream so that IODataResponseStream.GetStream is only called once.
        /// This helps us to assert that no one internally calls the GetStream method more than once.
        /// </summary>
        private Stream responseStream;

        /// <summary>
        /// Constructor for the response. This method is internal since we don't want users to create instances
        /// of this class.
        /// </summary>
        /// <param name="response">The web response to wrap.</param>
        internal DataServiceStreamResponse(IODataResponseMessage response)
        {
            Debug.Assert(response != null, "Can't create a stream response object from a null response.");
            this.responseMessage = response;
        }

        /// <summary>Gets the content type of the response stream.</summary>
        /// <returns>The content type of the response stream.</returns>
        /// <remarks>If the Content-Type header was not present in the response this property will return null.</remarks>
        public string ContentType
        {
            get
            {
                this.CheckDisposed();
                return this.responseMessage.GetHeader(XmlConstants.HttpContentType);
            }
        }

        /// <summary>Gets the Content-Disposition header field for the response stream.</summary>
        /// <returns>The contents of the Content-Disposition header field.</returns>
        /// /// <remarks>If the Content-Disposition header was not present in the response this property will return null.</remarks>
        public string ContentDisposition
        {
            get
            {
                this.CheckDisposed();
                return this.responseMessage.GetHeader(XmlConstants.HttpContentDisposition);
            }
        }

        /// <summary>Gets the collection of headers from the response.</summary>
        /// <returns>The headers collection from the response message as a <see cref="T:System.Collections.Generic.Dictionary`2" /> object.</returns>
        public Dictionary<string, string> Headers
        {
            get
            {
                this.CheckDisposed();
                if (this.headers == null)
                {
                    // by mistake in V2 we made this public API not expose the interface, but we don't
                    // want the rest of the codebase to use this type, so we only cast it when absolutely
                    // required by the public API.
                    this.headers = (Dictionary<string, string>)new HeaderCollection(this.responseMessage).UnderlyingDictionary;
                }

                return this.headers;
            }
        }

        /// <summary>Gets the binary property data from the data service as a binary stream. </summary>
        /// <returns>The stream that contains the binary property data.</returns>
        /// <exception cref="T:System.ObjectDisposedException">When the <see cref="T:System.Data.Services.Client.DataServiceStreamResponse" /> is already disposed.</exception>
        /// <remarks>
        /// Returns the stream obtained from the data service. When reading from this stream
        /// the operations may throw if a network error occurs. This stream is read-only.
        /// 
        /// Caller must call Dispose/Close on either the returned stream or on the response
        /// object itself. Otherwise the network connection will be left open and the caller
        /// might run out of available connections.
        /// </remarks>
        public Stream Stream
        {
            get
            {
                this.CheckDisposed();
                if (this.responseStream == null)
                {
                    this.responseStream = this.responseMessage.GetStream();
                }

                return this.responseStream;
            }
        }

        #region IDisposable Members

        /// <summary>Releases all resources used by the current instance of the <see cref="T:System.Data.Services.Client.DataServiceStreamResponse" /> class.</summary>
        public void Dispose()
        {
            WebUtil.DisposeMessage(this.responseMessage);
        }

        #endregion

        /// <summary>Checks if the object has already been disposed. If so it throws the ObjectDisposedException.</summary>
        /// <exception cref="ObjectDisposedException">If the object has already been disposed.</exception>
        private void CheckDisposed()
        {
            if (this.responseMessage == null)
            {
                Error.ThrowObjectDisposed(this.GetType());
            }
        }
    }
}

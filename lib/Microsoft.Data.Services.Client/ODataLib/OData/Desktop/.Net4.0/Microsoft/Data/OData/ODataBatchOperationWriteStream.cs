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
    using System.IO;

#if WINRT
    using System.Threading;
    using System.Threading.Tasks;
#endif
    #endregion Namespaces

    /// <summary>
    /// A stream handed to clients from ODataBatchOperationMessage.GetStream or ODataBatchOperationMessage.GetStreamAsync. 
    /// This stream communicates status changes to the owning batch writer (via IODataBatchOperationListener) 
    /// to properly flush buffered data and move the batch writer's state machine forward.
    /// </summary>
    internal sealed class ODataBatchOperationWriteStream : ODataBatchOperationStream
    {
        /// <summary>The batch stream underlying this operation stream.</summary>
        private Stream batchStream;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="batchStream">The underlying stream to write the message to.</param>
        /// <param name="listener">Listener interface to be notified of operation changes.</param>
        internal ODataBatchOperationWriteStream(Stream batchStream, IODataBatchOperationListener listener)
            : base(listener)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(batchStream != null, "batchStream != null");
            this.batchStream = batchStream;
        }

        /// <summary>
        /// Determines if the stream can read - this one can't
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the stream can seek - this one can't
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the stream can write - this one can
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the length of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                this.ValidateNotDisposed();
                return this.batchStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the position in the stream. Setting of the position is not supported since the stream doesn't support seeking.
        /// </summary>
        public override long Position
        {
            get
            {
                this.ValidateNotDisposed();
                return this.batchStream.Position;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The length in bytes to set.</param>
        public override void SetLength(long value)
        {
            this.ValidateNotDisposed();
            this.batchStream.SetLength(value);
        }

        /// <summary>
        /// Writes to the stream.
        /// </summary>
        /// <param name="buffer">The buffer to get data from.</param>
        /// <param name="offset">The offset in the buffer to start from.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.ValidateNotDisposed();
            this.batchStream.Write(buffer, offset, count);
        }

#if WINRT
        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.ValidateNotDisposed();
            return this.batchStream.WriteAsync(buffer, offset, count, cancellationToken);
        }
#else
        /// <summary>
        /// Writes to the stream.
        /// </summary>
        /// <param name="buffer">The buffer to get data from.</param>
        /// <param name="offset">The offset in the buffer to start from.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="callback">The callback to be called when the asynchronous operation completes.</param>
        /// <param name="state">A custom state object to be associated with the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncResult"/> for the asynchronous writing of the buffer to the stream.</returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.ValidateNotDisposed();
            return this.batchStream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Finish the asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> returned from BaginWrite.</param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.ValidateNotDisposed();
            this.batchStream.EndWrite(asyncResult);
        }
#endif

        /// <summary>
        /// Reads data from the stream. This operation is not supported by this stream.
        /// </summary>
        /// <param name="buffer">The buffer to read the data to.</param>
        /// <param name="offset">The offset in the buffer to write to.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes actually read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Flush the stream to the underlying batch stream.
        /// </summary>
        public override void Flush()
        {
            this.ValidateNotDisposed();
            this.batchStream.Flush();
        }

        /// <summary>
        /// Dispose the operation stream.
        /// </summary>
        /// <param name="disposing">If 'true' this method is called from user code; if 'false' it is called by the runtime.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // NOTE: don't dispose the batch stream since this instance does not own it.
                this.batchStream = null;
            }

            base.Dispose(disposing);
        }
    }
}

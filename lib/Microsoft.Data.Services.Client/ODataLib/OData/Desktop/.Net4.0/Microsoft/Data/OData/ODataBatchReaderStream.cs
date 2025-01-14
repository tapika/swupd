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
    using System.Globalization;
    using System.IO;
    using System.Text;
    #endregion Namespaces

    /// <summary>
    /// Class used by the <see cref="ODataBatchReader"/> to read the various pieces of a batch payload.
    /// </summary>
    /// <remarks>
    /// This stream separates a batch payload into multiple parts by scanning ahead and matching
    /// a boundary string against the current payload.
    /// </remarks>
    internal sealed class ODataBatchReaderStream
    {
        /// <summary>
        /// The default length for the line buffer byte array used to read lines; expecting lines to normally be less than 2000 bytes.
        /// </summary>
        private const int LineBufferLength = 2000;

        /// <summary>
        /// The byte array used for reading lines from the stream. We cache the byte array on the stream instance
        /// rather than allocating a new one for each ReadLine call.
        /// </summary>
        private readonly byte[] lineBuffer;

        /// <summary>The input context to read the content from.</summary>
        private readonly ODataRawInputContext inputContext;

        /// <summary>The boundary string for the batch structure itself.</summary>
        private readonly string batchBoundary;

        /// <summary>The buffer used by the batch reader stream to scan for boundary strings.</summary>
        private readonly ODataBatchReaderStreamBuffer batchBuffer;

        /// <summary>The encoding to use to read from the batch stream.</summary>
        private Encoding batchEncoding;

        /// <summary>The boundary string for a changeset (or null if not in a changeset part).</summary>
        private string changesetBoundary;

        /// <summary>The encoding for a given changeset.</summary>
        private Encoding changesetEncoding;

        /// <summary>
        /// true if the underlying stream was exhausted during a read operation; we won't try to read from the 
        /// underlying stream again once it was exhausted.
        /// </summary>
        private bool underlyingStreamExhausted;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputContext">The input context to read the content from.</param>
        /// <param name="batchBoundary">The boundary string for the batch structure itself.</param>
        /// <param name="batchEncoding">The encoding to use to read from the batch stream.</param>
        internal ODataBatchReaderStream(
            ODataRawInputContext inputContext, 
            string batchBoundary, 
            Encoding batchEncoding)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(inputContext != null, "inputContext != null");
            Debug.Assert(!string.IsNullOrEmpty(batchBoundary), "!string.IsNullOrEmpty(batchBoundary)");

            this.inputContext = inputContext;
            this.batchBoundary = batchBoundary;
            this.batchEncoding = batchEncoding;

            this.batchBuffer = new ODataBatchReaderStreamBuffer();

            // When we allocate a batch reader stream we will in almost all cases also call ReadLine
            // (to read the headers of the parts); so allocating it here.
            this.lineBuffer = new byte[LineBufferLength];
        }

        /// <summary>
        /// The boundary string for the batch structure itself.
        /// </summary>
        internal string BatchBoundary
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.batchBoundary;
            }
        }

        /// <summary>
        /// The boundary string for the current changeset (only set when reading a changeset
        /// or an operation in a changeset).
        /// </summary>
        /// <remarks>When not reading a changeset (or operation in a changeset) this field is null.</remarks>
        internal string ChangeSetBoundary
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.changesetBoundary;
            }
        }

        /// <summary>
        /// The current boundary string to be used for reading with delimiter.
        /// </summary>
        /// <remarks>This is the changeset boundary when reading a changeset or the batch boundary otherwise.</remarks>
        private IEnumerable<string> CurrentBoundaries
        {
            get
            {
                if (this.changesetBoundary != null)
                {
                    yield return this.changesetBoundary;
                }

                yield return this.batchBoundary;
            }
        }

        /// <summary>
        /// The current encoding to use when reading from the stream.
        /// </summary>
        /// <remarks>This is the changeset encoding when reading a changeset or the batch encoding otherwise.</remarks>
        private Encoding CurrentEncoding
        {
            get
            {
                return this.changesetEncoding ?? this.batchEncoding;
            }
        }

        /// <summary>
        /// Resets the changeset boundary at the end of the changeset.
        /// </summary>
        internal void ResetChangeSetBoundary()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.changesetBoundary != null, "Only valid to reset a changeset boundary if one exists.");
            this.changesetBoundary = null;
            this.changesetEncoding = null;
        }

        /// <summary>
        /// Skips all the data in the stream until a boundary is found.
        /// </summary>
        /// <param name="isEndBoundary">true if the boundary that was found is an end boundary; otherwise false.</param>
        /// <param name="isParentBoundary">true if the detected boundary is a parent boundary (i.e., the expected boundary is missing).</param>
        /// <returns>true if a boundary was found; otherwise false.</returns>
        internal bool SkipToBoundary(out bool isEndBoundary, out bool isParentBoundary)
        {
            DebugUtils.CheckNoExternalCallers();

            // Ensure we have a batch encoding; if not detect it on the first read/skip.
            this.EnsureBatchEncoding();

            ODataBatchReaderStreamScanResult scanResult = ODataBatchReaderStreamScanResult.NoMatch;
            while (scanResult != ODataBatchReaderStreamScanResult.Match)
            {
                int boundaryStartPosition, boundaryEndPosition;
                scanResult = this.batchBuffer.ScanForBoundary(
                    this.CurrentBoundaries,
                    /*stopAfterIfNotFound*/int.MaxValue, 
                    out boundaryStartPosition, 
                    out boundaryEndPosition,
                    out isEndBoundary,
                    out isParentBoundary);
                switch (scanResult)
                {
                    case ODataBatchReaderStreamScanResult.NoMatch:
                        if (this.underlyingStreamExhausted)
                        {
                            // there is nothing else to load from the underlying stream; the requested boundary does not exist
                            this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + this.batchBuffer.NumberOfBytesInBuffer);
                            return false;
                        }

                        // skip everything in the buffer and refill it from the underlying stream; continue scanning
                        this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, /*preserveFrom*/ODataBatchReaderStreamBuffer.BufferLength);

                        break;
                    case ODataBatchReaderStreamScanResult.PartialMatch:
                        if (this.underlyingStreamExhausted)
                        {
                            // there is nothing else to load from the underlying stream; the requested boundary does not exist
                            this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + this.batchBuffer.NumberOfBytesInBuffer);
                            return false;
                        }

                        this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, /*preserveFrom*/boundaryStartPosition);

                        break;
                    case ODataBatchReaderStreamScanResult.Match:
                        // If we found the expected boundary, position the reader on the position after the boundary end.
                        // If we found a parent boundary, position the reader on the boundary start so we'll detect the boundary
                        // again on the next Read call.
                        this.batchBuffer.SkipTo(isParentBoundary ? boundaryStartPosition : boundaryEndPosition + 1);
                        return true;

                    default:
                        throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_SkipToBoundary));
                }
            }

            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_SkipToBoundary));
        }

        /// <summary>
        /// Reads from the batch stream while ensuring that we stop reading at each boundary.
        /// </summary>
        /// <param name="userBuffer">The byte array to read bytes into.</param>
        /// <param name="userBufferOffset">The offset in the buffer where to start reading bytes into.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes actually read.</returns>
        internal int ReadWithDelimiter(byte[] userBuffer, int userBufferOffset, int count)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(userBuffer != null, "userBuffer != null");
            Debug.Assert(userBufferOffset >= 0 && userBufferOffset < userBuffer.Length, "Offset must be within the range of the user buffer.");
            Debug.Assert(count >= 0, "count >= 0");
            Debug.Assert(this.batchEncoding != null, "Batch encoding should have been established on first call to SkipToBoundary.");

            if (count == 0)
            {
                // Nothing to read.
                return 0;
            }

            int remainingNumberOfBytesToRead = count;
            ODataBatchReaderStreamScanResult scanResult = ODataBatchReaderStreamScanResult.NoMatch;

            while (remainingNumberOfBytesToRead > 0 && scanResult != ODataBatchReaderStreamScanResult.Match)
            {
                int boundaryStartPosition, boundaryEndPosition;
                bool isEndBoundary, isParentBoundary;
                scanResult = this.batchBuffer.ScanForBoundary(
                    this.CurrentBoundaries,
                    remainingNumberOfBytesToRead, 
                    out boundaryStartPosition, 
                    out boundaryEndPosition,
                    out isEndBoundary,
                    out isParentBoundary);

                int bytesBeforeBoundaryStart;
                switch (scanResult)
                {
                    case ODataBatchReaderStreamScanResult.NoMatch:
                        // The boundary was not found in the buffer or after the required number of bytes to be read; 
                        // Check whether we can satisfy the full read request from the buffer
                        // or whether we have to split the request and read more data into the buffer.
                        if (this.batchBuffer.NumberOfBytesInBuffer >= remainingNumberOfBytesToRead)
                        {
                            // we can satisfy the full read request from the buffer
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, remainingNumberOfBytesToRead);
                            this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + remainingNumberOfBytesToRead);
                            return count;
                        }
                        else
                        {
                            // we can only partially satisfy the read request
                            int availableBytesToRead = this.batchBuffer.NumberOfBytesInBuffer;
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, availableBytesToRead);
                            remainingNumberOfBytesToRead -= availableBytesToRead;
                            userBufferOffset += availableBytesToRead;

                            // we exhausted the buffer; if the underlying stream is not exceeded, refill the buffer
                            if (this.underlyingStreamExhausted)
                            {
                                // We cannot fully satisfy the read request since there are not enough bytes in the stream.
                                // Return the number of bytes we read.
                                this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + availableBytesToRead);
                                return count - remainingNumberOfBytesToRead;
                            }
                            else
                            {
                                this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, /*preserveFrom*/ ODataBatchReaderStreamBuffer.BufferLength);
                            }
                        }

                        break;
                    case ODataBatchReaderStreamScanResult.PartialMatch:
                        // A partial match for the boundary was found at the end of the buffer.
                        // If the underlying stream is not exceeded, refill the buffer. Otherwise return
                        // the available bytes.
                        if (this.underlyingStreamExhausted)
                        {
                            // We cannot fully satisfy the read request since there are not enough bytes in the stream.
                            // Return the remaining bytes in the buffer independently of where a portentially boundary
                            // start was detected since no full boundary can ever be detected if the stream is exhausted.
                            int bytesToReturn = Math.Min(this.batchBuffer.NumberOfBytesInBuffer, remainingNumberOfBytesToRead);
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, bytesToReturn);
                            this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + bytesToReturn);
                            remainingNumberOfBytesToRead -= bytesToReturn;
                            return count - remainingNumberOfBytesToRead;
                        }
                        else
                        {
                            // Copy the bytes prior to the potential boundary start into the user buffer, refill the buffer and continue.
                            bytesBeforeBoundaryStart = boundaryStartPosition - this.batchBuffer.CurrentReadPosition;
                            Debug.Assert(bytesBeforeBoundaryStart < remainingNumberOfBytesToRead, "When reporting a partial match we should never have read all the remaining bytes to read (or more).");

                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, bytesBeforeBoundaryStart);
                            remainingNumberOfBytesToRead -= bytesBeforeBoundaryStart;
                            userBufferOffset += bytesBeforeBoundaryStart;

                            this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, /*preserveFrom*/ boundaryStartPosition);
                        }

                        break;
                    case ODataBatchReaderStreamScanResult.Match:
                        // We found the full boundary match; copy everything before the boundary to the buffer
                        bytesBeforeBoundaryStart = boundaryStartPosition - this.batchBuffer.CurrentReadPosition;
                        Debug.Assert(bytesBeforeBoundaryStart <= remainingNumberOfBytesToRead, "When reporting a full match we should never have read more than the remaining bytes to read.");
                        Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, bytesBeforeBoundaryStart);
                        remainingNumberOfBytesToRead -= bytesBeforeBoundaryStart;
                        userBufferOffset += bytesBeforeBoundaryStart;

                        // position the reader on the position of the boundary start
                        this.batchBuffer.SkipTo(boundaryStartPosition);

                        // return the number of bytes that were read
                        return count - remainingNumberOfBytesToRead;
                        
                    default:
                        break;
                }
            }

            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_ReadWithDelimiter));
        }

        /// <summary>
        /// Reads from the batch stream without checking for a boundary delimiter since we 
        /// know the length of the stream.
        /// </summary>
        /// <param name="userBuffer">The byte array to read bytes into.</param>
        /// <param name="userBufferOffset">The offset in the buffer where to start reading bytes into.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes actually read.</returns>
        internal int ReadWithLength(byte[] userBuffer, int userBufferOffset, int count)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(userBuffer != null, "userBuffer != null");
            Debug.Assert(userBufferOffset >= 0, "userBufferOffset >= 0");
            Debug.Assert(count >= 0, "count >= 0");
            Debug.Assert(this.batchEncoding != null, "Batch encoding should have been established on first call to SkipToBoundary.");

            //// NOTE: if we have a stream with length we don't even check for boundaries but rely solely on the content length

            int remainingNumberOfBytesToRead = count;
            while (remainingNumberOfBytesToRead > 0)
            {
                // check whether we can satisfy the full read request from the buffer
                // or whether we have to split the request and read more data into the buffer.
                if (this.batchBuffer.NumberOfBytesInBuffer >= remainingNumberOfBytesToRead)
                {
                    // we can satisfy the full read request from the buffer
                    Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, remainingNumberOfBytesToRead);
                    this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + remainingNumberOfBytesToRead);
                    remainingNumberOfBytesToRead = 0;
                }
                else
                {
                    // we can only partially satisfy the read request
                    int availableBytesToRead = this.batchBuffer.NumberOfBytesInBuffer;
                    Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, availableBytesToRead);
                    remainingNumberOfBytesToRead -= availableBytesToRead;
                    userBufferOffset += availableBytesToRead;

                    // we exhausted the buffer; if the underlying stream is not exhausted, refill the buffer
                    if (this.underlyingStreamExhausted)
                    {
                        // We cannot fully satisfy the read request since there are not enough bytes in the stream.
                        // This means that the content length of the stream was incorrect; this should never happen
                        // since the caller should already have checked this.
                        throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStreamBuffer_ReadWithLength));
                    }
                    else
                    {
                        this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, /*preserveFrom*/ ODataBatchReaderStreamBuffer.BufferLength);
                    }
                }
            }

            // return the number of bytes that were read
            return count - remainingNumberOfBytesToRead;
        }

        /// <summary>
        /// Reads the headers of a part.
        /// </summary>
        /// <returns>true if the start of a changeset part was detected; otherwise false.</returns>
        internal bool ProcessPartHeader()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.batchEncoding != null, "Batch encoding should have been established on first call to SkipToBoundary.");

            bool isChangeSetPart;
            ODataBatchOperationHeaders headers = this.ReadPartHeaders(out isChangeSetPart);

            if (isChangeSetPart)
            {
                // determine the changeset boundary and the changeset encoding from the content type header
                this.DetermineChangesetBoundaryAndEncoding(headers[ODataConstants.ContentTypeHeader]);

                if (this.changesetEncoding == null)
                {
                    // NOTE: No changeset encoding was specified in the changeset's content type header.
                    //       Determine the changeset encoding from the first bytes in the changeset.
                    // NOTE: We do not have to skip over the potential preamble of the encoding
                    //       because the batch reader will skip over everything (incl. the preamble)
                    //       until it finds the first changeset (or batch) boundary
                    this.changesetEncoding = this.DetectEncoding();
                }

                // Verify that we only allow single byte encodings and UTF-8 for now.
                ReaderValidationUtils.ValidateEncodingSupportedInBatch(this.changesetEncoding);
            }

            return isChangeSetPart;
        }

        /// <summary>
        /// Reads the headers of a batch part or an operation.
        /// </summary>
        /// <returns>A dictionary of header names to header values; never null.</returns>
        internal ODataBatchOperationHeaders ReadHeaders()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.batchEncoding != null, "Batch encoding should have been established on first call to SkipToBoundary.");

            ODataBatchOperationHeaders headers = new ODataBatchOperationHeaders();

            // Read all the headers
            string headerLine = this.ReadLine();
            while (headerLine != null && headerLine.Length > 0)
            {
                string headerName, headerValue;
                ValidateHeaderLine(headerLine, out headerName, out headerValue);

                if (headers.ContainsKeyOrdinal(headerName))
                {
                    throw new ODataException(Strings.ODataBatchReaderStream_DuplicateHeaderFound(headerName));
                }

                headers.Add(headerName, headerValue);
                headerLine = this.ReadLine();
            }

            return headers;
        }

        /// <summary>
        /// Read and return the next line from the batch stream, skipping all empty lines.
        /// </summary>
        /// <remarks>This method will throw if end-of-input was reached while looking for the next line.</remarks>
        /// <returns>The text of the first non-empty line (not including any terminating newline characters).</returns>
        internal string ReadFirstNonEmptyLine()
        {
            DebugUtils.CheckNoExternalCallers();

            string line;
            do
            {
                line = this.ReadLine();

                // null indicates end of input, which is unexpected at this point.
                if (line == null)
                {
                    throw new ODataException(Strings.ODataBatchReaderStream_UnexpectedEndOfInput);
                }
            }
            while (line.Length == 0);

            return line;
        }

        /// <summary>
        /// Parses a header line and validates that it has the correct format.
        /// </summary>
        /// <param name="headerLine">The header line to validate.</param>
        /// <param name="headerName">The name of the header.</param>
        /// <param name="headerValue">The value of the header.</param>
        private static void ValidateHeaderLine(string headerLine, out string headerName, out string headerValue)
        {
            Debug.Assert(headerLine != null && headerLine.Length > 0, "Expected non-empty header line.");

            int colon = headerLine.IndexOf(':');
            if (colon <= 0)
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidHeaderSpecified(headerLine));
            }

            headerName = headerLine.Substring(0, colon).Trim();
            headerValue = headerLine.Substring(colon + 1).Trim();
        }

        /// <summary>
        /// Reads a line (all bytes until a line feed) from the underlying stream.
        /// </summary>
        /// <returns>Returns the string that was read from the underyling stream (not including a terminating line feed), or null if the end of input was reached.</returns>
        private string ReadLine()
        {
            Debug.Assert(this.batchEncoding != null, "Batch encoding should have been established on first call to SkipToBoundary.");
            Debug.Assert(this.lineBuffer != null && this.lineBuffer.Length == LineBufferLength, "Line buffer should have been created.");

            // The number of bytes in the line buffer that make up the line.
            int lineBufferSize = 0;

            // Start with the pre-allocated line buffer array.
            byte[] bytesForString = this.lineBuffer;

            ODataBatchReaderStreamScanResult scanResult = ODataBatchReaderStreamScanResult.NoMatch;
            while (scanResult != ODataBatchReaderStreamScanResult.Match)
            {
                int byteCount, lineEndStartPosition, lineEndEndPosition;
                scanResult = this.batchBuffer.ScanForLineEnd(out lineEndStartPosition, out lineEndEndPosition);

                switch (scanResult)
                {
                    case ODataBatchReaderStreamScanResult.NoMatch:
                        // Copy all the bytes in the batchBuffer into the result byte[] and then continue
                        byteCount = this.batchBuffer.NumberOfBytesInBuffer;
                        if (byteCount > 0)
                        {
                            // TODO: [Design] Consider security limits for data being read
                            ODataBatchUtils.EnsureArraySize(ref bytesForString, lineBufferSize, byteCount);
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, bytesForString, lineBufferSize, byteCount);
                            lineBufferSize += byteCount;
                        }

                        if (this.underlyingStreamExhausted)
                        {
                            if (lineBufferSize == 0)
                            {
                                // If there's nothing more to pull from the underlying stream, and we didn't read anything 
                                // in this invocation of ReadLine(), return null to indicate end of input.
                                return null;
                            }

                            // Nothing more to read; stop looping
                            scanResult = ODataBatchReaderStreamScanResult.Match;
                            this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + byteCount);
                        }
                        else
                        {
                            this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, /*preserveFrom*/ ODataBatchReaderStreamBuffer.BufferLength);
                        }

                        break;
                    case ODataBatchReaderStreamScanResult.PartialMatch:
                        // We found the start of a line end in the buffer but could not verify whether we saw all of it.
                        // This can happen if a line end is represented as \r\n and we found \r at the very last position in the buffer.
                        // In this case we copy the bytes into the result byte[] and continue at the start of the line end; this will guarantee
                        // that the next scan will find the full line end, not find any additional bytes and then skip the full line end.
                        // It is safe to copy the string right here because we will also accept \r as a line end; we are just not sure whether there 
                        // will be a subsequent \n.
                        // This can also happen if the last byte in the stream is \r.
                        byteCount = lineEndStartPosition - this.batchBuffer.CurrentReadPosition;
                        if (byteCount > 0)
                        {
                            ODataBatchUtils.EnsureArraySize(ref bytesForString, lineBufferSize, byteCount);
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, bytesForString, lineBufferSize, byteCount);
                            lineBufferSize += byteCount;
                        }

                        if (this.underlyingStreamExhausted)
                        {
                            // Nothing more to read; stop looping
                            scanResult = ODataBatchReaderStreamScanResult.Match;
                            this.batchBuffer.SkipTo(lineEndStartPosition + 1);
                        }
                        else
                        {
                            this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, /*preserveFrom*/ lineEndStartPosition);
                        }

                        break;
                    case ODataBatchReaderStreamScanResult.Match:
                        // We found a line end in the buffer
                        Debug.Assert(lineEndStartPosition >= this.batchBuffer.CurrentReadPosition, "Line end must be at or after current position.");
                        Debug.Assert(lineEndEndPosition < this.batchBuffer.CurrentReadPosition + this.batchBuffer.NumberOfBytesInBuffer, "Line end must finish withing buffer range.");

                        byteCount = lineEndStartPosition - this.batchBuffer.CurrentReadPosition;
                        if (byteCount > 0)
                        {
                            ODataBatchUtils.EnsureArraySize(ref bytesForString, lineBufferSize, byteCount);
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, bytesForString, lineBufferSize, byteCount);
                            lineBufferSize += byteCount;
                        }

                        this.batchBuffer.SkipTo(lineEndEndPosition + 1);
                        break;
                    default:
                        throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_ReadLine));
                }
            }

            Debug.Assert(bytesForString != null, "bytesForString != null");

            return this.CurrentEncoding.GetString(bytesForString, 0, lineBufferSize);
        }

        /// <summary>
        /// Ensure that a batch encoding exists; if not, detect it from the first couple of bytes of the stream.
        /// </summary>
        private void EnsureBatchEncoding()
        {
            // If no batch encoding is specified we detect it from the first bytes in the buffer.
            if (this.batchEncoding == null)
            {
                // NOTE: The batch encoding will only ever be null on the first call to this method which 
                //       happens before the batch reader skips to the first boundary.
                this.batchEncoding = this.DetectEncoding();
            }

            // Verify that we only allow single byte encodings and UTF-8 for now.
            ReaderValidationUtils.ValidateEncodingSupportedInBatch(this.batchEncoding);
        }

        /// <summary>Detect the encoding based data from the stream.</summary>
        /// <returns>The encoding discovered from the bytes in the buffer or the fallback encoding.</returns>
        /// <remarks>
        /// We don't have to skip a potential preamble of the encoding since the batch reader
        /// will skip over everything (incl. the potential preamble) until it finds the first
        /// boundary.
        /// </remarks>
        private Encoding DetectEncoding()
        {
            // We need at most 4 bytes in the buffer to determine the encoding; if we have less than that,
            // refill the buffer.
            while (!this.underlyingStreamExhausted && this.batchBuffer.NumberOfBytesInBuffer < 4)
            {
                this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, this.batchBuffer.CurrentReadPosition);
            }

            // Now we should have a full buffer unless the underlying stream did not have enough bytes.
            int numberOfBytesInBuffer = this.batchBuffer.NumberOfBytesInBuffer;
            if (numberOfBytesInBuffer < 2)
            {
                Debug.Assert(this.underlyingStreamExhausted, "Underlying stream must be exhausted if we have less than 2 bytes in the buffer after refilling.");

                // If we cannot read any of the known preambles we fall back to the default encoding, which is US-ASCII.
#if SILVERLIGHT || WINDOWS_PHONE || PORTABLELIB
                // ASCII not available; use UTF8 without preamble
                return MediaType.FallbackEncoding;
#else
                return Encoding.ASCII;
#endif
            }
            else if (this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0xFE && this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0xFF)
            {   
                // Big Endian Unicode
                return new UnicodeEncoding(/*bigEndian*/ true, /*byteOrderMark*/ true);
            }
            else if (this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0xFF && this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0xFE)
            {   
                // Little Endian Unicode, or possibly little endian UTF32
                if (numberOfBytesInBuffer >= 4 &&
                    this.batchBuffer[this.batchBuffer.CurrentReadPosition + 2] == 0 &&
                    this.batchBuffer[this.batchBuffer.CurrentReadPosition + 3] == 0)
                {
#if SILVERLIGHT || WINDOWS_PHONE || PORTABLELIB
                    // Little Endian UTF32 not available
                    throw Error.NotSupported();
#else
                    return new UTF32Encoding(/*bigEndian*/ false, /*byteOrderMark*/ true);
#endif
                }
                else
                {
                    return new UnicodeEncoding(/*bigEndian*/ false, /*byteOrderMark*/ true);
                }
            }
            else if (numberOfBytesInBuffer >= 3 &&
                     this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0xEF &&
                     this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0xBB &&
                     this.batchBuffer[this.batchBuffer.CurrentReadPosition + 2] == 0xBF)
            {   
                // UTF-8
                return Encoding.UTF8;
            }
            else if (numberOfBytesInBuffer >= 4 &&
                     this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0 &&
                     this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0 &&
                     this.batchBuffer[this.batchBuffer.CurrentReadPosition + 2] == 0xFE &&
                     this.batchBuffer[this.batchBuffer.CurrentReadPosition + 3] == 0xFF)
            {   
                // Big Endian UTF32
#if SILVERLIGHT || WINDOWS_PHONE || PORTABLELIB
                // Big Endian UTF32 not available
                throw Error.NotSupported();
#else
                return new UTF32Encoding(/*bigEndian*/ true, /*byteOrderMark*/ true);
#endif
            }
            else
            {
#if SILVERLIGHT || WINDOWS_PHONE || PORTABLELIB
                // ASCII not available; use UTF8 without preamble
                return MediaType.FallbackEncoding;
#else
                return Encoding.ASCII;
#endif
            }
        }

        /// <summary>
        /// Reads and validates the headers of a batch part.
        /// </summary>
        /// <param name="isChangeSetPart">true if the headers indicate a changset part; otherwise false.</param>
        /// <returns>A dictionary of header names to header values; never null.</returns>
        private ODataBatchOperationHeaders ReadPartHeaders(out bool isChangeSetPart)
        {
            ODataBatchOperationHeaders partHeaders = this.ReadHeaders();
            return this.ValidatePartHeaders(partHeaders, out isChangeSetPart);
        }

        /// <summary>
        /// Validates the headers that have been read for a part.
        /// </summary>
        /// <param name="headers">The set of headers to validate.</param>
        /// <param name="isChangeSetPart">true if the headers indicate a changset part; otherwise false.</param>
        /// <returns>The set of validated headers.</returns>
        /// <remarks>
        /// An operation part is required to have content type 'application/http' and content transfer
        /// encoding 'binary'. A changeset is required to have content type 'multipart/mixed'.
        /// Note that we allow additional headers for batch parts; clients of the library can choose
        /// to be more strict.
        /// </remarks>
        private ODataBatchOperationHeaders ValidatePartHeaders(ODataBatchOperationHeaders headers, out bool isChangeSetPart)
        {
            string contentType;
            if (!headers.TryGetValue(ODataConstants.ContentTypeHeader, out contentType))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_MissingContentTypeHeader);
            }

            if (MediaTypeUtils.MediaTypeAndSubtypeAreEqual(contentType, MimeConstants.MimeApplicationHttp))
            {
                isChangeSetPart = false;

                // An operation part is required to have application/http content type and 
                // binary content transfer encoding. 
                string transferEncoding;
                if (!headers.TryGetValue(ODataConstants.ContentTransferEncoding, out transferEncoding) ||
                    string.Compare(transferEncoding, ODataConstants.BatchContentTransferEncoding, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new ODataException(Strings.ODataBatchReaderStream_MissingOrInvalidContentEncodingHeader(
                        ODataConstants.ContentTransferEncoding,
                        ODataConstants.BatchContentTransferEncoding));
                }
            }
            else if (MediaTypeUtils.MediaTypeStartsWithTypeAndSubtype(contentType, MimeConstants.MimeMultipartMixed))
            {
                isChangeSetPart = true;

                if (this.changesetBoundary != null)
                {
                    // Nested changesets are not supported
                    throw new ODataException(Strings.ODataBatchReaderStream_NestedChangesetsAreNotSupported);
                }
            }
            else
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidContentTypeSpecified(
                    ODataConstants.ContentTypeHeader,
                    contentType,
                    MimeConstants.MimeMultipartMixed,
                    MimeConstants.MimeApplicationHttp));
            }

            return headers;
        }

        /// <summary>
        /// Parse the content type header value to retrieve the boundary and encoding of a changeset.
        /// </summary>
        /// <param name="contentType">The content type to parse.</param>
        private void DetermineChangesetBoundaryAndEncoding(string contentType)
        {
            Debug.Assert(!string.IsNullOrEmpty(contentType), "Should have validated that non-null, non-empty content type header exists.");

            MediaType mediaType;
            ODataPayloadKind readerPayloadKind;
            MediaTypeUtils.GetFormatFromContentType(
                contentType, 
                new ODataPayloadKind[] { ODataPayloadKind.Batch }, 
                MediaTypeResolver.DefaultMediaTypeResolver,
                out mediaType, 
                out this.changesetEncoding, 
                out readerPayloadKind, 
                out this.changesetBoundary);
            Debug.Assert(readerPayloadKind == ODataPayloadKind.Batch, "Must find batch payload kind.");
            Debug.Assert(this.changesetBoundary != null && this.changesetBoundary.Length > 0, "Boundary string should have been validated by now.");
            Debug.Assert(HttpUtils.CompareMediaTypeNames(MimeConstants.MimeMultipartMixed, mediaType.FullTypeName), "Must be multipart/mixed media type.");
        }
    }
}

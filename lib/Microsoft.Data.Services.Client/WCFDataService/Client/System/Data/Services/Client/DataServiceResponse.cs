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

    /// <summary>
    /// Data service response to ExecuteBatch &amp; SaveChanges
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010", Justification = "required for this feature")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public sealed class DataServiceResponse : IEnumerable<OperationResponse>
    {
        /// <summary>Http headers of the response.</summary>
        private readonly HeaderCollection headers;

        /// <summary>Http status code of the response.</summary>
        private readonly int statusCode;

        /// <summary>responses</summary>
        private readonly IEnumerable<OperationResponse> response;

        /// <summary>true if this is a batch response, otherwise false.</summary>
        private readonly bool batchResponse;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="headers">HTTP headers</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="response">list of responses</param>
        /// <param name="batchResponse">true if this represents a batch response, otherwise false.</param>
        internal DataServiceResponse(HeaderCollection headers, int statusCode, IEnumerable<OperationResponse> response, bool batchResponse)
        {
            Debug.Assert(response != null, "response!=null");
            this.headers = headers ?? new HeaderCollection();
            this.statusCode = statusCode;
            this.batchResponse = batchResponse;
            this.response = response;
        }

        /// <summary>The headers from an HTTP response associated with a batch request.</summary>
        /// <returns>An <see cref="T:System.Collections.IDictionary" /> object containing the name-value pairs of an HTTP response.</returns>
        public IDictionary<string, string> BatchHeaders
        {
            get { return this.headers.UnderlyingDictionary; }
        }

        /// <summary>The status code from an HTTP response associated with a batch request.</summary>
        /// <returns>An integer based on status codes defined in Hypertext Transfer Protocol.</returns>
        public int BatchStatusCode
        {
            get { return this.statusCode; }
        }

        /// <summary>Gets a Boolean value that indicates whether the response contains multiple results.</summary>
        /// <returns>A Boolean value that indicates whether the response contains multiple results.</returns>
        public bool IsBatchResponse
        {
            get { return this.batchResponse; }
        }

        /// <summary>Gets an enumerator that enables retrieval of responses to operations being tracked by <see cref="T:System.Data.Services.Client.OperationResponse" /> objects within the <see cref="T:System.Data.Services.Client.DataServiceResponse" />.</summary>
        /// <returns>An enumerator over the response received from the service.</returns>
        public IEnumerator<OperationResponse> GetEnumerator()
        {
            return this.response.GetEnumerator();
        }

        /// <summary>Gets an enumerator that enables retrieval of responses to operations being tracked by <see cref="T:System.Data.Services.Client.OperationResponse" /> objects within the <see cref="T:System.Data.Services.Client.DataServiceResponse" />.</summary>
        /// <returns>An enumerator over the response received from the service.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

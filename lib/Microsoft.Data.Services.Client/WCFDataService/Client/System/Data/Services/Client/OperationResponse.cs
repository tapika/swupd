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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>Operation response base class</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010", Justification = "required for this feature")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public abstract class OperationResponse
    {
        /// <summary>Http headers of the response.</summary>
        private readonly HeaderCollection headers;

        /// <summary>Http status code of the response.</summary>
        private int statusCode;

        /// <summary>exception to throw during get results</summary>
        private Exception innerException;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="headers">HTTP headers</param>
        internal OperationResponse(HeaderCollection headers)
        {
            Debug.Assert(null != headers, "null headers");
            this.headers = headers;
        }

        /// <summary>When overridden in a derived class, contains the HTTP response headers associated with a single operation.</summary>
        /// <returns><see cref="T:System.Collections.IDictionary" /> object that contains name value pairs of headers and values.</returns>
        public IDictionary<string, string> Headers
        {
            get { return this.headers.UnderlyingDictionary; }
        }

        /// <summary>When overridden in a derived class, gets or sets the HTTP response code associated with a single operation.</summary>
        /// <returns>Integer value that contains response code.</returns>
        public int StatusCode
        {
            get { return this.statusCode; }
            internal set { this.statusCode = value; }
        }

        /// <summary>Gets error thrown by the operation.</summary>
        /// <returns>An <see cref="T:System.Exception" /> object that contains the error.</returns>
        public Exception Error
        {
            get
            {
                return this.innerException;
            }

            set
            {
                this.innerException = value;
            }
        }

        /// <summary>Http headers of the response.</summary>
        internal HeaderCollection HeaderCollection
        {
            get { return this.headers; }
        }
    }
}

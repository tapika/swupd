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
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>non-generic placeholder for generic implementation</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010", Justification = "required for this feature")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public abstract class DataServiceQuery : DataServiceRequest, IQueryable
    {
        /// <summary>internal constructor so that only our assembly can provide an implementation</summary>
        internal DataServiceQuery()
        {
        }

        /// <summary>Represents an expression that contains the query to the data service.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> object that represents the query.</returns>
        public abstract Expression Expression
        {
            get;
        }

        /// <summary>Represents the query provider instance.</summary>
        /// <returns>An <see cref="T:System.Linq.IQueryProvider" /> representing the data source provider.</returns>
        public abstract IQueryProvider Provider
        {
            get;
        }

        /// <summary>Gets the <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection returned by the query.</summary>
        /// <returns>An enumerator over the query results.</returns>
        /// <remarks>Expect derived class to override this with an explict interface implementation</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033", Justification = "required for this feature")]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw Error.NotImplemented();
        }

#if !ASTORIA_LIGHT && !PORTABLELIB
        /// <summary>Executes the query against the data service.Not supported by the WCF Data Services 5.0 client for Silverlight.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the results of the query operation.</returns>
        /// <exception cref="T:System.Data.Services.Client.DataServiceQueryException">When the data service returns an HTTP 404: Resource Not Found error.</exception>
        public IEnumerable Execute()
        {
            return this.ExecuteInternal();
        }
#endif

        /// <summary>Asynchronously sends a request to execute the data service query.</summary>
        /// <returns>An <see cref="T:System.IAsyncResult" /> object that is used to track the status of the asynchronous operation.</returns>
        /// <param name="callback">Delegate to invoke when results are available for client consumption.</param>
        /// <param name="state">User-defined state object passed to the callback.</param>
        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            return this.BeginExecuteInternal(callback, state);
        }

        /// <summary>Called to complete the asynchronous operation of executing a data service query.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the results of the query operation.</returns>
        /// <param name="asyncResult">The result from the <see cref="M:System.Data.Services.Client.DataServiceQuery.BeginExecute(System.AsyncCallback,System.Object)" /> operation that contains the query results.</param>
        /// <exception cref="T:System.Data.Services.Client.DataServiceQueryException">When the data service returns an HTTP 404: Resource Not Found error.</exception>
        public IEnumerable EndExecute(IAsyncResult asyncResult)
        {
            return this.EndExecuteInternal(asyncResult);
        }

#if !ASTORIA_LIGHT && !PORTABLELIB
        /// Synchronous methods not available
        /// <summary>
        /// Returns an IEnumerable from an Internet resource. 
        /// </summary>
        /// <returns>An IEnumerable that contains the response from the Internet resource.</returns>
        internal abstract IEnumerable ExecuteInternal();
#endif

        /// <summary>
        /// Begins an asynchronous request to an Internet resource.
        /// </summary>
        /// <param name="callback">The AsyncCallback delegate.</param>
        /// <param name="state">The state object for this request.</param>
        /// <returns>An IAsyncResult that references the asynchronous request for a response.</returns>
        internal abstract IAsyncResult BeginExecuteInternal(AsyncCallback callback, object state);

        /// <summary>
        /// Ends an asynchronous request to an Internet resource.
        /// </summary>
        /// <param name="asyncResult">The pending request for a response. </param>
        /// <returns>An IEnumerable that contains the response from the Internet resource.</returns>
        internal abstract IEnumerable EndExecuteInternal(IAsyncResult asyncResult);
    }
}

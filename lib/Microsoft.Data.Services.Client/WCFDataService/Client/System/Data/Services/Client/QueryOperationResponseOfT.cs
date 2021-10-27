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
    #region Namespaces

    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;

    #endregion Namespaces

    /// <summary>
    /// Response to a batched query or Execute call.
    /// </summary>
    /// <typeparam name="T">The type to construct for the request results</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public sealed class QueryOperationResponse<T> : QueryOperationResponse, IEnumerable<T>
    {
        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="headers">HTTP headers</param>
        /// <param name="query">original query</param>
        /// <param name="results">retrieved objects</param>
        internal QueryOperationResponse(HeaderCollection headers, DataServiceRequest query, MaterializeAtom results)
            : base(headers, query, results)
        {
        }

        #endregion Constructors

        #region Public properties

        /// <summary>The server result set count value from a query, if the query has requested the value.</summary>
        /// <returns>The return value can be either zero or a positive value equal to the number of entities in the set on the server.</returns>
        public override long TotalCount
        {
            get
            {
                if (this.Results != null && this.Results.IsCountable)
                {
                    return this.Results.CountValue();
                }
                else
                {
                    throw new InvalidOperationException(Strings.MaterializeFromAtom_CountNotPresent);
                }
            }
        }

        #endregion Public properties

        #region Public methods

        /// <summary>Gets a <see cref="T:System.Data.Services.Client.DataServiceQueryContinuation`1" /> object that contains the URI that is used to retrieve the next results page.</summary>
        /// <returns>An object that contains the URI that is used to return the next results page.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "required for this feature")]
        public new DataServiceQueryContinuation<T> GetContinuation()
        {
            return (DataServiceQueryContinuation<T>)base.GetContinuation();
        }

        /// <summary>Executes the <see cref="T:System.Data.Services.Client.DataServiceQuery`1" /> and gets <see cref="T:System.Data.Services.Client.QueryOperationResponse`1" /> items.</summary>
        /// <returns>An enumerator to a collection of <see cref="T:System.Data.Services.Client.QueryOperationResponse`1" /> items.</returns>
        /// <remarks>In the case of Collection(primitive) or Collection(complex), the entire collection is
        /// materialized when this is called.</remarks>
        public new IEnumerator<T> GetEnumerator()
        {
            return this.GetEnumeratorHelper<IEnumerator<T>>(() => this.Results.Cast<T>().GetEnumerator());
        }

        #endregion Public methods.
    }
}

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

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Microsoft.Data.OData;

    #endregion Namespaces

    /// <summary>Represents the components of query.</summary>
    internal class QueryComponents
    {
        #region Private fields

        /// <summary> type </summary>
        private readonly Type lastSegmentType;

        /// <summary>Records the generated-to-source rewrites created.</summary>
        private readonly Dictionary<Expression, Expression> normalizerRewrites;

        /// <summary>selector Lambda Expression</summary>
        private readonly LambdaExpression projection;

        /// <summary>HttpMethod to use in the query.</summary>
        private readonly string httpMethod;

        /// <summary>List of parameters for a service operation or a service function.</summary>
        private readonly List<UriOperationParameter> uriOperationParameters;

        /// <summary>List of parameters for service action.</summary>
        private readonly List<BodyOperationParameter> bodyOperationParameters;

        /// <summary>
        /// Optional field; not all codepaths set this.  If true, then a single primitive or complex value is expected.
        /// If false, then a collection of primitives or complex is expected. A null value makes no claim as to what
        /// the return type should be. It follows that a single entry or a feed would always have this value as null.
        /// </summary>
        private readonly bool? singleResult;

        /// <summary>Query option used in projection queries.</summary>
        private const string SelectQueryOption = "$select=";

        /// <summary>Select query option as it appears at the beginning of a query string.</summary>
        private const string SelectQueryOptionWithQuestionMark = "?" + SelectQueryOption;

        /// <summary>Select query option as it appears in the middle of a query string.</summary>
        private const string SelectQueryOptionWithAmpersand = "&" + SelectQueryOption;

        /// <summary> Version for query </summary>
        private Version version;

        #endregion Private fields

        #region Constructors

        /// <summary>
        ///  Constructs a container for query components with HttpMethod GET.
        /// </summary>
        /// <param name="uri">URI for the query</param>
        /// <param name="version">Version for the query</param>
        /// <param name="lastSegmentType">Element type for the query</param>
        /// <param name="projection">selector Lambda Expression</param>
        /// <param name="normalizerRewrites">Records the generated-to-source rewrites created (possibly null).</param>
        internal QueryComponents(Uri uri, Version version, Type lastSegmentType, LambdaExpression projection, Dictionary<Expression, Expression> normalizerRewrites)
        {
            this.projection = projection;
            this.normalizerRewrites = normalizerRewrites;
            this.lastSegmentType = lastSegmentType;
            this.Uri = uri;
            this.version = version;
            this.httpMethod = XmlConstants.HttpMethodGet;
        }

        /// <summary>
        ///  Constructs a container for query components
        /// </summary>
        /// <param name="uri">URI for the query</param>
        /// <param name="version">Version for the query</param>
        /// <param name="lastSegmentType">Element type for the query</param>
        /// <param name="projection">selector Lambda Expression</param>
        /// <param name="normalizerRewrites">Records the generated-to-source rewrites created (possibly null).</param>
        /// <param name="httpMethod">The HttpMethod to be used in the request.</param>
        /// <param name="singleResult">If true, then a single primitive or complex value is expected. If false, then a collection of primitives or complex 
        /// is expected. Should be null when expecting a void response, a single entry, or a feed.</param>
        /// <param name="bodyOperationParameters">The body operation parameters associated with a service action.</param>
        /// <param name="uriOperationParameters">The uri operation parameters associated with a service function or a service operation.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used in DataServiceContext")]
        internal QueryComponents(
            Uri uri, 
            Version version, 
            Type lastSegmentType, 
            LambdaExpression projection, 
            Dictionary<Expression, Expression> normalizerRewrites, 
            string httpMethod, 
            bool? singleResult, 
            List<BodyOperationParameter> bodyOperationParameters,
            List<UriOperationParameter> uriOperationParameters)
        {
            Debug.Assert(
                string.CompareOrdinal(XmlConstants.HttpMethodGet, httpMethod) == 0 || 
                string.CompareOrdinal(XmlConstants.HttpMethodPost, httpMethod) == 0, 
                "httpMethod should only be GET or POST");

            this.projection = projection;
            this.normalizerRewrites = normalizerRewrites;
            this.lastSegmentType = lastSegmentType;
            this.Uri = uri;
            this.version = version;
            this.httpMethod = httpMethod;
            this.uriOperationParameters = uriOperationParameters;
            this.bodyOperationParameters = bodyOperationParameters;
            this.singleResult = singleResult;
        }

        #endregion

        /// <summary>Records the generated-to-source rewrites created.</summary>
        internal Dictionary<Expression, Expression> NormalizerRewrites
        {
            get 
            { 
                return this.normalizerRewrites; 
            }
        }

        /// <summary>The projection expression for a query</summary>
        internal LambdaExpression Projection
        {
            get
            {
                return this.projection;
            }
        }

        /// <summary>The last segment type for query</summary>
        internal Type LastSegmentType
        {
            get
            {
                return this.lastSegmentType;
            }
        }

        /// <summary>The data service version associated with the uri</summary>
        internal Version Version
        {
            get
            {
                return this.version;
            }
        }

        /// <summary>The HttpMethod to be used in the query.</summary>
        internal string HttpMethod
        {
            get
            {
                return this.httpMethod;
            }
        }

        /// <summary>
        /// List of operation parameters for service operation or a service function. 
        /// </summary>
        internal List<UriOperationParameter> UriOperationParameters
        {
            get
            {
                return this.uriOperationParameters;
            }
        }

        /// <summary>
        /// List of operation parameters for a service action.
        /// </summary>
        internal List<BodyOperationParameter> BodyOperationParameters
        {
            get
            {
                return this.bodyOperationParameters;
            }
        }

        /// <summary>
        /// Optional field; not all codepaths set this.  If true, then a single primitive or complex value is expected.
        /// If false, then a collection of primitives or complex is expected. A null value makes no claim as to what
        /// the return type should be. It follows that a single entry or a feed would always have this value as null.
        /// </summary>
        internal bool? SingleResult
        {
            get
            {
                return this.singleResult;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the URI for this query has the select query option.
        /// </summary>
        internal bool HasSelectQueryOption
        {
            get { return this.Uri != null && ContainsSelectQueryOption(UriUtil.UriToString(this.Uri)); }
        }

        /// <summary>Gets or sets the URI for a query, possibly with query options added to the cached URI.</summary>
        /// <value> URI with additional query options added if required. </value>
        internal Uri Uri { get; set; }

        /// <summary>
        /// Determines whether or not the specified query string contains the $select query option.
        /// </summary>
        /// <param name="queryString">String that may contain $select.</param>
        /// <returns>True if the specified string contains the $select query option, otherwise false.</returns>
        /// <remarks>
        /// This method is specifically looking for patterns that would indicate we really have the specific query option and not something like $selectNew. It also expects that
        /// any data string being passed to this method has already been escaped, as the things we are looking for specifically contain equals signs that would be escaped if in a data value.
        /// </remarks>
        private static bool ContainsSelectQueryOption(string queryString)
        {
            return queryString.Contains(SelectQueryOptionWithQuestionMark) || queryString.Contains(SelectQueryOptionWithAmpersand);
        }
    }
}

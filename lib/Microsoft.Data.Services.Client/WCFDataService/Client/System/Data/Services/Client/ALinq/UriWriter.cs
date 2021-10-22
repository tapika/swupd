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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    #endregion Namespaces

    /// <summary>
    /// Translates resource bound expression trees into URIs.
    /// </summary>
    internal class UriWriter : DataServiceALinqExpressionVisitor
    {
        /// <summary>Data context used to generate type names for types.</summary>
        private readonly DataServiceContext context;

        /// <summary>stringbuilder for constructed URI</summary>
        private readonly StringBuilder uriBuilder;

        /// <summary>the request data service version for the uri</summary>
        private Version uriVersion;

        /// <summary>the leaf resourceset for the URI being written</summary>
        private ResourceSetExpression leafResourceSet;

        /// <summary>
        /// For caching query options to be grouped
        /// </summary>
        private Dictionary<string, List<string>> cachedQueryOptions = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        /// <summary>
        /// Private constructor for creating UriWriter
        /// </summary>
        /// <param name='context'>Data context used to generate type names for types.</param>
        private UriWriter(DataServiceContext context)
        {
            Debug.Assert(context != null, "context != null");
            this.context = context;
            this.uriBuilder = new StringBuilder();
            this.uriVersion = Util.DataServiceVersion1;
        }

        /// <summary>
        /// Translates resource bound expression tree to a URI.
        /// </summary>
        /// <param name='context'>Data context used to generate type names for types.</param>
        /// <param name="addTrailingParens">flag to indicate whether generated URI should include () if leaf is ResourceSet</param>
        /// <param name="e">The expression to translate</param>
        /// <param name="uri">uri</param>
        /// <param name="version">version for query</param>    
        internal static void Translate(DataServiceContext context, bool addTrailingParens, Expression e, out Uri uri, out Version version)
        {
            var writer = new UriWriter(context);
            writer.leafResourceSet = addTrailingParens ? (e as ResourceSetExpression) : null;
            writer.Visit(e);
            uri = UriUtil.CreateUri(writer.uriBuilder.ToString(), UriKind.Absolute);
            version = writer.uriVersion;
        }

        /// <summary>
        /// MethodCallExpression visit method
        /// </summary>
        /// <param name="m">The MethodCallExpression expression to visit</param>
        /// <returns>The visited MethodCallExpression expression </returns>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            throw Error.MethodNotSupported(m);
        }

        /// <summary>
        /// UnaryExpression visit method
        /// </summary>
        /// <param name="u">The UnaryExpression expression to visit</param>
        /// <returns>The visited UnaryExpression expression </returns>
        internal override Expression VisitUnary(UnaryExpression u)
        {
            throw new NotSupportedException(Strings.ALinq_UnaryNotSupported(u.NodeType.ToString()));
        }

        /// <summary>
        /// BinaryExpression visit method
        /// </summary>
        /// <param name="b">The BinaryExpression expression to visit</param>
        /// <returns>The visited BinaryExpression expression </returns>
        internal override Expression VisitBinary(BinaryExpression b)
        {
            throw new NotSupportedException(Strings.ALinq_BinaryNotSupported(b.NodeType.ToString()));
        }

        /// <summary>
        /// ConstantExpression visit method
        /// </summary>
        /// <param name="c">The ConstantExpression expression to visit</param>
        /// <returns>The visited ConstantExpression expression </returns>
        internal override Expression VisitConstant(ConstantExpression c)
        {
            throw new NotSupportedException(Strings.ALinq_ConstantNotSupported(c.Value));
        }

        /// <summary>
        /// TypeBinaryExpression visit method
        /// </summary>
        /// <param name="b">The TypeBinaryExpression expression to visit</param>
        /// <returns>The visited TypeBinaryExpression expression </returns>
        internal override Expression VisitTypeIs(TypeBinaryExpression b)
        {
            throw new NotSupportedException(Strings.ALinq_TypeBinaryNotSupported);
        }

        /// <summary>
        /// ConditionalExpression visit method
        /// </summary>
        /// <param name="c">The ConditionalExpression expression to visit</param>
        /// <returns>The visited ConditionalExpression expression </returns>
        internal override Expression VisitConditional(ConditionalExpression c)
        {
            throw new NotSupportedException(Strings.ALinq_ConditionalNotSupported);
        }

        /// <summary>
        /// ParameterExpression visit method
        /// </summary>
        /// <param name="p">The ParameterExpression expression to visit</param>
        /// <returns>The visited ParameterExpression expression </returns>
        internal override Expression VisitParameter(ParameterExpression p)
        {
            throw new NotSupportedException(Strings.ALinq_ParameterNotSupported);
        }

        /// <summary>
        /// MemberExpression visit method
        /// </summary>
        /// <param name="m">The MemberExpression expression to visit</param>
        /// <returns>The visited MemberExpression expression </returns>
        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            throw new NotSupportedException(Strings.ALinq_MemberAccessNotSupported(m.Member.Name));
        }

        /// <summary>
        /// LambdaExpression visit method
        /// </summary>
        /// <param name="lambda">The LambdaExpression to visit</param>
        /// <returns>The visited LambdaExpression</returns>
        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            throw new NotSupportedException(Strings.ALinq_LambdaNotSupported);
        }

        /// <summary>
        /// NewExpression visit method
        /// </summary>
        /// <param name="nex">The NewExpression to visit</param>
        /// <returns>The visited NewExpression</returns>
        internal override NewExpression VisitNew(NewExpression nex)
        {
            throw new NotSupportedException(Strings.ALinq_NewNotSupported);
        }

        /// <summary>
        /// MemberInitExpression visit method
        /// </summary>
        /// <param name="init">The MemberInitExpression to visit</param>
        /// <returns>The visited MemberInitExpression</returns>
        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            throw new NotSupportedException(Strings.ALinq_MemberInitNotSupported);
        }

        /// <summary>
        /// ListInitExpression visit method
        /// </summary>
        /// <param name="init">The ListInitExpression to visit</param>
        /// <returns>The visited ListInitExpression</returns>
        internal override Expression VisitListInit(ListInitExpression init)
        {
            throw new NotSupportedException(Strings.ALinq_ListInitNotSupported);
        }

        /// <summary>
        /// NewArrayExpression visit method
        /// </summary>
        /// <param name="na">The NewArrayExpression to visit</param>
        /// <returns>The visited NewArrayExpression</returns>
        internal override Expression VisitNewArray(NewArrayExpression na)
        {
            throw new NotSupportedException(Strings.ALinq_NewArrayNotSupported);
        }

        /// <summary>
        /// InvocationExpression visit method
        /// </summary>
        /// <param name="iv">The InvocationExpression to visit</param>
        /// <returns>The visited InvocationExpression</returns>
        internal override Expression VisitInvocation(InvocationExpression iv)
        {
            throw new NotSupportedException(Strings.ALinq_InvocationNotSupported);
        }

        /// <summary>
        /// NavigationPropertySingletonExpression visit method.
        /// </summary>
        /// <param name="npse">NavigationPropertySingletonExpression expression to visit</param>
        /// <returns>Visited NavigationPropertySingletonExpression expression</returns>
        internal override Expression VisitNavigationPropertySingletonExpression(NavigationPropertySingletonExpression npse)
        {
            this.Visit(npse.Source);
            this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(this.ExpressionToString(npse.MemberExpression, /*inPath*/ true));
            this.VisitQueryOptions(npse);
            return npse;
        }

        /// <summary>
        /// ResourceSetExpression visit method.
        /// </summary>
        /// <param name="rse">ResourceSetExpression expression to visit</param>
        /// <returns>Visited ResourceSetExpression expression</returns>
        internal override Expression VisitResourceSetExpression(ResourceSetExpression rse)
        {
            if ((ResourceExpressionType)rse.NodeType == ResourceExpressionType.ResourceNavigationProperty)
            {
                this.Visit(rse.Source);
                this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(this.ExpressionToString(rse.MemberExpression, /*inPath*/ true));
            }
            else
            {
                // this is a resource set expression
                // we should be at the very begining of
                // the URI
                Debug.Assert(this.uriBuilder.Length == 0, "The builder is not empty while we are adding a resourset");
                string entitySetName = (String)((ConstantExpression)rse.MemberExpression).Value;
                this.uriBuilder.Append(this.context.BaseUriResolver.GetEntitySetUri(entitySetName));
            }

            WebUtil.RaiseVersion(ref this.uriVersion, rse.UriVersion);

            if (rse.ResourceTypeAs != null)
            {
                this.uriBuilder.Append(UriHelper.FORWARDSLASH);
                UriHelper.AppendTypeSegment(this.uriBuilder, rse.ResourceTypeAs, this.context, /*inPath*/ true, ref this.uriVersion);
            }

            if (rse.KeyPredicateConjuncts.Count > 0)
            {
                this.context.UrlConventions.AppendKeyExpression(rse.GetKeyProperties(), kvp => kvp.Key.Name, kvp => kvp.Value.Value, this.uriBuilder);
            }
            else if (rse == this.leafResourceSet)
            {
                // if resourceset is on the leaf, append ()
                this.uriBuilder.Append(UriHelper.LEFTPAREN);
                this.uriBuilder.Append(UriHelper.RIGHTPAREN);
            }

            if (rse.CountOption == CountOption.ValueOnly)
            {
                // append $count segment: /$count
                this.uriBuilder.Append(UriHelper.FORWARDSLASH).Append(UriHelper.DOLLARSIGN).Append(UriHelper.COUNT);
                WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
            }

            this.VisitQueryOptions(rse);
            return rse;
        }

        /// <summary>
        /// Visit Query options for Resource
        /// </summary>
        /// <param name="re">Resource Expression with query options</param>
        internal void VisitQueryOptions(ResourceExpression re)
        {
            if (re.HasQueryOptions)
            {
                this.uriBuilder.Append(UriHelper.QUESTIONMARK);

                ResourceSetExpression rse = re as ResourceSetExpression;
                if (rse != null)
                {
                    IEnumerator options = rse.SequenceQueryOptions.GetEnumerator();
                    while (options.MoveNext())
                    {
                        Expression e = ((Expression)options.Current);
                        ResourceExpressionType et = (ResourceExpressionType)e.NodeType;
                        switch (et)
                        {
                            case ResourceExpressionType.SkipQueryOption:
                                this.VisitQueryOptionExpression((SkipQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.TakeQueryOption:
                                this.VisitQueryOptionExpression((TakeQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.OrderByQueryOption:
                                this.VisitQueryOptionExpression((OrderByQueryOptionExpression)e);
                                break;
                            case ResourceExpressionType.FilterQueryOption:
                                this.VisitQueryOptionExpression((FilterQueryOptionExpression)e);
                                break;
                            default:
                                Debug.Assert(false, "Unexpected expression type " + (int)et);
                                break;
                        }
                    }
                }

                if (re.ExpandPaths.Count > 0)
                {
                    this.VisitExpandOptions(re.ExpandPaths);
                }

                if (re.Projection != null && re.Projection.Paths.Count > 0)
                {
                    this.VisitProjectionPaths(re.Projection.Paths);
                }

                if (re.CountOption == CountOption.InlineAll)
                {
                    this.VisitCountOptions();
                }

                if (re.CustomQueryOptions.Count > 0)
                {
                    this.VisitCustomQueryOptions(re.CustomQueryOptions);
                }

                this.AppendCachedQueryOptionsToUriBuilder();
            }
        }

        /// <summary>
        /// SkipQueryOptionExpression visit method.
        /// </summary>
        /// <param name="sqoe">SkipQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(SkipQueryOptionExpression sqoe)
        {
            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONSKIP, this.ExpressionToString(sqoe.SkipAmount, /*inPath*/ false));
        }

        /// <summary>
        /// TakeQueryOptionExpression visit method.
        /// </summary>
        /// <param name="tqoe">TakeQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(TakeQueryOptionExpression tqoe)
        {
            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONTOP, this.ExpressionToString(tqoe.TakeAmount, /*inPath*/ false));
        }

        /// <summary>
        /// FilterQueryOptionExpression visit method.
        /// </summary>
        /// <param name="fqoe">FilterQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(FilterQueryOptionExpression fqoe)
        {
            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONFILTER, this.ExpressionToString(fqoe.GetPredicate(), /*inPath*/ false));
        }

        /// <summary>
        /// OrderByQueryOptionExpression visit method.
        /// </summary>
        /// <param name="oboe">OrderByQueryOptionExpression expression to visit</param>
        internal void VisitQueryOptionExpression(OrderByQueryOptionExpression oboe)
        {
            StringBuilder tmpBuilder = new StringBuilder();
            int ii = 0;
            while (true)
            {
                var selector = oboe.Selectors[ii];

                tmpBuilder.Append(this.ExpressionToString(selector.Expression, /*inPath*/ false));
                if (selector.Descending)
                {
                    tmpBuilder.Append(UriHelper.SPACE);
                    tmpBuilder.Append(UriHelper.OPTIONDESC);
                }

                if (++ii == oboe.Selectors.Count)
                {
                    break;
                }

                tmpBuilder.Append(UriHelper.COMMA);
            }

            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONORDERBY, tmpBuilder.ToString());
        }

        /// <summary>
        /// VisitExpandOptions visit method.
        /// </summary>
        /// <param name="paths">Expand Paths</param>
        internal void VisitExpandOptions(List<string> paths)
        {
            StringBuilder tmpBuilder = new StringBuilder();
            int ii = 0;
            while (true)
            {
                tmpBuilder.Append(paths[ii]);

                if (++ii == paths.Count)
                {
                    break;
                }

                tmpBuilder.Append(UriHelper.COMMA);
            }

            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONEXPAND, tmpBuilder.ToString());
        }

        /// <summary>
        /// ProjectionPaths visit method.
        /// </summary>
        /// <param name="paths">Projection Paths</param>
        internal void VisitProjectionPaths(List<string> paths)
        {
            StringBuilder tmpBuilder = new StringBuilder();
            int ii = 0;
            while (true)
            {
                string path = paths[ii];

                tmpBuilder.Append(path);

                if (++ii == paths.Count)
                {
                    break;
                }

                tmpBuilder.Append(UriHelper.COMMA);
            }

            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONSELECT, tmpBuilder.ToString());
            WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
        }

        /// <summary>
        /// VisitCountOptions visit method.
        /// </summary>
        internal void VisitCountOptions()
        {
            this.AddAsCachedQueryOption(UriHelper.DOLLARSIGN + UriHelper.OPTIONCOUNT, UriHelper.COUNTALL);
            WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
        }

        /// <summary>
        /// VisitCustomQueryOptions visit method.
        /// </summary>
        /// <param name="options">Custom query options</param>
        internal void VisitCustomQueryOptions(Dictionary<ConstantExpression, ConstantExpression> options)
        {
            List<ConstantExpression> keys = options.Keys.ToList();
            List<ConstantExpression> values = options.Values.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                string k = keys[i].Value + "";
                string v = values[i].Value + "";
                this.AddAsCachedQueryOption(k, v);
            }
        }

        /// <summary>
        /// Caches query option to be grouped
        /// </summary>
        /// <param name="optionKey">The key.</param>
        /// <param name="optionValue">The value</param>
        private void AddAsCachedQueryOption(string optionKey, string optionValue)
        {
            List<string> tmp = null;
            if (!this.cachedQueryOptions.TryGetValue(optionKey, out tmp))
            {
                tmp = new List<string>();
                this.cachedQueryOptions.Add(optionKey, tmp);
            }

            tmp.Add(optionValue);
        }

        /// <summary>
        /// Append all cached query options to uri.
        /// </summary>
        private void AppendCachedQueryOptionsToUriBuilder()
        {
            int i = 0;
            foreach (var queryOption in this.cachedQueryOptions)
            {
                if (i++ != 0)
                {
                    this.uriBuilder.Append(UriHelper.AMPERSAND);
                }

                string keyStr = queryOption.Key;
                string valueStr = string.Join(",", queryOption.Value);
                this.uriBuilder.Append(keyStr);
                this.uriBuilder.Append(UriHelper.EQUALSSIGN);
                this.uriBuilder.Append(valueStr);

                if (keyStr.Equals(UriHelper.DOLLARSIGN + UriHelper.OPTIONCOUNT, StringComparison.Ordinal) ||
                    keyStr.Equals(UriHelper.DOLLARSIGN + UriHelper.OPTIONSELECT, StringComparison.Ordinal))
                {
                    WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion2);
                }
            }
        }

        /// <summary>Serializes an expression to a string.</summary>
        /// <param name="expression">Expression to serialize</param>
        /// <param name='inPath'>Whether or not the expression being written is part of the path of the URI.</param>
        /// <returns>The serialized expression.</returns>
        private string ExpressionToString(Expression expression, bool inPath)
        {
            return ExpressionWriter.ExpressionToString(this.context, expression, inPath, ref this.uriVersion);
        }
    }
}

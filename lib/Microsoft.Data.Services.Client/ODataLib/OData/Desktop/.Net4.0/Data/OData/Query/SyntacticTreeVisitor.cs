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

namespace Microsoft.Data.OData.Query.SyntacticAst
{
    using System;

    /// <summary>
    /// Visitor interface for walking the Syntactic Tree.
    /// </summary>
    /// <typeparam name="T">Generic type produced by the visitor.</typeparam>
    internal abstract class SyntacticTreeVisitor<T> : ISyntacticTreeVisitor<T>
    {
        /// <summary>
        /// Visit an AllToken
        /// </summary>
        /// <param name="tokenIn">The All token to visit</param>
        /// <returns>An AllNode bound to this token</returns>
        public virtual T Visit(AllToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits an AnyToken
        /// </summary>
        /// <param name="tokenIn">The Any token to visit</param>
        /// <returns>An AnyNode that's bound to this token</returns>
        public virtual T Visit(AnyToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a BinaryOperatorToken
        /// </summary>
        /// <param name="tokenIn">The Binary operator token to visit.</param>
        /// <returns>A BinaryOperatorNode thats bound to this token</returns>
        public virtual T Visit(BinaryOperatorToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a DottedIdentifierToken
        /// </summary>
        /// <param name="tokenIn">The DottedIdentifierToken to visit</param>
        /// <returns>Either a SingleEntityCastNode, or EntityCollectionCastNode bound to this DottedIdentifierToken</returns>
        public virtual T Visit(DottedIdentifierToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits an ExpandToken
        /// </summary>
        /// <param name="tokenIn">The ExpandToken to visit</param>
        /// <returns>A QueryNode bound to this ExpandToken</returns>
        public virtual T Visit(ExpandToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits an ExpandTermToken
        /// </summary>
        /// <param name="tokenIn">The ExpandTermToken to visit</param>
        /// <returns>A QueryNode bound to this ExpandTermToken</returns>
        public virtual T Visit(ExpandTermToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a FunctionCallToken
        /// </summary>
        /// <param name="tokenIn">The FunctionCallToken to visit</param>
        /// <returns>A SingleValueFunctionCallNode bound to this FunctionCallToken</returns>
        public virtual T Visit(FunctionCallToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a LiteralToken
        /// </summary>
        /// <param name="tokenIn">The LiteralToken to visit</param>
        /// <returns>A ConstantNode bound to this LambdaToken</returns>
        public virtual T Visit(LiteralToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a LambdaToken
        /// </summary>
        /// <param name="tokenIn">The LambdaToken to visit</param>
        /// <returns>A LambdaNode bound to this LambdaToken</returns>
        public virtual T Visit(LambdaToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a InnerPathToken
        /// </summary>
        /// <param name="tokenIn">The InnerPathToken to bind</param>
        /// <returns>A SingleValueNode or SingleEntityNode bound to this InnerPathToken</returns>
        public virtual T Visit(InnerPathToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits an OrderByToken
        /// </summary>
        /// <param name="tokenIn">The OrderByToken to bind</param>
        /// <returns>An OrderByClause bound to this OrderByToken</returns>
        public virtual T Visit(OrderByToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits an EndPathToken
        /// </summary>
        /// <param name="tokenIn">The EndPathToken to bind</param>
        /// <returns>A PropertyAccessClause bound to this EndPathToken</returns>
        public virtual T Visit(EndPathToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a CustomQueryOptionToken
        /// </summary>
        /// <param name="tokenIn">The CustomQueryOptionToken to bind</param>
        /// <returns>A CustomQueryOptionNode bound to this CustomQueryOptionToken</returns>
        public virtual T Visit(CustomQueryOptionToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a RangeVariableToken
        /// </summary>
        /// <param name="tokenIn">The RangeVariableToken to bind</param>
        /// <returns>An Entity or NonEntity RangeVariableReferenceNode bound to this RangeVariableToken</returns>
        public virtual T Visit(RangeVariableToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a SelectToken
        /// </summary>
        /// <param name="tokenIn">The SelectToken to bind</param>
        /// <returns>A QueryNode bound to this SelectToken</returns>
        public virtual T Visit(SelectToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a StarToken
        /// </summary>
        /// <param name="tokenIn">The StarToken to bind</param>
        /// <returns>A QueryNode bound to this StarToken</returns>
        public virtual T Visit(StarToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a UnaryOperatorToken
        /// </summary>
        /// <param name="tokenIn">The UnaryOperatorToken to bind</param>
        /// <returns>A UnaryOperatorNode bound to this UnaryOperatorToken</returns>
        public virtual T Visit(UnaryOperatorToken tokenIn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits a FuntionParameterToken
        /// </summary>
        /// <param name="tokenIn">The FunctionParameterToken to bind</param>
        /// <returns>A user defined value</returns>
        public virtual T Visit(FunctionParameterToken tokenIn)
        {
            throw new NotImplementedException();
        }
    }
}

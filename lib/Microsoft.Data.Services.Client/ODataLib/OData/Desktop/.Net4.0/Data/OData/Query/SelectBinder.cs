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
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Query.SemanticAst;

    /// <summary>
    /// Use a Select syntactic tree to populate the correct values for Selection in an already parsed
    /// Expand Semantic Tree.
    /// </summary>
    internal sealed class SelectBinder
    {
        /// <summary>
        /// Visitor object to walk the select tree
        /// </summary>
        private readonly SelectPropertyVisitor visitor;

        /// <summary>
        /// Constructs a new SelectBinder.
        /// </summary>
        /// <param name="model">The model used for binding.</param>
        /// <param name="entityType">The entity type that the $select is being applied to.</param>
        /// <param name="maxDepth">the maximum recursive depth.</param>
        /// <param name="expandClauseToDecorate">The already built expand clause to decorate</param>
        public SelectBinder(IEdmModel model, IEdmEntityType entityType, int maxDepth, SelectExpandClause expandClauseToDecorate)
        {
            ExceptionUtils.CheckArgumentNotNull(model, "tokenIn");
            ExceptionUtils.CheckArgumentNotNull(entityType, "entityType");

            this.visitor = new SelectPropertyVisitor(model, entityType, maxDepth, expandClauseToDecorate);
        }

        /// <summary>
        /// Visits the top level select token
        /// </summary>
        /// <param name="tokenIn">the select token to visit</param>
        /// <returns>A new SelectExpandClause decorated with the information from the selectToken</returns>
        public SelectExpandClause Bind(SelectToken tokenIn)
        {
            if (tokenIn == null || !tokenIn.Properties.Any())
            {
                this.visitor.DecoratedExpandClause.SetAllSelectionRecursively();
            }
            else
            {
                foreach (PathSegmentToken property in tokenIn.Properties)
                {
                    property.Accept(this.visitor);
                }
            }

            return this.visitor.DecoratedExpandClause;
        }
    }
}

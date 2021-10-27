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

using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm.Library.Expressions
{
    /// <summary>
    /// Represents an EDM labeled expression.
    /// </summary>
    public class EdmLabeledExpression : EdmElement, IEdmLabeledExpression
    {
        private readonly string name;
        private readonly IEdmExpression expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmLabeledExpression"/> class.
        /// </summary>
        /// <param name="name">Label of the expression.</param>
        /// <param name="expression">Underlying expression.</param>
        public EdmLabeledExpression(string name, IEdmExpression expression)
        {
            EdmUtil.CheckArgumentNull(name, "name");
            EdmUtil.CheckArgumentNull(expression, "expression");

            this.name = name;
            this.expression = expression;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the underlying expression.
        /// </summary>
        public IEdmExpression Expression
        {
            get { return this.expression; }
        }

        /// <summary>
        /// Gets the expression kind.
        /// </summary>
        public EdmExpressionKind ExpressionKind
        {
            get { return EdmExpressionKind.Labeled; }
        }
    }
}

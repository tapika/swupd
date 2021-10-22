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
using Microsoft.Data.Edm.Values;

namespace Microsoft.Data.Edm.Library.Values
{
    /// <summary>
    /// Represents an EDM floating point constant.
    /// </summary>
    public class EdmFloatingConstant : EdmValue, IEdmFloatingConstantExpression
    {
        private readonly double value;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmFloatingConstant"/> class.
        /// </summary>
        /// <param name="value">Floating point value represented by this value.</param>
        public EdmFloatingConstant(double value)
            : this(null, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmFloatingConstant"/> class.
        /// </summary>
        /// <param name="type">Type of the floating point.</param>
        /// <param name="value">Floating point value represented by this value.</param>
        public EdmFloatingConstant(IEdmPrimitiveTypeReference type, double value)
            : base(type)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the definition of this value.
        /// </summary>
        public double Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Gets the kind of this expression.
        /// </summary>
        public EdmExpressionKind ExpressionKind
        {
            get { return EdmExpressionKind.FloatingConstant; }
        }

        /// <summary>
        /// Gets the kind of this value.
        /// </summary>
        public override EdmValueKind ValueKind
        {
            get { return EdmValueKind.Floating; }
        }
    }
}

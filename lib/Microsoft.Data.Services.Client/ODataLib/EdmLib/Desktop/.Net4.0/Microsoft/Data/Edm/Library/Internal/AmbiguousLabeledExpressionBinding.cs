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

using System;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Values;

namespace Microsoft.Data.Edm.Library.Internal
{
    /// <summary>
    /// Represents a labeled expression binding to more than one item.
    /// </summary>
    internal class AmbiguousLabeledExpressionBinding : AmbiguousBinding<IEdmLabeledExpression>, IEdmLabeledExpression
    {
        private readonly IEdmLabeledExpression first;

        private readonly Cache<AmbiguousLabeledExpressionBinding, IEdmExpression> expressionCache = new Cache<AmbiguousLabeledExpressionBinding, IEdmExpression>();
        private static readonly Func<AmbiguousLabeledExpressionBinding, IEdmExpression> ComputeExpressionFunc = (me) => me.ComputeExpression();

        public AmbiguousLabeledExpressionBinding(IEdmLabeledExpression first, IEdmLabeledExpression second)
            : base(first, second)
        {
            this.first = first;
        }

        public IEdmExpression Expression
        {
            get { return this.expressionCache.GetValue(this, ComputeExpressionFunc, null); }
        }

        public EdmExpressionKind ExpressionKind
        {
            get { return EdmExpressionKind.Labeled; }
        }

        private IEdmExpression ComputeExpression()
        {
            return EdmNullExpression.Instance;
        }
    }
}

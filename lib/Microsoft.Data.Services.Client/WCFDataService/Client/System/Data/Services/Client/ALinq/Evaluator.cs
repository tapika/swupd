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
    using System.Linq.Expressions;

    /// <summary>
    /// performs funcletization on an expression tree
    /// </summary>
    internal static class Evaluator
    {
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="canBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        internal static Expression PartialEval(Expression expression, Func<Expression, bool> canBeEvaluated)
        {
            Nominator nominator = new Nominator(canBeEvaluated);
            HashSet<Expression> candidates = nominator.Nominate(expression);
            return new SubtreeEvaluator(candidates).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        internal static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
        }

        /// <summary>
        /// Evaluates if an expression can be evaluated locally.
        /// </summary>
        /// <param name="expression">the expression.</param>
        /// <returns>true/ false if can be evaluated locally</returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter &&
                expression.NodeType != ExpressionType.Lambda &&
                expression.NodeType != (ExpressionType)ResourceExpressionType.RootResourceSet;
        }

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        internal class SubtreeEvaluator : DataServiceALinqExpressionVisitor
        {
            /// <summary>list of candidates</summary>
            private HashSet<Expression> candidates;

            /// <summary>
            /// constructs an expression evaluator with a list of candidates
            /// </summary>
            /// <param name="candidates">List of expressions to evaluate</param>
            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }

            /// <summary>
            /// Evaluates an expression sub-tree
            /// </summary>
            /// <param name="exp">The expression to evaluate.</param>
            /// <returns>The evaluated expression.</returns>
            internal Expression Eval(Expression exp)
            {
                return this.Visit(exp);
            }

            /// <summary>
            /// Visit method for visitor
            /// </summary>
            /// <param name="exp">the expression to visit</param>
            /// <returns>visited expression</returns>
            internal override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }

                if (this.candidates.Contains(exp))
                {
                    return Evaluate(exp);
                }

                return base.Visit(exp);
            }

            /// <summary>
            /// Evaluates expression
            /// </summary>
            /// <param name="e">the expression to evaluate</param>
            /// <returns>constant expression with return value of evaluation</returns>
            private static Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }

#if ASTORIA_LIGHT
                LambdaExpression lambda = ExpressionHelpers.CreateLambda(e, new ParameterExpression[0]); 
#else
                LambdaExpression lambda = Expression.Lambda(e);
#endif
                Delegate fn = lambda.Compile();
                object constantValue = fn.DynamicInvoke(null);
                Debug.Assert(!(constantValue is Expression), "!(constantValue is Expression)");
                
                // Use the expression type unless it's an array type,
                // where the actual type may be a vector array rather
                // than an array with a dynamic lower bound.
                Type constantType = e.Type;
                if (constantValue != null && constantType.IsArray && constantType.GetElementType() == constantValue.GetType().GetElementType())
                {
                    constantType = constantValue.GetType();
                }

                return Expression.Constant(constantValue, constantType);
            }
        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        internal class Nominator : DataServiceALinqExpressionVisitor
        {
            /// <summary>func to determine whether expression can be evaluated</summary>
            private Func<Expression, bool> functionCanBeEvaluated;

            /// <summary>candidate expressions for evaluation</summary>
            private HashSet<Expression> candidates;

            /// <summary>flag for when sub tree cannot be evaluated</summary>
            private bool cannotBeEvaluated;

            /// <summary>
            /// Creates the Nominator based on the function passed.
            /// </summary>
            /// <param name="functionCanBeEvaluated">
            /// A Func speficying whether an expression can be evaluated or not.
            /// </param>
            /// <returns>visited expression</returns>
            internal Nominator(Func<Expression, bool> functionCanBeEvaluated)
            {
                this.functionCanBeEvaluated = functionCanBeEvaluated;
            }

            /// <summary>
            /// Nominates an expression to see if it can be evaluated
            /// </summary>
            /// <param name="expression">
            /// Expression to check
            /// </param>
            /// <returns>a list of expression sub trees that can be evaluated</returns>
            internal HashSet<Expression> Nominate(Expression expression)
            {
                this.candidates = new HashSet<Expression>(EqualityComparer<Expression>.Default);
                this.Visit(expression);
                return this.candidates;
            }

            /// <summary>
            /// Visit method for walking expression tree bottom up.
            /// </summary>
            /// <param name="expression">
            /// root expression to visit
            /// </param>
            /// <returns>visited expression</returns>
            internal override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;

                    base.Visit(expression);

                    if (!this.cannotBeEvaluated)
                    {
                        if (this.functionCanBeEvaluated(expression))
                        {
                            this.candidates.Add(expression);
                        }
                        else
                        {
                            this.cannotBeEvaluated = true;
                        }
                    }

                    this.cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return expression;
            }
        }
    } 
}

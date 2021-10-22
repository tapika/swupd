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

namespace Microsoft.Data.OData.Query
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Public enumeration of kinds of query nodes. A subset of InternalQueryNodeKind
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "QueryNodeKind is not a flag.")]
    public enum QueryNodeKind
    {
        /// <summary>
        /// No query node kind...  the default value.
        /// </summary>
        None = InternalQueryNodeKind.None,

        /// <summary>
        /// A constant value.
        /// </summary>
        Constant = InternalQueryNodeKind.Constant,

        /// <summary>
        /// A node that represents conversion from one type to another.
        /// </summary>
        Convert = InternalQueryNodeKind.Convert,

        /// <summary>
        /// Non-entity node referencing a range variable.
        /// </summary>
        NonentityRangeVariableReference = InternalQueryNodeKind.NonentityRangeVariableReference,

        /// <summary>
        /// Node used to represent a binary operator.
        /// </summary>
        BinaryOperator = InternalQueryNodeKind.BinaryOperator,

        /// <summary>
        /// Node used to represent a unary operator.
        /// </summary>
        UnaryOperator = InternalQueryNodeKind.UnaryOperator,

        /// <summary>
        /// Node describing access to a property which is a single (non-collection) non-entity value.
        /// </summary>
        SingleValuePropertyAccess = InternalQueryNodeKind.SingleValuePropertyAccess,

        /// <summary>
        /// Node describing access to a property which is a non-entity collection value.
        /// </summary>
        CollectionPropertyAccess = InternalQueryNodeKind.CollectionPropertyAccess,

        /// <summary>
        /// Function call returning a single value.
        /// </summary>
        SingleValueFunctionCall = InternalQueryNodeKind.SingleValueFunctionCall,

        /// <summary>
        /// Any query.
        /// </summary>
        Any = InternalQueryNodeKind.Any,

        /// <summary>
        /// Node for a navigation property with target multiplicity Many.
        /// </summary>
        CollectionNavigationNode = InternalQueryNodeKind.CollectionNavigationNode,

        /// <summary>
        /// Node for a navigation property with target multiplicity ZeroOrOne or One.
        /// </summary>
        SingleNavigationNode = InternalQueryNodeKind.SingleNavigationNode,

        /// <summary>
        /// Single-value property access that refers to an open property.
        /// </summary>
        SingleValueOpenPropertyAccess = InternalQueryNodeKind.SingleValueOpenPropertyAccess,

        /// <summary>
        /// Cast on a single thing.
        /// </summary>
        SingleEntityCast = InternalQueryNodeKind.SingleEntityCast,

        /// <summary>
        /// All query.
        /// </summary>
        All = InternalQueryNodeKind.All,

        /// <summary>
        /// Cast on a collection of entities.
        /// </summary>
        EntityCollectionCast = InternalQueryNodeKind.EntityCollectionCast,

        /// <summary>
        /// Placeholder node referencing a rangeVariable on the binding stack that references an entity.
        /// </summary>
        EntityRangeVariableReference = InternalQueryNodeKind.EntityRangeVariableReference,
        
        /// <summary>
        /// Node the represents a function call that returns a single entity.
        /// </summary>
        SingleEntityFunctionCall = InternalQueryNodeKind.SingleEntityFunctionCall,

        /// <summary>
        /// Node that represents a function call that returns a collection.
        /// </summary>
        CollectionFunctionCall = InternalQueryNodeKind.CollectionFunctionCall,

        /// <summary>
        /// Node that represents a funciton call that returns a collection of entities.
        /// </summary>
        EntityCollectionFunctionCall = InternalQueryNodeKind.EntityCollectionFunctionCall,

        /// <summary>
        /// Node that represents a named function parameter. 
        /// </summary>
        NamedFunctionParameter = InternalQueryNodeKind.NamedFunctionParameter,
    }

    /// <summary>
    /// Internal enumeration of kinds of query nodes. A superset of QueryNodeKind
    /// </summary>
    internal enum InternalQueryNodeKind
    {
        /// <summary>
        /// none... default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// The constant value.
        /// </summary>
        Constant = 1,

        /// <summary>
        /// A node that signifies the promotion of a primitive type.
        /// </summary>
        Convert = 2,

        /// <summary>
        /// Non-entity node referencing a range variable.
        /// </summary>
        NonentityRangeVariableReference = 3,

        /// <summary>
        /// Parameter node used to represent a binary operator.
        /// </summary>
        BinaryOperator = 4,

        /// <summary>
        /// Parameter node used to represent a unary operator.
        /// </summary>
        UnaryOperator = 5,

        /// <summary>
        /// Node describing access to a property which is a single (non-collection) non-entity value.
        /// </summary>
        SingleValuePropertyAccess = 6,

        /// <summary>
        /// Node describing access to a property which is a non-entity collection value.
        /// </summary>
        CollectionPropertyAccess = 7,

        /// <summary>
        /// Function call returning a single value.
        /// </summary>
        SingleValueFunctionCall = 8,

        /// <summary>
        /// Any query.
        /// </summary>
        Any = 9,

        /// <summary>
        /// Node for a navigation property with target multiplicity Many.
        /// </summary>
        CollectionNavigationNode = 10,

        /// <summary>
        /// Node for a navigation property with target multiplicity ZeroOrOne or One.
        /// </summary>
        SingleNavigationNode = 11,

        /// <summary>
        /// Single-value property access that refers to an open property.
        /// </summary>
        SingleValueOpenPropertyAccess = 12,

        /// <summary>
        /// Cast on a single thing.
        /// </summary>
        SingleEntityCast = 13,

        /// <summary>
        /// All query.
        /// </summary>
        All = 14,

        /// <summary>
        /// Cast on a collection.
        /// </summary>
        EntityCollectionCast = 15,

        /// <summary>
        /// Entity  node referencing a range variable.
        /// </summary>
        EntityRangeVariableReference = 16,

        /// <summary>
        /// SingleEntityFunctionCall node.
        /// </summary>
        SingleEntityFunctionCall = 17,
                
        /// <summary>
        /// Node that represents a function call that returns a collection.
        /// </summary>
        CollectionFunctionCall = 18,

        /// <summary>
        /// Node that represents a funciton call that returns a collection of entities.
        /// </summary>
        EntityCollectionFunctionCall = 19,

        /// <summary>
        /// Node that represents a named function parameter.
        /// </summary>
        NamedFunctionParameter = 20,

        /// <summary>
        /// The entity set node.
        /// </summary>
        EntitySet = 21,

        /// <summary>
        /// The key lookup on a collection.
        /// </summary>
        KeyLookup = 22,
    }
}

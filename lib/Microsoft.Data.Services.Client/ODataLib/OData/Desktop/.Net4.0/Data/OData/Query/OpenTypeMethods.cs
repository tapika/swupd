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
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.Data.OData;
    using Edm = Microsoft.Data.Edm;
    #endregion

    /// <summary>Use this class to perform late-bound operations on open properties.</summary>    
    /// <remarks>This class was copied from the product.</remarks>
    internal static class OpenTypeMethods
    {
        #region Reflection OpenType MethodInfos

        /// <summary>MethodInfo for Add.</summary>
        internal static readonly MethodInfo AddMethodInfo = typeof(OpenTypeMethods).GetMethod("Add", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for AndAlso.</summary>
        internal static readonly MethodInfo AndAlsoMethodInfo = typeof(OpenTypeMethods).GetMethod("AndAlso", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Convert.</summary>
        internal static readonly MethodInfo ConvertMethodInfo = typeof(OpenTypeMethods).GetMethod("Convert", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Divide.</summary>
        internal static readonly MethodInfo DivideMethodInfo = typeof(OpenTypeMethods).GetMethod("Divide", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Equal.</summary>
        internal static readonly MethodInfo EqualMethodInfo = typeof(OpenTypeMethods).GetMethod("Equal", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for GreaterThan.</summary>
        internal static readonly MethodInfo GreaterThanMethodInfo = typeof(OpenTypeMethods).GetMethod("GreaterThan", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for GreaterThanOrEqual.</summary>
        internal static readonly MethodInfo GreaterThanOrEqualMethodInfo = typeof(OpenTypeMethods).GetMethod("GreaterThanOrEqual", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for LessThan.</summary>
        internal static readonly MethodInfo LessThanMethodInfo = typeof(OpenTypeMethods).GetMethod("LessThan", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for LessThanOrEqual.</summary>
        internal static readonly MethodInfo LessThanOrEqualMethodInfo = typeof(OpenTypeMethods).GetMethod("LessThanOrEqual", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Modulo.</summary>
        internal static readonly MethodInfo ModuloMethodInfo = typeof(OpenTypeMethods).GetMethod("Modulo", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Multiply.</summary>
        internal static readonly MethodInfo MultiplyMethodInfo = typeof(OpenTypeMethods).GetMethod("Multiply", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Negate.</summary>
        internal static readonly MethodInfo NegateMethodInfo = typeof(OpenTypeMethods).GetMethod("Negate", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Not.</summary>
        internal static readonly MethodInfo NotMethodInfo = typeof(OpenTypeMethods).GetMethod("Not", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for NotEqual.</summary>
        internal static readonly MethodInfo NotEqualMethodInfo = typeof(OpenTypeMethods).GetMethod("NotEqual", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for OrElse.</summary>
        internal static readonly MethodInfo OrElseMethodInfo = typeof(OpenTypeMethods).GetMethod("OrElse", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for Subtract.</summary>
        internal static readonly MethodInfo SubtractMethodInfo = typeof(OpenTypeMethods).GetMethod("Subtract", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for TypeIs.</summary>
        internal static readonly MethodInfo TypeIsMethodInfo = typeof(OpenTypeMethods).GetMethod("TypeIs", BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for object OpenTypeMethods.GetValue(this object value, string propertyName).</summary>
        internal static readonly MethodInfo GetValueOpenPropertyMethodInfo = typeof(OpenTypeMethods).GetMethod(
            "GetValue",
            new Type[] { typeof(object), typeof(string) },
            true /*isPublic*/,
            true /*isStatic*/);

        #endregion Internal fields.

        #region Property Accessor

        /// <summary>Gets a named value from the specified object.</summary>
        /// <param name='value'>Object to get value from.</param>
        /// <param name='propertyName'>Name of property to get.</param>
        /// <returns>The requested value; null if not found.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object GetValue(object value, string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region binary operators

        /// <summary>Adds two values with no overflow checking.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The added value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Add(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Performs logical and of two expressions.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The result of logical and.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object AndAlso(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Divides two values.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The divided value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Divide(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Checks whether two values are equal.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left equals right; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Equal(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Checks whether the left value is greater than the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is greater than right; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object GreaterThan(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Checks whether the left value is greater than or equal to the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is greater than or equal to right; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object GreaterThanOrEqual(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Checks whether the left value is less than the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is less than right; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object LessThan(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Checks whether the left value is less than or equal to the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is less than or equal to right; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object LessThanOrEqual(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Calculates the remainder of dividing the left value by the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The remainder value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Modulo(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Multiplies two values with no overflow checking.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The multiplication value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Multiply(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Checks whether two values are not equal.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is does not equal right; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object NotEqual(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Performs logical or of two expressions.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The result of logical or.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object OrElse(object left, object right)
        {
            throw new NotImplementedException();
        }

        /// <summary>Subtracts the right value from the left value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The subtraction value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Subtract(object left, object right)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region unary operators

        /// <summary>Negates (arithmetically) the specified value.</summary>
        /// <param name='value'>Value.</param>
        /// <returns>The negated value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Negate(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>Negates (logically) the specified value.</summary>
        /// <param name='value'>Value.</param>
        /// <returns>The negated value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Not(object value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Type Conversions

        /// <summary>Performs an type cast on the specified value.</summary>
        /// <param name='value'>Value.</param>
        /// <param name='typeReference'>Type reference to check for.</param>
        /// <returns>Casted value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Convert(object value, Edm.IEdmTypeReference typeReference)
        {
            throw new NotImplementedException();
        }

        /// <summary>Performs an type check on the specified value.</summary>
        /// <param name='value'>Value.</param>
        /// <param name='typeReference'>Type reference to check for.</param>
        /// <returns>True if value is-a type; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object TypeIs(object value, Edm.IEdmTypeReference typeReference)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Canonical functions

        #region String functions

        /// <summary>
        /// Concats the given 2 string.
        /// </summary>
        /// <param name="first">first string.</param>
        /// <param name="second">second string.</param>
        /// <returns>returns a new instance of the concatenated string.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Concat(object first, object second)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks with the parameters are of string type, if no, then they throw.
        /// Otherwise returns true if the target string ends with the given sub string
        /// </summary>
        /// <param name="targetString">target string</param>
        /// <param name="substring">sub string</param>
        /// <returns>Returns true if the target string ends with the given sub string, otherwise return false.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object EndsWith(object targetString, object substring)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the index of the given substring in the target string.
        /// </summary>
        /// <param name="targetString">target string</param>
        /// <param name="substring">sub string to match</param>
        /// <returns>returns the index of the given substring in the target string if present, otherwise returns null.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object IndexOf(object targetString, object substring)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the length of the given string value. If the value is not of string type, then it throws.
        /// </summary>
        /// <param name="value">value whose length needs to be calculated.</param>
        /// <returns>length of the string value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Length(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replaces the given substring with the new string in the target string.
        /// </summary>
        /// <param name="targetString">target string</param>
        /// <param name="substring">substring to be replaced.</param>
        /// <param name="newString">new string that replaces the sub string.</param>
        /// <returns>returns a new string with the substring replaced with new string.</returns> 
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Replace(object targetString, object substring, object newString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether the target string starts with the substring.
        /// </summary>
        /// <param name="targetString">target string.</param>
        /// <param name="substring">substring</param>
        /// <returns>returns true if the target string starts with the given sub string, otherwise returns false.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object StartsWith(object targetString, object substring)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the substring given the starting index
        /// </summary>
        /// <param name="targetString">target string</param>
        /// <param name="startIndex">starting index for the substring.</param>
        /// <returns>the substring given the starting index.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Substring(object targetString, object startIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the substring from the target string.
        /// </summary>
        /// <param name="targetString">target string.</param>
        /// <param name="startIndex">starting index for the substring.</param>
        /// <param name="length">length of the substring.</param>
        /// <returns>Returns the substring given the starting index and length.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Substring(object targetString, object startIndex, object length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether the given string is a substring of the target string.
        /// </summary>
        /// <param name="substring">substring to check for.</param>
        /// <param name="targetString">target string.</param>
        /// <returns>returns true if the target string contains the substring, otherwise returns false.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object SubstringOf(object substring, object targetString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a copy of the target string converted to lowercase.
        /// </summary>
        /// <param name="targetString">target string</param>
        /// <returns>a new string instance with everything in lowercase.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        [SuppressMessage("Microsoft.Globalization", "CA1308", Justification = "Need to support ToLower function")]
        public static object ToLower(object targetString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a copy of the target string converted to uppercase.
        /// </summary>
        /// <param name="targetString">target string</param>
        /// <returns>a new string instance with everything in uppercase.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object ToUpper(object targetString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the target string. 
        /// </summary>
        /// <param name="targetString">target string.</param>
        /// <returns>returns the trimed string.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Trim(object targetString)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Datetime functions

        /// <summary>
        /// Returns the year value of the given datetime.
        /// </summary>
        /// <param name="dateTime">datetime object.</param>
        /// <returns>returns the year value of the given datetime.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Year(object dateTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the month value of the given datetime.
        /// </summary>
        /// <param name="dateTime">datetime object.</param>
        /// <returns>returns the month value of the given datetime.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Month(object dateTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the day value of the given datetime.
        /// </summary>
        /// <param name="dateTime">datetime object.</param>
        /// <returns>returns the day value of the given datetime.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Day(object dateTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the hour value of the given datetime.
        /// </summary>
        /// <param name="dateTime">datetime object.</param>
        /// <returns>returns the hour value of the given datetime.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Hour(object dateTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the minute value of the given datetime.
        /// </summary>
        /// <param name="dateTime">datetime object.</param>
        /// <returns>returns the minute value of the given datetime.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Minute(object dateTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the second value of the given datetime.
        /// </summary>
        /// <param name="dateTime">datetime object.</param>
        /// <returns>returns the second value of the given datetime.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Second(object dateTime)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Numeric functions

        /// <summary>
        /// Returns the ceiling of the given value
        /// </summary>
        /// <param name="value">decimal or double object.</param>
        /// <returns>returns the ceiling value for the given double or decimal value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Ceiling(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// returns the floor of the given value.
        /// </summary>
        /// <param name="value">decimal or double object.</param>
        /// <returns>returns the floor value for the given double or decimal value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Floor(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rounds the given value.
        /// </summary>
        /// <param name="value">decimal or double object.</param>
        /// <returns>returns the round value for the given double or decimal value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Parameters will be used in the actual impl")]
        public static object Round(object value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Factory methods for expression tree nodes.

        /// <summary>Creates an expression that adds two values with no overflow checking.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The added value.</returns>
        internal static Expression AddExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Add(
                    ExpressionAsObject(left),
                    ExpressionAsObject(right), 
                    AddMethodInfo);
        }

        /// <summary>Creates a call expression that represents a conditional AND operation that evaluates the second operand only if it has to.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The conditional expression; null if the expressions aren't of the right type.</returns>
        internal static Expression AndAlsoExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Call(
                    OpenTypeMethods.AndAlsoMethodInfo, 
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right)); 
        }

        /// <summary>Creates an expression that divides two values.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The divided value.</returns>
        internal static Expression DivideExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Divide(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    DivideMethodInfo);
        }

        /// <summary>Creates an expression that checks whether two values are equal.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left equals right; false otherwise.</returns>
        internal static Expression EqualExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Equal(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    false, 
                    EqualMethodInfo);
        }

        /// <summary>Creates an expression that checks whether the left value is greater than the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is greater than right; false otherwise.</returns>
        internal static Expression GreaterThanExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.GreaterThan(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    false, 
                    GreaterThanMethodInfo);
        }

        /// <summary>Creates an expression that checks whether the left value is greater than or equal to the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is greater than or equal to right; false otherwise.</returns>
        internal static Expression GreaterThanOrEqualExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.GreaterThanOrEqual(
                    ExpressionAsObject(left),
                    ExpressionAsObject(right), 
                    false, 
                    GreaterThanOrEqualMethodInfo);
        }

        /// <summary>Creates an expression that checks whether the left value is less than the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is less than right; false otherwise.</returns>
        internal static Expression LessThanExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.LessThan(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    false, 
                    LessThanMethodInfo);
        }

        /// <summary>Creates an expression that checks whether the left value is less than or equal to the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is less than or equal to right; false otherwise.</returns>
        internal static Expression LessThanOrEqualExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.LessThanOrEqual(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    false, 
                    LessThanOrEqualMethodInfo);
        }

        /// <summary>Creates an expression that calculates the remainder of dividing the left value by the right value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The remainder value.</returns>
        internal static Expression ModuloExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Modulo(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    ModuloMethodInfo);
        }

        /// <summary>Creates an expression that multiplies two values with no overflow checking.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The multiplication value.</returns>
        internal static Expression MultiplyExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Multiply(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    MultiplyMethodInfo);
        }

        /// <summary>Creates a call expression that represents a conditional OR operation that evaluates the second operand only if it has to.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The conditional expression; null if the expressions aren't of the right type.</returns>
        internal static Expression OrElseExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Call(
                    OpenTypeMethods.OrElseMethodInfo, 
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right)); 
        }

        /// <summary>Creates an expression that checks whether two values are not equal.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>true if left is does not equal right; false otherwise.</returns>
        internal static Expression NotEqualExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.NotEqual(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    false, 
                    NotEqualMethodInfo);
        }

        /// <summary>Creates an expression that subtracts the right value from the left value.</summary>
        /// <param name='left'>Left value.</param><param name='right'>Right value.</param>
        /// <returns>The subtraction value.</returns>
        internal static Expression SubtractExpression(Expression left, Expression right)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Subtract(
                    ExpressionAsObject(left), 
                    ExpressionAsObject(right), 
                    SubtractMethodInfo);
        }

        /// <summary>Creates an expression that negates (arithmetically) the specified value.</summary>
        /// <param name='expression'>Value expression.</param>
        /// <returns>The negated value.</returns>
        internal static Expression NegateExpression(Expression expression)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Negate(
                    ExpressionAsObject(expression), 
                    NegateMethodInfo);
        }

        /// <summary>Creates an expression that negates (logically) the specified value.</summary>
        /// <param name='expression'>Value expression.</param>
        /// <returns>The negated value.</returns>
        internal static Expression NotExpression(Expression expression)
        {
            DebugUtils.CheckNoExternalCallers();
            return Expression.Not(
                    ExpressionAsObject(expression), 
                    NotMethodInfo);
        }

        #endregion Factory methods for expression tree nodes.

        #region Helper methods

        /// <summary>
        /// Returns the specified <paramref name="expression"/> with a 
        /// type assignable to System.Object.
        /// </summary>
        /// <param name="expression">Expression to convert.</param>
        /// <returns>
        /// The specified <paramref name="expression"/> with a type assignable 
        /// to System.Object.
        /// </returns>
        private static Expression ExpressionAsObject(Expression expression)
        {
            Debug.Assert(expression != null, "expression != null");
            return expression.Type.IsValueType() ? Expression.Convert(expression, typeof(object)) : expression;
        }

        #endregion
    }
}

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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Data.Edm;
    #endregion

    /// <summary>Use this class to perform late-bound operations on data service entity sets.</summary>
    /// <remarks>This class was copied from the product.</remarks>
    internal static class DataServiceProviderMethods
    {
        #region Internal MethodInfos

        /// <summary>MethodInfo for object DataServiceProviderMethods.GetValue(this object value, string propertyName).</summary>
        internal static readonly MethodInfo GetValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "GetValue",
            new Type[] { typeof(object), typeof(IEdmProperty) },
            true /*isPublic*/,
            true /*isStatic*/);

        /// <summary>MethodInfo for IEnumerable&lt;T&gt; DataServiceProviderMethods.GetSequenceValue(this object value, string propertyName).</summary>
        internal static readonly MethodInfo GetSequenceValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "GetSequenceValue",
            new Type[] { typeof(object), typeof(IEdmProperty) },
            true /*isPublic*/,
            true /*isStatic*/);

        /// <summary>MethodInfo for Convert.</summary>
        internal static readonly MethodInfo ConvertMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "Convert",
            BindingFlags.Static | BindingFlags.Public);

        /// <summary>MethodInfo for TypeIs.</summary>
        internal static readonly MethodInfo TypeIsMethodInfo = typeof(DataServiceProviderMethods).GetMethod(
            "TypeIs",
            BindingFlags.Static | BindingFlags.Public);

        /// <summary>Method info for string comparison</summary>
        internal static readonly MethodInfo StringCompareMethodInfo = typeof(DataServiceProviderMethods)
                                                              .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                              .Single(m => m.Name == "Compare" && m.GetParameters()[0].ParameterType == typeof(string));

        /// <summary>Method info for Bool comparison</summary>
        internal static readonly MethodInfo BoolCompareMethodInfo = typeof(DataServiceProviderMethods)
                                                              .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                              .Single(m => m.Name == "Compare" && m.GetParameters()[0].ParameterType == typeof(bool));

        /// <summary>Method info for Bool? comparison</summary>
        internal static readonly MethodInfo BoolCompareMethodInfoNullable = typeof(DataServiceProviderMethods)
                                                              .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                              .Single(m => m.Name == "Compare" && m.GetParameters()[0].ParameterType == typeof(bool?));

        /// <summary>Method info for Guid comparison</summary>
        internal static readonly MethodInfo GuidCompareMethodInfo = typeof(DataServiceProviderMethods)
                                                              .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                              .Single(m => m.Name == "Compare" && m.GetParameters()[0].ParameterType == typeof(Guid));

        /// <summary>Method info for Guid? comparison</summary>
        internal static readonly MethodInfo GuidCompareMethodInfoNullable = typeof(DataServiceProviderMethods)
                                                              .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                              .Single(m => m.Name == "Compare" && m.GetParameters()[0].ParameterType == typeof(Guid?));

        /// <summary>Method info for byte array comparison.</summary>
        internal static readonly MethodInfo AreByteArraysEqualMethodInfo = typeof(DataServiceProviderMethods)
                                                              .GetMethod("AreByteArraysEqual", BindingFlags.Public | BindingFlags.Static);

        /// <summary>Method info for byte array comparison.</summary>
        internal static readonly MethodInfo AreByteArraysNotEqualMethodInfo = typeof(DataServiceProviderMethods)
                                                              .GetMethod("AreByteArraysNotEqual", BindingFlags.Public | BindingFlags.Static);

        #endregion

        #region GetValue, GetSequenceValue

        /// <summary>Gets a named value from the specified object.</summary>
        /// <param name='value'>Object to get value from.</param>
        /// <param name='property'><see cref="IEdmProperty"/> describing the property whose value needs to be fetched.</param>
        /// <returns>The requested value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "rangeVariables will be used in the actual impl")]
        public static object GetValue(object value, IEdmProperty property)
        {
            throw new NotImplementedException();
        }

        /// <summary>Gets a named value from the specified object as a sequence.</summary>
        /// <param name='value'>Object to get value from.</param>
        /// <param name='property'><see cref="IEdmProperty"/> describing the property whose value needs to be fetched.</param>
        /// <typeparam name="T">expected result type</typeparam>
        /// <returns>The requested value as a sequence; null if not found.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "rangeVariables will be used in the actual impl")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "need T for proper binding to collections")]
        public static IEnumerable<T> GetSequenceValue<T>(object value, IEdmProperty property)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Type Conversions

        /// <summary>Performs an type cast on the specified value.</summary>
        /// <param name='value'>Value.</param>
        /// <param name='typeReference'>Type reference to check for.</param>
        /// <returns>Casted value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "rangeVariables will be used in the actual impl")]
        public static object Convert(object value, IEdmTypeReference typeReference)
        {
            throw new NotImplementedException();
        }

        /// <summary>Performs an type check on the specified value.</summary>
        /// <param name='value'>Value.</param>
        /// <param name='typeReference'>Type reference type to check for.</param>
        /// <returns>True if value is-a type; false otherwise.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "rangeVariables will be used in the actual impl")]
        public static bool TypeIs(object value, IEdmTypeReference typeReference)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Type Comparers

        /// <summary>
        /// Compares 2 strings by ordinal, used to obtain MethodInfo for comparison operator expression parameter
        /// </summary>
        /// <param name="left">Left Parameter</param>
        /// <param name="right">Right Parameter</param>
        /// <returns>0 for equality, -1 for left less than right, 1 for left greater than right</returns>
        /// <remarks>
        /// Do not change the name of this function because LINQ to SQL is sensitive about the 
        /// method name, so is EF probably.
        /// </remarks>
        public static int Compare(String left, String right)
        {
            return Comparer<string>.Default.Compare(left, right);
        }

        /// <summary>
        /// Compares 2 booleans with true greater than false, used to obtain MethodInfo for comparison operator expression parameter
        /// </summary>
        /// <param name="left">Left Parameter</param>
        /// <param name="right">Right Parameter</param>
        /// <returns>0 for equality, -1 for left less than right, 1 for left greater than right</returns>
        /// <remarks>
        /// Do not change the name of this function because LINQ to SQL is sensitive about the 
        /// method name, so is EF probably.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "Need implementation")]
        public static int Compare(bool left, bool right)
        {
            return Comparer<bool>.Default.Compare(left, right);
        }

        /// <summary>
        /// Compares 2 nullable booleans with true greater than false, used to obtain MethodInfo for comparison operator expression parameter
        /// </summary>
        /// <param name="left">Left Parameter</param>
        /// <param name="right">Right Parameter</param>
        /// <returns>0 for equality, -1 for left less than right, 1 for left greater than right</returns>
        /// <remarks>
        /// Do not change the name of this function because LINQ to SQL is sensitive about the 
        /// method name, so is EF probably.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "Need implementation")]
        public static int Compare(bool? left, bool? right)
        {
            return Comparer<bool?>.Default.Compare(left, right);
        }

        /// <summary>
        /// Compares 2 guids by byte order, used to obtain MethodInfo for comparison operator expression parameter
        /// </summary>
        /// <param name="left">Left Parameter</param>
        /// <param name="right">Right Parameter</param>
        /// <returns>0 for equality, -1 for left less than right, 1 for left greater than right</returns>
        /// <remarks>
        /// Do not change the name of this function because LINQ to SQL is sensitive about the 
        /// method name, so is EF probably.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "Need implementation")]
        public static int Compare(Guid left, Guid right)
        {
            return Comparer<Guid>.Default.Compare(left, right);
        }

        /// <summary>
        /// Compares 2 nullable guids by byte order, used to obtain MethodInfo for comparison operator expression parameter
        /// </summary>
        /// <param name="left">Left Parameter</param>
        /// <param name="right">Right Parameter</param>
        /// <returns>0 for equality, -1 for left less than right, 1 for left greater than right</returns>
        /// <remarks>
        /// Do not change the name of this function because LINQ to SQL is sensitive about the 
        /// method name, so is EF probably.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "Need implementation")]
        public static int Compare(Guid? left, Guid? right)
        {
            return Comparer<Guid?>.Default.Compare(left, right);
        }

        /// <summary>Compares two byte arrays for equality.</summary>
        /// <param name="left">First byte array.</param>
        /// <param name="right">Second byte array.</param>
        /// <returns>true if the arrays are equal; false otherwise.</returns>
        public static bool AreByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == right)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares two byte arrays for equality.</summary>
        /// <param name="left">First byte array.</param>
        /// <param name="right">Second byte array.</param>
        /// <returns>true if the arrays are not equal; false otherwise.</returns>
        public static bool AreByteArraysNotEqual(byte[] left, byte[] right)
        {
            return !AreByteArraysEqual(left, right);
        }
        #endregion
    }
}

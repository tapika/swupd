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

using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

#if ASTORIA_CLIENT
namespace System.Data.Services.Client
#else
#if SPATIAL
namespace System.Spatial
#else
#if ODATALIB || ODATALIB_QUERY
namespace Microsoft.Data.OData
#else
namespace Microsoft.Data.Edm
#endif
#endif
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
#if PORTABLELIB
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
#endif
    using System.Reflection;
#if WINRT
    using System.Threading;
#endif
    using System.Xml;

#if WINRT

    #region Missing enums
    /// <summary>
    /// Replacement for TypeCode enum.
    /// </summary>
    internal enum TypeCode
    {
        /// <summary>Indicates that no specific TypeCode exists for this type.</summary>
        Object = 1,

        /// <summary>Boolean</summary>
        Boolean = 3,

        /// <summary>Char</summary>
        Char = 4,

        /// <summary>Signed 8-bit integer</summary>
        SByte = 5,

        /// <summary>Unsigned 8-bit integer</summary>
        Byte = 6,

        /// <summary>Signed 16-bit integer</summary>
        Int16 = 7,

        /// <summary>Unsigned 16-bit integer</summary>
        UInt16 = 8,

        /// <summary>Signed 32-bit integer</summary>
        Int32 = 9,

        /// <summary>Unsigned 32-bit integer</summary>
        UInt32 = 10,

        /// <summary>Signed 64-bit integer</summary>
        Int64 = 11,

        /// <summary>Unsigned 64-bit integer</summary>
        UInt64 = 12,

        /// <summary>IEEE 32-bit float</summary>
        Single = 13,

        /// <summary>IEEE 64-bit double</summary>
        Double = 14,

        /// <summary>Decimal</summary>
        Decimal = 15,

        /// <summary>DateTime</summary>
        DateTime = 16,

        /// <summary>Unicode character string</summary>
        String = 18,
    }

    #endregion

    [Flags]
    internal enum BindingFlags
    {
        /// <summary>Specifies that the case of the member name should not be considered when binding.</summary>
        IgnoreCase = 1,

        /// <summary>Specifies that only members declared at the level of the supplied type's hierarchy should be
        ///  considered. Inherited members are not considered.</summary>
        DeclaredOnly = 2,

        /// <summary>Specifies that instance members are to be included in the search.</summary>
        Instance = 4,

        /// <summary>Specifies that static members are to be included in the search.</summary>
        Static = 8,

        /// <summary>Specifies that public members are to be included in the search.</summary>
        Public = 16,

        /// <summary>Specifies that non-public members are to be included in the search.</summary>
        NonPublic = 32,

        /// <summary>Specifies that public and protected static members up the hierarchy should
        /// be returned. Private static members in inherited classes are not returned.
        /// Static members include fields, methods, events, and properties. Nested types are not returned.</summary>
        FlattenHierarchy = 64,
       
        /// <summary>Specifies that types of the supplied arguments must exactly match the types
        /// of the corresponding formal parameters. Reflection throws an exception if
        /// the caller supplies a non-null Binder object, since that implies that the
        /// caller is supplying BindToXXX implementations that will pick the appropriate method.</summary>
        ExactBinding = 65536,

        /// <summary>Returns the set of members whose parameter count matches the number of supplied
        /// arguments. This binding flag is used for methods with parameters that have
        /// default values and methods with variable arguments (varargs). This flag should
        /// only be used with System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[],System.Reflection.ParameterModifier[],System.Globalization.CultureInfo,System.String[]).
        /// </summary>
        OptionalParamBinding = 262144,
    }
#endif

    /// <summary>
    /// Helper methods that provide a common API surface on all platforms.
    /// </summary>
    internal static class PlatformHelper
    {
        /// <summary>
        /// Use this instead of Type.EmptyTypes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static readonly Type[] EmptyTypes = new Type[0];

#if PORTABLELIB
        /// <summary>
        /// Replacement for Uri.UriSchemeHttp, which does not exist on.
        /// </summary>
        internal static readonly string UriSchemeHttp = "http";

        /// <summary>
        /// Replacement for Uri.UriSchemeHttps, which does not exist on.
        /// </summary>
        internal static readonly string UriSchemeHttps = "https";
#else
        /// <summary>
        /// Use this instead of Uri.UriSchemeHttp.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static readonly string UriSchemeHttp = Uri.UriSchemeHttp;

        /// <summary>
        /// Use this instead of Uri.UriSchemeHttps.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static readonly string UriSchemeHttps = Uri.UriSchemeHttps;
#endif

#if WINRT
        /// <summary>
        /// Map of TypeCodes used with GetTypeCode method. Only initialized if that method is called.
        /// </summary>
        private static TypeCodeMap typeCodeMap;
#endif

        #region Helper methods for properties

        /// <summary>
        /// Replacement for Type.Assembly.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static Assembly GetAssembly(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsValueType.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsValueType(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsGenericParameter.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsGenericParameter(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.IsGenericParameter;
        }

        /// <summary>
        /// Replacement for Type.IsAbstract.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsAbstract(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsGenericType.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsGenericType(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsGenericTypeDefinition.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsGenericTypeDefinition(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsGenericTypeDefinition;
#else
            return type.IsGenericTypeDefinition;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsVisible.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsVisible(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsVisible;
#else
            return type.IsVisible;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsInterface.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsInterface(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsClass.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsClass(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        /// <summary>
        /// Replacement for Type.IsEnum.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsEnum(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        /// <summary>
        /// Replacement for Type.BaseType.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static Type GetBaseType(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        /// <summary>
        /// Replacement for Type.ContainsGenericParameters.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool ContainsGenericParameters(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().ContainsGenericParameters;
#else
            return type.ContainsGenericParameters;
#endif
        }

        #endregion

        #region Helper methods for static methods

        /// <summary>
        /// Replacement for Array.AsReadOnly(T[]).
        /// </summary>
        /// <typeparam name="T">Type of items in the array.</typeparam>
        /// <param name="array">Array to use to create the ReadOnlyCollection.</param>
        /// <returns>ReadOnlyCollection containing the specified array items.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if PORTABLELIB
            return new ReadOnlyCollection<T>(array);
#else
            return Array.AsReadOnly(array);
#endif
        }

        /// <summary>
        /// Converts a string to a DateTime.
        /// </summary>
        /// <param name="text">String to be converted.</param>
        /// <returns>See documentation for method being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static DateTime ConvertStringToDateTime(string text)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif

            // Workaround for XmlConvert.ToDateTime issue which is not able to convert string without seconds to DateTime, So the seconds part is added as zeros.
            text = AddSecondsPaddingIfMissing(text);
#if PORTABLELIB
            // DateTimeOffset always applies an offset (using the local one if one is not present in the input), but the old XmlConvert methods
            // would produce a DateTime value with InternalKind=DateTimeKind.Unspecified and no offset applied if none was specified in the input string.
            // Before we convert to DateTimeOffset, we need to determine what kind of input we have so we can still produce the same kind of DateTime
            // instances that we would have gotten on other platforms with XmlConvert.ToDateTime.
            // 
            // The XML DateTime pattern is described here: http://www.w3.org/TR/xmlschema-2/#dateTime
            // For example:
            //      No timezone specified: "2012-12-21T15:01:23.1234567"
            //      UTC timezone: "2012-12-21T15:01:23.1234567Z"
            //      Timezone offset from UTC: "2012-12-21T15:01:23.1234567-08:00" or "2012-12-21T15:01:23.1234567+08:00"
            // If timezone is specified, the indicator will always be at the same place from the end of the string as in the examples above, so we can look there for the Z or +/-.
            DateTimeKind inputKind;
            const int TimeZoneSignOffset = 6;
            if (text[text.Length - 1] == 'Z')
            {
                inputKind = DateTimeKind.Utc;
            }
            else if (text[text.Length - TimeZoneSignOffset] == '-' || text[text.Length - TimeZoneSignOffset] == '+')
            {
                inputKind = DateTimeKind.Local;
            }
            else
            {
                // To prevent ToDateTimeOffset from applying the local offset in this case, we will append the Z to indicate UTC time
                inputKind = DateTimeKind.Unspecified;
                text = text + "Z";
            }

            var dateTimeOffset = XmlConvert.ToDateTimeOffset(text);
            switch (inputKind)
            {
                case DateTimeKind.Local:
                    return dateTimeOffset.LocalDateTime;
                case DateTimeKind.Utc:
                    return dateTimeOffset.UtcDateTime;
                default:
                    Debug.Assert(inputKind == DateTimeKind.Unspecified, "All dates must be Utc, Local, or Unspecified.");
                    return dateTimeOffset.DateTime;
            }
#else
            return XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.RoundtripKind);
#endif
        }

        /// <summary>
        /// Converts a string to a DateTimeOffset.
        /// </summary>
        /// <param name="text">String to be converted.</param>
        /// <returns>See documentation for method being accessed in the body of the method.</returns>
        internal static DateTimeOffset ConvertStringToDateTimeOffset(string text)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            text = AddSecondsPaddingIfMissing(text);
            return XmlConvert.ToDateTimeOffset(text);
        }

        /// <summary>
        /// Adds the seconds padding as zeros to the date time string if seconds part is missing.
        /// </summary>
        /// <param name="text">String that needs seconds padding</param>
        /// <returns>DateTime string after adding seconds padding</returns>
        internal static string AddSecondsPaddingIfMissing(string text)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            int indexOfT = text.IndexOf("T", System.StringComparison.Ordinal);
            const int ColonBeforeSecondsOffset = 6;
            int indexOfColonBeforeSeconds = indexOfT + ColonBeforeSecondsOffset;

            // check if the string is in the format of yyyy-mm-ddThh:mm or in the format of yyyy-mm-ddThh:mm[- or +]hh:mm 
            if (indexOfT > 0 && (text.Length <= indexOfColonBeforeSeconds || text[indexOfColonBeforeSeconds] != ':'))
            {
                text = text.Insert(indexOfColonBeforeSeconds, ":00");
            }
            
            return text;
        }

        /// <summary>
        /// Converts the DateTime to a string, internal method.
        /// </summary>
        /// <param name="dateTime">DateTime to convert to String.</param>
        /// <returns>Converted String.</returns>
        internal static string ConvertDateTimeToStringInternal(DateTime dateTime)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                // If we just cast to DateTimeOffset here, the local offset will be applied, which can alter the meaning of the value.
                // Instead we need to create a new DateTimeOffset with the timezone explicitly set to UTC, which will prevent
                // any offset from being used. The resulting string does have the Z on it in that case, but we want to leave the timezone
                // unspecified here, so we will just remove that.
                DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);
                string outputWithZ = XmlConvert.ToString(dateTimeOffset);
                Debug.Assert(outputWithZ[outputWithZ.Length - 1] == 'Z', "Expected DateTimeOffset to be a UTC value.");
                return outputWithZ.TrimEnd('Z');
            }
            else
            {
                // For Utc and Local kinds, ToString produces the same string that the old XmlConvert methods would produce.
                return XmlConvert.ToString((DateTimeOffset)dateTime);
            }    
        }

        /// <summary>
        /// Converts a DateTime to a string.
        /// </summary>
        /// <param name="dateTime">DateTime to be converted.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static string ConvertDateTimeToString(DateTime dateTime)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if PORTABLELIB
            return ConvertDateTimeToStringInternal(dateTime);
#else
            return XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.RoundtripKind);
#endif
        }

        /// <summary>
        /// Gets the specified type.
        /// </summary>
        /// <param name="typeName">Name of the type to get.</param>
        /// <exception cref="TypeLoadException">Throws if the type could not be found.</exception>
        /// <returns>Type instance that represents the specified type name.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static Type GetTypeOrThrow(string typeName)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return Type.GetType(typeName, true);
        }

        /// <summary>
        /// Gets the TypeCode for the specified type.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>TypeCode representing the specified type.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static TypeCode GetTypeCode(Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            if (typeCodeMap == null)
            {
                Interlocked.CompareExchange(ref typeCodeMap, new TypeCodeMap(), null);
            }

            return typeCodeMap.GetTypeCode(type);
#else
            return Type.GetTypeCode(type);
#endif
        }

        #endregion

        #region Methods to replace other changed functionality where the replacement doesn't map exactly to an existing method on other platforms

        /// <summary>
        /// Gets the Unicode Category of the specified character.
        /// </summary>
        /// <param name="c">Character to get category of.</param>
        /// <returns>Category of the character.</returns>
        internal static UnicodeCategory GetUnicodeCategory(Char c)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // Portable Library platform doesn't have Char.GetUnicodeCategory, its on CharUnicodeInfo instead.
#if PORTABLELIB
            return CharUnicodeInfo.GetUnicodeCategory(c);
#else
            return Char.GetUnicodeCategory(c);
#endif
        }

        /// <summary>
        /// Replacement for usage of MemberInfo.MemberType property.
        /// </summary>
        /// <param name="member">MemberInfo on which to access this method.</param>
        /// <returns>True if the specified member is a property, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsProperty(MemberInfo member)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if PORTABLELIB
            return member is PropertyInfo;
#else
            return member.MemberType == MemberTypes.Property;
#endif
        }

        /// <summary>
        /// Replacement for usage of Type.IsPrimitive property.
        /// </summary>
        /// <param name="type">Type on which to access this method.</param>
        /// <returns>True if the specified type is primitive, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsPrimitive(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        /// <summary>
        /// Replacement for usage of Type.IsSealed property.
        /// </summary>
        /// <param name="type">Type on which to access this method.</param>
        /// <returns>True if the specified type is sealed, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsSealed(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().IsSealed;
#else
            return type.IsSealed;
#endif
        }

        /// <summary>
        /// Replacement for usage of MemberInfo.MemberType property.
        /// </summary>
        /// <param name="member">MemberInfo on which to access this method.</param>
        /// <returns>True if the specified member is a method, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool IsMethod(MemberInfo member)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if PORTABLELIB
            return member is MethodInfo;
#else
            return member.MemberType == MemberTypes.Method;
#endif
        }

        /// <summary>
        /// Compares two methodInfos and returns true if they represent the same method.
        /// Need this for Windows Phone as the method Infos of the same method are not always instance equivalent.
        /// </summary>
        /// <param name="member1">MemberInfo to compare.</param>
        /// <param name="member2">MemberInfo to compare.</param>
        /// <returns>True if the specified member is a method, otherwise false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static bool AreMembersEqual(MemberInfo member1, MemberInfo member2)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if PORTABLELIB
            return member1 == member2;
#else 
            return member1.MetadataToken == member2.MetadataToken; 
#endif
        }

        /// <summary>
        /// Gets public properties for the specified type.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="instanceOnly">True if method should return only instance properties, false if it should return both instance and static properties.</param>
        /// <returns>Enumerable of public properties for the type.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static IEnumerable<PropertyInfo> GetPublicProperties(this Type type, bool instanceOnly)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return GetPublicProperties(type, instanceOnly, false /*declaredOnly*/);
        }

        /// <summary>
        /// Gets public properties for the specified type.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="instanceOnly">True if method should return only instance properties, false if it should return both instance and static properties.</param>
        /// <param name="declaredOnly">True if method should return only properties that are declared on the type, false if it should return properties declared on the type as well as those inherited from any base types.</param>
        /// <returns>Enumerable of public properties for the type.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static IEnumerable<PropertyInfo> GetPublicProperties(this Type type, bool instanceOnly, bool declaredOnly)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // WINRT: The BindingFlags enum and all related reflection method overloads have been removed from WINRT. Instead of trying to provide
            // a general purpose flags enum and methods that can take any combination of the flags, we provide more restrictive methods that
            // still allow for the same functionality as needed by the calling code.
#if WINRT
            // TypeInfo.DeclaredProperties and Type.GetRuntimeProperties return both public and private properties, so need to filter out only public ones.
            IEnumerable<PropertyInfo> properties = declaredOnly ? type.GetTypeInfo().DeclaredProperties : type.GetRuntimeProperties();
            return properties.Where(p => IsPublic(p) && (!instanceOnly || IsInstance(p)));
#else
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (!instanceOnly)
            {
                bindingFlags |= BindingFlags.Static;
            }

            if (declaredOnly)
            {
                bindingFlags |= BindingFlags.DeclaredOnly;
            }

            return type.GetProperties(bindingFlags);
#endif
        }

        /// <summary>
        /// Gets instance constructors for the specified type.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="isPublic">True if method should return only public constructors, false if it should return only non-public constructors.</param>
        /// <returns>Enumerable of instance constructors for the specified type.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static IEnumerable<ConstructorInfo> GetInstanceConstructors(this Type type, bool isPublic)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && isPublic == c.IsPublic);
#else
            BindingFlags bindingFlags = BindingFlags.Instance;
            bindingFlags |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            return type.GetConstructors(bindingFlags);
#endif
        }

        /// <summary>
        /// Gets a instance constructor for the type that takes the specified argument types.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="isPublic">True if method should search only public constructors, false if it should search only non-public constructors.</param>
        /// <param name="argTypes">Array of argument types for the constructor.</param>
        /// <returns>ConstructorInfo for the constructor with the specified characteristics if found, otherwise null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static ConstructorInfo GetInstanceConstructor(this Type type, bool isPublic, Type[] argTypes)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if PORTABLELIB
            return GetInstanceConstructors(type, isPublic).SingleOrDefault(c => CheckTypeArgs(c, argTypes));
#endif
#if !PORTABLELIB
            BindingFlags bindingFlags = BindingFlags.Instance;
            bindingFlags |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            return type.GetConstructor(bindingFlags, null, argTypes, null);
#endif
        }

        /// <summary>
        /// Tries to the get method from the type, returns null if not found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>Returns True if found.</returns>
        internal static bool TryGetMethod(this Type type, string name, Type[] parameterTypes, out MethodInfo foundMethod)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            foundMethod = null;
            try
            {
                foundMethod = type.GetMethod(name, parameterTypes);
                return foundMethod != null;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a method on the specified type.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Name of the method on the type.</param>
        /// <param name="isPublic">True if method should search only public methods, false if it should search only non-public methods.</param>
        /// <param name="isStatic">True if method should search only static methods, false if it should search only instance methods.</param>
        /// <returns>MethodInfo for the method with the specified characteristics if found, otherwise null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static MethodInfo GetMethod(this Type type, string name, bool isPublic, bool isStatic)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // WINRT: The BindingFlags enum and all related reflection method overloads have been removed from Win8. Instead of trying to provide
            // a general purpose flags enum and methods that can take any combination of the flags, we provide more restrictive methods that
            // still allow for the same functionality as needed by the calling code.
#if WINRT
            return type.GetRuntimeMethods()
                .Where(
                    m =>
                        m.Name == name &&
                        isPublic == m.IsPublic &&
                        isStatic == m.IsStatic)
                .SingleOrDefault();
#else
            // PortableLib: The BindingFlags enum and all related reflection method overloads have been removed from . Instead of trying to provide
            // a general purpose flags enum and methods that can take any combination of the flags, we provide more restrictive methods that
            // still allow for the same functionality as needed by the calling code.
           
#if PORTABLELIB
            BindingFlags bindingFlags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingFlags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            return type.GetMethod(name, bindingFlags);
#endif
#if !PORTABLELIB
            BindingFlags bindingFlags = BindingFlags.Default;
            bindingFlags |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingFlags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            return type.GetMethod(name, bindingFlags);
#endif
#endif
        }

#if WINRT
        internal static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingAttr)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            var isPublic = bindingAttr.HasFlag(BindingFlags.Public);
            var isStatic = bindingAttr.HasFlag(BindingFlags.Static);
            return type.GetMethod(name, isPublic, isStatic);
        }

        internal static MethodInfo[] GetMethods(this Type type, BindingFlags bindingAttr)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            var isPublic = bindingAttr.HasFlag(BindingFlags.Public);
            var isStatic = bindingAttr.HasFlag(BindingFlags.Static);

            return type.GetRuntimeMethods().Where(m => isPublic == m.IsPublic && isStatic == m.IsStatic).ToArray();
        }
#endif

        /// <summary>
        /// Gets a method on the specified type.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Name of the method on the type.</param>
        /// <param name="types">Argument types for the method.</param>
        /// <param name="isPublic">True if method should search only public methods, false if it should search only non-public methods.</param>
        /// <param name="isStatic">True if method should search only static methods, false if it should search only instance methods.</param>
        /// <returns>MethodInfo for the method with the specified characteristics if found, otherwise null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static MethodInfo GetMethod(this Type type, string name, Type[] types, bool isPublic, bool isStatic)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if PORTABLELIB
            MethodInfo methodInfo = type.GetMethod(name, types);
            if (isPublic == methodInfo.IsPublic && isStatic == methodInfo.IsStatic)
            {
                return methodInfo;
            }

            return null;
#endif
#if !PORTABLELIB
            BindingFlags bindingFlags = BindingFlags.Default;
            bindingFlags |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingFlags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            return type.GetMethod(name, bindingFlags, null, types, null);
#endif
        }

        /// <summary>
        /// Gets all public static methods for a type.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>Enumerable of all public static methods for the specified type.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetRuntimeMethods().Where(m => m.IsPublic && m.IsStatic);
#else
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public);
#endif
        }

        /// <summary>
        /// Replacement for Type.GetNestedTypes(BindingFlags.NonPublic)
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>All types nested in the current type</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Code is shared among multiple assemblies and this method should be available as a helper in case it is needed in new code.")]
        internal static IEnumerable<Type> GetNonPublicNestedTypes(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
#if WINRT
            return type.GetTypeInfo().DeclaredNestedTypes.Where(t => t.IsNotPublic).Select(t => t.AsType());
#else
            return type.GetNestedTypes(BindingFlags.NonPublic);
#endif
        }
        #endregion

#if PORTABLELIB
        /// <summary>
        /// Checks if the specified constructor takes arguments of the specified types.
        /// </summary>
        /// <param name="constructorInfo">ConstructorInfo on which to call this helper method.</param>
        /// <param name="types">Array of type arguments to check against the constructor parameters.</param>
        /// <returns>True if the constructor takes arguments of the specified types, otherwise false.</returns>
        private static bool CheckTypeArgs(ConstructorInfo constructorInfo, Type[] types)
        {
            Debug.Assert(types != null, "Types should not be null, use a different overload of the calling method if you don't care about the parameter types.");

            ParameterInfo[] parameters = constructorInfo.GetParameters();
            if (parameters.Length != types.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != types[i])
                {
                    return false;
                }
            }

            return true;
        }
#endif

#if WINRT
        #region Extension Methods to replace missing functionality (used for WINRT only, methods with these signatures already exist on other platforms)
        /// <summary>
        /// Replacement for Type.IsAssignableFrom(Type)
        /// </summary>
        /// <param name="thisType">Type on which to call this helper method.</param>
        /// <param name="otherType">Type to test for assignability.</param>
        /// <returns>See documentation for method being accessed in the body of the method.</returns>
        internal static bool IsAssignableFrom(this Type thisType, Type otherType)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return thisType.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }

        /// <summary>
        /// Replacement for Type.IsSubclassOf(Type).
        /// </summary>
        /// <param name="thisType">Type on which to call this helper method.</param>
        /// <param name="otherType">Type to test if typeType is a subclass.</param>
        /// <returns>True if thisType is a subclass of otherType, otherwise false.</returns>
        /// <remarks>
        /// TODO: Dev11:279438 is going to add this back to TypeInfo. This method will still be needed since it works on Type, but the 
        ///       implementation should just be able to call the TypeInfo version directly instead of the full implementation here.
        /// </remarks>
        internal static bool IsSubclassOf(this Type thisType, Type otherType)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // devnotes (sparra):
            //      (1) This intentionally does not take interfaces into account, because the other platform implementations also do not.
            //          E.g, a type is never a subclass of an interface, even if the type is an interface itself.
            //      (2) On other platforms, there is an odd behavior where typeof(SomeInterface).IsSubclassOf(typeof(object)) returns true for the default
            //          implementation on Type, even though object is not a BaseType of any interface. Since we are not using IsSubclassOf in any way that
            //          would be affected by this, we do not try to duplicate that behavior with this helper method, and it will return false in that case.

            // If the types are the same one cannot be a subclass of the other.
            if (thisType == otherType)
            {
                return false;
            }

            Type type = thisType.GetTypeInfo().BaseType;
            while (type != null)
            {
                if (type == otherType)
                {
                    return true;
                }

                type = type.GetTypeInfo().BaseType;
            }

            return false;
        }

        /// <summary>
        /// Replacement for GetMethod(string).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Method to find on the specified type.</param>
        /// <returns>MethodInfo if one was found for the specified type, otherwise false.</returns>
        internal static MethodInfo GetMethod(this Type type, string name)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetRuntimeMethods().Where(m => m.IsPublic && m.Name == name).SingleOrDefault();
        }

        /// <summary>
        /// Replacement for Type.GetMethod(string, Type[]).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Name of method to find on the specified type.</param>
        /// <param name="types">Array of arguments to the method.</param>
        /// <returns>MethodInfo if one was found for the specified type, otherwise false.</returns>
        internal static MethodInfo GetMethod(this Type type, string name, Type[] types)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // GetRuntimeMethod(string, Type[]) only searched public methods, so it matches Type.GetMethod(string, Type[]) behavior on other platforms.
            return type.GetRuntimeMethod(name, types);
        }

        /// <summary>
        /// Gets a MethodInfo from the specified type. Replaces uses of Type.GetMember.
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Name of the method to find.</param>
        /// <param name="isPublic">True if the method is public, false otherwise.</param>
        /// <param name="isStatic">True if the method is static, false otherwise.</param>
        /// <param name="genericArgCount">Number of generics arguments the method has.</param>
        /// <returns>MethodInfo for the method that was found.</returns>
        internal static MethodInfo GetMethodWithGenericArgs(this Type type, string name, bool isPublic, bool isStatic, int genericArgCount)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetRuntimeMethods().Single(m => m.Name == name && m.IsPublic == isPublic && m.IsStatic == isStatic && m.GetGenericArguments().Count() == genericArgCount);
        }

        /// <summary>
        /// Replacement for Type.GetProperty(string, Type).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Name of public property to find on the specified type.</param>
        /// <param name="returnType">Return type for the property.</param>
        /// <returns>PropertyInfo if a property was found on the type with the specified name and return type, otherwise null.</returns>
        internal static PropertyInfo GetProperty(this Type type, string name, Type returnType)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // Type.GetRuntimeProperty returns public properties only
            PropertyInfo propertyInfo = type.GetRuntimeProperty(name);
            if (propertyInfo != null && propertyInfo.PropertyType == returnType)
            {
                return propertyInfo;
            }

            return null;
        }

        /// <summary>
        /// Replacement for Type.GetProperty(string).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Name of public property to find on the specified type.</param>
        /// <returns>PropertyInfo if a property was found on the type with the specified name and return type, otherwise null.</returns>
        internal static PropertyInfo GetProperty(this Type type, string name)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // Type.GetRuntimeProperty returns public properties only
            return type.GetRuntimeProperty(name);
        }

        internal static PropertyInfo GetProperty(this Type type, string name, bool isPublic, bool isStatic)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // Type.GetRuntimeProperty returns public properties only
            if (!isPublic)
            {
                return null;
            }

            var propertyType = type.GetRuntimeProperty(name);
            if (propertyType != null && propertyType.GetMethod.IsStatic == isStatic
                && propertyType.SetMethod.IsStatic == isStatic)
            {
                return propertyType;
            }

            return null;
        }

        /// <summary>
        /// Replacement for PropertyInfo.GetGetMethod().
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo on which to call this helper method.</param>
        /// <returns>MethodInfo for the public get accessor of the specified PropertyInfo, or null if there is no get accessor or it is non-public.</returns>
        internal static MethodInfo GetGetMethod(this PropertyInfo propertyInfo)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            MethodInfo getMethod = propertyInfo.GetMethod;
            if (getMethod != null && getMethod.IsPublic)
            {
                return getMethod;
            }

            return null;
        }

        /// <summary>
        /// Replacement for PropertyInfo.GetSetMethod().
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo on which to call this helper method.</param>
        /// <returns>MethodInfo for the public set accessor of the specified PropertyInfo, or null if there is no set accessor or it is non-public.</returns>
        internal static MethodInfo GetSetMethod(this PropertyInfo propertyInfo)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            MethodInfo setMethod = propertyInfo.SetMethod;
            if (setMethod != null && setMethod.IsPublic)
            {
                return setMethod;
            }

            return null;
        }

        /// <summary>
        /// Replacement for MethodInfo.GetBaseDefinition().
        /// </summary>
        /// <param name="methodInfo">MethodInfo on which to call this helper method.</param>
        /// <returns>See documentation for method being accessed in the body of the method.</returns>
        internal static MethodInfo GetBaseDefinition(this MethodInfo methodInfo)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return methodInfo.GetRuntimeBaseDefinition();
        }

        /// <summary>
        /// Replacement for Type.GetProperties().
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>Enumerable of all instance and static public properties on the type.</returns>
        internal static IEnumerable<PropertyInfo> GetProperties(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return GetPublicProperties(type, false /*instanceOnly*/);
        }

        /// <summary>
        /// Replacement for Type.GetFields(string).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>Enumerable of all public instance fields for the specified type.</returns>
        internal static IEnumerable<FieldInfo> GetFields(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            // Need to filter to public only to match Type.GetFields() behavior on other platforms.
            return type.GetRuntimeFields()
                .Where(m => m.IsPublic);
        }

        /// <summary>
        /// Replacement for Type.GetFields(bindingAttr).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="isPublic">True if method should search only public fields, false if it should search only non-public fields.</param>
        /// <param name="isStatic">True if method should search only static fields, false if it should search only instance fields.</param>
        /// <returns>Enumerable of all public instance fields for the specified type.</returns>
        internal static IEnumerable<FieldInfo> GetFields(this Type type, bool isPublic, bool isStatic)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetRuntimeFields().Where(m => m.IsPublic == isPublic && m.IsStatic == isStatic);
        }
        
        /// <summary>
        /// Replacement for Type.GetCustomAttributes(Type, bool).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="attributeType">Attribute type to find on the specified type.</param>
        /// <param name="inherit">True if the base types should be searched, false otherwise.</param>
        /// <returns>See documentation for method being accessed in the body of the method.</returns>
        internal static IEnumerable<object> GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);
        }

        /// <summary>
        /// Replacement for Type.GetCustomAttributes(bool).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="inherit">True if the base types should be searched, false otherwise.</param>
        /// <returns>See documentation for method being accessed in the body of the method.</returns>
        internal static IEnumerable<object> GetCustomAttributes(this Type type, bool inherit)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetTypeInfo().GetCustomAttributes(inherit);
        }

        /// <summary>
        /// Replacement for Type.GetGenericArguments().
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>Array of Type objects that represent the type arguments of a generic type or the type parameters of a generic type definition.</returns>
        internal static Type[] GetGenericArguments(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            if (type.GetTypeInfo().IsGenericTypeDefinition)
            {
                return type.GetTypeInfo().GenericTypeParameters;
            }
            else
            {
                return type.GenericTypeArguments;
            }
        }

        /// <summary>
        /// Replacement for Type.GetInterfaces().
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <returns>See documentation for property being accessed in the body of the method.</returns>
        internal static IEnumerable<Type> GetInterfaces(this Type type)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        /// <summary>
        /// Replacement for Type.IsInstanceOfType(object).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="obj">Object to test to see if it's an instance of the specified type.</param>
        /// <returns>See documentation for method being accessed in the body of the method.</returns>
        internal static bool IsInstanceOfType(this Type type, object obj)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo());
        }

        /// <summary>
        /// Replacement for Stream.Close().
        /// </summary>
        /// <param name="stream">Stream on which to call this helper method.</param>
        /// <remarks>
        /// Many Close methods have been eliminated on WinRT, the recommended pattern is to just use Dispose instead.
        /// </remarks>
        internal static void Close(this Stream stream)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            stream.Dispose();
        }

        /// <summary>
        /// Replacement for Assembly.GetType(string, bool).
        /// </summary>
        /// <param name="assembly">Assembly on which to call this helper method.</param>
        /// <param name="typeName">Name of the type to get from the assembly.</param>
        /// <param name="throwOnError">True if an exception should be thrown if the type cannot be found, otherwise false.</param>
        /// <returns>Type instance if the type could be found in the assembly, otherwise null.</returns>
        /// <remarks>
        /// TODO: Dev11:279441 will add a new method called Assembly.GetDefinedType(string) that returns a TypeInfo and will throw like Assembly.GetType(string, true) used to.
        ///       This helper method will still be needed but should be updated to use the new implementation once it exists.
        /// </remarks>
        internal static Type GetType(this Assembly assembly, string typeName, bool throwOnError)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            Type type = assembly.GetType(typeName);
            if (type == null && throwOnError)
            {
                throw new TypeLoadException();
            }

            return type;
        }

        /// <summary>
        /// Replacement for Assembly.GetTypes().
        /// </summary>
        /// <param name="assembly">Assembly on which to call this helper method.</param>
        /// <returns>Enumerable of the types in the assembly.</returns>
        internal static IEnumerable<Type> GetTypes(this Assembly assembly)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return assembly.DefinedTypes.Select(dt => dt.AsType());
        }

        /// <summary>
        /// Replacement for GetField(string).
        /// </summary>
        /// <param name="type">Type on which to call this helper method.</param>
        /// <param name="name">Method to find on the specified type.</param>
        /// <returns>FieldInfo if one was found for the specified type, otherwise false.</returns>
        internal static FieldInfo GetField(this Type type, string name)
        {
#if ODATALIB
            DebugUtils.CheckNoExternalCallers();
#endif
            return type.GetFields().SingleOrDefault(field => field.Name == name);
        }

        /// <summary>
        /// Checks if the specified PropertyInfo is an instance property.
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo on which to call this helper method.</param>
        /// <returns>True if either the GetMethod or SetMethod for the property is an instance method.</returns>
        private static bool IsInstance(PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetMethod != null && !propertyInfo.GetMethod.IsStatic) || (propertyInfo.SetMethod != null && !propertyInfo.SetMethod.IsStatic);
        }

        /// <summary>
        /// Checks if the specified PropertyInfo is a public property.
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo on which to call this helper method.</param>
        /// <returns>True if either the GetMethod or SetMethod for the property is public.</returns>
        private static bool IsPublic(PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic) || (propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic);
        }
        #endregion
        /// <summary>
        /// Manages the type code mapping used to provide the GetTypeCode functionality.
        /// </summary>
        private class TypeCodeMap
        {
            /// <summary>
            /// Dictionary of types and their type codes.
            /// </summary>
            private Dictionary<Type, TypeCode> typeCodes = new Dictionary<Type, TypeCode>(EqualityComparer<Type>.Default);

            /// <summary>
            /// Constructor for the map.
            /// </summary>
            internal TypeCodeMap()
            {
                this.typeCodes.Add(typeof(bool), TypeCode.Boolean);
                this.typeCodes.Add(typeof(char), TypeCode.Char);
                this.typeCodes.Add(typeof(byte), TypeCode.Byte);
                this.typeCodes.Add(typeof(DateTime), TypeCode.DateTime);
                this.typeCodes.Add(typeof(decimal), TypeCode.Decimal);
                this.typeCodes.Add(typeof(double), TypeCode.Double);
                this.typeCodes.Add(typeof(Int16), TypeCode.Int16);
                this.typeCodes.Add(typeof(UInt16), TypeCode.UInt16);
                this.typeCodes.Add(typeof(Int32), TypeCode.Int32);
                this.typeCodes.Add(typeof(UInt32), TypeCode.UInt32);
                this.typeCodes.Add(typeof(Int64), TypeCode.Int64);
                this.typeCodes.Add(typeof(UInt64), TypeCode.UInt64);
                this.typeCodes.Add(typeof(sbyte), TypeCode.SByte);
                this.typeCodes.Add(typeof(Single), TypeCode.Single);
                this.typeCodes.Add(typeof(string), TypeCode.String);
            }

            /// <summary>
            /// Method that does the lookup in the type map, given a type.
            /// </summary>
            /// <param name="type">Type for which to find the type code.</param>
            /// <returns>TypeCode for the specified type if it's in the map, otherwise TypeCode.Object.</returns>
            internal TypeCode GetTypeCode(Type type)
            {
                TypeCode typeCode;
                if (this.typeCodes.TryGetValue(type, out typeCode))
                {
                    return typeCode;
                }

                return TypeCode.Object;
            }
        }
#endif

        /// <summary>
        /// Creates a Compiled Regex expression
        /// </summary>
        /// <param name="pattern">Pattern to match.</param>
        /// <param name="options">Options to use.</param>
        /// <returns>Regex expression to match supplied patter</returns>
        /// <remarks>Is marked as compiled option only in platforms otherwise RegexOption.None is used</remarks>
        public static Regex CreateCompiled(string pattern, RegexOptions options)
        {
#if SILVERLIGHT || ORCAS || PORTABLELIB
            options = options | RegexOptions.None;
#else
            options = options | RegexOptions.Compiled;
#endif
            return new Regex(pattern, options);
        }

        public static string[] GetSegments(this Uri uri)
        {
#if SILVERLIGHT || PORTABLELIB
            return uri.AbsolutePath.Split('/');
#else
            return uri.Segments;
#endif
        }
    }
}

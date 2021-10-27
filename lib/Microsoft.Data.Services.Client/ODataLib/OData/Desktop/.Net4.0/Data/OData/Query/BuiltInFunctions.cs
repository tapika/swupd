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
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Class containing definitions of all the built-in functions.
    /// </summary>
    internal static class BuiltInFunctions
    {
        /// <summary>
        /// Dictionary of the name of the built-in function and all the signatures.
        /// </summary>
        private static readonly Dictionary<string, FunctionSignatureWithReturnType[]> builtInFunctions = InitializeBuiltInFunctions();

        /// <summary>
        /// Returns a list of signatures for a function name.
        /// </summary>
        /// <param name="name">The name of the function to look for.</param>
        /// <param name="signatures">The list of signatures available for the function name.</param>
        /// <returns>true if the function was found, or false otherwise.</returns>
        internal static bool TryGetBuiltInFunction(string name, out FunctionSignatureWithReturnType[] signatures)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(name != null, "name != null");

            return builtInFunctions.TryGetValue(name, out signatures);
        }

        /// <summary>Builds a description of a list of function signatures.</summary>
        /// <param name="name">Function name.</param>
        /// <param name="signatures">Function signatures.</param>
        /// <returns>A string with ';'-separated list of function signatures.</returns>
        internal static string BuildFunctionSignatureListDescription(string name, IEnumerable<FunctionSignature> signatures)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(name != null, "name != null");
            Debug.Assert(signatures != null, "signatures != null");

            StringBuilder builder = new StringBuilder();
            string descriptionSeparator = "";
            foreach (FunctionSignatureWithReturnType signature in signatures)
            {
                builder.Append(descriptionSeparator);
                descriptionSeparator = "; ";

                string parameterSeparator = "";
                builder.Append(name);
                builder.Append('(');
                foreach (IEdmTypeReference type in signature.ArgumentTypes)
                {
                    builder.Append(parameterSeparator);
                    parameterSeparator = ", ";

                    if (type.IsODataPrimitiveTypeKind() && type.IsNullable)
                    {
                        builder.Append(type.ODataFullName());
                        builder.Append(" Nullable=true");
                    }
                    else
                    {
                        builder.Append(type.ODataFullName());
                    }
                }

                builder.Append(')');
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates all of the spatial functions
        /// </summary>
        /// <param name="functions">Dictionary of functions to add to.</param>
        internal static void CreateSpatialFunctions(IDictionary<string, FunctionSignatureWithReturnType[]> functions)
        {
            DebugUtils.CheckNoExternalCallers();

            // double geo.distance(geographyPoint, geographyPoint)
            // double geo.distance(geometryPoint, geometryPoint)
            FunctionSignatureWithReturnType[] signatures = new FunctionSignatureWithReturnType[]
            {
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetDouble(true), 
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPoint, true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPoint, true)),
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetDouble(true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, true))
            };
            functions.Add("geo.distance", signatures);

            // bool geo.intersects(geometry.Point, geometry.Polygon)
            // bool geo.intersects(geometry.Polygon, geomtery.Point)
            // bool geo.intersects(geography.Point, geography.Polygon)
            // bool geo.intersects(geography.Polygon, geography.Point)
            signatures = new FunctionSignatureWithReturnType[]
            {
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetBoolean(true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPolygon, true)),
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetBoolean(true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPolygon, true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, true)),
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetBoolean(true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPoint, true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPolygon, true)),
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetBoolean(true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPolygon, true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyPoint, true))
            };
            functions.Add("geo.intersects", signatures);

            // double geo.length(geometryLineString)
            // double geo.length(geographyLineString)
            signatures = new FunctionSignatureWithReturnType[]
            {
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetDouble(true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryLineString, true)),
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetDouble(true),
                    EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeographyLineString, true)), 
            };
            functions.Add("geo.length", signatures);
        }

        /// <summary>
        /// Builds the list of all built-in functions.
        /// </summary>
        /// <returns>Returns a dictionary of built in functions.</returns>
        private static Dictionary<string, FunctionSignatureWithReturnType[]> InitializeBuiltInFunctions()
        {
            var functions = new Dictionary<string, FunctionSignatureWithReturnType[]>(StringComparer.Ordinal);
            CreateStringFunctions(functions);
            CreateSpatialFunctions(functions);
            CreateDateTimeFunctions(functions);
            CreateMathFunctions(functions);
            return functions;
        }

        /// <summary>
        /// Creates all string functions.
        /// </summary>
        /// <param name="functions">Dictionary of functions to add to.</param>
        private static void CreateStringFunctions(IDictionary<string, FunctionSignatureWithReturnType[]> functions)
        {
            FunctionSignatureWithReturnType signature;
            FunctionSignatureWithReturnType[] signatures;

            // bool endswith(string, string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetBoolean(false),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("endswith", new FunctionSignatureWithReturnType[] { signature });

            // int indexof(string, string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("indexof", new FunctionSignatureWithReturnType[] { signature });

            // string replace(string, string, string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("replace", new FunctionSignatureWithReturnType[] { signature });

            // bool startswith(string, string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetBoolean(false),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("startswith", new FunctionSignatureWithReturnType[] { signature });

            // string tolower(string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("tolower", new FunctionSignatureWithReturnType[] { signature });

            // string toupper(string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("toupper", new FunctionSignatureWithReturnType[] { signature });

            // string trim(string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("trim", new FunctionSignatureWithReturnType[] { signature });

            signatures = new FunctionSignatureWithReturnType[] 
            {
                // string substring(string, int)
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false)), 

                // string substring(string, int?)
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(true)), 

                // string substring(string, int, int)
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false),
                    EdmCoreModel.Instance.GetInt32(false)), 

                // string substring(string, int?, int)
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(true),
                    EdmCoreModel.Instance.GetInt32(false)), 

                // string substring(string, int, int?)
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false),
                    EdmCoreModel.Instance.GetInt32(true)), 

                // string substring(string, int?, int?)
                new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(true),
                    EdmCoreModel.Instance.GetInt32(true)), 
            };
            functions.Add("substring", signatures);

            // bool substringof(string, string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetBoolean(false),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("substringof", new FunctionSignatureWithReturnType[] { signature });

            // string concat(string, string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("concat", new FunctionSignatureWithReturnType[] { signature });

            // int length(string)
            signature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetString(true));
            functions.Add("length", new FunctionSignatureWithReturnType[] { signature });  
        }

        /// <summary>
        /// Creates all date and time functions.
        /// </summary>
        /// <param name="functions">Dictionary of functions to add to.</param>
        private static void CreateDateTimeFunctions(IDictionary<string, FunctionSignatureWithReturnType[]> functions)
        {
            FunctionSignatureWithReturnType[] withoutTimeSpan = CreateDateTimeFunctionSignatureArray();
            FunctionSignatureWithReturnType[] withTimeSpan = withoutTimeSpan.Concat(CreateTimeSpanFunctionSignatures()).ToArray();

            // int year(DateTime)
            // int year(DateTime?)
            // int year(DateTimeOffset)
            // int year(DateTimeOffset?)
            functions.Add("year", withoutTimeSpan);

            // int month(DateTime)
            // int month(DateTime?)
            // int month(DateTimeOffset)
            // int month(DateTimeOffset?)
            functions.Add("month", withoutTimeSpan);

            // int day(DateTime)
            // int day(DateTime?)
            // int day(DateTimeOffset)
            // int day(DateTimeOffset?)
            functions.Add("day", withoutTimeSpan);

            // int hour(DateTime)
            // int hour(DateTime?)
            // int hour(DateTimeOffset)
            // int hour(DateTimeOffset?)
            // int second(TimeSpan)
            // int second(TimeSpan?)
            functions.Add("hour", withTimeSpan);

            // int minute(DateTime)
            // int minute(DateTime?)
            // int minute(DateTimeOffset)
            // int minute(DateTimeOffset?)
            // int second(TimeSpan)
            // int second(TimeSpan?)
            functions.Add("minute", withTimeSpan);

            // int second(DateTime)
            // int second(DateTime?)
            // int second(DateTimeOffset)
            // int second(DateTimeOffset?)
            // int second(TimeSpan)
            // int second(TimeSpan?)
            functions.Add("second", withTimeSpan);
        }
        
        /// <summary>
        /// Builds an array of signatures for date time functions.
        /// </summary>
        /// <returns>The array of signatures for a date time functions.</returns>
        private static FunctionSignatureWithReturnType[] CreateDateTimeFunctionSignatureArray()
        {
            FunctionSignatureWithReturnType dateTimeNonNullable = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTime, false));

            FunctionSignatureWithReturnType dateTimeNullable = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTime, true));

            FunctionSignatureWithReturnType dateTimeOffsetNonNullable = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTimeOffset, false));

            FunctionSignatureWithReturnType dateTimeOffsetNullable = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTimeOffset, true));

            return new[] { dateTimeNonNullable, dateTimeNullable, dateTimeOffsetNonNullable, dateTimeOffsetNullable };
        }

        /// <summary>
        /// Builds the set of signatures for timespan functions.
        /// </summary>
        /// <returns>The set of signatures for timespan functions.</returns>
        private static IEnumerable<FunctionSignatureWithReturnType> CreateTimeSpanFunctionSignatures()
        {
            yield return new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.Time, false));

            yield return new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetInt32(false),
                EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.Time, true));
        }

        /// <summary>
        /// Creates all math functions.
        /// </summary>
        /// <param name="functions">Dictionary of functions to add to.</param>
        private static void CreateMathFunctions(IDictionary<string, FunctionSignatureWithReturnType[]> functions)
        {
            // double round(double)
            // decimal round(decimal)
            // double round(double?)
            // decimal round(decimal?)
            functions.Add("round", CreateMathFunctionSignatureArray());

            // double floor(double)
            // decimal floor(decimal)
            // double floor(double?)
            // decimal floor(decimal?)
            functions.Add("floor", CreateMathFunctionSignatureArray());

            // double ceiling(double)
            // decimal ceiling(decimal)
            // double ceiling(double?)
            // decimal ceiling(decimal?)
            functions.Add("ceiling", CreateMathFunctionSignatureArray());
        }

        /// <summary>
        /// Builds an array of signatures for math functions.
        /// </summary>
        /// <returns>The array of signatures for math functions.</returns>
        private static FunctionSignatureWithReturnType[] CreateMathFunctionSignatureArray()
        {
            FunctionSignatureWithReturnType doubleSignature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetDouble(false),
                EdmCoreModel.Instance.GetDouble(false));
            FunctionSignatureWithReturnType nullableDoubleSignature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetDouble(false),
                EdmCoreModel.Instance.GetDouble(true));
            FunctionSignatureWithReturnType decimalSignature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetDecimal(false),
                EdmCoreModel.Instance.GetDecimal(false));
            FunctionSignatureWithReturnType nullableDecimalSignature = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetDecimal(false),
                EdmCoreModel.Instance.GetDecimal(true));

            return new FunctionSignatureWithReturnType[] { doubleSignature, decimalSignature, nullableDoubleSignature, nullableDecimalSignature };
        }
    }
}

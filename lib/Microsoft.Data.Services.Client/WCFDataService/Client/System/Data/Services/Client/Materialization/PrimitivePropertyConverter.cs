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

namespace System.Data.Services.Client.Materialization
{
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Spatial;
    using System.Xml;
    using Microsoft.Data.OData;
    using PlatformHelper = System.Data.Services.Client.PlatformHelper;

    /// <summary>
    /// Converter for primitive values which do not match the client property types. This can happen for two reasons:
    ///   1) The client property types do not exist in the protocol (Uri, XElement, etc)
    ///   2) The values were read using the service's model, and the client types are slightly different (ie float vs double, int vs long).
    /// </summary>
    internal class PrimitivePropertyConverter
    {
        /// <summary>The response format the values were originally read from. Required for re-interpreting spatial values correctly.</summary>
        private readonly ODataFormat format;

        /// <summary>Geo JSON formatter used for converting spatial values. Lazily created in case no spatial values are ever converted.</summary>
        private readonly SimpleLazy<GeoJsonObjectFormatter> lazyGeoJsonFormatter = new SimpleLazy<GeoJsonObjectFormatter>(GeoJsonObjectFormatter.Create);

        /// <summary>Gml formatter used for converting spatial values. Lazily created in case no spatial values are ever converted.</summary>
        private readonly SimpleLazy<GmlFormatter> lazyGmlFormatter = new SimpleLazy<GmlFormatter>(GmlFormatter.Create);

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitivePropertyConverter"/> class.
        /// </summary>
        /// <param name="format">The response format the values were originally read from. Required for re-interpreting spatial values correctly.</param>
        internal PrimitivePropertyConverter(ODataFormat format)
        {
            Debug.Assert(format != null, "format != null");
            this.format = format;
        }

        /// <summary>
        /// Converts a value to primitive value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns>The converted value if the value can be converted</returns>
        internal object ConvertPrimitiveValue(object value, Type propertyType)
        {
            // System.Xml.Linq.XElement and System.Data.Linq.Binaries primitive types are not supported by ODataLib directly,
            // so if the property is of one of those types, we need to convert the value to that type here.
            if (propertyType != null && value != null)
            {
                Debug.Assert(PrimitiveType.IsKnownNullableType(propertyType), "GetPrimitiveValue must be called only for primitive types");

                // Fast path for the supported primitive types that have a type code and are supported by ODataLib.
                Type nonNullablePropertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                TypeCode typeCode = PlatformHelper.GetTypeCode(nonNullablePropertyType);
                switch (typeCode)
                {
                    case TypeCode.Boolean:  // fall through
                    case TypeCode.Byte:     // fall through
                    case TypeCode.DateTime: // fall through
                    case TypeCode.Decimal:  // fall through
                    case TypeCode.Double:   // fall through
                    case TypeCode.Int16:    // fall through
                    case TypeCode.Int32:    // fall through
                    case TypeCode.Int64:    // fall through
                    case TypeCode.SByte:    // fall through
                    case TypeCode.Single:   // fall through
                    case TypeCode.String:
                        return this.ConvertValueIfNeeded(value, propertyType);
                }

                // Do the conversion for types that are not supported by ODataLib e.g. char[], char, etc
                // PropertyType might be nullable. Hence to avoid nullable checks, we currently check for
                // primitiveType.ClrType
                if (typeCode == TypeCode.Char ||
                    typeCode == TypeCode.UInt16 ||
                    typeCode == TypeCode.UInt32 ||
                    typeCode == TypeCode.UInt64 ||
                    nonNullablePropertyType == typeof(Char[]) ||
                    nonNullablePropertyType == typeof(Type) ||
                    nonNullablePropertyType == typeof(Uri) ||
                    nonNullablePropertyType == typeof(Xml.Linq.XDocument) ||
                    nonNullablePropertyType == typeof(Xml.Linq.XElement))
                {
                    PrimitiveType primitiveType;
                    PrimitiveType.TryGetPrimitiveType(propertyType, out primitiveType);
                    Debug.Assert(primitiveType != null, "must be a known primitive type");

                    string stringValue = (string)this.ConvertValueIfNeeded(value, typeof(string));
                    return primitiveType.TypeConverter.Parse(stringValue);
                }

#if !ASTORIA_LIGHT && !PORTABLELIB
                if (propertyType == BinaryTypeConverter.BinaryType)
                {
                    byte[] byteArray = (byte[])this.ConvertValueIfNeeded(value, typeof(byte[]));
                    Debug.Assert(byteArray != null, "If the property is of type System.Data.Linq.Binary then ODataLib should have read it as byte[].");
                    return Activator.CreateInstance(BinaryTypeConverter.BinaryType, byteArray);
                }
#endif
            }

            return this.ConvertValueIfNeeded(value, propertyType);
        }

        /// <summary>
        /// Converts a non-spatial primitive value to the target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <returns>The converted value.</returns>
        private static object ConvertNonSpatialValue(object value, Type targetType)
        {
            Debug.Assert(value != null, "value != null");
            TypeCode targetTypeCode = PlatformHelper.GetTypeCode(targetType);

            // These types can be safely converted to directly, as there is no risk of precision being lost.
            switch (targetTypeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }

            string stringValue = ClientConvert.ToString(value);
            return ClientConvert.ChangeType(stringValue, targetType);
        }

        /// <summary>
        /// Converts the value to the target type if needed.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type.</param>
        /// <returns>The converted value.</returns>
        private object ConvertValueIfNeeded(object value, Type targetType)
        {
            // if conversion is not needed, just short cut here.
            if (value == null || targetType.IsInstanceOfType(value))
            {
                return value;
            }

            // spatial types require some extra work, as they cannot be easily converted to/from string.
            if (typeof(ISpatial).IsAssignableFrom(targetType) || value is ISpatial)
            {
                return this.ConvertSpatialValue(value, targetType);
            }

            var nullableUnderlyingType = Nullable.GetUnderlyingType(targetType);
            if (nullableUnderlyingType != null)
            {
                targetType = nullableUnderlyingType;
            }

            // re-parse the primitive value.
            return ConvertNonSpatialValue(value, targetType);
        }

        /// <summary>
        /// Converts a spatial value by from geometry to geography or vice versa. Will return the original instance if it is already of the appropriate hierarchy.
        /// Will throw whatever parsing/format exceptions occur if the sub type is not the same.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <returns>The original or converted value.</returns>
        private object ConvertSpatialValue(object value, Type targetType)
        {
            Debug.Assert(value != null, "value != null");
            Debug.Assert(value is ISpatial && typeof(ISpatial).IsAssignableFrom(targetType), "Arguments must be spatial values/types.");

            // because spatial values already encode their specific subtype (ie point vs polygon), then the only way this conversion can work
            // is if the switch is from geometry to geography, but with the same subtype. So if we detect that the value is already in the same
            // hierarchy as the target type, then simply return it.
            if (typeof(Geometry).IsAssignableFrom(targetType))
            {
                var geographyValue = value as Geography;
                if (geographyValue == null)
                {
                    return value;
                }

                return this.ConvertSpatialValue<Geography, Geometry>(geographyValue);
            }

            Debug.Assert(typeof(Geography).IsAssignableFrom(targetType), "Unrecognized spatial target type: " + targetType.FullName);

            // as above, if the hierarchy already matches, simply return it.
            var geometryValue = value as Geometry;
            if (geometryValue == null)
            {
                return value;
            }

            return this.ConvertSpatialValue<Geometry, Geography>(geometryValue);
        }

        /// <summary>
        /// Converts a spatial value by from geometry to geography or vice versa. Will return the original instance if it is already of the appropriate hierarchy.
        /// Will throw whatever parsing/format exceptions occur if the sub type is not the same.
        /// </summary>
        /// <typeparam name="TIn">The type of the value being converted.</typeparam>
        /// <typeparam name="TOut">The target type of the conversion.</typeparam>
        /// <param name="valueToConvert">The value to convert.</param>
        /// <returns>The original or converted value.</returns>
        private TOut ConvertSpatialValue<TIn, TOut>(TIn valueToConvert)
            where TIn : ISpatial
            where TOut : class, ISpatial
        {
            // This is format specific because the interpretation of which value is longitude/latitude vs x/y is format specific.
            if (this.format == ODataFormat.Atom)
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = XmlWriter.Create(stream))
                    {
                        this.lazyGmlFormatter.Value.Write(valueToConvert, writer);
                    }

                    stream.Position = 0;
                    using (var reader = XmlReader.Create(stream))
                    {
                        return this.lazyGmlFormatter.Value.Read<TOut>(reader);
                    }
                }
            }

            Debug.Assert(this.format == ODataFormat.Json || this.format == ODataFormat.VerboseJson, "Expected a JSON-based format.");
            var json = this.lazyGeoJsonFormatter.Value.Write(valueToConvert);
            return this.lazyGeoJsonFormatter.Value.Read<TOut>(json);
        }
    }
}

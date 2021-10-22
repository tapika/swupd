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

namespace Microsoft.Data.Edm
{
    /// <summary>
    /// Enumerates the kinds of Edm Primitives.
    /// </summary>
    public enum EdmPrimitiveTypeKind
    {
        /// <summary>
        /// Represents a primitive type of unknown kind.
        /// </summary>
        None,

        /// <summary>
        /// Represents a Binary type.
        /// </summary>
        Binary,

        /// <summary>
        /// Represents a Boolean type.
        /// </summary>
        Boolean,

        /// <summary>
        /// Represents a Byte type.
        /// </summary>
        Byte,

        /// <summary>
        /// Represents a DateTime type.
        /// </summary>
        DateTime,

        /// <summary>
        /// Represents a DateTimeOffset type.
        /// </summary>
        DateTimeOffset,

        /// <summary>
        /// Represents a Decimal type.
        /// </summary>
        Decimal,

        /// <summary>
        /// Represents a Double type.
        /// </summary>
        Double,

        /// <summary>
        /// Represents a Guid type.
        /// </summary>
        Guid,

        /// <summary>
        /// Represents a Int16 type.
        /// </summary>
        Int16,

        /// <summary>
        /// Represents a Int32 type.
        /// </summary>
        Int32,

        /// <summary>
        /// Represents a Int64 type.
        /// </summary>
        Int64,

        /// <summary>
        /// Represents a SByte type.
        /// </summary>
        SByte,

        /// <summary>
        /// Represents a Single type.
        /// </summary>
        Single,

        /// <summary>
        /// Represents a String type.
        /// </summary>
        String,

        /// <summary>
        /// Represents a Stream type.
        /// </summary>
        Stream,

        /// <summary>
        /// Represents a Time type.
        /// </summary>
        Time,

        /// <summary>
        /// Represents an arbitrary Geography type.
        /// </summary>
        Geography,

        /// <summary>
        /// Represents a geography Point type.
        /// </summary>
        GeographyPoint,

        /// <summary>
        /// Represents a geography LineString type.
        /// </summary>
        GeographyLineString,

        /// <summary>
        /// Represents a geography Polygon type.
        /// </summary>
        GeographyPolygon,

        /// <summary>
        /// Represents a geography GeographyCollection type.
        /// </summary>
        GeographyCollection,

        /// <summary>
        /// Represents a geography MultiPolygon type.
        /// </summary>
        GeographyMultiPolygon,

        /// <summary>
        /// Represents a geography MultiLineString type.
        /// </summary>
        GeographyMultiLineString,

        /// <summary>
        /// Represents a geography MultiPoint type.
        /// </summary>
        GeographyMultiPoint,

        /// <summary>
        /// Represents an arbitrary Geometry type.
        /// </summary>
        Geometry,

        /// <summary>
        /// Represents a geometry Point type.
        /// </summary>
        GeometryPoint,

        /// <summary>
        /// Represents a geometry LineString type.
        /// </summary>
        GeometryLineString,

        /// <summary>
        /// Represents a geometry Polygon type.
        /// </summary>
        GeometryPolygon,

        /// <summary>
        /// Represents a geometry GeometryCollection type.
        /// </summary>
        GeometryCollection,

        /// <summary>
        /// Represents a geometry MultiPolygon type.
        /// </summary>
        GeometryMultiPolygon,

        /// <summary>
        /// Represents a geometry MultiLineString type.
        /// </summary>
        GeometryMultiLineString,

        /// <summary>
        /// Represents a geometry MultiPoint type.
        /// </summary>
        GeometryMultiPoint
    }

    /// <summary>
    /// Represents a definition of an EDM primitive type.
    /// </summary>
    public interface IEdmPrimitiveType : IEdmSchemaType
    {
        /// <summary>
        /// Gets the primitive kind of this type.
        /// </summary>
        EdmPrimitiveTypeKind PrimitiveKind { get; }
    }
}

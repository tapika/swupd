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

namespace Microsoft.Data.Spatial
{
    /// <summary>
    /// Gml Constants
    /// </summary>
    internal class GmlConstants
    {
        /// <summary>
        /// Gml Namespace
        /// </summary>
        internal const string GmlNamespace = "http://www.opengis.net/gml";

        /// <summary>
        /// FullGlobe namespace
        /// </summary>
        internal const string FullGlobeNamespace = "http://schemas.microsoft.com/sqlserver/2011/geography";

        /// <summary>
        /// Gml Prefix
        /// </summary>
        internal const string GmlPrefix = "gml";

        /// <summary>
        /// System reference attribute name
        /// </summary>
        internal const string SrsName = "srsName";

        /// <summary>
        /// gml:id attribute name
        /// </summary>
        internal const string IdName = "id";

        /// <summary>
        /// System Reference Prefix
        /// </summary>
        internal const string SrsPrefix = "http://www.opengis.net/def/crs/EPSG/0/";
        
        /// <summary>
        /// Gml representation of a point
        /// </summary>
        internal const string Position = "pos";

        /// <summary>
        /// The Gml:name element name
        /// </summary>
        internal const string Name = "name";

        /// <summary>
        /// the Gml:Description element name
        /// </summary>
        internal const string Description = "description";

        /// <summary>
        /// the metadata property element name
        /// </summary>
        internal const string MetadataProperty = "metaDataProperty";

        /// <summary>
        /// Description Reference element name
        /// </summary>
        internal const string DescriptionReference = "descriptionReference";

        /// <summary>
        /// identifier element name
        /// </summary>
        internal const string IdentifierElement = "identifier";
        
        /// <summary>
        /// Gml representation of a point
        /// </summary>
        internal const string PointProperty = "pointProperty";

        /// <summary>
        /// Gml representation of a point array
        /// </summary>
        internal const string PositionList = "posList";

        /// <summary>
        /// Gml Point
        /// </summary>
        internal const string Point = "Point";

        /// <summary>
        /// Gml representation of a linestring
        /// </summary>
        internal const string LineString = "LineString";

        /// <summary>
        /// Gml Polygon
        /// </summary>
        internal const string Polygon = "Polygon";

        /// <summary>
        /// Gml MultiPoint
        /// </summary>
        internal const string MultiPoint = "MultiPoint";

        /// <summary>
        /// Gml MultiLineString
        /// </summary>
        internal const string MultiLineString = "MultiCurve";

        /// <summary>
        /// Gml MultiPolygon
        /// </summary>
        internal const string MultiPolygon = "MultiSurface";

        /// <summary>
        /// Gml Collection
        /// </summary>
        internal const string Collection = "MultiGeometry";

        /// <summary>
        /// Gml FullGlobe
        /// </summary>
        internal const string FullGlobe = "FullGlobe";

        /// <summary>
        /// Gml Polygon exterior ring
        /// </summary>
        internal const string ExteriorRing = "exterior";

        /// <summary>
        /// Gml Polygon interior ring
        /// </summary>
        internal const string InteriorRing = "interior";
        
        /// <summary>
        /// Gml Ring
        /// </summary>
        internal const string LinearRing = "LinearRing";

        /// <summary>
        /// Member Tag for MultiPoint
        /// </summary>
        internal const string PointMember = "pointMember";

        /// <summary>
        /// Members Tag for MultiPoint
        /// </summary>
        internal const string PointMembers = "pointMembers";

        /// <summary>
        /// Member Tag for MultiLineString
        /// </summary>
        internal const string LineStringMember = "curveMember";

        /// <summary>
        /// Members Tag for MultiLineString
        /// </summary>
        internal const string LineStringMembers = "curveMembers";

        /// <summary>
        /// Member Tag for MultiPolygon
        /// </summary>
        internal const string PolygonMember = "surfaceMember";

        /// <summary>
        /// Members Tag for MultiPolygon
        /// </summary>
        internal const string PolygonMembers = "surfaceMembers";

        /// <summary>
        /// Member Tag for Collection
        /// </summary>
        internal const string CollectionMember = "geometryMember";

        /// <summary>
        /// Members Tag for Collection
        /// </summary>
        internal const string CollectionMembers = "geometryMembers";

        /// <summary>
        /// Attribute name for Axis Labels
        /// </summary>
        internal const string AxisLabels = "axisLabels";

        /// <summary>
        /// Attribute name for unit of measure labels
        /// </summary>
        internal const string UomLabels = "uomLabels";

        /// <summary>
        /// Attribute name for count 
        /// </summary>
        internal const string Count = "count";
    }
}

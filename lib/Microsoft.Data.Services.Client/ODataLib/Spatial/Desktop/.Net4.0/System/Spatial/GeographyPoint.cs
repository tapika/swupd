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

namespace System.Spatial
{
#if WINDOWS_PHONE
    using System.Runtime.Serialization;
#endif

    /// <summary>Represents a geography point.</summary>
#if WINDOWS_PHONE
    [DataContract]
#endif
    public abstract class GeographyPoint : Geography
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Spatial.GeographyPoint" /> class.</summary>
        /// <param name="coordinateSystem">The coordinate system of this instance.</param>
        /// <param name="creator">The implementation that created this instance.</param>
        protected GeographyPoint(CoordinateSystem coordinateSystem, SpatialImplementation creator)
            : base(coordinateSystem, creator)
        {
        }

        /// <summary>Gets the latitude.</summary>
        /// <returns>The latitude.</returns>
        public abstract double Latitude { get; }

        /// <summary>Gets the longitude.</summary>
        /// <returns>The longitude.</returns>
        public abstract double Longitude { get; }

        /// <summary>Gets the nullable Z.</summary>
        /// <returns>The nullable Z.</returns>
        /// <remarks>Z is the altitude portion of position.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "z is meaningful")]
        public abstract double? Z { get; }

        /// <summary>Gets the nullable M.</summary>
        /// <returns>The nullable M.</returns>
        /// <remarks>M is the arbitrary measure associated with a position.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "m is meaningful")]
        public abstract double? M { get; }

        /// <summary>Creates a geography point using the specified latitude and longitude.</summary>
        /// <returns>The geography point that was created.</returns>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public static GeographyPoint Create(double latitude, double longitude)
        {
            return Create(CoordinateSystem.DefaultGeography, latitude, longitude, null, null);
        }

        /// <summary>Creates a geography point using the specified latitude, longitude and dimension.</summary>
        /// <returns>The geography point that was created.</returns>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="z">The z dimension.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "z is meaningful")]
        public static GeographyPoint Create(double latitude, double longitude, double? z)
        {
            return Create(CoordinateSystem.DefaultGeography, latitude, longitude, z, null);
        }

        /// <summary>Creates a geography point using the specified latitude, longitude and dimensions.</summary>
        /// <returns>The geography point that was created.</returns>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="z">The z dimension.</param>
        /// <param name="m">The m dimension.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "z and m are meaningful")]
        public static GeographyPoint Create(double latitude, double longitude, double? z, double? m)
        {
            return Create(CoordinateSystem.DefaultGeography, latitude, longitude, z, m);
        }

        /// <summary>Creates a geography point using the specified coordinate system, latitude, longitude and dimensions.</summary>
        /// <returns>The geography point that was created.</returns>
        /// <param name="coordinateSystem">The coordinate system to use.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="z">The z dimension.</param>
        /// <param name="m">The m dimension.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "z and m are meaningful")]
        public static GeographyPoint Create(CoordinateSystem coordinateSystem, double latitude, double longitude, double? z, double? m)
        {
            var builder = SpatialBuilder.Create();
            var pipeline = builder.GeographyPipeline;
            pipeline.SetCoordinateSystem(coordinateSystem);
            pipeline.BeginGeography(SpatialType.Point);
            pipeline.BeginFigure(new GeographyPosition(latitude, longitude, z, m));
            pipeline.EndFigure();
            pipeline.EndGeography();
            return (GeographyPoint)builder.ConstructedGeography;
        }

        /// <summary>Determines whether this instance and another specified geography instance have the same value.</summary>
        /// <returns>true if the value of the value parameter is the same as this instance; otherwise, false.</returns>
        /// <param name="other">The geography to compare to this instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "null is a valid value")]
        public bool Equals(GeographyPoint other)
        {
            return this.BaseEquals(other) ?? this.Latitude == other.Latitude && this.Longitude == other.Longitude && this.Z == other.Z && this.M == other.M;
        }

        /// <summary>Determines whether this instance and the specified object have the same value.</summary>
        /// <returns>true if the value of the value parameter is the same as this instance; otherwise, false.</returns>
        /// <param name="obj">The object to compare to this instance.</param>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyPoint);
        }

        /// <summary>Gets the hash code.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor(this.CoordinateSystem, new[] { this.IsEmpty ? 0 : this.Latitude, this.IsEmpty ? 0 : this.Longitude, this.Z ?? 0, this.M ?? 0 });
        }
    }
}

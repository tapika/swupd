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
    using Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents one position in the Geometry coordinate system
    /// </summary>
    public class GeometryPosition : IEquatable<GeometryPosition>
    {
        /// <summary>arbitrary measure associated with a position</summary>
        private readonly double? m;

        /// <summary>x portion of position</summary>
        private readonly double x;

        /// <summary>y portion of position</summary>
        private readonly double y;

        /// <summary>altitude portion of position</summary>
        private readonly double? z;

        /// <summary>Creates a new instance of the <see cref="T:System.Spatial.GeometryPosition" /> from components.</summary>
        /// <param name="x">The X portion of position.</param>
        /// <param name="y">The Y portion of position.</param>
        /// <param name="z">The altitude portion of position.</param>
        /// <param name="m">The arbitrary measure associated with a position.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z, m make sense in context")]
        public GeometryPosition(double x, double y, double? z, double? m)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.m = m;
        }

        /// <summary>Creates a new instance of the <see cref="T:System.Spatial.GeometryPosition" /> from components.</summary>
        /// <param name="x">The X portion of position.</param>
        /// <param name="y">The Y portion of position.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y make sense in context")]
        public GeometryPosition(double x, double y)
            : this(x, y, null, null)
        {
        }

        /// <summary>Gets the arbitrary measure associated with a position.</summary>
        /// <returns>The arbitrary measure associated with a position.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z, m make sense in context")]
        public double? M
        {
            get { return m; }
        }

        /// <summary>Gets the X portion of position.</summary>
        /// <returns>The X portion of position.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z, m make sense in context")]
        public double X
        {
            get { return x; }
        }

        /// <summary>Gets the Y portion of position.</summary>
        /// <returns>The Y portion of position.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z, m make sense in context")]
        public double Y
        {
            get { return y; }
        }

        /// <summary>Gets the altitude portion of position.</summary>
        /// <returns>The altitude portion of position.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z, m make sense in context")]
        public double? Z
        {
            get { return z; }
        }

        /// <summary>Performs the equality comparison.</summary>
        /// <returns>true if each pair of coordinates is equal; otherwise, false.</returns>
        /// <param name="left">The first position.</param>
        /// <param name="right">The second position.</param>
        public static bool operator ==(GeometryPosition left, GeometryPosition right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            else if (ReferenceEquals(right, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>Performs the inequality comparison.</summary>
        /// <returns>true if left is not equal to right; otherwise, false.</returns>
        /// <param name="left">The first position.</param>
        /// <param name="right">The other position.</param>
        public static bool operator !=(GeometryPosition left, GeometryPosition right)
        {
            return !(left == right);
        }

        /// <summary>Performs the equality comparison on an object.</summary>
        /// <returns>true if each pair of coordinates is equal; otherwise, false.</returns>
        /// <param name="obj">The object for comparison.</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (obj.GetType() != typeof(GeometryPosition))
            {
                return false;
            }

            return Equals((GeometryPosition)obj);
        }

        /// <summary>Performs the equality comparison on a spatial geometry position.</summary>
        /// <returns>true if each pair of coordinates is equal; otherwise, false.</returns>
        /// <param name="other">The other position.</param>
        public bool Equals(GeometryPosition other)
        {
            return other != null && other.x.Equals(x) && other.y.Equals(y) && other.z.Equals(z) && other.m.Equals(m);
        }

        /// <summary>Computes a hash code.</summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = x.GetHashCode();
                result = (result * 397) ^ y.GetHashCode();
                result = (result * 397) ^ (z.HasValue ? z.Value.GetHashCode() : 0);
                result = (result * 397) ^ (m.HasValue ? m.Value.GetHashCode() : 0);
                return result;
            }
        }
        
        /// <summary>Formats this instance to a readable string.</summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "GeometryPosition({0}, {1}, {2}, {3})", this.x, this.y, this.z.HasValue ? this.z.ToString() : "null", this.m.HasValue ? this.m.ToString() : "null");
        }
    }
}

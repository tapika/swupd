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
    using Collections.Generic;
    using Diagnostics.CodeAnalysis;
    using Globalization;
    using Microsoft.Data.Spatial;
#if WINDOWS_PHONE
    using Runtime.Serialization;
#endif

    /// <summary>
    ///   Coordinate System Reference
    /// </summary>
#if WINDOWS_PHONE
    [DataContract]
#endif
    public class CoordinateSystem
    {
        /// <summary>
        ///   Default Geometry Reference
        /// </summary>
        public static readonly CoordinateSystem DefaultGeometry = new CoordinateSystem(0, "Unitless Plane", Topology.Geometry);

        /// <summary>
        ///   Default Geography Reference (SRID 4326, WGS84)
        /// </summary>
        public static readonly CoordinateSystem DefaultGeography = new CoordinateSystem(4326, "WGS84", Topology.Geography);

        /// <summary>
        ///   List of registered references
        /// </summary>
        private static readonly Dictionary<CompositeKey<int, Topology>, CoordinateSystem> References;

        /// <summary>
        ///   A lock object for the References static dict
        /// </summary>
        private static readonly object referencesLock = new object();

        /// <summary>
        ///   The shape of the space that this coordinate system measures.
        /// </summary>
        private Topology topology;

        /// <summary>Initializes a static instance of the <see cref="T:System.Spatial.CoordinateSystem" /> class.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1810", Justification = "Static Constructor required")]
        [SuppressMessage("Microsoft.MSInternal", "CA908", Justification = "generic of int required")]
        static CoordinateSystem()
        {
            References = new Dictionary<CompositeKey<int, Topology>, CoordinateSystem>(EqualityComparer<CompositeKey<int, Topology>>.Default);
            AddRef(DefaultGeometry);
            AddRef(DefaultGeography);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Spatial.CoordinateSystem" /> class.</summary>
        /// <param name = "epsgId">The coordinate system ID, according to the EPSG</param>
        /// <param name = "name">The Name of the system</param>
        /// <param name = "topology">The topology of this coordinate system</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "epsg", Justification = "This is not hungarian notation, but the widley accepted abreviation")]
        internal CoordinateSystem(int epsgId, string name, Topology topology)
        {
            this.topology = topology;
            this.EpsgId = epsgId;
            this.Name = name;
        }

        /// <summary>
        ///   The shapes of the spaces measured by coordinate systems.
        /// </summary>
        internal enum Topology
        {
            /// <summary>
            ///   Ellipsoidal coordinates
            /// </summary>
            Geography = 0,

            /// <summary>
            ///   Planar coordinates
            /// </summary>
            Geometry
        }

        /// <summary>Gets the coordinate system ID according to the EPSG, or NULL if this is not an EPSG coordinate system.</summary>
        /// <returns>The coordinate system ID according to the EPSG.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Epsg", Justification = "This is not hungarian notation, but the widley accepted abreviation")]
#if WINDOWS_PHONE
        [DataMember]
#endif
        public int? EpsgId { get; internal set; }

        /// <summary>Gets the coordinate system Id, no matter what scheme is used.</summary>
        /// <returns>The coordinate system Id.</returns>
        public string Id
        {
            get { return EpsgId.Value.ToString(CultureInfo.InvariantCulture); }
        }

        /// <summary>Gets the Name of the Reference.</summary>
        /// <returns>The Name of the Reference.</returns>
#if WINDOWS_PHONE
        [DataMember]
#endif
        public string Name { get; internal set; }

#if WINDOWS_PHONE
        /// <summary>
        /// internal property used to save state when this instance is serialized.
        /// </summary>
        [DataMember]
        internal Topology TopologyValue
        {
            get { return this.topology; }

            set { this.topology = value; }
        }
#endif
        /// <summary>Gets or creates a Geography coordinate system with the ID, or the default if null is given.</summary>
        /// <returns>The coordinate system.</returns>
        /// <param name="epsgId">The coordinate system id, according to the EPSG. Null indicates the default should be returned.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "epsg", Justification = "This is not hungarian notation, but the widley accepted abreviation")]
        public static CoordinateSystem Geography(int? epsgId)
        {
            return epsgId.HasValue ? GetOrCreate(epsgId.Value, Topology.Geography) : DefaultGeography;
        }

        /// <summary>Gets or creates a Geometry coordinate system with the ID, or the default if null is given.</summary>
        /// <returns>The coordinate system.</returns>
        /// <param name="epsgId">The coordinate system id, according to the EPSG. Null indicates the default should be returned.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "epsg", Justification = "This is not hungarian notation, but the widley accepted abreviation")]
        public static CoordinateSystem Geometry(int? epsgId)
        {
            return epsgId.HasValue ? GetOrCreate(epsgId.Value, Topology.Geometry) : DefaultGeometry;
        }

        /// <summary>Displays the coordinate system for debugging.</summary>
        /// <returns>The coordinate system, for debugging.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}CoordinateSystem(EpsgId={1})", this.topology, this.EpsgId);
        }

        /// <summary>Displays a string that can be used with extended WKT.</summary>
        /// <returns>String representation in the form of SRID=#;</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wkt", Justification = "This is not hungarian notation, but the widley accepted abreviation")]
        public string ToWktId()
        {
            return WellKnownTextConstants.WktSrid + WellKnownTextConstants.WktEquals + this.EpsgId + WellKnownTextConstants.WktSemiColon;
        }

        /// <summary>Indicates the Equals overload.</summary>
        /// <returns>True if equal.</returns>
        /// <param name="obj">The other CoordinateSystem.</param>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as CoordinateSystem);
        }

        /// <summary>Indicates the Equals overload.</summary>
        /// <returns>True if equal.</returns>
        /// <param name="other">The other CoordinateSystem.</param>
        public bool Equals(CoordinateSystem other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.topology, this.topology) && other.EpsgId.Equals(this.EpsgId);
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.topology.GetHashCode() * 397) ^ (this.EpsgId.HasValue ? this.EpsgId.Value : 0);
            }
        }

        /// <summary>
        ///   For tests only. Identifies whether the coordinate system is of the designated topology.
        /// </summary>
        /// <param name = "expected">The expected topology.</param>
        /// <returns>True if this coordinate system is of the expected topology.</returns>
        internal bool TopologyIs(Topology expected)
        {
            return this.topology == expected;
        }

        /// <summary>
        ///   Get or create a CoordinateSystem with ID
        /// </summary>
        /// <param name = "epsgId">The SRID</param>
        /// <param name = "topology">The topology.</param>
        /// <returns>
        ///   A CoordinateSystem object
        /// </returns>
        private static CoordinateSystem GetOrCreate(int epsgId, Topology topology)
        {
            CoordinateSystem r;
            lock (referencesLock)
            {
                if (References.TryGetValue(KeyFor(epsgId, topology), out r))
                {
                    return r;
                }

                r = new CoordinateSystem(epsgId, "ID " + epsgId, topology);
                AddRef(r);
            }

            return r;
        }

        /// <summary>
        ///   Remember this coordinate system in the references dictionary.
        /// </summary>
        /// <param name = "coords">The coords.</param>
        private static void AddRef(CoordinateSystem coords)
        {
            References.Add(KeyFor(coords.EpsgId.Value, coords.topology), coords);
        }

        /// <summary>
        ///   Gets the key for a coordinate system
        /// </summary>
        /// <param name = "epsgId">ID</param>
        /// <param name = "topology">topology</param>
        /// <returns>The key to use with the references dict.</returns>
        private static CompositeKey<int, Topology> KeyFor(int epsgId, Topology topology)
        {
            return new CompositeKey<int, Topology>(epsgId, topology);
        }
    }
}

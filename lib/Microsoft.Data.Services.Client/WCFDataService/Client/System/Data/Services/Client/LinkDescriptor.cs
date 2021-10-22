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

namespace System.Data.Services.Client
{
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// represents the association between two entities
    /// </summary>
    [DebuggerDisplay("State = {state}")]
#if WINDOWS_PHONE
    [DataContract]
#endif
    public sealed class LinkDescriptor : Descriptor
    {
        #region Fields

        /// <summary>equivalence comparer</summary>
        internal static readonly System.Collections.Generic.IEqualityComparer<LinkDescriptor> EquivalenceComparer = new Equivalent();

        /// <summary>source entity</summary>
        private object source;

        /// <summary>name of property on source entity that references the target entity</summary>
        private string sourceProperty;

        /// <summary>target entity</summary>
        private object target;

#if DEBUG && WINDOWS_PHONE
        /// <summary> True,if this instance is being deserialized via DataContractSerialization, false otherwise </summary>
        private bool deserializing;
#endif
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source entity</param>
        /// <param name="sourceProperty">Navigation property on the source entity</param>
        /// <param name="target">Target entity</param>
        /// <param name="model">The client model.</param>
        internal LinkDescriptor(object source, string sourceProperty, object target, ClientEdmModel model)
            : this(source, sourceProperty, target, EntityStates.Unchanged)
        {
            this.IsSourcePropertyCollection = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(source.GetType())).GetProperty(sourceProperty, false).IsEntityCollection;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source entity</param>
        /// <param name="sourceProperty">Navigation property on the source entity</param>
        /// <param name="target">Target entity</param>
        /// <param name="state">The link state</param>
        internal LinkDescriptor(object source, string sourceProperty, object target, EntityStates state)
            : base(state)
        {
            Debug.Assert(source != null, "source != null");
            Debug.Assert(!String.IsNullOrEmpty(sourceProperty), "!String.IsNullOrEmpty(propertyName)");
            Debug.Assert(!sourceProperty.Contains("/"), "!sourceProperty.Contains('/')");

            this.source = source;
            this.sourceProperty = sourceProperty;
            this.target = target;
        }

        #region Public Properties

        /// <summary>The source entity in a link returned by a <see cref="T:System.Data.Services.Client.DataServiceResponse" />. </summary>
        /// <returns><see cref="T:System.Object" />.</returns>
#if WINDOWS_PHONE
        [DataMember]
#endif
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811", Justification = "The setter is called during de-serialization")]

        public object Target
        {
            get
            {
                return this.target;
            }

            internal set
            {
#if DEBUG && WINDOWS_PHONE
                Debug.Assert(this.deserializing, "This property can only be set during deserialization");
#endif
                this.target = value;
            }
        }

        /// <summary>A source entity in a link returned by a <see cref="T:System.Data.Services.Client.DataServiceResponse" />.</summary>
        /// <returns><see cref="T:System.Object" />.</returns>
#if WINDOWS_PHONE
        [DataMember]
#endif
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811", Justification = "The setter is called during de-serialization")]
        public object Source
        {
            get
            {
                return this.source;
            }

            internal set
            {
#if DEBUG && WINDOWS_PHONE
                Debug.Assert(this.deserializing, "This property can only be set during deserialization");
#endif
                this.source = value;
            }
        }

        /// <summary>The identifier property of the source entity in a link returned by a <see cref="T:System.Data.Services.Client.DataServiceResponse" />.</summary>
        /// <returns>The string identifier of an identity property in a source entity. </returns>
#if WINDOWS_PHONE
        [DataMember]
#endif
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811", Justification = "The setter is called during de-serialization")]
        public string SourceProperty
        {
            get
            {
                return this.sourceProperty;
            }

            internal set
            {
#if DEBUG && WINDOWS_PHONE
                Debug.Assert(this.deserializing, "This property can only be set during deserialization");
#endif
                this.sourceProperty = value;
            }
        }

        #endregion

        /// <summary>this is a link</summary>
        internal override DescriptorKind DescriptorKind
        {
            get { return DescriptorKind.Link; }
        }

        /// <summary>is this a collection property or not</summary>
#if WINDOWS_PHONE
        [DataMember]
#endif
        internal bool IsSourcePropertyCollection
        {
            get;
            set;
        }

        /// <summary>
        /// Clear all the changes associated with this descriptor
        /// This method is called when the client is done with sending all the pending requests.
        /// </summary>
        internal override void ClearChanges()
        {
            // Do nothing
        }

        /// <summary>
        /// If the current instance of link descriptor is equivalent to the parameters supplied
        /// </summary>
        /// <param name="src">The source entity</param>
        /// <param name="srcPropName">The source property name</param>
        /// <param name="targ">The target entity</param>
        /// <returns>true if equivalent</returns>
        internal bool IsEquivalent(object src, string srcPropName, object targ)
        {
            return (this.source == src &&
                this.target == targ &&
                this.sourceProperty == srcPropName);
        }

#if DEBUG && WINDOWS_PHONE
        /// <summary>
        /// Called during deserialization of this instance by DataContractSerialization
        /// </summary>
        /// <param name="context">Streaming context for this deserialization session</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.deserializing = true;
        }

        /// <summary>
        /// Called after this instance has been deserialized by DataContractSerialization
        /// </summary>
        /// <param name="context">Streaming context for this deserialization session</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.deserializing = false;
        }
#endif

        /// <summary>equivalence comparer</summary>
        private sealed class Equivalent : System.Collections.Generic.IEqualityComparer<LinkDescriptor>
        {
            /// <summary>are two LinkDescriptors equivalent, ignore state</summary>
            /// <param name="x">link descriptor x</param>
            /// <param name="y">link descriptor y</param>
            /// <returns>true if equivalent</returns>
            public bool Equals(LinkDescriptor x, LinkDescriptor y)
            {
                return (null != x) && (null != y) && x.IsEquivalent(y.source, y.sourceProperty, y.target);
            }

            /// <summary>compute hashcode for LinkDescriptor</summary>
            /// <param name="obj">link descriptor</param>
            /// <returns>hashcode</returns>
            public int GetHashCode(LinkDescriptor obj)
            {
                return (null != obj) ? (obj.Source.GetHashCode() ^ ((null != obj.Target) ? obj.Target.GetHashCode() : 0) ^ obj.SourceProperty.GetHashCode()) : 0;
            }
        }
    }
}

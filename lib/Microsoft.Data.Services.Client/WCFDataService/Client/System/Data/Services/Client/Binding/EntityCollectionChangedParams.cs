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
#region Namespaces
    using System.Collections;
    using System.Collections.Specialized;
#endregion    

    /// <summary>Encapsulates the arguments of a <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> delegate.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Name gets too long with Parameters")]
    public sealed class EntityCollectionChangedParams
    {
        #region Fields
        
        /// <summary>Context associated with the BindingObserver.</summary>
        private readonly DataServiceContext context;

        /// <summary>
        /// The source object that references the target object through a collection navigation property.
        /// </summary>
        private readonly object sourceEntity;

        /// <summary>The property of the source object that references the collection that has changed.</summary>
        private readonly string propertyName;

        /// <summary>The entity set of the source object.</summary>
        private readonly string sourceEntitySet;

        /// <summary>The collection that has changed.</summary>
        private readonly ICollection collection;

        /// <summary>The target entity object involved in the change.</summary>
        private readonly object targetEntity;

        /// <summary>The entity set name of the target object.</summary>
        private readonly string targetEntitySet;

        /// <summary>
        /// The action that indicates how the collection was changed. The value will be Add or Remove.
        /// </summary>
        private readonly NotifyCollectionChangedAction action;

        #endregion

        #region Constructor
        
        /// <summary>
        /// Construct an EntityCollectionChangedParams object.
        /// </summary>
        /// <param name="context">The DataServiceContext associated with the BindingObserver.</param>
        /// <param name="sourceEntity">The source object that references the target object through a collection navigation property.</param>
        /// <param name="propertyName">The property of the source object that references the collection that has changed.</param>
        /// <param name="sourceEntitySet">The entity set of the source object.</param>
        /// <param name="collection">The collection that has changed.</param>
        /// <param name="targetEntity">The target entity object involved in the change.</param>
        /// <param name="targetEntitySet">The entity set name of the target object.</param>
        /// <param name="action">The action that indicates how the collection was changed. The value will be Add or Remove.</param>
        internal EntityCollectionChangedParams(
            DataServiceContext context,
            object sourceEntity,
            string propertyName,
            string sourceEntitySet,
            ICollection collection,
            object targetEntity,
            string targetEntitySet,
            NotifyCollectionChangedAction action)
        {
            this.context = context;
            this.sourceEntity = sourceEntity;
            this.propertyName = propertyName;
            this.sourceEntitySet = sourceEntitySet;
            this.collection = collection;
            this.targetEntity = targetEntity;
            this.targetEntitySet = targetEntitySet;
            this.action = action;
        }
        
        #endregion

        #region Properties
        
        /// <summary>The <see cref="T:System.Data.Services.Client.DataServiceContext" /> associated with the <see cref="T:System.Data.Services.Client.DataServiceCollection`1" /> that has changed.</summary>
        /// <returns>The context associated with the collection that has changed</returns>
        public DataServiceContext Context
        {
            get { return this.context; }
        }

        /// <summary>The source object that references the target object in the collection by using a navigation property. </summary>
        /// <returns>The source object.</returns>
        public object SourceEntity
        {
            get { return this.sourceEntity; }
        }

        /// <summary>The navigation property on the source object that references the collection that has changed.</summary>
        /// <returns>The navigation property name.</returns>
        public string PropertyName
        {
            get { return this.propertyName; }
        }

        /// <summary>The entity set of the source object.</summary>
        /// <returns>An entity set name.</returns>
        public string SourceEntitySet
        {
            get { return this.sourceEntitySet; }
        }

        /// <summary>The entity object in the collection that has changed.</summary>
        /// <returns>The changed entity object in the collection.</returns>
        public object TargetEntity
        {
            get { return this.targetEntity; }
        }

        /// <summary>The entity set name of the object in the collection.</summary>
        /// <returns>An entity set name.</returns>
        public string TargetEntitySet
        {
            get { return this.targetEntitySet; }
        }

        /// <summary>The <see cref="T:System.Data.Services.Client.DataServiceCollection`1" /> that has changed.</summary>
        /// <returns>A reference to the collection that has changed.</returns>
        public ICollection Collection
        {
            get { return this.collection; }
        }

        /// <summary>A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedAction" /> value that indicates how the collection was changed. </summary>
        /// <returns>A value that indicates how the collection was changed.</returns>
        public NotifyCollectionChangedAction Action
        {
            get { return this.action; }
        }

        #endregion    
    }
}

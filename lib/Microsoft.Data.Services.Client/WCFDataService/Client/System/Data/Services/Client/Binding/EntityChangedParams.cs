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
    /// <summary>Encapsulates the arguments of a <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> delegate</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Name gets too long with Parameters")]
    public sealed class EntityChangedParams
    {
        #region Fields
        
        /// <summary>Context associated with the BindingObserver.</summary>
        private readonly DataServiceContext context;
        
        /// <summary>The entity object that has changed.</summary>
        private readonly object entity;
        
        /// <summary>The property of the entity that has changed.</summary>
        private readonly string propertyName;
        
        /// <summary>The current value of the target property.</summary>
        private readonly object propertyValue;

        /// <summary>Entity set to which the entity object belongs</summary>
        private readonly string sourceEntitySet;
        
        /// <summary>Entity set to which the target propertyValue entity belongs</summary>
        private readonly string targetEntitySet;

        #endregion

        #region Constructor
        
        /// <summary>
        /// Construct an EntityChangedParams object.
        /// </summary>
        /// <param name="context">Context to which the entity and propertyValue belong.</param>
        /// <param name="entity">The entity object that has changed.</param>
        /// <param name="propertyName">The property of the target entity object that has changed.</param>
        /// <param name="propertyValue">The current value of the entity property.</param>
        /// <param name="sourceEntitySet">Entity set to which the entity object belongs</param>
        /// <param name="targetEntitySet">Entity set to which the target propertyValue entity belongs</param>
        internal EntityChangedParams(
            DataServiceContext context,
            object entity,
            string propertyName,
            object propertyValue,
            string sourceEntitySet,
            string targetEntitySet)
        {
            this.context = context;
            this.entity = entity;
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
            this.sourceEntitySet = sourceEntitySet;
            this.targetEntitySet = targetEntitySet;
        }
        
        #endregion

        #region Properties

        /// <summary>The context that is associated with the entity object that has changed.</summary>
        /// <returns>The context that is tracking the changed object.</returns>
        public DataServiceContext Context
        {
            get { return this.context; }
        }

        /// <summary>The entity object that has changed.</summary>
        /// <returns>The changed object.</returns>
        public object Entity
        {
            get { return this.entity; }
        }

        /// <summary>The name of the property on the entity object that references the target object.</summary>
        /// <returns>The name of the changed property.</returns>
        public string PropertyName
        {
            get { return this.propertyName; }
        }

        /// <summary>The object that is currently referenced by the changed property on the entity object.</summary>
        /// <returns>The current value that references a target entity. </returns>
        public object PropertyValue
        {
            get { return this.propertyValue; }
        }

        /// <summary>The entity set of the source object.</summary>
        /// <returns>An entity set name.</returns>
        public string SourceEntitySet
        {
            get { return this.sourceEntitySet; }
        }

        /// <summary>The entity set to which the target entity object belongs</summary>
        /// <returns>An entity set name.</returns>
        public string TargetEntitySet
        {
            get { return this.targetEntitySet; }
        }
        
        #endregion
    }
}

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
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Diagnostics;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    #endregion

    /// <summary>
    /// Materializer for LoadProperty API call for navigation properties.
    /// </summary>
    internal class ODataLoadNavigationPropertyMaterializer : ODataReaderEntityMaterializer
    {
        /// <summary>
        /// Response Info object.
        /// </summary>
        private LoadPropertyResponseInfo responseInfo;

        /// <summary>
        /// Items that have been read from the feed or entry.
        /// </summary>
        private List<object> items;

        /// <summary>
        /// Iteration of the entity collection reader.
        /// </summary>
        private int iteration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataLoadNavigationPropertyMaterializer" /> class.
        /// </summary>
        /// <param name="odataMessageReader">The odata message reader.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="materializerContext">The materializer context.</param>
        /// <param name="entityTrackingAdapter">The entity tracking adapter.</param>
        /// <param name="queryComponents">The query components.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="materializeEntryPlan">The materialize entry plan.</param>
        /// <param name="responseInfo">LoadProperty Response Info object.</param>
        public ODataLoadNavigationPropertyMaterializer(
            ODataMessageReader odataMessageReader,
            ODataReaderWrapper reader,
            IODataMaterializerContext materializerContext,
            EntityTrackingAdapter entityTrackingAdapter,
            QueryComponents queryComponents,
            Type expectedType,
            ProjectionPlan materializeEntryPlan,
            LoadPropertyResponseInfo responseInfo)
            : base(odataMessageReader, reader, materializerContext, entityTrackingAdapter, queryComponents, expectedType, materializeEntryPlan)
        {
            this.responseInfo = responseInfo;
            this.items = new List<object>();
        }

        /// <summary>
        /// Implementation of Read/>.
        /// </summary>
        /// <returns>
        /// Return value of Read/>
        /// </returns>
        protected override bool ReadImplementation()
        {
            // Eagerly materialize the entire collection of objects into the items cache in LoadProperty scenario.
            if (this.iteration == 0)
            {
                while (base.ReadImplementation())
                {
                    this.items.Add(this.currentValue);
                }

                ClientPropertyAnnotation property = this.responseInfo.Property;
                EntityDescriptor entityDescriptor = this.responseInfo.EntityDescriptor;
                object entity = entityDescriptor.Entity;

                MaterializerEntry entry = MaterializerEntry.CreateEntryForLoadProperty(
                        entityDescriptor,
                        this.Format,
                        this.responseInfo.MergeOption != MergeOption.NoTracking);

                entry.ActualType = this.responseInfo.Model.GetClientTypeAnnotation(this.responseInfo.Model.GetOrCreateEdmType(entity.GetType()));

                if (property.IsEntityCollection)
                {
                    this.EntryValueMaterializationPolicy.ApplyItemsToCollection(
                        entry,
                        property,
                        this.items,
                        this.CurrentFeed != null ? this.CurrentFeed.NextPageLink : null,
                        this.MaterializeEntryPlan,
                        this.responseInfo.IsContinuation);
                }
                else
                {
                    Debug.Assert(this.items.Count <= 1, "Expecting 0 or 1 element.");
                    object target = this.items.Count > 0 ? this.items[0] : null;

                    Debug.Assert(property.EdmProperty.Type.TypeKind() == EdmTypeKind.Entity, "Must be entity typed property if not an entity collection property.");
                    this.EntityTrackingAdapter.MaterializationLog.SetLink(entry, property.PropertyName, target);

                    // Singleton entity property
#if ASTORIA_OPEN_OBJECT
                    object openProperties = null;
                    property.SetValue(entity, target, property.PropertyName, ref openProperties, false);
#else
                    property.SetValue(entity, target, property.PropertyName, false);
#endif
                }

                // Apply the materialization log.
                this.ApplyLogToContext();

                // Clear the log after applying it.
                this.ClearLog();
            }

            // Read object from the already loaded items cache.
            if (this.items.Count > this.iteration)
            {
                this.currentValue = this.items[this.iteration];
                this.iteration++;
                return true;
            }
            else
            {
                this.currentValue = null;
                return false;
            }
        }
    }
}

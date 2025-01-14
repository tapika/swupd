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

    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;

    #endregion Namespaces

    /// <summary>
    /// Entity Tracker base, allows more decoupling for testing.
    /// </summary>
    internal abstract class EntityTrackerBase
    {
        /// <summary>
        /// Find tracked entity by its resourceUri and update its etag.
        /// </summary>
        /// <param name="resourceUri">resource id</param>
        /// <param name="state">state of entity</param>
        /// <returns>entity if found else null</returns>
        internal abstract object TryGetEntity(String resourceUri, out EntityStates state);

        /// <summary>
        /// get the related links ignoring target entity
        /// </summary>
        /// <param name="source">source entity</param>
        /// <param name="sourceProperty">source entity's property</param>
        /// <returns>enumerable of related ends</returns>
        internal abstract IEnumerable<LinkDescriptor> GetLinks(object source, string sourceProperty);

        /// <summary>
        /// Attach entity into the context in the Unchanged state.
        /// </summary>
        /// <param name="entityDescriptorFromMaterializer">entity descriptor from the response</param>
        /// <param name="failIfDuplicated">fail for public api else change existing relationship to unchanged</param>
        /// <remarks>Caller should validate descriptor instance.</remarks>
        /// <returns>The attached descriptor, if one already exists in the context and failIfDuplicated is set to false, then the existing instance is returned</returns>
        /// <exception cref="InvalidOperationException">if entity is already being tracked by the context</exception>
        /// <exception cref="InvalidOperationException">if identity is pointing to another entity</exception>
        internal abstract EntityDescriptor InternalAttachEntityDescriptor(EntityDescriptor entityDescriptorFromMaterializer, bool failIfDuplicated);

        /// <summary>
        /// verify the resource being tracked by context
        /// </summary>
        /// <param name="resource">resource</param>
        /// <returns>The given resource.</returns>
        /// <exception cref="InvalidOperationException">if resource is not contained</exception>
        internal abstract EntityDescriptor GetEntityDescriptor(object resource);

        /// <summary>Detach existing link</summary>
        /// <param name="existingLink">link to detach</param>
        /// <param name="targetDelete">true if target is being deleted, false otherwise</param>
        internal abstract void DetachExistingLink(LinkDescriptor existingLink, bool targetDelete);

        /// <summary>
        /// attach the link with the given source, sourceProperty and target.
        /// </summary>
        /// <param name="source">source entity of the link.</param>
        /// <param name="sourceProperty">name of the property on the source entity.</param>
        /// <param name="target">target entity of the link.</param>
        /// <param name="linkMerge">merge option to be used to merge the link if there is an existing link.</param>
        internal abstract void AttachLink(object source, string sourceProperty, object target, MergeOption linkMerge);

        /// <summary>response materialization has an identity to attach to the inserted object</summary>
        /// <param name="entityDescriptorFromMaterializer">entity descriptor containing all the information about the entity from the response.</param>
        /// <param name="metadataMergeOption">mergeOption based on which EntityDescriptor will be merged.</param>
        internal abstract void AttachIdentity(EntityDescriptor entityDescriptorFromMaterializer, MergeOption metadataMergeOption);
    }
}

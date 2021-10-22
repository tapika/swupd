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

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
    /// <summary>
    /// Represents a definition of an EDM entity type.
    /// </summary>
    public class EdmEntityType : EdmStructuredType, IEdmEntityType
    {
        private readonly string namespaceName;
        private readonly string name;
        private List<IEdmStructuralProperty> declaredKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEntityType"/> class.
        /// </summary>
        /// <param name="namespaceName">Namespace the entity belongs to.</param>
        /// <param name="name">Name of the entity.</param>
        public EdmEntityType(string namespaceName, string name)
            : this(namespaceName, name, null, false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEntityType"/> class.
        /// </summary>
        /// <param name="namespaceName">Namespace the entity belongs to.</param>
        /// <param name="name">Name of the entity.</param>
        /// <param name="baseType">The base type of this entity type.</param>
        public EdmEntityType(string namespaceName, string name, IEdmEntityType baseType)
            : this(namespaceName, name, baseType, false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEntityType"/> class.
        /// </summary>
        /// <param name="namespaceName">Namespace the entity belongs to.</param>
        /// <param name="name">Name of the entity.</param>
        /// <param name="baseType">The base type of this entity type.</param>
        /// <param name="isAbstract">Denotes an entity that cannot be instantiated.</param>
        /// <param name="isOpen">Denotes if the type is open.</param>
        public EdmEntityType(string namespaceName, string name, IEdmEntityType baseType, bool isAbstract, bool isOpen)
            : base(isAbstract, isOpen, baseType)
        {
            EdmUtil.CheckArgumentNull(namespaceName, "namespaceName");
            EdmUtil.CheckArgumentNull(name, "name");

            this.namespaceName = namespaceName;
            this.name = name;
        }

        /// <summary>
        /// Gets the structural properties of the entity type that make up the entity key.
        /// </summary>
        public virtual IEnumerable<IEdmStructuralProperty> DeclaredKey
        {
            get { return this.declaredKey; }
        }

        /// <summary>
        /// Gets the kind of this schema element.
        /// </summary>
        public EdmSchemaElementKind SchemaElementKind
        {
            get { return EdmSchemaElementKind.TypeDefinition; }
        }

        /// <summary>
        /// Gets the namespace this schema element belongs to.
        /// </summary>
        public string Namespace
        {
            get { return this.namespaceName; }
        }

        /// <summary>
        /// Gets the name of this element.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the kind of this type.
        /// </summary>
        public override EdmTypeKind TypeKind
        {
            get { return EdmTypeKind.Entity; }
        }

        /// <summary>
        /// Gets the term kind of the entity type.
        /// </summary>
        public EdmTermKind TermKind
        {
            get { return EdmTermKind.Type; }
        }

        /// <summary>
        /// Adds the <paramref name="keyProperties"/> to the key of this entity type.
        /// </summary>
        /// <param name="keyProperties">The key properties.</param>
        public void AddKeys(params IEdmStructuralProperty[] keyProperties)
        {
            this.AddKeys((IEnumerable<IEdmStructuralProperty>)keyProperties);
        }

        /// <summary>
        /// Adds the <paramref name="keyProperties"/> to the key of this entity type.
        /// </summary>
        /// <param name="keyProperties">The key properties.</param>
        public void AddKeys(IEnumerable<IEdmStructuralProperty> keyProperties)
        {
            EdmUtil.CheckArgumentNull(keyProperties, "keyProperties");

            foreach (IEdmStructuralProperty property in keyProperties)
            {
                if (this.declaredKey == null)
                {
                    this.declaredKey = new List<IEdmStructuralProperty>();
                }

                this.declaredKey.Add(property);
            }
        }

        /// <summary>
        /// Creates and adds a unidirectional navigation property to this type.
        /// Default partner property is created, but not added to the navigation target type.
        /// </summary>
        /// <param name="propertyInfo">Information to create the navigation property.</param>
        /// <returns>Created navigation property.</returns>
        public EdmNavigationProperty AddUnidirectionalNavigation(EdmNavigationPropertyInfo propertyInfo)
        {
            return AddUnidirectionalNavigation(propertyInfo, this.FixUpDefaultPartnerInfo(propertyInfo, null));
        }

        /// <summary>
        /// Creates and adds a unidirectional navigation property to this type.
        /// Navigation property partner is created, but not added to the navigation target type.
        /// </summary>
        /// <param name="propertyInfo">Information to create the navigation property.</param>
        /// <param name="partnerInfo">Information to create the partner navigation property.</param>
        /// <returns>Created navigation property.</returns>
        public EdmNavigationProperty AddUnidirectionalNavigation(EdmNavigationPropertyInfo propertyInfo, EdmNavigationPropertyInfo partnerInfo)
        {
            EdmUtil.CheckArgumentNull(propertyInfo, "propertyInfo");

            EdmNavigationProperty property = EdmNavigationProperty.CreateNavigationPropertyWithPartner(propertyInfo, this.FixUpDefaultPartnerInfo(propertyInfo, partnerInfo));

            this.AddProperty(property);
            return property;
        }

        /// <summary>
        /// Creates and adds a navigation property to this type and adds its navigation partner to the navigation target type.
        /// </summary>
        /// <param name="propertyInfo">Information to create the navigation property.</param>
        /// <param name="partnerInfo">Information to create the partner navigation property.</param>
        /// <returns>Created navigation property.</returns>
        public EdmNavigationProperty AddBidirectionalNavigation(EdmNavigationPropertyInfo propertyInfo, EdmNavigationPropertyInfo partnerInfo)
        {
            EdmUtil.CheckArgumentNull(propertyInfo, "propertyInfo");
            EdmUtil.CheckArgumentNull(propertyInfo.Target, "propertyInfo.Target");

            EdmEntityType targetType = propertyInfo.Target as EdmEntityType;
            if (targetType == null)
            {
                throw new ArgumentException("propertyInfo.Target", Strings.Constructable_TargetMustBeStock(typeof(EdmEntityType).FullName));
            }

            EdmNavigationProperty property = EdmNavigationProperty.CreateNavigationPropertyWithPartner(propertyInfo, this.FixUpDefaultPartnerInfo(propertyInfo, partnerInfo));

            this.AddProperty(property);
            targetType.AddProperty(property.Partner);
            return property;
        }

        /// <summary>
        /// The puspose of this method is to make sure that some of the <paramref name="partnerInfo"/> fields are set to valid partner defaults.
        /// For example if <paramref name="partnerInfo"/>.Target is null, it will be set to this entity type. If <paramref name="partnerInfo"/>.TargetMultiplicity
        /// is unknown, it will be set to 0..1, etc.
        /// Whenever this method applies new values to <paramref name="partnerInfo"/>, it will return a copy of it (thus won't modify the original).
        /// If <paramref name="partnerInfo"/> is null, a new info object will be produced.
        /// </summary>
        /// <param name="propertyInfo">Primary navigation property info.</param>
        /// <param name="partnerInfo">Partner navigation property info. May be null.</param>
        /// <returns>Partner info.</returns>
        private EdmNavigationPropertyInfo FixUpDefaultPartnerInfo(EdmNavigationPropertyInfo propertyInfo, EdmNavigationPropertyInfo partnerInfo)
        {
            EdmNavigationPropertyInfo partnerInfoOverride = null;

            if (partnerInfo == null)
            {
                partnerInfo = partnerInfoOverride = new EdmNavigationPropertyInfo();
            }

            if (partnerInfo.Name == null)
            {
                if (partnerInfoOverride == null)
                {
                    partnerInfoOverride = partnerInfo.Clone();
                }

                partnerInfoOverride.Name = (propertyInfo.Name ?? String.Empty) + "Partner";
            }

            if (partnerInfo.Target == null)
            {
                if (partnerInfoOverride == null)
                {
                    partnerInfoOverride = partnerInfo.Clone();
                }

                partnerInfoOverride.Target = this;
            }

            if (partnerInfo.TargetMultiplicity == EdmMultiplicity.Unknown)
            {
                if (partnerInfoOverride == null)
                {
                    partnerInfoOverride = partnerInfo.Clone();
                }

                partnerInfoOverride.TargetMultiplicity = EdmMultiplicity.ZeroOrOne;
            }

            return partnerInfoOverride ?? partnerInfo;
        }
    }
}

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
using Microsoft.Data.Edm.Internal;

namespace Microsoft.Data.Edm.Library
{
    /// <summary>
    /// Common base class for definitions of EDM structured types.
    /// </summary>
    public abstract class EdmStructuredType : EdmType, IEdmStructuredType
    {
        /// <summary>
        /// The lock used when accessing the internal cached properties.
        /// </summary>
        protected readonly object LockObj = new object();

        private readonly IEdmStructuredType baseStructuredType;
        private readonly List<IEdmProperty> declaredProperties = new List<IEdmProperty>();
        private readonly bool isAbstract;
        private readonly bool isOpen;

        // PropertiesDictionary cache.
        private readonly Cache<EdmStructuredType, IDictionary<string, IEdmProperty>> propertiesDictionary = new Cache<EdmStructuredType, IDictionary<string, IEdmProperty>>();
        private static readonly Func<EdmStructuredType, IDictionary<string, IEdmProperty>> ComputePropertiesDictionaryFunc = (me) => me.ComputePropertiesDictionary();

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmStructuredType"/> class.
        /// </summary>
        /// <param name="isAbstract">Denotes a structured type that cannot be instantiated.</param>
        /// <param name="isOpen">Denotes if the type is open.</param>
        /// <param name="baseStructuredType">Base type of the type</param>
        protected EdmStructuredType(bool isAbstract, bool isOpen, IEdmStructuredType baseStructuredType)
        {
            this.isAbstract = isAbstract;
            this.isOpen = isOpen;
            this.baseStructuredType = baseStructuredType;
        }

        /// <summary>
        /// Gets a value indicating whether this type is abstract.
        /// </summary>
        public bool IsAbstract
        {
            get { return this.isAbstract; }
        }

        /// <summary>
        /// Gets a value indicating whether this type is open.
        /// </summary>
        public bool IsOpen
        {
            get { return this.isOpen; }
        }

        /// <summary>
        /// Gets the properties declared immediately within this type.
        /// </summary>
        public virtual IEnumerable<IEdmProperty> DeclaredProperties
        {
            get { return this.declaredProperties; }
        }

        /// <summary>
        /// Gets the base type of this type.
        /// </summary>
        public IEdmStructuredType BaseType
        {
            get { return this.baseStructuredType; }
        }

        /// <summary>
        /// Gets a dictionary of the properties in this type definition for faster lookup.
        /// </summary>
        protected IDictionary<string, IEdmProperty> PropertiesDictionary
        {
            get
            {
                lock (LockObj)
                {
                    return this.propertiesDictionary.GetValue(this, ComputePropertiesDictionaryFunc, null);
                }
            }
        }

        /// <summary>
        /// Adds the <paramref name="property"/> to this type.
        /// <see cref="IEdmProperty.DeclaringType"/> of the <paramref name="property"/> must be this type.
        /// </summary>
        /// <param name="property">The property being added.</param>
        public void AddProperty(IEdmProperty property)
        {
            EdmUtil.CheckArgumentNull(property, "property");

            if (!Object.ReferenceEquals(this, property.DeclaringType))
            {
                throw new InvalidOperationException(Edm.Strings.EdmModel_Validator_Semantic_DeclaringTypeMustBeCorrect(property.Name));
            }

            this.declaredProperties.Add(property);

            lock (LockObj)
            {
                this.propertiesDictionary.Clear(null);
            }
        }

        /// <summary>
        /// Creates and adds a nullable structural property to this type.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="type">Type of the property.</param>
        /// <returns>Created structural property.</returns>
        public EdmStructuralProperty AddStructuralProperty(string name, EdmPrimitiveTypeKind type)
        {
            EdmStructuralProperty property = new EdmStructuralProperty(this, name, EdmCoreModel.Instance.GetPrimitive(type, true));
            this.AddProperty(property);
            return property;
        }

        /// <summary>
        /// Creates and adds a nullable structural property to this type.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="type">Type of the property.</param>
        /// <param name="isNullable">Flag specifying if the property is nullable.</param>
        /// <returns>Created structural property.</returns>
        public EdmStructuralProperty AddStructuralProperty(string name, EdmPrimitiveTypeKind type, bool isNullable)
        {
            EdmStructuralProperty property = new EdmStructuralProperty(this, name, EdmCoreModel.Instance.GetPrimitive(type, isNullable));
            this.AddProperty(property);
            return property;
        }

        /// <summary>
        /// Creates and adds a structural property to this type.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="type">Type of the property.</param>
        /// <returns>Created structural property.</returns>
        public EdmStructuralProperty AddStructuralProperty(string name, IEdmTypeReference type)
        {
            EdmStructuralProperty property = new EdmStructuralProperty(this, name, type);
            this.AddProperty(property);
            return property;
        }

        /// <summary>
        /// Creates and adds a structural property to this type.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="type">Type of the property.</param>
        /// <param name="defaultValue">The default value of this property.</param>
        /// <param name="concurrencyMode">The concurrency mode of this property.</param>
        /// <returns>Created structural property.</returns>
        public EdmStructuralProperty AddStructuralProperty(string name, IEdmTypeReference type, string defaultValue, EdmConcurrencyMode concurrencyMode)
        {
            EdmStructuralProperty property = new EdmStructuralProperty(this, name, type, defaultValue, concurrencyMode);
            this.AddProperty(property);
            return property;
        }

        /// <summary>
        /// Searches for a structural or navigation property with the given name in this type and all base types and returns null if no such property exists.
        /// </summary>
        /// <param name="name">The name of the property being found.</param>
        /// <returns>The requested property, or null if no such property exists.</returns>
        public IEdmProperty FindProperty(string name)
        {
            IEdmProperty property;
            return this.PropertiesDictionary.TryGetValue(name, out property) ? property : null;
        }

        /// <summary>
        /// Computes the the cached dictionary of properties for this type definition.
        /// </summary>
        /// <returns>Dictionary of properties keyed by their name.</returns>
        private IDictionary<string, IEdmProperty> ComputePropertiesDictionary()
        {
            Dictionary<string, IEdmProperty> properties = new Dictionary<string, IEdmProperty>();
            foreach (IEdmProperty property in this.Properties())
            {
                RegistrationHelper.RegisterProperty(property, property.Name, properties);
            }

            return properties;
        }
    }
}

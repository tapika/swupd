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

namespace System.Data.Services.Common
{
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Values;
    using Error = System.Data.Services.Client.Error;
    using ErrorStrings = System.Data.Services.Client.Strings;

    /// <summary>
    /// Component for controlling what convention set is used for generating URLs.
    /// </summary>
    public sealed class DataServiceUrlConventions
    {
        /// <summary>Singleton instance of the default conventions.</summary>
        private static readonly DataServiceUrlConventions DefaultInstance = new DataServiceUrlConventions(UrlConvention.CreateWithExplicitValue(/*generateKeyAsSegment*/ false));

        /// <summary>Singleton instance of the key-as-segment conventions.</summary>
        private static readonly DataServiceUrlConventions KeyAsSegmentInstance = new DataServiceUrlConventions(UrlConvention.CreateWithExplicitValue(/*generateKeyAsSegment*/ true));

        /// <summary>The key serializer to use.</summary>
        private readonly KeySerializer keySerializer;

        /// <summary>The url convention to use.</summary>
        private readonly UrlConvention urlConvention;

        /// <summary>
        /// Prevents a default instance of the <see cref="DataServiceUrlConventions"/> class from being created.
        /// </summary>
        /// <param name="urlConvention">The url convention to use.</param>
        private DataServiceUrlConventions(UrlConvention urlConvention)
        {
            Debug.Assert(urlConvention != null, "urlConvention != null");
            this.urlConvention = urlConvention;
            this.keySerializer = KeySerializer.Create(urlConvention);
        }

        /// <summary>
        /// An instance of <see cref="DataServiceUrlConventions"/> which uses default URL conventions. Specifically, this instance will produce keys that use parentheses like "Customers('ALFKI')".
        /// </summary>
        public static DataServiceUrlConventions Default
        {
            get { return DefaultInstance; }
        }

        /// <summary>
        /// An instance of <see cref="DataServiceUrlConventions"/> which uses key-as-segment URL conventions. Specifically, this instance will produce keys that use segments like "Customers/ALFKI".
        /// </summary>
        public static DataServiceUrlConventions KeyAsSegment
        {
            get { return KeyAsSegmentInstance; }
        }

        /// <summary>
        /// Appends the key expression for the given entity to the given <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="entity">The entity to build the key expression from.</param>
        /// <param name="builder">The builder to append onto.</param>
        internal void AppendKeyExpression(IEdmStructuredValue entity, StringBuilder builder)
        {
            Debug.Assert(entity != null, "entity != null");
            Debug.Assert(builder != null, "builder != null");

            IEdmEntityTypeReference edmEntityTypeReference = entity.Type as IEdmEntityTypeReference;
            if (edmEntityTypeReference == null || !edmEntityTypeReference.Key().Any())
            {
                throw Error.Argument(ErrorStrings.Content_EntityWithoutKey, "entity");
            }

            this.AppendKeyExpression(edmEntityTypeReference.Key().ToList(), p => p.Name, p => GetPropertyValue(entity.FindPropertyValue(p.Name), entity.Type), builder);
        }

        /// <summary>
        /// Appends the key expression for the given entity to the given <see cref="StringBuilder"/>
        /// </summary>
        /// <typeparam name="T">The type of the properties.</typeparam>
        /// <param name="keyProperties">The properties of the key.</param>
        /// <param name="getPropertyName">Delegate to get the name of a property.</param>
        /// <param name="getValueForProperty">Delegate to get the value of a property.</param>
        /// <param name="builder">The builder to append onto.</param>
        internal void AppendKeyExpression<T>(ICollection<T> keyProperties, Func<T, string> getPropertyName, Func<T, object> getValueForProperty, StringBuilder builder)
        {
            Func<T, object> getValueForPropertyWithNullCheck = p =>
            {
                Debug.Assert(getValueForProperty != null, "getValueForProperty != null");
                object propertyValue = getValueForProperty(p);
                if (propertyValue == null)
                {
                    throw Error.InvalidOperation(Client.Strings.Context_NullKeysAreNotSupported(getPropertyName(p)));
                }

                return propertyValue;
            };
            this.keySerializer.AppendKeyExpression(builder, keyProperties, getPropertyName, getValueForPropertyWithNullCheck);
        }

        /// <summary>
        /// Adds the required headers for the url convention.
        /// </summary>
        /// <param name="requestHeaders">The request headers to add to.</param>
        internal void AddRequiredHeaders(HeaderCollection requestHeaders)
        {
            this.urlConvention.AddRequiredHeaders(requestHeaders);
        }

        /// <summary>
        /// Gets the raw CLR value for the given <see cref="IEdmPropertyValue"/>.
        /// </summary>
        /// <param name="property">The property to get the value for.</param>
        /// <param name="type">The type which declared the property.</param>
        /// <returns>The raw CLR value of the property.</returns>
        private static object GetPropertyValue(IEdmPropertyValue property, IEdmTypeReference type)
        {
            Debug.Assert(property != null, "property != null");
            IEdmValue propertyValue = property.Value;

            // DEVNOTE: though this check is not strictly necessary, and would be caught by later checks,
            // it seems worthwhile to fail fast if we can.
            if (propertyValue.ValueKind == EdmValueKind.Null)
            {
                throw Error.InvalidOperation(ErrorStrings.Context_NullKeysAreNotSupported(property.Name));
            }

            var primitiveValue = propertyValue as IEdmPrimitiveValue;
            if (primitiveValue == null)
            {
                throw Error.InvalidOperation(ErrorStrings.ClientType_KeysMustBeSimpleTypes(type.FullName()));
            }

            // DEVNOTE: This can return null, and will be handled later. The reason for this is that the client
            // and server have different ways of getting property values, but both will eventually hit the same 
            // codepath and that is where the null is handled.
            return primitiveValue.ToClrValue();
        }
    }
}

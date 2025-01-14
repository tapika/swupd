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

namespace System.Data.Services.Client.Metadata
{
    #region Namespaces.

    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using c = System.Data.Services.Client;

    #endregion Namespaces.

    /// <summary>
    /// Utility methods for client types.
    /// </summary>
    internal static class ClientTypeUtil
    {
        /// <summary>A static empty PropertyInfo array.</summary>
        internal static readonly PropertyInfo[] EmptyPropertyInfoArray = new PropertyInfo[0];

        /// <summary>
        /// Enumeration for the kind of key
        /// </summary>
        private enum KeyKind
        {
            /// <summary>If this is not a key </summary>
            NotKey = 0,

            /// <summary> If the key property name was equal to ID </summary>
            Id = 1,

            /// <summary> If the key property name was equal to TypeName+ID </summary>
            TypeNameId = 2,

            /// <summary> if the key property was attributed </summary>
            AttributedKey = 3,
        }

        /// <summary>
        /// Sets the single instance of <see cref="ClientTypeAnnotation"/> on the given instance of <paramref name="edmType"/>.
        /// </summary>
        /// <param name="model">The model the <paramref name="edmType"/> belongs to.</param>
        /// <param name="edmType">IEdmType instance to set the annotation on.</param>
        /// <param name="annotation">The annotation to set</param>
        internal static void SetClientTypeAnnotation(this IEdmModel model, IEdmType edmType, ClientTypeAnnotation annotation)
        {
            Debug.Assert(model != null, "model != null");
            Debug.Assert(edmType != null, "edmType != null");
            Debug.Assert(annotation != null, "annotation != null");
            model.SetAnnotationValue<ClientTypeAnnotation>(edmType, annotation);
        }

        /// <summary>
        /// Gets the ClientTypeAnnotation for the given type.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="type">Type for which the annotation needs to be returned.</param>
        /// <returns>An instance of ClientTypeAnnotation containing metadata about the given type.</returns>
        internal static ClientTypeAnnotation GetClientTypeAnnotation(this ClientEdmModel model, Type type)
        {
            Debug.Assert(model != null, "model != null");
            Debug.Assert(type != null, "type != null");

            IEdmType edmType = model.GetOrCreateEdmType(type);
            return model.GetClientTypeAnnotation(edmType);
        }

        /// <summary>
        /// Gets the single instance of <see cref="ClientTypeAnnotation"/> from the given instance of <paramref name="edmType"/>.
        /// </summary>
        /// <param name="model">The model the <paramref name="edmType"/> belongs to.</param>
        /// <param name="edmType">IEdmType instance to get the annotation.</param>
        /// <returns>Returns the single instance of <see cref="ClientTypeAnnotation"/> from the given instance of <paramref name="edmType"/>.</returns>
        internal static ClientTypeAnnotation GetClientTypeAnnotation(this IEdmModel model, IEdmType edmType)
        {
            Debug.Assert(model != null, "model != null");
            Debug.Assert(edmType != null, "edmType != null");

            return model.GetAnnotationValue<ClientTypeAnnotation>(edmType);
        }

        /// <summary>
        /// Sets the given instance of <paramref name="annotation"/> to the given instance of <paramref name="edmProperty"/>.
        /// </summary>
        /// <param name="edmProperty">IEdmProperty instance to set the annotation.</param>
        /// <param name="annotation">Annotation instance to set.</param>
        internal static void SetClientPropertyAnnotation(this IEdmProperty edmProperty, ClientPropertyAnnotation annotation)
        {
            Debug.Assert(edmProperty != null, "edmProperty != null");
            Debug.Assert(annotation != null, "annotation != null");
            annotation.Model.SetAnnotationValue<ClientPropertyAnnotation>(edmProperty, annotation);
        }

        /// <summary>
        /// Gets the single instance of ClientPropertyAnnotation from the given instance of <paramref name="edmProperty"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="edmProperty">IEdmProperty instance to get the annotation.</param>
        /// <returns>Returns the single instance of ClientPropertyAnnotation from the given instance of <paramref name="edmProperty"/>.</returns>
        internal static ClientPropertyAnnotation GetClientPropertyAnnotation(this IEdmModel model, IEdmProperty edmProperty)
        {
            Debug.Assert(model != null, "model != null");
            Debug.Assert(edmProperty != null, "edmProperty != null");

            return model.GetAnnotationValue<ClientPropertyAnnotation>(edmProperty);
        }

        /// <summary>
        /// Gets the instance of ClientTypeAnnotation from the given instance of <paramref name="edmProperty"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="edmProperty">IEdmProperty instance to get the annotation.</param>
        /// <returns>Returns the instance of ClientTypeAnnotation from the given instance of <paramref name="edmProperty"/>.</returns>
        internal static ClientTypeAnnotation GetClientTypeAnnotation(this IEdmModel model, IEdmProperty edmProperty)
        {
            Debug.Assert(model != null, "model != null");
            Debug.Assert(edmProperty != null, "edmProperty != null");

            IEdmType edmType = edmProperty.Type.Definition;
            Debug.Assert(edmType != null, "edmType != null");

            return model.GetAnnotationValue<ClientTypeAnnotation>(edmType);
        }

        /// <summary>
        /// Returns the corresponding edm type reference for the given edm type.
        /// </summary>
        /// <param name="edmType">EdmType instance.</param>
        /// <param name="isNullable">A boolean value indicating whether the clr type of this edm type is nullable</param>
        /// <returns>Returns the corresponding edm type reference for the given edm type.</returns>
        internal static IEdmTypeReference ToEdmTypeReference(this IEdmType edmType, bool isNullable)
        {
            return EdmLibraryExtensions.ToTypeReference(edmType, isNullable);
        }

        /// <summary>
        /// Returns the full name for the given edm type
        /// </summary>
        /// <param name="edmType">EdmType instance.</param>
        /// <returns>the full name of the edmType.</returns>
        internal static string FullName(this IEdmType edmType)
        {
            IEdmSchemaElement schemaElement = edmType as IEdmSchemaElement;
            if (schemaElement != null)
            {
                return schemaElement.FullName();
            }

            return null;
        }

        /// <summary>
        /// Determine whether the type is primitive or primitive collection.
        /// </summary>
        /// <param name="edmType">The type to be evaluated.</param>
        /// <returns>True if the type is primitive or primitive collection.</returns>
        internal static bool IsPrimitive(this IEdmType edmType)
        {
            IEdmCollectionType collectionType = edmType as IEdmCollectionType;
            IEdmType type = (collectionType != null) ? collectionType.ElementType.Definition : edmType;

            return type.TypeKind == EdmTypeKind.Primitive;
        }

        /// <summary>
        /// Returns MethodInfo instance for a generic type retrieved by using <paramref name="methodName"/> and gets 
        /// element type for the provided <paramref name="genericTypeDefinition"/>.
        /// </summary>
        /// <param name="propertyType">starting type</param>
        /// <param name="genericTypeDefinition">the generic type definition to find</param>
        /// <param name="methodName">the method to search for</param>
        /// <param name="type">the element type for <paramref name="genericTypeDefinition" /> if found</param>
        /// <returns>element types</returns>
        internal static MethodInfo GetMethodForGenericType(Type propertyType, Type genericTypeDefinition, string methodName, out Type type)
        {
            Debug.Assert(null != propertyType, "null propertyType");
            Debug.Assert(null != genericTypeDefinition, "null genericTypeDefinition");
            Debug.Assert(genericTypeDefinition.IsGenericTypeDefinition(), "!IsGenericTypeDefinition");

            type = null;

            Type implementationType = ClientTypeUtil.GetImplementationType(propertyType, genericTypeDefinition);
            if (null != implementationType)
            {
                Type[] genericArguments = implementationType.GetGenericArguments();
                MethodInfo methodInfo = implementationType.GetMethod(methodName);
                Debug.Assert(null != methodInfo, "should have found the method");

#if DEBUG
                Debug.Assert(null != genericArguments, "null genericArguments");
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (0 < parameters.Length)
                {
                    // following assert was disabled for Contains which returns bool
                    //// Debug.Assert(typeof(void) == methodInfo.ReturnParameter.ParameterType, "method doesn't return void");

                    Debug.Assert(genericArguments.Length == parameters.Length, "genericArguments don't match parameters");
                    for (int i = 0; i < genericArguments.Length; ++i)
                    {
                        Debug.Assert(genericArguments[i] == parameters[i].ParameterType, "parameter doesn't match generic argument");
                    }
                }
#endif
                type = genericArguments[genericArguments.Length - 1];
                return methodInfo;
            }

            return null;
        }

        /// <summary>Gets a delegate that can be invoked to add an item to a collection of the specified type.</summary>
        /// <param name='listType'>Type of list to use.</param>
        /// <returns>The delegate to invoke.</returns>
        internal static Action<object, object> GetAddToCollectionDelegate(Type listType)
        {
            Debug.Assert(listType != null, "listType != null");

            Type listElementType;
            MethodInfo addMethod = ClientTypeUtil.GetAddToCollectionMethod(listType, out listElementType);
            ParameterExpression list = Expression.Parameter(typeof(object), "list");
            ParameterExpression item = Expression.Parameter(typeof(object), "element");
            Expression body = Expression.Call(Expression.Convert(list, listType), addMethod, Expression.Convert(item, listElementType));
#if ASTORIA_LIGHT
            LambdaExpression lambda = ExpressionHelpers.CreateLambda(body, list, item);
#else
            LambdaExpression lambda = Expression.Lambda(body, list, item);
#endif
            return (Action<object, object>)lambda.Compile();
        }

        /// <summary>
        /// Gets the Add method to add items to a collection of the specified type.
        /// </summary>
        /// <param name="collectionType">Type for the collection.</param>
        /// <param name="type">The element type in the collection if found; null otherwise.</param>
        /// <returns>The method to invoke to add to a collection of the specified type.</returns>
        internal static MethodInfo GetAddToCollectionMethod(Type collectionType, out Type type)
        {
            return ClientTypeUtil.GetMethodForGenericType(collectionType, typeof(ICollection<>), "Add", out type);
        }

        /// <summary>
        /// get concrete type that implements the genericTypeDefinition
        /// </summary>
        /// <param name="type">starting type</param>
        /// <param name="genericTypeDefinition">the generic type definition to find</param>
        /// <returns>concrete type that implementats the generic type</returns>
        internal static Type GetImplementationType(Type type, Type genericTypeDefinition)
        {
            if (IsConstructedGeneric(type, genericTypeDefinition))
            {   // propertyType is genericTypeDefinition (e.g. ICollection<T>)
                return type;
            }
            else
            {
                Type implementationType = null;
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (IsConstructedGeneric(interfaceType, genericTypeDefinition))
                    {
                        if (null == implementationType)
                        {   // found implmentation of genericTypeDefinition (e.g. ICollection<T>)
                            implementationType = interfaceType;
                        }
                        else
                        {   // Multiple implementations (e.g. ICollection<int> and ICollection<int?>)
                            throw c.Error.NotSupported(c.Strings.ClientType_MultipleImplementationNotSupported);
                        }
                    }
                }

                return implementationType;
            }
        }

        /// <summary>
        /// Is the type an Entity Type?
        /// </summary>
        /// <param name="t">Type to examine</param>
        /// <param name="model">The client model.</param>
        /// <returns>bool indicating whether or not entity type</returns>
        internal static bool TypeIsEntity(Type t, ClientEdmModel model)
        {
            return model.GetOrCreateEdmType(t).TypeKind == EdmTypeKind.Entity;
        }

        /// <summary>
        /// Is the type or element type (in the case of nullableOfT or IEnumOfT) a Entity Type?
        /// </summary>
        /// <param name="type">Type to examine</param>
        /// <returns>bool indicating whether or not entity type</returns>
        internal static bool TypeOrElementTypeIsEntity(Type type)
        {
            type = TypeSystem.GetElementType(type);
            type = Nullable.GetUnderlyingType(type) ?? type;
            return !PrimitiveType.IsKnownType(type) && ClientTypeUtil.GetKeyPropertiesOnType(type) != null;
        }

        /// <summary>Checks whether the specified type is a DataServiceCollection type (or inherits from one).</summary>
        /// <param name='type'>Type to check.</param>
        /// <returns>true if the type inherits from DataServiceCollection; false otherwise.</returns>
        internal static bool IsDataServiceCollection(Type type)
        {
            while (type != null)
            {
                if (c.PlatformHelper.IsGenericType(type) && WebUtil.IsDataServiceCollectionType(type.GetGenericTypeDefinition()))
                {
                    return true;
                }

                type = c.PlatformHelper.GetBaseType(type);
            }

            return false;
        }

        /// <summary>Whether a variable of <paramref name="type"/> can be assigned null.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns>true if a variable of type <paramref name="type"/> can be assigned null; false otherwise.</returns>
        internal static bool CanAssignNull(Type type)
        {
            Debug.Assert(type != null, "type != null");
            return !type.IsValueType() || (type.IsGenericType() && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>Returns the list of properties defined on <paramref name="type"/>.</summary>
        /// <param name="type">Type instance in question.</param>
        /// <param name="declaredOnly">True to to get the properties declared on <paramref name="type"/>; false to get all properties defined on <paramref name="type"/>.</param>
        /// <returns>Returns the list of properties defined on <paramref name="type"/>.</returns>
        internal static IEnumerable<PropertyInfo> GetPropertiesOnType(Type type, bool declaredOnly)
        {
            string typeName = type.ToString();

            if (!PrimitiveType.IsKnownType(type))
            {
                foreach (PropertyInfo propertyInfo in type.GetPublicProperties(true /*instanceOnly*/, declaredOnly))
                {
                    //// examples where class<PropertyType>

                    //// the normal examples
                    //// PropertyType Property { get; set }
                    //// Nullable<PropertyType> Property { get; set; }

                    //// if 'Property: struct' then we would be unable set the property during construction (and have them stick)
                    //// but when its a class, we can navigate if non-null and set the nested properties
                    //// PropertyType Property { get; } where PropertyType: class

                    //// we do support adding elements to collections
                    //// ICollection<PropertyType> { get; /*ignored set;*/ }

                    //// indexed properties are not suported because 
                    //// we don't have anything to use as the index
                    //// PropertyType Property[object x] { /*ignored get;*/ /*ignored set;*/ }

                    //// also ignored 
                    //// if PropertyType.IsPointer (like byte*)
                    //// if PropertyType.IsArray except for byte[] and char[]
                    //// if PropertyType == IntPtr or UIntPtr

                    //// Properties overriding abstract or virtual properties on a base type
                    //// are also ignored (because they are part of the base type declaration
                    //// and not of the derived type).

                    Type propertyType = propertyInfo.PropertyType; // class / interface / value
                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    if (propertyType.IsPointer ||
                        (propertyType.IsArray && (typeof(byte[]) != propertyType) && typeof(char[]) != propertyType) ||
                        (typeof(IntPtr) == propertyType) ||
                        (typeof(UIntPtr) == propertyType))
                    {
                        continue;
                    }

                    // Ignore properties overriding abstract/virtual properties of a base type
                    // when only getting the declared properties (otherwise the property will 
                    // only be included once in the property list anyways).
                    if (declaredOnly && IsOverride(type, propertyInfo))
                    {
                        continue;
                    }

                    Debug.Assert(!propertyType.ContainsGenericParameters(), "remove when test case is found that encounters this");

                    if (propertyInfo.CanRead &&
                        (!propertyType.IsValueType() || propertyInfo.CanWrite) &&
                        !propertyType.ContainsGenericParameters() &&
                        (0 == propertyInfo.GetIndexParameters().Length))
                    {
                        yield return propertyInfo;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of key properties defined on <paramref name="type"/>; null if <paramref name="type"/> is complex.
        /// </summary>
        /// <param name="type">Type in question.</param>
        /// <returns>Returns the list of key properties defined on <paramref name="type"/>; null if <paramref name="type"/> is complex.</returns>
        internal static PropertyInfo[] GetKeyPropertiesOnType(Type type)
        {
            bool hasProperties;
            return GetKeyPropertiesOnType(type, out hasProperties);
        }

        /// <summary>
        /// Returns the list of key properties defined on <paramref name="type"/>; null if <paramref name="type"/> is complex.
        /// </summary>
        /// <param name="type">Type in question.</param>
        /// <param name="hasProperties">true if <paramref name="type"/> has any (declared or inherited) properties; otherwise false.</param>
        /// <returns>Returns the list of key properties defined on <paramref name="type"/>; null if <paramref name="type"/> is complex.</returns>
        internal static PropertyInfo[] GetKeyPropertiesOnType(Type type, out bool hasProperties)
        {
            if (CommonUtil.IsUnsupportedType(type))
            {
                throw new InvalidOperationException(c.Strings.ClientType_UnsupportedType(type));
            }

            string typeName = type.ToString();
            IEnumerable<object> customAttributes = type.GetCustomAttributes(true);
            bool isEntity = customAttributes.OfType<DataServiceEntityAttribute>().Any();
            DataServiceKeyAttribute dataServiceKeyAttribute = customAttributes.OfType<DataServiceKeyAttribute>().FirstOrDefault();
            List<PropertyInfo> keyProperties = new List<PropertyInfo>();
            PropertyInfo[] properties = ClientTypeUtil.GetPropertiesOnType(type, false /*declaredOnly*/).ToArray();
            hasProperties = properties.Length > 0;

            KeyKind currentKeyKind = KeyKind.NotKey;
            KeyKind newKeyKind = KeyKind.NotKey;
            foreach (PropertyInfo propertyInfo in properties)
            {
                if ((newKeyKind = ClientTypeUtil.IsKeyProperty(propertyInfo, dataServiceKeyAttribute)) != KeyKind.NotKey)
                {
                    if (newKeyKind > currentKeyKind)
                    {
                        keyProperties.Clear();
                        currentKeyKind = newKeyKind;
                        keyProperties.Add(propertyInfo);
                    }
                    else if (newKeyKind == currentKeyKind)
                    {
                        keyProperties.Add(propertyInfo);
                    }
                }
            }

            Type keyPropertyDeclaringType = null;
            foreach (PropertyInfo key in keyProperties)
            {
                if (null == keyPropertyDeclaringType)
                {
                    keyPropertyDeclaringType = key.DeclaringType;
                }
                else if (keyPropertyDeclaringType != key.DeclaringType)
                {
                    throw c.Error.InvalidOperation(c.Strings.ClientType_KeysOnDifferentDeclaredType(typeName));
                }

                if (!PrimitiveType.IsKnownType(key.PropertyType))
                {
                    throw c.Error.InvalidOperation(c.Strings.ClientType_KeysMustBeSimpleTypes(typeName));
                }
            }

            if (newKeyKind == KeyKind.AttributedKey && keyProperties.Count != dataServiceKeyAttribute.KeyNames.Count)
            {
                var m = (from string a in dataServiceKeyAttribute.KeyNames
                         where null == (from b in properties
                                        where b.Name == a
                                        select b).FirstOrDefault()
                         select a).First<string>();
                throw c.Error.InvalidOperation(c.Strings.ClientType_MissingProperty(typeName, m));
            }

            return keyProperties.Count > 0 ? keyProperties.ToArray() : (isEntity ? ClientTypeUtil.EmptyPropertyInfoArray : null);
        }

        /// <summary>Gets the type of the specified <paramref name="member"/>.</summary>
        /// <param name="member">Member to get type of (typically PropertyInfo or FieldInfo).</param>
        /// <returns>The type of property or field type.</returns>
        internal static Type GetMemberType(MemberInfo member)
        {
            Debug.Assert(member != null, "member != null");

            PropertyInfo propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
            {
                return propertyInfo.PropertyType;
            }

            FieldInfo fieldInfo = member as FieldInfo;
            Debug.Assert(fieldInfo != null, "fieldInfo != null -- otherwise Expression.Member factory should have thrown an argument exception");
            return fieldInfo.FieldType;
        }

        /// <summary>
        /// Returns the KeyKind if <paramref name="propertyInfo"/> is declared as a key in <paramref name="dataServiceKeyAttribute"/> or it follows the key naming convension.
        /// </summary>
        /// <param name="propertyInfo">Property in question.</param>
        /// <param name="dataServiceKeyAttribute">DataServiceKeyAttribute instance.</param>
        /// <returns>Returns the KeyKind if <paramref name="propertyInfo"/> is declared as a key in <paramref name="dataServiceKeyAttribute"/> or it follows the key naming convension.</returns>
        private static KeyKind IsKeyProperty(PropertyInfo propertyInfo, DataServiceKeyAttribute dataServiceKeyAttribute)
        {
            Debug.Assert(propertyInfo != null, "propertyInfo != null");

            string propertyName = propertyInfo.Name;
            KeyKind keyKind = KeyKind.NotKey;
            if (dataServiceKeyAttribute != null && dataServiceKeyAttribute.KeyNames.Contains(propertyName))
            {
                keyKind = KeyKind.AttributedKey;
            }
            else if (propertyName.EndsWith("ID", StringComparison.Ordinal))
            {
                string declaringTypeName = propertyInfo.DeclaringType.Name;
                if ((propertyName.Length == (declaringTypeName.Length + 2)) && propertyName.StartsWith(declaringTypeName, StringComparison.Ordinal))
                {
                    // matched "DeclaringType.Name+ID" pattern
                    keyKind = KeyKind.TypeNameId;
                }
                else if (2 == propertyName.Length)
                {
                    // matched "ID" pattern
                    keyKind = KeyKind.Id;
                }
            }

            return keyKind;
        }

        /// <summary>
        /// Checks whether the specified <paramref name="type"/> is a 
        /// closed constructed type of the generic type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="genericTypeDefinition">Generic type for checkin.</param>
        /// <returns>true if <paramref name="type"/> is a constructed type of <paramref name="genericTypeDefinition"/>.</returns>
        /// <remarks>The check is an immediate check; no inheritance rules are applied.</remarks>
        private static bool IsConstructedGeneric(Type type, Type genericTypeDefinition)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(genericTypeDefinition != null, "genericTypeDefinition != null");

            return type.IsGenericType() && (type.GetGenericTypeDefinition() == genericTypeDefinition) && !type.ContainsGenericParameters();
        }

        /// <summary>
        /// Determines whether the <paramref name="propertyInfo"/> declared on <paramref name="type"/>
        /// overrides a (virtual/abstract) property of a base type.
        /// </summary>
        /// <param name="type">The declaring type of the property.</param>
        /// <param name="propertyInfo">The property to check.</param>
        /// <returns>true if <paramref name="propertyInfo"/> overrides a property on a base types; otherwise false.</returns>
        private static bool IsOverride(Type type, PropertyInfo propertyInfo)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(propertyInfo != null, "propertyInfo != null");

            // We only check the getter method; if a property does not have a getter method we don't consider it
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod != null && getMethod.GetBaseDefinition().DeclaringType != type)
            {
                return true;
            }

            return false;
        }
    }
}

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
    using System.Data.Services.Client;
    using System.Data.Services.Client.Providers;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using c = System.Data.Services.Client;

    #endregion Namespaces.

    /// <summary>
    /// Caches wire type names and their mapped client CLR types.
    /// </summary>
    [DebuggerDisplay("{PropertyName}")]
    internal static class ClientTypeCache
    {
        /// <summary>cache &lt;T&gt; and wireName to mapped type</summary>
        private static readonly Dictionary<TypeName, Type> namedTypes = new Dictionary<TypeName, Type>(new TypeNameEqualityComparer());

#if !ASTORIA_LIGHT && !PORTABLELIB
        /// <summary>
        /// resolve the wireName/userType pair to a CLR type
        /// </summary>
        /// <param name="wireName">type name sent by server</param>
        /// <param name="userType">type passed by user or on propertyType from a class</param>
        /// <returns>mapped clr type</returns>
        internal static Type ResolveFromName(string wireName, Type userType)
#else
        /// <summary>
        /// resolve the wireName/userType pair to a CLR type
        /// </summary>
        /// <param name="wireName">type name sent by server</param>
        /// <param name="userType">type passed by user or on propertyType from a class</param>
        /// <param name="contextType">typeof context for strongly typed assembly</param>
        /// <returns>mapped clr type</returns>
        internal static Type ResolveFromName(string wireName, Type userType, Type contextType)
#endif
        {
            Type foundType;

            TypeName typename;
            typename.Type = userType;
            typename.Name = wireName;

            // search the "wirename"-userType key in type cache
            bool foundInCache;
            lock (ClientTypeCache.namedTypes)
            {
                foundInCache = ClientTypeCache.namedTypes.TryGetValue(typename, out foundType);
            }

            // at this point, if we have seen this type before, we either have the resolved type "foundType", 
            // or we have tried to resolve it before but did not success, in which case foundType will be null.
            // Either way we should return what's in the cache since the result is unlikely to change.
            // We only need to keep on searching if there isn't an entry in the cache.
            if (!foundInCache)
            {
                string name = wireName;
                int index = wireName.LastIndexOf('.');
                if ((0 <= index) && (index < wireName.Length - 1))
                {
                    name = wireName.Substring(index + 1);
                }

                if (userType.Name == name)
                {
                    foundType = userType;
                }
                else
                {
#if !ASTORIA_LIGHT && !PORTABLELIB
                    // searching only loaded assemblies, not referenced assemblies
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
#else
                    foreach (Assembly assembly in new Assembly[] { userType.GetAssembly(), contextType.GetAssembly() }.Distinct())
#endif
                    {
                        Type found = assembly.GetType(wireName, false);
                        ResolveSubclass(name, userType, found, ref foundType);

                        if (null == found)
                        {
                            IEnumerable<Type> types = null;
                            try
                            {
                                types = assembly.GetTypes();
                            }
                            catch (ReflectionTypeLoadException)
                            {
                            }

                            if (null != types)
                            {
                                foreach (Type t in types)
                                {
                                    ResolveSubclass(name, userType, t, ref foundType);
                                }
                            }
                        }
                    }
                }

                // The above search can all fail and leave "foundType" to be null
                // we should cache this result too so we won't waste time searching again.
                lock (ClientTypeCache.namedTypes)
                {
                    ClientTypeCache.namedTypes[typename] = foundType;
                }
            }

            return foundType;
        }

        /// <summary>
        /// is the type a visible subclass with correct name
        /// </summary>
        /// <param name="wireClassName">type name from server</param>
        /// <param name="userType">the type from user for materialization or property type</param>
        /// <param name="type">type being tested</param>
        /// <param name="existing">the previously discovered matching type</param>
        /// <exception cref="InvalidOperationException">if the mapping is ambiguous</exception>
        private static void ResolveSubclass(string wireClassName, Type userType, Type type, ref Type existing)
        {
            if ((null != type) && c.PlatformHelper.IsVisible(type) && (wireClassName == type.Name) && userType.IsAssignableFrom(type))
            {
                if (null != existing)
                {
                    throw c.Error.InvalidOperation(c.Strings.ClientType_Ambiguous(wireClassName, userType));
                }

                existing = type;
            }
        }

        /// <summary>type + wireName combination</summary>
        private struct TypeName
        {
            /// <summary>type</summary>
            internal Type Type;

            /// <summary>type name from server</summary>
            internal string Name;
        }

        /// <summary>equality comparer for TypeName</summary>
        private sealed class TypeNameEqualityComparer : IEqualityComparer<TypeName>
        {
            /// <summary>equality comparer for TypeName</summary>
            /// <param name="x">left type</param>
            /// <param name="y">right type</param>
            /// <returns>true if x and y are equal</returns>
            public bool Equals(TypeName x, TypeName y)
            {
                return (x.Type == y.Type && x.Name == y.Name);
            }

            /// <summary>compute hashcode for TypeName</summary>
            /// <param name="obj">object to compute hashcode for</param>
            /// <returns>computed hashcode</returns>
            public int GetHashCode(TypeName obj)
            {
                return obj.Type.GetHashCode() ^ obj.Name.GetHashCode();
            }
        }
    }
}

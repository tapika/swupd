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

#if ASTORIA_CLIENT
namespace System.Data.Services.Client
#else
namespace System.Data.Services
#endif
{
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
#if !ASTORIA_LIGHT
    using System.Dynamic;
#endif
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Microsoft.Data.OData;

    /// <summary>
    /// Common defintions and functions for the server and client lib
    /// </summary>
    internal static partial class CommonUtil
    {
        /// <summary>
        /// List of types unsupported by the client
        /// </summary>
        private static readonly Type[] unsupportedTypes = new Type[]
        {
#if !ASTORIA_LIGHT  // System.Dynamic & tuples not available (as of SL 2.0)
                typeof(IDynamicMetaObjectProvider),
                typeof(Tuple<>),         // 1-Tuple
                typeof(Tuple<,>),        // 2-Tuple
                typeof(Tuple<,,>),       // 3-Tuple
                typeof(Tuple<,,,>),      // 4-Tuple
                typeof(Tuple<,,,,>),     // 5-Tuple
                typeof(Tuple<,,,,,>),    // 6-Tuple
                typeof(Tuple<,,,,,,>),   // 7-Tuple
                typeof(Tuple<,,,,,,,>)   // 8-Tuple
#endif
        };

        /// <summary>
        /// Test whether a type is unsupported by the client lib
        /// </summary>
        /// <param name="type">The type to test</param>
        /// <returns>Returns true if the type is not supported</returns>
        internal static bool IsUnsupportedType(Type type)
        {
#if ASTORIA_CLIENT
            if (type.IsGenericType())
#else
            if (type.IsGenericType)
#endif
            {
                type = type.GetGenericTypeDefinition();
            }

            if (unsupportedTypes.Any(t => t.IsAssignableFrom(type)))
            {
                return true;
            }

            Debug.Assert(!type.FullName.StartsWith("System.Tuple", StringComparison.Ordinal), "System.Tuple is not blocked by unsupported type check");
            return false;
        }

        /// <summary>
        /// Returns collection item type name or null if the provided type name is not a collection.
        /// </summary>
        /// <param name="typeName">Collection type name read from payload.</param>
        /// <param name="isNested">Whether it is a nested (recursive) call.</param>
        /// <returns>Collection element type name or null if not a collection.</returns>
        /// <remarks>
        /// The following rules are used for collection type names:
        /// - it has to start with "Collection(" and end with ")" - trailing and leading whitespaces make the type not to be recognized as collection. 
        /// - there is to be no characters (including whitespaces) between "Collection" and "(" - otherwise it won't be recognized as collection
        /// - collection item type name has to be a non-empty string - i.e. "Collection()" won't be recognized as collection
        /// - nested collection - e.g. "Collection(Collection(Edm.Int32))" - are not supported - we will throw
        /// Note the following are examples of valid type names which are not collection:
        /// - "Collection()"
        /// - " Collection(Edm.Int32)"
        /// - "Collection (Edm.Int32)"
        /// - "Collection("
        /// If the type name is not recognized as a collection it will be eventually passed to type resolver if it is not a known primitive type.
        /// </remarks>
        internal static string GetCollectionItemTypeName(string typeName, bool isNested)
        {
            // to be recognized as a collection wireTypeName must not be null, has to start with "Collection(" and end with ")" and must not be "Collection()"
            if (typeName != null && typeName.StartsWith(XmlConstants.CollectionTypeQualifier + "(", StringComparison.Ordinal) && typeName[typeName.Length - 1] == ')' && typeName.Length != (XmlConstants.CollectionTypeQualifier + "()").Length)
            {
                if (isNested)
                {
#if ASTORIA_CLIENT
                    throw Error.InvalidOperation(Strings.ClientType_CollectionOfCollectionNotSupported);
#else
                    throw DataServiceException.CreateBadRequestError(Strings.BadRequest_CollectionOfCollectionNotSupported);
#endif
                }

                string innerTypeName = typeName.Substring((XmlConstants.CollectionTypeQualifier + "(").Length, typeName.Length - (XmlConstants.CollectionTypeQualifier + "()").Length);

                // Check if it is not a nested collection and throw if it is
                GetCollectionItemTypeName(innerTypeName, true);

                return innerTypeName;
            }

            return null;
        }

#if !ASTORIA_CLIENT
        /// <summary>
        /// checks whether the given xml reader element is empty or not.
        /// This method reads over the start tag and if this returns false,
        /// one needs to throw an appropriate exception
        /// </summary>
        /// <param name="reader">reader instance.</param>
        /// <returns>true if the current element is empty. Otherwise false.</returns>
        internal static bool ReadEmptyElement(XmlReader reader)
        {
            Debug.Assert(reader.NodeType == XmlNodeType.Element, "IsEmptyElement method must be called for elements only");
            if (reader.IsEmptyElement)
            {
                return true;
            }

            if (reader.Read() && reader.NodeType == XmlNodeType.EndElement)
            {
                return true;
            }

            return false;
        }
#endif

        /// <summary>
        /// Convert the DataServiceProtocolVersion to ODataVersion.
        /// </summary>
        /// <param name="maxProtocolVersion">DataServiceProtocolVersion value to convert.</param>
        /// <returns>an ODataVersion value for the given DataServiceProtocolVersion value.</returns>
        internal static ODataVersion ConvertToODataVersion(DataServiceProtocolVersion maxProtocolVersion)
        {
            switch (maxProtocolVersion)
            {
                case DataServiceProtocolVersion.V1:
                    return ODataVersion.V1;

                case DataServiceProtocolVersion.V2:
                    return ODataVersion.V2;

                case DataServiceProtocolVersion.V3:
                    return ODataVersion.V3;

                default:
                    Debug.Assert(false, "Need to add a case for the new version that got added");
                    return (ODataVersion)(-1);
            }
        }

        /// <summary>
        /// Converts the given version instance to ODataVersion enum.
        /// </summary>
        /// <param name="version">Version instance containing the response payload.</param>
        /// <returns>ODataVersion enum value for the given version.</returns>
        internal static ODataVersion ConvertToODataVersion(Version version)
        {
            Debug.Assert(version != null, "version != null");
            if (version.Major == 1 && version.Minor == 0)
            {
                return ODataVersion.V1;
            }

            if (version.Major == 2 && version.Minor == 0)
            {
                return ODataVersion.V2;
            }

            Debug.Assert(version.Major == 3 && version.Minor == 0, "version.Major == 3 && version.Minor == 0");
            return ODataVersion.V3;
        }

        /// <summary>
        /// Gets the type name (without namespace) of the specified <paramref name="type"/>,
        /// appropriate as an externally-visible type name.
        /// </summary>
        /// <param name="type">Type to get name for.</param>
        /// <returns>The type name for <paramref name="type"/>.</returns>
        internal static string GetModelTypeName(Type type)
        {
            Debug.Assert(type != null, "type != null");
#if ASTORIA_CLIENT
            if (type.IsGenericType())
#else
            if (type.IsGenericType)
#endif
            {
                Type[] genericArguments = type.GetGenericArguments();
                StringBuilder builder = new StringBuilder(type.Name.Length * 2 * (1 + genericArguments.Length));
                if (type.IsNested)
                {
                    Debug.Assert(type.DeclaringType != null, "type.DeclaringType != null");
                    builder.Append(GetModelTypeName(type.DeclaringType));
                    builder.Append('_');
                }

                builder.Append(type.Name);
                builder.Append('[');
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(' ');
                    }

                    if (genericArguments[i].IsGenericParameter)
                    {
                        builder.Append(genericArguments[i].Name);
                    }
                    else
                    {
                        string genericNamespace = GetModelTypeNamespace(genericArguments[i]);
                        if (!String.IsNullOrEmpty(genericNamespace))
                        {
                            builder.Append(genericNamespace);
                            builder.Append('.');
                        }

                        builder.Append(GetModelTypeName(genericArguments[i]));
                    }
                }

                builder.Append(']');
                return builder.ToString();
            }
            else if (type.IsNested)
            {
                Debug.Assert(type.DeclaringType != null, "type.DeclaringType != null");
                return GetModelTypeName(type.DeclaringType) + "_" + type.Name;
            }
            else
            {
                return type.Name;
            }
        }

        /// <summary>
        /// Gets the type namespace of the specified <paramref name="type"/>,
        /// appropriate as an externally-visible type name.
        /// </summary>
        /// <param name="type">Type to get namespace for.</param>
        /// <returns>The namespace for <paramref name="type"/>.</returns>
        internal static string GetModelTypeNamespace(Type type)
        {
            Debug.Assert(type != null, "type != null");
            return type.Namespace ?? String.Empty;
        }

        /// <summary>Tries to read a WCF Data Service version string.</summary>
        /// <param name="text">Text to read.</param>
        /// <param name="result">Parsed version and trailing text.</param>
        /// <returns>true if the version was read successfully; false otherwise.</returns>
        internal static bool TryReadVersion(string text, out KeyValuePair<Version, string> result)
        {
            Debug.Assert(text != null, "text != null");

            // Separate version number and extra string.
            int separator = text.IndexOf(';');
            string versionText, libraryName;
            if (separator >= 0)
            {
                versionText = text.Substring(0, separator);
                libraryName = text.Substring(separator + 1).Trim();
            }
            else
            {
                versionText = text;
                libraryName = null;
            }

            result = default(KeyValuePair<Version, string>);
            versionText = versionText.Trim();

            // The Version constructor allows for a more complex syntax, including
            // build, revisions, and major/minor for revisions. We only take two
            // number parts separated by a single dot.
            bool dotFound = false;
            for (int i = 0; i < versionText.Length; i++)
            {
                if (versionText[i] == '.')
                {
                    if (dotFound)
                    {
                        return false;
                    }

                    dotFound = true;
                }
                else if (versionText[i] < '0' || versionText[i] > '9')
                {
                    return false;
                }
            }

            try
            {
                result = new KeyValuePair<Version, string>(new Version(versionText), libraryName);
                return true;
            }
            catch (Exception e)
            {
                if (CommonUtil.IsCatchableExceptionType(e) &&
                    (e is FormatException || e is OverflowException || e is ArgumentException))
                {
                    return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Set the message quota limits for WCF Data services server.
        /// </summary>
        /// <param name="messageQuotas">Instance of ODataMessageQuotas.</param>
        internal static void SetDefaultMessageQuotas(ODataMessageQuotas messageQuotas)
        {
            // NOTE: the size of the input message is only limited by the WCF message size in Astoria
            // In WCF DS client, we never had a limit on any of these. Hence for client, it makes sense
            // to set these values to some high limit. In WCF DS server, there are bunch of API's to 
            // cover some of these limits and if we pass the value to ODL, for batch requests, there is
            // a breaking change, since WCF DS server cannot figure out why the exception was thrown and
            // and hence fail way early. For now, the best way is to tell ODL to not impose any limits
            // and WCF DS server imposes the limits in its own way.
            messageQuotas.MaxReceivedMessageSize = long.MaxValue;
            messageQuotas.MaxPartsPerBatch = int.MaxValue;
            messageQuotas.MaxOperationsPerChangeset = int.MaxValue;
            messageQuotas.MaxNestingDepth = int.MaxValue;
            messageQuotas.MaxEntityPropertyMappingsPerType = int.MaxValue;
        }
    }
}

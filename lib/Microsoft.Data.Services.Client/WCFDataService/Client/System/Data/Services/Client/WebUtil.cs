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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.Data.OData;

    /// <summary>web utility functions</summary>
    internal static partial class WebUtil
    {
        /// <summary>
        /// Default buffer size used for stream copy.
        /// </summary>
        internal const int DefaultBufferSizeForStreamCopy = 64 * 1024;

        /// <summary>
        /// Whether DataServiceCollection&lt;&gt; type is available. 
        /// </summary>
        private static bool? dataServiceCollectionAvailable = null;

        /// <summary>Method info for GetDefaultValue&lt;T&gt;.</summary>
#if WINRT
        private static MethodInfo getDefaultValueMethodInfo = typeof(WebUtil).GetMethodWithGenericArgs("GetDefaultValue", false /*isPublic*/, true /*isStatic*/, 1 /*genericArgCount*/);
#else
        private static MethodInfo getDefaultValueMethodInfo = (MethodInfo)typeof(WebUtil).GetMember("GetDefaultValue", BindingFlags.NonPublic | BindingFlags.Static).Single(m => ((MethodInfo)m).GetGenericArguments().Count() == 1);
#endif

        /// <summary>
        /// Returns true if DataServiceCollection&lt;&gt; type is available or false otherwise.
        /// </summary>
        private static bool DataServiceCollectionAvailable
        {
            get
            {
                if (dataServiceCollectionAvailable == null)
                {
                    try
                    {
                        dataServiceCollectionAvailable = GetDataServiceCollectionOfTType() != null;
                    }
                    catch (FileNotFoundException)
                    {
                        // the assembly or one of its dependencies (read: WindowsBase.dll) was not found. DataServiceCollection is not available.
                        dataServiceCollectionAvailable = false;
                    }
                }

                Debug.Assert(dataServiceCollectionAvailable != null, "observableCollectionOfTAvailable must not be null here.");

                return (bool)dataServiceCollectionAvailable;
            }
        }

        /// <summary>copy from one stream to another</summary>
        /// <param name="input">input stream</param>
        /// <param name="output">output stream</param>
        /// <param name="refBuffer">reusable buffer</param>
        /// <returns>count of copied bytes</returns>
        internal static long CopyStream(Stream input, Stream output, ref byte[] refBuffer)
        {
            Debug.Assert(null != input, "null input stream");
            Debug.Assert(null != output, "null output stream");

            long total = 0;
            byte[] buffer = refBuffer;
            if (null == buffer)
            {
                refBuffer = buffer = new byte[1000];
            }

            int count = 0;
            while (input.CanRead && (0 < (count = input.Read(buffer, 0, buffer.Length))))
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }

        /// <summary>get response object from possible WebException</summary>
        /// <param name="exception">exception to probe</param>
        /// <param name="response">http web respose object from exception</param>
        /// <returns>an instance of InvalidOperationException.</returns>
        internal static InvalidOperationException GetHttpWebResponse(InvalidOperationException exception, ref IODataResponseMessage response)
        {
            if (null == response)
            {
                DataServiceTransportException webexception = (exception as DataServiceTransportException);
                if (null != webexception)
                {
                    response = webexception.Response;
                    return (InvalidOperationException)webexception.InnerException;
                }
            }

            return exception;
        }

        /// <summary>is this a success status code</summary>
        /// <param name="status">status code</param>
        /// <returns>true if status is between 200-299</returns>
        internal static bool SuccessStatusCode(System.Net.HttpStatusCode status)
        {
            return (200 <= (int)status && (int)status < 300);
        }

        /// <summary>
        /// Checks if the provided type is a collection type (i.e. it implements ICollection and the collection item type is not an entity).
        /// </summary>
        /// <param name="type">Type being checked.</param>
        /// <param name="model">The client model.</param>
        /// <returns>True if the CLR type is a collection compatible type. False otherwise.</returns>
        internal static bool IsCLRTypeCollection(Type type, ClientEdmModel model)
        {
            // char[] and byte[] implements ICollection<> but we should not threat them as collections since they are primitive types for us.
            if (!PrimitiveType.IsKnownNullableType(type))
            {
                Type collectionType = ClientTypeUtil.GetImplementationType(type, typeof(ICollection<>));
                if (collectionType != null)
                {
                    // collectionType is ICollection so we know that the first generic parameter 
                    // is the collection item type
                    if (!ClientTypeUtil.TypeIsEntity(collectionType.GetGenericArguments()[0], model))
                    {
                        if (model.MaxProtocolVersion <= DataServiceProtocolVersion.V2)
                        {
                            throw new InvalidOperationException(Strings.WebUtil_CollectionTypeNotSupportedInV2OrBelow(type.FullName));
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the provided type name is a name of a collection type.
        /// </summary>
        /// <param name="wireTypeName">Type name read from the payload.</param>
        /// <returns>true if <paramref name="wireTypeName"/> is a name of a collection type otherwise false.</returns>
        internal static bool IsWireTypeCollection(string wireTypeName)
        {
            return CommonUtil.GetCollectionItemTypeName(wireTypeName, false) != null;
        }

        /// <summary>
        /// Returns collection item type name or null if the provided type name is not a collection.
        /// </summary>
        /// <param name="wireTypeName">Collection type name read from payload.</param>
        /// <returns>Collection item type name or null if not a collection.</returns>
        internal static string GetCollectionItemWireTypeName(string wireTypeName)
        {
            return CommonUtil.GetCollectionItemTypeName(wireTypeName, false);
        }

        /// <summary>
        /// Resolves and creates if necessary a backing type for the <paramref name="collectionPropertyType"/>.
        /// </summary>
        /// <param name="collectionPropertyType">Type of a collection property as defined by the user - can be just an interface or generic type.</param>
        /// <param name="collectionItemType">Type of items stored in the collection.</param>
        /// <returns>Resolved concrete type that can be instantiated and will back the collection property. Can be the <paramref name="collectionPropertyType"/> type.</returns>
        internal static Type GetBackingTypeForCollectionProperty(Type collectionPropertyType, Type collectionItemType)
        {
            Debug.Assert(collectionPropertyType != null, "collectionPropertyType != null");
            Debug.Assert(ClientTypeUtil.GetImplementationType(collectionPropertyType, typeof(ICollection<>)) != null, "The type backing a collection has to implement ICollection<> interface.");
            Debug.Assert(collectionItemType != null, "collectionItemType != null");

            Type collectionBackingType = null;

            // If the user's type is an interface we are using collectionItemType we have just resolved as the generic element type of the ICollection<T> instance (i.e. T).
            // Otherwise we can use directly the type defined by the user.
            // Note that we don't check here if the type we created can be assigned to the user's type. This should be done by the caller (if requested)
            if (collectionPropertyType.IsInterface())
            {
                collectionBackingType = typeof(ObservableCollection<>).MakeGenericType(collectionItemType);
            }
            else
            {
                collectionBackingType = collectionPropertyType;
            }

            Debug.Assert(collectionBackingType != null, "The backing type for collection property must not be null at this point.");

            return collectionBackingType;
        }

        /// <summary>
        /// Checks the argument value for null and throw ArgumentNullException if it is null
        /// </summary>
        /// <typeparam name="T">type of the argument</typeparam>
        /// <param name="value">argument whose value needs to be checked</param>
        /// <param name="parameterName">name of the argument</param>
        /// <returns>returns the argument back</returns>
        internal static T CheckArgumentNull<T>([ValidatedNotNull] T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw Error.ArgumentNull(parameterName);
            }

            return value;
        }

        /// <summary>
        /// Checks if the collection is valid. Throws if the collection is not valid.
        /// </summary>
        /// <param name="collectionItemType">The type of the collection item. Can not be null.</param>
        /// <param name="propertyValue">Collection instance to be validated.</param>
        /// <param name="propertyName">The name of the property being serialized (for exception messages). Can be null if the type is not a property.</param>
        internal static void ValidateCollection(Type collectionItemType, object propertyValue, string propertyName)
        {
            Debug.Assert(collectionItemType != null, "collectionItemType != null");

            // nested collections are not supported. Need to exclude primitve types - e.g. string implements IEnumerable<char>
            if (!PrimitiveType.IsKnownNullableType(collectionItemType) &&
                collectionItemType.GetInterfaces().SingleOrDefault(t => t == typeof(IEnumerable)) != null)
            {
                throw Error.InvalidOperation(Strings.ClientType_CollectionOfCollectionNotSupported);
            }

            if (propertyValue == null)
            {
                if (propertyName != null)
                {
                    throw Error.InvalidOperation(Strings.Collection_NullCollectionNotSupported(propertyName));
                }
                else
                {
                    throw Error.InvalidOperation(Strings.Collection_NullNonPropertyCollectionNotSupported(collectionItemType));
                }
            }
        }

        /// <summary>
        /// Checks if the value of a collection item is valid. Throws if it finds the value invalid.
        /// </summary>
        /// <param name="itemValue">The value of the collection item.</param>
        /// <remarks>This method should be called first to validate all items in a collection regardless of their type.</remarks>
        internal static void ValidateCollectionItem(object itemValue)
        {
            if (itemValue == null)
            {
                throw Error.InvalidOperation(Strings.Collection_NullCollectionItemsNotSupported);
            }
        }

        /// <summary>
        /// Checks if the value of a primitive collection item is valid. Throws if it finds the value invalid.
        /// </summary>
        /// <param name="itemValue">The value of the collection item.</param>
        /// <param name="propertyName">The name of the collection property being serialized. Can be null.</param>
        /// <param name="collectionItemType">The type of the collection item as declared by the collection.</param>
        internal static void ValidatePrimitiveCollectionItem(object itemValue, string propertyName, Type collectionItemType)
        {
            Debug.Assert(itemValue != null, "itemValue != null. The ValidateDataServiceCollectionItem needs to be called first on each item.");
            Debug.Assert(collectionItemType != null, "collectionItemType != null");
            Debug.Assert(PrimitiveType.IsKnownNullableType(collectionItemType), "This method should only be called for primitive types.");

            Type itemValueType = itemValue.GetType();

            if (!PrimitiveType.IsKnownNullableType(itemValueType))
            {
                throw Error.InvalidOperation(Strings.Collection_ComplexTypesInCollectionOfPrimitiveTypesNotAllowed);
            }

            if (!collectionItemType.IsAssignableFrom(itemValueType))
            {
                if (propertyName != null)
                {
                    throw Error.InvalidOperation(Strings.WebUtil_TypeMismatchInCollection(propertyName));
                }
                else
                {
                    throw Error.InvalidOperation(Strings.WebUtil_TypeMismatchInNonPropertyCollection(collectionItemType));
                }
            }
        }

        /// <summary>
        /// Checks if the value of a complex collection item is valid. Throws if it finds the value invalid.
        /// </summary>
        /// <param name="itemValue">The value of the collection item.</param>
        /// <param name="propertyName">The name of the collection property being serialized. Can be null if the type is not a property.</param>
        /// <param name="collectionItemType">The type of the collection item as declared by the collection.</param>
        internal static void ValidateComplexCollectionItem(object itemValue, string propertyName, Type collectionItemType)
        {
            Debug.Assert(itemValue != null, "itemValue != null. The ValidateDataServiceCollectionItem needs to be called first on each item.");
            Debug.Assert(collectionItemType != null, "collectionItemType != null");
            Debug.Assert(!PrimitiveType.IsKnownNullableType(collectionItemType), "This method should only be called for complex types.");

            Type itemValueType = itemValue.GetType();

            if (PrimitiveType.IsKnownNullableType(itemValueType))
            {
                throw Error.InvalidOperation(Strings.Collection_PrimitiveTypesInCollectionOfComplexTypesNotAllowed);
            }

            if (itemValueType != collectionItemType)
            {
                if (propertyName != null)
                {
                    throw Error.InvalidOperation(Strings.WebUtil_TypeMismatchInCollection(propertyName));
                }
                else
                {
                    throw Error.InvalidOperation(Strings.WebUtil_TypeMismatchInNonPropertyCollection(collectionItemType));
                }
            }
        }

        /// <summary>
        /// Validates the value of the identity, the atom:id or DataServiceId
        /// </summary>
        /// <param name="identity">The value to validate</param>
        internal static void ValidateIdentityValue(string identity)
        {
            // here we could just assign idText to Identity
            // however we used to check for AbsoluteUri, thus we need to 
            // convert string to Uri and check for absoluteness
            Uri idUri = UriUtil.CreateUri(identity, UriKind.RelativeOrAbsolute);
            if (!idUri.IsAbsoluteUri)
            {
                throw Error.InvalidOperation(Strings.Context_TrackingExpectsAbsoluteUri);
            }
        }

        /// <summary>
        /// Validates the value of the 'Location' response header.
        /// </summary>
        /// <param name="location">the value as seen on the wire.</param>
        /// <returns>an absolute uri or </returns>
        internal static Uri ValidateLocationHeader(string location)
        {
            // We used to call the Uri constructor with the kind set to Absolute.
            // Hence now checking for the absoluteness.
            Uri locationUri = UriUtil.CreateUri(location, UriKind.RelativeOrAbsolute);
            if (!locationUri.IsAbsoluteUri)
            {
                throw Error.InvalidOperation(Strings.Context_LocationHeaderExpectsAbsoluteUri);
            }

            return locationUri;
        }

        /// <summary>
        /// Determines the value of the Prefer header based on the response preference settings. Also modifies the request version as necessary.
        /// </summary>
        /// <param name="responsePreference">The response preference setting for the request.</param>
        /// <param name="requestVersion">The request version so far, this might be modified (raised) if necessary due to response preference being applied.</param>
        /// <returns>The value of the Prefer header to apply to the request.</returns>
        internal static string GetPreferHeaderAndRequestVersion(DataServiceResponsePreference responsePreference, ref Version requestVersion)
        {
            string preferHeaderValue = null;

            if (responsePreference != DataServiceResponsePreference.None)
            {
                if (responsePreference == DataServiceResponsePreference.IncludeContent)
                {
                    preferHeaderValue = XmlConstants.HttpPreferReturnContent;
                }
                else
                {
                    Debug.Assert(responsePreference == DataServiceResponsePreference.NoContent, "Invalid value for DataServiceResponsePreference.");
                    preferHeaderValue = XmlConstants.HttpPreferReturnNoContent;
                }

                RaiseVersion(ref requestVersion, Util.DataServiceVersion3);
            }

            return preferHeaderValue;
        }

        /// <summary>
        /// Raises the version specified to the new minimal version (if it's lower than the current one)
        /// </summary>
        /// <param name="version">The version to be raised.</param>
        /// <param name="minimalVersion">The minimal version needed.</param>
        internal static void RaiseVersion(ref Version version, Version minimalVersion)
        {
            if (version == null || version < minimalVersion)
            {
                version = minimalVersion;
            }
        }

#if WINDOWS_PHONE
        /// <summary>
        /// Applies headers in the dictionary to a request message.
        /// </summary>
        /// <param name="headers">The dictionary with the headers to apply.</param>
        /// <param name="requestMessage">The request message to apply the headers to.</param>
        /// <param name="ignoreAcceptHeader">If set to true the Accept header will be ignored.</param>
        internal static void ApplyHeadersToRequest(HeaderCollection headers, IODataRequestMessage requestMessage, bool ignoreAcceptHeader)
        {
            // NetCF bug with how the enumerators for dictionaries work.
            foreach (KeyValuePair<string, string> header in headers.AsEnumerable().ToList())
            {
                if (string.Equals(header.Key, XmlConstants.HttpRequestAccept, StringComparison.Ordinal) && ignoreAcceptHeader)
                {
                    continue;
                }

                requestMessage.SetHeader(header.Key, header.Value);
            }
        }
#endif

        /// <summary>
        /// Checks if the given type is DataServiceCollection&lt;&gt; type.
        /// </summary>
        /// <param name="t">Type to be checked.</param>
        /// <returns>true if the provided type is DataServiceCollection&lt;&gt; or false otherwise.</returns>
        internal static bool IsDataServiceCollectionType(Type t)
        {
            if (DataServiceCollectionAvailable)
            {
                return t == GetDataServiceCollectionOfTType();
            }

            return false;
        }

        /// <summary>
        /// Creates an instance of DataServiceCollection&lt;&gt; class using provided types.
        /// </summary>
        /// <param name="typeArguments">Types to be used for creating DataServiceCollection&lt;&gt; object.</param>
        /// <returns>
        /// Instance of DataServiceCollection&lt;&gt; class created using provided types or null if DataServiceCollection&lt;&gt;
        /// type is not avaiable.
        /// </returns>
        internal static Type GetDataServiceCollectionOfT(params Type[] typeArguments)
        {
            if (DataServiceCollectionAvailable)
            {
                Debug.Assert(
                    GetDataServiceCollectionOfTType() != null,
                    "DataServiceCollection is available so GetDataServiceCollectionOfTType() must not return null.");

                return GetDataServiceCollectionOfTType().MakeGenericType(typeArguments);
            }

            return null;
        }

        /// <summary>
        /// Returns the default value for the given type
        /// </summary>
        /// <param name="type">type to get the default value</param>
        /// <returns>returns the default value for <paramref name="type"/>.</returns>
        internal static object GetDefaultValue(Type type)
        {
            Debug.Assert(getDefaultValueMethodInfo != null, "WebUtil.getDefaultValueMethodInfo != null");
            return getDefaultValueMethodInfo.MakeGenericMethod(type).Invoke(null, null);
        }

        /// <summary>
        /// Returns the default value for the given type
        /// </summary>
        /// <typeparam name="T">type to get the default value</typeparam>
        /// <returns>returns the default value for <typeparamref name="T"/>.</returns>
        internal static T GetDefaultValue<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Dispose the message if it implements IDisposable.
        /// </summary>
        /// <param name="responseMessage">IODataResponseMessage to dispose.</param>
        internal static void DisposeMessage(IODataResponseMessage responseMessage)
        {
            IDisposable disposableMessage = responseMessage as IDisposable;
            if (disposableMessage != null)
            {
                disposableMessage.Dispose();
            }
        }

#if WINDOWS_PHONE
        /// <summary>
        /// Fires the WritingRequest event on the <paramref name="requestInfo"/> instance.
        /// </summary>
        /// <param name="requestHeaders">Request Headers</param>
        /// <param name="readableStream">A stream which can be read.</param>
        /// <param name="isBatchPart">Boolean flag indicating if this request is part of a batch request..</param>
        /// <param name="requestInfo">Request information to help fire events on the context.</param>
        /// <param name="requestMessage">The request message to apply the headers to.</param>
        /// <param name="ignoreAcceptHeader">If set to true the Accept header will be ignored.</param>
        /// <returns>A stream containing the content of the request/response.</returns>
        internal static Stream FireWritingRequest(HeaderCollection requestHeaders, Stream readableStream, bool isBatchPart, RequestInfo requestInfo, IODataRequestMessage requestMessage, bool ignoreAcceptHeader)
        {
            ReadingWritingHttpMessageEventArgs args = new ReadingWritingHttpMessageEventArgs(requestHeaders, readableStream, isBatchPart);
            var rewrittenArgs = requestInfo.FireWritingRequestEvent(args);
            WebUtil.ApplyHeadersToRequest(rewrittenArgs.HeaderCollection, requestMessage, true);
            return rewrittenArgs.Content;
        }
#endif

        /// <summary>
        /// Forces loading WindowsBase assembly. If WindowsBase assembly is not present JITter will throw an exception. 
        /// This method MUST NOT be inlined otherwise we won't be able to catch the exception by JITter in the caller.
        /// </summary>
        /// <returns>typeof(DataServiceCollection&lt;&gt;)</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type GetDataServiceCollectionOfTType()
        {
            return typeof(DataServiceCollection<>);
        }

        /// <summary>
        /// A workaround to a problem with FxCop which does not recognize the CheckArgumentNotNull method
        /// as the one which validates the argument is not null.
        /// </summary>
        /// <remarks>This has been suggested as a workaround in msdn forums by the VS team. Note that even though this is production code
        /// the attribute has no effect on anything else.</remarks>
        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}

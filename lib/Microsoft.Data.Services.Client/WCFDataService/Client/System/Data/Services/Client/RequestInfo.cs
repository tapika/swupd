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
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
#if !ASTORIA_LIGHT 
    using System.Net;
#else // Data.Services http stack
    using System.Data.Services.Http;
#endif
    using System.Reflection;
    using System.Xml.Linq;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;

    /// <summary>
    /// Class which wraps the dataservicecontext and exposes information required for 
    /// generating request to send to the server
    /// </summary>
    internal class RequestInfo
    {
        /// <summary>The type resolver for the current request.</summary>
        private readonly TypeResolver typeResolver;

        /// <summary>
        /// Creates a new instance of RequestInfo class which is used to build the request to send to the server
        /// </summary>
        /// <param name="context">wrapping context instance.</param>
        /// <param name="isContinuation">Whether this is a continuation request.</param>
        internal RequestInfo(DataServiceContext context, bool isContinuation) : this(context)
        {
            this.IsContinuation = isContinuation;
        }

        /// <summary>
        /// Creates a new instance of RequestInfo class which is used to build the request to send to the server
        /// </summary>
        /// <param name="context">wrapping context instance.</param>
        internal RequestInfo(DataServiceContext context)
        {
            Debug.Assert(context != null, "context != null");
            this.Context = context;
            this.WriteHelper = new ODataMessageWritingHelper(this);
            this.typeResolver = new TypeResolver(context.Model, context.ResolveTypeFromName, context.ResolveNameFromType, context.Format.ServiceModel);
        }

        #region Properties

        /// <summary>The writing helper to use.</summary>
        internal ODataMessageWritingHelper WriteHelper { get; private set; }

        /// <summary>context instance.</summary>
        internal DataServiceContext Context { get; private set; }

        /// <summary>
        /// Whether this is a continuation request.
        /// </summary>
        internal bool IsContinuation { get; private set; }

#pragma warning disable 0618
        /// <summary>Gets the URI used to indicate what type scheme is used by the service.</summary>
        internal Uri TypeScheme
        {
            get { return this.Context.TypeScheme; }
        }

        /// <summary>Override the namespace used for the data parts of the ATOM entries</summary>
        internal string DataNamespace
        {
            get { return this.Context.DataNamespace; }
        }
#pragma warning restore 0618

        /// <summary> Gets the configurations. </summary>
        internal DataServiceClientConfigurations Configurations
        {
            get { return this.Context.Configurations; }
        }

        /// <summary>Returns the instance of entity tracker class which tracks all the entities and links for the context.</summary>
        internal EntityTracker EntityTracker
        {
            get { return this.Context.EntityTracker; }
        }

        /// <summary>Indicates if there are subscribers for the WritingEntity event</summary>
        internal bool HasWritingEventHandlers
        {
            get { return this.Context.HasWritingEntityHandlers; }
        }

        /// <summary>Whether to ignore extra properties in the response payload.</summary>
        internal bool IgnoreResourceNotFoundException
        {
            get { return this.Context.IgnoreResourceNotFoundException; }
        }

        /// <summary>True if the context's ResolveName property has been set, otherwise false.</summary>
        internal bool HasResolveName
        {
            get { return this.Context.ResolveName != null; }
        }

        /// <summary>True if the context's ResolveName property can be determined to be a user-supplied value, instead of the one provided by codegen.</summary>
        internal bool IsUserSuppliedResolver
        {
            get
            {
                Debug.Assert(this.Context.ResolveName != null, "this.context.ResolveName != null.");
#if WINRT
    // Func<>.Method property does not exist on Win8 and there is no other way to access any MethodInfo that is behind the Func,
    // so we have no way to determine if the Func was supplied by the user or if it's the one provided by codegen.
    // In this case we will always assume it's the one provided by codegen, which means we'll try to resolve the name using the entity descriptor
    // first. This is likely to be correct in more cases than using the codegen resolver, so it is safer to make this assumption than the reverse.
                return false;
#else
                MethodInfo resolveNameMethodInfo = this.Context.ResolveName.Method;
                var codegenAttr = resolveNameMethodInfo.GetCustomAttributes(false).OfType<GeneratedCodeAttribute>().FirstOrDefault();
                return codegenAttr == null || codegenAttr.Tool != Util.CodeGeneratorToolName;
#endif
            }
        }

        /// <summary>Gets the BaseUriResolver</summary>
        internal UriResolver BaseUriResolver
        {
            get { return this.Context.BaseUriResolver; }
        }

        /// <summary>Gets the response preference for Add and Update operations.</summary>
        internal DataServiceResponsePreference AddAndUpdateResponsePreference
        {
            get { return this.Context.AddAndUpdateResponsePreference; }
        }

        /// <summary>The maximum protocol version the client should understand.</summary>
        internal Version MaxProtocolVersionAsVersion
        {
            get { return this.Context.MaxProtocolVersionAsVersion; }
        }
        
        /// <summary>
        /// Returns true if there are subscribers to SendingRequest event.
        /// </summary>
        internal bool HasSendingRequestEventHandlers
        {
            get { return this.Context.HasSendingRequestEventHandlers; }
        }

        /// <summary>
        /// Returns true if there are subscribers to SendingRequest event.
        /// </summary>
        internal bool HasSendingRequest2EventHandlers
        {
            get { return this.Context.HasSendingRequest2EventHandlers; }
        }

        /// <summary>
        /// True if the user could have modified a part of the request. This lets us turn off assertions that normally
        /// prevent us from making certain mistakes we don't mind the user intentionally ignoring.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Will be called in retail soon")]
        internal bool UserModifiedRequestInBuildingRequest
        {
            get { return this.Context.HasBuildingRequestEventHandlers; }
        }

#if WINDOWS_PHONE
        /// <summary>Indicates if there are subscribers for the WritingRequest event</summary>
        internal bool HasWritingRequestEventHandlers
        {
            get
            {
                return this.Context.HasWritingRequestEventHandlers;
            }
        }

        /// <summary>Indicates if there are subscribers for the ReadingResponse event</summary>
        internal bool HasReadingResponseEventHandlers
        {
            get
            {
                return this.Context.HasReadingResponseEventHandlers;
            }
        }
#endif

        /// <summary>
        /// Gets the authentication information used by each query created using the context object.
        /// </summary>
        internal System.Net.ICredentials Credentials
        {
            get { return this.Context.Credentials; }
        }

#if !ASTORIA_LIGHT && !PORTABLELIB
        /// <summary>
        /// Get the timeout span in seconds to use for the underlying HTTP request to the data service.
        /// </summary>
        internal int Timeout
        {
            get { return this.Context.Timeout; }
        }
#endif

        /// <summary>
        /// Whether to use post-tunneling for PUT/DELETE.
        /// </summary>
        internal bool UsePostTunneling
        {
            get { return this.Context.UsePostTunneling; }
        }

        /// <summary>
        /// Gets the client model.
        /// </summary>
        internal ClientEdmModel Model
        {
            get { return this.Context.Model; }
        }

        /// <summary>
        /// Gets the tracker for the user-specified format to use for requests.
        /// </summary>
        internal DataServiceClientFormat Format
        {
            get { return this.Context.Format; }
        }

        /// <summary>
        /// Gets the type resolver.
        /// </summary>
        internal TypeResolver TypeResolver
        {
            get { return this.typeResolver; }
        }

        /// <summary>
        /// Gets the URL conventions the user set on the context.
        /// </summary>
        internal DataServiceUrlConventions UrlConventions
        {
            get { return this.Context.UrlConventions; }
        }

        /// <summary>
        /// The HTTP stack to use in Silverlight.
        /// </summary>
        internal HttpStack HttpStack
        {
            get { return this.Context.HttpStack; }
        }

        /// <summary>
        /// Gets a System.Boolean value that controls whether default credentials are sent with requests.
        /// </summary>
        internal bool UseDefaultCredentials
        {
            get { return this.Context.UseDefaultCredentials; }
        }
        #endregion Properties

        #region Methods

#if !ASTORIA_LIGHT && !PORTABLELIB
        /// <summary>
        /// This method wraps the HttpWebRequest.GetSyncronousResponse method call. The reasons for doing this are to give us a place
        /// to invoke internal test hook callbacks that can validate the response headers, and also so that we can do
        /// debug validation to make sure that the headers have not changed since they were originally configured on the request.
        /// </summary>
        /// <param name="request">ODataRequestMessageWrapper instance</param>
        /// <param name="handleWebException">If set to true, this method will only re-throw the WebException that was caught if
        /// the response in the exception is null. If set to false, this method will always re-throw in case of a WebException.</param>
        /// <returns>
        /// Returns the HttpWebResponse from the wrapped GetSyncronousResponse method.
        /// </returns>
        internal IODataResponseMessage GetSyncronousResponse(ODataRequestMessageWrapper request, bool handleWebException)
        {
            return this.Context.GetSyncronousResponse(request, handleWebException);
        }
#endif

        /// <summary>
        /// This method wraps the HttpWebRequest.EndGetResponse method call. The reason for doing this is to give us a place
        /// to invoke internal test hook callbacks that can validate the response headers.
        /// </summary>
        /// <param name="request">HttpWebRequest instance</param>
        /// <param name="asyncResult">Async result obtained from previous call to BeginGetResponse.</param>
        /// <returns>Returns the HttpWebResponse from the wrapped EndGetResponse method.</returns>
        internal IODataResponseMessage EndGetResponse(ODataRequestMessageWrapper request, IAsyncResult asyncResult)
        {
            return this.Context.EndGetResponse(request, asyncResult);
        }

        /// <summary>
        /// Get the server type name - either from the entity descriptor or using the type resolver.
        /// </summary>
        /// <param name="descriptor">The entity descriptor.</param>
        /// <returns>The server type name for the entity.</returns>
        internal string GetServerTypeName(EntityDescriptor descriptor)
        {
            Debug.Assert(descriptor != null && descriptor.Entity != null, "Null descriptor or no entity in descriptor");
            string serverTypeName = null;

            if (this.HasResolveName)
            {
                Type entityType = descriptor.Entity.GetType();
                if (this.IsUserSuppliedResolver)
                {
                    // User-supplied resolver, must call first
                    serverTypeName = this.ResolveNameFromType(entityType) ?? descriptor.GetLatestServerTypeName();
                }
                else
                {
                    // V2+ codegen resolver, called last
                    serverTypeName = descriptor.GetLatestServerTypeName() ?? this.ResolveNameFromType(entityType);
                }
            }
            else
            {
                serverTypeName = descriptor.GetLatestServerTypeName();
            }

            return serverTypeName;
        }

        /// <summary>
        /// Get the server type name - either from the entity descriptor or using the type resolver.
        /// </summary>
        /// <param name="clientTypeAnnotation">Client type annotation.</param>
        /// <returns>The server type name for the entity.</returns>
        internal string GetServerTypeName(ClientTypeAnnotation clientTypeAnnotation)
        {
            Debug.Assert(!clientTypeAnnotation.IsEntityType, "This method must be only called for complex types");
            string serverTypeName = this.ResolveNameFromType(clientTypeAnnotation.ElementType);

            return serverTypeName;
        }

        /// <summary>
        /// Infers the server type name for the entity tracked in the given descriptor based on the server model.
        /// </summary>
        /// <param name="descriptor">The descriptor containing the entity to get the type name for.</param>
        /// <returns>The type name or null if it could not be inferred.</returns>
        internal string InferServerTypeNameFromServerModel(EntityDescriptor descriptor)
        {
            Debug.Assert(descriptor != null, "descriptor != null");
            Debug.Assert(descriptor.ServerTypeName == null, "Should not be called if the server type name is already known.");

            if (descriptor.EntitySetName != null)
            {
                string serverTypeName;
                if (this.TypeResolver.TryResolveEntitySetBaseTypeName(descriptor.EntitySetName, out serverTypeName))
                {
                    return serverTypeName;
                }
            }
            else if (descriptor.IsDeepInsert)
            {
                string parentServerTypeName = this.GetServerTypeName(descriptor.ParentForInsert);
                if (parentServerTypeName == null)
                {
                    parentServerTypeName = this.InferServerTypeNameFromServerModel(descriptor.ParentForInsert);
                }

                string serverTypeName;
                if (this.TypeResolver.TryResolveNavigationTargetTypeName(parentServerTypeName, descriptor.ParentPropertyForInsert, out serverTypeName))
                {
                    return serverTypeName;
                }
            }

            return null;
        }

#if WINDOWS_PHONE
        /// <summary>
        /// Fires the WritingRequest event
        /// </summary>
        /// <param name="args">Arguments for raising this event.</param>
        /// <returns>The re-written arguments which includes the request stream and the request headers.</returns>
        internal ReadingWritingHttpMessageEventArgs FireWritingRequestEvent(ReadingWritingHttpMessageEventArgs args)
        {
            return this.Context.FireWritingRequestEvent(args);
        }

        /// <summary>
        /// Fires the ReadingResponse event
        /// </summary>
        /// <param name="args">Arguments for raising this event.</param>
        /// <returns>The re-written arguments which includes the request stream and the request headers.</returns>
        internal ReadingWritingHttpMessageEventArgs FireReadingResponseEvent(ReadingWritingHttpMessageEventArgs args)
        {
            return this.Context.FireReadingResponseEvent(args);
        }
#endif

        /// <summary>
        /// Fires the WritingEvent event
        /// </summary>
        /// <param name="element">Entity being serialized</param>
        /// <param name="data">XML data of the ATOM entry</param>
        /// <param name="baseUri">The xml base of the feed or entry containing the current ATOM entry</param>
        internal void FireWritingEntityEvent(object element, XElement data, Uri baseUri)
        {
            this.Context.FireWritingAtomEntityEvent(element, data, baseUri);
        }

        /// <summary>
        /// The reverse of ResolveType, use for complex types and LINQ query expression building
        /// </summary>
        /// <param name="type">client type</param>
        /// <returns>type for the server</returns>
        internal string ResolveNameFromType(Type type)
        {
            return this.Context.ResolveNameFromType(type);
        }

        /// <summary>
        /// Returns the instance of ResponseInfo class, which provides all the information for response handling.
        /// </summary>
        /// <param name="mergeOption">merge option to use for handling the response conflicts.
        /// If this parameter is null the default MergeOption value from the context is used.</param>
        /// <returns>instance of response info class.</returns>
        internal ResponseInfo GetDeserializationInfo(MergeOption? mergeOption)
        {
            return new ResponseInfo(
                this,
                mergeOption.HasValue ? mergeOption.Value : this.Context.MergeOption);
        }

        /// <summary>
        /// Returns the instance of LoadPropertyResponseInfo class, which provides information for LoadProperty response handling.
        /// </summary>
        /// <param name="mergeOption">Merge option to use for conflict handling.</param>
        /// <param name="entityDescriptor">Entity whose property is being loaded.</param>
        /// <param name="property">Property which is being loaded.</param>
        /// <returns>Instance of the LoadPropertyResponseInfo class.</returns>
        internal ResponseInfo GetDeserializationInfoForLoadProperty(MergeOption? mergeOption, EntityDescriptor entityDescriptor, ClientPropertyAnnotation property)
        {
            return new LoadPropertyResponseInfo(
                this,
                mergeOption.HasValue ? mergeOption.Value : this.Context.MergeOption,
                entityDescriptor,
                property);
        }

        /// <summary>
        /// Validates that the response version can be accepted as a response for this request
        /// </summary>
        /// <param name="responseVersion">The version of the response (possibly null if none was specified)</param>
        /// <returns>Exception if the version can't be accepted, otherwise null.</returns>
        internal InvalidOperationException ValidateResponseVersion(Version responseVersion)
        {
            if (responseVersion != null && responseVersion > this.Context.MaxProtocolVersionAsVersion)
            {
                string message = Strings.Context_ResponseVersionIsBiggerThanProtocolVersion(
                    responseVersion.ToString(),
                    this.Context.MaxProtocolVersion.ToString());
                return Error.InvalidOperation(message);
            }

            return null;
        }

        /// <summary>
        /// Fires the SendingRequest event.
        /// </summary>
        /// <param name="eventArgs">SendingRequestEventArgs instance containing all information about the request.</param>
        internal void FireSendingRequest(SendingRequestEventArgs eventArgs)
        {
#if DEBUG
            Version requestVersion = this.GetRequestVersion(eventArgs.RequestHeaders);
            Debug.Assert(this.UserModifiedRequestInBuildingRequest || requestVersion == null || requestVersion <= this.MaxProtocolVersionAsVersion, "requestVersion must not be greater than the maxProtocolVersion");
            Debug.Assert(this.UserModifiedRequestInBuildingRequest || eventArgs.RequestHeaders[XmlConstants.HttpMaxDataServiceVersion] == this.MaxProtocolVersionAsVersion.ToString() + Util.VersionSuffix, "requestMDSV must be set to the maxProtocolVersion");
#endif
            this.Context.FireSendingRequest(eventArgs);
        }

        /// <summary>
        /// Fires the SendingRequest2 event.
        /// </summary>
        /// <param name="eventArgs">SendingRequest2EventArgs instance containing all information about the request.</param>
        internal void FireSendingRequest2(SendingRequest2EventArgs eventArgs)
        {
            this.Context.FireSendingRequest2(eventArgs);
        }

        /// <summary>
        /// Returns an instance of the IODataRequestMessage 
        /// </summary>
        /// <param name="requestMessageArgs">Arguments for creating the request message.</param>
        /// <returns>an instance of the IODataRequestMessage </returns>
        internal DataServiceClientRequestMessage CreateRequestMessage(BuildingRequestEventArgs requestMessageArgs)
        {
            var headersDictionary = requestMessageArgs.HeaderCollection.UnderlyingDictionary;

            // We are implementing the PostTunneling logic here. The reason for doing this is
            // 1> In this public class, the Method property returns the actual method (PUT, MERGE, DELETE),
            //    and not the verb that goes in the wire. So this class needs to know about
            //    actual verb since it will be using this verb to send over http.     
            if (this.UsePostTunneling)
            {
                bool setXHttpMethodHeader = false;
                if (string.CompareOrdinal(XmlConstants.HttpMethodGet, requestMessageArgs.Method) != 0 &&
                    string.CompareOrdinal(XmlConstants.HttpMethodPost, requestMessageArgs.Method) != 0)
                {
                    setXHttpMethodHeader = true;
                }

                // Setting the actual method in the header
                if (setXHttpMethodHeader)
                {
                    headersDictionary[XmlConstants.HttpXMethod] = requestMessageArgs.Method;
                }

                // Set the Content-length of a Delete to 0. Important for post tunneling when its a post, content-length must be zero in this case.
                if (string.CompareOrdinal(XmlConstants.HttpMethodDelete, requestMessageArgs.Method) == 0)
                {
                    headersDictionary[XmlConstants.HttpContentLength] = "0";

#if DEBUG
                    if (!this.UserModifiedRequestInBuildingRequest)
                    {
                        Debug.Assert(!requestMessageArgs.HeaderCollection.HasHeader(XmlConstants.HttpContentType), "Content-Type header must not be set for DELETE requests");
                    }
#endif
                }
            }

#if !ASTORIA_LIGHT
            var clientRequestMessageArgs = new DataServiceClientRequestMessageArgs(requestMessageArgs.Method, requestMessageArgs.RequestUri, this.UseDefaultCredentials, this.UsePostTunneling, headersDictionary);
#else
            var clientRequestMessageArgs = new DataServiceClientRequestMessageArgs(requestMessageArgs.Method, requestMessageArgs.RequestUri, this.UsePostTunneling, this.UseDefaultCredentials, headersDictionary, requestMessageArgs.ClientHttpStack);
#endif

            DataServiceClientRequestMessage clientRequestMessage;
            if (this.Configurations.RequestPipeline.OnMessageCreating != null)
            {
                clientRequestMessage = this.Configurations.RequestPipeline.OnMessageCreating(clientRequestMessageArgs);
                if (clientRequestMessage == null)
                {
                    throw Error.InvalidOperation(Strings.Context_OnMessageCreatingReturningNull);
                }
            }
            else
            {
                clientRequestMessage = new HttpWebRequestMessage(clientRequestMessageArgs, this);
            }

            return clientRequestMessage;
        }

        /// <summary>
        /// Asks the context to Fire the BuildingRequest event and get RequestMessageArgs.
        /// </summary>
        /// <param name="method">Http method for the request.</param>
        /// <param name="requestUri">Base Uri for the request.</param>
        /// <param name="headers">Request headers.</param>
        /// <param name="httpStack">HttpStack to use.</param>
        /// <param name="descriptor">Descriptor for the request, if there is one.</param>
        /// <returns>A new RequestMessageArgs object for building the request message.</returns>
        internal BuildingRequestEventArgs CreateRequestArgsAndFireBuildingRequest(string method, Uri requestUri, HeaderCollection headers, HttpStack httpStack, Descriptor descriptor)
        {
            return this.Context.CreateRequestArgsAndFireBuildingRequest(method, requestUri, headers, httpStack, descriptor);
        }

#if DEBUG
        /// <summary>
        /// Return the request DSV header value for this request.
        /// </summary>
        /// <returns>The request DSV header value for this request as Version instance.</returns>
#if !ASTORIA_LIGHT
        private Version GetRequestVersion(WebHeaderCollection headers)
#else
        private Version GetRequestVersion(Net.WebHeaderCollection headers)
#endif
        {
            string requestDSVHeaderValue = headers[XmlConstants.HttpDataServiceVersion];
            if (!String.IsNullOrEmpty(requestDSVHeaderValue))
            {
                Debug.Assert(requestDSVHeaderValue.Contains(";"), "Unexpected request DSV header value");
                return Version.Parse(requestDSVHeaderValue.Substring(0, requestDSVHeaderValue.IndexOf(';')));
            }

            return null;
        }
#endif

        #endregion Methods
    }
}

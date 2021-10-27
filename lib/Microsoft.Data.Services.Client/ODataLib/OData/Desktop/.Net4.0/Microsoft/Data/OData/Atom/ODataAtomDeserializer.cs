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

namespace Microsoft.Data.OData.Atom
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    #endregion Namespaces

    /// <summary>
    /// Base class for all OData ATOM deserializers.
    /// </summary>
    internal abstract class ODataAtomDeserializer : ODataDeserializer
    {
        /// <summary>The ATOM input context to use for reading.</summary>
        private readonly ODataAtomInputContext atomInputContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomInputContext">The ATOM input context to read from.</param>
        protected ODataAtomDeserializer(ODataAtomInputContext atomInputContext)
            : base(atomInputContext)
        {
            Debug.Assert(atomInputContext != null, "atomInputContext != null");

            this.atomInputContext = atomInputContext;
        }

        /// <summary>
        /// The XML reader to read the input from.
        /// </summary>
        internal BufferingXmlReader XmlReader
        {
            get 
            {
                DebugUtils.CheckNoExternalCallers();
                return this.atomInputContext.XmlReader;
            }
        }

        /// <summary>
        /// The ATOM input context to use for reading.
        /// </summary>
        protected ODataAtomInputContext AtomInputContext
        {
            get { return this.atomInputContext; }
        }

        /// <summary>
        /// Reads the start of the payload. Wraps the call to XmlReaderExtensions.ReadPayloadStart().
        /// </summary>
        internal void ReadPayloadStart()
        {
            DebugUtils.CheckNoExternalCallers();
            this.XmlReader.AssertNotBuffering();

            // TODO: When we implement the XmlReader extensibility this code will have to change since we should not
            // read over top-level nodes before and after the top-level element.
            this.XmlReader.ReadPayloadStart();

            this.XmlReader.AssertNotBuffering();
        }

        /// <summary>
        /// Reads till the end of the payload. Wraps the call to XmlReaderExtensions.ReadPayloadEnd().
        /// </summary>
        internal void ReadPayloadEnd()
        {
            DebugUtils.CheckNoExternalCallers();
            this.XmlReader.AssertNotBuffering();

            // TODO: When we implement the XmlReader extensibility this code will have to change since we should not
            // read over top-level nodes before and after the top-level element.
            this.XmlReader.ReadPayloadEnd();

            this.XmlReader.AssertNotBuffering();
        }

        /// <summary>
        /// Given a URI from the payload, this method will try to make it absolute, or fail otherwise.
        /// </summary>
        /// <param name="uriFromPayload">The URI string from the payload to process.</param>
        /// <param name="xmlBaseUri">The (optional) Xml base URI as specified in the payload.</param>
        /// <returns>An absolute URI to report.</returns>
        internal Uri ProcessUriFromPayload(string uriFromPayload, Uri xmlBaseUri)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(uriFromPayload != null, "uriFromPayload != null");
            return this.ProcessUriFromPayload(uriFromPayload, xmlBaseUri, /*makeAbsolute*/ true);
        }

        /// <summary>
        /// Given a string representation of a URI from the payload, this method will return an absolute or relative URI.
        /// </summary>
        /// <param name="uriFromPayload">The URI string from the payload to process.</param>
        /// <param name="xmlBaseUri">The (optional) Xml base URI as specified in the payload.</param>
        /// <param name="makeAbsolute">If true, then this method will try to make the URI absolute, or fail otherwise.</param>
        /// <returns>An absolute or relative URI to report based on the value of the <paramref name="makeAbsolute"/> parameter.</returns>
        internal Uri ProcessUriFromPayload(string uriFromPayload, Uri xmlBaseUri, bool makeAbsolute)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(uriFromPayload != null, "uriFromPayload != null");

            // Figure out what base URI to use (if any)
            Uri baseUri = xmlBaseUri;
            if (baseUri != null)
            {
                Debug.Assert(baseUri.IsAbsoluteUri, "Base URI from the Xml reader should always be absolute.");
            }
            else
            {
                baseUri = this.MessageReaderSettings.BaseUri;
                if (baseUri != null)
                {
                    Debug.Assert(this.MessageReaderSettings.BaseUri.IsAbsoluteUri, "The BaseUri on settings should have been verified to be absolute by now.");
                }
            }

            Uri uri = new Uri(uriFromPayload, UriKind.RelativeOrAbsolute);

            // Try to resolve the URI using a custom URL resolver first.
            Uri resolvedUri = this.AtomInputContext.ResolveUri(baseUri, uri);
            if (resolvedUri != null)
            {
                return resolvedUri;
            }

            if (!uri.IsAbsoluteUri && makeAbsolute)
            {
                // Try to apply the base Uri if it's available
                if (baseUri != null)
                {
                    uri = UriUtils.UriToAbsoluteUri(baseUri, uri);
                }
                else
                {
                    // Otherwise fail
                    throw new ODataException(Strings.ODataAtomDeserializer_RelativeUriUsedWithoutBaseUriSpecified(uriFromPayload));
                }
            }

            Debug.Assert(uri.IsAbsoluteUri || !makeAbsolute, "If makeAbsolute was true, by now we should have absolute URI.");
            return uri;
        }

        /// <summary>
        /// Asserts that the XML reader is positioned on one of the specified node types.
        /// </summary>
        /// <param name="allowedNodeTypes">The node types which should appear at this point.</param>
        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Needs access to this in Debug only.")]
        internal void AssertXmlCondition(params XmlNodeType[] allowedNodeTypes)
        {
            DebugUtils.CheckNoExternalCallers();

#if DEBUG
            this.AssertXmlCondition(false, allowedNodeTypes);
#endif
        }

        /// <summary>
        /// Asserts that the XML reader is positioned on one of the specified node types.
        /// </summary>
        /// <param name="allowEmptyElement">True if an empty element node should be added to the list.</param>
        /// <param name="allowedNodeTypes">The node types which should appear at this point.</param>
        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Needs access to this in Debug only.")]
        internal void AssertXmlCondition(bool allowEmptyElement, params XmlNodeType[] allowedNodeTypes)
        {
            DebugUtils.CheckNoExternalCallers();

#if DEBUG
            if (allowEmptyElement && this.XmlReader.NodeType == XmlNodeType.Element && this.XmlReader.IsEmptyElement)
            {
                return;
            }

            if (allowedNodeTypes.Contains(this.XmlReader.NodeType))
            {
                return;
            }

            string message = string.Format(
                CultureInfo.InvariantCulture,
                "XML condition failed: the XmlReader is on node {0} (LocalName: {1}, Namespace: {2}, Value: {3}) but it was expected be on {4}.",
                this.XmlReader.NodeType == XmlNodeType.Element ? "Element (IsEmptyElement: " + this.XmlReader.IsEmptyElement.ToString() + ")" : this.XmlReader.NodeType.ToString(),
                this.XmlReader.LocalName,
                this.XmlReader.NamespaceURI,
                this.XmlReader.Value,
                (allowEmptyElement ? "Element (IsEmptyElement: true), " : "") + string.Join(",", allowedNodeTypes.Select(n => n.ToString()).ToArray()));
            Debug.Assert(false, message);
#endif
        }
    }
}

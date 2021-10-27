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

#if !INTERNAL_DROP || ODATALIB

namespace Microsoft.Data.OData
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Xml;
    using Microsoft.Data.OData.Atom;
    #endregion Namespaces

    /// <summary>
    /// Utility methods serializing the xml error payload
    /// </summary>
    internal static class ErrorUtils
    {
        /// <summary>Default language for error messages if not specified.</summary>
        /// <remarks>
        /// This constant is included here since this file is compiled into WCF DS Server as well
        /// so we can't compile in the ODataConstants.
        /// </remarks>
        internal const string ODataErrorMessageDefaultLanguage = "en-US";

        /// <summary>
        /// Extracts error details from an <see cref="ODataError"/>.
        /// </summary>
        /// <param name="error">The ODataError instance to extract the error details from.</param>
        /// <param name="code">A data service-defined string which serves as a substatus to the HTTP response code.</param>
        /// <param name="message">A human readable message describing the error.</param>
        /// <param name="messageLanguage">The language identifier representing the language the error message is in.</param>
        internal static void GetErrorDetails(ODataError error, out string code, out string message, out string messageLanguage)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(error != null, "error != null");

            code = error.ErrorCode ?? string.Empty;
            message = error.Message ?? string.Empty;
            messageLanguage = error.MessageLanguage ?? ErrorUtils.ODataErrorMessageDefaultLanguage;
        }
        
        /// <summary>
        /// Write an error message.
        /// </summary>
        /// <param name="writer">The Xml writer to write to.</param>
        /// <param name="error">The error instance to write.</param>
        /// <param name="includeDebugInformation">A flag indicating whether error details should be written (in debug mode only) or not.</param>
        /// <param name="maxInnerErrorDepth">The maximumum number of nested inner errors to allow.</param>
        internal static void WriteXmlError(XmlWriter writer, ODataError error, bool includeDebugInformation, int maxInnerErrorDepth)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(writer != null, "writer != null");
            Debug.Assert(error != null, "error != null");

            string code, message, messageLanguage;
            ErrorUtils.GetErrorDetails(error, out code, out message, out messageLanguage);

            ODataInnerError innerError = includeDebugInformation ? error.InnerError : null;
            WriteXmlError(writer, code, message, messageLanguage, innerError, maxInnerErrorDepth);
        }

        /// <summary>
        /// Write an error message.
        /// </summary>
        /// <param name="writer">The Xml writer to write to.</param>
        /// <param name="code">The code of the error.</param>
        /// <param name="message">The message of the error.</param>
        /// <param name="messageLanguage">The language of the message.</param>
        /// <param name="innerError">Inner error details that will be included in debug mode (if present).</param>
        /// <param name="maxInnerErrorDepth">The maximumum number of nested inner errors to allow.</param>
        private static void WriteXmlError(XmlWriter writer, string code, string message, string messageLanguage, ODataInnerError innerError, int maxInnerErrorDepth)
        {
            Debug.Assert(writer != null, "writer != null");
            Debug.Assert(code != null, "code != null");
            Debug.Assert(message != null, "message != null");
            Debug.Assert(messageLanguage != null, "messageLanguage != null");

            // <m:error>
            writer.WriteStartElement(AtomConstants.ODataMetadataNamespacePrefix, AtomConstants.ODataErrorElementName, AtomConstants.ODataMetadataNamespace);

            // <m:code>code</m:code>
            writer.WriteElementString(AtomConstants.ODataMetadataNamespacePrefix, AtomConstants.ODataErrorCodeElementName, AtomConstants.ODataMetadataNamespace, code);

            // <m:message>
            writer.WriteStartElement(AtomConstants.ODataMetadataNamespacePrefix, AtomConstants.ODataErrorMessageElementName, AtomConstants.ODataMetadataNamespace);

            // xml:lang="..."
            writer.WriteAttributeString(AtomConstants.XmlNamespacePrefix, AtomConstants.XmlLangAttributeName, AtomConstants.XmlNamespace, messageLanguage);

            writer.WriteString(message);

            // </m:message>
            writer.WriteEndElement();

            if (innerError != null)
            {
                WriteXmlInnerError(writer, innerError, AtomConstants.ODataInnerErrorElementName, /* recursionDepth */ 0, maxInnerErrorDepth);
            }

            // </m:error>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the inner exception information in debug mode.
        /// </summary>
        /// <param name="writer">The Xml writer to write to.</param>
        /// <param name="innerError">The inner error to write.</param>
        /// <param name="innerErrorElementName">The local name of the element representing the inner error.</param>
        /// <param name="recursionDepth">The number of times this method has been called recursively.</param>
        /// <param name="maxInnerErrorDepth">The maximumum number of nested inner errors to allow.</param>
        private static void WriteXmlInnerError(XmlWriter writer, ODataInnerError innerError, string innerErrorElementName, int recursionDepth, int maxInnerErrorDepth)
        {
            Debug.Assert(writer != null, "writer != null");

            recursionDepth++;
            if (recursionDepth > maxInnerErrorDepth)
            {
#if ODATALIB
                throw new ODataException(Strings.ValidationUtils_RecursionDepthLimitReached(maxInnerErrorDepth));
#else
                throw new ODataException(System.Data.Services.Strings.BadRequest_DeepRecursion(maxInnerErrorDepth));
#endif
            }

            // <m:innererror> or <m:internalexception>
            writer.WriteStartElement(AtomConstants.ODataMetadataNamespacePrefix, innerErrorElementName, AtomConstants.ODataMetadataNamespace);

            //// NOTE: we add empty elements if no information is provided for the message, error type and stack trace
            ////       to stay compatible with Astoria.

            // <m:message>...</m:message>
            string errorMessage = innerError.Message ?? String.Empty;
            writer.WriteStartElement(AtomConstants.ODataInnerErrorMessageElementName, AtomConstants.ODataMetadataNamespace);
            writer.WriteString(errorMessage);
            writer.WriteEndElement();

            // <m:type>...</m:type>
            string errorType = innerError.TypeName ?? string.Empty;
            writer.WriteStartElement(AtomConstants.ODataInnerErrorTypeElementName, AtomConstants.ODataMetadataNamespace);
            writer.WriteString(errorType);
            writer.WriteEndElement();

            // <m:stacktrace>...</m:stacktrace>
            string errorStackTrace = innerError.StackTrace ?? String.Empty;
            writer.WriteStartElement(AtomConstants.ODataInnerErrorStackTraceElementName, AtomConstants.ODataMetadataNamespace);
            writer.WriteString(errorStackTrace);
            writer.WriteEndElement();

            if (innerError.InnerError != null)
            {
                WriteXmlInnerError(writer, innerError.InnerError, AtomConstants.ODataInnerErrorInnerErrorElementName, recursionDepth, maxInnerErrorDepth);
            }

            // </m:innererror> or </m:internalexception>
            writer.WriteEndElement();
        }
    }
}

#endif

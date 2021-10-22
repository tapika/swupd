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
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData.Metadata;
    using ODataErrorStrings = Microsoft.Data.OData.Strings;
    #endregion Namespaces

    /// <summary>
    /// Helper methods used by the OData reader for the ATOM format.
    /// </summary>
    internal static class ODataAtomReaderUtils
    {
        /// <summary>
        /// Creates an Xml reader over the specified stream with the provided settings.
        /// </summary>
        /// <param name="stream">The stream to create the XmlReader over.</param>
        /// <param name="encoding">The encoding to use to read the input.</param>
        /// <param name="messageReaderSettings">The OData message reader settings used to control the settings of the Xml reader.</param>
        /// <returns>An <see cref="XmlReader"/> instance configured with the provided settings.</returns>
        internal static XmlReader CreateXmlReader(Stream stream, Encoding encoding, ODataMessageReaderSettings messageReaderSettings)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(stream != null, "stream != null");
            Debug.Assert(messageReaderSettings != null, "messageReaderSettings != null");

            XmlReaderSettings xmlReaderSettings = CreateXmlReaderSettings(messageReaderSettings);

            if (encoding != null)
            {
                // Use the encoding from the content type if specified.
                // NOTE: The XmlReader will scan ahead and determine the encoding from the Xml declaration
                //       and or the payload. Only if no encoding is specified in the Xml declaration and 
                //       the Xml reader cannot figure out the encoding from the payload, can it happen
                //       that we need to specify the encoding explicitly (and that wrapping the stream with
                //       a stream reader makes a difference in the first place).
                return XmlReader.Create(new StreamReader(stream, encoding), xmlReaderSettings);
            }

            return XmlReader.Create(stream, xmlReaderSettings);
        }

        /// <summary>
        /// Parses the value of the m:null attribute and returns a boolean.
        /// </summary>
        /// <param name="attributeValue">The string value of the m:null attribute.</param>
        /// <returns>true if the value denotes that the element should be null; false otherwise.</returns>
        internal static bool ReadMetadataNullAttributeValue(string attributeValue)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(attributeValue != null, "attributeValue != null");

            return XmlConvert.ToBoolean(attributeValue);
        }

        /// <summary>
        /// Creates a new XmlReaderSettings instance using the encoding.
        /// </summary>
        /// <param name="messageReaderSettings">Configuration settings of the OData reader.</param>
        /// <returns>The Xml reader settings to use for this reader.</returns>
        private static XmlReaderSettings CreateXmlReaderSettings(ODataMessageReaderSettings messageReaderSettings)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CheckCharacters = messageReaderSettings.CheckCharacters;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.CloseInput = true;

            // We do not allow DTDs - this is the default
#if ORCAS
            settings.ProhibitDtd = true;
#else
            settings.DtdProcessing = DtdProcessing.Prohibit;
#endif

            return settings;
        }
    }
}

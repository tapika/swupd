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
    using System.Text;
    using System.Xml;
    #endregion Namespaces

    /// <summary>
    /// Extension methods for the XML reader.
    /// </summary>
    internal static class XmlReaderExtensions
    {
        /// <summary>
        /// Asserts that the reader is not buffer.
        /// </summary>
        /// <param name="bufferedXmlReader">The <see cref="BufferingXmlReader"/> to read from.</param>
        [Conditional("DEBUG")]
        internal static void AssertNotBuffering(this BufferingXmlReader bufferedXmlReader)
        {
            DebugUtils.CheckNoExternalCallers();

#if DEBUG
            Debug.Assert(!bufferedXmlReader.IsBuffering, "!bufferedXmlReader.IsBuffering");
#endif
        }

        /// <summary>
        /// Asserts that the reader is buffer.
        /// </summary>
        /// <param name="bufferedXmlReader">The <see cref="BufferingXmlReader"/> to read from.</param>
        [Conditional("DEBUG")]
        internal static void AssertBuffering(this BufferingXmlReader bufferedXmlReader)
        {
            DebugUtils.CheckNoExternalCallers();

#if DEBUG
            Debug.Assert(bufferedXmlReader.IsBuffering, "bufferedXmlReader.IsBuffering");
#endif
        }

        /// <summary>
        /// Reads the value of the element as a string.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The string value of the element.</returns>
        /// <remarks>
        /// Pre-Condition:   XmlNodeType.Element   - the element to read the value for.
        ///                  XmlNodeType.Attribute - an attribute on the element to read the value for.
        /// Post-Condition:  Any                   - the node after the element.
        ///                  
        /// This method is similar to ReadElementContentAsString with one difference:
        /// - It ignores Whitespace nodes - this is needed for compatiblity, WCF DS ignores insignificant whitespaces
        ///     it does that by setting the IgnoreWhitespace option on reader settings, ODataLib can't do that
        ///     cause it doesn't always control the creation of the XmlReader, so it has to explicitely ignore
        ///     insignificant whitespaces.
        /// </remarks>
        internal static string ReadElementValue(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();

            string result = reader.ReadElementContentValue();
            reader.Read();
            return result;
        }

        /// <summary>
        /// Reads the value of the first text, CDATA or significant whitespace node.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The value of the first text, CDATA or significant whitespace node.</returns>
        /// <remarks>
        /// Pre-Condition:   XmlNodeType.Element   - the element to read the value for.
        /// Post-Condition:  Any                   - the node after the element.
        /// </remarks>
        internal static string ReadFirstTextNodeValue(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element,
                "Pre-Condition: XML reader must be on an Element node.");

            reader.MoveToElement();

            string result = null;
            if (!reader.IsEmptyElement)
            {
                bool endElementFound = false;
                while (!endElementFound && reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.EndElement:
                            endElementFound = true;
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                            if (result == null)
                            {
                                result = reader.Value;
                            }

                            break;

                        // Ignore elements and all their content
                        case XmlNodeType.Element:
                            reader.SkipElementContent();
                            break;
                        
                        // Ignore anything else (everything else is a single node, so calling Read in the while loop will skip it)
                        default:
                            break;
                    }
                }
            }

            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement,
                "Post-Condition: XML reader must be on Element or EndElement node.");

            // Move the reader to the node after the end tag
            reader.Read();
            return result ?? string.Empty;
        }

        /// <summary>
        /// Reads the value of the element as a string.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The string value of the element.</returns>
        /// <remarks>
        /// Pre-Condition:   XmlNodeType.Element   - the element to read the value for.
        ///                  XmlNodeType.Attribute - an attribute on the element to read the value for.
        /// Post-Condition:  XmlNodeType.Element    - the element was empty.
        ///                  XmlNodeType.EndElement - the element had some value.
        ///                  
        /// This method is similar to ReadElementContentAsString with two differences:
        /// - It ignores Whitespace nodes - this is needed for compatiblity, WCF DS ignores insignificant whitespaces
        ///     it does that by setting the IgnoreWhitespace option on reader settings, ODataLib can't do that
        ///     cause it doesn't always control the creation of the XmlReader, so it has to explicitely ignore
        ///     insignificant whitespaces.
        /// - It leaves the reader positioned on the EndElement node (or the start node if it was empty).
        /// </remarks>
        internal static string ReadElementContentValue(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Attribute,
                "Pre-Condition: XML reader must be on Element or Attribute node.");

            reader.MoveToElement();

            string result = null;
            if (reader.IsEmptyElement)
            {
                result = string.Empty;
            }
            else
            {
                StringBuilder builder = null;
                bool endElementFound = false;
                while (!endElementFound && reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.EndElement:
                            endElementFound = true;
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                            if (result == null)
                            {
                                result = reader.Value;
                            }
                            else if (builder == null)
                            {
                                builder = new StringBuilder();
                                builder.Append(result);
                                builder.Append(reader.Value);
                            }
                            else
                            {
                                builder.Append(reader.Value);
                            }

                            break;

                        // Ignore comments, whitespaces and processing instructions.
                        case XmlNodeType.Comment:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Whitespace:
                            break;

                        default:
                            throw new ODataException(Strings.XmlReaderExtension_InvalidNodeInStringValue(reader.NodeType));
                    }
                }

                if (builder != null)
                {
                    result = builder.ToString();
                }
                else if (result == null)
                {
                    result = string.Empty;
                }
            }

            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement,
                "Post-Condition: XML reader must be on Element or EndElement node.");
            Debug.Assert(result != null, "The method should never return null since it doesn't handle null values.");
            return result;
        }

        /// <summary>
        /// Reads from the XML reader skipping insignificant nodes.
        /// </summary>
        /// <param name="reader">The XML reader to read from.</param>
        /// <remarks>Do not use MoveToContent since for backward compatibility reasons we skip over nodes reported as Text which have
        /// whitespace only content (even though the XmlReader should report those as Whitespace).</remarks>
        internal static void SkipInsignificantNodes(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");

            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        return;
                    case XmlNodeType.Comment:
                    case XmlNodeType.None:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.Whitespace:
                        break;
                    case XmlNodeType.Text:
                        if (IsNullOrWhitespace(reader.Value))
                        {
                            break;
                        }

                        return;

                    default:
                        return;
                }
            }
            while (reader.Read());
        }

        /// <summary>
        /// Skips the content of the element and leaves the reader on the end element (or empty start element)
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <remarks>
        /// Pre-Condition:   XmlNodeType.Element - the element to read
        ///                  XmlNodeType.Attribute - attribute on the element to read
        /// Post-Condition:  XmlNodeType.Element - if the element was empty element with no content.
        ///                  XmlNodeType.EndElement - if the element was element with empty content.
        /// </remarks>
        internal static void SkipElementContent(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Attribute,
                "Pre-Condition: XML reader must be on Element or Attribute node.");

            reader.MoveToElement();
            if (!reader.IsEmptyElement)
            {
                // Move to the first child.
                reader.Read();

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    // Skip all children until we find the end element.
                    reader.Skip();
                }
            }
        }

        /// <summary>
        /// Reads from the input until the first element is found.
        /// </summary>
        /// <param name="reader">The XML reader to read from.</param>
        /// <remarks>
        /// Pre-Condition:  XmlNodeType.None    - the reader hasn't been used yet.
        /// Post-Condition: XmlNodeType.Element - the reader is positioned on the root/first element.
        /// Note that the method will fail if the top-level contains any significant node other than the root element
        /// or if no root element is found.
        /// </remarks>
        internal static void ReadPayloadStart(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(reader.NodeType == XmlNodeType.None, "Pre-Condition: XML reader must not have been used yet.");

            reader.SkipInsignificantNodes();
            if (reader.NodeType != XmlNodeType.Element)
            {
                throw new ODataException(Strings.XmlReaderExtension_InvalidRootNode(reader.NodeType));
            }

            Debug.Assert(reader.NodeType == XmlNodeType.Element, "Post-Condition: XML reader must be on Element node.");
        }

        /// <summary>
        /// Reads till the end of the input payload.
        /// </summary>
        /// <param name="reader">The XML reader to read from.</param>
        /// <remarks>
        /// Pre-Condition:  any               - the reader will verify that only insignificant node is present.
        /// Post-Condition: XmlNodeType.None  - the reader is at the end of the input.
        /// </remarks>
        internal static void ReadPayloadEnd(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");

            reader.SkipInsignificantNodes();
            if (reader.NodeType != XmlNodeType.None && !reader.EOF)
            {
                throw new ODataException(Strings.XmlReaderExtension_InvalidRootNode(reader.NodeType));
            }

            Debug.Assert(
                reader.NodeType == XmlNodeType.None && reader.EOF,
                "Post-Condition: XML reader must be positioned at the end of the input.");
        }

        /// <summary>
        /// Determines if the current node's namespace equals to the specified <paramref name="namespaceUri"/>
        /// </summary>
        /// <param name="reader">The XML reader to get the current node from.</param>
        /// <param name="namespaceUri">The namespace URI to compare, this must be a string already atomized in the <paramref name="reader"/> name table.</param>
        /// <returns>true if the current node is in the specified namespace; false otherwise.</returns>
        internal static bool NamespaceEquals(this XmlReader reader, string namespaceUri)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Attribute,
                "The namespace of the node should only be tested on Element or Attribute nodes.");
            Debug.Assert(object.ReferenceEquals(reader.NameTable.Get(namespaceUri), namespaceUri), "The namespaceUri was not atomized on this reader.");

            return object.ReferenceEquals(reader.NamespaceURI, namespaceUri);
        }

        /// <summary>
        /// Determines if the current node's local name equals to the specified <paramref name="localName"/>
        /// </summary>
        /// <param name="reader">The XML reader to get the current node from.</param>
        /// <param name="localName">The local name to compare, this must be a string already atomized in the <paramref name="reader"/> name table.</param>
        /// <returns>true if the current node has the specified local name; false otherwise.</returns>
        internal static bool LocalNameEquals(this XmlReader reader, string localName)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Attribute,
                "The namespace of the node should only be tested on Element or Attribute nodes.");
            Debug.Assert(object.ReferenceEquals(reader.NameTable.Get(localName), localName), "The localName was not atomized on this reader.");

            return object.ReferenceEquals(reader.LocalName, localName);
        }

        /// <summary>
        /// Tries to read the current element as an empty element (no or empty content).
        /// </summary>
        /// <param name="reader">The XML reader to read from.</param>
        /// <returns>true if the reader was on an empty element; false otherwise.</returns>
        /// <remarks>
        /// Pre-Condition:   XmlNodeType.Element - the element to read
        ///                  XmlNodeType.Attribute - attribute on the element to read
        /// Post-Condition:  XmlNodeType.Element - if the element was empty element with no content.
        ///                  XmlNodeType.EndElement - if the element was element with empty content.
        ///                  any other - the first child node of the element, in this case the method returns false.
        /// </remarks>
        internal static bool TryReadEmptyElement(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Attribute,
                "Pre-Condition: XML reader must be on Element or Attribute node.");

            reader.MoveToElement();
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

        /// <summary>
        /// Reads to the next element encountered in an Xml payload.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> to read from.</param>
        /// <returns>true if the method reached the next element; otherwise false.</returns>
        internal static bool TryReadToNextElement(this XmlReader reader)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the specifies string is null or blank.
        /// </summary>
        /// <param name="text">Text to check.</param>
        /// <returns>true if text is null, empty, or all whitespace characters.</returns>
        private static bool IsNullOrWhitespace(string text)
        {
#if WINDOWS_PHONE || ORCAS || PORTABLELIB
            if (text == null)
            {
                return true;
            }
            else
            {
                foreach (char c in text)
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        return false;
                    }
                }

                return true;
            }
#else
            return string.IsNullOrWhiteSpace(text);
#endif
        }
    }
}

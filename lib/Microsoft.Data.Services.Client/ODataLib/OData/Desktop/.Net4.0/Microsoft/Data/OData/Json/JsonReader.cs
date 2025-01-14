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

#if SPATIAL
namespace Microsoft.Data.Spatial
#else
namespace Microsoft.Data.OData.Json
#endif
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
#if SPATIAL
    using System.Spatial;
#endif
    using System.Text;
    #endregion Namespaces

    /// <summary>
    /// Reader for the JSON format. http://www.json.org
    /// </summary>
    [DebuggerDisplay("{NodeType}: {Value}")]
#if ODATALIB_PUBLICJSONREADER
    public
#else
    internal
#endif
 class JsonReader
    {
        /// <summary>
        /// The initial size of the buffer of characters.
        /// </summary>
        /// <remarks>
        /// 4K (page size) divided by the size of a single character 2 and a little less
        /// so that array structures also fit into that page.
        /// The goal is for the entire buffer to fit into one page so that we don't cause
        /// too many L1 cache misses.
        /// </remarks>
        private const int InitialCharacterBufferSize = ((4 * 1024) / 2) - 8;

        /// <summary>
        /// Maximum number of characters to move in the buffer. If the current token size is bigger than this, we will allocate a larger buffer.
        /// </summary>
        /// <remarks>This threshold is copied from the XmlReader implementation.</remarks>
        private const int MaxCharacterCountToMove = 128 / 2;

        /// <summary>
        /// The text which every date time value starts with.
        /// </summary>
        private const string DateTimeFormatPrefix = "/Date(";

        /// <summary>
        /// The text which every date time value ends with.
        /// </summary>
        private const string DateTimeFormatSuffix = ")/";

        /// <summary>
        /// The text reader to read input characters from.
        /// </summary>
        private readonly TextReader reader;

        /// <summary>
        /// Stack of scopes.
        /// </summary>
        /// <remarks>
        /// At the begining the Root scope is pushed to the stack and stays there for the entire parsing 
        ///   (so that we don't have to check for empty stack and also to track the number of root-level values)
        /// Each time a new object or array is started the Object or Array scope is pushed to the stack.
        /// If a property inside an Object is found, the Property scope is pushed to the stack.
        /// The Property is popped once we find the value for the property.
        /// The Object and Array scopes are popped when their end is found.
        /// </remarks>
        private readonly Stack<Scope> scopes;

        /// <summary>true if annotations are allowed and thus the reader has to 
        /// accept more characters in property names than we do normally; otherwise false.</summary>
        private readonly bool allowAnnotations;

        /// <summary>true if the reader should recognize ASP.NET JSON DateTime and DateTimeOffset format "\/Date(...)\/".
        /// false if the reader should not recognize such strings and read them as arbitrary string.</summary>
        private readonly bool supportAspNetDateTimeFormat;

        /// <summary>
        /// End of input from the reader was already reached.
        /// </summary>
        /// <remarks>This is used to avoid calling Read on the text reader multiple times
        /// even though it already reported the end of input.</remarks>
        private bool endOfInputReached;

        /// <summary>
        /// Buffer of characters from the input.
        /// </summary>
        private char[] characterBuffer;

        /// <summary>
        /// Number of characters available in the input buffer.
        /// </summary>
        /// <remarks>This can have value of 0 to characterBuffer.Length.</remarks>
        private int storedCharacterCount;

        /// <summary>
        /// Index into the characterBuffer which points to the first character
        /// of the token being currently processed (while in the Read method)
        /// or of the next token to be processed (while in the caller code).
        /// </summary>
        /// <remarks>This can have value from 0 to storedCharacterCount.</remarks>
        private int tokenStartIndex;

        /// <summary>
        /// The last reported node type.
        /// </summary>
        private JsonNodeType nodeType;

        /// <summary>
        /// The value of the last reported node.
        /// </summary>
        private object nodeValue;

        /// <summary>
        /// The json raw string or char of the last reported node.
        /// </summary>
        private string nodeRawValue;

        /// <summary>
        /// Cached string builder to be used when constructing string values (needed to resolve escape sequences).
        /// </summary>
        /// <remarks>The string builder instance is cached to avoid excessive allocation when many string values with escape sequences
        /// are found in the payload.</remarks>
        private StringBuilder stringValueBuilder;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader">The text reader to read input characters from.</param>
        /// <param name="jsonFormat">The specific JSON-based format expected by the reader.</param>
        public JsonReader(TextReader reader, ODataFormat jsonFormat)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(jsonFormat == ODataFormat.Json || jsonFormat == ODataFormat.VerboseJson, "Expected a json-based format to create a JsonReader");

            this.nodeType = JsonNodeType.None;
            this.nodeValue = null;
            this.reader = reader;
            this.characterBuffer = new char[InitialCharacterBufferSize];
            this.storedCharacterCount = 0;
            this.tokenStartIndex = 0;
            this.endOfInputReached = false;
            this.allowAnnotations = jsonFormat == ODataFormat.Json;
            this.supportAspNetDateTimeFormat = jsonFormat == ODataFormat.VerboseJson;
            this.scopes = new Stack<Scope>();
            this.scopes.Push(new Scope(ScopeType.Root));
        }

        /// <summary>
        /// Various scope types for Json writer.
        /// </summary>
        private enum ScopeType
        {
            /// <summary>
            /// Root scope - the top-level of the JSON content.
            /// </summary>
            /// <remarks>This scope is only once on the stack and that is at the bottom, always.
            /// It's used to track the fact that only one top-level value is allowed.</remarks>
            Root,

            /// <summary>
            /// Array scope - inside an array.
            /// </summary>
            /// <remarks>This scope is pushed when [ is found and is active before the first and between the elements in the array.
            /// Between the elements it's active when the parser is in front of the comma, the parser is never after comma as then
            /// it always immediately processed the next token.</remarks>
            Array,

            /// <summary>
            /// Object scope - inside the object (but not in a property value).
            /// </summary>
            /// <remarks>This scope is pushed when { is found and is active before the first and between the properties in the object.
            /// Between the properties it's active when the parser is in front of the comma, the parser is never after comma as then
            /// it always immediately processed the next token.</remarks>
            Object,

            /// <summary>
            /// Property scope - after the property name and colon and througout the value.
            /// </summary>
            /// <remarks>This scope is pushed when a property name and colon is found.
            /// The scope remains on the stack while the property value is parsed, but once the property value ends, it's immediately removed
            /// so that it doesn't appear on the stack after the value (ever).</remarks>
            Property,
        }

        /// <summary>
        /// The value of the last reported node.
        /// </summary>
        /// <remarks>This is non-null only if the last node was a PrimitiveValue or Property.
        /// If the last node is a PrimitiveValue this property returns the value:
        /// - null if the null token was found.
        /// - boolean if the true or false token was found.
        /// - string if a string token was found.
        /// - DateTime if a string token formatted as DateTime was found.
        /// - Int32 if a number which fits into the Int32 was found.
        /// - Double if a number which doesn't fit into Int32 was found.
        /// If the last node is a Property this property returns a string which is the name of the property.
        /// </remarks>
        public virtual object Value
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.nodeValue;
            }
        }

        /// <summary>
        /// The type of the last node read.
        /// </summary>
        public virtual JsonNodeType NodeType
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.nodeType;
            }
        }

        /// <summary>
        /// Gets json raw string/char.
        /// </summary>
        public virtual string RawValue
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.nodeRawValue;
            }
        }

        /// <summary>
        /// Reads the next node from the input.
        /// </summary>
        /// <returns>true if a new node was found, or false if end of input was reached.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Not really feasible to extract code to methods without introducing unnecessary complexity.")]
        public virtual bool Read()
        {
            DebugUtils.CheckNoExternalCallers();

            // Reset the node value.
            this.nodeValue = null;
            this.nodeRawValue = null;

#if DEBUG
            // Reset the node type to None - so that we can verify that the Read method actually sets it.
            this.nodeType = JsonNodeType.None;
#endif

            // Skip any whitespace characters.
            // This also makes sure that we have at least one non-whitespace character available.
            if (!this.SkipWhitespaces())
            {
                return this.EndOfInput();
            }

            Debug.Assert(
                this.tokenStartIndex < this.storedCharacterCount && !IsWhitespaceCharacter(this.characterBuffer[this.tokenStartIndex]),
                "The SkipWhitespaces didn't correctly skip all whitespace characters from the input.");

            Scope currentScope = this.scopes.Peek();

            bool commaFound = false;
            if (this.characterBuffer[this.tokenStartIndex] == ',')
            {
                commaFound = true;
                this.TryAppendJsonRawValue(this.characterBuffer[this.tokenStartIndex]);
                this.tokenStartIndex++;

                // Note that validity of the comma is verified below depending on the current scope.
                // Skip all whitespaces after comma.
                // Note that this causes "Unexpected EOF" error if the comma is the last thing in the input.
                // It might not be the best error message in certain cases, but it's still correct (a JSON payload can never end in comma).
                if (!this.SkipWhitespaces())
                {
                    return this.EndOfInput();
                }

                Debug.Assert(
                    this.tokenStartIndex < this.storedCharacterCount && !IsWhitespaceCharacter(this.characterBuffer[this.tokenStartIndex]),
                    "The SkipWhitespaces didn't correctly skip all whitespace characters from the input.");
            }

            string RawValueTmp = null;
            switch (currentScope.Type)
            {
                case ScopeType.Root:
                    if (commaFound)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Root));
                    }

                    if (currentScope.ValueCount > 0)
                    {
                        // We already found the top-level value, so fail
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_MultipleTopLevelValues);
                    }

                    // We expect a "value" - start array, start object or primitive value
                    this.nodeType = this.ParseValue(out RawValueTmp);
                    this.TryAppendJsonRawValue(RawValueTmp);
                    break;

                case ScopeType.Array:
                    if (commaFound && currentScope.ValueCount == 0)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Array));
                    }

                    // We might see end of array here
                    if (this.characterBuffer[this.tokenStartIndex] == ']')
                    {
                        this.TryAppendJsonRawValue(this.characterBuffer[this.tokenStartIndex]);
                        this.tokenStartIndex++;

                        // End of array is only valid when there was no comma before it.
                        if (commaFound)
                        {
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Array));
                        }

                        this.PopScope();
                        this.nodeType = JsonNodeType.EndArray;
                        break;
                    }

                    if (!commaFound && currentScope.ValueCount > 0)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_MissingComma(ScopeType.Array));
                    }

                    // We expect element which is a "value" - start array, start object or primitive value
                    this.nodeType = this.ParseValue(out RawValueTmp);
                    this.TryAppendJsonRawValue(RawValueTmp);
                    break;

                case ScopeType.Object:
                    if (commaFound && currentScope.ValueCount == 0)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Object));
                    }

                    // We might see end of object here
                    if (this.characterBuffer[this.tokenStartIndex] == '}')
                    {
                        this.TryAppendJsonRawValue(this.characterBuffer[this.tokenStartIndex]);
                        this.tokenStartIndex++;

                        // End of object is only valid when there was no comma before it.
                        if (commaFound)
                        {
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Object));
                        }

                        this.PopScope();
                        this.nodeType = JsonNodeType.EndObject;
                        break;
                    }
                    else
                    {
                        if (!commaFound && currentScope.ValueCount > 0)
                        {
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_MissingComma(ScopeType.Object));
                        }

                        // We expect a property here
                        this.nodeType = this.ParseProperty(out RawValueTmp);
                        this.TryAppendJsonRawValue(RawValueTmp);
                        break;
                    }

                case ScopeType.Property:
                    if (commaFound)
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedComma(ScopeType.Property));
                    }

                    // We expect the property value, which is a "value" - start array, start object or primitive value
                    this.nodeType = this.ParseValue(out RawValueTmp);
                    this.TryAppendJsonRawValue(RawValueTmp);
                    break;

                default:
#if SPATIAL
                    throw JsonReaderExtensions.CreateException(Strings.JsonReader_InternalError);
#else
                    throw JsonReaderExtensions.CreateException(Strings.General_InternalError(InternalErrorCodes.JsonReader_Read));
#endif
            }

            Debug.Assert(
                this.nodeType != JsonNodeType.None && this.nodeType != JsonNodeType.EndOfInput,
                "Read should never go back to None and EndOfInput should be reported by directly returning.");

            return true;
        }

        /// <summary>
        /// Appends current JSON raw string.
        /// </summary>
        /// <param name="rawValue">The string.</param>
        private void TryAppendJsonRawValue(string rawValue)
        {
            this.nodeRawValue += rawValue;
        }

        /// <summary>
        /// Appends current JSON raw string.
        /// </summary>
        /// <param name="rawValue">The char.</param>
        private void TryAppendJsonRawValue(char rawValue)
        {
            this.nodeRawValue += rawValue;
        }

        /// <summary>
        /// Determines if a given character is a whitespace character.
        /// </summary>
        /// <param name="character">The character to test.</param>
        /// <returns>true if the <paramref name="character"/> is a whitespace; false otherwise.</returns>
        /// <remarks>Note that the behavior of this method is different from Char.IsWhitespace, since that method
        /// returns true for all characters defined as whitespace by the Unicode spec (which is a lot of characters),
        /// this one on the other hand recognizes just the whitespaces as defined by the JSON spec.</remarks>
        private static bool IsWhitespaceCharacter(char character)
        {
            // The whitespace characters are 0x20 (space), 0x09 (tab), 0x0A (new line), 0x0D (carriage return)
            // Anything above 0x20 is a non-whitespace character.
            if (character > (char)0x20 || character != (char)0x20 && character != (char)0x09 && character != (char)0x0A && character != (char)0x0D)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Parses a date time primitive value.
        /// </summary>
        /// <param name="stringValue">The string value to parse.</param>
        /// <returns>The parsed date time value, or null if the string value doesn't represent a date time value.</returns>
        private static object TryParseDateTimePrimitiveValue(string stringValue)
        {
            if (!stringValue.StartsWith(DateTimeFormatPrefix, StringComparison.Ordinal) || !stringValue.EndsWith(DateTimeFormatSuffix, StringComparison.Ordinal))
            {
                return null;
            }

            // Note that WCF DS does not fail if the format uses offset (and thus should be DateTimeOffset).
            // As a result it actually reads the value as a string.
            // It might be a breaking change to support DateTimeOffset without versioning or something similar.
            string tickStringValue = stringValue.Substring(
                DateTimeFormatPrefix.Length,
                stringValue.Length - (DateTimeFormatPrefix.Length + DateTimeFormatSuffix.Length));

            // Datetime ticks can be negative if it is less than the json reference datetime.
            // Hence while looking for the offset ticks, we need to start searching after the first
            // character.
            int index = tickStringValue.IndexOfAny(new char[] { '+', '-' }, 1);
            if (index == -1)
            {
                long ticks;
                if (long.TryParse(tickStringValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out ticks))
                {
                    return new DateTime(JsonValueUtils.JsonTicksToDateTimeTicks(ticks), DateTimeKind.Utc);
                }
            }
            else
            {
                string offsetMinutesStringValue = tickStringValue.Substring(index);
                tickStringValue = tickStringValue.Substring(0, index);

                long ticks;
                int offsetMinutes;
                if (long.TryParse(tickStringValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out ticks) &&
                    int.TryParse(offsetMinutesStringValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out offsetMinutes))
                {
                    return new DateTimeOffset(JsonValueUtils.JsonTicksToDateTimeTicks(ticks), new TimeSpan(0, offsetMinutes, 0));
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a "value", that is an array, object or primitive value.
        /// </summary>
        /// <param name="rawValue">The raw string, out parameter.</param>
        /// <returns>The node type to report to the user.</returns>
        private JsonNodeType ParseValue(out string rawValue)
        {
            Debug.Assert(
                this.tokenStartIndex < this.storedCharacterCount && !IsWhitespaceCharacter(this.characterBuffer[this.tokenStartIndex]),
                "The SkipWhitespaces wasn't called or it didn't correctly skip all whitespace characters from the input.");
            Debug.Assert(this.scopes.Count >= 1 && this.scopes.Peek().Type != ScopeType.Object, "Value can only occure at the root, in array or as a property value.");

            // Increase the count of values under the current scope.
            this.scopes.Peek().ValueCount++;

            char currentCharacter = this.characterBuffer[this.tokenStartIndex];
            switch (currentCharacter)
            {
                case '{':
                    // Start of object
                    this.PushScope(ScopeType.Object);
                    this.tokenStartIndex++;
                    rawValue = currentCharacter.ToString();
                    return JsonNodeType.StartObject;

                case '[':
                    // Start of array
                    this.PushScope(ScopeType.Array);
                    this.tokenStartIndex++;
                    rawValue = currentCharacter.ToString();
                    return JsonNodeType.StartArray;

                case '"':
                case '\'':
                    // String primitive value
                    bool hasLeadingBackslash;
                    this.nodeValue = rawValue = this.ParseStringPrimitiveValue(out hasLeadingBackslash);
                    rawValue = string.Format(CultureInfo.InvariantCulture, "{0}{1}{0}", currentCharacter, rawValue);

                    // If the value started with \ then try to parse it as a date time value.
                    if (hasLeadingBackslash && this.supportAspNetDateTimeFormat)
                    {
                        object dateTimeValue = TryParseDateTimePrimitiveValue((string)this.nodeValue);
                        if (dateTimeValue != null)
                        {
                            this.nodeValue = dateTimeValue;
                        }
                    }

                    break;

                case 'n':
                    this.nodeValue = this.ParseNullPrimitiveValue(out rawValue);
                    break;

                case 't':
                case 'f':
                    this.nodeValue = this.ParseBooleanPrimitiveValue(out rawValue);
                    break;

                default:
                    if (Char.IsDigit(currentCharacter) || (currentCharacter == '-') || (currentCharacter == '.'))
                    {
                        this.nodeValue = this.ParseNumberPrimitiveValue(out rawValue);
                        break;
                    }
                    else
                    {
                        // Unknown token - fail.
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedToken);
                    }
            }

            this.TryPopPropertyScope();
            return JsonNodeType.PrimitiveValue;
        }

        /// <summary>
        /// Parses a property name and the colon after it.
        /// </summary>
        /// <param name="rawValue">The raw string, out parameter.</param>
        /// <returns>The node type to report to the user.</returns>
        private JsonNodeType ParseProperty(out string rawValue)
        {
            // Increase the count of values under the object (the number of properties).
            Debug.Assert(this.scopes.Count >= 1 && this.scopes.Peek().Type == ScopeType.Object, "Property can only occur in an object.");
            this.scopes.Peek().ValueCount++;

            this.PushScope(ScopeType.Property);

            // Parse the name of the property
            this.nodeValue = this.ParseName(out rawValue);

            if (string.IsNullOrEmpty((string)this.nodeValue))
            {
                // The name can't be empty.
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_InvalidPropertyNameOrUnexpectedComma((string)this.nodeValue));
            }

            if (!this.SkipWhitespaces() || this.characterBuffer[this.tokenStartIndex] != ':')
            {
                // We need the colon character after the property name
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_MissingColon((string)this.nodeValue));
            }

            // Consume the colon.
            Debug.Assert(this.characterBuffer[this.tokenStartIndex] == ':', "The above should verify that there's a colon.");
            rawValue += ":";
            this.tokenStartIndex++;

            return JsonNodeType.Property;
        }

        /// <summary>
        /// Parses a primitive string value.
        /// </summary>
        /// <returns>The value of the string primitive value.</returns>
        /// <remarks>
        /// Assumes that the current token position points to the opening quote.
        /// Note that the string parsing can never end with EndOfInput, since we're already seen the quote.
        /// So it can either return a string succesfully or fail.</remarks>
        private string ParseStringPrimitiveValue()
        {
            bool hasLeadingBackslash;
            return this.ParseStringPrimitiveValue(out hasLeadingBackslash);
        }

        /// <summary>
        /// Parses a primitive string value.
        /// </summary>
        /// <param name="hasLeadingBackslash">Set to true if the first character in the string was a backslash. This is used when parsing DateTime values
        /// since they must start with an escaped slash character (\/).</param>
        /// <returns>The value of the string primitive value.</returns>
        /// <remarks>
        /// Assumes that the current token position points to the opening quote.
        /// Note that the string parsing can never end with EndOfInput, since we're already seen the quote.
        /// So it can either return a string succesfully or fail.</remarks>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Splitting the function would make it hard to understand.")]
        private string ParseStringPrimitiveValue(out bool hasLeadingBackslash)
        {
            Debug.Assert(this.tokenStartIndex < this.storedCharacterCount, "At least the quote must be present.");

            hasLeadingBackslash = false;

            char openingQuoteCharacter = this.characterBuffer[this.tokenStartIndex];
            Debug.Assert(openingQuoteCharacter == '"' || openingQuoteCharacter == '\'', "The quote character must be the current character when this method is called.");

            // Consume the quote character
            this.tokenStartIndex++;

            // String builder to be used if we need to resolve escape sequences.
            StringBuilder valueBuilder = null;

            int currentCharacterTokenRelativeIndex = 0;
            while ((this.tokenStartIndex + currentCharacterTokenRelativeIndex) < this.storedCharacterCount || this.ReadInput())
            {
                Debug.Assert((this.tokenStartIndex + currentCharacterTokenRelativeIndex) < this.storedCharacterCount, "ReadInput didn't read more data but returned true.");

                char character = this.characterBuffer[this.tokenStartIndex + currentCharacterTokenRelativeIndex];
                if (character == '\\')
                {
                    // If we're at the begining of the string
                    // (means that relative token index must be 0 and we must not have consumed anything into our value builder yet)
                    if (currentCharacterTokenRelativeIndex == 0 && valueBuilder == null)
                    {
                        hasLeadingBackslash = true;
                    }

                    // We will need the stringbuilder to resolve the escape sequences.
                    if (valueBuilder == null)
                    {
                        if (this.stringValueBuilder == null)
                        {
                            this.stringValueBuilder = new StringBuilder();
                        }
                        else
                        {
                            this.stringValueBuilder.Length = 0;
                        }

                        valueBuilder = this.stringValueBuilder;
                    }

                    // Append everything up to the \ character to the value.
                    valueBuilder.Append(this.ConsumeTokenToString(currentCharacterTokenRelativeIndex));
                    currentCharacterTokenRelativeIndex = 0;
                    Debug.Assert(this.characterBuffer[this.tokenStartIndex] == '\\', "We should have consumed everything up to the escape character.");

                    // Escape sequence - we need at least two characters, the backslash and the one character after it.
                    if (!this.EnsureAvailableCharacters(2))
                    {
                        throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence("\\"));
                    }

                    // To simplify the code, consume the character after the \ as well, since that is the start of the escape sequence.
                    character = this.characterBuffer[this.tokenStartIndex + 1];
                    this.tokenStartIndex += 2;

                    switch (character)
                    {
                        case 'b':
                            valueBuilder.Append('\b');
                            break;
                        case 'f':
                            valueBuilder.Append('\f');
                            break;
                        case 'n':
                            valueBuilder.Append('\n');
                            break;
                        case 'r':
                            valueBuilder.Append('\r');
                            break;
                        case 't':
                            valueBuilder.Append('\t');
                            break;
                        case '\\':
                        case '\"':
                        case '\'':
                        case '/':
                            valueBuilder.Append(character);
                            break;
                        case 'u':
                            Debug.Assert(currentCharacterTokenRelativeIndex == 0, "The token should be starting at the first character after the \\u");

                            // We need 4 hex characters
                            if (!this.EnsureAvailableCharacters(4))
                            {
                                throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence("\\uXXXX"));
                            }

                            string unicodeHexValue = this.ConsumeTokenToString(4);
                            int characterValue;
                            if (!Int32.TryParse(unicodeHexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out characterValue))
                            {
                                throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence("\\u" + unicodeHexValue));
                            }

                            valueBuilder.Append((char)characterValue);
                            break;
                        default:
                            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnrecognizedEscapeSequence("\\" + character));
                    }
                }
                else if (character == openingQuoteCharacter)
                {
                    // Consume everything up to the quote character
                    string result = this.ConsumeTokenToString(currentCharacterTokenRelativeIndex);
                    Debug.Assert(this.characterBuffer[this.tokenStartIndex] == openingQuoteCharacter, "We should have consumed everything up to the quote character.");

                    // Consume the quote character as well.
                    this.tokenStartIndex++;

                    if (valueBuilder != null)
                    {
                        valueBuilder.Append(result);
                        result = valueBuilder.ToString();
                    }

                    return result;
                }
                else
                {
                    // Normal character, just skip over it - it will become part of the value as is.
                    currentCharacterTokenRelativeIndex++;
                }
            }

            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedEndOfString);
        }

        /// <summary>
        /// Parses the null primitive value.
        /// </summary>
        /// <param name="rawValue">The raw string, out parameter.</param>
        /// <returns>Always returns null if successful. Otherwise throws.</returns>
        /// <remarks>Assumes that the current token position points to the 'n' character.</remarks>
        private object ParseNullPrimitiveValue(out string rawValue)
        {
            Debug.Assert(
                this.tokenStartIndex < this.storedCharacterCount && this.characterBuffer[this.tokenStartIndex] == 'n',
                "The method should only be called when the 'n' character is the start of the token.");

            // We can call ParseName since we know the first character is 'n' and thus it won't be quoted.
            string token = this.ParseName(out rawValue);

            if (!string.Equals(token, JsonConstants.JsonNullLiteral))
            {
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedToken(token));
            }

            return null;
        }

        /// <summary>
        /// Parses the true or false primitive values.
        /// </summary>
        /// <param name="rawValue">The raw string, out parameter.</param>
        /// <returns>true of false boolean value if successful. Otherwise throws.</returns>
        /// <remarks>Assumes that the current token position points to the 't' or 'f' character.</remarks>
        private object ParseBooleanPrimitiveValue(out string rawValue)
        {
            Debug.Assert(
                this.tokenStartIndex < this.storedCharacterCount && (this.characterBuffer[this.tokenStartIndex] == 't' || this.characterBuffer[this.tokenStartIndex] == 'f'),
                "The method should only be called when the 't' or 'f' character is the start of the token.");

            // We can call ParseName since we know the first character is 't' or 'f' and thus it won't be quoted.
            string token = this.ParseName(out rawValue);
            if (string.Equals(token, JsonConstants.JsonFalseLiteral))
            {
                return false;
            }

            if (string.Equals(token, JsonConstants.JsonTrueLiteral))
            {
                return true;
            }

            throw JsonReaderExtensions.CreateException(Strings.JsonReader_UnexpectedToken(token));
        }

        /// <summary>
        /// Parses the number primitive values.
        /// </summary>
        /// <param name="rawValue">The raw string, out parameter.</param>
        /// <returns>Int32 or Double value if successful. Otherwise throws.</returns>
        /// <remarks>Assumes that the current token position points to the first character of the number, so either digit, dot or dash.</remarks>
        private object ParseNumberPrimitiveValue(out string rawValue)
        {
            Debug.Assert(
                this.tokenStartIndex < this.storedCharacterCount && (this.characterBuffer[this.tokenStartIndex] == '.' || this.characterBuffer[this.tokenStartIndex] == '-' || Char.IsDigit(this.characterBuffer[this.tokenStartIndex])),
                "The method should only be called when a digit, dash or dot character is the start of the token.");

            // Walk over all characters which might belong to the number
            // Skip the first one since we already verified it belongs to the number.
            int currentCharacterTokenRelativeIndex = 1;
            while ((this.tokenStartIndex + currentCharacterTokenRelativeIndex) < this.storedCharacterCount || this.ReadInput())
            {
                char character = this.characterBuffer[this.tokenStartIndex + currentCharacterTokenRelativeIndex];
                if (Char.IsDigit(character) ||
                    (character == '.') ||
                    (character == 'E') ||
                    (character == 'e') ||
                    (character == '-') ||
                    (character == '+'))
                {
                    currentCharacterTokenRelativeIndex++;
                }
                else
                {
                    break;
                }
            }

            // We now have all the characters which belong to the number, consume it into a string.
            string numberString = this.ConsumeTokenToString(currentCharacterTokenRelativeIndex);
            rawValue = numberString;
            double doubleValue;
            int intValue;

            // We will first try and convert the value to Int32. If it succeeds, use that.
            // Otherwise, we will try and convert the value into a double.
            if (Int32.TryParse(numberString, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out intValue))
            {
                return intValue;
            }

            if (Double.TryParse(numberString, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out doubleValue))
            {
                return doubleValue;
            }

            throw JsonReaderExtensions.CreateException(Strings.JsonReader_InvalidNumberFormat(numberString));
        }

        /// <summary>
        /// Parses a name token.
        /// </summary>
        /// <param name="rawValue">The raw string, out parameter.</param>
        /// <returns>The value of the name token.</returns>
        /// <remarks>Name tokens are (for backward compat reasons) either
        /// - string value quoted with double quotes.
        /// - string value quoted with single quotes.
        /// - sequence of letters, digits, underscores and dollar signs (without quoted and in any order).</remarks>
        private string ParseName(out string rawValue)
        {
            Debug.Assert(this.tokenStartIndex < this.storedCharacterCount, "Must have at least one character available.");

            char firstCharacter = this.characterBuffer[this.tokenStartIndex];
            if ((firstCharacter == '"') || (firstCharacter == '\''))
            {
                string ret = this.ParseStringPrimitiveValue();
                rawValue = string.Format(CultureInfo.InvariantCulture, "{0}{1}{0}", firstCharacter, ret);
                return ret;
            }

            int currentCharacterTokenRelativeIndex = 0;
            do
            {
                Debug.Assert(this.tokenStartIndex < this.storedCharacterCount, "Must have at least one character available.");

                char character = this.characterBuffer[this.tokenStartIndex + currentCharacterTokenRelativeIndex];

                if (character == '_' ||
                    Char.IsLetterOrDigit(character) ||
                    character == '$' ||
                    (this.allowAnnotations && (character == '.' || character == '@')))
                {
                    currentCharacterTokenRelativeIndex++;
                }
                else
                {
                    break;
                }
            }
            while ((this.tokenStartIndex + currentCharacterTokenRelativeIndex) < this.storedCharacterCount || this.ReadInput());

            rawValue = this.ConsumeTokenToString(currentCharacterTokenRelativeIndex);
            return rawValue;
        }

        /// <summary>
        /// Called when end of input is reached.
        /// </summary>
        /// <returns>Always returns false, used for easy readability of the callers.</returns>
        private bool EndOfInput()
        {
            // We should be ending the input only with Root in the scope.
            if (this.scopes.Count > 1)
            {
                // Not all open scopes were closed.
                throw JsonReaderExtensions.CreateException(Strings.JsonReader_EndOfInputWithOpenScope);
            }

            Debug.Assert(
                this.scopes.Count > 0 && this.scopes.Peek().Type == ScopeType.Root && this.scopes.Peek().ValueCount <= 1,
                "The end of input should only occure with root at the top of the stack with zero or one value.");
            Debug.Assert(this.nodeValue == null, "The node value should have been reset to null.");

            this.nodeType = JsonNodeType.EndOfInput;
            return false;
        }

        /// <summary>
        /// Creates a new scope of type <paramref name="newScopeType"/> and pushes the stack.
        /// </summary>
        /// <param name="newScopeType">The scope type to push.</param>
        private void PushScope(ScopeType newScopeType)
        {
            Debug.Assert(this.scopes.Count >= 1, "The root must always be on the stack.");
            Debug.Assert(newScopeType != ScopeType.Root, "We should never try to push root scope.");
            Debug.Assert(newScopeType != ScopeType.Property || this.scopes.Peek().Type == ScopeType.Object, "We should only try to push property onto an object.");
            Debug.Assert(newScopeType == ScopeType.Property || this.scopes.Peek().Type != ScopeType.Object, "We should only try to push property onto an object.");

            this.scopes.Push(new Scope(newScopeType));
        }

        /// <summary>
        /// Pops a scope from the stack.
        /// </summary>
        private void PopScope()
        {
            Debug.Assert(this.scopes.Count > 1, "We can never pop the root.");
            Debug.Assert(this.scopes.Peek().Type != ScopeType.Property, "We should never try to pop property alone.");

            this.scopes.Pop();
            this.TryPopPropertyScope();
        }

        /// <summary>
        /// Pops a property scope if it's present on the stack.
        /// </summary>
        private void TryPopPropertyScope()
        {
            Debug.Assert(this.scopes.Count > 0, "There should always be at least root on the stack.");
            if (this.scopes.Peek().Type == ScopeType.Property)
            {
                Debug.Assert(this.scopes.Count > 2, "If the property is at the top of the stack there must be an object after it and then root.");
                this.scopes.Pop();
                Debug.Assert(this.scopes.Peek().Type == ScopeType.Object, "The parent of a property must be an object.");
            }
        }

        /// <summary>
        /// Skips all whitespace characters in the input.
        /// </summary>
        /// <returns>true if a non-whitespace character was found in which case the tokenStartIndex is pointing at that character.
        /// false if there are no non-whitespace characters left in the input.</returns>
        private bool SkipWhitespaces()
        {
            do
            {
                for (; this.tokenStartIndex < this.storedCharacterCount; this.tokenStartIndex++)
                {
                    if (!IsWhitespaceCharacter(this.characterBuffer[this.tokenStartIndex]))
                    {
                        return true;
                    }
                }
            }
            while (this.ReadInput());

            return false;
        }

        /// <summary>
        /// Ensures that a specified number of characters after the token start is available in the buffer.
        /// </summary>
        /// <param name="characterCountAfterTokenStart">The number of character after the token to make available.</param>
        /// <returns>true if at least the required number of characters is available; false if end of input was reached.</returns>
        private bool EnsureAvailableCharacters(int characterCountAfterTokenStart)
        {
            while (this.tokenStartIndex + characterCountAfterTokenStart > this.storedCharacterCount)
            {
                if (!this.ReadInput())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes the <paramref name="characterCount"/> characters starting at the start of the token
        /// and returns them as a string.
        /// </summary>
        /// <param name="characterCount">The number of characters after the token start to consume.</param>
        /// <returns>The string value of the consumed token.</returns>
        private string ConsumeTokenToString(int characterCount)
        {
            Debug.Assert(characterCount >= 0, "characterCount >= 0");
            Debug.Assert(this.tokenStartIndex + characterCount <= this.storedCharacterCount, "characterCount specified characters outside of the available range.");

            string result = new string(this.characterBuffer, this.tokenStartIndex, characterCount);
            this.tokenStartIndex += characterCount;

            return result;
        }

        /// <summary>
        /// Reads more characters from the input.
        /// </summary>
        /// <returns>true if more characters are available; false if end of input was reached.</returns>
        /// <remarks>This may move characters in the characterBuffer, so after this is called
        /// all indeces to the characterBuffer are invalid except for tokenStartIndex.</remarks>
        private bool ReadInput()
        {
            Debug.Assert(this.storedCharacterCount <= this.characterBuffer.Length, "We can only store as many characters as fit into our buffer.");
            Debug.Assert(this.tokenStartIndex >= 0 && this.tokenStartIndex <= this.storedCharacterCount, "The token start is out of stored characters range.");

            if (this.endOfInputReached)
            {
                return false;
            }

            // We need to make sure we have more room in the buffer to read characters into
            if (this.storedCharacterCount == this.characterBuffer.Length)
            {
                // No more room in the buffer, move or grow the buffer.
                if (this.tokenStartIndex == this.storedCharacterCount)
                {
                    // If the buffer is empty (all characters were consumed from it)
                    // just start over.
                    this.tokenStartIndex = 0;
                    this.storedCharacterCount = 0;
                }
                else if (this.tokenStartIndex > (this.characterBuffer.Length - MaxCharacterCountToMove))
                {
                    // Some characters were consumed, we can just move them in the buffer
                    // to get more room without allocating.
                    Array.Copy(
                        this.characterBuffer,
                        this.tokenStartIndex,
                        this.characterBuffer,
                        0,
                        this.storedCharacterCount - this.tokenStartIndex);
                    this.storedCharacterCount -= this.tokenStartIndex;
                    this.tokenStartIndex = 0;
                }
                else
                {
                    // The entire buffer is full of unconsumed characters
                    // We need to grow the buffer.
                    // Double the size of the buffer.
                    int newBufferSize = this.characterBuffer.Length * 2;
                    char[] newCharacterBuffer = new char[newBufferSize];

                    // Copy the existing characters to the new buffer.
                    Array.Copy(
                        this.characterBuffer,
                        0,  // The tokenStartIndex is 0 - we checked above.
                        newCharacterBuffer,
                        0,
                        this.characterBuffer.Length);  // The storedCharacterCount is the size of the old buffer

                    // And switch the buffers
                    this.characterBuffer = newCharacterBuffer;

                    // Note that the number of characters stored in the buffer remains unchanged
                    // as well as the token start index which is 0.
                }
            }

            Debug.Assert(
                this.storedCharacterCount < this.characterBuffer.Length,
                "We should have more room in the buffer by now.");

            // Read more characters from the input.
            // Use the Read method which returns any character as soon as it's available
            // we don't want to wait for the entire buffer to fill if the input doesn't have
            // the characters ready.
            int readCount = this.reader.Read(
                this.characterBuffer,
                this.storedCharacterCount,
                this.characterBuffer.Length - this.storedCharacterCount);

            if (readCount == 0)
            {
                // No more characters available, end of input.
                this.endOfInputReached = true;
                return false;
            }

            this.storedCharacterCount += readCount;
            return true;
        }

        /// <summary>
        /// Class representing scope information.
        /// </summary>
        private sealed class Scope
        {
            /// <summary>
            /// The type of the scope.
            /// </summary>
            private readonly ScopeType type;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="type">The type of the scope.</param>
            public Scope(ScopeType type)
            {
                this.type = type;
            }

            /// <summary>
            /// Get/Set the number of values found under the current scope.
            /// </summary>
            public int ValueCount
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the scope type for this scope.
            /// </summary>
            public ScopeType Type
            {
                get
                {
                    return this.type;
                }
            }
        }
    }
}

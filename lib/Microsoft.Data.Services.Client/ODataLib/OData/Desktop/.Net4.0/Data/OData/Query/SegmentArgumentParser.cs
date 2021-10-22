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

namespace Microsoft.Data.OData.Query
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;

    /// <summary>Provides a class used to represent a key for a resource.</summary>
    /// <remarks>
    /// Internally, every key instance has a collection of values. These values
    /// can be named or positional, depending on how they were specified
    /// if parsed from a URI.
    /// </remarks>
    internal sealed class SegmentArgumentParser
    {
        /// <summary>Empty key singleton.</summary>
        private static readonly SegmentArgumentParser Empty = new SegmentArgumentParser();

        /// <summary>Named values.</summary>
        private readonly Dictionary<string, string> namedValues;

        /// <summary>Positional values.</summary>
        private readonly List<string> positionalValues;

        /// <summary>Whether or not the key was formatted as a segment.</summary>
        private readonly bool keysAsSegments;

        /// <summary>Initializes a new empty <see cref="SegmentArgumentParser"/> instance.</summary>
        private SegmentArgumentParser()
        {
        }

        /// <summary>Initializes a new <see cref="SegmentArgumentParser"/> instance.</summary>
        /// <param name='namedValues'>Named values.</param>
        /// <param name='positionalValues'>Positional values for this instance.</param>
        /// <param name="keysAsSegments">Whether or not the key was formatted as a segment.</param>
        /// <remarks>
        /// One of namedValues or positionalValues should be non-null, but not both.
        /// </remarks>
        private SegmentArgumentParser(Dictionary<string, string> namedValues, List<string> positionalValues, bool keysAsSegments)
        {
            Debug.Assert(
                (namedValues == null) != (positionalValues == null),
                "namedValues == null != positionalValues == null -- one or the other should be assigned, but not both");
            this.namedValues = namedValues;
            this.positionalValues = positionalValues;
            this.keysAsSegments = keysAsSegments;
        }

        /// <summary>Whether the values have a name.</summary>
        internal bool AreValuesNamed
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.namedValues != null;
            }
        }

        /// <summary>Checks whether this key has any values.</summary>
        internal bool IsEmpty
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this == Empty;
            }
        }

        /// <summary>Returns a dictionary of named values when they AreValuesNamed is true.</summary>
        internal IDictionary<string, string> NamedValues
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.namedValues;
            }
        }

        /// <summary>Returns a list of values when they AreValuesNamed is false.</summary>
        internal IList<string> PositionalValues
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.positionalValues;
            }
        }

        /// <summary>Number of values in the key.</summary>
        internal int ValueCount
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                if (this == Empty)
                {
                    return 0;
                }

                if (this.namedValues != null)
                {
                    return this.namedValues.Count;
                }

                Debug.Assert(this.positionalValues != null, "this.positionalValues != null");
                return this.positionalValues.Count;
            }
        }

        /// <summary>Attempts to parse key values from the specified text.</summary>
        /// <param name='text'>Text to parse (not null).</param>
        /// <param name='instance'>After invocation, the parsed key instance.</param>
        /// <returns>
        /// true if the key instance was parsed; false if there was a 
        /// syntactic error.
        /// </returns>
        /// <remarks>
        /// The returned instance contains only string values. To get typed values, a call to
        /// TryConvertValues is necessary.
        /// </remarks>
        internal static bool TryParseKeysFromUri(string text, out SegmentArgumentParser instance)
        {
            DebugUtils.CheckNoExternalCallers();
            return TryParseFromUri(text, true /*allowNamedValues*/, false /*allowNull*/, out instance);
        }

        /// <summary>
        /// Creates a key instance from the given raw segment text with a single positional value.
        /// </summary>
        /// <param name="segmentText">The segment text.</param>
        /// <returns>A key instance with the given segment text as its only value.</returns>
        internal static SegmentArgumentParser FromSegment(string segmentText)
        {
            DebugUtils.CheckNoExternalCallers();
            return new SegmentArgumentParser(null, new List<string> { segmentText }, true);
        }

        /// <summary>Attempts to parse nullable values (only positional values, no name-value pairs) from the specified text.</summary>
        /// <param name='text'>Text to parse (not null).</param>
        /// <param name='instance'>After invocation, the parsed key instance.</param>
        /// <returns>
        /// true if the given values were parsed; false if there was a 
        /// syntactic error.
        /// </returns>
        /// <remarks>
        /// The returned instance contains only string values. To get typed values, a call to
        /// TryConvertValues is necessary.
        /// </remarks>
        internal static bool TryParseNullableTokens(string text, out SegmentArgumentParser instance)
        {
            DebugUtils.CheckNoExternalCallers();
            return TryParseFromUri(text, false /*allowNamedValues*/, true /*allowNull*/, out instance);
        }

        /// <summary>Tries to convert values to the keys of the specified type.</summary>
        /// <param name="keyProperties">The key properties to use for the conversion.</param>
        /// <param name="keyPairs">The converted key-value pairs.</param>
        /// <returns>true if all values were converted; false otherwise.</returns>
        internal bool TryConvertValues(IList<IEdmStructuralProperty> keyProperties, out IEnumerable<KeyValuePair<string, object>> keyPairs)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(!this.IsEmpty, "!this.IsEmpty -- caller should check");
            Debug.Assert(keyProperties.Count == this.ValueCount, "type.KeyProperties.Count == this.ValueCount -- will change with containment");

            if (this.NamedValues != null)
            {
                var convertedPairs = new Dictionary<string, object>(StringComparer.Ordinal);
                keyPairs = convertedPairs;

                foreach (IEdmStructuralProperty property in keyProperties)
                {
                    string valueText;
                    if (!this.NamedValues.TryGetValue(property.Name, out valueText))
                    {
                        return false;
                    }

                    Debug.Assert(property.Type.IsPrimitive(), "Keys can only be primitive");
                    object convertedValue;
                    bool result = TryConvertValue(property.Type.AsPrimitive(), valueText, out convertedValue);
                    if (!result)
                    {
                        return false;
                    }

                    convertedPairs[property.Name] = convertedValue;
                }
            }
            else
            {
                Debug.Assert(this.positionalValues != null, "positionalValues != null -- otherwise this is Empty");
                Debug.Assert(this.PositionalValues.Count == keyProperties.Count, "Count of positional values does not match.");

                var keyPairList = new List<KeyValuePair<string, object>>(this.positionalValues.Count);
                keyPairs = keyPairList;

                for (int i = 0; i < keyProperties.Count; i++)
                {
                    string valueText = (string)this.positionalValues[i];
                    IEdmProperty keyProperty = keyProperties[i];
                    object convertedValue;
                    bool result = TryConvertValue(keyProperty.Type.AsPrimitive(), valueText, out convertedValue);
                    if (!result)
                    {
                        return false;
                    }

                    keyPairList.Add(new KeyValuePair<string, object>(keyProperty.Name, convertedValue));
                }
            }

            return true;
        }

        /// <summary>
        /// Try to convert a value into an EDM primitive type
        /// </summary>
        /// <param name="primitiveType">the type to convert to</param>
        /// <param name="valueText">the value to convert</param>
        /// <param name="convertedValue">The converted value, if conversion succeeded.</param>
        /// <returns>true if the conversion was successful.</returns>
        private bool TryConvertValue(IEdmPrimitiveTypeReference primitiveType, string valueText, out object convertedValue)
        {
            Type primitiveClrType = EdmLibraryExtensions.GetPrimitiveClrType((IEdmPrimitiveType)primitiveType.Definition, primitiveType.IsNullable);
            LiteralParser literalParser = LiteralParser.ForKeys(this.keysAsSegments);
            return literalParser.TryParseLiteral(primitiveClrType, valueText, out convertedValue);
        }

        /// <summary>Attempts to parse key values from the specified text.</summary>
        /// <param name='text'>Text to parse (not null).</param>
        /// <param name="allowNamedValues">Set to true if the parser should accept named values
        ///     so syntax like Name='value'. If this is false, the parsing will fail on such constructs.</param>
        /// <param name="allowNull">Set to true if the parser should accept null values.
        ///     If set to false, the parser will fail on null values.</param>
        /// <param name='instance'>After invocation, the parsed key instance.</param>
        /// <returns>
        /// true if the key instance was parsed; false if there was a 
        /// syntactic error.
        /// </returns>
        /// <remarks>
        /// The returned instance contains only string values. To get typed values, a call to
        /// TryConvertValues is necessary.
        /// </remarks>
        private static bool TryParseFromUri(string text, bool allowNamedValues, bool allowNull, out SegmentArgumentParser instance)
        {
            Debug.Assert(text != null, "text != null");

            Dictionary<string, string> namedValues = null;
            List<string> positionalValues = null;
            ExpressionLexer lexer = new ExpressionLexer(text, true, false);
            ExpressionToken currentToken = lexer.CurrentToken;
            if (currentToken.Kind == ExpressionTokenKind.End)
            {
                instance = Empty;
                return true;
            }

            instance = null;
            do
            {
                if (currentToken.Kind == ExpressionTokenKind.Identifier && allowNamedValues)
                {
                    // Name-value pair.
                    if (positionalValues != null)
                    {
                        // We cannot mix named and non-named values.
                        return false;
                    }

                    string identifier = lexer.CurrentToken.GetIdentifier();
                    lexer.NextToken();
                    if (lexer.CurrentToken.Kind != ExpressionTokenKind.Equal)
                    {
                        return false;
                    }

                    lexer.NextToken();
                    if (!lexer.CurrentToken.IsKeyValueToken)
                    {
                        return false;
                    }

                    string namedValue = lexer.CurrentToken.Text;
                    CreateIfNull(ref namedValues);
                    if (namedValues.ContainsKey(identifier))
                    {
                        // Duplicate name.
                        return false;
                    }

                    namedValues.Add(identifier, namedValue);
                }
                else if (currentToken.IsKeyValueToken || (allowNull && currentToken.Kind == ExpressionTokenKind.NullLiteral))
                {
                    // Positional value.
                    if (namedValues != null)
                    {
                        // We cannot mix named and non-named values.
                        return false;
                    }

                    CreateIfNull(ref positionalValues);
                    positionalValues.Add(lexer.CurrentToken.Text);
                }
                else
                {
                    return false;
                }

                // Read the next token. We should be at the end, or find
                // we have a comma followed by something.
                lexer.NextToken();
                currentToken = lexer.CurrentToken;
                if (currentToken.Kind == ExpressionTokenKind.Comma)
                {
                    lexer.NextToken();
                    currentToken = lexer.CurrentToken;
                    if (currentToken.Kind == ExpressionTokenKind.End)
                    {
                        // Trailing comma.
                        return false;
                    }
                }
            }
            while (currentToken.Kind != ExpressionTokenKind.End);

            instance = new SegmentArgumentParser(namedValues, positionalValues, false);
            return true;
        }

        /// <summary>Creates a new instance if the specified value is null.</summary>
        /// <typeparam name="T">Type of variable.</typeparam>
        /// <param name="value">Current value.</param>
        private static void CreateIfNull<T>(ref T value) where T : new()
        {
            if (value == null)
            {
                value = new T();
            }
        }
    }
}

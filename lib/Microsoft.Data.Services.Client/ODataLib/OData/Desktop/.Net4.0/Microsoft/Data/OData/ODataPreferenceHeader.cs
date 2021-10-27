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

namespace Microsoft.Data.OData
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Class to set the "Prefer" header on an <see cref="IODataRequestMessage"/> or 
    /// the "Preference-Applied" header on an <see cref="IODataResponseMessage"/>.
    /// </summary>
    public sealed class ODataPreferenceHeader
    {
        /// <summary>
        /// The return-no-content preference token.
        /// </summary>
        private const string ReturnNoContentPreferenceToken = "return-no-content";

        /// <summary>
        /// The return-content preference token.
        /// </summary>
        private const string ReturnContentPreferenceToken = "return-content";

        /// <summary>
        /// The odata-annotations preference-extensions token.
        /// </summary>
        private const string ODataAnnotationPreferenceToken = "odata.include-annotations";

        /// <summary>
        /// The Prefer header name.
        /// </summary>
        private const string PreferHeaderName = "Prefer";

        /// <summary>
        /// The Preference-Applied header name.
        /// </summary>
        private const string PreferenceAppliedHeaderName = "Preference-Applied";

        /// <summary>
        /// Empty header parameters
        /// </summary>
        private static readonly KeyValuePair<string, string>[] EmptyParameters = new KeyValuePair<string, string>[0];

        /// <summary>
        /// The return-no-content preference.
        /// </summary>
        private static readonly HttpHeaderValueElement ReturnNoContentPreference = new HttpHeaderValueElement(ReturnNoContentPreferenceToken, null, EmptyParameters);

        /// <summary>
        /// The return-content preference.
        /// </summary>
        private static readonly HttpHeaderValueElement ReturnContentPreference = new HttpHeaderValueElement(ReturnContentPreferenceToken, null, EmptyParameters);

        /// <summary>
        /// The message to set the preference header to and to get the preference header from.
        /// </summary>
        private readonly ODataMessage message;

        /// <summary>
        /// "Prefer" if message is an IODataRequestMessage; "Preference-Applied" if message is an IODataResponseMessage.
        /// </summary>
        private readonly string preferenceHeaderName;

        /// <summary>
        /// Dictionary of preferences in the header
        /// </summary>
        private HttpHeaderValue preferences;

        /// <summary>
        /// Internal constructor to instantiate an <see cref="ODataPreferenceHeader"/> from an <see cref="IODataRequestMessage"/>.
        /// </summary>
        /// <param name="requestMessage">The request message to get and set the "Prefer" header.</param>
        internal ODataPreferenceHeader(IODataRequestMessage requestMessage)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(requestMessage != null, "requestMessage != null");
            this.message = new ODataRequestMessage(requestMessage, /*writing*/ true, /*disableMessageStreamDisposal*/ false, /*maxMessageSize*/ -1);
            this.preferenceHeaderName = PreferHeaderName;
        }

        /// <summary>
        /// Internal constructor to instantiate an <see cref="ODataPreferenceHeader"/> from an <see cref="IODataResponseMessage"/>.
        /// </summary>
        /// <param name="responseMessage">The response message to get and set the "Preference-Applied" header.</param>
        internal ODataPreferenceHeader(IODataResponseMessage responseMessage)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(responseMessage != null, "responseMessage != null");
            this.message = new ODataResponseMessage(responseMessage, /*writing*/ true, /*disableMessageStreamDisposal*/ false, /*maxMessageSize*/ -1);
            this.preferenceHeaderName = PreferenceAppliedHeaderName;
        }

        /// <summary>
        /// Property to get and set the "return-content" and "return-no-content" preferences to the "Prefer" header on the underlying IODataRequestMessage or
        /// the "Preference-Applied" header on the underlying IODataResponseMessage.
        /// Setting true sets the "return-content" preference and clears the "return-no-content" preference.
        /// Setting false sets the "return-no-content" preference and clears the "return-content" preference.
        /// Setting null clears the "return-content" and "return-no-content" preferences.
        /// Returns true if the "return-content" preference is on the header. Otherwise returns false if the "return-no-content" is on the header.
        /// Returning null indicates that "return-content" and "return-no-content" are not on the header.
        /// </summary>
        public bool? ReturnContent
        {
            get
            {
                if (this.PreferenceExists(ReturnContentPreferenceToken))
                {
                    return true;
                }
                
                if (this.PreferenceExists(ReturnNoContentPreferenceToken))
                {
                    return false;
                }

                return null;
            }

            set
            {
                // if the value is null, both "return-content" and "return-no-content" are cleared.   
                this.Clear(ReturnContentPreferenceToken);
                this.Clear(ReturnNoContentPreferenceToken);

                if (value == true)
                {
                    this.Set(ReturnContentPreference);
                }

                if (value == false)
                {
                    this.Set(ReturnNoContentPreference);
                }
            }
        }

        /// <summary>
        /// Property to get and set the "odata.include-annotations" preference with the given filter to the "Prefer" header on the underlying IODataRequestMessage or
        /// the "Preference-Applied" header on the underlying IODataResponseMessage.
        /// If the "odata-annotations" preference is already on the header, set replaces the existing instance.
        /// Returning null indicates that the "odata.include-annotations" preference is not on the header.
        /// 
        /// The filter string may be a comma delimited list of any of the following supported patterns:
        ///   "*"        -- Matches all annotation names.
        ///   "ns.*"     -- Matches all annotation names under the namespace "ns".
        ///   "ns.name"  -- Matches only the annotation name "ns.name".
        ///   "-"        -- The exclude operator may be used with any of the supported pattern, for example:
        ///                 "-ns.*"    -- Excludes all annotation names under the namespace "ns".
        ///                 "-ns.name" -- Excludes only the annotation name "ns.name".
        /// Null or empty filter is equivalent to "-*".
        /// 
        /// The relative priority of the pattern is base on the relative specificity of the patterns being compared. If pattern1 is under the namespace pattern2,
        /// pattern1 is more specific than pattern2 because pattern1 matches a subset of what pattern2 matches. We give higher priority to the pattern that is more specific.
        /// For example:
        ///  "ns.*" has higher priority than "*"
        ///  "ns.name" has higher priority than "ns.*"
        ///  "ns1.name" has same priority as "ns2.*"
        /// 
        /// Patterns with the exclude operator takes higher precedence than the same pattern without.
        /// For example: "-ns.name" has higher priority than "ns.name".
        /// 
        /// Examples:
        ///   "ns1.*,ns.name"       -- Matches any annotation name under the "ns1" namespace and the "ns.name" annotation.
        ///   "*,-ns.*,ns.name"     -- Matches any annotation name outside of the "ns" namespace and only "ns.name" under the "ns" namespace.
        /// </summary>
        public string AnnotationFilter
        {
            get
            {
                var odataAnnotations = this.Get(ODataAnnotationPreferenceToken);

                if (odataAnnotations != null)
                {
                    return odataAnnotations.Value.Trim('"');
                }

                return null;
            }

            set
            {
                ExceptionUtils.CheckArgumentStringNotEmpty(value, "AnnotationFilter");

                if (value == null)
                {
                    this.Clear(ODataAnnotationPreferenceToken);
                }
                else
                {
                    this.Set(new HttpHeaderValueElement(ODataAnnotationPreferenceToken, AddQuotes(value), EmptyParameters));                    
                }
            }
        }

        /// <summary>
        /// Dictionary of preferences in the header.
        /// </summary>
        private HttpHeaderValue Preferences
        {
            get { return this.preferences ?? (this.preferences = this.ParsePreferences()); }
        }

        /// <summary>
        /// Adds quotes around the given text value.
        /// </summary>
        /// <param name="text">text to quote.</param>
        /// <returns>Returns the quoted text.</returns>
        private static string AddQuotes(string text)
        {
            return "\"" + text + "\"";
        }

        /// <summary>
        /// Returns true if the given preference exists in the header, false otherwise.
        /// </summary>
        /// <param name="preference">Preference in question.</param>
        /// <returns>Returns true if the given preference exists in the header, false otherwise.</returns>
        private bool PreferenceExists(string preference)
        {
            return this.Get(preference) != null;
        }

        /// <summary>
        /// Clears the <paramref name="preference"/> from the "Prefer" header on the underlying IODataRequestMessage or
        /// the "Preference-Applied" header on the underlying IODataResponseMessage.
        /// </summary>
        /// <param name="preference">The preference to clear.</param>
        private void Clear(string preference)
        {
            Debug.Assert(!string.IsNullOrEmpty(preference), "!string.IsNullOrEmpty(preference)");
            if (this.Preferences.Remove(preference))
            {
                this.SetPreferencesToMessageHeader();
            }
        }

        /// <summary>
        /// Sets the <paramref name="preference"/> to the "Prefer" header on the underlying IODataRequestMessage or
        /// the "Preference-Applied" header on the underlying IODataResponseMessage.
        /// </summary>
        /// <param name="preference">The preference to set.</param>
        /// <remarks>
        /// If <paramref name="preference"/> is already on the header, this method does a replace rather than adding another instance of the same preference.
        /// </remarks>
        private void Set(HttpHeaderValueElement preference)
        {
            Debug.Assert(preference != null, "preference != null");
            this.Preferences[preference.Name] = preference;
            this.SetPreferencesToMessageHeader();
        }

        /// <summary>
        /// Gets the <paramref name="preferenceName"/> from the "Prefer" header from the underlying <see cref="IODataRequestMessage"/> or
        /// the "Preference-Applied" header from the underlying <see cref="IODataResponseMessage"/>.
        /// </summary>
        /// <param name="preferenceName">The preference to get.</param>
        /// <returns>Returns a key value pair of the <paramref name="preferenceName"/> and its value. The Value property of the key value pair may be null since not
        /// all preferences have value. If the <paramref name="preferenceName"/> is missing from the header, null is returned.</returns>
        private HttpHeaderValueElement Get(string preferenceName)
        {
            Debug.Assert(!string.IsNullOrEmpty(preferenceName), "!string.IsNullOrEmpty(preferenceName)");
            HttpHeaderValueElement value;
            if (!this.Preferences.TryGetValue(preferenceName, out value))
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// Parses the current preference values to a dictionary of preference and value pairs.
        /// </summary>
        /// <returns>Returns a dictionary of preference and value pairs; null if the preference header has not been set.</returns>
        private HttpHeaderValue ParsePreferences()
        {
            string preferenceHeaderValue = this.message.GetHeader(this.preferenceHeaderName);
            HttpHeaderValueLexer preferenceHeaderLexer = HttpHeaderValueLexer.Create(this.preferenceHeaderName, preferenceHeaderValue);
            return preferenceHeaderLexer.ToHttpHeaderValue();
        }

        /// <summary>
        /// Sets the "Prefer" or the "Preference-Applied" header to the underlying message.
        /// </summary>
        private void SetPreferencesToMessageHeader()
        {
            Debug.Assert(this.preferences != null, "this.preferences != null");
            this.message.SetHeader(this.preferenceHeaderName, this.Preferences.ToString());
        }
    }
}

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
    #endregion Namespaces

    /// <summary>
    /// Helper methods related to the ATOM Format
    /// </summary>
    internal static class AtomUtils
    {
        /// <summary>The length of the media type for ATOM payloads (application/atom+xml).</summary>
        private const int MimeApplicationAtomXmlLength = 20;

        /// <summary>The length of the media type for ATOM payloads when terminated by a ';' (application/atom+xml;).</summary>
        private const int MimeApplicationAtomXmlLengthWithSemicolon = 21;

        /// <summary>The length of the media type for links referencing a single entry (application/atom+xml;type=entry).</summary>
        private const int MimeApplicationAtomXmlTypeEntryLength = 31;

        /// <summary>The length of the media type for links referencing a collection of entries (application/atom+xml;type=feed).</summary>
        private const int MimeApplicationAtomXmlTypeFeedLength = 30;

        /// <summary>Parameter string for the media type for links referencing a single entry.</summary>
        private const string MimeApplicationAtomXmlTypeEntryParameter = ";" + MimeConstants.MimeTypeParameterName + "=" + MimeConstants.MimeTypeParameterValueEntry;

        /// <summary>Parameter string for the media type for links referencing a collection of entries.</summary>
        private const string MimeApplicationAtomXmlTypeFeedParameter = ";" + MimeConstants.MimeTypeParameterName + "=" + MimeConstants.MimeTypeParameterValueFeed;

        /// <summary>
        /// Creates the value for the navigation property's link relation attribute.
        /// </summary>
        /// <param name="navigationLink">The link representing the navigation property for which the relation value is created.</param>
        /// <returns>The relation attribute value for the navigation property's link relation.</returns>
        internal static string ComputeODataNavigationLinkRelation(ODataNavigationLink navigationLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(navigationLink != null, "navigationLink != null");
            Debug.Assert(navigationLink.Name != null, "navigationLink.Name != null");

            return string.Join("/", new string[] { AtomConstants.ODataNamespace, AtomConstants.ODataNavigationPropertiesRelatedSegmentName, navigationLink.Name });
        }

        /// <summary>
        /// Creates the value for the navigation property's type attribute.
        /// </summary>
        /// <param name="navigationLink">The link representing the navigation property for which the type value is created.</param>
        /// <returns>The type attribute value for the navigation property.</returns>
        internal static string ComputeODataNavigationLinkType(ODataNavigationLink navigationLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(navigationLink != null, "navigationLink != null");
            Debug.Assert(navigationLink.IsCollection.HasValue, "navigationLink.IsCollection.HasValue");

            // "application/atom+xml;type=entry" or type="application/atom+xml;type=feed"
            return navigationLink.IsCollection.Value ? MimeConstants.MimeApplicationAtomXmlTypeFeed : MimeConstants.MimeApplicationAtomXmlTypeEntry;
        }

        /// <summary>
        /// Creates the value for the navigation property's association link relation attribute.
        /// </summary>
        /// <param name="associationLink">The link representing the navigation property's association for which the relation value is created.</param>
        /// <returns>The relation attribute value for the navigation property's association link relation.</returns>
        internal static string ComputeODataAssociationLinkRelation(ODataAssociationLink associationLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(associationLink != null, "link != null");
            Debug.Assert(associationLink.Name != null, "link.Name != null");

            return string.Join("/", new string[] { AtomConstants.ODataNamespace, AtomConstants.ODataNavigationPropertiesAssociationRelatedSegmentName, associationLink.Name });
        }

        /// <summary>
        /// Creates the value for the stream property's link relation attribute.
        /// </summary>
        /// <param name="streamProperty">The stream property to create the relation for.</param>
        /// <param name="forEditLink">'true' if the relation is computed for an edit link; otherwise 'false'.</param>
        /// <returns>The relation attribute value for the stream property's link relation.</returns>
        internal static string ComputeStreamPropertyRelation(ODataProperty streamProperty, bool forEditLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(streamProperty != null, "streamProperty != null");
            Debug.Assert(!string.IsNullOrEmpty(streamProperty.Name), "!string.IsNullOrEmpty(streamProperty.Name)");

            string segmentName = forEditLink ? AtomConstants.ODataStreamPropertyEditMediaSegmentName : AtomConstants.ODataStreamPropertyMediaResourceSegmentName;
            return string.Join("/", new string[] { AtomConstants.ODataNamespace, segmentName, streamProperty.Name });
        }

        /// <summary>
        /// Unescape the <paramref name="relation"/> attribute value for ATOM link element.
        /// </summary>
        /// <param name="relation">ATOM link relation attribute value.</param>
        /// <returns>
        /// The unescaped relation attribute string if it's a valid URI.
        /// null if relation attribute is not a valid URI.
        /// </returns>
        internal static string UnescapeAtomLinkRelationAttribute(string relation)
        {
            DebugUtils.CheckNoExternalCallers();

            if (!string.IsNullOrEmpty(relation))
            {
                Uri uri;
                if (Uri.TryCreate(relation, UriKind.RelativeOrAbsolute, out uri) && uri.IsAbsoluteUri)
                {
                    return uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
                }
            }

            return null;
        }

        /// <summary>
        /// Return name following the specified <paramref name="namespacePrefix"/> in the <paramref name="relation"/>.
        /// </summary>
        /// <param name="relation">ATOM link relation attribute value, unescaped parsed URI string.</param>
        /// <param name="namespacePrefix">Value which the rel attribute should start with.</param>
        /// <returns>
        /// The name if the <paramref name="relation"/> starts with the given <paramref name="namespacePrefix"/>.
        /// If the <paramref name="relation"/> value does not start with the <paramref name="namespacePrefix"/> a null value is returned.
        /// </returns>
        internal static string GetNameFromAtomLinkRelationAttribute(string relation, string namespacePrefix)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(
                relation == null || relation == UnescapeAtomLinkRelationAttribute(relation),
                "The relation attribute was not unescaped, it is necessary to first call the UnescapeAtomLinkRelationAttribute method.");
            Debug.Assert(namespacePrefix != null, "namespacePrefix != null");

            if (relation != null && relation.StartsWith(namespacePrefix, StringComparison.Ordinal))
            {
                return relation.Substring(namespacePrefix.Length);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the type of a navigation link has one of the expected standard values.
        /// </summary>
        /// <param name="navigationLinkType">The navigation link type to check.</param>
        /// <param name="hasEntryType">true if the navigation link type has a 'type' parameter with the value 'entry'; otherwise false.</param>
        /// <param name="hasFeedType">true if the navigation link type has a 'type' parameter with the value 'feed'; otherwise false.</param>
        /// <returns>true if the navigation link type is the expected application/atom+xml; otherwise false.</returns>
        internal static bool IsExactNavigationLinkTypeMatch(string navigationLinkType, out bool hasEntryType, out bool hasFeedType)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(!string.IsNullOrEmpty(navigationLinkType), "!string.IsNullOrEmpty(navigationLinkType)");

            hasEntryType = false;
            hasFeedType = false;

            // NOTE: using ordinal comparison since this is the fast path and ordinal comparison is faster than ignore-case comparison
            if (!navigationLinkType.StartsWith(MimeConstants.MimeApplicationAtomXml, StringComparison.Ordinal))
            {
                return false;
            }

            int typeLength = navigationLinkType.Length;
            switch (typeLength)
            {
                case MimeApplicationAtomXmlLength:
                    return true;

                case MimeApplicationAtomXmlLengthWithSemicolon:
                    // If there is a trailing ';' we also accept it
                    return navigationLinkType[typeLength - 1] == ';';

                case MimeApplicationAtomXmlTypeEntryLength:
                    hasEntryType = string.Compare(
                        MimeApplicationAtomXmlTypeEntryParameter, 
                        0, 
                        navigationLinkType, 
                        MimeApplicationAtomXmlLength, 
                        MimeApplicationAtomXmlTypeEntryParameter.Length, 
                        StringComparison.Ordinal) == 0;
                    return hasEntryType;

                case MimeApplicationAtomXmlTypeFeedLength:
                    hasFeedType = string.Compare(
                        MimeApplicationAtomXmlTypeFeedParameter, 
                        0, 
                        navigationLinkType, 
                        MimeApplicationAtomXmlLength, 
                        MimeApplicationAtomXmlTypeFeedParameter.Length, 
                        StringComparison.Ordinal) == 0;
                    return hasFeedType;

                default:
                    return false;
            }
        }
    }
}

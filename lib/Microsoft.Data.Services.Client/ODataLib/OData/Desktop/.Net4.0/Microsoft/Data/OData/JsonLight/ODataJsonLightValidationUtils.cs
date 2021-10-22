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

namespace Microsoft.Data.OData.JsonLight
{
    #region Namespaces
    using System;
    using System.Diagnostics;
    #endregion Namespaces

    /// <summary>
    /// Helper methods used by the OData reader for the JsonLight format.
    /// </summary>
    internal static class ODataJsonLightValidationUtils
    {
        /// <summary>
        /// Validates that a string is either a valid absolute URI, or (if it begins with '#') it is a valid URI fragment.
        /// </summary>
        /// <param name="metadataDocumentUri">The metadata document uri.</param>
        /// <param name="propertyName">The property name to validate.</param>
        internal static void ValidateMetadataReferencePropertyName(Uri metadataDocumentUri, string propertyName)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(metadataDocumentUri != null, "metadataDocumentUri != null");
            Debug.Assert(metadataDocumentUri.IsAbsoluteUri, "metadataDocumentUri.IsAbsoluteUri");
            Debug.Assert(!String.IsNullOrEmpty(propertyName), "!string.IsNullOrEmpty(propertyName)");

            string uriStringToValidate = propertyName;

            // If it starts with a '#', validate that the rest of the string is a valid Uri fragment.
            if (propertyName[0] == JsonLightConstants.MetadataUriFragmentIndicator)
            {
                // In order to use System.Uri to validate a fragement, we first prepend the metadataDocumentUri
                // so that it becomes an absolute URI which we can validate with Uri.IsWellFormedUriString.
                uriStringToValidate = UriUtilsCommon.UriToString(metadataDocumentUri) + UriUtils.EnsureEscapedFragment(propertyName);
            }

            if (!Uri.IsWellFormedUriString(uriStringToValidate, UriKind.Absolute) ||
                !ODataJsonLightUtils.IsMetadataReferenceProperty(propertyName) ||
                propertyName[propertyName.Length - 1] == JsonLightConstants.MetadataUriFragmentIndicator)
            {
                throw new ODataException(Strings.ValidationUtils_InvalidMetadataReferenceProperty(propertyName));
            }

            if (IsOpenMetadataReferencePropertyName(metadataDocumentUri, propertyName))
            {
                throw new ODataException(Strings.ODataJsonLightValidationUtils_OpenMetadataReferencePropertyNotSupported(propertyName, UriUtilsCommon.UriToString(metadataDocumentUri)));
            }
        }

        /// <summary>
        /// Validates an operation is valid.
        /// </summary>
        /// <param name="metadataDocumentUri">The metadata document uri.</param>
        /// <param name="operation">The operation to validate.</param>
        internal static void ValidateOperation(Uri metadataDocumentUri, ODataOperation operation)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(operation != null, "operation != null");

            ValidationUtils.ValidateOperationMetadataNotNull(operation);
            string name = UriUtilsCommon.UriToString(operation.Metadata);

            if (metadataDocumentUri != null)
            {
                Debug.Assert(metadataDocumentUri.IsAbsoluteUri, "metadataDocumentUri.IsAbsoluteUri");
                ValidateMetadataReferencePropertyName(metadataDocumentUri, name);
                Debug.Assert(!IsOpenMetadataReferencePropertyName(metadataDocumentUri, name), "!IsOpenMetadataReferencePropertyName(metadataDocumentUri, name)");
            }
        }

        /// <summary>
        /// Determines if the specified property name is a name of an open metadata reference property.
        /// </summary>
        /// <param name="metadataDocumentUri">The metadata document uri.</param>
        /// <param name="propertyName">The property name in question.</param>
        /// <returns>true if the specified property name is a name of an open metadata reference property; false otherwise.</returns>
        internal static bool IsOpenMetadataReferencePropertyName(Uri metadataDocumentUri, string propertyName)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(metadataDocumentUri != null, "metadataDocumentUri != null");
            Debug.Assert(metadataDocumentUri.IsAbsoluteUri, "metadataDocumentUri.IsAbsoluteUri");
            Debug.Assert(!String.IsNullOrEmpty(propertyName), "!string.IsNullOrEmpty(propertyName)");

            // If a metadata reference property isn't based off of the known metadata document URI (for example, it points to a model on another server), 
            // then it must be open. It is based off the known metadata document URI if it either is a fragment (i.e., starts with a hash) or starts with the known absolute URI.
            return ODataJsonLightUtils.IsMetadataReferenceProperty(propertyName)
                && propertyName[0] != JsonLightConstants.MetadataUriFragmentIndicator
                && !propertyName.StartsWith(UriUtilsCommon.UriToString(metadataDocumentUri), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates that the property in an operation (an action or a function) is valid.
        /// </summary>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="propertyName">The name of the property (used for error reporting).</param>
        /// <param name="metadata">The metadata value for the operation (used for error reporting).</param>
        internal static void ValidateOperationPropertyValueIsNotNull(object propertyValue, string propertyName, string metadata)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(!String.IsNullOrEmpty(metadata), "!string.IsNullOrEmpty(metadata)");

            if (propertyValue == null)
            {
                throw new ODataException(OData.Strings.ODataJsonLightValidationUtils_OperationPropertyCannotBeNull(propertyName, metadata));
            }
        }
    }
}

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
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Class with utility methods for validating OData content when writing.
    /// </summary>
    internal static class WriterValidationUtils
    {
        /// <summary>
        /// Validates that message writer settings are correct.
        /// </summary>
        /// <param name="messageWriterSettings">The message writer settings to validate.</param>
        /// <param name="writingResponse">True if we are writing a response.</param>
        internal static void ValidateMessageWriterSettings(ODataMessageWriterSettings messageWriterSettings, bool writingResponse)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(messageWriterSettings != null, "messageWriterSettings != null");

            if (messageWriterSettings.BaseUri != null && !messageWriterSettings.BaseUri.IsAbsoluteUri)
            {
                throw new ODataException(Strings.WriterValidationUtils_MessageWriterSettingsBaseUriMustBeNullOrAbsolute(UriUtilsCommon.UriToString(messageWriterSettings.BaseUri)));
            }

            if (messageWriterSettings.HasJsonPaddingFunction() && !writingResponse)
            {
                throw new ODataException(Strings.WriterValidationUtils_MessageWriterSettingsJsonPaddingOnRequestMessage);
            }
        }

        /// <summary>
        /// Validates an <see cref="ODataProperty"/> for not being null.
        /// </summary>
        /// <param name="property">The property to validate for not being null.</param>
        internal static void ValidatePropertyNotNull(ODataProperty property)
        {
            DebugUtils.CheckNoExternalCallers();

            if (property == null)
            {
                throw new ODataException(Strings.WriterValidationUtils_PropertyMustNotBeNull);
            }
        }

        /// <summary>
        /// Validates a property name to ensure all required information is specified.
        /// </summary>
        /// <param name="propertyName">The property name to validate..</param>
        internal static void ValidatePropertyName(string propertyName)
        {
            DebugUtils.CheckNoExternalCallers();

            // Properties must have a non-empty name
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ODataException(Strings.WriterValidationUtils_PropertiesMustHaveNonEmptyName);
            }

            ValidationUtils.ValidatePropertyName(propertyName);
        }

        /// <summary>
        /// Validates that a property with the specified name exists on a given structured type.
        /// The structured type can be null if no metadata is available.
        /// </summary>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="owningStructuredType">The owning type of the property with name <paramref name="propertyName"/>
        /// or null if no metadata is available.</param>
        /// <param name="undeclaredPropertyBehaviorKinds">Value of UndeclaredPropertyBehaviorKinds in message settings.</param>
        /// <returns>The <see cref="IEdmProperty"/> instance representing the property with name <paramref name="propertyName"/> 
        /// or null if no metadata is available.</returns>
        internal static IEdmProperty ValidatePropertyDefined(string propertyName, IEdmStructuredType owningStructuredType, ODataUndeclaredPropertyBehaviorKinds undeclaredPropertyBehaviorKinds)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "!string.IsNullOrEmpty(propertyName)");

            if (owningStructuredType == null)
            {
                return null;
            }

            // .None / .LinkProperty
            bool throwErr = !undeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty)
                && !undeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.SupportUndeclaredValueProperty);

            IEdmProperty property = owningStructuredType.FindProperty(propertyName);

            // verify that the property is declared if the type is not an open type.
            if (throwErr && !owningStructuredType.IsOpen && property == null)
            {
                throw new ODataException(Strings.ValidationUtils_PropertyDoesNotExistOnType(propertyName, owningStructuredType.ODataFullName()));
            }

            return property;
        }

        /// <summary>
        /// Validates that a navigation property with the specified name exists on a given entity type.
        /// The entity type can be null if no metadata is available.
        /// </summary>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="owningEntityType">The owning entity type or null if no metadata is available.</param>
        /// <param name="undeclaredPropertyBehaviorKinds">Value of UndeclaredPropertyBehaviorKinds in message settings.</param>
        /// <returns>The <see cref="IEdmProperty"/> instance representing the navigation property with name <paramref name="propertyName"/>
        /// or null if no metadata is available.</returns>
        internal static IEdmNavigationProperty ValidateNavigationPropertyDefined(string propertyName, IEdmEntityType owningEntityType, ODataUndeclaredPropertyBehaviorKinds undeclaredPropertyBehaviorKinds)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "!string.IsNullOrEmpty(propertyName)");

            if (owningEntityType == null)
            {
                return null;
            }

            IEdmProperty property = ValidatePropertyDefined(propertyName, owningEntityType, undeclaredPropertyBehaviorKinds);
            if (property == null)
            {
                // .None / .LinkProperty
                bool throwErr = !undeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty)
                    && !undeclaredPropertyBehaviorKinds.HasFlag(ODataUndeclaredPropertyBehaviorKinds.SupportUndeclaredValueProperty);

                // We don't support open navigation properties
                // when writing undeclared property is supported, shouldn't throw exception
                if (throwErr)
                {
                    Debug.Assert(owningEntityType.IsOpen, "We should have already failed on non-existing property on a closed type.");
                }

                throw new ODataException(Strings.ValidationUtils_OpenNavigationProperty(propertyName, owningEntityType.ODataFullName()));
            }

            if (property.PropertyKind != EdmPropertyKind.Navigation)
            {
                // The property must be a navigation property
                throw new ODataException(Strings.ValidationUtils_NavigationPropertyExpected(propertyName, owningEntityType.ODataFullName(), property.PropertyKind.ToString()));
            }

            return (IEdmNavigationProperty)property;
        }

        /// <summary>
        /// Validates an entry in an expanded link to make sure the entity types match.
        /// </summary>
        /// <param name="entryEntityType">The <see cref="IEdmEntityType"/> of the entry.</param>
        /// <param name="parentNavigationPropertyType">The type of the parent navigation property.</param>
        internal static void ValidateEntryInExpandedLink(IEdmEntityType entryEntityType, IEdmEntityType parentNavigationPropertyType)
        {
            DebugUtils.CheckNoExternalCallers();

            if (parentNavigationPropertyType == null)
            {
                return;
            }

            Debug.Assert(entryEntityType != null, "If we have a parent navigation property type we should also have an entry type.");

            // Make sure the entity types are compatible
            if (!parentNavigationPropertyType.IsAssignableFrom(entryEntityType))
            {
                throw new ODataException(Strings.WriterValidationUtils_EntryTypeInExpandedLinkNotCompatibleWithNavigationPropertyType(entryEntityType.ODataFullName(), parentNavigationPropertyType.ODataFullName()));
            }
        }

        /// <summary>
        /// Validates an <see cref="ODataAssociationLink"/> to ensure all required information is specified and valid.
        /// </summary>
        /// <param name="associationLink">The association link to validate.</param>
        /// <param name="version">The version of the OData protocol used for checking.</param>
        /// <param name="writingResponse">true if we are writing a response; otherwise false.</param>
        internal static void ValidateAssociationLink(ODataAssociationLink associationLink, ODataVersion version, bool writingResponse)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(associationLink != null, "associationLink != null");

            ODataVersionChecker.CheckAssociationLinks(version);

            if (!writingResponse)
            {
                throw new ODataException(Strings.WriterValidationUtils_AssociationLinkInRequest(associationLink.Name));
            }

            ValidationUtils.ValidateAssociationLink(associationLink);
        }

        /// <summary>
        /// Validates that an <see cref="ODataOperation"/> can be written.
        /// </summary>
        /// <param name="operation">The operation (an action or a function) to validate.</param>
        /// <param name="writingResponse">true if writing a response; otherwise false.</param>
        internal static void ValidateCanWriteOperation(ODataOperation operation, bool writingResponse)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(operation != null, "operation != null");

            // Operations are only valid in responses; we fail on them in requests
            if (!writingResponse)
            {
                throw new ODataException(Strings.WriterValidationUtils_OperationInRequest(operation.Metadata));
            }
        }

        /// <summary>
        /// Validates an <see cref="ODataFeed"/> to ensure all required information is specified and valid on the WriteEnd call.
        /// </summary>
        /// <param name="feed">The feed to validate.</param>
        /// <param name="writingRequest">Flag indicating whether the feed is written as part of a request or a response.</param>
        /// <param name="version">The version of the OData protocol used for checking.</param>
        internal static void ValidateFeedAtEnd(ODataFeed feed, bool writingRequest, ODataVersion version)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(feed != null, "feed != null");

            // Verify next link
            if (feed.NextPageLink != null)
            {
                // Check that NextPageLink is not set for requests
                if (writingRequest)
                {
                    throw new ODataException(Strings.WriterValidationUtils_NextPageLinkInRequest);
                }

                // Verify version requirements
                ODataVersionChecker.CheckNextLink(version);
            }

            // Verify Delta link, writingRequest is not verfied in order to be consistent with the behavior in Json.
            if (feed.DeltaLink != null)
            {
                // Verify version requirements
                ODataVersionChecker.CheckDeltaLink(version);
            }
        }

        /// <summary>
        /// Validates an <see cref="ODataEntry"/> to ensure all required information is specified and valid on WriteStart call.
        /// </summary>
        /// <param name="entry">The entry to validate.</param>
        internal static void ValidateEntryAtStart(ODataEntry entry)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entry != null, "entry != null");

            // Id can be specified at the beginning (and might be written here), so we need to validate it here.
            ValidateEntryId(entry.Id);

            // Type name is verified in the format readers/writers since it's shared with other non-entity types
            // and verifying it here would mean doing it twice.
        }

        /// <summary>
        /// Validates an <see cref="ODataEntry"/> to ensure all required information is specified and valid on WriteEnd call.
        /// </summary>
        /// <param name="entry">The entry to validate.</param>
        internal static void ValidateEntryAtEnd(ODataEntry entry)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entry != null, "entry != null");

            // If the Id was not specified in the beginning it might have been specified at the end, so validate it here as well.
            ValidateEntryId(entry.Id);
        }

        /// <summary>
        /// Validates an <see cref="ODataStreamReferenceValue"/> to ensure all required information is specified and valid.
        /// </summary>
        /// <param name="streamReference">The stream reference to validate.</param>
        /// <param name="isDefaultStream">true if <paramref name="streamReference"/> is the default stream for an entity; false if it is a named stream property value.</param>
        internal static void ValidateStreamReferenceValue(ODataStreamReferenceValue streamReference, bool isDefaultStream)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(streamReference != null, "streamReference != null");

            if (streamReference.ContentType != null && streamReference.ContentType.Length == 0)
            {
                throw new ODataException(Strings.WriterValidationUtils_StreamReferenceValueEmptyContentType);
            }

            if (isDefaultStream && streamReference.ReadLink == null && streamReference.ContentType != null)
            {
                throw new ODataException(Strings.WriterValidationUtils_DefaultStreamWithContentTypeWithoutReadLink);
            }

            if (isDefaultStream && streamReference.ReadLink != null && streamReference.ContentType == null)
            {
                throw new ODataException(Strings.WriterValidationUtils_DefaultStreamWithReadLinkWithoutContentType);
            }

            // Default stream can be completely empty (no links or anything)
            // that is used to effectively mark the entry as MLE without providing any MR information.
            // OData clients when creating new MLE/MR might not have the MR information (yet) when sending the first PUT, but they still
            // need to mark the entry as MLE so that properties are written out-of-content. In such scenario the client can just set an empty
            // default stream to mark the entry as MLE.
            // That will cause the ATOM writer to write the properties outside the content without producing any content element.
            if (streamReference.EditLink == null && streamReference.ReadLink == null && !isDefaultStream)
            {
                throw new ODataException(Strings.WriterValidationUtils_StreamReferenceValueMustHaveEditLinkOrReadLink);
            }

            if (streamReference.EditLink == null && streamReference.ETag != null)
            {
                throw new ODataException(Strings.WriterValidationUtils_StreamReferenceValueMustHaveEditLinkToHaveETag);
            }
        }

        /// <summary>
        /// Validates a named stream property to ensure it's not null and it's name if correct.
        /// </summary>
        /// <param name="streamProperty">The stream reference property to validate.</param>
        /// <param name="edmProperty">Property metadata to validate against.</param>
        /// <param name="version">The version of the OData protocol used for checking.</param>
        /// <param name="writingResponse">true when writing a response; otherwise false.</param>
        /// <remarks>This does NOT validate the value of the stream property, just the property itself.</remarks>
        internal static void ValidateStreamReferenceProperty(ODataProperty streamProperty, IEdmProperty edmProperty, ODataVersion version, bool writingResponse)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(streamProperty != null, "streamProperty != null");

            ODataVersionChecker.CheckStreamReferenceProperty(version);
            ValidationUtils.ValidateStreamReferenceProperty(streamProperty, edmProperty);

            if (!writingResponse)
            {
                throw new ODataException(Strings.WriterValidationUtils_StreamPropertyInRequest(streamProperty.Name));
            }
        }

        /// <summary>
        /// Validates that the specified <paramref name="entityReferenceLink"/> is not null.
        /// </summary>
        /// <param name="entityReferenceLink">The entity reference link to validate.</param>
        /// <remarks>This should be called only for entity reference links inside the ODataEntityReferenceLinks.Links collection.</remarks>
        internal static void ValidateEntityReferenceLinkNotNull(ODataEntityReferenceLink entityReferenceLink)
        {
            DebugUtils.CheckNoExternalCallers();

            if (entityReferenceLink == null)
            {
                throw new ODataException(Strings.WriterValidationUtils_EntityReferenceLinksLinkMustNotBeNull);
            }
        }

        /// <summary>
        /// Validates an entity reference link instance.
        /// </summary>
        /// <param name="entityReferenceLink">The entity reference link to validate.</param>
        internal static void ValidateEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(entityReferenceLink != null, "entityReferenceLink != null");

            if (entityReferenceLink.Url == null)
            {
                throw new ODataException(Strings.WriterValidationUtils_EntityReferenceLinkUrlMustNotBeNull);
            }
        }

        /// <summary>
        /// Validates an <see cref="ODataNavigationLink"/> to ensure all required information is specified and valid.
        /// </summary>
        /// <param name="navigationLink">The navigation link to validate.</param>
        /// <param name="declaringEntityType">The <see cref="IEdmEntityType"/> declaring the navigation property; or null if metadata is not available.</param>
        /// <param name="expandedPayloadKind">The <see cref="ODataPayloadKind"/> of the expanded content of this navigation link or null for deferred links.</param>
        /// <param name="undeclaredPropertyBehaviorKinds">Value of UndeclaredPropertyBehaviorKinds in message settings.</param>
        /// <returns>The type of the navigation property for this navigation link; or null if no <paramref name="declaringEntityType"/> was specified.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Keeping the validation code for navigation link multiplicity in one place.")]
        internal static IEdmNavigationProperty ValidateNavigationLink(
            ODataNavigationLink navigationLink,
            IEdmEntityType declaringEntityType,
            ODataPayloadKind? expandedPayloadKind,
            ODataUndeclaredPropertyBehaviorKinds undeclaredPropertyBehaviorKinds)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(navigationLink != null, "navigationLink != null");
            Debug.Assert(
                !expandedPayloadKind.HasValue ||
                expandedPayloadKind.Value == ODataPayloadKind.EntityReferenceLink ||
                expandedPayloadKind.Value == ODataPayloadKind.Entry ||
                expandedPayloadKind.Value == ODataPayloadKind.Feed,
                "If an expanded payload kind is specified it must be entry, feed or entity reference link.");

            // Navigation link must have a non-empty name
            if (string.IsNullOrEmpty(navigationLink.Name))
            {
                throw new ODataException(Strings.ValidationUtils_LinkMustSpecifyName);
            }

            // If we write an entity reference link, don't validate the multiplicity of the IsCollection
            // property if it is 'false' (since we allow writing a singleton navigation link for
            // a collection navigation property in requests) nor the consistency of payload kind and metadata
            // (which is done separately in ODataWriterCore.CheckForNavigationLinkWithContent).
            bool isEntityReferenceLinkPayload = expandedPayloadKind == ODataPayloadKind.EntityReferenceLink;

            // true only if the expandedPayloadKind has a value and the value is 'Feed'
            bool isFeedPayload = expandedPayloadKind == ODataPayloadKind.Feed;

            // Make sure the IsCollection property agrees with the payload kind for entry and feed payloads
            Func<object, string> errorTemplate = null;
            if (!isEntityReferenceLinkPayload && navigationLink.IsCollection.HasValue && expandedPayloadKind.HasValue)
            {
                // For feed/entry make sure the IsCollection property is set correctly.
                if (isFeedPayload != navigationLink.IsCollection.Value)
                {
                    errorTemplate = expandedPayloadKind.Value == ODataPayloadKind.Feed
                        ? (Func<object, string>)Strings.WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedContent
                        : Strings.WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryContent;
                }
            }

            IEdmNavigationProperty navigationProperty = null;
            if (errorTemplate == null && declaringEntityType != null)
            {
                navigationProperty = WriterValidationUtils.ValidateNavigationPropertyDefined(navigationLink.Name, declaringEntityType, undeclaredPropertyBehaviorKinds);
                Debug.Assert(navigationProperty != null, "If we have a declaring type we expect a non-null navigation property since open nav props are not allowed.");

                bool isCollectionType = navigationProperty.Type.TypeKind() == EdmTypeKind.Collection;

                // Make sure the IsCollection property agrees with the metadata type for entry and feed payloads
                if (navigationLink.IsCollection.HasValue && isCollectionType != navigationLink.IsCollection)
                {
                    // Ignore the case where IsCollection is 'false' and we are writing an entity reference link
                    // (see comment above)
                    if (!(navigationLink.IsCollection == false && isEntityReferenceLinkPayload))
                    {
                        errorTemplate = isCollectionType
                            ? (Func<object, string>)Strings.WriterValidationUtils_ExpandedLinkIsCollectionFalseWithFeedMetadata
                            : Strings.WriterValidationUtils_ExpandedLinkIsCollectionTrueWithEntryMetadata;
                    }
                }

                // Make sure that the payload kind agrees with the metadata.
                // For entity reference links we check separately in ODataWriterCore.CheckForNavigationLinkWithContent.
                if (!isEntityReferenceLinkPayload && expandedPayloadKind.HasValue && isCollectionType != isFeedPayload)
                {
                    errorTemplate = isCollectionType
                        ? (Func<object, string>)Strings.WriterValidationUtils_ExpandedLinkWithEntryPayloadAndFeedMetadata
                        : Strings.WriterValidationUtils_ExpandedLinkWithFeedPayloadAndEntryMetadata;
                }
            }

            if (errorTemplate != null)
            {
                string uri = navigationLink.Url == null ? "null" : UriUtilsCommon.UriToString(navigationLink.Url);
                throw new ODataException(errorTemplate(uri));
            }

            return navigationProperty;
        }

        /// <summary>
        /// Validates that the specified navigation link has a Url.
        /// </summary>
        /// <param name="navigationLink">The navigation link to validate.</param>
        internal static void ValidateNavigationLinkUrlPresent(ODataNavigationLink navigationLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(navigationLink != null, "navigationLink != null");

            // Navigation link must specify the Url
            // NOTE: we currently only require a non-null Url for ATOM payloads and non-expanded navigation links in JSON.
            //       There is no place in JSON to write a Url if the navigation link is expanded.
            if (navigationLink.Url == null)
            {
                throw new ODataException(Strings.WriterValidationUtils_NavigationLinkMustSpecifyUrl(navigationLink.Name));
            }
        }

        /// <summary>
        /// Validates that the sepcified navigation link has cardinality, that is it has the IsCollection value set.
        /// </summary>
        /// <param name="navigationLink">The navigation link to validate.</param>
        internal static void ValidateNavigationLinkHasCardinality(ODataNavigationLink navigationLink)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(navigationLink != null, "navigationLink != null");

            if (!navigationLink.IsCollection.HasValue)
            {
                throw new ODataException(Strings.WriterValidationUtils_NavigationLinkMustSpecifyIsCollection(navigationLink.Name));
            }
        }

        /// <summary>
        /// Validates that the expected property allows null value.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected property type or null if we don't have any.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="writerBehavior">The <see cref="ODataWriterBehavior"/> instance controlling the behavior of the writer.</param>
        /// <param name="model">The model to use to get the data service version.</param>
        internal static void ValidateNullPropertyValue(IEdmTypeReference expectedPropertyTypeReference, string propertyName, ODataWriterBehavior writerBehavior, IEdmModel model)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(writerBehavior != null, "writerBehavior != null");
            Debug.Assert(model != null, "For null validation, model is required.");

            if (expectedPropertyTypeReference != null)
            {
                if (expectedPropertyTypeReference.IsNonEntityCollectionType())
                {
                    throw new ODataException(Strings.WriterValidationUtils_CollectionPropertiesMustNotHaveNullValue(propertyName));
                }

                if (expectedPropertyTypeReference.IsODataPrimitiveTypeKind())
                {
                    // WCF DS allows null values for non-nullable primitive types, so we need to check for a knob which enables this behavior.
                    // See the description of ODataWriterBehavior.AllowNullValuesForNonNullablePrimitiveTypes for more details.
                    if (!expectedPropertyTypeReference.IsNullable && !writerBehavior.AllowNullValuesForNonNullablePrimitiveTypes)
                    {
                        throw new ODataException(Strings.WriterValidationUtils_NonNullablePropertiesMustNotHaveNullValue(propertyName, expectedPropertyTypeReference.ODataFullName()));
                    }
                }
                else if (expectedPropertyTypeReference.IsStream())
                {
                    throw new ODataException(Strings.WriterValidationUtils_StreamPropertiesMustNotHaveNullValue(propertyName));
                }
                else if (expectedPropertyTypeReference.IsODataComplexTypeKind())
                {
                    if (ValidationUtils.ShouldValidateComplexPropertyNullValue(model))
                    {
                        IEdmComplexTypeReference complexTypeReference = expectedPropertyTypeReference.AsComplex();
                        if (!complexTypeReference.IsNullable)
                        {
                            throw new ODataException(Strings.WriterValidationUtils_NonNullablePropertiesMustNotHaveNullValue(propertyName, expectedPropertyTypeReference.ODataFullName()));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the value of the Id property on an entry.
        /// </summary>
        /// <param name="id">The id value for an entry to validate.</param>
        private static void ValidateEntryId(string id)
        {
            // Verify non-empty ID (entries can have no (null) ID for insert scenarios; empty IDs are not allowed)
            if (id != null && id.Length == 0)
            {
                throw new ODataException(Strings.WriterValidationUtils_EntriesMustHaveNonEmptyId);
            }
        }
    }
}

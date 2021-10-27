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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Json;
    using Microsoft.Data.OData.Metadata;
    using ODataErrorStrings = Microsoft.Data.OData.Strings;
    #endregion Namespaces

    /// <summary>
    /// OData JsonLight deserializer for properties and value types.
    /// </summary>
    internal class ODataJsonLightPropertyAndValueDeserializer : ODataJsonLightDeserializer
    {
        /// <summary>A sentinel value indicating a missing property value.</summary>
        private static readonly object missingPropertyValue = new object();

        /// <summary>
        /// The current recursion depth of values read by this deserializer, measured by the number of complex, collection, JSON object and JSON array values read so far.
        /// </summary>
        private int recursionDepth;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLightInputContext">The JsonLight input context to read from.</param>
        internal ODataJsonLightPropertyAndValueDeserializer(ODataJsonLightInputContext jsonLightInputContext)
            : base(jsonLightInputContext)
        {
            DebugUtils.CheckNoExternalCallers();
        }

        /// <summary>
        /// This method creates an reads the property from the input and 
        /// returns an <see cref="ODataProperty"/> representing the read property.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        /// <returns>An <see cref="ODataProperty"/> representing the read property.</returns>
        internal ODataProperty ReadTopLevelProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.JsonReader.NodeType == JsonNodeType.None, "Pre-Condition: expected JsonNodeType.None, the reader must not have been used yet.");
            this.JsonReader.AssertNotBuffering();

            // We use this to store annotations and check for duplicate annotation names, but we don't really store properties in it.
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = this.CreateDuplicatePropertyNamesChecker();

            this.ReadPayloadStart(
                ODataPayloadKind.Property,
                duplicatePropertyNamesChecker,
                /*isReadingNestedPayload*/false,
                /*allowEmptyPayload*/false);

            ODataProperty resultProperty = this.ReadTopLevelPropertyImplementation(expectedPropertyTypeReference, duplicatePropertyNamesChecker);

            this.ReadPayloadEnd(/*isReadingNestedPayload*/ false);

            Debug.Assert(this.JsonReader.NodeType == JsonNodeType.EndOfInput, "Post-Condition: expected JsonNodeType.EndOfInput");
            this.JsonReader.AssertNotBuffering();

            return resultProperty;
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// This method creates an reads the property from the input and 
        /// returns an <see cref="ODataProperty"/> representing the read property.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        /// <returns>A task which returns an <see cref="ODataProperty"/> representing the read property.</returns>
        internal Task<ODataProperty> ReadTopLevelPropertyAsync(IEdmTypeReference expectedPropertyTypeReference)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.JsonReader.NodeType == JsonNodeType.None, "Pre-Condition: expected JsonNodeType.None, the reader must not have been used yet.");
            this.JsonReader.AssertNotBuffering();

            // We use this to store annotations and check for duplicate annotation names, but we don't really store properties in it.
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = this.CreateDuplicatePropertyNamesChecker();

            return this.ReadPayloadStartAsync(
                ODataPayloadKind.Property,
                duplicatePropertyNamesChecker,
                /*isReadingNestedPayload*/false,
                /*allowEmptyPayload*/false)

                .FollowOnSuccessWith(t =>
            {
                ODataProperty resultProperty = this.ReadTopLevelPropertyImplementation(expectedPropertyTypeReference, duplicatePropertyNamesChecker);

                this.ReadPayloadEnd(/*isReadingNestedPayload*/ false);

                Debug.Assert(this.JsonReader.NodeType == JsonNodeType.EndOfInput, "Post-Condition: expected JsonNodeType.EndOfInput");
                this.JsonReader.AssertNotBuffering();

                return resultProperty;
            });
        }
#endif

        /// <summary>
        /// Reads a primitive value, complex value or collection.
        /// </summary>
        /// <param name="payloadTypeName">The type name read from the payload as a property annotation, or null if none is available.</param>
        /// <param name="expectedValueTypeReference">The expected type reference of the property value.</param>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to use - if null the method should create a new one if necessary.</param>
        /// <param name="collectionValidator">The collection validator instance if no expected item type has been specified; otherwise null.</param>
        /// <param name="validateNullValue">true to validate null values; otherwise false.</param>
        /// <param name="isTopLevelPropertyValue">true if we are reading a top-level property value; otherwise false.</param>
        /// <param name="insideComplexValue">true if we are reading a complex value and the reader is already positioned inside the complex value; otherwise false.</param>
        /// <param name="propertyName">The name of the property whose value is being read, if applicable (used for error reporting).</param>
        /// <returns>The value of the property read.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.PrimitiveValue   - the value of the property is a primitive value
        ///                 JsonNodeType.StartObject      - the value of the property is an object
        ///                 JsonNodeType.StartArray       - the value of the property is an array - method will fail in this case.
        /// Post-Condition: almost anything - the node after the property value.
        ///                 
        /// Returns the value of the property read, which can be one of:
        /// - null
        /// - primitive value
        /// - <see cref="ODataComplexValue"/>
        /// - <see cref="ODataCollectionValue"/>
        /// </remarks>
        internal object ReadNonEntityValue(
            string payloadTypeName,
            IEdmTypeReference expectedValueTypeReference,
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker,
            CollectionWithoutExpectedTypeValidator collectionValidator,
            bool validateNullValue,
            bool isTopLevelPropertyValue,
            bool insideComplexValue,
            string propertyName)
        {
            DebugUtils.CheckNoExternalCallers();

            return this.ReadNonEntityValue(
                payloadTypeName,
                expectedValueTypeReference,
                duplicatePropertyNamesChecker,
                collectionValidator,
                validateNullValue,
                isTopLevelPropertyValue,
                insideComplexValue,
                propertyName,
                false);
        }

        /// <summary>
        /// Reads a primitive value, complex value or collection.
        /// </summary>
        /// <param name="payloadTypeName">The type name read from the payload as a property annotation, or null if none is available.</param>
        /// <param name="expectedValueTypeReference">The expected type reference of the property value.</param>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to use - if null the method should create a new one if necessary.</param>
        /// <param name="collectionValidator">The collection validator instance if no expected item type has been specified; otherwise null.</param>
        /// <param name="validateNullValue">true to validate null values; otherwise false.</param>
        /// <param name="isTopLevelPropertyValue">true if we are reading a top-level property value; otherwise false.</param>
        /// <param name="insideComplexValue">true if we are reading a complex value and the reader is already positioned inside the complex value; otherwise false.</param>
        /// <param name="propertyName">The name of the property whose value is being read, if applicable (used for error reporting).</param>
        /// <param name="readRawValueEvenIfNoTypeFound">If true: when no type info, read raw value as primitive (not including spatial type), untyped complex or untype collection.</param>
        /// <returns>The value of the property read.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.PrimitiveValue   - the value of the property is a primitive value
        ///                 JsonNodeType.StartObject      - the value of the property is an object
        ///                 JsonNodeType.StartArray       - the value of the property is an array - method will fail in this case.
        /// Post-Condition: almost anything - the node after the property value.
        ///                 
        /// Returns the value of the property read, which can be one of:
        /// - null
        /// - primitive value
        /// - <see cref="ODataComplexValue"/>
        /// - <see cref="ODataCollectionValue"/>
        /// </remarks>
        internal object ReadNonEntityValue(
            string payloadTypeName,
            IEdmTypeReference expectedValueTypeReference,
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker,
            CollectionWithoutExpectedTypeValidator collectionValidator,
            bool validateNullValue,
            bool isTopLevelPropertyValue,
            bool insideComplexValue,
            string propertyName,
            bool readRawValueEvenIfNoTypeFound)
        {
            DebugUtils.CheckNoExternalCallers();

            this.AssertRecursionDepthIsZero();
            object nonEntityValue = this.ReadNonEntityValueImplementation(
                payloadTypeName,
                expectedValueTypeReference,
                duplicatePropertyNamesChecker,
                collectionValidator,
                validateNullValue,
                isTopLevelPropertyValue,
                insideComplexValue,
                propertyName,
                readRawValueEvenIfNoTypeFound);
            this.AssertRecursionDepthIsZero();

            return nonEntityValue;
        }

        /// <summary>
        /// Gets and validates the type name annotation for the specified property.
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker in use for the entry content.</param>
        /// <param name="propertyName">The name of the property to get the type name for.</param>
        /// <returns>The type name for the property or null if no type name was found.</returns>
        internal static string ValidateDataPropertyTypeNameAnnotation(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, string propertyName)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(duplicatePropertyNamesChecker != null, "duplicatePropertyNamesChecker != null");
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "!string.IsNullOrEmpty(propertyName)");

            Dictionary<string, object> propertyAnnotations = duplicatePropertyNamesChecker.GetODataPropertyAnnotations(propertyName);
            string propertyTypeName = null;
            if (propertyAnnotations != null)
            {
                foreach (KeyValuePair<string, object> propertyAnnotation in propertyAnnotations)
                {
                    if (string.CompareOrdinal(propertyAnnotation.Key, ODataAnnotationNames.ODataType) != 0)
                    {
                        throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedDataPropertyAnnotation(propertyName, propertyAnnotation.Key));
                    }

                    Debug.Assert(propertyAnnotation.Value is string && propertyAnnotation.Value != null, "The odata.type annotation should have been parsed as a non-null string.");
                    propertyTypeName = (string)propertyAnnotation.Value;
                }
            }

            return propertyTypeName;
        }

        /// <summary>
        /// Tries to read an annotation as OData type name annotation.
        /// </summary>
        /// <param name="annotationName">The annotation name on which value the reader is positioned on.</param>
        /// <param name="value">The read value of the annotation (string).</param>
        /// <returns>true if the annotation is an OData type name annotation, false otherwise.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.PrimitiveValue - the value of the annotation
        ///                 JsonNodeType.StartObject
        ///                 JsonNodeType.StartArray
        /// Post-Condition: JsonNodeType.Property       - the next property after the annotation
        ///                 JsonNodeType.EndObject      - end of the parent object
        ///                 JsonNodeType.PrimitiveValue - the reader didn't move
        ///                 JsonNodeType.StartObject
        ///                 JsonNodeType.StartArray
        ///                 
        /// If the method returns true, it consumed the value of the annotation from the reader.
        /// If it returns false, it didn't move the reader.
        /// </remarks>
        protected bool TryReadODataTypeAnnotationValue(string annotationName, out string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(annotationName), "!string.IsNullOrEmpty(annotationName)");

            if (string.CompareOrdinal(annotationName, ODataAnnotationNames.ODataType) == 0)
            {
                value = this.ReadODataTypeAnnotationValue();
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Reads the value of the odata.type annotation.
        /// </summary>
        /// <returns>The type name read from the annotation.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.PrimitiveValue - the value of the annotation, will fail if it's not PrimitiveValue
        ///                 JsonNodeType.StartObject
        ///                 JsonNodeType.StartArray
        /// Post-Condition: JsonNodeType.Property    - the next property after the annotation
        ///                 JsonNodeType.EndObject   - end of the parent object
        /// </remarks>
        protected string ReadODataTypeAnnotationValue()
        {
            this.AssertJsonCondition(JsonNodeType.PrimitiveValue, JsonNodeType.StartObject, JsonNodeType.StartArray);

            string typeName = this.JsonReader.ReadStringValue();
            if (typeName == null)
            {
                throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_InvalidTypeName(typeName));
            }

            this.AssertJsonCondition(JsonNodeType.Property, JsonNodeType.EndObject);
            return typeName;
        }

        /// <summary>
        /// Reads top-level property payload property annotation value.
        /// </summary>
        /// <param name="propertyAnnotationName">The name of the property annotation.</param>
        /// <returns>The value of the annotation read.</returns>
        protected object ReadTypePropertyAnnotationValue(string propertyAnnotationName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyAnnotationName), "!string.IsNullOrEmpty(propertyAnnotationName)");
            Debug.Assert(
                propertyAnnotationName.StartsWith(JsonLightConstants.ODataAnnotationNamespacePrefix, StringComparison.Ordinal),
                "The method should only be called with OData. annotations");

            string typeName;
            if (this.TryReadODataTypeAnnotationValue(propertyAnnotationName, out typeName))
            {
                return typeName;
            }

            throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedAnnotationProperties(propertyAnnotationName));
        }

        /// <summary>
        /// Check if a property value type in non-open entity is deterministic .
        /// </summary>
        /// <param name="jsonReaderNodeType">The current JsonReader NodeType.</param>
        /// <param name="jsonReaderValue">The current JsonReader Value</param>
        /// <param name="payloadTypeName">The 'odata.type' annotation in payload.</param>
        /// <param name="payloadTypeReference">The payloadTypeReference of 'odata.type'.</param>
        /// <returns>True if property value type is deterministic.</returns>
        protected static bool IsKnownValueTypeForNonOpenEntityOrComplex(JsonNodeType jsonReaderNodeType, object jsonReaderValue, string payloadTypeName, IEdmTypeReference payloadTypeReference)
        {
            if (string.IsNullOrEmpty(payloadTypeName))
            {
                bool isNullValue = (jsonReaderNodeType == JsonNodeType.PrimitiveValue) && (jsonReaderValue == null);
                bool isBoolValue = (jsonReaderNodeType == JsonNodeType.PrimitiveValue) && (jsonReaderValue is bool);

                // non-open: string or numeric, complex, collection, spatial will be unknown
                return isNullValue || isBoolValue;
            }
            else
            {
                return (payloadTypeReference != null);
            }
        }

        /// <summary>
        /// Check if a property value type in open entity is deterministic .
        /// </summary>
        /// <param name="jsonReaderNodeType">The current JsonReader NodeType.</param>
        /// <param name="jsonReaderValue">The current JsonReader Value</param>
        /// <param name="payloadTypeName">The 'odata.type' annotation in payload.</param>
        /// <param name="payloadTypeReference">The payloadTypeReference of 'odata.type'.</param>
        /// <returns>True if property value type is deterministic.</returns>
        protected static bool IsKnownValueTypeForOpenEntityOrComplex(JsonNodeType jsonReaderNodeType, object jsonReaderValue, string payloadTypeName, IEdmTypeReference payloadTypeReference)
        {
            if (string.IsNullOrEmpty(payloadTypeName))
            {
                // open: bool, null, string or numeric will be *known*
                return (jsonReaderNodeType == JsonNodeType.PrimitiveValue);
            }
            else
            {
                return (payloadTypeReference != null);
            }
        }

        /// <summary>
        /// Try to read or peek the odata.type annotation.
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">The current level's DuplicatePropertyNamesChecker.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="insideComplexValue">If inside complex value.</param>
        /// <returns>The odata.type value or null.</returns>
        protected string TryReadOrPeekPayloadType(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, string propertyName, bool insideComplexValue)
        {
            string payloadTypeName = ValidateDataPropertyTypeNameAnnotation(duplicatePropertyNamesChecker, propertyName);
            bool valueIsJsonObject = this.JsonReader.NodeType == JsonNodeType.StartObject;
            if (string.IsNullOrEmpty(payloadTypeName) && valueIsJsonObject)
            {
                try
                {
                    this.JsonReader.StartBuffering();

                    // If we have an object value initialize the duplicate property names checker
                    duplicatePropertyNamesChecker = this.CreateDuplicatePropertyNamesChecker();

                    // Read the payload type name 
                    string typeName;
                    bool typeNameFoundInPayload = this.TryReadPayloadTypeFromObject(
                        duplicatePropertyNamesChecker,
                        insideComplexValue,
                        out typeName);
                    if (typeNameFoundInPayload)
                    {
                        payloadTypeName = typeName;
                    }
                }
                finally
                {
                    this.JsonReader.StopBuffering();
                }
            }

            return payloadTypeName;
        }

        /// <summary>
        /// Reads a non-open entity or complex type's undeclared property.
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">duplicatePropertyNamesChecker.</param>
        /// <param name="propertyName">Now this name can't be found in model.</param>
        /// <param name="isTopLevelPropertyValue">bool</param>
        /// <returns>The read result.</returns>
        protected object InnerReadNonOpenUndeclaredProperty(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, string propertyName, bool isTopLevelPropertyValue)
        {
            // Now we know the property name is undeclared, but not sure if property value type is known/unknown.
            // For any Property in NON-OPEN complex / entity :
            // Property name,     Property value type,     Deserialized result
            // unknown,     known,     Primitive/ODataValue
            // unknown,     Unknown(string/numeric, no/unrecognized odata.type),     ODataUntypedValue
            // known,     Inconsistent with model,     Throw exception
            bool insideComplexValue = false;
            string outterPayloadTypeName = ValidateDataPropertyTypeNameAnnotation(duplicatePropertyNamesChecker, propertyName);
            string payloadTypeName = this.TryReadOrPeekPayloadType(duplicatePropertyNamesChecker, propertyName, insideComplexValue);
            EdmTypeKind payloadTypeKind;
            IEdmType payloadType = ReaderValidationUtils.ResolvePayloadTypeName(
                this.Model,
                null, // expectedTypeReference
                payloadTypeName,
                EdmTypeKind.Complex,
                this.MessageReaderSettings.ReaderBehavior,
                this.Version,
                out payloadTypeKind);
            IEdmTypeReference payloadTypeReference = null;
            if (!string.IsNullOrEmpty(payloadTypeName) && payloadType != null)
            {
                // only try resolving for known type (the below will throw on unknown type name) :
                SerializationTypeNameAnnotation serializationTypeNameAnnotation;
                EdmTypeKind targetTypeKind;
                payloadTypeReference = ReaderValidationUtils.ResolvePayloadTypeNameAndComputeTargetType(
                    EdmTypeKind.None,
                    /*defaultPrimitivePayloadType*/ null,
                    null, // expectedTypeReference 
                    payloadTypeName,
                    this.Model,
                    this.MessageReaderSettings,
                    this.Version,
                    this.GetNonEntityValueKind,
                    out targetTypeKind,
                    out serializationTypeNameAnnotation);
            }

            object propertyValue = null;
            {
                bool isKnownValueType = IsKnownValueTypeForNonOpenEntityOrComplex(this.JsonReader.NodeType, this.JsonReader.Value, payloadTypeName, payloadTypeReference);
                if (isKnownValueType)
                {
                    bool validateNullValue = true;
                    if (ODataJsonReaderCoreUtils.TryReadNullValue(this.JsonReader, this.JsonLightInputContext, payloadTypeReference, validateNullValue, propertyName))
                    {
                        if (isTopLevelPropertyValue)
                        {
                            // For a top-level property value a special null marker object has to be used to indicate  a null value.
                            // If we find a null value for a property at the top-level, it is an invalid payload
                            throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_TopLevelPropertyWithPrimitiveNullValue(ODataAnnotationNames.ODataNull, JsonLightConstants.ODataNullAnnotationTrueValue));
                        }

                        propertyValue = null;
                    }
                    else
                    {
                        this.JsonReader.AssertNotBuffering();
                        string propertyTypeName = ValidateDataPropertyTypeNameAnnotation(duplicatePropertyNamesChecker, propertyName);
                        propertyValue = this.ReadNonEntityValueImplementation(
                            outterPayloadTypeName,
                            payloadTypeReference,
                            /*duplicatePropertyNamesChecker*/ null,
                            /*collectionValidator*/ null,
                            false, // validateNullValue
                            isTopLevelPropertyValue,
                            insideComplexValue,
                            propertyName);
                    }
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    this.JsonReader.SkipValue(builder);
                    ODataUntypedValue tmp = new ODataUntypedValue()
                    {
                        RawJson = builder.ToString()
                    };
                    propertyValue = tmp;
                }
            }

            this.JsonReader.AssertNotBuffering();
            Debug.Assert(
                this.JsonReader.NodeType == JsonNodeType.Property || this.JsonReader.NodeType == JsonNodeType.EndObject,
                "Post-Condition: expected JsonNodeType.Property or JsonNodeType.EndObject");
            return propertyValue;
        }

        /// <summary>
        /// Adds an ODataJsonLightRawAnnotationSet to the property's value (ODataAnnotatable) if it has raw annotation.
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">The DuplicatePropertyNamesChecker already containing raw annotations.</param>
        /// <param name="property">The target property.</param>
        /// <returns>True if annotation is added to property value.</returns>
        protected static bool TryAttachRawAnnotationSetToPropertyValue(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, ODataProperty property)
        {
            if (duplicatePropertyNamesChecker != null)
            {
                ODataJsonLightRawAnnotationSet rawAnnotations = duplicatePropertyNamesChecker.AnnotationCollector.
                    GetPropertyRawAnnotationSet(property.Name);
                if (rawAnnotations != null)
                {
                    ODataUntypedValue untypedValue = property.Value as ODataUntypedValue;
                    ODataAnnotatable valueTmp = (ODataAnnotatable)untypedValue ?? (ODataAnnotatable)property.ODataValue;
                    if (valueTmp != null)
                    {
                        valueTmp.SetAnnotation(rawAnnotations);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines the value kind for a non-entity value (that is top-level property value, property value on a complex type, item in a collection)
        /// </summary>
        /// <returns>The type kind of the property value.</returns>
        /// <remarks>
        /// Doesn't move the JSON reader.
        /// </remarks>
        protected EdmTypeKind GetNonEntityValueKind()
        {
            // If we get here, we did not find a type name in the payload and don't have an expected type.
            // This can only happen for error cases when using open properties (for declared properties we always
            // have an expected type and for open properties we always require a type). We then decide based on 
            // the node type of the reader.
            // PrimitiveValue       - we know that it is a primitive value; 
            // StartArray           - we know that we have a collection;
            // Other                - for a JSON object value (and if we did not already find a payload type name)
            //                        we have already started reading the object to find a type name (and have failed)
            //                        and might thus be on a Property or EndObject node.
            //                        Also note that in this case we can't distinguish whether what we are looking at is 
            //                        a complex value or a spatial value (both are JSON objects). We will report 
            //                        'Complex' in that case which will fail we an appropriate error message 
            //                        also for spatial ('value without type name found').
            switch (this.JsonReader.NodeType)
            {
                case JsonNodeType.PrimitiveValue: return EdmTypeKind.Primitive;
                case JsonNodeType.StartArray: return EdmTypeKind.Collection;
                default: return EdmTypeKind.Complex;
            }
        }

        /// <summary>
        /// Tries to read an annotation as OData type name annotation.
        /// </summary>
        /// <param name="payloadTypeName">The read value of the annotation (string).</param>
        /// <returns>true if the annotation is an OData type name annotation, false otherwise.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.Property       - the property that possibly is an odata.type instance annotation
        /// Post-Condition: JsonNodeType.Property       - the next property after the annotation or if the reader did not move
        ///                 JsonNodeType.EndObject      - end of the parent object
        /// If the method returns true, it consumed the value of the annotation from the reader.
        /// If it returns false, it didn't move the reader.
        /// </remarks>
        private bool TryReadODataTypeAnnotation(out string payloadTypeName)
        {
            this.AssertJsonCondition(JsonNodeType.Property);
            payloadTypeName = null;

            bool result = false;
            string propertyName = this.JsonReader.GetPropertyName();
            if (string.CompareOrdinal(propertyName, ODataAnnotationNames.ODataType) == 0)
            {
                // Read over the property name
                this.JsonReader.ReadNext();
                payloadTypeName = this.ReadODataTypeAnnotationValue();
                result = true;
            }

            this.AssertJsonCondition(JsonNodeType.Property, JsonNodeType.EndObject);
            return result;
        }

        /// <summary>
        /// This method creates an reads the property from the input and 
        /// returns an <see cref="ODataProperty"/> representing the read property.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to use.</param>
        /// <returns>An <see cref="ODataProperty"/> representing the read property.</returns>
        /// <remarks>
        /// The method assumes that the ReadPayloadStart has already been called and it will not call ReadPayloadEnd.
        /// </remarks>
        private ODataProperty ReadTopLevelPropertyImplementation(IEdmTypeReference expectedPropertyTypeReference, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(
                expectedPropertyTypeReference == null || !expectedPropertyTypeReference.IsODataEntityTypeKind(),
                "If the expected type is specified it must not be an entity type.");
            Debug.Assert(duplicatePropertyNamesChecker != null, "duplicatePropertyNamesChecker != null");

            expectedPropertyTypeReference = this.UpdateExpectedTypeBasedOnMetadataUri(expectedPropertyTypeReference);

            object propertyValue = missingPropertyValue;

            // Check for the special top-level null marker
            if (this.IsTopLevelNullValue())
            {
                // NOTE: when reading a null value we will never ask the type resolver (if present) to resolve the
                //       type; we always fall back to the expected type.
                ReaderValidationUtils.ValidateNullValue(
                    this.Model,
                    expectedPropertyTypeReference,
                    this.MessageReaderSettings,
                    /*validateNullValue*/ true,
                    this.Version,
                    /*propertyName*/ null);

                // We don't allow properties or non-custom annotations in the null payload.
                this.ValidateNoPropertyInNullPayload(duplicatePropertyNamesChecker);

                propertyValue = null;
            }
            else
            {
                string payloadTypeName = null;
                if (this.ReadingComplexProperty(duplicatePropertyNamesChecker, expectedPropertyTypeReference, out payloadTypeName))
                {
                    // Figure out whether we are reading a complex property or not; complex properties are not wrapped while all others are.
                    // Since we don't have metadata in all cases (open properties), we have to detect the type in some cases.
                    this.AssertJsonCondition(JsonNodeType.Property, JsonNodeType.EndObject);

                    // Now read the property value
                    propertyValue = this.ReadNonEntityValue(
                        payloadTypeName,
                        expectedPropertyTypeReference,
                        duplicatePropertyNamesChecker,
                        /*collectionValidator*/ null,
                        /*validateNullValue*/ true,
                        /*isTopLevelPropertyValue*/ true,
                        /*insideComplexValue*/ true,
                        /*propertyName*/ null);
                }
                else
                {
                    bool isReordering = this.JsonReader is ReorderingJsonReader;

                    Func<string, object> propertyAnnotationReaderForTopLevelProperty =
                        annotationName => { throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedODataPropertyAnnotation(annotationName)); };

                    // Read through all top-level properties, ignore the ones with reserved names (i.e., reserved 
                    // characters in their name) and throw if we find none or more than one properties without reserved name.
                    while (this.JsonReader.NodeType == JsonNodeType.Property)
                    {
                        this.ProcessProperty(
                            duplicatePropertyNamesChecker,
                            propertyAnnotationReaderForTopLevelProperty,
                            (propertyParsingResult, propertyName) =>
                            {
                                switch (propertyParsingResult)
                                {
                                    case PropertyParsingResult.ODataInstanceAnnotation:
                                        if (string.CompareOrdinal(ODataAnnotationNames.ODataType, propertyName) == 0)
                                        {
                                            // When we are not using the reordering reader we have to ensure that the 'odata.type' property appears before
                                            // the 'value' property; otherwise we already scanned ahead and read the type name and have to now
                                            // ignore it (even if it is after the 'value' property).
                                            if (isReordering)
                                            {
                                                this.JsonReader.SkipValue();
                                            }
                                            else
                                            {
                                                if (!object.ReferenceEquals(missingPropertyValue, propertyValue))
                                                {
                                                    throw new ODataException(
                                                        ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_TypePropertyAfterValueProperty(ODataAnnotationNames.ODataType, JsonLightConstants.ODataValuePropertyName));
                                                }

                                                payloadTypeName = this.ReadODataTypeAnnotationValue();
                                            }
                                        }
                                        else
                                        {
                                            throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedAnnotationProperties(propertyName));
                                        }

                                        break;
                                    case PropertyParsingResult.CustomInstanceAnnotation:
                                        this.JsonReader.SkipValue();
                                        break;

                                    case PropertyParsingResult.PropertyWithoutValue:
                                        throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_TopLevelPropertyAnnotationWithoutProperty(propertyName));

                                    case PropertyParsingResult.PropertyWithValue:
                                        if (string.CompareOrdinal(JsonLightConstants.ODataValuePropertyName, propertyName) == 0)
                                        {
                                            // Now read the property value
                                            propertyValue = this.ReadNonEntityValue(
                                                payloadTypeName,
                                                expectedPropertyTypeReference,
                                                /*duplicatePropertyNamesChecker*/ null,
                                                /*collectionValidator*/ null,
                                                /*validateNullValue*/ true,
                                                /*isTopLevelPropertyValue*/ true,
                                                /*insideComplexValue*/ false,
                                                /*propertyName*/ propertyName);
                                        }
                                        else
                                        {
                                            throw new ODataException(
                                                ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_InvalidTopLevelPropertyName(propertyName, JsonLightConstants.ODataValuePropertyName));
                                        }

                                        break;

                                    case PropertyParsingResult.EndOfObject:
                                        break;

                                    case PropertyParsingResult.MetadataReferenceProperty:
                                        throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedMetadataReferenceProperty(propertyName));
                                }
                            });
                    }

                    if (object.ReferenceEquals(missingPropertyValue, propertyValue))
                    {
                        // No property found; there should be exactly one property in the top-level property wrapper that does not have a reserved name.
                        throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_InvalidTopLevelPropertyPayload);
                    }
                }
            }

            Debug.Assert(!object.ReferenceEquals(missingPropertyValue, propertyValue), "!object.ReferenceEquals(missingPropertyValue, propertyValue)");
            ODataProperty resultProperty = new ODataProperty()
            {
                // The property name is not on the metadata URI or the payload, we report null.
                Name = null,
                Value = propertyValue
            };

            // Read over the end object - note that this might be the last node in the input (in case there's no response wrapper)
            this.JsonReader.Read();
            return resultProperty;
        }

        /// <summary>
        /// Updates the expected type based on the metadata URI if there is one.
        /// </summary>
        /// <param name="expectedPropertyTypeReference">The expected property type reference provided by the user through public APIs, or null if one was not provided.</param>
        /// <returns>The expected type reference updated based on the metadata uri, if there is one.</returns>
        private IEdmTypeReference UpdateExpectedTypeBasedOnMetadataUri(IEdmTypeReference expectedPropertyTypeReference)
        {
            Debug.Assert(!this.JsonLightInputContext.ReadingResponse || this.MetadataUriParseResult != null, "Responses should always have a metadata uri, and that should already have been validated.");
            if (this.MetadataUriParseResult == null || this.MetadataUriParseResult.EdmType == null)
            {
                return expectedPropertyTypeReference;
            }

            IEdmType typeFromMetadataUri = this.MetadataUriParseResult.EdmType;
            if (expectedPropertyTypeReference != null && !expectedPropertyTypeReference.Definition.IsAssignableFrom(typeFromMetadataUri))
            {
                throw new ODataException(ODataErrorStrings.ReaderValidationUtils_TypeInMetadataUriDoesNotMatchExpectedType(
                        UriUtilsCommon.UriToString(this.MetadataUriParseResult.MetadataUri),
                        typeFromMetadataUri.ODataFullName(),
                        expectedPropertyTypeReference.ODataFullName()));
            }

            // Assume the value is nullable as its the looser option and the value may come from an open property.
            bool isNullable = true;
            if (expectedPropertyTypeReference != null)
            {
                // if there is a user-provided expected type, then flow nullability information from it.
                isNullable = expectedPropertyTypeReference.IsNullable;
            }

            return typeFromMetadataUri.ToTypeReference(isNullable);
        }

        /// <summary>
        /// Reads a collection value.
        /// </summary>
        /// <param name="collectionValueTypeReference">The collection type reference of the value.</param>
        /// <param name="payloadTypeName">The type name read from the payload.</param>
        /// <param name="serializationTypeNameAnnotation">The serialization type name for the collection value (possibly null).</param>
        /// <returns>The value of the collection.</returns>
        /// <remarks>
        /// Pre-Condition:  Fails if the current node is not a JsonNodeType.StartArray
        /// Post-Condition: almost anything - the node after the collection value (after the EndArray)
        /// </remarks>
        private ODataCollectionValue ReadCollectionValue(
            IEdmCollectionTypeReference collectionValueTypeReference,
            string payloadTypeName,
            SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(
                collectionValueTypeReference == null || collectionValueTypeReference.IsNonEntityCollectionType(),
                "If the metadata is specified it must denote a Collection for this method to work.");

            ODataVersionChecker.CheckCollectionValue(this.Version);

            this.IncreaseRecursionDepth();

            // Read over the start array
            this.JsonReader.ReadStartArray();

            ODataCollectionValue collectionValue = new ODataCollectionValue();
            collectionValue.TypeName = collectionValueTypeReference != null ? collectionValueTypeReference.ODataFullName() : payloadTypeName;
            if (serializationTypeNameAnnotation != null)
            {
                collectionValue.SetAnnotation(serializationTypeNameAnnotation);
            }

            if (collectionValueTypeReference != null)
            {
                collectionValue.SetAnnotation(new ODataTypeAnnotation(collectionValueTypeReference));
            }

            List<object> items = new List<object>();
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = this.CreateDuplicatePropertyNamesChecker();
            IEdmTypeReference itemType = null;
            if (collectionValueTypeReference != null)
            {
                itemType = collectionValueTypeReference.CollectionDefinition().ElementType;
            }

            // NOTE: we do not support reading JSON Light without metadata right now so we always have an expected item type;
            //       The collection validator is always null.
            CollectionWithoutExpectedTypeValidator collectionValidator = null;

            while (this.JsonReader.NodeType != JsonNodeType.EndArray)
            {
                object itemValue = this.ReadNonEntityValueImplementation(
                    /*payloadTypeName*/ null,
                    itemType,
                    duplicatePropertyNamesChecker,
                    collectionValidator,
                    /*validateNullValue*/ true,
                    /*isTopLevelPropertyValue*/ false,
                    /*insideComplexValue*/ false,
                    /*propertyName*/ null);

                // Validate the item (for example that it's not null)
                ValidationUtils.ValidateCollectionItem(itemValue, false /* isStreamable */);

                // Note that the ReadNonEntityValue already validated that the actual type of the value matches
                // the expected type (the itemType).
                items.Add(itemValue);
            }

            Debug.Assert(this.JsonReader.NodeType == JsonNodeType.EndArray, "The results value must end with an end array.");
            this.JsonReader.ReadEndArray();

            collectionValue.Items = new ReadOnlyEnumerable(items);

            this.DecreaseRecursionDepth();

            return collectionValue;
        }

        /// <summary>
        /// Reads a primitive value.
        /// </summary>
        /// <param name="insideJsonObjectValue">true if the reader is positioned on the first property of the value which is a JSON Object 
        ///     (or the second property if the first one was odata.type).</param>
        /// <param name="expectedValueTypeReference">The expected type reference of the value, or null if none is available.</param>
        /// <param name="validateNullValue">true to validate null values; otherwise false.</param>
        /// <param name="propertyName">The name of the property whose value is being read, if applicable (used for error reporting).</param>
        /// <returns>The value of the primitive value.</returns>
        /// <remarks>
        /// Pre-Condition:  insideJsonObjectValue == false -> none - Fails if the current node is not a JsonNodeType.PrimitiveValue
        ///                 insideJsonObjectValue == true -> JsonNodeType.Property or JsonNodeType.EndObject - the first property of the value object,
        ///                     or the second property if first was odata.type, or the end-object.
        /// Post-Condition: almost anything - the node after the primitive value.
        /// </remarks>
        private object ReadPrimitiveValue(bool insideJsonObjectValue, IEdmPrimitiveTypeReference expectedValueTypeReference, bool validateNullValue, string propertyName)
        {
            object result;

            if (expectedValueTypeReference != null && expectedValueTypeReference.IsSpatial())
            {
                result = ODataJsonReaderCoreUtils.ReadSpatialValue(
                    this.JsonReader,
                    insideJsonObjectValue,
                    this.JsonLightInputContext,
                    expectedValueTypeReference,
                    validateNullValue,
                    this.recursionDepth,
                    propertyName);
            }
            else
            {
                if (insideJsonObjectValue)
                {
                    // We manually throw JSON exception here to get a nicer error message (we expect primitive value and got object).
                    // Otherwise the ReadPrimitiveValue would fail with something like "expected primitive value but found property/end object" which is rather confusing.
                    throw new ODataException(ODataErrorStrings.JsonReaderExtensions_UnexpectedNodeDetected(JsonNodeType.PrimitiveValue, JsonNodeType.StartObject));
                }

                result = this.JsonReader.ReadPrimitiveValue();

                if (expectedValueTypeReference != null)
                {
                    result = ODataJsonLightReaderUtils.ConvertValue(
                        result,
                        expectedValueTypeReference,
                        this.MessageReaderSettings,
                        this.Version,
                        validateNullValue,
                        propertyName);
                }
            }

            return result;
        }

        /// <summary>
        /// Reads a complex value.
        /// </summary>
        /// <param name="complexValueTypeReference">The expected type reference of the value.</param>
        /// <param name="payloadTypeName">The type name read from the payload.</param>
        /// <param name="serializationTypeNameAnnotation">The serialization type name for the collection value (possibly null).</param>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to use - this is always initialized as necessary, do not clear.</param>
        /// <returns>The value of the complex value.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.Property - the first property of the complex value object, or the second one if the first one was odata.type.
        ///                 JsonNodeType.EndObject - the end object of the complex value object.
        /// Post-Condition: almost anything - the node after the complex value (after the EndObject)
        /// </remarks>
        private ODataComplexValue ReadComplexValue(
            IEdmComplexTypeReference complexValueTypeReference,
            string payloadTypeName,
            SerializationTypeNameAnnotation serializationTypeNameAnnotation,
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker)
        {
            this.AssertJsonCondition(JsonNodeType.Property, JsonNodeType.EndObject);
            Debug.Assert(duplicatePropertyNamesChecker != null, "duplicatePropertyNamesChecker != null");

            this.IncreaseRecursionDepth();

            ODataComplexValue complexValue = new ODataComplexValue();
            complexValue.TypeName = complexValueTypeReference != null ? complexValueTypeReference.ODataFullName() : payloadTypeName;
            if (serializationTypeNameAnnotation != null)
            {
                complexValue.SetAnnotation(serializationTypeNameAnnotation);
            }

            if (complexValueTypeReference != null)
            {
                complexValue.SetAnnotation(new ODataTypeAnnotation(complexValueTypeReference));
            }

            List<ODataProperty> properties = new List<ODataProperty>();
            while (this.JsonReader.NodeType == JsonNodeType.Property)
            {
                this.ProcessProperty(
                    duplicatePropertyNamesChecker,
                    this.ReadTypePropertyAnnotationValue,
                    (propertyParsingResult, propertyName) =>
                    {
                        switch (propertyParsingResult)
                        {
                            case PropertyParsingResult.ODataInstanceAnnotation:
                                if (string.CompareOrdinal(ODataAnnotationNames.ODataType, propertyName) == 0)
                                {
                                    throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_ComplexTypeAnnotationNotFirst);
                                }
                                else
                                {
                                    throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedAnnotationProperties(propertyName));
                                }

                            case PropertyParsingResult.CustomInstanceAnnotation:
                                this.JsonReader.SkipValue();
                                break;

                            case PropertyParsingResult.PropertyWithoutValue:
                                throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_ComplexValuePropertyAnnotationWithoutProperty(propertyName));

                            case PropertyParsingResult.PropertyWithValue:
                                // Any other property is data
                                ODataProperty property = new ODataProperty();
                                property.Name = propertyName;

                                // Lookup the property in metadata
                                IEdmProperty edmProperty = null;
                                bool ignoreProperty = false;
                                if (complexValueTypeReference != null)
                                {
                                    edmProperty = ReaderValidationUtils.ValidateValuePropertyDefined(propertyName, complexValueTypeReference.ComplexDefinition(), this.MessageReaderSettings, out ignoreProperty);
                                }

                                if (ignoreProperty)
                                {
                                    // in case of ignoreProperty = true which means undeclared property
                                    this.JsonReader.SkipValue();
                                }
                                else
                                {
                                    ODataNullValueBehaviorKind nullValueReadBehaviorKind = this.ReadingResponse || edmProperty == null
                                        ? ODataNullValueBehaviorKind.Default
                                        : this.Model.NullValueReadBehaviorKind(edmProperty);

                                    IEdmProperty propertyTmp = complexValueTypeReference == null
                                        ?
                                        null
                                        :
                                        complexValueTypeReference.FindProperty(propertyName);
                                    object propertyValue = null;
                                    bool toAddPropertyValue = true;
                                    if (propertyTmp == null)
                                    {
                                        if (!this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(
                                                ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty)
                                            && !this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(
                                                ODataUndeclaredPropertyBehaviorKinds.SupportUndeclaredValueProperty))
                                        {
                                            IEdmStructuredType owningStructuredType = complexValueTypeReference.Definition as IEdmStructuredType;
                                            throw new ODataException(ODataErrorStrings.ValidationUtils_PropertyDoesNotExistOnType(propertyName, (owningStructuredType != null) ? owningStructuredType.ODataFullName() : null));
                                        }
                                        else if (this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(ODataUndeclaredPropertyBehaviorKinds.SupportUndeclaredValueProperty))
                                        {
                                            bool isTopLevelPropertyValue = false;
                                            propertyValue = this.InnerReadNonOpenUndeclaredProperty(duplicatePropertyNamesChecker, propertyName, isTopLevelPropertyValue);
                                        }
                                        else
                                        {
                                            this.JsonReader.SkipValue();
                                            toAddPropertyValue = false;
                                        }
                                    }
                                    else
                                    {
                                        // Read the property value
                                        propertyValue = this.ReadNonEntityValueImplementation(
                                            ValidateDataPropertyTypeNameAnnotation(duplicatePropertyNamesChecker, propertyName),
                                            edmProperty == null ? null : edmProperty.Type,
                                            /*duplicatePropertyNamesChecker*/ null,
                                            /*collectionValidator*/ null,
                                            nullValueReadBehaviorKind == ODataNullValueBehaviorKind.Default,
                                            /*isTopLevelPropertyValue*/ false,
                                            /*insideComplexValue*/ false,
                                            propertyName);
                                    }

                                    if (toAddPropertyValue
                                        && (nullValueReadBehaviorKind != ODataNullValueBehaviorKind.IgnoreValue || propertyValue != null))
                                    {
                                        duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(property);
                                        property.Value = propertyValue;
                                        TryAttachRawAnnotationSetToPropertyValue(duplicatePropertyNamesChecker, property);
                                        properties.Add(property);
                                    }
                                }

                                break;

                            case PropertyParsingResult.EndOfObject:
                                break;

                            case PropertyParsingResult.MetadataReferenceProperty:
                                throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedMetadataReferenceProperty(propertyName));
                        }
                    });
            }

            Debug.Assert(this.JsonReader.NodeType == JsonNodeType.EndObject, "After all the properties of a complex value are read the EndObject node is expected.");
            this.JsonReader.ReadEndObject();

            complexValue.Properties = new ReadOnlyEnumerable<ODataProperty>(properties);
            this.DecreaseRecursionDepth();
            return complexValue;
        }

        /// <summary>
        /// Reads a primitive, complex or collection value.
        /// </summary>
        /// <param name="payloadTypeName">The type name read from the payload as a property annotation, or null if none is available.</param>
        /// <param name="expectedTypeReference">The expected type reference of the property value.</param>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to use - if null the method should create a new one if necessary.</param>
        /// <param name="collectionValidator">The collection validator instance if no expected item type has been specified; otherwise null.</param>
        /// <param name="validateNullValue">true to validate null values; otherwise false.</param>
        /// <param name="isTopLevelPropertyValue">true if we are reading a top-level property value; otherwise false.</param>
        /// <param name="insideComplexValue">true if we are reading a complex value and the reader is already positioned inside the complex value; otherwise false.</param>
        /// <param name="propertyName">The name of the property whose value is being read, if applicable (used for error reporting).</param>
        /// <returns>The value of the property read.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.PrimitiveValue   - the value of the property is a primitive value
        ///                 JsonNodeType.StartObject      - the value of the property is an object
        ///                 JsonNodeType.StartArray       - the value of the property is an array
        /// Post-Condition: almost anything - the node after the property value.
        ///                 
        /// Returns the value of the property read, which can be one of:
        /// - null
        /// - primitive value
        /// - <see cref="ODataComplexValue"/>
        /// - <see cref="ODataCollectionValue"/>
        /// </remarks>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "No easy way to refactor.")]
        private object ReadNonEntityValueImplementation(
            string payloadTypeName,
            IEdmTypeReference expectedTypeReference,
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker,
            CollectionWithoutExpectedTypeValidator collectionValidator,
            bool validateNullValue,
            bool isTopLevelPropertyValue,
            bool insideComplexValue,
            string propertyName)
        {
            return ReadNonEntityValueImplementation(
                payloadTypeName,
                expectedTypeReference,
                duplicatePropertyNamesChecker,
                collectionValidator,
                validateNullValue,
                isTopLevelPropertyValue,
                insideComplexValue,
                propertyName,
                false);
        }

        /// <summary>
        /// Reads a primitive, complex or collection value.
        /// </summary>
        /// <param name="payloadTypeName">The type name read from the payload as a property annotation, or null if none is available.</param>
        /// <param name="expectedTypeReference">The expected type reference of the property value.</param>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to use - if null the method should create a new one if necessary.</param>
        /// <param name="collectionValidator">The collection validator instance if no expected item type has been specified; otherwise null.</param>
        /// <param name="validateNullValue">true to validate null values; otherwise false.</param>
        /// <param name="isTopLevelPropertyValue">true if we are reading a top-level property value; otherwise false.</param>
        /// <param name="insideComplexValue">true if we are reading a complex value and the reader is already positioned inside the complex value; otherwise false.</param>
        /// <param name="propertyName">The name of the property whose value is being read, if applicable (used for error reporting). this property name may be re-read from inside json object's odata.type.</param>
        /// <param name="readRawValueEvenIfNoTypeFound">If true: when no type info, read raw value as primitive (not including spatial type), untyped complex or untype collection.</param>
        /// <returns>The value of the property read.</returns>
        /// <remarks>
        /// Pre-Condition:  JsonNodeType.PrimitiveValue   - the value of the property is a primitive value
        ///                 JsonNodeType.StartObject      - the value of the property is an object
        ///                 JsonNodeType.StartArray       - the value of the property is an array
        /// Post-Condition: almost anything - the node after the property value.
        ///                 
        /// Returns the value of the property read, which can be one of:
        /// - null
        /// - primitive value
        /// - <see cref="ODataComplexValue"/>
        /// - <see cref="ODataCollectionValue"/>
        /// </remarks>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "No easy way to refactor.")]
        private object ReadNonEntityValueImplementation(
            string payloadTypeName,
            IEdmTypeReference expectedTypeReference,
            DuplicatePropertyNamesChecker duplicatePropertyNamesChecker,
            CollectionWithoutExpectedTypeValidator collectionValidator,
            bool validateNullValue,
            bool isTopLevelPropertyValue,
            bool insideComplexValue,
            string propertyName,
            bool readRawValueEvenIfNoTypeFound)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(
                this.JsonReader.NodeType == JsonNodeType.PrimitiveValue || this.JsonReader.NodeType == JsonNodeType.StartObject ||
                this.JsonReader.NodeType == JsonNodeType.StartArray || (insideComplexValue && (this.JsonReader.NodeType == JsonNodeType.Property || this.JsonReader.NodeType == JsonNodeType.EndObject)),
                "Pre-Condition: expected JsonNodeType.PrimitiveValue or JsonNodeType.StartObject or JsonNodeType.StartArray or JsonNodeTypeProperty (when inside complex value).");
            Debug.Assert(
                expectedTypeReference == null || !expectedTypeReference.IsODataEntityTypeKind(),
                "Only primitive, complex or collection types can be read by this method.");
            Debug.Assert(
                expectedTypeReference == null || collectionValidator == null,
                "If an expected value type reference is specified, no collection validator must be provided.");

            bool valueIsJsonObject = this.JsonReader.NodeType == JsonNodeType.StartObject;
            bool payloadTypeNameFromPropertyAnnotation;
            if ((duplicatePropertyNamesChecker != null) && (propertyName != null))
            {
                string payloadTypeAnnotationAtSameLevel = ValidateDataPropertyTypeNameAnnotation(duplicatePropertyNamesChecker, propertyName);
                payloadTypeNameFromPropertyAnnotation = !insideComplexValue && (payloadTypeAnnotationAtSameLevel != null);
            }
            else
            {
                payloadTypeNameFromPropertyAnnotation = !insideComplexValue && payloadTypeName != null;
            }

            bool typeNameFoundInPayload = false;
            if (valueIsJsonObject || insideComplexValue)
            {
                // If we have an object value initialize the duplicate property names checker
                if (duplicatePropertyNamesChecker == null)
                {
                    duplicatePropertyNamesChecker = this.CreateDuplicatePropertyNamesChecker();
                }
                else
                {
                    duplicatePropertyNamesChecker.Clear();
                }

                // Read the payload type name
                if (!insideComplexValue)
                {
                    string typeName;
                    typeNameFoundInPayload = this.TryReadPayloadTypeFromObject(
                        duplicatePropertyNamesChecker,
                        insideComplexValue,
                        out typeName);
                    if (typeNameFoundInPayload)
                    {
                        payloadTypeName = typeName;
                    }
                }
            }

            if (string.IsNullOrEmpty(payloadTypeName) && (expectedTypeReference == null) && readRawValueEvenIfNoTypeFound)
            {
                // this code path is for defect # 1768490 (ODataLib should read schema-less custom annotations in error payloads)
                // even if no @odata.type in payload and no expected type in model,
                // can still read raw value as primitive (not including spatial type), untyped complex or untype collection.
                ODataJsonLightGeneralDeserializer generalDeserializer = new ODataJsonLightGeneralDeserializer(this.JsonLightInputContext);
                return generalDeserializer.ReadValue();
            }

            SerializationTypeNameAnnotation serializationTypeNameAnnotation;
            EdmTypeKind targetTypeKind;
            IEdmTypeReference targetTypeReference = ReaderValidationUtils.ResolvePayloadTypeNameAndComputeTargetType(
                EdmTypeKind.None,
                /*defaultPrimitivePayloadType*/ null,
                expectedTypeReference,
                payloadTypeName,
                this.Model,
                this.MessageReaderSettings,
                this.Version,
                this.GetNonEntityValueKind,
                out targetTypeKind,
                out serializationTypeNameAnnotation);

            object result;

            // Try to read a null value
            if (ODataJsonReaderCoreUtils.TryReadNullValue(this.JsonReader, this.JsonLightInputContext, targetTypeReference, validateNullValue, propertyName))
            {
                if (isTopLevelPropertyValue)
                {
                    // For a top-level property value a special null marker object has to be used to indicate  a null value.
                    // If we find a null value for a property at the top-level, it is an invalid payload
                    throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_TopLevelPropertyWithPrimitiveNullValue(ODataAnnotationNames.ODataNull, JsonLightConstants.ODataNullAnnotationTrueValue));
                }

                result = null;
            }
            else
            {
                if (!this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty) &&
                    !this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(ODataUndeclaredPropertyBehaviorKinds.SupportUndeclaredValueProperty) &&
                    targetTypeReference == null &&
                    targetTypeKind != EdmTypeKind.Primitive)
                {
                    // In JSON Light we have to always know the target type; either from an expected type specified in
                    // the API or the metadata URI or from a payload type name.
                    // NOTE: throw the same error message as we do for other cases where we don't have an expected
                    //       type and don't find a payload type; see ReaderValidationUtils.ResolveAndValidateTargetTypeWithNoExpectedType.
                    throw new ODataException(ODataErrorStrings.ReaderValidationUtils_ValueWithoutType);
                }

                Debug.Assert(
                    !valueIsJsonObject || this.JsonReader.NodeType == JsonNodeType.Property || this.JsonReader.NodeType == JsonNodeType.EndObject,
                    "If the value was an object the reader must be on either property or end object.");
                switch (targetTypeKind)
                {
                    case EdmTypeKind.Primitive:
                        Debug.Assert(targetTypeReference == null || targetTypeReference.IsODataPrimitiveTypeKind(), "Expected an OData primitive type.");
                        IEdmPrimitiveTypeReference primitiveTargetTypeReference = targetTypeReference == null ? null : targetTypeReference.AsPrimitive();

                        // If we found an odata.type annotation inside a primitive value, we have to fail; type annotations
                        // for primitive values are property annotations, not instance annotations inside the value.
                        if (typeNameFoundInPayload)
                        {
                            throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_ODataTypeAnnotationInPrimitiveValue(ODataAnnotationNames.ODataType));
                        }

                        result = this.ReadPrimitiveValue(
                            valueIsJsonObject,
                            primitiveTargetTypeReference,
                            validateNullValue,
                            propertyName);
                        break;

                    case EdmTypeKind.Complex:
                        if (this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty) ||
                            this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(ODataUndeclaredPropertyBehaviorKinds.SupportUndeclaredValueProperty))
                        {
                            Debug.Assert(targetTypeReference == null || targetTypeReference.IsComplex(), "Expected null or a complex type.");
                        }
                        else
                        {
                            Debug.Assert(targetTypeReference.IsComplex(), "Expected a complex type.");
                        }

                        if (payloadTypeNameFromPropertyAnnotation)
                        {
                            // We already have type name specified as annotation on the parent property.
                            // OData type property annotation on a complex value - fail.
                            throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_ComplexValueWithPropertyTypeAnnotation(ODataAnnotationNames.ODataType));
                        }

                        if (!valueIsJsonObject && !insideComplexValue)
                        {
                            this.JsonReader.ReadStartObject();
                        }

                        if (this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty) ||
                            this.MessageReaderSettings.ContainUndeclaredPropertyBehavior(ODataUndeclaredPropertyBehaviorKinds.SupportUndeclaredValueProperty))
                        {
                            result = this.ReadComplexValue(
                                targetTypeReference == null ? null : targetTypeReference.AsComplex(),
                                payloadTypeName,
                                serializationTypeNameAnnotation,
                                duplicatePropertyNamesChecker);
                        }
                        else
                        {
                            result = this.ReadComplexValue(
                                targetTypeReference.AsComplex(),
                                payloadTypeName,
                                serializationTypeNameAnnotation,
                                duplicatePropertyNamesChecker);
                        }

                        break;

                    case EdmTypeKind.Collection:
                        Debug.Assert(this.Version >= ODataVersion.V3, "Type resolution should already fail if we would decide to read a collection value in V1/V2 payload.");
                        IEdmCollectionTypeReference collectionTypeReference = ValidationUtils.ValidateCollectionType(targetTypeReference);
                        if (valueIsJsonObject)
                        {
                            // We manually throw JSON exception here to get a nicer error message (we expect array value and got object).
                            // Otherwise the ReadCollectionValue would fail with something like "expected array value but found property/end object" which is rather confusing.
                            throw new ODataException(ODataErrorStrings.JsonReaderExtensions_UnexpectedNodeDetected(JsonNodeType.StartArray, JsonNodeType.StartObject));
                        }

                        result = this.ReadCollectionValue(
                            collectionTypeReference,
                            payloadTypeName,
                            serializationTypeNameAnnotation);
                        break;

                    default:
                        throw new ODataException(ODataErrorStrings.General_InternalError(InternalErrorCodes.ODataJsonLightPropertyAndValueDeserializer_ReadPropertyValue));
                }

                // If we have no expected type make sure the collection items are of the same kind and specify the same name.
                if (collectionValidator != null)
                {
                    string payloadTypeNameFromResult = ODataJsonLightReaderUtils.GetPayloadTypeName(result);
                    Debug.Assert(expectedTypeReference == null, "If a collection validator is specified there must not be an expected value type reference.");
                    collectionValidator.ValidateCollectionItem(payloadTypeNameFromResult, targetTypeKind);
                }
            }

            return result;
        }

        /// <summary>
        /// Reads the payload type name from a JSON object (if it exists).
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to track the detected 'odata.type' annotation (if any).</param>
        /// <param name="insideComplexValue">true if we are reading a complex value and the reader is already positioned inside the complex value; otherwise false.</param>
        /// <param name="payloadTypeName">The value of the odata.type annotation or null if no such annotation exists.</param>
        /// <returns>true if a type name was read from the payload; otherwise false.</returns>
        /// <remarks>
        /// Precondition:   StartObject     the start of a JSON object
        /// Postcondition:  Property        the first property of the object if no 'odata.type' annotation exists as first property
        ///                                 or the first property after the 'odata.type' annotation.
        ///                 EndObject       for an empty JSON object or an object with only the 'odata.type' annotation
        /// </remarks>
        private bool TryReadPayloadTypeFromObject(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, bool insideComplexValue, out string payloadTypeName)
        {
            Debug.Assert(duplicatePropertyNamesChecker != null, "duplicatePropertyNamesChecker != null");
            Debug.Assert(
                (this.JsonReader.NodeType == JsonNodeType.StartObject && !insideComplexValue) ||
                ((this.JsonReader.NodeType == JsonNodeType.Property || this.JsonReader.NodeType == JsonNodeType.EndObject) && insideComplexValue),
                "Pre-Condition: JsonNodeType.StartObject when not inside complex value; JsonNodeType.Property or JsonNodeType.EndObject otherwise.");
            bool readTypeName = false;
            payloadTypeName = null;

            // If not already positioned inside the JSON object, read over the object start
            if (!insideComplexValue)
            {
                this.JsonReader.ReadStartObject();
            }

            if (this.JsonReader.NodeType == JsonNodeType.Property)
            {
                readTypeName = this.TryReadODataTypeAnnotation(out payloadTypeName);
                if (readTypeName)
                {
                    // Register the odata.type annotation we just found with the duplicate property names checker.
                    duplicatePropertyNamesChecker.MarkPropertyAsProcessed(ODataAnnotationNames.ODataType);
                }
            }

            this.AssertJsonCondition(JsonNodeType.Property, JsonNodeType.EndObject);
            return readTypeName;
        }

        /// <summary>
        /// Detects whether we are currently reading a complex property or not. This can be determined from metadata (if we have it)
        /// or from the presence of the odata.type instance annotation in the payload.
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker in use for the entry content.</param>
        /// <param name="expectedPropertyTypeReference">The expected type reference of the property to read.</param>
        /// <param name="payloadTypeName">The type name of the complex value if found in the payload; otherwise null.</param>
        /// <returns>true if we are reading a complex property; otherwise false.</returns>
        /// <remarks>
        /// This method does not move the reader.
        /// </remarks>
        private bool ReadingComplexProperty(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, IEdmTypeReference expectedPropertyTypeReference, out string payloadTypeName)
        {
            payloadTypeName = null;
            bool readingComplexProperty = false;

            // First try to use the metadata if is available
            if (expectedPropertyTypeReference != null)
            {
                readingComplexProperty = expectedPropertyTypeReference.IsComplex();
            }

            // Then check whether the first property in the JSON object is the 'odata.type'
            // annotation; if we don't have an expected property type reference, the 'odata.type'
            // annotation has to exist for complex properties. (This can happen for top-level open 
            // properties).
            if (this.JsonReader.NodeType == JsonNodeType.Property && this.TryReadODataTypeAnnotation(out payloadTypeName))
            {
                // Register the odata.type annotation we just found with the duplicate property names checker.
                duplicatePropertyNamesChecker.MarkPropertyAsProcessed(ODataAnnotationNames.ODataType);

                IEdmType expectedPropertyType = null;
                if (expectedPropertyTypeReference != null)
                {
                    expectedPropertyType = expectedPropertyTypeReference.Definition;
                }

                EdmTypeKind typeKind = EdmTypeKind.None;
                IEdmType actualWirePropertyTypeReference = MetadataUtils.ResolveTypeNameForRead(
                    this.Model,
                    expectedPropertyType,
                    payloadTypeName,
                    this.MessageReaderSettings.ReaderBehavior,
                    this.MessageReaderSettings.MaxProtocolVersion,
                    out typeKind);

                if (actualWirePropertyTypeReference != null)
                {
                    readingComplexProperty = actualWirePropertyTypeReference.IsODataComplexTypeKind();
                }
            }

            return readingComplexProperty;
        }

        /// <summary>
        /// Tries to read a top-level null value from the JSON reader.
        /// </summary>
        /// <returns>true if a null value could be read from the JSON reader; otherwise false.</returns>
        /// <remarks>If the method detects the odata.null annotation, it will read it; otherwise the reader does not move.</remarks>
        private bool IsTopLevelNullValue()
        {
            bool edmNullInMetadata = this.MetadataUriParseResult != null && this.MetadataUriParseResult.IsNullProperty;
            bool odataNullAnnotationInPayload = this.JsonReader.NodeType == JsonNodeType.Property && string.CompareOrdinal(ODataAnnotationNames.ODataNull, this.JsonReader.GetPropertyName()) == 0;
            if (odataNullAnnotationInPayload)
            {
                // If we found the expected annotation read over the property name
                this.JsonReader.ReadNext();

                // Now check the value of the annotation
                object nullAnnotationValue = this.JsonReader.ReadPrimitiveValue();
                if (!(nullAnnotationValue is bool) || (bool)nullAnnotationValue == false)
                {
                    throw new ODataException(ODataErrorStrings.ODataJsonLightReaderUtils_InvalidValueForODataNullAnnotation(ODataAnnotationNames.ODataNull, JsonLightConstants.ODataNullAnnotationTrueValue));
                }
            }

            if (!edmNullInMetadata && !odataNullAnnotationInPayload)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make sure that we don't find any other odata.* annotations or properties after reading a payload with the odata.null annotation or the odata.metadata annotation with value ending #Edm.Null
        /// </summary>
        /// <param name="duplicatePropertyNamesChecker">The duplicate property names checker to use.</param>
        private void ValidateNoPropertyInNullPayload(DuplicatePropertyNamesChecker duplicatePropertyNamesChecker)
        {
            Debug.Assert(duplicatePropertyNamesChecker != null, "duplicatePropertyNamesChecker != null");

            // we use the ParseProperty method to ignore custom annotations.
            Func<string, object> propertyAnnotationReaderForTopLevelNull = annotationName => { throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedODataPropertyAnnotation(annotationName)); };
            while (this.JsonReader.NodeType == JsonNodeType.Property)
            {
                this.ProcessProperty(
                    duplicatePropertyNamesChecker,
                    propertyAnnotationReaderForTopLevelNull,
                    (propertyParsingResult, propertyName) =>
                    {
                        switch (propertyParsingResult)
                        {
                            case PropertyParsingResult.ODataInstanceAnnotation:
                                throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedAnnotationProperties(propertyName));

                            case PropertyParsingResult.CustomInstanceAnnotation:
                                this.JsonReader.SkipValue();
                                break;

                            case PropertyParsingResult.PropertyWithoutValue:
                                throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_TopLevelPropertyAnnotationWithoutProperty(propertyName));

                            case PropertyParsingResult.PropertyWithValue:
                                throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_NoPropertyAndAnnotationAllowedInNullPayload(propertyName));

                            case PropertyParsingResult.EndOfObject:
                                break;

                            case PropertyParsingResult.MetadataReferenceProperty:
                                throw new ODataException(ODataErrorStrings.ODataJsonLightPropertyAndValueDeserializer_UnexpectedMetadataReferenceProperty(propertyName));
                        }
                    });
            }
        }

        /// <summary>
        /// Increases the recursion depth of values by 1. This will throw if the recursion depth exceeds the current limit.
        /// </summary>
        private void IncreaseRecursionDepth()
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref this.recursionDepth, this.MessageReaderSettings.MessageQuotas.MaxNestingDepth);
        }

        /// <summary>
        /// Decreases the recursion depth of values by 1.
        /// </summary>
        private void DecreaseRecursionDepth()
        {
            Debug.Assert(this.recursionDepth > 0, "Can't decrease recursion depth below 0.");

            this.recursionDepth--;
        }

        /// <summary>
        /// Asserts that the current recursion depth of values is zero. This should be true on all calls into this class from outside of this class.
        /// </summary>
        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "The this is needed in DEBUG build.")]
        private void AssertRecursionDepthIsZero()
        {
            Debug.Assert(this.recursionDepth == 0, "The current recursion depth must be 0.");
        }
    }
}

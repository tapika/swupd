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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.OData.Json;
    #endregion Namespaces

    /// <summary>
    /// OData JsonLight deserializer for detecting the payload kind of a JsonLight payload.
    /// </summary>
    internal sealed class ODataJsonLightPayloadKindDetectionDeserializer : ODataJsonLightPropertyAndValueDeserializer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLightInputContext">The JsonLight input context to read from.</param>
        internal ODataJsonLightPayloadKindDetectionDeserializer(ODataJsonLightInputContext jsonLightInputContext)
            : base(jsonLightInputContext)
        {
            DebugUtils.CheckNoExternalCallers();
        }

        /// <summary>
        /// Detects the payload kind(s).
        /// </summary>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>An enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        internal IEnumerable<ODataPayloadKind> DetectPayloadKind(ODataPayloadKindDetectionInfo detectionInfo)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(detectionInfo != null, "detectionInfo != null");
            Debug.Assert(this.ReadingResponse, "Payload kind detection is only supported in responses.");

            // prevent the buffering JSON reader from detecting in-stream errors - we read the error ourselves
            this.JsonReader.DisableInStreamErrorDetection = true;

            try
            {
                this.ReadPayloadStart(
                    ODataPayloadKind.Unsupported,
                    /*duplicatePropertyNamesChecker*/null,
                    /*isReadingNestedPayload*/false,
                    /*allowEmptyPayload*/false);
                return this.DetectPayloadKindImplementation(detectionInfo);
            }
            catch (ODataException)
            {
                // If we are not able to read the payload in the expected JSON format/structure
                // return no detected payload kind below.
                return Enumerable.Empty<ODataPayloadKind>();
            }
            finally
            {
                this.JsonReader.DisableInStreamErrorDetection = false;
            }
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Detects the payload kind(s).
        /// </summary>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>A task which returns an enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        internal Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(ODataPayloadKindDetectionInfo detectionInfo)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(detectionInfo != null, "detectionInfo != null");
            Debug.Assert(this.ReadingResponse, "Payload kind detection is only supported in responses.");

            // prevent the buffering JSON reader from detecting in-stream errors - we read the error ourselves
            this.JsonReader.DisableInStreamErrorDetection = true;

            return this.ReadPayloadStartAsync(
                ODataPayloadKind.Unsupported,
                /*duplicatePropertyNamesChecker*/null,
                /*isReadingNestedPayload*/false,
                /*allowEmptyPayload*/false)

                .FollowOnSuccessWith(t =>
                    {
                        return this.DetectPayloadKindImplementation(detectionInfo);
                    })

                .FollowOnFaultAndCatchExceptionWith<IEnumerable<ODataPayloadKind>, ODataException>(t =>
                    {
                        // If we are not able to read the payload in the expected JSON format/structure
                        // return no detected payload kind below.
                        return Enumerable.Empty<ODataPayloadKind>();
                    })

                .FollowAlwaysWith(t =>
                    {
                        this.JsonReader.DisableInStreamErrorDetection = false;
                    });
        }
#endif

        /// <summary>
        /// Detects the payload kind(s).
        /// </summary>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>An enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        private IEnumerable<ODataPayloadKind> DetectPayloadKindImplementation(ODataPayloadKindDetectionInfo detectionInfo)
        {
            Debug.Assert(detectionInfo != null, "detectionInfo != null");
            Debug.Assert(this.JsonReader.DisableInStreamErrorDetection, "The in-stream error detection should be disabled for payload kind detection.");

            this.AssertJsonCondition(JsonNodeType.Property, JsonNodeType.EndObject);

            // If we found a metadata URI and parsed it, look at the detected payload kind and return it.
            if (this.MetadataUriParseResult != null)
            {
                // Store the parsed metadata URI on the input context so we can avoid parsing it again.
                detectionInfo.SetPayloadKindDetectionFormatState(new ODataJsonLightPayloadKindDetectionState(this.MetadataUriParseResult));

                return this.MetadataUriParseResult.DetectedPayloadKinds;
            }

            // Otherwise this is a payload without metadata URI and we have to start sniffing; only odata.error payloads
            // don't have a metadata URI so check for a single 'odata.error' property (ignoring custom annotations).
            ODataError error = null;
            while (this.JsonReader.NodeType == JsonNodeType.Property)
            {
                string propertyName = this.JsonReader.ReadPropertyName();
                if (!ODataJsonLightReaderUtils.IsAnnotationProperty(propertyName))
                {
                    // If we find a non-annotation property, this is not an error payload
                    return Enumerable.Empty<ODataPayloadKind>();
                }

                string annotatedPropertyName, annotationName;
                if (!ODataJsonLightDeserializer.TryParsePropertyAnnotation(propertyName, out annotatedPropertyName, out annotationName))
                {
                    // Instance annotation; check for odata.error
                    if (ODataJsonLightReaderUtils.IsODataAnnotationName(propertyName))
                    {
                        if (string.CompareOrdinal(ODataAnnotationNames.ODataError, propertyName) == 0)
                        {
                            // If we find multiple errors or an invalid error value, this is not an error payload.
                            if (error != null || !this.JsonReader.StartBufferingAndTryToReadInStreamErrorPropertyValue(out error))
                            {
                                return Enumerable.Empty<ODataPayloadKind>();
                            }

                            // At this point we successfully read the first odata.error property.
                            // Skip the error value and check whether there are more properties.
                            this.JsonReader.SkipValue();
                        }
                        else
                        {
                            // Any odata.* instance annotations other than odata.error are not allowed for errors.
                            return Enumerable.Empty<ODataPayloadKind>();
                        }
                    }
                    else
                    {
                        // Skip custom instance annotations
                        this.JsonReader.SkipValue();
                    }
                }
                else
                {
                    // Property annotation; not allowed for errors
                    return Enumerable.Empty<ODataPayloadKind>();
                }
            }

            // If we got here without finding a metadata URI or an error payload, we don't know what this is.
            if (error == null)
            {
                return Enumerable.Empty<ODataPayloadKind>();
            }

            return new ODataPayloadKind[] { ODataPayloadKind.Error };
        }
    }
}

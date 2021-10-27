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
    /// <summary>
    /// An enumeration that lists the internal errors.
    /// </summary>
    internal enum InternalErrorCodes
    {
        /// <summary>Unreachable codepath in ODataWriterCore.WriteEnd</summary>
        ODataWriterCore_WriteEnd_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataWriterCore.ValidateTransition</summary>
        ODataWriterCore_ValidateTransition_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataWriterCore.Scope.Create</summary>
        ODataWriterCore_Scope_Create_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataWriterCore.DuplicatePropertyNamesChecker.</summary>
        ODataWriterCore_DuplicatePropertyNamesChecker,

        /// <summary>Unreachable codepath in ODataWriterCore.ParentNavigationLinkScope.</summary>
        ODataWriterCore_ParentNavigationLinkScope,

        /// <summary>Unreachable codepath in ODataUtils.VersionString</summary>
        ODataUtils_VersionString_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataUtilsInternal.ToDataServiceVersion</summary>
        ODataUtilsInternal_ToDataServiceVersion_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataUtilsInternal.IsPayloadKindSupported</summary>
        ODataUtilsInternal_IsPayloadKindSupported_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataUtils.GetDefaultEncoding</summary>
        ODataUtils_GetDefaultEncoding_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataUtils.ParseSerializableEpmAnnotations</summary>
        ODataUtils_ParseSerializableEpmAnnotations_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataMessageWriter.WriteProperty</summary>
        ODataMessageWriter_WriteProperty,

        /// <summary>Unreachable codepath in ODataMessageWriter.WriteEntityReferenceLink</summary>
        ODataMessageWriter_WriteEntityReferenceLink,

        /// <summary>Unreachable codepath in ODataMessageWriter.WriteEntityReferenceLinks</summary>
        ODataMessageWriter_WriteEntityReferenceLinks,

        /// <summary>Unreachable codepath in ODataMessageWriter.WriteError</summary>
        ODataMessageWriter_WriteError,

        /// <summary>Unreachable codepath in ODataMessageWriter.WriteServiceDocument</summary>
        ODataMessageWriter_WriteServiceDocument,

        /// <summary>Unreachable codepath in ODataMessageWriter.WriteMetadataDocument</summary>
        ODataMessageWriter_WriteMetadataDocument,

        /// <summary>Unreachable codepath in EpmSyndicationWriter.WriteEntryEpm when writing content target.</summary>
        EpmSyndicationWriter_WriteEntryEpm_ContentTarget,

        /// <summary>Unreachable codepath in EpmSyndicationWriter.CreateAtomTextConstruct when converting text kind from Syndication enumeration.</summary>
        EpmSyndicationWriter_CreateAtomTextConstruct,

        /// <summary>Unreachable codepath in EpmSyndicationWriter.WritePersonEpm.</summary>
        EpmSyndicationWriter_WritePersonEpm,

        /// <summary>Unhandled EpmTargetPathSegment.SegmentName in EpmSyndicationWriter.WriteParentSegment.</summary>
        EpmSyndicationWriter_WriteParentSegment_TargetSegmentName,

        /// <summary>Unreachable codepath in ODataAtomConvert.ToString(AtomTextConstructKind)</summary>
        ODataAtomConvert_ToString,

        /// <summary>Unreachable codepath in ODataCollectionWriter.CreateCollectionWriter</summary>
        ODataCollectionWriter_CreateCollectionWriter_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataCollectionWriterCore.ValidateTransition</summary>
        ODataCollectionWriterCore_ValidateTransition_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataCollectionWriterCore.WriteEnd</summary>
        ODataCollectionWriterCore_WriteEnd_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataParameterWriter.CreateParameterWriter</summary>
        ODataParameterWriter_CannotCreateParameterWriterForFormat,

        /// <summary>Unreachable codepath in ODataParameterWriter.ValidateTransition</summary>
        ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromStart,

        /// <summary>Unreachable codepath in ODataParameterWriter.ValidateTransition</summary>
        ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromCanWriteParameter,

        /// <summary>Unreachable codepath in ODataParameterWriter.ValidateTransition</summary>
        ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromActiveSubWriter,

        /// <summary>Unreachable codepath in ODataParameterWriter.ValidateTransition</summary>
        ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromCompleted,

        /// <summary>Unreachable codepath in ODataParameterWriter.ValidateTransition</summary>
        ODataParameterWriterCore_ValidateTransition_InvalidTransitionFromError,

        /// <summary>Unreachable codepath in ODataParameterWriter.ValidateTransition</summary>
        ODataParameterWriterCore_ValidateTransition_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataParameterWriter.WriteEndImplementation</summary>
        ODataParameterWriterCore_WriteEndImplementation_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataPathValidator.ValidateSegment root branch</summary>
        QueryPathValidator_ValidateSegment_Root,

        /// <summary>Unreachable codepath in ODataPathValidator.ValidateSegment non-root branch</summary>
        QueryPathValidator_ValidateSegment_NonRoot,

        /// <summary>Unreachable codepath in ODataBatchWriter.ValidateTransition</summary>
        ODataBatchWriter_ValidateTransition_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataBatchWriter.ToText(this HttpMethod).</summary>
        ODataBatchWriterUtils_HttpMethod_ToText_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataBatchReader.ReadImplementation.</summary>
        ODataBatchReader_ReadImplementation,

        /// <summary>Unreachable codepath in ODataBatchReader.GetEndBoundary in state Completed.</summary>
        ODataBatchReader_GetEndBoundary_Completed,

        /// <summary>Unreachable codepath in ODataBatchReader.GetEndBoundary in state Exception.</summary>
        ODataBatchReader_GetEndBoundary_Exception,

        /// <summary>Unreachable codepath in ODataBatchReader.GetEndBoundary because of invalid enum value.</summary>
        ODataBatchReader_GetEndBoundary_UnknownValue,

        /// <summary>Unreachable codepath in ODataBatchReaderStream.SkipToBoundary.</summary>
        ODataBatchReaderStream_SkipToBoundary,

        /// <summary>Unreachable codepath in ODataBatchReaderStream.ReadLine.</summary>
        ODataBatchReaderStream_ReadLine,

        /// <summary>Unreachable codepath in ODataBatchReaderStream.ReadWithDelimiter.</summary>
        ODataBatchReaderStream_ReadWithDelimiter,

        /// <summary>Unreachable codepath in ODataBatchReaderStreamBuffer.ScanForBoundary.</summary>
        ODataBatchReaderStreamBuffer_ScanForBoundary,

        /// <summary>Unreachable codepath in ODataBatchReaderStreamBuffer.ReadWithLength.</summary>
        ODataBatchReaderStreamBuffer_ReadWithLength,

        /// <summary>Unreachable codepath in JsonReader.Read.</summary>
        JsonReader_Read,

        /// <summary>Unreachable codepath in ODataReader.CreateReader.</summary>
        ODataReader_CreateReader_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataReaderCore.ReadImplementation.</summary>
        ODataReaderCore_ReadImplementation,

        /// <summary>Unreachable codepath in ODataReaderCoreAsync.ReadAsynchronously.</summary>
        ODataReaderCoreAsync_ReadAsynchronously,

        /// <summary>Unreachable codepath in ODataVerboseJsonEntryAndFeedDeserializer.ReadFeedProperty.</summary>
        ODataVerboseJsonEntryAndFeedDeserializer_ReadFeedProperty,

        /// <summary>Unreachable codepath in ODataVerboseJsonReader.ReadEntryStart.</summary>
        ODataVerboseJsonReader_ReadEntryStart,

        /// <summary>Unreachable codepath in ODataVerboseJsonPropertyAndValueDeserializer.ReadPropertyValue.</summary>
        ODataVerboseJsonPropertyAndValueDeserializer_ReadPropertyValue,

        /// <summary>Unreachable codepath in ODataCollectionReader.CreateReader.</summary>
        ODataCollectionReader_CreateReader_UnreachableCodePath,

        /// <summary>Unreachable codepath in ODataCollectionReaderCore.ReadImplementation.</summary>
        ODataCollectionReaderCore_ReadImplementation,

        /// <summary>Unreachable codepath in ODataCollectionReaderCoreAsync.ReadAsynchronously.</summary>
        ODataCollectionReaderCoreAsync_ReadAsynchronously,

        /// <summary>Unreachable codepath in ODataParameterReaderCore.ReadImplementation.</summary>
        ODataParameterReaderCore_ReadImplementation,

        /// <summary>Unreachable codepath in ODataParameterReaderCoreAsync.ReadAsynchronously.</summary>
        ODataParameterReaderCoreAsync_ReadAsynchronously,

        /// <summary>The value from the parameter reader must be a primitive value, an ODataComplexValue or null</summary>
        ODataParameterReaderCore_ValueMustBePrimitiveOrComplexOrNull,

        /// <summary>Unreachable codepath in ODataAtomReader.ReadAtNavigationLinkStartImplementation.</summary>
        ODataAtomReader_ReadAtNavigationLinkStartImplementation,

        /// <summary>Unreachable codepath in ODataAtomPropertyAndValueDeserializer.ReadNonEntityValue.</summary>
        ODataAtomPropertyAndValueDeserializer_ReadNonEntityValue,

        /// <summary>Unreachable codepath in AtomValueUtils.ConvertStringToPrimitive.</summary>
        AtomValueUtils_ConvertStringToPrimitive,

        /// <summary>Unreachable codepath in EdmCoreModel.PrimitiveType (unsupported type).</summary>
        EdmCoreModel_PrimitiveType,

        /// <summary>Unreachable codepath in EpmSyndicationReader.ReadEntryEpm when reading content target.</summary>
        EpmSyndicationReader_ReadEntryEpm_ContentTarget,

        /// <summary>Unreachable codepath in EpmSyndicationReader.ReadParentSegment.</summary>
        EpmSyndicationReader_ReadParentSegment_TargetSegmentName,

        /// <summary>Unreachable codepath in EpmSyndicationReader.ReadPersonEpm.</summary>
        EpmSyndicationReader_ReadPersonEpm,

        /// <summary>Unreachable codepath in EpmReader.SetEpmValueForSegment when found unexpected type kind.</summary>
        EpmReader_SetEpmValueForSegment_TypeKind,

        /// <summary>Unreachable codepath in EpmReader.SetEpmValueForSegment when found EPM for a primitive stream property.</summary>
        EpmReader_SetEpmValueForSegment_StreamProperty,

        /// <summary>Unreachable codepath in ReaderValidationUtils.ResolveAndValidateTypeName in the strict branch, unexpected type kind.</summary>
        ReaderValidationUtils_ResolveAndValidateTypeName_Strict_TypeKind,

        /// <summary>Unreachable codepath in ReaderValidationUtils.ResolveAndValidateTypeName in the lax branch, unexpected type kind.</summary>
        ReaderValidationUtils_ResolveAndValidateTypeName_Lax_TypeKind,

        /// <summary>Unreachable codepath in EpmExtensionMethods.ToAttributeValue(ODataSyndicationItemProperty) when found unexpected type syndication item property kind.</summary>
        EpmExtensionMethods_ToAttributeValue_SyndicationItemProperty,

        /// <summary>The ODataMetadataFormat.CreateOutputContextAsync was called, but this method is not yet supported.</summary>
        ODataMetadataFormat_CreateOutputContextAsync,

        /// <summary>The ODataMetadataFormat.CreateInputContextAsync was called, but this method is not yet supported.</summary>
        ODataMetadataFormat_CreateInputContextAsync,

        /// <summary>An unsupported method or property has been called on the IDictionary implementation of the ODataModelFunctions.</summary>
        ODataModelFunctions_UnsupportedMethodOrProperty,

        /// <summary>Unreachable codepath in ODataJsonLightPropertyAndValueDeserializer.ReadPropertyValue.</summary>
        ODataJsonLightPropertyAndValueDeserializer_ReadPropertyValue,

        /// <summary>Unreachable codepath in ODataJsonLightPropertyAndValueDeserializer.GetNonEntityValueKind.</summary>
        ODataJsonLightPropertyAndValueDeserializer_GetNonEntityValueKind,

        /// <summary>Unreachable codepath in ODataJsonLightEntryAndFeedDeserializer.ReadFeedProperty.</summary>
        ODataJsonLightEntryAndFeedDeserializer_ReadFeedProperty,

        /// <summary>Unreachable codepath in ODataJsonLightReader.ReadEntryStart.</summary>
        ODataJsonLightReader_ReadEntryStart,

        /// <summary>Unreachable codepath in ODataJsonLightEntryAndFeedDeserializer_ReadTopLevelFeedAnnotations.ReadTopLevelFeedAnnotations.</summary>
        ODataJsonLightEntryAndFeedDeserializer_ReadTopLevelFeedAnnotations,

        /// <summary>Unreachable codepath in ODataJsonLightReader.ReadFeedEnd.</summary>
        ODataJsonLightReader_ReadFeedEnd,

        /// <summary>Unreachable codepath in ODataJsonLightCollectionDeserializer.ReadCollectionStart.</summary>
        ODataJsonLightCollectionDeserializer_ReadCollectionStart,

        /// <summary>Unreachable codepath in ODataJsonLightCollectionDeserializer.ReadCollectionStart.TypeKindFromPayloadFunc.</summary>
        ODataJsonLightCollectionDeserializer_ReadCollectionStart_TypeKindFromPayloadFunc,

        /// <summary>Unreachable codepath in ODataJsonLightCollectionDeserializer.ReadCollectionEnd.</summary>
        ODataJsonLightCollectionDeserializer_ReadCollectionEnd,

        /// <summary>Unreachable codepath in ODataJsonLightEntityReferenceLinkDeserializer.ReadSingleEntityReferenceLink.</summary>
        ODataJsonLightEntityReferenceLinkDeserializer_ReadSingleEntityReferenceLink,

        /// <summary>Unreachable codepath in ODataJsonLightEntityReferenceLinkDeserializer.ReadEntityReferenceLinksAnnotations.</summary>
        ODataJsonLightEntityReferenceLinkDeserializer_ReadEntityReferenceLinksAnnotations,

        /// <summary>Unreachable codepath in ODataJsonLightParameterDeserializer.ReadNextParameter.</summary>
        ODataJsonLightParameterDeserializer_ReadNextParameter,

        /// <summary>Unreachable codepath in ODataJsonLightAnnotationGroupDeserializer.ReadAnnotationGroupDeclaration.</summary>
        ODataJsonLightAnnotationGroupDeserializer_ReadAnnotationGroupDeclaration,

        /// <summary>Unreachable codepath in EdmTypeWriterResolver.GetReturnType for function import group.</summary>
        EdmTypeWriterResolver_GetReturnTypeForFunctionImportGroup,

        /// <summary>Unreachable codepath in the indexer of ODataVersionCache for unknown versions.</summary>
        ODataVersionCache_UnknownVersion,
    }
}

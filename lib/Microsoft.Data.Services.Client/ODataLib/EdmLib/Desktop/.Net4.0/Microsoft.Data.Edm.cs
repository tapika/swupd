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

namespace Microsoft.Data.Edm {
    using System;
    using System.Reflection;
    using System.Globalization;
    using System.Resources;
    using System.Text;
    using System.Threading;
#if !PORTABLELIB
    using System.Security.Permissions;
#endif

    

    /// <summary>
    ///    AutoGenerated resource class. Usage:
    ///
    ///        string s = EntityRes.GetString(EntityRes.MyIdenfitier);
    /// </summary>
    
    internal sealed class EntityRes {
        internal const string EdmPrimitive_UnexpectedKind = "EdmPrimitive_UnexpectedKind";
        internal const string Annotations_DocumentationPun = "Annotations_DocumentationPun";
        internal const string Annotations_TypeMismatch = "Annotations_TypeMismatch";
        internal const string Constructable_VocabularyAnnotationMustHaveTarget = "Constructable_VocabularyAnnotationMustHaveTarget";
        internal const string Constructable_EntityTypeOrCollectionOfEntityTypeExpected = "Constructable_EntityTypeOrCollectionOfEntityTypeExpected";
        internal const string Constructable_TargetMustBeStock = "Constructable_TargetMustBeStock";
        internal const string TypeSemantics_CouldNotConvertTypeReference = "TypeSemantics_CouldNotConvertTypeReference";
        internal const string EdmModel_CannotUseElementWithTypeNone = "EdmModel_CannotUseElementWithTypeNone";
        internal const string EdmEntityContainer_CannotUseElementWithTypeNone = "EdmEntityContainer_CannotUseElementWithTypeNone";
        internal const string ValueWriter_NonSerializableValue = "ValueWriter_NonSerializableValue";
        internal const string ValueHasAlreadyBeenSet = "ValueHasAlreadyBeenSet";
        internal const string PathSegmentMustNotContainSlash = "PathSegmentMustNotContainSlash";
        internal const string Edm_Evaluator_NoTermTypeAnnotationOnType = "Edm_Evaluator_NoTermTypeAnnotationOnType";
        internal const string Edm_Evaluator_NoValueAnnotationOnType = "Edm_Evaluator_NoValueAnnotationOnType";
        internal const string Edm_Evaluator_NoValueAnnotationOnElement = "Edm_Evaluator_NoValueAnnotationOnElement";
        internal const string Edm_Evaluator_UnrecognizedExpressionKind = "Edm_Evaluator_UnrecognizedExpressionKind";
        internal const string Edm_Evaluator_UnboundFunction = "Edm_Evaluator_UnboundFunction";
        internal const string Edm_Evaluator_UnboundPath = "Edm_Evaluator_UnboundPath";
        internal const string Edm_Evaluator_FailedTypeAssertion = "Edm_Evaluator_FailedTypeAssertion";
        internal const string EdmModel_Validator_Semantic_SystemNamespaceEncountered = "EdmModel_Validator_Semantic_SystemNamespaceEncountered";
        internal const string EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys = "EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys";
        internal const string EdmModel_Validator_Semantic_DuplicateEndName = "EdmModel_Validator_Semantic_DuplicateEndName";
        internal const string EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey = "EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey";
        internal const string EdmModel_Validator_Semantic_InvalidComplexTypeAbstract = "EdmModel_Validator_Semantic_InvalidComplexTypeAbstract";
        internal const string EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic = "EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic";
        internal const string EdmModel_Validator_Semantic_InvalidKeyNullablePart = "EdmModel_Validator_Semantic_InvalidKeyNullablePart";
        internal const string EdmModel_Validator_Semantic_EntityKeyMustBeScalar = "EdmModel_Validator_Semantic_EntityKeyMustBeScalar";
        internal const string EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass = "EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass";
        internal const string EdmModel_Validator_Semantic_KeyMissingOnEntityType = "EdmModel_Validator_Semantic_KeyMissingOnEntityType";
        internal const string EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole = "EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole";
        internal const string EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame = "EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame";
        internal const string EdmModel_Validator_Semantic_BadNavigationPropertyCouldNotDetermineType = "EdmModel_Validator_Semantic_BadNavigationPropertyCouldNotDetermineType";
        internal const string EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation = "EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation";
        internal const string EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified = "EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified";
        internal const string EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate = "EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate";
        internal const string EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint = "EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint";
        internal const string EdmModel_Validator_Semantic_NavigationPropertyPrincipalEndMultiplicityUpperBoundMustBeOne = "EdmModel_Validator_Semantic_NavigationPropertyPrincipalEndMultiplicityUpperBoundMustBeOne";
        internal const string EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNonnullable = "EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNonnullable";
        internal const string EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNullable = "EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNullable";
        internal const string EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeZeroOneOrOne = "EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeZeroOneOrOne";
        internal const string EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeMany = "EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeMany";
        internal const string EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint = "EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint";
        internal const string EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint = "EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint";
        internal const string EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint = "EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint";
        internal const string EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintDependentEnd = "EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintDependentEnd";
        internal const string EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintPrimaryEnd = "EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintPrimaryEnd";
        internal const string EdmModel_Validator_Semantic_NullableComplexTypeProperty = "EdmModel_Validator_Semantic_NullableComplexTypeProperty";
        internal const string EdmModel_Validator_Semantic_InvalidPropertyType = "EdmModel_Validator_Semantic_InvalidPropertyType";
        internal const string EdmModel_Validator_Semantic_ComposableFunctionImportCannotBeSideEffecting = "EdmModel_Validator_Semantic_ComposableFunctionImportCannotBeSideEffecting";
        internal const string EdmModel_Validator_Semantic_BindableFunctionImportMustHaveParameters = "EdmModel_Validator_Semantic_BindableFunctionImportMustHaveParameters";
        internal const string EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeV1 = "EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeV1";
        internal const string EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeAfterV1 = "EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeAfterV1";
        internal const string EdmModel_Validator_Semantic_FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet = "EdmModel_Validator_Semantic_FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet";
        internal const string EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet = "EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet";
        internal const string EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet2 = "EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet2";
        internal const string EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionKindIsInvalid = "EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionKindIsInvalid";
        internal const string EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionIsInvalid = "EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionIsInvalid";
        internal const string EdmModel_Validator_Semantic_FunctionImportSpecifiesEntitySetButNotEntityType = "EdmModel_Validator_Semantic_FunctionImportSpecifiesEntitySetButNotEntityType";
        internal const string EdmModel_Validator_Semantic_ComposableFunctionImportMustHaveReturnType = "EdmModel_Validator_Semantic_ComposableFunctionImportMustHaveReturnType";
        internal const string EdmModel_Validator_Semantic_ParameterNameAlreadyDefinedDuplicate = "EdmModel_Validator_Semantic_ParameterNameAlreadyDefinedDuplicate";
        internal const string EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName = "EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName";
        internal const string EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined = "EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined";
        internal const string EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName = "EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName";
        internal const string EdmModel_Validator_Semantic_PropertyNameAlreadyDefined = "EdmModel_Validator_Semantic_PropertyNameAlreadyDefined";
        internal const string EdmModel_Validator_Semantic_BaseTypeMustHaveSameTypeKind = "EdmModel_Validator_Semantic_BaseTypeMustHaveSameTypeKind";
        internal const string EdmModel_Validator_Semantic_RowTypeMustNotHaveBaseType = "EdmModel_Validator_Semantic_RowTypeMustNotHaveBaseType";
        internal const string EdmModel_Validator_Semantic_FunctionsNotSupportedBeforeV2 = "EdmModel_Validator_Semantic_FunctionsNotSupportedBeforeV2";
        internal const string EdmModel_Validator_Semantic_FunctionImportSideEffectingNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_FunctionImportSideEffectingNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_FunctionImportComposableNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_FunctionImportComposableNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_FunctionImportBindableNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_FunctionImportBindableNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_KeyPropertyMustBelongToEntity = "EdmModel_Validator_Semantic_KeyPropertyMustBelongToEntity";
        internal const string EdmModel_Validator_Semantic_DependentPropertiesMustBelongToDependentEntity = "EdmModel_Validator_Semantic_DependentPropertiesMustBelongToDependentEntity";
        internal const string EdmModel_Validator_Semantic_DeclaringTypeMustBeCorrect = "EdmModel_Validator_Semantic_DeclaringTypeMustBeCorrect";
        internal const string EdmModel_Validator_Semantic_InaccessibleType = "EdmModel_Validator_Semantic_InaccessibleType";
        internal const string EdmModel_Validator_Semantic_AmbiguousType = "EdmModel_Validator_Semantic_AmbiguousType";
        internal const string EdmModel_Validator_Semantic_InvalidNavigationPropertyType = "EdmModel_Validator_Semantic_InvalidNavigationPropertyType";
        internal const string EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentTargetMustBeOptional = "EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentTargetMustBeOptional";
        internal const string EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne = "EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne";
        internal const string EdmModel_Validator_Semantic_NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne = "EdmModel_Validator_Semantic_NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne";
        internal const string EdmModel_Validator_Semantic_NavigationPropertyContainsTargetNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_NavigationPropertyContainsTargetNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_OnlyInputParametersAllowedInFunctions = "EdmModel_Validator_Semantic_OnlyInputParametersAllowedInFunctions";
        internal const string EdmModel_Validator_Semantic_InvalidFunctionImportParameterMode = "EdmModel_Validator_Semantic_InvalidFunctionImportParameterMode";
        internal const string EdmModel_Validator_Semantic_FunctionImportParameterIncorrectType = "EdmModel_Validator_Semantic_FunctionImportParameterIncorrectType";
        internal const string EdmModel_Validator_Semantic_RowTypeMustHaveProperties = "EdmModel_Validator_Semantic_RowTypeMustHaveProperties";
        internal const string EdmModel_Validator_Semantic_ComplexTypeMustHaveProperties = "EdmModel_Validator_Semantic_ComplexTypeMustHaveProperties";
        internal const string EdmModel_Validator_Semantic_DuplicateDependentProperty = "EdmModel_Validator_Semantic_DuplicateDependentProperty";
        internal const string EdmModel_Validator_Semantic_ScaleOutOfRange = "EdmModel_Validator_Semantic_ScaleOutOfRange";
        internal const string EdmModel_Validator_Semantic_PrecisionOutOfRange = "EdmModel_Validator_Semantic_PrecisionOutOfRange";
        internal const string EdmModel_Validator_Semantic_StringMaxLengthOutOfRange = "EdmModel_Validator_Semantic_StringMaxLengthOutOfRange";
        internal const string EdmModel_Validator_Semantic_MaxLengthOutOfRange = "EdmModel_Validator_Semantic_MaxLengthOutOfRange";
        internal const string EdmModel_Validator_Semantic_InvalidPropertyTypeConcurrencyMode = "EdmModel_Validator_Semantic_InvalidPropertyTypeConcurrencyMode";
        internal const string EdmModel_Validator_Semantic_EntityKeyMustNotBeBinaryBeforeV2 = "EdmModel_Validator_Semantic_EntityKeyMustNotBeBinaryBeforeV2";
        internal const string EdmModel_Validator_Semantic_EnumsNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_EnumsNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_EnumMemberTypeMustMatchEnumUnderlyingType = "EdmModel_Validator_Semantic_EnumMemberTypeMustMatchEnumUnderlyingType";
        internal const string EdmModel_Validator_Semantic_EnumMemberNameAlreadyDefined = "EdmModel_Validator_Semantic_EnumMemberNameAlreadyDefined";
        internal const string EdmModel_Validator_Semantic_ValueTermsNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_ValueTermsNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_VocabularyAnnotationsNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_VocabularyAnnotationsNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_OpenTypesSupportedOnlyInV12AndAfterV3 = "EdmModel_Validator_Semantic_OpenTypesSupportedOnlyInV12AndAfterV3";
        internal const string EdmModel_Validator_Semantic_OpenTypesSupportedForEntityTypesOnly = "EdmModel_Validator_Semantic_OpenTypesSupportedForEntityTypesOnly";
        internal const string EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull = "EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull";
        internal const string EdmModel_Validator_Semantic_InvalidElementAnnotationMismatchedTerm = "EdmModel_Validator_Semantic_InvalidElementAnnotationMismatchedTerm";
        internal const string EdmModel_Validator_Semantic_InvalidElementAnnotationValueInvalidXml = "EdmModel_Validator_Semantic_InvalidElementAnnotationValueInvalidXml";
        internal const string EdmModel_Validator_Semantic_InvalidElementAnnotationNotIEdmStringValue = "EdmModel_Validator_Semantic_InvalidElementAnnotationNotIEdmStringValue";
        internal const string EdmModel_Validator_Semantic_InvalidElementAnnotationNullNamespaceOrName = "EdmModel_Validator_Semantic_InvalidElementAnnotationNullNamespaceOrName";
        internal const string EdmModel_Validator_Semantic_CannotAssertNullableTypeAsNonNullableType = "EdmModel_Validator_Semantic_CannotAssertNullableTypeAsNonNullableType";
        internal const string EdmModel_Validator_Semantic_ExpressionPrimitiveKindCannotPromoteToAssertedType = "EdmModel_Validator_Semantic_ExpressionPrimitiveKindCannotPromoteToAssertedType";
        internal const string EdmModel_Validator_Semantic_NullCannotBeAssertedToBeANonNullableType = "EdmModel_Validator_Semantic_NullCannotBeAssertedToBeANonNullableType";
        internal const string EdmModel_Validator_Semantic_ExpressionNotValidForTheAssertedType = "EdmModel_Validator_Semantic_ExpressionNotValidForTheAssertedType";
        internal const string EdmModel_Validator_Semantic_CollectionExpressionNotValidForNonCollectionType = "EdmModel_Validator_Semantic_CollectionExpressionNotValidForNonCollectionType";
        internal const string EdmModel_Validator_Semantic_PrimitiveConstantExpressionNotValidForNonPrimitiveType = "EdmModel_Validator_Semantic_PrimitiveConstantExpressionNotValidForNonPrimitiveType";
        internal const string EdmModel_Validator_Semantic_RecordExpressionNotValidForNonStructuredType = "EdmModel_Validator_Semantic_RecordExpressionNotValidForNonStructuredType";
        internal const string EdmModel_Validator_Semantic_RecordExpressionMissingProperty = "EdmModel_Validator_Semantic_RecordExpressionMissingProperty";
        internal const string EdmModel_Validator_Semantic_RecordExpressionHasExtraProperties = "EdmModel_Validator_Semantic_RecordExpressionHasExtraProperties";
        internal const string EdmModel_Validator_Semantic_DuplicateAnnotation = "EdmModel_Validator_Semantic_DuplicateAnnotation";
        internal const string EdmModel_Validator_Semantic_IncorrectNumberOfArguments = "EdmModel_Validator_Semantic_IncorrectNumberOfArguments";
        internal const string EdmModel_Validator_Semantic_StreamTypeReferencesNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_StreamTypeReferencesNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_SpatialTypeReferencesNotSupportedBeforeV3 = "EdmModel_Validator_Semantic_SpatialTypeReferencesNotSupportedBeforeV3";
        internal const string EdmModel_Validator_Semantic_DuplicateEntityContainerName = "EdmModel_Validator_Semantic_DuplicateEntityContainerName";
        internal const string EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType = "EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType";
        internal const string EdmModel_Validator_Semantic_IntegerConstantValueOutOfRange = "EdmModel_Validator_Semantic_IntegerConstantValueOutOfRange";
        internal const string EdmModel_Validator_Semantic_StringConstantLengthOutOfRange = "EdmModel_Validator_Semantic_StringConstantLengthOutOfRange";
        internal const string EdmModel_Validator_Semantic_BinaryConstantLengthOutOfRange = "EdmModel_Validator_Semantic_BinaryConstantLengthOutOfRange";
        internal const string EdmModel_Validator_Semantic_TypeMustNotHaveKindOfNone = "EdmModel_Validator_Semantic_TypeMustNotHaveKindOfNone";
        internal const string EdmModel_Validator_Semantic_TermMustNotHaveKindOfNone = "EdmModel_Validator_Semantic_TermMustNotHaveKindOfNone";
        internal const string EdmModel_Validator_Semantic_SchemaElementMustNotHaveKindOfNone = "EdmModel_Validator_Semantic_SchemaElementMustNotHaveKindOfNone";
        internal const string EdmModel_Validator_Semantic_PropertyMustNotHaveKindOfNone = "EdmModel_Validator_Semantic_PropertyMustNotHaveKindOfNone";
        internal const string EdmModel_Validator_Semantic_PrimitiveTypeMustNotHaveKindOfNone = "EdmModel_Validator_Semantic_PrimitiveTypeMustNotHaveKindOfNone";
        internal const string EdmModel_Validator_Semantic_EntityContainerElementMustNotHaveKindOfNone = "EdmModel_Validator_Semantic_EntityContainerElementMustNotHaveKindOfNone";
        internal const string EdmModel_Validator_Semantic_DuplicateNavigationPropertyMapping = "EdmModel_Validator_Semantic_DuplicateNavigationPropertyMapping";
        internal const string EdmModel_Validator_Semantic_EntitySetNavigationMappingMustBeBidirectional = "EdmModel_Validator_Semantic_EntitySetNavigationMappingMustBeBidirectional";
        internal const string EdmModel_Validator_Semantic_EntitySetCanOnlyBeContainedByASingleNavigationProperty = "EdmModel_Validator_Semantic_EntitySetCanOnlyBeContainedByASingleNavigationProperty";
        internal const string EdmModel_Validator_Semantic_TypeAnnotationMissingRequiredProperty = "EdmModel_Validator_Semantic_TypeAnnotationMissingRequiredProperty";
        internal const string EdmModel_Validator_Semantic_TypeAnnotationHasExtraProperties = "EdmModel_Validator_Semantic_TypeAnnotationHasExtraProperties";
        internal const string EdmModel_Validator_Semantic_EnumMustHaveIntegralUnderlyingType = "EdmModel_Validator_Semantic_EnumMustHaveIntegralUnderlyingType";
        internal const string EdmModel_Validator_Semantic_InaccessibleTerm = "EdmModel_Validator_Semantic_InaccessibleTerm";
        internal const string EdmModel_Validator_Semantic_InaccessibleTarget = "EdmModel_Validator_Semantic_InaccessibleTarget";
        internal const string EdmModel_Validator_Semantic_ElementDirectValueAnnotationFullNameMustBeUnique = "EdmModel_Validator_Semantic_ElementDirectValueAnnotationFullNameMustBeUnique";
        internal const string EdmModel_Validator_Semantic_NoEntitySetsFoundForType = "EdmModel_Validator_Semantic_NoEntitySetsFoundForType";
        internal const string EdmModel_Validator_Semantic_CannotInferEntitySetWithMultipleSetsPerType = "EdmModel_Validator_Semantic_CannotInferEntitySetWithMultipleSetsPerType";
        internal const string EdmModel_Validator_Semantic_EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet = "EdmModel_Validator_Semantic_EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet";
        internal const string EdmModel_Validator_Semantic_NavigationPropertyEntityMustNotIndirectlyContainItself = "EdmModel_Validator_Semantic_NavigationPropertyEntityMustNotIndirectlyContainItself";
        internal const string EdmModel_Validator_Semantic_PathIsNotValidForTheGivenContext = "EdmModel_Validator_Semantic_PathIsNotValidForTheGivenContext";
        internal const string EdmModel_Validator_Semantic_EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty = "EdmModel_Validator_Semantic_EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty";
        internal const string EdmModel_Validator_Syntactic_MissingName = "EdmModel_Validator_Syntactic_MissingName";
        internal const string EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong = "EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong";
        internal const string EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed = "EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed";
        internal const string EdmModel_Validator_Syntactic_MissingNamespaceName = "EdmModel_Validator_Syntactic_MissingNamespaceName";
        internal const string EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsTooLong = "EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsTooLong";
        internal const string EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsNotAllowed = "EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsNotAllowed";
        internal const string EdmModel_Validator_Syntactic_PropertyMustNotBeNull = "EdmModel_Validator_Syntactic_PropertyMustNotBeNull";
        internal const string EdmModel_Validator_Syntactic_EnumPropertyValueOutOfRange = "EdmModel_Validator_Syntactic_EnumPropertyValueOutOfRange";
        internal const string EdmModel_Validator_Syntactic_InterfaceKindValueMismatch = "EdmModel_Validator_Syntactic_InterfaceKindValueMismatch";
        internal const string EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch = "EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch";
        internal const string EdmModel_Validator_Syntactic_InterfaceKindValueUnexpected = "EdmModel_Validator_Syntactic_InterfaceKindValueUnexpected";
        internal const string EdmModel_Validator_Syntactic_EnumerableMustNotHaveNullElements = "EdmModel_Validator_Syntactic_EnumerableMustNotHaveNullElements";
        internal const string EdmModel_Validator_Syntactic_NavigationPartnerInvalid = "EdmModel_Validator_Syntactic_NavigationPartnerInvalid";
        internal const string EdmModel_Validator_Syntactic_InterfaceCriticalCycleInTypeHierarchy = "EdmModel_Validator_Syntactic_InterfaceCriticalCycleInTypeHierarchy";
        internal const string Serializer_SingleFileExpected = "Serializer_SingleFileExpected";
        internal const string Serializer_UnknownEdmVersion = "Serializer_UnknownEdmVersion";
        internal const string Serializer_UnknownEdmxVersion = "Serializer_UnknownEdmxVersion";
        internal const string Serializer_NonInlineFunctionImportReturnType = "Serializer_NonInlineFunctionImportReturnType";
        internal const string Serializer_ReferencedTypeMustHaveValidName = "Serializer_ReferencedTypeMustHaveValidName";
        internal const string Serializer_OutOfLineAnnotationTargetMustHaveValidName = "Serializer_OutOfLineAnnotationTargetMustHaveValidName";
        internal const string Serializer_NoSchemasProduced = "Serializer_NoSchemasProduced";
        internal const string XmlParser_EmptyFile = "XmlParser_EmptyFile";
        internal const string XmlParser_EmptySchemaTextReader = "XmlParser_EmptySchemaTextReader";
        internal const string XmlParser_MissingAttribute = "XmlParser_MissingAttribute";
        internal const string XmlParser_TextNotAllowed = "XmlParser_TextNotAllowed";
        internal const string XmlParser_UnexpectedAttribute = "XmlParser_UnexpectedAttribute";
        internal const string XmlParser_UnexpectedElement = "XmlParser_UnexpectedElement";
        internal const string XmlParser_UnusedElement = "XmlParser_UnusedElement";
        internal const string XmlParser_UnexpectedNodeType = "XmlParser_UnexpectedNodeType";
        internal const string XmlParser_UnexpectedRootElement = "XmlParser_UnexpectedRootElement";
        internal const string XmlParser_UnexpectedRootElementWrongNamespace = "XmlParser_UnexpectedRootElementWrongNamespace";
        internal const string XmlParser_UnexpectedRootElementNoNamespace = "XmlParser_UnexpectedRootElementNoNamespace";
        internal const string CsdlParser_InvalidAlias = "CsdlParser_InvalidAlias";
        internal const string CsdlParser_AssociationHasAtMostOneConstraint = "CsdlParser_AssociationHasAtMostOneConstraint";
        internal const string CsdlParser_InvalidDeleteAction = "CsdlParser_InvalidDeleteAction";
        internal const string CsdlParser_MissingTypeAttributeOrElement = "CsdlParser_MissingTypeAttributeOrElement";
        internal const string CsdlParser_InvalidAssociationIncorrectNumberOfEnds = "CsdlParser_InvalidAssociationIncorrectNumberOfEnds";
        internal const string CsdlParser_InvalidAssociationSetIncorrectNumberOfEnds = "CsdlParser_InvalidAssociationSetIncorrectNumberOfEnds";
        internal const string CsdlParser_InvalidConcurrencyMode = "CsdlParser_InvalidConcurrencyMode";
        internal const string CsdlParser_InvalidParameterMode = "CsdlParser_InvalidParameterMode";
        internal const string CsdlParser_InvalidEndRoleInRelationshipConstraint = "CsdlParser_InvalidEndRoleInRelationshipConstraint";
        internal const string CsdlParser_InvalidMultiplicity = "CsdlParser_InvalidMultiplicity";
        internal const string CsdlParser_ReferentialConstraintRequiresOneDependent = "CsdlParser_ReferentialConstraintRequiresOneDependent";
        internal const string CsdlParser_ReferentialConstraintRequiresOnePrincipal = "CsdlParser_ReferentialConstraintRequiresOnePrincipal";
        internal const string CsdlParser_InvalidIfExpressionIncorrectNumberOfOperands = "CsdlParser_InvalidIfExpressionIncorrectNumberOfOperands";
        internal const string CsdlParser_InvalidIsTypeExpressionIncorrectNumberOfOperands = "CsdlParser_InvalidIsTypeExpressionIncorrectNumberOfOperands";
        internal const string CsdlParser_InvalidAssertTypeExpressionIncorrectNumberOfOperands = "CsdlParser_InvalidAssertTypeExpressionIncorrectNumberOfOperands";
        internal const string CsdlParser_InvalidLabeledElementExpressionIncorrectNumberOfOperands = "CsdlParser_InvalidLabeledElementExpressionIncorrectNumberOfOperands";
        internal const string CsdlParser_InvalidTypeName = "CsdlParser_InvalidTypeName";
        internal const string CsdlParser_InvalidQualifiedName = "CsdlParser_InvalidQualifiedName";
        internal const string CsdlParser_NoReadersProvided = "CsdlParser_NoReadersProvided";
        internal const string CsdlParser_NullXmlReader = "CsdlParser_NullXmlReader";
        internal const string CsdlParser_InvalidEntitySetPath = "CsdlParser_InvalidEntitySetPath";
        internal const string CsdlParser_InvalidEnumMemberPath = "CsdlParser_InvalidEnumMemberPath";
        internal const string CsdlSemantics_ReferentialConstraintMismatch = "CsdlSemantics_ReferentialConstraintMismatch";
        internal const string CsdlSemantics_EnumMemberValueOutOfRange = "CsdlSemantics_EnumMemberValueOutOfRange";
        internal const string CsdlSemantics_ImpossibleAnnotationsTarget = "CsdlSemantics_ImpossibleAnnotationsTarget";
        internal const string CsdlSemantics_DuplicateAlias = "CsdlSemantics_DuplicateAlias";
        internal const string EdmxParser_EdmxVersionMismatch = "EdmxParser_EdmxVersionMismatch";
        internal const string EdmxParser_EdmxDataServiceVersionInvalid = "EdmxParser_EdmxDataServiceVersionInvalid";
        internal const string EdmxParser_EdmxMaxDataServiceVersionInvalid = "EdmxParser_EdmxMaxDataServiceVersionInvalid";
        internal const string EdmxParser_BodyElement = "EdmxParser_BodyElement";
        internal const string EdmParseException_ErrorsEncounteredInEdmx = "EdmParseException_ErrorsEncounteredInEdmx";
        internal const string ValueParser_InvalidBoolean = "ValueParser_InvalidBoolean";
        internal const string ValueParser_InvalidInteger = "ValueParser_InvalidInteger";
        internal const string ValueParser_InvalidLong = "ValueParser_InvalidLong";
        internal const string ValueParser_InvalidFloatingPoint = "ValueParser_InvalidFloatingPoint";
        internal const string ValueParser_InvalidMaxLength = "ValueParser_InvalidMaxLength";
        internal const string ValueParser_InvalidSrid = "ValueParser_InvalidSrid";
        internal const string ValueParser_InvalidGuid = "ValueParser_InvalidGuid";
        internal const string ValueParser_InvalidDecimal = "ValueParser_InvalidDecimal";
        internal const string ValueParser_InvalidDateTimeOffset = "ValueParser_InvalidDateTimeOffset";
        internal const string ValueParser_InvalidDateTime = "ValueParser_InvalidDateTime";
        internal const string ValueParser_InvalidTime = "ValueParser_InvalidTime";
        internal const string ValueParser_InvalidBinary = "ValueParser_InvalidBinary";
        internal const string UnknownEnumVal_Multiplicity = "UnknownEnumVal_Multiplicity";
        internal const string UnknownEnumVal_SchemaElementKind = "UnknownEnumVal_SchemaElementKind";
        internal const string UnknownEnumVal_TypeKind = "UnknownEnumVal_TypeKind";
        internal const string UnknownEnumVal_PrimitiveKind = "UnknownEnumVal_PrimitiveKind";
        internal const string UnknownEnumVal_ContainerElementKind = "UnknownEnumVal_ContainerElementKind";
        internal const string UnknownEnumVal_EdmxTarget = "UnknownEnumVal_EdmxTarget";
        internal const string UnknownEnumVal_FunctionParameterMode = "UnknownEnumVal_FunctionParameterMode";
        internal const string UnknownEnumVal_ConcurrencyMode = "UnknownEnumVal_ConcurrencyMode";
        internal const string UnknownEnumVal_PropertyKind = "UnknownEnumVal_PropertyKind";
        internal const string UnknownEnumVal_TermKind = "UnknownEnumVal_TermKind";
        internal const string UnknownEnumVal_ExpressionKind = "UnknownEnumVal_ExpressionKind";
        internal const string Bad_AmbiguousElementBinding = "Bad_AmbiguousElementBinding";
        internal const string Bad_UnresolvedType = "Bad_UnresolvedType";
        internal const string Bad_UnresolvedComplexType = "Bad_UnresolvedComplexType";
        internal const string Bad_UnresolvedEntityType = "Bad_UnresolvedEntityType";
        internal const string Bad_UnresolvedPrimitiveType = "Bad_UnresolvedPrimitiveType";
        internal const string Bad_UnresolvedFunction = "Bad_UnresolvedFunction";
        internal const string Bad_AmbiguousFunction = "Bad_AmbiguousFunction";
        internal const string Bad_FunctionParametersDontMatch = "Bad_FunctionParametersDontMatch";
        internal const string Bad_UnresolvedEntitySet = "Bad_UnresolvedEntitySet";
        internal const string Bad_UnresolvedEntityContainer = "Bad_UnresolvedEntityContainer";
        internal const string Bad_UnresolvedEnumType = "Bad_UnresolvedEnumType";
        internal const string Bad_UnresolvedEnumMember = "Bad_UnresolvedEnumMember";
        internal const string Bad_UnresolvedProperty = "Bad_UnresolvedProperty";
        internal const string Bad_UnresolvedParameter = "Bad_UnresolvedParameter";
        internal const string Bad_UnresolvedLabeledElement = "Bad_UnresolvedLabeledElement";
        internal const string Bad_CyclicEntity = "Bad_CyclicEntity";
        internal const string Bad_CyclicComplex = "Bad_CyclicComplex";
        internal const string Bad_CyclicEntityContainer = "Bad_CyclicEntityContainer";
        internal const string Bad_UncomputableAssociationEnd = "Bad_UncomputableAssociationEnd";
        internal const string RuleSet_DuplicateRulesExistInRuleSet = "RuleSet_DuplicateRulesExistInRuleSet";
        internal const string EdmToClr_UnsupportedTypeCode = "EdmToClr_UnsupportedTypeCode";
        internal const string EdmToClr_StructuredValueMappedToNonClass = "EdmToClr_StructuredValueMappedToNonClass";
        internal const string EdmToClr_IEnumerableOfTPropertyAlreadyHasValue = "EdmToClr_IEnumerableOfTPropertyAlreadyHasValue";
        internal const string EdmToClr_StructuredPropertyDuplicateValue = "EdmToClr_StructuredPropertyDuplicateValue";
        internal const string EdmToClr_CannotConvertEdmValueToClrType = "EdmToClr_CannotConvertEdmValueToClrType";
        internal const string EdmToClr_CannotConvertEdmCollectionValueToClrType = "EdmToClr_CannotConvertEdmCollectionValueToClrType";
        internal const string EdmToClr_TryCreateObjectInstanceReturnedWrongObject = "EdmToClr_TryCreateObjectInstanceReturnedWrongObject";

        static EntityRes loader = null;
        ResourceManager resources;

        internal EntityRes() {
#if !WINRT        
            resources = new System.Resources.ResourceManager("Microsoft.Data.Edm", this.GetType().Assembly);
#else
            resources = new System.Resources.ResourceManager("Microsoft.Data.Edm", this.GetType().GetTypeInfo().Assembly);
#endif
        }
        
        private static EntityRes GetLoader() {
            if (loader == null) {
                EntityRes sr = new EntityRes();
                Interlocked.CompareExchange(ref loader, sr, null);
            }
            return loader;
        }

        private static CultureInfo Culture {
            get { return null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }
        
        public static ResourceManager Resources {
            get {
                return GetLoader().resources;
            }
        }
        
        public static string GetString(string name, params object[] args) {
            EntityRes sys = GetLoader();
            if (sys == null)
                return null;
            string res = sys.resources.GetString(name, EntityRes.Culture);

            if (args != null && args.Length > 0) {
                for (int i = 0; i < args.Length; i ++) {
                    String value = args[i] as String;
                    if (value != null && value.Length > 1024) {
                        args[i] = value.Substring(0, 1024 - 3) + "...";
                    }
                }
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else {
                return res;
            }
        }

        public static string GetString(string name) {
            EntityRes sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetString(name, EntityRes.Culture);
        }
        
        public static string GetString(string name, out bool usedFallback) {
            // always false for this version of gensr
            usedFallback = false;
            return GetString(name);
        }
#if !PORTABLELIB
        public static object GetObject(string name) {
            EntityRes sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetObject(name, EntityRes.Culture);
        }
#endif
    }
}

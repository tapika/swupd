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
    using System.Resources;

    /// <summary>
    ///    Strongly-typed and parameterized string resources.
    /// </summary>
    internal static class Strings {
        /// <summary>
        /// A string like "Unexpected primitive type kind."
        /// </summary>
        internal static string EdmPrimitive_UnexpectedKind {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmPrimitive_UnexpectedKind);
            }
        }

        /// <summary>
        /// A string like "Annotations in the 'Documentation' namespace must implement 'IEdmDocumentation', but '{0}' does not."
        /// </summary>
        internal static string Annotations_DocumentationPun(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Annotations_DocumentationPun,p0);
        }

        /// <summary>
        /// A string like "Annotation of type '{0}' cannot be interpreted as '{1}'."
        /// </summary>
        internal static string Annotations_TypeMismatch(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Annotations_TypeMismatch,p0,p1);
        }

        /// <summary>
        /// A string like "The annotation must have non-null target."
        /// </summary>
        internal static string Constructable_VocabularyAnnotationMustHaveTarget {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Constructable_VocabularyAnnotationMustHaveTarget);
            }
        }

        /// <summary>
        /// A string like "An entity type or a collection of an entity type is expected."
        /// </summary>
        internal static string Constructable_EntityTypeOrCollectionOfEntityTypeExpected {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Constructable_EntityTypeOrCollectionOfEntityTypeExpected);
            }
        }

        /// <summary>
        /// A string like "Navigation target entity type must be '{0}'."
        /// </summary>
        internal static string Constructable_TargetMustBeStock(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Constructable_TargetMustBeStock,p0);
        }

        /// <summary>
        /// A string like "The type '{0}' could not be converted to be a '{1}' type."
        /// </summary>
        internal static string TypeSemantics_CouldNotConvertTypeReference(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.TypeSemantics_CouldNotConvertTypeReference,p0,p1);
        }

        /// <summary>
        /// A string like "An element with type 'None' cannot be used in a model."
        /// </summary>
        internal static string EdmModel_CannotUseElementWithTypeNone {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_CannotUseElementWithTypeNone);
            }
        }

        /// <summary>
        /// A string like "An element with type 'None' cannot be used in an entity container."
        /// </summary>
        internal static string EdmEntityContainer_CannotUseElementWithTypeNone {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmEntityContainer_CannotUseElementWithTypeNone);
            }
        }

        /// <summary>
        /// A string like "The value writer cannot write a value of kind '{0}'."
        /// </summary>
        internal static string ValueWriter_NonSerializableValue(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueWriter_NonSerializableValue,p0);
        }

        /// <summary>
        /// A string like "Value has already been set."
        /// </summary>
        internal static string ValueHasAlreadyBeenSet {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueHasAlreadyBeenSet);
            }
        }

        /// <summary>
        /// A string like "Path segments must not contain '/' character."
        /// </summary>
        internal static string PathSegmentMustNotContainSlash {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.PathSegmentMustNotContainSlash);
            }
        }

        /// <summary>
        /// A string like "Type '{0}' must have a single type annotation with term type '{1}'."
        /// </summary>
        internal static string Edm_Evaluator_NoTermTypeAnnotationOnType(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Edm_Evaluator_NoTermTypeAnnotationOnType,p0,p1);
        }

        /// <summary>
        /// A string like "Type '{0}' must have a single value annotation with term '{1}'."
        /// </summary>
        internal static string Edm_Evaluator_NoValueAnnotationOnType(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Edm_Evaluator_NoValueAnnotationOnType,p0,p1);
        }

        /// <summary>
        /// A string like "Element must have a single value annotation with term '{0}'."
        /// </summary>
        internal static string Edm_Evaluator_NoValueAnnotationOnElement(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Edm_Evaluator_NoValueAnnotationOnElement,p0);
        }

        /// <summary>
        /// A string like "Expression with kind '{0}' cannot be evaluated."
        /// </summary>
        internal static string Edm_Evaluator_UnrecognizedExpressionKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Edm_Evaluator_UnrecognizedExpressionKind,p0);
        }

        /// <summary>
        /// A string like "Function '{0}' is not present in the execution environment."
        /// </summary>
        internal static string Edm_Evaluator_UnboundFunction(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Edm_Evaluator_UnboundFunction,p0);
        }

        /// <summary>
        /// A string like "Path segment '{0}' has no binding in the execution environment."
        /// </summary>
        internal static string Edm_Evaluator_UnboundPath(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Edm_Evaluator_UnboundPath,p0);
        }

        /// <summary>
        /// A string like "Value fails to match type '{0}'."
        /// </summary>
        internal static string Edm_Evaluator_FailedTypeAssertion(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Edm_Evaluator_FailedTypeAssertion,p0);
        }

        /// <summary>
        /// A string like "The namespace '{0}' is a system namespace and cannot be used by non-system types. Please choose a different namespace."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_SystemNamespaceEncountered(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_SystemNamespaceEncountered,p0);
        }

        /// <summary>
        /// A string like "The entity set '{0}' is based on type '{1}' that has no keys defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntitySetTypeHasNoKeys,p0,p1);
        }

        /// <summary>
        /// A string like "An end with the name '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DuplicateEndName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DuplicateEndName,p0);
        }

        /// <summary>
        /// A string like "The key specified in entity type '{0}' is not valid. Property '{1}' is referenced more than once in the key element."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DuplicatePropertyNameSpecifiedInEntityKey,p0,p1);
        }

        /// <summary>
        /// A string like "The complex type '{0}' is marked as abstract. Abstract complex types are only supported in version 1.1 EDM models."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidComplexTypeAbstract(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidComplexTypeAbstract,p0);
        }

        /// <summary>
        /// A string like "The complex type '{0}' has a base type specified. Complex type inheritance is only supported in version 1.1 EDM models."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidComplexTypePolymorphic,p0);
        }

        /// <summary>
        /// A string like "The key part '{0}' for type '{1}' is not valid. All parts of the key must be non nullable."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidKeyNullablePart(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidKeyNullablePart,p0,p1);
        }

        /// <summary>
        /// A string like "The property '{0}' in entity type '{1}' is not valid. All properties that are part of the entity key must be of primitive type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntityKeyMustBeScalar(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntityKeyMustBeScalar,p0,p1);
        }

        /// <summary>
        /// A string like "The key usage is not valid. '{0}' cannot define keys because one of its base classes '{1}' defines keys."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidKeyKeyDefinedInBaseClass,p0,p1);
        }

        /// <summary>
        /// A string like "The entity type '{0}' has no key defined. Define the key for this entity type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_KeyMissingOnEntityType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_KeyMissingOnEntityType,p0);
        }

        /// <summary>
        /// A string like "The navigation property '{0}' is not valid. The role '{1}' is not defined in relationship '{2}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole(object p0, object p1, object p2) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_BadNavigationPropertyUndefinedRole,p0,p1,p2);
        }

        /// <summary>
        /// A string like "The navigation property '{0}'is not valid. The from role and to role are the same."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_BadNavigationPropertyRolesCannotBeTheSame,p0);
        }

        /// <summary>
        /// A string like "The navigation property type could not be determined from the role '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_BadNavigationPropertyCouldNotDetermineType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_BadNavigationPropertyCouldNotDetermineType,p0);
        }

        /// <summary>
        /// A string like "An on delete action can only be specified on one end of an association."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidOperationMultipleEndsInAssociation);
            }
        }

        /// <summary>
        /// A string like "The navigation property '{0}' cannot have 'OnDelete' specified since its multiplicity is '*'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EndWithManyMultiplicityCannotHaveOperationsSpecified,p0);
        }

        /// <summary>
        /// A string like "Each name and plural name in a relationship must be unique. '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate,p0);
        }

        /// <summary>
        /// A string like "In relationship '{0}', the principal and dependent role of the referential constraint refers to the same role in the relationship type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_SameRoleReferredInReferentialConstraint,p0);
        }

        /// <summary>
        /// A string like "The principal navigation property '{0}' has an invalid multiplicity. Valid values for the multiplicity of a principal end are '0..1' or '1'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NavigationPropertyPrincipalEndMultiplicityUpperBoundMustBeOne(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NavigationPropertyPrincipalEndMultiplicityUpperBoundMustBeOne,p0);
        }

        /// <summary>
        /// A string like "The multiplicity of the principal end '{0}' is not valid. Because all dependent properties of the end '{1}' are non-nullable, the multiplicity of the principal end must be '1'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNonnullable(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNonnullable,p0,p1);
        }

        /// <summary>
        /// A string like "The multiplicity of the principal end '{0}' is not valid. Because all dependent properties of the end '{1}' are nullable, the multiplicity of the principal end must be '0..1'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNullable(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidMultiplicityOfPrincipalEndDependentPropertiesAllNullable,p0,p1);
        }

        /// <summary>
        /// A string like "The multiplicity of the dependent end '{0}' is not valid. Because the dependent properties represent the dependent end key, the multiplicity of the dependent end must be '0..1' or '1'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeZeroOneOrOne(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeZeroOneOrOne,p0);
        }

        /// <summary>
        /// A string like "The multiplicity of the dependent end '{0}' is not valid. Because the dependent properties don't represent the dependent end key, the the multiplicity of the dependent end must be '*'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeMany(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidMultiplicityOfDependentEndMustBeMany,p0);
        }

        /// <summary>
        /// A string like "The properties referred by the dependent role '{0}' must be a subset of the key of the entity type '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidToPropertyInRelationshipConstraint,p0,p1);
        }

        /// <summary>
        /// A string like "The number of properties in the dependent and principal role in a relationship constraint must be exactly identical."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_MismatchNumberOfPropertiesinRelationshipConstraint);
            }
        }

        /// <summary>
        /// A string like "The types of all properties in the dependent role of a referential constraint must be the same as the corresponding property types in the principal role. The type of property '{0}' on entity '{1}' does not match the type of property '{2}' on entity '{3}' in the referential constraint '{4}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint(object p0, object p1, object p2, object p3, object p4) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_TypeMismatchRelationshipConstraint,p0,p1,p2,p3,p4);
        }

        /// <summary>
        /// A string like "There is no property with name '{0}' defined in the type referred to by role '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintDependentEnd(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintDependentEnd,p0,p1);
        }

        /// <summary>
        /// A string like "The principal end properties in the referential constraint of the association '{0}' do not match the key of the type referred to by role '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintPrimaryEnd(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidPropertyInRelationshipConstraintPrimaryEnd,p0,p1);
        }

        /// <summary>
        /// A string like "The property '{0}' is of a complex type and is nullable. Nullable complex type properties are not supported in EDM versions 1.0 and 2.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NullableComplexTypeProperty(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NullableComplexTypeProperty,p0);
        }

        /// <summary>
        /// A string like "A property cannot be of type '{0}'. The property type must be a complex, a primitive or an enum type, or a collection of complex, primitive, or enum types."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidPropertyType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidPropertyType,p0);
        }

        /// <summary>
        /// A string like "The function import '{0}' cannot be composable and side-effecting at the same time."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ComposableFunctionImportCannotBeSideEffecting(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ComposableFunctionImportCannotBeSideEffecting,p0);
        }

        /// <summary>
        /// A string like "The bindable function import '{0}' must have at least one parameter."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_BindableFunctionImportMustHaveParameters(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_BindableFunctionImportMustHaveParameters,p0);
        }

        /// <summary>
        /// A string like "The return type is not valid in function import '{0}'. In version 1.0 a function import can have no return type or return a collection of scalar values or a collection of entities."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeV1(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeV1,p0);
        }

        /// <summary>
        /// A string like "The return type is not valid in function import '{0}'. The function import can have no return type or return a scalar, a complex type, an entity type or a collection of those."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeAfterV1(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportWithUnsupportedReturnTypeAfterV1,p0);
        }

        /// <summary>
        /// A string like "The function import '{0}' returns entities but does not specify an entity set."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet,p0);
        }

        /// <summary>
        /// A string like "The function import '{0}' returns entities of type '{1}' that cannot exist in the entity set '{2}' specified for the function import."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet(object p0, object p1, object p2) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet,p0,p1,p2);
        }

        /// <summary>
        /// A string like "The function import '{0}' returns entities of type '{1}' that cannot be returned by the entity set path specified for the function import."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet2(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportEntityTypeDoesNotMatchEntitySet2,p0,p1);
        }

        /// <summary>
        /// A string like "The function import '{0}' specifies an entity set expression of kind {1} which is not supported in this context. Function import entity set expression can be either an entity set reference or a path starting with a function import parameter and traversing navigation properties."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionKindIsInvalid(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionKindIsInvalid,p0,p1);
        }

        /// <summary>
        /// A string like "The function import '{0}' specifies an entity set expression which is not valid. Function import entity set expression can be either an entity set reference or a path starting with a function import parameter and traversing navigation properties."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionIsInvalid(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionIsInvalid,p0);
        }

        /// <summary>
        /// A string like "The function import '{0}' specifies an entity set but does not return entities."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportSpecifiesEntitySetButNotEntityType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportSpecifiesEntitySetButNotEntityType,p0);
        }

        /// <summary>
        /// A string like "The composable function import '{0}' must specify a return type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ComposableFunctionImportMustHaveReturnType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ComposableFunctionImportMustHaveReturnType,p0);
        }

        /// <summary>
        /// A string like "Each parameter name in a function must be unique. The parameter name '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ParameterNameAlreadyDefinedDuplicate(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ParameterNameAlreadyDefinedDuplicate,p0);
        }

        /// <summary>
        /// A string like "Each member name in an EntityContainer must be unique. A member with name '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName,p0);
        }

        /// <summary>
        /// A string like "An element with the name '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined,p0);
        }

        /// <summary>
        /// A string like "The member name '{0}' cannot be used in a type with the same name. Member names cannot be the same as their enclosing type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidMemberNameMatchesTypeName,p0);
        }

        /// <summary>
        /// A string like "Each property name in a type must be unique. Property name '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_PropertyNameAlreadyDefined(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_PropertyNameAlreadyDefined,p0);
        }

        /// <summary>
        /// A string like "The base type kind of a structured type must be the same as its derived type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_BaseTypeMustHaveSameTypeKind {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_BaseTypeMustHaveSameTypeKind);
            }
        }

        /// <summary>
        /// A string like "Row types cannot have a base type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_RowTypeMustNotHaveBaseType {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_RowTypeMustNotHaveBaseType);
            }
        }

        /// <summary>
        /// A string like "Functions are not supported prior to version 2.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionsNotSupportedBeforeV2 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionsNotSupportedBeforeV2);
            }
        }

        /// <summary>
        /// A string like "The 'SideEffecting' setting of function imports is not supported before version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportSideEffectingNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportSideEffectingNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "The 'Composable' setting of function imports is not supported before version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportComposableNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportComposableNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "The 'Bindable' setting of function imports is not supported before version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportBindableNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportBindableNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "The key property '{0}' must belong to the entity '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_KeyPropertyMustBelongToEntity(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_KeyPropertyMustBelongToEntity,p0,p1);
        }

        /// <summary>
        /// A string like "The dependent property '{0}' must belong to the dependent entity '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DependentPropertiesMustBelongToDependentEntity(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DependentPropertiesMustBelongToDependentEntity,p0,p1);
        }

        /// <summary>
        /// A string like "The property '{0}' cannot belong to a type other than its declaring type. "
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DeclaringTypeMustBeCorrect(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DeclaringTypeMustBeCorrect,p0);
        }

        /// <summary>
        /// A string like "The named type '{0}' could not be found from the model being validated."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InaccessibleType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InaccessibleType,p0);
        }

        /// <summary>
        /// A string like "The named type '{0}' is ambiguous from the model being validated."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_AmbiguousType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_AmbiguousType,p0);
        }

        /// <summary>
        /// A string like "The type of the navigation property '{0}' is invalid. The navigation target type must be an entity type or a collection of entity type. The navigation target entity type must match the declaring type of the partner property."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidNavigationPropertyType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidNavigationPropertyType,p0);
        }

        /// <summary>
        /// A string like "The target multiplicity of the navigation property '{0}' is invalid. If a navigation property has 'ContainsTarget' set to true and declaring entity type of the property is the same or inherits from the target entity type, then the property represents a recursive containment and it must have an optional target represented by a collection or a nullable entity type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentTargetMustBeOptional(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentTargetMustBeOptional,p0);
        }

        /// <summary>
        /// A string like "The source multiplicity of the navigation property '{0}' is invalid. If a navigation property has 'ContainsTarget' set to true and declaring entity type of the property is the same or inherits from the target entity type, then the property represents a recursive containment and the multiplicity of the navigation source must be zero or one."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NavigationPropertyWithRecursiveContainmentSourceMustBeFromZeroOrOne,p0);
        }

        /// <summary>
        /// A string like "The source multiplicity of the navigation property '{0}' is invalid. If a navigation property has 'ContainsTarget' set to true and declaring entity type of the property is not the same as the target entity type, then the property represents a non-recursive containment and the multiplicity of the navigation source must be exactly one."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NavigationPropertyWithNonRecursiveContainmentSourceMustBeFromOne,p0);
        }

        /// <summary>
        /// A string like "The 'ContainsTarget' setting of navigation properties is not supported before version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NavigationPropertyContainsTargetNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NavigationPropertyContainsTargetNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "The mode of the parameter '{0}' in the function '{1}' is invalid. Only input parameters are allowed in functions."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_OnlyInputParametersAllowedInFunctions(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_OnlyInputParametersAllowedInFunctions,p0,p1);
        }

        /// <summary>
        /// A string like "The mode of the parameter '{0}' in the function import '{1}' is invalid."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidFunctionImportParameterMode(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidFunctionImportParameterMode,p0,p1);
        }

        /// <summary>
        /// A string like "The type '{0}' of parameter '{1}' is invalid. A function import parameter must be one of the following types: A simple type or complex type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_FunctionImportParameterIncorrectType(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_FunctionImportParameterIncorrectType,p0,p1);
        }

        /// <summary>
        /// A string like "The row type is invalid. A row must contain at least one property."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_RowTypeMustHaveProperties {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_RowTypeMustHaveProperties);
            }
        }

        /// <summary>
        /// A string like "The complex type '{0}' is invalid. A complex type must contain at least one property."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ComplexTypeMustHaveProperties(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ComplexTypeMustHaveProperties,p0);
        }

        /// <summary>
        /// A string like "The dependent property '{0}' of navigation property '{1}' is a duplicate."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DuplicateDependentProperty(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DuplicateDependentProperty,p0,p1);
        }

        /// <summary>
        /// A string like "The scale value can range from 0 through the specified precision value."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ScaleOutOfRange {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ScaleOutOfRange);
            }
        }

        /// <summary>
        /// A string like "Precision cannot be negative."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_PrecisionOutOfRange {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_PrecisionOutOfRange);
            }
        }

        /// <summary>
        /// A string like "The max length facet specifies the maximum length of an instance of the string type. For unicode equal to 'true', the max length can range from 1 to 2^30, or if 'false', 1 to 2^31."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_StringMaxLengthOutOfRange {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_StringMaxLengthOutOfRange);
            }
        }

        /// <summary>
        /// A string like "Max length can range from 1 to 2^31."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_MaxLengthOutOfRange {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_MaxLengthOutOfRange);
            }
        }

        /// <summary>
        /// A string like "A property with a fixed concurrency mode cannot be of type '{0}'. The property type must be a primitive type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidPropertyTypeConcurrencyMode(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidPropertyTypeConcurrencyMode,p0);
        }

        /// <summary>
        /// A string like "The property '{0}' in entity type '{1}' is not valid. Binary types are not allowed in entity keys before version 2.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntityKeyMustNotBeBinaryBeforeV2(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntityKeyMustNotBeBinaryBeforeV2,p0,p1);
        }

        /// <summary>
        /// A string like "Enums are not supported prior to version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EnumsNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EnumsNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "The type of the value of enum member '{0}' must match the underlying type of the parent enum."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EnumMemberTypeMustMatchEnumUnderlyingType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EnumMemberTypeMustMatchEnumUnderlyingType,p0);
        }

        /// <summary>
        /// A string like "Each member name of an enum type must be unique. Enum member name '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EnumMemberNameAlreadyDefined(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EnumMemberNameAlreadyDefined,p0);
        }

        /// <summary>
        /// A string like "Value terms are not supported prior to version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ValueTermsNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ValueTermsNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "Vocabulary annotations are not supported prior to version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_VocabularyAnnotationsNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_VocabularyAnnotationsNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "Open types are supported only in version 1.2 and after version 2.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_OpenTypesSupportedOnlyInV12AndAfterV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_OpenTypesSupportedOnlyInV12AndAfterV3);
            }
        }

        /// <summary>
        /// A string like "Only entity types can be open types."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_OpenTypesSupportedForEntityTypesOnly {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_OpenTypesSupportedForEntityTypesOnly);
            }
        }

        /// <summary>
        /// A string like "The string reference is invalid because if 'IsUnbounded' is true 'MaxLength' must be null."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull);
            }
        }

        /// <summary>
        /// A string like "The declared name and namespace of the annotation must match the name and namespace of its xml value."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidElementAnnotationMismatchedTerm {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidElementAnnotationMismatchedTerm);
            }
        }

        /// <summary>
        /// A string like "The value of an annotation marked to be serialized as an xml element must have a well-formed xml value."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidElementAnnotationValueInvalidXml {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidElementAnnotationValueInvalidXml);
            }
        }

        /// <summary>
        /// A string like "The value of an annotation marked to be serialized as an xml element must be IEdmStringValue."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidElementAnnotationNotIEdmStringValue {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidElementAnnotationNotIEdmStringValue);
            }
        }

        /// <summary>
        /// A string like "The value of an annotation marked to be serialized as an xml element must be a string representing an xml element with non-empty name and namespace."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InvalidElementAnnotationNullNamespaceOrName {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InvalidElementAnnotationNullNamespaceOrName);
            }
        }

        /// <summary>
        /// A string like "Cannot assert the nullable type '{0}' as a non-nullable type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_CannotAssertNullableTypeAsNonNullableType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_CannotAssertNullableTypeAsNonNullableType,p0);
        }

        /// <summary>
        /// A string like "Cannot promote the primitive type '{0}' to the specified primitive type '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ExpressionPrimitiveKindCannotPromoteToAssertedType(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ExpressionPrimitiveKindCannotPromoteToAssertedType,p0,p1);
        }

        /// <summary>
        /// A string like "Null value cannot have a non-nullable type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NullCannotBeAssertedToBeANonNullableType {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NullCannotBeAssertedToBeANonNullableType);
            }
        }

        /// <summary>
        /// A string like "The type of the expression is incompatible with the asserted type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ExpressionNotValidForTheAssertedType {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ExpressionNotValidForTheAssertedType);
            }
        }

        /// <summary>
        /// A string like "A collection expression is incompatible with a non-collection type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_CollectionExpressionNotValidForNonCollectionType {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_CollectionExpressionNotValidForNonCollectionType);
            }
        }

        /// <summary>
        /// A string like "A primitive expression is incompatible with a non-primitive type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_PrimitiveConstantExpressionNotValidForNonPrimitiveType {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_PrimitiveConstantExpressionNotValidForNonPrimitiveType);
            }
        }

        /// <summary>
        /// A string like "A record expression is incompatible with a non-structured type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_RecordExpressionNotValidForNonStructuredType {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_RecordExpressionNotValidForNonStructuredType);
            }
        }

        /// <summary>
        /// A string like "The record expression does not have a constructor for a property named '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_RecordExpressionMissingProperty(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_RecordExpressionMissingProperty,p0);
        }

        /// <summary>
        /// A string like "The type of the record expression is not open and does not contain a property named '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_RecordExpressionHasExtraProperties(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_RecordExpressionHasExtraProperties,p0);
        }

        /// <summary>
        /// A string like "The annotated element '{0}' has multiple annotations with the term '{1}' and the qualifier '{2}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DuplicateAnnotation(object p0, object p1, object p2) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DuplicateAnnotation,p0,p1,p2);
        }

        /// <summary>
        /// A string like "The function application provides '{0}' arguments, but the function '{1}' expects '{2}' arguments."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_IncorrectNumberOfArguments(object p0, object p1, object p2) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_IncorrectNumberOfArguments,p0,p1,p2);
        }

        /// <summary>
        /// A string like "References to EDM stream type are not supported before version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_StreamTypeReferencesNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_StreamTypeReferencesNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "References to EDM spatial types are not supported before version 3.0."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_SpatialTypeReferencesNotSupportedBeforeV3 {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_SpatialTypeReferencesNotSupportedBeforeV3);
            }
        }

        /// <summary>
        /// A string like "Each entity container name in a function must be unique. The name '{0}' is already defined."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DuplicateEntityContainerName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DuplicateEntityContainerName,p0);
        }

        /// <summary>
        /// A string like "The primitive expression is not compatible with the asserted type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ExpressionPrimitiveKindNotValidForAssertedType);
            }
        }

        /// <summary>
        /// A string like "The value of the integer constant is out of range for the asserted type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_IntegerConstantValueOutOfRange {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_IntegerConstantValueOutOfRange);
            }
        }

        /// <summary>
        /// A string like "The value of the string constant is '{0}' characters long, but the max length of its type is '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_StringConstantLengthOutOfRange(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_StringConstantLengthOutOfRange,p0,p1);
        }

        /// <summary>
        /// A string like "The value of the binary constant is '{0}' characters long, but the max length of its type is '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_BinaryConstantLengthOutOfRange(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_BinaryConstantLengthOutOfRange,p0,p1);
        }

        /// <summary>
        /// A string like "A type without other errors must not have kind of none."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_TypeMustNotHaveKindOfNone {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_TypeMustNotHaveKindOfNone);
            }
        }

        /// <summary>
        /// A string like "A term without other errors must not have kind of none. The kind of term '{0}' is none."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_TermMustNotHaveKindOfNone(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_TermMustNotHaveKindOfNone,p0);
        }

        /// <summary>
        /// A string like "A schema element without other errors must not have kind of none. The kind of schema element '{0}' is none."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_SchemaElementMustNotHaveKindOfNone(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_SchemaElementMustNotHaveKindOfNone,p0);
        }

        /// <summary>
        /// A string like "A property without other errors must not have kind of none. The kind of property '{0}' is none."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_PropertyMustNotHaveKindOfNone(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_PropertyMustNotHaveKindOfNone,p0);
        }

        /// <summary>
        /// A string like "A primitive type without other errors must not have kind of none. The kind of primitive type '{0}' is none."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_PrimitiveTypeMustNotHaveKindOfNone(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_PrimitiveTypeMustNotHaveKindOfNone,p0);
        }

        /// <summary>
        /// A string like "An entity container element without other errors must not have kind of none. The kind of entity container element '{0}' is none."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntityContainerElementMustNotHaveKindOfNone(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntityContainerElementMustNotHaveKindOfNone,p0);
        }

        /// <summary>
        /// A string like "The entity set '{0}' should have only a single mapping for the property '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_DuplicateNavigationPropertyMapping(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_DuplicateNavigationPropertyMapping,p0,p1);
        }

        /// <summary>
        /// A string like "The mapping of the entity set '{0}' and navigation property '{1}' is invalid because the navigation property mapping must have a mapping with the navigation property's partner that points back to the originating entity set. "
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntitySetNavigationMappingMustBeBidirectional(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntitySetNavigationMappingMustBeBidirectional,p0,p1);
        }

        /// <summary>
        /// A string like "The entity set '{0}' is invalid because it is contained by more than one navigation property."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntitySetCanOnlyBeContainedByASingleNavigationProperty(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntitySetCanOnlyBeContainedByASingleNavigationProperty,p0);
        }

        /// <summary>
        /// A string like "The type annotation is missing a binding for the property '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_TypeAnnotationMissingRequiredProperty(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_TypeAnnotationMissingRequiredProperty,p0);
        }

        /// <summary>
        /// A string like "They type of the type annotation is not open, and does not contain a property named '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_TypeAnnotationHasExtraProperties(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_TypeAnnotationHasExtraProperties,p0);
        }

        /// <summary>
        /// A string like "The underlying type of '{0}' is not valid. The underlying type of an enum type must be an integral type. "
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EnumMustHaveIntegralUnderlyingType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EnumMustHaveIntegralUnderlyingType,p0);
        }

        /// <summary>
        /// A string like "The term '{0}' could not be found from the model being validated."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InaccessibleTerm(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InaccessibleTerm,p0);
        }

        /// <summary>
        /// A string like "The target '{0}' could not be found from the model being validated."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_InaccessibleTarget(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_InaccessibleTarget,p0);
        }

        /// <summary>
        /// A string like "An element already has a direct value annotation with the namespace '{0}' and name '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_ElementDirectValueAnnotationFullNameMustBeUnique(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_ElementDirectValueAnnotationFullNameMustBeUnique,p0,p1);
        }

        /// <summary>
        /// A string like "The association set '{0}' cannot assume an entity set for the role '{2}' because there are no entity sets for the role type '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NoEntitySetsFoundForType(object p0, object p1, object p2) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NoEntitySetsFoundForType,p0,p1,p2);
        }

        /// <summary>
        /// A string like "The association set '{0}' must specify an entity set for the role '{2}' because there are multiple entity sets for the role type '{1}'."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_CannotInferEntitySetWithMultipleSetsPerType(object p0, object p1, object p2) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_CannotInferEntitySetWithMultipleSetsPerType,p0,p1,p2);
        }

        /// <summary>
        /// A string like "Because the navigation property '{0}' is recursive, the mapping from the entity set '{1}' must point back to itself."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntitySetRecursiveNavigationPropertyMappingsMustPointBackToSourceEntitySet,p0,p1);
        }

        /// <summary>
        /// A string like "The navigation property '{0}' is invalid because it indirectly contains itself."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_NavigationPropertyEntityMustNotIndirectlyContainItself(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_NavigationPropertyEntityMustNotIndirectlyContainItself,p0);
        }

        /// <summary>
        /// A string like "The path cannot be resolved in the given context. The segment '{0}' failed to resolve."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_PathIsNotValidForTheGivenContext(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_PathIsNotValidForTheGivenContext,p0);
        }

        /// <summary>
        /// A string like "The entity set '{1}' is not a valid destination for the navigation property '{0}' because it cannot hold an element of the target entity type."
        /// </summary>
        internal static string EdmModel_Validator_Semantic_EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Semantic_EntitySetNavigationPropertyMappingMustPointToValidTargetForProperty,p0,p1);
        }

        /// <summary>
        /// A string like "The name is missing or not valid."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_MissingName {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_MissingName);
            }
        }

        /// <summary>
        /// A string like "The specified name must not be longer than 480 characters: '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_EdmModel_NameIsTooLong,p0);
        }

        /// <summary>
        /// A string like "The specified name is not allowed: '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_EdmModel_NameIsNotAllowed,p0);
        }

        /// <summary>
        /// A string like "The namespace name is missing or not valid."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_MissingNamespaceName {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_MissingNamespaceName);
            }
        }

        /// <summary>
        /// A string like "The specified name must not be longer than 480 characters: '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsTooLong(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsTooLong,p0);
        }

        /// <summary>
        /// A string like "The specified namespace name is not allowed: '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsNotAllowed(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_EdmModel_NamespaceNameIsNotAllowed,p0);
        }

        /// <summary>
        /// A string like "The value of the property '{0}.{1}' must not be null."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_PropertyMustNotBeNull(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_PropertyMustNotBeNull,p0,p1);
        }

        /// <summary>
        /// A string like "The property '{0}.{1}' of type '{2}' has value '{3}' that is not a valid enum member."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_EnumPropertyValueOutOfRange(object p0, object p1, object p2, object p3) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_EnumPropertyValueOutOfRange,p0,p1,p2,p3);
        }

        /// <summary>
        /// A string like "An object with the value '{0}' of the '{1}.{2}' property must implement '{3}' interface."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_InterfaceKindValueMismatch(object p0, object p1, object p2, object p3) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_InterfaceKindValueMismatch,p0,p1,p2,p3);
        }

        /// <summary>
        /// A string like "An object implementing '{0}' interface has type definition of kind '{1}'. The type reference interface must match to the kind of the  definition."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch,p0,p1);
        }

        /// <summary>
        /// A string like "The value '{0}' of the property '{1}.{2}' is not semantically valid. A semantically valid model must not contain elements of kind '{0}'."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_InterfaceKindValueUnexpected(object p0, object p1, object p2) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_InterfaceKindValueUnexpected,p0,p1,p2);
        }

        /// <summary>
        /// A string like "The value of the enumeration the property '{0}.{1}' contains a null element. Enumeration properties must not contain null elements."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_EnumerableMustNotHaveNullElements(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_EnumerableMustNotHaveNullElements,p0,p1);
        }

        /// <summary>
        /// A string like "The partner of the navigation property '{0}' must not be the same property, and must point back to the navigation property."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_NavigationPartnerInvalid(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_NavigationPartnerInvalid,p0);
        }

        /// <summary>
        /// A string like "The chain of base types of type '{0}' is cyclic."
        /// </summary>
        internal static string EdmModel_Validator_Syntactic_InterfaceCriticalCycleInTypeHierarchy(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmModel_Validator_Syntactic_InterfaceCriticalCycleInTypeHierarchy,p0);
        }

        /// <summary>
        /// A string like "Single file provided but model cannot be serialized into single file."
        /// </summary>
        internal static string Serializer_SingleFileExpected {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Serializer_SingleFileExpected);
            }
        }

        /// <summary>
        /// A string like "Unknown Edm version."
        /// </summary>
        internal static string Serializer_UnknownEdmVersion {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Serializer_UnknownEdmVersion);
            }
        }

        /// <summary>
        /// A string like "Unknown Edmx version."
        /// </summary>
        internal static string Serializer_UnknownEdmxVersion {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Serializer_UnknownEdmxVersion);
            }
        }

        /// <summary>
        /// A string like "The function import '{0}' could not be serialized because its return type cannot be represented inline."
        /// </summary>
        internal static string Serializer_NonInlineFunctionImportReturnType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Serializer_NonInlineFunctionImportReturnType,p0);
        }

        /// <summary>
        /// A string like "A referenced type can not be serialized with an invalid name. The name '{0}' is invalid."
        /// </summary>
        internal static string Serializer_ReferencedTypeMustHaveValidName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Serializer_ReferencedTypeMustHaveValidName,p0);
        }

        /// <summary>
        /// A string like "The annotation can not be serialized with an invalid target name. The name '{0}' is invalid."
        /// </summary>
        internal static string Serializer_OutOfLineAnnotationTargetMustHaveValidName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Serializer_OutOfLineAnnotationTargetMustHaveValidName,p0);
        }

        /// <summary>
        /// A string like "No CSDL is written because no schema elements could be produced. This is likely because the model is empty."
        /// </summary>
        internal static string Serializer_NoSchemasProduced {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Serializer_NoSchemasProduced);
            }
        }

        /// <summary>
        /// A string like "{0} does not contain a schema definition, or the XmlReader provided started at the end of the file."
        /// </summary>
        internal static string XmlParser_EmptyFile(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_EmptyFile,p0);
        }

        /// <summary>
        /// A string like "The source XmlReader does not contain a schema definition or started at the end of the file."
        /// </summary>
        internal static string XmlParser_EmptySchemaTextReader {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_EmptySchemaTextReader);
            }
        }

        /// <summary>
        /// A string like "Required schema attribute '{0}' is not present on element '{1}'."
        /// </summary>
        internal static string XmlParser_MissingAttribute(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_MissingAttribute,p0,p1);
        }

        /// <summary>
        /// A string like "The current schema element does not support text '{0}'."
        /// </summary>
        internal static string XmlParser_TextNotAllowed(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_TextNotAllowed,p0);
        }

        /// <summary>
        /// A string like "The attribute '{0}' was not expected in the given context."
        /// </summary>
        internal static string XmlParser_UnexpectedAttribute(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_UnexpectedAttribute,p0);
        }

        /// <summary>
        /// A string like "The schema element '{0}' was not expected in the given context."
        /// </summary>
        internal static string XmlParser_UnexpectedElement(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_UnexpectedElement,p0);
        }

        /// <summary>
        /// A string like "Unused schema element: '{0}'."
        /// </summary>
        internal static string XmlParser_UnusedElement(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_UnusedElement,p0);
        }

        /// <summary>
        /// A string like "Unexpected XML node type: {0}."
        /// </summary>
        internal static string XmlParser_UnexpectedNodeType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_UnexpectedNodeType,p0);
        }

        /// <summary>
        /// A string like "The element '{0}' was unexpected for the root element. The root element should be {1}."
        /// </summary>
        internal static string XmlParser_UnexpectedRootElement(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_UnexpectedRootElement,p0,p1);
        }

        /// <summary>
        /// A string like "The namespace '{0}' is invalid. The root element is expected to belong to one of the following namespaces: '{1}'."
        /// </summary>
        internal static string XmlParser_UnexpectedRootElementWrongNamespace(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_UnexpectedRootElementWrongNamespace,p0,p1);
        }

        /// <summary>
        /// A string like "The root element has no namespace. The root element is expected to belong to one of the following namespaces: '{0}'."
        /// </summary>
        internal static string XmlParser_UnexpectedRootElementNoNamespace(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.XmlParser_UnexpectedRootElementNoNamespace,p0);
        }

        /// <summary>
        /// A string like "The alias '{0}' is not a valid simple name."
        /// </summary>
        internal static string CsdlParser_InvalidAlias(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidAlias,p0);
        }

        /// <summary>
        /// A string like "Associations may have at most one constraint. Multiple constraints were specified for this association."
        /// </summary>
        internal static string CsdlParser_AssociationHasAtMostOneConstraint {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_AssociationHasAtMostOneConstraint);
            }
        }

        /// <summary>
        /// A string like "The delete action '{0}' is not valid. Action must be: 'None', 'Cascade', or 'Restrict'."
        /// </summary>
        internal static string CsdlParser_InvalidDeleteAction(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidDeleteAction,p0);
        }

        /// <summary>
        /// A string like "An XML attribute or sub-element representing an EDM type is missing."
        /// </summary>
        internal static string CsdlParser_MissingTypeAttributeOrElement {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_MissingTypeAttributeOrElement);
            }
        }

        /// <summary>
        /// A string like "The association '{0}' is not valid. Associations must contain exactly two end elements."
        /// </summary>
        internal static string CsdlParser_InvalidAssociationIncorrectNumberOfEnds(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidAssociationIncorrectNumberOfEnds,p0);
        }

        /// <summary>
        /// A string like "The association set '{0}' is not valid. Association sets must contain at most two end elements."
        /// </summary>
        internal static string CsdlParser_InvalidAssociationSetIncorrectNumberOfEnds(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidAssociationSetIncorrectNumberOfEnds,p0);
        }

        /// <summary>
        /// A string like "The concurrency mode '{0}' is not valid. Concurrency mode must be: 'None', or 'Fixed'."
        /// </summary>
        internal static string CsdlParser_InvalidConcurrencyMode(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidConcurrencyMode,p0);
        }

        /// <summary>
        /// A string like "Parameter mode '{0}' is not valid. Parameter mode must be: 'In', 'Out', or 'InOut'."
        /// </summary>
        internal static string CsdlParser_InvalidParameterMode(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidParameterMode,p0);
        }

        /// <summary>
        /// A string like "There is no Role with name '{0}' defined in relationship '{1}'."
        /// </summary>
        internal static string CsdlParser_InvalidEndRoleInRelationshipConstraint(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidEndRoleInRelationshipConstraint,p0,p1);
        }

        /// <summary>
        /// A string like "The multiplicity '{0}' is not valid. Multiplicity must be: '*', '0..1', or '1'."
        /// </summary>
        internal static string CsdlParser_InvalidMultiplicity(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidMultiplicity,p0);
        }

        /// <summary>
        /// A string like "Referential constraints requires one dependent role. Multiple dependent roles were specified for this referential constraint."
        /// </summary>
        internal static string CsdlParser_ReferentialConstraintRequiresOneDependent {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_ReferentialConstraintRequiresOneDependent);
            }
        }

        /// <summary>
        /// A string like "Referential constraints requires one principal role. Multiple principal roles were specified for this referential constraint."
        /// </summary>
        internal static string CsdlParser_ReferentialConstraintRequiresOnePrincipal {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_ReferentialConstraintRequiresOnePrincipal);
            }
        }

        /// <summary>
        /// A string like "If expression must contain 3 operands, the first being a boolean test, the second being being evaluated if the first is true, and the third being evaluated if the first is false."
        /// </summary>
        internal static string CsdlParser_InvalidIfExpressionIncorrectNumberOfOperands {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidIfExpressionIncorrectNumberOfOperands);
            }
        }

        /// <summary>
        /// A string like "The IsType expression must contain 1 operand."
        /// </summary>
        internal static string CsdlParser_InvalidIsTypeExpressionIncorrectNumberOfOperands {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidIsTypeExpressionIncorrectNumberOfOperands);
            }
        }

        /// <summary>
        /// A string like "The AssertType expression must contain 1 operand."
        /// </summary>
        internal static string CsdlParser_InvalidAssertTypeExpressionIncorrectNumberOfOperands {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidAssertTypeExpressionIncorrectNumberOfOperands);
            }
        }

        /// <summary>
        /// A string like "The LabeledElement expression must contain 1 operand."
        /// </summary>
        internal static string CsdlParser_InvalidLabeledElementExpressionIncorrectNumberOfOperands {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidLabeledElementExpressionIncorrectNumberOfOperands);
            }
        }

        /// <summary>
        /// A string like "The type name '{0}' is invalid. The type name must be that of a primitive type, a fully qualified name or an inline 'Collection' or 'Ref' type."
        /// </summary>
        internal static string CsdlParser_InvalidTypeName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidTypeName,p0);
        }

        /// <summary>
        /// A string like "The qualified name '{0}' is invalid. A qualified name must have a valid namespace or alias, and a valid name."
        /// </summary>
        internal static string CsdlParser_InvalidQualifiedName(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidQualifiedName,p0);
        }

        /// <summary>
        /// A string like "A model could not be produced because no XML readers were provided."
        /// </summary>
        internal static string CsdlParser_NoReadersProvided {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_NoReadersProvided);
            }
        }

        /// <summary>
        /// A string like "A model could not be produced because one of the XML readers was null."
        /// </summary>
        internal static string CsdlParser_NullXmlReader {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_NullXmlReader);
            }
        }

        /// <summary>
        /// A string like "'{0}' is not a valid entity set path."
        /// </summary>
        internal static string CsdlParser_InvalidEntitySetPath(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidEntitySetPath,p0);
        }

        /// <summary>
        /// A string like "'{0}' is not a valid enum member path."
        /// </summary>
        internal static string CsdlParser_InvalidEnumMemberPath(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlParser_InvalidEnumMemberPath,p0);
        }

        /// <summary>
        /// A string like " There was a mismatch in the principal and dependent ends of the referential constraint."
        /// </summary>
        internal static string CsdlSemantics_ReferentialConstraintMismatch {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlSemantics_ReferentialConstraintMismatch);
            }
        }

        /// <summary>
        /// A string like "The enumeration member value exceeds the range of its data type 'http://www.w3.org/2001/XMLSchema:long'."
        /// </summary>
        internal static string CsdlSemantics_EnumMemberValueOutOfRange {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlSemantics_EnumMemberValueOutOfRange);
            }
        }

        /// <summary>
        /// A string like "The annotation target '{0}' could not be resolved because it cannot refer to an annotatable element."
        /// </summary>
        internal static string CsdlSemantics_ImpossibleAnnotationsTarget(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlSemantics_ImpossibleAnnotationsTarget,p0);
        }

        /// <summary>
        /// A string like "The schema '{0}' contains the alias '{1}' more than once."
        /// </summary>
        internal static string CsdlSemantics_DuplicateAlias(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.CsdlSemantics_DuplicateAlias,p0,p1);
        }

        /// <summary>
        /// A string like "The EDMX version specified in the 'Version' attribute does not match the version corresponding to the namespace of the 'Edmx' element."
        /// </summary>
        internal static string EdmxParser_EdmxVersionMismatch {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmxParser_EdmxVersionMismatch);
            }
        }

        /// <summary>
        /// A string like "The specified value of data service version is invalid."
        /// </summary>
        internal static string EdmxParser_EdmxDataServiceVersionInvalid {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmxParser_EdmxDataServiceVersionInvalid);
            }
        }

        /// <summary>
        /// A string like "The specified value of max data service version is invalid."
        /// </summary>
        internal static string EdmxParser_EdmxMaxDataServiceVersionInvalid {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmxParser_EdmxMaxDataServiceVersionInvalid);
            }
        }

        /// <summary>
        /// A string like "Unexpected {0} element while parsing Edmx. Edmx is expected to have at most one of 'Runtime' or 'DataServices' elements."
        /// </summary>
        internal static string EdmxParser_BodyElement(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmxParser_BodyElement,p0);
        }

        /// <summary>
        /// A string like "Encountered the following errors when parsing the EDMX document: \r\n{0}"
        /// </summary>
        internal static string EdmParseException_ErrorsEncounteredInEdmx(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmParseException_ErrorsEncounteredInEdmx,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid boolean. The value must be 'true' or 'false'."
        /// </summary>
        internal static string ValueParser_InvalidBoolean(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidBoolean,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid integer. The value must be a valid 32 bit integer."
        /// </summary>
        internal static string ValueParser_InvalidInteger(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidInteger,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid integer. The value must be a valid 64 bit integer."
        /// </summary>
        internal static string ValueParser_InvalidLong(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidLong,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid floating point value. "
        /// </summary>
        internal static string ValueParser_InvalidFloatingPoint(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidFloatingPoint,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid integer. The value must be a valid 32 bit integer or 'Max'."
        /// </summary>
        internal static string ValueParser_InvalidMaxLength(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidMaxLength,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid SRID. The value must either be a 32 bit integer or 'Variable'."
        /// </summary>
        internal static string ValueParser_InvalidSrid(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidSrid,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid Guid. "
        /// </summary>
        internal static string ValueParser_InvalidGuid(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidGuid,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid decimal."
        /// </summary>
        internal static string ValueParser_InvalidDecimal(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidDecimal,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid date time offset value."
        /// </summary>
        internal static string ValueParser_InvalidDateTimeOffset(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidDateTimeOffset,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid date time value."
        /// </summary>
        internal static string ValueParser_InvalidDateTime(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidDateTime,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid time value."
        /// </summary>
        internal static string ValueParser_InvalidTime(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidTime,p0);
        }

        /// <summary>
        /// A string like "The value '{0}' is not a valid binary value. The value must be a hexadecimal string and must not be prefixed by '0x'."
        /// </summary>
        internal static string ValueParser_InvalidBinary(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.ValueParser_InvalidBinary,p0);
        }

        /// <summary>
        /// A string like "Invalid multiplicity: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_Multiplicity(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_Multiplicity,p0);
        }

        /// <summary>
        /// A string like "Invalid schema element kind: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_SchemaElementKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_SchemaElementKind,p0);
        }

        /// <summary>
        /// A string like "Invalid type kind: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_TypeKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_TypeKind,p0);
        }

        /// <summary>
        /// A string like "Invalid primitive kind: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_PrimitiveKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_PrimitiveKind,p0);
        }

        /// <summary>
        /// A string like "Invalid container element kind: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_ContainerElementKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_ContainerElementKind,p0);
        }

        /// <summary>
        /// A string like "Invalid edmx target: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_EdmxTarget(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_EdmxTarget,p0);
        }

        /// <summary>
        /// A string like "Invalid function parameter mode: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_FunctionParameterMode(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_FunctionParameterMode,p0);
        }

        /// <summary>
        /// A string like "Invalid concurrency mode: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_ConcurrencyMode(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_ConcurrencyMode,p0);
        }

        /// <summary>
        /// A string like "Invalid property kind: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_PropertyKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_PropertyKind,p0);
        }

        /// <summary>
        /// A string like "Invalid term kind: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_TermKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_TermKind,p0);
        }

        /// <summary>
        /// A string like "Invalid expression kind: '{0}'"
        /// </summary>
        internal static string UnknownEnumVal_ExpressionKind(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.UnknownEnumVal_ExpressionKind,p0);
        }

        /// <summary>
        /// A string like "The name '{0}' is ambiguous."
        /// </summary>
        internal static string Bad_AmbiguousElementBinding(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_AmbiguousElementBinding,p0);
        }

        /// <summary>
        /// A string like "The type '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedType,p0);
        }

        /// <summary>
        /// A string like "The complex type '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedComplexType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedComplexType,p0);
        }

        /// <summary>
        /// A string like "The entity type '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedEntityType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedEntityType,p0);
        }

        /// <summary>
        /// A string like "The primitive type '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedPrimitiveType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedPrimitiveType,p0);
        }

        /// <summary>
        /// A string like "The function '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedFunction(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedFunction,p0);
        }

        /// <summary>
        /// A string like "The function '{0}' could not be resolved because more than one function could be used for this application."
        /// </summary>
        internal static string Bad_AmbiguousFunction(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_AmbiguousFunction,p0);
        }

        /// <summary>
        /// A string like "The function '{0}' could not be resolved because none of the functions with that name take the correct set of parameters."
        /// </summary>
        internal static string Bad_FunctionParametersDontMatch(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_FunctionParametersDontMatch,p0);
        }

        /// <summary>
        /// A string like "The entity set '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedEntitySet(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedEntitySet,p0);
        }

        /// <summary>
        /// A string like "The entity container '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedEntityContainer(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedEntityContainer,p0);
        }

        /// <summary>
        /// A string like "The enum type '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedEnumType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedEnumType,p0);
        }

        /// <summary>
        /// A string like "The enum member '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedEnumMember(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedEnumMember,p0);
        }

        /// <summary>
        /// A string like "The property '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedProperty(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedProperty,p0);
        }

        /// <summary>
        /// A string like "The parameter '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedParameter(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedParameter,p0);
        }

        /// <summary>
        /// A string like "The labeled element '{0}' could not be found."
        /// </summary>
        internal static string Bad_UnresolvedLabeledElement(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UnresolvedLabeledElement,p0);
        }

        /// <summary>
        /// A string like "The entity '{0}' is invalid because its base type is cyclic."
        /// </summary>
        internal static string Bad_CyclicEntity(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_CyclicEntity,p0);
        }

        /// <summary>
        /// A string like "The complex type '{0}' is invalid because its base type is cyclic."
        /// </summary>
        internal static string Bad_CyclicComplex(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_CyclicComplex,p0);
        }

        /// <summary>
        /// A string like "The entity container '{0}' is invalid because its extends hierarchy is cyclic."
        /// </summary>
        internal static string Bad_CyclicEntityContainer(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_CyclicEntityContainer,p0);
        }

        /// <summary>
        /// A string like "The association end '{0}' could not be computed."
        /// </summary>
        internal static string Bad_UncomputableAssociationEnd(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.Bad_UncomputableAssociationEnd,p0);
        }

        /// <summary>
        /// A string like "The same rule cannot be in the same rule set twice."
        /// </summary>
        internal static string RuleSet_DuplicateRulesExistInRuleSet {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.RuleSet_DuplicateRulesExistInRuleSet);
            }
        }

        /// <summary>
        /// A string like "Conversion of EDM values to a CLR type with type code {0} is not supported."
        /// </summary>
        internal static string EdmToClr_UnsupportedTypeCode(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmToClr_UnsupportedTypeCode,p0);
        }

        /// <summary>
        /// A string like "Conversion of an EDM structured value is supported only to a CLR class."
        /// </summary>
        internal static string EdmToClr_StructuredValueMappedToNonClass {
            get {
                return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmToClr_StructuredValueMappedToNonClass);
            }
        }

        /// <summary>
        /// A string like "Cannot initialize a property '{0}' on an object of type '{1}'. The property already has a value."
        /// </summary>
        internal static string EdmToClr_IEnumerableOfTPropertyAlreadyHasValue(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmToClr_IEnumerableOfTPropertyAlreadyHasValue,p0,p1);
        }

        /// <summary>
        /// A string like "An EDM structured value contains multiple values for the property '{0}'. Conversion of an EDM structured value with duplicate property values is not supported."
        /// </summary>
        internal static string EdmToClr_StructuredPropertyDuplicateValue(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmToClr_StructuredPropertyDuplicateValue,p0);
        }

        /// <summary>
        /// A string like "Conversion of an EDM value of the type '{0}' to the CLR type '{1}' is not supported."
        /// </summary>
        internal static string EdmToClr_CannotConvertEdmValueToClrType(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmToClr_CannotConvertEdmValueToClrType,p0,p1);
        }

        /// <summary>
        /// A string like "Conversion of an edm collection value to the CLR type '{0}' is not supported. EDM collection values can be converted to System.Collections.Generic.IEnumerable&lt;T&gt;, System.Collections.Generic.IList&lt;T&gt; or System.Collections.Generic.ICollection&lt;T&gt;."
        /// </summary>
        internal static string EdmToClr_CannotConvertEdmCollectionValueToClrType(object p0) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmToClr_CannotConvertEdmCollectionValueToClrType,p0);
        }

        /// <summary>
        /// A string like "The type '{0}' of the object returned by the TryCreateObjectInstance delegate is not assignable to the expected type '{1}'."
        /// </summary>
        internal static string EdmToClr_TryCreateObjectInstanceReturnedWrongObject(object p0, object p1) {
            return Microsoft.Data.Edm.EntityRes.GetString(Microsoft.Data.Edm.EntityRes.EdmToClr_TryCreateObjectInstanceReturnedWrongObject,p0,p1);
        }

    }

    /// <summary>
    ///    Strongly-typed and parameterized exception factory.
    /// </summary>
    internal static partial class Error {

        /// <summary>
        /// The exception that is thrown when a null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument.
        /// </summary>
        internal static Exception ArgumentNull(string paramName) {
            return new ArgumentNullException(paramName);
        }
        
        /// <summary>
        /// The exception that is thrown when the value of an argument is outside the allowable range of values as defined by the invoked method.
        /// </summary>
        internal static Exception ArgumentOutOfRange(string paramName) {
            return new ArgumentOutOfRangeException(paramName);
        }

        /// <summary>
        /// The exception that is thrown when the author has yet to implement the logic at this point in the program. This can act as an exception based TODO tag.
        /// </summary>
        internal static Exception NotImplemented() {
            return new NotImplementedException();
        }

        /// <summary>
        /// The exception that is thrown when an invoked method is not supported, or when there is an attempt to read, seek, or write to a stream that does not support the invoked functionality. 
        /// </summary>
        internal static Exception NotSupported() {
            return new NotSupportedException();
        }        
    }
}

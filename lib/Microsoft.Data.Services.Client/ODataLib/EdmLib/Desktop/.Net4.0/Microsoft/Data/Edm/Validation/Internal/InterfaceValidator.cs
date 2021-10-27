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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Values;

namespace Microsoft.Data.Edm.Validation.Internal
{
    internal class InterfaceValidator
    {
        private static readonly Dictionary<Type, VisitorBase> InterfaceVisitors = CreateInterfaceVisitorsMap();
#if !SILVERLIGHT
        /// <summary>
        /// This is a thread-safe cache of object type to interface visitors which is shared between all instances of the validator.
        /// </summary>
        private static readonly Memoizer<Type, IEnumerable<VisitorBase>> ConcreteTypeInterfaceVisitors = new Memoizer<Type, IEnumerable<VisitorBase>>(ComputeInterfaceVisitorsForObject, null);
#else
        /// <summary>
        /// This is cache of object type to interface visitors which is NOT shared between instances of the validator.
        /// </summary>
        private readonly Dictionary<Type, IEnumerable<VisitorBase>> concreteTypeInterfaceVisitors = new Dictionary<Type, IEnumerable<VisitorBase>>();
#endif
        private readonly HashSetInternal<object> visited = new HashSetInternal<object>();
        private readonly HashSetInternal<object> visitedBad = new HashSetInternal<object>();
        private readonly HashSetInternal<object> danglingReferences = new HashSetInternal<object>();
        private readonly HashSetInternal<object> skipVisitation;
        private readonly bool validateDirectValueAnnotations;
        private readonly IEdmModel model;

        private InterfaceValidator(HashSetInternal<object> skipVisitation, IEdmModel model, bool validateDirectValueAnnotations)
        {
            this.skipVisitation = skipVisitation;
            this.model = model;
            this.validateDirectValueAnnotations = validateDirectValueAnnotations;
        }

        public static IEnumerable<EdmError> ValidateModelStructureAndSemantics(IEdmModel model, ValidationRuleSet semanticRuleSet)
        {
            InterfaceValidator modelValidator = new InterfaceValidator(null, model, true);

            // Perform structural validation of the root object.
            List<EdmError> errors = new List<EdmError>(modelValidator.ValidateStructure(model));

            // Then check references for structural integrity using separate validator (in order to avoid adding referenced objects to the this.visited).
            InterfaceValidator referencesValidator = new InterfaceValidator(modelValidator.visited, model, false);
            IEnumerable<object> referencesToStructurallyValidate = modelValidator.danglingReferences;
            while (referencesToStructurallyValidate.FirstOrDefault() != null)
            {
                foreach (object reference in referencesToStructurallyValidate)
                {
                    errors.AddRange(referencesValidator.ValidateStructure(reference));
                }

                referencesToStructurallyValidate = referencesValidator.danglingReferences.ToArray();
            }

            // If there are any critical structural errors detected, then it is not safe to traverse the root object, so return the errors without further processing.
            if (errors.Any(ValidationHelper.IsInterfaceCritical))
            {
                return errors;
            }

            // If the root object is structurally sound, apply validation rules to the visited objects that are not known to be bad.
            ValidationContext semanticValidationContext = new ValidationContext(
                model, (item) => modelValidator.visitedBad.Contains(item) || referencesValidator.visitedBad.Contains(item));
            Dictionary<Type, List<ValidationRule>> concreteTypeSemanticInterfaceVisitors = new Dictionary<Type, List<ValidationRule>>();
            foreach (object item in modelValidator.visited)
            {
                if (!modelValidator.visitedBad.Contains(item))
                {
                    foreach (ValidationRule rule in GetSemanticInterfaceVisitorsForObject(item.GetType(), semanticRuleSet, concreteTypeSemanticInterfaceVisitors))
                    {
                        rule.Evaluate(semanticValidationContext, item);
                    }
                }
            }

            errors.AddRange(semanticValidationContext.Errors);
            return errors;
        }

        public static IEnumerable<EdmError> GetStructuralErrors(IEdmElement item)
        {
            IEdmModel model = item as IEdmModel;
            InterfaceValidator structuralValidator = new InterfaceValidator(null, model, model != null);
            return structuralValidator.ValidateStructure(item);
        }

        #region Private implementation

        private static Dictionary<Type, VisitorBase> CreateInterfaceVisitorsMap()
        {
            Dictionary<Type, VisitorBase> map = new Dictionary<Type, VisitorBase>();

            foreach (Type nestedType in typeof(InterfaceValidator).GetNonPublicNestedTypes())
            {
                if (nestedType.IsClass())
                {
                    Type baseType = nestedType.GetBaseType();
                    if (baseType.IsGenericType() && baseType.GetBaseType() == typeof(VisitorBase))
                    {
                        map.Add(baseType.GetGenericArguments()[0], (VisitorBase)Activator.CreateInstance(nestedType));
                    }
                }
            }

            return map;
        }

        private static IEnumerable<VisitorBase> ComputeInterfaceVisitorsForObject(Type objectType)
        {
            List<VisitorBase> visitors = new List<VisitorBase>();
            foreach (Type type in objectType.GetInterfaces())
            {
                VisitorBase visitor;
                if (InterfaceVisitors.TryGetValue(type, out visitor))
                {
                    visitors.Add(visitor);
                }
            }

            return visitors;
        }

        private static EdmError CreatePropertyMustNotBeNullError<T>(T item, string propertyName)
        {
            return new EdmError(
                GetLocation(item),
                EdmErrorCode.InterfaceCriticalPropertyValueMustNotBeNull,
                Strings.EdmModel_Validator_Syntactic_PropertyMustNotBeNull(typeof(T).Name, propertyName));
        }

        private static EdmError CreateEnumPropertyOutOfRangeError<T, E>(T item, E enumValue, string propertyName)
        {
            return new EdmError(
                GetLocation(item),
                EdmErrorCode.InterfaceCriticalEnumPropertyValueOutOfRange,
                Strings.EdmModel_Validator_Syntactic_EnumPropertyValueOutOfRange(typeof(T).Name, propertyName, typeof(E).Name, enumValue));
        }

        private static EdmError CheckForInterfaceKindValueMismatchError<T, K, I>(T item, K kind, string propertyName)
        {
            // If object implements an expected interface, return no error.
            if (item is I)
            {
                return null;
            }

            return new EdmError(
                GetLocation(item),
                EdmErrorCode.InterfaceCriticalKindValueMismatch,
                Strings.EdmModel_Validator_Syntactic_InterfaceKindValueMismatch(kind, typeof(T).Name, propertyName, typeof(I).Name));
        }

        private static EdmError CreateInterfaceKindValueUnexpectedError<T, K>(T item, K kind, string propertyName)
        {
            return new EdmError(
                GetLocation(item),
                EdmErrorCode.InterfaceCriticalKindValueUnexpected,
                Strings.EdmModel_Validator_Syntactic_InterfaceKindValueUnexpected(kind, typeof(T).Name, propertyName));
        }

        private static EdmError CreateTypeRefInterfaceTypeKindValueMismatchError<T>(T item) where T : IEdmTypeReference
        {
            Debug.Assert(item.Definition != null, "item.Definition != null");
            return new EdmError(
                GetLocation(item),
                EdmErrorCode.InterfaceCriticalKindValueMismatch,
                Strings.EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch(typeof(T).Name, item.Definition.TypeKind));
        }

        private static EdmError CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError<T>(T item) where T : IEdmPrimitiveTypeReference
        {
            Debug.Assert(item.Definition is IEdmPrimitiveType, "item.Definition is IEdmPrimitiveType");
            return new EdmError(
                GetLocation(item),
                EdmErrorCode.InterfaceCriticalKindValueMismatch,
                Strings.EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch(typeof(T).Name, ((IEdmPrimitiveType)item.Definition).PrimitiveKind));
        }

        private static void ProcessEnumerable<T, E>(T item, IEnumerable<E> enumerable, string propertyName, IList targetList, ref List<EdmError> errors)
        {
            if (enumerable == null)
            {
                CollectErrors(CreatePropertyMustNotBeNullError(item, propertyName), ref errors);
            }
            else
            {
                foreach (E enumMember in enumerable)
                {
                    if (enumMember != null)
                    {
                        targetList.Add(enumMember);
                    }
                    else
                    {
                        CollectErrors(
                            new EdmError(GetLocation(item), EdmErrorCode.InterfaceCriticalEnumerableMustNotHaveNullElements, Strings.EdmModel_Validator_Syntactic_EnumerableMustNotHaveNullElements(typeof(T).Name, propertyName)),
                            ref errors);
                        break;
                    }
                }
            }
        }

        private static void CollectErrors(EdmError newError, ref List<EdmError> errors)
        {
            if (newError != null)
            {
                if (errors == null)
                {
                    errors = new List<EdmError>();
                }

                errors.Add(newError);
            }
        }

        private static bool IsCheckableBad(object element)
        {
            IEdmCheckable checkable = element as IEdmCheckable;
            return checkable != null && checkable.Errors != null && checkable.Errors.Count() > 0;
        }

        private static EdmLocation GetLocation(object item)
        {
            IEdmLocatable edmLocatable = item as IEdmLocatable;
            return edmLocatable != null && edmLocatable.Location != null ? edmLocatable.Location : new ObjectLocation(item);
        }

        private static IEnumerable<ValidationRule> GetSemanticInterfaceVisitorsForObject(Type objectType, ValidationRuleSet ruleSet, Dictionary<Type, List<ValidationRule>> concreteTypeSemanticInterfaceVisitors)
        {
            List<ValidationRule> visitors;
            if (!concreteTypeSemanticInterfaceVisitors.TryGetValue(objectType, out visitors))
            {
                visitors = new List<ValidationRule>();
                foreach (Type type in objectType.GetInterfaces())
                {
                    visitors.AddRange(ruleSet.GetRules(type));
                }

                concreteTypeSemanticInterfaceVisitors.Add(objectType, visitors);
            }

            return visitors;
        }

        private IEnumerable<EdmError> ValidateStructure(object item)
        {
            if (item is IEdmValidCoreModelElement || this.visited.Contains(item) || (this.skipVisitation != null && this.skipVisitation.Contains(item)))
            {
                // If we already visited this object, then errors (if any) have already been reported.
                return Enumerable.Empty<EdmError>();
            }

            this.visited.Add(item);
            if (this.danglingReferences.Contains(item))
            {
                // If this edm element is visited, then it is no longer a dangling reference.
                this.danglingReferences.Remove(item);
            }

            //// First pass: collect immediate errors for each interface and collect followup objects for the second pass.

            List<EdmError> immediateErrors = null;
            List<object> followup = new List<object>();
            List<object> references = new List<object>();
            IEnumerable<VisitorBase> visitors;
#if !SILVERLIGHT
            visitors = ConcreteTypeInterfaceVisitors.Evaluate(item.GetType());
#else
            visitors = this.GetInterfaceVisitorsForObject(item.GetType());
#endif
            foreach (VisitorBase visitor in visitors)
            {
                IEnumerable<EdmError> errors = visitor.Visit(item, followup, references);

                // For performance reasons some visitors may return null errors enumerator.
                if (errors != null)
                {
                    foreach (EdmError error in errors)
                    {
                        if (immediateErrors == null)
                        {
                            immediateErrors = new List<EdmError>();
                        }

                        immediateErrors.Add(error);
                    }
                }
            }

            // End of the first pass: if there are immediate errors, return them without doing the second pass.
            if (immediateErrors != null)
            {
                this.visitedBad.Add(item);
                return immediateErrors;
            }

            //// Second pass: collect errors from followup objects.

            List<EdmError> followupErrors = new List<EdmError>();

            // An element's direct value annotations are available only through a model,
            // and so are not found in a normal traversal.
            if (this.validateDirectValueAnnotations)
            {
                IEdmElement element = item as IEdmElement;
                if (element != null)
                {
                    foreach (IEdmDirectValueAnnotation annotation in this.model.DirectValueAnnotations(element))
                    {
                        followupErrors.AddRange(this.ValidateStructure(annotation));
                    }
                }
            }

            foreach (object followupItem in followup)
            {
                followupErrors.AddRange(this.ValidateStructure(followupItem));
            }

            foreach (object referencedItem in references)
            {
                this.CollectReference(referencedItem);
            }

            return followupErrors;
        }

        private void CollectReference(object reference)
        {
            if (!(reference is IEdmValidCoreModelElement) &&
                !this.visited.Contains(reference) &&
                (this.skipVisitation == null || !this.skipVisitation.Contains(reference)))
            {
                this.danglingReferences.Add(reference);
            }
        }

#if SILVERLIGHT
        private IEnumerable<VisitorBase> GetInterfaceVisitorsForObject(Type objectType)
        {
            IEnumerable<VisitorBase> visitors;
            if (!this.concreteTypeInterfaceVisitors.TryGetValue(objectType, out visitors))
            {
                visitors = ComputeInterfaceVisitorsForObject(objectType);
                this.concreteTypeInterfaceVisitors.Add(objectType, visitors);
            }

            return visitors;
        }
#endif

        #endregion

        #region Interface validators

        /*
         * The general shape of a validation visitor is
         *      IEnumerable<EdmError> Visit(IEdmXYZInterface item, List<object> followup, List<object> references)
         * Each visitor may return a null or empty collection of errors.
         * Note that if a visitor returns errors, followup and references will be ignored.
         */

        private abstract class VisitorBase
        {
            public abstract IEnumerable<EdmError> Visit(object item, List<object> followup, List<object> references);
        }

        private abstract class VisitorOfT<T> : VisitorBase
        {
            public override IEnumerable<EdmError> Visit(object item, List<object> followup, List<object> references)
            {
                return this.VisitT((T)item, followup, references);
            }

            protected abstract IEnumerable<EdmError> VisitT(T item, List<object> followup, List<object> references);
        }

        #region Core interfaces

        private sealed class VisitorOfIEdmCheckable : VisitorOfT<IEdmCheckable>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmCheckable checkable, List<object> followup, List<object> references)
            {
                List<EdmError> checkableErrors = new List<EdmError>();
                List<EdmError> errors = null;
                ProcessEnumerable(checkable, checkable.Errors, "Errors", checkableErrors, ref errors);
                return errors ?? checkableErrors;
            }
        }

        private sealed class VisitorOfIEdmElement : VisitorOfT<IEdmElement>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmElement element, List<object> followup, List<object> references)
            {
                return null;
            }
        }

        private sealed class VisitorOfIEdmNamedElement : VisitorOfT<IEdmNamedElement>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmNamedElement element, List<object> followup, List<object> references)
            {
                return element.Name != null ? null : new EdmError[] { CreatePropertyMustNotBeNullError(element, "Name") };
            }
        }

        private sealed class VisitorOfIEdmSchemaElement : VisitorOfT<IEdmSchemaElement>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmSchemaElement element, List<object> followup, List<object> references)
            {
                List<EdmError> errors = new List<EdmError>();

                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.TypeDefinition:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmSchemaType>(element, element.SchemaElementKind, "SchemaElementKind"), ref errors);
                        break;

                    case EdmSchemaElementKind.Function:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmFunction>(element, element.SchemaElementKind, "SchemaElementKind"), ref errors);
                        break;

                    case EdmSchemaElementKind.ValueTerm:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmValueTerm>(element, element.SchemaElementKind, "SchemaElementKind"), ref errors);
                        break;

                    case EdmSchemaElementKind.EntityContainer:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmEntityContainer>(element, element.SchemaElementKind, "SchemaElementKind"), ref errors);
                        break;

                    case EdmSchemaElementKind.None:
                        break;

                    default:
                        CollectErrors(CreateEnumPropertyOutOfRangeError(element, element.SchemaElementKind, "SchemaElementKind"), ref errors);
                        break;
                }

                if (element.Namespace == null)
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(element, "Namespace"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmModel : VisitorOfT<IEdmModel>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmModel model, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                ProcessEnumerable(model, model.SchemaElements, "SchemaElements", followup, ref errors);
                ProcessEnumerable(model, model.VocabularyAnnotations, "VocabularyAnnotations", followup, ref errors);
                return errors;
            }
        }

        private sealed class VisitorOfIEdmEntityContainer : VisitorOfT<IEdmEntityContainer>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntityContainer container, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                ProcessEnumerable(container, container.Elements, "Elements", followup, ref errors);
                return errors;
            }
        }

        private sealed class VisitorOfIEdmEntityContainerElement : VisitorOfT<IEdmEntityContainerElement>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntityContainerElement element, List<object> followup, List<object> references)
            {
                EdmError termKindError = null;
                switch (element.ContainerElementKind)
                {
                    case EdmContainerElementKind.EntitySet:
                        termKindError = CheckForInterfaceKindValueMismatchError<IEdmEntityContainerElement, EdmContainerElementKind, IEdmEntitySet>(element, element.ContainerElementKind, "ContainerElementKind");
                        break;

                    case EdmContainerElementKind.FunctionImport:
                        termKindError = CheckForInterfaceKindValueMismatchError<IEdmEntityContainerElement, EdmContainerElementKind, IEdmFunctionImport>(element, element.ContainerElementKind, "ContainerElementKind");
                        break;

                    case EdmContainerElementKind.None:
                        break;

                    default:
                        termKindError = CreateEnumPropertyOutOfRangeError(element, element.ContainerElementKind, "ContainerElementKind");
                        break;
                }

                return termKindError != null ? new EdmError[] { termKindError } : null;
            }
        }

        private sealed class VisitorOfIEdmEntitySet : VisitorOfT<IEdmEntitySet>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntitySet set, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (set.ElementType != null)
                {
                    references.Add(set.ElementType);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(set, "ElementType"), ref errors);
                }

                // Navigation targets are not EDM elements, so we expand and process them here instead of adding them as followups.
                List<IEdmNavigationTargetMapping> navTargetMappings = new List<IEdmNavigationTargetMapping>();
                ProcessEnumerable(set, set.NavigationTargets, "NavigationTargets", navTargetMappings, ref errors);
                foreach (IEdmNavigationTargetMapping navTargetMapping in navTargetMappings)
                {
                    if (navTargetMapping.NavigationProperty != null)
                    {
                        references.Add(navTargetMapping.NavigationProperty);
                    }
                    else
                    {
                        CollectErrors(CreatePropertyMustNotBeNullError(navTargetMapping, "NavigationProperty"), ref errors);
                    }

                    if (navTargetMapping.TargetEntitySet != null)
                    {
                        references.Add(navTargetMapping.TargetEntitySet);
                    }
                    else
                    {
                        CollectErrors(CreatePropertyMustNotBeNullError(navTargetMapping, "TargetEntitySet"), ref errors);
                    }
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmTypeReference : VisitorOfT<IEdmTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmTypeReference type, List<object> followup, List<object> references)
            {
                if (type.Definition != null)
                {
                    // Transient types, such as collections, rows and entity refs are considered to be owned by the type reference, so they go as followups.
                    // Schema types are owned by their model, so they go as references.
                    if (type.Definition is IEdmSchemaType)
                    {
                        references.Add(type.Definition);
                    }
                    else
                    {
                        followup.Add(type.Definition);
                    }

                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(type, "Definition") };
                }
            }
        }

        private sealed class VisitorOfIEdmType : VisitorOfT<IEdmType>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmType type, List<object> followup, List<object> references)
            {
                EdmError typeKindError = null;
                switch (type.TypeKind)
                {
                    case EdmTypeKind.Primitive:
                        typeKindError = CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmPrimitiveType>(type, type.TypeKind, "TypeKind");
                        break;

                    case EdmTypeKind.Entity:
                        typeKindError = CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmEntityType>(type, type.TypeKind, "TypeKind");
                        break;

                    case EdmTypeKind.Complex:
                        typeKindError = CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmComplexType>(type, type.TypeKind, "TypeKind");
                        break;

                    case EdmTypeKind.Row:
                        typeKindError = CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmRowType>(type, type.TypeKind, "TypeKind");
                        break;

                    case EdmTypeKind.Collection:
                        typeKindError = CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmCollectionType>(type, type.TypeKind, "TypeKind");
                        break;

                    case EdmTypeKind.EntityReference:
                        typeKindError = CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmEntityReferenceType>(type, type.TypeKind, "TypeKind");
                        break;

                    case EdmTypeKind.Enum:
                        typeKindError = CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmEnumType>(type, type.TypeKind, "TypeKind");
                        break;

                    case EdmTypeKind.None:
                        break;

                    default:
                        typeKindError = CreateInterfaceKindValueUnexpectedError(type, type.TypeKind, "TypeKind");
                        break;
                }

                return typeKindError != null ? new EdmError[] { typeKindError } : null;
            }
        }

        private sealed class VisitorOfIEdmPrimitiveType : VisitorOfT<IEdmPrimitiveType>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmPrimitiveType type, List<object> followup, List<object> references)
            {
                // Trying to reduce amount of noise in errors - if this type is bad, then most likely it will have an unacceptable kind, no need to report it.
                if (!IsCheckableBad(type) && (type.PrimitiveKind < EdmPrimitiveTypeKind.None || type.PrimitiveKind > EdmPrimitiveTypeKind.GeometryMultiPoint))
                {
                    return new EdmError[] { CreateInterfaceKindValueUnexpectedError(type, type.PrimitiveKind, "PrimitiveKind") };
                }
                else
                {
                    return null;
                }
            }
        }

        private sealed class VisitorOfIEdmStructuredType : VisitorOfT<IEdmStructuredType>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmStructuredType type, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                ProcessEnumerable(type, type.DeclaredProperties, "DeclaredProperties", followup, ref errors);

                if (type.BaseType != null)
                {
                    HashSetInternal<IEdmStructuredType> visitiedTypes = new HashSetInternal<IEdmStructuredType>();
                    visitiedTypes.Add(type);
                    for (IEdmStructuredType currentBaseType = currentBaseType = type.BaseType; currentBaseType != null; currentBaseType = currentBaseType.BaseType)
                    {
                        if (visitiedTypes.Contains(currentBaseType))
                        {
                            IEdmSchemaType schemaType = type as IEdmSchemaType;
                            string typeName = schemaType != null ? schemaType.FullName() : typeof(Type).Name;
                            CollectErrors(new EdmError(GetLocation(type), EdmErrorCode.InterfaceCriticalCycleInTypeHierarchy, Strings.EdmModel_Validator_Syntactic_InterfaceCriticalCycleInTypeHierarchy(typeName)), ref errors);
                            break;
                        }
                    }

                    references.Add(type.BaseType);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmEntityType : VisitorOfT<IEdmEntityType>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntityType type, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                if (type.DeclaredKey != null)
                {
                    ProcessEnumerable(type, type.DeclaredKey, "DeclaredKey", references, ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmEntityReferenceType : VisitorOfT<IEdmEntityReferenceType>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntityReferenceType type, List<object> followup, List<object> references)
            {
                if (type.EntityType != null)
                {
                    references.Add(type.EntityType);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(type, "EntityType") };
                }
            }
        }

        private sealed class VisitorOfIEdmEnumType : VisitorOfT<IEdmEnumType>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEnumType type, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                ProcessEnumerable(type, type.Members, "Members", followup, ref errors);

                if (type.UnderlyingType != null)
                {
                    references.Add(type.UnderlyingType);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(type, "UnderlyingType"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmTerm : VisitorOfT<IEdmTerm>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmTerm term, List<object> followup, List<object> references)
            {
                List<EdmError> termKindError = null;

                switch (term.TermKind)
                {
                    case EdmTermKind.Type:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmTerm, EdmTermKind, IEdmSchemaType>(term, term.TermKind, "TermKind"), ref termKindError);
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmTerm, EdmTermKind, IEdmStructuredType>(term, term.TermKind, "TermKind"), ref termKindError);
                        break;

                    case EdmTermKind.Value:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmTerm, EdmTermKind, IEdmValueTerm>(term, term.TermKind, "TermKind"), ref termKindError);
                        break;

                    case EdmTermKind.None:
                        break;

                    default:
                        CollectErrors(CreateInterfaceKindValueUnexpectedError(term, term.TermKind, "TermKind"), ref termKindError);
                        break;
                }

                return termKindError;
            }
        }

        private sealed class VisitorOfIEdmValueTerm : VisitorOfT<IEdmValueTerm>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmValueTerm term, List<object> followup, List<object> references)
            {
                if (term.Type != null)
                {
                    // Value term owns its element type reference, so it goes as a followup.
                    followup.Add(term.Type);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(term, "Type") };
                }
            }
        }

        private sealed class VisitorOfIEdmCollectionType : VisitorOfT<IEdmCollectionType>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmCollectionType type, List<object> followup, List<object> references)
            {
                if (type.ElementType != null)
                {
                    // Collection owns its element type reference, so it goes as a followup.
                    followup.Add(type.ElementType);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(type, "ElementType") };
                }
            }
        }

        private sealed class VisitorOfIEdmProperty : VisitorOfT<IEdmProperty>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmProperty property, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                switch (property.PropertyKind)
                {
                    case EdmPropertyKind.Structural:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmProperty, EdmPropertyKind, IEdmStructuralProperty>(property, property.PropertyKind, "PropertyKind"), ref errors);
                        break;

                    case EdmPropertyKind.Navigation:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmProperty, EdmPropertyKind, IEdmNavigationProperty>(property, property.PropertyKind, "PropertyKind"), ref errors);
                        break;

                    case EdmPropertyKind.None:
                        break;

                    default:
                        CollectErrors(CreateInterfaceKindValueUnexpectedError(property, property.PropertyKind, "PropertyKind"), ref errors);
                        break;
                }

                if (property.Type != null)
                {
                    // Property owns its type reference, so it goes as a followup.
                    followup.Add(property.Type);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(property, "Type"), ref errors);
                }

                if (property.DeclaringType != null)
                {
                    references.Add(property.DeclaringType);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(property, "DeclaringType"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmStructuralProperty : VisitorOfT<IEdmStructuralProperty>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmStructuralProperty property, List<object> followup, List<object> references)
            {
                if (property.ConcurrencyMode < EdmConcurrencyMode.None || property.ConcurrencyMode > EdmConcurrencyMode.Fixed)
                {
                    return new EdmError[] { CreateEnumPropertyOutOfRangeError(property, property.ConcurrencyMode, "ConcurrencyMode") };
                }
                else
                {
                    return null;
                }
            }
        }

        private sealed class VisitorOfIEdmNavigationProperty : VisitorOfT<IEdmNavigationProperty>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmNavigationProperty property, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (property.Partner != null)
                {
                    // If the declaring type of the partner does not contain the partner, it is a silent partner, and belongs to this property.
                    if (!property.Partner.DeclaringType.DeclaredProperties.Contains(property.Partner))
                    {
                        followup.Add(property.Partner);
                    }
                    else
                    {
                        references.Add(property.Partner);
                    }

                    if (property.Partner.Partner != property || property.Partner == property)
                    {
                        CollectErrors(new EdmError(GetLocation(property), EdmErrorCode.InterfaceCriticalNavigationPartnerInvalid, Strings.EdmModel_Validator_Syntactic_NavigationPartnerInvalid(property.Name)), ref errors);
                    }
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(property, "Partner"), ref errors);
                }

                if (property.DependentProperties != null)
                {
                    ProcessEnumerable(property, property.DependentProperties, "DependentProperties", references, ref errors);
                }

                if (property.OnDelete < EdmOnDeleteAction.None || property.OnDelete > EdmOnDeleteAction.Cascade)
                {
                    CollectErrors(CreateEnumPropertyOutOfRangeError(property, property.OnDelete, "OnDelete"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmEnumMember : VisitorOfT<IEdmEnumMember>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEnumMember member, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (member.DeclaringType != null)
                {
                    references.Add(member.DeclaringType);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(member, "DeclaringType"), ref errors);
                }

                if (member.Value != null)
                {
                    followup.Add(member.Value);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(member, "Value"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmFunctionBase : VisitorOfT<IEdmFunctionBase>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmFunctionBase function, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                ProcessEnumerable(function, function.Parameters, "Parameters", followup, ref errors);

                // Return type is optional for function imports and is required for MDFs. Both cases are derived interfaces (IEdmFunctionImport and IEdmFunction).
                // So, from the point of view of this interface, we consider return type as optional and it is expected that IEdmFunction visitor will have 
                // an additional null check for the return type.
                if (function.ReturnType != null)
                {
                    // Function owns its return type reference, so it goes as a followup.
                    followup.Add(function.ReturnType);
                }
                
                return errors;
            }
        }

        private sealed class VisitorOfIEdmFunction : VisitorOfT<IEdmFunction>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmFunction function, List<object> followup, List<object> references)
            {
                // No need to return the return type as followup - it is handled as such in the IEdmFunctionBase visitor.
                return function.ReturnType == null ? new EdmError[] { CreatePropertyMustNotBeNullError(function, "ReturnType") } : null;
            }
        }

        private sealed class VisitorOfIEdmFunctionImport : VisitorOfT<IEdmFunctionImport>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmFunctionImport functionImport, List<object> followup, List<object> references)
            {
                if (functionImport.EntitySet != null)
                {
                    followup.Add(functionImport.EntitySet);
                }

                return null;
            }
        }

        private sealed class VisitorOfIEdmFunctionParameter : VisitorOfT<IEdmFunctionParameter>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmFunctionParameter parameter, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (parameter.Type != null)
                {
                    // Parameter owns its type reference, so it goes as a followup.
                    followup.Add(parameter.Type);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(parameter, "Type"), ref errors);
                }

                if (parameter.DeclaringFunction != null)
                {
                    references.Add(parameter.DeclaringFunction);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(parameter, "DeclaringFunction"), ref errors);
                }

                if (parameter.Mode < EdmFunctionParameterMode.None || parameter.Mode > EdmFunctionParameterMode.InOut)
                {
                    CollectErrors(CreateEnumPropertyOutOfRangeError(parameter, parameter.Mode, "Mode"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmCollectionTypeReference : VisitorOfT<IEdmCollectionTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmCollectionTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && typeRef.Definition.TypeKind != EdmTypeKind.Collection ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmEntityReferenceTypeReference : VisitorOfT<IEdmEntityReferenceTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntityReferenceTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && typeRef.Definition.TypeKind != EdmTypeKind.EntityReference ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmStructuredTypeReference : VisitorOfT<IEdmStructuredTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmStructuredTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && !typeRef.Definition.TypeKind.IsStructured() ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmEntityTypeReference : VisitorOfT<IEdmEntityTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntityTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && typeRef.Definition.TypeKind != EdmTypeKind.Entity ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmComplexTypeReference : VisitorOfT<IEdmComplexTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmComplexTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && typeRef.Definition.TypeKind != EdmTypeKind.Complex ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmRowTypeReference : VisitorOfT<IEdmRowTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmRowTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && typeRef.Definition.TypeKind != EdmTypeKind.Row ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmEnumTypeReference : VisitorOfT<IEdmEnumTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEnumTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && typeRef.Definition.TypeKind != EdmTypeKind.Enum ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmPrimitiveTypeReference : VisitorOfT<IEdmPrimitiveTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmPrimitiveTypeReference typeRef, List<object> followup, List<object> references)
            {
                return typeRef.Definition != null && typeRef.Definition.TypeKind != EdmTypeKind.Primitive ? new EdmError[] { CreateTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmBinaryTypeReference : VisitorOfT<IEdmBinaryTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmBinaryTypeReference typeRef, List<object> followup, List<object> references)
            {
                IEdmPrimitiveType primitive = typeRef.Definition as IEdmPrimitiveType;
                return primitive != null && primitive.PrimitiveKind != EdmPrimitiveTypeKind.Binary ? new EdmError[] { CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmDecimalTypeReference : VisitorOfT<IEdmDecimalTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmDecimalTypeReference typeRef, List<object> followup, List<object> references)
            {
                IEdmPrimitiveType primitive = typeRef.Definition as IEdmPrimitiveType;
                return primitive != null && primitive.PrimitiveKind != EdmPrimitiveTypeKind.Decimal ? new EdmError[] { CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmStringTypeReference : VisitorOfT<IEdmStringTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmStringTypeReference typeRef, List<object> followup, List<object> references)
            {
                IEdmPrimitiveType primitive = typeRef.Definition as IEdmPrimitiveType;
                return primitive != null && primitive.PrimitiveKind != EdmPrimitiveTypeKind.String ? new EdmError[] { CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmTemporalTypeReference : VisitorOfT<IEdmTemporalTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmTemporalTypeReference typeRef, List<object> followup, List<object> references)
            {
                IEdmPrimitiveType primitive = typeRef.Definition as IEdmPrimitiveType;
                return primitive != null && !primitive.PrimitiveKind.IsTemporal() ? new EdmError[] { CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        private sealed class VisitorOfIEdmSpatialTypeReference : VisitorOfT<IEdmSpatialTypeReference>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmSpatialTypeReference typeRef, List<object> followup, List<object> references)
            {
                IEdmPrimitiveType primitive = typeRef.Definition as IEdmPrimitiveType;
                return primitive != null && !primitive.PrimitiveKind.IsSpatial() ? new EdmError[] { CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError(typeRef) } : null;
            }
        }

        #endregion

        #region Expressions

        private sealed class VisitorOfIEdmExpression : VisitorOfT<IEdmExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmExpression expression, List<object> followup, List<object> references)
            {
                // Trying to reduce amount of noise in errors - if this expression is bad, then most likely it will have an unacceptable kind, no need to report it.
                EdmError expressionKindError = null;
                if (!IsCheckableBad(expression))
                {
                    switch (expression.ExpressionKind)
                    {
                        case EdmExpressionKind.IntegerConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmIntegerConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.StringConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmStringConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.BinaryConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmBinaryConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.BooleanConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmBooleanConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.DateTimeConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmDateTimeConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.DateTimeOffsetConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmDateTimeOffsetConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.TimeConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmTimeConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.DecimalConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmDecimalConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.FloatingConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmFloatingConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.GuidConstant:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmGuidConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.Null:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmNullExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.Record:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmRecordExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.Collection:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmCollectionExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.Path:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmPathExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.ParameterReference:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmParameterReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.FunctionReference:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmFunctionReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.PropertyReference:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmPropertyReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.ValueTermReference:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmValueTermReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.EntitySetReference:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmEntitySetReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.EnumMemberReference:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmEnumMemberReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.If:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmIfExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.AssertType:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmAssertTypeExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.IsType:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmIsTypeExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.FunctionApplication:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmApplyExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.Labeled:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmLabeledExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        case EdmExpressionKind.LabeledExpressionReference:
                            expressionKindError = CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmLabeledExpressionReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
                            break;

                        default:
                            expressionKindError = CreateInterfaceKindValueUnexpectedError(expression, expression.ExpressionKind, "ExpressionKind");
                            break;
                    }
                }

                return expressionKindError != null ? new EdmError[] { expressionKindError } : null;
            }
        }

        private sealed class VisitorOfIEdmRecordExpression : VisitorOfT<IEdmRecordExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmRecordExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                ProcessEnumerable(expression, expression.Properties, "Properties", followup, ref errors);

                if (expression.DeclaredType != null)
                {
                    // Record constructor owns its type reference, so it goes as a followup.
                    followup.Add(expression.DeclaredType);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmPropertyConstructor : VisitorOfT<IEdmPropertyConstructor>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmPropertyConstructor expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (expression.Name == null)
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Name"), ref errors);
                }

                if (expression.Value != null)
                {
                    followup.Add(expression.Value);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Value"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmCollectionExpression : VisitorOfT<IEdmCollectionExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmCollectionExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                ProcessEnumerable(expression, expression.Elements, "Elements", followup, ref errors);

                if (expression.DeclaredType != null)
                {
                    // Collection constructor owns its type reference, so it goes as a followup.
                    followup.Add(expression.DeclaredType);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmLabeledElement : VisitorOfT<IEdmLabeledExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmLabeledExpression expression, List<object> followup, List<object> references)
            {
                if (expression.Expression != null)
                {
                    followup.Add(expression.Expression);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(expression, "Expression") };
                }
            }
        }

        private sealed class VisitorOfIEdmPathExpression : VisitorOfT<IEdmPathExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmPathExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                List<string> segments = new List<string>();
                ProcessEnumerable(expression, expression.Path, "Path", segments, ref errors);

                return errors;
            }
        }

        private sealed class VisitorOfIEdmParameterReferenceExpression : VisitorOfT<IEdmParameterReferenceExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmParameterReferenceExpression expression, List<object> followup, List<object> references)
            {
                if (expression.ReferencedParameter != null)
                {
                    references.Add(expression.ReferencedParameter);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(expression, "ReferencedParameter") };
                }
            }
        }

        private sealed class VisitorOfIEdmFunctionReferenceExpression : VisitorOfT<IEdmFunctionReferenceExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmFunctionReferenceExpression expression, List<object> followup, List<object> references)
            {
                if (expression.ReferencedFunction != null)
                {
                    Debug.Assert(expression.ReferencedFunction is IEdmSchemaElement || expression is IEdmEntityContainerElement, "Return as followup if the referenced object is not a schema function or a function import.");
                    references.Add(expression.ReferencedFunction);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(expression, "ReferencedFunction") };
                }
            }
        }

        private sealed class VisitorOfIEdmPropertyReferenceExpression : VisitorOfT<IEdmPropertyReferenceExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmPropertyReferenceExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (expression.Base != null)
                {
                    followup.Add(expression.Base);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Base"), ref errors);
                }

                if (expression.ReferencedProperty != null)
                {
                    references.Add(expression.ReferencedProperty);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "ReferencedProperty"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmValueTermReferenceExpression : VisitorOfT<IEdmValueTermReferenceExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmValueTermReferenceExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (expression.Base != null)
                {
                    followup.Add(expression.Base);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Base"), ref errors);
                }

                if (expression.Term != null)
                {
                    references.Add(expression.Term);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Term"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VistorOfIEdmEntitySetReferenceExpression : VisitorOfT<IEdmEntitySetReferenceExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEntitySetReferenceExpression expression, List<object> followup, List<object> references)
            {
                if (expression.ReferencedEntitySet != null)
                {
                    references.Add(expression.ReferencedEntitySet);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(expression, "ReferencedEntitySet") };
                }
            }
        }

        private sealed class VistorOfIEdmEnumMemberReferenceExpression : VisitorOfT<IEdmEnumMemberReferenceExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEnumMemberReferenceExpression expression, List<object> followup, List<object> references)
            {
                if (expression.ReferencedEnumMember != null)
                {
                    references.Add(expression.ReferencedEnumMember);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(expression, "ReferencedEnumMember") };
                }
            }
        }

        private sealed class VistorOfIEdmIfExpression : VisitorOfT<IEdmIfExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmIfExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (expression.TestExpression != null)
                {
                    followup.Add(expression.TestExpression);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "TestExpression"), ref errors);
                }

                if (expression.TrueExpression != null)
                {
                    followup.Add(expression.TrueExpression);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "TrueExpression"), ref errors);
                }

                if (expression.FalseExpression != null)
                {
                    followup.Add(expression.FalseExpression);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "FalseExpression"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VistorOfIEdmAssertTypeExpression : VisitorOfT<IEdmAssertTypeExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmAssertTypeExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (expression.Operand != null)
                {
                    followup.Add(expression.Operand);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Operand"), ref errors);
                }

                if (expression.Type != null)
                {
                    // Assert owns its type reference, so it goes as a followup.
                    followup.Add(expression.Type);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Type"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VistorOfIEdmIsTypeExpression : VisitorOfT<IEdmIsTypeExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmIsTypeExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (expression.Operand != null)
                {
                    followup.Add(expression.Operand);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Operand"), ref errors);
                }

                if (expression.Type != null)
                {
                    // Assert owns its type reference, so it goes as a followup.
                    followup.Add(expression.Type);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "Type"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VistorOfIEdmFunctionApplicationExpression : VisitorOfT<IEdmApplyExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmApplyExpression expression, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (expression.AppliedFunction != null)
                {
                    followup.Add(expression.AppliedFunction);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(expression, "AppliedFunction"), ref errors);
                }

                ProcessEnumerable(expression, expression.Arguments, "Arguments", followup, ref errors);

                return errors;
            }
        }

        private sealed class VistorOfIEdmLabeledElementReferenceExpression : VisitorOfT<IEdmLabeledExpressionReferenceExpression>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmLabeledExpressionReferenceExpression expression, List<object> followup, List<object> references)
            {
                if (expression.ReferencedLabeledExpression != null)
                {
                    references.Add(expression.ReferencedLabeledExpression);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(expression, "ReferencedLabeledExpression") };
                }
            }
        }

        #endregion

        #region Values

        private sealed class VisitorOfIEdmValue : VisitorOfT<IEdmValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmValue value, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                if (value.Type != null)
                {
                    // Value owns its type reference, so it goes as a followup.
                    followup.Add(value.Type);
                }

                switch (value.ValueKind)
                {
                    case EdmValueKind.Binary:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmBinaryValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Boolean:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmBooleanValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Collection:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmCollectionValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.DateTime:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmDateTimeValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.DateTimeOffset:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmDateTimeOffsetValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Decimal:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmDecimalValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Enum:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmEnumValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Floating:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmFloatingValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Guid:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmGuidValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Integer:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmIntegerValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Null:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmNullValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.String:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmStringValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Structured:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmStructuredValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.Time:
                        CollectErrors(CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmTimeValue>(value, value.ValueKind, "ValueKind"), ref errors);
                        break;

                    case EdmValueKind.None:
                        break;

                    default:
                        CollectErrors(CreateInterfaceKindValueUnexpectedError(value, value.ValueKind, "ValueKind"), ref errors);
                        break;
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmDelayedValue : VisitorOfT<IEdmDelayedValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmDelayedValue value, List<object> followup, List<object> references)
            {
                if (value.Value != null)
                {
                    followup.Add(value.Value);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(value, "Value") };
                }
            }
        }

        private sealed class VisitorOfIEdmPropertyValue : VisitorOfT<IEdmPropertyValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmPropertyValue value, List<object> followup, List<object> references)
            {
                return value.Name == null ? new EdmError[] { CreatePropertyMustNotBeNullError(value, "Name") } : null;
            }
        }

        private sealed class VisitorOfIEdmEnumValue : VisitorOfT<IEdmEnumValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmEnumValue value, List<object> followup, List<object> references)
            {
                if (value.Value != null)
                {
                    followup.Add(value.Value);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(value, "Value") };
                }
            }
        }

        private sealed class VisitorOfIEdmCollectionValue : VisitorOfT<IEdmCollectionValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmCollectionValue value, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                ProcessEnumerable(value, value.Elements, "Elements", followup, ref errors);
                return errors;
            }
        }

        private sealed class VisitorOfIEdmStructuredValue : VisitorOfT<IEdmStructuredValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmStructuredValue value, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                ProcessEnumerable(value, value.PropertyValues, "PropertyValues", followup, ref errors);
                return errors;
            }
        }

        private sealed class VisitorOfIEdmBinaryValue : VisitorOfT<IEdmBinaryValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmBinaryValue value, List<object> followup, List<object> references)
            {
                return value.Value == null ? new EdmError[] { CreatePropertyMustNotBeNullError(value, "Value") } : null;
            }
        }

        private sealed class VisitorOfIEdmStringValue : VisitorOfT<IEdmStringValue>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmStringValue value, List<object> followup, List<object> references)
            {
                return value.Value == null ? new EdmError[] { CreatePropertyMustNotBeNullError(value, "Value") } : null;
            }
        }

        #endregion

        #region Annotations

        private sealed class VisitorOfIEdmVocabularyAnnotation : VisitorOfT<IEdmVocabularyAnnotation>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmVocabularyAnnotation annotation, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (annotation.Term != null)
                {
                    references.Add(annotation.Term);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(annotation, "Term"), ref errors);
                }

                if (annotation.Target != null)
                {
                    references.Add(annotation.Target);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(annotation, "Target"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmValueAnnotation : VisitorOfT<IEdmValueAnnotation>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmValueAnnotation annotation, List<object> followup, List<object> references)
            {
                if (annotation.Value != null)
                {
                    followup.Add(annotation.Value);
                    return null;
                }
                else
                {
                    return new EdmError[] { CreatePropertyMustNotBeNullError(annotation, "Value") };
                }
            }
        }

        private sealed class VisitorOfIEdmTypeAnnotation : VisitorOfT<IEdmTypeAnnotation>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmTypeAnnotation annotation, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;
                ProcessEnumerable(annotation, annotation.PropertyValueBindings, "PropertyValueBindings", followup, ref errors);
                return errors;
            }
        }

        private sealed class VisitorOfIEdmPropertyValueBinding : VisitorOfT<IEdmPropertyValueBinding>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmPropertyValueBinding binding, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (binding.Value != null)
                {
                    followup.Add(binding.Value);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(binding, "Value"), ref errors);
                }

                if (binding.BoundProperty != null)
                {
                    references.Add(binding.BoundProperty);
                }
                else
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(binding, "BoundProperty"), ref errors);
                }

                return errors;
            }
        }

        private sealed class VisitorOfIEdmDirectValueAnnotation : VisitorOfT<IEdmDirectValueAnnotation>
        {
            protected override IEnumerable<EdmError> VisitT(IEdmDirectValueAnnotation annotation, List<object> followup, List<object> references)
            {
                List<EdmError> errors = null;

                if (annotation.NamespaceUri == null)
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(annotation, "NamespaceUri"), ref errors);
                }

                if (annotation.Value == null)
                {
                    CollectErrors(CreatePropertyMustNotBeNullError(annotation, "Value"), ref errors);
                }

                return errors;
            }
        }

        #endregion

        #endregion
    }
}

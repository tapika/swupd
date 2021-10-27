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
using System.Collections.Generic;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm
{
    internal abstract class EdmModelVisitor
    {
        protected readonly IEdmModel Model;

        protected EdmModelVisitor(IEdmModel model)
        {
            this.Model = model;
        }

        public void VisitEdmModel()
        {
            this.ProcessModel(this.Model);
        }

        #region Visit Methods

        #region Elements

        public void VisitSchemaElements(IEnumerable<IEdmSchemaElement> elements)
        {
            VisitCollection(elements, this.VisitSchemaElement);
        }

        public void VisitSchemaElement(IEdmSchemaElement element)
        {
            switch (element.SchemaElementKind)
            {
                case EdmSchemaElementKind.Function:
                    this.ProcessFunction((IEdmFunction)element);
                    break;
                case EdmSchemaElementKind.TypeDefinition:
                    this.VisitSchemaType((IEdmType)element);
                    break;
                case EdmSchemaElementKind.ValueTerm:
                    this.ProcessValueTerm((IEdmValueTerm)element);
                    break;
                case EdmSchemaElementKind.EntityContainer:
                    this.ProcessEntityContainer((IEdmEntityContainer)element);
                    break;
                case EdmSchemaElementKind.None:
                    this.ProcessSchemaElement(element);
                    break;
                default:
                    throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_SchemaElementKind(element.SchemaElementKind));
            }
        }

        #endregion

        #region Annotations

        public void VisitAnnotations(IEnumerable<IEdmDirectValueAnnotation> annotations)
        {
            VisitCollection(annotations, this.VisitAnnotation);
        }

        public void VisitVocabularyAnnotations(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            VisitCollection(annotations, this.VisitVocabularyAnnotation);
        }

        public void VisitAnnotation(IEdmDirectValueAnnotation annotation)
        {
            this.ProcessImmediateValueAnnotation((IEdmDirectValueAnnotation)annotation);
        }

        public void VisitVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
        {
            if (annotation.Term != null)
            {
                switch (annotation.Term.TermKind)
                {
                    case EdmTermKind.Type:
                        this.ProcessTypeAnnotation((IEdmTypeAnnotation)annotation);
                        break;
                    case EdmTermKind.Value:
                        this.ProcessValueAnnotation((IEdmValueAnnotation)annotation);
                        break;
                    case EdmTermKind.None:
                        this.ProcessVocabularyAnnotation(annotation);
                        break;
                    default:
                        throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_TermKind(annotation.Term.TermKind));
                }
            }
            else
            {
                this.ProcessVocabularyAnnotation(annotation);
            }
        }

        public void VisitPropertyValueBindings(IEnumerable<IEdmPropertyValueBinding> bindings)
        {
            VisitCollection(bindings, this.ProcessPropertyValueBinding);
        }

        #endregion

        #region Expressions

        public void VisitExpressions(IEnumerable<IEdmExpression> expressions)
        {
            VisitCollection(expressions, this.VisitExpression);
        }

        public void VisitExpression(IEdmExpression expression)
        {
            switch (expression.ExpressionKind)
            {
                case EdmExpressionKind.AssertType:
                    this.ProcessAssertTypeExpression((IEdmAssertTypeExpression)expression);
                    break;
                case EdmExpressionKind.BinaryConstant:
                    this.ProcessBinaryConstantExpression((IEdmBinaryConstantExpression)expression);
                    break;
                case EdmExpressionKind.BooleanConstant:
                    this.ProcessBooleanConstantExpression((IEdmBooleanConstantExpression)expression);
                    break;
                case EdmExpressionKind.Collection:
                    this.ProcessCollectionExpression((IEdmCollectionExpression)expression);
                    break;
                case EdmExpressionKind.DateTimeConstant:
                    this.ProcessDateTimeConstantExpression((IEdmDateTimeConstantExpression)expression);
                    break;
                case EdmExpressionKind.DateTimeOffsetConstant:
                    this.ProcessDateTimeOffsetConstantExpression((IEdmDateTimeOffsetConstantExpression)expression);
                    break;
                case EdmExpressionKind.DecimalConstant:
                    this.ProcessDecimalConstantExpression((IEdmDecimalConstantExpression)expression);
                    break;
                case EdmExpressionKind.EntitySetReference:
                    this.ProcessEntitySetReferenceExpression((IEdmEntitySetReferenceExpression)expression);
                    break;
                case EdmExpressionKind.EnumMemberReference:
                    this.ProcessEnumMemberReferenceExpression((IEdmEnumMemberReferenceExpression)expression);
                    break;
                case EdmExpressionKind.FloatingConstant:
                    this.ProcessFloatingConstantExpression((IEdmFloatingConstantExpression)expression);
                    break;
                case EdmExpressionKind.FunctionApplication:
                    this.ProcessFunctionApplicationExpression((IEdmApplyExpression)expression);
                    break;
                case EdmExpressionKind.FunctionReference:
                    this.ProcessFunctionReferenceExpression((IEdmFunctionReferenceExpression)expression);
                    break;
                case EdmExpressionKind.GuidConstant:
                    this.ProcessGuidConstantExpression((IEdmGuidConstantExpression)expression);
                    break;
                case EdmExpressionKind.If:
                    this.ProcessIfExpression((IEdmIfExpression)expression);
                    break;
                case EdmExpressionKind.IntegerConstant:
                    this.ProcessIntegerConstantExpression((IEdmIntegerConstantExpression)expression);
                    break;
                case EdmExpressionKind.IsType:
                    this.ProcessIsTypeExpression((IEdmIsTypeExpression)expression);
                    break;
                case EdmExpressionKind.ParameterReference:
                    this.ProcessParameterReferenceExpression((IEdmParameterReferenceExpression)expression);
                    break;
                case EdmExpressionKind.LabeledExpressionReference:
                    this.ProcessLabeledExpressionReferenceExpression((IEdmLabeledExpressionReferenceExpression)expression);
                    break;
                case EdmExpressionKind.Labeled:
                    this.ProcessLabeledExpression((IEdmLabeledExpression)expression);
                    break;
                case EdmExpressionKind.Null:
                    this.ProcessNullConstantExpression((IEdmNullExpression)expression);
                    break;
                case EdmExpressionKind.Path:
                    this.ProcessPathExpression((IEdmPathExpression)expression);
                    break;
                case EdmExpressionKind.PropertyReference:
                    this.ProcessPropertyReferenceExpression((IEdmPropertyReferenceExpression)expression);
                    break;
                case EdmExpressionKind.Record:
                    this.ProcessRecordExpression((IEdmRecordExpression)expression);
                    break;
                case EdmExpressionKind.StringConstant:
                    this.ProcessStringConstantExpression((IEdmStringConstantExpression)expression);
                    break;
                case EdmExpressionKind.TimeConstant:
                    this.ProcessTimeConstantExpression((IEdmTimeConstantExpression)expression);
                    break;
                case EdmExpressionKind.ValueTermReference:
                    this.ProcessPropertyReferenceExpression((IEdmPropertyReferenceExpression)expression);
                    break;
                case EdmExpressionKind.None:
                    this.ProcessExpression(expression);
                    break;
                default:
                    throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_ExpressionKind(expression.ExpressionKind));
            }
        }

        public void VisitPropertyConstructors(IEnumerable<IEdmPropertyConstructor> constructor)
        {
            VisitCollection(constructor, this.ProcessPropertyConstructor);
        }

        #endregion

        #region Data Model

        public void VisitEntityContainerElements(IEnumerable<IEdmEntityContainerElement> elements)
        {
            foreach (IEdmEntityContainerElement element in elements)
            {
                switch (element.ContainerElementKind)
                {
                    case EdmContainerElementKind.EntitySet:
                        this.ProcessEntitySet((IEdmEntitySet)element);
                        break;
                    case EdmContainerElementKind.FunctionImport:
                        this.ProcessFunctionImport((IEdmFunctionImport)element);
                        break;
                    case EdmContainerElementKind.None:
                        this.ProcessEntityContainerElement(element);
                        break;
                    default:
                        throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_ContainerElementKind(element.ContainerElementKind.ToString()));
                }
            }
        }

        #endregion

        #region Type References

        public void VisitTypeReference(IEdmTypeReference reference)
        {
            switch (reference.TypeKind())
            {
                case EdmTypeKind.Collection:
                    this.ProcessCollectionTypeReference(reference.AsCollection());
                    break;
                case EdmTypeKind.Complex:
                    this.ProcessComplexTypeReference(reference.AsComplex());
                    break;
                case EdmTypeKind.Entity:
                    this.ProcessEntityTypeReference(reference.AsEntity());
                    break;
                case EdmTypeKind.EntityReference:
                    this.ProcessEntityReferenceTypeReference(reference.AsEntityReference());
                    break;
                case EdmTypeKind.Enum:
                    this.ProcessEnumTypeReference(reference.AsEnum());
                    break;
                case EdmTypeKind.Primitive:
                    this.VisitPrimitiveTypeReference(reference.AsPrimitive());
                    break;
                case EdmTypeKind.Row:
                    this.ProcessRowTypeReference(reference.AsRow());
                    break;
                case EdmTypeKind.None:
                    this.ProcessTypeReference(reference);
                    break;
                default:
                    throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_TypeKind(reference.TypeKind().ToString()));
            }
        }

        public void VisitPrimitiveTypeReference(IEdmPrimitiveTypeReference reference)
        {
            switch (reference.PrimitiveKind())
            {
                case EdmPrimitiveTypeKind.Binary:
                    this.ProcessBinaryTypeReference(reference.AsBinary());
                    break;
                case EdmPrimitiveTypeKind.Decimal:
                    this.ProcessDecimalTypeReference(reference.AsDecimal());
                    break;
                case EdmPrimitiveTypeKind.String:
                    this.ProcessStringTypeReference(reference.AsString());
                    break;
                case EdmPrimitiveTypeKind.DateTime:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Time:
                    this.ProcessTemporalTypeReference(reference.AsTemporal());
                    break;
                case EdmPrimitiveTypeKind.Geography:
                case EdmPrimitiveTypeKind.GeographyPoint:
                case EdmPrimitiveTypeKind.GeographyLineString:
                case EdmPrimitiveTypeKind.GeographyPolygon:
                case EdmPrimitiveTypeKind.GeographyCollection:
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                case EdmPrimitiveTypeKind.Geometry:
                case EdmPrimitiveTypeKind.GeometryPoint:
                case EdmPrimitiveTypeKind.GeometryLineString:
                case EdmPrimitiveTypeKind.GeometryPolygon:
                case EdmPrimitiveTypeKind.GeometryCollection:
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    this.ProcessSpatialTypeReference(reference.AsSpatial());
                    break;
                case EdmPrimitiveTypeKind.Boolean:
                case EdmPrimitiveTypeKind.Byte:
                case EdmPrimitiveTypeKind.Double:
                case EdmPrimitiveTypeKind.Guid:
                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                case EdmPrimitiveTypeKind.Int64:
                case EdmPrimitiveTypeKind.SByte:
                case EdmPrimitiveTypeKind.Single:
                case EdmPrimitiveTypeKind.Stream:
                case EdmPrimitiveTypeKind.None:
                    this.ProcessPrimitiveTypeReference(reference);
                    break;
                default:
                    throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_PrimitiveKind(reference.PrimitiveKind().ToString()));
            }
        }

        #endregion

        #region Type Definitions

        public void VisitSchemaType(IEdmType definition)
        {
            switch (definition.TypeKind)
            {
                case EdmTypeKind.Complex:
                    this.ProcessComplexType((IEdmComplexType)definition);
                    break;
                case EdmTypeKind.Entity:
                    this.ProcessEntityType((IEdmEntityType)definition);
                    break;
                case EdmTypeKind.Enum:
                    this.ProcessEnumType((IEdmEnumType)definition);
                    break;
                case EdmTypeKind.None:
                    this.VisitSchemaType(definition);
                    break;
                default:
                    throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_TypeKind(definition.TypeKind));
            }
        }

        public void VisitProperties(IEnumerable<IEdmProperty> properties)
        {
            VisitCollection(properties, this.VisitProperty);
        }

        public void VisitProperty(IEdmProperty property)
        {
            switch (property.PropertyKind)
            {
                case EdmPropertyKind.Navigation:
                    this.ProcessNavigationProperty((IEdmNavigationProperty)property);
                    break;
                case EdmPropertyKind.Structural:
                    this.ProcessStructuralProperty((IEdmStructuralProperty)property);
                    break;
                case EdmPropertyKind.None:
                    this.ProcessProperty(property);
                    break;
                default:
                    throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_PropertyKind(property.PropertyKind.ToString()));
            }
        }

        public void VisitEnumMembers(IEnumerable<IEdmEnumMember> enumMembers)
        {
            VisitCollection(enumMembers, this.VisitEnumMember);
        }

        public void VisitEnumMember(IEdmEnumMember enumMember)
        {
            this.ProcessEnumMember(enumMember);
        }

        #endregion

        #region Function Related

        public void VisitFunctionParameters(IEnumerable<IEdmFunctionParameter> parameters)
        {
            VisitCollection(parameters, this.ProcessFunctionParameter);
        }

        #endregion

        protected static void VisitCollection<T>(IEnumerable<T> collection, Action<T> visitMethod)
        {
            foreach (T element in collection)
            {
                visitMethod(element);
            }
        }
        #endregion

        #region Process Methods

        protected virtual void ProcessModel(IEdmModel model)
        {
            this.ProcessElement(model);
            this.VisitSchemaElements(model.SchemaElements);
            this.VisitVocabularyAnnotations(model.VocabularyAnnotations);
        }

        #region Base Element Types

        protected virtual void ProcessElement(IEdmElement element)
        {
            this.VisitAnnotations(this.Model.DirectValueAnnotations(element));
        }

        protected virtual void ProcessNamedElement(IEdmNamedElement element)
        {
            this.ProcessElement(element);
        }

        protected virtual void ProcessSchemaElement(IEdmSchemaElement element)
        {
            this.ProcessVocabularyAnnotatable(element);
            this.ProcessNamedElement(element);
        }

        protected virtual void ProcessVocabularyAnnotatable(IEdmVocabularyAnnotatable annotatable)
        {
        }

        #endregion

        #region Type References

        protected virtual void ProcessComplexTypeReference(IEdmComplexTypeReference reference)
        {
            this.ProcessStructuredTypeReference(reference);
        }

        protected virtual void ProcessEntityTypeReference(IEdmEntityTypeReference reference)
        {
            this.ProcessStructuredTypeReference(reference);
        }

        protected virtual void ProcessEntityReferenceTypeReference(IEdmEntityReferenceTypeReference reference)
        {
            this.ProcessTypeReference(reference);
            this.ProcessEntityReferenceType(reference.EntityReferenceDefinition());
        }

        protected virtual void ProcessRowTypeReference(IEdmRowTypeReference reference)
        {
            this.ProcessStructuredTypeReference(reference);
            this.ProcessRowType(reference.RowDefinition());
        }

        protected virtual void ProcessCollectionTypeReference(IEdmCollectionTypeReference reference)
        {
            this.ProcessTypeReference(reference);
            this.ProcessCollectionType(reference.CollectionDefinition());
        }

        protected virtual void ProcessEnumTypeReference(IEdmEnumTypeReference reference)
        {
            this.ProcessTypeReference(reference);
        }

        protected virtual void ProcessBinaryTypeReference(IEdmBinaryTypeReference reference)
        {
            this.ProcessPrimitiveTypeReference(reference);
        }

        protected virtual void ProcessDecimalTypeReference(IEdmDecimalTypeReference reference)
        {
            this.ProcessPrimitiveTypeReference(reference);
        }

        protected virtual void ProcessSpatialTypeReference(IEdmSpatialTypeReference reference)
        {
            this.ProcessPrimitiveTypeReference(reference);
        }

        protected virtual void ProcessStringTypeReference(IEdmStringTypeReference reference)
        {
            this.ProcessPrimitiveTypeReference(reference);
        }

        protected virtual void ProcessTemporalTypeReference(IEdmTemporalTypeReference reference)
        {
            this.ProcessPrimitiveTypeReference(reference);
        }

        protected virtual void ProcessPrimitiveTypeReference(IEdmPrimitiveTypeReference reference)
        {
            this.ProcessTypeReference(reference);
        }

        protected virtual void ProcessStructuredTypeReference(IEdmStructuredTypeReference reference)
        {
            this.ProcessTypeReference(reference);
        }

        protected virtual void ProcessTypeReference(IEdmTypeReference element)
        {
            this.ProcessElement(element);
        }

        #endregion

        #region Terms

        protected virtual void ProcessTerm(IEdmTerm term)
        {
            // Do not visit NamedElement as that gets visited by other means.
        }

        protected virtual void ProcessValueTerm(IEdmValueTerm term)
        {
            this.ProcessSchemaElement(term);
            this.ProcessTerm(term);
            this.VisitTypeReference(term.Type);
        }

        #endregion

        #region Type Definitions

        protected virtual void ProcessComplexType(IEdmComplexType definition)
        {
            this.ProcessSchemaElement(definition);
            this.ProcessStructuredType(definition);
            this.ProcessSchemaType(definition);
        }

        protected virtual void ProcessEntityType(IEdmEntityType definition)
        {
            this.ProcessSchemaElement(definition);
            this.ProcessTerm(definition);
            this.ProcessStructuredType(definition);
            this.ProcessSchemaType(definition);
        }

        protected virtual void ProcessRowType(IEdmRowType definition)
        {
            this.ProcessElement(definition);
            this.ProcessStructuredType(definition);
        }

        protected virtual void ProcessCollectionType(IEdmCollectionType definition)
        {
            this.ProcessElement(definition);
            this.ProcessType(definition);
            this.VisitTypeReference(definition.ElementType);
        }

        protected virtual void ProcessEnumType(IEdmEnumType definition)
        {
            this.ProcessSchemaElement(definition);
            this.ProcessType(definition);
            this.ProcessSchemaType(definition);
            this.VisitEnumMembers(definition.Members);
        }

        protected virtual void ProcessEntityReferenceType(IEdmEntityReferenceType definition)
        {
            this.ProcessElement(definition);
            this.ProcessType(definition);
        }

        protected virtual void ProcessStructuredType(IEdmStructuredType definition)
        {
            this.ProcessType(definition);
            this.VisitProperties(definition.DeclaredProperties);
        }

        protected virtual void ProcessSchemaType(IEdmSchemaType type)
        {
            // Do not visit type or schema element, because all types will do that on thier own.
        }

        protected virtual void ProcessType(IEdmType definition)
        {
        }

        #endregion

        #region Definition Components

        protected virtual void ProcessNavigationProperty(IEdmNavigationProperty property)
        {
            this.ProcessProperty(property);
        }

        protected virtual void ProcessStructuralProperty(IEdmStructuralProperty property)
        {
            this.ProcessProperty(property);
        }

        protected virtual void ProcessProperty(IEdmProperty property)
        {
            this.ProcessVocabularyAnnotatable(property);
            this.ProcessNamedElement(property);
            this.VisitTypeReference(property.Type);
        }

        protected virtual void ProcessEnumMember(IEdmEnumMember enumMember)
        {
            this.ProcessNamedElement(enumMember);
        }

        #endregion

        #region Annotations

        protected virtual void ProcessVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
        {
            this.ProcessElement(annotation);
        }

        protected virtual void ProcessImmediateValueAnnotation(IEdmDirectValueAnnotation annotation)
        {
            this.ProcessNamedElement(annotation);
        }

        protected virtual void ProcessValueAnnotation(IEdmValueAnnotation annotation)
        {
            this.ProcessVocabularyAnnotation(annotation);
            this.VisitExpression(annotation.Value);
        }

        protected virtual void ProcessTypeAnnotation(IEdmTypeAnnotation annotation)
        {
            this.ProcessVocabularyAnnotation(annotation);
            this.VisitPropertyValueBindings(annotation.PropertyValueBindings);
        }

        protected virtual void ProcessPropertyValueBinding(IEdmPropertyValueBinding binding)
        {
            this.VisitExpression(binding.Value);
        }

        #endregion

        #region Expressions

        protected virtual void ProcessExpression(IEdmExpression expression)
        {
        }

        protected virtual void ProcessStringConstantExpression(IEdmStringConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessBinaryConstantExpression(IEdmBinaryConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessRecordExpression(IEdmRecordExpression expression)
        {
            this.ProcessExpression(expression);
            if (expression.DeclaredType != null)
            {
                this.VisitTypeReference(expression.DeclaredType);
            }

            this.VisitPropertyConstructors(expression.Properties);
        }

        protected virtual void ProcessPropertyReferenceExpression(IEdmPropertyReferenceExpression expression)
        {
            this.ProcessExpression(expression);
            if (expression.Base != null)
            {
                this.VisitExpression(expression.Base);
            }
        }

        protected virtual void ProcessPathExpression(IEdmPathExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessParameterReferenceExpression(IEdmParameterReferenceExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessCollectionExpression(IEdmCollectionExpression expression)
        {
            this.ProcessExpression(expression);
            this.VisitExpressions(expression.Elements);
        }

        protected virtual void ProcessLabeledExpressionReferenceExpression(IEdmLabeledExpressionReferenceExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessIsTypeExpression(IEdmIsTypeExpression expression)
        {
            this.ProcessExpression(expression);
            this.VisitTypeReference(expression.Type);
            this.VisitExpression(expression.Operand);
        }

        protected virtual void ProcessIntegerConstantExpression(IEdmIntegerConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessIfExpression(IEdmIfExpression expression)
        {
            this.ProcessExpression(expression);
            this.VisitExpression(expression.TestExpression);
            this.VisitExpression(expression.TrueExpression);
            this.VisitExpression(expression.FalseExpression);
        }

        protected virtual void ProcessFunctionReferenceExpression(IEdmFunctionReferenceExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessFunctionApplicationExpression(IEdmApplyExpression expression)
        {
            this.ProcessExpression(expression);
            this.VisitExpression(expression.AppliedFunction);
            this.VisitExpressions(expression.Arguments);
        }

        protected virtual void ProcessFloatingConstantExpression(IEdmFloatingConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessGuidConstantExpression(IEdmGuidConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessEnumMemberReferenceExpression(IEdmEnumMemberReferenceExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessEntitySetReferenceExpression(IEdmEntitySetReferenceExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessDecimalConstantExpression(IEdmDecimalConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessDateTimeConstantExpression(IEdmDateTimeConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessDateTimeOffsetConstantExpression(IEdmDateTimeOffsetConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessTimeConstantExpression(IEdmTimeConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessBooleanConstantExpression(IEdmBooleanConstantExpression expression)
        {
            this.ProcessExpression(expression);
        }

        protected virtual void ProcessAssertTypeExpression(IEdmAssertTypeExpression expression)
        {
            this.ProcessExpression(expression);
            this.VisitTypeReference(expression.Type);
            this.VisitExpression(expression.Operand);
        }

        protected virtual void ProcessLabeledExpression(IEdmLabeledExpression element)
        {
            this.VisitExpression(element.Expression);
        }

        protected virtual void ProcessPropertyConstructor(IEdmPropertyConstructor constructor)
        {
            this.VisitExpression(constructor.Value);
        }

        protected virtual void ProcessNullConstantExpression(IEdmNullExpression expression)
        {
            this.ProcessExpression(expression);
        }

        #endregion

        #region Data Model

        protected virtual void ProcessEntityContainer(IEdmEntityContainer container)
        {
            this.ProcessVocabularyAnnotatable(container);
            this.ProcessNamedElement(container);
            this.VisitEntityContainerElements(container.Elements);
        }

        protected virtual void ProcessEntityContainerElement(IEdmEntityContainerElement element)
        {
            this.ProcessNamedElement(element);
        }

        protected virtual void ProcessEntitySet(IEdmEntitySet set)
        {
            this.ProcessEntityContainerElement(set);
        }

        #endregion

        #region Function Related
        protected virtual void ProcessFunction(IEdmFunction function)
        {
            this.ProcessSchemaElement(function);
            this.ProcessFunctionBase(function);
        }

        protected virtual void ProcessFunctionImport(IEdmFunctionImport functionImport)
        {
            this.ProcessEntityContainerElement(functionImport);
            this.ProcessFunctionBase(functionImport);
        }

        protected virtual void ProcessFunctionBase(IEdmFunctionBase functionBase)
        {
            if (functionBase.ReturnType != null)
            {
                this.VisitTypeReference(functionBase.ReturnType);
            }
            
            // Do not visit vocabularyAnnotatable because functions and function imports are always going to be either a schema element or a container element and will be visited through those paths.
            this.VisitFunctionParameters(functionBase.Parameters);
        }

        protected virtual void ProcessFunctionParameter(IEdmFunctionParameter parameter)
        {
            this.ProcessVocabularyAnnotatable(parameter);
            this.ProcessNamedElement(parameter);
            this.VisitTypeReference(parameter.Type);
        }

        #endregion

        #endregion
    }
}

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
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Common;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing
{
    /// <summary>
    /// CSDL document parser.
    /// </summary>
    internal class CsdlDocumentParser : EdmXmlDocumentParser<CsdlSchema>
    {
        private Version artifactVersion;

        internal CsdlDocumentParser(string documentPath, XmlReader reader)
            : base(documentPath, reader)
        {
        }

        internal override IEnumerable<KeyValuePair<Version, string>> SupportedVersions
        {
            get { return CsdlConstants.SupportedVersions.SelectMany(kvp => kvp.Value.Select(ns => new KeyValuePair<Version, string>(kvp.Key, ns))); }
        }

        protected override bool TryGetDocumentElementParser(Version csdlArtifactVersion, XmlElementInfo rootElement, out XmlElementParser<CsdlSchema> parser)
        {
            EdmUtil.CheckArgumentNull(rootElement, "rootElement");
            this.artifactVersion = csdlArtifactVersion;
            if (string.Equals(rootElement.Name, CsdlConstants.Element_Schema, StringComparison.Ordinal))
            {
                parser = this.CreateRootElementParser();
                return true;
            }

            parser = null;
            return false;
        }

        protected override void AnnotateItem(object result, XmlElementValueCollection childValues)
        {
            CsdlElement annotatedItem = result as CsdlElement;
            if (annotatedItem == null)
            {
                return;
            }

            foreach (var xmlAnnotation in this.currentElement.Annotations)
            {
                annotatedItem.AddAnnotation(new CsdlDirectValueAnnotation(xmlAnnotation.NamespaceName, xmlAnnotation.Name, xmlAnnotation.Value, xmlAnnotation.IsAttribute, xmlAnnotation.Location));
            }

            foreach (var annotation in childValues.ValuesOfType<CsdlVocabularyAnnotationBase>())
            {
                annotatedItem.AddAnnotation(annotation);
            }
        }

        private void AddChildParsers(XmlElementParser parent, IEnumerable<XmlElementParser> children)
        {
            foreach (XmlElementParser child in children)
            {
                parent.AddChildParser(child);
            }
        }

        private XmlElementParser<CsdlSchema> CreateRootElementParser()
        {
            var documentationParser =
                //// <Documentation>
                CsdlElement<CsdlDocumentation>(CsdlConstants.Element_Documentation, this.OnDocumentationElement,
                    //// <Summary/>
                   Element(CsdlConstants.Element_Summary, (element, children) => children.FirstText.Value),
                    //// <LongDescription/>
                   Element(CsdlConstants.Element_LongDescription, (element, children) => children.FirstText.TextValue));
                //// </Documentation>

            // There is recursion in the grammar between RowType, CollectionType, ReturnType, and Property within RowType.
            // This requires breaking up the parser construction into pieces and then weaving them together with AddChildParser.
            var referenceTypeParser =
                //// <ReferenceType/>
                CsdlElement<CsdlTypeReference>(CsdlConstants.Element_ReferenceType, this.OnEntityReferenceTypeElement, documentationParser);

            var rowTypeParser =
                //// <RowType/>
                CsdlElement<CsdlTypeReference>(CsdlConstants.Element_RowType, this.OnRowTypeElement);

            var collectionTypeParser =
                //// <CollectionType>
                CsdlElement<CsdlTypeReference>(CsdlConstants.Element_CollectionType, this.OnCollectionTypeElement, documentationParser,
                    //// <TypeRef/>
                    CsdlElement<CsdlTypeReference>(CsdlConstants.Element_TypeRef, this.OnTypeRefElement, documentationParser),
                    //// <RowType/>
                    rowTypeParser,
                    //// <ReferenceType/>
                    referenceTypeParser);
                //// </CollectionType>

            var nominalTypePropertyElementParser =
                //// <Property/>
                CsdlElement<CsdlProperty>(CsdlConstants.Element_Property, this.OnPropertyElement, documentationParser);

            var rowTypePropertyElementParser =
                //// <Property>
                CsdlElement<CsdlProperty>(CsdlConstants.Element_Property, this.OnPropertyElement, documentationParser,
                //// <TypeRef/>
                    CsdlElement<CsdlTypeReference>(CsdlConstants.Element_TypeRef, this.OnTypeRefElement, documentationParser),
                //// <RowType/>
                    rowTypeParser,
                //// <CollectionType/>
                    collectionTypeParser,
                //// <ReferenceType/>
                    referenceTypeParser);
            //// </Property>

            var stringConstantExpressionParser =
                //// <String/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_String, OnStringConstantExpression);

            var binaryConstantExpressionParser =
                //// <Binary/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Binary, OnBinaryConstantExpression);

            var intConstantExpressionParser =
                //// <Int/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Int, OnIntConstantExpression);

            var floatConstantExpressionParser =
                //// <Float/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Float, OnFloatConstantExpression);

            var guidConstantExpressionParser =
                //// <Guid/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Guid, OnGuidConstantExpression);

            var decimalConstantExpressionParser =
                //// <Decimal/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Decimal, OnDecimalConstantExpression);

            var boolConstantExpressionParser =
                //// <Bool/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Bool, OnBoolConstantExpression);

            var dateTimeConstantExpressionParser =
                //// <DateTime/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_DateTime, OnDateTimeConstantExpression);

            var timeConstantExpressionParser =
                //// <Time/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Time, OnTimeConstantExpression);

            var dateTimeOffsetConstantExpressionParser =
                //// <DateTimeOffset/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_DateTimeOffset, OnDateTimeOffsetConstantExpression);

            var nullConstantExpressionParser =
                //// <Null/>
               CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Null, OnNullConstantExpression);

            var pathExpressionParser =
                //// <Path/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Path, OnPathExpression);

            var functionReferenceExpressionParser =
                //// <FunctionReference/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_FunctionReference, this.OnFunctionReferenceExpression);

            var parameterReferenceExpressionParser =
                //// <ParameterReference/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_ParameterReference, this.OnParameterReferenceExpression);

            var entitySetReferenceExpressionParser =
                //// <EntitySetReference/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_EntitySetReference, this.OnEntitySetReferenceExpression);

            var enumMemberReferenceExpressionParser =
                //// <EnumMemberReference/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_EnumMemberReference, this.OnEnumMemberReferenceExpression);

            var propertyReferenceExpressionParser =
            //// <PropertyReference>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_PropertyReference, this.OnPropertyReferenceExpression);


            var ifExpressionParser =
                //// <If>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_If, this.OnIfExpression);

            var assertTypeExpressionParser =
                //// <AssertType>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_AssertType, this.OnAssertTypeExpression);

            var isTypeExpressionParser =
                //// <IsType>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_IsType, this.OnIsTypeExpression);
                
            var propertyValueParser =
                //// <PropertyValue>
                CsdlElement<CsdlPropertyValue>(CsdlConstants.Element_PropertyValue, this.OnPropertyValueElement);

            var recordExpressionParser =
                //// <Record>
                CsdlElement<CsdlRecordExpression>(CsdlConstants.Element_Record, this.OnRecordElement,
                    //// <PropertyValue />
                    propertyValueParser);
                //// </Record>

            var labeledElementParser =
                //// <LabeledElement>
                CsdlElement<CsdlLabeledExpression>(CsdlConstants.Element_LabeledElement, this.OnLabeledElement);

            var collectionExpressionParser =
                //// <Collection>
                CsdlElement<CsdlCollectionExpression>(CsdlConstants.Element_Collection, this.OnCollectionElement);

            var applyExpressionParser =
                //// <Apply>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_Apply, this.OnApplyElement);

            var labeledElementReferenceExpressionParser =
                //// <LabeledElementReference/>
                CsdlElement<CsdlExpressionBase>(CsdlConstants.Element_LabeledElementReference, this.OnLabeledElementReferenceExpression);

            XmlElementParser[] expressionParsers = 
            {
                //// <String/>
                stringConstantExpressionParser,
                //// <Binary/>
                binaryConstantExpressionParser,
                //// <Int/>
                intConstantExpressionParser,
                //// <Float/>
                floatConstantExpressionParser,
                //// <Guid/>
                guidConstantExpressionParser,
                //// <Decimal/>
                decimalConstantExpressionParser,
                //// <Bool/>
                boolConstantExpressionParser,
                //// <DateTime/>
                dateTimeConstantExpressionParser,
                //// <DateTimeOffset/>
                dateTimeOffsetConstantExpressionParser,
                //// <Time/>
                timeConstantExpressionParser,
                //// <Null/>
                nullConstantExpressionParser,
                //// <Path/>
                pathExpressionParser,
                //// <If/>
                ifExpressionParser,
                //// <IsType/>
                isTypeExpressionParser,
                //// <AssertType>
                assertTypeExpressionParser,
                //// <Record/>
                recordExpressionParser,
                //// <Collection/>
                collectionExpressionParser,
                //// <LabeledElementReference/>
                labeledElementReferenceExpressionParser,
                //// <PropertyReference/>
                propertyReferenceExpressionParser,
                //// <PropertyValue/>
                propertyValueParser,
                //// <LabeledElement/>
                labeledElementParser,
                //// <FunctionReference/>
                functionReferenceExpressionParser,
                //// <EntitySetReference/>
                entitySetReferenceExpressionParser,
                //// <EnumConstantReference/>
                enumMemberReferenceExpressionParser,
                //// <ParameterReference/>
                parameterReferenceExpressionParser,
                //// </Apply>
                applyExpressionParser
            };

            AddChildParsers(propertyReferenceExpressionParser, expressionParsers);
            AddChildParsers(ifExpressionParser, expressionParsers);
            AddChildParsers(assertTypeExpressionParser, expressionParsers);
            AddChildParsers(isTypeExpressionParser, expressionParsers);
            AddChildParsers(propertyValueParser, expressionParsers);
            AddChildParsers(collectionExpressionParser, expressionParsers);
            AddChildParsers(labeledElementParser, expressionParsers);
            AddChildParsers(applyExpressionParser, expressionParsers);
            
            var valueAnnotationParser =
                //// <ValueAnnotation>
                CsdlElement<CsdlValueAnnotation>(CsdlConstants.Element_ValueAnnotation, this.OnValueAnnotationElement);

             AddChildParsers(valueAnnotationParser, expressionParsers);
                
            var typeAnnotationParser =
                //// <TypeAnnotation>
                CsdlElement<CsdlTypeAnnotation>(CsdlConstants.Element_TypeAnnotation, this.OnTypeAnnotationElement,
                    //// <PropertyValue />
                    propertyValueParser);
                //// </TypeAnnotation>

            nominalTypePropertyElementParser.AddChildParser(valueAnnotationParser);
            nominalTypePropertyElementParser.AddChildParser(typeAnnotationParser);

            rowTypePropertyElementParser.AddChildParser(valueAnnotationParser);
            rowTypePropertyElementParser.AddChildParser(typeAnnotationParser);
            rowTypeParser.AddChildParser(rowTypePropertyElementParser);

            collectionTypeParser.AddChildParser(collectionTypeParser);

            var rootElementParser =
            //// <Schema>
            CsdlElement<CsdlSchema>(CsdlConstants.Element_Schema, this.OnSchemaElement,
                documentationParser,
                //// <Using/>
                CsdlElement<CsdlUsing>(CsdlConstants.Element_Using, this.OnUsingElement),
                //// <ComplexType>
                CsdlElement<CsdlComplexType>(CsdlConstants.Element_ComplexType, this.OnComplexTypeElement,
                    documentationParser,
                    //// <Property />
                    nominalTypePropertyElementParser,
                    //// <ValueAnnotation/>
                    valueAnnotationParser,
                    //// <TypeAnnotation/>
                    typeAnnotationParser),
                //// </ComplexType>
              
                //// <EntityType>
                CsdlElement<CsdlEntityType>(CsdlConstants.Element_EntityType, this.OnEntityTypeElement,
                    documentationParser,
                    //// <Key>
                    CsdlElement<CsdlKey>(CsdlConstants.Element_Key, OnEntityKeyElement,
                        //// <PropertyRef/>
                        CsdlElement<CsdlPropertyReference>(CsdlConstants.Element_PropertyRef, this.OnPropertyRefElement)),
                    //// </Key>
                    
                    //// <Property />
                    nominalTypePropertyElementParser,

                    //// <NavigationProperty>
                    CsdlElement<CsdlNavigationProperty>(CsdlConstants.Element_NavigationProperty, this.OnNavigationPropertyElement, documentationParser,
                        //// <ValueAnnotation/>
                        valueAnnotationParser,
                        //// <TypeAnnotation/>
                        typeAnnotationParser),
                    //// </NavigationProperty>

                    //// <ValueAnnotation/>
                    valueAnnotationParser,
                    //// <TypeAnnotation/>
                    typeAnnotationParser),
                //// </EntityType>
                
                //// <Association>
                CsdlElement<CsdlAssociation>(CsdlConstants.Element_Association, this.OnAssociationElement,
                    documentationParser,
                    //// <End>
                    CsdlElement<CsdlAssociationEnd>(CsdlConstants.Element_End, this.OnAssociationEndElement,
                        documentationParser,
                        //// <OnDelete/>
                        CsdlElement<CsdlOnDelete>(CsdlConstants.Element_OnDelete, this.OnDeleteActionElement, documentationParser)),
                    //// </End>
                    
                    //// <ReferentialConstraint>
                    CsdlElement<CsdlReferentialConstraint>(CsdlConstants.Element_ReferentialConstraint, this.OnReferentialConstraintElement,
                        documentationParser,
                        //// <Principal>
                        CsdlElement<CsdlReferentialConstraintRole>(CsdlConstants.Element_Principal, this.OnReferentialConstraintRoleElement,
                            documentationParser,
                            //// <PropertyRef/>
                            CsdlElement<CsdlPropertyReference>(CsdlConstants.Element_PropertyRef, this.OnPropertyRefElement)),
                        //// </Principal>
                        
                        //// <Dependent>
                        CsdlElement<CsdlReferentialConstraintRole>(CsdlConstants.Element_Dependent, this.OnReferentialConstraintRoleElement,
                            documentationParser,
                            //// <PropertyRef/>
                            CsdlElement<CsdlPropertyReference>(CsdlConstants.Element_PropertyRef, this.OnPropertyRefElement)))),
                        //// </Dependent> 
                    //// </ReferentialConstraint>
                //// </Association>
                
                //// <EnumType>
                CsdlElement<CsdlEnumType>(CsdlConstants.Element_EnumType, this.OnEnumTypeElement,
                    documentationParser,
                    //// <Member>
                    CsdlElement<CsdlEnumMember>(CsdlConstants.Element_Member, this.OnEnumMemberElement, documentationParser),
                    //// <ValueAnnotation/>
                    valueAnnotationParser,
                    //// <TypeAnnotation/>
                    typeAnnotationParser),
                //// </EnumType>
                
                //// <Function>
                CsdlElement<CsdlFunction>(CsdlConstants.Element_Function, this.OnFunctionElement,
                    documentationParser,
                    //// <Parameter>
                    CsdlElement<CsdlFunctionParameter>(CsdlConstants.Element_Parameter, this.OnParameterElement,
                        documentationParser,
                        //// <TypeRef/>
                        CsdlElement<CsdlTypeReference>(CsdlConstants.Element_TypeRef, this.OnTypeRefElement, documentationParser),
                        //// <RowType/>
                        rowTypeParser,
                        //// <CollectionType/>
                        collectionTypeParser,
                        //// <ReferenceType/>
                        referenceTypeParser,
                        //// <ValueAnnotation/>
                        valueAnnotationParser,
                        //// <TypeAnnotation/>
                        typeAnnotationParser),
                    //// </Parameter
                    
                    //// <DefiningExpression/>
                    Element(CsdlConstants.Element_DefiningExpression, (element, children) => children.FirstText.Value),
                    //// <ReturnType>
                    CsdlElement<CsdlFunctionReturnType>(CsdlConstants.Element_ReturnType, this.OnReturnTypeElement,
                        documentationParser,
                        //// <TypeRef/>
                        CsdlElement<CsdlTypeReference>(CsdlConstants.Element_TypeRef, this.OnTypeRefElement, documentationParser),
                        //// <RowType/>
                        rowTypeParser,
                        //// <CollectionType/>
                        collectionTypeParser,
                        //// <ReferenceType/>
                        referenceTypeParser),
                    //// </ReturnType>

                    //// <ValueAnnotation/>
                    valueAnnotationParser,
                    //// <TypeAnnotation>
                    typeAnnotationParser),
                //// </Function>
                
                //// <ValueTerm>
                CsdlElement<CsdlValueTerm>(CsdlConstants.Element_ValueTerm, this.OnValueTermElement,
                    //// <TypeRef/>
                    CsdlElement<CsdlTypeReference>(CsdlConstants.Element_TypeRef, this.OnTypeRefElement, documentationParser),
                    //// <RowType/>
                    rowTypeParser,
                    //// <CollectionType/>
                    collectionTypeParser,
                    //// <ReferenceType/>
                    referenceTypeParser,
                    //// <ValueAnnotation/>
                    valueAnnotationParser,
                    //// <TypeAnnotation/>
                    typeAnnotationParser),
                //// </ValueTerm

                //// <Annotations>
                CsdlElement<CsdlAnnotations>(CsdlConstants.Element_Annotations, this.OnAnnotationsElement,
                    //// <ValueAnnotation/>
                    valueAnnotationParser,
                    //// <TypeAnnotation/>
                    typeAnnotationParser),
                //// </Annotations>

                //// <EntityContainer>
                CsdlElement<CsdlEntityContainer>(CsdlConstants.Element_EntityContainer, this.OnEntityContainerElement,
                    documentationParser,
                    //// <EntitySet>
                    CsdlElement<CsdlEntitySet>(CsdlConstants.Element_EntitySet, this.OnEntitySetElement, documentationParser,
                        //// <ValueAnnotation/>
                        valueAnnotationParser,
                        //// <TypeAnnotation/>
                        typeAnnotationParser),
                    //// </EntitySet>

                    //// <AssociationSet>
                    CsdlElement<CsdlAssociationSet>(CsdlConstants.Element_AssociationSet, this.OnAssociationSetElement,
                        documentationParser,
                        //// <End/>
                        CsdlElement<CsdlAssociationSetEnd>(CsdlConstants.Element_End, this.OnAssociationSetEndElement, documentationParser)),
                    //// </AssociationSet>
                    
                    //// <Function Import
                    CsdlElement<CsdlFunctionImport>(CsdlConstants.Element_FunctionImport, this.OnFunctionImportElement,
                        documentationParser,

                        //// <Parameter />
                        CsdlElement<CsdlFunctionParameter>(CsdlConstants.Element_Parameter, this.OnFunctionImportParameterElement, documentationParser,
                            //// <ValueAnnotation/>
                            valueAnnotationParser,
                            //// <TypeAnnotation/>
                            typeAnnotationParser),
                        ////</Parameter>

                        //// <ValueAnnotation/>
                        valueAnnotationParser,
                        //// <TypeAnnotation/>
                        typeAnnotationParser),
                    ////</FunctionImport
                    
                    //// <ValueAnnotation/>
                    valueAnnotationParser,
                    //// <TypeAnnotation/>
                    typeAnnotationParser));
                ////</EntityContainer>
            //// </Schema>

            return rootElementParser;
        }

        private static CsdlDocumentation Documentation(XmlElementValueCollection childValues)
        {
            return childValues.ValuesOfType<CsdlDocumentation>().FirstOrDefault();
        }

        private CsdlSchema OnSchemaElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string namespaceName = Optional(CsdlConstants.Attribute_Namespace) ?? string.Empty;
            string alias = OptionalAlias(CsdlConstants.Attribute_Alias);

            CsdlSchema result =
                new CsdlSchema(
                    namespaceName,
                    alias,
                    this.artifactVersion,
                    childValues.ValuesOfType<CsdlUsing>(),
                    childValues.ValuesOfType<CsdlAssociation>(),
                    childValues.ValuesOfType<CsdlStructuredType>(),
                    childValues.ValuesOfType<CsdlEnumType>(),
                    childValues.ValuesOfType<CsdlFunction>(),
                    childValues.ValuesOfType<CsdlValueTerm>(),
                    childValues.ValuesOfType<CsdlEntityContainer>(),
                    childValues.ValuesOfType<CsdlAnnotations>(),
                    Documentation(childValues),
                    element.Location);

            return result;
        }

        private CsdlDocumentation OnDocumentationElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return new CsdlDocumentation(childValues[CsdlConstants.Element_Summary].TextValue, childValues[CsdlConstants.Element_LongDescription].TextValue, element.Location);
        }

        private CsdlUsing OnUsingElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string namespaceName = Required(CsdlConstants.Attribute_Namespace);
            string alias = RequiredAlias(CsdlConstants.Attribute_Alias);

            return new CsdlUsing(namespaceName, alias, Documentation(childValues), element.Location);
        }

        private CsdlComplexType OnComplexTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string baseType = OptionalQualifiedName(CsdlConstants.Attribute_BaseType);
            bool isAbstract = OptionalBoolean(CsdlConstants.Attribute_Abstract) ?? CsdlConstants.Default_Abstract;

            return new CsdlComplexType(name, baseType, isAbstract, childValues.ValuesOfType<CsdlProperty>(), Documentation(childValues), element.Location);
        }

        private CsdlEntityType OnEntityTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string baseType = OptionalQualifiedName(CsdlConstants.Attribute_BaseType);
            bool isOpen = OptionalBoolean(CsdlConstants.Attribute_OpenType) ?? CsdlConstants.Default_OpenType;
            bool isAbstract = OptionalBoolean(CsdlConstants.Attribute_Abstract) ?? CsdlConstants.Default_Abstract;

            CsdlKey key = childValues.ValuesOfType<CsdlKey>().FirstOrDefault();

            return new CsdlEntityType(name, baseType, isAbstract, isOpen, key, childValues.ValuesOfType<CsdlProperty>(), childValues.ValuesOfType<CsdlNavigationProperty>(), Documentation(childValues), element.Location);
        }

        private CsdlProperty OnPropertyElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = OptionalType(CsdlConstants.Attribute_Type);
            CsdlTypeReference type = this.ParseTypeReference(typeName, childValues, element.Location, Optionality.Required);
            string name = Required(CsdlConstants.Attribute_Name);
            string defaultValue = Optional(CsdlConstants.Attribute_DefaultValue);
            EdmConcurrencyMode? concurrencyMode = OptionalConcurrencyMode(CsdlConstants.Attribute_ConcurrencyMode);
            bool isFixedConcurrency = (concurrencyMode ?? EdmConcurrencyMode.None) == EdmConcurrencyMode.Fixed;

            return new CsdlProperty(name, type, isFixedConcurrency, defaultValue, Documentation(childValues), element.Location);
        }

        private CsdlValueTerm OnValueTermElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = OptionalType(CsdlConstants.Attribute_Type);
            CsdlTypeReference type = this.ParseTypeReference(typeName, childValues, element.Location, Optionality.Required);
            string name = Required(CsdlConstants.Attribute_Name);

            return new CsdlValueTerm(name, type, Documentation(childValues), element.Location);
        }

        private CsdlAnnotations OnAnnotationsElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string target = Required(CsdlConstants.Attribute_Target);
            string qualifier = Optional(CsdlConstants.Attribute_Qualifier);
            IEnumerable<CsdlVocabularyAnnotationBase> annotations = childValues.ValuesOfType<CsdlVocabularyAnnotationBase>();

            return new CsdlAnnotations(annotations, target, qualifier);
        }

        private CsdlValueAnnotation OnValueAnnotationElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string term = RequiredQualifiedName(CsdlConstants.Attribute_Term);
            string qualifier = Optional(CsdlConstants.Attribute_Qualifier);
            CsdlExpressionBase expression = this.ParseAnnotationExpression(element, childValues);

            return new CsdlValueAnnotation(term, qualifier, expression, element.Location);
        }

        private CsdlTypeAnnotation OnTypeAnnotationElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string term = RequiredQualifiedName(CsdlConstants.Attribute_Term);
            string qualifier = Optional(CsdlConstants.Attribute_Qualifier);
            IEnumerable<CsdlPropertyValue> properties = childValues.ValuesOfType<CsdlPropertyValue>();

            return new CsdlTypeAnnotation(term, qualifier, properties, element.Location);
        }

        private CsdlPropertyValue OnPropertyValueElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string property = Required(CsdlConstants.Attribute_Property);
            CsdlExpressionBase expression = this.ParseAnnotationExpression(element, childValues);

            return new CsdlPropertyValue(property, expression, element.Location);
        }

        private CsdlRecordExpression OnRecordElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string type = OptionalQualifiedName(CsdlConstants.Attribute_Type);
            IEnumerable<CsdlPropertyValue> propertyValues = childValues.ValuesOfType<CsdlPropertyValue>();

            return new CsdlRecordExpression(type != null ? new CsdlNamedTypeReference(type, false, element.Location) : null, propertyValues, element.Location);
        }

        private CsdlCollectionExpression OnCollectionElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = OptionalType(CsdlConstants.Attribute_Type);
            CsdlTypeReference type = this.ParseTypeReference(typeName, childValues, element.Location, Optionality.Optional);
            IEnumerable<CsdlExpressionBase> elementValues = childValues.ValuesOfType<CsdlExpressionBase>();

            return new CsdlCollectionExpression(type, elementValues, element.Location);
        }

        private CsdlLabeledExpression OnLabeledElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            IEnumerable<CsdlExpressionBase> expressions = childValues.ValuesOfType<CsdlExpressionBase>();
            if (expressions.Count() != 1)
            {
                this.ReportError(element.Location, EdmErrorCode.InvalidLabeledElementExpressionIncorrectNumberOfOperands, Edm.Strings.CsdlParser_InvalidLabeledElementExpressionIncorrectNumberOfOperands);
            }

            return new CsdlLabeledExpression(
                name,
                expressions.ElementAtOrDefault(0),
                element.Location);
        }

        private CsdlApplyExpression OnApplyElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string function = Optional(CsdlConstants.Attribute_Function);
            IEnumerable<CsdlExpressionBase> arguments = childValues.ValuesOfType<CsdlExpressionBase>();

            return new CsdlApplyExpression(function, arguments, element.Location);
        }

        private static CsdlConstantExpression ConstantExpression(EdmValueKind kind, XmlElementValueCollection childValues, CsdlLocation location)
        {
            XmlTextValue text = childValues.FirstText;
            return new CsdlConstantExpression(kind, text != null ? text.TextValue : string.Empty, location);
        }

        private static CsdlConstantExpression OnIntConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Integer, childValues, element.Location);
        }

        private static CsdlConstantExpression OnStringConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.String, childValues, element.Location);
        }

        private static CsdlConstantExpression OnBinaryConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Binary, childValues, element.Location);
        }

        private static CsdlConstantExpression OnFloatConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Floating, childValues, element.Location);
        }

        private static CsdlConstantExpression OnGuidConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Guid, childValues, element.Location);
        }

        private static CsdlConstantExpression OnDecimalConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Decimal, childValues, element.Location);
        }

        private static CsdlConstantExpression OnBoolConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Boolean, childValues, element.Location);
        }

        private static CsdlConstantExpression OnTimeConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Time, childValues, element.Location);
        }

        private static CsdlConstantExpression OnDateTimeConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.DateTime, childValues, element.Location);
        }

        private static CsdlConstantExpression OnDateTimeOffsetConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.DateTimeOffset, childValues, element.Location);
        }

        private static CsdlConstantExpression OnNullConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return ConstantExpression(EdmValueKind.Null, childValues, element.Location);
        }

        private static CsdlPathExpression OnPathExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            XmlTextValue text = childValues.FirstText;
            return new CsdlPathExpression(text != null ? text.TextValue : string.Empty, element.Location);
        }

        private CsdlLabeledExpressionReferenceExpression OnLabeledElementReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            return new CsdlLabeledExpressionReferenceExpression(name, element.Location);
        }

        private CsdlEntitySetReferenceExpression OnEntitySetReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string entitySetPath = RequiredEntitySetPath(CsdlConstants.Attribute_Name);
            return new CsdlEntitySetReferenceExpression(entitySetPath, element.Location);
        }

        private CsdlEnumMemberReferenceExpression OnEnumMemberReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string memberPath = RequiredEnumMemberPath(CsdlConstants.Attribute_Name);
            return new CsdlEnumMemberReferenceExpression(memberPath, element.Location);
        }

        private CsdlPropertyReferenceExpression OnPropertyReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            return new CsdlPropertyReferenceExpression(
                name,
                childValues.ValuesOfType<CsdlExpressionBase>().FirstOrDefault(),
                element.Location);
        }

        private CsdlFunctionReferenceExpression OnFunctionReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = RequiredQualifiedName(CsdlConstants.Attribute_Name);
            return new CsdlFunctionReferenceExpression(name, element.Location);
        }

        private CsdlParameterReferenceExpression OnParameterReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            return new CsdlParameterReferenceExpression(name, element.Location);
        }

        private CsdlExpressionBase OnIfExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            IEnumerable<CsdlExpressionBase> expressions = childValues.ValuesOfType<CsdlExpressionBase>();
            if (expressions.Count() != 3)
            {
                this.ReportError(element.Location, EdmErrorCode.InvalidIfExpressionIncorrectNumberOfOperands, Edm.Strings.CsdlParser_InvalidIfExpressionIncorrectNumberOfOperands);
            }

            return new CsdlIfExpression(
                expressions.ElementAtOrDefault(0),
                expressions.ElementAtOrDefault(1),
                expressions.ElementAtOrDefault(2),
                element.Location);
        }

        private CsdlExpressionBase OnAssertTypeExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = OptionalType(CsdlConstants.Attribute_Type);
            CsdlTypeReference type = this.ParseTypeReference(typeName, childValues, element.Location, Optionality.Required);

            IEnumerable<CsdlExpressionBase> expressions = childValues.ValuesOfType<CsdlExpressionBase>();
            if (expressions.Count() != 1)
            {
                this.ReportError(element.Location, EdmErrorCode.InvalidAssertTypeExpressionIncorrectNumberOfOperands, Edm.Strings.CsdlParser_InvalidAssertTypeExpressionIncorrectNumberOfOperands);
            }

            return new CsdlAssertTypeExpression(type, expressions.ElementAtOrDefault(0), element.Location);
        }

        private CsdlExpressionBase OnIsTypeExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = OptionalType(CsdlConstants.Attribute_Type);
            CsdlTypeReference type = this.ParseTypeReference(typeName, childValues, element.Location, Optionality.Required);

            IEnumerable<CsdlExpressionBase> expressions = childValues.ValuesOfType<CsdlExpressionBase>();
            if (expressions.Count() != 1)
            {
                this.ReportError(element.Location, EdmErrorCode.InvalidIsTypeExpressionIncorrectNumberOfOperands, Edm.Strings.CsdlParser_InvalidIsTypeExpressionIncorrectNumberOfOperands);
            }

            return new CsdlIsTypeExpression(type, expressions.ElementAtOrDefault(0), element.Location);
        }

        private CsdlExpressionBase ParseAnnotationExpression(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            CsdlExpressionBase expression = childValues.ValuesOfType<CsdlExpressionBase>().FirstOrDefault();
            if (expression != null)
            {
                return expression;
            }

            string pathValue = Optional(CsdlConstants.Attribute_Path);
            if (pathValue != null)
            {
                return new CsdlPathExpression(pathValue, element.Location);
            }

            EdmValueKind kind;

            string value = Optional(CsdlConstants.Attribute_String);
            if (value != null)
            {
                kind = EdmValueKind.String;
            }
            else
            {
                value = Optional(CsdlConstants.Attribute_Bool);
                if (value != null)
                {
                    kind = EdmValueKind.Boolean;
                }
                else
                {
                    value = Optional(CsdlConstants.Attribute_Int);
                    if (value != null)
                    {
                        kind = EdmValueKind.Integer;
                    }
                    else
                    {
                        value = Optional(CsdlConstants.Attribute_Float);
                        if (value != null)
                        {
                            kind = EdmValueKind.Floating;
                        }
                        else
                        {
                            value = Optional(CsdlConstants.Attribute_DateTime);
                            if (value != null)
                            {
                                kind = EdmValueKind.DateTime;
                            }
                            else
                            {
                                value = Optional(CsdlConstants.Attribute_DateTimeOffset);
                                if (value != null)
                                {
                                    kind = EdmValueKind.DateTimeOffset;
                                }
                                else
                                {
                                    value = Optional(CsdlConstants.Attribute_Time);
                                    if (value != null)
                                    {
                                        kind = EdmValueKind.Time;
                                    }
                                    else
                                    {
                                        value = Optional(CsdlConstants.Attribute_Decimal);
                                        if (value != null)
                                        {
                                            kind = EdmValueKind.Decimal;
                                        }
                                        else
                                        {
                                            value = Optional(CsdlConstants.Attribute_Binary);
                                            if (value != null)
                                            {
                                                kind = EdmValueKind.Binary;
                                            }
                                            else
                                            {
                                                value = Optional(CsdlConstants.Attribute_Guid);
                                                if (value != null)
                                                {
                                                    kind = EdmValueKind.Guid;
                                                }
                                                else
                                                {
                                                    //// Annotation expressions are always optional.
                                                    return null;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return new CsdlConstantExpression(kind, value, element.Location);
        }

        private CsdlNamedTypeReference ParseNamedTypeReference(string typeName, bool isNullable, CsdlLocation parentLocation)
        {
            var edm = Microsoft.Data.Edm.Library.EdmCoreModel.Instance;
            EdmPrimitiveTypeKind kind = edm.GetPrimitiveTypeKind(typeName);
            switch (kind)
            {
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
                    return new CsdlPrimitiveTypeReference(kind, typeName, isNullable, parentLocation);

                case EdmPrimitiveTypeKind.Binary:
                    {
                        int? maxLength;
                        bool isUnbounded;
                        bool? isFixedLength;
                        this.ParseBinaryFacets(out isUnbounded, out maxLength, out isFixedLength);

                        return new CsdlBinaryTypeReference(isFixedLength, isUnbounded, maxLength, typeName, isNullable, parentLocation);
                    }

                case EdmPrimitiveTypeKind.DateTime:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Time:
                    {
                        int? precision;
                        this.ParseTemporalFacets(out precision);

                        return new CsdlTemporalTypeReference(kind, precision, typeName, isNullable, parentLocation);
                    }

                case EdmPrimitiveTypeKind.Decimal:
                    {
                        int? precision;
                        int? scale;
                        this.ParseDecimalFacets(out precision, out scale);

                        return new CsdlDecimalTypeReference(precision, scale, typeName, isNullable, parentLocation);
                    }

                case EdmPrimitiveTypeKind.String:
                    {
                        int? maxLength;
                        bool isUnbounded;
                        bool? isFixedLength;
                        bool? isUnicode;
                        string collation;
                        this.ParseStringFacets(out isUnbounded, out maxLength, out isFixedLength, out isUnicode, out collation);

                        return new CsdlStringTypeReference(isFixedLength, isUnbounded, maxLength, isUnicode, collation, typeName, isNullable, parentLocation);
                    }

                case EdmPrimitiveTypeKind.Geography:
                case EdmPrimitiveTypeKind.GeographyPoint:
                case EdmPrimitiveTypeKind.GeographyLineString:
                case EdmPrimitiveTypeKind.GeographyPolygon:
                case EdmPrimitiveTypeKind.GeographyCollection:
                case EdmPrimitiveTypeKind.GeographyMultiPolygon:
                case EdmPrimitiveTypeKind.GeographyMultiLineString:
                case EdmPrimitiveTypeKind.GeographyMultiPoint:
                    {
                        int? srid;
                        this.ParseSpatialFacets(out srid, CsdlConstants.Default_SpatialGeographySrid);
                        return new CsdlSpatialTypeReference(kind, srid, typeName, isNullable, parentLocation);
                    }

                case EdmPrimitiveTypeKind.Geometry:
                case EdmPrimitiveTypeKind.GeometryPoint:
                case EdmPrimitiveTypeKind.GeometryLineString:
                case EdmPrimitiveTypeKind.GeometryPolygon:
                case EdmPrimitiveTypeKind.GeometryCollection:
                case EdmPrimitiveTypeKind.GeometryMultiPolygon:
                case EdmPrimitiveTypeKind.GeometryMultiLineString:
                case EdmPrimitiveTypeKind.GeometryMultiPoint:
                    {
                        int? srid;
                        this.ParseSpatialFacets(out srid, CsdlConstants.Default_SpatialGeometrySrid);
                        return new CsdlSpatialTypeReference(kind, srid, typeName, isNullable, parentLocation);
                    }
            }

            return new CsdlNamedTypeReference(typeName, isNullable, parentLocation);
        }

        private CsdlTypeReference ParseTypeReference(string typeString, XmlElementValueCollection childValues, CsdlLocation parentLocation, Optionality typeInfoOptionality)
        {
            bool isNullable = OptionalBoolean(CsdlConstants.Attribute_Nullable) ?? CsdlConstants.Default_Nullable;

            CsdlTypeReference elementType = null;
            if (typeString != null)
            {
                string[] typeInformation = typeString.Split(new char[] { '(', ')' });
                string typeName = typeInformation[0];
                switch (typeName)
                {
                    case CsdlConstants.Value_Collection:
                        {
                            string elementTypeName = typeInformation.Count() > 1 ? typeInformation[1] : typeString;
                            elementType = new CsdlExpressionTypeReference(
                                          new CsdlCollectionType(
                                          this.ParseNamedTypeReference(elementTypeName, isNullable, parentLocation), parentLocation), CsdlConstants.Default_CollectionNullable, parentLocation);
                        }

                        break;
                    case CsdlConstants.Value_Ref:
                        {
                            string elementTypeName = typeInformation.Count() > 1 ? typeInformation[1] : typeString;
                            elementType = new CsdlExpressionTypeReference(
                                          new CsdlEntityReferenceType(
                                          this.ParseNamedTypeReference(elementTypeName, isNullable, parentLocation), parentLocation), CsdlConstants.Default_Nullable, parentLocation);
                        }

                        break;
                    default:
                        elementType = this.ParseNamedTypeReference(typeName, isNullable, parentLocation);
                        break;
                }
            }
            else if (childValues != null)
            {
                elementType = childValues.ValuesOfType<CsdlTypeReference>().FirstOrDefault();
            }

            if (elementType == null && typeInfoOptionality == Optionality.Required)
            {
                if (childValues != null)
                {
                    // If childValues is null, then it is the case when a required type attribute was expected.
                    // In this case, we do not report the error as it should already be reported by EdmXmlDocumentParser.RequiredType method.
                    this.ReportError(parentLocation, EdmErrorCode.MissingType, Edm.Strings.CsdlParser_MissingTypeAttributeOrElement);
                }
                else
                {
                    Debug.Assert(this.Errors.Count() > 0, "There should be an error reported for the missing required type attribute.");
                }
                elementType = new CsdlNamedTypeReference(String.Empty, isNullable, parentLocation);
            }

            return elementType;
        }

        private CsdlNavigationProperty OnNavigationPropertyElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string relationship = Required(CsdlConstants.Attribute_Relationship);
            string toRole = Required(CsdlConstants.Attribute_ToRole);
            string fromRole = Required(CsdlConstants.Attribute_FromRole);
            bool? containsTarget = OptionalBoolean(CsdlConstants.Attribute_ContainsTarget);

            return new CsdlNavigationProperty(name, relationship, toRole, fromRole, containsTarget ?? false, Documentation(childValues), element.Location);
        }

        private static CsdlKey OnEntityKeyElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return new CsdlKey(new List<CsdlPropertyReference>(childValues.ValuesOfType<CsdlPropertyReference>()), element.Location);
        }

        private CsdlPropertyReference OnPropertyRefElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return new CsdlPropertyReference(Required(CsdlConstants.Attribute_Name), element.Location);
        }

        private CsdlEnumType OnEnumTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string underlyingType = OptionalType(CsdlConstants.Attribute_UnderlyingType);
            bool? isFlags = OptionalBoolean(CsdlConstants.Attribute_IsFlags);

            return new CsdlEnumType(name, underlyingType, isFlags ?? false, childValues.ValuesOfType<CsdlEnumMember>(), Documentation(childValues), element.Location);
        }

        private CsdlEnumMember OnEnumMemberElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            long? value = OptionalLong(CsdlConstants.Attribute_Value);

            return new CsdlEnumMember(name, value, Documentation(childValues), element.Location);
        }

        private CsdlAssociation OnAssociationElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);

            IEnumerable<CsdlAssociationEnd> ends = childValues.ValuesOfType<CsdlAssociationEnd>();
            if (ends.Count() != 2)
            {
                this.ReportError(element.Location, EdmErrorCode.InvalidAssociation, Edm.Strings.CsdlParser_InvalidAssociationIncorrectNumberOfEnds(name));
            }

            IEnumerable<CsdlReferentialConstraint> constraints = childValues.ValuesOfType<CsdlReferentialConstraint>();
            if (constraints.Count() > 1)
            {
                this.ReportError(childValues.OfResultType<CsdlReferentialConstraint>().ElementAt(1).Location, EdmErrorCode.InvalidAssociation, Edm.Strings.CsdlParser_AssociationHasAtMostOneConstraint);
            }

            return new CsdlAssociation(name, ends.ElementAtOrDefault(0), ends.ElementAtOrDefault(1), constraints.FirstOrDefault(), Documentation(childValues), element.Location);
        }

        private CsdlAssociationEnd OnAssociationEndElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string type = RequiredType(CsdlConstants.Attribute_Type);
            string role = Optional(CsdlConstants.Attribute_Role);
            EdmMultiplicity multiplicity = RequiredMultiplicity(CsdlConstants.Attribute_Multiplicity);
            CsdlOnDelete onDelete = childValues.ValuesOfType<CsdlOnDelete>().FirstOrDefault();

            bool isNullable;
            switch (multiplicity)
            {
                case EdmMultiplicity.One:
                case EdmMultiplicity.Many:
                    isNullable = false;
                    break;
                default:
                    Debug.Assert(multiplicity == EdmMultiplicity.ZeroOrOne, "multiplicity == EdmAssociationMultiplicity.ZeroOrOne");
                    isNullable = true;
                    break;
            }

            CsdlNamedTypeReference endType = new CsdlNamedTypeReference(type, isNullable, element.Location);
            return new CsdlAssociationEnd(role, endType, multiplicity, onDelete, Documentation(childValues), element.Location);
        }

        private CsdlOnDelete OnDeleteActionElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            EdmOnDeleteAction action = RequiredOnDeleteAction(CsdlConstants.Attribute_Action);

            return new CsdlOnDelete(action, Documentation(childValues), element.Location);
        }

        private CsdlReferentialConstraint OnReferentialConstraintElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            CsdlReferentialConstraintRole principal = this.GetConstraintRole(childValues, CsdlConstants.Element_Principal, () => Edm.Strings.CsdlParser_ReferentialConstraintRequiresOnePrincipal);
            CsdlReferentialConstraintRole dependent = this.GetConstraintRole(childValues, CsdlConstants.Element_Dependent, () => Edm.Strings.CsdlParser_ReferentialConstraintRequiresOneDependent);

            return new CsdlReferentialConstraint(principal, dependent, Documentation(childValues), element.Location);
        }

        private CsdlReferentialConstraintRole GetConstraintRole(XmlElementValueCollection childValues, string roleElementName, Func<string> improperNumberMessage)
        {
            IEnumerable<XmlElementValue<CsdlReferentialConstraintRole>> roleChildren = childValues.FindByName<CsdlReferentialConstraintRole>(roleElementName).ToArray();
            if (roleChildren.Count() > 1)
            {
                ReportError(roleChildren.ElementAt(1).Location, EdmErrorCode.InvalidAssociation, improperNumberMessage());
            }
            XmlElementValue<CsdlReferentialConstraintRole> result = roleChildren.FirstOrDefault();
            return (result != null) ? result.Value : null;
        }

        private CsdlReferentialConstraintRole OnReferentialConstraintRoleElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string role = Required(CsdlConstants.Attribute_Role);
            IEnumerable<CsdlPropertyReference> properties = childValues.ValuesOfType<CsdlPropertyReference>();

            return new CsdlReferentialConstraintRole(role, properties, Documentation(childValues), element.Location);
        }

        private CsdlFunction OnFunctionElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string returnTypeName = OptionalType(CsdlConstants.Attribute_ReturnType);
            IEnumerable<CsdlFunctionParameter> parameters = childValues.ValuesOfType<CsdlFunctionParameter>();
            var definingExpressionElement = childValues[CsdlConstants.Element_DefiningExpression];
            string definingExpression = null;
            if (!(definingExpressionElement is XmlElementValueCollection.MissingXmlElementValue))
            {
                //// If the element exists we want to reflect that it had a defining expression, but that it was empty 
                //// rather than never having one at all.
                definingExpression = definingExpressionElement.TextValue ?? string.Empty;
            }

            CsdlTypeReference returnType = null;
            if (returnTypeName == null)
            {
                CsdlFunctionReturnType returnTypeElementValue = childValues.ValuesOfType<CsdlFunctionReturnType>().FirstOrDefault();
                if (returnTypeElementValue != null)
                {
                    returnType = returnTypeElementValue.ReturnType;
                }
            }
            else
            {
                //// Still need to cope with Collection(type) shortcuts so ParseNamedTypeReference cannot be used.
                returnType = this.ParseTypeReference(returnTypeName, null, element.Location, Optionality.Required);
            }

            return new CsdlFunction(name, parameters, definingExpression, returnType, Documentation(childValues), element.Location);
        }

        private CsdlFunctionParameter OnParameterElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string typeName = OptionalType(CsdlConstants.Attribute_Type);
            CsdlTypeReference type = this.ParseTypeReference(typeName, childValues, element.Location, Optionality.Required);

            return new CsdlFunctionParameter(name, type, CsdlConstants.Default_FunctionParameterMode, Documentation(childValues), element.Location);
        }

        private CsdlFunctionImport OnFunctionImportElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string returnTypeName = OptionalType(CsdlConstants.Attribute_ReturnType);
            CsdlTypeReference returnType = this.ParseTypeReference(returnTypeName, childValues, element.Location, Optionality.Optional);
            bool composable = OptionalBoolean(CsdlConstants.Attribute_IsComposable) ?? CsdlConstants.Default_IsComposable;
            bool sideEffecting = OptionalBoolean(CsdlConstants.Attribute_IsSideEffecting) ?? (composable ? false : CsdlConstants.Default_IsSideEffecting);
            bool bindable = OptionalBoolean(CsdlConstants.Attribute_IsBindable) ?? CsdlConstants.Default_IsBindable;
            string entitySet = Optional(CsdlConstants.Attribute_EntitySet);
            string entitySetPath = Optional(CsdlConstants.Attribute_EntitySetPath);
            IEnumerable<CsdlFunctionParameter> parameters = childValues.ValuesOfType<CsdlFunctionParameter>();

            return new CsdlFunctionImport(
                name,
                sideEffecting,
                composable,
                bindable,
                entitySet,
                entitySetPath,
                parameters,
                returnType,
                Documentation(childValues),
                element.Location);
        }

        private CsdlFunctionParameter OnFunctionImportParameterElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string typeName = RequiredType(CsdlConstants.Attribute_Type);
            EdmFunctionParameterMode? mode = OptionalFunctionParameterMode(CsdlConstants.Attribute_Mode);
            CsdlTypeReference type = this.ParseTypeReference(typeName, null, element.Location, Optionality.Required);
            return new CsdlFunctionParameter(name, type, mode ?? CsdlConstants.Default_FunctionParameterMode, Documentation(childValues), element.Location);
        }

        private CsdlTypeReference OnEntityReferenceTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = RequiredType(CsdlConstants.Attribute_Type);
            return new CsdlExpressionTypeReference(new CsdlEntityReferenceType(this.ParseTypeReference(typeName, null, element.Location, Optionality.Required), element.Location), CsdlConstants.Default_Nullable, element.Location);
        }

        private CsdlTypeReference OnTypeRefElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = RequiredType(CsdlConstants.Attribute_Type);
            return this.ParseTypeReference(typeName, null, element.Location, Optionality.Required);
        }

        private CsdlTypeReference OnRowTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            return new CsdlExpressionTypeReference(new CsdlRowType(childValues.ValuesOfType<CsdlProperty>(), element.Location), CsdlConstants.Default_Nullable, element.Location);
        }

        private CsdlTypeReference OnCollectionTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string elementTypeName = OptionalType(CsdlConstants.Attribute_ElementType);
            CsdlTypeReference elementType = this.ParseTypeReference(elementTypeName, childValues, element.Location, Optionality.Required);

            return new CsdlExpressionTypeReference(new CsdlCollectionType(elementType, element.Location), CsdlConstants.Default_CollectionNullable, element.Location);
        }

        private CsdlFunctionReturnType OnReturnTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string typeName = OptionalType(CsdlConstants.Attribute_Type);
            CsdlTypeReference type = this.ParseTypeReference(typeName, childValues, element.Location, Optionality.Required);
            return new CsdlFunctionReturnType(type, element.Location);
        }

        private CsdlEntityContainer OnEntityContainerElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string extends = Optional(CsdlConstants.Attribute_Extends);

            return new CsdlEntityContainer(
                name,
                extends,
                childValues.ValuesOfType<CsdlEntitySet>(),
                childValues.ValuesOfType<CsdlAssociationSet>(),
                childValues.ValuesOfType<CsdlFunctionImport>(),
                Documentation(childValues),
                element.Location);
        }

        private CsdlEntitySet OnEntitySetElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string entityType = RequiredQualifiedName(CsdlConstants.Attribute_EntityType);

            return new CsdlEntitySet(name, entityType, Documentation(childValues), element.Location);
        }

        private CsdlAssociationSet OnAssociationSetElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string name = Required(CsdlConstants.Attribute_Name);
            string association = RequiredQualifiedName(CsdlConstants.Attribute_Association);
            IEnumerable<CsdlAssociationSetEnd> ends = childValues.ValuesOfType<CsdlAssociationSetEnd>();
            if (ends.Count() > 2)
            {
                this.ReportError(element.Location, EdmErrorCode.InvalidAssociationSet, Edm.Strings.CsdlParser_InvalidAssociationSetIncorrectNumberOfEnds(name));
            }

            return new CsdlAssociationSet(name, association, ends.ElementAtOrDefault(0), ends.ElementAtOrDefault(1), Documentation(childValues), element.Location);
        }

        private CsdlAssociationSetEnd OnAssociationSetEndElement(XmlElementInfo element, XmlElementValueCollection childValues)
        {
            string role = Required(CsdlConstants.Attribute_Role);
            string entitySet = Required(CsdlConstants.Attribute_EntitySet);

            return new CsdlAssociationSetEnd(role, entitySet, Documentation(childValues), element.Location);
        }

        private void ParseMaxLength(out bool Unbounded, out int? maxLength)
        {
            string max = Optional(CsdlConstants.Attribute_MaxLength);
            if (max == null)
            {
                Unbounded = false;
                maxLength = null;
            }
            else if (max.EqualsOrdinalIgnoreCase(CsdlConstants.Value_Max))
            {
                Unbounded = true;
                maxLength = null;
            }
            else
            {
                Unbounded = false;
                maxLength = OptionalMaxLength(CsdlConstants.Attribute_MaxLength);
            }
        }

        private void ParseBinaryFacets(out bool Unbounded, out int? maxLength, out bool? fixedLength)
        {
            this.ParseMaxLength(out Unbounded, out maxLength);
            fixedLength = OptionalBoolean(CsdlConstants.Attribute_FixedLength);
        }

        private void ParseDecimalFacets(out int? precision, out int? scale)
        {
            precision = OptionalInteger(CsdlConstants.Attribute_Precision);
            scale = OptionalInteger(CsdlConstants.Attribute_Scale);
        }

        private void ParseStringFacets(out bool Unbounded, out int? maxLength, out bool? fixedLength, out bool? unicode, out string collation)
        {
            this.ParseMaxLength(out Unbounded, out maxLength);
            fixedLength = OptionalBoolean(CsdlConstants.Attribute_FixedLength);
            unicode = OptionalBoolean(CsdlConstants.Attribute_Unicode);
            collation = Optional(CsdlConstants.Attribute_Collation);
        }

        private void ParseTemporalFacets(out int? precision)
        {
            precision = OptionalInteger(CsdlConstants.Attribute_Precision);
        }

        private void ParseSpatialFacets(out int? srid, int defaultSrid)
        {
            srid = OptionalSrid(CsdlConstants.Attribute_Srid, defaultSrid);
        }

        private enum Optionality
        {
            Optional,
            Required
        }
    }
}

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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm.Annotations;

namespace Microsoft.Data.Edm.Csdl.Internal.Serialization
{
    internal class EdmModelSchemaSeparationSerializationVisitor : EdmModelVisitor
    {
        private bool visitCompleted = false;
        private Dictionary<string, EdmSchema> modelSchemas = new Dictionary<string, EdmSchema>();
        private EdmSchema activeSchema;

        public EdmModelSchemaSeparationSerializationVisitor(IEdmModel visitedModel)
            : base(visitedModel)
        {
        }

        public IEnumerable<EdmSchema> GetSchemas()
        {
            if (!this.visitCompleted)
            {
                this.Visit();
            }

            return this.modelSchemas.Values;
        }

        protected void Visit()
        {
            this.VisitEdmModel();
            this.visitCompleted = true;
        }

        protected override void ProcessModel(IEdmModel model)
        {
            this.ProcessElement(model);
            this.VisitSchemaElements(model.SchemaElements);
            this.VisitVocabularyAnnotations(model.VocabularyAnnotations.Where(a => !a.IsInline(this.Model)));
        }

        protected override void ProcessVocabularyAnnotatable(IEdmVocabularyAnnotatable element)
        {
            this.VisitAnnotations(this.Model.DirectValueAnnotations(element));
            this.VisitVocabularyAnnotations(this.Model.FindDeclaredVocabularyAnnotations(element).Where(a => a.IsInline(this.Model)));
        }

        protected override void ProcessSchemaElement(IEdmSchemaElement element)
        {
            string namespaceName = element.Namespace;

            // Put all of the namespaceless stuff into one schema.
            if (EdmUtil.IsNullOrWhiteSpaceInternal(namespaceName))
            {
                namespaceName = string.Empty;
            }

            EdmSchema schema;
            if (!this.modelSchemas.TryGetValue(namespaceName, out schema))
            {
                schema = new EdmSchema(namespaceName);
                this.modelSchemas.Add(namespaceName, schema);
            }
            
            schema.AddSchemaElement(element);
            this.activeSchema = schema;
            
            base.ProcessSchemaElement(element);
        }

        protected override void ProcessVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
        {
            if (!annotation.IsInline(this.Model))
            {
                var annotationSchemaNamespace = annotation.GetSchemaNamespace(this.Model) ?? this.modelSchemas.Select(s => s.Key).FirstOrDefault() ?? string.Empty;

                EdmSchema annotationSchema;
                if (!this.modelSchemas.TryGetValue(annotationSchemaNamespace, out annotationSchema))
                {
                    annotationSchema = new EdmSchema(annotationSchemaNamespace);
                    this.modelSchemas.Add(annotationSchema.Namespace, annotationSchema);
                }

                annotationSchema.AddVocabularyAnnotation(annotation);
                this.activeSchema = annotationSchema;
            }

            if (annotation.Term != null)
            {
                this.CheckSchemaElementReference(annotation.Term);
            }

            base.ProcessVocabularyAnnotation(annotation);
        }

        /// <summary>
        /// When we see an entity container, we see if it has <see cref="CsdlConstants.SchemaNamespaceAnnotation"/>.
        /// If it does, then we attach it to that schema, otherwise we attached to the first existing schema.
        /// If there are no schemas, we create the one named "Default" and attach container to it.
        /// </summary>
        /// <param name="element">The entity container being processed.</param>
        protected override void ProcessEntityContainer(IEdmEntityContainer element)
        {
            var containerSchemaNamespace = element.Namespace;

            EdmSchema containerSchema;
            if (!this.modelSchemas.TryGetValue(containerSchemaNamespace, out containerSchema))
            {
                containerSchema = new EdmSchema(containerSchemaNamespace);
                this.modelSchemas.Add(containerSchema.Namespace, containerSchema);
            }

            containerSchema.AddEntityContainer(element);
            this.activeSchema = containerSchema;

            base.ProcessEntityContainer(element);
        }

        protected override void ProcessComplexTypeReference(IEdmComplexTypeReference element)
        {
            this.CheckSchemaElementReference(element.ComplexDefinition());
        }

        protected override void ProcessEntityTypeReference(IEdmEntityTypeReference element)
        {
            this.CheckSchemaElementReference(element.EntityDefinition());
        }

        protected override void ProcessEntityReferenceTypeReference(IEdmEntityReferenceTypeReference element)
        {
            this.CheckSchemaElementReference(element.EntityType());
        }

        protected override void ProcessEnumTypeReference(IEdmEnumTypeReference element)
        {
            this.CheckSchemaElementReference(element.EnumDefinition());
        }

        protected override void ProcessEntityType(IEdmEntityType element)
        {
            base.ProcessEntityType(element);
            if (element.BaseEntityType() != null)
            {
                this.CheckSchemaElementReference(element.BaseEntityType());
            }
        }

        protected override void ProcessComplexType(IEdmComplexType element)
        {
            base.ProcessComplexType(element);
            if (element.BaseComplexType() != null)
            {
                this.CheckSchemaElementReference(element.BaseComplexType());
            }
        }

        protected override void ProcessEnumType(IEdmEnumType element)
        {
            base.ProcessEnumType(element);
            this.CheckSchemaElementReference(element.UnderlyingType);
        }

        protected override void ProcessNavigationProperty(IEdmNavigationProperty property)
        {
            var associationNamespace = SerializationExtensionMethods.GetAssociationNamespace(Model, property);

            EdmSchema associationSchema;
            if (!this.modelSchemas.TryGetValue(associationNamespace, out associationSchema))
            {
                associationSchema = new EdmSchema(associationNamespace);
                this.modelSchemas.Add(associationSchema.Namespace, associationSchema);
            }

            associationSchema.AddAssociatedNavigationProperty(property);
            associationSchema.AddNamespaceUsing(property.DeclaringEntityType().Namespace);
            associationSchema.AddNamespaceUsing(property.Partner.DeclaringEntityType().Namespace);
            this.activeSchema.AddNamespaceUsing(associationNamespace);

            base.ProcessNavigationProperty(property);
        }

        private void CheckSchemaElementReference(IEdmSchemaElement element)
        {
            this.CheckSchemaElementReference(element.Namespace);
        }

        private void CheckSchemaElementReference(string namespaceName)
        {
            if (this.activeSchema != null)
            {
                this.activeSchema.AddNamespaceUsing(namespaceName);
            }
        }
    }
}

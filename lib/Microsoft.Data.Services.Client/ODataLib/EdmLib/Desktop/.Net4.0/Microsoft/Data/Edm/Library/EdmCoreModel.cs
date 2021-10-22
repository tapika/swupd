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
using System.Linq;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library.Annotations;

namespace Microsoft.Data.Edm.Library
{
    /// <summary>
    /// This is a marker interface for core model elements that do not require validation.
    /// </summary>
    internal interface IEdmValidCoreModelElement
    {
    }

    /// <summary>
    /// Provides predefined declarations relevant to EDM semantics.
    /// </summary>
    public class EdmCoreModel : EdmElement, IEdmModel, IEdmValidCoreModelElement
    {
        /// <summary>
        /// The default core EDM model.
        /// </summary>
        public static readonly EdmCoreModel Instance = new EdmCoreModel();

        private readonly EdmValidCoreModelPrimitiveType[] primitiveTypes;
        private const string EdmNamespace = "Edm";
        
        private readonly Dictionary<string, EdmPrimitiveTypeKind> primitiveTypeKinds = new Dictionary<string, EdmPrimitiveTypeKind>();
        private readonly Dictionary<EdmPrimitiveTypeKind, EdmValidCoreModelPrimitiveType> primitiveTypesByKind = new Dictionary<EdmPrimitiveTypeKind, EdmValidCoreModelPrimitiveType>();
        private readonly Dictionary<string, EdmValidCoreModelPrimitiveType> primitiveTypeByName = new Dictionary<string, EdmValidCoreModelPrimitiveType>();

        private readonly IEdmDirectValueAnnotationsManager annotationsManager = new EdmDirectValueAnnotationsManager();

        private EdmCoreModel()
        {
            var primitiveDouble = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Double", EdmPrimitiveTypeKind.Double);
            var primitiveSingle = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Single", EdmPrimitiveTypeKind.Single);

            var primitiveInt64 = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Int64", EdmPrimitiveTypeKind.Int64);
            var primitiveInt32 = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Int32", EdmPrimitiveTypeKind.Int32);
            var primitiveInt16 = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Int16", EdmPrimitiveTypeKind.Int16);
            var primitiveSByte = new EdmValidCoreModelPrimitiveType(EdmNamespace, "SByte", EdmPrimitiveTypeKind.SByte);
            var primitiveByte = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Byte", EdmPrimitiveTypeKind.Byte);

            var primitiveBoolean = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Boolean", EdmPrimitiveTypeKind.Boolean);
            var primitiveGuid = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Guid", EdmPrimitiveTypeKind.Guid);

            var primitiveTime = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Time", EdmPrimitiveTypeKind.Time);
            var primitiveDateTime = new EdmValidCoreModelPrimitiveType(EdmNamespace, "DateTime", EdmPrimitiveTypeKind.DateTime);
            var primitiveDateTimeOffset = new EdmValidCoreModelPrimitiveType(EdmNamespace, "DateTimeOffset", EdmPrimitiveTypeKind.DateTimeOffset);

            var primitiveDecimal = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Decimal", EdmPrimitiveTypeKind.Decimal);

            var primitiveBinary = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Binary", EdmPrimitiveTypeKind.Binary);
            var primitiveString = new EdmValidCoreModelPrimitiveType(EdmNamespace, "String", EdmPrimitiveTypeKind.String);
            var primitiveStream = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Stream", EdmPrimitiveTypeKind.Stream);

            var primitiveGeography = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Geography", EdmPrimitiveTypeKind.Geography);
            var primitiveGeographyPoint = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeographyPoint", EdmPrimitiveTypeKind.GeographyPoint);
            var primitiveGeographyLineString = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeographyLineString", EdmPrimitiveTypeKind.GeographyLineString);
            var primitiveGeographyPolygon = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeographyPolygon", EdmPrimitiveTypeKind.GeographyPolygon);
            var primitiveGeographyCollection = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeographyCollection", EdmPrimitiveTypeKind.GeographyCollection);
            var primitiveGeographyMultiPolygon = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeographyMultiPolygon", EdmPrimitiveTypeKind.GeographyMultiPolygon);
            var primitiveGeographyMultiLineString = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeographyMultiLineString", EdmPrimitiveTypeKind.GeographyMultiLineString);
            var primitiveGeographyMultiPoint = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeographyMultiPoint", EdmPrimitiveTypeKind.GeographyMultiPoint);
            var primitiveGeometry = new EdmValidCoreModelPrimitiveType(EdmNamespace, "Geometry", EdmPrimitiveTypeKind.Geometry);
            var primitiveGeometryPoint = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeometryPoint", EdmPrimitiveTypeKind.GeometryPoint);
            var primitiveGeometryLineString = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeometryLineString", EdmPrimitiveTypeKind.GeometryLineString);
            var primitiveGeometryPolygon = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeometryPolygon", EdmPrimitiveTypeKind.GeometryPolygon);
            var primitiveGeometryCollection = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeometryCollection", EdmPrimitiveTypeKind.GeometryCollection);
            var primitiveGeometryMultiPolygon = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeometryMultiPolygon", EdmPrimitiveTypeKind.GeometryMultiPolygon);
            var primitiveGeometryMultiLineString = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeometryMultiLineString", EdmPrimitiveTypeKind.GeometryMultiLineString);
            var primitiveGeometryMultiPoint = new EdmValidCoreModelPrimitiveType(EdmNamespace, "GeometryMultiPoint", EdmPrimitiveTypeKind.GeometryMultiPoint);

            this.primitiveTypes = new EdmValidCoreModelPrimitiveType[]
            {
                primitiveBinary,
                primitiveBoolean,
                primitiveByte,
                primitiveDateTime,
                primitiveDateTimeOffset,
                primitiveDecimal,
                primitiveDouble,
                primitiveGuid,
                primitiveInt16,
                primitiveInt32,
                primitiveInt64,
                primitiveSByte,
                primitiveSingle,
                primitiveStream,
                primitiveString,
                primitiveTime,
                primitiveGeography,
                primitiveGeographyPoint,
                primitiveGeographyLineString,
                primitiveGeographyPolygon,
                primitiveGeographyCollection,
                primitiveGeographyMultiPolygon,
                primitiveGeographyMultiLineString,
                primitiveGeographyMultiPoint,
                primitiveGeometry,
                primitiveGeometryPoint,
                primitiveGeometryLineString,
                primitiveGeometryPolygon,
                primitiveGeometryCollection,
                primitiveGeometryMultiPolygon,
                primitiveGeometryMultiLineString,
                primitiveGeometryMultiPoint
            };
            
            foreach (var primitive in this.primitiveTypes)
            {
                this.primitiveTypeKinds[primitive.Name] = primitive.PrimitiveKind;
                this.primitiveTypeKinds[primitive.Namespace + '.' + primitive.Name] = primitive.PrimitiveKind;
                this.primitiveTypesByKind[primitive.PrimitiveKind] = primitive;
                this.primitiveTypeByName[primitive.Namespace + '.' + primitive.Name] = primitive;
            }
        }

        /// <summary>
        /// Gets the namespace of this core model.
        /// </summary>
        public static string Namespace
        {
            get { return "Edm"; }
        }

        /// <summary>
        /// Gets the types defined in this core model.
        /// </summary>
        public IEnumerable<IEdmSchemaElement> SchemaElements
        {
            get { return this.primitiveTypes; }
        }

        /// <summary>
        /// Gets the vocabulary annotations defined in this model.
        /// </summary>
        public IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
        {
            get { return Enumerable.Empty<IEdmVocabularyAnnotation>(); }
        }

        /// <summary>
        /// Gets the collection of models referred to by this model.
        /// </summary>
        public IEnumerable<IEdmModel> ReferencedModels
        {
            get { return Enumerable.Empty<IEdmModel>(); }
        }

        /// <summary>
        /// Gets the model's annotations manager.
        /// </summary>
        public IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
        {
            get { return this.annotationsManager; }
        }

        /// <summary>
        /// Gets a reference to a non-atomic collection type definition.
        /// </summary>
        /// <param name="elementType">Type of elements in the collection.</param>
        /// <returns>A new non-atomic collection type reference.</returns>
        public static IEdmCollectionTypeReference GetCollection(IEdmTypeReference elementType)
        {
            return new EdmCollectionTypeReference(new EdmCollectionType(elementType), false);
        }

        /// <summary>
        /// Searches for a type with the given name in this model and returns null if no such type exists.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the type being found.</param>
        /// <returns>The requested type, or null if no such type exists.</returns>
        public IEdmSchemaType FindDeclaredType(string qualifiedName)
        {
            EdmValidCoreModelPrimitiveType element;
            return this.primitiveTypeByName.TryGetValue(qualifiedName, out element) ? element : null;
        }

        /// <summary>
        /// Searches for a value term with the given name in this model and returns null if no such value term exists.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the value term being found.</param>
        /// <returns>The requested value term, or null if no such value term exists.</returns>
        public IEdmValueTerm FindDeclaredValueTerm(string qualifiedName)
        {
            return null;
        }

        /// <summary>
        /// Searches for functions with the given name in this model and returns an empty enumerable if no such function exists.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the function being found.</param>
        /// <returns>A set functions sharing the specified qualified name, or an empty enumerable if no such function exists.</returns>
        public IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName)
        {
            return Enumerable.Empty<IEdmFunction>();
        }

        /// <summary>
        /// Searches for an entity container with the given name in this model and returns null if no such entity container exists.
        /// </summary>
        /// <param name="name">The name of the entity container being found.</param>
        /// <returns>The requested entity container, or null if no such entity container exists.</returns>
        public IEdmEntityContainer FindDeclaredEntityContainer(string name)
        {
            return null;
        }

        /// <summary>
        /// Gets primitive type by kind.
        /// </summary>
        /// <param name="kind">Kind of the primitive type.</param>
        /// <returns>Primitive type definition.</returns>
        public IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind kind)
        {
            return this.GetCoreModelPrimitiveType(kind);
        }

        /// <summary>
        /// Gets the EdmPrimitiveTypeKind by the type name.
        /// </summary>
        /// <param name="typeName">Name of the type to look up.</param>
        /// <returns>EdmPrimitiveTypeKind of the type.</returns>
        public EdmPrimitiveTypeKind GetPrimitiveTypeKind(string typeName)
        {
            EdmPrimitiveTypeKind kind;
            return this.primitiveTypeKinds.TryGetValue(typeName, out kind) ? kind : EdmPrimitiveTypeKind.None;
        }

        /// <summary>
        /// Gets a reference to a primitive type of the specified kind.
        /// </summary>
        /// <param name="kind">Primitive kind of the type reference being created.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetPrimitive(EdmPrimitiveTypeKind kind, bool isNullable)
        {
            IEdmPrimitiveType primitiveDefinition = this.GetCoreModelPrimitiveType(kind);
            if (primitiveDefinition != null)
            {
                return primitiveDefinition.GetPrimitiveTypeReference(isNullable);
            }
            else
            {
                throw new InvalidOperationException(Edm.Strings.EdmPrimitive_UnexpectedKind);
            }
        }

        /// <summary>
        /// Gets a reference to the Int16 primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetInt16(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Int16), isNullable);
        }

        /// <summary>
        /// Gets a reference to the Int32 primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetInt32(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Int32), isNullable);
        }

        /// <summary>
        /// Gets a reference to the Int64 primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetInt64(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Int64), isNullable);
        }

        /// <summary>
        /// Gets a reference to the Boolean primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetBoolean(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Boolean), isNullable);
        }

        /// <summary>
        /// Gets a reference to the Byte primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetByte(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Byte), isNullable);
        }

        /// <summary>
        /// Gets a reference to the SByte primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetSByte(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.SByte), isNullable);
        }

        /// <summary>
        /// Gets a reference to the Guid primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new primitive type reference.</returns>
        public IEdmPrimitiveTypeReference GetGuid(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Guid), isNullable);
        }

        /// <summary>
        /// Gets a reference to a datetime primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new datetime type reference.</returns>
        public IEdmTemporalTypeReference GetDateTime(bool isNullable)
        {
            return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.DateTime), isNullable);
        }

        /// <summary>
        /// Gets a reference to a datetime with offset primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new datetime with offset type reference.</returns>
        public IEdmTemporalTypeReference GetDateTimeOffset(bool isNullable)
        {
            return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset), isNullable);
        }

        /// <summary>
        /// Gets a reference to a time primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new time type reference.</returns>
        public IEdmTemporalTypeReference GetTime(bool isNullable)
        {
            return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Time), isNullable);
        }

        /// <summary>
        /// Gets a reference to a decimal primitive type definition.
        /// </summary>
        /// <param name="precision">Precision of values of this type.</param>
        /// <param name="scale">Scale of values of this type.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new decimal type reference.</returns>
        public IEdmDecimalTypeReference GetDecimal(int? precision, int? scale, bool isNullable)
        {
            if (precision.HasValue || scale.HasValue)
            {
                // Facet values may render this reference as semantically invalid, so can't return an IEdmValidCoreModelElement.
                return new EdmDecimalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Decimal), isNullable, precision, scale);
            }
            else
            {
                return new EdmDecimalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Decimal), isNullable);
            }
        }

        /// <summary>
        /// Gets a reference to a decimal primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new decimal type reference.</returns>
        public IEdmDecimalTypeReference GetDecimal(bool isNullable)
        {
            return new EdmDecimalTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Decimal), isNullable);
        }

        /// <summary>
        /// Gets a reference to a single primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new decimal type reference.</returns>
        public IEdmPrimitiveTypeReference GetSingle(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Single), isNullable);
        }

        /// <summary>
        /// Gets a reference to a double primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new decimal type reference.</returns>
        public IEdmPrimitiveTypeReference GetDouble(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Double), isNullable);
        }

        /// <summary>
        /// Gets a reference to a stream primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new stream type reference.</returns>
        public IEdmPrimitiveTypeReference GetStream(bool isNullable)
        {
            return new EdmPrimitiveTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Stream), isNullable);
        }

        /// <summary>
        /// Gets a reference to a temporal primitive type definition.
        /// </summary>
        /// <param name="kind">Primitive kind of the type reference being created.</param>
        /// <param name="precision">Precision of values of this type.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new temporal type reference.</returns>
        public IEdmTemporalTypeReference GetTemporal(EdmPrimitiveTypeKind kind, int? precision, bool isNullable)
        {
            switch (kind)
            {
                case EdmPrimitiveTypeKind.DateTime:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Time:
                    return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable, precision);
                default:
                    throw new InvalidOperationException(Edm.Strings.EdmPrimitive_UnexpectedKind);
            }
        }

        /// <summary>
        /// Gets a reference to a temporal primitive type definition.
        /// </summary>
        /// <param name="kind">Primitive kind of the type reference being created.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new temporal type reference.</returns>
        public IEdmTemporalTypeReference GetTemporal(EdmPrimitiveTypeKind kind, bool isNullable)
        {
            switch (kind)
            {
                case EdmPrimitiveTypeKind.DateTime:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                case EdmPrimitiveTypeKind.Time:
                    return new EdmTemporalTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable);
                default:
                    throw new InvalidOperationException(Edm.Strings.EdmPrimitive_UnexpectedKind);
            }
        }

        /// <summary>
        /// Gets a reference to a binary primitive type definition.
        /// </summary>
        /// <param name="isUnbounded">Flag specifying if max length is unbounded.</param>
        /// <param name="maxLength">Maximum length of the type.</param>
        /// <param name="isFixedLength">Flag specifying if the type will have a fixed length.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new binary type reference.</returns>
        public IEdmBinaryTypeReference GetBinary(bool isUnbounded, int? maxLength, bool? isFixedLength, bool isNullable)
        {
            return new EdmBinaryTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Binary), isNullable, isUnbounded, maxLength, isFixedLength);
        }

        /// <summary>
        /// Gets a reference to a binary primitive type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new binary type reference.</returns>
        public IEdmBinaryTypeReference GetBinary(bool isNullable)
        {
            return new EdmBinaryTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.Binary), isNullable);
        }

        /// <summary>
        /// Gets a reference to a spatial primitive type definition.
        /// </summary>
        /// <param name="kind">Primitive kind of the type reference being created.</param>
        /// <param name="spatialReferenceIdentifier">Spatial Reference Identifier for the spatial type being created.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new spatial type reference.</returns>
        public IEdmSpatialTypeReference GetSpatial(EdmPrimitiveTypeKind kind, int? spatialReferenceIdentifier, bool isNullable)
        {
            switch (kind)
            {
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
                    return new EdmSpatialTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable, spatialReferenceIdentifier);
                default:
                    throw new InvalidOperationException(Edm.Strings.EdmPrimitive_UnexpectedKind);
            }
        }

        /// <summary>
        /// Gets a reference to a spatial primitive type definition.
        /// </summary>
        /// <param name="kind">Primitive kind of the type reference being created.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new spatial type reference.</returns>
        public IEdmSpatialTypeReference GetSpatial(EdmPrimitiveTypeKind kind, bool isNullable)
        {
            switch (kind)
            {
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
                    return new EdmSpatialTypeReference(this.GetCoreModelPrimitiveType(kind), isNullable);
                default:
                    throw new InvalidOperationException(Edm.Strings.EdmPrimitive_UnexpectedKind);
            }
        }

        /// <summary>
        /// Gets a reference to a string primitive type definition.
        /// </summary>
        /// <param name="isUnbounded">Flag specifying if max length is the maximum allowable value.</param>
        /// <param name="maxLength">Maximum length of the type.</param>
        /// <param name="isFixedLength">Flag specifying if the type will have a fixed length.</param>
        /// <param name="isUnicode">Flag specifying if the type should support unicode encoding.</param>
        /// <param name="collation">String representing how data should be ordered.</param>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new string type reference.</returns>
        public IEdmStringTypeReference GetString(bool isUnbounded, int? maxLength, bool? isFixedLength, bool? isUnicode, string collation, bool isNullable)
        {
            return new EdmStringTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.String), isNullable, isUnbounded, maxLength, isFixedLength, isUnicode, collation);
        }

        /// <summary>
        /// Gets a reference to a binary string type definition.
        /// </summary>
        /// <param name="isNullable">Flag specifying if the referenced type should be nullable.</param>
        /// <returns>A new string type reference.</returns>
        public IEdmStringTypeReference GetString(bool isNullable)
        {
            return new EdmStringTypeReference(this.GetCoreModelPrimitiveType(EdmPrimitiveTypeKind.String), isNullable);
        }

        /// <summary>
        /// Searches for vocabulary annotations specified by this model or a referenced model for a given element.
        /// </summary>
        /// <param name="element">The annotated element.</param>
        /// <returns>The vocabulary annotations for the element.</returns>
        public IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
        {
            return Enumerable.Empty<IEdmVocabularyAnnotation>();
        }
        
        /// <summary>
        /// Finds a list of types that derive from the supplied type.
        /// </summary>
        /// <param name="baseType">The base type that derived types are being searched for.</param>
        /// <returns>A list of types that derive from the type.</returns>
        public IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
        {
            return Enumerable.Empty<IEdmStructuredType>();
        }

        private EdmValidCoreModelPrimitiveType GetCoreModelPrimitiveType(EdmPrimitiveTypeKind kind)
        {
            EdmValidCoreModelPrimitiveType definition;
            return this.primitiveTypesByKind.TryGetValue(kind, out definition) ? definition : null;
        }

        #region Core model types and type references

        private sealed class EdmValidCoreModelPrimitiveType : EdmType, IEdmPrimitiveType, IEdmValidCoreModelElement
        {
            private readonly string namespaceName;
            private readonly string name;
            private readonly EdmPrimitiveTypeKind primitiveKind;

            public EdmValidCoreModelPrimitiveType(string namespaceName, string name, EdmPrimitiveTypeKind primitiveKind)
            {
                this.namespaceName = namespaceName ?? string.Empty;
                this.name = name ?? string.Empty;
                this.primitiveKind = primitiveKind;
            }

            public string Name
            {
                get { return this.name; }
            }

            public string Namespace
            {
                get { return this.namespaceName; }
            }

            /// <summary>
            /// Gets the kind of this type.
            /// </summary>
            public override EdmTypeKind TypeKind
            {
                get { return EdmTypeKind.Primitive; }
            }

            public EdmPrimitiveTypeKind PrimitiveKind
            {
                get { return this.primitiveKind; }
            }

            public EdmSchemaElementKind SchemaElementKind
            {
                get { return EdmSchemaElementKind.TypeDefinition; }
            }
        }

        #endregion
    }
}

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
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
    internal abstract class EdmXmlDocumentParser<TResult> : XmlDocumentParser<TResult>
    {
        protected XmlElementInfo currentElement;
        private readonly Stack<XmlElementInfo> elementStack = new Stack<XmlElementInfo>();
        private HashSetInternal<string> edmNamespaces;

        internal EdmXmlDocumentParser(string artifactLocation, XmlReader reader)
            : base(reader, artifactLocation)
        {
        }

        internal abstract IEnumerable<KeyValuePair<Version, string>> SupportedVersions { get; }

        internal XmlAttributeInfo GetOptionalAttribute(XmlElementInfo element, string attributeName)
        {
            return element.Attributes[attributeName];
        }

        internal XmlAttributeInfo GetRequiredAttribute(XmlElementInfo element, string attributeName)
        {
            var attr = element.Attributes[attributeName];
            if (attr.IsMissing)
            {
                this.ReportError(element.Location, EdmErrorCode.MissingAttribute, Edm.Strings.XmlParser_MissingAttribute(attributeName, element.Name));
                return attr;
            }

            return attr;
        }

        protected override XmlReader InitializeReader(XmlReader reader)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings
                                                   {
                                                       CheckCharacters = true,
                                                       CloseInput = false,
                                                       IgnoreWhitespace = true,
                                                       ConformanceLevel = ConformanceLevel.Auto,
                                                       IgnoreComments = true,
                                                       IgnoreProcessingInstructions = true,
#if !ORCAS
                                                       DtdProcessing = DtdProcessing.Prohibit
#endif
                                                   };

            // user specified a stream to read from, read from it.
            // The Uri is just used to identify the stream in errors.
            return XmlReader.Create(reader, readerSettings);
        }

        protected override bool TryGetDocumentVersion(string xmlNamespaceName, out Version version, out string[] expectedNamespaces)
        {
            expectedNamespaces = this.SupportedVersions.Select(v => v.Value).ToArray();
            version = this.SupportedVersions.Where(v => v.Value == xmlNamespaceName).Select(v => v.Key).FirstOrDefault();
            return version != null;
        }

        protected override bool IsOwnedNamespace(string namespaceName)
        {
            return this.IsEdmNamespace(namespaceName);
        }

        #region Utility Methods for derived document parsers

        protected XmlElementParser<TItem> CsdlElement<TItem>(string elementName, Func<XmlElementInfo, XmlElementValueCollection, TItem> initializer, params XmlElementParser[] childParsers)
           where TItem : class
        {
            return Element<TItem>(
                elementName, 
                (element, childValues) =>
                {
                    BeginItem(element);
                    TItem result = initializer(element, childValues);
                    AnnotateItem(result, childValues);
                    EndItem();

                    return result;
                },
                childParsers);
        }

        protected void BeginItem(XmlElementInfo element)
        {
            this.elementStack.Push(element);
            this.currentElement = element;
        }

        protected abstract void AnnotateItem(object result, XmlElementValueCollection childValues);
        
        protected void EndItem()
        {
            this.elementStack.Pop();
            this.currentElement = this.elementStack.Count == 0 ? null : this.elementStack.Peek();
        }

        protected int? OptionalInteger(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                int? value;
                if (!EdmValueParser.TryParseInt(attr.Value, out value))
                {
                    this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidInteger, Edm.Strings.ValueParser_InvalidInteger(attr.Value));
                }

                return value;
            }

            return null;
        }

        protected long? OptionalLong(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                long? value;
                if (!EdmValueParser.TryParseLong(attr.Value, out value))
                {
                    this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidLong, Edm.Strings.ValueParser_InvalidLong(attr.Value));
                }

                return value;
            }

            return null;
        }

        protected int? OptionalSrid(string attributeName, int defaultSrid)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                int? srid;
                if (attr.Value.EqualsOrdinalIgnoreCase(CsdlConstants.Value_SridVariable))
                {
                    srid = null;
                }
                else
                {
                    if (!EdmValueParser.TryParseInt(attr.Value, out srid))
                    {
                        this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidSrid, Edm.Strings.ValueParser_InvalidSrid(attr.Value));
                    }
                }

                return srid;
            }

            return defaultSrid;
        }

        protected int? OptionalMaxLength(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                int? value;
                if (!EdmValueParser.TryParseInt(attr.Value, out value))
                {
                    this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidMaxLength, Edm.Strings.ValueParser_InvalidMaxLength(attr.Value));
                }

                return value;
            }

            return null;
        }

        protected EdmFunctionParameterMode? OptionalFunctionParameterMode(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                switch (attr.Value)
                {
                    case CsdlConstants.Value_ModeIn:
                        return EdmFunctionParameterMode.In;
                    case CsdlConstants.Value_ModeInOut:
                        return EdmFunctionParameterMode.InOut;
                    case CsdlConstants.Value_ModeOut:
                        return EdmFunctionParameterMode.Out;
                    default:
                        this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidParameterMode, Edm.Strings.CsdlParser_InvalidParameterMode(attr.Value));
                        return EdmFunctionParameterMode.None;
                }
            }

            return null;
        }

        protected EdmConcurrencyMode? OptionalConcurrencyMode(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                switch (attr.Value)
                {
                    case CsdlConstants.Value_None:
                        return EdmConcurrencyMode.None;
                    case CsdlConstants.Value_Fixed:
                        return EdmConcurrencyMode.Fixed;
                    default:
                        this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidConcurrencyMode, Edm.Strings.CsdlParser_InvalidConcurrencyMode(attr.Value));
                        break;
                }
            }

            return null;
        }

        protected EdmMultiplicity RequiredMultiplicity(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                switch (attr.Value)
                {
                    case CsdlConstants.Value_EndRequired:
                        return EdmMultiplicity.One;
                    case CsdlConstants.Value_EndOptional:
                        return EdmMultiplicity.ZeroOrOne;
                    case CsdlConstants.Value_EndMany:
                        return EdmMultiplicity.Many;
                    default:
                        this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidMultiplicity, Edm.Strings.CsdlParser_InvalidMultiplicity(attr.Value));
                        break;
                }
            }

           return EdmMultiplicity.One;
        }

        protected EdmOnDeleteAction RequiredOnDeleteAction(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                switch (attr.Value)
                {
                    case CsdlConstants.Value_None:
                        return EdmOnDeleteAction.None;
                    case CsdlConstants.Value_Cascade:
                        return EdmOnDeleteAction.Cascade;
                    default:
                        this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidOnDelete, Edm.Strings.CsdlParser_InvalidDeleteAction(attr.Value));
                        break;
                }
            }

            return EdmOnDeleteAction.None;
        }

        protected bool? OptionalBoolean(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                bool? value;
                if (!EdmValueParser.TryParseBool(attr.Value, out value))
                {
                    this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidBoolean, Edm.Strings.ValueParser_InvalidBoolean(attr.Value));
                }

                return value;
            }

            return null;
        }

        protected string Optional(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            return !attr.IsMissing ? attr.Value : null;
        }

        protected string Required(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            return !attr.IsMissing ? attr.Value : string.Empty;
        }

        protected string OptionalAlias(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateAlias(attr.Value);
            }

            return null;
        }

        protected string RequiredAlias(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateAlias(attr.Value);
            }

            return null;
        }

        protected string RequiredEntitySetPath(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateEntitySetPath(attr.Value);
            }

            return null;
        }

        protected string RequiredEnumMemberPath(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateEnumMemberPath(attr.Value);
            }

            return null;
        }

        protected string OptionalType(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateTypeName(attr.Value);
            }

            return null;
        }

        protected string RequiredType(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateTypeName(attr.Value);
            }

            return null;
        }

        protected string OptionalQualifiedName(string attributeName)
        {
            XmlAttributeInfo attr = this.GetOptionalAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateQualifiedName(attr.Value);
            }

            return null;
        }

        protected string RequiredQualifiedName(string attributeName)
        {
            XmlAttributeInfo attr = this.GetRequiredAttribute(this.currentElement, attributeName);
            if (!attr.IsMissing)
            {
                return this.ValidateQualifiedName(attr.Value);
            }

            return null;
        }

        private string ValidateTypeName(string name)
        {
            string[] typeInformation = name.Split(new char[] { '(', ')' });
            string typeName = typeInformation[0];

            // For inline types, we need to check that the name contained inside is a valid type name
            switch (typeName)
            {
                case CsdlConstants.Value_Collection:
                    // 'Collection' on its own is a valid type string.
                    if (typeInformation.Count() == 1)
                    {
                        return name;
                    }
                    else
                    {
                        typeName = typeInformation[1];
                    }

                    break;
                case CsdlConstants.Value_Ref:
                    // 'Ref' on its own is not a valid type string.
                    if (typeInformation.Count() == 1)
                    {
                        this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidTypeName, Edm.Strings.CsdlParser_InvalidTypeName(name));
                        return name;
                    }
                    else
                    {
                        typeName = typeInformation[1];
                    }

                    break;
            }

            if (EdmUtil.IsQualifiedName(typeName) || Microsoft.Data.Edm.Library.EdmCoreModel.Instance.GetPrimitiveTypeKind(typeName) != EdmPrimitiveTypeKind.None)
            {
                return name;
            }
            else
            {
                this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidTypeName, Edm.Strings.CsdlParser_InvalidTypeName(name));
                return name;
            }
        }

        private string ValidateAlias(string name)
        {
            if (!EdmUtil.IsValidUndottedName(name))
            {
                this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidQualifiedName, Edm.Strings.CsdlParser_InvalidAlias(name));
            }

            return name;
        }

        private string ValidateEntitySetPath(string path)
        {
            string[] segments = path.Split('/');
            if (!(segments.Count() == 2 &&
                EdmUtil.IsValidDottedName(segments[0]) &&
                EdmUtil.IsValidUndottedName(segments[1])))
            {
                this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidEntitySetPath, Edm.Strings.CsdlParser_InvalidEntitySetPath(path));
            }

            return path;
        }

        private string ValidateEnumMemberPath(string path)
        {
            string[] segments = path.Split('/');
            if (!(segments.Count() == 2 &&
                EdmUtil.IsValidDottedName(segments[0]) &&
                EdmUtil.IsValidUndottedName(segments[1])))
            {
                this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidEnumMemberPath, Edm.Strings.CsdlParser_InvalidEnumMemberPath(path));
            }

            return path;
        }

        private string ValidateQualifiedName(string qualifiedName)
        {
            if (!EdmUtil.IsQualifiedName(qualifiedName))
            {
                this.ReportError(this.currentElement.Location, EdmErrorCode.InvalidQualifiedName, Edm.Strings.CsdlParser_InvalidQualifiedName(qualifiedName));
            }

            return qualifiedName;
        }

        private bool IsEdmNamespace(string xmlNamespaceUri)
        {
            Debug.Assert(!string.IsNullOrEmpty(xmlNamespaceUri), "Ensure namespace URI is not null or empty before calling IsEdmNamespace");

            if (this.edmNamespaces == null)
            {
                this.edmNamespaces = new HashSetInternal<string>();
                foreach (var namespaces in CsdlConstants.SupportedVersions.Values)
                {
                    foreach (var edmNamespace in namespaces)
                    {
                        this.edmNamespaces.Add(edmNamespace);
                    }
                }
            }

            return this.edmNamespaces.Contains(xmlNamespaceUri);
        }
        #endregion
    }
}

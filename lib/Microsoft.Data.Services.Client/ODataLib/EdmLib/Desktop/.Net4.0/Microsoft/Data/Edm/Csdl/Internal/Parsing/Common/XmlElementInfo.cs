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
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
    internal interface IXmlElementAttributes
    {
        IEnumerable<XmlAttributeInfo> Unused
        {
            get;
        }

        XmlAttributeInfo this[string attributeName]
        {
            get;
        }
    }

    internal class XmlElementInfo : IXmlElementAttributes
    {
        private readonly Dictionary<string, XmlAttributeInfo> attributes;
        private List<XmlAnnotationInfo> annotations;

        internal XmlElementInfo(string elementName, CsdlLocation elementLocation, IList<XmlAttributeInfo> attributes, List<XmlAnnotationInfo> annotations)
        {
            this.Name = elementName;
            this.Location = elementLocation;
            if (attributes != null && attributes.Count > 0)
            {
                this.attributes = new Dictionary<string, XmlAttributeInfo>();
                foreach (XmlAttributeInfo newAttr in attributes)
                {
                    Debug.Assert(!this.attributes.ContainsKey(newAttr.Name), "Multiple attributes with the same name are not supported");
                    this.attributes.Add(newAttr.Name, newAttr);
                }
            }

            this.annotations = annotations;
        }

        IEnumerable<XmlAttributeInfo> IXmlElementAttributes.Unused
        {
            get
            {
                if (this.attributes != null)
                {
                    foreach (XmlAttributeInfo attr in this.attributes.Values.Where(attr => !attr.IsUsed))
                    {
                        yield return attr;
                    }
                }
            }
        }

        internal string Name
        {
            get;
            private set;
        }

        internal CsdlLocation Location
        {
            get;
            private set;
        }

        internal IXmlElementAttributes Attributes
        {
            get { return (IXmlElementAttributes)this; }
        }

        internal IList<XmlAnnotationInfo> Annotations
        {
            get { return this.annotations ?? ((IList<XmlAnnotationInfo>)new XmlAnnotationInfo[] { }); }
        }

        XmlAttributeInfo IXmlElementAttributes.this[string attributeName]
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(attributeName), "Ensure attribute name is not null or empty before accessing Attributes");

                XmlAttributeInfo foundAttr;
                if (this.attributes != null && this.attributes.TryGetValue(attributeName, out foundAttr))
                {
                    foundAttr.IsUsed = true;
                    return foundAttr;
                }

                return XmlAttributeInfo.Missing;
            }
        }

        internal void AddAnnotation(XmlAnnotationInfo annotation)
        {
            if (this.annotations == null)
            {
                this.annotations = new List<XmlAnnotationInfo>();
            }

            this.annotations.Add(annotation);
        }
    }

    internal class XmlAnnotationInfo
    {
        internal XmlAnnotationInfo(CsdlLocation location, string namespaceName, string name, string value, bool isAttribute)
        {
            this.Location = location;
            this.NamespaceName = namespaceName;
            this.Name = name;
            this.Value = value;
            this.IsAttribute = isAttribute;
        }

        internal string NamespaceName
        {
            get;
            private set;
        }

        internal string Name
        {
            get; 
            private set;
        }

        internal CsdlLocation Location
        {
            get; 
            private set;
        }

        internal string Value
        {
            get; 
            private set;
        }

        internal bool IsAttribute
        {
            get; 
            private set;
        }
    }

    internal class XmlAttributeInfo
    {
        internal static readonly XmlAttributeInfo Missing = new XmlAttributeInfo();
        private readonly string name;
        private readonly string attributeValue;
        private readonly CsdlLocation location;

        internal XmlAttributeInfo(string attrName, string attrValue, CsdlLocation attrLocation)
        {
            this.name = attrName;
            this.attributeValue = attrValue;
            this.location = attrLocation;
        }

        private XmlAttributeInfo()
        {
        }

        internal bool IsMissing
        {
            get { return object.ReferenceEquals(XmlAttributeInfo.Missing, this); }
        }

        internal bool IsUsed
        {
            get; 
            set;
        }

        internal CsdlLocation Location
        {
            get { return this.location; }
        }

        internal string Name
        {
            get { return this.name; }
        }

        internal string Value
        {
            get { return this.attributeValue; }
        }
    }
}

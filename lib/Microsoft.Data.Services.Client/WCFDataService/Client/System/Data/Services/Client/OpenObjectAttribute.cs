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

#if ASTORIA_OPEN_OBJECT
namespace System.Data.Services.Client
{
    using System;

    /// <summary>
    /// Attribute to be annotated on class to designate the name of the instance property to store name-value pairs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OpenObjectAttribute : System.Attribute
    {
        /// <summary>
        /// The name of the instance property returning an IDictionary&lt;string,object&gt;.
        /// </summary>
        private readonly string openObjectPropertyName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="openObjectPropertyName">The name of the instance property returning an IDictionary&lt;string,object&gt;.</param>
        public OpenObjectAttribute(string openObjectPropertyName)
        {
            Util.CheckArgumentNotEmpty(openObjectPropertyName, "openObjectPropertyName");
            this.openObjectPropertyName = openObjectPropertyName;
        }

        /// <summary>
        /// The name of the instance property returning an IDictionary&lt;string,object&gt;.
        /// </summary>
        public string OpenObjectPropertyName
        {
            get { return this.openObjectPropertyName; }
        }
    }
}
#endif

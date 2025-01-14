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
using System.Globalization;

namespace Microsoft.Data.Edm.Csdl
{
    /// <summary>
    /// Defines a location in a XML file.
    /// </summary>
    public class CsdlLocation : EdmLocation
    {
        internal CsdlLocation(int number, int position)
        {
            this.LineNumber = number;
            this.LinePosition = position;
        }

        /// <summary>
        /// Gets the line number in the file.
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets the position in the line.
        /// </summary>
        public int LinePosition { get; private set; }

        /// <summary>
        /// Gets a string representation of the location.
        /// </summary>
        /// <returns>A string representation of the location.</returns>
        public override string ToString()
        {
            return "(" + Convert.ToString(this.LineNumber, CultureInfo.InvariantCulture) + ", " + Convert.ToString(this.LinePosition, CultureInfo.InvariantCulture) + ")";
        }
    }
}

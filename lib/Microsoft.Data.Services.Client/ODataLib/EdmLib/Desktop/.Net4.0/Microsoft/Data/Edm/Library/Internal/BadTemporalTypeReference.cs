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
using Microsoft.Data.Edm.Validation;

namespace Microsoft.Data.Edm.Library.Internal
{
    /// <summary>
    /// Represents a reference to a semantically invalid EDM temporal (Time, DateTime, DateTimeOffset) type.
    /// </summary>
    internal class BadTemporalTypeReference : EdmTemporalTypeReference, IEdmCheckable
    {
        private readonly IEnumerable<EdmError> errors;

        public BadTemporalTypeReference(string qualifiedName, bool isNullable, IEnumerable<EdmError> errors)
            : base(new BadPrimitiveType(qualifiedName, EdmPrimitiveTypeKind.None, errors), isNullable, null)
        {
            this.errors = errors;
        }

        public IEnumerable<EdmError> Errors
        {
            get { return this.errors; }
        }

        public override string ToString()
        {
            EdmError error = this.Errors.FirstOrDefault();
            Debug.Assert(error != null, "error != null");
            string prefix = error != null ? error.ErrorCode.ToString() + ":" : "";
            return prefix + this.ToTraceString();
        }
    }
}

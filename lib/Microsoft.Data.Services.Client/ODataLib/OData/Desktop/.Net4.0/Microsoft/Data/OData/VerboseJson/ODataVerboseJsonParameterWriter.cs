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

namespace Microsoft.Data.OData.VerboseJson
{
    #region Namespaces
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    #endregion Namespaces

    /// <summary>
    /// ODataParameterWriter for the Verbose JSON format.
    /// </summary>
    internal sealed class ODataVerboseJsonParameterWriter : ODataParameterWriterCore
    {
        /// <summary>
        /// The output context to write to.
        /// </summary>
        private readonly ODataVerboseJsonOutputContext verboseJsonOutputContext;

        /// <summary>
        /// The JSON property and value serializer to use.
        /// </summary>
        private readonly ODataVerboseJsonPropertyAndValueSerializer verboseJsonPropertyAndValueSerializer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="verboseJsonOutputContext">The output context to write to.</param>
        /// <param name="functionImport">The function import whose parameters will be written.</param>
        internal ODataVerboseJsonParameterWriter(ODataVerboseJsonOutputContext verboseJsonOutputContext, IEdmFunctionImport functionImport)
            : base(verboseJsonOutputContext, functionImport)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(verboseJsonOutputContext != null, "verboseJsonOutputContext != null");

            this.verboseJsonOutputContext = verboseJsonOutputContext;
            this.verboseJsonPropertyAndValueSerializer = new ODataVerboseJsonPropertyAndValueSerializer(this.verboseJsonOutputContext);
        }

        /// <summary>
        /// Check if the object has been disposed; called from all public API methods. Throws an ObjectDisposedException if the object
        /// has already been disposed.
        /// </summary>
        protected override void VerifyNotDisposed()
        {
            this.verboseJsonOutputContext.VerifyNotDisposed();
        }

        /// <summary>
        /// Flush the output.
        /// </summary>
        protected override void FlushSynchronously()
        {
            this.verboseJsonOutputContext.Flush();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Flush the output.
        /// </summary>
        /// <returns>Task representing the pending flush operation.</returns>
        protected override Task FlushAsynchronously()
        {
            return this.verboseJsonOutputContext.FlushAsync();
        }
#endif

        /// <summary>
        /// Start writing an OData payload.
        /// </summary>
        protected override void StartPayload()
        {
            // We are always writing a request payload here, no need to write the data wrapper.
            this.verboseJsonPropertyAndValueSerializer.WritePayloadStart();
            this.verboseJsonOutputContext.JsonWriter.StartObjectScope();
        }

        /// <summary>
        /// Finish writing an OData payload.
        /// </summary>
        protected override void EndPayload()
        {
            // We are always writing a request payload here, no need to write the data wrapper.
            this.verboseJsonOutputContext.JsonWriter.EndObjectScope();
            this.verboseJsonPropertyAndValueSerializer.WritePayloadEnd();
        }

        /// <summary>
        /// Writes a value parameter (either primitive or complex)
        /// </summary>
        /// <param name="parameterName">The name of the parameter to write.</param>
        /// <param name="parameterValue">The value of the parameter to write.</param>
        /// <param name="expectedTypeReference">The expected type reference of the parameter value.</param>
        protected override void WriteValueParameter(string parameterName, object parameterValue, IEdmTypeReference expectedTypeReference)
        {
            Debug.Assert(!string.IsNullOrEmpty(parameterName), "!string.IsNullOrEmpty(parameterName)");
            this.verboseJsonOutputContext.JsonWriter.WriteName(parameterName);
            if (parameterValue == null)
            {
                this.verboseJsonOutputContext.JsonWriter.WriteValue(null);
            }
            else
            {
                ODataComplexValue complexValue = parameterValue as ODataComplexValue;
                if (complexValue != null)
                {
                    this.verboseJsonPropertyAndValueSerializer.AssertRecursionDepthIsZero();
                    this.verboseJsonPropertyAndValueSerializer.WriteComplexValue(
                        complexValue,
                        expectedTypeReference,
                        false,
                        this.DuplicatePropertyNamesChecker,
                        null /*collectionValidator*/);
                    this.verboseJsonPropertyAndValueSerializer.AssertRecursionDepthIsZero();
                    this.DuplicatePropertyNamesChecker.Clear();
                }
                else
                {
                    Debug.Assert(!(parameterValue is ODataCollectionValue), "!(parameterValue is ODataCollectionValue)");
                    Debug.Assert(!(parameterValue is ODataStreamReferenceValue), "!(parameterValue is ODataStreamReferenceValue)");
                    Debug.Assert(!(parameterValue is Stream), "!(parameterValue is Stream)");
                    this.verboseJsonPropertyAndValueSerializer.WritePrimitiveValue(parameterValue, /*collectionValidator*/ null, expectedTypeReference);   
                }
            }
        }

        /// <summary>
        /// Creates a format specific <see cref="ODataCollectionWriter"/> to write the value of a collection parameter.
        /// </summary>
        /// <param name="parameterName">The name of the collection parameter to write.</param>
        /// <param name="expectedItemType">The type reference of the expected item type or null if no expected item type exists.</param>
        /// <returns>The newly created <see cref="ODataCollectionWriter"/>.</returns>
        protected override ODataCollectionWriter CreateFormatCollectionWriter(string parameterName, IEdmTypeReference expectedItemType)
        {
            Debug.Assert(!string.IsNullOrEmpty(parameterName), "!string.IsNullOrEmpty(parameterName)");
            this.verboseJsonOutputContext.JsonWriter.WriteName(parameterName);
            return new ODataVerboseJsonCollectionWriter(this.verboseJsonOutputContext, expectedItemType, /*listener*/ this);
        }
    }
}

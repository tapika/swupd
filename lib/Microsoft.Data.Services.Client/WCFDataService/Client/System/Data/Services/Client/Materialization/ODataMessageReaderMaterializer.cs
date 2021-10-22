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

namespace System.Data.Services.Client.Materialization
{
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using ClientStrings = System.Data.Services.Client.Strings;

    /// <summary>
    /// Used to materialize a value from an <see cref="ODataMessageReader"/>.
    /// </summary>
    internal abstract class ODataMessageReaderMaterializer : ODataMaterializer
    {
        /// <summary>Optional field that indicates if we should expect a single result to materialize, as opposed to a collection.</summary>
        protected readonly bool? SingleResult;

        /// <summary>Reader for a message that contains a value or property.</summary>
        protected ODataMessageReader messageReader;

        /// <summary>Has the value been read.</summary>
        private bool hasReadValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMessageReaderMaterializer"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="context">The materializer context.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="singleResult">The single result.</param>
        public ODataMessageReaderMaterializer(ODataMessageReader reader, IODataMaterializerContext context, Type expectedType, bool? singleResult)
            : base(context, expectedType)
        {
            this.messageReader = reader;
            this.SingleResult = singleResult;
        }

        /// <summary>
        /// Feed being materialized; possibly null.
        /// </summary>
        internal sealed override ODataFeed CurrentFeed
        {
            get { return null; }
        }

        /// <summary>
        /// Entry being materialized; possibly null.
        /// </summary>
        internal sealed override ODataEntry CurrentEntry
        {
            get { return null; }
        }

        /// <summary>
        /// Whether we have finished processing the current data stream.
        /// </summary>
        internal sealed override bool IsEndOfStream
        {
            get { return this.hasReadValue; }
        }

        /// <summary>
        /// The count tag's value, if requested
        /// </summary>
        /// <returns>The count value returned from the server</returns>
        internal override long CountValue
        {
            get { throw new InvalidOperationException(ClientStrings.MaterializeFromAtom_CountNotPresent); }
        }

        /// <summary>
        /// Function to materialize an entry and produce a value.
        /// </summary>
        internal sealed override ProjectionPlan MaterializeEntryPlan
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Returns true if the materializer has been disposed
        /// </summary>
        protected sealed override bool IsDisposed
        {
            get { return this.messageReader == null; }
        }

        /// <summary>
        /// The format of the response being materialized.
        /// </summary>
        protected override ODataFormat Format
        {
            get { return ODataUtils.GetReadFormat(this.messageReader); }
        }

        /// <summary>Clears the materialization log of activity.</summary>
        internal sealed override void ClearLog()
        {
            // no log to clear
        }

        /// <summary>Applies the materialization log to the context.</summary>
        internal sealed override void ApplyLogToContext()
        {
            // no log to apply
        }

        /// <summary>
        /// Implementation of Read>.
        /// </summary>
        /// <returns>
        /// Return value of Read/>
        /// </returns>
        protected sealed override bool ReadImplementation()
        {
            if (!this.hasReadValue)
            {
                try
                {
                    ClientEdmModel model = this.MaterializerContext.Model;
                    Type expectedType = this.ExpectedType;
                    IEdmTypeReference expectedClientType = model.GetOrCreateEdmType(expectedType).ToEdmTypeReference(ClientTypeUtil.CanAssignNull(expectedType));
                    if ((this.SingleResult.HasValue && !this.SingleResult.Value) && expectedClientType.Definition.TypeKind != EdmTypeKind.Collection)
                    {
                        expectedType = typeof(ICollection<>).MakeGenericType(expectedType);

                        // we do not allow null values for collection
                        expectedClientType = model.GetOrCreateEdmType(expectedType).ToEdmTypeReference(false);
                    }

                    IEdmTypeReference expectedReaderType = this.MaterializerContext.ResolveExpectedTypeForReading(expectedType).ToEdmTypeReference(expectedClientType.IsNullable);

                    this.ReadWithExpectedType(expectedClientType, expectedReaderType);
                }
                catch (ODataErrorException e)
                {
                    throw new DataServiceClientException(ClientStrings.Deserialize_ServerException(e.Error.Message), e);
                }
                catch (ODataException e)
                {
                    throw new InvalidOperationException(e.Message, e);
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException(e.Message, e);
                }
                finally
                {
                    this.hasReadValue = true;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Called when IDisposable.Dispose is called.
        /// </summary>
        protected sealed override void OnDispose()
        {
            if (this.messageReader != null)
            {
                this.messageReader.Dispose();
                this.messageReader = null;
            }
        }

        /// <summary>
        /// Reads a value from the message reader.
        /// </summary>
        /// <param name="expectedClientType">The expected client type being materialized into.</param>
        /// <param name="expectedReaderType">The expected type for the underlying reader.</param>
        protected abstract void ReadWithExpectedType(IEdmTypeReference expectedClientType, IEdmTypeReference expectedReaderType);
    }
}

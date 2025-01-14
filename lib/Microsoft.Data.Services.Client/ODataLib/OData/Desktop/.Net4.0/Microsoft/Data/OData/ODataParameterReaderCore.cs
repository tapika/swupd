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

namespace Microsoft.Data.OData
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    #endregion Namespaces

    /// <summary>
    /// Base class for OData parameter readers that verifies a proper sequence of read calls on the reader.
    /// </summary>
    internal abstract class ODataParameterReaderCore : ODataParameterReader, IODataReaderWriterListener
    {
        /// <summary>The input context to read from.</summary>
        private readonly ODataInputContext inputContext;

        /// <summary>The function import whose parameters are being read.</summary>
        private readonly IEdmFunctionImport functionImport;

        /// <summary>Stack of reader scopes to keep track of the current context of the reader.</summary>
        private readonly Stack<Scope> scopes = new Stack<Scope>();

        /// <summary>Hash set to keep track of all the parameters read from the payload.</summary>
        private readonly HashSet<string> parametersRead = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Tracks the state of the sub-reader.</summary>
        private SubReaderState subReaderState;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputContext">The input to read from.</param>
        /// <param name="functionImport">The function import whose parameters are being read.</param>
        protected ODataParameterReaderCore(
            ODataInputContext inputContext,
            IEdmFunctionImport functionImport)
        {
            this.inputContext = inputContext;
            this.functionImport = functionImport;

            this.EnterScope(ODataParameterReaderState.Start, null, null);
        }

        /// <summary>Enum to track the state of the sub-reader.</summary>
        private enum SubReaderState
        {
            /// <summary>No sub-reader has been created for the current parameter.</summary>
            None,

            /// <summary>CreateEntryReader(), CreateFeedReader() or CreateCollectionReader() has been called for the current parameter
            /// and the newly created reader is not in Completed state.</summary>
            /// <remarks>If the sub-reader is in Error state, the ODataParameterReader will enter ODataParameterReaderState.Error.</remarks>
            Active,

            /// <summary>The created sub-reader is in Completed state.</summary>
            Completed,
        }

        /// <summary>
        /// The current state of the reader.
        /// </summary>
        public override sealed ODataParameterReaderState State
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                Debug.Assert(this.scopes != null && this.scopes.Count > 0, "A scope must always exist.");
                return this.scopes.Peek().State;
            }
        }

        /// <summary>
        /// The name of the current parameter that is being read.
        /// </summary>
        public override string Name
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                Debug.Assert(this.scopes != null && this.scopes.Count > 0, "A scope must always exist.");
                return this.scopes.Peek().Name;
            }
        }

        /// <summary>
        /// The value of the current parameter that is being read.
        /// </summary>
        /// <remarks>
        /// This property returns a primitive value, an ODataComplexValue or null when State is ODataParameterReaderState.Value.
        /// This property returns null when State is ODataParameterReaderState.Entry, ODataParameterReaderState.Feed or ODataParameterReaderState.Collection.
        /// </remarks>
        public override object Value
        {
            get
            {
                this.inputContext.VerifyNotDisposed();
                Debug.Assert(this.scopes != null && this.scopes.Count > 0, "A scope must always exist.");
                return this.scopes.Peek().Value;
            }
        }

        /// <summary>
        /// The function import whose parameters are being read.
        /// </summary>
        protected IEdmFunctionImport FunctionImport
        {
            get
            {
                return this.functionImport;
            }
        }

#if SUPPORT_ENTITY_PARAMETER        
        /// <summary>
        /// This method creates an <see cref="ODataReader"/> to read the entry value when the state is ODataParameterReaderState.Entry.
        /// </summary>
        /// <remarks>
        /// When the state is ODataParameterReaderState.Entry, the Name property of the <see cref="ODataParameterReader"/> returns the name of the parameter
        /// and the Value property of the <see cref="ODataParameterReader"/> returns null. Calling this method in any other state will cause an ODataException to be thrown.
        /// </remarks>
        /// <returns>Returns an <see cref="ODataReader"/> to read the entry value when the state is ODataParameterReaderState.Entry.</returns>
        public override ODataReader CreateEntryReader()
        {
            this.VerifyCanCreateSubReader(ODataParameterReaderState.Entry);
            this.subReaderState = SubReaderState.Active;
            Debug.Assert(this.Name != null, "this.Name != null");
            Debug.Assert(this.Value == null, "this.Value == null");
            IEdmEntityType expectedEntityType = (IEdmEntityType)this.GetParameterTypeReference(this.Name).Definition;
            return this.CreateEntryReader(expectedEntityType);
        }

        /// <summary>
        /// This method creates an <see cref="ODataReader"/> to read the feed value when the state is ODataParameterReaderState.Feed.
        /// </summary>
        /// <remarks>
        /// When the state is ODataParameterReaderState.Feed, the Name property of the <see cref="ODataParameterReader"/> returns the name of the parameter
        /// and the Value property of the <see cref="ODataParameterReader"/> returns null. Calling this method in any other state will cause an ODataException to be thrown.
        /// </remarks>
        /// <returns>Returns an <see cref="ODataReader"/> to read the feed value when the state is ODataParameterReaderState.Feed.</returns>
        public override ODataReader CreateFeedReader()
        {
            this.VerifyCanCreateSubReader(ODataParameterReaderState.Feed);
            this.subReaderState = SubReaderState.Active;
            Debug.Assert(this.Name != null, "this.Name != null");
            Debug.Assert(this.Value == null, "this.Value == null");
            IEdmEntityType expectedEntityType = (IEdmEntityType)((IEdmCollectionType)this.GetParameterTypeReference(this.Name).Definition).ElementType.Definition;
            return this.CreateFeedReader(expectedEntityType);
        }
#endif

        /// <summary>
        /// This method creates an <see cref="ODataCollectionReader"/> to read the collection value when the state is ODataParameterReaderState.Collection.
        /// </summary>
        /// <remarks>
        /// When the state is ODataParameterReaderState.Collection, the Name property of the <see cref="ODataParameterReader"/> returns the name of the parameter
        /// and the Value property of the <see cref="ODataParameterReader"/> returns null. Calling this method in any other state will cause an ODataException to be thrown.
        /// </remarks>
        /// <returns>Returns an <see cref="ODataCollectionReader"/> to read the collection value when the state is ODataParameterReaderState.Collection.</returns>
        public override ODataCollectionReader CreateCollectionReader()
        {
            this.VerifyCanCreateSubReader(ODataParameterReaderState.Collection);
            this.subReaderState = SubReaderState.Active;
            Debug.Assert(this.Name != null, "this.Name != null");
            Debug.Assert(this.Value == null, "this.Value == null");
            IEdmTypeReference expectedItemTypeReference = ((IEdmCollectionType)this.GetParameterTypeReference(this.Name).Definition).ElementType;
            return this.CreateCollectionReader(expectedItemTypeReference);
        }

        /// <summary>
        /// Reads the next item from the message payload.
        /// </summary>
        /// <returns>true if more items were read; otherwise false.</returns>
        public override sealed bool Read()
        {
            this.VerifyCanRead(true);
            return this.InterceptException(this.ReadSynchronously);
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously reads the next item from the message payload.
        /// </summary>
        /// <returns>A task that when completed indicates whether more items were read.</returns>
        [SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "API design calls for a bool being returned from the task here.")]
        public override sealed Task<bool> ReadAsync()
        {
            this.VerifyCanRead(false);
            return this.ReadAsynchronously().FollowOnFaultWith(t => this.EnterScope(ODataParameterReaderState.Exception, null, null));
        }
#endif

        /// <summary>
        /// This method notifies the implementer of this interface that the created reader is in Exception state.
        /// </summary>
        void IODataReaderWriterListener.OnException()
        {
            Debug.Assert(
#if SUPPORT_ENTITY_PARAMETER
                this.State == ODataParameterReaderState.Entry ||
                this.State == ODataParameterReaderState.Feed ||
#endif
                this.State == ODataParameterReaderState.Collection,
                "OnException called in unexpected state: " + this.State);
            Debug.Assert(this.State == ODataParameterReaderState.Exception || this.subReaderState == SubReaderState.Active, "OnException called in unexpected subReaderState: " + this.subReaderState);
            this.EnterScope(ODataParameterReaderState.Exception, null, null);
        }

        /// <summary>
        /// This method notifies the implementer of this interface that the created reader is in Completed state.
        /// </summary>
        void IODataReaderWriterListener.OnCompleted()
        {
            Debug.Assert(
#if SUPPORT_ENTITY_PARAMETER
                this.State == ODataParameterReaderState.Entry ||
                this.State == ODataParameterReaderState.Feed ||
#endif
                this.State == ODataParameterReaderState.Collection,
                "OnCompleted called in unexpected state: " + this.State);
            Debug.Assert(this.State == ODataParameterReaderState.Exception || this.subReaderState == SubReaderState.Active, "OnCompleted called in unexpected subReaderState: " + this.subReaderState);
            this.subReaderState = SubReaderState.Completed;
        }

        /// <summary>
        /// Returns the type reference of the parameter in question.
        /// </summary>
        /// <param name="parameterName">Name of the parameter in question.</param>
        /// <returns>Returns the type reference of the parameter in question.</returns>
        protected internal IEdmTypeReference GetParameterTypeReference(string parameterName)
        {
            Debug.Assert(!string.IsNullOrEmpty(parameterName), "!string.IsNullOrEmpty(parameterName)");
            IEdmFunctionParameter parameter = this.FunctionImport.FindParameter(parameterName);
            if (parameter == null)
            {
                throw new ODataException(Strings.ODataParameterReaderCore_ParameterNameNotInMetadata(parameterName, this.FunctionImport.Name));
            }

            return this.inputContext.EdmTypeResolver.GetParameterType(parameter);
        }

        /// <summary>
        /// Creates a new <see cref="Scope"/> for the specified <paramref name="state"/> with the provided
        /// <paramref name="name"/> and <paramref name="value"/> and pushes it on the stack of scopes.
        /// </summary>
        /// <param name="state">The <see cref="ODataParameterReaderState"/> to use for the new scope.</param>
        /// <param name="name">The paramter name to attach with the state in the new scope.</param>
        /// <param name="value">The paramter value to attach with the state in the new scope.</param>
        protected internal void EnterScope(ODataParameterReaderState state, string name, object value)
        {
            if (state == ODataParameterReaderState.Value)
            {
                if (value != null && !(value is ODataComplexValue) && !EdmLibraryExtensions.IsPrimitiveType(value.GetType()))
                {
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataParameterReaderCore_ValueMustBePrimitiveOrComplexOrNull));
                }
            }

            Debug.Assert(this.scopes.Count == 0 || this.State != ODataParameterReaderState.Exception || state == ODataParameterReaderState.Exception, "If the reader is already in Exception state, we shouldn't call EnterScope() except for another Exception.");

            // We only need to enter the exception state once.
            if (this.scopes.Count == 0 || this.State != ODataParameterReaderState.Exception)
            {
                Debug.Assert(
                    state == ODataParameterReaderState.Exception ||
                    state == ODataParameterReaderState.Start && this.scopes.Count == 0 ||
                    state == ODataParameterReaderState.Completed && this.scopes.Count == 0 ||
                    this.scopes.Count == 1 && this.scopes.Peek().State == ODataParameterReaderState.Start,
                    "Unexpected state in the scopes stack.");

                // Make sure there aren't any missing parameter in the payload.
                if (state == ODataParameterReaderState.Completed)
                {
                    List<string> missingParameters = new List<string>();

                    // Note that the binding parameter will be specified on the Uri rather than the payload, skip the binding parameter.
                    foreach (IEdmFunctionParameter parameter in this.FunctionImport.Parameters.Skip(this.FunctionImport.IsBindable ? 1 : 0))
                    {
                        if (!this.parametersRead.Contains(parameter.Name) && !this.inputContext.EdmTypeResolver.GetParameterType(parameter).IsNullable)
                        {
                            missingParameters.Add(parameter.Name);
                        }
                    }

                    if (missingParameters.Count > 0)
                    {
                        this.scopes.Push(new Scope(ODataParameterReaderState.Exception, null, null));
                        throw new ODataException(Strings.ODataParameterReaderCore_ParametersMissingInPayload(this.FunctionImport.Name, string.Join(",", missingParameters.ToArray())));
                    }
                }
                else if (name != null)
                {
                    // Record the parameter names we read and check for duplicates.
                    if (this.parametersRead.Contains(name))
                    {
                        throw new ODataException(Strings.ODataParameterReaderCore_DuplicateParametersInPayload(name));
                    }

                    this.parametersRead.Add(name);
                }

                this.scopes.Push(new Scope(state, name, value));
            }
        }

        /// <summary>
        /// Removes the current scope from the stack of all scopes.
        /// </summary>
        /// <param name="state">The expected state of the current scope (to be popped).</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "state", Justification = "Used in debug builds in assertions.")]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "scope", Justification = "Used in debug builds in assertions.")]
        protected internal void PopScope(ODataParameterReaderState state)
        {
            Debug.Assert(this.scopes.Count > 0, "Stack must have more than 1 items in order to pop an item.");

            Scope scope = this.scopes.Pop();
            Debug.Assert(scope.State == state, "scope.State == state");
        }

        /// <summary>
        /// Called when the a parameter was completed.
        /// </summary>
        protected void OnParameterCompleted()
        {
            Debug.Assert(this.State == ODataParameterReaderState.Value || this.subReaderState == SubReaderState.Completed, "this.State == ODataParameterReaderState.Value || this.subReaderState == SubReaderState.Completed");
            this.subReaderState = SubReaderState.None;
        }

        /// <summary>
        /// Reads the next <see cref="ODataItem"/> from the message payload.
        /// </summary>
        /// <returns>true if more items were read; otherwise false.</returns>
        protected bool ReadImplementation()
        {
            bool result = false;
            switch (this.State)
            {
                case ODataParameterReaderState.Start:
                    result = this.ReadAtStartImplementation();
                    Debug.Assert(
                        this.State == ODataParameterReaderState.Value ||
#if SUPPORT_ENTITY_PARAMETER
                        this.State == ODataParameterReaderState.Entry ||
                        this.State == ODataParameterReaderState.Feed ||
#endif
                        this.State == ODataParameterReaderState.Collection ||
                        this.State == ODataParameterReaderState.Completed,
                        "ReadAtStartImplementation should transition the state to ODataParameterReaderState.Value, ODataParameterReaderState.Entry, ODataParameterReaderState.Feed, ODataParameterReaderState.Collection or ODataParameterReaderState.Completed. The current state is: " + this.State);
                    break;

                case ODataParameterReaderState.Value:   // fall through
#if SUPPORT_ENTITY_PARAMETER
                case ODataParameterReaderState.Entry:
                case ODataParameterReaderState.Feed:
#endif
                case ODataParameterReaderState.Collection:
                    this.OnParameterCompleted();
                    result = this.ReadNextParameterImplementation();
                    Debug.Assert(
                        this.State == ODataParameterReaderState.Value ||
#if SUPPORT_ENTITY_PARAMETER
                        this.State == ODataParameterReaderState.Entry ||
                        this.State == ODataParameterReaderState.Feed ||
#endif
                        this.State == ODataParameterReaderState.Collection ||
                        this.State == ODataParameterReaderState.Completed,
                        "ReadNextParameterImplementation should transition the state to ODataParameterReaderState.Value, ODataParameterReaderState.Entry, ODataParameterReaderState.Feed, ODataParameterReaderState.Collection or ODataParameterReaderState.Completed. The current state is: " + this.State);
                    break;

                case ODataParameterReaderState.Exception:    // fall through
                case ODataParameterReaderState.Completed:
                    Debug.Assert(false, "This case should have been caught earlier.");
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataParameterReaderCore_ReadImplementation));

                default:
                    Debug.Assert(false, "Unsupported parameter reader state " + this.State + " detected.");
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataParameterReaderCore_ReadImplementation));
            }

            return result;
        }

        /// <summary>
        /// Implementation of the parameter reader logic when in state 'Start'.
        /// </summary>
        /// <returns>true if more items can be read from the reader; otherwise false.</returns>
        protected abstract bool ReadAtStartImplementation();

        /// <summary>
        /// Implementation of the reader logic when in state Value, Entry, Feed or Collection state.
        /// </summary>
        /// <returns>true if more items can be read from the reader; otherwise false.</returns>
        protected abstract bool ReadNextParameterImplementation();

#if SUPPORT_ENTITY_PARAMETER
        /// <summary>
        /// Creates an <see cref="ODataReader"/> to read the entry value of type <paramref name="expectedEntityType"/>.
        /// </summary>
        /// <param name="expectedEntityType">Expected entity type to read.</param>
        /// <returns>An <see cref="ODataReader"/> to read the entry value of type <paramref name="expectedEntityType"/>.</returns>
        protected abstract ODataReader CreateEntryReader(IEdmEntityType expectedEntityType);

        /// <summary>
        /// Cretes an <see cref="ODataReader"/> to read the feed value of type <paramref name="expectedEntityType"/>.
        /// </summary>
        /// <param name="expectedEntityType">Expected feed element type to read.</param>
        /// <returns>An <see cref="ODataReader"/> to read the feed value of type <paramref name="expectedEntityType"/>.</returns>
        protected abstract ODataReader CreateFeedReader(IEdmEntityType expectedEntityType);
#endif

        /// <summary>
        /// Creates an <see cref="ODataCollectionReader"/> to read the collection with type <paramref name="expectedItemTypeReference"/>.
        /// </summary>
        /// <param name="expectedItemTypeReference">Expected item type reference of the collection to read.</param>
        /// <returns>An <see cref="ODataCollectionReader"/> to read the collection with type <paramref name="expectedItemTypeReference"/>.</returns>
        protected abstract ODataCollectionReader CreateCollectionReader(IEdmTypeReference expectedItemTypeReference);

        /// <summary>
        /// Reads the next <see cref="ODataItem"/> from the message payload.
        /// </summary>
        /// <returns>true if more items were read; otherwise false.</returns>
        protected bool ReadSynchronously()
        {
            return this.ReadImplementation();
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Asynchronously reads the next <see cref="ODataItem"/> from the message payload.
        /// </summary>
        /// <returns>A task that when completed indicates whether more items were read.</returns>
        [SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "API design calls for a bool being returned from the task here.")]
        protected virtual Task<bool> ReadAsynchronously()
        {
            // We are reading from the fully buffered read stream here; thus it is ok
            // to use synchronous reads and then return a completed task
            // NOTE: once we switch to fully async reading this will have to change
            return TaskUtils.GetTaskForSynchronousOperation<bool>(this.ReadImplementation);
        }
#endif

        /// <summary>
        /// Gets the corresponding create reader method name for the given state.
        /// </summary>
        /// <param name="state">State in question.</param>
        /// <returns>Returns the name of the method to create the correct reader for the given state.</returns>
        private static string GetCreateReaderMethodName(ODataParameterReaderState state)
        {
#if SUPPORT_ENTITY_PARAMETER
            Debug.Assert(state == ODataParameterReaderState.Entry || state == ODataParameterReaderState.Feed || state == ODataParameterReaderState.Collection, "state must be Entry, Feed or Collection.");
#else
            Debug.Assert(state == ODataParameterReaderState.Collection, "state must be Collection.");
#endif
            return "Create" + state.ToString() + "Reader";
        }

        /// <summary>
        /// Verifies that one of CreateEntryReader(), CreateFeedReader() or CreateCollectionReader() can be called.
        /// </summary>
        /// <param name="expectedState">The expected state of the reader.</param>
        private void VerifyCanCreateSubReader(ODataParameterReaderState expectedState)
        {
            this.inputContext.VerifyNotDisposed();
            if (this.State != expectedState)
            {
                throw new ODataException(Strings.ODataParameterReaderCore_InvalidCreateReaderMethodCalledForState(ODataParameterReaderCore.GetCreateReaderMethodName(expectedState), this.State));
            }

            if (this.subReaderState != SubReaderState.None)
            {
                Debug.Assert(this.Name != null, "this.Name != null");
                throw new ODataException(Strings.ODataParameterReaderCore_CreateReaderAlreadyCalled(ODataParameterReaderCore.GetCreateReaderMethodName(expectedState), this.Name));
            }
        }

        /// <summary>
        /// Catch any exception thrown by the action passed in; in the exception case move the reader into
        /// state ExceptionThrown and then rethrow the exception.
        /// </summary>
        /// <typeparam name="T">The type returned from the <paramref name="action"/> to execute.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <returns>The result of executing the <paramref name="action"/>.</returns>
        private T InterceptException<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                if (ExceptionUtils.IsCatchableExceptionType(e))
                {
                    this.EnterScope(ODataParameterReaderState.Exception, null, null);
                }

                throw;
            }
        }

        /// <summary>
        /// Verifies that calling Read is valid.
        /// </summary>
        /// <param name="synchronousCall">true if the call is to be synchronous; false otherwise.</param>
        private void VerifyCanRead(bool synchronousCall)
        {
            this.inputContext.VerifyNotDisposed();
            this.VerifyCallAllowed(synchronousCall);

            if (this.State == ODataParameterReaderState.Exception || this.State == ODataParameterReaderState.Completed)
            {
                throw new ODataException(Strings.ODataParameterReaderCore_ReadOrReadAsyncCalledInInvalidState(this.State));
            }

#if SUPPORT_ENTITY_PARAMETER
            if (this.State == ODataParameterReaderState.Entry || this.State == ODataParameterReaderState.Feed || this.State == ODataParameterReaderState.Collection)
#else
            if (this.State == ODataParameterReaderState.Collection)
#endif
            {
                if (this.subReaderState == SubReaderState.None)
                {
                    throw new ODataException(Strings.ODataParameterReaderCore_SubReaderMustBeCreatedAndReadToCompletionBeforeTheNextReadOrReadAsyncCall(this.State, ODataParameterReaderCore.GetCreateReaderMethodName(this.State)));
                }
                else if (this.subReaderState == SubReaderState.Active)
                {
                    throw new ODataException(Strings.ODataParameterReaderCore_SubReaderMustBeInCompletedStateBeforeTheNextReadOrReadAsyncCall(this.State, ODataParameterReaderCore.GetCreateReaderMethodName(this.State)));
                }
            }
        }

        /// <summary>
        /// Verifies that a call is allowed to the reader.
        /// </summary>
        /// <param name="synchronousCall">true if the call is to be synchronous; false otherwise.</param>
        private void VerifyCallAllowed(bool synchronousCall)
        {
            if (synchronousCall)
            {
                this.VerifySynchronousCallAllowed();
            }
            else
            {
#if ODATALIB_ASYNC
                this.VerifyAsynchronousCallAllowed();
#else
                Debug.Assert(false, "Async calls are not allowed in this build.");
#endif
            }
        }

        /// <summary>
        /// Verifies that a synchronous operation is allowed on this reader.
        /// </summary>
        private void VerifySynchronousCallAllowed()
        {
            if (!this.inputContext.Synchronous)
            {
                throw new ODataException(Strings.ODataParameterReaderCore_SyncCallOnAsyncReader);
            }
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Verifies that an asynchronous operation is allowed on this reader.
        /// </summary>
        private void VerifyAsynchronousCallAllowed()
        {
            if (this.inputContext.Synchronous)
            {
                throw new ODataException(Strings.ODataParameterReaderCore_AsyncCallOnSyncReader);
            }
        }
#endif

        /// <summary>
        /// A parameter reader scope; keeping track of the current reader state and an item associated with this state.
        /// </summary>
        protected sealed class Scope
        {
            /// <summary>The reader state of this scope.</summary>
            private readonly ODataParameterReaderState state;

            /// <summary>The parameter name attached to this scope.</summary>
            private readonly string name;

            /// <summary>The parameter value attached to this scope.</summary>
            private readonly object value;

            /// <summary>
            /// Constructor creating a new reader scope.
            /// </summary>
            /// <param name="state">The reader state of this scope.</param>
            /// <param name="name">The parameter name attached to this scope.</param>
            /// <param name="value">The parameter value attached to this scope.</param>
            public Scope(ODataParameterReaderState state, string name, object value)
            {
                Debug.Assert(
                   state == ODataParameterReaderState.Start && name == null && value == null ||
                   state == ODataParameterReaderState.Value && !string.IsNullOrEmpty(name) && (value == null || value is ODataComplexValue || EdmLibraryExtensions.IsPrimitiveType(value.GetType())) ||
#if SUPPORT_ENTITY_PARAMETER
                   state == ODataParameterReaderState.Entry && !string.IsNullOrEmpty(name) && value == null ||
                   state == ODataParameterReaderState.Feed && !string.IsNullOrEmpty(name) && value == null ||
#endif
                   state == ODataParameterReaderState.Collection && !string.IsNullOrEmpty(name) && value == null ||
                   state == ODataParameterReaderState.Exception && name == null && value == null ||
                   state == ODataParameterReaderState.Completed && name == null && value == null,
                   "Reader state and associated item do not match.");

                this.state = state;
                this.name = name;
                this.value = value;
            }

            /// <summary>
            /// The reader state of this scope.
            /// </summary>
            public ODataParameterReaderState State
            {
                get
                {
                    return this.state;
                }
            }

            /// <summary>
            /// The parameter name attached to this scope.
            /// </summary>
            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            /// <summary>
            /// The parameter value attached to this scope.
            /// </summary>
            public object Value
            {
                get
                {
                    return this.value;
                }
            }
        }
    }
}

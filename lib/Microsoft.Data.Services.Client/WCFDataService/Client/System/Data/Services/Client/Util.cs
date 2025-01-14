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

namespace System.Data.Services.Client
{
    #region Namespaces

    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Xml;

    #endregion Namespaces

    /// <summary>
    /// static utility function
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// String Suffix on outgoing version numbers
        /// </summary>
        internal const string VersionSuffix = ";NetFx";

        /// <summary>Tool name for the GeneratedCode attribute used by Astoria CodeGen</summary>
        internal const string CodeGeneratorToolName = "System.Data.Services.Design";

        /// <summary>Method name for the LoadProperty method.</summary>
        internal const string LoadPropertyMethodName = "LoadProperty";

        /// <summary>Method name for the Execute method.</summary>
        internal const string ExecuteMethodName = "Execute";

        /// <summary>Method name for the Async Execute method overload which expects void result.</summary>
        internal const string ExecuteMethodNameForVoidResults = "ExecuteVoid";

        /// <summary>Method name for the SaveChanges method.</summary>
        internal const string SaveChangesMethodName = "SaveChanges";

        /// <summary>
        /// Empty Data Service Version - represents a blank DSV header
        /// </summary>
        internal static readonly Version DataServiceVersionEmpty = new Version(0, 0);

        /// <summary>
        /// Data Service Version 1
        /// </summary>
        internal static readonly Version DataServiceVersion1 = new Version(1, 0);

        /// <summary>
        /// Data Service Version 2
        /// </summary>
        internal static readonly Version DataServiceVersion2 = new Version(2, 0);

        /// <summary>
        /// Data Service Version 3
        /// </summary>
        internal static readonly Version DataServiceVersion3 = new Version(3, 0);

        /// <summary>
        /// Data service versions supported on the client
        /// </summary>
        internal static readonly Version[] SupportedResponseVersions = 
        { 
            DataServiceVersion1,
            DataServiceVersion2,
            DataServiceVersion3
        };

        /// <summary>
        /// static char[] for indenting whitespace when tracing xml
        /// </summary>
        private static char[] whitespaceForTracing = new char[] { '\r', '\n', ' ', ' ', ' ', ' ', ' ' };

#if DEBUG
        /// <summary>
        /// DebugFaultInjector is a test hook to inject faults in specific locations. The string is the ID for the location
        /// </summary>
        private static Action<string> DebugFaultInjector = new Action<string>((s) => { });
#endif

        /// <summary>
        /// Converts the DataServiceProtocolVersion to a Version instance.
        /// </summary>
        /// <param name="maxProtocolVersion">The max protocol version value.</param>
        /// <returns>The same version expressed as Version instance.</returns>
        internal static Version GetVersionFromMaxProtocolVersion(DataServiceProtocolVersion maxProtocolVersion)
        {
            switch (maxProtocolVersion)
            {
                case DataServiceProtocolVersion.V1:
                    return Util.DataServiceVersion1;
                case DataServiceProtocolVersion.V2:
                    return Util.DataServiceVersion2;
                case DataServiceProtocolVersion.V3:
                    return Util.DataServiceVersion3;
                default:
                    Debug.Assert(false, "Unexpected max protocol version values.");
                    return Util.DataServiceVersion2;
            }
        }

        /// <summary>
        /// DebugInjectFault is a test hook to inject faults in specific locations. The string is the ID for the location
        /// </summary>
        /// <param name="state">The injector state parameter</param>
        [Conditional("DEBUG")]
        internal static void DebugInjectFault(string state)
        {
#if DEBUG
            DebugFaultInjector(state);
#endif
        }

        /// <summary>
        /// Checks the argument value for null and throw ArgumentNullException if it is null
        /// </summary>
        /// <typeparam name="T">type of the argument to prevent accidental boxing of value types</typeparam>
        /// <param name="value">argument whose value needs to be checked</param>
        /// <param name="parameterName">name of the argument</param>
        /// <exception cref="System.ArgumentNullException">if value is null</exception>
        /// <returns>value</returns>
        internal static T CheckArgumentNull<T>([ValidatedNotNull] T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw Error.ArgumentNull(parameterName);
            }

            return value;
        }

        /// <summary>
        /// Checks the string value is not empty
        /// </summary>
        /// <param name="value">value to check </param>
        /// <param name="parameterName">parameterName of public function</param>
        /// <exception cref="System.ArgumentNullException">if value is null</exception>
        /// <exception cref="System.ArgumentException">if value is empty</exception>
        internal static void CheckArgumentNullAndEmpty([ValidatedNotNull] string value, string parameterName)
        {
            CheckArgumentNull(value, parameterName);
            CheckArgumentNotEmpty(value, parameterName);
        }

        /// <summary>
        /// Checks the string value is not empty, but allows it to be null
        /// </summary>
        /// <param name="value">value to check</param>
        /// <param name="parameterName">parameterName of public function</param>
        /// <exception cref="System.ArgumentException">if value is empty</exception>
        internal static void CheckArgumentNotEmpty(string value, string parameterName)
        {
            if (value != null && 0 == value.Length)
            {
                throw Error.Argument(Strings.Util_EmptyString, parameterName);
            }
        }

        /// <summary>
        /// Checks the array value is not empty
        /// </summary>
        /// <typeparam name="T">type of the argument to prevent accidental boxing of value types</typeparam>
        /// <param name="value">value to check </param>
        /// <param name="parameterName">parameterName of public function</param>
        /// <exception cref="System.ArgumentNullException">if value is null</exception>
        /// <exception cref="System.ArgumentException">if value is empty or contains null elements</exception>
        internal static void CheckArgumentNotEmpty<T>(T[] value, string parameterName) where T : class
        {
            CheckArgumentNull(value, parameterName);
            if (0 == value.Length)
            {
                throw Error.Argument(Strings.Util_EmptyArray, parameterName);
            }

            for (int i = 0; i < value.Length; ++i)
            {
                if (Object.ReferenceEquals(value[i], null))
                {
                    throw Error.Argument(Strings.Util_NullArrayElement, parameterName);
                }
            }
        }

        /// <summary>
        /// Validate MergeOption
        /// </summary>
        /// <param name="value">option to validate</param>
        /// <param name="parameterName">name of the parameter being validated</param>
        /// <exception cref="System.ArgumentOutOfRangeException">if option is not valid</exception>
        /// <returns>option</returns>
        internal static MergeOption CheckEnumerationValue(MergeOption value, string parameterName)
        {
            switch (value)
            {
                case MergeOption.AppendOnly:
                case MergeOption.OverwriteChanges:
                case MergeOption.PreserveChanges:
                case MergeOption.NoTracking:
                    return value;
                default:
                    throw Error.ArgumentOutOfRange(parameterName);
            }
        }

        /// <summary>
        /// Validate MaxProtocolVersion
        /// </summary>
        /// <param name="value">version to validate</param>
        /// <param name="parameterName">name of the parameter being validated</param>
        /// <exception cref="System.ArgumentOutOfRangeException">if version is not valid</exception>
        /// <returns>version</returns>
        internal static DataServiceProtocolVersion CheckEnumerationValue(DataServiceProtocolVersion value, string parameterName)
        {
            switch (value)
            {
                case DataServiceProtocolVersion.V1:
                case DataServiceProtocolVersion.V2:
                case DataServiceProtocolVersion.V3:
                    return value;
                default:
                    throw Error.ArgumentOutOfRange(parameterName);
            }
        }

        /// <summary>
        /// Validate HttpStack
        /// </summary>
        /// <param name="value">option to validate</param>
        /// <param name="parameterName">name of the parameter being validated</param>
        /// <exception cref="System.ArgumentOutOfRangeException">if option is not valid</exception>
        /// <returns>option</returns>
        internal static HttpStack CheckEnumerationValue(HttpStack value, string parameterName)
        {
            switch (value)
            {
                case HttpStack.Auto:
#if ASTORIA_LIGHT
                case HttpStack.ClientHttp:
                case HttpStack.XmlHttp:
#endif
                    return value;
                default:
                    throw Error.ArgumentOutOfRange(parameterName);
            }
        }

        /// <summary>
        /// get char[] for indenting whitespace when tracing xml
        /// </summary>
        /// <param name="depth">how many characters to trace</param>
        /// <returns>char[]</returns>
        internal static char[] GetWhitespaceForTracing(int depth)
        {
            char[] whitespace = Util.whitespaceForTracing;
            while (whitespace.Length <= depth)
            {
                char[] tmp = new char[2 * whitespace.Length];
                tmp[0] = '\r';
                tmp[1] = '\n';
                for (int i = 2; i < tmp.Length; ++i)
                {
                    tmp[i] = ' ';
                }

                System.Threading.Interlocked.CompareExchange(ref Util.whitespaceForTracing, tmp, whitespace);
                whitespace = tmp;
            }

            return whitespace;
        }

        /// <summary>dispose of the object and set the reference to null</summary>
        /// <typeparam name="T">type that implements IDisposable</typeparam>
        /// <param name="disposable">object to dispose</param>
        internal static void Dispose<T>(ref T disposable) where T : class, IDisposable
        {
            Dispose(disposable);
            disposable = null;
        }

        /// <summary>dispose of the object</summary>
        /// <typeparam name="T">type that implements IDisposable</typeparam>
        /// <param name="disposable">object to dispose</param>
        internal static void Dispose<T>(T disposable) where T : class, IDisposable
        {
            if (null != disposable)
            {
                disposable.Dispose();
            }
        }
        
        /// <summary>
        /// Checks whether the exception type is one of the DataService*Exception
        /// </summary>
        /// <param name="ex">exception to test</param>
        /// <returns>true if the exception type is one of the DataService*Exception</returns>
        internal static bool IsKnownClientExcption(Exception ex)
        {
            return (ex is DataServiceClientException) || (ex is DataServiceQueryException) || (ex is DataServiceRequestException);
        }

        /// <summary>validate value is non-null</summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="value">value</param>
        /// <param name="errorcode">error code to throw if null</param>
        /// <returns>the non-null value</returns>
        internal static T NullCheck<T>(T value, InternalError errorcode) where T : class
        {
            if (Object.ReferenceEquals(value, null))
            {
                Error.ThrowInternalError(errorcode);
            }

            return value;
        }

        /// <summary>
        /// check the atom:null="true" attribute
        /// </summary>
        /// <param name="reader">XmlReader</param>
        /// <returns>true of null is true</returns>
        internal static bool DoesNullAttributeSayTrue(XmlReader reader)
        {
            string attributeValue = reader.GetAttribute(XmlConstants.AtomNullAttributeName, XmlConstants.DataWebMetadataNamespace);
            return ((null != attributeValue) && XmlConvert.ToBoolean(attributeValue));
        }

        /// <summary>Set the continuation for the following results for a collection.</summary>
        /// <param name="collection">The collection to set the links to</param>
        /// <param name="continuation">The continuation for the collection.</param>
        internal static void SetNextLinkForCollection(object collection, DataServiceQueryContinuation continuation)
        {
            Debug.Assert(collection != null, "collection != null");

            // We do a convention call for setting Continuation. We'll invoke this
            // for all properties named 'Continuation' that is a DataServiceQueryContinuation
            // (assigning to a single one would make it inconsistent if reflection
            // order is changed).
            foreach (var property in collection.GetType().GetPublicProperties(true /*instanceOnly*/))
            {
                if (property.Name != "Continuation" || !property.CanWrite)
                {
                    continue;
                }

                if (typeof(DataServiceQueryContinuation).IsAssignableFrom(property.PropertyType))
                {
                    property.SetValue(collection, continuation, null);
                }
            }
        }

        /// <summary>
        /// Determines if the current type is nullable or not
        /// </summary>
        /// <param name="t">The type parameter.</param>
        /// <returns>true if its nullable false otherwise</returns>
        internal static bool IsNullableType(Type t)
        {
            if (t.IsClass())
            {
                return true;
            }
            
            if (Nullable.GetUnderlyingType(t) != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Similar to Activator.CreateInstance, but uses LCG to avoid 
        /// more stringent Reflection security constraints.in Silverlight
        /// </summary>
        /// <param name="type">Type to create.</param>
        /// <param name="arguments">Arguments.</param>
        /// <returns>The newly instantiated object.</returns>
        internal static object ActivatorCreateInstance(Type type, params object[] arguments)
        {
            Debug.Assert(type != null, "type != null");
#if ASTORIA_LIGHT
            // Look up the constructor
            // We don't do overload resolution, make sure that the specific type has only one constructor with specified number of arguments
            int argumentCount = (arguments == null) ? 0 : arguments.Length;
            IEnumerable<ConstructorInfo> constructors = type.GetInstanceConstructors(true /*isPublic*/);
            ConstructorInfo constructor = null;
            foreach (ConstructorInfo ci in constructors)
            {
                if (ci.GetParameters().Length == argumentCount)
                {
                    Debug.Assert(constructor == null, "Make sure that the specific type has only one constructor with specified argument count");
                    constructor = ci;
#if !DEBUG
                    break;
#endif
                }
            }

            // A constructor should have been found, but we could run into a case with
            // complex types where there is no default constructor. A better error
            // message can be provided after localization freeze.
            if (constructor == null)
            {
                throw new MissingMethodException();
            }

            return ConstructorInvoke(constructor, arguments);
#else // ASTORIA_LIGHT
            // Different error messages occur for each call to activator.CreateInstance, when no parameters specified error message is more meaningful
            if (arguments.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return Activator.CreateInstance(type, arguments);
            }
#endif
        }

        /// <summary>
        /// Similar to ConstructorInfo.Invoke, but uses LCG to avoid 
        /// more stringent Reflection security constraints in Silverlight
        /// </summary>
        /// <param name="constructor">Constructor to invoke.</param>
        /// <param name="arguments">Arguments.</param>
        /// <returns>The newly instantiated object.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal static object ConstructorInvoke(ConstructorInfo constructor, object[] arguments)
        {
            if (constructor == null)
            {
#if PORTABLELIB
                throw new MissingMemberException();
#else
                throw new MissingMethodException();
#endif
            }
#if ASTORIA_LIGHT
            int argumentCount = (arguments == null) ? 0 : arguments.Length;
            ParameterExpression argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
            Expression[] argumentExpressions = new Expression[argumentCount];
            ParameterInfo[] parameters = constructor.GetParameters();
            for (int i = 0; i < argumentExpressions.Length; i++)
            {
                argumentExpressions[i] = Expression.Constant(arguments[i], parameters[i].ParameterType);
            }

            Expression newExpression = Expression.New(constructor, argumentExpressions);
            Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(newExpression, typeof(object)),
                argumentsExpression);
            object result = lambda.Compile()(arguments);
            return result;
#else
            return constructor.Invoke(arguments);
#endif
        }

        /// <summary>
        /// checks whether the given flag is set on the options
        /// </summary>
        /// <param name="options">options as specified by the user.</param>
        /// <param name="flag">whether the given flag is set on the options</param>
        /// <returns>true if the given flag is set, otherwise false.</returns>
        internal static bool IsFlagSet(SaveChangesOptions options, SaveChangesOptions flag)
        {
            return ((options & flag) == flag);
        }

        /// <summary>
        /// checks whether any batch flag is set on the options
        /// </summary>
        /// <param name="options">options as specified by the user.</param>
        /// <returns>true if the given flag is set, otherwise false.</returns>
        internal static bool IsBatch(SaveChangesOptions options)
        {
            return IsBatchWithSingleChangeset(options) || IsBatchWithIndependentOperations(options);
        }

        /// <summary>
        /// checks whether the batch flag is set on the options for the single changeset
        /// </summary>
        /// <param name="options">options as specified by the user.</param>
        /// <returns>true if the given flag is set, otherwise false.</returns>
        internal static bool IsBatchWithSingleChangeset(SaveChangesOptions options)
        {
            if (Util.IsFlagSet(options, SaveChangesOptions.Batch))
            {
                Debug.Assert(!Util.IsFlagSet(options, SaveChangesOptions.BatchWithIndependentOperations), "!Util.IsFlagSet(options, SaveChangesOptions.BatchWithIndependentOperations)");
                return true;
            }

            return false;
        }

        /// <summary>
        /// checks whether the batch flag with independent Operation per change set is set 
        /// </summary>
        /// <param name="options">options as specified by the user.</param>
        /// <returns>true if the given flag is set, otherwise false.</returns>
        internal static bool IsBatchWithIndependentOperations(SaveChangesOptions options)
        {
            if (Util.IsFlagSet(options, SaveChangesOptions.BatchWithIndependentOperations))
            {
                Debug.Assert(!Util.IsFlagSet(options, SaveChangesOptions.Batch), "!Util.IsFlagSet(options, SaveChangesOptions.Batch)");
                return true;
            }

            return false;
        }

        /// <summary>modified or unchanged</summary>
        /// <param name="x">state to test</param>
        /// <returns>true if modified or unchanged</returns>
        internal static bool IncludeLinkState(EntityStates x)
        {
            return ((EntityStates.Modified == x) || (EntityStates.Unchanged == x));
        }

        #region Tracing

        /// <summary>
        /// trace Element node
        /// </summary>
        /// <param name="reader">XmlReader</param>
        /// <param name="writer">TextWriter</param>
        [Conditional("TRACE")]
        internal static void TraceElement(XmlReader reader, System.IO.TextWriter writer)
        {
            Debug.Assert(XmlNodeType.Element == reader.NodeType, "not positioned on Element");

            if (null != writer)
            {
                writer.Write(Util.GetWhitespaceForTracing(2 + reader.Depth), 0, 2 + reader.Depth);
                writer.Write("<{0}", reader.Name);

                if (reader.MoveToFirstAttribute())
                {
                    do
                    {
                        writer.Write(" {0}=\"{1}\"", reader.Name, reader.Value);
                    }
                    while (reader.MoveToNextAttribute());

                    reader.MoveToElement();
                }

                writer.Write(reader.IsEmptyElement ? " />" : ">");
            }
        }

        /// <summary>
        /// trace EndElement node
        /// </summary>
        /// <param name="reader">XmlReader</param>
        /// <param name="writer">TextWriter</param>
        /// <param name="indent">indent or not</param>
        [Conditional("TRACE")]
        internal static void TraceEndElement(XmlReader reader, System.IO.TextWriter writer, bool indent)
        {
            if (null != writer)
            {
                if (indent)
                {
                    writer.Write(Util.GetWhitespaceForTracing(2 + reader.Depth), 0, 2 + reader.Depth);
                }

                writer.Write("</{0}>", reader.Name);
            }
        }

        /// <summary>
        /// trace string value
        /// </summary>
        /// <param name="writer">TextWriter</param>
        /// <param name="value">value</param>
        [Conditional("TRACE")]
        internal static void TraceText(System.IO.TextWriter writer, string value)
        {
            if (null != writer)
            {
                writer.Write(value);
            }
        }
        
        #endregion

        /// <summary>
        /// Converts the given IEnumerable into IEnumerable<typeparamref name="T"/> 
        /// </summary>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <param name="enumerable">IEnumerable which contains the list of the objects that needs to be converted.</param>
        /// <param name="valueConverter">Delegate to use to convert the value.</param>
        /// <returns>An instance of IEnumerable<typeparamref name="T"/> which contains the converted values.</returns>
        internal static IEnumerable<T> GetEnumerable<T>(IEnumerable enumerable, Func<object, T> valueConverter)
        {
            List<T> list = new List<T>();

            // Note that foreach will call Dispose on the used IEnumerator in a finally block
            foreach (object value in enumerable)
            {
                list.Add(valueConverter(value));
            }

            return list;
        }

        /// <summary>Given a <see cref="DataServiceProtocolVersion"/> enumeration returns the <see cref="Version"/> instance with the same version number.</summary>
        /// <param name="protocolVersion">The protocol version enum value to convert.</param>
        /// <returns>The version instance with the version number for the specified protocol version.</returns>
        internal static Version ToVersion(this DataServiceProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case DataServiceProtocolVersion.V1:
                    return DataServiceVersion1;
                case DataServiceProtocolVersion.V2:
                    return DataServiceVersion2;
                default:
                    Debug.Assert(protocolVersion == DataServiceProtocolVersion.V3, "Did you add a new version?");
                    return DataServiceVersion3;
            }
        }

        /// <summary>
        /// A workaround to a problem with FxCop which does not recognize the CheckArgumentNotNull method
        /// as the one which validates the argument is not null.
        /// </summary>
        /// <remarks>This has been suggested as a workaround in msdn forums by the VS team. Note that even though this is production code
        /// the attribute has no effect on anything else.</remarks>
        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}

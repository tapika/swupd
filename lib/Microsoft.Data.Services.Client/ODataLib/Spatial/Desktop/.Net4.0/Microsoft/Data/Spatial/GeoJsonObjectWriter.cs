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

namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Collections;

    /// <summary>
    /// Convert Spatial objects into json writer
    /// </summary>
    internal sealed class GeoJsonObjectWriter : GeoJsonWriterBase
    {
        /// <summary>
        /// Stack of json objects
        /// </summary>
        private readonly Stack<object> containers = new Stack<object>();

        /// <summary>
        /// Buffered key of the current name-value pair
        /// </summary>
        private string currentPropertyName;

        /// <summary>
        /// Stores the last object fully serialized
        /// </summary>
        private object lastCompletedObject;
        
        /// <summary>
        /// Get the top level json object
        /// </summary>
        internal IDictionary<String, Object> JsonObject
        {
            get
            {
                return lastCompletedObject as IDictionary<String, Object>;
            }
        }

        /// <summary>
        /// Test if the current container is an array
        /// </summary>
        private bool IsArray
        {
            get { return this.containers.Peek() is IList; }
        }

        /// <summary>
        /// Start a new json object scope
        /// </summary>
        protected override void StartObjectScope()
        {
            object jsonObject = new Dictionary<String, Object>(StringComparer.Ordinal);

            if (this.containers.Count > 0)
            {
                this.AddToScope(jsonObject);
            }

            // switch into the new container
            this.containers.Push(jsonObject);
        }
        
        /// <summary>
        /// Start a new json array scope
        /// </summary>
        protected override void StartArrayScope()
        {
            Debug.Assert(this.containers.Count > 0, "Array scope cannot be a top level GeoJson object");
            object jsonObject = new List<Object>();
            this.AddToScope(jsonObject);
            this.containers.Push(jsonObject);
        }

        /// <summary>
        /// Add a property name to the current json object
        /// </summary>
        /// <param name="name">The name to add</param>
        protected override void AddPropertyName(String name)
        {
            Debug.Assert(this.currentPropertyName == null, "A property name has already been set and not been used");
            this.currentPropertyName = name;
        }

        /// <summary>
        /// Add a value to the current json scope
        /// </summary>
        /// <param name="value">The value to add</param>
        protected override void AddValue(String value)
        {
            this.AddToScope(value);
        }

        /// <summary>
        /// Add a value to the current json scope
        /// </summary>
        /// <param name="value">The value to add</param>
        protected override void AddValue(double value)
        {
            this.AddToScope(value);
        }

        /// <summary>
        /// End the current json array scope
        /// </summary>
        protected override void EndArrayScope()
        {
            this.containers.Pop();
        }

        /// <summary>
        /// End the current json object scope
        /// </summary>
        protected override void EndObjectScope()
        {
            object o = this.containers.Pop();

            if (this.containers.Count == 0)
            {
                this.lastCompletedObject = o;
            }
        }

        /// <summary>
        /// Add an json object to the current scope
        /// </summary>
        /// <param name="jsonObject">The json object</param>
        private void AddToScope(object jsonObject)
        {
            Debug.Assert(this.containers.Count > 0, "No scope has been created");

            if (this.IsArray)
            {
                Debug.Assert(this.GetAndClearCurrentPropertyName() == null, "Array member must not have names");
                this.AsList().Add(jsonObject);
            }
            else
            {
                string name = this.GetAndClearCurrentPropertyName();
                Debug.Assert(name != null, "Dictionary members must have a name");
                this.AsDictionary().Add(name, jsonObject);
            }
        }

        /// <summary>
        /// Return the current property name, and clear the buffer
        /// </summary>
        /// <returns>The current property name</returns>
        /// <remarks>
        /// When inserting to a dictionary, the name-value pair comes across multiple pipeline calls
        /// Therefore we need to buffer the name part and wait for the value part.
        /// You can get into an incorrect state (caught by asserts) if you add a property name without 
        /// using it immediately next.
        /// </remarks>
        private String GetAndClearCurrentPropertyName()
        {
            String name = this.currentPropertyName;
            this.currentPropertyName = null;
            return name;
        }

        /// <summary>
        /// Access the current container as a List
        /// </summary>
        /// <returns>The current container as list</returns>
        private IList AsList()
        {
            var ret = this.containers.Peek() as IList;
            Debug.Assert(ret != null, "Calling AsList() on a scope that is not a list");
            return ret;
        }

        /// <summary>
        /// Access the current container as a Dictionary
        /// </summary>
        /// <returns>The current container as dictionary</returns>
        private IDictionary<String, Object> AsDictionary()
        {
            var ret = this.containers.Peek() as IDictionary<String, Object>;
            Debug.Assert(ret != null, "Calling AsDictionary() on a scope that is not a dictionary");
            return ret;
        }
    }
}

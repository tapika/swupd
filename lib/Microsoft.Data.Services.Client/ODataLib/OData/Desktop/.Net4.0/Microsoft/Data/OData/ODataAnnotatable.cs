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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    #endregion Namespaces

    /// <summary>
    /// Base class for all annotatable types in OData library.
    /// </summary>
#if !WINDOWS_PHONE && !SILVERLIGHT  && !PORTABLELIB
    [Serializable]
#endif
    public abstract class ODataAnnotatable
    {
        /// <summary>The map of annotationsAsArray keyed by type.</summary>
#if !WINDOWS_PHONE && !SILVERLIGHT  && !PORTABLELIB
        [NonSerialized]
#endif
        private object annotations;

        /// <summary>
        /// Collection of custom instance annotations.
        /// </summary>
#if !WINDOWS_PHONE && !SILVERLIGHT && !PORTABLELIB
        [NonSerialized]
#endif
        private ICollection<ODataInstanceAnnotation> instanceAnnotations = new Collection<ODataInstanceAnnotation>();

        /// <summary>Gets or sets the annotation by type.</summary>
        /// <returns>The annotation of type T or null if not present.</returns>
        /// <typeparam name="T">The type of the annotation.</typeparam>
        public T GetAnnotation<T>() where T : class
        {
            if (this.annotations != null)
            {
                object[] annotationsAsArray = this.annotations as object[];
                if (annotationsAsArray == null)
                {
                    return (this.annotations as T);
                }

                for (int i = 0; i < annotationsAsArray.Length; i++)
                {
                    object annotation = annotationsAsArray[i];
                    if (annotation == null)
                    {
                        break;
                    }

                    T typedAnnotation = annotation as T;
                    if (typedAnnotation != null)
                    {
                        return typedAnnotation;
                    }
                }
            }

            return null;
        }

        /// <summary>Sets an annotation of type T.</summary>
        /// <param name="annotation">The annotation to set.</param>
        /// <typeparam name="T">The type of the annotation.</typeparam>
        public void SetAnnotation<T>(T annotation) where T : class
        {
            this.VerifySetAnnotation(annotation);

            if (annotation == null)
            {
                RemoveAnnotation<T>();
            }
            else
            {
                this.AddOrReplaceAnnotation(annotation);
            }
        }

        /// <summary>
        /// Verifies that <paramref name="annotation"/> can be added as an annotation of this.
        /// </summary>
        /// <param name="annotation">Annotation instance.</param>
        internal virtual void VerifySetAnnotation(object annotation)
        {
            DebugUtils.CheckNoExternalCallers();
#pragma warning disable 618 // Disable "obsolete" warning for the InstanceAnnotationCollection. Used for backwards compatibilty.
            if (annotation is InstanceAnnotationCollection)
#pragma warning restore 618
            {
                throw new NotSupportedException(Strings.ODataAnnotatable_InstanceAnnotationsOnlyOnODataError);
            }
        }

        /// <summary>
        /// Get the annotation of type <typeparamref name="T"/>. If the annotation is not found, create a new
        /// instance of the annotation and call SetAnnotation on it then return the newly created instance.
        /// </summary>
        /// <typeparam name="T">The type of the annotation.</typeparam>
        /// <returns>The annotation of type <typeparamref name="T"/>.</returns>
        internal T GetOrCreateAnnotation<T>() where T : class, new()
        {
            DebugUtils.CheckNoExternalCallers();

            T annotation = this.GetAnnotation<T>();
            if (annotation == null)
            {
                annotation = new T();
                this.SetAnnotation(annotation);
            }

            return annotation;
        }

        /// <summary>
        /// Gets the custom instance annotations.
        /// </summary>
        /// <returns>The custom instance annotations.</returns>
        internal ICollection<ODataInstanceAnnotation> GetInstanceAnnotations()
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(this.instanceAnnotations != null, "this.instanceAnnotations != null");
            return this.instanceAnnotations;
        }

        /// <summary>
        /// Sets the custom instance annotations.
        /// </summary>
        /// <param name="value">The new value to set.</param>
        internal void SetInstanceAnnotations(ICollection<ODataInstanceAnnotation> value)
        {
            DebugUtils.CheckNoExternalCallers();
            ExceptionUtils.CheckArgumentNotNull(value, "value");
            this.instanceAnnotations = value;
        }

        /// <summary>
        /// Check whether a given (non-null) instance is of the specified type (no sub-type).
        /// </summary>
        /// <param name="instance">The (non-null) instance to test.</param>
        /// <param name="type">The type to check for.</param>
        /// <returns>True if the types match; otherwise false.</returns>
        private static bool IsOfType(object instance, Type type)
        {
            return instance.GetType() == type;
        }

        /// <summary>
        /// Replace an existing annotation of type T or add a new one 
        /// if no annotation of type T exists.
        /// </summary>
        /// <typeparam name="T">The type of the annotation.</typeparam>
        /// <param name="annotation">The annotation to set.</param>
        private void AddOrReplaceAnnotation<T>(T annotation) where T : class
        {
            Debug.Assert(annotation != null, "annotation != null");

            if (this.annotations == null)
            {
                this.annotations = annotation;
            }
            else
            {
                object[] annotationsAsArray = this.annotations as object[];
                if (annotationsAsArray == null)
                {
                    if (IsOfType(this.annotations, typeof(T)))
                    {
                        this.annotations = annotation;
                    }
                    else
                    {
                        this.annotations = new object[] { this.annotations, annotation };
                    }
                }
                else
                {
                    int index = 0;
                    for (; index < annotationsAsArray.Length; index++)
                    {
                        // NOTE: current is only null if we are past the last annotation
                        object current = annotationsAsArray[index];
                        if (current == null || IsOfType(current, typeof(T)))
                        {
                            annotationsAsArray[index] = annotation;
                            break;
                        }
                    }

                    if (index == annotationsAsArray.Length)
                    {
                        Array.Resize<object>(ref annotationsAsArray, index * 2);
                        this.annotations = annotationsAsArray;
                        annotationsAsArray[index] = annotation;
                    }
                }
            }
        }

        /// <summary>
        /// Remove the annotation of type T from the set of annotations (if such an annotation exists).
        /// We only allow a single occurence of an annotation of type T.
        /// </summary>
        /// <typeparam name="T">The type of the annotation to remove.</typeparam>
        private void RemoveAnnotation<T>() where T : class
        {
            if (this.annotations != null)
            {
                object[] annotationsAsArray = this.annotations as object[];
                if (annotationsAsArray == null)
                {
                    if (IsOfType(this.annotations, typeof(T)))
                    {
                        this.annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int foundAt = -1;
                    int length = annotationsAsArray.Length;
                    while (index < length)
                    {
                        object current = annotationsAsArray[index];
                        if (current == null)
                        {
                            break;
                        }
                        else if (IsOfType(current, typeof(T)))
                        {
                            foundAt = index;
                            break;
                        }

                        index++;
                    }

                    if (foundAt >= 0)
                    {
                        for (int i = foundAt; i < length - 1; ++i)
                        {
                            annotationsAsArray[i] = annotationsAsArray[i + 1];
                        }

                        annotationsAsArray[length - 1] = null;
                    }
                }
            }
        }
    }
}

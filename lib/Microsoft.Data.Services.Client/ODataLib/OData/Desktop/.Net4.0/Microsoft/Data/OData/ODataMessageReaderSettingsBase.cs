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
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Base Configuration settings for OData message readers.
    /// </summary>
    public abstract class ODataMessageReaderSettingsBase
    {
        /// <summary> Quotas to use for limiting resource consumption when reading an OData message. </summary>
        private ODataMessageQuotas messageQuotas;

        /// <summary> The check characters. </summary>
        private bool checkCharacters;

        /// <summary> The enable atom metadata reading. </summary>
        private bool enableAtomMetadataReading;

        /// <summary> The annotation filter. </summary>
        private Func<string, bool> shouldIncludeAnnotation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMessageReaderSettingsBase" /> class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:contains a call chain that results in a call to a virtual method defined by the class", Justification = "One derived type will only ever be created")]
        protected ODataMessageReaderSettingsBase()
        {
            // Dev Note: using private values with public properties because of violations 
            // assigning virtual properties in the constructors
            // On reading the default value for 'CheckCharacters' is set to false so that we 
            // can consume valid and invalid Xml documents per default.
            this.checkCharacters = false;

            // ATOM metadata reading is disabled by default for performance reasons and because 
            // few clients will need the ATOM metadata.
            this.enableAtomMetadataReading = false;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:contains a call chain that results in a call to a virtual method defined by the class", Justification = "One derived type will only ever be created")]
        protected ODataMessageReaderSettingsBase(ODataMessageReaderSettingsBase other)
        {
            ExceptionUtils.CheckArgumentNotNull(other, "other");

            this.checkCharacters = other.checkCharacters;
            this.enableAtomMetadataReading = other.enableAtomMetadataReading;
            this.messageQuotas = new ODataMessageQuotas(other.MessageQuotas);
            this.shouldIncludeAnnotation = other.shouldIncludeAnnotation;
        }

        /// <summary>
        /// Flag to control whether the reader should check for valid Xml characters or not.
        /// </summary>
        public virtual bool CheckCharacters
        {
            get { return this.checkCharacters; }
            set { this.checkCharacters = value; }
        }
        
        /// <summary>
        /// Flag to control whether ATOM metadata is read in ATOM payloads.
        /// </summary>
        public virtual bool EnableAtomMetadataReading
        {
            get { return this.enableAtomMetadataReading; }
            set { this.enableAtomMetadataReading = value; }
        }
        
        /// <summary>
        /// Quotas to use for limiting resource consumption when reading an OData message.
        /// </summary>
        public virtual ODataMessageQuotas MessageQuotas
        {
            get
            {
                if (this.messageQuotas == null)
                {
                    this.messageQuotas = new ODataMessageQuotas();
                }

                return this.messageQuotas;
            }

            set
            {
                this.messageQuotas = value;
            }
        }

        /// <summary>
        /// Func to evaluate whether an annotation should be read or skipped by the reader. The func should return true if the annotation should
        /// be read and false if the annotation should be skipped. A null value indicates that all annotations should be skipped.
        /// </summary>
        public virtual Func<string, bool> ShouldIncludeAnnotation
        {
            get { return this.shouldIncludeAnnotation; }
            set { this.shouldIncludeAnnotation = value; }
        }
    }
}

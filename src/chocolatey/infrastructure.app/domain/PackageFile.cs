// Copyright © 2017 - 2021 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.app.domain
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    ///   A package file in the snapshot.
    /// </summary>
    [Serializable]
    [XmlType("file")]
    public sealed class PackageFile
    {
        /// <summary>
        ///   Gets or sets the path of the file.
        /// </summary>
        /// <value>
        ///   The path.
        /// </value>
        [XmlAttribute(AttributeName = "path")]
        public string _path { get; set; }

        /// <summary>
        /// Store path in relative form against package location, but appear it as absolute to end-user.
        /// </summary>
        [XmlIgnore]
        public string Path
        {
            get {
                if (System.IO.Path.IsPathRooted(_path))
                {
                    return _path;
                }

                return System.IO.Path.Combine(InstallContext.Instance.PackagesLocation, _path);
            }

            set {
                // Force path to become relative.
                _path = value;

                var pkgLocation = InstallContext.Instance.PackagesLocation;
                if (_path.StartsWith(pkgLocation))
                {
                    _path = value.Substring(pkgLocation.Length + 1);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the checksum, currently only md5 because fast computation.
        /// </summary>
        /// <value>
        ///   The checksum.
        /// </value>
        [XmlAttribute(AttributeName = "checksum")]
        public string Checksum { get; set; }
    }
}

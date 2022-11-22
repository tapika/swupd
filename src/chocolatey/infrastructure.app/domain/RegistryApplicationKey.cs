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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Microsoft.Win32;
    using xml;

    [Serializable]
    [XmlType("key")]
    public class RegistryApplicationKey : IEquatable<RegistryApplicationKey>
    {
        [XmlIgnore]
        public RegistryHive Hive { get; set; }

        public RegistryView RegistryView { get; set; }

        public string KeyPath { get; set; }
        
        /// <summary>
        /// By default package id is the same as registry subkey name - normal installers
        /// (Wix, etc...) might be using GUID for it. GUID is of course cryptic and not so useful.
        /// 
        /// It's possible to override package id by using 'PackageId' registry key value.
        /// 
        /// How unique that packageId is difficult to say - it might depend on nuget package server
        /// and package maintainers.
        /// </summary>
        public string PackageId { get; set; }
        public bool IsPinned { get; set; }

        [XmlAttribute(AttributeName = "installerType")]
        public InstallerType InstallerType { get; set; }

        public string DefaultValue { get; set; }

        [XmlAttribute(AttributeName = "displayName")]
        public string DisplayName { get; set; }

        public XmlCData InstallLocation { get; set; }
        public XmlCData UninstallString { get; set; }
        public bool HasQuietUninstall { get; set; }

        // informational
        public XmlCData Publisher { get; set; }
        public string InstallDate { get; set; }
        public XmlCData InstallSource { get; set; }
        public string Language { get; set; } //uint

        // version stuff
        [XmlAttribute(AttributeName = "displayVersion")]
        public string DisplayVersion { get; set; }

        public string Version { get; set; } //uint
        public string VersionMajor { get; set; } //uint
        public string VersionMinor { get; set; } //uint
        public string VersionRevision { get; set; } //uint
        public string VersionBuild { get; set; } //uint

        // install information
        public bool SystemComponent { get; set; }
        public bool WindowsInstaller { get; set; }
        public bool NoRemove { get; set; }
        public bool NoModify { get; set; }
        public bool NoRepair { get; set; }
        public string ReleaseType { get; set; } //hotfix, securityupdate, update rollup, servicepack
        public string ParentKeyName { get; set; }
        public XmlCData LocalPackage { get; set; }
        [XmlIgnore]
        public string DisplayIcon { get; set; }
        [XmlIgnore]
        public long EstimatedSize { get; internal set; }

        /// <summary>
        /// Currently used only to tag specific installation for testing.
        /// </summary>
        [XmlIgnore]
        public string Tags { get; set; }

        /// <summary>
        ///   Is an application listed in ARP (Programs and Features)?
        /// </summary>
        /// <returns>true if the key should be listed as a program</returns>
        /// <remarks>
        ///   http://community.spiceworks.com/how_to/show/2238-how-add-remove-programs-works
        /// </remarks>
        public bool is_in_programs_and_features()
        {
            return !string.IsNullOrWhiteSpace(DisplayName)
                   && !string.IsNullOrWhiteSpace(UninstallString.to_string())
                   && InstallerType != InstallerType.HotfixOrSecurityUpdate
                   && InstallerType != InstallerType.ServicePack
                   && string.IsNullOrWhiteSpace(ParentKeyName)
                   && !NoRemove
                   && !SystemComponent
                ;
        }

        public string ToStringFull(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            List<string> ignoreProps = new[] {
                nameof(InstallDate),            // changing everytime.
                nameof(RegistryView),           // not sure if test machine is 32 or 64 bit.
                nameof(InstallerType),          // don't care ...
                nameof(SystemComponent),
                nameof(WindowsInstaller),
            }.ToList();

            foreach (var prop in GetType().GetProperties().OrderBy(x => x.MetadataToken))
            {
                object obj = prop.GetValue(this);
                if (obj == null)
                {
                    continue;
                }

                if (ignoreProps.Contains(prop.Name))
                { 
                    continue;
                }

                string value = InstallContext.NormalizeMessage(obj.ToString());
                if (!string.IsNullOrEmpty(value))
                { 
                    sb.AppendLine($"{prefix}{prop.Name}: {value}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets property names which are saved in registry
        /// </summary>
        /// <param name="haveIcon">true if have icon</param>
        public static List<String> GetPropertyNames(bool haveIcon)
        {
            List<String> propNames = new List<string>() {
                nameof(RegistryApplicationKey.DisplayName),
                nameof(RegistryApplicationKey.Version),
                nameof(RegistryApplicationKey.InstallLocation),
                nameof(RegistryApplicationKey.PackageId),
                nameof(RegistryApplicationKey.UninstallString),
                nameof(RegistryApplicationKey.Publisher),
                nameof(RegistryApplicationKey.NoModify),
                nameof(RegistryApplicationKey.NoRepair),
                nameof(RegistryApplicationKey.InstallDate),
                nameof(RegistryApplicationKey.DisplayVersion),
                nameof(RegistryApplicationKey.EstimatedSize),
                nameof(RegistryApplicationKey.Tags)
            };

            if (haveIcon)
            { 
                propNames.Add(nameof(RegistryApplicationKey.DisplayIcon));
            }

            return propNames;
        }

        public override string ToString()
        {
            return "{0}|{1}|{2}|{3}|{4}".format_with(
                DisplayName,
                DisplayVersion,
                InstallerType,
                UninstallString,
                KeyPath
            );
        }

        public override int GetHashCode()
        {
            return DisplayName.GetHashCode()
                   & DisplayVersion.GetHashCode()
                   & UninstallString.to_string().GetHashCode()
                   & KeyPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            return Equals(obj as RegistryApplicationKey);
        }

        bool IEquatable<RegistryApplicationKey>.Equals(RegistryApplicationKey other)
        {
            if (ReferenceEquals(other, null)) return false;

            return DisplayName.to_string().is_equal_to(other.DisplayName)
                   && DisplayVersion.is_equal_to(other.DisplayVersion)
                   && UninstallString.to_string().is_equal_to(other.UninstallString.to_string())
                   && KeyPath.is_equal_to(other.KeyPath)
                ;
        }
    }
}

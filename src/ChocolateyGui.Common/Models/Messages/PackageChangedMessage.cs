// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Chocolatey" file="PackageChangedMessage.cs">
//   Copyright 2017 - Present Chocolatey Software, LLC
//   Copyright 2014 - 2017 Rob Reynolds, the maintainers of Chocolatey, and RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using ChocolateyGui.Common.ViewModels.Items;
using NuGet;

namespace ChocolateyGui.Common.Models.Messages
{
    public enum PackageChangeType
    {
        Updated,
        Uninstalled,
        Installed,
        Pinned,
        Unpinned
    }

    public class PackageChangedMessage
    {
        public PackageChangedMessage(IPackageViewModel package, PackageChangeType changeType, SemanticVersion version = null)
        {
            Package = package;
            ChangeType = changeType;
            Version = version;
        }

        public IPackageViewModel Package { get; }

        public string Id
        { 
            get { return Package.Id; } 
        }

        public SemanticVersion Version { get; }

        public PackageChangeType ChangeType { get; }
    }
}
using System;
using System.Runtime.Versioning;

namespace NuGet
{
    public enum WalkerType
    { 
        install,
        update,
        uninstall
    }

    /// <summary>
    /// Parameters for specific walker initialization.
    /// </summary>
    public class WalkerInfo
    {
        public WalkerType type;

        // for install
        public bool ignoreDependencies;
        public bool ignoreWalkInfo = false;
        public FrameworkName targetFramework = null;

        // for update
        public bool updateDependencies;

        // for install & update
        public bool allowPrereleaseVersions;

        // for uninstall
        public bool forceRemove;
        public bool removeDependencies;
    }

    public interface IPackageManager
    {
        /// <summary>
        /// File system used to perform local operations in.
        /// </summary>
        IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Local repository to install and reference packages.
        /// </summary>
        IPackageRepository LocalRepository { get; }

        ILogger Logger { get; set; }

        DependencyVersion DependencyVersion { get; set; }

        bool WhatIf { get; set; }
        
        /// <summary>
        /// Remote repository to install packages from.
        /// </summary>
        IPackageRepository SourceRepository { get; }

        /// <summary>
        /// PathResolver used to determine paths for installed packages.
        /// </summary>
        IPackagePathResolver PathResolver { get; }

        event EventHandler<PackageOperationEventArgs> PackageInstalled;
        event EventHandler<PackageOperationEventArgs> PackageInstalling;
        event EventHandler<PackageOperationEventArgs> PackageUninstalled;
        event EventHandler<PackageOperationEventArgs> PackageUninstalling;

        void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions);
        void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions, bool ignoreWalkInfo);
        void InstallPackage(string packageId, SemanticVersion version, bool ignoreDependencies, bool allowPrereleaseVersions);
        void UpdatePackage(IPackage newPackage, bool updateDependencies, bool allowPrereleaseVersions);
        void UpdatePackage(string packageId, SemanticVersion version, bool updateDependencies, bool allowPrereleaseVersions);
        void UpdatePackage(string packageId, IVersionSpec versionSpec, bool updateDependencies, bool allowPrereleaseVersions);
        void UninstallPackage(IPackage package, bool forceRemove, bool removeDependencies);
        void UninstallPackage(string packageId, SemanticVersion version, bool forceRemove, bool removeDependencies);

        /// <summary>
        /// Creates package dependency walker
        /// </summary>
        /// <param name="walkerInfo">walker parameters</param>
        /// <returns>walker</returns>
        IPackageOperationResolver GetWalker( WalkerInfo walkerInfo );
    }
}

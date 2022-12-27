using chocolatey.infrastructure.app.services;
using NuGet;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace chocolatey.infrastructure.app.nuget
{
    /// <summary>
    /// Bit extended version of Nuget.PackageManager - will support also in registry queries.
    /// </summary>
    public class PackageManagerEx: PackageManager
    {
        private readonly IRegistryService _registryService;

        public PackageManagerEx(
            IRegistryService registryService,
            IPackageRepository sourceRepository, IPackagePathResolver pathResolver, 
            IFileSystem fileSystem, IPackageRepository localRepository) :
                base(sourceRepository, pathResolver, fileSystem, localRepository)
        {
            _registryService = registryService;
        }

        /// <summary>
        /// Finds locally installed package - either in local repostory or via registry
        /// </summary>
        public IPackage FindAnyLocalPackage(string packageName)
        {
            IPackage package = LocalRepository.FindPackage(packageName);
            if (package != null)
            {
                return package;
            }

            // At the moment bit slow, maybe needs to be precached.
            var registry = _registryService.get_installer_keys(packageName, null, true);
            if (registry.RegistryKeys.Count != 0)
            {
                return new RegistryPackage(registry.RegistryKeys.First());
            }

            return null;
        }


        public IEnumerable<IPackage> FindLocalPackages(string packageId, SemanticVersion semanticVersion = null)
        {
            IEnumerable<IPackage> locals;
            if (semanticVersion != null)
            {
                locals = LocalRepository.FindPackagesById(packageId).Where((p) => p.Version.Equals(semanticVersion));
            }
            else
            { 
                locals = LocalRepository.FindPackagesById(packageId);
            }

            var registry = _registryService.get_installer_keys(packageId, null, true);
            return locals.Concat(registry.RegistryKeys.Select(x => new RegistryPackage(x)));
        }
    }
}

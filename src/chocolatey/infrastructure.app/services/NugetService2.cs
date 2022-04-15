using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.nuget;
using chocolatey.infrastructure.logging;
using NuGet;
using System;

namespace chocolatey.infrastructure.app.services
{
    public partial class NugetService : INugetService
    {
        public PackageManagerEx GetPackageManager(
            ChocolateyConfiguration configuration, 
            Action<PackageOperationEventArgs> installSuccessAction = null, 
            Action<PackageOperationEventArgs> uninstallSuccessAction = null,
            bool addUninstallHandler = false
        )
        {
            IFileSystem nugetPackagesFileSystem = NugetCommon.GetNuGetFileSystem(configuration, _nugetLogger);
            IPackagePathResolver pathResolver = NugetCommon.GetPathResolver(configuration, nugetPackagesFileSystem);
            var packageManager = new PackageManagerEx(_registryService, NugetCommon.GetRemoteRepository(configuration, _nugetLogger, _packageDownloader), pathResolver, nugetPackagesFileSystem,
                NugetCommon.GetLocalRepository(pathResolver, nugetPackagesFileSystem, _nugetLogger))
            {
                DependencyVersion = DependencyVersion.Highest,
                Logger = _nugetLogger,
            };

            // GH-1548
            //note: is this a good time to capture a backup (for dependencies) / maybe grab remembered arguments here instead / and somehow get out of the endless loop! 
            //NOTE DO NOT EVER use this method - packageManager.PackageInstalling += (s, e) => { };

            packageManager.PackageInstalled += (s, e) =>
            {
                var pkg = e.Package;
                "chocolatey".Log().Info(ChocolateyLoggers.Important, "{0}{1} v{2}{3}{4}{5}".format_with(
                    System.Environment.NewLine,
                    pkg.Id,
                    pkg.Version.to_string(),
                    configuration.Force ? " (forced)" : string.Empty,
                    pkg.IsApproved ? " [Approved]" : string.Empty,
                    pkg.PackageTestResultStatus == "Failing" && pkg.IsDownloadCacheAvailable ? " - Likely broken for FOSS users (due to download location changes)" : pkg.PackageTestResultStatus == "Failing" ? " - Possibly broken" : string.Empty
                    ));

                if (installSuccessAction != null) installSuccessAction.Invoke(e);
            };

            if (addUninstallHandler)
            {
                // NOTE DO NOT EVER use this method, or endless loop - packageManager.PackageUninstalling += (s, e) =>

                packageManager.PackageUninstalled += (s, e) =>
                {
                    IPackage pkg = packageManager.LocalRepository.FindPackage(e.Package.Id, e.Package.Version);
                    if (pkg != null)
                    {
                        // install not actually removed, let's clean it up. This is a bug with nuget, where it reports it removed some package and did NOTHING
                        // this is what happens when you are switching from AllowMultiple to just one and back
                        var chocoPathResolver = packageManager.PathResolver as ChocolateyPackagePathResolver;
                        if (chocoPathResolver != null)
                        {
                            chocoPathResolver.UseSideBySidePaths = !chocoPathResolver.UseSideBySidePaths;

                            // an unfound package folder can cause an endless loop.
                            // look for it and ignore it if doesn't line up with versioning
                            if (nugetPackagesFileSystem.DirectoryExists(chocoPathResolver.GetInstallPath(pkg)))
                            {
                                //todo: This causes an issue with upgrades.
                                // this causes this to be called again, which should then call the uninstallSuccessAction below
                                packageManager.UninstallPackage(pkg, forceRemove: configuration.Force, removeDependencies: false);
                            }

                            chocoPathResolver.UseSideBySidePaths = configuration.AllowMultipleVersions;
                        }
                    }
                    else
                    {
                        if (uninstallSuccessAction != null) uninstallSuccessAction.Invoke(e);
                    }
                };
            }

            return packageManager;
        }
    }
}

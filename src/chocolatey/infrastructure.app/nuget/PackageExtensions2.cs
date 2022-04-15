using chocolatey.infrastructure.app.domain;
using NuGet;
using System.IO;

namespace chocolatey.infrastructure.app.nuget
{
    static public class PackageExtensions2
    {
        /// <summary>
        /// Gets package installed location
        /// </summary>
        public static string GetPackageLocation(this IPackage package, ChocolateyPackageInformation pkgInfo = null)
        {
            if (package is RegistryPackage regp && regp.RegistryKey != null)
            {
                return regp.RegistryKey.InstallLocation;
            }

            var isSideBySide = pkgInfo != null && pkgInfo.IsSideBySide;
            var installDir = Path.Combine(InstallContext.Instance.PackagesLocation,  
                "{0}{1}".format_with(package.Id, isSideBySide ? "." + package.Version.to_string() : string.Empty));

            return installDir;
        }
    }
}

using NuGet;
using System.IO;

namespace chocolatey.infrastructure.app.nuget
{
    static public class PackageExtensions2
    {
        /// <summary>
        /// Gets package installed location
        /// </summary>
        public static string GetPackageLocation(this IPackage package)
        {
            if (package is RegistryPackage regp)
            {
                return regp.RegistryKey.InstallLocation;
            }

            return Path.Combine(InstallContext.Instance.PackagesLocation, package.Id);
        }
    }
}

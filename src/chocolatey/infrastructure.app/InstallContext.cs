using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace chocolatey.infrastructure.app
{
    /// <summary>
    /// Represents application configuration in which application operates - all pathes / configuration.
    /// 
    /// For unit testing each unit test will have it's own InstallContext
    /// </summary>
    public class InstallContext
    {
        static public ThreadLocal<InstallContext> _instance = 
            new ThreadLocal<InstallContext>( () => { return new InstallContext(); });

        static public InstallContext Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public static string ApplicationInstallLocation
        {
            get {
                // See also: https://github.com/dotnet/runtime/issues/13051
                return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
        }

        string rootLocation;

        /// <summary>
        /// Root folder under which chocolatey directories resides, like
        /// 
        ///     .chocolatey\
        ///     lib\
        ///     packages\
        ///     logs\
        /// </summary>
        public string RootLocation
        {
            get {
                if (rootLocation != null)
                {
                    return rootLocation;
                }

                return ApplicationInstallLocation;
            }
            
            set
            {
                rootLocation = value;
            }
        }

        string packagesLocation;
        public string PackagesLocation
        {
            get {
                if (packagesLocation != null)
                {
                    return packagesLocation;
                }

                // Default value until overridden
                return Path.Combine(RootLocation, "lib");
            }

            set {
                packagesLocation = value;
            }
        }
        
        public string LoggingLocation                       { get { return Path.Combine(RootLocation, "logs"); } }
        public string PackageFailuresLocation               { get { return Path.Combine(RootLocation, "lib-bad"); } }
        public string PackageBackupLocation                 { get { return Path.Combine(RootLocation, "lib-bkp"); } }
        public string ShimsLocation                         { get { return Path.Combine(RootLocation, "bin"); } }
        public string ChocolateyPackageInfoStoreLocation    { get { return Path.Combine(RootLocation, ".chocolatey"); } }
        public string ExtensionsLocation                    { get { return Path.Combine(RootLocation, "extensions"); } }
        public string TemplatesLocation                     { get { return Path.Combine(RootLocation, "templates"); } }
        public string ConfigLocation                        { get { return Path.Combine(RootLocation, "config"); } }
        public string GlobalConfigFileLocation              { get { return Path.Combine(ConfigLocation, "chocolatey.config"); } }
     
        // Unlike rest of directories - tools will always resides against application location
        public string ToolsLocation                         { get { return Path.Combine(ApplicationInstallLocation, "tools"); } }
    }
}


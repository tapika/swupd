﻿using chocolatey.infrastructure.app.configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
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
        static public AsyncLocal<InstallContext> _instance = new AsyncLocal<InstallContext>();

        static public InstallContext Instance
        {
            get
            {
                if (_instance.Value == null)
                {
                    _instance.Value = new InstallContext();
                }

                return _instance.Value;
            }

            set
            {
                _instance.Value = null;
            }
        }

        public static string ApplicationInstallLocation
        {
            get {
                return Instance.InstallLocation;
            }
        }

        string getExecutableDirectory()
        {
            // Visual studio / .net 4.8 / testhost*.exe
            if (System.Diagnostics.Process.GetCurrentProcess().ProcessName.StartsWith("testhost"))
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }

            // See also: https://github.com/dotnet/runtime/issues/13051
            return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        string installLocation;
        /// <summary>
        /// Where choco is installed.
        /// </summary>
        public string InstallLocation
        {
            get
            {
                if (installLocation == null)
                {
                    installLocation = getExecutableDirectory();
                }

                return installLocation;
            }

            set
            {
                installLocation = value;
            }
        }

        /// <summary>
        /// choco's .ps1 helper functions
        /// </summary>
        public static string HelpersLocation
        {
            get
            { 
                return Path.Combine(Instance.installLocation, "helpers");
            }
        }

        string rootLocation;

        /// <summary>
        /// Root folder under which chocolatey directories resides, like
        /// 
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
                // Setting RootLocation will also reset package location to default one.
                packagesLocation = null;
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

        public const string LoggingDirectory = "logs";
        public string LoggingLocation                       { get { return Path.Combine(RootLocation, LoggingDirectory); } }

        public string DisplayLoggingLocation
        {
            get {
                if (ApplicationParameters.runningUnitTesting)
                    return LoggingDirectory;

                return LoggingLocation;
            }
        }

        public string PackageFailuresLocation               { get { return Path.Combine(RootLocation, "lib-bad"); } }
        public const string BackupFolderName = "lib-bkp";
        public string PackageBackupLocation                 { get { return Path.Combine(RootLocation, BackupFolderName); } }
        public string ShimsLocation                         { get { return Path.Combine(RootLocation, "bin"); } }
        public string ExtensionsLocation                    { get { return Path.Combine(RootLocation, "extensions"); } }
        public string TemplatesLocation                     { get { return Path.Combine(RootLocation, "templates"); } }
        public string ConfigLocation                        { get { return Path.Combine(RootLocation, "config"); } }
        public string GlobalConfigFileLocation              { get { return Path.Combine(ConfigLocation, "chocolatey.config"); } }
     
        // Unlike rest of directories - tools will always resides against application location
        public string ToolsLocation                         { get { return Path.Combine(ApplicationInstallLocation, "tools"); } }

        // Used by test applications only
        public static string TestPackagesFolder             { get { return Path.Combine(ApplicationInstallLocation, "context"); } }
        public static string SharedPackageFolder            { get { return Path.Combine(ApplicationInstallLocation, "tests_shared"); } }
        public static string IsolatedTestFolder             { get { return Path.Combine(ApplicationInstallLocation, "tests"); } }


        /// <summary>
        /// Normalizes message by removing any references to absolute root path.
        /// </summary>
        /// <returns>message without absolute path references</returns>
        public static string NormalizeMessage(string message)
        {
            if (ApplicationParameters.runningUnitTesting)
            {
                return message.Replace(InstallContext.Instance.RootLocation + System.IO.Path.DirectorySeparatorChar, "");
            }

            return message;
        }

        string _cacheLocation;

        /// <summary>
        /// Determines where choco keeps it's temporary files. If you change this during run-time - you will need also to clean up
        /// old temporary folder.
        /// </summary>
        public string CacheLocation
        {
            get {
                if (_cacheLocation == null)
                    builders.ConfigurationBuilder.ReconfigureCacheLocation(new ChocolateyConfiguration());

                return _cacheLocation;
            }

            set {
                _cacheLocation = value;
            }
        }

        /// <summary>
        /// Generates tempotary path for specific tool. Directory is not physically created, but 
        /// just logical path is given. End-user is responsible for deleting given directory name.
        /// </summary>
        /// <param name="tempName">tool name for temp folder</param>
        /// <returns>thread local temporary directory</returns>
        public string GetThreadTempFolderName(string tempName)
        {
            return Path.Combine(CacheLocation,
                // Currently we don't need process specific temporary folder, but leave it here just in case if later on we
                // won't redirect TEMP folder.
                $"{tempName}_{Process.GetCurrentProcess().Id}_{Thread.CurrentThread.ManagedThreadId}");
        }

        string _powerShellTempLocation;

        public string PowerShellTempLocation
        {
            get
            {
                if (_powerShellTempLocation == null)
                {
                    _powerShellTempLocation = GetThreadTempFolderName("PwSh");
                }

                return _powerShellTempLocation;
            }

            set
            {
                _powerShellTempLocation = value;
            }
        }

    }
}


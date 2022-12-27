using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace chocolatey.tests2
{
    /// <summary>
    /// Helper class for registry testing. Only one test will be allowed to start registry based testing.
    /// </summary>
    public class TestRegistry: IDisposable
    {
        static AutoResetEvent locker = new AutoResetEvent(true);
        RegistryService registryService = new RegistryService(null, null);

        public TestRegistry(bool initialLock = true)
        {
            if (initialLock)
            { 
                Lock();
            }
        }

        public void Lock()
        {
            locker.WaitOne();
        }

        public void Unlock()
        {
            locker.Set();
        }

        public void DeleteInstallEntries(params string[] packageIds)
        {
            foreach (string packageId in packageIds)
            {
                var keyInfo = new RegistryApplicationKey()
                {
                    Hive = Microsoft.Win32.RegistryHive.LocalMachine,
                    KeyPath = $"HKEY_LOCAL_MACHINE\\{RegistryService.UNINSTALLER_KEY_NAME}\\{packageId}",
                    RegistryView = Microsoft.Win32.RegistryView.Default
                };

                registryService.delete_key(keyInfo);
            }
        }

        public void AddInstallEntry(RegistryApplicationKey appKey)
        {
            Type type = appKey.GetType();
            appKey.Hive = Microsoft.Win32.RegistryHive.LocalMachine;
            appKey.RegistryView = Microsoft.Win32.RegistryView.Default;
            appKey.KeyPath = $"HKEY_LOCAL_MACHINE\\{RegistryService.UNINSTALLER_KEY_NAME}\\{appKey.PackageId}";
            appKey.Publisher = "TestRegistry";

            // So control panel would see it:
            appKey.UninstallString = "none";  
            appKey.DisplayName = appKey.PackageId;
            appKey.DisplayVersion = appKey.Version.ToString();

            List<string> propNames = RegistryApplicationKey.GetPropertyNames(true);
            foreach (string propName in propNames.ToArray())
            {
                var prop = type.GetProperty(propName);
                if (prop.GetValue(appKey) == null)
                    propNames.Remove(propName);
            }

            registryService.set_key_values(appKey, propNames.ToArray());
        }

        public void AddInstallPackage2Entry(  
            string packageId = "installpackage2", 
            string installdirectory = "custominstalldir", 
            string version = "1.0.0" )
        {
            AddInstallEntry(
                new RegistryApplicationKey()
                {
                    PackageId = packageId,
                    Version = version,
                    InstallLocation = Path.Combine(InstallContext.Instance.RootLocation, installdirectory, packageId),
                    Tags = "test"
                }
            );
        }

        public void LogInstallEntries(bool afterOp, params string[] packageIds)
        {
            var console = LogService.console;
            string prefix = (afterOp) ? "after operation": "before operation";
            foreach (string packageId in packageIds)
            {
                var registry = registryService.get_installer_keys(packageId);
                if (registry.RegistryKeys.Count == 0)
                {
                    console.Info("");
                    console.Info($"- {prefix} {packageId} - not installed");
                }
                else
                { 
                    console.Info("");
                    console.Info($"- {prefix} {packageId} registry:");
                    foreach (var registryKey in registry.RegistryKeys)
                    {
                        console.Info(registryKey.ToStringFull("  "));
                    }
                }
            }        
        }

        public void Dispose()
        {
            Unlock();
        }
    }
}

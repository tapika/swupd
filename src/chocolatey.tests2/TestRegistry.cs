using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.logging;
using System;
using System.Threading;

namespace chocolatey.tests2
{
    /// <summary>
    /// Helper class for registry testing. Only one test will be allowed to start registry based testing.
    /// </summary>
    public class TestRegistry: IDisposable
    {
        static object locker = new object();
        RegistryService registryService = new RegistryService(null, null);

        public TestRegistry()
        {
            Monitor.Enter(locker);
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

        public void LogInstallEntries(params string[] packageIds)
        {
            var console = LogService.console;
            foreach (string packageId in packageIds)
            {
                var registry = registryService.get_installer_keys(packageId);
                if (registry.RegistryKeys.Count == 0)
                {
                    console.Info("");
                    console.Info($"- {packageId} - not installed");
                }
                else
                { 
                    console.Info("");
                    console.Info($"- {packageId} registry:");
                    foreach (var registryKey in registry.RegistryKeys)
                    {
                        console.Info(registryKey.ToStringFull("  "));
                    }
                }
            }        
        }

        public void Dispose()
        {
            Monitor.Exit(locker);
        }
    }
}

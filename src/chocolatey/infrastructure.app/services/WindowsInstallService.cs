using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.results;
using SimpleInjector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace chocolatey.infrastructure.app.services
{
    /// <summary>
    /// Manipulates windows registry to install / uninstall application
    /// </summary>
    public sealed class WindowsInstallService : ISourceRunner
    {
        IRegistryService _registryService;
        Container _container;

        public WindowsInstallService(Container container, IRegistryService registryService /*, IChocolateyPackageService packageService*/)
        {
            _registryService = registryService;
            _container = container;
        }

        public SourceType SourceType => SourceType.windowsinstall;

        public int count_run(ChocolateyConfiguration config)
        {
            return get_apps().Count();
        }

        public void ensure_source_app_installed(ChocolateyConfiguration config, Action<PackageResult> ensureAction)
        {
            // We run, but we are not necessarily installed. Let choco / chocogui deal with it's installation.
        }

        public void install_noop(ChocolateyConfiguration config, Action<PackageResult> continueAction)
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, PackageResult> install_run(ChocolateyConfiguration config, Action<PackageResult> continueAction)
        {
            throw new NotImplementedException();
        }

        public void list_noop(ChocolateyConfiguration config)
        {
            throw new NotImplementedException();
        }

        IEnumerable<RegistryApplicationKey> get_apps()
        {
            return _registryService.get_installer_keys().RegistryKeys.
                    Where((p) => p.is_in_programs_and_features()).OrderBy((p) => p.DisplayName).Distinct();
        }

        public IEnumerable<PackageResult> list_run(ChocolateyConfiguration config)
        {
            this.Log().Info(() => "");

            string match = config.Input;
            if (String.IsNullOrEmpty(match))
            {
                match = "*";
            }
            var reMatcher = FindFilesPatternToRegex.Convert(match);
            foreach (var key in get_apps())
            {
                if (!reMatcher.IsMatch(key.DisplayName))
                {
                    continue;
                }

                var r = new PackageResult(key.DisplayName, key.DisplayName, key.InstallLocation );
                NuGet.SemanticVersion version;

                if (!NuGet.SemanticVersion.TryParse(key.DisplayVersion, out version))
                { 
                    version = new NuGet.SemanticVersion(1, 0, 0, 0);
                }

                var rp = new NuGet.RegistryPackage() { Id = key.DisplayName, Version = version };
                r.Package = rp;
                rp.IsPinned = key.IsPinned;
                rp.RegistryKey = key;
                yield return r;
            }
        }

        public void uninstall_noop(ChocolateyConfiguration config, Action<PackageResult> continueAction)
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, PackageResult> uninstall_run(ChocolateyConfiguration config, Action<PackageResult> continueAction, Action<PackageResult> beforeUninstallAction = null)
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, PackageResult> upgrade_noop(ChocolateyConfiguration config, Action<PackageResult> continueAction)
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, PackageResult> upgrade_run(ChocolateyConfiguration config, Action<PackageResult> continueAction, Action<PackageResult> beforeUpgradeAction = null)
        {
            throw new NotImplementedException();
        }
    }
}

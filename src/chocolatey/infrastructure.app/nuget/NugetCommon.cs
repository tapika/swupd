// Copyright © 2017 - 2021 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.app.nuget
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Text;
    using adapters;
    using infrastructure.configuration;
    using NuGet;
    using configuration;
    using logging;
    using Console = adapters.Console;
    using Environment = adapters.Environment;

    // ReSharper disable InconsistentNaming

    public sealed class NugetCommon
    {
        private static Lazy<IConsole> _console = new Lazy<IConsole>(() => new Console());

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void initialize_with(Lazy<IConsole> console)
        {
            _console = console;
        }

        private static IConsole Console
        {
            get { return _console.Value; }
        }

        public static IFileSystem GetNuGetFileSystem(ChocolateyConfiguration configuration, ILogger nugetLogger)
        {
            return new ChocolateyPhysicalFileSystem(ApplicationParameters.PackagesLocation) { Logger = nugetLogger };
        }

        public static IPackagePathResolver GetPathResolver(ChocolateyConfiguration configuration, IFileSystem nugetPackagesFileSystem)
        {
            return new ChocolateyPackagePathResolver(nugetPackagesFileSystem, configuration.AllowMultipleVersions);
        }

        public static IPackageRepository GetLocalRepository(IPackagePathResolver pathResolver, IFileSystem nugetPackagesFileSystem, ILogger nugetLogger)
        {
            return new ChocolateyLocalPackageRepository(pathResolver, nugetPackagesFileSystem) { Logger = nugetLogger, PackageSaveMode = PackageSaveModes.Nupkg | PackageSaveModes.Nuspec };
        }

        public static IPackageRepository GetRemoteRepository(ChocolateyConfiguration configuration, ILogger nugetLogger, IPackageDownloader packageDownloader)
        {
            if (configuration.Features.ShowDownloadProgress)
            {
                packageDownloader.ProgressAvailable += (sender, e) =>
                {
                    // http://stackoverflow.com/a/888569/18475
                    Console.Write("\rProgress: {0} {1}%".format_with(e.Operation, e.PercentComplete.to_string()).PadRight(Console.WindowWidth));
                    if (e.PercentComplete == 100)
                    {
                        Console.WriteLine("");
                    }
                };
            }
            
            IEnumerable<string> sources = configuration.Sources.to_string().Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);

            IList<IPackageRepository> repositories = new List<IPackageRepository>();

            // ensure credentials can be grabbed from configuration
            HttpClient.DefaultCredentialProvider = new ChocolateyNugetCredentialProvider(configuration);
            HttpClient.DefaultCertificateProvider = new ChocolateyClientCertificateProvider(configuration);
            if (!string.IsNullOrWhiteSpace(configuration.Proxy.Location))
            {
                "chocolatey".Log().Debug("Using proxy server '{0}'.".format_with(configuration.Proxy.Location));
                var proxy = new WebProxy(configuration.Proxy.Location, true);

                if (!String.IsNullOrWhiteSpace(configuration.Proxy.User) && !String.IsNullOrWhiteSpace(configuration.Proxy.EncryptedPassword))
                {
                    proxy.Credentials = new NetworkCredential(configuration.Proxy.User, NugetEncryptionUtility.DecryptString(configuration.Proxy.EncryptedPassword));
                }

                if (!string.IsNullOrWhiteSpace(configuration.Proxy.BypassList))
                {
                    "chocolatey".Log().Debug("Proxy has a bypass list of '{0}'.".format_with(configuration.Proxy.BypassList.escape_curly_braces()));
                    proxy.BypassList = configuration.Proxy.BypassList.Replace("*",".*").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }

                proxy.BypassProxyOnLocal = configuration.Proxy.BypassOnLocal;

                ProxyCache.Instance.Override(proxy);
            }

            var updatedSources = new StringBuilder();
            foreach (var sourceValue in sources.or_empty_list_if_null())
            {

                var source = sourceValue;
                var bypassProxy = false;
                if (configuration.MachineSources.Any(m => m.Name.is_equal_to(source) || m.Key.is_equal_to(source)))
                {
                    try
                    {
                        var machineSource = configuration.MachineSources.FirstOrDefault(m => m.Key.is_equal_to(source));
                        if (machineSource == null)
                        {
                            machineSource = configuration.MachineSources.FirstOrDefault(m => m.Name.is_equal_to(source));
                            "chocolatey".Log().Debug("Switching source name {0} to actual source value '{1}'.".format_with(sourceValue, machineSource.Key.to_string()));
                            source = machineSource.Key;
                        }

                        if (machineSource != null)
                        {
                            bypassProxy = machineSource.BypassProxy;
                            if (bypassProxy) "chocolatey".Log().Debug("Source '{0}' is configured to bypass proxies.".format_with(source));
                        }
                    }
                    catch (Exception ex)
                    {
                        "chocolatey".Log().Warn("Attempted to use a source name {0} to get default source but failed:{1} {2}".format_with(sourceValue, System.Environment.NewLine, ex.Message));
                    }
                }

                updatedSources.AppendFormat("{0};", source);

                try
                {
                    var uri = new Uri(source);
                    if (uri.IsFile || uri.IsUnc)
                    {
                        repositories.Add(new ChocolateyLocalPackageRepository(uri.LocalPath) { Logger = nugetLogger });
                    }
                    else
                    {
                        repositories.Add(new DataServicePackageRepository(new RedirectedHttpClient(uri, bypassProxy) { UserAgent = "Chocolatey Core" }, packageDownloader) { Logger = nugetLogger });
                    }
                }
                catch (Exception)
                {
                    repositories.Add(new ChocolateyLocalPackageRepository(source) { Logger = nugetLogger });
                }
            }

            if (updatedSources.Length != 0)
            {
                configuration.Sources = updatedSources.Remove(updatedSources.Length - 1, 1).to_string();
            }

            var repository = new AggregateRepository(repositories, ignoreFailingRepositories: true)
            {
                IgnoreFailingRepositories = true,
                Logger = nugetLogger,
                ResolveDependenciesVertically = true
            };

            return repository;
        }

    }

    // ReSharper restore InconsistentNaming
}
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

namespace chocolatey.infrastructure.app.registration
{
    using System.Collections.Generic;
    using infrastructure.events;
    using infrastructure.tasks;
    using NuGet;
    using SimpleInjector;
    using adapters;
    using commands;
    using filesystem;
    using infrastructure.commands;
    using infrastructure.configuration;
    using infrastructure.services;
    using infrastructure.validations;
    using nuget;
    using services;
    using tasks;
    using validations;
    using CryptoHashProvider = cryptography.CryptoHashProvider;
    using IFileSystem = filesystem.IFileSystem;
    using IHashProvider = cryptography.IHashProvider;

    // ReSharper disable InconsistentNaming

    /// <summary>
    ///   The main inversion container registration for the application. Look for other container bindings in client projects.
    /// </summary>
    public sealed class ContainerBinding
    {
        /// <summary>
        ///   Loads the module into the kernel.
        /// </summary>
        public void RegisterComponents(Container container)
        {
            var configuration = Config.get_configuration_settings();

            container.Register(() => configuration, Lifestyle.Singleton);
            container.Register<IFileSystem, DotNetFileSystem>(Lifestyle.Singleton);
            container.Register<IXmlService, XmlService>(Lifestyle.Singleton);
            container.Register<IDateTimeService, SystemDateTimeUtcService>(Lifestyle.Singleton);

            //nuget
            container.Register<ILogger, ChocolateyNugetLogger>(Lifestyle.Singleton);
            container.Register<INugetService, NugetService>(Lifestyle.Singleton);
            container.Register<IPackageDownloader, PackageDownloader>(Lifestyle.Singleton);
            container.Register<IPowershellService, PowershellService>(Lifestyle.Singleton);
            container.Register<IChocolateyPackageInformationService, ChocolateyPackageInformationService>(Lifestyle.Singleton);
            container.Register<IShimGenerationService, ShimGenerationService>(Lifestyle.Singleton);
            container.Register<IRegistryService, RegistryService>(Lifestyle.Singleton);
            container.Register<IPendingRebootService, PendingRebootService>(Lifestyle.Singleton);
            container.Register<IFilesService, FilesService>(Lifestyle.Singleton);
            container.Register<IConfigTransformService, ConfigTransformService>(Lifestyle.Singleton);
            container.Register<IHashProvider>(() => new CryptoHashProvider(container.GetInstance<IFileSystem>()), Lifestyle.Singleton);
            container.Register<ITemplateService, TemplateService>(Lifestyle.Singleton);
            container.Register<IChocolateyConfigSettingsService, ChocolateyConfigSettingsService>(Lifestyle.Singleton);
            container.Register<IChocolateyPackageService, ChocolateyPackageService>(Lifestyle.Singleton);
            container.Register<IAutomaticUninstallerService, AutomaticUninstallerService>(Lifestyle.Singleton);
            container.Register<ICommandExecutor, CommandExecutor>(Lifestyle.Singleton);
            container.Register(() => new CustomString(string.Empty));

            container.Register<IEnumerable<ICommand>>(() =>
                {
                    return ApplicationManager.Instance.Commands.AsReadOnly();
                }, Lifestyle.Singleton);

            container.Register<IEnumerable<ISourceRunner>>(() =>
                {
                    return ApplicationManager.Instance.Sources.AsReadOnly();
                }, Lifestyle.Singleton);

            container.Register<IEventSubscriptionManagerService, EventSubscriptionManagerService>(Lifestyle.Singleton);
            EventManager.initialize_with(container.GetInstance<IEventSubscriptionManagerService>);

            container.Register<IEnumerable<ITask>>(
              () =>
              {
                  var list = new List<ITask>
                    {
                        new RemovePendingPackagesTask(container.GetInstance<IFileSystem>(), container.GetInstance<IDateTimeService>())
                    };

                  return list.AsReadOnly();
              },
              Lifestyle.Singleton);

            container.Register<IEnumerable<IValidation>>(
                () =>
                {
                    var list = new List<IValidation>
                    {
                        new GlobalConfigurationValidation(),
                        new SystemStateValidation(container.GetInstance<IPendingRebootService>())
                    };

                    return list.AsReadOnly();
                },
                Lifestyle.Singleton);
        }
    }

    // ReSharper restore InconsistentNaming
}
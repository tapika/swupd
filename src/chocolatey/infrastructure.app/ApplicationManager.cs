﻿using chocolatey.infrastructure.app.attributes;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.commands;
using chocolatey.infrastructure.filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace chocolatey.infrastructure.app
{
    public class ApplicationManager
    {
        static public AsyncLocal<ApplicationManager> _instance = new AsyncLocal<ApplicationManager>();

        /// <summary>
        /// Returns true if ApplicationManager.Instance was initialized.
        /// </summary>
        static public bool IsInitialized()
        {
            return _instance.Value != null;
        }

        /// <summary>
        /// Application manager instance
        /// </summary>
        static public ApplicationManager Instance
        {
            get
            {
                if (_instance.Value == null)
                {
                    _instance.Value = new ApplicationManager();
                }

                return _instance.Value;
            }

            set
            {
                _instance.Value = value;
            }
        }

        List<ICommand> _commands;
        /// <summary>
        /// Gets list of all supported commands by ApplicationManager.
        /// </summary>
        public List<ICommand> Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = new List<ICommand>()
                    {
                        new ChocolateyListCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyHelpCommand(Container),
                        new ChocolateyInfoCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyInstallCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyPinCommand(Container.GetInstance<IChocolateyPackageInformationService>(), Container.GetInstance<NuGet.ILogger>(), Container.GetInstance<INugetService>()),
                        new ChocolateyOutdatedCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyUpgradeCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyUninstallCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyPackCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyPushCommand(Container.GetInstance<IChocolateyPackageService>(), Container.GetInstance<IChocolateyConfigSettingsService>()),
                        new ChocolateyNewCommand(Container.GetInstance<ITemplateService>()),
                        new ChocolateySourceCommand(Container.GetInstance<IChocolateyConfigSettingsService>()),
                        new ChocolateyConfigCommand(Container.GetInstance<IChocolateyConfigSettingsService>()),
                        new ChocolateyFeatureCommand(Container.GetInstance<IChocolateyConfigSettingsService>()),
                        new ChocolateyApiKeyCommand(Container.GetInstance<IChocolateyConfigSettingsService>()),
                        new ChocolateyUnpackSelfCommand(Container.GetInstance<IFileSystem>()),
                        new ChocolateyVersionCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyUpdateCommand(Container.GetInstance<IChocolateyPackageService>())
                    };
                }
                return _commands;
            }
        }

        /// <summary>
        /// Finds command by name
        /// </summary>
        /// <param name="commandName">command name</param>
        /// <exception cref="ArgumentException">thrown when command was not found</exception>
        public ICommand FindCommand(string commandName)
        {
            var command = Commands.Where((c) =>
            {
                var attributes = c.GetType().GetCustomAttributes(typeof(CommandForAttribute), false);
                return attributes.Cast<CommandForAttribute>().Any(attribute => attribute.CommandName.is_equal_to(commandName));
            }).FirstOrDefault();

            if (command == null)
            {
                throw new ArgumentException(@$"Could not find a command registered that meets '{commandName}'.
 Try choco -? for command reference/help.");
            }

            return command;
        }

        public ApplicationManager()
        {
            //LogService = LogService.GetInstance(!ApplicationParameters.runningUnitTesting);
            //Locations = new InstallContext();
        }

        /// <summary>
        /// In long term perspective it's planned to either remove SimpleInjector container or
        /// replace it with Caliburn.Micro.
        /// 
        /// See following articles:
        ///     https://www.palmmedia.de/Blog/2011/8/30/ioc-container-benchmark-performance-comparison
        ///     https://github.com/Caliburn-Micro/Caliburn.Micro/issues/795
        /// </summary>
        public SimpleInjector.Container Container { get; set; }

        //LogService LogService { get; set; }
        //InstallContext Locations { get; set; }
    }
}

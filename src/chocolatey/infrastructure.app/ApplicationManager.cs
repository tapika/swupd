using chocolatey.infrastructure.app.attributes;
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
        static ApplicationManager _mainThreadInstance = null;
        static AsyncLocal<ApplicationManager> _instance = new AsyncLocal<ApplicationManager>();

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
                    //
                    // If we have main application (choco, chocogui) - then _mainThreadInstance gets initialized,
                    // and all threads will log to same ApplicationManager instance.
                    //
                    // When unit testing - there will be separate / independent ApplicationManager instance for each testing task.
                    //
                    if (_mainThreadInstance != null)
                    {
                        _instance.Value = _mainThreadInstance;
                    }
                    else
                    { 
                        _instance.Value = new ApplicationManager();

                        if (_mainThreadInstance == null && Thread.CurrentThread.ManagedThreadId == 1)
                        {
                            _mainThreadInstance = _instance.Value;
                        }
                    }
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
                        new ChocolateyPinCommand(
                            Container.GetInstance<IRegistryService>(), 
                            Container.GetInstance<IChocolateyPackageService>(),
                            Container.GetInstance<IChocolateyPackageInformationService>()
                        ),
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
                        new ChocolateyPsRunCommand(
                            Container.GetInstance<IChocolateyPackageService>(), 
                            Container.GetInstance<IPowershellService>(),
                            Container.GetInstance<IFileSystem>()
                        ),
                        new ChocolateyVersionCommand(Container.GetInstance<IChocolateyPackageService>()),
                        new ChocolateyUpdateCommand(Container.GetInstance<IChocolateyPackageService>())
                    };
                }
                return _commands;
            }
        }

        List<ISourceRunner> _sources;
        /// <summary>
        /// Different sources on which ApplicationManager can operate
        /// </summary>
        public List<ISourceRunner> Sources
        {
            get 
            {
                if (_sources == null)
                {
                    _sources = new List<ISourceRunner>()
                    {
                        Container.GetInstance<INugetService>(),
                        new WebPiService(Container.GetInstance<ICommandExecutor>(), Container.GetInstance<INugetService>()),
                        new WindowsFeatureService(Container.GetInstance<ICommandExecutor>(), Container.GetInstance<INugetService>(), Container.GetInstance<IFileSystem>()),
                        new CygwinService(Container.GetInstance<ICommandExecutor>(), Container.GetInstance<INugetService>(), Container.GetInstance<IFileSystem>(), Container.GetInstance<IRegistryService>()),
                        new PythonService(Container.GetInstance<ICommandExecutor>(), Container.GetInstance<INugetService>(), Container.GetInstance<IFileSystem>(), Container.GetInstance<IRegistryService>()),
                        new RubyGemsService(Container.GetInstance<ICommandExecutor>(), Container.GetInstance<INugetService>()),
                        new WindowsInstallService(Container, Container.GetInstance<IRegistryService>())
                    };
                }

                return _sources;
            }
        }

        /// <summary>
        /// Gets types of all commands.
        /// </summary>
        public Type[] CommandTypes
        {
            get
            {
                return new[] {
                    typeof(ChocolateyListCommand),
                    typeof(ChocolateyHelpCommand),
                    typeof(ChocolateyInfoCommand),
                    typeof(ChocolateyInstallCommand),
                    typeof(ChocolateyPinCommand),
                    typeof(ChocolateyOutdatedCommand),
                    typeof(ChocolateyUpgradeCommand),
                    typeof(ChocolateyUninstallCommand),
                    typeof(ChocolateyPackCommand),
                    typeof(ChocolateyPushCommand),
                    typeof(ChocolateyNewCommand),
                    typeof(ChocolateySourceCommand),
                    typeof(ChocolateyConfigCommand),
                    typeof(ChocolateyFeatureCommand),
                    typeof(ChocolateyApiKeyCommand),
                    typeof(ChocolateyUnpackSelfCommand),
                    typeof(ChocolateyPsRunCommand),
                    typeof(ChocolateyVersionCommand),
                    typeof(ChocolateyUpdateCommand),
                };
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
                throw new ArgumentException($@"Could not find a command registered that meets '{commandName}'.
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
        /// In long term perspective it's planned to either remove SimpleInjector Container or
        /// replace it with Caliburn.Micro.
        /// 
        /// See following articles:
        ///     https://www.palmmedia.de/Blog/2011/8/30/ioc-Container-benchmark-performance-comparison
        ///     https://github.com/Caliburn-Micro/Caliburn.Micro/issues/795
        /// </summary>
        public SimpleInjector.Container Container { get; set; }

        //LogService LogService { get; set; }
        //InstallContext Locations { get; set; }
    }
}

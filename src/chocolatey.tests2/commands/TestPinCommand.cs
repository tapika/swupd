using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.commands;
using chocolatey.infrastructure.logging;
using chocolatey.tests.integration;
using logtesting;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace chocolatey.tests2.commands
{
    [Serializable]
    public class PinCommandConfig: PinCommandConfiguration
    {
        public string Version { get; set; }
    }


    [Parallelizable(ParallelScope.All)]
    public class TestPinCommand: LogTesting
    {
        protected ChocolateyPinCommand pincmd;

        public TestPinCommand()
        {
            pincmd = ApplicationManager.Instance.Commands.OfType<ChocolateyPinCommand>().Single();
        }

        public void PinContext(
            Func<ChocolateyConfiguration, PinCommandConfig[]> getPinTestCommands,
            [CallerMemberName] string testFolder = ""
        )
        {
            var conf = Scenario.pin(true);
            conf.PinCommand.Command = PinCommandType.list;
            InstallContext.Instance.RootLocation = PrepareTestFolder(ChocoTestContext.installed_5_packages, 
                conf, Path.Combine(nameof(TestPinCommand), testFolder));

            conf.Sources = InstallContext.Instance.PackagesLocation;
            conf.ListCommand.LocalOnly = true;
            conf.AllVersions = true;
            conf.Prerelease = true;

            foreach (var cmd in getPinTestCommands(conf))
            {
                conf.PinCommand = cmd;
                conf.Version = cmd.Version;
                try
                {
                    pincmd.run(conf);
                }
                catch (ApplicationException pinexception)
                {
                    LogService.console.Info($"ApplicationException: {pinexception.Message}");
                }
            }
            
            conf.PinCommand.Command = PinCommandType.list;
            conf.Version = null;
            pincmd.run(conf);
        }

        [LogTest()]
        public void when_listing_pins_with_no_pins()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] { };  
            });
        }

        [LogTest()]
        public void when_listing_pins_with_an_existing_pin()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] { 
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage" }
                };
            });
        }

        [LogTest()]
        public void when_listing_pins_with_existing_pins()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage" },
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "installpackage" }
                };
            });
        }

        [LogTest()]
        public void when_setting_a_pin_for_an_installed_package()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage" },
                };
            });
        }

        [LogTest()]
        public void when_setting_a_pin_for_an_already_pinned_package()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage" },
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage" },
                };
            });
        }

        [LogTest()]
        public void when_setting_a_pin_for_a_non_installed_package()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "whatisthis" },
                };
            });
        }

        [LogTest()]
        public void when_removing_a_pin_for_a_pinned_package()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage" },
                    new PinCommandConfig{ Command = PinCommandType.remove, Name = "upgradepackage" },
                };
            });
        }

        [LogTest()]
        public void when_removing_a_pin_for_an_unpinned_package()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.remove, Name = "upgradepackage" },
                };
            });
        }

        [LogTest()]
        public void when_removing_a_pin_for_a_non_installed_package()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.remove, Name = "whatisthis" },
                };
            });
        }

        [LogTest]
        public void when_listing_pins_with_certain_existing_version()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage", Version = "1.0.0" },
                };
            });
        }

        [LogTest]
        public void when_listing_pins_with_certain_nonexisting_version()
        {
            PinContext((conf) =>
            {
                return new PinCommandConfig[] {
                    new PinCommandConfig{ Command = PinCommandType.add, Name = "upgradepackage", Version = "4.3.1" },
                };
            });
        }

    }
}

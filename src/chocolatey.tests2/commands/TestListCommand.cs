using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.logging;
using chocolatey.tests.integration;
using logtesting;
using NuGet;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace chocolatey.tests2.commands
{
    public enum ListFolderNames
    { 
        installupdate,
        exactpackage
    };

    public class ListScenario : LogTesting
    {
        protected IChocolateyPackageService Service;

        public ListScenario()
        {
            Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();
        }

        static ConcurrentDictionary<string, Task> updaters = new ConcurrentDictionary<string, Task>();
        
        /// <summary>
        /// Gets test folder for testing. If folder does not exists, creates new task which will create specific folder.
        /// </summary>
        public string PrepareTestFolder(string folder, ChocolateyConfiguration conf)
        {
            string folderPath = Path.Combine(InstallContext.ApplicationInstallLocation, "test_folders", folder);
            string folderPathOk = Path.Combine(folderPath, $".updated_ok");

            if (Directory.Exists(folderPathOk))
            {
                return folderPath;
            }

            Task newtask;

            switch (folder)
            {
                default: throw new Exception($"GetListFolder: {folder} is not known");

                case nameof(ListFolderNames.installupdate):
                    {
                        newtask = new Task(() =>
                        {
                            InstallContext.Instance.ShowShortPaths = true;

                            using (new VerifyingLog(folder))
                            {
                                if (Directory.Exists(folderPath))
                                {
                                    Directory.Delete(folderPath, true);
                                }
                                Directory.CreateDirectory(folderPath);

                                string oldSources = conf.Sources;
                                conf.Sources = Path.Combine(InstallContext.ApplicationInstallLocation, "context");
                                InstallContext.Instance.RootLocation = folderPath;
                                Scenario.install_package(conf, "installpackage", "1.0.0");
                                Scenario.install_package(conf, "upgradepackage", "1.0.0");

                                conf.Sources = oldSources;
                                Directory.CreateDirectory(folderPathOk);
                            }
                        });
                    }
                    break;

                case nameof(ListFolderNames.exactpackage):
                    {
                        newtask = new Task(() =>
                        {
                            InstallContext.Instance.ShowShortPaths = true;

                            using (new VerifyingLog(folder))
                            {
                                if (Directory.Exists(folderPath))
                                {
                                    Directory.Delete(folderPath, true);
                                }
                                Directory.CreateDirectory(folderPath);

                                string oldSources = conf.Sources;
                                InstallContext.Instance.RootLocation = folderPath;
                                conf.Sources = InstallContext.Instance.PackagesLocation;

                                Scenario.add_packages_to_source_location(conf, "exactpackage*" + Constants.PackageExtension);

                                conf.Sources = oldSources;
                                Directory.CreateDirectory(folderPathOk);
                            }
                        });
                    }
                    break;
            }

            var task = updaters.GetOrAdd(folder, newtask);
            if (task == newtask)
                newtask.Start();

            task.Wait();
            return folderPath;
        }
    }

    [Parallelizable(ParallelScope.All)]
    public class TestListCommand: ListScenario
    {
        public void CommonList(Action<ChocolateyConfiguration> confPatch)
        {
            var conf = Scenario.list(true);
            InstallContext.Instance.RootLocation =
                PrepareTestFolder(nameof(ListFolderNames.installupdate), conf);
            conf.Sources = InstallContext.Instance.PackagesLocation;
            confPatch(conf);
            Service.list_run(conf).ToList();
        }

        // when_searching_packages_with_no_filter_happy_path
        // when_searching_all_available_packages
        // almost identical to when_listing_local_packages

        // when_searching_for_a_particular_package
        [LogTest]
        public void ListSpecificPackage()
        {
            CommonList((conf) =>
            {
                conf.Input = conf.PackageNames = "upgradepackage";
            });
        }

        // when_searching_all_available_packages
        [LogTest]
        public void ListAllPackages()
        {
            CommonList((conf) =>
            {
                conf.AllVersions = true;
            });
        }

        // when_searching_packages_with_verbose
        [LogTest]
        public void ListWithVerbose()
        {
            CommonList((conf) =>
            {
                conf.Verbose = true;
            });
        }

        // when_listing_local_packages
        [LogTest]
        public void ListAll()
        {
            CommonList((conf) =>
            {
                conf.ListCommand.LocalOnly = true;
            });
        }

        // when_listing_local_packages_limiting_output
        [LogTest]
        public void ListAllRegular()
        {
            CommonList((conf) =>
            {
                conf.RegularOutput = false;
            });
        }

        // when_listing_local_packages_limiting_output_with_id_only
        [LogTest]
        public void ListPackageIds()
        {
            CommonList((conf) =>
            {
                conf.RegularOutput = false;
                conf.ListCommand.IdOnly = true;
            });
        }

        // when_listing_packages_with_no_sources_enabled
        [LogTest]
        public void ListNoSources()
        {
            CommonList((conf) =>
            {
                conf.Sources = null;
            });
        }

        public void ExactPackagesList(Action<ChocolateyConfiguration> confPatch)
        {
            var conf = Scenario.list(true);
            InstallContext.Instance.RootLocation =
                PrepareTestFolder(nameof(ListFolderNames.exactpackage), conf);
            conf.Sources = InstallContext.Instance.PackagesLocation;
            confPatch(conf);
            Service.list_run(conf).ToList();
        }

        // when_searching_for_an_exact_package
        [LogTest]
        public void SearchingExactPackage()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.Input = conf.PackageNames = "exactpackage";
            });
        }

        // when_searching_for_an_exact_package
        [LogTest]
        public void SearchingNoPackages()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.Input = conf.PackageNames = "exactpackage123";
            });
        }

        // when_searching_for_all_packages_with_exact_id
        [LogTest]
        public void SearchingPackages()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.AllVersions = true;
                conf.Input = conf.PackageNames = "exactpackage";
            });
        }

        // when_searching_for_all_packages_including_prerelease_with_exact_id
        [LogTest]
        public void SearchingAllPackages()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.AllVersions = true;
                conf.Prerelease = true;
                conf.Input = conf.PackageNames = "exactpackage";
            });
        }

    }
}


using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.logging;
using chocolatey.infrastructure.results;
using chocolatey.tests.integration;
using logtesting;
using NuGet;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace chocolatey.tests2.commands
{
    public class InstallScenario : LogTesting
    {
        protected IChocolateyPackageService Service;

        public InstallScenario()
        {
            Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();
        }
    };

    [Parallelizable(ParallelScope.All), FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class TestInstallCommand: InstallScenario
    {
        static List<string> GetFilesAndFolders(string path)
        {
            var list = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList();
            list.Sort();
            return list;
        }

        List<string> addedFiles;
        ChocolateyConfiguration conf;

        public void InstallOn(
            ChocoTestContext testcontext,
            Action<ChocolateyConfiguration> confPatch = null,
            [CallerMemberName] string testFolder = ""
        )
        {
            conf = Scenario.baseline_configuration(true);
            string rootDirBefore = PrepareTestFolder(testcontext, conf, testFolder);
            InstallContext.Instance.RootLocation = rootDirBefore;
            conf.Sources = InstallContext.TestPackagesFolder;
            conf.PackageNames = conf.Input = "installpackage";
            if (confPatch != null)
            { 
                confPatch(conf);
            }

            string rootDir = InstallContext.Instance.RootLocation;
            var listBeforeUpdate = GetFilesAndFolders(rootDir);

            if (conf.Noop)
            {
                Service.install_noop(conf);
            }
            else
            { 
                var results = Service.install_run(conf);
                var packages = results.Keys.ToList();
                packages.Sort();
                var console = LogService.console;

                foreach (var package in packages)
                {
                    var pkgresult = results[package];
                    console.Info($"=> install result for {pkgresult.Name}/{pkgresult.Version}: "
                        + ((pkgresult.Success) ? "succeeded" : "FAILED"));

                    foreach (var resultType in new[] { ResultType.Error, ResultType.Warn, ResultType.Inconclusive })
                    {
                        var msgs = pkgresult.Messages.Where(x => x.MessageType == resultType).ToList();

                        if (msgs.Count == 0)
                        {
                            continue;
                        }

                        console.Info($"  {resultType}: ");
                        foreach (var msg in msgs)
                        { 
                            console.Info($"  - {msg.Message}");
                        }
                    }
                }
            }
            var listAfterUpdate = GetFilesAndFolders(rootDir);
            addedFiles = new List<string>();

            foreach (var file in listAfterUpdate)
            {
                if (!listBeforeUpdate.Contains(file))
                {
                    addedFiles.Add(file.Substring(rootDir.Length + 1));
                }
            }
            
            ListUpdates();
        }

        void ListUpdates()
        {
            var console = LogService.console;
            if (addedFiles.Count == 0)
            {
                console.Info("=> folder was not updated");
                return;
            }

            console.Info("=> added new files:");
            foreach (var f in addedFiles)
            { 
                console.Info(f);

                if (Path.GetExtension(f) == Constants.PackageExtension)
                {
                    string nupkgPath = Path.Combine(InstallContext.Instance.RootLocation, f);
                    var package = new OptimizedZipPackage(nupkgPath);
                    console.Info("  version: " + package.Version.Version.to_string());
                }
            }
        }

        public void InstallOnEmpty(
            Action<ChocolateyConfiguration> confPatch = null,
            [CallerMemberName] string testFolder = ""
        )
        {
            InstallOn(ChocoTestContext.empty, confPatch, testFolder);
        }

        // when_noop_installing_a_package
        [LogTest()]
        public void NoInstall()
        {
            InstallOnEmpty((conf) =>
            {
                conf.Noop = true;
            });
        }

        // when_noop_installing_a_package_that_does_not_exist
        [LogTest()]
        public void InstallNonExistingPackage()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "somethingnonexisting";
                conf.Noop = true;
            });
        }

        // when_installing_a_package_happy_path
        [LogTest()]
        public void Install()
        {
            InstallOnEmpty((conf) =>
            {
                //conf.Features.UseShimGenService = true;
            });

            // not ported:
            //should_have_a_console_shim_that_is_set_for_non_gui_access
            //should_have_a_graphical_shim_that_is_set_for_gui_access
            // maybe later on...
        }

        // when_installing_packages_with_packages_config
        [LogTest()]
        public void InstallWithConfig()
        {
            InstallOnEmpty((conf) =>
            {
                var packagesConfig = "{0}\\context\\testing.packages.config".format_with(Scenario.get_top_level());
                conf.PackageNames = conf.Input = packagesConfig;
            });
        }

        public void InstallOnInstall(
            Action<ChocolateyConfiguration> confPatch = null,
            [CallerMemberName] string testFolder = ""
        )
        {
            InstallOn(ChocoTestContext.install, confPatch, testFolder);
        }

        void InstalledPackageIs_1_0()
        {
            var packageFile = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, conf.PackageNames + Constants.PackageExtension);
            var package = new OptimizedZipPackage(packageFile);
            Assert.AreEqual(package.Version.Version.to_string(), "1.0.0.0");
        }

        // when_installing_an_already_installed_package
        [LogTest()]
        public void InstallOnAlreadyInstalled()
        {
            InstallOnInstall((conf) => { });
            InstalledPackageIs_1_0();
        }

        // when_force_installing_an_already_installed_package
        [LogTest()]
        public void ForceInstallOnAlreadyInstalled()
        {
            string modifiedFilePath = null;
            string modifiedContent = "bob";
            InstallOnInstall((conf) => 
                { 
                    conf.Force = true;
                    modifiedFilePath = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                    File.WriteAllText(modifiedFilePath, modifiedContent);
                }
            );
            Assert.AreNotEqual(File.ReadAllText(modifiedFilePath), modifiedContent);
            InstalledPackageIs_1_0();
        }

        // when_force_installing_an_already_installed_package_that_errors
        [LogTest()]
        public void ForceInstallOnErrorPackage()
        {
            string modifiedFilePath = null;
            string modifiedContent = "bob";

            InstallOn(
                ChocoTestContext.badpackage,
                (conf) =>
                {
                    conf.PackageNames = conf.Input = "badpackage";
                    conf.Force = true;
                    modifiedFilePath = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                    File.WriteAllText(modifiedFilePath, modifiedContent);
                });
        
            Assert.AreEqual(File.ReadAllText(modifiedFilePath), modifiedContent);
            InstalledPackageIs_1_0();
        }

        void InstallOnLockedCommon(FileShare shareMode, [CallerMemberName] string testFolder = "")
        {
            FileStream fileStream = null;

            InstallOnInstall((conf) =>
            {
                conf.Force = true;
                string modifiedFilePath = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                fileStream = new FileStream(modifiedFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, shareMode);
            }, testFolder
            );

            fileStream.Close();
            InstalledPackageIs_1_0();
        }

        // when_force_installing_an_already_installed_package_with_a_read_and_delete_share_locked_file
        [LogTest()]
        public void InstallOnLockedFile()
        {
            InstallOnLockedCommon(FileShare.Read | FileShare.Delete);
        }

        // when_force_installing_an_already_installed_package_with_with_an_exclusively_locked_file
        [LogTest()]
        public void InstallOnExclusvelyLockedFile()
        {
            InstallOnLockedCommon(FileShare.None);

            // Several tests in InstallScenarious.cs were marked as 
            // "Force install with file locked leaves inconsistent state - GH-114"
        }

        // when_installing_a_package_that_exists_but_a_version_that_does_not_exist
        [LogTest()]
        public void InstallPackageVersionDoesNotExists()
        {
            InstallOnEmpty((conf) =>
            {
                conf.Version = "1.0.1";
            });
        }

        // when_installing_a_package_that_does_not_exist
        [LogTest()]
        public void InstallPackageDoesNotExists()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "nonexisting";
            });
        }

        // when_installing_a_package_that_errors
        [LogTest()]
        public void InstallBadPackage()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "badpackage";
            });

            //maybe should not create ".chocolatey\badpackage.2.0\.files" file
        }

        // when_installing_a_package_that_has_nonterminating_errors
        [LogTest()]
        public void InstallNonTerminatingPackage()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "nonterminatingerror";
            });
        }

        // when_installing_a_package_that_has_nonterminating_errors_with_fail_on_stderr
        [LogTest()]
        public void InstallTerminatingPackage()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "nonterminatingerror";
                conf.Features.FailOnStandardError = true;
            });
        }

    }
}


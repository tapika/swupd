﻿using chocolatey.infrastructure.app;
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

    [Parallelizable(ParallelScope.All)]
    public class TestInstallCommand: InstallScenario
    {
        static List<string> GetFilesAndFolders(string path)
        {
            var list = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList();
            list.Sort();
            return list;
        }

        List<string> addedFiles;

        public void InstallOnEmpty(
            Action<ChocolateyConfiguration> confPatch = null,
            [CallerMemberName] string testFolder = ""
        )
        {
            var conf = Scenario.baseline_configuration(true);
            string rootDirBefore = PrepareTestFolder(ChocoTestContext.empty, conf, testFolder);
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

        // when_noop_installing_a_package
        [LogTest()]
        public void NoInstall()
        {
            InstallOnEmpty((conf) =>
            {
                conf.Noop = true;
            });
            ListUpdates();
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
            ListUpdates();
        }

        // when_installing_a_package_happy_path
        [LogTest()]
        public void Install()
        {
            InstallOnEmpty((conf) =>
            {
                //conf.Features.UseShimGenService = true;
            });
            ListUpdates();

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
            ListUpdates();
        }

    }
}


using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.logging;
using chocolatey.tests.integration;
using logtesting;
using NuGet;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.XPath;

namespace chocolatey.tests2.commands
{
    [Parallelizable(ParallelScope.All), FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class TestInstallCommand: LogTesting
    {
        public string GetTestFolder([CallerMemberName] string testFolder = "")
        { 
            return Path.Combine(nameof(TestInstallCommand), testFolder);
        }

        public void InstallOnEmpty(
            Action<ChocolateyConfiguration> confPatch = null,
            ChocoTestContext packagesContext = ChocoTestContext.packages_default,
            [CallerMemberName] string testFolder = ""
        )
        {
            ExecuteConf(ChocoTestContext.empty, confPatch, packagesContext, GetTestFolder(testFolder));
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
            ExecuteConf(ChocoTestContext.install, confPatch, ChocoTestContext.packages_default, GetTestFolder(testFolder));
        }

        void InstalledPackageIs_1_0()
        {
            var packageFile = Path.Combine(InstallContext.Instance.PackagesLocation, lastconf.PackageNames, lastconf.PackageNames + Constants.PackageExtension);
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

        public void LocalInstallOn(
            ChocoTestContext testcontext,
            Action<ChocolateyConfiguration> confPatch = null,
            ChocoTestContext packagesContext = ChocoTestContext.packages_default,
            [CallerMemberName] string testFolder = ""
        )
        {
            ExecuteConf(testcontext, confPatch, packagesContext, GetTestFolder(testFolder));
        }

        // when_force_installing_an_already_installed_package_that_errors
        [LogTest()]
        public void ForceInstallOnErrorPackage()
        {
            string modifiedFilePath = null;
            string modifiedContent = "bob";

            LocalInstallOn(
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
                lockedFilePath = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyinstall.ps1");
                fileStream = new FileStream(lockedFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, shareMode);
            }, GetTestFolder(testFolder)
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

        // when_installing_a_side_by_side_package
        [LogTest()]
        public void InstallSideBySide()
        {
            InstallOnEmpty((conf) =>
            {
                conf.AllowMultipleVersions = true;
            });
        }

        // when_switching_a_normal_package_to_a_side_by_side_package
        [LogTest()]
        public void SwitchToSideBySide()
        {
            InstallOnInstall((conf) =>
            {
                conf.AllowMultipleVersions = true;
                conf.Force = true;
            });
        }

        // when_switching_a_side_by_side_package_to_a_normal_package
        [LogTest()]
        public void SwitchSxsToNormal()
        {
            LocalInstallOn(ChocoTestContext.install_sxs, (conf) =>
            {
                conf.AllowMultipleVersions = false;
                conf.Force = true;
            });
        }

        // when_installing_a_package_with_dependencies_happy
        [LogTest()]
        public void InstallWithDependencies()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
            }, ChocoTestContext.packages_for_dependency_testing );
        }

        // when_force_installing_an_already_installed_package_with_dependencies
        [LogTest()]
        public void InstallWithDependenciesOnInstalled()
        {
            LocalInstallOn(ChocoTestContext.hasdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
                conf.Force = true;
            }, ChocoTestContext.packages_for_dependency_testing3);

            InstalledPackageIs_1_0();
        }

        // when_force_installing_an_already_installed_package_forcing_dependencies
        [LogTest()]
        public void InstallWithAllDependenciesOnInstalled()
        {
            LocalInstallOn(ChocoTestContext.hasdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
                conf.Force = true;
                conf.ForceDependencies = true;
            }, ChocoTestContext.packages_for_dependency_testing2);

            InstalledPackageIs_1_0();
        }

        // when_force_installing_an_already_installed_package_ignoring_dependencies
        [LogTest()]
        public void InstallIgnoreDependencies()
        {
            LocalInstallOn(ChocoTestContext.hasdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
                conf.Force = true;
                conf.IgnoreDependencies = true;
            }, ChocoTestContext.packages_for_dependency_testing3);

            InstalledPackageIs_1_0();
        }

        // when_force_installing_an_already_installed_package_forcing_and_ignoring_dependencies
        [LogTest()]
        public void InstallForceAndIgnoreDependencies()
        {
            LocalInstallOn(ChocoTestContext.hasdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
                conf.Force = true;
                conf.ForceDependencies = true;
                conf.IgnoreDependencies = true;
            }, ChocoTestContext.packages_for_dependency_testing3);
        }

        // when_installing_a_package_with_dependencies_and_dependency_cannot_be_found
        [LogTest()]
        public void InstallMissingDependencies()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
            }, ChocoTestContext.pack_hasdependency_2_1_0);
        }

        // when_installing_a_package_ignoring_dependencies_that_cannot_be_found
        [LogTest()]
        public void InstallIgnoreMissingDependencies()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
                conf.IgnoreDependencies = true;
            }, ChocoTestContext.pack_hasdependency_2_1_0);
        }

        // when_installing_a_package_that_depends_on_a_newer_version_of_an_installed_dependency
        [LogTest()]
        public void InstallPackageDependOnNewerVersion()
        {
            LocalInstallOn(ChocoTestContext.isdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
            }, ChocoTestContext.packages_for_dependency_testing5);
        }

        // when_installing_a_package_that_depends_on_an_unavailable_newer_version_of_an_installed_dependency
        [LogTest()]
        public void InstallPackageDependOnNonExistingVersion()
        {
            LocalInstallOn(ChocoTestContext.isdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
            }, ChocoTestContext.packages_for_dependency_testing4);
        }

        // when_installing_a_package_that_depends_on_an_unavailable_newer_version_of_an_installed_dependency_ignoring_dependencies
        [LogTest()]
        public void InstallPackageDependOnNonExistingVersionIgnoreDependencies()
        {
            LocalInstallOn(ChocoTestContext.isdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
                conf.IgnoreDependencies = true;
            }, ChocoTestContext.packages_for_dependency_testing4);
        }

        // when_installing_a_package_with_dependencies_on_a_newer_version_of_a_package_than_an_existing_package_has_with_that_dependency
        [LogTest()]
        public void InstallPackageWithDependenciesOnNewerVesion()
        {
            LocalInstallOn(ChocoTestContext.isdependency_hasdependency, (conf) =>
            {
                conf.PackageNames = conf.Input = "conflictingdependency";
            }, ChocoTestContext.packages_for_dependency_testing6);

            ListPackageVersions("isdependency");
        }

        // when_installing_a_package_with_dependencies_on_a_newer_version_of_a_package_than_are_allowed_by_an_existing_package_with_that_dependency
        // when_installing_a_package_with_dependencies_on_an_older_version_of_a_package_than_is_already_installed
        // when_installing_a_package_with_a_dependent_package_that_also_depends_on_a_less_constrained_but_still_valid_dependency_of_the_same_package
        // - All tests ignored ([Pending])

        // when_installing_a_package_from_a_nupkg_file
        [LogTest()]
        public void InstallByPath()
        {
            InstallOnEmpty((conf) =>
            {
                string path = Path.Combine(conf.Sources, "installpackage.1.0.0.nupkg");
                conf.PackageNames = conf.Input = path;
                conf.Features.UseShimGenService = true;
            });
        }

        // when_installing_a_package_with_config_transforms
        [LogTest()]
        public void InstallWithConfigTransforms()
        {
            InstallOnEmpty((conf) =>
            {
                conf.PackageNames = conf.Input = "upgradepackage";
            }, ChocoTestContext.pack_upgradepackage_1_0_0);

            string xmlFilePath = Path.Combine(InstallContext.Instance.PackagesLocation, lastconf.PackageNames, "tools", "console.exe.config");
            var xmlDocument = new XPathDocument(xmlFilePath);
            var xPathNavigator = xmlDocument.CreateNavigator();
            var console = LogService.console;
            console.Info("=> tools/console.exe.config values:");
            foreach (string tag in new[] { "test", "testReplace", "insert" })
            {
                string value = xPathNavigator.SelectSingleNode($"//configuration/appSettings/add[@key='{tag}']/@value").TypedValue.to_string();
                console.Info($"  {tag}: {value}");
            }
        }

        // when_installing_a_package_with_no_sources_enabled
        [LogTest()]
        public void InstallNoSources()
        {
            InstallOnEmpty((conf) =>
            {
                conf.Sources = null;
            });
        }

        [LogTest]
        public void when_installing_regpackage_on_empty()
        {
            using (var tester = new TestRegistry())
            {
                tester.DeleteInstallEntries(installpackage2_id);
                tester.LogInstallEntries(false, installpackage2_id);

                InstallOnEmpty((conf) =>
                {
                    conf.PackageNames = conf.Input = installpackage2_id;
                }, ChocoTestContext.pack_installpackage2_1_0_0);

                tester.LogInstallEntries(true, installpackage2_id);
                tester.DeleteInstallEntries(installpackage2_id);
            }
        }

        [LogTest]
        public void when_installing_regpackage_on_already_installed()
        {
            using (var tester = new TestRegistry(false))
            {
                LocalInstallOn(ChocoTestContext.installupdate2, (conf) =>
                {
                    conf.PackageNames = conf.Input = installpackage2_id;

                    tester.Lock();
                    tester.DeleteInstallEntries(installpackage2_id);
                    tester.AddInstallPackage2Entry();

                }, ChocoTestContext.pack_installpackage2_1_0_0);

                tester.DeleteInstallEntries(installpackage2_id);
            }
        }


    }
}


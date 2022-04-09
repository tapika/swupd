﻿using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.filesystem;
using chocolatey.infrastructure.logging;
using logtesting;
using NuGet;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace chocolatey.tests2.commands
{
    [Parallelizable(ParallelScope.All), FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class TestUpgradeCommand : LogTesting
    {
        public string DoOperation(
            ChocoTestContext testcontext = ChocoTestContext.upgrade_testing_context,
            Action<ChocolateyConfiguration> confPatch = null,
            CommandNameType operation = CommandNameType.upgrade,
            ChocoTestContext packagesContext = ChocoTestContext.packages_for_upgrade_testing,
            [CallerMemberName] string testFolder = ""
        )
        {
            string packageName = null;
            Action<ChocolateyConfiguration> upgradePatch = (conf) =>
            {
                conf.CommandName = operation.ToString();
                conf.PackageNames = conf.Input = "upgradepackage";

                if (confPatch != null)
                {
                    confPatch(conf);
                }

                packageName = conf.PackageNames;
            };

            InstallOn(testcontext, upgradePatch,
                packagesContext,
                Path.Combine(nameof(TestUpgradeCommand), testFolder));

            return packageName;
        }

        public void Preinstall(
            string version,
            bool allowPrerelease = false,
            ChocoTestContext testcontext = ChocoTestContext.upgrade_testing_context,
            Action<ChocolateyConfiguration> confPatch = null,
            [CallerMemberName] string testFolder = ""
        )
        {
            Action<ChocolateyConfiguration> upgradePatch = (conf) =>
            {
                conf.Version = version;
                conf.Prerelease = allowPrerelease;

                if (confPatch != null)
                {
                    confPatch(conf);
                }
            };

            DoOperation(testcontext, upgradePatch, CommandNameType.install, ChocoTestContext.packages_for_upgrade_testing, testFolder);
        }

        public void TestUpgrade(
            Action<ChocolateyConfiguration> confPatch = null,
            ChocoTestContext testcontext = ChocoTestContext.upgrade_testing_context,
            ChocoTestContext packagesContext = ChocoTestContext.packages_for_upgrade_testing,
            [CallerMemberName] string testFolder = ""
        )
        {
            string packageName = DoOperation(testcontext, confPatch, CommandNameType.upgrade, packagesContext, testFolder);

            WriteFileContext(Path.Combine(InstallContext.Instance.PackagesLocation, packageName, "tools", "console.exe"));
            WriteNupkgInfo(packageName);
        }

        void WriteNupkgInfo(string packageId)
        {
            WriteFileContext(Path.Combine(InstallContext.Instance.PackagesLocation, packageId, packageId + Constants.PackageExtension));
        }

        void WriteFileContext(string path)
        {
            string fileContent;

            if (!File.Exists(path))
            {
                return;
            }

            if (Path.GetExtension(path) == Constants.PackageExtension)
            {
                var pkg = new OptimizedZipPackage(path);
                var ver1 = pkg.Version.ToString();
                var ver2 = pkg.Version.Version.ToString();
                if (ver1 == ver2 || $"{ver1}.0" == ver2)
                {
                    fileContent = ver2;
                }
                else
                {
                    fileContent = $"{ver1} (normalized: {ver2})";
                }
            }
            else
            {
                fileContent = File.ReadAllText(path);
            }

            string shortPath = InstallContext.NormalizeMessage(path);
            LogService.console.Info($"{shortPath}: {fileContent}");
        }


        [LogTest]
        public void when_noop_upgrading_a_package_that_has_available_upgrades()
        {
            TestUpgrade((conf) =>
            {
                conf.Noop = true;
            });
        }

        [LogTest]
        public void when_noop_upgrading_a_package_that_does_not_have_available_upgrades()
        {
            TestUpgrade((conf) =>
            {
                conf.PackageNames = conf.Input = "installpackage";
                conf.Noop = true;
            });
        }

        [LogTest]
        public void when_noop_upgrading_a_package_that_does_not_exist()
        {
            TestUpgrade((conf) =>
            {
                conf.PackageNames = conf.Input = "nonexistingpackage";
                conf.Noop = true;
            });
        }

        [LogTest]
        public void when_upgrading_an_existing_package_happy_path()
        {
            TestUpgrade();
        }

        void TitleText(string message)
        {
            string slash = string.Concat(Enumerable.Repeat("-", (80 - message.Length) / 2));
            LogService.console.Info($"{slash} {message} {slash}");
        }

        [LogTest]
        public void when_upgrading_an_existing_package_with_prerelease_available_without_prerelease_specified()
        {
            TitleText("preinstall");
            Preinstall("1.1.0");

            TitleText("testupgrade");
            TestUpgrade((conf) =>
            {
                conf.Version = "1.1.0";
            }, ChocoTestContext.skipcontextinit);
        }

        [LogTest]
        public void when_upgrading_an_existing_package_with_prerelease_available_and_prerelease_specified()
        {
            TestUpgrade((conf) =>
            {
                conf.Prerelease = true;
            });
        }


        [LogTest]
        public void when_upgrading_an_existing_prerelease_package_without_prerelease_specified()
        {
            TitleText("preinstall");
            Preinstall("1.1.1-beta", true);

            TitleText("upgrade");
            TestUpgrade(null, ChocoTestContext.skipcontextinit);
        }

        [LogTest]
        public void when_upgrading_an_existing_prerelease_package_with_prerelease_available_with_excludeprelease_and_without_prerelease_specified()
        {
            TitleText("preinstall");
            Preinstall("1.1.1-beta", true);

            TitleText("upgrade");
            TestUpgrade((conf) => {
                conf.UpgradeCommand.ExcludePrerelease = true;
            }, ChocoTestContext.skipcontextinit);
        }

        [LogTest]
        public void when_upgrading_an_existing_prerelease_package_with_allow_downgrade_with_excludeprelease_and_without_prerelease_specified()
        {
            //Identical result to previous test
            TitleText("preinstall");
            Preinstall("1.1.1-beta", true);

            TitleText("upgrade");
            TestUpgrade((conf) => {
                conf.UpgradeCommand.ExcludePrerelease = true;
                conf.AllowDowngrade = true;
            }, ChocoTestContext.skipcontextinit);
        }

        [LogTest]
        public void when_force_upgrading_a_package()
        {
            TestUpgrade((conf) =>
            {
                conf.Force = true;
            });
        }

        [LogTest]
        public void when_upgrading_a_package_that_does_not_have_available_upgrades()
        {
            TestUpgrade((conf) =>
            {
                conf.PackageNames = conf.Input = "installpackage";
            });
        }

        [LogTest]
        public void when_force_upgrading_a_package_that_does_not_have_available_upgrades()
        {
            TestUpgrade((conf) =>
            {
                conf.PackageNames = conf.Input = "installpackage";
                conf.Force = true;
            });
        }

        [Test]
        public void when_upgrading_packages_with_packages_config()
        {
            Assert.Throws<ApplicationException>(() =>
            {
                TestUpgrade((conf) =>
                {
                    var confPath = Path.Combine(InstallContext.ApplicationInstallLocation, "context", "testing.packages.config");
                    conf.PackageNames = conf.Input = confPath;
                });
            });
        }

        [LogTest]
        public void when_upgrading_a_package_with_readonly_files()
        {
            TestUpgrade((conf) =>
            {
                var path = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                var fileSystem = new DotNetFileSystem();
                fileSystem.ensure_file_attribute_set(path, FileAttributes.ReadOnly);
            });
        }

        [LogTest]
        public void when_upgrading_a_package_with_a_read_and_delete_share_locked_file()
        {
            FileStream fileStream = null;
            try
            {
                TestUpgrade((conf) =>
                {
                    var path = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                    fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read | FileShare.Delete);
                });
            }
            finally
            {
                fileStream.Close();
            }
        }

        [LogTest]
        public void when_upgrading_a_package_with_an_exclusively_locked_file()
        {
            // graphical.exe.gui gets deleted - bug?    
            FileStream fileStream = null;
            try
            {
                TestUpgrade((conf) =>
                {
                    var path = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                    fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                });
            }
            finally
            {
                fileStream.Close();
            }
        }

        [LogTest]
        public void when_upgrading_a_package_with_added_files()
        {
            string path = null;
            TestUpgrade((conf) =>
            {
                path = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "dude.txt");
                File.WriteAllText(path, "hellow");
            });
            Assert.True(File.Exists(path));
        }

        [LogTest]
        public void when_upgrading_a_package_that_does_not_exist()
        {
            TestUpgrade((conf) =>
            {
                conf.PackageNames = conf.Input = "nonexistingpackage";
            });
        }

        [LogTest]
        public void when_upgrading_a_package_that_is_not_installed()
        {
            TestUpgrade((conf) =>
            {
                conf.PackageNames = conf.Input = "installpackage";
            }, ChocoTestContext.empty);
        }

        [LogTest]
        public void when_upgrading_a_package_that_errors()
        {
            TestUpgrade((conf) =>
            {
                conf.PackageNames = conf.Input = "badpackage";
            });
        }

        void TestDependencyUpgrade(
            Action<ChocolateyConfiguration> confPatch = null,
            ChocoTestContext packagesContext = ChocoTestContext.packages_for_dependency_testing10,
            ChocoTestContext testcontext = ChocoTestContext.isdependency_hasdependency,
            [CallerMemberName] string testFolder = ""
        )
        {
            Action<ChocolateyConfiguration> confDepPatch = (conf) =>
            {
                conf.PackageNames = conf.Input = "hasdependency";
                if (confPatch != null)
                {
                    confPatch(conf);
                }
            };

            DoOperation(testcontext, confDepPatch, 
                CommandNameType.upgrade, packagesContext, testFolder);

            WriteNupkgInfo("hasdependency");
            WriteNupkgInfo("isdependency");
            WriteNupkgInfo("isexactversiondependency");
        }


        [LogTest]
        public void when_upgrading_a_package_with_dependencies_happy()
        {
            TestDependencyUpgrade(null, ChocoTestContext.packages_for_dependency_testing10);
        }

        [LogTest]
        public void when_upgrading_a_package_with_unavailable_dependencies()
        {
            TestDependencyUpgrade(null, ChocoTestContext.packages_for_dependency_testing9);
        }

        [LogTest]
        public void when_upgrading_a_package_with_unavailable_dependencies_ignoring_dependencies()
        {
            TestDependencyUpgrade(
                (conf) => {
                    conf.IgnoreDependencies = true;
                }

                ,ChocoTestContext.packages_for_dependency_testing9
            );
        }

        [LogTest]
        public void when_upgrading_a_dependency_happy()
        {
            TestDependencyUpgrade(
                (conf) => {
                    conf.PackageNames = conf.Input = "isdependency";
                },

                ChocoTestContext.packages_for_dependency_testing11
            );
        }

        [LogTest]
        public void when_upgrading_a_dependency_legacy_folder_version()
        {
            TestDependencyUpgrade(
                (conf) => {
                    // This apparently a walkaround to get rid of "isdependency.1.0.0" folder.
                    // Bug, maybe should be fixed?
                    Directory.Delete(InstallContext.Instance.ChocolateyPackageInfoStoreLocation, true);

                    conf.PackageNames = conf.Input = "isdependency";
                },
                
                ChocoTestContext.packages_for_dependency_testing11,
                ChocoTestContext.isdependency_hasdependency_sxs
            );

            WriteNupkgInfo("hasdependency.1.0.0");
            WriteNupkgInfo("isexactversiondependency.1.0.0");
        }

    }
}

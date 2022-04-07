using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
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
    public class TestUpgradeCommand: LogTesting
    {
        public void TestUpgrade(
            Action<ChocolateyConfiguration> confPatch = null,
            [CallerMemberName] string testFolder = ""
        )
        {
            string packageName = null;
            Action<ChocolateyConfiguration> upgradePatch = (conf) =>
            {
                conf.CommandName = nameof(CommandNameType.upgrade);
                conf.PackageNames = conf.Input = "upgradepackage";

                if (confPatch != null)
                {
                    confPatch(conf);
                }

                packageName = conf.PackageNames;
            };
        
            InstallOn(ChocoTestContext.upgrade_testing_context, upgradePatch, 
                ChocoTestContext.packages_for_upgrade_testing,
                Path.Combine(nameof(TestUpgradeCommand), testFolder));

            WriteFileContext(Path.Combine(InstallContext.Instance.PackagesLocation, packageName, "tools", "console.exe"));
            WriteFileContext(Path.Combine(InstallContext.Instance.PackagesLocation, packageName, packageName + Constants.PackageExtension));
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
                fileContent = new OptimizedZipPackage(path).Version.Version.ToString();
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


    }
}


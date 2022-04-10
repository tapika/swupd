using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.filesystem;
using logtesting;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace chocolatey.tests2.commands
{
    [Parallelizable(ParallelScope.All), FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class TestUninstallCommand : LogTesting
    {
        public void TestUninstall(
            Action<ChocolateyConfiguration> confPatch = null,
            ChocoTestContext testcontext = ChocoTestContext.uninstall_testing_context,
            [CallerMemberName] string testFolder = ""
        )
        {
            Action<ChocolateyConfiguration> uninstallPatch = (conf) =>
            {
                conf.CommandName = CommandNameType.uninstall.ToString();
                conf.PackageNames = conf.Input = "installpackage";

                if (confPatch != null)
                {
                    confPatch(conf);
                }
            };

            ExecuteConf(testcontext, uninstallPatch, ChocoTestContext.empty,
                Path.Combine(nameof(TestUninstallCommand), testFolder));
        }

        [LogTest]
        public void when_noop_uninstalling_a_package()
        {
            TestUninstall((conf) =>
            {
                conf.Noop = true;
            });
        }

        [LogTest]
        public void when_noop_uninstalling_a_package_that_does_not_exist()
        {
            TestUninstall((conf) =>
            {
                conf.PackageNames = conf.Input = "somethingnonexisting";
                conf.Noop = true;
            });
        }

        [LogTest]
        public void when_uninstalling_a_package_happy_path()
        {
            TestUninstall();
        }

        [LogTest]
        public void when_force_uninstalling_a_package()
        {
            TestUninstall((conf) =>
            {
                conf.Force = true;
            });
        }

        [Test]
        public void when_uninstalling_packages_with_packages_config()
        {
            Assert.Throws<ApplicationException>(() =>
            {
                TestUninstall((conf) =>
                {
                    var confPath = Path.Combine(InstallContext.ApplicationInstallLocation, "context", "testing.packages.config");
                    conf.PackageNames = conf.Input = confPath;
                });
            });
        }

        [LogTest]
        public void when_uninstalling_a_package_with_readonly_files()
        {
            TestUninstall((conf) =>
            {
                var path = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                new DotNetFileSystem().ensure_file_attribute_set(path, FileAttributes.ReadOnly);
            });
        }

        [LogTest]
        public void when_uninstalling_a_package_with_a_read_and_delete_share_locked_file()
        {
            FileStream fileStream = null;
            try
            {
                TestUninstall((conf) =>
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
        public void when_uninstalling_a_package_with_an_exclusively_locked_file()
        {
            FileStream fileStream = null;
            try
            {
                TestUninstall((conf) =>
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
        public void when_uninstalling_a_package_with_added_files()
        {
            string path = null;

            TestUninstall((conf) =>
            {
                path = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "dude.txt");
                File.WriteAllText(path, "hellow");
            });

            Assert.True(File.Exists(path));
        }

        [LogTest]
        public void when_uninstalling_a_package_with_changed_files()
        {
            string path = null;

            TestUninstall((conf) =>
            {
                path = Path.Combine(InstallContext.Instance.PackagesLocation, conf.PackageNames, "tools", "chocolateyInstall.ps1");
                File.WriteAllText(path, "hellow");
            });

            Assert.True(File.Exists(path)); //should keep changed file
        }

    }
}


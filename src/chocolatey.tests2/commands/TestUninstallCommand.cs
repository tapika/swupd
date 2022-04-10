using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
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

    }
}


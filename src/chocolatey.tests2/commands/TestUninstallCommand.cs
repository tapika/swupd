﻿using chocolatey.infrastructure.app.configuration;
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

    }
}


using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.logging;
using chocolatey.tests.integration;
using logtesting;
using NUnit.Framework;
using System;
using System.Linq;

namespace chocolatey.tests2.commands
{
    [Parallelizable(ParallelScope.All)]
    public class TestPsRunCommand: LogTesting
    {
        protected ChocolateyPsRunCommand psruncmd;

        public TestPsRunCommand()
        {
            psruncmd = ApplicationManager.Instance.Commands.OfType<ChocolateyPsRunCommand>().Single();
        }

        public void PrepareBaseline(Action<ChocolateyConfiguration> confPatch, ChocoTestContext context = ChocoTestContext.install)
        {
            var conf = Scenario.baseline_configuration(true);
            InstallContext.Instance.RootLocation = PrepareTestFolder(context, conf);
            conf.Sources = InstallContext.Instance.PackagesLocation;
            conf.Input = conf.PackageNames = "installpackage";
            confPatch(conf);
            try
            {
                psruncmd.handle_validation(conf);
                LogService.console.Info($"> psrun install -s {conf.PsRunCommand.step}");
                psruncmd.run(conf);
            }
            catch (ApplicationException appex)
            { 
                LogService.console.Info($"ApplicationException: {appex.Message}");
            }
        }

        [LogTest]
        public void test_runs()
        {
            foreach (var value in Enum.GetValues(typeof(ChocolateyPsRunCommand.ExecuteStep)))
            {
                PrepareBaseline((conf) =>
                {
                    conf.PsRunCommand.step = value.ToString();
                });
            }
        }

        [LogTest]
        public void invalid_step()
        {
            PrepareBaseline((conf) =>
            {
                conf.PsRunCommand.step = "invalid_operation";
            });
        }

        [LogTest]
        public void multiple_packages()
        {
            PrepareBaseline((conf) =>
            {
                conf.Input = "";
            }, ChocoTestContext.installupdate);
        }

    }
}


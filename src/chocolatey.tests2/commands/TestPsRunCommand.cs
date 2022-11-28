using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.logging;
using chocolatey.tests.integration;
using logtesting;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

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


        /// <summary>
        /// Constructs command line from config
        /// </summary>
        /// <param name="config">config</param>
        /// <param name="switchPropertyPairs">property name, switch name pairs</param>
        /// <returns>command line</returns>
        string GetCommandLineFromConfig(object config, params string[] switchPropertyPairs)
        {
            var confType = config.GetType();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < switchPropertyPairs.Length; i += 2)
            {
                string propName = switchPropertyPairs[i];
                string switchName = switchPropertyPairs[i + 1];
                if (switchName == null) switchName = propName;

                var prop = confType.GetProperty(propName);
                object value = prop.GetValue(config);

                if (prop.PropertyType == typeof(bool))
                {
                    if (((bool)value) == false)
                    {
                        continue;
                    }
                }

                if (sb.Length != 0)
                    sb.Append(" ");

                if (prop.PropertyType == typeof(bool))
                {
                    sb.Append($"-{switchName}");
                    continue;
                }

                sb.Append($"-{switchName} {value}");
            }

            return sb.ToString();
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
                string cmdArgs = GetCommandLineFromConfig(conf.PsRunCommand,
                    nameof(PsRunCommandConfiguration.step), "s",
                    nameof(PsRunCommandConfiguration.keeptemp), null
                );
                LogService.console.Info($"> psrun install {cmdArgs}");
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
        public void test_runs_keep_temp()
        {
            string tempDir = null;
            
            PrepareBaseline((conf) =>
            {
                tempDir = Path.Combine(InstallContext.Instance.CacheLocation, ChocolateyPsRunCommand.PSRunTempFolder);
                InstallContext.Instance.PowerShellTempLocation = tempDir;
                conf.PsRunCommand.keeptemp = true;
            }, ChocoTestContext.installpackage3);

            string tempFile = "tempFile.txt";
            string tempPath = Path.Combine(tempDir, tempFile);

            LogService.console.Info($"{tempFile} contents: '{File.ReadAllText(tempPath).Trim()}'");
            File.Delete(tempPath);
            Directory.Delete(tempDir);
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


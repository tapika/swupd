using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.logging;
using chocolatey.tests.integration;
using logtesting;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace chocolatey.tests2.commands
{
    [Parallelizable(ParallelScope.All), FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class TestPackCommand: LogTesting
    {
        protected ChocolateyPackCommand packcmd;

        public TestPackCommand()
        {
            packcmd = ApplicationManager.Instance.Commands.OfType<ChocolateyPackCommand>().Single();
        }

        public void PackContext(
            Action<ChocolateyConfiguration> confPatch = null,
            string nuspecFile = "test1.nuspec",
            [CallerMemberName] string testFolder = "",
            [CallerFilePath] string sourcePath = ""
        )
        {
            var conf = Scenario.pack(true);
            InstallContext.Instance.RootLocation = PrepareTestFolder(ChocoTestContext.empty, conf, Path.Combine(nameof(TestPackCommand), testFolder));

            string rootDir = InstallContext.Instance.RootLocation;

            // ChocolateyPackCommand.run can also operate via Directory.SetCurrentDirectory
            // which does not work with multitasking.
            //Directory.SetCurrentDirectory(rootDir);

            string srcpath = CallerFilePathHelper.CallerFilePathToSolutionSourcePath(sourcePath);
            string srcdir = Path.GetDirectoryName(srcpath);
            string nuspecPath = Path.Combine(srcdir, nuspecFile);
            File.Copy(nuspecPath, Path.Combine(rootDir, nuspecFile), true);
            File.Copy(srcpath, Path.Combine(rootDir, "justsomefile.cs"), true);

            conf.Input = Path.Combine(rootDir, nuspecFile);
            conf.OutputDirectory = rootDir;

            var listBeforeUpdate = GetFileListing(rootDir);
            try
            {
                if (confPatch != null)
                {
                    confPatch(conf);
                }
                packcmd.run(conf);
            }
            catch (InvalidOperationException ex)
            {
                LogService.console.Info($"InvalidOperationException: {ex.Message}");
            }

            DisplayUpdates(listBeforeUpdate);
        }

        [LogTest]
        public void when_packing_without_specifying_an_output_directory()
        {
            PackContext();
        }

        [LogTest]
        public void when_packing_with_an_output_directory()
        {
            PackContext((conf) =>
            {
                conf.OutputDirectory = Path.Combine(InstallContext.Instance.RootLocation, "packageOutput");
                Directory.CreateDirectory(conf.OutputDirectory);
            });
        }

        [LogTest]
        public void when_packing_with_properties()
        {
            PackContext((conf) =>
                {
                    conf.Version = "0.1.0";
                    conf.PackCommand.Properties.Add("commitId", "1234abcd");
                    LogService.Instance.AddVerifyingLogRule<NugetService>();
                }
                , "test2.nuspec"
            );
        }
    }
}


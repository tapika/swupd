using Cake.Common.Tools.VSTest;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cakebuild.commands
{
    [TaskName(nameof(test))]
    [IsDependentOn(typeof(buildsolution))]
    public class test : FrostingTask<BuildContext>
    {
        ICakeLog log;

        public test(ICakeLog _log)
        {
            log = _log;
        }

        public void LogInfo(string value)
        {
            log.Information(value);
        }

        public override void Run(BuildContext context)
        {
            if (!context.cmdArgs.test.HasValue)
            {
                context.cmdArgs.test = false;
            }

            if (!context.cmdArgs.test.Value)
            {
                LogInfo($"--{nameof(CommandLineArgs.test)} not selected, skipping");
                return;
            }

            string rootDir = context.RootDirectory;
            string outDir = Path.Combine(rootDir, $@"src\bin\{context.cmdArgs.NetFramework}-Release");

            List<Cake.Core.IO.FilePath> paths = new List<Cake.Core.IO.FilePath>();
            var testsToRun = helpers.split(context.cmdArgs.testsToRun);
            foreach (var test in testsToRun)
            {
                string outPath = Path.Combine(outDir, $"{test}.dll");
                paths.Add(new Cake.Core.IO.FilePath(outPath));
            }

            string operation = (context.cmdArgs.DryRun) ? "Would test" : "Testing";
            bool enableCodeCoverage = (context.cmdArgs.codecoverage.HasValue) ?
                    context.cmdArgs.codecoverage.Value : false;
            string coverageEnabledStr = (enableCodeCoverage) ? "(code coverage enabled)" : "(code coverage disabled)";

            LogInfo($"> {operation} {string.Join(',', testsToRun)}... {coverageEnabledStr}");

            if (context.cmdArgs.DryRun)
            {
                return;
            }

            string coverageRunsettings = Path.Combine(rootDir, ".runsettings"); ;

            string testResultsDir = Path.Combine(rootDir, @"build_output\temp_codecoverage");

            // Directory must be cleaned and re-created, as *.coverage filename changes everytime
            if (Directory.Exists(testResultsDir))  Directory.Delete(testResultsDir, true);
            if (!Directory.Exists(testResultsDir)) Directory.CreateDirectory(testResultsDir);

            Environment.CurrentDirectory = outDir;

            var settings = new VSTestSettings
            {
                WorkingDirectory = outDir,
                //ArgumentCustomization = args =>
                //{
                //    args.Append(new TextArgument($"/help"));
                //    return args;
                //},
                ResultsDirectory = testResultsDir,
                EnableCodeCoverage = enableCodeCoverage,
                SettingsFile = coverageRunsettings
            };

            context.VSTest( paths, settings );
        }
    }

}


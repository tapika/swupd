using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.MSBuild;
using Cake.Common.Tools.DotNetCore.Test;
using Cake.Common.Tools.VSTest;
using Cake.Core.Diagnostics;
using Cake.Core.IO.Arguments;
using Cake.Coverlet;
using Cake.Frosting;
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
            string outPath = Path.Combine(outDir, "chocolatey.tests.dll");

            string operation = (context.cmdArgs.DryRun) ? "Would test" : "Testing";
            bool enableCodeCoverage = (context.cmdArgs.codecoverage.HasValue) ?
                    context.cmdArgs.codecoverage.Value : false;
            string coverageEnabledStr = (enableCodeCoverage) ? "(code coverage enabled)" : "(code coverage disabled)";

            LogInfo($"> {operation} {Path.GetFileName(outPath)}... {coverageEnabledStr}");

            if (context.cmdArgs.DryRun)
            {
                return;
            }

            string coverageRunsettings = Path.Combine(rootDir, ".runsettings"); ;

            string testResultsDir = Path.Combine(rootDir, @"build_output\temp_codecoverage");

            // Directory must be cleaned and re-created, as *.coverage filename changes everytime
            if (Directory.Exists(testResultsDir))  Directory.Delete(testResultsDir, true);
            if (!Directory.Exists(testResultsDir)) Directory.CreateDirectory(testResultsDir);


            var settings = new VSTestSettings
            {
                WorkingDirectory = testResultsDir,
                //ArgumentCustomization = args =>
                //{
                //    args.Append(new TextArgument($"/help"));
                //    return args;
                //},
                ResultsDirectory = testResultsDir,
                EnableCodeCoverage = enableCodeCoverage,
                SettingsFile = coverageRunsettings
            };

            context.VSTest(
                new string[] { outPath }.Select(x => new Cake.Core.IO.FilePath(x)),
                settings
            );
        }
    }

}


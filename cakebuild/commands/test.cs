using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Test;
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

            var testsToRun = helpers.split(context.cmdArgs.testsToRun);

            string operation = (context.cmdArgs.DryRun) ? "Would test" : "Testing";
            bool enableCodeCoverage = (context.cmdArgs.codecoverage.HasValue) ?
                    context.cmdArgs.codecoverage.Value : false;
            string coverageEnabledStr = (enableCodeCoverage) ? "(code coverage enabled)" : "(code coverage disabled)";

            LogInfo($"> {operation} {string.Join(',', testsToRun)}... {coverageEnabledStr}");

            if (context.cmdArgs.DryRun)
            {
                return;
            }

            string testResultsDir = context.TestResultsDirectory;

            // Directory must be cleaned and re-created, as *.coverage filename changes everytime
            if (Directory.Exists(testResultsDir)) Directory.Delete(testResultsDir, true);
            if (!Directory.Exists(testResultsDir)) Directory.CreateDirectory(testResultsDir);

            switch (context.cmdArgs.coverageMethod)
            {
                case "vs":
                    {
                        List<Cake.Core.IO.FilePath> paths = new List<Cake.Core.IO.FilePath>();
                        foreach (var test in testsToRun)
                        {
                            string outPath = Path.Combine(outDir, $"{test}.dll");
                            paths.Add(new Cake.Core.IO.FilePath(outPath));
                        }

                        string coverageRunsettings = Path.Combine(rootDir, ".runsettings"); ;


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

                        context.VSTest(paths, settings);
                    }
                    break;

                case "coverlet":
                    { 
                        string solutionPath = Path.Combine(rootDir, $@"src\chocolatey{context.cmdArgs.NetFrameworkSuffix}.sln");
                        string coverletSettings = Path.Combine(rootDir, "coverlet.runsettings");
                        List<string> slnOrCsprojToRun = new List<string>();

                        if (testsToRun.Contains("chocolatey.tests") && testsToRun.Contains("chocolatey.tests.integration"))
                        {
                            slnOrCsprojToRun.Add(solutionPath);
                        }
                        else
                        {
                            foreach (var test in testsToRun)
                            {
                                string csprojPath = Path.Combine(rootDir, $@"src\{test}\{test}.csproj");
                                slnOrCsprojToRun.Add(csprojPath);
                            }
                        }

                        Environment.SetEnvironmentVariable("MySolutionName", $"chocolatey{context.cmdArgs.NetFrameworkSuffix}");

                        foreach (var slnOrCsProj in slnOrCsprojToRun)
                        {
                            LogInfo($"Running test for {slnOrCsProj} {coverageEnabledStr}...");

                            var testSettings = new DotNetCoreTestSettings
                            {
                                OutputDirectory = outDir,
                                NoBuild = true,
                                Settings = coverletSettings,
                                Configuration = "Release",
                                ResultsDirectory = testResultsDir,
                                //Verbosity = Cake.Common.Tools.DotNet.DotNetVerbosity.Detailed
                            };

                            if (enableCodeCoverage)
                            {
                                testSettings.Collectors = new[] { "XPlat Code Coverage" };
                            }

                            context.DotNetCoreTest(slnOrCsProj, testSettings);
                        }
                    }
                    break;
                default:
                    throw new Exception($"Unknown coverage method: --{nameof(CommandLineArgs.coverageMethod)} {context.cmdArgs.coverageMethod}");
            }

        }
    }

}


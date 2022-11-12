using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNetCore.MSBuild;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using System.IO;

namespace cakebuild.commands
{
    [TaskName(nameof(buildsolution))]
    public class buildsolution : FrostingTask<BuildContext>
    {
        ICakeLog log;

        public buildsolution(ICakeLog _log)
        {
            log = _log;
        }

        public void LogInfo(string value)
        {
            log.Information(value);
        }

        public override void Run(BuildContext context)
        {
            if (!context.cmdArgs.build.HasValue)
            {
                context.cmdArgs.build = true;
            }

            if (!context.cmdArgs.build.Value)
            {
                LogInfo($"--{nameof(CommandLineArgs.build)} not selected, skipping");
                return;
            }

            string rootDir = context.RootDirectory;

            string[] solutions = new[] { 
                // Build tool first so it can be used for chocolatey
                @"src\shimgen\shimgen.sln", 
                $@"src\chocolatey{context.cmdArgs.NetFrameworkSuffix}.sln" };

            foreach (string solution in solutions)
            {
                string solutionPath = Path.Combine(rootDir, solution);

                if (context.cmdArgs.DryRun)
                {
                    LogInfo($"> would build {solutionPath}");
                    return;
                }

                LogInfo($"- Building {Path.GetFileName(solutionPath)}...");


                context.ConfigureDotNet(Path.Combine(rootDir, "src"));

                DotNetCoreMSBuildSettings msbuild_settings = new DotNetCoreMSBuildSettings()
                {
                    Verbosity = DotNetVerbosity.Minimal
                };

                context.DotNetBuild(solutionPath,
                    new DotNetBuildSettings
                    {
                        Configuration = "Release",
                        NoLogo = true,
                        MSBuildSettings = msbuild_settings
                    }
                );
            }
        }
    }

}


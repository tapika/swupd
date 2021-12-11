using Cake.Common.Tools.MSBuild;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cakebuild.commands
{
    [TaskName(nameof(buildexe))]
    public class buildexe : FrostingTask<BuildContext>
    {
        ICakeLog log;

        public buildexe(ICakeLog _log)
        {
            log = _log;
        }

        public string FileSize(string path)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(path).Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        public void LogInfo(string value)
        {
            log.Information(value);
        }

        public void BuildReadyToRun(BuildContext context, string os, string readytorun_target)
        {
            string rootDir = context.RootDirectory;
            string buildProject = readytorun_target;
            switch (readytorun_target)
            {
                case "choco":
                    buildProject = "chocolatey.console";
                    break;
                case "chocogui":
                    buildProject = "ChocolateyGui";
                    break;
            }
            string projectPath = Path.Combine(rootDir, $@"src\{buildProject}\{buildProject}.csproj"); ;

            string runtimeIdentifier = $"{os}-x64";
            string publishDir = Path.Combine(rootDir, $@"bin\publish_{runtimeIdentifier}_netcoreapp_3.1");
            string exeFile = (os == "linux") ? "choco" : "choco.exe";
            string outExe = Path.Combine(publishDir, exeFile);

            MSBuildSettings settings = new MSBuildSettings()
            {
                ToolVersion = MSBuildToolVersion.VS2019,
                MSBuildPlatform = MSBuildPlatform.x64,
                Verbosity = Verbosity.Minimal
            };

            Array.ForEach(new string[] { "restore", "build", "publish" }, (x) => { settings.Targets.Add(x); });

            Dictionary<string, string> d = new Dictionary<string, string>();
            // 2 next options are needed when compiling .csproj instead of .sln
            d["SolutionName"] = $"chocolatey{context.cmdArgs.NetFrameworkSuffix}";      // Ensure that project can set right targetframework
            d["SolutionDir"] = rootDir + "\\";

            d["DeployOnBuild"] = "true";
            d["Configuration"] = "Release";
            d["Platform"] = "Any CPU";
            d["PublishDir"] = publishDir;
            d["PublishProtocol"] = "FileSystem";
            d["RuntimeIdentifier"] = runtimeIdentifier;
            d["SelfContained"] = "true";
            d["PublishSingleFile"] = "true";
            d["PublishReadyToRun"] = "false";
            d["IncludeNativeLibrariesForSelfExtract"] = "true";
            d["IncludeAllContentForSelfExtract"] = "true";
            d["PublishTrimmed"] = "true";
            d[$"PUBLISH_{readytorun_target.ToUpper()}"] = "true";

            foreach (var k in d.Keys)
            {
                settings.Properties.Add(k, new[] { d[k] });
            }

            if (context.cmdArgs.DryRun)
            {
                LogInfo($"> msbuild.exe restore,build,publish ... with ");
                LogInfo(string.Join("\n", d.Select(x => $"  {x.Key}: {x.Value}")));
                LogInfo("");
            }
            else
            {
                LogInfo($"- Building {exeFile} for {os}...");
                context.MSBuild(projectPath, settings);
                LogInfo($"- {Path.GetFileName(outExe)} file size: {FileSize(outExe)}");
            }
        }

        public override void Run(BuildContext context)
        {
            if (!context.cmdArgs.r2r_build)
            {
                LogInfo($"--{nameof(CommandLineArgs.r2r_build)} not selected, skipping");
                return;
            }

            foreach (var os in helpers.split(context.cmdArgs.OSS) )
            {
                foreach (var readytorun_target in helpers.split(context.cmdArgs.r2r_targets))
                { 
                    BuildReadyToRun(context, os.Trim().ToLower(), readytorun_target.Trim().ToLower());
                }
            }
        }
    }

}


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
        public override bool ShouldRun(BuildContext context)
        {
            return context.commandLineArguments.buildexe;
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

        public override void Run(BuildContext context)
        {
            string rootDir = context.RootDirectory;
            string solutionPath = Path.Combine(rootDir, $@"src\chocolatey{context.commandLineArguments.NetFrameworkSuffix}.sln");
            string projectPath = solutionPath;
            string runtimeIdentifier = $"{context.commandLineArguments.OS}-x64";
            string publishDir = Path.Combine(rootDir, $@"bin\publish_{runtimeIdentifier}_netcoreapp_3.1");
            string outExe = Path.Combine(publishDir, "choco.exe");

            MSBuildSettings settings = new MSBuildSettings()
            {
                ToolVersion = MSBuildToolVersion.VS2019,
                MSBuildPlatform = MSBuildPlatform.x64,
                Verbosity = Cake.Core.Diagnostics.Verbosity.Minimal
            };

            Array.ForEach(new string[] { "restore", "build", "publish" }, (x) => { settings.Targets.Add(x); });

            Dictionary<string, string> d = new Dictionary<string, string>();

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
            d["PUBLISH_CHOCO"] = "true";

            foreach (var k in d.Keys)
            {
                settings.Properties.Add(k, new[] { d[k] });
            }

            context.MSBuild(projectPath, settings);
            context.Log.Information($"- {Path.GetFileName(outExe)} file size: {FileSize(outExe)}");
        }
    }

}


using Cake.Common.Tools.Chocolatey;
using Cake.Common.Tools.Chocolatey.Pack;
using Cake.Core.Diagnostics;
using Cake.Core.IO.Arguments;
using Cake.Frosting;
using System;
using System.IO;
using System.Linq;

namespace cakebuild.commands
{
    [TaskName(nameof(choco_pack))]
    [IsDependentOn(typeof(test))]
    public class choco_pack : FrostingTask<BuildContext>
    {
        ICakeLog log;

        public choco_pack(ICakeLog _log)
        {
            log = _log;
        }

        public void LogInfo(string value)
        {
            log.Information(value);
        }

        public override void Run(BuildContext context)
        {
            if (!context.cmdArgs.nuget.HasValue)
            {
                context.cmdArgs.nuget = false;
            }

            if (!context.cmdArgs.nuget.Value)
            {
                LogInfo($"--{nameof(CommandLineArgs.nuget)} not selected, skipping");
                return;
            }

            string rootDir = context.RootDirectory;

            string publishNugetDir = Path.Combine(rootDir, "artifacts-nuget");

            if (!Directory.Exists(publishNugetDir))
            {
                Directory.CreateDirectory(publishNugetDir);
            }
            
            string nuspecFilePath = Path.Combine(rootDir, "nuspec", "chocolatey", "choco", "choco.nuspec");
            string tempDir = Path.Combine(rootDir, "bin", "temp");
            string binDir = Path.Combine(rootDir, $@"bin\{context.cmdArgs.NetFramework}-Release");

            string path = Environment.GetEnvironmentVariable("PATH");
            if (!path.StartsWith(binDir))
            { 
                Environment.SetEnvironmentVariable("PATH", $"{binDir};{path}");
            }

            ChocolateyPackSettings packSettings = new ChocolateyPackSettings()
            {
                OutputDirectory = publishNugetDir,
                ArgumentCustomization = builder =>
                {
                    new string[] { 
                        "--root", tempDir, 
                        "--in", binDir 
                    }.Select(x => new TextArgument(x)).ToList().ForEach(x => builder.Append(x));
                    //builder.Append($"choco_build_tag=\"{branchName} \"");
                    //string branchNameSuffix = (branchName.Length != 0) ? $"-{branchName}" : "";
                    //builder.Append($"choco_build_tag_suffix=\"{branchNameSuffix} \"");
                    return builder;
                }
            };
            context.ChocolateyPack(nuspecFilePath, packSettings);

        }
    }

}


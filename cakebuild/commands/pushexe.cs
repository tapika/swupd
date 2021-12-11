using Cake.Common;
using Cake.Common.Tools.MSBuild;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cakebuild.commands
{
    [TaskName(nameof(pushexe))]
    [IsDependentOn(typeof(buildexe))]
    public class pushexe : FrostingTask<BuildContext>
    {
        const string repositoryName = "swupd";
        
        ICakeLog log;

        public pushexe(ICakeLog _log)
        {
            log = _log;
        }

        public override bool ShouldRun(BuildContext context)
        {
            return context.cmdArgs.push_r2r;
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


            if (context.cmdArgs.DryRun)
            {
                //LogInfo($"> msbuild.exe restore,build,publish ... with ");
                //LogInfo(string.Join("\n", d.Select(x => $"  {x.Key}: {x.Value}")));
                //LogInfo("");
            }
            else
            {
                string authToken = context.EnvironmentVariable("API_KEY");
                if (string.IsNullOrEmpty(authToken))
                {
                    string apiKeyFile = @"c:\Private\github_apikey.txt";
                    if (File.Exists(apiKeyFile))
                    {
                        authToken = File.ReadAllText(apiKeyFile).Trim('\n', ' ');
                    }
                }

                GitHubClient ghClient = new GitHubClient(new ProductHeaderValue("CakeBuilder"))
                {
                    Credentials = new Credentials(authToken)
                };
                
                var timeout = TimeSpan.FromMinutes(20);
                ghClient.Connection.SetRequestTimeout(timeout);
                ghClient.Connection.SendDataTimeout = timeout;
                //Repository r = ghClient.Repository.Get("tapika", repositoryName).Result;

                //LogInfo($"git repository id: {r.Id}");
                
                //LogInfo($"- Building {exeFile} for {os}...");
                //context.MSBuild(projectPath, settings);
                //LogInfo($"- {Path.GetFileName(outExe)} file size: {FileSize(outExe)}");
            }
        }


        async public override void Run(BuildContext context)
        {
            if (!context.cmdArgs.push_r2r)
            {
                LogInfo("--push_r2r not selected, skipping");
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


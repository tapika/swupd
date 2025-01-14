﻿using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Tools.MSBuild;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Git;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public void LogInfo(string value)
        {
            log.Information(value);
        }

        public void PushRelease(BuildContext context, string releaseTag, List<string> files, string branchName)
        {
            bool isRelease = branchName == "master";
            if (!isRelease)
            {
                releaseTag += "_beta";
            }

            if (context.cmdArgs.DryRun)
            {
                LogInfo($"Would create new release '{releaseTag}' with following files:");
                LogInfo(string.Join("\n", files.Select(x => $"  {x}")));
                return;
            }

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

            string owner = "tapika";

            Repository repo = ghClient.Repository.Get(owner, repositoryName).Result;
            Release release;
            try
            {
                release = ghClient.Repository.Release.Get(repo.Id, releaseTag).Result;
                LogInfo($"- deleting release '{releaseTag}'");
                ghClient.Repository.Release.Delete(repo.Id, release.Id);
            }
            catch (AggregateException ex)
            {
                LogInfo($"- note: Release '{releaseTag}' does not exists ('{ex.Message}')");
            }

            var commitId = context.GitLogTip(context.RootDirectory).Sha;
            LogInfo($"- Last commit id: {commitId}");

            var newRelease = new NewRelease(releaseTag)
            {
                Name = "",
                Prerelease = !isRelease,
                TargetCommitish = commitId
            };

            release = null;
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    if (retry == 0)
                    {
                        LogInfo($"- creating new release '{releaseTag}'");
                    }
                    else
                    { 
                        LogInfo($"- Retrying ({retry + 1}) creating new release '{releaseTag}'");
                    }
                    release = ghClient.Repository.Release.Create("tapika", repositoryName, newRelease).Result;
                    break;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }
                    LogInfo($"- failed to create release: '{ex.Message}' ({ex.GetType().FullName})");
                    Thread.Sleep(1500);
                }
            }

            if (release == null)
            {
                LogInfo($"- Failed to create new release, aborting gracefully.");
                return;
            }

            List<Task> tasks = new List<Task>();
            
            foreach (var file in files)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using (var fileStream = File.OpenRead(file))
                    {
                        LogInfo($"- Uploading {Path.GetFileName(file)}... ");
                        var newAsset = new ReleaseAssetUpload
                        {
                            FileName = Path.GetFileName(file),
                            ContentType = "application/octet-stream",
                            RawData = fileStream
                        };

                        await ghClient.Repository.Release.UploadAsset(release, newAsset);
                    }
                }
                ));
            }

            LogInfo("waiting for all uploads to complete...");
            Task.WaitAll(tasks.ToArray());
            LogInfo("uploads completed.");
        }


        public override void Run(BuildContext context)
        {
            var r2r_push = context.cmdArgs.r2r_push;

            // cannot use --depth / fetch-depth if need to get tags from git
            //string currentGitTag = context.GitDescribe(context.RootDirectory, GitDescribeStrategy.Tags).Split('-')[0];
            string currentReleaseTag = "1.0";
            string branchName = context.cmdArgs.branch;

            if (String.IsNullOrEmpty(branchName))
            {
                branchName = context.GitBranchCurrent(context.RootDirectory).FriendlyName;
            }

            if (!r2r_push.HasValue)
            {
                var isLocalBuild = context.BuildSystem().IsLocalBuild;
                r2r_push = !isLocalBuild;
                LogInfo($"{nameof(pushexe)}: isLocalBuild: {isLocalBuild}: => will push: {r2r_push.Value}");
            }

            if (!r2r_push.Value)
            {
                LogInfo($"--{nameof(CommandLineArgs.r2r_push)} not selected, skipping");
                return;
            }

            List<string> files = new List<string>();

            foreach (var os in helpers.split(context.cmdArgs.OSS).Select(x => x.Trim().ToLower()) )
            {
                foreach (var readytorun_target in helpers.split(context.cmdArgs.r2r_targets).Select(x => x.Trim().ToLower()))
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
                    string publishDir = Path.Combine(rootDir, $@"bin\publish_{runtimeIdentifier}_{context.cmdArgs.NetFramework}");
                    string exeFile = (os == "linux") ? "choco" : "choco.exe";
                    string outExe = Path.Combine(publishDir, exeFile);

                    if (!File.Exists(outExe))
                    {
                        throw new Exception($"File '{outExe}' does not exists");
                    }

                    files.Add(outExe);
                }
            }

            PushRelease(context, currentReleaseTag, files, branchName);
        }
    }

}


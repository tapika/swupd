using Cake.Common.Build;
using Cake.Core.Diagnostics;
using Cake.Coveralls;
using Cake.Frosting;
using Cake.Git;
using System;
using System.IO;
using System.Linq;

namespace cakebuild.commands
{
    [TaskName(nameof(upload_coverage_results))]
    [IsDependentOn(typeof(process_test_results))]
    public class upload_coverage_results : FrostingTask<BuildContext>
    {
        ICakeLog log;

        public upload_coverage_results(ICakeLog _log)
        {
            log = _log;
        }

        public void LogInfo(string value)
        {
            log.Information(value);
        }

        public override bool ShouldRun(BuildContext context)
        {
            if (!context.cmdArgs.test.HasValue || !context.cmdArgs.test.Value || 
                !context.cmdArgs.codecoverage.HasValue || !context.cmdArgs.codecoverage.Value)
            {
                return false;
            }

            return true;
        }

        public override void Run(BuildContext context)
        {
            string rootDir = context.RootDirectory;
            string coverageHtml = System.IO.Path.Combine(rootDir, @"build_output\build_artifacts\codecoverage\Html");
            string coverageXml = Path.Combine(coverageHtml, "Cobertura.xml");

            string repoTokenFile = @"c:\Private\coveralls_io_token.txt";
            string repoToken = null;
            if (File.Exists(repoTokenFile))
            {
                repoToken = File.ReadAllText(repoTokenFile).Trim('\n', ' ');
            }

            if (!string.IsNullOrEmpty(repoToken))
            {
                Environment.SetEnvironmentVariable("COVERALLS_REPO_TOKEN", repoToken);
            }

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("COVERALLS_REPO_TOKEN")))
            {
                LogInfo(@"Environment variable 'COVERALLS_REPO_TOKEN' not set and c:\Private\coveralls_io_token.txt not available - " +
                    "coverage report upload skipped");
                return;
            }

            if (!context.cmdArgs.uploadCoverageResults.HasValue || !context.cmdArgs.uploadCoverageResults.Value)
            {
                LogInfo($"--{nameof(CommandLineArgs.uploadCoverageResults)} not selected, skipping");
                return;
            }

            string runId = "1";
            string commitId = null;
            string commitMessage = null;
            string branch = null;

            if (context.BuildSystem().IsRunningOnGitHubActions)
            {
                var workflow = context.BuildSystem().GitHubActions.Environment.Workflow;
                runId = workflow.RunId;
                commitId = workflow.Sha;
                branch = workflow.Ref;
            }
            
            if (string.IsNullOrEmpty(commitId) || string.IsNullOrEmpty(commitMessage))
            {
                var logTip = context.GitLogTip(context.RootDirectory);

                if (string.IsNullOrEmpty(commitId))
                {
                    commitId = logTip.Sha;
                }

                if (string.IsNullOrEmpty(commitMessage))
                {
                    commitMessage = logTip.Message;
                }
            }

            if (string.IsNullOrEmpty(branch))
            { 
                branch = context.GitBranchCurrent(context.RootDirectory).CanonicalName;
            }

            var branchParts = branch.Split('/');
            if (branchParts.Length >= 2)
            {
                branch = branchParts.Last();
            }

            // https://github.com/coveralls-net/coveralls.net.git
            // Detects upload information via Appveyor environment variables. Of course if we are running on custom build machine
            // or in github actions - none of environment variables are present. We mimic here Appveyor behavior.
            Environment.SetEnvironmentVariable("APPVEYOR_JOB_ID", runId);
            Environment.SetEnvironmentVariable("APPVEYOR", "true");
            Environment.SetEnvironmentVariable("APPVEYOR_REPO_COMMIT", commitId);
            Environment.SetEnvironmentVariable("APPVEYOR_REPO_COMMIT_MESSAGE", commitMessage);
            Environment.SetEnvironmentVariable("APPVEYOR_REPO_BRANCH", branch);

            LogInfo($"runId: {runId}, branchRef: {branch}, commitId: {commitId}");
            LogInfo($"commitMessage: '{commitMessage}'");

            LogInfo($"Uploading coverage report {Path.GetFileName(coverageXml)} to coveralls.io...");

            context.CoverallsIo(new Cake.Core.IO.FilePath(coverageXml), new CoverallsIoSettings()
                {
                    ParseType = CoverageParseType.cobertura
                }
            );
        }
    }
}

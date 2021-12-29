using Cake.Common.Build;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using cakebuild.CodeCoverageTool;
using System;
using System.IO;
using System.Linq;

namespace cakebuild.commands
{
    [TaskName(nameof(process_test_results))]
    [IsDependentOn(typeof(test))]
    public class process_test_results : FrostingTask<BuildContext>
    {
        ICakeLog log;

        public process_test_results(ICakeLog _log)
        {
            log = _log;
        }

        public void LogInfo(string value)
        {
            log.Information(value);
        }

        public override bool ShouldRun(BuildContext context)
        {
            if (!context.cmdArgs.test.HasValue || !context.cmdArgs.test.Value)
            {
                return false;
            }

            return true;
        }

        public override void Run(BuildContext context)
        {
            bool enableCodeCoverage = (context.cmdArgs.codecoverage.HasValue) ? context.cmdArgs.codecoverage.Value : false;

            if (!enableCodeCoverage)
            {
                LogInfo($"--{nameof(CommandLineArgs.codecoverage)} not selected, skipping");
                return;
            }

            string rootDir = context.RootDirectory;
            string testResultsDir = context.TestResultsDirectory;

            ReportGeneratorSettings coverageSettings = new ReportGeneratorSettings();
            foreach (string covFormatStr in helpers.split(context.cmdArgs.coverageFormats))
            {
                var covFormat = (ReportGeneratorReportType)Enum.Parse(typeof(ReportGeneratorReportType), covFormatStr);
                coverageSettings.ReportTypes.Add(covFormat);
            }

            string coverageHtml = System.IO.Path.Combine(rootDir, @"build_output\build_artifacts\codecoverage\Html");
            if (Directory.Exists(coverageHtml)) Directory.Delete(coverageHtml, true);

            switch (context.cmdArgs.coverageMethod)
            {
                case "vs":
                    {
                        string coveragePath = Directory.GetFiles(testResultsDir, "*.coverage", SearchOption.AllDirectories).First();
                        string coverageXml = Path.Combine(testResultsDir, "vs_coverage.xml");

                        if (File.Exists(coverageXml)) File.Delete(coverageXml);

                        LogInfo($"Converting *.coverage to vs_coverage.xml file format...");
                        context.ConvertCoverageReport(coveragePath, coverageXml);

                        LogInfo($"Generating report in {coverageHtml}...");

                        context.ReportGenerator(
                            new Cake.Core.IO.GlobPattern(coverageXml),
                            new Cake.Core.IO.DirectoryPath(coverageHtml),
                            coverageSettings);
                    }
                    break;

                case "coverlet":
                    {
                        LogInfo($"Generating report in {coverageHtml}...");

                        context.ReportGenerator(
                            new Cake.Core.IO.GlobPattern($@"{testResultsDir}\**\*.opencover.xml"),
                            new Cake.Core.IO.DirectoryPath(coverageHtml),
                            coverageSettings);
                    }
                    break;

                default:
                    throw new Exception($"Unknown coverage method: --{nameof(CommandLineArgs.coverageMethod)} {context.cmdArgs.coverageMethod}");
            }

            // Upload to report to GitHub if running in there.
            if (context.BuildSystem().IsRunningOnGitHubActions)
            {
                // Coverage summary
                string htmlSummaryPath = Path.Combine(coverageHtml, "summary.html");
                if (File.Exists(htmlSummaryPath))
                {
                    LogInfo($"Uploading code coverage test results, 1 ({htmlSummaryPath})...");
                    context.BuildSystem().GitHubActions.Commands.UploadArtifact(new Cake.Core.IO.FilePath(htmlSummaryPath), "CoverageShortSummary").GetAwaiter().GetResult();
                    File.Delete(htmlSummaryPath);

                    htmlSummaryPath = Path.Combine(coverageHtml, "summary.htm");
                    if (File.Exists(htmlSummaryPath)) File.Delete(htmlSummaryPath);
                }

                //Entire coverage report
                LogInfo($"Uploading code coverage test results, 2 ({coverageHtml})...");
                context.BuildSystem().GitHubActions.Commands.UploadArtifact(new Cake.Core.IO.DirectoryPath(coverageHtml), "CoverageFullReport").GetAwaiter().GetResult();
            }
        }
    }
}

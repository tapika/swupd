using Cake.Common.Tools.ReportGenerator;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using cakebuild.CodeCoverageTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        public override void Run(BuildContext context)
        {
            string rootDir = context.RootDirectory;
            string testResultsDir = Path.Combine(rootDir, @"build_output\temp_codecoverage");
            string coveragePath = Directory.GetFiles(testResultsDir, "*.coverage", SearchOption.AllDirectories).First();
            string coverageXml = Path.Combine(testResultsDir, "cobertura_coverage.xml");

            if (File.Exists(coverageXml)) File.Delete(coverageXml);
            
            LogInfo($"Converting *.coverage to *.cobertura_coverage.xml file format...");
            context.ConvertCoverageReport(coveragePath, coverageXml);

            string coverageHtml = System.IO.Path.Combine(testResultsDir, "html");

            if (Directory.Exists(coverageHtml)) Directory.Delete(coverageHtml, true);

            LogInfo($"Generating report in {coverageHtml}...");

            context.ReportGenerator(new Cake.Core.IO.GlobPattern(coverageXml), new Cake.Core.IO.DirectoryPath(coverageHtml));
        }
    }
}

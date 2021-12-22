using Cake.Core;
using Cake.Core.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace cakebuild.CodeCoverageTool
{
    public static class CodeCoverageAliases
    {
        [CakeMethodAlias]
        public static void ConvertCoverageReport(this ICakeContext context, string coveragePath, string outputPath)
        {
            var settings = new CodeCoverageSettings()
            {
                OutputPath = outputPath,
                CoveragePath = coveragePath
            };

            new CodeCoverageRunner(context).Run(settings);
        }
    }
}

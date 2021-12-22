using Cake.Core.Tooling;
using System;
using System.Collections.Generic;
using System.Text;

namespace cakebuild.CodeCoverageTool
{
    public class CodeCoverageSettings : ToolSettings
    {
        public CodeCoverageCommand command { get; set; } = CodeCoverageCommand.analyze;
        public string CoveragePath { get; set; }
        public string OutputPath { get; set; }
    }
}

using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Tooling;
using System.Collections.Generic;

namespace cakebuild.CodeCoverageTool
{
    public class CodeCoverageRunner : Tool<CodeCoverageSettings>
    {
        public CodeCoverageRunner(ICakeContext context) :
            base(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools)
        { 
        }

        private ProcessArgumentBuilder GetArguments(CodeCoverageSettings settings)
        {
            var builder = new ProcessArgumentBuilder();
            builder.Append(settings.command.ToString());
            builder.AppendSwitchQuoted($"/output", ":", settings.OutputPath);
            builder.AppendQuoted(settings.CoveragePath);
            return builder;
        }

        public void Run(CodeCoverageSettings settings)
        { 
            Run(settings, GetArguments(settings));

        }

        protected override IEnumerable<string> GetToolExecutableNames()
        {
            return new[] { "CodeCoverage.exe" };
        }

        protected override string GetToolName()
        {
            return "CodeCoverage";
        }
    }
}

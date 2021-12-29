using Cake.Core;
using Cake.Frosting;
using System.IO;
using System.Reflection;

namespace cakebuild
{
    public class BuildContext : FrostingContext
    {
        public CommandLineArgs cmdArgs;

        public BuildContext(ICakeContext context, CommandLineArgs _commandLineArguments) : base(context)
        {
            cmdArgs = _commandLineArguments;
        }

        public string RootDirectory
        {
            get
            {
                string rootDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\.."));
                return rootDir;
            }
        }

        public string TestResultsDirectory
        { 
            get
            { 
                return Path.Combine(RootDirectory, $@"build_output\temp_codecoverage_{cmdArgs.coverageMethod}");
            }
        }
    }
}


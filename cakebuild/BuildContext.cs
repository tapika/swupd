using Cake.Core;
using Cake.Frosting;
using System.IO;
using System.Reflection;

namespace cakebuild
{
    public class BuildContext : FrostingContext
    {
        public CommandLineArgs commandLineArguments;

        public BuildContext(ICakeContext context, CommandLineArgs _commandLineArguments) : base(context)
        {
            commandLineArguments = _commandLineArguments;
        }

        public string RootDirectory
        {
            get
            {
                string rootDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\.."));
                return rootDir;
            }
        }

    }
}


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

        public static string GetRootDirectory()
        {
            string rootDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\.."));
            return rootDir;
        }

        public string RootDirectory
        {
            get
            {
                return GetRootDirectory();
            }
        }

        public string TestResultsDirectory
        { 
            get
            { 
                return Path.Combine(RootDirectory, $@"build_output\temp_codecoverage_{cmdArgs.coverageMethod}");
            }
        }

        public void ConfigureDotNet(string dir)
        {
            string globalJson = Path.Combine(dir, "global.json");
            string versionToRequest = null;
            switch (cmdArgs.NetFramework)
            {
                case "netcoreapp3.1":
                    versionToRequest = "3.1.101";
                    break;
                case "net5.0":
                    versionToRequest = "5.0.406";
                    break;
                case "net6.0":
                    versionToRequest = "6.0.0";
                    break;
            }

            if (versionToRequest == null)
            {
                if (File.Exists(globalJson))
                {
                    File.Delete(globalJson);
                }
            }
            else
            {
                var content = $@"{{
  ""sdk"": {{
    ""version"": ""{versionToRequest}"",
    ""rollForward"": ""latestFeature""
  }}
}}
";

                if (!File.Exists(globalJson) || File.ReadAllText(globalJson) != content)
                {
                    File.WriteAllText(globalJson, content);
                }
            }
        }


    }
}


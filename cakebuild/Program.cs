using Cake.Frosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace cakebuild
{
    class Program
    {
        static int Main(string[] _args)
        {
            CommandLineArgs args = CommandLineArgumentParser.Parse(_args);

            var host = new CakeHost();
            host.UseContext<BuildContext>();
            host.ConfigureServices((config) =>
            {
                config.AddSingleton(args);
            }
            );
            // Needed when using Visual Studio built-in code coverage tools
            host.InstallTool(new Uri("nuget:?package=Microsoft.CodeCoverage&version=17.0.0"));

            // Needed when using .ReportGenerator(...) call
            host.InstallTool(new Uri("nuget:?package=ReportGenerator&version=5.0.0"));

            // Needed for uploading coverage test result to coveralls.io
            host.InstallTool(new Uri("nuget:?package=coveralls.io&version=1.4.2"));
            //host.InstallTool(new Uri("nuget:?package=coveralls.io&version=1.1.86"));

            // Can be also installed globally
            // dotnet tool install --global coverlet.console --version 3.1.0
            host.InstallTool(new Uri("dotnet:?package=coverlet.console&version=3.1.0"));
            host.InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.13.0"));
            //host.InstallTool(new Uri("nuget:?package=coveralls.io&version=1.1.86"));
            //host.InstallTool(new Uri("nuget:?package=ReportGenerator&version=5.0.0"));
            return host.Run(args.GetCakeArgs());
        }
    }
}


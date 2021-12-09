using Cake.Frosting;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            //host.InstallTool(new Uri("nuget:?package=coveralls.io&version=1.1.86"));
            //host.InstallTool(new Uri("nuget:?package=ReportGenerator&version=5.0.0"));
            return host.Run(args.GetCakeArgs());
        }
    }
}


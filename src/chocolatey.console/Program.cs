﻿// Copyright © 2017 - 2021 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using chocolatey.infrastructure.app.commands;
    using infrastructure.app;
    using infrastructure.app.builders;
    using infrastructure.app.configuration;
    using infrastructure.app.runners;
    using infrastructure.commandline;
    using infrastructure.configuration;
    using infrastructure.extractors;
    using infrastructure.logging;
    using infrastructure.registration;
    using infrastructure.tolerance;
    using resources;
    using Assembly = infrastructure.adapters.Assembly;
    using Console = System.Console;
    using Environment = System.Environment;
    using IFileSystem = infrastructure.filesystem.IFileSystem;

    public sealed class Program
    {
        // ReSharper disable InconsistentNaming
        private static void Main(string[] args)
        // ReSharper restore InconsistentNaming
        {
            try
            {
                Action failLogInit = () =>
                {
                    Log.InitializeWith<NLogLog>();
                    LogService.Instance.configure();
                };

                var config = Config.get_configuration_settings();
                if (new ChocolateyOptionSet().Parse(args, new ChocolateyStartupCommand(), config, failLogInit))
                {
                    Environment.Exit(config.UnsuccessfulParsing ? 1 : 0);
                }

                string loggingLocation = ApplicationParameters.LoggingLocation;
                if (!Directory.Exists(loggingLocation)) Directory.CreateDirectory(loggingLocation);

                LogService.Instance.configure(loggingLocation);
                Bootstrap.initialize();
                Bootstrap.startup();
                //LogService.Test();
                var container = SimpleInjectorContainer.Container;

                "LogFileOnly".Log().Info(() => "".PadRight(60, '='));

                var fileSystem = container.GetInstance<IFileSystem>();

                var warnings = new List<string>();

                if (!ConfigurationBuilder.set_up_configuration(
                     args,
                     config,
                     container,
                     warning => { warnings.Add(warning); }
                     ))
                { 
                    Environment.Exit(config.UnsuccessfulParsing ? 1 : 0);
                }

                if (config.Features.LogWithoutColor)
                {
                    LogService.Instance.enableColors(false);
                }

                if (!string.IsNullOrWhiteSpace(config.AdditionalLogFileLocation))
                {
                    LogService.Instance.configureAdditionalLogFile(fileSystem.get_full_path(config.AdditionalLogFileLocation));
                }

                report_version_and_exit_if_requested(args, config);

                trap_exit_scenarios(config);

                warn_on_nuspec_or_nupkg_usage(args, config);

                if (config.RegularOutput)
                {
                    "chocolatey".Log().Info(ChocolateyLoggers.Important, () => "{0} v{1}".format_with(ApplicationParameters.Name, config.Information.ChocolateyProductVersion));
                    if (args.Length == 0)
                    {
                        "chocolatey".Log().Info(ChocolateyLoggers.Important, () => "Please run 'choco -?' or 'choco <command> -?' for help menu.");
                    }
                }

                if (warnings.Count != 0 && config.RegularOutput)
                {
                    foreach (var warning in warnings.or_empty_list_if_null())
                    {
                        "chocolatey".Log().Warn(ChocolateyLoggers.Important, warning);
                    }
                }

                if (config.HelpRequested || config.UnsuccessfulParsing)
                {
                    Environment.Exit(config.UnsuccessfulParsing ? 1 : 0);
                }

                LogService.Instance.adjustLogLevels(config.Debug, config.Verbose, config.Trace);
                "chocolatey".Log().Debug(() => "{0} is running on {1} v {2}".format_with(ApplicationParameters.Name, config.Information.PlatformType, config.Information.PlatformVersion.to_string()));
                //"chocolatey".Log().Debug(() => "Command Line: {0}".format_with(Environment.CommandLine));

                remove_old_chocolatey_exe(fileSystem);

                AssemblyFileExtractor.extract_all_resources_to_relative_directory(fileSystem, Assembly.GetAssembly(typeof(Program)), ApplicationParameters.InstallLocation, new List<string>(), "chocolatey.console", throwError:false);
                //refactor - thank goodness this is temporary, cuz manifest resource streams are dumb
                IList<string> folders = new List<string>
                    {
                        "helpers",
                        "functions",
                        "redirects",
                        "tools"
                    };
                AssemblyFileExtractor.extract_all_resources_to_relative_directory(fileSystem, Assembly.GetAssembly(typeof(ChocolateyResourcesAssembly)), ApplicationParameters.InstallLocation, folders, ApplicationParameters.ChocolateyFileResources, throwError: false);
                var application = new ConsoleApplication();
                application.run(args, config, container);
            }
            catch (Exception ex)
            {
                if (ApplicationParameters.is_debug_mode_cli_primitive())
                {
                    "chocolatey".Log().Error(() => "{0} had an error occur:{1}{2}".format_with(
                        ApplicationParameters.Name,
                        Environment.NewLine,
                        ex.ToString()));
                }
                else
                {
                    "chocolatey".Log().Error(ChocolateyLoggers.Important, () => "{0}".format_with(ex.Message));
                    "chocolatey".Log().Error(ChocolateyLoggers.LogFileOnly, () => "More Details: {0}".format_with(ex.ToString()));
                }

                if (Environment.ExitCode == 0) Environment.ExitCode = 1;
            }
            finally
            {
                "chocolatey".Log().Debug(() => "Exiting with {0}".format_with(Environment.ExitCode));
#if DEBUG || RELWITHDEBINFO
                "chocolatey".Log().Info(() => "Exiting with {0}".format_with(Environment.ExitCode));
#endif
                Bootstrap.shutdown();
                Environment.Exit(Environment.ExitCode);
            }
        }

        private static void warn_on_nuspec_or_nupkg_usage(string[] args, ChocolateyConfiguration config)
        {
            var commandLine = Environment.CommandLine;
            if (!(commandLine.contains(" pack ") || commandLine.contains(" push ")) && (commandLine.contains(".nupkg") || commandLine.contains(".nuspec")))
            {
                if (config.RegularOutput) "chocolatey".Log().Warn("The use of .nupkg or .nuspec in for package name or source is known to cause issues. Please use the package id from the nuspec `<id />` with `-s .` (for local folder where nupkg is found).");
            }
        }

        private static void report_version_and_exit_if_requested(string[] args, ChocolateyConfiguration config)
        {
            if (args == null || args.Length == 0) return;

            var firstArg = args.FirstOrDefault();
            if (firstArg.is_equal_to("-v") || firstArg.is_equal_to("--version"))
            {
                "chocolatey".Log().Info(ChocolateyLoggers.Important, () => "{0}".format_with(config.Information.ChocolateyProductVersion));
                "chocolatey".Log().Debug(() => "Exiting with 0");
                Environment.Exit(0);
            }
        }

        private static void trap_exit_scenarios(ChocolateyConfiguration config)
        {
            ExitScenarioHandler.SetHandler();
        }

        private static void remove_old_chocolatey_exe(IFileSystem fileSystem)
        {
            FaultTolerance.try_catch_with_logging_exception(
                () =>
                {
                    fileSystem.delete_file(fileSystem.get_current_assembly_path() + ".old");
                    fileSystem.delete_file(fileSystem.combine_paths(AppDomain.CurrentDomain.BaseDirectory, "choco.exe.old"));
                },
                errorMessage: "Attempting to delete choco.exe.old ran into an issue",
                throwError: false,
                logWarningInsteadOfError: true,
                logDebugInsteadOfError: false,
                isSilent: true
                );
        }
    }
}

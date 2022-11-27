using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using chocolatey.infrastructure.app.attributes;
using chocolatey.infrastructure.commandline;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.commands;
using chocolatey.infrastructure.logging;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.filesystem;

namespace chocolatey.infrastructure.app.commands
{
    [CommandFor("psrun", "runs ps-script for local packages")]
    public class ChocolateyPsRunCommand : ICommand
    {
        private readonly IChocolateyPackageService _packageService;
        private readonly IPowershellService _powershellService;
        private readonly IFileSystem _fileSystem;

        public enum ExecuteStep
        { 
            i,
            install,
            u,
            uninstall,
            bm,
            before_modify
        }

        public ChocolateyPsRunCommand(IChocolateyPackageService packageService, IPowershellService powershellService, 
            IFileSystem fileSystem)
        {
            _packageService = packageService;
            _powershellService = powershellService;
            _fileSystem = fileSystem;
        }

        public virtual void configure_argument_parser(OptionSet optionSet, ChocolateyConfiguration configuration)
        {
            optionSet
                 .Add("s=|step=",
                     "Operation to execute, one of: install/i(default), before_modify/bm, uninstall/u",
                      option => configuration.PsRunCommand.step = option)
                 .Add("keeptemp",
                     $"Keep temporary files in %TEMP%\\{builders.ConfigurationBuilder.chocoTempFolderName}\\{PSRunTempFolder}",
                      option => configuration.PsRunCommand.keeptemp = option != null)
                 .Add("pre|prerelease",
                     "Prerelease - Include Prereleases? Defaults to false.",
                     option => configuration.Prerelease = option != null)
                 .Add("e|exact",
                     "Exact - Only return packages with this exact name.",
                     option => configuration.ListCommand.Exact = option != null)
                 .Add("by-id-only",
                     "ByIdOnly - Only return packages where the id contains the search filter.",
                     option => configuration.ListCommand.ByIdOnly = option != null)
                 .Add("by-tag-only|by-tags-only",
                     "ByTagOnly - Only return packages where the search filter matches on the tags.",
                     option => configuration.ListCommand.ByTagOnly = option != null)
                 .Add("id-starts-with",
                     "IdStartsWith - Only return packages where the id starts with the search filter.",
                     option => configuration.ListCommand.IdStartsWith = option != null)
                 .Add("approved-only",
                     "ApprovedOnly - Only return approved packages - this option will filter out results not from the community repository.",
                     option => configuration.ListCommand.ApprovedOnly = option != null)   
                 .Add("not-broken",
                     "NotBroken - Only return packages that are not failing testing - this option only filters out failing results from the community feed. It will not filter against other sources.",
                     option => configuration.ListCommand.NotBroken = option != null)
                ;
        }

        public virtual void handle_additional_argument_parsing(IList<string> unparsedArguments, ChocolateyConfiguration configuration)
        {
            configuration.Input = string.Join(" ", unparsedArguments);
        }

        ExecuteStep operation = ExecuteStep.install;

        public virtual void handle_validation(ChocolateyConfiguration config)
        {
            if (!Enum.TryParse<ExecuteStep>(config.PsRunCommand.step.ToLower(), out operation))
            {
                throw new ApplicationException($"operation '{config.PsRunCommand.step}' is not valid: allowed only install/i, uninstall/u, before_modify/bm");    
            }
        }

        public virtual void help_message(ChocolateyConfiguration configuration)
        {
            var (hiconsole, console) = LogService.Instance.HelpLoggers;

            hiconsole.Info("PsRun Command");

            if (ApplicationParameters.runningUnitTesting)
                console = hiconsole = LogService.Instance.NullLogger;

            console.Info(@"
Chocolatey will execure powershell script in installed locally packages.
This is useful after installing specific package with skippowershell option.

Used for testing powershell scripts.
");

            hiconsole.Info("Usage");
            console.Info(@"
    choco psrun <package> [<options/switches>]
");

            hiconsole.Info("Examples");
            console.Info(@"
    choco psrun installpackage
    choco psrun installpackage -s install
    choco psrun installpackage -s uninstall
    choco psrun installpackage -s u
    choco psrun installpackage -s before_modify
");
      
            hiconsole.Info("Exit Codes");
            console.Info(@"
Exit codes that normally result from running this command.

Normal:
 - 0: operation was successful, no issues detected
 - -1 or 1: an error has occurred

");
        }

        public virtual void noop(ChocolateyConfiguration configuration)
        {
            _packageService.list_noop(configuration);
        }

        public const string PSRunTempFolder = "psrun";

        public virtual void run(ChocolateyConfiguration config)
        {
            config.QuietOutput = true;
            config.ListCommand.LocalOnly = true;
            _packageService.ensure_source_app_installed(config);
            var packageResults = _packageService.list_run(config).ToList();

            if (packageResults.Count == 0)
            {
                throw new ApplicationException($"No packages found: '{config.Input}'");
            }

            if (packageResults.Count >= 2)
            {
                throw new ApplicationException($"More than one package found by criteria: '{config.Input}'");
            }

            config.QuietOutput = false;
            var pkgResult = packageResults.First();
            pkgResult.InstallLocation = Path.Combine(InstallContext.Instance.PackagesLocation, pkgResult.Package.Id);

            // Short aliases to longer form
            switch (operation)
            {
                case ExecuteStep.i:
                    operation = ExecuteStep.install;
                    break;
                case ExecuteStep.u:
                    operation = ExecuteStep.uninstall;
                    break;
                case ExecuteStep.bm:
                    operation = ExecuteStep.before_modify;
                    break;
            }

            bool keeptemp = config.PsRunCommand.keeptemp;

            if (keeptemp)
            {
                InstallContext.Instance.PowerShellTempLocation = Path.Combine(InstallContext.Instance.CacheLocation, PSRunTempFolder);
            }

            var powershellTemp = InstallContext.Instance.PowerShellTempLocation;

            LogService.console.Info($"- Package '{pkgResult.Package.Id}': executing {operation}...");
            switch (operation)
            {
                case ExecuteStep.install:
                    _powershellService.install(config, pkgResult);
                    break;
                case ExecuteStep.uninstall:
                    _powershellService.uninstall(config, pkgResult);
                    break;
                case ExecuteStep.before_modify:
                    _powershellService.before_modify(config, pkgResult);
                    break;
            }

            // if there are no results, exit with a 2.
            if (config.Features.UseEnhancedExitCodes && packageResults.Count == 0 && Environment.ExitCode == 0)
            {
                Environment.ExitCode = 2;
            }

            if (!keeptemp)
            { 
                _fileSystem.delete_directory_if_exists(powershellTemp, true);
            }
        }

        public virtual bool may_require_admin_access()
        {
            return false;
        }
    }
}

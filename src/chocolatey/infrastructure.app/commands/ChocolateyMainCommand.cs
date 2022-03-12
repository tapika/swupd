namespace chocolatey.infrastructure.app.commands
{
    using System.Collections.Generic;
    using chocolatey.infrastructure.app.nuget;
    using commandline;
    using configuration;
    using infrastructure.commands;

    public class ChocolateyMainCommand : ICommand
    {
        public virtual void configure_argument_parser(OptionSet optionSet, ChocolateyConfiguration config)
        {
            ChocolateyStartupCommand.configureStartupOptions(optionSet);

            optionSet
                .Add("d|debug",
                     "Debug - Show debug messaging.",
                     option => config.Debug = option != null)
                .Add("v|verbose",
                     "Verbose - Show verbose messaging. Very verbose messaging, avoid using under normal circumstances.",
                     option => config.Verbose = option != null)
                .Add("trace",
                     "Trace - Show trace messaging. Very, very verbose trace messaging. Avoid except when needing super low-level .NET Framework debugging. Available in 0.10.4+.",
                     option => config.Trace = option != null)
                .Add("nocolor|no-color",
                     "No Color - Do not show colorization in logging output. This overrides the feature '{0}', set to '{1}'. Available in 0.10.9+.".format_with(ApplicationParameters.Features.LogWithoutColor, config.Features.LogWithoutColor),
                     option => config.Features.LogWithoutColor = option != null)
                .Add("acceptlicense|accept-license",
                     "AcceptLicense - Accept license dialogs automatically. Reserved for future use.",
                     option => config.AcceptLicense = option != null)
                .Add("y|yes|confirm",
                     "Confirm all prompts - Chooses affirmative answer instead of prompting. Implies --accept-license",
                     option =>
                     {
                         config.PromptForConfirmation = option == null;
                         config.AcceptLicense = option != null;
                     })
                .Add("f|force",
                     "Force - force the behavior. Do not use force during normal operation - it subverts some of the smart behavior for commands.",
                     option => config.Force = option != null)
                .Add("noop|whatif|what-if",
                     "NoOp / WhatIf - Don't actually do anything.",
                     option => config.Noop = option != null)
                .Add("r|limitoutput|limit-output",
                     "LimitOutput - Limit the output to essential information",
                     option => config.RegularOutput = option == null)
                .Add("timeout=|execution-timeout=",
                     "CommandExecutionTimeout (in seconds) - The time to allow a command to finish before timing out. Overrides the default execution timeout in the configuration of {0} seconds. '0' for infinite starting in 0.10.4.".format_with(config.CommandExecutionTimeoutSeconds.to_string()),
                    option =>
                    {
                        int timeout = 0;
                        var timeoutString = option.remove_surrounding_quotes();
                        int.TryParse(timeoutString, out timeout);
                        if (timeout > 0 || timeoutString.is_equal_to("0"))
                        {
                            config.CommandExecutionTimeoutSeconds = timeout;
                        }
                    })
                .Add("c=|cache=|cachelocation=|cache-location=",
                     "CacheLocation - Location for download cache, defaults to %TEMP% or value in chocolatey.config file.",
                     option => config.CacheLocation = option.remove_surrounding_quotes())
                .Add("failstderr|failonstderr|fail-on-stderr|fail-on-standard-error|fail-on-error-output",
                     "FailOnStandardError - Fail on standard error output (stderr), typically received when running external commands during install providers. This overrides the feature failOnStandardError.",
                     option => config.Features.FailOnStandardError = option != null)
                .Add("use-system-powershell",
                     "UseSystemPowerShell - Execute PowerShell using an external process instead of the built-in PowerShell host. Should only be used when internal host is failing. Available in 0.9.10+.",
                     option => config.Features.UsePowerShellHost = option == null)
                .Add("no-progress",
                     "Do Not Show Progress - Do not show download progress percentages. Available in 0.10.4+.",
                     option => config.Features.ShowDownloadProgress = option == null)
                .Add("proxy=",
                    "Proxy Location - Explicit proxy location. Overrides the default proxy location of '{0}'. Available for config settings in 0.9.9.9+, this CLI option available in 0.10.4+.".format_with(config.Proxy.Location),
                    option => config.Proxy.Location = option.remove_surrounding_quotes())
                .Add("proxy-user=",
                    "Proxy User Name - Explicit proxy user (optional). Requires explicit proxy (`--proxy` or config setting). Overrides the default proxy user of '{0}'. Available for config settings in 0.9.9.9+, this CLI option available in 0.10.4+.".format_with(config.Proxy.User),
                    option => config.Proxy.User = option.remove_surrounding_quotes())
                .Add("proxy-password=",
                    "Proxy Password - Explicit proxy password (optional) to be used with username. Requires explicit proxy (`--proxy` or config setting) and user name.  Overrides the default proxy password (encrypted in settings if set). Available for config settings in 0.9.9.9+, this CLI option available in 0.10.4+.",
                    option => config.Proxy.EncryptedPassword = NugetEncryptionUtility.EncryptString(option.remove_surrounding_quotes()))
                .Add("proxy-bypass-list=",
                     "ProxyBypassList - Comma separated list of regex locations to bypass on proxy. Requires explicit proxy (`--proxy` or config setting). Overrides the default proxy bypass list of '{0}'. Available in 0.10.4+.".format_with(config.Proxy.BypassList),
                     option => config.Proxy.BypassList = option.remove_surrounding_quotes())
                .Add("proxy-bypass-on-local",
                     "Proxy Bypass On Local - Bypass proxy for local connections. Requires explicit proxy (`--proxy` or config setting). Overrides the default proxy bypass on local setting of '{0}'. Available in 0.10.4+.".format_with(config.Proxy.BypassOnLocal),
                     option => config.Proxy.BypassOnLocal = option != null)
                 .Add("log-file=",
                     "Log File to output to in addition to regular loggers. Available in 0.10.8+.",
                     option => config.AdditionalLogFileLocation = option.remove_surrounding_quotes())
                ;
        }

        public virtual void handle_additional_argument_parsing(IList<string> unparsedArguments, ChocolateyConfiguration configuration)
        {
        }

        public virtual void handle_validation(ChocolateyConfiguration configuration)
        {
        }

        public virtual void help_message(ChocolateyConfiguration configuration)
        {
            ChocolateyHelpCommand.display_help_message(ApplicationManager.Instance.Container);
        }

        public virtual void noop(ChocolateyConfiguration configuration)
        {
            ChocolateyHelpCommand.display_help_message(ApplicationManager.Instance.Container);
        }

        public virtual void run(ChocolateyConfiguration configuration)
        {
            ChocolateyHelpCommand.display_help_message(ApplicationManager.Instance.Container);
        }

        public virtual bool may_require_admin_access()
        {
            return false;
        }
    }
}

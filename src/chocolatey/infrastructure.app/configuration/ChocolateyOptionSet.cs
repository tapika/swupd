using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.commandline;
using chocolatey.infrastructure.commands;
using chocolatey.infrastructure.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace chocolatey.infrastructure.app.configuration
{
    public class ChocolateyOptionSet: OptionSet
    {
        static ChocolateyOptionSet _instance;
        public static ChocolateyOptionSet Instance
        {
            get
            {
                if (_instance == null) _instance = new ChocolateyOptionSet();
                return _instance;
            }
        }

        /// <summary>
        /// Parses options targetted for specific command.
        /// </summary>
        /// <param name="args">Arguments to process</param>
        /// <param name="command">Command from which to take options</param>
        /// <param name="config">configuration into which to parse</param>
        /// <returns>true if command was processed (e.g. help), false if not</returns>
        public bool Parse(IEnumerable<string> args, ICommand command, ChocolateyConfiguration config, Action failLogInit = null)
        {
            if (failLogInit == null) failLogInit = () => { };

            if (Count == 0)
            {
                Add("?|help|h", "Prints out the help menu.",
                    option => config.HelpRequested = option != null);
            }

            command.configure_argument_parser(this, config);
            List<string> unparsedArguments;
            var console = LogService.console;

            try
            {
                unparsedArguments = base.Parse(args);
            }
            catch (Exception ex)
            {
                // ArgumentException is maybe a bug in Parse.
                if (ex is OptionException || ex is ArgumentException)
                {
                    unparsedArguments = args.ToList();
                    failLogInit();
                    console.Info($"Error: {ex.Message}");
                    config.UnsuccessfulParsing = true;
                    return true;
                }

                throw ex;
            }

            if (command is ChocolateyStartupCommand)
            {
                return false;
            }

            // the command argument
            if (string.IsNullOrWhiteSpace(config.CommandName) && unparsedArguments.Contains(args.FirstOrDefault()))
            {
                var commandName = args.FirstOrDefault();
                if (!Regex.IsMatch(commandName, @"^[-\/+]"))
                {
                    config.CommandName = commandName;
                }
                else if (commandName.is_equal_to("-v") || commandName.is_equal_to("--version"))
                {
                    // skip help menu
                }
                else
                {
                    console.Info($"Error: Command not specified and argument is not known: {unparsedArguments.First()}");
                    config.UnsuccessfulParsing = true;
                }
            }

            if (command is ChocolateyMainCommand)
            {
                if (!string.IsNullOrWhiteSpace(config.CommandName))
                {
                    // save help for next menu
                    config.HelpRequested = false;
                    config.UnsuccessfulParsing = false;
                }
            }
            else
            {
                // options are parsed. Attempt to set it again once local options are set.
                // This does mean some output from debug will be missed (but not much)
                if (config.Debug) LogService.Instance.adjustLogLevels(config.Debug, config.Verbose, config.Trace);
                command.handle_additional_argument_parsing(unparsedArguments, config);

                if (!config.Features.IgnoreInvalidOptionsSwitches)
                {
                    // all options / switches should be parsed,
                    //  so show help menu if there are any left
                    foreach (var unparsedArg in unparsedArguments.or_empty_list_if_null())
                    {
                        if (unparsedArg.StartsWith("-") || unparsedArg.StartsWith("/"))
                        {
                            console.Info($"Error: Unknown command line option: {unparsedArg}\n" +
                            "   Ignoring unknown command line argument can be enabled via:\n" +
                            "   choco feature enable -n=ignoreInvalidOptionsSwitches");
                            config.UnsuccessfulParsing = true;
                            return true;
                        }
                    }
                }
            }

            if (config.HelpRequested)
            {
                failLogInit();
                command.help_message(config);
                var sb = new StringBuilder();
                var stringWriter = new StringWriter(sb);
                WriteOptionDescriptions(stringWriter);
                if (!ApplicationParameters.runningUnitTesting)
                {
                    "chocolatey".Log().Info(sb.ToString());
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Splits command line arguments.
        /// </summary>
        /// <param name="commandLineArguments">command line arguments</param>
        /// <returns></returns>
        public static string[] SplitArgs( string commandLineArguments)
        {
            // Parse command line arguments: https://stackoverflow.com/a/19091999/2338477
            var re = @"\G(""((""""|[^""])+)""|(\S+)) *";
            var ms = Regex.Matches(commandLineArguments, re);
            var args = ms.Cast<System.Text.RegularExpressions.Match>()
                         .Select(m => Regex.Replace(
                             m.Groups[2].Success
                                 ? m.Groups[2].Value
                                 : m.Groups[4].Value, @"""""", @"""")).ToArray();

            return args;
        }

    }


}

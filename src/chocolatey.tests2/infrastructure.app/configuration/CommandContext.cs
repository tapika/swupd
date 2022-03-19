using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.attributes;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.commandline;
using chocolatey.infrastructure.commands;
using chocolatey.infrastructure.logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace chocolatey.tests2.infrastructure.app.configuration
{
    /// <summary>
    /// Here we try to mimic what is happening in choco application itself.
    /// Logic is the same, only resides in different places of code.
    /// </summary>
    public class CommandContext
    {
        public static List<string> CommandNames
        {
            get
            {
                var cmdTypes = ApplicationManager.Instance.CommandTypes;
                var names = cmdTypes.Select(x => x.GetCustomAttributes<CommandForAttribute>().First().CommandName).ToList();
                return names;
            }
        }

        /// <summary>
        /// Dictionary of all mock and interface types
        /// </summary>
        public Dictionary<Type, object> InstanceOfType = new Dictionary<Type, object>();

        /// <summary>
        /// Command line parser, after initialization contains generic and command specific command arguments
        /// </summary>
        ChocolateyOptionSet parser;

        /// <summary>
        /// Amount of generic options before command specific options
        /// </summary>
        int genericArgumentsCount;

        /// <summary>
        /// Command instance
        /// </summary>
        ICommand command;

        /// <summary>
        /// Original configuration (only with command initialized)
        /// </summary>
        ChocolateyConfiguration original;

        /// <summary>
        /// Gets instance of specific mock interface
        /// </summary>
        public Mock<T> Mock<T>() where T : class
        {
            return (Mock<T>)InstanceOfType[typeof(Mock<T>)];
        }

        public CommandContext()
        { 
        }

        public CommandContext(string command, bool logCalls = true)
        {
            InstantiateCommand(command, logCalls);
        }

        /// <summary>
        /// Parses command line. Instantiates command if not already instantiated.
        /// Cannot change command type without creating new instance of CommandContext
        /// </summary>
        /// <param name="cmdLine">command line to process</param>
        /// <param name="diffAtStart">true if configuration comparison starts from start of command line processing, false if after main
        /// command is executed</param>
        public IEnumerable<Option> ParseCommandLine(string cmdLine, bool diffAtStart = false, bool logCalls = false, bool validateRun = true)
        {
            var console = LogService.console;

            console.Info($"> {cmdLine}");
            string[] args = ChocolateyOptionSet.SplitArgs(cmdLine);
            ChocolateyConfiguration config = new ChocolateyConfiguration();
            parser = new ChocolateyOptionSet();

            if (diffAtStart) original = config.deep_copy();
            if (parser.Parse(args, new ChocolateyStartupCommand(), config, () =>
            {
                console.Info("- logInit called.");
            }
                ))
            {
                console.Info("chocoArgsParse: return on startup");
                return parser;
            }

            parser = new ChocolateyOptionSet();

            if (parser.Parse(args, new ChocolateyMainCommand(), config))
            {
                console.Info("chocoArgsParse: return on main");
                return parser;
            }

            genericArgumentsCount = parser.Count;

            if (args.Length == 0)
            {
                "chocolatey".Log().Info(ChocolateyLoggers.Important, () => "Please run 'choco -?' or 'choco <command> -?' for help menu.");
            }

            InstantiateCommand(config.CommandName, logCalls);
            if (command == null)
            { 
                return parser.Skip(genericArgumentsCount);
            }

            LogService.Instance.adjustLogLevels(config.Debug, config.Verbose, config.Trace);

            if (!diffAtStart) original = config.deep_copy();

            string phase = "parse";
            try
            {
                if (!parser.Parse(args.Skip(1), command, config))
                {
                    if (!validateRun)
                    {
                        return parser;
                    }

                    phase = "validation";
                    command.handle_validation(config);

                    console.Info("config: " + config.CompareWith(original));

                    // GenericRunner.run logic, see after line:
                    // var command = find_command(config, container, isConsole, parseArgs);
                    if (config.Noop)
                    {
                        if (config.RegularOutput)
                        {
                            this.Log().Info("_ {0}:{1} - Noop Mode _".format_with(ApplicationParameters.Name, command.GetType().Name));
                        }

                        command.noop(config);
                    }
                    else
                    {
                        this.Log().Debug("_ {0}:{1} - Normal Run Mode _".format_with(ApplicationParameters.Name, command.GetType().Name));
                        command.run(config);
                    }
                }
            }
            catch (ApplicationException ex)
            {
                console.Info($"{phase} failure: " + ex.Message);
            }

            return parser.Skip(genericArgumentsCount);
        }

        /// <summary>
        /// Instantiate command if it's not yet instatiated
        /// </summary>
        /// <param name="commandName">command to instatiate</param>
        /// <param name="logCalls">Log calls from Mock</param>
        private void InstantiateCommand(string commandName, bool logCalls)
        {
            if (command != null)
            {
                return;
            }
        
            var cmdTypes = ApplicationManager.Instance.CommandTypes;
            var names = CommandNames;
            int index = names.IndexOf(commandName);

            // Cannot instatiate, ignore request
            if (index == -1)
            {
                return;
            }

            Type cmdType = cmdTypes[index];
            ConstructorInfo ctorInfo = cmdType.GetConstructors().First();
            var ctorTypes = ctorInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            object[] ctorArgs = new object[ctorTypes.Length];
            for (int i = 0; i < ctorTypes.Length; i++)
            {
                var mockType = typeof(Mock<>).MakeGenericType(ctorTypes[i]);

                // Corresponds to: mock = new Mock<ChocolateyListCommand>()
                Mock mock = (Mock)Activator.CreateInstance(mockType);
                var loggedGeneric = typeof(MockExtensions).GetMethod("Logged", BindingFlags.Public | BindingFlags.Static);
                InstanceOfType.Add(mockType, mock);

                if (!logCalls)
                {
                    ctorArgs[i] = mock.Object;
                }
                else
                {
                    // Corresponds to: MockExtensions.Logged(mock)
                    var logged = loggedGeneric.MakeGenericMethod(ctorTypes[i]);
                    var loggedMock = logged.Invoke(null, new object[] { mock, new string[] { } });
                    ctorArgs[i] = ((Mock)loggedMock).Object;
                }

                InstanceOfType.Add(ctorTypes[i], ctorArgs[i]);
            }

            command = (ICommand)ctorInfo.Invoke(ctorArgs);
        }

    }
}

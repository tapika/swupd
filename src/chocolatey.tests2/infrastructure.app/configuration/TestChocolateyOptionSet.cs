using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.attributes;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.commandline;
using chocolatey.infrastructure.commands;
using chocolatey.infrastructure.logging;
using logtesting;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace chocolatey.tests2.infrastructure.app.configuration
{
    [Parallelizable(ParallelScope.All)]
    public class TestChocolateyOptionSet : LogTesting
    {
        List<string> CommandNames
        {
            get {
                var cmdTypes = ApplicationManager.Instance.CommandTypes;
                var names = cmdTypes.Select(x => x.GetCustomAttributes<CommandForAttribute>().First().CommandName).ToList();
                return names;
            }
        }

        /// <summary>
        /// Here we try to mimic what is happening in choco application itself.
        /// Logic is the same, only resides in different places of code.
        /// </summary>
        public IEnumerable<Option> _chocoArgsParse(string cmdLine, int diffLevel = 3, bool logCalls = false)
        {
            var console = LogService.console;

            console.Info($"> {cmdLine}");
            string[] args = ChocolateyOptionSet.SplitArgs(cmdLine);
            ChocolateyConfiguration config = new ChocolateyConfiguration();
            ChocolateyConfiguration original = config.deep_copy();
            ChocolateyOptionSet parser = new ChocolateyOptionSet();

            if(diffLevel == 1) original = config.deep_copy();
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

            if (diffLevel == 2) original = config.deep_copy();
            if (parser.Parse(args, new ChocolateyMainCommand(), config))
            {
                console.Info("chocoArgsParse: return on main");
                return parser;
            }

            int genericOptions = parser.Count;

            if (args.Length == 0)
            {
                "chocolatey".Log().Info(ChocolateyLoggers.Important, () => "Please run 'choco -?' or 'choco <command> -?' for help menu.");
            }

            var cmdTypes = ApplicationManager.Instance.CommandTypes;
            var names = CommandNames;
            int index =  names.IndexOf(config.CommandName);
            if (index != -1)
            {
                Type cmdType = cmdTypes[index];
                ConstructorInfo ctorInfo = cmdType.GetConstructors( ).First();
                var ctorTypes = ctorInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                object[] ctorArgs = new object[ctorTypes.Length];
                for (int i = 0; i < ctorTypes.Length; i++)
                {
                    var mockType = typeof(Mock<>).MakeGenericType(ctorTypes[i]);

                    // Corresponds to: mock = new Mock<ChocolateyListCommand>()
                    Mock mock = (Mock)Activator.CreateInstance(mockType);
                    var loggedGeneric = typeof(MockExtensions).GetMethod("Logged", BindingFlags.Public | BindingFlags.Static);

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
                }

                ICommand command = (ICommand)ctorInfo.Invoke(ctorArgs);

                LogService.Instance.adjustLogLevels(config.Debug, config.Verbose, config.Trace);

                if (diffLevel == 3) original = config.deep_copy();
                if (!parser.Parse(args.Skip(1), command, config))
                {
                    try
                    {
                        command.handle_validation(config);
                    }
                    catch (ApplicationException ex)
                    { 
                        console.Info("validation failure: " + ex.Message);
                    }

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
            return parser.Skip(genericOptions);
        }

        public IEnumerable<Option> chocoArgsParse(string cmdLine)
        {
            var opts = _chocoArgsParse(cmdLine);
            LogService.console.Info("");
            return opts;
        }

        public IEnumerable<Option> chocoArgsBasicParse(string cmdLine)
        {
            var opts = _chocoArgsParse(cmdLine, 1, true);
            LogService.console.Info("");
            return opts;
        }


        [LogTest]
        public void basic_commands()
        {
            chocoArgsBasicParse("");
            chocoArgsBasicParse("-?");
            chocoArgsBasicParse("list --root subfolder");
            chocoArgsBasicParse("list --root");
            chocoArgsBasicParse("list -d -lo");
            chocoArgsBasicParse("list");
            chocoArgsBasicParse("list -?");
            chocoArgsBasicParse("list -lo");
            chocoArgsBasicParse("list -lo --noop");
            chocoArgsBasicParse("-d");
            chocoArgsBasicParse("-d list");
        }

        [Test]
        public void command()
        {
            List<string> commandNames = CommandNames;
            commandNames.Remove("help");
            commandNames.Remove("update");      //depricated
            commandNames.Remove("version");     //depricated

            //commandNames = new[] { "pin" }.ToList();

            foreach (string cmd in commandNames)
            {
                using (new VerifyingLog(cmd))
                {
                    testCommand(cmd);
                }
            }
        }

        public void testCommand(string command)
        {
            // Prevent password prompt queries
            ApplicationParameters.AllowPrompts = false;
            var args = chocoArgsParse($"{command} -?").ToList();
            string extraArg = "";

            switch (command)
            {
                case "install":
                case "new":
                case "uninstall":
                case "upgrade":
                    extraArg = " pkg";
                    break;

            }

            foreach (var arg in args)
            {
                string argSwitch = arg.GetNames()[0];
                string cmd = command;
                string value = "1";

                if (command == "push" && argSwitch == "s")
                {
                    cmd += " -key mykey";
                    value = "http://www.google.com";
                }

                if (arg.OptionValueType == OptionValueType.None)
                { 
                    cmd += $" --{argSwitch}";
                }
                else
                {
                    cmd += $" --{argSwitch} {value}";       
                }
                
                cmd += extraArg;

                chocoArgsParse(cmd);
            }
        }

    }
}

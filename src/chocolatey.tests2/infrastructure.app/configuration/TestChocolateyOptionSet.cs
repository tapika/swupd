using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.attributes;
using chocolatey.infrastructure.app.commands;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.commands;
using chocolatey.infrastructure.logging;
using logtesting;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace chocolatey.tests2.infrastructure.app.configuration
{
    [Parallelizable(ParallelScope.All)]
    public class TestChocolateyOptionSet : LogTesting
    {
        /// <summary>
        /// Here we try to mimic what is happening in choco application itself.
        /// Logic is the same, only resides in different places of code.
        /// </summary>
        public void _chocoArgsParse(string cmdLine)
        {
            var console = LogService.console;

            console.Info($"> {cmdLine}");
            string[] args = ChocolateyOptionSet.SplitArgs(cmdLine);
            ChocolateyConfiguration config = new ChocolateyConfiguration();
            ChocolateyConfiguration original = config.deep_copy();
            ChocolateyOptionSet parser = new ChocolateyOptionSet();

            if (parser.Parse(args, new ChocolateyStartupCommand(), config, () =>
                 {
                     console.Info("- logInit called.");
                 }
                ))
            { 
                console.Info("chocoArgsParse: return on startup");
                return;
            }
            
            parser = new ChocolateyOptionSet();
            
            if (parser.Parse(args, new ChocolateyMainCommand(), config))
            {
                console.Info("chocoArgsParse: return on main");
                return;
            }

            if (args.Length == 0)
            {
                "chocolatey".Log().Info(ChocolateyLoggers.Important, () => "Please run 'choco -?' or 'choco <command> -?' for help menu.");
            }

            var cmdTypes = ApplicationManager.Instance.CommandTypes;
            var names = cmdTypes.Select(x => x.GetCustomAttributes<CommandForAttribute>().First().CommandName ).ToList();
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

                    // Corresponds to: MockExtensions.Logged(mock)
                    var logged = loggedGeneric.MakeGenericMethod(ctorTypes[i]);
                    var loggedMock = logged.Invoke(null, new object[] { mock, new string[] { } });
                    ctorArgs[i] = ((Mock)loggedMock).Object;
                }

                ICommand command = (ICommand)ctorInfo.Invoke(ctorArgs);

                LogService.Instance.adjustLogLevels(config.Debug, config.Verbose, config.Trace);

                if (!parser.Parse(args, command, config))
                {
                    command.handle_validation(config);

                    console.Info("config:");
                    console.Info(config.CompareWith(original));

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
        }

        public void chocoArgsParse(string cmdLine)
        {
            _chocoArgsParse(cmdLine);
            LogService.console.Info("");
        }

        [LogTest()]
        public void basic_commands()
        {
            chocoArgsParse("");
            chocoArgsParse("-?");
            chocoArgsParse("list --root subfolder");
            chocoArgsParse("list --root");
            chocoArgsParse("list -d -lo");
            chocoArgsParse("list");
            chocoArgsParse("list -?");
            chocoArgsParse("list -lo");
            chocoArgsParse("list -lo --noop");
        }

    }
}

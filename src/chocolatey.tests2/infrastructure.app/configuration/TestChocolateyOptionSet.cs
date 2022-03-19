using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.attributes;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.commandline;
using chocolatey.infrastructure.logging;
using logtesting;
using Moq;
using NuGet;
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
        public IEnumerable<Option> chocoArgsParse(string cmdLine)
        {
            var opts = new CommandContext().ParseCommandLine(cmdLine);
            LogService.console.Info("");
            return opts;
        }

        public IEnumerable<Option> chocoArgsBasicParse(string cmdLine)
        {
            var opts = new CommandContext().ParseCommandLine(cmdLine, true, true);
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
            List<string> commandNames = CommandContext.CommandNames;
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

        //originad from when_handling_additional_argument_parsing
        [LogTest]
        public void test_pin()
        {
            CommandContext cc = new CommandContext("pin");
            Action<string> parseCommand = (cmd) =>
            {
                cc.ParseCommandLine(cmd);
                LogService.console.Info("");
            };

            parseCommand("pin");            //no argument, defaults to list
            parseCommand("pin list");
            parseCommand("pin wtf");        //invalid argument, set to list
            parseCommand("pin wtf bbq");
            parseCommand("pin add");        //no package name
            parseCommand("pin add -n pkg --noop");
            parseCommand("pin ADD -n pkg --noop");
            parseCommand("pin remove");     //no package name
            parseCommand("pin \" \"");      // empty argument
        }

    }
}

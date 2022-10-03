using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.attributes;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.commandline;
using chocolatey.infrastructure.logging;
using chocolatey.infrastructure.results;
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
            var opts = new CommandContext().ParseCommandLine2(cmdLine);
            LogService.console.Info("");
            return opts;
        }

        public IEnumerable<Option> chocoArgsBasicParse(string cmdLine)
        {
            var opts = new CommandContext().ParseCommandLine2(cmdLine, true, true);
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
            chocoArgsBasicParse("list --instdir");
            chocoArgsBasicParse("list --instdir instdir");
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

                if (command == "pin" && argSwitch == "s")
                {
                    value = nameof(SourceType.windowsinstall);
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

        //originad from ChocolateyPinCommandSpecs.cs
        [LogTest]
        public void test_pin()
        {
            CommandContext cc = new CommandContext("pin");
            cc.ParseCommandLine("pin");            //no argument, defaults to list
            cc.ParseCommandLine("pin list");
            cc.ParseCommandLine("pin list -s invalid");
            cc.ParseCommandLine("pin list -s windowsinstall");
            cc.ParseCommandLine("pin list -s windowsinstall somepack*");
            cc.ParseCommandLine("pin list 1 2");
            cc.ParseCommandLine("pin wtf");        //invalid argument, set to list
            cc.ParseCommandLine("pin wtf bbq");
            cc.ParseCommandLine("pin list somedir*");
            cc.ParseCommandLine("pin add");        //no package name
            cc.ParseCommandLine("pin add -n pkg --noop");
            cc.ParseCommandLine("pin ADD -n pkg --noop");
            cc.ParseCommandLine("pin remove");     //no package name
            cc.ParseCommandLine("pin \" \"");      // empty argument

            // If we have two packages, one of them is pinned, only pinned should be shown.
            var packageInfoService = cc.Mock<IChocolateyPackageInformationService>();

            var package = new Mock<IPackage>();
            package.Setup(p => p.Id).Returns("regular");
            package.Setup(p => p.Version).Returns(new SemanticVersion("1.2.0"));

            packageInfoService.Setup(s => s.get_package_information(package.Object)).Returns(
                new ChocolateyPackageInformation(package.Object) { IsPinned = false });

            var pinnedPackage = new Mock<IPackage>();
            pinnedPackage.Setup(p => p.Id).Returns("pinned");
            pinnedPackage.Setup(p => p.Version).Returns(new SemanticVersion("1.1.0"));

            packageInfoService.Setup(s => s.get_package_information(pinnedPackage.Object)).Returns(
                new ChocolateyPackageInformation(pinnedPackage.Object) { IsPinned = true });

            var packageResults = new[]
            {
                new PackageResult(package.Object, null),
                new PackageResult(pinnedPackage.Object, null)
            };

            cc.Mock<IChocolateyPackageService>().Setup(n => n.list_run(It.IsAny<ChocolateyConfiguration>())).Returns(packageResults);
            cc.ParseCommandLine("pin list");
            cc.ParseCommandLine("pin list -u");
        }
    }
}

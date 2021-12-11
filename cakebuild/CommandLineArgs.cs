using Cake.Cli;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace cakebuild
{
    /// <summary>
    /// Copied from Cake
    /// src\Cake.Frosting\Internal\Commands\DefaultCommandSettings.cs
    /// due to protected nature of class.
    /// </summary>
    public class DefaultCommandLineArgs : CommandSettings
    {
        [CommandOption("--target|-t <TARGET>")]
        [DefaultValue("Default")]
        [Description("Target task to invoke.")]
        public string Target { get; set; } = "Default";

        [CommandOption("--working|-w <PATH>")]
        [TypeConverter(typeof(Cake.Cli.DirectoryPathConverter))]
        [Description("Sets the working directory")]
        public DirectoryPath WorkingDirectory { get; set; }

        [CommandOption("--verbosity|-v <VERBOSITY>")]
        [Description("Specifies the amount of information to be displayed.\n(Quiet, Minimal, Normal, Verbose, Diagnostic)")]
        [TypeConverter(typeof(VerbosityConverter))]
        [DefaultValue(Verbosity.Normal)]
        public Verbosity Verbosity { get; set; } = Verbosity.Normal;

        [CommandOption("--description|--descriptions|--showdescription|--showdescriptions")]
        [Description("Shows description for each task.")]
        public bool Description { get; set; }

        [CommandOption("--tree|--showtree")]
        [Description("Shows the task dependency tree.")]
        public bool Tree { get; set; }

        [CommandOption("--dryrun|--noop|--whatif")]
        [Description("Performs a dry run.")]
        public bool DryRun { get; set; }

        [CommandOption("--exclusive|-e")]
        [Description("Executes the target task without any dependencies.")]
        public bool Exclusive { get; set; }

        // Does not work in cake
        //[CommandOption("--version|--ver")]
        //[Description("Displays version information.")]
        //public bool Version { get; set; }

        [CommandOption("--info")]
        [Description("Displays additional information about Cake.")]
        public bool Info { get; set; }
    }

    public class CommandLineArgs : DefaultCommandLineArgs
    {
        [CommandArgument(0, "[COMMANDS]")]
        public string[] Commands { get; set; } = Array.Empty<string>();

        public bool displaysHelp = false;

        [CommandOption("--net <FRAMEWORK>")]
        [Description(".net framework to use (netcoreapp3.1, net5.0, net6.0, net4.8)")]
        public string NetFramework { get; set; } = "netcoreapp3.1";
        
        public string NetFrameworkSuffix
        {
            get {
                if (NetFramework.Length == 0)
                    return "";
                return "_" + NetFramework;
            }
        }

        //-----------------------------------------------------------------------------------
        // buildexe / readytorun options
        //-----------------------------------------------------------------------------------

        [CommandOption("--r2r_targets <targets>")]
        [Description("one or more targets to build, comma separated. For example choco or chocogui.")]
        public string r2r_targets { get; set; } = "";


        [CommandOption("--os <oses>")]
        [Description(
            "os for which to publish (win7, win81, linux, ...)\n" +
            "See also: .NET Runtime Identifier (RID) Catalog\n" +
            "Documentation: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog\n" +
            "Same in git: https://github.com/jesulink2514/docs-1/blob/master/docs/core/rid-catalog.md"
        )]
        public string OSS { get; set; } = "";

        [CommandOption($"--{nameof(r2r_build)}")]
        [Description("Builds readytorun executables")]
        public bool r2r_build { get; set; }

        [CommandOption($"--{nameof(r2r_push)}")]
        [Description("Pushes readytorun executables to github")]
        public bool? r2r_push { get; set; }

        //-----------------------------------------------------------------------------------
        // other options
        //-----------------------------------------------------------------------------------
        [CommandOption("--show")]
        [Description("Show commands being executed")]
        public bool ShowCommands { get; set; }


        public string[] GetCakeArgs()
        {
            DefaultCommandLineArgs defArgs = new DefaultCommandLineArgs();
            DefaultCommandLineArgs thisBase = this;
            List<String> args = new List<string>();

            foreach (var pi in typeof(DefaultCommandLineArgs).GetProperties())
            {
                // Handle dryrun by ourselves
                if (pi.Name == nameof(DefaultCommandLineArgs.DryRun))
                {
                    continue;
                }

                object value = pi.GetValue(thisBase);
                if (!object.Equals(value, pi.GetValue(defArgs)))
                {
                    args.Add("--" + pi.GetCustomAttribute<CommandOptionAttribute>().LongNames.First());
                    if (pi.PropertyType == typeof(bool))
                    {
                        continue;
                    }
                    args.Add(value.ToString());
                }
            }

            if (displaysHelp)
            { 
                args.Add("--help");
            }

            if (ShowCommands)
            {
                args.Add("--Settings_ShowProcessCommandLine");
                args.Add("true");
            }

            //Console.WriteLine("routed command: " + string.Join(' ', args));
            return args.ToArray();
        }

    }

    public class ParseCommandLineArgs : Command<CommandLineArgs>
    {
        public override int Execute(CommandContext context, CommandLineArgs settings)
        {
            return 0;
        }
    }

    public class CommandLineArgumentParser
    {
        /// <summary>
        /// Parses user defined command line arguments above.
        /// </summary>
        /// <param name="args">Command line arguments compatible with Cake</param>
        /// <returns></returns>
        public static CommandLineArgs Parse(string[] args)
        {
            bool displaysHelp = false;
            var newArgs = new CommandLineArgs();
            var app = new CommandApp<ParseCommandLineArgs>();
            app.Configure((config) =>
                {
                    FieldInfo fi = config.GetType().GetField("_registrar", BindingFlags.NonPublic | BindingFlags.Instance);
                    var reg = (ITypeRegistrar)fi.GetValue(config);

                    reg.RegisterLazy(typeof(CommandLineArgs), () => { return newArgs; });
                    config.PropagateExceptions();

                    // From src\Cake\Program.cs: Top level examples.
                    string exeName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    config.SetApplicationName(exeName);
                    //config.AddExample(new[] { string.Empty });
                    //config.AddExample(new[] { "build.cake", "--verbosity", "quiet" });
                    //config.AddExample(new[] { "build.cake", "--tree" });
                }
            );
            try
            {
                var helpOpt = new CommandOptionAttribute("-h|--help");
                var isMatch = helpOpt.GetType().GetMethod("IsMatch", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var arg in args)
                {
                    if ((bool)isMatch.Invoke(helpOpt, new object[] { arg.TrimStart('-') }))
                    { 
                         displaysHelp = true;
                    }
                }

                app.Run(args);
            }
            catch (CommandAppException ex)
            {
                MethodInfo mi = typeof(CommandApp).GetMethod("GetRenderableErrorMessage", BindingFlags.NonPublic | BindingFlags.Static);
                var renderables = (List<IRenderable>)mi.Invoke(null, new object[2] { ex, true });
                renderables.ForEach((x) => AnsiConsole.Console.Write(x));
                Environment.Exit(-2);
            }

            foreach (var a in newArgs.Commands)
            {
                switch (a.ToLower())
                {
                    case "all":
                        newArgs.OSS = "win7,linux";
                        newArgs.r2r_targets = "choco";
                        newArgs.r2r_build = true;
                        break;
                }
            }

            if (displaysHelp)
            {
                newArgs.displaysHelp = true;
                Environment.Exit(0);
            }

            return newArgs;
        }
    }

}

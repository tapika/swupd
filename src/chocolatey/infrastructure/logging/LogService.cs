using chocolatey.infrastructure.app;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace chocolatey.infrastructure.logging
{
    public class LogService
    {
        static public Logger console;
        static public Logger consoleHighlight;
        static public Logger consoleDebug;
        static public Logger consoleTrace;



        // Internal implementation names, don't use outside
        //const string normalConsoleLoggerName = "console";
        const string consoleTargetName = "console";
        const string normalConsoleLoggerName = "chocolatey";
        const string highlightedConsoleLoggerName = "Important";
        const string debugConsoleLoggerName = "Verbose";
        const string traceConsoleLoggerName = "Trace";
        const string fileLoggerName = "chocolog";
        const string fileSummaryLoggerName = "chocolog_summary";

        private static string DisabledName(string s)
        {
            return "#" + s;
        }

        public static void TraceAll(string loggerName)
        {
            var log = LogManager.GetLogger(loggerName);
            ILog log2 = loggerName.Log();
            log.Fatal("- Printing using logger '" + loggerName + "'");log2.Fatal("- Printing using logger '" + loggerName + "'");
            Console.WriteLine();
            log.Trace("trace 1");log2.Trace("trace 1");
            log.Debug("debug 2 ");log2.Debug("debug 2 ");
            log.Info("info 3");log2.Info("info 3");
            log.Warn("warn 4");log2.Warn("warn 4");
            log.Error("error 5");log2.Error("error 5");
            log.Fatal("fatal 6");log2.Fatal("fatal 6");
        }

        public static void TraceAll2<T>(T t)
        {
            Type logType = t.GetType();
            string loggerName = logType.FullName;
            var log = LogManager.GetLogger(loggerName);
            ILog log2 = t.Log();
            log.Fatal("- Printing using logger '" + loggerName + "'"); log2.Fatal("- Printing using logger '" + loggerName + "'");
            Console.WriteLine();
            log.Trace("trace 1"); log2.Trace("trace 1");
            log.Debug("debug 2 "); log2.Debug("debug 2 ");
            log.Info("info 3"); log2.Info("info 3");
            log.Warn("warn 4"); log2.Warn("warn 4");
            log.Error("error 5"); log2.Error("error 5");
            log.Fatal("fatal 6"); log2.Fatal("fatal 6");
        }


        //const string fileLogPatternLayout = @"${date:format=yyyy-MM-dd HH\:mm\:ss,fff} ${processid} [${uppercase:${level:padding=-5:alignmentOnTruncation=left}}] - ${message}";
        const string fileLogPatternLayout = @"${processid} [${uppercase:${level:padding=-5:alignmentOnTruncation=left}}] - ${message}";
        //const string convPatternDebug = "%property{pid}:%thread [%-5level] - %message - %file:%method:%line %newline";
        const string fileLogDebugPatternLayout = @"${processid}:${threadid} [${uppercase:${level:padding=-5:alignmentOnTruncation=left}}] - ${message} - " +
            "${callsite-filename:includeSourcePath=True}:" +
            "${callsite:classname=false:methodname=true}:" +
            "${callsite-linenumber}"
        ;





        static string outputDirectory;

        public static void configure(string outputDirectory = null)
        {
            LogService.outputDirectory = outputDirectory;
            string path = Path.Combine(ApplicationParameters.InstallLocation, "nlog.config");

            // Create initial configuration just so it would be easy to modify, and also watch from file system.
            if (!File.Exists(path))
            {
                File.WriteAllText(path,
@"<nlog throwExceptions='true' autoReload='true'>
  <targets>
    <target name='console' type='coloredConsole' />
  </targets>
  <rules>
     <!-- log first, and then discard the rest (so chocolatey* would not log it)
    <logger name='<class name>' minlevel='Error' writeTo='console' final=""true"" />
    <logger name='<class name>' final=""true"" />
    -->
   </rules>
</nlog>
");
            }

            var conf = new XmlLoggingConfiguration(path, LogManager.LogFactory);
            LogManager.ConfigurationReloaded += LogManager_ConfigurationReloaded;

            bool clearLogFile = true;
            //bool clearLogFile = ApplicationParameters.LogsAppendToFile;
            reconfigure(clearLogFile, conf);
            //Console.WriteLine("press any key...");
            //Console.ReadLine();
        }

        static Dictionary<string, LogLevel> defaultLogLevel;


        static void reconfigure(bool clearLogFile, LoggingConfiguration conf)
        {
            string logFile11 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingFile);
            string logFile12 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingSummaryFile);
            string logFile21 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingFile + "_2");
            string logFile22 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingSummaryFile + "_2");

            if (clearLogFile && File.Exists(logFile11)) { File.Delete(logFile11); }
            if (clearLogFile && File.Exists(logFile12)) { File.Delete(logFile12); }
            if (clearLogFile && File.Exists(logFile21)) { File.Delete(logFile21); }
            if (clearLogFile && File.Exists(logFile22)) { File.Delete(logFile22); }

            Log4NetAppenderConfiguration.configure(outputDirectory, ChocolateyLoggers.Trace.to_string());
            const string console2LoggerName = "console2";

            var consoletarget = new ColoredConsoleTarget() { Layout = "${message}", Name = consoleTargetName };
            var consoletarget2 = new ColoredConsoleTarget() { Layout = "${message}", Name = console2LoggerName };

            string andHighlightConsole = " and equals(logger, '" + highlightedConsoleLoggerName + "')";
            string andDebugConsole = " and equals(logger, '" + debugConsoleLoggerName + "')";
            string andTraceConsole = " and equals(logger, '" + traceConsoleLoggerName + "')";

            ((List<ConsoleRowHighlightingRule>)consoletarget.RowHighlightingRules).AddRange(new[] {
                new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.White, ConsoleOutputColor.Red),
                new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.Gray, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
                new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
            });

            ((List<ConsoleRowHighlightingRule>)consoletarget2.RowHighlightingRules).AddRange(new[] {
                // Originally copied from NLog, search from DefaultConsoleRowHighlightingRules
                new ConsoleRowHighlightingRule("level == LogLevel.Fatal" + andHighlightConsole, ConsoleOutputColor.White, ConsoleOutputColor.Red),
                new ConsoleRowHighlightingRule("level == LogLevel.Error" + andHighlightConsole, ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule("level == LogLevel.Warn" + andHighlightConsole, ConsoleOutputColor.Magenta, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule("level == LogLevel.Info" + andHighlightConsole, ConsoleOutputColor.Green, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Debug" + andHighlightConsole, ConsoleOutputColor.Blue, ConsoleOutputColor.Gray),
                new ConsoleRowHighlightingRule("level == LogLevel.Trace" + andHighlightConsole, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),

                new ConsoleRowHighlightingRule("level == LogLevel.Fatal" + andDebugConsole, ConsoleOutputColor.White, ConsoleOutputColor.Red),
                new ConsoleRowHighlightingRule("level == LogLevel.Error" + andDebugConsole, ConsoleOutputColor.Red, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Warn" + andDebugConsole, ConsoleOutputColor.DarkMagenta, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Info" + andDebugConsole, ConsoleOutputColor.DarkGreen, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Debug" + andDebugConsole, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
                new ConsoleRowHighlightingRule("level == LogLevel.Trace" + andDebugConsole, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),

                new ConsoleRowHighlightingRule("level == LogLevel.Fatal" + andTraceConsole, ConsoleOutputColor.White, ConsoleOutputColor.Red),
                new ConsoleRowHighlightingRule("level == LogLevel.Error" + andTraceConsole, ConsoleOutputColor.Red, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Warn" + andTraceConsole, ConsoleOutputColor.DarkMagenta, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Info" + andTraceConsole, ConsoleOutputColor.DarkGreen, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Debug" + andTraceConsole, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
                new ConsoleRowHighlightingRule("level == LogLevel.Trace" + andTraceConsole, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
            });

            var targets = conf.AllTargets;
            var targetNames = targets.Select(x => x.Name).ToList();
            Action<Target> replaceOrAddTarget = (Target t) =>
            {
                conf.AddTarget(t);

                if (targetNames.Contains(t.Name))
                {
                    foreach (var rule in conf.LoggingRules.ToArray())
                    {
                        var targets2 = rule.Targets;
                        var existingRule = targets2.Where(x => x.Name == t.Name).FirstOrDefault();
                        if (existingRule != null)
                        {
                            targets2.Remove(existingRule);
                            targets2.Add(t);
                        }
                    }
                }
            };

            defaultLogLevel = new Dictionary<string, LogLevel>();

            replaceOrAddTarget(consoletarget);
            replaceOrAddTarget(consoletarget2);

            Action<LogLevel, Target, string> addRule = (LogLevel minLogLevel, Target t, string name) =>
            {
                defaultLogLevel.Add(name, minLogLevel);
                conf.AddRule(minLogLevel, LogLevel.Fatal, t, name);
            };

            foreach (var rule in conf.LoggingRules)
            {
                foreach(var ll in LogLevel.AllLevels)
                {
                    if (rule.IsLoggingEnabledForLevel(ll) && !defaultLogLevel.ContainsKey(rule.LoggerNamePattern))
                    {
                        defaultLogLevel.Add(rule.LoggerNamePattern, ll);
                        break;
                    }
                }
            }

            //Trace, Debug are excluded
            addRule(LogLevel.Info, consoletarget, normalConsoleLoggerName + "*");
            addRule(LogLevel.Info, consoletarget2, highlightedConsoleLoggerName);
            //only Error and Fatal
            conf.AddRule(LogLevel.Error, LogLevel.Fatal, consoletarget2, debugConsoleLoggerName);
            conf.AddRule(LogLevel.Fatal, LogLevel.Fatal, consoletarget2, traceConsoleLoggerName);

            var filetargetDetailed = new FileTarget
            {
                Name = fileLoggerName,
                FileName = logFile21,
                Layout = fileLogPatternLayout,
                CreateDirs = true,
                AutoFlush = true,
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 1
            };

            var filetargetSummary = new FileTarget
            {
                Name = fileSummaryLoggerName,
                FileName = logFile22,
                Layout = fileLogPatternLayout,
                CreateDirs = true,
                AutoFlush = true,
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 1
            };

            var layerNameLevelList = new List<(string, LogLevel)>
            {
                (normalConsoleLoggerName + "*", LogLevel.Debug),
                (highlightedConsoleLoggerName, LogLevel.Debug),
                (debugConsoleLoggerName, LogLevel.Debug),
            };

            foreach (var layerLevel in layerNameLevelList)
            {
                conf.AddRule(layerLevel.Item2, LogLevel.Fatal, filetargetDetailed, layerLevel.Item1);
                conf.AddTarget(filetargetDetailed);

                conf.AddRule(LogLevel.Info, LogLevel.Fatal, filetargetSummary, layerLevel.Item1);
                conf.AddTarget(filetargetSummary);
            }


            conf.AddRule(LogLevel.Trace, LogLevel.Fatal, filetargetDetailed, DisabledName(traceConsoleLoggerName));
            replaceOrAddTarget(filetargetDetailed);
            conf.AddRule(LogLevel.Trace, LogLevel.Fatal, filetargetSummary, DisabledName(traceConsoleLoggerName));
            replaceOrAddTarget(filetargetSummary);

            LogManager.Configuration = conf;
            LogManager.ReconfigExistingLoggers();

            console = LogManager.GetLogger(normalConsoleLoggerName);
            consoleHighlight = LogManager.GetLogger(highlightedConsoleLoggerName);
            consoleDebug = LogManager.GetLogger(debugConsoleLoggerName);
            consoleTrace = LogManager.GetLogger(traceConsoleLoggerName);
        }

        /// <summary>
        /// Called when nlog.config gets saved on disk
        /// </summary>
        private static void LogManager_ConfigurationReloaded(object sender, LoggingConfigurationReloadedEventArgs e)
        {
            if (!e.Succeeded)
            {
                var exc = (NLogConfigurationException)e.Exception;

                Console.WriteLine(exc.Message + ":");
                Console.WriteLine("    " + exc.InnerException.Message);
                return;
            }

            reconfigure(false, LogManager.Configuration);
            Console.WriteLine("- logger configuration file reloaded");
            // Cannot be modified programmatically at the moment (unless re-inherit from XmlLoggingConfiguration and override Initialize/autoReloadDefault)
            if (!((XmlLoggingConfiguration)LogManager.Configuration).AutoReload)
            {
                Console.WriteLine("Warning: Autoreload disabled");
            }
        }


        /// <summary>
        /// Adjusts logging level options
        /// </summary>
        public static void adjustLogLevels(bool debug, bool verbose, bool trace)
        {
            var verboseAppenderName = "{0}LoggingColoredConsoleAppender".format_with(ChocolateyLoggers.Verbose.to_string());
            var traceAppenderName = "{0}LoggingColoredConsoleAppender".format_with(ChocolateyLoggers.Trace.to_string());

            Log4NetAppenderConfiguration.set_logging_level_debug_when_debug(debug, verboseAppenderName, traceAppenderName);
            Log4NetAppenderConfiguration.set_verbose_logger_when_verbose(verbose, debug, verboseAppenderName);
            Log4NetAppenderConfiguration.set_trace_logger_when_trace(trace, traceAppenderName);

            LogLevel minLogLevelDebug = null;

            if (trace)
            { 
                minLogLevelDebug = LogLevel.Trace;
            } else
            {
                if (verbose)
                {
                    if (debug)
                    {
                        minLogLevelDebug = LogLevel.Debug;
                    }
                    else
                    {
                        minLogLevelDebug = LogLevel.Info;
                    }
                }
                else
                {
                    minLogLevelDebug = LogLevel.Error;
                }
            }

            var conf = LogManager.Configuration;
            var rulesList = conf.LoggingRules.ToList();
            for(int i = 0; i < rulesList.Count; i++)
            {
                var rule = rulesList[i];

                if (rule.Targets.Count == 0)
                {
                    continue;
                }

                var target = rule.Targets.First();
                var name = target.Name;
                bool moreDetails = name == fileLoggerName;

                LogLevel minLogLevelConsole = null;

                if (trace)
                {
                    minLogLevelConsole = (moreDetails) ? LogLevel.Trace: LogLevel.Info;
                }
                else
                {
                    minLogLevelConsole = LogLevel.Info;

                    if (verbose)
                    {
                        minLogLevelConsole = LogLevel.Debug;
                    }

                    if (debug)
                    {
                        minLogLevelConsole = LogLevel.Debug;
                    }
                }

                LogLevel minLogLevelFile;
                bool useDefLogLevel = defaultLogLevel.ContainsKey(rule.LoggerNamePattern);
                bool userDefinedLogRule = rule.LoggerNamePattern != normalConsoleLoggerName + "*" && rule.LoggerNamePattern != highlightedConsoleLoggerName;

                if (useDefLogLevel)
                {
                    if (!userDefinedLogRule || debug || trace || verbose)
                    {
                        useDefLogLevel = !moreDetails;
                    }
                }

                if (useDefLogLevel)
                {
                    minLogLevelFile = defaultLogLevel[rule.LoggerNamePattern];
                }
                else
                {
                    if (trace)
                    {
                        if (moreDetails)
                        {
                            minLogLevelFile = LogLevel.Trace;
                        }
                        else
                        {
                            minLogLevelFile = LogLevel.Info;
                        }
                    }
                    else
                    {
                        minLogLevelFile = (name == fileSummaryLoggerName) ? LogLevel.Info : LogLevel.Debug;
                    }
                }


                if (name == fileLoggerName || name == fileSummaryLoggerName)
                {
                    FileTarget filetarget = ((FileTarget)target);
                    if (trace)
                    {
                        if (moreDetails)
                        { 
                            filetarget.Layout = fileLogDebugPatternLayout;
                        }

                        if (rule.LoggerNamePattern == DisabledName(traceConsoleLoggerName))
                        {
                            rule.LoggerNamePattern = traceConsoleLoggerName;
                        }
                    }
                    else
                    {
                        if (moreDetails)
                        { 
                            filetarget.Layout = fileLogPatternLayout;
                        }

                        if (rule.LoggerNamePattern == traceConsoleLoggerName)
                        {
                            rule.LoggerNamePattern = DisabledName(traceConsoleLoggerName);
                        }
                    }

                    rule.SetLoggingLevels(minLogLevelFile, LogLevel.Fatal);
                    continue;
                }

                if (rule.LoggerNamePattern == highlightedConsoleLoggerName)
                {
                    rule.SetLoggingLevels(minLogLevelConsole, LogLevel.Fatal);
                    continue;
                }

                if (rule.LoggerNamePattern == debugConsoleLoggerName)
                {
                    rule.SetLoggingLevels(minLogLevelDebug, LogLevel.Fatal);
                    continue;
                }

                if (rule.LoggerNamePattern == traceConsoleLoggerName)
                {
                    rule.SetLoggingLevels(minLogLevelConsole, LogLevel.Fatal);
                    continue;
                }

                if (name == consoleTargetName)
                {
                    rule.SetLoggingLevels(minLogLevelFile, LogLevel.Fatal);
                }
            }

            //conf.LoggingRules.Where(x => x.LoggerNamePattern == normalConsoleLoggerName).Select(x => conf.LoggingRules.Remove(x));
            LogManager.ReconfigExistingLoggers();
        }

        public static void Test()
        {
            //adjustLogLevels(true, false, false);
            for (int i = 0; i < 8; i++)
            {
                bool debug = (i & 1) == 1;
                bool verbose = (i & 2) == 2;
                bool trace = (i & 4) == 4;
                string s = i + ") Adjusting levels to debug=" + debug + " verbose=" + verbose + " trace=" + trace;
                console.Fatal(s); "chocolatey".Log().Fatal(s);
                adjustLogLevels(debug, verbose, trace);

                TraceAll(normalConsoleLoggerName);
                TraceAll(highlightedConsoleLoggerName);
                TraceAll(debugConsoleLoggerName);
                TraceAll(traceConsoleLoggerName);
                TraceAll("chocolatey.infrastructure.logging.LogService");
                TraceAll("chocolatey.infrastructure.logging.LogService2");
            }

            Environment.Exit(2);
        }

    }
}

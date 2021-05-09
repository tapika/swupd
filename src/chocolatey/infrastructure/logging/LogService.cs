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

#if USE_LOG4NET
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
#endif

        static string dateFormatString = @"${date:format=yyyy-MM-dd HH\:mm\:ss,fff} ";
        static string fileLogPatternLayout = @"${processid} [${uppercase:${level:padding=-5:alignmentOnTruncation=left}}] - ${message}";
        const string fileLogDebugPatternLayout = @"${processid}:${threadid} [${uppercase:${level:padding=-5:alignmentOnTruncation=left}}] - ${message} - " +
            "${callsite-filename:includeSourcePath=True}:" +
            "${callsite:classname=false:methodname=true}:" +
            "${callsite-linenumber}"
        ;


        static string outputDirectory;

        public static void configure(string outputDirectory = null)
        {
            if (outputDirectory != null)
            { 
                LogService.outputDirectory = outputDirectory;
            }
            string path = Path.Combine(ApplicationParameters.InstallLocation, "nlog.config");

            // Create initial configuration just so it would be easy to modify, and also watch from file system.
            if (!File.Exists(path))
            {
                File.WriteAllText(path,
@"<nlog throwExceptions='true' autoReload='true'>
  <targets>
    <target name='console' type='coloredConsole' />
    <target name='chocolog' type='null' />
    <target name='chocolog_summary' type='null' />
  </targets>
  <rules>
     <!-- log first, and then discard the rest (so chocolatey* would not log it)
    <logger name='<class name>' minlevel='Error' writeTo='console,chocolog,chocolog_summary' final=""true"" />
    <logger name='<class name>' final=""true"" />
    -->
   </rules>
</nlog>
");
            }

            var conf = new XmlLoggingConfiguration(path, LogManager.LogFactory);
            LogManager.ConfigurationReloaded -= LogManager_ConfigurationReloaded;
            LogManager.ConfigurationReloaded += LogManager_ConfigurationReloaded;

#if USE_LOG4NET
            const bool clearLogFile = true;
#else
            const bool clearLogFile = false;

            // If you wish to disable date/time formatting
            //dateFormatString = "";
#endif
            //bool clearLogFile = ApplicationParameters.LogsAppendToFile;
            reconfigure(clearLogFile, conf);
            //Console.WriteLine("press any key...");
            //Console.ReadLine();
        }

        /// <summary>
        /// Enables or disables colors (Normally called before adjustLogLevels)
        /// </summary>
        /// <param name="b">true to enable</param>
        public static void enableColors(bool b = true)
        {
            colorsEnabled = b;
            configure();
        }

        static bool colorsEnabled = true;
        static Dictionary<string, LogLevel> defaultLogLevel;


        static void reconfigure(bool clearLogFile, LoggingConfiguration conf)
        {
#if USE_LOG4NET
            string logFile11 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingFile);
            string logFile12 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingSummaryFile);
            const string logSuffix = "_2";
#else
            const string logSuffix = "";
#endif
            string logFile21 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingFile + logSuffix);
            string logFile22 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingSummaryFile + logSuffix);

#if USE_LOG4NET
            if (clearLogFile && File.Exists(logFile11)) { File.Delete(logFile11); }
            if (clearLogFile && File.Exists(logFile12)) { File.Delete(logFile12); }
#endif
            if (clearLogFile && File.Exists(logFile21)) { File.Delete(logFile21); }
            if (clearLogFile && File.Exists(logFile22)) { File.Delete(logFile22); }

#if USE_LOG4NET
            Log4NetAppenderConfiguration.configure(outputDirectory, ChocolateyLoggers.Trace.to_string());
#endif
            const string console2LoggerName = "console2";

            TargetWithLayoutHeaderAndFooter consoletarget;
            TargetWithLayoutHeaderAndFooter consoletarget2;

            if (colorsEnabled)
            {
                var cct1 = new ColoredConsoleTarget() { Layout = "${message}", Name = consoleTargetName };
                consoletarget = cct1;
                var cct2 = new ColoredConsoleTarget() { Layout = "${message}", Name = console2LoggerName };
                consoletarget2 = cct2;

                ((List<ConsoleRowHighlightingRule>)cct1.RowHighlightingRules).AddRange(new[] {
                    new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.White, ConsoleOutputColor.Red),
                    new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
                    new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange),
                    new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.Gray, ConsoleOutputColor.Black),
                    new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
                    new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
                });

                string andHighlightConsole = " and equals(logger, '" + highlightedConsoleLoggerName + "')";
                string andDebugConsole = " and equals(logger, '" + debugConsoleLoggerName + "')";
                string andTraceConsole = " and equals(logger, '" + traceConsoleLoggerName + "')";

                ((List<ConsoleRowHighlightingRule>)cct2.RowHighlightingRules).AddRange(new[] {
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
            }
            else 
            { 
                consoletarget = new ConsoleTarget() { Layout = "${message}", Name = consoleTargetName };
                consoletarget2 = new ConsoleTarget() { Layout = "${message}", Name = console2LoggerName };
            }

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

            // If any rule has multiple targets, we make multiple rules, so can reconfigure each rule levels individually.
            if (conf.LoggingRules.Any(x => x.Targets.Count > 1))
            {
                foreach (var rule in conf.LoggingRules.ToList())
                {
                    LogLevel min = LogLevel.AllLevels.Where(x => rule.IsLoggingEnabledForLevel(x)).First();

                    foreach (var t in rule.Targets)
                    {
                        conf.AddRule(min, LogLevel.Fatal, t, rule.LoggerNamePattern, rule.Final);
                    }

                    if (rule.Targets.Count == 0)
                    { 
                        conf.LoggingRules.Add(rule);
                    }

                    conf.LoggingRules.Remove(rule);
                }
            }


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
                Layout = dateFormatString + fileLogPatternLayout,
                CreateDirs = true,
                AutoFlush = true,
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 1
            };

            var filetargetSummary = new FileTarget
            {
                Name = fileSummaryLoggerName,
                FileName = logFile22,
                Layout = dateFormatString + fileLogPatternLayout,
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
#if USE_LOG4NET
            var verboseAppenderName = "{0}LoggingColoredConsoleAppender".format_with(ChocolateyLoggers.Verbose.to_string());
            var traceAppenderName = "{0}LoggingColoredConsoleAppender".format_with(ChocolateyLoggers.Trace.to_string());

            Log4NetAppenderConfiguration.set_logging_level_debug_when_debug(debug, verboseAppenderName, traceAppenderName);
            Log4NetAppenderConfiguration.set_verbose_logger_when_verbose(verbose, debug, verboseAppenderName);
            Log4NetAppenderConfiguration.set_trace_logger_when_trace(trace, traceAppenderName);
#endif
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
                LogLevel loglevel = null;

                if (rule.Targets.Count == 0)
                {
                    continue;
                }

                var target = rule.Targets.First();
                var name = target.Name;
                bool moreDetails = name == fileLoggerName;

                LogLevel minLogLevelConsole = null;
                LogLevel minLogLevelConsoleHighlight = null;
                LogLevel minLogLevelTrace = null;

                if (trace)
                {
                    minLogLevelConsole = LogLevel.Trace;
                    minLogLevelConsoleHighlight = LogLevel.Trace;
                    minLogLevelTrace = LogLevel.Trace;
                }
                else
                {
                    minLogLevelConsole = LogLevel.Info;
                    minLogLevelConsoleHighlight = LogLevel.Info;
                    minLogLevelTrace = LogLevel.Fatal;

                    if (verbose)
                    {
                        minLogLevelConsole = LogLevel.Debug;
                        minLogLevelConsoleHighlight = LogLevel.Debug;
                        minLogLevelTrace = LogLevel.Fatal;
                    }

                    if (debug)
                    {
                        minLogLevelConsole = LogLevel.Debug;
                        minLogLevelConsoleHighlight = LogLevel.Debug;
                        minLogLevelTrace = LogLevel.Fatal;
                    }
                }

                if (defaultLogLevel.ContainsKey(rule.LoggerNamePattern) && !(debug || trace || verbose))
                {
                    loglevel = defaultLogLevel[rule.LoggerNamePattern];
                }
                else
                {
                    loglevel = minLogLevelConsole;
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
                            filetarget.Layout = dateFormatString + fileLogDebugPatternLayout;
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
                            filetarget.Layout = dateFormatString + fileLogPatternLayout;
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
                    rule.SetLoggingLevels(minLogLevelConsoleHighlight, LogLevel.Fatal);
                    continue;
                }

                if (rule.LoggerNamePattern == debugConsoleLoggerName)
                {
                    rule.SetLoggingLevels(minLogLevelDebug, LogLevel.Fatal);
                    continue;
                }

                if (rule.LoggerNamePattern == traceConsoleLoggerName)
                {
                    rule.SetLoggingLevels(minLogLevelTrace, LogLevel.Fatal);
                    continue;
                }

                if (name == consoleTargetName)
                {
                    rule.SetLoggingLevels(loglevel, LogLevel.Fatal);
                }
            }

            //conf.LoggingRules.Where(x => x.LoggerNamePattern == normalConsoleLoggerName).Select(x => conf.LoggingRules.Remove(x));
            LogManager.ReconfigExistingLoggers();
        }

#if USE_LOG4NET
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
#endif

    }
}

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
        const string normalConsoleLoggerName = "chocolatey";
        const string highlightedConsoleLoggerName = "Important";
        const string debugConsoleLoggerName = "Verbose";
        const string traceConsoleLoggerName = "Trace";


        public static void TraceAll(string loggerName)
        {
            var log = LogManager.GetLogger(loggerName);
            var log2 = loggerName.Log();

            log.Fatal("- Printing using logger '" + loggerName + "'");
            log2.Fatal("- Printing using logger '" + loggerName + "'");
            Console.WriteLine();
            log.Trace("trace 1");
            log2.Trace("trace 1");
            log.Debug("debug 2 ");
            log2.Debug("debug 2 ");
            log.Info("info 3");
            log2.Info("info 3");
            log.Warn("warn 4");
            log2.Warn("warn 4");
            log.Error("error 5");
            log2.Error("error 5");
            log.Fatal("fatal 6");
            log2.Fatal("fatal 6");
        }

        public static void configure(string outputDirectory = null)
        {
            string logFile11 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingFile);
            string logFile12 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingSummaryFile);
            string logFile21 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingFile + "_2");
            string logFile22 = Path.Combine(Path.GetFullPath(outputDirectory), ApplicationParameters.LoggingSummaryFile + "_2");

            bool clearLogFile = true;
            //bool clearLogFile = ApplicationParameters.LogsAppendToFile;

            if (clearLogFile && File.Exists(logFile11)) { File.Delete(logFile11); }
            if (clearLogFile && File.Exists(logFile12)) { File.Delete(logFile12); }
            if (clearLogFile && File.Exists(logFile21)) { File.Delete(logFile21); }
            if (clearLogFile && File.Exists(logFile22)) { File.Delete(logFile22); }

            Log4NetAppenderConfiguration.configure(outputDirectory, ChocolateyLoggers.Trace.to_string());
            const string consoleLoggerName = "console";

            var conf = new LoggingConfiguration();
            var consoletarget = new ColoredConsoleTarget() { Layout = "${message}", Name = consoleLoggerName };

            var list = (List<ConsoleRowHighlightingRule>) consoletarget.RowHighlightingRules;
            string andNormalConsole = " and equals(logger, '" + normalConsoleLoggerName + "')";
            string andHighlightConsole = " and equals(logger, '" + highlightedConsoleLoggerName + "')";
            string andDebugConsole = " and equals(logger, '" + debugConsoleLoggerName + "')";
            string andTraceConsole = " and equals(logger, '" + traceConsoleLoggerName + "')";
            list.AddRange(new[] {
                // Originally copied from NLog, search from DefaultConsoleRowHighlightingRules
                new ConsoleRowHighlightingRule("level == LogLevel.Fatal" + andNormalConsole, ConsoleOutputColor.White, ConsoleOutputColor.Red),
                new ConsoleRowHighlightingRule("level == LogLevel.Error" + andNormalConsole, ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule("level == LogLevel.Warn" + andNormalConsole, ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule("level == LogLevel.Info" + andNormalConsole, ConsoleOutputColor.Gray, ConsoleOutputColor.Black),
                new ConsoleRowHighlightingRule("level == LogLevel.Debug" + andNormalConsole, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),
                new ConsoleRowHighlightingRule("level == LogLevel.Trace" + andNormalConsole, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Gray),

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

            conf.AddTarget(consoletarget);
            //Trace, Debug are excluded
            conf.AddRule(LogLevel.Info, LogLevel.Fatal,consoletarget, normalConsoleLoggerName);
            conf.AddRule(LogLevel.Info, LogLevel.Fatal,consoletarget, highlightedConsoleLoggerName);
            //only Error and Fatal
            conf.AddRule(LogLevel.Error, LogLevel.Fatal,consoletarget, debugConsoleLoggerName);
            conf.AddRule(LogLevel.Fatal, LogLevel.Fatal,consoletarget, traceConsoleLoggerName);

            var filetarget1 = new FileTarget
            {
                Name = "chocolog",
                FileName = logFile21,
                //Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss,fff} ${processid} [${uppercase:${level:padding=-5:alignmentOnTruncation=left}}] - ${message}",
                Layout = @"${processid} [${uppercase:${level:padding=-5:alignmentOnTruncation=left}}] - ${message}",
                CreateDirs = true,
                AutoFlush = true,
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 1
            };

            var filetarget2 = new FileTarget
            {
                Name = "chocolog_summary",
                FileName = logFile22,
                Layout = "${message}",
                CreateDirs = true,
                AutoFlush = true,
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 1
            };

            conf.AddRule(LogLevel.Debug, LogLevel.Fatal, filetarget1);
            conf.AddTarget(filetarget1);
            
            conf.AddRule(LogLevel.Info, LogLevel.Fatal, filetarget2);
            conf.AddTarget(filetarget2);

            // Uncomment if you suspect that something does not work correctly.
            //NLog.LogManager.ThrowConfigExceptions = true;
            LogManager.Configuration = conf;
            console = LogManager.GetLogger(normalConsoleLoggerName);
            consoleHighlight = LogManager.GetLogger(highlightedConsoleLoggerName);
            consoleDebug = LogManager.GetLogger(debugConsoleLoggerName);
            consoleTrace = LogManager.GetLogger(traceConsoleLoggerName);
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

            LogLevel minLogLevel = null;
            LogLevel minLogLevelDebug = null;

            if (trace)
            {
                minLogLevelDebug = minLogLevel = LogLevel.Trace;
            }
            else {
                minLogLevel = LogLevel.Info;

                if (verbose)
                { 
                    minLogLevel = LogLevel.Debug;
                }

                if (debug)
                {
                    minLogLevel = LogLevel.Debug;
                    minLogLevelDebug = LogLevel.Debug;
                }
                else
                { 
                    minLogLevelDebug = LogLevel.Info;
                }
            }

            var conf = LogManager.Configuration;
            foreach (var rule in conf.LoggingRules.ToList())
            {
                if (rule.LoggerNamePattern == normalConsoleLoggerName)
                {
                    conf.AddRule(minLogLevel, LogLevel.Fatal, rule.Targets.First(), rule.LoggerNamePattern);
                    conf.LoggingRules.Remove(rule);
                }

                if (rule.LoggerNamePattern == highlightedConsoleLoggerName)
                {
                    conf.AddRule(minLogLevel, LogLevel.Fatal, rule.Targets.First(), rule.LoggerNamePattern);
                    conf.LoggingRules.Remove(rule);
                }

                if (rule.LoggerNamePattern == debugConsoleLoggerName && verbose)
                {
                    conf.AddRule(minLogLevelDebug, LogLevel.Fatal, rule.Targets.First(), rule.LoggerNamePattern);
                    conf.LoggingRules.Remove(rule);
                }

                if (rule.LoggerNamePattern == traceConsoleLoggerName && trace)
                {
                    conf.AddRule(minLogLevel, LogLevel.Fatal, rule.Targets.First(), rule.LoggerNamePattern);
                    conf.LoggingRules.Remove(rule);
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
                console.Fatal("Adjusting levels to debug=" + debug + " verbose=" + verbose + " trace=" + trace);
                "chocolatey".Log().Fatal("Adjusting levels to debug=" + debug + " verbose=" + verbose + " trace=" + trace);
                adjustLogLevels(debug, verbose, trace);

                TraceAll(normalConsoleLoggerName);
                TraceAll(highlightedConsoleLoggerName);
                TraceAll(debugConsoleLoggerName);
                TraceAll(traceConsoleLoggerName);
                TraceAll("Unknown");
            }

            Environment.Exit(2);
        }

    }
}

using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace chocolatey.infrastructure.logging
{
    /// <summary>
    /// Basic class for switching logging from one verifying log to another one, after dispose 
    /// original verifying log context would be restored
    /// 
    /// Useful for multitasked testing, when second task is responsible for different thing than main
    /// task waiting for it's completion.
    /// 
    /// Usage:
    ///     ... in new Task ...    
    ///     using( new VerifyingLog() )
    ///     {
    ///     
    ///     }
    /// 
    /// </summary>
    public class VerifyingLog: IDisposable
    {
        Target oldTarget = null;
        public const string LogName = nameof(VerifyingLog);
        string oldLoggerName;

        public VerifyingLog(
            string variantContext = "",
            bool createLog = false, 
            [CallerFilePath] string sourcePath = "", 
            [CallerMemberName] string func = "",
            Assembly asm = null)
        {
            if (asm == null)
            {
                asm = Assembly.GetCallingAssembly();
            }

            if (variantContext.Length != 0)
            {
                variantContext = $"_{variantContext}";
            }

            var conf = LogService.Instance.LogFactory.Configuration;
            oldTarget = conf.FindTargetByName(LogName);

            // If we have some logging active - we remove all rules to that specific target - and this will disable 
            // logging, but will keep VerifyingLogTarget alive. Later on in Dispose we restore VerifyingLogTarget
            // name and rules.
            if (oldTarget != null)
            {
                var list = conf.AllTargets.ToList();
                for (int i = 1; ; i++)
                {
                    oldLoggerName = $"{LogName}_old_{i}"; // Rename VerifyingLogTarget =>
                                                          // VerifyingLogTarget_old_1, VerifyingLogTarget_old_2 and so on...
                    if (list.Where(x => x.Name == oldLoggerName).FirstOrDefault() == null)
                    {
                        break;
                    }
                }

                oldTarget.Name = oldLoggerName;

                var loggingRules = conf.LoggingRules;
                for (int i = 0; i < loggingRules.Count; i++)
                {
                    if (loggingRules[i].Targets.Contains(oldTarget))
                    {
                        loggingRules.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
            }

            string srcpath = CallerFilePathHelper.CallerFilePathToSolutionSourcePath(sourcePath, asm);
            string srcdir = Path.GetDirectoryName(srcpath);
            string logdir = Path.Combine(srcdir,Path.GetFileNameWithoutExtension(srcpath));
            string targetLogPath = Path.Combine(logdir, $"{func}{variantContext}.txt");
            ApplyTarget(new VerifyingLogTarget(LogName, targetLogPath, createLog), false);
        }

        public void Dispose()
        {
            LogService.Instance.LogFactory.Configuration.RemoveTarget(LogName);
            ApplyTarget(oldTarget, true);
        }

        void ApplyTarget(Target logTarget, bool restoreOld)
        {
            if (logTarget == null)
            {
                return;
            }

            var factory = LogService.GetInstance(false).LogFactory;
            var conf = factory.Configuration;
            if (!restoreOld)
            {
                conf.AddTarget(logTarget);

                foreach (var name in LogService.GetLoggerNames())
                {
                    conf.AddRule(LogLevel.Info, LogLevel.Fatal, logTarget, name);
                }
            }
            else
            {
                oldTarget.Name = LogName;

                foreach (var name in LogService.GetLoggerNames())
                {
                    conf.AddRule(LogLevel.Info, LogLevel.Fatal, logTarget, name);
                }
            }

            factory.ReconfigExistingLoggers();
        }
    }
}

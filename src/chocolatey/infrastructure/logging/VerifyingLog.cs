using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
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

            var factory = LogService.GetInstance(false).LogFactory;
            var conf = factory.Configuration;
            oldTarget = conf.FindTargetByName(LogName);
            conf.RemoveTarget(LogName);
            string path = CallerFilePathHelper.CallerFilePathToSolutionSourcePath(sourcePath, asm);
            ApplyTarget(new VerifyingLogTarget(LogName, $"{path}_{func}{variantContext}.txt", createLog));
        }

        public void Dispose()
        {
            ApplyTarget(oldTarget);
        }

        void ApplyTarget(Target logTarget)
        {
            if (logTarget == null)
            {
                return;
            }

            var factory = LogService.GetInstance(false).LogFactory;
            var conf = factory.Configuration;
            conf.AddTarget(logTarget);

            foreach (var name in LogService.GetLoggerNames())
            {
                conf.AddRule(LogLevel.Info, LogLevel.Fatal, logTarget, name);
            }
            factory.ReconfigExistingLoggers();
        }
    }
}

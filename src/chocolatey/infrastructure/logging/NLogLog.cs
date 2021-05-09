namespace chocolatey.infrastructure.logging
{
    using NLog;
    using System;
    using System.Runtime;

    // ReSharper disable InconsistentNaming

    /// <summary>
    ///   Log4net logger implementing special ILog class
    /// </summary>
    public sealed class NLogLog : ILog, ILog<NLogLog>
    {
        private Logger _logger;
        // ignore Log4NetLog in the call stack
        private static readonly Type _declaringType = typeof(NLogLog);

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void InitializeFor(string loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Debug(string message, params object[] formatting)
        {
            if (_logger.IsDebugEnabled) Log(LogLevel.Debug, decorate_message_with_audit_information(message), formatting);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Debug(Func<string> message)
        {
            if (_logger.IsDebugEnabled) Log(LogLevel.Debug, decorate_message_with_audit_information(message.Invoke()).escape_curly_braces());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Info(string message, params object[] formatting)
        {
            if (_logger.IsInfoEnabled) Log(LogLevel.Info, decorate_message_with_audit_information(message), formatting);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Info(Func<string> message)
        {
            if (_logger.IsInfoEnabled) Log(LogLevel.Info, decorate_message_with_audit_information(message.Invoke()).escape_curly_braces());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Warn(string message, params object[] formatting)
        {
            if (_logger.IsWarnEnabled) Log(LogLevel.Warn, decorate_message_with_audit_information(message), formatting);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Warn(Func<string> message)
        {
            if (_logger.IsWarnEnabled) Log(LogLevel.Warn, decorate_message_with_audit_information(message.Invoke()).escape_curly_braces());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Error(string message, params object[] formatting)
        {
            // don't need to check for enabled at this level
            Log(LogLevel.Error, decorate_message_with_audit_information(message), formatting);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Error(Func<string> message)
        {
            // don't need to check for enabled at this level
            Log(LogLevel.Error, decorate_message_with_audit_information(message.Invoke()).escape_curly_braces());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Fatal(string message, params object[] formatting)
        {
            // don't need to check for enabled at this level
            Log(LogLevel.Fatal, decorate_message_with_audit_information(message), formatting);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Fatal(Func<string> message)
        {
            // don't need to check for enabled at this level
            Log(LogLevel.Fatal, decorate_message_with_audit_information(message.Invoke()).escape_curly_braces());
        }

        public string decorate_message_with_audit_information(string message)
        {
            return message;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void Log(LogLevel level, string message, params object[] args)
        {
            _logger.Log(level, message, args);
        }

    }

    // ReSharper restore InconsistentNaming
}

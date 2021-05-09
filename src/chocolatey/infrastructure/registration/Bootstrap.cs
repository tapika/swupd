// Copyright © 2017 - 2018 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.registration
{
    using System;
    using app;
#if USE_LOG4NET
    using log4net;
#else
    using NLog;
#endif
    using logging;
#if USE_LOG4NET
    using ILog = log4net.ILog;
#endif

    /// <summary>
    ///   Application bootstrapping - sets up logging and errors for the app domain
    /// </summary>
    public sealed class Bootstrap
    {
#if USE_LOG4NET
        private static readonly ILog _logger = LogManager.GetLogger(typeof (Bootstrap));
#endif
        /// <summary>
        ///   Initializes this instance.
        /// </summary>
        public static void initialize()
        {
#if USE_LOG4NET
            Log.InitializeWith<Log4NetLog>();
#else
            Log.InitializeWith<NLogLog>();
#endif
        }

        /// <summary>
        ///   Startups this instance.
        /// </summary>
        public static void startup()
        {
            AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;
        }

        /// <summary>
        ///   Handles unhandled exception for the application domain.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///   The <see cref="System.UnhandledExceptionEventArgs" /> instance containing the event data.
        /// </param>
// ReSharper disable InconsistentNaming
        private static void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
// ReSharper restore InconsistentNaming
        {
            var ex = e.ExceptionObject as Exception;
            string exceptionMessage = string.Empty;
            if (ex != null)
            {
                exceptionMessage = ex.ToString();
            }
#if !USE_LOG4NET
            var _logger = LogService.console;
            _logger.Error(
#else
            _logger.ErrorFormat(
#endif
                "{0} had an error on {1} (with user {2}):{3}{4}",
                                ApplicationParameters.Name,
                                Environment.MachineName,
                                Environment.UserName,
                                Environment.NewLine,
                                exceptionMessage
                );
        }

        /// <summary>
        ///   Shutdowns this instance.
        /// </summary>
        public static void shutdown()
        {
        }
    }
}
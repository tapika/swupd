using chocolatey.infrastructure.logging;
using logtesting;
using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace chocolatey.tests2
{
    [Parallelizable(ParallelScope.All)]
    public class Logging: LogTesting
    {
        [LogTest]
        public void Multiline()
        {
            LogService.console.Info("line1\nline2");
            LogService.console.Info("line3\rline4");
            LogService.console.Info("line5\r\nline6");
            LogService.console.Info("");
            LogService.console.Info("line7");
        }

    }
}

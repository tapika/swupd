using chocolatey.infrastructure.logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace chocolatey.tests2
{
    public class LogScope : IDisposable
    {
        public LogScope(string line)
        {
            LogService.console.Info("- " + line + ":");
        }

        public void Dispose()
        {
            LogService.console.Info("");
        }
    }
}

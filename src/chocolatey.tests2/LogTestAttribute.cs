using chocolatey;
using chocolatey.infrastructure.logging;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace logtesting
{
    public class LogTestAttribute : TestAttribute
    {
        public string FilePath { get; }
        public bool CreateLog { get; }

        public LogTestAttribute(bool createLog = false, [CallerFilePath] string filePath = "")
        {
            FilePath = filePath;
            CreateLog = createLog;
        }

        public string GetFilePath(MemberInfo mi)
        {
            return CallerFilePathHelper.CallerFilePathToSolutionSourcePath(FilePath, mi.DeclaringType.Assembly);
        }
    }
}

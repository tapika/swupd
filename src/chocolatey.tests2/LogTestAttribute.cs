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

        /// <summary>
        /// End-user can override this callback function to specify how to locate SolutionDir from assembly path which he has.
        /// </summary>
        public Func<Assembly, string, string> TranslateAssemblyPathToSolutionPath = TranslatePath;

        public LogTestAttribute(bool createLog = false, [CallerFilePath] string filePath = "")
        {
            FilePath = filePath;
            CreateLog = createLog;
        }

        public string GetFilePath(MemberInfo mi)
        {
            if (Path.IsPathRooted(FilePath))
            {
                return FilePath;
            }

            var asm = mi.DeclaringType.Assembly;
            string dir = Path.GetDirectoryName(asm.Location);
            string localPath = FilePath.Substring(2).Replace('/', Path.DirectorySeparatorChar);
            string path = Path.GetFullPath(Path.Combine(dir, TranslateAssemblyPathToSolutionPath(asm, localPath)));
            return path;
        }

        static string TranslatePath(Assembly asm, string path)
        {
            return Path.Combine(@"..\..\..", path);
        }
    }
}

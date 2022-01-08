using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace chocolatey
{
    public static class CallerFilePathHelper
    {
        /// <summary>
        /// End-user can override this callback function to specify how to locate SolutionDir from assembly path which he has.
        /// </summary>
        public static Func<Assembly, string, string> TranslateAssemblyPathToSolutionPath = TranslatePath;

        /// <summary>
        /// Translates relative [CallerFilePath] to absolute,source file path in solution.
        /// </summary>
        /// <param name="path">Path to translate</param>
        /// <param name="asm">assembly where </param>
        /// <returns></returns>
        static public string CallerFilePathToSolutionSourcePath(string path, Assembly asm = null)
        {
            if (asm == null)
            {
                asm = Assembly.GetCallingAssembly();
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            string dir = Path.GetDirectoryName(asm.Location);
            string localPath = path.Substring(2).Replace('/', Path.DirectorySeparatorChar);
            string newpath = Path.GetFullPath(Path.Combine(dir, TranslateAssemblyPathToSolutionPath(asm, localPath)));
            return newpath;
        }

        static string TranslatePath(Assembly asm, string path)
        {
            return Path.Combine(@"..\..\..", path);
        }
    }
}

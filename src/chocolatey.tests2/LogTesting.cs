namespace logtesting
{
    using NUnit.Framework;
    using System.Reflection;
    using chocolatey.infrastructure.logging;
    using NUnit.Framework.Internal;
    using chocolatey.infrastructure.app.configuration;
    using chocolatey.infrastructure.app;
    using System.IO;
    using System.Threading.Tasks;
    using System;
    using chocolatey.tests.integration;
    using chocolatey.tests2;
    using NuGet;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;

    [TestFixture]
    public class LogTesting
    {
        /// <summary>
        /// Accesses private class type via reflection.
        /// </summary>
        /// <param name="_o">input object</param>
        /// <param name="propertyPath">List of properties in one string, comma separated.</param>
        /// <returns>output object</returns>
        object getPrivate(object _o, string propertyPath)
        {
            object o = _o;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            
            foreach (var name in propertyPath.Split('.'))
            {
                System.Type type = o.GetType();

                if (char.IsUpper(name[0]))
                    o = type.GetProperty(name, flags).GetValue(o);
                else
                    o = type.GetField(name, flags).GetValue(o);
            }

            return o;
        }

        (LogTestAttribute,MemberInfo) getTestMethodInfo()
        {
            // https://stackoverflow.com/questions/19147236/how-can-i-get-access-to-the-current-test-method-in-nunit
            var mi = (MemberInfo)getPrivate(TestContext.CurrentContext.Test, "_test.Method.MethodInfo");
            LogTestAttribute attr = mi.GetCustomAttribute<LogTestAttribute>();
            return (attr,mi);
        }

        [SetUp]
        public void EachSpecSetup()
        {
            // Tasks might re-use existing thread, cleanup all logging services to start from scratch
            LogService.Instance = null;
            InstallContext.Instance = null;

            var mi = getTestMethodInfo();
            if (mi.Item1 == null)
            {
                return;
            }

            new VerifyingLog("", mi.Item1.CreateLog,  mi.Item1.FilePath, 
                TestContext.CurrentContext.Test.MethodName, mi.Item2.DeclaringType.Assembly);
        }


        [TearDown]
        public void EachSpecTearDown()
        {
            var mi = getTestMethodInfo();
            if (mi.Item1 == null)
            {
                return;
            }

            if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed)
            { 
                LogService.console.Info("end of test");
            }

            LogService.Instance.LogFactory.Configuration.RemoveTarget(VerifyingLog.LogName);
        }

        static ConcurrentDictionary<string, Task> updaters = new ConcurrentDictionary<string, Task>();

        /// <summary>
        /// Quick and dirty way of detecting if we need to re-generate cache folders - if you 
        /// modify this function. Maybe improve later on by taking into account assembly hash ?
        /// </summary>
        static int LogTestingVersion = LastLineNumber();


        /// <summary>
        /// Copies directory recursively
        /// </summary>
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        /// <summary>
        /// Gets test folder for testing. If folder does not exists, creates new task which will create specific folder.
        /// </summary>
        /// <param name="fuction">if not empty - copies shared folder also to isolated folder for modifying</param>
        public string PrepareTestFolder(
            ChocoTestContext testcontext, 
            ChocolateyConfiguration conf,
            string testFolder = ""
        )
        {
            string folderPath = Path.Combine(InstallContext.SharedPackageFolder, testcontext.ToString());
            string folderPathOk = Path.Combine(folderPath, $".updated_{LogTestingVersion}_ok");
            string isolatedTestFolder = Path.Combine(InstallContext.IsolatedTestFolder, testFolder);

            if (!String.IsNullOrEmpty(testFolder) && Directory.Exists(isolatedTestFolder))
            {
                Directory.Delete(isolatedTestFolder, true);
            }

            if (Directory.Exists(folderPathOk))
            {
                if (!String.IsNullOrEmpty(testFolder))
                {
                    CopyDirectory(folderPath, isolatedTestFolder);
                    return isolatedTestFolder;
                }

                return folderPath;
            }

            Task newtask;

            string oldSources = conf.Sources;
            conf.Sources = InstallContext.TestPackagesFolder;

            newtask = new Task(() =>
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
                Directory.CreateDirectory(folderPath);

                InstallContext.Instance.RootLocation = folderPath;

                using (new VerifyingLog(testcontext.ToString()))
                {
                    PrepareTestContext(testcontext, conf);
                    LogService.console.Info("shared context ends");
                }

                Directory.CreateDirectory(folderPathOk);
            });

            var task = updaters.GetOrAdd(testcontext.ToString(), newtask);
            if (task == newtask)
                newtask.Start();

            task.Wait();
            conf.Sources = oldSources;

            if (!String.IsNullOrEmpty(testFolder))
            {
                CopyDirectory(folderPath, isolatedTestFolder);
                return isolatedTestFolder;
            }

            return folderPath;
        }

        void PrepareTestContext(ChocoTestContext testcontext, ChocolateyConfiguration conf)
        {
            switch (testcontext)
            {
                case ChocoTestContext.installupdate:
                    {
                        Scenario.install_package(conf, "installpackage", "1.0.0");
                        Scenario.install_package(conf, "upgradepackage", "1.0.0");
                    }
                    break;

                case ChocoTestContext.exactpackage:
                    {
                        conf.Sources = InstallContext.Instance.PackagesLocation;

                        Scenario.add_packages_to_source_location(conf, "exactpackage*" + Constants.PackageExtension);
                    }
                    break;

                case ChocoTestContext.empty:
                    break;
            }
        }

        static int _LastLineNumber([CallerLineNumber] int line = 0)
        { 
            return line;
        }

        static int LastLineNumber()
        {
            return _LastLineNumber();
        }
    }
}



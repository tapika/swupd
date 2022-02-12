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
    using System.Collections.Generic;
    using chocolatey.infrastructure.app.services;
    using System.Linq;
    using chocolatey.infrastructure.results;
    using chocolatey;
    using System.Text.RegularExpressions;
    using chocolatey.infrastructure.app.commands;

    [TestFixture]
    public class LogTesting
    {
        protected IChocolateyPackageService Service;

        public LogTesting()
        {
            Service = chocolatey.tests.integration.NUnitSetup.Container.GetInstance<IChocolateyPackageService>();
        }

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
                    // Special kind of folder just to indicate that it's ready, no need to copy.
                    if (subDir.Name.StartsWith(".updated_") && subDir.Name.EndsWith("_ok"))
                        continue;
                
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        static List<string> GetFileListing(string path)
        {
            var list = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList();
            list.Sort();
            return list;
        }

        public List<string> addedFiles;
        public List<string> removedFiles;

        /// <summary>
        /// Last configuration used for installation
        /// </summary>
        public ChocolateyConfiguration lastconf = null;

        /// <summary>
        /// Installs specific package - can be used only within this file
        /// </summary>
        private void Install(string package, string version, ChocoTestContext packagesContext = ChocoTestContext.packages_default)
        {
            InstallOn(ChocoTestContext.skipcontextinit, (conf2) =>
            {
                conf2.PackageNames = conf2.Input = package;
                conf2.Version = version;
            }, ChocoTestContext.packages_for_dependency_testing6);
        }


        public void InstallOn(
            ChocoTestContext testcontext,
            Action<ChocolateyConfiguration> confPatch = null,
            ChocoTestContext packagesContext = ChocoTestContext.packages_default,
            [CallerMemberName] string testFolder = ""
        )
        {
            ChocolateyConfiguration conf = Scenario.baseline_configuration(true);
            if (testcontext != ChocoTestContext.skipcontextinit)
            {
                string dir = PrepareTestFolder(testcontext, conf, testFolder);
                InstallContext.Instance.RootLocation = dir;
            }

            if (packagesContext == ChocoTestContext.packages_default)
            {
                conf.Sources = InstallContext.TestPackagesFolder;
            }
            else
            {
                conf.Sources = PrepareTestFolder(packagesContext, conf);
            }

            conf.PackageNames = conf.Input = "installpackage";
            if (confPatch != null)
            {
                confPatch(conf);
            }

            string rootDir = InstallContext.Instance.RootLocation;
            var listBeforeUpdate = GetFileListing(rootDir);

            if (conf.Noop)
            {
                Service.install_noop(conf);
            }
            else
            {
                var results = Service.install_run(conf);
                var packages = results.Keys.ToList();
                packages.Sort();
                var console = LogService.console;

                foreach (var package in packages)
                {
                    var pkgresult = results[package];
                    console.Info($"=> install result for {pkgresult.Name}/{pkgresult.Version}: "
                        + ((pkgresult.Success) ? "succeeded" : "FAILED"));

                    foreach (var resultType in new[] { ResultType.Error, ResultType.Warn, ResultType.Inconclusive })
                    {
                        var msgs = pkgresult.Messages.Where(x => x.MessageType == resultType).ToList();

                        if (msgs.Count == 0)
                        {
                            continue;
                        }

                        console.Info($"  {resultType}: ");
                        foreach (var msg in msgs)
                        {
                            console.Info($"  - {msg.Message}");
                        }
                    }
                }
            }
            var listAfterUpdate = GetFileListing(rootDir);
            addedFiles = new List<string>();
            removedFiles = new List<string>();

            foreach (var file in listAfterUpdate)
            {
                if (!listBeforeUpdate.Contains(file))
                {
                    addedFiles.Add(file.Substring(rootDir.Length + 1));
                }
            }

            foreach (var file in listBeforeUpdate)
            {
                if (!listAfterUpdate.Contains(file))
                {
                    removedFiles.Add(file.Substring(rootDir.Length + 1));
                }
            }

            ListUpdates();

            lastconf = conf;
        }

        /// <summary>
        /// Lists what updates were performed to folder.
        /// </summary>
        void ListUpdates()
        {
            var console = LogService.console;
            List<string>[] lists = new List<string>[2] { addedFiles, removedFiles };
            string[] listName = new[] { "added new", "removed" };

            for (int iList = 0; iList < 2; iList++)
            {
                var list = lists[iList];
                var name = listName[iList];

                if (list.Count == 0)
                {
                    if (iList == 0)
                    {
                        console.Info("=> folder was not updated");
                    }
                }
                else
                {
                    console.Info($"=> {name} files:");
                    foreach (var f in list)
                    {
                        console.Info(f);

                        if (Path.GetExtension(f) == Constants.PackageExtension && iList == 0)
                        {
                            string nupkgPath = Path.Combine(InstallContext.Instance.RootLocation, f);
                            var package = new OptimizedZipPackage(nupkgPath);
                            console.Info("  version: " + package.Version.Version.to_string());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lists additional package versions
        /// </summary>
        /// <param name="packageNames">package names</param>
        public void ListPackageVersions(params string[] packageNames)
        {
            var console = LogService.console;

            console.Info("=> additional version information:");

            foreach (string packageName in packageNames)
            {
                string nupkg = Path.Combine(InstallContext.Instance.PackagesLocation, packageName, packageName + Constants.PackageExtension);
                var package = new OptimizedZipPackage(nupkg);
                console.Info($"  {packageName} version: " + package.Version.Version.to_string());
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

            string oldSources = null;
            if (conf != null)
            {
                oldSources = conf.Sources;
                conf.Sources = InstallContext.TestPackagesFolder;
            }
            string rootDir = InstallContext.Instance.RootLocation;

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
                    if (PrepareTestContext(testcontext, conf))
                    { 
                       LogService.console.Info("shared context ends");
                    }
                }

                Directory.CreateDirectory(folderPathOk);
            });

            var task = updaters.GetOrAdd(testcontext.ToString(), newtask);
            if (task == newtask)
                newtask.Start();
            else
                rootDir = null;

            task.Wait();
            if (oldSources != null)
            { 
                conf.Sources = oldSources;
            }

            if (rootDir != null)
            {
                InstallContext.Instance.RootLocation = rootDir;
            }

            if (!String.IsNullOrEmpty(testFolder))
            {
                CopyDirectory(folderPath, isolatedTestFolder);
                return isolatedTestFolder;
            }

            return folderPath;
        }

        static Regex rePack = new Regex("pack_(.*?)_(.*)");

        /// <returns>true if log needs to be created, false if not</returns>
        bool PrepareTestContext(ChocoTestContext testcontext, ChocolateyConfiguration conf)
        {
            // Generic nupkg creation.
            var re = rePack.Match(testcontext.ToString());
            if (re.Success)
            {
                string package = re.Groups[1].Value;
                string version = re.Groups[2].Value.Replace('_','.').Replace(".beta", "-beta");

                string nuspecMatch = $"{Path.DirectorySeparatorChar}{package}{Path.DirectorySeparatorChar}{version}";
                var nuspecs = Directory.GetFiles(InstallContext.TestPackagesFolder, "*.nuspec", SearchOption.AllDirectories).Where(x => Path.GetDirectoryName(x).EndsWith(nuspecMatch)).ToList();

                if (nuspecs.Count != 1)
                {
                    throw new Exception($"Package .nuspec not found: {nuspecMatch}");
                }

                var command = chocolatey.tests.integration.NUnitSetup.Commands.OfType<ChocolateyPackCommand>().Single();
                var packConf = Scenario.pack(true);
                packConf.Input = nuspecs[0];
                packConf.QuietOutput = true;
                packConf.OutputDirectory = InstallContext.Instance.RootLocation;
                command.run(packConf);
                return false;
            }
            
            switch (testcontext)
            {
                case ChocoTestContext.packages_for_dependency_testing:
                    PrepareMultiPackageFolder( 
                        ChocoTestContext.pack_badpackage_1_0,
                        ChocoTestContext.pack_hasdependency_1_0_0,
                        ChocoTestContext.pack_installpackage_1_0_0,
                        ChocoTestContext.pack_isdependency_1_0_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_1,
                        ChocoTestContext.pack_isexactversiondependency_1_1_0,
                        ChocoTestContext.pack_isexactversiondependency_2_0_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing2:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.packages_for_dependency_testing,
                        ChocoTestContext.pack_hasdependency_1_0_1,
                        ChocoTestContext.pack_hasdependency_1_1_0,
                        ChocoTestContext.pack_hasdependency_1_5_0,
                        ChocoTestContext.pack_hasdependency_1_6_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing3:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.packages_for_dependency_testing2,
                        ChocoTestContext.pack_isdependency_1_0_1,
                        ChocoTestContext.pack_isdependency_1_1_0,
                        ChocoTestContext.pack_isdependency_2_0_0,
                        ChocoTestContext.pack_isdependency_2_1_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing4:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.pack_hasdependency_1_6_0,
                        ChocoTestContext.pack_isdependency_1_0_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_1,
                        ChocoTestContext.pack_isexactversiondependency_1_1_0,
                        ChocoTestContext.pack_isexactversiondependency_2_0_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing5:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.packages_for_dependency_testing4,
                        ChocoTestContext.pack_badpackage_1_0,
                        ChocoTestContext.pack_installpackage_1_0_0,
                        ChocoTestContext.pack_isdependency_1_1_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing6:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.pack_hasdependency_1_0_0,
                        ChocoTestContext.pack_conflictingdependency_1_0_1,
                        ChocoTestContext.pack_isdependency_1_0_0,
                        ChocoTestContext.pack_isdependency_1_0_1,
                        ChocoTestContext.pack_isexactversiondependency_1_0_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_1,
                        ChocoTestContext.pack_isexactversiondependency_1_1_0,
                        ChocoTestContext.pack_isexactversiondependency_2_0_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing7:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.pack_hasdependency_1_0_0,
                        ChocoTestContext.pack_conflictingdependency_2_1_0,
                        ChocoTestContext.pack_isdependency_1_0_0,
                        ChocoTestContext.pack_isdependency_1_0_1,
                        ChocoTestContext.pack_isdependency_1_1_0,
                        ChocoTestContext.pack_isdependency_2_0_0,
                        ChocoTestContext.pack_isdependency_2_1_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_1,
                        ChocoTestContext.pack_isexactversiondependency_1_1_0,
                        ChocoTestContext.pack_isexactversiondependency_2_0_0
                    );
                    break;

                case ChocoTestContext.badpackage:
                    {
                        conf.SkipPackageInstallProvider = true;
                        Scenario.install_package(conf, "badpackage", "1.0.0");
                        conf.SkipPackageInstallProvider = false;
                    }
                    break;

                case ChocoTestContext.install:
                    {
                        Scenario.install_package(conf, "installpackage", "1.0.0");
                    }
                    break;

                case ChocoTestContext.install_sxs:
                    {
                        conf.AllowMultipleVersions = true;
                        Scenario.install_package(conf, "installpackage", "1.0.0");
                    }
                    break;

                case ChocoTestContext.installupdate:
                    {
                        Scenario.install_package(conf, "installpackage", "1.0.0");
                        Scenario.install_package(conf, "upgradepackage", "1.0.0");
                    }
                    break;

                case ChocoTestContext.hasdependency:
                    {
                        InstallOn(ChocoTestContext.skipcontextinit, (conf2) =>
                        {
                            conf2.PackageNames = conf2.Input = "hasdependency";
                            conf2.Version = "1.0.0";
                        }, ChocoTestContext.packages_for_dependency_testing2);
                    }
                    break;

                case ChocoTestContext.isdependency:
                    {
                        InstallOn(ChocoTestContext.skipcontextinit, (conf2) =>
                        {
                            conf2.PackageNames = conf2.Input = "isdependency";
                            conf2.Version = "1.0.0";
                        }, ChocoTestContext.packages_for_dependency_testing5);
                    }
                    break;

                case ChocoTestContext.isdependency_hasdependency:
                    {
                        Install("isdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing6);
                        Install("hasdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing6);
                    }
                    break;

                case ChocoTestContext.isdependency_hasdependency_2:
                    {
                        Install("isdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing7);
                        Install("hasdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing7);
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

            if (testcontext.ToString().StartsWith("packages_"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Prepares separate folder for multiple nuget packages in same folder.
        /// </summary>
        private void PrepareMultiPackageFolder(params ChocoTestContext[] packcontexts)
        {
            string[] folders = packcontexts.Select(x => PrepareTestFolder(x, null)).ToArray();
            foreach (var folder in folders)
            {
                foreach (var nupkg in Directory.GetFiles(folder, "*" + Constants.PackageExtension))
                {
                    string destPath = Path.Combine(InstallContext.Instance.RootLocation, Path.GetFileName(nupkg));
                    File.Copy(nupkg, destPath, true);
                }
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



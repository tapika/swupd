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
    using chocolatey.infrastructure.app.domain;

    [TestFixture]
    public class LogTesting
    {
        protected IChocolateyPackageService Service;

        public const string installpackage2_id = "installpackage2";

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

        /// <summary>
        /// Gets all files listing by excluding shim generated files in shimDir.
        /// </summary>
        /// <returns>current folder listing</returns>
        public static List<string> GetFileListing(string path, string shimDir = null)
        {
            var list = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList();
            // If we want to be portable accross all .net's - better use Ordinal sorting method, see
            // https://forum.stimulsoft.com/viewtopic.php?t=59248
            list.Sort(StringComparer.OrdinalIgnoreCase);

            if (shimDir == null)
            {
                return list;
            }

            // Filter out all shim generated files about which we don't care.
            // besides console.exe, there is also console.dll, console.deps.json, console.runtimeconfig.dev.json, console.runtimeconfig.json
            shimDir += Path.DirectorySeparatorChar;
            string[] excludedExtensions = new[] { ".json", ".pdb", ".dll" };
            var list2 = list.Where(x => !(x.StartsWith(shimDir) && excludedExtensions.Contains(Path.GetExtension(x).ToLower()))).ToList();
            return list2;
        }

        public List<string> addedFiles;
        public List<string> removedFiles;

        /// <summary>
        /// Locked file path to exclude from test results. On windows 10 - locked file will be deleted
        /// after folder rename, but on older windows that file will remain in file system.
        /// We need to exclude that as a result.
        /// </summary>
        public string lockedFilePath;

        /// <summary>
        /// Last configuration used for installation
        /// </summary>
        public ChocolateyConfiguration lastconf = null;

        /// <summary>
        /// Installs specific package - can be used only within this file
        /// </summary>
        private void Install(
            string package, string version, 
            ChocoTestContext packagesContext = ChocoTestContext.packages_default, 
            bool SkipPackageInstallProvider = false, 
            bool Prerelease = false
        )
        {
            ExecuteConf(ChocoTestContext.skipcontextinit, (conf2) =>
            {
                conf2.PackageNames = conf2.Input = package;
                conf2.Version = version;

                if (SkipPackageInstallProvider)
                {
                    conf2.SkipPackageInstallProvider = true;
                }

                conf2.Prerelease = Prerelease;
            }, packagesContext);
        }

        /// <summary>
        /// Installs specific package using sxs / AllowMultipleVersions
        /// </summary>
        private void InstallSxs(
            string package, string version,
            ChocoTestContext packagesContext = ChocoTestContext.packages_default)
        {
            ExecuteConf(ChocoTestContext.skipcontextinit, (conf2) =>
            {
                conf2.PackageNames = conf2.Input = package;
                conf2.Version = version;
                conf2.AllowMultipleVersions = true;
            }, packagesContext);
        }

        /// <summary>
        /// Executes operation defined in configuration
        /// </summary>
        /// <param name="testcontext">test content to start from</param>
        /// <param name="confPatch">additional patch action after configuration is created</param>
        /// <param name="packagesContext">package content to request</param>
        /// <param name="testFolder">test folder where to execute</param>
        public void ExecuteConf(
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

            conf.CommandName = nameof(CommandNameType.install);
            conf.PackageNames = conf.Input = "installpackage";
            if (confPatch != null)
            {
                confPatch(conf);
            }

            string rootDir = InstallContext.Instance.RootLocation;
            var listBeforeUpdate = GetFileListing(rootDir);

            if (conf.Noop)
            {
                switch (conf.CommandName)
                {
                    case nameof(CommandNameType.install):
                        Service.install_noop(conf);
                        break;
                    case nameof(CommandNameType.upgrade):
                        Service.upgrade_noop(conf);
                        break;
                    case nameof(CommandNameType.uninstall):
                        Service.uninstall_noop(conf);
                        break;
                }
            }
            else
            {
                ConcurrentDictionary<string, PackageResult> results = null;

                switch (conf.CommandName)
                {
                    case nameof(CommandNameType.install):
                        results = Service.install_run(conf);
                        break;
                    case nameof(CommandNameType.upgrade):
                        results = Service.upgrade_run(conf);
                        break;
                    case nameof(CommandNameType.uninstall):
                        results = Service.uninstall_run(conf);
                        break;
                }

                var packages = results.Keys.ToList();
                packages.Sort();
                var console = LogService.console;

                foreach (var package in packages)
                {
                    var pkgresult = results[package];
                    console.Info($"=> {conf.CommandName} result for {pkgresult.Name}/{pkgresult.Version}: "
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

            DisplayUpdates(listBeforeUpdate);
            lastconf = conf;
        }

        public void DisplayUpdates(List<string> listBeforeUpdate)
        {
            string rootDir = InstallContext.Instance.RootLocation;
            var listAfterUpdate = GetFileListing(rootDir, InstallContext.Instance.ShimsLocation);
            addedFiles = new List<string>();
            removedFiles = new List<string>();

            foreach (var file in listAfterUpdate)
            {
                if (!listBeforeUpdate.Contains(file))
                {
                    bool addfile = true;
                    string relativePath = file.Substring(rootDir.Length + 1);

                    if (lockedFilePath != null)
                    {
                        var pparts1 = relativePath.Split(Path.DirectorySeparatorChar);
                        if (pparts1.First() == InstallContext.BackupFolderName)
                        { 
                            var pparts2 = lockedFilePath.Substring(rootDir.Length + 1).Split(Path.DirectorySeparatorChar);
                            addfile = Path.Combine(pparts1.Skip(1).ToArray()) != Path.Combine(pparts2.Skip(1).ToArray());
                        }
                    }

                    if (addfile)
                    { 
                        addedFiles.Add(relativePath);
                    }
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
        bool PrepareTestContext(ChocoTestContext testcontext, ChocolateyConfiguration _conf)
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

                var command = ApplicationManager.Instance.Commands.OfType<ChocolateyPackCommand>().Single();
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

                case ChocoTestContext.packages_for_dependency_testing8:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.pack_hasdependency_1_0_0,
                        ChocoTestContext.pack_hasdependency_1_0_1,
                        ChocoTestContext.pack_hasdependency_1_1_0,
                        ChocoTestContext.pack_hasdependency_1_5_0,
                        ChocoTestContext.pack_hasdependency_1_6_0,
                        ChocoTestContext.pack_hasdependency_2_0_0,
                        ChocoTestContext.pack_hasdependency_2_1_0,
                        ChocoTestContext.pack_isdependency_1_0_0,
                        ChocoTestContext.pack_isdependency_1_0_1,
                        ChocoTestContext.pack_isdependency_1_1_0,
                        ChocoTestContext.pack_isexactversiondependency_1_0_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing9:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.packages_for_dependency_testing8,
                        ChocoTestContext.pack_isdependency_2_0_0,
                        ChocoTestContext.pack_isdependency_2_1_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing10:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.packages_for_dependency_testing9,
                        ChocoTestContext.pack_isexactversiondependency_1_0_1,
                        ChocoTestContext.pack_isexactversiondependency_1_1_0,
                        ChocoTestContext.pack_isexactversiondependency_2_0_0
                    );
                    break;

                case ChocoTestContext.packages_for_dependency_testing11:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.packages_for_dependency_testing8,
                        ChocoTestContext.pack_isexactversiondependency_1_0_1,
                        ChocoTestContext.pack_isexactversiondependency_1_1_0,
                        ChocoTestContext.pack_isexactversiondependency_2_0_0
                    );
                    break;

                case ChocoTestContext.packages_for_upgrade_testing:
                    PrepareMultiPackageFolder(
                        ChocoTestContext.pack_badpackage_1_0,
                        ChocoTestContext.pack_badpackage_2_0,
                        ChocoTestContext.pack_installpackage_1_0_0,
                        ChocoTestContext.pack_upgradepackage_1_0_0,
                        ChocoTestContext.pack_upgradepackage_1_1_0,
                        ChocoTestContext.pack_upgradepackage_1_1_1_beta,
                        ChocoTestContext.pack_upgradepackage_1_1_1_beta2
                    );
                    break;

                case ChocoTestContext.badpackage:
                    {
                        _conf.SkipPackageInstallProvider = true;
                        Scenario.install_package(_conf, "badpackage", "1.0.0");
                        _conf.SkipPackageInstallProvider = false;
                    }
                    break;

                case ChocoTestContext.install:
                    {
                        Scenario.install_package(_conf, "installpackage", "1.0.0");
                    }
                    break;

                case ChocoTestContext.install_sxs:
                    {
                        _conf.AllowMultipleVersions = true;
                        Scenario.install_package(_conf, "installpackage", "1.0.0");
                    }
                    break;

                case ChocoTestContext.installupdate:
                    {
                        Scenario.install_package(_conf, "installpackage", "1.0.0");
                        Scenario.install_package(_conf, "upgradepackage", "1.0.0");
                    }
                    break;

                case ChocoTestContext.installupdate2:
                    {
                        const string packageId = "installpackage2";

                        using (var tester = new TestRegistry())
                        {
                            tester.DeleteInstallEntries(packageId);

                            Install("installpackage2", "1.0.0", ChocoTestContext.pack_installpackage2_1_0_0);
                            
                            tester.LogInstallEntries(true, packageId);
                            tester.DeleteInstallEntries(packageId);
                        }
                    }
                    break;

                case ChocoTestContext.installed_5_packages:
                    {
                        Install("installpackage", "1.0.0");
                        Install("upgradepackage", "1.0.0");
                        Install("hasdependency", "1.0.0");
                    }
                    break;
                case ChocoTestContext.upgrade_testing_context:
                    {
                        ChocoTestContext packagesContext = ChocoTestContext.packages_for_upgrade_testing;
                        Install("installpackage", "1.0.0", packagesContext);
                        Install("upgradepackage", "1.0.0", packagesContext);
                        Install("badpackage", "1.0", packagesContext, true);
                    }
                    break;

                case ChocoTestContext.uninstall_testing_context:
                    {
                        ChocoTestContext packagesContext = ChocoTestContext.packages_for_upgrade_testing;
                        Install("installpackage", "1.0.0", packagesContext);
                        Install("badpackage", "1.0", packagesContext, true);
                    }
                    break;

                case ChocoTestContext.hasdependency:
                    {
                        Install("hasdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing2);
                    }
                    break;

                case ChocoTestContext.isdependency:
                    {
                        Install("isdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing5);
                    }
                    break;

                case ChocoTestContext.isdependency_hasdependency:
                    {
                        Install("isdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing6);
                        Install("hasdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing6);
                    }
                    break;

                case ChocoTestContext.isdependency_hasdependency_sxs:
                    {
                        InstallSxs("isdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing6);
                        InstallSxs("hasdependency", "1.0.0", ChocoTestContext.packages_for_dependency_testing6);
                    }
                    break;

                case ChocoTestContext.upgradepackage_1_1_1_beta:
                    {
                        PrepareTestContext(ChocoTestContext.upgrade_testing_context, _conf);
                        Install("upgradepackage", "1.1.1-beta", ChocoTestContext.packages_for_upgrade_testing, false, true);
                    }
                    break;

                case ChocoTestContext.exactpackage:
                    {
                        _conf.Sources = InstallContext.Instance.PackagesLocation;

                        Scenario.add_packages_to_source_location(_conf, "exactpackage*" + Constants.PackageExtension);
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



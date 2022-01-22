using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.logging;
using chocolatey.tests.integration;
using logtesting;
using NuGet;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace chocolatey.tests2.commands
{
    public class ListScenario : LogTesting
    {
        protected IChocolateyPackageService Service;

        public ListScenario()
        {
            Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();
        }
    };
    

    [Parallelizable(ParallelScope.All)]
    public class TestListCommand: ListScenario
    {
        public void CommonList(Action<ChocolateyConfiguration> confPatch)
        {
            var conf = Scenario.list(true);
            InstallContext.Instance.RootLocation = PrepareTestFolder(ChocoTestContext.installupdate, conf);
            conf.Sources = InstallContext.Instance.PackagesLocation;
            confPatch(conf);
            Service.list_run(conf).ToList();
        }

        // when_searching_packages_with_no_filter_happy_path
        // when_searching_all_available_packages
        // almost identical to when_listing_local_packages

        // when_searching_for_a_particular_package
        [LogTest]
        public void ListSpecificPackage()
        {
            CommonList((conf) =>
            {
                conf.Input = conf.PackageNames = "upgradepackage";
            });
        }

        // when_searching_all_available_packages
        [LogTest]
        public void ListAllPackages()
        {
            CommonList((conf) =>
            {
                conf.AllVersions = true;
            });
        }

        // when_searching_packages_with_verbose
        [LogTest]
        public void ListWithVerbose()
        {
            CommonList((conf) =>
            {
                conf.Verbose = true;
            });
        }

        // when_listing_local_packages
        [LogTest]
        public void ListAll()
        {
            CommonList((conf) =>
            {
                conf.ListCommand.LocalOnly = true;
            });
        }

        // when_listing_local_packages_limiting_output
        [LogTest]
        public void ListAllRegular()
        {
            CommonList((conf) =>
            {
                conf.RegularOutput = false;
            });
        }

        // when_listing_local_packages_limiting_output_with_id_only
        [LogTest]
        public void ListPackageIds()
        {
            CommonList((conf) =>
            {
                conf.RegularOutput = false;
                conf.ListCommand.IdOnly = true;
            });
        }

        // when_listing_packages_with_no_sources_enabled
        [LogTest]
        public void ListNoSources()
        {
            CommonList((conf) =>
            {
                conf.Sources = null;
            });
        }

        public void ExactPackagesList(Action<ChocolateyConfiguration> confPatch)
        {
            var conf = Scenario.list(true);
            InstallContext.Instance.RootLocation = PrepareTestFolder(ChocoTestContext.exactpackage, conf);
            conf.Sources = InstallContext.Instance.PackagesLocation;
            confPatch(conf);
            Service.list_run(conf).ToList();
        }

        // when_searching_for_an_exact_package
        [LogTest]
        public void SearchingExactPackage()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.Input = conf.PackageNames = "exactpackage";
            });
        }

        // when_searching_for_an_exact_package
        [LogTest]
        public void SearchingNoPackages()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.Input = conf.PackageNames = "exactpackage123";
            });
        }

        // when_searching_for_all_packages_with_exact_id
        [LogTest]
        public void SearchingPackages()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.AllVersions = true;
                conf.Input = conf.PackageNames = "exactpackage";
            });
        }

        // when_searching_for_all_packages_including_prerelease_with_exact_id
        [LogTest]
        public void SearchingAllPackages()
        {
            ExactPackagesList((conf) =>
            {
                conf.ListCommand.Exact = true;
                conf.AllVersions = true;
                conf.Prerelease = true;
                conf.Input = conf.PackageNames = "exactpackage";
            });
        }

    }
}


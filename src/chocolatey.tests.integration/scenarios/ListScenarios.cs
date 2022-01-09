// Copyright © 2017 - 2021 Chocolatey Software, Inc
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

namespace chocolatey.tests.integration.scenarios
{
    using System.Collections.Generic;
    using System.Linq;
    using bdddoc.core;
    using chocolatey.infrastructure.app;
    using chocolatey.infrastructure.app.commands;
    using chocolatey.infrastructure.app.configuration;
    using chocolatey.infrastructure.app.services;
    using chocolatey.infrastructure.results;
    using NuGet;
    using FluentAssertions;

    public class ListScenarios
    {
        public abstract class ScenariosBase : TinySpec
        {
            protected IList<PackageResult> Results;
            protected ChocolateyConfiguration Configuration;
            protected IChocolateyPackageService Service;

            public override void Context()
            {
                Configuration = Scenario.list();
                Scenario.reset(Configuration);
                Scenario.add_packages_to_source_location(Configuration, Configuration.Input + "*" + Constants.PackageExtension);
                Scenario.add_packages_to_source_location(Configuration, "installpackage*" + Constants.PackageExtension);
                Scenario.install_package(Configuration, "installpackage", "1.0.0");
                Scenario.install_package(Configuration, "upgradepackage", "1.0.0");

                Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_packages_with_no_filter_happy_path : ScenariosBase
        {
            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_list_available_packages_only_once()
            {
                MockLogger.contains_message_count("upgradepackage").Should().Be(1);
            }

            [Fact]
            public void should_contain_packages_and_versions_with_a_space_between_them()
            {
                MockLogger.contains_message("upgradepackage 1.1.0").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_packages_and_versions_with_a_pipe_between_them()
            {
                MockLogger.contains_message("upgradepackage|1.1.0").Should().BeFalse();
            }

            [Fact]
            public void should_contain_a_summary()
            {
                MockLogger.contains_message("packages found").Should().BeTrue();
            }

            [Fact]
            public void should_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeTrue();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_for_a_particular_package : ScenariosBase
        {
            public override void Context()
            {
                base.Context();
                Configuration.Input = Configuration.PackageNames = "upgradepackage";
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_contain_packages_and_versions_with_a_space_between_them()
            {
                MockLogger.contains_message("upgradepackage 1.1.0").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_packages_that_do_not_match()
            {
                MockLogger.contains_message("installpackage").Should().BeFalse();
            }

            [Fact]
            public void should_contain_a_summary()
            {
                MockLogger.contains_message("packages found").Should().BeTrue();
            }

            [Fact]
            public void should_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeTrue();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_all_available_packages : ScenariosBase
        {
            public override void Context()
            {
                base.Context();
                Configuration.AllVersions = true;
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_list_available_packages_as_many_times_as_they_show_on_the_feed()
            {
                MockLogger.contains_message_count("upgradepackage").Should().NotBe(0);
                MockLogger.contains_message_count("upgradepackage").Should().NotBe(1);
            }

            [Fact]
            public void should_contain_packages_and_versions_with_a_space_between_them()
            {
                MockLogger.contains_message("upgradepackage 1.1.0").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_packages_and_versions_with_a_pipe_between_them()
            {
                MockLogger.contains_message("upgradepackage|1.1.0").Should().BeFalse();
            }

            [Fact]
            public void should_contain_a_summary()
            {
                MockLogger.contains_message("packages found").Should().BeTrue();
            }

            [Fact]
            public void should_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeTrue();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_packages_with_verbose : ScenariosBase
        {
            public override void Context()
            {
                base.Context();
                Configuration.Verbose = true;
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_contain_packages_and_versions_with_a_space_between_them()
            {
                MockLogger.contains_message("upgradepackage 1.1.0").Should().BeTrue();
            }

            [Fact]
            public void should_contain_description()
            {
                MockLogger.contains_message("Description: ").Should().BeTrue();
            }

            [Fact]
            public void should_contain_tags()
            {
                MockLogger.contains_message("Tags: ").Should().BeTrue();
            }

            [Fact]
            public void should_contain_download_counts()
            {
                MockLogger.contains_message("Number of Downloads: ").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_packages_and_versions_with_a_pipe_between_them()
            {
                MockLogger.contains_message("upgradepackage|1.1.0").Should().BeFalse();
            }

            [Fact]
            public void should_contain_a_summary()
            {
                MockLogger.contains_message("packages found").Should().BeTrue();
            }

            [Fact]
            public void should_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeTrue();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_listing_local_packages : ScenariosBase
        {
            public override void Context()
            {
                base.Context();
                Configuration.ListCommand.LocalOnly = true;
                Configuration.Sources = ApplicationParameters.PackagesLocation;
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_contain_packages_and_versions_with_a_space_between_them()
            {
                MockLogger.contains_message("upgradepackage 1.0.0").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_packages_and_versions_with_a_pipe_between_them()
            {
                MockLogger.contains_message("upgradepackage|1.0.0").Should().BeFalse();
            }

            [Fact]
            public void should_contain_a_summary()
            {
                MockLogger.contains_message("packages installed").Should().BeTrue();
            }

            [Fact]
            public void should_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeTrue();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_listing_local_packages_with_id_only : ScenariosBase
        {
            public override void Context()
            {
                base.Context();
                Configuration.ListCommand.LocalOnly = true;
                Configuration.ListCommand.IdOnly = true;
                Configuration.Sources = ApplicationParameters.PackagesLocation;
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_contain_package_name()
            {
                MockLogger.contains_message("upgradepackage").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_any_version_number()
            {
                MockLogger.contains_message(".0").Should().BeFalse();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_listing_local_packages_limiting_output : ScenariosBase
        {
            public override void Context()
            {
                base.Context();

                Configuration.ListCommand.LocalOnly = true;
                Configuration.Sources = ApplicationParameters.PackagesLocation;
                Configuration.RegularOutput = false;
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_contain_packages_and_versions_with_a_pipe_between_them()
            {
                MockLogger.contains_message("upgradepackage|1.0.0").Should().BeTrue();
            }

            [Fact]
            public void should_only_have_messages_related_to_package_information()
            {
                var count = MockLogger.Messages.SelectMany(messageLevel => messageLevel.Value.or_empty_list_if_null()).Count();
                count.Should().Be(2);
            }

            [Fact]
            public void should_not_contain_packages_and_versions_with_a_space_between_them()
            {
                MockLogger.contains_message("upgradepackage 1.0.0").Should().BeFalse();
            }

            [Fact]
            public void should_not_contain_a_summary()
            {
                MockLogger.contains_message("packages installed").Should().BeFalse();
            }

            [Fact]
            public void should_not_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeFalse();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeFalse();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeFalse();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeFalse();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_listing_local_packages_limiting_output_with_id_only : ScenariosBase
        {
            public override void Context()
            {
                base.Context();

                Configuration.ListCommand.LocalOnly = true;
                Configuration.ListCommand.IdOnly = true;
                Configuration.Sources = ApplicationParameters.PackagesLocation;
                Configuration.RegularOutput = false;
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_contain_packages_id()
            {
                MockLogger.contains_message("upgradepackage").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_any_version_number()
            {
                MockLogger.contains_message(".0").Should().BeFalse();
            }

            [Fact]
            public void should_not_contain_pipe()
            {
                MockLogger.contains_message("|").Should().BeFalse();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_listing_packages_with_no_sources_enabled : ScenariosBase
        {
            public override void Context()
            {
                base.Context();
                Configuration.Sources = null;
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_have_no_sources_enabled_result()
            {
                MockLogger.contains_message("Unable to search for packages when there are no sources enabled for", LogLevel.Error).Should().BeTrue();
            }

            [Fact]
            public void should_not_list_any_packages()
            {
                Results.Count().Should().Be(0);
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_for_an_exact_package : ScenariosBase
        {
            public override void Context()
            {
                Configuration = Scenario.list();
                Scenario.reset(Configuration);
                Scenario.add_packages_to_source_location(Configuration, "exactpackage*" + Constants.PackageExtension);
                Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();

                Configuration.ListCommand.Exact = true;
                Configuration.Input = Configuration.PackageNames = "exactpackage";
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_not_error()
            {
                // nothing necessary here
            }

            [Fact]
            public void should_find_exactly_one_result()
            {
                Results.Count.Should().Be(1);
            }

            [Fact]
            public void should_contain_packages_and_versions_with_a_space_between_them()
            {
                MockLogger.contains_message("exactpackage 1.0.0").Should().BeTrue();
            }

            [Fact]
            public void should_not_contain_packages_that_do_not_match()
            {
                MockLogger.contains_message("exactpackage.dontfind").Should().BeFalse();
            }

            [Fact]
            public void should_contain_a_summary()
            {
                MockLogger.contains_message("packages found").Should().BeTrue();
            }

            [Fact]
            public void should_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeTrue();
            }
        }        
        
        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_for_an_exact_package_with_zero_results : ScenariosBase
        {
            public override void Context()
            {
                Configuration = Scenario.list();
                Scenario.reset(Configuration);
                Scenario.add_packages_to_source_location(Configuration, "exactpackage*" + Constants.PackageExtension);
                Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();

                Configuration.ListCommand.Exact = true;
                Configuration.Input = Configuration.PackageNames = "exactpackage123";
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }


            [Fact]
            public void should_not_error()
            {
                // nothing necessary here
            }


            [Fact]
            public void should_not_have_any_results()
            {
                Results.Count.Should().Be(0);
            }

            [Fact]
            public void should_not_contain_packages_that_do_not_match()
            {
                MockLogger.contains_message("exactpackage.dontfind").Should().BeFalse();
            }

            [Fact]
            public void should_contain_a_summary()
            {
                MockLogger.contains_message("packages found").Should().BeTrue();
            }

            [Fact]
            public void should_contain_debugging_messages()
            {
                MockLogger.contains_message("Searching for package information", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Running list with the following filter", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("Start of List", LogLevel.Debug).Should().BeTrue();
                MockLogger.contains_message("End of List", LogLevel.Debug).Should().BeTrue();
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_for_all_packages_with_exact_id : ScenariosBase
        {
            public override void Context()
            {
                Configuration = Scenario.list();
                Scenario.reset(Configuration);
                Scenario.add_packages_to_source_location(Configuration, "exactpackage*" + Constants.PackageExtension);
                Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();

                Configuration.ListCommand.Exact = true;
                Configuration.AllVersions = true;
                Configuration.Input = Configuration.PackageNames = "exactpackage";
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_not_error()
            {
                // nothing necessary here
            }

            [Fact]
            public void should_find_two_results()
            {
                Results.Count.Should().Be(2);
            }

            [Fact]
            public void should_find_only_packages_with_exact_id()
            {
                Results[0].Package.Id.Should().Be("exactpackage");
                Results[1].Package.Id.Should().Be("exactpackage");
            }

            [Fact]
            public void should_find_all_non_prerelease_versions_in_descending_order()
            {
                Results[0].Package.Version.ToNormalizedString().Should().Be("1.0.0");
                Results[1].Package.Version.ToNormalizedString().Should().Be("0.9.0");
            }
        }

        [Concern(typeof(ChocolateyListCommand))]
        public class when_searching_for_all_packages_including_prerelease_with_exact_id : ScenariosBase
        {
            public override void Context()
            {
                Configuration = Scenario.list();
                Scenario.reset(Configuration);
                Scenario.add_packages_to_source_location(Configuration, "exactpackage*" + Constants.PackageExtension);
                Service = NUnitSetup.Container.GetInstance<IChocolateyPackageService>();

                Configuration.ListCommand.Exact = true;
                Configuration.AllVersions = true;
                Configuration.Prerelease = true;
                Configuration.Input = Configuration.PackageNames = "exactpackage";
            }

            public override void Because()
            {
                MockLogger.reset();
                Results = Service.list_run(Configuration).ToList();
            }

            [Fact]
            public void should_not_error()
            {
                // nothing necessary here
            }

            [Fact]
            public void should_find_three_results()
            {
                Results.Count.Should().Be(3);
            }

            [Fact]
            public void should_find_only_packages_with_exact_id()
            {
                Results[0].Package.Id.Should().Be("exactpackage");
                Results[1].Package.Id.Should().Be("exactpackage");
                Results[2].Package.Id.Should().Be("exactpackage");
            }

            [Fact]
            public void should_find_all_versions_in_descending_order()
            {
                Results[0].Package.Version.ToNormalizedString().Should().Be("1.0.0");
                Results[1].Package.Version.ToNormalizedString().Should().Be("1.0.0-beta1");
                Results[2].Package.Version.ToNormalizedString().Should().Be("0.9.0");
            }
        }
    }
}

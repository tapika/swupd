﻿// Copyright © 2017 - 2021 Chocolatey Software, Inc
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

namespace chocolatey.infrastructure.app.configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using domain;
    using logging;
    using platforms;

    /// <summary>
    ///   The chocolatey configuration.
    /// </summary>
    [Serializable]
    public class ChocolateyConfiguration
    {
        public ChocolateyConfiguration()
        {
            RegularOutput = true;
            PromptForConfirmation = true;
            SourceType = SourceType.normal;
            Information = new InformationCommandConfiguration();
            Features = new FeaturesConfiguration();
            NewCommand = new NewCommandConfiguration();
            ListCommand = new ListCommandConfiguration();
            PsRunCommand = new PsRunCommandConfiguration();
            UpgradeCommand = new UpgradeCommandConfiguration();
            SourceCommand = new SourcesCommandConfiguration();
            MachineSources = new List<MachineSourceConfiguration>();
            FeatureCommand = new FeatureCommandConfiguration();
            ConfigCommand = new ConfigCommandConfiguration();
            ApiKeyCommand = new ApiKeyCommandConfiguration();
            PackCommand = new PackCommandConfiguration();
            PushCommand = new PushCommandConfiguration();
            PinCommand = new PinCommandConfiguration();
            OutdatedCommand = new OutdatedCommandConfiguration();
            Proxy = new ProxyConfiguration();
        }

        // overrides
        public override string ToString()
        {
            if (string.IsNullOrEmpty(CommandName))
            {
                return "Empty config";
            }

            return $"{CommandName}-config";
        }

        public string ToStringFull()
        {
            var properties = new StringBuilder();

            this.Log().Debug(ChocolateyLoggers.Important, @"
NOTE: Hiding sensitive configuration data! Please double and triple
 check to be sure no sensitive data is shown, especially if copying
 output to a gist for review.");
            return CompareWith(null);
        }

        public string CompareWith(ChocolateyConfiguration second)
        {
            var properties = new StringBuilder();
            output_tostring(properties, GetType().GetProperties(), this, second, "");
            return properties.ToString();
        }

        private void output_tostring(StringBuilder propertyValues, IEnumerable<PropertyInfo> properties, object obj, object secondobj, string prepend)
        {
            foreach (var propertyInfo in properties.or_empty_list_if_null())
            {
                // skip sensitive data info
                if (!ApplicationParameters.runningUnitTesting)
                {
                    if (propertyInfo.Name.contains("password") || propertyInfo.Name.contains("sensitive") || propertyInfo.Name == "Key" || propertyInfo.Name == "ConfigValue" || propertyInfo.Name == "MachineSources")
                    {
                        continue;
                    }
                }

                var objectValue = propertyInfo.GetValue(obj, null);
                object secondObjectValue = null;
                if (secondobj != null)
                {
                    secondObjectValue = propertyInfo.GetValue(secondobj, null);
                }

                if (propertyInfo.PropertyType.is_built_in_system_type())
                {
                    bool doAppend = !string.IsNullOrWhiteSpace(objectValue.to_string());

                    if (doAppend)
                    {
                        doAppend = !Object.Equals(objectValue, secondObjectValue);
                    }
                    
                    if (doAppend)
                    {
                        var output = "{0}{1}='{2}'".format_with(
                            string.IsNullOrWhiteSpace(prepend) ? "" : prepend + ".",
                            propertyInfo.Name,
                            objectValue.to_string());

                        if (propertyInfo.Name == "Sources")
                        {
                            // Can contain paths
                            output = InstallContext.NormalizeMessage(output);
                        }

                        append_output(propertyValues, output);
                    }
                }
                else if (propertyInfo.PropertyType.is_collections_type())
                {
                    var list = (objectValue as IDictionary<string, string>).or_empty_list_if_null().ToList();
                    var secondlist = (secondObjectValue as IDictionary<string, string>).or_empty_list_if_null().ToList();
                                        
                    for(int i = 0; i < list.Count; i++)
                    {
                        if (i < secondlist.Count && Object.Equals(list[i], secondlist[i]))
                        {
                            continue;
                        }

                        var item = list[i];
                        var output = "{0}{1}.{2}='{3}'".format_with(
                            string.IsNullOrWhiteSpace(prepend) ? "" : prepend + ".",
                            propertyInfo.Name,
                            item.Key,
                            item.Value);

                        append_output(propertyValues, output);
                    }
                }
                else
                {
                    output_tostring(propertyValues, propertyInfo.PropertyType.GetProperties(), objectValue,secondObjectValue, propertyInfo.Name);
                }
            }
        }

        private const int MAX_CONSOLE_LINE_LENGTH = 80;
        private int _currentLineLength = 0;

        private void append_output(StringBuilder propertyValues, string append)
        {
            if (_currentLineLength != 0)
            {
                propertyValues.Append(",");     // Separator
            }

            bool startOfLine = _currentLineLength == 0;

            _currentLineLength += append.Length;
            if (!startOfLine && _currentLineLength > MAX_CONSOLE_LINE_LENGTH)
            {
                propertyValues.Append(Environment.NewLine);
                _currentLineLength = append.Length;
            }
            propertyValues.Append(append);
        }

        /// <summary>
        ///   Gets or sets the name of the command.
        ///   This is the command that choco runs.
        /// </summary>
        /// <value>
        ///   The name of the command.
        /// </value>
        public string CommandName { get; set; }

        // configuration set variables
        public string CacheLocation
        { 
            get
            {
                return InstallContext.Instance.CacheLocation;
            }
        
            set
            {
                InstallContext.Instance.CacheLocation = value;
            }
        }
        public bool ContainsLegacyPackageInstalls { get; set; }
        public int CommandExecutionTimeoutSeconds { get; set; }
        public int WebRequestTimeoutSeconds { get; set; }

        /// <summary>
        ///   One or more source locations set by configuration or by command line. Separated by semi-colon
        /// </summary>
        public string Sources { get; set; }
        public SourceType SourceType { get; set; }

        // top level commands

        public bool Debug { get; set; }
        public bool Verbose { get; set; }
        public bool Trace { get; set; }
        public bool Force { get; set; }
        public bool Noop { get; set; }
        public bool HelpRequested { get; set; }
        /// <summary>
        ///   Gets or sets a value indicating whether parsing was successful (everything parsed) or not.
        /// </summary>
        public bool UnsuccessfulParsing { get; set; }

        // TODO: Should look into using mutually exclusive output levels - Debug, Info (Regular), Error (Quiet)
        // Verbose and Important are not part of the levels at all
        /// <summary>
        /// Gets or sets a value indicating whether output should be limited.
        /// This supports the --limit-output parameter.
        /// </summary>
        /// <value><c>true</c> for regular output; <c>false</c> for limited output.</value>
        public bool RegularOutput { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether console logging should be supressed.
        /// This is for use by API calls which surface results in alternate forms.
        /// </summary>
        /// <value><c>true</c> for no output; <c>false</c> for regular or limited output.</value>
        /// <remarks>This has only been implemented for NuGet List</remarks>
        public bool QuietOutput { get; set; }
        public bool PromptForConfirmation { get; set; }
        public bool AcceptLicense { get; set; }
        public string AdditionalLogFileLocation { get; set; }

        /// <summary>
        ///   Usually related to unparsed arguments.
        /// </summary>
        public string Input { get; set; }

        // command level options
        public string Version { get; set; }
        public bool AllVersions { get; set; }
        public bool SkipPackageInstallProvider { get; set; }
        public string OutputDirectory { get; set; }

        // install/update
        /// <summary>
        ///   Gets or sets the package names. Space separated
        /// </summary>
        /// <value>
        ///   Space separated package names.
        /// </value>
        public string PackageNames { get; set; }

        public bool Prerelease { get; set; }
        public bool ForceX86 { get; set; }
        public string InstallArguments { get; set; }
        public bool OverrideArguments { get; set; }
        public bool NotSilent { get; set; }
        public string PackageParameters { get; set; }
        public bool ApplyPackageParametersToDependencies { get; set; }
        public bool ApplyInstallArgumentsToDependencies { get; set; }
        public bool IgnoreDependencies { get; set; }
        public bool AllowMultipleVersions { get; set; }
        public bool AllowDowngrade { get; set; }
        public bool ForceDependencies { get; set; }
        public string DownloadChecksum { get; set; }
        public string DownloadChecksum64 { get; set; }
        public string DownloadChecksumType { get; set; }
        public string DownloadChecksumType64 { get; set; }

        /// <summary>
        ///   Configuration values provided by choco.
        /// </summary>
        public InformationCommandConfiguration Information { get;  set; }

        /// <summary>
        ///   Configuration related to features and whether they are enabled.
        /// </summary>
        public FeaturesConfiguration Features { get;  set; }

        /// <summary>
        ///   Configuration related specifically to List command
        /// </summary>
        public ListCommandConfiguration ListCommand { get;  set; }

        /// <summary>
        /// Configuration related to PsRun command
        /// </summary>
        public PsRunCommandConfiguration PsRunCommand { get;  set; }

        /// <summary>
        ///   Configuration related specifically to Upgrade command
        /// </summary>
        public UpgradeCommandConfiguration UpgradeCommand { get;  set; }

        /// <summary>
        ///   Configuration related specifically to New command
        /// </summary>
        public NewCommandConfiguration NewCommand { get;  set; }

        /// <summary>
        ///   Configuration related specifically to Source command
        /// </summary>
        public SourcesCommandConfiguration SourceCommand { get;  set; }

        /// <summary>
        ///   Default Machine Sources Configuration
        /// </summary>
        public IList<MachineSourceConfiguration> MachineSources { get; set; }

        /// <summary>
        /// Configuration related specifically to the Feature command
        /// </summary>
        public FeatureCommandConfiguration FeatureCommand { get; set; }

        /// <summary>
        /// Configuration related to the configuration file.
        /// </summary>
        public ConfigCommandConfiguration ConfigCommand { get; set; }

        /// <summary>
        ///   Configuration related specifically to ApiKey command
        /// </summary>
        public ApiKeyCommandConfiguration ApiKeyCommand { get;  set; }

        /// <summary>
        ///   Configuration related specifically to the Pack command.
        /// </summary>
        public PackCommandConfiguration PackCommand { get; set; }

        /// <summary>
        ///   Configuration related specifically to Push command
        /// </summary>
        public PushCommandConfiguration PushCommand { get;  set; }

        /// <summary>
        /// Configuration related specifically to Pin command
        /// </summary>
        public PinCommandConfiguration PinCommand { get; set; }

        /// <summary>
        /// Configuration related specifically to Outdated command
        /// </summary>
        public OutdatedCommandConfiguration OutdatedCommand { get; set; }

        /// <summary>
        /// Configuration related specifically to proxies.
        /// </summary>
        public ProxyConfiguration Proxy { get; set; }
    }

    [Serializable]
    public sealed class InformationCommandConfiguration
    {
        // application set variables
        public PlatformType PlatformType { get; set; }
        public Version PlatformVersion { get; set; }
        public string PlatformName { get; set; }
        public string ChocolateyVersion { get; set; }
        public string ChocolateyProductVersion { get; set; }
        public string FullName { get; set; }
        public bool Is64BitOperatingSystem { get; set; }
        public bool Is64BitProcess { get; set; }
        public bool IsInteractive { get; set; }
        public string UserName { get; set; }
        public string UserDomainName { get; set; }
        public bool IsUserAdministrator { get; set; }
        public bool IsUserSystemAccount { get; set; }
        public bool IsUserRemoteDesktop { get; set; }
        public bool IsUserRemote { get; set; }
        public bool IsProcessElevated { get; set; }
        public string CurrentDirectory { get; set; }
    }

    [Serializable]
    public sealed class FeaturesConfiguration
    {
        public bool AutoUninstaller { get; set; }
        public bool ChecksumFiles { get; set; }
        public bool AllowEmptyChecksums { get; set; }
        public bool AllowEmptyChecksumsSecure { get; set; }
        public bool FailOnAutoUninstaller { get; set; }
        public bool FailOnStandardError { get; set; }
        public bool UsePowerShellHost { get; set; }
        public bool LogEnvironmentValues { get; set; }
        public bool LogWithoutColor { get; set; }
        public bool VirusCheck { get; set; }
        public bool FailOnInvalidOrMissingLicense { get; set; }
        public bool IgnoreInvalidOptionsSwitches { get; set; }
        public bool UsePackageExitCodes { get; set; } 
        public bool UseEnhancedExitCodes { get; set; }
        public bool UseFipsCompliantChecksums { get; set; }
        public bool ShowNonElevatedWarnings { get; set; }
        public bool ShowDownloadProgress { get; set; }
        public bool StopOnFirstPackageFailure { get; set; }
        public bool UseRememberedArgumentsForUpgrades { get; set; }
        public bool IgnoreUnfoundPackagesOnUpgradeOutdated { get; set; }
        public bool SkipPackageUpgradesWhenNotInstalled { get; set; }
        public bool ExitOnRebootDetected { get; set; }
        public bool LogValidationResultsOnWarnings { get; set; }
        public bool UsePackageRepositoryOptimizations { get; set; }
        public bool UseShimGenService { get; set; }
    }

    //todo: retrofit other command configs this way

    [Serializable]
    public sealed class ListCommandConfiguration
    {
        public ListCommandConfiguration()
        {
            PageSize = 25;
        }

        // list
        public bool LocalOnly { get; set; }
        bool? _showRegistryPackages;
        
        /// <summary>
        /// When not running unit tests - we always return this as true (feature rich).
        /// If running unit tests - by default we disable registry access (requires lock), unless
        /// overridden from test itself.
        /// </summary>
        public bool ShowRegistryPackages
        {
            get
            {
                if (!ApplicationParameters.runningUnitTesting)
                {
                    return true;
                }

                if (!_showRegistryPackages.HasValue)
                {
                    return false;
                }

                return _showRegistryPackages.Value;
            }

            set
            {
                _showRegistryPackages = value;
            }
        }

        public bool IdOnly { get; set; }
        public bool IncludeRegistryPrograms { get; set; }
        public int? Page { get; set; }
        public int PageSize { get; set; }
        public bool Exact { get; set; }
        public bool ByIdOnly { get; set; }
        
        /// <summary>
        /// When set to true - searches for text in package.Tag
        /// </summary>
        public bool ByTagOnly { get; set; }

        /// <summary>
        /// Specific tag to search
        /// </summary>
        public string SearchTag { get; set; }

        public bool IdStartsWith { get; set; }
        public bool OrderByPopularity { get; set; }
        public bool ApprovedOnly { get; set; }
        public bool DownloadCacheAvailable { get; set; }
        public bool NotBroken { get; set; }
        public bool IncludeVersionOverrides { get; set; }
    }

    [Serializable]
    public sealed class PsRunCommandConfiguration
    {
        public string step { get; set; } = "install";
        public bool keeptemp { get; set; }
    }


    [Serializable]
    public sealed class UpgradeCommandConfiguration
    {
        public bool FailOnUnfound { get; set; }
        public bool FailOnNotInstalled { get; set; }
        public bool NotifyOnlyAvailableUpgrades { get; set; }
        public string PackageNamesToSkip { get; set; }
        public bool ExcludePrerelease { get; set; }
    }

    [Serializable]
    public sealed class NewCommandConfiguration
    {
        public NewCommandConfiguration()
        {
            TemplateProperties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public string TemplateName { get; set; }
        public string Name { get; set; }
        public bool AutomaticPackage { get; set; }
        public IDictionary<string, string> TemplateProperties { get; private set; }
        public bool UseOriginalTemplate { get; set; }
    }

    [Serializable]
    public sealed class SourcesCommandConfiguration
    {
        public string Name { get; set; }
        public SourceCommandType Command { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Priority { get; set; }
        public string Certificate { get; set; }
        public string CertificatePassword { get; set; }
        public bool BypassProxy { get; set; }
        public bool AllowSelfService { get; set; }
        public bool VisibleToAdminsOnly { get; set; }
    }

    [Serializable]
    public sealed class MachineSourceConfiguration
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public int Priority { get; set; }
        public string Certificate { get; set; }
        public string EncryptedCertificatePassword { get; set; }
        public bool BypassProxy { get; set; }
        public bool AllowSelfService { get; set; }
        public bool VisibleToAdminsOnly { get; set; }
    }

    [Serializable]
    public sealed class FeatureCommandConfiguration
    {
        public string Name { get; set; }
        public FeatureCommandType Command { get; set; }
    }

    [Serializable]
    public sealed class ConfigCommandConfiguration
    {
        public string Name { get; set; }
        public string ConfigValue { get; set; }
        public ConfigCommandType Command { get; set; }
    }

    [Serializable]
    public class PinCommandConfiguration
    {
        public string Name { get; set; }
        public PinCommandType Command { get; set; }
        public string Sources { get; set; }
        public bool Unpinned { get; set; }
    }

    [Serializable]
    public sealed class OutdatedCommandConfiguration
    {
        public bool IgnorePinned { get; set; }
    }

    [Serializable]
    public sealed class ApiKeyCommandConfiguration
    {
        public string Key { get; set; }
        public bool Remove { get; set; }
    }

    [Serializable]
    public sealed class PackCommandConfiguration
    {
        public PackCommandConfiguration()
        {
            Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, string> Properties { get; private set; }
        public string InputDirectory { get; set; }
    }

    [Serializable]
    public sealed class PushCommandConfiguration
    {
        public string Key { get; set; }
        //DisableBuffering?
    }

    [Serializable]
    public sealed class ProxyConfiguration
    {
        public string Location { get; set; }
        public string User { get; set; }
        public string EncryptedPassword { get; set; }
        public string BypassList { get; set; }
        public bool BypassOnLocal { get; set; }
    }
}
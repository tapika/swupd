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

namespace chocolatey.infrastructure.app.runners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector;
    using configuration;
    using infrastructure.validations;
    using logging;
    using utility;
    using validations;

    /// <summary>
    ///   Console application responsible for running chocolatey
    /// </summary>
    public sealed class ConsoleApplication
    {
        public void run(string[] args, ChocolateyConfiguration config, Container container)
        {
            var commandLine = Environment.CommandLine;

            if (ArgumentsUtility.arguments_contain_sensitive_information(commandLine))
            {
                this.Log().Debug(() => "Command line not shown - sensitive arguments may have been passed.");
            }
            else
            {
                this.Log().Debug(() => "Command line: {0}".format_with(commandLine));
                this.Log().Debug(() => "Received arguments: {0}".format_with(string.Join(" ", args)));
            }

            IList<string> commandArgs = new List<string>();
            //shift the first arg off
            int count = 0;
            foreach (var arg in args)
            {
                if (count == 0)
                {
                    count += 1;
                    continue;
                }

                commandArgs.Add(arg);
            }

            var runner = new GenericRunner();
            runner.run(config, container, isConsole: true, parseArgs: command =>
                {
                    if (!ChocolateyOptionSet.Instance.Parse(commandArgs, command, config))
                    {
                        this.Log().Debug(() => "Performing validation checks.");
                        command.handle_validation(config);

                        var validationResults = new List<ValidationResult>();
                        var validationChecks = container.GetAllInstances<IValidation>();
                        foreach (var validationCheck in validationChecks)
                        {
                            validationResults.AddRange(validationCheck.validate(config));
                        }

                        var validationErrors = report_validation_summary(validationResults, config);

                        if (validationErrors != 0)
                        {
                            // NOTE: This is intentionally left blank, as the reason for throwing is
                            // documented in the report_validation_summary above, and a duplication
                            // is not required in the exception.
                            throw new ApplicationException("");
                        }
                    }
                });
        }

        private int report_validation_summary(IList<ValidationResult> validationResults, ChocolateyConfiguration config)
        {
            var successes = validationResults.Count(v => v.Status == ValidationStatus.Success);
            var warnings = validationResults.Count(v => v.Status == ValidationStatus.Warning);
            var errors = validationResults.Count(v => v.Status == ValidationStatus.Error);

            var logOnWarnings = config.Features.LogValidationResultsOnWarnings;
            if (config.RegularOutput)
            {
                this.Log().Info(errors + (logOnWarnings ? warnings : 0) == 0 ? ChocolateyLoggers.LogFileOnly : ChocolateyLoggers.Important, () => "{0} validations performed. {1} success(es), {2} warning(s), and {3} error(s).".format_with(
                    validationResults.Count,
                    successes,
                    warnings,
                    errors));

                if (warnings != 0)
                {
                    var warningLogger = logOnWarnings ? ChocolateyLoggers.Normal : ChocolateyLoggers.LogFileOnly;
                    this.Log().Info(warningLogger, "");
                    this.Log().Warn(warningLogger, "Validation Warnings:");
                    foreach (var warning in validationResults.Where(p => p.Status == ValidationStatus.Warning).or_empty_list_if_null())
                    {
                        this.Log().Warn(warningLogger, " - {0}".format_with(warning.Message));
                    }
                }
            }

            if (errors != 0)
            {
                this.Log().Info("");
                this.Log().Error("Validation Errors:");
                foreach (var error in validationResults.Where(p => p.Status == ValidationStatus.Error).or_empty_list_if_null())
                {
                    this.Log().Error(" - {0}".format_with(error.Message));
                }
            }

            return errors;
        }
    }
}
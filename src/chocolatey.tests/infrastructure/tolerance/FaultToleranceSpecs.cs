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

namespace chocolatey.tests.infrastructure.tolerance
{
    using System;
    using chocolatey.infrastructure.tolerance;
    using NUnit.Framework;
    using FluentAssertions;

    public class FaultToleranceSpecs
    {
        public abstract class FaultToleranceSpecsBase : TinySpec
        {
            public override void Context()
            {
            }

            protected void reset()
            {
                MockLogger.reset();
            }
        }

        public class when_retrying_an_action : FaultToleranceSpecsBase
        {
            public override void Because()
            {
            }

            [Fact]
            public void should_not_allow_the_number_of_retries_to_be_zero()
            {
                reset();

                Assert.Throws<ApplicationException>(() => FaultTolerance.retry(0, () => { }));
            }

            [Fact]
            public void should_throw_an_error_if_retries_are_reached()
            {
                reset();

                Assert.Throws<Exception>(
                    () => FaultTolerance.retry(2, () => { throw new Exception("YIKES"); }, waitDurationMilliseconds: 0),
                    "YIKES"
                );
            }

            [Fact]
            public void should_log_warning_each_time()
            {
                reset();

                try
                {
                    FaultTolerance.retry(3, () => { throw new Exception("YIKES"); }, waitDurationMilliseconds: 0);
                }
                catch
                {
                    // don't care
                }

                MockLogger.MessagesFor(LogLevel.Warn).Count.Should().Be(2);
            }

            [Fact]
            public void should_retry_the_number_of_times_specified()
            {
                reset();

                var i = 0;
                try
                {
                    FaultTolerance.retry(
                        10,
                        () =>
                        {
                            i += 1;
                            throw new Exception("YIKES");
                        },
                        waitDurationMilliseconds: 0);
                }
                catch
                {
                    // don't care
                }

                i.Should().Be(10);
            }

            [Fact]
            public void should_return_immediately_when_successful()
            {
                reset();

                var i = 0;
                FaultTolerance.retry(3, () => { i += 1; }, waitDurationMilliseconds: 0);

                i.Should().Be(1);

                MockLogger.MessagesFor(LogLevel.Warn).Count.Should().Be(0);
            }
        }

        public class when_wrapping_a_function_with_try_catch : FaultToleranceSpecsBase
        {
            public override void Because()
            {
            }

            [Fact]
            public void should_log_an_error_message()
            {
                reset();

                FaultTolerance.try_catch_with_logging_exception(
                    () => { throw new Exception("This is the message"); },
                    "You have an error"
                );

                MockLogger.MessagesFor(LogLevel.Error).Count.Should().Be(1);
            }

            [Fact]
            public void should_log_the_expected_error_message()
            {
                reset();

                FaultTolerance.try_catch_with_logging_exception(
                    () => { throw new Exception("This is the message"); },
                    "You have an error"
                );

                MockLogger.MessagesFor(LogLevel.Error)[0].Should().Be("You have an error:{0} This is the message".format_with(Environment.NewLine));
            }

            [Fact]
            public void should_log_a_warning_message_when_set_to_warn()
            {
                reset();

                FaultTolerance.try_catch_with_logging_exception(
                    () => { throw new Exception("This is the message"); },
                    "You have an error",
                    logWarningInsteadOfError: true
                );

                MockLogger.MessagesFor(LogLevel.Warn).Count.Should().Be(1);
            }

            [Fact]
            public void should_throw_an_error_if_throwError_set_to_true()
            {
                reset();
                Assert.Throws<Exception>(
                    () => FaultTolerance.try_catch_with_logging_exception(
                        () => { throw new Exception("This is the message"); },
                        "You have an error",
                        throwError: true
                    ),
                    "This is the message"
                );
            }

            [Fact]
            public void should_still_throw_an_error_when_warn_is_set_if_throwError_set_to_true()
            {
                reset();

                Assert.Throws<Exception>(
                    () => FaultTolerance.try_catch_with_logging_exception(
                        () => { throw new Exception("This is the message"); },
                        "You have an error",
                        logWarningInsteadOfError: true,
                        throwError: true
                    ),
                    "This is the message"
                );

            }
        }
    }
}

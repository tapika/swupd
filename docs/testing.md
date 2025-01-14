# Overview

- [Overview](#overview)
- [Chocolatey unit testing](#chocolatey-unit-testing)
  * [Normal unit testing](#normal-unit-testing)
  * [Verifying log testing](#verifying-log-testing)
    + [What can be placed in log file?](#what-can-be-placed-in-log-file)
    + [How to construct your first test case ?](#how-to-construct-your-first-test-case-)
    + [From where logging can be performed ?](#from-where-logging-can-be-performed-)
    + [What happens when log line does not match to logged line ?](#what-happens-when-log-line-does-not-match-to-logged-line-)
      - [Post mortem debugging versus alive debugging](#post-mortem-debugging-versus-alive-debugging)
    + [Using for/while loops](#using-forwhile-loops)
    + [Developer is responsible for what log file contains](#developer-is-responsible-for-what-log-file-contains)
      - [Large scale code refactoring](#large-scale-code-refactoring)
    + [Multitasking](#multitasking)
    + [Unit test code amount](#unit-test-code-amount)
    + [Verifying log performance and usability](#verifying-log-performance-and-usability)
    + [Mocking and verifying log](#mocking-and-verifying-log)
    + [Code coverage and verifying log](#code-coverage-and-verifying-log)
    + [Log performance](#log-performance)

# Chocolatey unit testing

## Normal unit testing

`chocolatey` unit testing is based on `NUnit` test framework.

Typical test case adding looks like this:

```c#
[Fact]
public void MyTest()
{
    bool operationSucceed = true;
    Assert.IsTrue(operationSucceed);
}
```

You can use also `FluentAssertions` nuget package and can use also `Should` extension.

```c#
[Fact]
public void MyTest()
{
    bool operationSucceed = true;
    operationSucceed.Should().BeTrue();
}
```



Normal unit testing allows access to topmost public API's - however if you're interested in accessing lower layers of application - then you're dealing with either logging or Mock:ing of lower layer API's. 

## Verifying log testing

Besides normal unit testing, you can use also verifying log testing.

Verifying logging works so that test gets executed, test performs logging and logs gets saved into `.txt` log file and subsequent executions will be compared against first test run.

```c#
test.cs:

[Parallelizable(ParallelScope.All), FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
class TestClass: LogTesting
{ 

    [LogTest]
    public void MyTest()
    {
        bool operationSucceed = true;
        LogService.console.Info($"Operation status: {operationSucceed}");
    }

test\MyTest.txt:
Operation status: true
end of test
```



You can use normal unit testing for simple classes, more complex / integration level testing can be done with verifying log.

`LogService` is chocolatey class for handling logging functionality.

### What can be placed in log file?

Any text, which does not change from execution round can be used. Anything else what changes cannot be logged - this includes for example current date / time or absolute file / directory paths. If you're dealing with paths - extracting only filename can result in same string, which in a turn can be used for verifying log testing.

If you add any bigger listings - then maybe makes sense to sort them by name, just to get them in same order.

To not pollute log files too much - it's recommended to log just what you need - not everything.

### How to construct your first test case ?

If you just created new test case - you can switch it to always create log file - using attribute like this:

`[LogTest(true)]`

Log files must be committed into git as well, next to source codes - they represent golden recording of application flow.

`LogTest` attribute works in `Debug or RelWithDebug` configurations only - for release builds (and build machines) setting this flag is disabled. (See `VerifyingLogTarget`class, `allowToCreatingLog` variable `#if DEBUG` protection). This is done to prevent log recording on build machine.

### From where logging can be performed ?

Compared to normal unit testing - logging can be performed from any part of application.

At the moment log levels `Info, Warn, Error, Fatal` will be logged and on levels `Debug, Trace` will be not.

If you want to change default log level - you can use `LogService.Instance.AddVerifyingLogRule` to increase log level for individual class. Similar kind of code can we written to control filtering as per class or as per log level.

Logging from lower layers is ok if functionality is relatively complex, and you want to make sure that lower layers are executed correctly. If you want to log from lower layers - maybe makes sense to create separate unit test, especially focusing on that lower layer.

### What happens when log line does not match to logged line ?

Exception will be thrown from place where logging was done causing test to fail. Exception is thrown only during unit testing, not for normal application.

There is also possibility that developer has added `try-catch` block to guard against exception - then in this case same exception will be thrown from next logged line, in worst case scenario - at the end of test.

```c#

public void MyTest()
{
    try{
	  LogService.logger("1");   // < first exception will be thrown here
    } catch(Exception)
    {
    }
    LogService.logger("2");       // then will be thrown from here with same exception message (About invald "1" line)
}
```

Using generic exception handling is also bad from error handling perspective - in case above it's recommended to use more specific exception type than generic `Exception`-type.



#### Post mortem debugging versus alive debugging

With normal unit testing you're typically dealing with post mortem debugging - meaning functionality was executed and ended - you're comparing the results of last execution round. If code is rather complex or you don't understand how it works - then read code until you understand - so it's bit difficult - as always - when you're dealing with post mortem debugging.



With verifying log it's possible to do alive functionality debugging - for example you can even halt code in place where you want to bugfix or change it's behavior.

For exampe:

```c#
... somewhere in code ...

console.Info(Path.Combine("a", "b"))
```



Like you probably guess for windows this will output path with backslash (`\`), for linux with normal slash (`/`). For verifying log - this is not portable solution to have same unit test - as it will behave differently in each OS.

You can locate in your log file the log line which you don't like and modify that line in text:

```
test.txt:
1
a\b  --- modified...  place halt in place where logging of this line happens. (Any text modification here)
2
3
...
```

Then go into source code `VerifyingLogTarget.cs`, find line `Unexpected {lineN} line` - and set breakpoint in there.

Running debugger will halt you in place where that log line is logged out.

Generally when dealing with paths - you can either remove absolute path and just replace back slashes with front slashes. (`.Replace('\\', '/')`)

Using approach like this allow to debug alive functionality from point where execution is different.



### Using for/while loops

With normal unit testing using anykind of loops is rather difficult thing to do - it requires either copy-pasting `Assert`:s or creating separate expected results arrays / structures.

```
int multiply_by_two(int x)
{
    return i * 2;
}

var expectedResult = new int[] { 0, 2, 4, 6, 8 ... };
for(int i = 0; i < 20; i++)
{
    Assert.AreEqual(multiply_by_two(i), expectedResult[i]);
}

or:
    Assert.AreEqual(multiply_by_two(0), 0);
    Assert.AreEqual(multiply_by_two(1), 2);
    Assert.AreEqual(multiply_by_two(2), 4);
    Assert.AreEqual(multiply_by_two(3), 6);
    Assert.AreEqual(multiply_by_two(4), 8);
...
```

With verifying logging same functionality can be just logged out:

```
for(int i = 0; i < 20; i++)
{
    LogService.console.Info(multiply_by_two(i));
}
```

Where outcome will be saved into log file.

More loops - more complex unit test will become to develop and maintain if it's not verifying logging.

### Developer is responsible for what log file contains

Like with normal unit testing - it's possible to disable, abuse, make tests invalid - in similar manner it's possible to do also with verifying log.

One approach to re-record new result log file - is just delete it and re-run unit tests. Unit tests will produce new log file, after which all subsequent test invocations will be compared against new log file. That's why log file needs to be checked quite closely what it contains (like it's also with traditional unit tests)

#### Large scale code refactoring

Normally when you perform large scale code refactoring - it might influence multiple unit tests - they become broken. With normal unit testing you always need to fix unit test itself. With verifying logging - it's also possible to just delete all `*.txt` and then re-record new execution flow. It's recommended to closely review all log files after such refactoring.

### Multitasking

`chocolatey.tests & chocolatey.tests.integration` unit tests does not support multitasking, `chocolatey.tests2` includes verifying logging and also multi-tasking support. Multitasking is supported via usage `ASyncLocal` in `LogService & InstallContext` classes.

It's also possible that files packaged, installed, modified are used by subsequent run, and also multiple functions might depend on same prerequisite step.

```
[EmptyFolder] > (A) [Install installpackage v1.0] > (B) [Uninstall installpackage v1.0]
[EmptyFolder] > (A) [Install installpackage v1.0] > (C) [Upgrade to installpackage v2.0]
```

In this case step `(B)` depends on `(A)`, but also step `(C)` depends on `(A)`. `(A)` is launched as it's own separate task, where `(B) & (C)` waits for `(A)` to complete.

Common / reusable folders are stored in `tests_shared` folder and normal tests are in `tests`. Reusable folder are used as a cache for subsequent runs. Deleting that folder clears cache.

All folder names are determined by either test method name, or when test is common / reusable - it's located in `LogTesting.cs/PrepareTestContext function` and it's name is determined by `ChocoTestContext` enumeration.  Common / reused test has also it's own separate log (See `using (new VerifyingLog`).



When creating new tasks, they needs to be connected to same log factory as parent task - this can be done by backuping log factory in parent task:

`var loginstance = LogService.Instance;`

and restoring it's value in child task:

`LogService.Instance = loginstance;`

In main application `LogService.Instance` will be the same for all tasks.

### Unit test code amount

Verifying logging tests are normally 2-6 times smaller.

For example - when we deal with installation - we are typically interested on whether install application created new folder - this kind of check can be done using 

`Directory.Exists(packageDir).Should().BeTrue();`.

With verifying test logging - you don't care about whether folder was created or not - you can just list folders. Of course listing everything will pollute log file a lot - that's why it makes sense to perform comparison and log only file system changes.

```
var listBeforeUpdate = GetFileListing(rootDir);
...
var results = Service.install_run(conf);
...
var listAfterUpdate = GetFileListing(rootDir);

... compare what is changed between listBeforeUpdate & listAfterUpdate - and log changes...
```

For example older test framework had `71` lines of `Directory.Exists` comparisons, which can be compensated with `~30` lines of helper function performing same amount of comparison or even more.

### Verifying log performance and usability

In theory verifying log testing should be slower than normal unit testing - because data formatted to strings and because of log file usage itself. 

Verifying log testing in a turn uses smarter cache folder handling and uses multitasking.

Conversion of one set of integration test from old to new approach gave ~ 4.1 times faster results with 3.1 times smaller code.

Verifying log testing brings also simplicity to testing - normally two times smaller amount of code to write and also gives an opportunity to debug and troubleshoot functionality in live, quite often really close to failing point.

Of course if you instrument everything - it will dramatically slow down executed functionality - but quite often it's sufficient to instrument key function calls in code.

See also [Benchmarking 5 popular .NET logging libraries](https://www.loggly.com/blog/benchmarking-5-popular-net-logging-libraries/).

### Mocking and verifying log

You can also use `new Mock<I>` to create mocked object, where you can use `.Setup()` to set up custom mocked function, and `.Verify()` to verify that some specific function was or was not executed. `.Verify()` works in similar manner to normal unit testing - so verification of function call and it's arguments are performed after function call - again post mortem diagnostic. To cross connect our mock and logging - it's possible to use like this:

```c#
Mock<ISomeAPI> mock = new Mock<ISomeAPI>().Logged(
     nameof(ISomeAPI.utilityfunction1),
     nameof(ISomeAPI.utilityfunction2)
);
```

This approach forced to call all function in `ISomeAPI` except `utilityfunction1 & utilityfunction2` functions to log file. 

See also `LoggedMock.cs / LoggedMockInterceptor.cs`.

Proxying existing interfaces is useful when you want to know all function calls which were performed without logging instrumentation.

Once logging is enabled for mock - there is no need to use `.Verify()` anymore, as logging of function calls and it's argument should be sufficient.

Post mortem debugging is also changed to live debugging via logging.

### Code coverage and verifying log

Normally when dealing with loggers - in typical unit test scenarios you would try to replace logger and log somewhere - for example into buffer. Typical unit test case in that case looks like this (bit of pseudo code):

```
List<string> loggedData = ...;

...
loggedData.Add("Help of command list");
somewhereElse.Add("List usage:");
...

Assert.True(loggedData.Contains("Help of command list"));
```

 This give some headache to CPU, as afterwards you need to find specific string in array / buffer.

When logging with `LogService` - logged line will get immediately verified that it's valid one.

But you might not want to verify entire text which is displayed to end-user - then you can consider switching logger.

```
public virtual void help_message(ChocolateyConfiguration configuration)
{
    var (hiconsole, console) = LogService.Instance.HelpLoggers;

    hiconsole.Info("List/Search Command");

    if (ApplicationParameters.runningUnitTesting)
        console = hiconsole = LogService.Instance.NullLogger;

    hiconsole.Info("Usage");

```

So in this case - `"List/Search Command"` will be logged out and also tested that line will not change, but `"Usage"` will not be logged and not verified.

This gives some freedom that not all texts will introduce test breakage.

It's also possible to do same check like this:

    if (ApplicationParameters.runningUnitTesting)
        return;

But in this case rest of lines will not be executed at all, giving red color in code coverage.

Better to use earlier example, as it works better with code coverage.

It's also possible that some of strings will fail due to `null` reference violation, so it's better to test that code works, even thus it may not provide correct output.

### Log performance

Chocolatey is relatively small project so logging does influence it's performance much - however - it's possible that same testing framework could be used for other projects as well, except Chocolatey.

According to [NLog performance](https://github.com/NLog/NLog/wiki/Performance) wiki pages - if logging is not performed - then no need to perform string formatting as well - and for that purpose developer should use parameters instead of string interpolation (`$"Hello {world}"`).


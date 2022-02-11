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



Normal unit testing allows access to topmost public API's - however if you're interested in accessing lower layers of application - then you're dealing with either logging or Mock:ing lower layer API. 

## Verifying log testing

If verifying log case - test is executed once - it's execution will get recorded into `.txt` log file and subsequent executions will be compared against first run.

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

Any text, which does not change from execution round can be used. Anything else what changes cannot be logged - this includes for instance current date / time or absolute file / directory paths. If you're dealing with paths - extracting only filename could result in same string.

If you add any bigger listings - then maybe makes sense to sort them by name, just to get them in same order.

### How to construct your first test case ?

If you just created new test case - you can switch it to always create log file - using attribute like this:

`[LogTest(true)]`

This attribute works in `Debug` configuration only - for release builds (and build machines) setting this flag is disabled. (See `VerifyingLogTarget`class, `allowToCreatingLog` variable `#if DEBUG` protection).

Log files must be committed into git as well, next to source codes - they represent golden recording of application flow.

### From where logging can be performed ?

Compared to normal unit testing - logging can be performed from any part of application.

At the moment anything that is logged on levels `Info, Warn, Error, Fatal` will be logged / recorded and on levels `Debug, Trace` will be not.

In future it's possible to improve filtering based on logger name and/or log level - at this moment there were no such need. In chocolatey all loggers are named according to their own class name, where all resides in `chocolatey` namespace. That's why we have as a filtering rule `chocolatey.*` - it will pick up all chocolatey classes.

### What happens when log line does not match to logged line ?

Exception will be thrown from place where logging was done. Exception is thrown only during unit testing, not for normal application.

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

(Using generic exception handling is also bad from error handling perspective)



#### Post mortem debugging versus alive debugging

With normal unit testing you're typically dealing with post mortem debugging - meaning functionality was executed and ended - you're comparing the results of execution. If code is rather complex or you don't understand how it works - then read code until you understand - so it's bit difficult - as always when you're dealing with post mortem debugging.



With verifying log it's possible to do alive functionality debugging - for example you can even halt code in place where you want to bugfix or change it's behavior.

... somewhere in code ...

`console.Info(Path.Combine("a", "b"))`

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

### Unit test code amount

Unit tests written with verifying logging concept are normally 2-6 times smaller.

Even when we deal with installation - we are typically interested on whether install application created new folder - this kind of check can be done using 

`Directory.Exists(packageDir).Should().BeTrue();`.

With logging - you don't care about whether folder was created or not - you can just list of folders. Of course listing everything will pollute log file a lot - that's why it makes sense to perform comparison and log only changes in file system.

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

Verifying log testing brings also simplicity to testing - normally two times smaller amount of code to write and gives opportunity to debug and troubleshoot functionality in live, quite often really close to failing point.

Of course if you instrument everything - it will dramatically slow down executed functionality - but quite often it's sufficient to instrument key function calls in code.

### Mocking and verifying log

You can also use `new Mock<I>` to create mocked object, where you can use `.Setup()` to set up custom mocked function, and `.Verify()` to verify that some specific function was or was not executed. `.Verify()` works in similar manner to normal unit testing - so verification of function call and it's arguments are performed after call was performed - again post mortem diagnostic. To cross connect our mock and logging - it's possible to use like this:

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


namespace logtesting
{
    using NUnit.Framework;
    using System.Reflection;
    using chocolatey.infrastructure.logging;
    using NLog;
    using NUnit.Framework.Internal;

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
            var mi = getTestMethodInfo();
            if (mi.Item1 == null)
            {
                return;
            }

            string path = mi.Item1.GetFilePath(mi.Item2);
            string testfunc = TestContext.CurrentContext.Test.MethodName;

            var factory = LogService.GetInstance(false).LogFactory;
            var conf = factory.Configuration;
            var verifyingLogTarget = new VerifyingLogTarget("verifyingLog", $"{path}_{testfunc}.txt", mi.Item1.CreateLog);
            conf.AddTarget(verifyingLogTarget);

            foreach (var name in LogService.GetLoggerNames())
            {
                conf.AddRule(LogLevel.Info, LogLevel.Fatal, verifyingLogTarget, name);
            }

            factory.ReconfigExistingLoggers();
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

            LogService.Instance.LogFactory.Configuration.RemoveTarget("verifyingLog");
        }
    }
}



using chocolatey.infrastructure.logging;
using logtesting;
using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace chocolatey.tests2
{
    [Parallelizable(ParallelScope.All)]
    public class MultiThreadedLogging: LogTesting
    {
        void MultiTaskTesting(string testName)
        { 
            LogService.console.Info($"{testName} started");

            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(testName);
            byte[] hash = md5.ComputeHash(inputBytes);
            int hashcode = BitConverter.ToInt32(hash, 0);

            Random rand = new Random(hashcode);
            int c = rand.Next() % 10;

            for (int i = 0; i < c; i++)
            {
                System.Threading.Thread.Sleep(rand.Next() % 400);
                LogService.console.Info($"{testName}: {i}-th iteration");
            }

            LogService.console.Info($"{testName} ended");
        }

        [LogTest]
        public void Test1()
        {
            MultiTaskTesting(nameof(Test1));
        }

        [LogTest]
        public void Test2()
        {
            MultiTaskTesting(nameof(Test2));
        }

        [LogTest]
        public void Test3()
        {
            MultiTaskTesting(nameof(Test3));
        }

        [LogTest]
        public void Test4()
        {
            MultiTaskTesting(nameof(Test4));
        }

        [LogTest]
        public void Test5()
        {
            MultiTaskTesting(nameof(Test5));
        }
    }
}

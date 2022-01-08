using chocolatey.infrastructure.logging;
using logtesting;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace chocolatey.tests2
{
    [Parallelizable(ParallelScope.All)]
    public class MultiThreadedLogging: LogTesting
    {
        static ConcurrentDictionary<string, Task> updaters = new ConcurrentDictionary<string, Task>();

        Random GetRandomForString(string randomSeedString)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(randomSeedString);
                byte[] hash = md5.ComputeHash(inputBytes);
                int hashcode = BitConverter.ToInt32(hash, 0);

                return new Random(hashcode);
            }
        }

        /// <summary>
        /// Note commented lines: PerformOperation might be called from different task, if you activate any of those
        /// logs - tests will be failing randomly (as Test1 - Test5 are started each with it's own threshold).
        /// But if we observe only data which is the same - then execution is identical from test application perspective.
        /// </summary>
        void PerformOperation(string testName, string operation)
        {
            var newTask = new Task(() =>
            {
                using (new VerifyingLog("_" + operation))
                {
                    //Random rand = GetRandomForString(operation + "_" + testName);
                    Random rand = GetRandomForString(operation);
                    //LogService.console.Info($"Performing {operation} (called from {testName})");
                    Console.WriteLine($"{operation} was started from {testName}");  // This might be different everytime
                    LogService.console.Info($"Performing {operation}");
                    int c = rand.Next() % 10;

                    for (int i = 0; i < c; i++)
                    {
                        System.Threading.Thread.Sleep(rand.Next() % 50);
                        LogService.console.Info($"{operation}: {i}-th iteration");
                    }
                    //LogService.console.Info($"Resulting {operation} (called from {testName})");
                    LogService.console.Info($"Resulting {operation}");
                }
            });

            var task = updaters.GetOrAdd(operation, newTask);
            if (task == newTask)
            {
                newTask.Start();
                //LogService.console.Info($"New task started");
            }
            else
            { 
                //LogService.console.Info($"Waiting for existing task to complete");
            }
            
            LogService.console.Info($"Waiting for {operation} task to complete");
            task.Wait();
        }

        void MultiTaskTesting(string testName)
        { 
            LogService.console.Info($"{testName} started");

            Random rand = GetRandomForString(testName);
            int c = rand.Next() % 10;

            for (int i = 0; i < c; i++)
            {
                if (rand.Next() % 3 == 1)
                {
                    PerformOperation(testName, "install");
                }

                if (rand.Next() % 2 == 1)
                {
                    PerformOperation(testName, "upgrade");
                }

                System.Threading.Thread.Sleep(rand.Next() % 200);
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

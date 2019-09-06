using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CertificationTraining
{
    public static class Tasks
    {
        public static void DoWork ()
        {
            Console.WriteLine("Work Starting");
            Thread.Sleep(2000);
            Console.WriteLine("Work Finished");
        }

        public static void TaskStartDemo ()
        {
            Task newTask = new Task(() => DoWork());
            newTask.Start();
            newTask.Wait();
        }

        // So, in the.NET Framework 4.5 Developer Preview, we’ve introduced the new Task.Run method.
        // This in no way obsoletes Task.Factory.StartNew, but rather should simply be thought of as 
        // a quick way to use Task.Factory.StartNew without needing to specify a bunch of parameters.
        // It’s a shortcut. In fact, Task.Run is actually implemented in terms of the same logic used for Task.Factory.StartNew, 
        // just passing in some default parameters.When you pass an Action to Task.Run:

        //Task.Run(someAction);
        //        that’s exactly equivalent to:

        //Task.Factory.StartNew(someAction, 
        //CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);

        public static void TaskRunDemo ()
        {
            Task newTask = Task.Run(() => DoWork());
            newTask.Wait();
        }

        public static int CalculateResult()
        {
            Console.WriteLine("Work Starting");
            Thread.Sleep(2000);
            Console.WriteLine("Work Finished");
            return 99;
        }

        public static void ReturnValueFromTask()
        {
            Task<int> task = Task.Run(() =>
            {
                return CalculateResult();
            });

            Console.WriteLine(task.Result);
            Console.WriteLine("Finished processing, press any key to end.");
            Console.ReadKey();
        }

        public static void DoWork(int i)
        {
            Console.WriteLine("Task [0] starting", i);
            Thread.Sleep(2000);
            Console.WriteLine("Task [0] Finished", i);
        }

        public static void WaitAllTasks()
        {
            Task[] tasks = new Task[10];

            for(int i = 0; i < 10; i++)
            {
                int taskNum = i; // if you don't do this the loop gets ahead of the task runs and you get a 10 for all of them
                tasks[i] = Task.Run(() => DoWork());
            }

            Task.WaitAll(tasks);

            Console.WriteLine("Finished processing. Press a key.");
            Console.ReadKey();
        }

        public static void HelloTask()
        {
            Thread.Sleep(1000);
            Console.WriteLine("Hello");
        }

        public static void WorldTask()
        {
            Thread.Sleep(1000);
            Console.WriteLine("World");
        }

        public static void Continuations()
        {
            Task task = Task.Run(() => HelloTask());
            task.ContinueWith((prevTask) => WorldTask()
            ,TaskContinuationOptions.OnlyOnRanToCompletion
            );

            Console.WriteLine("Finished processing. Press any key");
            Console.ReadKey();
        }

        public static void DoChildTask(object state)
        {
            Console.WriteLine("Child {0} starting", state);
            Thread.Sleep(2000);
            Console.WriteLine("Child {0} finished", state);
        }

        public static void Demo_AttachedChildTask()
        {
            var parent = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Parent Starts");
                for(int i = 0; i < 10; i++)
                {
                    int taskNum = i;
                    Task.Factory.StartNew(x => 
                    DoChildTask(x), // lambda
                    taskNum, // state object
                    TaskCreationOptions.AttachedToParent); // parent will not finish until all children are finished
                }
            });

            parent.Wait(); // will wait for all attached children to complete

            Console.WriteLine("Parent finished, press any key to end");
            Console.ReadKey();
        }
    }
}

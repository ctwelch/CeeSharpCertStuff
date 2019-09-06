using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CertificationTraining
{
    public static class ManageMultithreading
    {
        static int[] items = Enumerable.Range(0, 50000001).ToArray();

        public static void SingleTaskSumming()
        {
            long total = 0;

            for (int i = 0; i < items.Length; i++)
            {
                total = total + items[i];
            }

            Console.WriteLine("The total is: {0}", total);
        }

        static long sharedTotal;

        static void AddRangeOfValues(int start, int end)
        {
            while(start < end)
            {
                sharedTotal += items[start];
                start++;
            }
        }

        // The problem here is withh the way in which all the tasks interact over the same shared value
        public static void DemoProblem()
        {
            List<Task> tasks = new List<Task>();

            int rangeSize = 1000;
            int rangeStart = 0;

            while (rangeStart < items.Length)
            {
                int rangeEnd = rangeStart + rangeSize;

                if(rangeEnd > items.Length)
                {
                    rangeEnd = items.Length;
                }

                // create local copies of the parameters
                int rs = rangeStart;
                int re = rangeEnd;

                // Consider this scenario
                // Task #1 starts performing an update to sharedTotal
                //  it fetches the contents of the sharedTotal variable into the CPU
                //  then adds an array element to sharedTotal but just before it writes it to memory it is interrupted
                // Task #2 updates sharedTotal and successfully writes it to memory then returns control to Task #1
                // Task #1 then writes to sharedTotal the value it has from the CPU, effectively wiping out the opertaion from Task #2

                // this is a race condition and the behavior of the program 
                // is dependent on which threads first get to the sharedTotal variable
                // tasks are fighting over a single value

                tasks.Add(Task.Run(() => AddRangeOfValues(rs, re)));
                rangeStart = rangeEnd;
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("The total is: {0}", sharedTotal);
        }

        // the ConcurrentDictionary has atomic actions, they cannot be interrupted by another process
        // a program can use Locking to ensure that a given action is atomic
        // Atomic actions are performed to completion
        // access to an atomic action is controlled by a locking object
        // the restroom key analogy -- if the key is gone have to wait until it comes back

        static object sharedTotalLock = new object();

        // the problem here is we have killed any benefit from multithreading
        // tasks spend most of their time waiting in like to get access to the shared resource
        static void AddRangeOfValuesSafe(int start, int end)
        {
            while (start < end)
            {
                lock (sharedTotalLock)
                {
                    sharedTotal = sharedTotal + items[start];
                }
                start++;
            }
        }

        static void AddRangeOfValuesSafeSmart(int start, int end)
        {
            long subTotal = 0;

            while (start < end)
            {
                subTotal = subTotal + items[start];
                start++;                
            }
            // here we only make one access to sharedTotal
            lock (sharedTotalLock)
            {
                sharedTotal = sharedTotal + subTotal;
            }
        }

        // Monitors allow a program to ensure that only one thread at a time can access a particular object
        // and they can check if they will be blocked
        static void AddRangeOfValuesWithMonitor(int start, int end)
        {
            long subTotal = 0;

            while (start < end)
            {
                subTotal = subTotal + items[start];
                start++;
            }

            //Monitor.Enter(sharedTotalLock);
            // TryEnter has overloads that help you write code that will wait and retry the lock
            if (Monitor.TryEnter(sharedTotalLock)) // TryEnter returns false when the lock could not be obtained
            {
                try
                {
                    sharedTotal = sharedTotal + subTotal;
                }
                catch
                {

                }
                finally
                {
                    Monitor.Exit(sharedTotalLock);
                }
            }
                  
        }

        // What you must avoid is the case of two different tasks that are each waiting on the other -- a deadlock
        static object lock1 = new object();
        static object lock2 = new object();

        static void Method1()
        {
            lock (lock1)
            {
                Console.WriteLine("Method 1 got lock 1");
                Console.WriteLine("Method 1 waiting for lock 2");
                lock (lock2)
                {
                    Console.WriteLine("Method 1 got lock 2");
                }
                Console.WriteLine("Method1 1 released lock 2");
            }
            Console.WriteLine("Method 1 released lock 1");
        }
        static void Method2()
        {
            lock (lock2)
            {
                Console.WriteLine("Method 2 got lock 2");
                Console.WriteLine("Method 2 waiting for lock 1");
                lock (lock1)
                {
                    Console.WriteLine("Method 2 got lock 1");
                }
                Console.WriteLine("Method1 2 released lock 1");
            }
            Console.WriteLine("Method 2 released lock 2");
        }

        public static void DemoDeadlockProblem()
        {
            // When these two methods are called one after the other, everything is fine
            //Method1();
            //Method2();
            // But when we use tasks
            Task t1 = Task.Factory.StartNew(() => Method1(), TaskCreationOptions.LongRunning);
            Task t2 = Task.Factory.StartNew(() => Method2(), TaskCreationOptions.LongRunning);
            Console.WriteLine("waiting for Task 2");
            t2.Wait();
            Console.WriteLine("Tasks Complete");
        }

        // Interlocked offers a better way to achieve thread safe access to the contents of a shared variable
        static void AddRangeOfValuesWithInterlocked(int start, int end)
        {
            long subTotal = 0;

            while (start < end)
            {
                subTotal = subTotal + items[start];
                start++;
            }

            Interlocked.Add(ref sharedTotal, subTotal);
            // Can also increment, decrement and swap two variables
        }

        // marking a variable as valotile will prevent the compiler from optimizing operations involving the variable

        // Threads can be aborted, while Tasks must monitor a CancellationToken
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        static void Clock()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Tick");
                Thread.Sleep(500);
            }
        }

        public static void DemoCancellationToken()
        {
            Task.Run(() => Clock());
            Console.WriteLine("Press any key to stop the clock");
            Console.ReadKey();
            cancellationTokenSource.Cancel();
            Console.WriteLine("Clock Stopped");
        }

        static int tickCount = 0;

        static void Clock(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && tickCount < 20)
            {
                tickCount++;
                Console.WriteLine("Tick");
                Thread.Sleep(500);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public static void DemoHandlingExceptionsWithCancellations()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task clock = Task.Run(() => Clock(cancellationTokenSource.Token));

            Console.WriteLine("Press any key to stop the clock");
            Console.ReadKey();

            if (clock.IsCompleted)
            {
                Console.WriteLine("Clock task completed");
            }
            else
            {
                try
                {
                    cancellationTokenSource.Cancel();
                    clock.Wait();
                }
                catch(AggregateException ex)
                {
                    Console.WriteLine("Clock stopped: {0}", ex.InnerExceptions[0].ToString());
                }
            }
        }
        
    }
}

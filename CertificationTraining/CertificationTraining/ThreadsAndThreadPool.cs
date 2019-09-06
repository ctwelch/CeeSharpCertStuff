using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CertificationTraining
{
    public static class ThreadsAndThreadPool
    {
        // Threads are created as foreground processes, unless set to run as background 
        // The OS will run a foreground process to completion, will not terminate while it contains an active foreground thread

        // Tasks are created as background processes, it terminates when all foreground threads complete

        // Threads have a priority property that can change over the lifetime of the thread
        // Cannot set the priority of a Task

        // A Thread cannot deliver a result to another thread
        // They must communicate using shared variables, introducing synchronization problems

        // Threads have no continuation, they have join which allows one thread to pause until another completes

        // Tasks provide exception aggregation, which Threads dont.
        static void ThreadHello()
        {
            Console.WriteLine("Hello from the thread");
            Thread.Sleep(2000);
        }

        public static void SayHello()
        {
            Thread thread = new Thread(ThreadHello);
            thread.Start();
        }

        public static void DemoThreadHello()
        {
            Thread thread = new Thread(() =>
            {
                Console.WriteLine("Hellofrom the thread");
            });
            thread.Start();
            Console.WriteLine("Press any key to end");
            Console.WriteLine("Waiting....");
            Console.ReadKey();
            
        }

        // old version of .NET used this to specify which method the thread should execute
        public static void UsingThreadStartDelegate()
        {
            ThreadStart threadStart = new ThreadStart(ThreadHello);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }

        // to pass a parameter value to a thread
        static void WorkOnData(object data)
        {
            Console.WriteLine("Working on: {0}", data);
            Thread.Sleep(1000);
        }

        public static void DemoPassParameterToThread()
        {
            ParameterizedThreadStart ps = new ParameterizedThreadStart(WorkOnData);
            Thread thread = new Thread(ps);
            thread.Start(99);
        }

        // or you can just use a lambda that takes a parameter
        public static void PassDataWithLambda()
        {
            // the data passed into the thread is always passed as an object reference
            // this way means there is no way to be sure at compile time that 
            // thread initialization is being performed with a particular type of data
            Thread thread = new Thread((data) =>
            {
                WorkOnData(data);
            });
            thread.Start();
        }

        static bool tickRunning; 
        // Threads have an abort method which terminates the thread immediately
        public static void RunTickThread()
        {
            tickRunning = true;

            Thread tickThread = new Thread(() =>
            {
                while (tickRunning)
                {
                    Console.WriteLine("Tick");
                    Thread.Sleep(1000);
                }
            });

            tickThread.Start();

            Console.WriteLine("Press a key to stop the ticking");
            Console.ReadKey();
            //tickThread.Abort();
            tickRunning = false; // better to do it this way because Abort stops everything immediately
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        // When a Thread calls the join method on another thread, the caller is held until the other thread completes
        public static void UsingJoin()
        {
            Thread threadToWaitFor = new Thread(() =>
            {
                Console.WriteLine("Thread starting");
                Thread.Sleep(2000);
                Console.WriteLine("Thread complete");
            });

            threadToWaitFor.Start();
            Console.WriteLine("Joining thread");
            threadToWaitFor.Join();            
        }

        // ThreadStatic 
        static ThreadLocal<Random> RandomGenerator =
            new ThreadLocal<Random>(() =>
            {
                return new Random(2);
            });

        // When different threads use the value of their RandomGenerator 
        // they will all produce the same sequence of random variables
        public static void DemoThreadLocal()
        {
            Thread t1 = new Thread(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("t1: {0}", RandomGenerator.Value.Next(10));
                    Thread.Sleep(500);
                }
            });

            Thread t2 = new Thread(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("t2: {0}", RandomGenerator.Value.Next(10));
                    Thread.Sleep(500);
                }
            });

            t1.Start();
            t2.Start();
        }

        // ThreadContext
        static void DisplayThread(Thread t)
        {
            Console.WriteLine("Name: {0}", t.Name);
            Console.WriteLine("Culture: {0}", t.CurrentCulture);
            Console.WriteLine("Priority: {0}", t.Priority);
            Console.WriteLine("Context: {0}", t.ExecutionContext);
            Console.WriteLine("IsBackground: {0}", t.IsBackground);
            Console.WriteLine("IsPool: {0}", t.IsThreadPoolThread);
        }

        public static void ShowThreadInfo()
        {
            Thread.CurrentThread.Name = "Entry point";
            DisplayThread(Thread.CurrentThread);
        }

        // Thread Pool stores a collection of reusable thread objects
        // It limits the number of active threads and maintains a queue of threads waiting to execute
        // so you don't overwhelm the device when you spawn a bunch of threads
        // ThreadPool only has a finite number of threads and you can block it if you create too many they will be idel for a long time
        // cannot manage the priority of a thread in a thread pool
        // ThreadPool threads are always background threads
        // local state variables should not be used because they are not cleared when a thread is reused

        static void DoThreadPoolWork(object state)
        {
            Console.WriteLine("Doing work: {0}", state);
            Thread.Sleep(500);
            Console.WriteLine("Work finished: {0}", state);
        }

        public static void DemoThreadPool()
        {
            for(int i = 0; i < 50; i++)
            {
                int stateNumber = i;
                // QueueUserWorkItem allocates a thread to run the supplied item of work
                ThreadPool.QueueUserWorkItem(state => DoThreadPoolWork(stateNumber));
            }
        }
    }
}

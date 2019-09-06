using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CertificationTraining
{
    public static class AsyncAndAwait
    {
        private static double computeAverages(long numOfValues)
        {
            double total = 0;
            Random rand = new Random();

            for (long values = 0; values < numOfValues; values++)
            {
                total = total + rand.NextDouble();
            }
            return total / numOfValues;
        }

        private static Task<double> asyncComnputeAverages(long numOfValues)
        {
            return Task<double>.Run(() =>
            {
                return computeAverages(numOfValues);
            });
        }

        // await keyword represents a statement of intent to perform an action
        // The keyword precedes a call of a method that will return the task to be performed
        // The compiler will generate code that will cause the async method
        //  to return to the caller at the point the await keyword is reached.
        // The compiler then will generate code that will perform the awaited action asynchronously
        //  and then contunue with the body of the async method

        public static async void DemoAsyncRunTask()
        {
            long numOfValues = 50;
            Console.WriteLine("Calculating...");
            double result = await (asyncComnputeAverages(numOfValues));
            Console.WriteLine("Result: " + result.ToString());
        }

        private static async Task<string> FetchWebPage(string url)
        {
            HttpClient httpClient = new HttpClient();
            return await httpClient.GetStringAsync(url);
        }

        private static async Task<IEnumerable<string>> FetchWebPages(string[] urls)
        {
            var tasks = new List<Task<String>>();
            foreach(string url in urls)
            {
                tasks.Add(FetchWebPage(url));
            }

            return await Task.WhenAll(tasks);
        }

        public static async void DemoAsyncWebRequest()
        {
            try
            {
                string resultText = await FetchWebPage("https://www.Google.com");
            }
            catch(Exception ex)
            {
                // it is possible to catch an exception here because FetchWebPage returns a result
                // if it were a void method 
                // even a method that just performs an action should return a status value so exceptions can be caught
                string result = ex.Message;
            }
        }

        //// Thread Safe describes code elements that work correctly 
        //   when used from multiple processes(tasks) at the same time

        // The standard .NET collections are not thread safe: List, Queue, Dictionary

        // Use these when designing multi-tasking applications:

        // BlockingCollection<T>

        //   Best to view each task as either a producer or consumer of data
        //   Provides a thread safe means of adding or removing items from a shared data store
        //   A Take action will block a task if there are no items to be taken
        //   A limit can be set, and attempts to add beyond the limit will be blocked
        public static void DemoBlockingCollection()
        {
            // BlockingCollection uses ConcurrentQueue by default
            BlockingCollection<int> data = new BlockingCollection<int>(new ConcurrentStack<int>(), 5);

            Task.Run(() =>
            {
                // will block after the 5th
                for (int i = 0; i < 11; i++)
                {
                    data.Add(i);
                    Console.WriteLine("Data {0} added successfully.", i);
                }
                // indicate we have no more data
                data.CompleteAdding();
            });

            Console.ReadKey();
            Console.WriteLine("Reading Collection");

            Task.Run(() =>
            {
                // IsCompleted returns true when the collection is empty and CompleteAdding has been called
                while (!data.IsCompleted)
                {
                    // the Take method can throw an exception if the following sequence occurs:
                    // 1) The taking task checks the IsCompleted flag and finds it to be false
                    // 2) The adding task(which is running concurrently with the taking task) then calls the CompleteAdding method
                    // 3) Now the Take operation fails because the collection has been marked complete
                    try
                    {
                        int v = data.Take();
                        Console.WriteLine("Data {0} taken successfully.", v);
                    }
                    catch(InvalidOperationException ex)
                    {

                    }
                }
            });
        }
        //  
        // ConcurrentQueue<T>
        //  Enqueue adds items to the queue, guaranteed to work, queues can be infinite length
        //  TryDequeue removes items, will return false if it fails
        //  TryPeek, allows to inspect the the element at the start of the queue
        //   it can still fail if another task removes the item after TryPeek confirms it is there

        // It's possible for a task to enumerate a concurrent queue
        // a task can use a foreach and at the start of the enumeration 
        //  a concurrent queue will provide a snapshot of the queue contents
        public static void DemoConcurrentQueue()
        {
            ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            queue.Enqueue("Chad");
            queue.Enqueue("Welch");
            string str;
            if (queue.TryPeek(out str))
            {
                Console.WriteLine("Peek: {0}", str);

            }
            if (queue.TryDequeue(out str))
            {
                Console.WriteLine("Dequeue: {0}", str);
            }

            
            // ConcurrentBag<T>
            // ConcurrentDictionary<TKey, TValue>

        }

        // ConcurrentStack<T>
        // Push adds items onto the stack
        // TryPop removes items
        // PushRange and TryPopRange
        public static void DemoConcurrentStack()
        {
            ConcurrentStack<string> queue = new ConcurrentStack<string>();
            queue.Push("Chad");
            queue.Push("Welch");
            string str;
            if (queue.TryPeek(out str))
            {
                Console.WriteLine("Peek: {0}", str);

            }
            if (queue.TryPop(out str))
            {
                Console.WriteLine("Dequeue: {0}", str);
            }
        }
    }
}

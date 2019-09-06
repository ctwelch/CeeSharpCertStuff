using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CertificationTraining
{
    public static class Parallelism
    {
        public static void Demonstrate_Parallel_Invoke()
        {
            Parallel.Invoke(() => Task1(), () => Task2());
        }

        public static void Task1()
        {
            Console.WriteLine("Task 1 starting");
            Thread.Sleep(2000);
            Console.WriteLine("Task 1 ending");
        }

        public static void Task2()
        {
            Console.WriteLine("Task 2 starting");
            Thread.Sleep(2000);
            Console.WriteLine("Task 2 ending");
        }

        public static void Demonstrate_ForEach ()
        {
            var items = Enumerable.Range(0, 500);
            Parallel.ForEach(items, item =>
            {
                WorkOnItem(item);
            });

            Console.WriteLine("Finished processing. Press any key to end.");
            Console.ReadKey();
        }

        public static void Demonstrate_For ()
        {
            var items = Enumerable.Range(0, 500).ToArray();
            Parallel.For(0, items.Length, i =>
            {
                WorkOnItem(items[i]);
            });

            Console.WriteLine("Finished processing. Press any key to end.");
            Console.ReadKey();
        }

        public static void StoppingAndBreaking ()
        {
            var items = Enumerable.Range(0, 500).ToArray();

            ParallelLoopResult result = 
                Parallel.For(0, items.Count(), (int i, ParallelLoopState loopState) =>
                {
                    // there is no guarantee that the running of the loop where i == 200
                    // will come before code that is already running with i == 201
                    // Stop() will prevent any new iterations running with i greater than 200
                    if(i == 200)
                    {
                        loopState.Stop();

                        // with Break() all iterations with an index lower than 200 
                        // are guaranteed to be completed before the loop is ended
                        
                        loopState.Break();
                    }

                    WorkOnItem(items[i]);
                });

            Console.WriteLine("Completed: " + result.IsCompleted);
            Console.WriteLine("Items: " + result.LowestBreakIteration);

            Console.WriteLine("Finished processing. Press any key to end.");
            Console.ReadKey();
    }

        static void WorkOnItem(object item)
        {
            Console.WriteLine("Started working on : " + item);
            Thread.Sleep(100);
            Console.WriteLine("Finished working on : " + item);
        }

        class Person
        {
            public string Name { get; set; }
            public string City { get; set; }
        }

        private static Person[] people = new Person[]
            {
                new Person { Name="Chad", City = "Clearwater" },
                new Person { Name="Roger", City = "LangleyFalls" },
                new Person { Name="Klause", City = "LangleyFalls" },
                new Person { Name="Cerci", City = "KingsLanding" },
                new Person { Name="TheMountain", City = "KingsLanding" },
                new Person { Name="Rick", City = "Earth 1-C238" },
                new Person { Name="Morty", City = "Earth 1-C238" },
                new Person { Name="Matt", City = "Columbus" },
                new Person { Name="Kashka", City = "SanDiego" },
                new Person { Name="Ben", City = "Houston" },
            };

        public static void P_LINQ()
        {
            var result =
                (
                from person in people.AsParallel()
                //.AsOrdered()
                .WithDegreeOfParallelism(4)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                //.AsOrdered()  //  Returns a sorted list
                //where person.City == "LangleyFalls" 
                select person
                // AsSequential actually executes the query in order
                )
                .AsSequential()
                .Take(4)
                ; 

            foreach (var person in result)
            {
                Console.WriteLine(person.Name);
            }

            Console.WriteLine("Finished processing. Press any key to end.");
            Console.ReadKey();
        }

        public static void PLINQ_ForAll()
        {
            var result = from person in people.AsParallel()
                         where person.City == "Earth 1-C238"
                         select person;
            result.ForAll(person => Console.WriteLine(person.Name));
        }
    }    
}

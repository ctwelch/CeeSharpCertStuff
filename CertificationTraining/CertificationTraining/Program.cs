using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificationTraining
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parallelism.P_LINQ();
            //Tasks.ReturnValueFromTask();
            //ThreadsAndThreadPool.DemoThreadHello();
            //AsyncAndAwait.DemoConcurrentStack();
            //ManageMultithreading.DemoCancellationToken();
            EventsAndCallbacks.DemoActionDelegate();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}

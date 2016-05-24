using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morsky.Nsudotnet.Schedule
{
    class MyJob1 : IJob
    {
        public void Execute(object argument)
        {
            System.Console.WriteLine("First!");
        }
    }

    class MyJob2 : IJob
    {
        public void Execute(object argument)
        {
            System.Console.WriteLine("Second!");
        }
    }

    class MyJob3 : IJob
    {
        public void Execute(object argument)
        {
            System.Console.WriteLine("Third!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Schedule scheldure = new Schedule();
            scheldure.ScheduleDelayedJob(new MyJob1(), TimeSpan.FromMilliseconds(2000), null);
            scheldure.SchedulePeriodicJob(new MyJob2(), TimeSpan.FromMilliseconds(3000), null);
            scheldure.SchedulePeriodicJob(new MyJob3(), TimeSpan.FromMilliseconds(1000), null);
            Console.ReadKey();
        }
    }
}

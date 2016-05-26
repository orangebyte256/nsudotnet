    using System;

namespace Morsky.Nsudotnet.TaskScheduler
{
    internal class MyJob1 : IJob
    {
        public void Execute(object argument)
        {
            Console.WriteLine("First!");
        }
    }

    internal class MyJob2 : IJob
    {
        public void Execute(object argument)
        {
            Console.WriteLine("Second!");
        }
    }

    internal class MyJob3 : IJob
    {
        public void Execute(object argument)
        {
            Console.WriteLine("Third!");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var scheldure = new Schedule();
            scheldure.ScheduleDelayedJob(new MyJob1(), TimeSpan.FromMilliseconds(2000), null);
            scheldure.SchedulePeriodicJob(new MyJob2(), TimeSpan.FromMilliseconds(3000), null);
            scheldure.SchedulePeriodicJob(new MyJob3(), TimeSpan.FromMilliseconds(1000), null);
            Console.ReadKey();
        }
    }
}

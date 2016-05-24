using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Morsky.Nsudotnet.Schedule
{
    interface IJob
    {
        void Execute(object argument);
    }

    class Job
    {
       
        IJob job;
        TimeSpan restTime;
        bool isPeriodic = false;
        protected TimeSpan origTime;
        object arg;
        public object getArg()
        {
            return arg;
        }
        public IJob getJob()
        {
            return job;
        }
        public TimeSpan getRestTime()
        {
            return restTime;
        }
        public Job(IJob job, TimeSpan origTime, bool isPeriodic, object arg)
        {
            this.job = job;
            this.restTime = origTime;
            this.isPeriodic = isPeriodic;
            this.origTime = origTime;
            this.arg = arg;
        }
        public static void DoJob(object data)
        {
            Tuple<IJob, object> parse = (Tuple<IJob, object>)data;
            parse.Item1.Execute(parse.Item2);
        }
        public bool needExecute()
        {
            if (restTime.Milliseconds <= 0)
            {
                return true;
            }
            return false;
        }
        public bool needKill()
        {
            if (!needExecute())
                return false;
            if (!isPeriodic)
                return true;
            return false;
        }
        public void leftTime(TimeSpan time)
        {
            restTime -= time;
        }
        public void update()
        {
            restTime = origTime;
        }
    }

    class Schedule
    {
        List<Job> jobs = new List<Job>();
        TimeSpan current = DateTime.Now.TimeOfDay;
        System.Timers.Timer myTimer = new System.Timers.Timer();
        public Schedule()
        {
             myTimer.Enabled = true;
             myTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.resetTimer);
             myTimer.Stop();
        }
        void update()
        {
            TimeSpan newTime = DateTime.Now.TimeOfDay;
            for (int i = 0; i < jobs.Count(); i++ )
            {
                Job cur = jobs[i];
                cur.leftTime(newTime - current);
                if (cur.needExecute())
                {
                    Thread newThread = new Thread(Job.DoJob);
                    newThread.Start(new Tuple<IJob, object>(cur.getJob(), cur.getArg()));
                    if (cur.needKill())
                    {
                        jobs.Remove(cur);
                        i--;
                    }
                    else
                    {
                        cur.update();
                    }
                }
            }
            current = newTime;
        }
        
        void resetTimer(object sender, System.EventArgs e)
        {
            update();
            if (jobs.Count() > 0)
            {
                TimeSpan min = jobs[0].getRestTime();
                foreach (Job cur in jobs)
                {
                    if (min > cur.getRestTime())
                    {
                        min = cur.getRestTime();
                    }
                }
                myTimer.Start();
                myTimer.Interval = min.TotalMilliseconds;
            }
            else
            {
                myTimer.Stop();
            }
        }
        public void ScheduleDelayedJob(IJob job, TimeSpan delay, object arg)
        {
            Job res = new Job(job, delay, false, arg);
            jobs.Add(res);
            resetTimer(this, null);
        }
        public void SchedulePeriodicJob(IJob job, TimeSpan period, object arg)
        {
            Job res = new Job(job, period, true, arg);
            jobs.Add(res);
            resetTimer(this, null);
        }
    }
}

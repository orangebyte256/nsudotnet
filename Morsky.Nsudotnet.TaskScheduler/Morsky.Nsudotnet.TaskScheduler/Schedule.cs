using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Morsky.Nsudotnet.TaskScheduler
{
    public interface IJob
    {
        void Execute(object argument);
    }

    internal class Job
    {
        readonly IJob _job;
        private TimeSpan _restTime;
        private readonly bool _isPeriodic;
        private readonly TimeSpan _origTime;
        readonly object _arg;
        public object GetArg()
        {
            return _arg;
        }
        public IJob GetJob()
        {
            return _job;
        }
        public TimeSpan GetRestTime()
        {
            return _restTime;
        }
        public Job(IJob job, TimeSpan origTime, bool isPeriodic, object arg)
        {
            _job = job;
            _restTime = origTime;
            _isPeriodic = isPeriodic;
            _origTime = origTime;
            _arg = arg;
        }
        public static void DoJob(object data)
        {
            Tuple<IJob, object> parse = (Tuple<IJob, object>)data;
            parse.Item1.Execute(parse.Item2);
        }
        public bool NeedExecute()
        {
            if (_restTime.TotalMilliseconds <= 0)
            {
                return true;
            }
            return false;
        }
        public bool NeedKill()
        {
            if (!NeedExecute())
                return false;
            if (!_isPeriodic)
                return true;
            return false;
        }
        public void LeftTime(TimeSpan time)
        {
            _restTime -= time;
        }
        public void Update()
        {
            _restTime = _origTime;
        }
    }

    public class Schedule
    {
        readonly List<Job> _jobs = new List<Job>();
        TimeSpan _current = DateTime.Now.TimeOfDay;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        public Schedule()
        {
            _timer.Enabled = true;
            _timer.Elapsed += ResetTimer;
            _timer.Stop();
        }
        private void Update()
        {
            TimeSpan newTime = DateTime.Now.TimeOfDay;
            for (int i = 0; i < _jobs.Count(); i++)
            {
                Job cur = _jobs[i];
                cur.LeftTime(newTime - _current);
                if (cur.NeedExecute())
                {
                    Thread newThread = new Thread(Job.DoJob);
                    newThread.Start(new Tuple<IJob, object>(cur.GetJob(), cur.GetArg()));
                    if (cur.NeedKill())
                    {
                        _jobs.Remove(cur);
                        i--;
                    }
                    else
                    {
                        cur.Update();
                    }
                }
            }
            _current = newTime;
        }

        private void ResetTimer(object sender, EventArgs e)
        {
            Update();
            if (_jobs.Any())
            {
                var min = _jobs[0].GetRestTime();
                foreach (var cur in _jobs)
                {
                    if (min > cur.GetRestTime())
                    {
                        min = cur.GetRestTime();
                    }
                }
                _timer.Start();
                _timer.Interval = min.TotalMilliseconds;
            }
            else
            {
                _timer.Stop();
            }
        }

        private void AddJob(Job job)
        {
            _jobs.Add(job);
            ResetTimer(this, null);
        }

        public void ScheduleDelayedJob(IJob job, TimeSpan delay, object arg)
        {
            var res = new Job(job, delay, false, arg);
            AddJob(res);
        }
        public void SchedulePeriodicJob(IJob job, TimeSpan period, object arg)
        {
            var res = new Job(job, period, true, arg);
            AddJob(res);
        }

        public void SchedulePeriodicJob(IJob job, string cronExpression, object arg)
        {
            var res = new Job(job, TimeSpan.Parse(cronExpression), true, arg);
            AddJob(res);
        }
    }
}

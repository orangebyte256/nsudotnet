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

    internal class Task
    {
        public object Arg
        {
            get
            {
                return _arg;
            }
            private set
            {
                _arg = value;
            }
        }
        public IJob Job
        {
            get
            {
                return _job;
            }
            private set
            {
                _job = value;
            }
        }
        public TimeSpan RestTime
        {
            get
            {
                return _restTime;
            }
            private set
            {
                _restTime = value;
            }
        }

        public Task(IJob job, TimeSpan origTime, bool isPeriodic, object arg)
        {
            Job = job;
            RestTime = origTime;
            Arg = arg;
            _isPeriodic = isPeriodic;
            _origTime = origTime;
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


        private object _arg;
        private IJob _job;
        private TimeSpan _restTime;

        private bool _isPeriodic;
        private TimeSpan _origTime;
    }

    public class Schedule
    {
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
                Task cur = _jobs[i];
                cur.LeftTime(newTime - _current);
                if (cur.NeedExecute())
                {
                    Thread newThread = new Thread(Task.DoJob);
                    newThread.Start(new Tuple<IJob, object>(cur.Job, cur.Arg));
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
                var min = _jobs.Min(x => x.RestTime); 
                _timer.Start();
                _timer.Interval = min.TotalMilliseconds;
            }
            else
            {
                _timer.Stop();
            }
        }

        private void AddJob(Task job)
        {
            _jobs.Add(job);
            ResetTimer(this, null);
        }

        public void ScheduleDelayedJob(IJob job, TimeSpan delay, object arg)
        {
            var res = new Task(job, delay, false, arg);
            AddJob(res);
        }
        public void SchedulePeriodicJob(IJob job, TimeSpan period, object arg)
        {
            var res = new Task(job, period, true, arg);
            AddJob(res);
        }

        public void SchedulePeriodicJob(IJob job, string cronExpression, object arg)
        {
            var res = new Task(job, TimeSpan.Parse(cronExpression), true, arg);
            AddJob(res);
        }

        readonly List<Task> _jobs = new List<Task>();
        private TimeSpan _current = DateTime.Now.TimeOfDay;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
    }
}

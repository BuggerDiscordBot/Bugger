using System.Collections.Generic;
using System.Timers;

namespace Bugger.Features.RepeatedTasks
{
    internal class RepeatedTaskHandler
    {
        internal Dictionary<string, Timer> Timers;

        internal RepeatedTaskHandler()
        {
            Timers = new Dictionary<string, Timer>();
        }

        public bool AddRepeatedTask(string name, int interval, ElapsedEventHandler task)
        {
            if (Timers.ContainsKey(name.ToLower())) return false;
            var timer = new Timer
            {
                Interval = interval,
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += task;
            Timers.Add(name.ToLower(), timer);
            return true;
        }

        public bool ChangeInterval(string name, int interval)
        {
            if (!Timers.ContainsKey(name)) return false;
            if (interval < Constants.MinTimerIntervall) return false;
            Timers[name].Interval = interval;
            return true;
        }

        public bool StartTimer(string name)
        {
            if (!Timers.ContainsKey(name)) return false;
            Timers[name].Start();
            return true;
        }

        public bool StopTimer(string name)
        {
            if (!Timers.ContainsKey(name)) return false;
            Timers[name].Stop();
            return true;
        }
    }
}

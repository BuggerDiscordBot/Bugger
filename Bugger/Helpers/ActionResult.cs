using System;
using System.Collections.Generic;

namespace Bugger.Helpers
{
    public class ActionResult
    {
        public bool Success { get; private set; }
        public List<Alert> Alerts { get; }

        public ActionResult()
        {
            Success = true;
            Alerts = new List<Alert>();
        }

        /// <summary>
        /// Merge the alerts of a Actionresult into a ActionResult and adds the Alerts and sets the Success
        /// </summary>
        /// <param name="r"></param>
        public void Merge(ActionResult r)
        {
            if (r != null)
            {
                foreach (var error in r.Alerts)
                {
                    Alerts.Add(error);
                }
                if (!r.Success)
                {
                    Success = false;
                }
            }
        }

        /// <summary>
        /// Adds a Alert into the Alers list
        /// </summary>
        /// <param name="alert"></param>
        public void AddAlert(Alert alert)
        {
            switch (alert.Level)
            {
                case LevelEnum.Success:
                    break;
                case LevelEnum.Info:
                    break;
                case LevelEnum.Error:
                case LevelEnum.Exception:
                    Success = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Alerts.Add(alert);
        }
    }

    public class ActionResult<T> : ActionResult
    {
        public T Value { get; set; }
    }

    public class Alert
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public LevelEnum Level { get; set; }

        public Alert(string name, string description, LevelEnum level)
        {
            Name = name;
            Description = description;
            Level = level;
        }
    }

    public enum LevelEnum
    {
        Success = 0,
        Info = 10,
        Error = 20,
        Exception = 30
    }
}

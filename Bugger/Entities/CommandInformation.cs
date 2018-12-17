using System;

namespace Bugger.Entities
{
    public class CommandInformation
    {
        public string Command { get; set; }
        public DateTime UsageDate { get; set; } = DateTime.Now;

        public CommandInformation()
        {
        }

        public CommandInformation(string command, DateTime usageDate)
        {
            Command = command;
            UsageDate = usageDate;
        }
    }
}

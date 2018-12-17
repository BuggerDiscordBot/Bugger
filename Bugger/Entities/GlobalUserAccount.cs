using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bugger.Entities
{
    public class GlobalUserAccount : IGlobalAccount
    {
        public GlobalUserAccount(ulong id)
        {
            Id = id;
        }

        public ulong Id { get; }

        public ulong Miunies { get; set; } = 1;

        public DateTime LastDaily { get; set; } = DateTime.UtcNow.AddDays(-2);

        public DateTime LastMessage { get; set; } = DateTime.UtcNow;

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public List<ReminderEntry> Reminders { get; internal set; } = new List<ReminderEntry>();

        private List<CommandInformation> _commandHistory { get; } = new List<CommandInformation>();

        public ReadOnlyCollection<CommandInformation> CommandHistory { get; private set; }

        public void AddCommandToHistory(CommandInformation commandInformation)
        {
            _commandHistory.Add(commandInformation);
            if (_commandHistory.Count > Constants.MaxCommandHistoryCapacity)
            {
                _commandHistory.RemoveAt(0); //remove the first element, ensures the list always got 5 elements maximum
            }
            
            CommandHistory = new ReadOnlyCollection<CommandInformation>(_commandHistory);
        }
        
        public string TimeZone { get; set; } // Please note, TimeZone ID works for LINUX, but for windows we need TimeZone NAME
        /* Add more values to store */

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals(obj as IGlobalAccount);
        }

        // implementation for IEquatable
        public bool Equals(IGlobalAccount other)
        {
            return Id == other.Id;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return unchecked((int)Id);
        }
    }

    public struct ReminderEntry
    {
        public DateTime DueDate;
        public string Description;

        public ReminderEntry(DateTime dueDate, string description)
        {
            DueDate = dueDate;
            Description = description;
        }
    }
}

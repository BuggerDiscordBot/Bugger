using System;
using System.Collections.Generic;
using Bugger.Features.GlobalAccounts;
using Bugger.Features.RoleAssignment;
using Discord;
using static Bugger.Modules.ServerBots;

namespace Bugger.Entities
{
    public class GlobalGuildAccount : IGlobalAccount
    {
        public GlobalGuildAccount(ulong id)
        {
            Id = id;
        }
        public ulong Id { get; }

        public ulong AnnouncementChannelId { get; private set; }

        public IReadOnlyList<string> Prefixes { get; private set; } = new List<string>();

        public IReadOnlyList<string> WelcomeMessages { get; private set; } = new List<string>();

        public IReadOnlyList<string> LeaveMessages { get; private set; } = new List<string>();

        public Dictionary<string, string> Tags { get; private set; } = new Dictionary<string, string>();

        public Modules.ServerBots.GuildData BotData { get; private set; }

        public RoleByPhraseSettings RoleByPhraseSettings { get; private set; } = new RoleByPhraseSettings();

        public int ServerActivityLog { get; set; }

        public ulong LogChannelId { get; set; }

        public string RoleOnJoin { get; set; }

        /* Add more values to store */
        
        public GlobalGuildAccount Modify(Action<GuildAccountSettings> func)
        {
            var settings = new GuildAccountSettings();
            func(settings);

            if (settings.AnnouncementChannelId.IsSpecified)
                AnnouncementChannelId = settings.AnnouncementChannelId.Value;
            if (settings.Prefixes.IsSpecified)
                Prefixes = settings.Prefixes.Value;
            if (settings.WelcomeMessages.IsSpecified)
                WelcomeMessages = settings.WelcomeMessages.Value;
            if (settings.LeaveMessages.IsSpecified)
                LeaveMessages = settings.LeaveMessages.Value;
            if (settings.Tags.IsSpecified)
                Tags = settings.Tags.Value;
            if (settings.BotData.IsSpecified)
                BotData = settings.BotData.Value;
            if (settings.RoleByPhraseSettings.IsSpecified)
                RoleByPhraseSettings = settings.RoleByPhraseSettings.Value;
            GlobalGuildAccounts.SaveAccounts(Id);
            return this;
        }
        
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
    public class GuildAccountSettings
    {
        public Optional<ulong> AnnouncementChannelId { get; private set; }
        public GuildAccountSettings SetAnnouncementChannelId(ulong id) { AnnouncementChannelId = id; return this; }

        public Optional<List<string>> Prefixes { get; private set; }
        public GuildAccountSettings SetPrefixes(List<string> prefixes) { Prefixes = prefixes; return this; }

        public Optional<List<string>> WelcomeMessages { get; private set; }
        public GuildAccountSettings SetWelcomeMessages(List<string> welcomeMessages) { WelcomeMessages = welcomeMessages; return this; }

        public Optional<List<string>> LeaveMessages { get; private set; }
        public GuildAccountSettings SetLeaveMessages(List<string> leaveMessages) { LeaveMessages = leaveMessages; return this; }

        public Optional<Dictionary<string, string>> Tags { get; private set; }
        public GuildAccountSettings SetTags(Dictionary<string, string> tags) { Tags = tags; return this; }

        public Optional<GuildData> BotData { get; private set; }
        public GuildAccountSettings SetBotData(GuildData botData) { BotData = botData; return this; }

        public Optional<RoleByPhraseSettings> RoleByPhraseSettings { get; private set; }
        public GuildAccountSettings SetBotData(RoleByPhraseSettings roleByPhraseSettings) { RoleByPhraseSettings = roleByPhraseSettings; return this; }
    }
}

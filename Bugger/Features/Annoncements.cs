using System.Threading.Tasks;
using Bugger.Features.GlobalAccounts;
using Discord.WebSocket;

namespace Bugger.Features
{
    public static class Announcements
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            var dmChannel = await user.GetOrCreateDMChannelAsync();
            var possibleMessages = GlobalGuildAccounts.GetGuildAccount(user.Guild.Id).WelcomeMessages;
            var messageString = possibleMessages[Global.Rng.Next(possibleMessages.Count)];
            messageString = messageString.ReplacePlacehoderStrings(user);
            if (string.IsNullOrEmpty(messageString)) return;
            await dmChannel.SendMessageAsync(messageString);
        }

        public static async Task UserLeft(SocketGuildUser user, DiscordSocketClient client)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(user.Guild.Id);
            if (guildAcc.AnnouncementChannelId == 0) return;
            if (!(client.GetChannel(guildAcc.AnnouncementChannelId) is SocketTextChannel channel)) return;
            var possibleMessages = guildAcc.LeaveMessages;
            var messageString = possibleMessages[Global.Rng.Next(possibleMessages.Count)];
            messageString = messageString.ReplacePlacehoderStrings(user);
            if (string.IsNullOrEmpty(messageString)) return;
            await channel.SendMessageAsync(messageString);
        }
    }
}

using Bugger.Entities;
using Bugger.Features.GlobalAccounts;
using Discord.Commands;
using Discord.WebSocket;

namespace Bugger.Extensions
{
    public class MiunieCommandContext : SocketCommandContext
    {
        public GlobalUserAccount UserAccount { get; }
        
        public MiunieCommandContext(DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
        {
            if (User is null) { return; }

            UserAccount = GlobalUserAccounts.GetUserAccount(User);
        }

        public void RegisterCommandUsage()
        {
            var commandUsedInformation = new CommandInformation(Message.Content, Message.CreatedAt.DateTime);
            
            UserAccount.AddCommandToHistory(commandUsedInformation);

            GlobalUserAccounts.SaveAccounts(UserAccount.Id);
        }
    }
}

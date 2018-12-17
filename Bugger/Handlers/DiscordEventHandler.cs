using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bugger.Configuration;
using Bugger.Features;
using Bugger.Features.Lists;
using Bugger.Features.Onboarding;
using Bugger.Features.Trivia;
using Bugger.Helpers;
using Bugger.Modules;
using Discord;
using Discord.WebSocket;

namespace Bugger.Handlers
{
    /// <summary>
    /// Put your subscriptions to events here!
    /// Just one non awaited async Method per functionality you want to provide 
    /// </summary>
    #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    #pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
    public class DiscordEventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ApplicationSettings _applicationSettings;
        private readonly Logger _logger;
        private readonly TriviaGames _triviaGames;
        private readonly ListManager _listManager;
        private readonly IOnboarding _onboarding;

        public DiscordEventHandler(Logger logger, TriviaGames triviaGames, DiscordSocketClient client, CommandHandler commandHandler, ApplicationSettings applicationSettings, ListManager listManager, IOnboarding onboarding)
        {
            _logger = logger;
            _client = client;
            _commandHandler = commandHandler;
            _applicationSettings = applicationSettings;
            _triviaGames = triviaGames;
            _listManager = listManager;
            _onboarding = onboarding;
        }

        public void InitDiscordEvents()
        {
            _client.ChannelCreated += ChannelCreated;
            _client.ChannelDestroyed += ChannelDestroyed;
            _client.ChannelUpdated += ChannelUpdated;
            _client.Connected += Connected;
            _client.CurrentUserUpdated += CurrentUserUpdated;
            _client.Disconnected += Disconnected;
            _client.GuildAvailable += GuildAvailable;
            _client.GuildMembersDownloaded += GuildMembersDownloaded;
            _client.GuildMemberUpdated += GuildMemberUpdated;
            _client.GuildUnavailable += GuildUnavailable;
            _client.GuildUpdated += GuildUpdated;
            _client.JoinedGuild += JoinedGuild;
            _client.LatencyUpdated += LatencyUpdated;
            _client.LeftGuild += LeftGuild;
            _client.Log += Log;
            _client.LoggedIn += LoggedIn;
            _client.LoggedOut += LoggedOut;
            _client.MessageDeleted += MessageDeleted;
            _client.MessageReceived += MessageReceived;
            _client.MessageUpdated += MessageUpdated;
            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
            _client.ReactionsCleared += ReactionsCleared;
            _client.Ready += Ready;
            _client.RecipientAdded += RecipientAdded;
            _client.RecipientRemoved += RecipientRemoved;
            _client.RoleCreated += RoleCreated;
            _client.RoleDeleted += RoleDeleted;
            _client.RoleUpdated += RoleUpdated;
            _client.UserBanned += UserBanned;
            _client.UserIsTyping += UserIsTyping;
            _client.UserJoined += UserJoined;
            _client.UserLeft += UserLeft;
            _client.UserUnbanned += UserUnbanned;
            _client.UserUpdated += UserUpdated;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            // THIS ONE IS AN EXCEPTION!
            // I don't know how we should handle contidional 
            // subscription to an event otherwise...
            if (!Global.Headless)
            {
                _client.Log += _logger.Log;
            }
        }

        private async Task ChannelCreated(SocketChannel channel)
        {
            
        }

        private async Task ChannelDestroyed(SocketChannel channel)
        {
            
        }

        private async Task ChannelUpdated(SocketChannel channelBefore, SocketChannel channelAfter)
        {
            
        }

        private async Task Connected()
        {
            
        }

        private async Task CurrentUserUpdated(SocketSelfUser userBefore, SocketSelfUser userAfter)
        {
            
        }

        private async Task Disconnected(Exception exception)
        {
            
        }

        private async Task GuildAvailable(SocketGuild guild)
        {
            
        }

        private async Task GuildMembersDownloaded(SocketGuild guild)
        {
            
        }

        private async Task GuildMemberUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            
        }

        private async Task GuildUnavailable(SocketGuild guild)
        {
            
        }

        private async Task GuildUpdated(SocketGuild guildBefore, SocketGuild guildAfter)
        {
            
        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            _onboarding.JoinedGuild(guild);
            ServerBots.JoinedGuild(guild);
        }

        private async Task LatencyUpdated(int latencyBefore, int latencyAfter)
        {
            
        }

        private async Task LeftGuild(SocketGuild guild)
        {
            
        }

        private async Task Log(LogMessage logMessage)
        {
            
        }

        private async Task LoggedIn()
        {
            
        }

        private async Task LoggedOut()
        {
            
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> cacheMessage, ISocketMessageChannel channel)
        {
            
        }

        private async Task MessageReceived(SocketMessage message)
        {
            _commandHandler.HandleCommandAsync(message);
            MessageRewardHandler.HandleMessageRewards(message);
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> cacheMessageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cacheMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) { return; }
            
            var user = _client.Guilds.First().GetUser(reaction.UserId);
            var roleIds = user.Roles.Select(r => r.Id).ToArray();
            (new ListReactionHandler()).HandleReactionAdded(new ListHelper.UserInfo(user.Id, roleIds), _listManager, cacheMessage, reaction);

            _triviaGames.HandleReactionAdded(cacheMessage, reaction);
            BlogHandler.ReactionAdded(reaction);
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cacheMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            
        }

        private async Task ReactionsCleared(Cacheable<IUserMessage, ulong> cacheMessage, ISocketMessageChannel channel)
        {
            
        }

        private async Task Ready()
        {
            RepeatedTaskFunctions.InitRepeatedTasks();
            ServerBots.Init();
         
        }

        private async Task RecipientAdded(SocketGroupUser user)
        {
            
        }

        private async Task RecipientRemoved(SocketGroupUser user)
        {
            
        }

        private async Task RoleCreated(SocketRole role)
        {
            
        }

        private async Task RoleDeleted(SocketRole role)
        {
            
        }

        private async Task RoleUpdated(SocketRole roleBefore, SocketRole roleAfter)
        {
            
        }

        private async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            
        }

        private async Task UserIsTyping(SocketUser user, ISocketMessageChannel channel)
        {
            
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            Announcements.UserJoined(user);
            
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            Announcements.UserLeft(user, _client);
        }

        private async Task UserUnbanned(SocketUser user, SocketGuild guild)
        {
            
        }

        private async Task UserUpdated(SocketUser user, SocketUser guild)
        {
            
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState voiceStateBefore, SocketVoiceState voiceStateAfter)
        {
            
        }
    }
}

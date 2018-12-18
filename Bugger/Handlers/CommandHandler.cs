using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bugger.Extensions;
using Discord.Commands;
using Discord.WebSocket;
using Bugger.Features.GlobalAccounts;
using Bugger.Providers;

namespace Bugger.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandler(DiscordSocketClient client, CommandService cmdService, IServiceProvider serviceProvider)
        {
            _client = client;
            _cmdService = cmdService;
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            Global.Client = _client;
        }

        public async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) { return; }
            if (msg.Channel is SocketDMChannel) { return; }
            if (msg.Author.IsBot) { return; }
            var context = new MiunieCommandContext(_client, msg);

            await RoleByPhraseProvider.EvaluateMessage(
                context.Guild,
                context.Message.Content,
                (SocketGuildUser) context.User
            );

            var argPos = 0;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || CheckPrefix(ref argPos, context))
            {
                var cmdSearchResult = _cmdService.Search(context, argPos);
                if (!cmdSearchResult.IsSuccess)
                {
                    await context.Channel.SendMessageAsync("Nie czaję... :sweat_smile: Sprawdź składnię i upewnij się, że taka komenda napewno istnieje :kissing_heart::stuck_out_tongue_closed_eyes:");
                    return;
                }
                
                context.RegisterCommandUsage();

                context.Message.DeleteAsync();

                var executionTask = _cmdService.ExecuteAsync(context, argPos, _serviceProvider);
 

                #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                executionTask.ContinueWith(task =>
                {
                    if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;
                    const string errTemplate = ":warning: Błąd :warning: {0}**!**\nPowód: {1}.";
                    var errMessage = string.Format(errTemplate, context.User.Mention, task.Result.ErrorReason);
                    context.Channel.SendMessageAsync(errMessage);
                    Console.WriteLine(context.Message.Timestamp + " | " + context.Message.Author + " | " + context.Guild + " ==> "+ context.Channel + " | BŁĄD: " + task.Result.ErrorReason);
                    
                });
                #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private static bool CheckPrefix(ref int argPos, SocketCommandContext context)
        {
            if (context.Guild is null) return false;
            var prefixes = GlobalGuildAccounts.GetGuildAccount(context.Guild.Id).Prefixes;
            var tmpArgPos = 0;
            var success = prefixes.Any(pre =>
            {
                if (!context.Message.Content.StartsWith(pre)) return false;
                tmpArgPos = pre.Length;
                return true;
            });
            argPos = tmpArgPos;
            return success;
        }
    }
}

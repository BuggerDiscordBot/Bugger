using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;
using Discord;
using Discord.Commands;

namespace Bugger.Modules
{
    public class UstawieniaSerwera : ModuleBase<MiunieCommandContext>

    {

        [Command("Logi")]
        [Remarks("Włączę/wyłączę czat z moimi logami serwerowymi.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetServerActivivtyLogOff()
        {
            var guild = GlobalGuildAccounts.GetGuildAccount(Context.Guild);
            guild.LogChannelId = 0;
            guild.ServerActivityLog = 0;
            GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);

            await ReplyAsync("Koniec loggowania");

        }

        /// <summary>
        /// by saying "SetLog" it will create a   channel itself, you may move and rname it
        /// by saying "SetLog ID" it will set channel "ID" as Logging Channel
        /// by saying "SetLog" again, it will turn off Logging, but will not delete it from the file
        /// </summary>
        /// <param name="KanałLogów"></param>
        /// <returns></returns>
        [Command("UstawLog")]
        [Alias("SetLogs")]
        [Remarks("Ustawienia logów serwerowych (opcjonalne, wersja beta).")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetServerActivivtyLog(ulong KanałLogów = 0)
        {
            var guild = GlobalGuildAccounts.GetGuildAccount(Context.Guild);

            if (KanałLogów != 0)
            {
                try
                {
                    var channel = Context.Guild.GetTextChannel(KanałLogów);
                    guild.LogChannelId = channel.Id;
                    guild.ServerActivityLog = 1;
                    GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);

                }
                catch
                {
//
                }

                return;
            }
            switch (guild.ServerActivityLog)
            {
                case 1:
                    guild.ServerActivityLog = 0;
                    guild.LogChannelId = 0;
                    GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);


                        await ReplyAsync("No more logging any activity now");

                    return;
                case 0:
                    try
                    {
                        try
                        {
                            var tryChannel = Context.Guild.GetTextChannel(guild.LogChannelId);
                            if (tryChannel.Name != null)
                            {
                                guild.LogChannelId = tryChannel.Id;
                                guild.ServerActivityLog = 1;
                                GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);

                                await ReplyAsync(
                                    $"Będę zapisywał wszystko na {tryChannel.Mention}, możesz go normalnie przesuwać albo zmieniać nazwę. :)");
                            }
                        }
                        catch
                        {

                            var channel = Context.Guild.CreateTextChannelAsync("OctoLogs");
                            guild.LogChannelId = channel.Result.Id;
                            guild.ServerActivityLog = 1;
                            GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);

                            await ReplyAsync(
                                $"Będę zapisywał wszystko na {channel.Result.Mention}, możesz go normalnie przesuwać albo zmieniać nazwę. :)");
                        }
                    }
                    catch
                    {
                     //ignored
                    }
                    break;
            }
        }



        [Command("RolaPoczątkowa")]
        [Alias("RoleOnJoin", "AutoRole", "Auto-Role", "Assignments", "SetRoleOnJoin"), Remarks("Będę przydzielał nowym użytkownikom daną rolę po dołączeniu do serwera.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetRoleOnJoin(string NazwaRoli = null)
        {

            string text;
            var guild = GlobalGuildAccounts.GetGuildAccount(Context.Guild);
            if (NazwaRoli == null)
            {
                guild.RoleOnJoin = null;
                text = $"Role początkowe nie będą przydzielane! :triumph:";
            }
            else
            {
                guild.RoleOnJoin = NazwaRoli;
                text = $"Wszyscy nowi będą dostawać rolę {NazwaRoli} odrazu na wstępie!";
            }

            GlobalGuildAccounts.SaveAccounts(Context.Guild.Id);
            await ReplyAsync(text);

        }
    }
}
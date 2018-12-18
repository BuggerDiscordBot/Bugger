using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Bugger.Extensions;
using Bugger.Handlers;
using Bugger.Preconditions;

namespace Bugger.Modules
{
    public class PodstawowaAdministracja : ModuleBase<MiunieCommandContext>
    {
        private static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);

        [Command("Czyść", RunMode = RunMode.Async), Alias("C", "Cz", "FBI", "fbi", "FBIOU", "FBIOPENUP", "Posprzątaj", "Wyczyść", "Usuń", "Oczyść", "Przeczyść", "Wymaż", "Purge", "Clean", "Clear", "Czysc", "Posprzataj", "Wyczysc", "Usun", "Oczysc", "Przeczysc", "Wymaz")]
        [Remarks("Usunę daną ilość wszystkich ostatnich wiadomości lub (jeśli sprecyzowano) ostanich wiadomości konkretnego użytkownika na tym czacie! :wastebasket: (domyślnie 255 wiad.)")]
        [RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Clean()
        {
            await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(await Context.Message.Channel.GetMessagesAsync().FlattenAsync());
        }

        [Command("Czyść", RunMode = RunMode.Async), Alias("C", "Cz", "Posprzątaj", "Wyczyść", "Usuń", "Oczyść", "Przeczyść", "Wymaż", "Purge", "Clean", "Clear", "Czysc", "Posprzataj", "Wyczysc",  "Usun", "Oczysc", "Przeczysc", "Wymaz")]
        [Remarks("Usunę daną ilość wszystkich ostatnich wiadomości lub (jeśli sprecyzowano) ostanich wiadomości konkretnego użytkownika na tym czacie! :wastebasket: (domyślnie 255 wiad.)")]
        [RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Clear(int IlośćWiadomościDoUsunięcia)
        {
            await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(await Context.Message.Channel.GetMessagesAsync(IlośćWiadomościDoUsunięcia+1).FlattenAsync());

            var embed = EmbedHandler.CreateEmbed("Wyczyściłem :recycle: " + IlośćWiadomościDoUsunięcia + ":recycle: ostatnich wiadomości na  " + Context.Channel + "!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Czyść"), Alias("C", "Cz", "Posprzątaj", "Wyczyść", "Usuń", "Oczyść", "Przeczyść", "Wymaż", "Purge", "Clean", "Clear", "Czysc", "Posprzataj", "Wyczysc", "Usun", "Oczysc", "Przeczysc", "Wymaz")]
        [Remarks("Usunę daną ilość ostatnich wiadomości konkretnego użytkownika na tym czacie! (Domyślnie 100 wiadomości)")]
        [RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Clear(SocketGuildUser Użytkownik, int IlośćWiadomościDoUsunięcia = 999)
        {

            if (Użytkownik == Context.User)
                IlośćWiadomościDoUsunięcia++;

            var messages = await Context.Message.Channel.GetMessagesAsync(IlośćWiadomościDoUsunięcia).FlattenAsync();
            var result = messages.Where(x => x.Author.Id == Użytkownik.Id && x.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)));
            await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(result);

            if (IlośćWiadomościDoUsunięcia == 999)
            {
                var embed = EmbedHandler.CreateEmbed("Wyczyściłem wszytkie wiadomości użytkownika :recycle: " + Użytkownik + " :recycle: z ostatnich dwóch tygodni na " + Context.Channel + "!", EmbedHandler.EmbedMessageType.Success);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var embed = EmbedHandler.CreateEmbed("Wyczyściłem :recycle: " + IlośćWiadomościDoUsunięcia + " :recycle: ostatnich wiadomości użytkownika " + Użytkownik + " na " + Context.Channel + "!", EmbedHandler.EmbedMessageType.Success);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("Wycisz"), Alias("W", "M", "Ucisz", "Tłumik", "Mute")]
        [Remarks("Wyciszę danego użytkownika! :mute:")]
        [RequireUserPermission(GuildPermission.MuteMembers), RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Mute(SocketGuildUser Użytkownik)
        {
            await Context.Guild.GetUser(Użytkownik.Id).ModifyAsync(x => x.Mute = true);

            var muteRole = await GetMuteRole(Użytkownik.Guild);
            if (!Użytkownik.Roles.Any(r => r.Id == muteRole.Id))
                await Użytkownik.AddRoleAsync(muteRole).ConfigureAwait(false);

            var embed = EmbedHandler.CreateEmbed("Wyciszyłem użytkownika :mute: " + Użytkownik + " :mute: na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Odcisz"), Alias("OC", "Unmute")]
        [Remarks("Odciszę danego użytkownika! :loud_sound:")]
        [RequireUserPermission(GuildPermission.MuteMembers), RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Unmute([NoSelf] SocketGuildUser Użytkownik)
        {
            await Context.Guild.GetUser(Użytkownik.Id).ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
            try { await Użytkownik.ModifyAsync(x => x.Mute = false).ConfigureAwait(false); } catch { }
            try { await Użytkownik.RemoveRoleAsync(await GetMuteRole(Użytkownik.Guild)).ConfigureAwait(false); } catch { }

            var embed = EmbedHandler.CreateEmbed("Odciszyłem użytkownika :loud_sound: " + Użytkownik + " :loud_sound: na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Wyrzuć"), Alias("Wy", "K", "Kik", "Kiknij", "Kicknij", "Wykop", "Kick", "Throw", "Wyrzuc")]
        [Remarks("Wyrzucę danego użytkownika! :boot::boom:")]
        [RequireUserPermission(GuildPermission.KickMembers), RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick([NoSelf][RequireBotHigherHirachy] SocketGuildUser Użytkownik)
        {
            await Użytkownik.KickAsync();

            var embed = EmbedHandler.CreateEmbed("Wyrzuciłem użytkownika :boot::boom: " + Użytkownik + " :boot::boom: z serwera!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Zbanuj"), Alias("B", "Ban", "Banicja", "Banuj")]
        [Remarks("Zbanuję danego użytkownika! :raised_back_of_hand:")]
        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban([NoSelf][RequireBotHigherHirachy] SocketGuildUser Użytkownik)
        {
            await Context.Guild.AddBanAsync(Użytkownik);

            var embed = EmbedHandler.CreateEmbed("Zbanowałem użytkownika :raised_back_of_hand: " + Użytkownik + " :raised_back_of_hand: na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        } 

        [Command("Odbanuj"), Alias("Łaska", "Un-Ban", "Unban")]
        [Remarks("Odbanuję danego użytkownika! :raised_hands:")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Unban([Remainder]string NazwaUżytkownika)
        {
            var bans = await Context.Guild.GetBansAsync();
            var theUser = bans.FirstOrDefault(x => x.User.ToString().ToLowerInvariant() == NazwaUżytkownika.ToLowerInvariant());
            await Context.Guild.RemoveBanAsync(theUser.User).ConfigureAwait(false);

            var embed = EmbedHandler.CreateEmbed("Odbanowałem :raised_hands: " + NazwaUżytkownika + " :raised_hands: na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Odbanuj"), Alias("OB", "Łaska", "Un-Ban", "Unban"), Remarks("Odbanuję danego użytkownika! :raised_hands:")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.BanMembers), RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Unban(ulong IdUżytkownika)
        {
            var bans = await Context.Guild.GetBansAsync();
            var theUser = bans.FirstOrDefault(x => x.User.Id == IdUżytkownika);
            await Context.Guild.RemoveBanAsync(theUser.User);

            var embed = EmbedHandler.CreateEmbed("Odbanowałem :raised_hands: " + IdUżytkownika + " :raised_hands: na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);

        }

        [Command("Ogłoś"), Alias("Og", "An", "Obwieszcz", "Głoś", "Ploklamuj", "Announcement", "Announce", "Oglos")]
        [Remarks("Stworzę ogłoszenie! :loudspeaker: Skł: ``<p>og <Tutuł> | <Treść>``")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Announce([Remainder]string TytułITreść)
        {
            string[] split = TytułITreść.Split(new string[] { " | " }, StringSplitOptions.None);
            string title = split.First();
            string content = split.Last();

            var embed = new EmbedBuilder();

            embed.WithTitle(title);
            embed.WithDescription(content);
            embed.WithFooter(Context.User.Username);
            embed.Footer.WithIconUrl(Context.User.GetAvatarUrl(ImageFormat.Auto));
            embed.WithCurrentTimestamp();
            embed.WithColor(0, 0, 200);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("Pseudonim"), Alias("P", "Ps", "N", "Pseudo", "Nick", "ZmieńNick", "UstawNick", "NowyNick", "ChangeNick", "SetNick", "NewNick")]
        [Remarks("Ustawię lub zmienię użytkownikowi pseudonim!")]
        [Cooldown(5)]
        [RequireUserPermission(Discord.GuildPermission.ManageNicknames)]
        public async Task Nickname(SocketGuildUser NazwaUżytkownika, [Remainder]string Pseudonim)
        {
            await Context.Guild.GetUser(NazwaUżytkownika.Id).ModifyAsync(x => x.Nickname = Pseudonim);

            var embed = EmbedHandler.CreateEmbed("Zmieniłem pseudonim użytkowinika " + NazwaUżytkownika + " na " + Pseudonim + " na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("StwórzCzat"), Alias("SCZ", "ZCZ", "KT", "MCH", "CCH", "Czat", "ZróbCzat", "StworzCzat", "NowyCzat", "ZrobCzat", "Chat", "MakeChat", "MakeCH", "MChat")]
        [Remarks("Stworzę nowy czat o danej nazwie!")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Text(string NazwaCzatu)
        {
            await Context.Guild.CreateTextChannelAsync(NazwaCzatu);

            var embed = EmbedHandler.CreateEmbed("Stworzyłem czat o nazwie " + NazwaCzatu + " na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("StwórzKG"), Alias("KG", "SKG", "ZKG", "MVCH", "CVCH", "VCH", "KanałGłosowy", "ZróbKG", "StworzKG", "ZrobKG", "MakeVChat", "MakeVoiceCH", "MakeVoiceChat", "MakeVCH")]
        [Remarks("Stworzę nowy kanał głosowy o danej nazwie!")]
        [Cooldown(5)]
        [RequireUserPermission(GuildPermission.ManageChannels)]        
        public async Task Voice([Remainder]string NazwaKanału)
        {
            await Context.Guild.CreateVoiceChannelAsync(NazwaKanału);

            var embed = EmbedHandler.CreateEmbed("Stworzyłem kanał głosowy o nazwie " + NazwaKanału + " na tym serwerze!", EmbedHandler.EmbedMessageType.Success);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("TęczoRola"), Alias("TR", "RR", "TeczoRola", "RainbowRole")]
        [Remarks("Sprawię, że kolor danej roli będzie tęczowy! :ok_woman::gay_pride_flag:")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RainbowRole(int Szybkość, string NazwaRoli)
        {
            if (Context.Message.Author.Id == 356147668231258113)
            {
                var embed = EmbedHandler.CreateEmbed("Sprawiłem, że rola o nazwie " + NazwaRoli + " ma gejowy kolor zmieniający się z szybkością " + Szybkość + "!", EmbedHandler.EmbedMessageType.Success);
                await Context.Channel.SendMessageAsync("", false, embed);

                var Role = Context.Guild.Roles.Where(x => x.Name == NazwaRoli).FirstOrDefault() as SocketRole;
                int r = 255, g = 0, b = 0;

                for (; ; )
                {
                    while (g + Szybkość <= 255)
                    {
                        g = g + Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (r - Szybkość >= 0)
                    {
                        r = r - Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (b + Szybkość <= 255)
                    {
                        b = b + Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (g - Szybkość >= 0)
                    {
                        g = g - Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (r + Szybkość <= 255)
                    {
                        r = r + Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (b - Szybkość >= 0)
                    {
                        b = b - Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (b + Szybkość <= 255)
                    {
                        b = b + Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (r - Szybkość >= 0)
                    {
                        r = r - Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (g + Szybkość <= 255)
                    {
                        g = g + Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (b - Szybkość >= 0)
                    {
                        b = b - Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (r + Szybkość <= 255)
                    {
                        r = r + Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                    while (g - Szybkość >= 0)
                    {
                        g = g - Szybkość;
                        await Role.ModifyAsync(x => x.Color = new Color(r, g, b));
                    }
                }
            }
            else
            {
                var embed = EmbedHandler.CreateEmbed("Odmowa serwera nadrzędnego: _Za dużo publicznych zgłoszeń_, Ilość Slotów: **5**/**5**. Zdonejtuj mnie, a automatycznie znikną Ci ograniczenia :money_with_wings::innocent:", EmbedHandler.EmbedMessageType.Exception);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("ZmieńGrę"), Alias("UstawGre", "ChangeGame", "SetGame")]
        [Remarks("Zagram w coś innego! :video_game: [Tylko dla współtwórców]")]
        [RequireOwner]
        public async Task SetGame(int typ,[Remainder] string NazwaGry)
        {
            if (typ == 0)
            {
                await Context.Client.SetGameAsync(NazwaGry, "https://www.twitch.tv/jucha1337", ActivityType.Streaming);

                var embed = EmbedHandler.CreateEmbed($"Zmieniłem na streama o nazwie **{NazwaGry}**", EmbedHandler.EmbedMessageType.Success);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                await Context.Client.SetGameAsync(NazwaGry);

                var embed = EmbedHandler.CreateEmbed($"Zmieniłem na grę **{NazwaGry}**", EmbedHandler.EmbedMessageType.Success);
                await Context.Channel.SendMessageAsync("", false, embed);
            }

            /*else if (typ == 2)
            {
                await Context.Client.SetGameAsync(NazwaGry, "https://www.twitch.tv/bugger", ActivityType.Listening);
            }
            else if (typ == 3)
            {
                await Context.Client.SetGameAsync(NazwaGry, "https://www.youtube.com/watch?v=Z-tc91hArlM", ActivityType.Watching);
            } */
        }

        [Command("ZmieńAvatar"), Alias("A", "Aw", "Av", "Avatar", "UstawAvatar", "ChangeAvatar", "SetAvatar", "ZmienAvatar")]
        [Remarks("Zmienię swój wygląd! :bust_in_silhouette: [Tylko dla współtwórców]")]
        [RequireOwner]
        public async Task SetAvatar(string link)
        {
            if (link == "domyślny")
            {
                var image = new Image(Context.Client.CurrentUser.GetDefaultAvatarUrl());
                await Context.Client.CurrentUser.ModifyAsync(k => k.Avatar = image);
            }
            else
            {
                var s = Context.Message.DeleteAsync();
                try
                {
                    var webClient = new WebClient();
                    byte[] imageBytes = webClient.DownloadData(link);

                    var stream = new MemoryStream(imageBytes);

                    var image = new Image(stream);
                    await Context.Client.CurrentUser.ModifyAsync(k => k.Avatar = image);

                    var embed = new EmbedBuilder();

                    embed.WithTitle("Zmieniłem avatar na :arrow_down:");
                    embed.WithImageUrl(link);
                    embed.WithColor(0, 255, 0);

                    await Context.Channel.SendMessageAsync("", embed: embed.Build());
                }

                catch (Exception)
                {
                    var embed = EmbedHandler.CreateEmbed("Nie udało mi się zmienić avatara!", EmbedHandler.EmbedMessageType.Exception);
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
            }
        }
            

        [Command("ZmieńStatus"), Alias("S", "Status")]
        [Remarks(":red_circle: [Tylko dla współtwórców]")]
        [RequireOwner]
        public async Task Status( string metoda)
        {
            if (metoda == "HB")
            {
                for (; ; )
                {
                    await Task.Delay(5000);
                    Context.Client.SetStatusAsync(UserStatus.AFK);
                    await Task.Delay(300);
                    Context.Client.SetStatusAsync(UserStatus.DoNotDisturb);
                    await Task.Delay(300);
                    Context.Client.SetStatusAsync(UserStatus.Online);
                }
            }
            else if (metoda == "PL")
            {
                for (; ; )
                {
                    Context.Client.SetStatusAsync(UserStatus.AFK);
                    await Task.Delay(500);
                    Context.Client.SetStatusAsync(UserStatus.DoNotDisturb);
                    await Task.Delay(500);
                }
            }
            else 
            {
                for (; ; )
                {
                    await Task.Delay(5000);
                    Context.Client.SetStatusAsync(UserStatus.DoNotDisturb);
                    await Task.Delay(300);
                    Context.Client.SetStatusAsync(UserStatus.Online);
                }
            }
        }
        public async Task<IRole> GetMuteRole(IGuild guild)
        {
            const string defaultMuteRoleName = "uciszony";

            var muteRoleName = "uciszony";

            var muteRole = guild.Roles.FirstOrDefault(r => r.Name == muteRoleName);

            if (muteRole == null)
            {
                try
                {
                    muteRole = await guild.CreateRoleAsync(muteRoleName, GuildPermissions.None).ConfigureAwait(false);
                }
                catch
                {
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name == muteRoleName) ?? await guild.CreateRoleAsync(defaultMuteRoleName, GuildPermissions.None).ConfigureAwait(false);
                }
            }

            foreach (var toOverwrite in (await guild.GetTextChannelsAsync()))
            {
                try
                {
                    if (!toOverwrite.PermissionOverwrites.Any(x => x.TargetId == muteRole.Id && x.TargetType == PermissionTarget.Role))
                    {
                        await toOverwrite.AddPermissionOverwriteAsync(muteRole, denyOverwrite)
                                .ConfigureAwait(false);

                        await Task.Delay(200).ConfigureAwait(false);
                    }
                }
                catch
                {

                }
            }

            return muteRole;
        }
    }
}

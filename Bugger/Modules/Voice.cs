using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Preconditions;

namespace Bugger.Modules
{
    public class TymczasowyKanałGłosowy : ModuleBase<MiunieCommandContext>
    {
        const int maxTimeInMinutes = 21600000;

        [Command("TymczasowyKanałGłosowy", RunMode = RunMode.Async)]
        [Alias("TKG", "TKGłosowy", "TKanałG", "TKanałGłosowy", "TymczasowyKG", "TymczasowyKGłosowy", "TymczasowyKanałG", "Tymczasowy", "TymczasowyGłosowy", "TymczasowyKanalGlosowy", "TymczasowyGlosowy", "TVCH", "TVChannel", "TVoiceCH", "TVoiceChannel", "TemporaryVCH", "TemporaryVChannel", "TemporaryVoiceCH", "Temporary", "TemporaryVoice", "TemporaryVCH", "TVoice", "TemporaryVoiceChannel")]
        [Summary("Pozwala tworzyć użytkownikom ich własne, tymczasowe kanały głosowe, które po czasie automatycznie znikają. **[VIP FEATURE]**")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [Cooldown(900, false)]
        public async Task CreateTemporaryVoiceChannel(int lifetimeInMinutes = 0)
        {
            lifetimeInMinutes *= 60000;
            if (lifetimeInMinutes == 0)
            {
                var use = await Context.Channel.SendMessageAsync("**Zapomniałeś o podaniu czasu...**\nSpróbuj tak:```<prefix>tymczasowykanałgłosowy {CzasWMinutach}```\nlub poprostu:```<prefix>tkg {CzasWMinutach}```");
                await Task.Delay(10000);
                await use.DeleteAsync();
            }
            else if (lifetimeInMinutes >= maxTimeInMinutes)
            {
                var embed = new EmbedBuilder();
                embed.WithTitle("**Tymczasowy Prywatny Kanał Głosowy**");
                embed.WithDescription("Maksymalny czas: **360** minut = 6h\n{CzasWMinutach} <= 360");
                embed.AddField("Spróbuj tak:\n", "```<prefix>tymczasowykanałgłosowy {CzasWMinutach}```");
                embed.AddField("\nlub poprostu:\n", "```<prefix> tkg {CzasWMinutach}```\n");
                embed.WithColor(0, 255, 0);
                await Context.Channel.SendMessageAsync("", embed: embed.Build());
            }
            else if (lifetimeInMinutes == 60000)
            {
                await Context.Message.DeleteAsync();
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync(name: $"Tymczasowy Kanał Głosowy użytkownika {Context.User.Username} na ({lifetimeInMinutes / 60000} minut)");
                var msg = await Context.Channel.SendMessageAsync($"Stworzyłem Ci tymczasowy kanał głosowy {Context.User.Mention}!");
                await msg.ModifyAsync(m => { m.Content = $"Stworzyłem Ci tymczasowy kanał głosowy! {Context.User.Mention}! Masz jeszcze {lifetimeInMinutes / 60000} minut."; });
                await Task.Delay(lifetimeInMinutes);
                await voiceChannel.DeleteAsync();
                await msg.DeleteAsync();
            }
            else if (lifetimeInMinutes >= 60001 && lifetimeInMinutes <= maxTimeInMinutes)
            {
                await Context.Message.DeleteAsync();
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync(name: $"Tymczasowy Kanał Głosowy użytkownika {Context.User.Username} na ({lifetimeInMinutes / 60000} minut)");
                var msg = await Context.Channel.SendMessageAsync($"Stworzyłem Ci tymczasowy kanał głosowy! {Context.User.Mention}! Masz jeszcze {lifetimeInMinutes / 60000} minut.");
                await Task.Delay(lifetimeInMinutes);
                await voiceChannel.DeleteAsync();
                await msg.ModifyAsync(x => { x.Content = $"{Context.User.Mention}, czas twojego kanału nadszedł... czas upłynął."; });
            }
        }
    }
}
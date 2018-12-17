using System.Linq;
using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;
using Discord;
using Discord.Commands;

namespace Bugger.Modules
{
    [Group("Prefix"), Alias("P", "Px", "Pref", "Prefiks"), Summary("Ustawienia prefixu na serwerze. (w razie czego zawsze możesz mnie oznaczyć zamiast prefixu :open_hands:)")]
    [RequireContext(ContextType.Guild)]
    public class Prefix : ModuleBase<MiunieCommandContext>
    {
        [Command("Dodaj"), Alias("Ustaw", "Add", "Set"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Dodam prefix do listy prefixów na serwerze")]
        public async Task AddPrefix([Remainder] string PrefiksDoDodania)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = $"Nie udało mi się dodać prefiksu.... Może `{PrefiksDoDodania}` jest już prefiksem?";
            if (!guildAcc.Prefixes.Contains(PrefiksDoDodania))
            {
                var prefixes = guildAcc.Prefixes.ToList();
                guildAcc.Modify(g => g.SetPrefixes(prefixes.Append(PrefiksDoDodania).ToList()));
                response =  $"Pomyślnie dodałem `{PrefiksDoDodania}` do listy prefiksów!";
            }
            await ReplyAsync(response);
        }

        [Command("Usuń"), Alias("Remove", "Delete", "Usun"), Remarks("Usunę prefiks z listy prefiksów na serwerze")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemovePrefix([Remainder] string PrefiksDoUsunięcia)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = $"Nie udało mi się usunąć prefiksu... Może `{PrefiksDoUsunięcia}` nie jest prefiksem?";
            if (guildAcc.Prefixes.Contains(PrefiksDoUsunięcia))
            {
                var prefixes = guildAcc.Prefixes.ToList();
                prefixes.Remove(PrefiksDoUsunięcia);
                guildAcc.Modify(g => g.SetPrefixes(prefixes));
                response =  $"Pomyślnie usunąłem `{PrefiksDoUsunięcia}` z listy prefiksów!";
            }
            await ReplyAsync(response);
        }

        [Command("Lista"), Alias("Prefixy", "Prefiksy", "List"), Remarks("Pokażę wszystkie działające prefiksy tego serwera")]
        public async Task ListPrefixes()
        {
            var prefixes = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id).Prefixes;
            var response = "Nie ustawiono jeszcze prefiksu ... po prostu wspomnij mnie, by użyć komend!";
            if (prefixes.Count != 0) response = "Dostępne prefixy:\n`" + string.Join("`, `", prefixes) + "`\nAlbo poprostu wspomnij mnie! :grin:";
            await ReplyAsync(response);
        }
    }
}

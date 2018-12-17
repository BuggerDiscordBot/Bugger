using System;
using System.Linq;
using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;
using Bugger.Providers;
using Discord;
using Discord.Commands;

namespace Bugger.Modules.RoleAssignments
{
    [Group("AutoRola"), Alias("AR", "RoleNaHasło", "AutomatyczneRole", "RNH", "AutoRoles"), Summary("Ustawienia dla automatycznego przydzielania użytkownikowi roli po wysłaniu określonego hasła albo wyrażenia (np. w celu cenzury czy też weryfikacji czy nowy użytkownik przeczytał reguamin).")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class RoleByPhrase : ModuleBase<MiunieCommandContext>
    {
        [Command("Status"), Alias("S", "L", "Lista", "List"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Pokażę obecny stan haseł, roli i ich realcji!")]
        public async Task RbpStatus()
        {
            var rbp = GlobalGuildAccounts.GetGuildAccount(Context.Guild).RoleByPhraseSettings;

            var phrases = rbp.Phrases.Any() ? string.Join("\n", rbp.Phrases.Select(p => $"({rbp.Phrases.IndexOf(p)}) - {p}")) : "Brak zapisanych haseł\nDodaj jakieś używając `<p>ar dh <hasło>`";
            var roles = rbp.RolesIds.Any() ? string.Join("\n", rbp.RolesIds.Select(r => $"({rbp.RolesIds.IndexOf(r)}) - {Context.Guild.GetRole(r).Name}")) : "Brak zapisanych ról\nDodaj jakieś używając `<p>ar dodajrolę <NazwaRoli>`";
            var relations = rbp.Relations.Any() ? string.Join("\n", rbp.Relations.Select(r => $"Hasło {r.PhraseIndex} => Rola {r.RoleIdIndex}")) : "Brak stworzonych relacji\nStwórz je używając `<p>rbp dodajrelację <NumerHasła> <NumerRoli>`";

            var embed = new EmbedBuilder();
            embed.WithColor(0, 255, 0);
            embed.WithTitle($"Ustawienia automatycznych roli serwera: {Context.Guild.Name}");
            embed.AddField("Hasła", phrases);
            embed.AddField("Role", roles);
            embed.AddField("Relacje", relations);
            embed.WithFooter(Global.GetRandomDidYouKnow());
            embed.WithCurrentTimestamp();

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("DodajHasło"), Alias("H", "DH", "Hasło", "UstawHasło", "DodajHaslo", "Haslo", "Password", "AddPassword"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Dodaje nowe hasło dle tego serwera, po wpisaniu którego przydzielę temu użytkownikowi rolę, jaka wynika z relacji.")]
        public async Task RbpAddPhrase([Remainder]string phrase)
        {
            var result = RoleByPhraseProvider.AddPhrase(Context.Guild, phrase);

            if (result == RoleByPhraseProvider.RoleByPhraseOperationResult.Success)
            {
                await RbpStatus();
            }
            else
            {
                await ReplyAsync("Pracuję 24/h... to nie mogło się udać...");
                Console.WriteLine(result.ToString());
            }
        }

        [Command("DodajRolę"), Alias("UstawRolę", "DodajRole", "UstawRole", "AddRole", "SetRole"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Dodaje nową rolę, która będzie mogła być używanania do automatycznego przydzielania na tym serwerze.")]
        public async Task RbpAddRole(IRole role)
        {
            var result = RoleByPhraseProvider.AddRole(Context.Guild, role);

            if (result == RoleByPhraseProvider.RoleByPhraseOperationResult.Success)
            {
                await RbpStatus();
            }
            else
            {
                await ReplyAsync("Łups... coś zyebauem... xd");
                Console.WriteLine(result.ToString());
            }
        }

        [Command("DodajRelację"), Alias("UstawRelację", "DodajRelacje", "UstawRelacje", "AddRelation", "SetRelation"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Dodam nową relację pomiędzy hasłem a rolą. Relacja po utworzeniu odrazu zostaje włączona.")]
        public async Task RbpAddRelation(int phraseIndex, int roleIndex)
        {
            var result = RoleByPhraseProvider.CreateRelation(Context.Guild, phraseIndex, roleIndex);

            if (result == RoleByPhraseProvider.RelationCreationResult.Success)
            {
                await RbpStatus();
            }
            else
            {
                await ReplyAsync("Aha... więc nooo ten... nie działa.");
                Console.WriteLine(result.ToString());
            }
        }

        [Command("UsuńHasło"), Alias("UH", "DP", "RP", "UsunSlowo", "RemovePharse", "DeletePharse"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Usunę słowo i jego relacje!")]
        public async Task RbpRemovePhrase(int phraseIndex)
        {
            RoleByPhraseProvider.RemovePhrase(Context.Guild, phraseIndex);
            await RbpStatus();
        }

        [Command("UsuńRolę"), Alias("UR", "DR", "RR", "UsunRole", "RemovePharse", "DeletePharse"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Usunę rolę i jej relacje!")]
        public async Task RbpRemoveRole(int roleIndex)
        {
            RoleByPhraseProvider.RemoveRole(Context.Guild, roleIndex);
            await RbpStatus();
        }

        [Command("UsuńRelację"), Alias("UR", "DR", "RR", "UsunRelacje", "RemoveRelation", "DeleteRelation"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Usunę relację pomiędzy słowem a rolą!")]
        public async Task RbpRemoveRelation(int phraseIndex, int roleIndex)
        {
            RoleByPhraseProvider.RemoveRelation(Context.Guild, phraseIndex, roleIndex);
            await RbpStatus();
        }
    }
}
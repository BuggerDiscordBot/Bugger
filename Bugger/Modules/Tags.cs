using Discord.Commands;
using System.Threading.Tasks;
using Bugger.Features.GlobalAccounts;
using Discord;
using Bugger.Entities;
using Bugger.Extensions;

namespace Bugger.Modules
{
    [Group("T"), Alias("Tag", "ServerTag", "Tags", "ServerTags")]
    [Summary("Własne komendy dla tego serwera za pomocą tagów!")]
    [RequireContext(ContextType.Guild)]
    public class ServerTags : ModuleBase<MiunieCommandContext>
    {
        [Command(""), Priority(-1), Remarks("Wypróbuję i wyślę info o danym tagu!")]
        public async Task ShowTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                await ReplyAsync("Musisz podać nazwę komendy!");
                return;
            }
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = TagFunctions.GetTag(tagName, guildAcc);
            await ReplyAsync(response);
        }

        [Command("Dodaj"), Alias("Ustaw", "Add", "Set"), Remarks("Dodam nowy tag dla tego serwera!")]
        public async Task AddTag(string tagName, [Remainder] string tagContent)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = TagFunctions.AddTag(tagName, tagContent, guildAcc);
            await ReplyAsync(response);
        }

        [Command("Aktualizuj"), Alias("A", "Update"), Remarks("Zaktualizuję serwerowy tag!")]
        public async Task UpdateTag(string tagName, [Remainder] string tagContent)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = TagFunctions.UpdateTag(tagName, tagContent, guildAcc);
            await ReplyAsync(response);
        }

        [Command("Usuń"), Alias("U", "Remove", "Delete", "Usun"), Remarks("Usunę tag z tego serwera!")]
        public async Task RemoveTag(string tagName)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = TagFunctions.RemoveTag(tagName, guildAcc);
            await ReplyAsync(response);
        }

        [Command("Lista"), Alias("List"), Remarks("Pokażę wszystkie specialne tagi tego serwera!")]
        public async Task ListTags()
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var emb = TagFunctions.BuildTagListEmbed(guildAcc);
            await ReplyAsync("", false, emb);
        }
    }

    [Group("PT"), Alias("OT", "OsobistyTag", "OsobisteTagi", "PersonalTags", "PersonalTag", "PTags", "PTag")]
    [Summary("Twoje (i tylko Twoje) własne komendy działające globalnie! (na wszystkich serwerach, jeśli tylko na nim jestem!)")]
    [RequireContext(ContextType.Guild)]
    public class PersonalTags : ModuleBase<MiunieCommandContext>
    {
        [Command(""), Priority(-1), Remarks("Wypróbuję i wyślę info o danym tagu!")]
        public async Task ShowTag(string tagName = "")
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                await ReplyAsync("Musisz podać nazwę tagu. Spróbuj ``<p>t <NazwaTagu>``");
                return;
            }
            var userAcc = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            var response = TagFunctions.GetTag(tagName, userAcc);
            await ReplyAsync(response);
        }

        [Command("Dodaj"), Alias("D", "Ustaw", "Add", "Set"), Remarks("Dodam Twój nowy osobisty tag!")]
        public async Task AddTag(string tagName, [Remainder] string tagContent)
        {
            var userAcc = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            var response = TagFunctions.AddTag(tagName, tagContent, userAcc);
            await ReplyAsync(response);
        }

        [Command("Aktualizuj"), Alias("A", "Update"), Remarks("Zaktualizuję Twój osobisty tag!")]
        public async Task UpdateTag(string tagName, [Remainder] string tagContent)
        {
            var userAcc = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            var response = TagFunctions.UpdateTag(tagName, tagContent, userAcc);
            await ReplyAsync(response);
        }

        [Command("Usuń"), Alias("U", "Remove", "Delete", "Usun"), Remarks("Usunę Twój osobisty tag!")]
        public async Task RemoveTag(string tagName)
        {
            var userAcc = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            var response = TagFunctions.RemoveTag(tagName, userAcc);
            await ReplyAsync(response);
        }

        [Command("Lista"), Alias("List"), Remarks("Pokażę wszystki Twoje własne osobiste tagi!")]
        public async Task ListTags()
        {
            var userAcc = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            var emb = TagFunctions.BuildTagListEmbed(userAcc);
            await ReplyAsync("", false, emb);
        }
    }


    internal static class TagFunctions
    {
        internal static string AddTag(string tagName, string tagContent, IGlobalAccount account)
        {
            var response = "Tag z tą nazwą już instnieje!\n" +
                           "proponuję go zaktualizować, użyj: `update <NazwaTagu> <ZawartośćTagu>`";
            if (account.Tags.ContainsKey(tagName)) return response;
            account.Tags.Add(tagName, tagContent);
            if (account is GlobalGuildAccount)
                GlobalGuildAccounts.SaveAccounts(account.Id);
            else GlobalUserAccounts.SaveAccounts(account.Id);
            response = $"Dodałem tag z nazwą: `{tagName}`.";

            return response;
        }

        internal static Embed BuildTagListEmbed(IGlobalAccount account)
        {
            var tags = account.Tags;
            var embB = new EmbedBuilder().WithTitle("Nie ustawiono jeszcze tagów.  psst... dodaj jakieś...");
            if (tags.Count > 0) embB.WithTitle("Wszyyystkie tagi:");

            foreach (var tag in tags)
            {
                embB.AddField(tag.Key, tag.Value, true);
            }

            return embB.Build();
        }

        internal static string GetTag(string tagName, IGlobalAccount account)
        {
            if (account.Tags.ContainsKey(tagName))
                return account.Tags[tagName];
            return "Taki tag nie istnieje!";
        }

        internal static string RemoveTag(string tagName, IGlobalAccount account)
        {
            if (!account.Tags.ContainsKey(tagName))
                return "PROTIP: Nie możesz usunąć tagu, który nie istnieje...";

            account.Tags.Remove(tagName);
            if (account is GlobalGuildAccount)
                GlobalGuildAccounts.SaveAccounts(account.Id);
            else GlobalUserAccounts.SaveAccounts(account.Id);

            return $"Usunąłem tag z nazwą: {tagName}!";
        }

        internal static string UpdateTag(string tagName, string tagContent, IGlobalAccount account)
        {
            if (!account.Tags.ContainsKey(tagName))
                return "Zaktualizować, tak? Coś czego nie ma, tak?";

            account.Tags[tagName] = tagContent;
            if (account is GlobalGuildAccount)
                GlobalGuildAccounts.SaveAccounts(account.Id);
            else GlobalUserAccounts.SaveAccounts(account.Id);

            return $"Zaktualizowałem tag z nazwą: {tagName}!";
        }
    }
}

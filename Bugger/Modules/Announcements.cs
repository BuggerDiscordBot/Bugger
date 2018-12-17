using System.Linq;
using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;
using Discord;
using Discord.Commands;

namespace Bugger.Modules
{
    [Group("Ogłoszenia"), Alias("Obwieszczenie", "Ogłoszenie", "Obwieszczenia", "Ogloszenie", "Ogloszenia", "Announcement", "Announcements"), Summary("Ustawienia kanału ogłoszeń dla np. customowych powitań nowych członków czy pożegnań tych niezrozumianych przez adminów.\n")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Announcement : ModuleBase<MiunieCommandContext>
    {
        [Command("UstawKanał"), Alias("Dodaj", "Ustaw", "Set", "SetChannel", "UstawKanal", "Add"), Remarks("Ustawię kanał, na którym będą publikowane ogłoszenia!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetAnnouncementChannel(ITextChannel OznaczenieCzatu)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            guildAcc.Modify(g => g.SetAnnouncementChannelId(OznaczenieCzatu.Id));
            await ReplyAsync("Kanał z ogłoszeniami został ustawiony na " + OznaczenieCzatu.Mention);
        }

        [Command("UsuńKanał"), Alias("Usuń", "UnsetChannel", "Unset", "Off", "UsunKanal", "Usun"), Remarks("Wyłączę kanał ogłoszeń!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task UnsetAnnouncementChannel()
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            guildAcc.Modify(g => g.SetAnnouncementChannelId(0));
            await ReplyAsync("Nie ma już kanału ogłoszeń! Koniec z ogłoszeniami... RIP!");
        }
    }
    [Group("Powitania"), Alias("Po", "Witaj", "Powitanie", "Powitaj", "Welcome")]
    [Summary("Ustawienia wysyłania przeze mnie bezpośrednich wiadomości (**DM**esów) użytkownikowi, który dołączył do serwera (jedną z kilku wcześniej zdefiniowanych przez Ciebie).\n")]
    public class WelcomeMessages : ModuleBase<MiunieCommandContext>
    {
        [Command("Dodaj"), Alias("D", "Do", "Ustaw", "Add", "Set")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Dodaje zredagowany przez Ciebie przykład wiadomości powitalnej!\n Przykład: `<p>powitania dodaj <OznaczenieUżytkownika>, witaj w **<NazwaSerwera>**!`\n" +
                    "Możesz użyć np: `<OznaczenieUżytkownika>`, `<NazwaUżytkownika>`, `<NazwaSerwera>`, `<NazwaBota>`, `<DyskryminatorBota>`, `<OznaczenieBota>`")]

        public async Task AddWelcomeMessage([Remainder] string TreśćWiadomościPowitalnej)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = $"Nie udało mi się dodać tej wiadomości powitalnej... Sry...";
            if (!guildAcc.WelcomeMessages.Contains(TreśćWiadomościPowitalnej))
            {
                var messages = guildAcc.WelcomeMessages.ToList();
                messages.Add(TreśćWiadomościPowitalnej);
                guildAcc.Modify(g => g.SetWelcomeMessages(messages));
                response =  $"Dodałem ```\n{TreśćWiadomościPowitalnej}\n``` jako jedną z wiadomości powitalnych!";
            }
            await ReplyAsync(response);
        }

        [Command("Usuń"), Alias("U", "Us", "Remove", "Delete", "Usun"), Remarks("Usunę daną wiadomość powitalną z listy używanych!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveWelcomeMessage(int NumerWiadomości)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var messages = guildAcc.WelcomeMessages.ToList();
            var response = $"Nie udało mi się usunąć tej wiadomości powitalnej... Spróbuj użyć numeru pokazanego w komendzie `<p>powitania lista` (to ta obok znaku `#`) ^^";
            if (messages.Count > NumerWiadomości - 1)
            {
                messages.RemoveAt(NumerWiadomości - 1);
                guildAcc.Modify(g => g.SetWelcomeMessages(messages));
                response =  $"Usunąłem wiadomość numer #{NumerWiadomości} z wiadomości powitalnych!";
            }
            await ReplyAsync(response);
        }

        [Command("Lista"), Alias("L", "Li", "List"), Remarks("Pokażę wszystkie ustawione wiadomości powitalne!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListWelcomeMessages()
        {
            var welcomeMessages = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id).WelcomeMessages;
            var embB = new EmbedBuilder().WithTitle("Nie mam ustawionych jeszcze żadnych wiadomości powitalnych... jeśli chcesz żeby nowi ludzie byli szczęśliwi, dodaj kilka ;)");
            if (welcomeMessages.Count > 0) embB.WithTitle("Wiadomości powitalne:");

            for (var i = 0; i < welcomeMessages.Count; i++)
            {
                embB.AddField($"Przykład #{i + 1}:", welcomeMessages[i]);
            }
            await ReplyAsync("", false, embB.Build());
        }
    }

    [Group("Pożegnania"), Alias("Pozegnania" ,"Odejście", "Odejścia", "Opuszczenie", "Opuszczenia", "Odejscie", "Odejscia", "Leave", "Left")]
    [Summary("Ustawienia ogłaszania przeze mnie odeszłego użytkownika na ustawionym kanale ogłoszeniowym (jedną ze zdefiniowanych wcześniej przez Ciebie wiadomości).\n")]
    public class LeaveMessages : ModuleBase<MiunieCommandContext>
    {
        [Command("Dodaj"), Alias("D", "Do", "Ustaw", "Add", "Set")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Dodaje zredagowany przez Ciebie przykład wiadomości pożegnalnej\nPrzykład: `<p>pożegnania dodaj Oh niet! <OznaczenieUżytkownika>, opuścił <NazwaSerwera>...`\n" +
                 "Możesz użyć np: `<OznaczenieUżytkownika>`, `<NazwaUżytkownika>`, `<NazwaSerwera>`, `<NazwaBota>`, `<DyskryminatorBota>`, `<OznaczenieBota>`")]
        public async Task AddLeaveMessage([Remainder] string TreśćWiadomościPożegnalnej)
        {
            var guildAcc = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var response = $"Nie udało mi się dodać tej wiadomości pożegnalnej...";
            if (!guildAcc.LeaveMessages.Contains(TreśćWiadomościPożegnalnej))
            {
                var messages = guildAcc.WelcomeMessages.ToList();
                messages.Add(TreśćWiadomościPożegnalnej);
                guildAcc.Modify(g => g.SetLeaveMessages(messages));
                response =  $"Dodałem `{TreśćWiadomościPożegnalnej}` jako wiadomość pożegnalną!";
            }
            await ReplyAsync(response);
        }

        [Command("Usuń"), Alias("U", "Us", "Remove", "Delete", "Usun"), Remarks("Usunę wiadomość pożegnalną z listy używanych!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveLeaveMessage(int NumerWiadomości)
        {
            var guildAcc= GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id);
            var messages = guildAcc.LeaveMessages.ToList();
            var response = $"Nie udało mi się usunąć tej wiadomości pożegnalnej... Spróbuj użyć numeru pokazanego w komendzie `pożegnania list` obok znaku `#` :)";
            if (messages.Count > NumerWiadomości - 1)
            {
                messages.RemoveAt(NumerWiadomości - 1);
                guildAcc.Modify(g => g.SetLeaveMessages(messages));
                response =  $"Usunąłem #{NumerWiadomości} jako wiadomość powitalną!";
            }
            await ReplyAsync(response);
        }

        [Command("Lista"), Alias("L", "Li", "List"), Remarks("Pokażę wszystkie ustawione wiadomości pożegnalne!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListLeaveMessages()
        {
            var leaveMessages = GlobalGuildAccounts.GetGuildAccount(Context.Guild.Id).LeaveMessages;
            var embB = new EmbedBuilder().WithTitle("Nie mam ustawionych jeszcze żadnych wiadomości pożegnalnych... jeśli chcesz żeby reszta serwera wiedziała o naszej \"stracie\", dodaj kilka ;)"); 
            if (leaveMessages.Count > 0) embB.WithTitle("Przykładowe wiadomości pożegnalne");

            for (var i = 0; i < leaveMessages.Count; i++)
            {
                embB.AddField($"Przykład #{i + 1}:", leaveMessages[i]);
            }
            await ReplyAsync("", false, embB.Build());
        }
    }
}

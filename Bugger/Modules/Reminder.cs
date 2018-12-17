using System;
using System.Globalization;
using System.Threading.Tasks;
using Bugger.Entities;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;
using Discord;
using Discord.Commands;

namespace Bugger.Modules
{
    public class ReminderFormat
    {
        public static string[] Formats =
        {
            // Służy do rozdzielenia np. 5d11h45m ===> 5d 11h 45m ; można dodawać/usuwać w razie potrzeby

            "d'd'",
            "d'd'm'm'", "d'd 'm'm'",
            "d'd'h'h'", "d'd 'h'h'",
            "d'd'h'h's's'", "d'd 'h'h 's's'",
            "d'd'm'm's's'", "d'd 'm'm 's's'",
            "d'd'h'h'm'm'", "d'd 'h'h 'm'm'",
            "d'd'h'h'm'm's's'", "d'd 'h'h 'm'm 's's'",

            "h'h'",
            "h'h'm'm'", "h'h m'm'",
            "h'h'm'm's's'", "h'h 'm'm 's's'",
            "h'h's's'", "h'h s's'",
            "h'h'm'm'", "h'h 'm'm'",
            "h'h's's'", "h'h 's's'",

            "m'm'",
            "m'm's's'", "m'm 's's'",

            "s's'"
        };
    }

    /// <summary>
    /// Uwaga! Obecna strefa czasowa będzie działać tylko na Maszynach opartych na Linuxie, NIE WINDOWSIE!
    /// aby uruchomić w systemie Windows, musimy użyć nazwy strefy czasowej, możesz zaimplementować obie, dodaj TimeZoneName jako zmienną do User accounts i zmień nazwę TimeZone na TimeZoneId
    /// </summary>
    [Group("Przypomnienie"), Alias("P", "Przypomnij", "Przypomnienia", "Alarm", "Obudź", "Obudz", "Budzik", "R", "Reminder", "Remind")]
    [Summary("Przypomnę Ci o czymś za jakiś czas, wysyłając Ci **DM**esa z tekstem, jaki sobie zamarzysz.")]
    public class Reminder : ModuleBase<MiunieCommandContext>
    {
        [Command(""), Alias("D", "Dodaj", "Ustaw", "Add", "Set", "New", "Add"), Priority(0), Remarks("Dodam przypomnienie")]
        public async Task AddReminder([Remainder] string args)
        {
            string[] splittedArgs = null;
            if (args.Contains(" za ")) splittedArgs = args.Split(new string[] {" za "}, StringSplitOptions.None);
            if (splittedArgs == null || splittedArgs.Length < 2)
            {
                await ReplyAsync("Pozwól biedna owieczko że wytłumaczę Ci jak to zrobić... OK?\n" +
                                 "Czyli tak: `<p>p <Czynności> za 2d 23h 3m 12s`\n" +
                                 "I to \"za\" jest troszkę ważne ^^");
                return;
            }

            var timeString = splittedArgs[splittedArgs.Length - 1];
            if (timeString == "24h")
                timeString = "1d";

            splittedArgs[splittedArgs.Length - 1] = "";
            var reminderString = string.Join(" za ", splittedArgs, 0, splittedArgs.Length - 1);

            var timeDateTime = DateTime.UtcNow + TimeSpan.ParseExact(timeString, ReminderFormat.Formats, CultureInfo.CurrentCulture);

            var newReminder = new ReminderEntry(timeDateTime, reminderString);

            var account = GlobalUserAccounts.GetUserAccount(Context.User.Id);

            account.Reminders.Add(newReminder);
            GlobalUserAccounts.SaveAccounts(Context.User.Id);


            var timezone = account.TimeZone ?? "UTC";
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById($"{timezone}");
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(timeDateTime, tz);

            var bigmess2 =
                $"{reminderString}\n\n" +
                $"Wyślę Ci **DM**esa za  __**{localTime}**__ `by {timezone}`\n";

            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User);
            embed.WithCurrentTimestamp();
            embed.WithColor(Color.Blue);
            embed.WithTitle("Przypomnę Ci przez **DM**esa:");
            embed.AddField($"**____**", $"{bigmess2}");

            ReplyAsync("", false, embed.Build());
        }

        [Command("")]
        [Alias("WłączPrzypomnienie")]
        [Priority(1)]
        [Remarks("Włącza Przypomnienie")]
        public async Task AddReminderOn(string timeOn, [Remainder] string args)
        {
            string[] splittedArgs = { };
            if (args.ToLower().Contains("  o "))
                splittedArgs = args.ToLower().Split(new[] { "  o " }, StringSplitOptions.None);
            else if (args.ToLower().Contains(" o  "))
                splittedArgs = args.ToLower().Split(new[] { " o  " }, StringSplitOptions.None);
            else if (args.ToLower().Contains("  o  "))
                splittedArgs = args.ToLower().Split(new[] { "  o  " }, StringSplitOptions.None);
            else if (args.ToLower().Contains(" o "))
                splittedArgs = args.ToLower().Split(new[] { " o " }, StringSplitOptions.None);

            if (!DateTime.TryParse(timeOn, out var myDate)) //|| myDate < DateTime.Now
            {
                await ReplyAsync("Format daty nieprawidłowy, spróbuj w ten sposób: `yyyy-mm-dd`");
                return;
            }

            if (splittedArgs == null)
            {
                await ReplyAsync("Pozwól biedna owieczko że wytłumaczę Ci jak to zrobić... OK?\n" +
                                 "Czyli tak: `<p>p 2018-08-22 <Tekst> at 14:22" +
                                 "I to \"za\" jest troszkę ważne ^^");
                return;
            }

            var account = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            var timezone = account.TimeZone ?? "UTC";
            var tz = TimeZoneInfo.FindSystemTimeZoneById($"{timezone}");
            var timeString = splittedArgs[splittedArgs.Length - 1];

            splittedArgs[splittedArgs.Length - 1] = "";

            var reminderString = string.Join(" at ", splittedArgs, 0, splittedArgs.Length - 1);
            var hourTime = TimeSpan.ParseExact(timeString, "h\\:mm", CultureInfo.CurrentCulture);
            var timeDateTime = TimeZoneInfo.ConvertTimeToUtc(myDate + hourTime, tz);
            var newReminder = new ReminderEntry(timeDateTime, reminderString);

            account.Reminders.Add(newReminder);
            GlobalUserAccounts.SaveAccounts(Context.User.Id);

            var bigmess2 =
                $"{reminderString}\n\n" +
                $"Wyślę Ci **DM**esa za __**{myDate + hourTime}**__ `by {timezone}`\n";

            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User);
            embed.WithCurrentTimestamp();
            embed.WithColor(Color.Blue);
            embed.WithTitle("Powiadomię Cię przez **DM**esa:");
            embed.AddField($"**____**", $"{bigmess2}");
            ReplyAsync("", false, embed.Build());
        }



        [Command("Lista"), Priority(2), Alias("Wszystkie", "Wykaz", "Wykaż", "Spis", "Pokaż", "PokażWszystkie", "PokazWszystkie", "List"), Remarks("Wyświetlę wszystkie twoje przypomnienia.")]
        public async Task ShowReminders()
        {
            var reminders = GlobalUserAccounts.GetUserAccount(Context.User.Id).Reminders;
            var embB = new EmbedBuilder()
                .WithTitle("Twoje przypomnienia (Czas według UTC / GMT+0)")
                .WithFooter("Czy wiesz, że? " + Global.GetRandomDidYouKnow())
                .WithDescription("Abym usunął przypomnienie, użyj polecenia `p usuń <numer>` " +
                                 "z numerem, który jest po lewej od dat w \"[]\".");

            for (var i = 0; i < reminders.Count; i++)
            {
                embB.AddField($"[{i+1}] {reminders[i].DueDate:f}", reminders[i].Description, true);
            }
            await ReplyAsync("", false, embB.Build(), null);
        }

        [Command("Usuń"), Priority(2), Alias("U", "D", "Usun", "Delete", "Remove"), Remarks("Usunę któreś z Twoich przypomnień")]
        public async Task DeleteReminder(int index)
        {
            var reminders = GlobalUserAccounts.GetUserAccount(Context.User.Id).Reminders;
            var responseString = "Ehhh... może najpier użyj `<p> lista` zanim zaczniesz usuwać przypomnienia które nie istnieją? :grin:";
            if (index > 0 && index <= reminders.Count)
            {
                reminders.RemoveAt(index - 1);
                GlobalUserAccounts.SaveAccounts(Context.User.Id);
                responseString = $"Usunąłem przypomnienie numer {index}!";
            }
            await ReplyAsync(responseString);
        }
    }
}

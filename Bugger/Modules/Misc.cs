using Bugger.Preconditions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Bugger.Helpers;
using System.Globalization;
using Bugger.Extensions;
using Bugger.Features.Lists;
using Discord.WebSocket;
using Discord.Rest;
using System.IO;

namespace Bugger.Modules
{
    public class Ogólne : ModuleBase<MiunieCommandContext>
    {
        private CommandService _service;
        private readonly ListManager _listManager;
        private int _fieldRange = 10;

        public Ogólne(CommandService service, ListManager listManager)
        {
            _service = service;
            _listManager = listManager;
        }

        [Cooldown(15, true)]
        [Command("Pomoc"), Alias("P", "H", "Pomocy", "Pomóż", "Pomoz", "Help"), Remarks("Pokażę dokładniejsze informacje na temat konkretnej komendy. Jeśli jej nie sprecyzujesz, wyślę Ci **DM**esa ze wszystkimi komendami.")]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync("Wysłałem! :sparkling_heart:");

            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

            var contextString = Context.Guild?.Name ?? "DMs with me";
            var builder = new EmbedBuilder()
            {
                Title = "          :sos:  **Pomoc**  :sos:",
                Description = $"Masz tutaj wszystkie moje komendy. Pamiętaj, że większość komend ma też łatwiejsze skróty i nazwy zastępcze.\nNp. dwie poniższe komendy są równoznaczne. ```<prefix>pseudonim <NazwaUżytkownika>```=```<prefix>ps <NazwaUżytkownika>```\n" +
                $"Jeśli chcesz uzyskać więcej informacji o danej komendzie (np. właśnie o jej zamiennikach, parametrach czy jeśli są wymaganiach) napisz: ```<prefix>pomoc <NazwaKomendy>```\n",
                Color = new Color(0, 255, 0)
            };

            foreach (var module in _service.Modules)
            {
                await AddModuleEmbedField(module, builder);
            }

            // Limit wynosi 6000 znaków dla wiadomości, więc bierzemy pierwsze dziesięć pól
            // a następnie wysłamy wiadomości. W bieżącym stanie bot wyśle 2 wiadomości.

            var fields = builder.Fields.ToList();
            while(builder.Length > 6000)
            {
                builder.Fields.RemoveRange(0, fields.Count);
                var firstSet = fields.Take(_fieldRange);
                builder.Fields.AddRange(firstSet);
                if (builder.Length > 6000)
                {
                    _fieldRange--;
                    continue;
                }
                await dmChannel.SendMessageAsync("", false, builder.Build());
                fields.RemoveRange(0, _fieldRange);
                builder.Fields.RemoveRange(0, _fieldRange);
                builder.Fields.AddRange(fields);
            }

            await dmChannel.SendMessageAsync("", false, builder.Build());

            // Embedy są ograniczone do 24 pól na maks. Więc wyczyśćmy kilka rzeczy
            // a następnie wyślij go w wielu embedach, jeśli jest zbyt duży.

            builder.WithTitle("")
                .WithDescription("")
                .WithAuthor("");
            while (builder.Fields.Count > 24)
            {
                builder.Fields.RemoveRange(0, 25);
                await dmChannel.SendMessageAsync("", false, builder.Build());

            }
        }

        [Command("Pomocy"), Alias("P", "H", "Pomocy", "Pomóż", "Pomoz", "Help"), Remarks("Pokażę Ci Pomoc dla wybranej komendy.")]
        [Cooldown(5, false)]
        public async Task HelpQuery([Remainder] string NazwaKomendy)
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 222),
                Title = $":sos:  Pomoc dla: **\"{NazwaKomendy}\"**  :sos: "
            };

            var result = _service.Search(Context, NazwaKomendy);
            if (NazwaKomendy.StartsWith("module "))
                NazwaKomendy = NazwaKomendy.Remove(0, "module ".Length);
            var emb = result.IsSuccess ? HelpCommand(result, builder) : await HelpModule(NazwaKomendy, builder);

            if (emb.Fields.Length == 0)
            {
                await ReplyAsync($"Sorka, nie znalazłem opisu dla: \"{NazwaKomendy}\".");
                return;
            }

            await Context.Channel.SendMessageAsync("", false, emb);
        }

        [Command("Twórcy"), Alias("NapisyKońcowe", "Twórca", "Credits")]
        [Summary("Przedstawię moich \"bogów\"! :poop::heart_eyes:")]
        [Cooldown(5)]
        public async Task Credits()
        {
            var embB = new EmbedBuilder()
                .WithAuthor("Bugger - licencja BEERWARE")
                .WithTitle("Twórcy:")
                .WithDescription("Hejjj, miło, że tu zajrzałeś. :blush:\n" +
                "Tworzył mnie przez dziesięć dni (i dziesięć nocy) względnie 3-osobowy zespół w składzie:\n" +
                "***__~~Ejmi~~__*** - najlepszej żeńskiej programistki w naszym zespole, _(a przynajmniej robi to lepeij niż gotuje xdd)_\n" +
                "***__~~ZottelvonUrvieh~~__*** - _(on też nie wie jak to przeczytać, spk)_\nmenadżer projektu, kierownik artystyczny , mistrz teorii, psycholog, pediatra dziecięcy i jeśli trzeba ginekolog po ojcu " +
                "1 słowem bez niego nie _(nie)_ byłoby nic. :heart:\n" +
                "***__~~DODEK~~__*** - też cośtam klikał. A tak to sumie to tak jak poprzednik, tylko, że on umie programować XD. Dobrze zastępuje komputer we nasze wspólne chłodne noce przed Visual Studio :joy::sweat_smile::cry::sob:\n" +
                "Dodatkowe źródła: Autor publicznego \"Community-Discord-BOT\" (na którego strukturach się uczyliśmy i wzorowaliśmy) i tutoriala do implementacji języka C# dla discorda - Petrspelos\n" +
                "Po dokończeniu tego projektu, który mamy zamiar aktywnie wspierać jeszcze koło miesiąc planujemy coś coś... a z resztą zobaczycie!!!\nLove & Care,  **ADz Tim**")
                .WithFooter("Więcej info? Też chcesz bota? A może chcesz nas wspomóc? (xD) Pisz na priv!")
                .WithColor(0, 255, 0);

           var contributions = await GitHub.Contributions("Dodek69", "Bugger");
            // Sort contributions by commits
            contributions = contributions.OrderByDescending(contribution => contribution.total).ToList();
            // Creating the embeds with all the contributers and their stats
            embB = contributions.Aggregate(embB, (emb, cont) =>
            {
                // Accumulate all the weeks stats to the total stat
                var stats = cont.weeks.Aggregate(
                    Tuple.Create(0, 0),
                    (acc, week) => Tuple.Create(acc.Item1 + week.a, acc.Item2 + week.d)
                );
                return emb.AddField(GitHub.ContributionStat(cont, stats));
            });

            await ReplyAsync("", false, embB.Build());
        }

        [Command("Wersja"), Alias("W", "Wer", "Wer.", "Ver", "Version", "--Version"), Remarks("Pochwalę się swoją wersją! :yum:")]
        [Cooldown(5)]
        public async Task Version()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = new Color(114, 137, 218);
            //builder.AddField("Moja Wersja... :thinking: ", $"Tu jest napisane że: `{Global.version}` lol\n_Mogliby mnie raz za czas chociaż spaczować :unamused:");
            builder.AddField("Moja Wersja... :thinking: ", $"Tu jest napisane że: `Public Alpha - 0.9.9.9` lol\n_Mogliby mnie raz za czas chociaż spaczować_ :unamused:");
            await ReplyAsync("", false, builder.Build());
        }

        private static Embed HelpCommand(SearchResult search, EmbedBuilder builder)
        {
            foreach (var match in search.Commands)
            {
                var cmd = match.Command;
                var parameters = cmd.Parameters.Select(p => string.IsNullOrEmpty(p.Summary) ? p.Name : p.Summary);
                var paramsString = $"Parametry: {string.Join(", ", parameters)}" +
                                   (string.IsNullOrEmpty(cmd.Remarks) ? "" : $"\nOpis: {cmd.Remarks}") +
                                   (string.IsNullOrEmpty(cmd.Summary) ? "" : $"\nZamienniki: {cmd.Summary}");

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = paramsString;
                    x.IsInline = false;
                });
            }
            return builder.Build();
        }

        private async Task<Embed> HelpModule(string moduleName, EmbedBuilder builder)
        {
            var module = _service.Modules.ToList().Find(mod =>
                string.Equals(mod.Name, moduleName, StringComparison.CurrentCultureIgnoreCase));
            await AddModuleEmbedField(module, builder);
            return builder.Build();
        }

        private async Task AddModuleEmbedField(ModuleInfo module, EmbedBuilder builder)
        {
            if (module is null) return;
            var descriptionBuilder = new List<string>();
            var duplicateChecker = new List<string>();
            foreach (var cmd in module.Commands)
            {
                var result = await cmd.CheckPreconditionsAsync(Context);
                if (!result.IsSuccess || duplicateChecker.Contains(cmd.Aliases.First())) continue;
                duplicateChecker.Add(cmd.Aliases.First());
                var cmdDescription = $"`{cmd.Aliases.First()}`";
                if (!string.IsNullOrEmpty(cmd.Summary))
                    cmdDescription += $" | {cmd.Summary}";
                if (!string.IsNullOrEmpty(cmd.Remarks))
                    cmdDescription += $" | {cmd.Remarks}";
                if (cmdDescription != "``")
                    descriptionBuilder.Add(cmdDescription);
            }

            if (descriptionBuilder.Count <= 0) return;
            var builtString = string.Join("\n", descriptionBuilder);
            var testLength = builtString.Length;
            if (testLength >= 1024)
            {
                throw new ArgumentException("Wartość modułu nie może przekroczyć 1024 znaków!");
            }
            var moduleNotes = "";
            if (!string.IsNullOrEmpty(module.Summary))
                moduleNotes += $" {module.Summary}";
            if (!string.IsNullOrEmpty(module.Remarks))
                moduleNotes += $" {module.Remarks}";
            if (!string.IsNullOrEmpty(moduleNotes))
                moduleNotes += "\n";
            if (!string.IsNullOrEmpty(module.Name))
            {
                builder.AddField($"__**{module.Name}:**__",
                    $"{moduleNotes} {builtString}\n{Constants.InvisibleString}");
            }
        }    

        [Command("Bug")]
        [Alias("Błąd", "Zgłoszenie", "Zgłoś", "BugReport", "Issue", "Feedback")]
        [Summary("Dam Ci info co zrobić, jeśli kozaku, znalazłeś błąd!")]
        [Cooldown(5)]
        public async Task Bug()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Zgłaszanie buga __**Bug**__gera");
            embed.WithDescription(@"Jeśli znalazłeś błąd, albo poprostu chcesz nam pomóc w ulepszaniu **tego projektu**, powiadom nas tworząc problem na **GitHubie** :point_down:" + "\n\n" +
            "**[ 🢂 :fire: TUTAJ :fire:  🢀 ](https://github.com/Dodek69/Community-Discord-BOT/issues/new/choose)**" + "\n");
            embed.WithImageUrl("https://c8.alamy.com/comp/X3GBDF/woman-inside-elderly-woman-computer-housework-laptop-success-ebay-notebook-surprises-surprise-mobile-X3GBDF.jpg");
            embed.WithFooter("W przypadku pilnych wiadomości pisz bezpośrednio do nas:\n<p>z <treść>");
            embed.WithColor(255, 0, 0);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Zgloś"), Alias("Z", "Zglos", "Report", "ReportInPrivate")]
        [Remarks("Wyślę pilną informację do serwera mojego autora z treścią, jaką podasz! **UWAGA:** Tylko ważne wiadomości!!! | Module: **spam=ban** = **ON**")]
        [Cooldown(900, true)]
        public async Task Report([Remainder] string TreśćZgłoszenia)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Zgłoszenie buga od " + Context.Message.Author + " o " + Context.Message.Timestamp + " | " + TreśćZgłoszenia);
            for (int i = 4; i > 0; i--)
            {
                Console.Beep();
            }
            Console.ResetColor();

            string[] lines = TreśćZgłoszenia.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            string filename = "Zgłoszenie";

            string filepath = @"/Pulpit/" + filename + ".txt";

            if (File.Exists(filepath))
            {
                filename = "Zgłoszenie1";
            }
            File.WriteAllLinesAsync(filepath, lines);

            var embed = new EmbedBuilder();

            embed.WithTitle("Wysłałeś wiadomość do mojego pana! :punch::boom:");
            embed.WithImageUrl("https://images-na.ssl-images-amazon.com/images/I/512OOMv4ZEL.jpg");
            embed.WithFooter("THX 4 HELP <3");
            embed.WithColor(255, 0, 255);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }
        [Command("Zaproś"), Alias("Invite"), Remarks("Dodaj mnie do swojego serwera!")]
        [Cooldown(5)]
        public async Task Invite()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor("Proszę, ale nie myśl sobie, że teraz będę się Cię słuchał!");
            embed.WithTitle(":scream::sunglasses::kiss: LINK :scream::sunglasses::kiss:");
            //embed.WithUrl();

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("Matma"), Alias("Matematyka", "Math"), Summary("Zwrócę wynik wyrażenia metematycznego!")]
        [Cooldown(15)]
        public async Task Computate(params String[] WyrażenieMatematyczne)
        {
            StringBuilder word = new StringBuilder();
            for (int i = 0; i < WyrażenieMatematyczne.Length; i++)
            {
                char[] inputWithoutSpaces = WyrażenieMatematyczne.ElementAt(i).Where(c => !Char.IsWhiteSpace(c)).ToArray();
                for (int j = 0; j < inputWithoutSpaces.Count(); j++)
                {
                    word.Append(inputWithoutSpaces[j]);
                }

                WyrażenieMatematyczne[i] = word.ToString();
                word = new StringBuilder();
                if (WyrażenieMatematyczne.ElementAt(i).Length > 2)
                {
                    WyrażenieMatematyczne[i] = Operations.PerformComputation(WyrażenieMatematyczne[i]).ToString(CultureInfo.CurrentCulture);
                }
            }
            StringBuilder sentence = new StringBuilder();
            for (int i = 0; i < WyrażenieMatematyczne.Length; i++)
            {
                sentence.Append(WyrażenieMatematyczne[i]);
            }

            await ReplyAsync($"{Operations.PerformComputation(sentence.ToString())}");
        }

        [Command("Lista")]
        [Alias("List")]
        [Summary("Zarządza listami z niestandardową dostępnością według roli [zaawansowane]")]
        [Cooldown(5)]
        public async Task ManageList(params String[] input)
        {
            if (input.Length == 0) { return; }
            var user = Context.User as SocketGuildUser;
            var roleIds = user.Roles.Select(r => r.Id).ToArray();
            var availableRoles = Context.Guild.Roles.ToDictionary(r => r.Name, r => r.Id);
            var output = _listManager.HandleIO(new ListHelper.UserInfo(user.Id, roleIds), availableRoles, Context.Message.Id, input);
            RestUserMessage message;
            if (output.permission != ListHelper.ListPermission.PRIVATE)
            {
                message = (RestUserMessage)await Context.Channel.SendMessageAsync(output.outputString, false, output.outputEmbed);
            }
            else
            {
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                message = (RestUserMessage)await dmChannel.SendMessageAsync(output.outputString, false, output.outputEmbed);
            }
            if (output.listenForReactions)
            {
                await message.AddReactionAsync(ListHelper.ControlEmojis["up"]);
                await message.AddReactionAsync(ListHelper.ControlEmojis["down"]);
                await message.AddReactionAsync(ListHelper.ControlEmojis["check"]);
                ListManager.ListenForReactionMessages.Add(message.Id, Context.User.Id);
            }
        }
    }
}

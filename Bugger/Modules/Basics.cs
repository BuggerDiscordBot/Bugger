using Bugger.Preconditions;
using Discord.Commands;
using System.Threading.Tasks;
using Bugger.Extensions;
using Discord;
using System;

namespace Bugger.Modules
{
    public class Proste : ModuleBase<MiunieCommandContext>
    {
        [Command("PrzedstawSię")]
        [Alias("PSZS", "KJ", "KJ?", "PrzedstawSie", "KimJesteś", "Przedstaw się", "Przedstaw sie", "Kim jesteś", "Kim jestes", "Kim jesteś?", "Kim jestes?")]
        [Remarks("Przedstawię się! :sunglasses:")]
        [Cooldown(5)]
        public async Task PrzedstawSie()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor("Bugger®");
            embed.Author.WithIconUrl(Context.Client.CurrentUser.GetDefaultAvatarUrl());
            embed.WithTitle("JESTEM __**BUGGER**__");
            embed.WithDescription("Najlepszy Polski `bot`... **EVER**:exclamation:\n" +
                "Jestem napisany w **C#** przy użyciu biblioteki **Discord .NET 2.0** _(najnowocześniejszej :smirk:)_\n" +
                "Urodziłem się 5-go listopada 2K18 roku, na dysku D, ale moje marzenia i ciężka kompilacja sprawiły, że tutaj zaszedłem!\n" +
                "_No i tak ogl w skróciku to lepszego polskiego bota na disa nie ma XD_ (srly)\n" +
                "Ilość moich funkcji ___**niszczy**___ nawet ~~finansowane pełne projekty botów~~, a jestem darmowy :poop::information_desk_person:\n" +
                "Jak chcesz mnie zobaczyć w praktyce możesz napisać np.: `<prefix>kickme`\n" +
                "**GL HF** Paaa <333");
            embed.WithFooter("Napisali mnie połączeni w imię wiecznej przyjaźni: Amie & Dodek... i ZottelvonUrvieh");
            embed.WithColor(0, 255, 0);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("Hej")]
        [Alias("Siemka", "Cześć", "Hejo", "Hejka", "Elo", "Siemano Kolano", "SiemanoKolano", "Dzień dobry", "Dzieńdobry", "Dzien dobry", "Dzien dobry", "Echo", "Czesc")]
        [Remarks("Przywitam się!:hugging::relieved:")]
        [Cooldown(5)]
        public async Task SayHello()
        {
            var embed = new EmbedBuilder();

            embed.WithTitle("Hejka " + Context.User.Username + " :vulcan::bangbang:");
            embed.WithColor(0, 255, 0);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("Pa")]
        [Alias("Żegnaj", "Bye", "Goodbye", "SeeYou")]
        [Remarks("Pożegnam... :sweat:")]
        [Cooldown(5)]
        public async Task SayBye()
        {
            var embed = new EmbedBuilder();

            embed.WithTitle("Żegnaj " + Context.User.Username + ":broken_heart::sob:");
            embed.WithColor(100, 100, 100);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("Powiedz")]
        [Alias("P", "Powtórz", "Say", "Tell", "Speak", "Repeat", "Powtorz")]
        [Remarks("Powtórzę po Tobie!")]
        [Cooldown(5)]
        public async Task Echo([Remainder] string Treść)
        {
            var embed = new EmbedBuilder();

            embed.WithDescription(Treść);
            embed.WithColor(0, 255, 0);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("Wybierz")]
        [Alias("W", "To", "Który", "Ktory", "Pick", "Choose")]
        [Remarks("Wybiorę którąś rzecz!\nSkładnia: ``<p>wybierz <Coś> czy <InneCoś> czy <InniejszeCoś>`` itd...")]
        [Cooldown(5)]
        public async Task PickOne([Remainder]string ToMiędzyCzymMamWybrać)
        {
            string[] options = ToMiędzyCzymMamWybrać.Split(new string[] { " czy " }, StringSplitOptions.None);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            var embed = new EmbedBuilder();

            embed.WithAuthor(Context.User.Username + "!");
            embed.Author.WithIconUrl("http://rs210.pbsrc.com/albums/bb252/Ronnies_Pets/QUESTION%20MARK/QSpinPink_zps9d4b061b.gif~c200");
            embed.WithTitle("Wybieram... :arrow_down:");
            embed.WithDescription(selection);
            embed.WithColor(255, 0, 255);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("SchowajSię"), Alias("SchowajSie", "Schowaj się", "Schowaj sie")]
        [Remarks("Schowam się!")]
        [Cooldown(65, true)]
        public async Task Hide()
        {
            await Context.Channel.SendMessageAsync("OK");
            await Context.Client.SetStatusAsync(UserStatus.Invisible);
            await Task.Delay(60000);
            await Context.Client.SetStatusAsync(UserStatus.Online);
        }

        [Command("Dodaj"), Alias("+", "Dodawanie", "Addition"), Summary("Dodam 2 cyfry!")]
        [Cooldown(5)]
        public async Task AddAsync(float PierwszySkładnik, float DrugiSkładnik)
        {
            await ReplyAsync($"Wynik dodawania {PierwszySkładnik} dodać {DrugiSkładnik} wynosi {PierwszySkładnik + DrugiSkładnik}");
        }

        [Command("Odejmij"), Alias("-", "Odejmowanie", "Subtract"), Summary("Odejmujmę 2 cyfry!")]
        [Cooldown(5)]
        public async Task SubstractAsync(float Odjemna, float Odjemnik)
        {
            await ReplyAsync($"Wynik odejmowania {Odjemna} odjąć {Odjemnik} wynosi {Odjemna - Odjemnik}");
        }

        [Command("Pomnóż"), Alias("*", "Mnożenie", "Multiply"), Summary("Pomnożę 2 cyfry!")]
        [Cooldown(5)]
        public async Task MultiplyAsync(float PierwszyCzynnik, float DrugiCzynnik)
        {
            await ReplyAsync($"Wynik mnożenia {PierwszyCzynnik} razy {DrugiCzynnik} wynosi {PierwszyCzynnik * DrugiCzynnik}");
        }

        [Command("Podziel"), Alias(":", "Dzielenie", "Divide"), Summary("Podzielę 2 cyfry!")]
        [Cooldown(5)]
        public async Task DivideAsync(float Dzielna, float Dzielnik)
        {
            await ReplyAsync($"Wynik dzielenia {Dzielna} przez {Dzielnik} wynosi {Dzielna / Dzielnik}");
        }

        [Command("Kill"), Alias("Kill-chan"), Remarks("\"Zarchiwizuję kanał!\"")]
        [Cooldown(5)]
        public async Task FBI()
        {
            var channel = Context.Guild.GetChannel(Context.Channel.Id);
            await channel.DeleteAsync();
        }

        [Command("Kochasz Mnie?")]
        [Cooldown(15)]
        public async Task DoUKnowMe()
        {
            if (Context.User.Id == 356147668231258113 || Context.User.Id == 515299328617873418)
            {
                var embed = new EmbedBuilder();
                embed.WithTitle("Bardzoooooo :heart_eyes::heart_eyes::heart_eyes::heart_eyes::heart_eyes::heart_eyes::heart_eyes::heart_eyes::heart_eyes::heart_eyes:");
                embed.WithColor(255, 0, 0);

                for (int i = 0; i < 20; i++)
                {
                    await Context.Channel.SendMessageAsync("", embed: embed.Build());
                    await Task.Delay(1000);
                }
            }
            else
            {
                await ReplyAsync("Nie lol");
            }
        }

        [Command("Mem")]
        [Alias("Meme")]
        [Remarks("Wrzucę memeska!")]
        [Cooldown(5)]
        public async Task Meme()
        {
            string[] memes = new string[]
            {
                "materials/memes/Cejrowski.jpg",
                "materials/memes/Co4.jpg",
                "materials/memes/Ehh.jpg",
                "materials/memes/FapANie.png",
                "materials/memes/InstantHappynes.jpg",
                "materials/memes/Luka.jpg",
                "materials/memes/Marsz.jpg",
                "materials/memes/Muzumanie.png",
                "materials/memes/Syn.png",
                "materials/memes/Tyler.png",
                "materials/memes/Wpierdol.png",
                "materials/memes/Chlanie.png"
            };

            Random r;
            r = new Random();
            int memenumber = r.Next(memes.Length);
            string meme = memes[memenumber];

            var msg = await Context.Channel.SendFileAsync(meme);

            await msg.AddReactionAsync(new Emoji("👍"));
            await msg.AddReactionAsync(new Emoji("👎"));
        }

        [Command("Roast")]
        [Alias("Zniszcz", "Zroastuj", "Destroy")]
        [Remarks("Zroastuję kogoś!")]
        [Cooldown(5)]
        public async Task Roast(string użytkownik)
        {
            string[] roasts = new string[]
            {
                "masz twarz, jakby Cię w dzeciństwie karmili z procy.",
                "mówiłeś coś, czy to tylko gnój parował?",
                "nie mów do mnie z bliska, bo Ci sperma z mordy tryska.",
                "ta szpara między zębami, to na żetony?",
                "masz mordę jakbyś na budowie twarzą cegły łapał.",
                "twoja dupa jest jak wózek w supermarkecie, wkładasz 2zł i pchasz ile chcesz.",
                "nie naruszaj mojej przestrzeni osobistej swoim wczorajszym oddechem.",
                "ej no weź, umyj wkońcu te zęby, bo się Sanepid dopierdzieli.",
                "idź się gdzieś przejść, bo jak na Ciebie patrzę, to jestem za aborcją.",
                "lubię żółty, ale Twoje zęby to już przesada.",
                "łokieć pięta, pierdol się.",
                "wkładasz głowę w kibel i myślisz że jesteś Posejdonem...",
                "debilizm, to jedna z Twoich nabytych czy wrodzonych cech?",
                "jak cię kopnę to cię nawet google nie znajdą.",
                "nie patrz się w lustro, bo będziesz miał koszmary w nocy."
            };

            Random r;
            r = new Random();
            int roastnumber = r.Next(roasts.Length);
            string roast = roasts[roastnumber];

            await Context.Channel.SendMessageAsync(użytkownik + " " + roast);
        }

        [Command("KickMe"), Alias("Kick Me")]
        [Remarks("Wywale Cię stąd, zanim to zrobi ktoś mądry!")]
        public async Task KickMe()
        {
            Random r;
            r = new Random();
            int numer = r.Next(80, 100);
            await ReplyAsync("No ej... nie zobaczyłeś jeszcze wszystkich moich możliwości! Do użycia zostało Ci: " + numer + " komend. Użyj ich wszystkich, a obiecuję, że dostaniesz swoją nagrodę" );
        }

        [Command("?"), Alias("ping")]
        [Remarks("Zareaguję, jeśli żyję")]
        public async Task What()
        {
            var msg = await Context.Channel.SendMessageAsync("?");
            await msg.AddReactionAsync(new Emoji("🔥"));
        }
    } 
}
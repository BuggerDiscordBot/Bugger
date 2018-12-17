using System;
using Bugger.Features.Economy;
using Bugger.Features.GlobalAccounts;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using Bugger.Extensions;
using static Bugger.Global;
using static Bugger.Features.Economy.Transfer;
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Bugger.Modules
{
    public class Ekonomia : ModuleBase<MiunieCommandContext>
    {
        [Command("Daily"), Remarks("Wręczę Ci Twoją dzienną nagrodę!")]
        [Alias("Dzienna", "DziennaNagroda", "GetDaily", "Claim", "ClaimDaily")]
        public async Task GetDaily()
        {
            var result = Daily.GetDaily(Context.User.Id);

            if (result.Success)
            {
                await ReplyAsync($"Dostałeś {Constants.DailyMuiniesGain} punktów szczęścia {Context.User.Mention}! Wróć jutro po więcej :kiss:");
            }
            else
            {
                var timeSpanString = string.Format("{0:%h} godzin {0:%m} minut i {0:%s} sekund", result.RefreshTimeSpan);
                await ReplyAsync($"Już Cię dzisiaj uszczęśliwiłem {Context.User.Mention}, nie zaspokajam Cię??? :persevere: \nWróć za {timeSpanString}. :upside_down:");
            }
        }

        [Command("PunktySzczęścia"), Remarks("Pokazuje ile masz Ty (lub oznaczony użytkownik) **Punktów Szczęścia**")]
        [Alias("PSZ", "Szczęście", "Kasa", "Pieniądze", "Waluta", "Money", "Cash")]
        public async Task CheckMiunies()
        {
            var account = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            await ReplyAsync(GetMiuniesReport(account.Miunies, Context.User.Mention));
        }

        [Command("PunktySzczęścia"), Remarks("Pokazuje ile dany użytkownik ma punktów")]
        [Alias("PSZ", "Szczęście", "Kasa", "Pieniądze", "Waluta", "Money", "Cash")]
        public async Task CheckMiuniesOther(IGuildUser Użytkownik)
        {
            var account = GlobalUserAccounts.GetUserAccount(Użytkownik.Id);
            await ReplyAsync(GetMiuniesReport(account.Miunies, Użytkownik.Mention));
        }

        [Command("Topka"), Remarks("Ogłoszę najszczęśliwszych na serwerze!")]
        [Alias("Najszczęśliwsi", "Top", "Najbogatsi", "Top10", "Najszczesliwsi")]
        public async Task ShowRichesPeople(int NumerStrony = 1)
        {
            if (NumerStrony < 1)
            {
                await ReplyAsync("Czy Ty naprawde próbujesz to zrobić? xD ***SRSLY?!***");
                return;
            }

            var guildUserIds = Context.Guild.Users.Select(user => user.Id);
            // Pokaż tylko użytkowników tego serwera
            var accounts = GlobalUserAccounts.GetFilteredAccounts(acc => guildUserIds.Contains(acc.Id));

            const int usersPerPage = 9;
            // Oblicz najwyższą akceptowany numer storny => amount of pages we need to be able to fit all users in them
            // (amount of users) / (how many to show per page + 1) results in +1 page more every time we exceed our usersPerPage  
            var lastPageNumber = 1 + (accounts.Count / (usersPerPage+1));
            if (NumerStrony > lastPageNumber)
            {
                await ReplyAsync($"Nie ma aż tylu stron...\nStrona numer {lastPageNumber} jest ostatnia.");
                return;
            }
            // Sortuj użytkowników malejąco według punktów
            var ordered = accounts.OrderByDescending(acc => acc.Miunies).ToList();

            var embB = new EmbedBuilder()
                .WithTitle("Najszczęśliwsi są:")
                .WithFooter($"Strona {NumerStrony}/{lastPageNumber}");

            // Add fields to the embed with information of users according to the provided page we should show
            // Two conditions because:  1. Only get as many as we want 
            //                          2. The last page might not be completely filled so we have to interrupt early
            NumerStrony--;
            for (var i = 1; i <= usersPerPage && i + usersPerPage * NumerStrony <= ordered.Count; i++)
            {
                // -1 because we take the users non zero based input
                var account = ordered[i - 1 + usersPerPage * NumerStrony];
                var user = Context.Client.GetUser(account.Id);

                //try to give it a medal in cases 1 - 3, if it is not possible just send it with out change
                var contentName = string.Empty;
                if (NumerStrony == 0)
                {
                    switch (i)
                    {
                        case 1:
                            contentName = $"🥇 #{i + usersPerPage * NumerStrony} {user.Username}";
                            break;
                        case 2:
                            contentName = $"🥈 #{i + usersPerPage * NumerStrony} {user.Username}";
                            break;
                        case 3:
                            contentName = $"🥉 #{i + usersPerPage * NumerStrony} {user.Username}";
                            break;
                        default:
                            contentName = $"#{i + usersPerPage * NumerStrony} {user.Username}";
                            break;
                    }
                }
                else
                {
                    contentName = $"#{i + usersPerPage * NumerStrony} {user.Username}";
                }
                embB.AddField(contentName, $"{account.Miunies} Punktów Szczęścia", true);
            }

            await ReplyAsync("", false, embB.Build());
        }

        [Command("Daj")]
        [Remarks("Oddam podaną wartość Twoich Punktów Szczęścia temu, kogo chcesz uszczęśliwić!")]
        [Alias("Oddaj", "Daj", "Przydziel", "Prezent", "DzieńDziecka", "DzienDziecka", "Transferuj", "Give", "Gift", "Transfer")]
        public async Task TransferMinuies(IGuildUser Użytkownik, ulong Ilość)
        {
            // Class name left for readability
            // UserToUser alone doesn't mean much.
            var result = Transfer.UserToUser(Context.User, Użytkownik, Ilość);

            if (result == TransferResult.SelfTransfer)
            {
                await ReplyAsync(":negative_squared_cross_mark: Nie możesz dać samemu sobie:bangbang:\n**i Ty o tym WIESZ**:exclamation:");
            }
            else if (result == TransferResult.TransferToBot)
            {
                await ReplyAsync(":negative_squared_cross_mark: To miłe, ale to **Ja** Ci je dałem... xD");
            }
            else if (result == TransferResult.NotEnoughMiunies)
            {
                var userAccount = GlobalUserAccounts.GetUserAccount(Context.User.Id);
                await ReplyAsync($":negative_squared_cross_mark: Jesteś za mało szczęśliwy, aby zadowolić {Użytkownik.Username}... Masz tylko {userAccount.Miunies} PSZ.");
            }
            else if (result == TransferResult.Success)
            {
                await ReplyAsync($":white_check_mark: {Context.User.Username} poświęcił swoje szczęście dla {Użytkownik.Username} :astonished: Całe {Ilość} punktów szczęścia!");
            }
        }

        public string GetMiuniesReport(ulong PunktySzczęścia, string Użytkownik)
        {
            return $"{Użytkownik} ma **{PunktySzczęścia} punktów szczęścia**! {GetMiuniesCountReaction(PunktySzczęścia, Użytkownik)} \n\n`{GetRandomDidYouKnow()}`";
        }

        [Command("NowaMaszyna"), Remarks("Stworzę nową maszynę losującą, jeśli ta Ci się nie podoba!")]
        [Alias("NM", "NewGane", "NewMachine", "NewSlots")]
        public async Task NewSlot(int IlośćLosowań = 0)
        {
            Global.Slot = new Slot(IlośćLosowań);
            await ReplyAsync("Wygenerowałem nową maszynę! Powodzenia tym razem! :money_mouth::moneybag:");
        }

        [Command("Graj"), Remarks("Uruchomię moją maszynę i będziesz mógł zagrać swoim szczęściem... i je pomnożyć! :game_die::moneybag:")]
        [Alias("Gra", "Hazard", "Game", "Play", "Slot", "Slots")]
        public async Task SpinSlot(uint Ilość)
        {
            if (Ilość < 1)
            {
                await ReplyAsync("Rozumiem, że tylko na tyle Cię stać, ale zachowójmy się poważnie!");
                return;
            }
            var account = GlobalUserAccounts.GetUserAccount(Context.User.Id);
            if (account.Miunies < Ilość)
            {
                await ReplyAsync($"Wybacz, wygląda na to, że jesteś za mało szczęśliwy... Masz tylko {account.Miunies} PSZ.");
                return;
            }

            account.Miunies -= Ilość;
            GlobalUserAccounts.SaveAccounts(Context.User.Id);

            var slotEmojis = Global.Slot.Spin();
            var payoutAndFlavour = Global.Slot.GetPayoutAndFlavourText(Ilość);

            if (payoutAndFlavour.Item1 > 0)
            {
                account.Miunies += payoutAndFlavour.Item1;
                GlobalUserAccounts.SaveAccounts();
            }

            await ReplyAsync(slotEmojis);
            await Task.Delay(1000);
            await ReplyAsync(payoutAndFlavour.Item2);
        }

        [Command("PokażMaszynę"), Remarks("Pokażę aktualną konfigurację maszyny losującej.")]
        [Alias("PM", "Maszyna", "ShowSlot")]
        public async Task ShowSlot()
        {
            await ReplyAsync(string.Join("\n", Global.Slot.GetCylinderEmojis(true)));
        }

        [Command("Sklep"), Alias("Shop"), Remarks("Wkrótce... | **DLC** jak nic xD")]
        public async Task Shop()
        {
            await ReplyAsync("Zbieramy na jego zawartość xD Narazie możemy zaoferować Amie od poniedziałku do środy od 19 Styknie?");
        }
    }
}

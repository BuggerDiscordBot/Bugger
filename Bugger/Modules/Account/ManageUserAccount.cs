using System;
using System.Linq;
using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;
using Bugger.Helpers;
using Bugger.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Bugger.Modules.Account
{
    [Group("Konto"), Alias("U¿ytkownik", "uZYTKOWNIK", "User", "Account")]
    public class ManageUserAccount : ModuleBase<MiunieCommandContext>
    {
        [Command("Info"), Alias("Informacje", "Informations"), Remarks("Poka¿ê tyle, co o tym kimœ wiem.")]
        public async Task AccountInformation(SocketGuildUser user = null)
        {
            user = user ?? (SocketGuildUser) Context.User;

            var userAccount = GlobalUserAccounts.GetUserAccount(user);
            
            var embed = new EmbedBuilder()
                .WithAuthor($"Informacje o {user.Username}", user.GetAvatarUrl())
                .AddField("Do³¹czy³: ", user.JoinedAt.Value.DateTime.ToString())
                .AddField("**Ostatnia wiadomoœæ:**", userAccount.LastMessage.ToString(), true)
                .AddField("**Ostatne Daily:**", userAccount.LastDaily.ToString(), true)
                .AddField("**Punkty Szczêœcia**:", userAccount.Miunies, true)
                .AddField("**Iloœæ prywatnych tagów**:", userAccount.Tags.Count, true)
                .AddField("**Iloœæ aktywnych przypomnieñ**: ", userAccount.Reminders.Count, true)
                .WithColor(0,255,0)
                .WithCurrentTimestamp()
                .WithFooter($"Na zlecenie: {Context.User.Username}")
                .Build();
            
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("HistoriaPoleceñ"), Alias("P", "HP", "Polecenia", "Komendy", "Poka¿HistoriêPoleceñ", "CommandHistory", "ShowCommandHistory", "HistoriaPolecen", "PokazHistoriePolecen"), Remarks("Zwrócê historiê poleceñ danego u¿ytkownika!")]
        public async Task ShowCommandHistory()
        {            
            await Context.Channel.SendMessageAsync(GetCommandHistory(Context.UserAccount));
        }
        
        //Could be in the extended ModuleBase, with a few changes
        private string GetCommandHistory(GlobalUserAccount userAccount)
        {
            var commandHistory = userAccount.CommandHistory.Select(cH => $"{cH.UsageDate.ToString("G")} {cH.Command}");
            return String.Join("\n", commandHistory); //Return the command history separated by line
        }
        
        [Command("Dane"), Alias("MojeDane", "DajMojeDane", "GetMyData", "MyData", "GetAllMyAccountData"), Remarks("Wyœlê plik z tym, co o Tb wiem. (a nawet jeszcze wiêcej)")]
        public async Task GetAccountFile()
        {
            var userFilePath = GlobalUserAccounts.GetAccountFilePath(Context.User.Id);
            if (String.IsNullOrEmpty(userFilePath))
            {
                Context.Channel.SendMessageAsync("Nie znam Ciê, kim jesteœ???");
                return;
            }

            await Context.User.SendFileAsync(userFilePath, $"Masz tu, wszyystko...");
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} **DM**es wys³any!");
        }

        [Command("UsuñWszystkieMojeDane", RunMode = RunMode.Async), Alias("UsunWszystkieMojeDane", "DeleteAllMyAccountData"), Remarks("Jako, i¿ jestem :heart: **___RODO___** :heart:-Friendly, usunê wszystkie zgromadzone Twoje dane.")]
        public async Task DeleteAccount()
        {
            var response = await AwaitMessageYesNo("Jesteœ pewnien? Stracisz wszystko!!!", "Tak, usuñ", "Nie, nie usuwaj");
            if (response is null)
            {
                await Context.Channel.SendMessageAsync($"Zdecydowa³eœ siê ju¿ {Context.User.Mention}?");
            }
            else
            {
                await EvaluateResponse(response, "Tak");
            }
        }

        
        private async Task EvaluateResponse(SocketMessage response, string optionYes)
        {
            var message = "";
            if (response.Content.ToLower().Contains(optionYes.ToLower()))
            {
                message = GlobalUserAccounts.DeleteAccountFile(Context.User.Id)
                    ? "Wymaza³em wszystkie Twoje dan... czekaj... a ktoœ Ty?"
                    : "Nie znam Ciê, wiêc nic nie usunê :smile:";
            }
            else
            {
                message = "Huhh... nigdy Ciê nie zapomnê!";
            }

            await Context.Channel.SendMessageAsync(Context.User.Mention + " " + message);
        }

        private async Task<SocketMessage> AwaitMessageYesNo(string message, string optionYes, string optionNo)
        {
            await Context.Channel.SendMessageAsync(
                $"{Context.User.Mention} {message} \nOdpowiedz `{optionYes}` albo `{optionNo}`");
            var response = await Context.Channel.AwaitMessage(msg => EvaluateResponse(msg, optionYes, optionNo));
            return response;
        }

        private bool EvaluateResponse(SocketMessage arg, params String[] options)
            => options.Any(option => arg.Content.ToLower().Contains(option.ToLower()) && arg.Author == Context.User);
    }
}

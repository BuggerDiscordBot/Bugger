using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bugger.Features.Economy;
using Bugger.Features.RepeatedTasks;
using Discord;
using Discord.WebSocket;
using System.Reflection;

namespace Bugger
{
    public static class Global
    {
        internal static DiscordSocketClient Client { get; set; }
        internal static Dictionary<ulong, string> MessagesIdToTrack { get; set; }
        internal static Random Rng { get; set; } = new Random();
        internal static Slot Slot = new Slot();
        internal static RepeatedTaskHandler TaskHander = new RepeatedTaskHandler();
        internal static readonly String version = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');
        internal static bool Headless = false;
        // Global Helper methods

        internal static string GetRandomDidYouKnow()
        {
            return Constants.RandomBotInfo[Rng.Next(0, Constants.RandomBotInfo.Count)];
        }
        
        public static string ReplacePlacehoderStrings(this string messageString, IGuildUser user = null)
        {
            var result = messageString;
            result = ReplaceGuildUserPlaceholderStrings(result, user);
            result = ReplaceClientPlaceholderStrings(result);
            return result;
        }

        private static string ReplaceGuildUserPlaceholderStrings(string messageString, IGuildUser user)
        {
            if (user == null) return messageString;
            return messageString.Replace("<NazwaUżytkownika>", user.Nickname ?? user.Username)
                .Replace("<OznaczenieUżytkownika>", user.Mention)
                .Replace("<NazwaSerwera>", user.Guild.Name);
        }

        private static string ReplaceClientPlaceholderStrings(string messageString)
        {
            if (Client == null) return messageString;
            return messageString.Replace("<OznaczenieBota>", Client.CurrentUser.Mention)
                .Replace("<DyskryminatorBota>", Client.CurrentUser.Discriminator)
                .Replace("<NazwaBota>", Client.CurrentUser.Username);
        }

        public static string GetMiuniesCountReaction(ulong value, string mention)
        {
            if (value > 100000)
            {
                return $"Matko Bosko, {mention} Ty czitujesz albo jesteś prawdziwym nerdem!!!";
            }
            if (value > 50000)
            {
                return $"Łał {mention} musisz być tutaj często, chcesz mi może coś wyznać czyyyy...?";
            }
            if (value > 20000)
            {
                return $"Dżizys {mention}! To prawie tyle szczęścia, co przy przypadkowym dotknięciu laski 10/10!";
            }
            if (value > 10000)
            {
                return $"Dobra {mention}, pobawiłeś się super, ale oddaj już PSZ ich właścicielowi!";
            }
            if (value > 5000)
            {
                return $"{mention} zaczyna serio być szczęśliwy, może nawet zbyt szczęśliwy...";
            }
            if (value > 2500)
            {
                return $"Uuu dobraa {mention}! Jaszcze trochę szczęścia, i może na chwilę zapomnisz, że jesteś adoptowany.";
            }
            if (value > 1100)
            {
                return $"{mention} musi lubić te całe internety!";
            }
            if (value > 800)
            {
                return $"Uuu, {mention} chyba serio chce się zabawić!";
            }
            if (value > 550)
            {
                return $"No coś coś już zaczynasz szczęśliwieć {mention}!";
            }
            if (value > 200)
            {
                return $"YYYł, {mention}! Mogłeś uprzedzić, że masz tylko tyle... wysłałbym prywatnie... nie przy wszystkich...";
            }
            if (value == 0)
            {
                return $"Tja, {mention} nie ma nic xD.";
            }

            return "Punkty szczęścia w przyszłości będzie można wymieniać na nagrody!!!\nTak, tak...\nSzykujemy płatne DLC:bangbang::bangbang:";
        }

        public static async Task<string> SendWebRequest(string requestUrl)
        {
            using (var client = new HttpClient(new HttpClientHandler()))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Community-Discord-BOT");
                using (var response = await client.GetAsync(requestUrl))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return response.StatusCode.ToString();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        internal static void WriteColoredLine(string text, ConsoleColor color, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}

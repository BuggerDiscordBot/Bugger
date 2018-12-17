using Discord.WebSocket;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Bugger.Configuration;
using Bugger.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Bugger.Features;

namespace Bugger
{
    class Program
    {
        private static DiscordSocketClient _client;
        private static IServiceProvider _serviceProvider;
        private static ApplicationSettings _appSettings;

        private static async Task Main(string[] args)
        {
            _appSettings = new ApplicationSettings(args);
            
            BlackBox.Initialize();

            InversionOfControl.InitializeContainer(_appSettings);

            _client = InversionOfControl.Container.GetInstance<DiscordSocketClient>();

            _serviceProvider = InversionOfControl.Container;

            _serviceProvider.GetRequiredService<DiscordEventHandler>().InitDiscordEvents();
            await _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();

            while (!await AttemptLogin()){}

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static async Task<bool> AttemptLogin()
        {
            try
            {
                await _client.LoginAsync(TokenType.Bot, BotSettings.config.Token);
                return true;
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException == null)
                {
                    Console.WriteLine($"An HTTP Request exception occurred.\nMessage:\n{e.Message}");
                }
                else
                {
                    Global.WriteColoredLine($"An HTTP request ran into a problem:\n{e.InnerException.Message}",
                        ConsoleColor.Red);
                }

                var shouldTryAgain = GetTryAgainRequested();
                if (!shouldTryAgain) Environment.Exit(0);
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine("An exception occurred. Your token might not be configured, or it might be wrong.");

                var shouldTryAgain = GetTryAgainRequested();
                if (!shouldTryAgain) Environment.Exit(0);
                BotSettings.LoadConfig();
                return false;
            }
        }

        private static bool GetTryAgainRequested()
        {
            if (Global.Headless) return false;

            Console.WriteLine("\nDo you want to try again? (y/n)");
            Global.WriteColoredLine("(not trying again closes the application)\n", ConsoleColor.Yellow);

            return Console.ReadKey().Key == ConsoleKey.Y;
        }
    }
}

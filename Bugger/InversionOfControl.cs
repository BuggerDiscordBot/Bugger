using Bugger.Configuration;
using Bugger.Features.Trivia;
using Bugger.Handlers;
using Bugger.Features.Lists;
using Discord.Commands;
using Discord.WebSocket;
using Lamar;
using Bugger.Features.Onboarding;

namespace Bugger
{
    public static class InversionOfControl
    {
        private static Container container;

        public static Container Container
        {
            get
            {
                return GetOrInitContainer();
            }
        }

        private static Container GetOrInitContainer()
        {
            if(container is null)
            {
                InitializeContainer();
            }

            return container;
        }

        public static void InitializeContainer(ApplicationSettings settings = null)
        {
            container = new Container(c =>
            {
                // c.For<UserIssueRepository>().Use<UserIssueDatabaseRepository>();
                // c.ForSingletonOf<ConnectionService>().UseIfNone<DiscordConnectionService>();
                // c.ForSingletonOf<DiscordSocketClient>().UseIfNone<DiscordSocketClient>();
                c.ForSingletonOf<Logger>().UseIfNone<Logger>();
                c.ForSingletonOf<TriviaGames>().UseIfNone<TriviaGames>();
                c.ForSingletonOf<DiscordEventHandler>().UseIfNone<DiscordEventHandler>();
                c.ForSingletonOf<CommandHandler>().UseIfNone<CommandHandler>();
                c.ForSingletonOf<CommandService>().UseIfNone<CommandService>();
                c.ForSingletonOf<DiscordSocketClient>().UseIfNone(DiscordClientFactory.GetBySettings(settings));
                c.ForSingletonOf<ApplicationSettings>().UseIfNone(settings);
                c.ForSingletonOf<IDataStorage>().UseIfNone<JsonDataStorage>();
                c.ForSingletonOf<ListManager>().UseIfNone<ListManager>();
                c.ForSingletonOf<IOnboarding>().UseIfNone<Onboarding>();
                c.ForSingletonOf<Features.Onboarding.Tasks.HelloWorldTask>().UseIfNone<Features.Onboarding.Tasks.HelloWorldTask>();
            });
        }
    }
}

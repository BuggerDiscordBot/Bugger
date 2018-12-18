using Discord;

namespace Bugger.Features.Onboarding.Tasks
{
    public class HelloWorldTask : IOnboardingTask
    {
        private readonly Logger logger;

        public HelloWorldTask(Logger logger)
        {
            this.logger = logger;
        }

        public async void OnJoined(IGuild guild)
        {
            var defaultChannel = await guild.GetDefaultChannelAsync();
            
            if(defaultChannel is null)
            {
                await logger.Log(LogSeverity.Error, "Onboarding > HelloWorldTask", $"Serwer ({guild.Name}) nie ma domyœlnego kana³u.");
                return;
            }
            
            await defaultChannel.SendMessageAsync("Siemka! Jestem **Bugger** :punch::boom: najlepszy polskojêzyczny bot discordowy :exclamation: _(bo jedyny xd)_ Nie chcê siê chwialiæ czy coœ, ale oprócz przedstawienia siê potrafiê te¿ np. reagowaæ na ludzkie komendy! Jeœli tylko bêdziecie gotowi oznaczcie mnie i napiszcie `pomoc` Mi³ej zabawy!");
        }
    }
}

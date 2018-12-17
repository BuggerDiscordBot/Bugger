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
                await logger.Log(LogSeverity.Error, "Onboarding > HelloWorldTask", $"Default channel of a new guild ({guild.Name}) is null.");
                return;
            }
            
            await defaultChannel.SendMessageAsync(":wave: Hello, everyone! I'm Miunie and I'll be here to reply to your human commands. You can use the `help` command to see what I can do.");
        }
    }
}

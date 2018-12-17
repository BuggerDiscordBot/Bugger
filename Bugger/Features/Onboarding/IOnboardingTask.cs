using Discord;

namespace Bugger.Features.Onboarding
{
    public interface IOnboardingTask
    {
        void OnJoined(IGuild guild);
    }
}

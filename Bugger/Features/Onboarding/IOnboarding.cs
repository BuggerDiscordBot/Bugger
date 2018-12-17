using Discord;

namespace Bugger.Features.Onboarding
{
    public interface IOnboarding
    {
        void JoinedGuild(IGuild guild);
    }
}

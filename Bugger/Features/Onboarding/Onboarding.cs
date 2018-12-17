using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bugger.Features.Onboarding.Tasks;
using Discord;

namespace Bugger.Features.Onboarding
{
    public class Onboarding : IOnboarding
    {
        private readonly IEnumerable<IOnboardingTask> tasks;

        public Onboarding()
        {
            tasks = GetOnboardingTasks();
        }

        public void JoinedGuild(IGuild guild)
        {
            foreach(var task in tasks)
            {
                task.OnJoined(guild);
            }
        }

        private static IEnumerable<IOnboardingTask> GetOnboardingTasks()
        {
            var taskType = typeof(IOnboardingTask);

            // Loops through Assemblies and types within them
            // and takes the ones that implement the task interface
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => taskType.IsAssignableFrom(p) && !p.IsInterface)
                .Select(t => (IOnboardingTask)InversionOfControl.Container.GetInstance(t));
        }
    }
}

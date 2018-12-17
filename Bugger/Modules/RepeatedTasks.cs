using System.Threading.Tasks;
using Bugger.Extensions;
using Discord;
using Discord.Commands;


namespace Bugger.Modules
{
    [Group("Zadania"), Remarks("Zarządzanie moimi zadaniami w tle [mining, anti-spam lub anti-raid] **[Tylko dla współtwórców]**.")]
    [Alias("Z", "Zadanie", "Tło", "Task")]
    public class RepeatedTasks : ModuleBase<MiunieCommandContext>
    {
        [Command("")]
        [Alias("Lista", "List", "L")]
        [Remarks("Pokażę listę wszystkich dostępnych zadań w tle!")]
        [RequireOwner]
        public Task ListTasks()
        {
            var embBuilder = new EmbedBuilder();
            embBuilder.WithAuthor("Oto wszystkie zarejestrowane zadania:");
            foreach (var timer in Global.TaskHander.Timers)
            {
                var enabled = timer.Value.Enabled ? "WŁĄCZONE" : "WYŁĄCZONE";
                embBuilder.AddField($"{timer.Key} [{ enabled}]", $"{timer.Value.Interval / 1000}s czasu.", true);
            }

            return ReplyAsync("", false, embBuilder.Build(), null);
        }

        [Command("Start")]
        [Remarks("Rozpocznę zadanie!")]
        [RequireOwner]
        public async Task StartTask([Remainder] string name)
        {
            var success = Global.TaskHander.StartTimer(name);
            var msgString = success ? $"{name} has been started!" : $"{name} is not a task that already exists...";
            await ReplyAsync(msgString);
        }

        [Command("Interwał")]
        [Remarks("Interwał zadań!")]
        [RequireOwner]
        public async Task ChangeIntervalOfTask(int interval, [Remainder] string name)
        {
            var success = Global.TaskHander.ChangeInterval(name, interval);
            var msgString = success ? $"Interval of {name} has been set to {interval/1000} seconds!" : $"{name} is not a task that already exists or the interval you tried to set was too low ({Constants.MinTimerIntervall}ms is the lowest)...";
            await ReplyAsync(msgString);
        }

        [Command("Stop")]
        [Remarks("Zatrzymam zadanie!")]
        [RequireOwner]
        public async Task StopTask([Remainder] string name)
        {
            var success = Global.TaskHander.StopTimer(name);
            var msgString = success ? $"{name} has been stopped!" : $"{name} is not a task that already exists...";
            await ReplyAsync(msgString);
        }

    }
}

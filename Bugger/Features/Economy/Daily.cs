using System;
using Bugger.Features.GlobalAccounts;

namespace Bugger.Features.Economy
{
    public static class Daily
    {
        public struct DailyResult {
            public bool Success;
            public TimeSpan RefreshTimeSpan;
        }

        public static DailyResult GetDaily(ulong userId)
        {
            var account = GlobalUserAccounts.GetUserAccount(userId);
            var difference = DateTime.UtcNow - account.LastDaily.AddDays(1);

            if (difference.TotalHours < 0) return new DailyResult { Success = false, RefreshTimeSpan = difference };

            account.Miunies += Constants.DailyMuiniesGain;
            account.LastDaily = DateTime.UtcNow;
            GlobalUserAccounts.SaveAccounts(userId);
            return new DailyResult { Success = true };
        }
    }
}

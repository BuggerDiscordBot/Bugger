using Bugger.Features.GlobalAccounts;
using Discord;

namespace Bugger.Features.Economy
{
    internal static class Transfer
    {
        internal enum TransferResult { Success, SelfTransfer, TransferToBot, NotEnoughMiunies }

        internal static TransferResult UserToUser(IUser from, IUser to, ulong amount)
        {
            if (from.Id == to.Id) return TransferResult.SelfTransfer;
            
            if (to.Id == Global.Client.CurrentUser.Id ) return TransferResult.TransferToBot;

            var transferSource = GlobalUserAccounts.GetUserAccount(from.Id);

            if (transferSource.Miunies < amount) return TransferResult.NotEnoughMiunies;

            var transferTarget = GlobalUserAccounts.GetUserAccount(to.Id);

            transferSource.Miunies -= amount;
            transferTarget.Miunies += amount;

            GlobalUserAccounts.SaveAccounts(transferSource.Id, transferTarget.Id);

            return TransferResult.Success;
        }
    }
}

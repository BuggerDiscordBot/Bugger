using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Bugger.Preconditions
{
    public class NoSelf : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            var user = value is IUser ? (IUser)value : null;
            if ((user != null) && (context.User.Id == user.Id))
                return Task.FromResult(PreconditionResult.FromError("Nie moge tego zrobić Tobie samemu"));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}

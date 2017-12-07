using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace DoggoBot.Core.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TypeOf : PreconditionAttribute
    {
        private TypeOfUser? ourType { get; set; }

        public TypeOf(TypeOfUser type)
            => ourType = type;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;

            if (ourType != null)
            {
                if (user == null)
                    return Task.FromResult(PreconditionResult.FromError("Command must be used in a Guild"));

                if (ourType == TypeOfUser.GuildModerator)
                    if (user.GuildPermissions.BanMembers || user.GuildPermissions.KickMembers || user.GuildPermissions.ManageMessages)
                        return Task.FromResult(PreconditionResult.FromSuccess());

                if (ourType == TypeOfUser.GuildAdmin)
                    if (user.GuildPermissions.Administrator)
                        return Task.FromResult(PreconditionResult.FromSuccess());

                if (ourType == TypeOfUser.GuildOwner)
                    if (user.Id == user.Guild.OwnerId)
                        return Task.FromResult(PreconditionResult.FromSuccess());

                return Task.FromResult(PreconditionResult.FromError("User does not have the correct permissions."));
            }
            else
                return Task.FromResult(PreconditionResult.FromError("No defined TypeOfUser - Should not see this message"));
        }
    }

    public enum TypeOfUser
    {
        GuildModerator = 0,
        GuildAdmin = 1,
        GuildOwner = 2
    }
}

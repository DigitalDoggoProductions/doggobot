using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using DoggoBot.Core.Services.Configuration.Bot;

namespace DoggoBot.Core.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequiredUserType : PreconditionAttribute
    {
        private TypeOfUser? ourType { get; set; }

        public RequiredUserType(TypeOfUser type)
            => ourType = type;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var guildUser = context.User as IGuildUser;

            if (guildUser == null)
                return Task.FromResult(PreconditionResult.FromError("Command must be used in a guild!"));

            if (ourType == TypeOfUser.GuildModerator)
            {
                if (guildUser.GuildPermissions.BanMembers || guildUser.GuildPermissions.KickMembers || guildUser.GuildPermissions.ManageMessages)
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError($"User requires guild permission(s): `Ban Members` | `Kick Members` | `Manage Messages`"));
            }
            else if (ourType == TypeOfUser.GuildAdmin)
            {
                if (guildUser.GuildPermissions.Administrator)
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError($"User requires guild permission: `Administrator`"));
            }
            else if (ourType == TypeOfUser.GuildOwner)
            {
                if (guildUser.Id == guildUser.Guild.OwnerId)
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError("Only the guild owner can use this command"));
            }
            else if (ourType == TypeOfUser.Doggo)
            {
                if (services.GetRequiredService<BotConfiguration>().Load().OwnerIDs.Contains(guildUser.Id))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError("Only the bot owner can use this command"));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    public enum TypeOfUser
    {
        GuildModerator = 0,
        GuildAdmin = 1,
        GuildOwner = 2,
        Doggo = 3
    }
}

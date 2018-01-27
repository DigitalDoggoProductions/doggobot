using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace DoggoBot.Core.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequiredChannelType : PreconditionAttribute
    {
        private TypeOfChannel? ourType { get; set; }

        public RequiredChannelType(TypeOfChannel type)
            => ourType = type;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (ourType == TypeOfChannel.Guild)
            {
                if (context.Guild == null)
                    return Task.FromResult(PreconditionResult.FromError("You can only use this command in a guild!"));
                else
                    return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else if (ourType == TypeOfChannel.Nsfw)
            {
                if (context.Guild == null)
                    return Task.FromResult(PreconditionResult.FromError("Nsfw commands can only be used in a guild!"));
                else if (!(context.Channel as ITextChannel).IsNsfw)
                    return Task.FromResult(PreconditionResult.FromError("You can only use this command in a Nsfw channel!"));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    public enum TypeOfChannel
    {
        Guild = 0,
        Nsfw = 1
    }
}

using Discord;
using Discord.Commands;

namespace DoggoBot.Core.Models.Context
{
    public class DoggoCommandContext : ICommandContext
    {
        public IDiscordClient Client { get; }
        public IGuild Guild { get; }
        public IMessageChannel Channel { get; }
        public IUser User { get; }
        public IUserMessage Message { get; }

        public bool IsPrivate => Channel is IPrivateChannel;

        public DoggoCommandContext(IDiscordClient client, IUserMessage msg, IUser user = null)
        {
            Client = client;
            Guild = (msg.Channel as IGuildChannel)?.Guild;
            Channel = msg.Channel;
            User = user ?? msg.Author;
            Message = msg;
        }
    }
}

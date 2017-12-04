using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using DoggoBot.Core.Common.Local;
using DoggoBot.Core.Common.Colors;
using DoggoBot.Core.Models.Context;

namespace DoggoBot.Core.Models.Module
{
    public abstract class DoggoModuleBase : ModuleBase<DoggoCommandContext>
    {
        private LocalString currentStrings = new LocalString();
        private Colors colors = new Colors();

        public Task<IUserMessage> ReplyLocalString(string key)
        {
            return Context.Channel.SendMessageAsync(currentStrings.GetLocalString(key));
        }

        public Task<IUserMessage> ReplyLocalString(string key, Color embedColor)
        {
            return Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription(currentStrings.GetLocalString(key)).WithColor(embedColor).Build());
        }

        public Task<IUserMessage> ReplyLocalString(string key, bool randomColor)
        {
            if (randomColor)
                return Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription(currentStrings.GetLocalString(key)).WithColor(colors.RandomColor()).Build());
            else
                return ReplyLocalString(currentStrings.GetLocalString(key));
        }

        public Task<IUserMessage> ReplyEmbed(Embed eb)
        {
            return Context.Channel.SendMessageAsync("", false, eb);
        }
    }
}

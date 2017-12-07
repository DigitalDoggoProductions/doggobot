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

        /// <summary>
        /// Reply with a local string
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <returns>A message with key value</returns>
        public Task<IUserMessage> ReplyLocalString(string key)
        {
            return Context.Channel.SendMessageAsync(currentStrings.GetLocalString(key));
        }

        /// <summary>
        /// Reply with a local string in an embed
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <param name="embedColor">Embed color</param>
        /// <returns>A colored embed with key value</returns>
        public Task<IUserMessage> ReplyLocalString(string key, Color embedColor)
        {
            return Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription(currentStrings.GetLocalString(key)).WithColor(embedColor).Build());
        }

        /// <summary>
        /// Reply with a local string in a randomly colored embed
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <param name="randomColor">Random embed color bool</param>
        /// <returns>A randomly colored embed with key value, or message with key value</returns>
        public Task<IUserMessage> ReplyLocalString(string key, bool randomColor)
        {
            if (randomColor)
                return Context.Channel.SendMessageAsync("", false, new EmbedBuilder().WithDescription(currentStrings.GetLocalString(key)).WithColor(colors.RandomColor()).Build());
            else
                return ReplyLocalString(currentStrings.GetLocalString(key));
        }

        /// <summary>
        /// Send am embed without having to type all that crap before the actual embed
        /// </summary>
        /// <param name="eb">Embed</param>
        /// <returns>Embed to Channel</returns>
        public Task<IUserMessage> ReplyEmbed(Embed eb)
        {
            return Context.Channel.SendMessageAsync("", false, eb);
        }

        /// <summary>
        /// Reply with a reaction to a users message as well as a message response
        /// </summary>
        /// <param name="message">User message</param>
        /// <param name="emoji">Emoji to react with</param>
        /// <param name="response">Message response</param>
        /// <returns>Reaction and Message or Message</returns>
        public Task<IUserMessage> ReplyReactionMessage(IUserMessage message, Emoji emoji, string response)
        {
            if (message.Reactions.ContainsKey(emoji))
                return Context.Channel.SendMessageAsync(response);

            else { message.AddReactionAsync(emoji); return Context.Channel.SendMessageAsync(response); }
        }
    }
}

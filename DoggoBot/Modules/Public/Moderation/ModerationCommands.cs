using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;

namespace DoggoBot.Modules.Public.Moderation
{
    [Name("Moderation")]
    [Summary("Contains moderation commands for server mods and admins")]
    [TypeOf(TypeOfUser.GuildModerator)]
    public class ModerationCommands : DoggoModuleBase
    {
        [Command("kick")]
        [Summary("Kick a user from the server")]
        [Remarks("kick @user <reason>")]
        public async Task KickUserAsync(IGuildUser user, [Remainder] string reason = "No reason provided")
        {
            if (user.Id == Context.Message.Author.Id)
                await ReplyReactionMessage(Context.Message, new Emoji("\U0000274c"), "You can't kick yourself silly :leaves:");
            else { await user.KickAsync(reason); await ReplyReactionMessage(Context.Message, new Emoji("\U00002705"), $"`{user}` was kick from the server.\nReason: {reason}"); }
        }

        [Command("ban")]
        [Summary("Ban a user from the server")]
        [Remarks("ban @user <reason>")]
        public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = "No reason provided")
        {
            if (user.Id == Context.Message.Author.Id)
                await ReplyReactionMessage(Context.Message, new Emoji("\U0000274c"), "You can't ban youself silly :leaves:");
            else { await Context.Guild.AddBanAsync(user, 7, reason); await ReplyReactionMessage(Context.Message, new Emoji("\U00002705"), $"`{user}` was banned from the server.\nReason: {reason}"); }
        }

        [Command("purge")]
        [Summary("Remove an amount of messages from chat")]
        [Remarks("purge <amount>")]
        public async Task PurgeChatAsync(int amount)
        {
            if (amount < 1)
                await ReplyReactionMessage(Context.Message, new Emoji("\U0000274c"), "That is not a valid amount of messages!");

            int lim = (amount < 100) ? amount + 1 : 100;
            var m = (await Context.Channel.GetMessagesAsync(lim).Flatten()).Where(x => DateTime.Now - x.CreatedAt < TimeSpan.FromDays(14));

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(m.Take(amount));
            await Context.Channel.SendMessageAsync($"I deleted `{m.Take(amount).Count()}` messages from the channel! :sparkles:");
        }
    }
}

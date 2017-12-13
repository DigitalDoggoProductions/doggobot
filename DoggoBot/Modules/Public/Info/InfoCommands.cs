using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Common.Colors;

namespace DoggoBot.Modules.Public.Info
{
    [Name("Info")]
    [Summary("Contains the info commands for users")]
    [RequiredChannelType(TypeOfChannel.Guild)]
    public class InfoCommands : DoggoModuleBase
    {
        private readonly Colors ourColors = new Colors();

        [Command("myinfo")]
        [Alias("me", "minfo")]
        [Summary("Display user information about yourself")]
        [Remarks("myinfo")]
        public async Task MyInfoAsync()
        {
            var u = Context.User as IGuildUser;
            await ReplyEmbed(new EmbedBuilder()
                .WithThumbnailUrl(u.GetAvatarUrl())
                .WithColor(ourColors.EmbedPurple)
                .AddField(x => { x.Name = "User Information"; x.Value = string.Join("\n", $"**Name:** `{u.Username}#{u.Discriminator}`", $"**Nickname:** {u.Nickname ?? "No Nickname"}", $"**ID:** {u.Id}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Game and Status"; x.Value = string.Join("\n", $"**Status:** {u.Status}", $"**Game:** {ValidateUserGame(u)}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Joined"; x.Value = string.Join("\n", $"**Joined Discord:** {u.CreatedAt:MM/dd/yyyy}", $"**Joined Server:** {u.JoinedAt:MM/dd/yyyy}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Roles"; x.Value = string.Join("\n", $"**Role Count:** {u.RoleIds.Count - 1}", $"**Role Names:** {string.Join(", ", u.RoleIds.Select(_r => u.Guild.GetRole(_r)).Where(r => r != null).Take(5).Where(r => r.Id != r.Guild.EveryoneRole.Id).Select(r => r.Name))}"); x.IsInline = true; }).Build());
        }

        [Command("userinfo")]
        [Alias("uinfo")]
        [Summary("Display user information about someone else")]
        [Remarks("userinfo @user")]
        public async Task UserInfoAsync(IGuildUser u)
            => await ReplyEmbed(new EmbedBuilder()
                .WithThumbnailUrl(u.GetAvatarUrl())
                .WithColor(ourColors.EmbedPurple)
                .AddField(x => { x.Name = "User Information"; x.Value = string.Join("\n", $"**Name:** `{u.Username}#{u.Discriminator}`", $"**Nickname:** {u.Nickname ?? "No Nickname"}", $"**ID:** {u.Id}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Game and Status"; x.Value = string.Join("\n", $"**Status:** {u.Status}", $"**Game:** {ValidateUserGame(u)}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Joined"; x.Value = string.Join("\n", $"**Joined Discord:** {u.CreatedAt:MM/dd/yyyy}", $"**Joined Server:** {u.JoinedAt:MM/dd/yyyy}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Roles"; x.Value = string.Join("\n", $"**Role Count:** {u.RoleIds.Count - 1}", $"**Role Names:** {string.Join(", ", u.RoleIds.Select(_r => u.Guild.GetRole(_r)).Where(r => r != null).Take(5).Where(r => r.Id != r.Guild.EveryoneRole.Id).Select(r => r.Name))}"); x.IsInline = true; }).Build());

        [Command("serverinfo")]
        [Alias("sinfo")]
        [Summary("Display server information about this server")]
        [Remarks("serverinfo")]
        public async Task ServerInfoAsync()
        {
            var g = Context.Guild;
            var owner = await g.GetOwnerAsync();
            var members = await g.GetUsersAsync();
            var tChannels = await g.GetTextChannelsAsync();
            var vChannels = await g.GetVoiceChannelsAsync();
            var roles = g.Roles.ToArray();

            var eb = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithColor(ourColors.EmbedPurple)
                .AddField(x => { x.Name = "Server"; x.Value = string.Join("\n", $"**Name:** {g.Name}", $"**ID:** {g.Id}", $"**Created At:** {g.CreatedAt:MM/dd/yyyy}", $"**Owner:** {owner.Mention}", $"**Members:** {members.Count}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Channels"; x.Value = string.Join("\n", $"**Text Channels:** {tChannels.Count}", $"**Voice Channels:** {vChannels.Count}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Settings"; x.Value = string.Join("\n", $"**Voice Region:** {g.VoiceRegionId}", $"**MFA Level:** {g.MfaLevel}", $"**Verification Level:** {g.VerificationLevel}", $"**Message Notifications:** {g.DefaultMessageNotifications}"); x.IsInline = true; })
                .AddField(x => { x.Name = "Roles"; x.Value = string.Join("\n", $"**Role Count:** {g.Roles.Count - 1}", $"**Roles:** {string.Join(", ", g.Roles.Take(5).Where(e => e.Id != g.EveryoneRole.Id).Select(r => r.Name))}..."); x.IsInline = true; });

            if (g.Emotes.Any())
                eb.AddField(x => { x.Name = "Emoji"; x.Value = string.Join("\n", $"**Emote Count:** {g.Emotes.Count}", $"**Emotes:** {string.Join(" ", g.Emotes.Take(10).Select(e => $"<:{e.Name}:{e.Id}>"))}..."); x.IsInline = true; });

            await ReplyEmbed(eb.Build());
        }

        /// <summary>
        /// Validate whether or not a user if playing a game
        /// </summary>
        /// <param name="u">IGuildUser to Validate</param>
        /// <returns>String saying their game or message saying no game</returns>
        private string ValidateUserGame(IGuildUser u)
        {
            if (u.Game == null)
                return "Not Playing a Game";
            else
                return u.Game.Value.Name;
        }
    }
}

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Newtonsoft.Json;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;

namespace DoggoBot.Modules.Private.Owner
{
    [Name("Owner")]
    [Summary("Contains private owner commands")]
    [RequiredUserType(TypeOfUser.Doggo)]
    public class OwnerCommands : DoggoModuleBase
    {
        private DiscordSocketClient borkClient;

        public OwnerCommands(DiscordSocketClient client)
            => borkClient = client;

        [Command("guilds")]
        [Summary("Get a list of all the guilds that the bot is in")]
        [Remarks("guilds")]
        public async Task GuildsAsync()
        {
            await Context.Channel.SendMessageAsync(":b:");

            List<string> guildList = new List<string>();

            foreach (SocketGuild g in borkClient.Guilds)
                guildList.Add($"[{borkClient.Guilds.ToList().IndexOf(g)}]: {g.Name} ({g.Id})");

            await (await borkClient.GetApplicationInfoAsync()).Owner.SendFileAsync(
                new MemoryStream(Encoding.Unicode.GetBytes(
                    JsonConvert.SerializeObject(guildList, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }) ?? "Err - NULL")), 
                "Guilds.json");
        }

        [Command("leave")]
        [Summary("Have the bot leave a guild")]
        [Remarks("leave <guild ID>")]
        public async Task LeaveAsync(ulong id)
        {
            IGuild guild = borkClient.GetGuild(id);
            await guild.LeaveAsync();

            await ReplyReactionMessage(Context.Message, new Emoji("\U0001f44c"), $"I have left `{guild.Name}` :ribbon:");
        }

        [Command("peek")]
        [Summary("Recieve a server invite to take a 'peek'")]
        [Remarks("peek <guild ID>")]
        public async Task PeekAsync(ulong id)
        {
            IGuild guild = borkClient.GetGuild(id) as IGuild;

            if (!(await guild.GetUserAsync(borkClient.CurrentUser.Id) as IGuildUser).GuildPermissions.CreateInstantInvite)
                await ReplyReactionMessage(Context.Message, new Emoji("\U0000274c"), "I don't have permission to create invites, rip");
            else
            {
                var guildInvite = await (await guild.GetDefaultChannelAsync()).CreateInviteAsync(86400, 1);
                await Context.Channel.SendMessageAsync($"Here you go: {guildInvite.Url}");
            }
        }

        [Command("echo")]
        [Summary("Have the bot repeat something you say")]
        [Remarks("echo <text>")]
        public async Task EchoAsync([Remainder] string text)
        {
            if (!(await Context.Guild.GetCurrentUserAsync()).GuildPermissions.ManageMessages)
                await Context.Channel.SendMessageAsync("good try <:lul:331630363136884736>");
            else
                await DoMessages(Context.Channel, Context.Message, text);
        }

        private async Task<IUserMessage> DoMessages(IMessageChannel chan, IUserMessage todelete, string tosend)
        {
            var t1 = todelete.DeleteAsync();
            var t2 = chan.SendMessageAsync(tosend);

            await Task.WhenAll(t1, t2);

            return await t2;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;

using Newtonsoft.Json;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Services.Configuration.Bot;

namespace DoggoBot.Modules.Private.Owner
{
    [Name("Owner")]
    [Summary("Contains private owner commands")]
    [RequiredUserType(TypeOfUser.Doggo)]
    public class OwnerCommands : DoggoModuleBase
    {
        private DiscordSocketClient borkClient;
        private BotConfiguration borkConfig;
        private InteractiveService borkInteract;

        public OwnerCommands(DiscordSocketClient client, BotConfiguration config, InteractiveService interact)
        {
            borkClient = client;
            borkConfig = config;
            borkInteract = interact;
        }

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

        [Command("block")]
        [Summary("Block a user from executing commands, helpful for spammers")]
        [Remarks("block")]
        public async Task BlockAsync()
        {
            IUserMessage ourMsg;

            ourMsg = await ReplyAsync("Would you like to remove a block, or add one? [Remove, Add]");

            var mRes = await borkInteract.WaitForMessage(Context.User, ourMsg.Channel, TimeSpan.FromSeconds(60));

            if (mRes != null)
            {
                if (mRes.Content.ToLower() == "remove")
                {
                    ourMsg = await DoMessages(ourMsg.Channel, ourMsg, "Please provide the user Id to remove from the block list.");
                    mRes = await borkInteract.WaitForMessage(mRes.Author, ourMsg.Channel, TimeSpan.FromSeconds(60));

                    if (mRes != null)
                        borkConfig.RemoveUserBlock(Convert.ToUInt64(mRes.Content));

                    await DoMessages(ourMsg.Channel, ourMsg, $"Thank you, I've removed `{borkClient.GetUser(Convert.ToUInt64(mRes.Content))}` from the Block List.");
                }
                else if (mRes.Content.ToLower() == "add")
                {
                    ourMsg = await DoMessages(ourMsg.Channel, ourMsg, "Please provide the user Id to add.");
                    mRes = await borkInteract.WaitForMessage(mRes.Author, ourMsg.Channel, TimeSpan.FromSeconds(60));

                    if (mRes != null)
                    {
                        ulong userId = Convert.ToUInt64(mRes.Content);

                        ourMsg = await DoMessages(ourMsg.Channel, ourMsg, "Is this permanent? [true, false]");
                        mRes = await borkInteract.WaitForMessage(mRes.Author, ourMsg.Channel, TimeSpan.FromSeconds(60));

                        if (mRes != null)
                        {
                            bool Permanent = Convert.ToBoolean(mRes.Content);

                            ourMsg = await DoMessages(ourMsg.Channel, ourMsg, "Please provide a reason for the block.");
                            mRes = await borkInteract.WaitForMessage(mRes.Author, ourMsg.Channel, TimeSpan.FromSeconds(60));

                            if (mRes != null)
                            {
                                borkConfig.AddUserBlock(userId, Permanent, DateTime.Now, mRes.Content);

                                await DoMessages(ourMsg.Channel, ourMsg, $"Thank you, I've added `{borkClient.GetUser(userId)}` to the block list.\n\n`Reason:` {mRes.Content}\n`Permanent:` {Permanent}");
                            }
                        }
                    }
                }
                else
                    await DoMessages(ourMsg.Channel, ourMsg, "Your request was invalid, try again.");
            }
            else
                await DoMessages(ourMsg.Channel, ourMsg, "Your request timed out, try again.");

            //if (borkConfig.BlockedUsers.ContainsKey(Id))
            //    await ReplyAsync("That user has already been blocked :x:");
            //else
            //{
            //    borkConfig.AddUserBlock(Id, isTemp, DateTime.Today, reason);
            //    await ReplyAsync($"Alrighty, I've added `{borkClient.GetUser(Id).Username}` to the block list.\n\n`Reason:` *{reason}*\n`Temp:` *{isTemp}*");
            //}
        }
    }
}

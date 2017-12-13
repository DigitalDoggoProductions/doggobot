using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.Addons.InteractiveCommands;

using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Services.Configuration.Bot;

namespace DoggoBot.Modules.Public.Help
{
    [Name("Help")]
    [Summary("Contains the help commands for users")]
    public class HelpCommands : DoggoModuleBase
    {
        private readonly BotConfiguration borkConfig;
        private readonly CommandService borkCommands;
        private readonly InteractiveService borkInteract;

        public HelpCommands(BotConfiguration config, CommandService commands, InteractiveService interactive)
        {
            borkConfig = config;
            borkCommands = commands;
            borkInteract = interactive;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Help the user with finding modules or commands")]
        [Remarks("help")]
        public async Task HelpAsync()
        {
            string info = null;
            IUserMessage ourMsg = null;

            try { ourMsg = await Context.User.SendMessageAsync("Hello! What can I assist you with today? [Module, Command]"); }
            catch (HttpException ex) when (ex.DiscordCode == 50007) { ourMsg = await ReplyAsync("Hello! What can I assist you with today? [Module, Command]"); }
            finally
            {
                var uRes = await borkInteract.WaitForMessage(Context.User, ourMsg.Channel, TimeSpan.FromSeconds(60));

                if (uRes != null)
                {
                    if (uRes.Content.ToLower() == "module")
                    {
                        foreach (var m in borkCommands.Modules.OrderBy(x => x.Name))
                            info += $"• **{m.Name}:** *{m.Summary ?? "No summary provided"}*\n";

                        ourMsg = await DoMessages(uRes.Channel, ourMsg, $"What module would you like to see commands for?\n\n__List of Modules__\n{info}");
                        uRes = await borkInteract.WaitForMessage(uRes.Author, ourMsg.Channel, TimeSpan.FromSeconds(60));

                        if (uRes != null)
                        {
                            var src = borkCommands.Modules.FirstOrDefault(x => x.Name.ToLower() == uRes.Content.ToLower());

                            if (src == null)
                                await DoMessages(ourMsg.Channel, ourMsg, "**The module name you gave was not valid, please use the help command again.**");
                            else
                            {
                                info = null;
                                foreach (var c in src.Commands)
                                    info += $"• **{c.Name}**\n*Summary: {c.Summary ?? "No summar provided"}*\n\n";

                                await DoMessages(ourMsg.Channel, ourMsg, $"Here you go, glad I could help!\n\n__Module Commands__\n{info}");
                            }
                        }
                        else
                            await DoMessages(ourMsg.Channel, ourMsg, "**Your request timed out, please use the help command again.**");
                    }
                    else if (uRes.Content.ToLower() == "command")
                    {
                        ourMsg = await DoMessages(ourMsg.Channel, ourMsg, "What command would you like to see information about?");
                        uRes = await borkInteract.WaitForMessage(uRes.Author, ourMsg.Channel, TimeSpan.FromSeconds(60));

                        if (uRes != null)
                        {
                            var src = borkCommands.Search(Context, uRes.Content);

                            if (!src.IsSuccess)
                                await DoMessages(ourMsg.Channel, ourMsg, "**The command name you gave was not valid, please use the help command again.**");
                            else
                            {
                                info = null;

                                foreach (var c in src.Commands)
                                    info += $"• **{c.Command.Name} - [Aliases: {string.Join(", ", c.Command.Aliases) ?? "No Aliases"}]**\n*Summary: {c.Command.Summary ?? "No summar provided"}*\n*Usage: {borkConfig.Load().BotPrefix + c.Command.Remarks}*";

                                await DoMessages(ourMsg.Channel, ourMsg, $"Here you go, glad I could help!\n\n__Command Information__\n{info}");
                            }
                        }
                        else
                            await DoMessages(ourMsg.Channel, ourMsg, "**Your request timed out, please use the help command again.**");
                    }
                    else
                        await DoMessages(ourMsg.Channel, ourMsg, "**The option you gave was not valid, please use the help command again.**");
                }
                else
                    await DoMessages(ourMsg.Channel, ourMsg, "**Your request timed out, please use the help command again.**");
            }
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

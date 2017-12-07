using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Addons.InteractiveCommands;

using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Configuration.Bot;

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

            var ourMsg = await Context.Message.Author.SendMessageAsync("Hello! What can I assist you with today? [Module, Command]");
            var uRes = await borkInteract.WaitForMessage(Context.Message.Author, ourMsg.Channel, TimeSpan.FromSeconds(60));

            if (uRes != null)
            {
                if (uRes.Content.ToLower() == "module")
                {
                    foreach (var m in borkCommands.Modules.OrderBy(x => x.Name))
                        info += $"• **{m.Name}:** *{m.Summary ?? "No summary provided"}*\n";

                    await ourMsg.ModifyAsync(x => x.Content = $"What module would you like to see commands for?\n\n__List of Modules__\n{info}");
                    uRes = await borkInteract.WaitForMessage(uRes.Author, uRes.Channel, TimeSpan.FromSeconds(60));

                    if (uRes != null)
                    {
                        var src = borkCommands.Modules.FirstOrDefault(x => x.Name.ToLower() == uRes.Content.ToLower());

                        if (src == null)
                            await ourMsg.ModifyAsync(x => x.Content = "**The module name you gave was not valid, please try again.**");
                        else
                        {
                            info = null;
                            foreach (var c in src.Commands)
                                info += $"• **{c.Name}**\n*Summary: {c.Summary ?? "No summar provided"}*\n\n";

                            await ourMsg.ModifyAsync(x => x.Content = $"Here you go, glad I could help!\n\n__Module Commands__\n{info}");
                        }
                    }
                    else
                        await ourMsg.ModifyAsync(x => x.Content = "**Your request timed out, please try again**");
                }
                else if (uRes.Content.ToLower() == "command")
                {
                    await ourMsg.ModifyAsync(x => x.Content = "What command would you like to see information about?");
                    uRes = await borkInteract.WaitForMessage(uRes.Author, uRes.Channel, TimeSpan.FromSeconds(60));

                    if (uRes != null)
                    {
                        var src = borkCommands.Search(Context, uRes.Content);

                        if (!src.IsSuccess)
                            await ourMsg.ModifyAsync(x => x.Content = "**The command name you gave was not valid, please try again.**");
                        else
                        {
                            info = null;

                            foreach (var c in src.Commands)
                                if (c.Command.Name.ToLower() == uRes.Content.ToLower())
                                    info += $"• **{c.Command.Name} - [Aliases: {string.Join(", ", c.Command.Aliases) ?? "No Aliases"}]**\n*Summary: {c.Command.Summary ?? "No summar provided"}*\n*Usage: {borkConfig.Load().BotPrefix + c.Command.Remarks}*";

                            await ourMsg.ModifyAsync(x => x.Content = $"Here you go, glad I could help!\n\n__Command Information__\n{info}");
                        }
                    }
                    else
                        await ourMsg.ModifyAsync(x => x.Content = "**Your request timed out, please try again**");
                }
                else
                    await ourMsg.ModifyAsync(x => x.Content = "**The option you gave was not valid, please try again.**");
            }
            else
                await ourMsg.ModifyAsync(x => x.Content = "**Your request timed out, please try again**");
        }
    }
}

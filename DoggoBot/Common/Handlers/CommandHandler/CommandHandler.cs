using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using DoggoBot.Core.Models.Context;
using DoggoBot.Core.Models.BlockedUser;
using DoggoBot.Core.Services.Configuration.Bot;

namespace DoggoBot.Common.Handlers.CommandHandler
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient borkClient;
        private readonly CommandService borkCommands;
        private readonly BotConfiguration borkConfig;
        private IServiceProvider borkServices;

        public CommandHandler(DiscordSocketClient client, CommandService commands, BotConfiguration config, IServiceProvider services)
        {
            borkClient = client;
            borkCommands = commands;
            borkConfig = config;
            borkServices = services;
        }

        public async Task InitAsync()
        {
            borkClient.MessageReceived += HandleMessageReceived;
            await borkCommands.AddModulesAsync(Assembly.GetEntryAssembly(), borkServices);
        }

        private async Task HandleMessageReceived(SocketMessage incoming)
        {
            var message = incoming as SocketUserMessage;
            if (message is null) return;

            int argPos = 0;
            var borkContext = new DoggoCommandContext(borkClient, message);

            if (!message.HasStringPrefix(borkConfig.LoadedSecrets.BotPrefix, ref argPos) || message.HasMentionPrefix(borkClient.CurrentUser, ref argPos)) return;

            if (borkConfig.BlockedUsers.ContainsKey(message.Author.Id))
            {
                string ifBlockIsPerm = "";
                BlockedUserModel blockedUser = borkConfig.BlockedUsers[message.Author.Id];

                if (blockedUser.Permanent)
                    ifBlockIsPerm = $"Your block is permanent, please DM {(await borkClient.GetApplicationInfoAsync()).Owner} if you wish to appeal.";
                else
                    ifBlockIsPerm = $"Your block is not permanent, it will be repealed eventually.";

                await borkContext.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    .WithColor(new Color(0, 0, 0))
                    .WithDescription($"**Error: You have been blocked from using commands.**\n`Blocked On:` *{blockedUser.BlockedTime.Date:MM/dd/yyyy}*\n\n`Reason:` *{blockedUser.Reason}*")
                    .WithFooter(x => { x.Text = ifBlockIsPerm; }).Build()); return;
            }

            using (IDisposable enterTyping = borkContext.Channel.EnterTypingState())
            {
                var res = await borkCommands.ExecuteAsync(borkContext, argPos, borkServices);

                if (!res.IsSuccess)
                {
                    if (res.Error == CommandError.UnknownCommand)
                        await borkContext.Channel.SendMessageAsync("Sorry! I didn't understand that command, please try again! :triangular_flag_on_post:");
                    else if (res.Error == CommandError.BadArgCount)
                        await borkContext.Channel.SendMessageAsync("Oh no! You didn't put enough parameters, check help if you need to! :scales:");
                    else
                        await borkContext.Channel.SendMessageAsync(res.ErrorReason);
                }
                enterTyping.Dispose();
            }
        }
    }
}

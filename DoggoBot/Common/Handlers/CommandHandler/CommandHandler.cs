using System;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using DoggoBot.Core.Models.Handler;
using DoggoBot.Core.Models.Context;
using DoggoBot.Core.Services.Configuration.Bot;

namespace DoggoBot.Common.Handlers.CommandHandler
{
    public class CommandHandler : BasicHandlerModel
    {
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services) : base(client, commands, services)
            => borkClient.MessageReceived += HandleMessageReceived;

        private async Task HandleMessageReceived(SocketMessage incoming)
        {
            var message = incoming as SocketUserMessage;
            if (message is null) return;

            int argPos = 0;
            var borkContext = new DoggoCommandContext(borkClient, message);

            if (!message.HasStringPrefix(borkServices.GetRequiredService<BotConfiguration>().Load().BotPrefix, ref argPos) || message.HasMentionPrefix(borkClient.CurrentUser, ref argPos)) return;

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

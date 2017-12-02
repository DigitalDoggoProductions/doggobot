using System;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using DoggoBot.Core.Models.Handler;
using DoggoBot.Core.Models.Context;
using DoggoBot.Core.Configuration.Bot;

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

            var res = await borkCommands.ExecuteAsync(borkContext, argPos, borkServices);

            if (!res.IsSuccess)
                await borkContext.Channel.SendMessageAsync(res.ErrorReason);
        }
    }
}

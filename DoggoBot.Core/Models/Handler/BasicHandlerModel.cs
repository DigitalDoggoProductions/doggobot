using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

namespace DoggoBot.Core.Models.Handler
{
    public class BasicHandlerModel
    {
        protected readonly DiscordSocketClient borkClient;
        protected readonly CommandService borkCommands;
        protected IServiceProvider borkServices;

        public BasicHandlerModel(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            borkClient = client;
            borkCommands = commands;
            borkServices = services;
        }

        public async Task InitAsync()
            => await borkCommands.AddModulesAsync(Assembly.GetEntryAssembly());
    }
}

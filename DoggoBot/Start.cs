using System;
using System.IO;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;

using Microsoft.Extensions.DependencyInjection;

using Tweetinvi;

using DoggoBot.Core.Services.Audio;
using DoggoBot.Core.Services.Configuration.Bot;
using DoggoBot.Common.Handlers.CommandHandler;

namespace DoggoBot
{
    public class Start
    {
        internal static void Main(string[] args)
            => new Start().BeginStartupAsync().GetAwaiter().GetResult();

        private DiscordSocketClient borkClient;
        private CommandService borkCommands;

        private async Task BeginStartupAsync()
        {
            borkClient = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100
            });

            borkCommands = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            });

            var serv = GenerateServices();
            var creds = serv.GetRequiredService<BotConfiguration>().Load();
            await serv.GetRequiredService<CommandHandler>().InitAsync();

            borkClient.Log += Logger;
            borkCommands.Log += Logger;

            await borkClient.LoginAsync(TokenType.Bot, creds.DiscordToken);
            await borkClient.StartAsync();

            // Easiest way to do this, looks crappy though
            Auth.SetUserCredentials(creds.ConsumerKey, creds.ConsumerSecret, creds.AccessToken, creds.AccessTokenSecret);

            await borkClient.SetGameAsync(creds.DiscordGame);

            await Task.Delay(-1);
        }

        private Task Logger(LogMessage msg)
        {
            string logM = $"[{DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss")}] ({msg.Severity}) {msg.Message} {msg.Exception}";

            using (StreamWriter file = new StreamWriter(File.Open(@"Data/Logging/FullLog.txt", FileMode.Append)))
            {
                file.Write(logM + Environment.NewLine);
                Console.WriteLine(logM);
            }

            return Task.CompletedTask;
        }

        private IServiceProvider GenerateServices()
        {
            return new ServiceCollection()
                // Core
                .AddSingleton(borkClient)
                .AddSingleton(borkCommands)
                .AddSingleton<CommandHandler>()
                // Configuration
                .AddSingleton<BotConfiguration>()
                // Other
                .AddSingleton<AudioService>()
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }
    }
}

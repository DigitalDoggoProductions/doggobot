using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Services.Evaluation;
using DoggoBot.Core.Services.Configuration.Bot;

namespace DoggoBot.Modules.Private.Bot
{
    [Name("Bot")]
    [Summary("Contains private bot commands")]
    [RequiredUserType(TypeOfUser.Doggo)]
    public class BotCommands : DoggoModuleBase
    {
        private DiscordSocketClient borkClient;
        private BotConfiguration borkConfig;

        public BotCommands(DiscordSocketClient client, BotConfiguration config)
        {
            borkClient = client;
            borkConfig = config;
        }

        [Command("nickname")]
        [Summary("Change the bots nickname in the current guild")]
        [Remarks("nickname <text>")]
        public async Task NicknameAsync([Remainder] string nick)
        {
            await Context.Message.AddReactionAsync(new Emoji("\U00002705"));

            if (nick == "reset")
                await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = null);
            else
                await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = nick);
        }

        [Command("game")]
        [Summary("Change the bots playing message")]
        [Remarks("game <ture/false> <text>")]
        public async Task GameAsync(bool streaming, [Remainder] string game)
        {
            await Context.Message.AddReactionAsync(new Emoji("\U00002705"));

            if (streaming == false && game == "reset")
                await borkClient.SetGameAsync(borkConfig.LoadedSecrets.DiscordGame);
            else if (streaming == true)
                await borkClient.SetGameAsync(game, "https://twitch.tv/zeplin88", ActivityType.Streaming);
            else
                await borkClient.SetGameAsync(game);
        }

        [Command("avatar")]
        [Summary("Change the bots profile picture on discord")]
        [Remarks("avatar <url>")]
        public async Task AvatarAsync([Remainder] string url)
        {
            using (HttpClient ourHttp = new HttpClient())
            using (var copyTo = File.Create($"Data/Temp/temp.jpg"))
                await (await ourHttp.GetStreamAsync(url)).CopyToAsync(copyTo);

            using (FileStream ourFile = File.OpenRead(@"Data/Temp/temp.jpg")) { await borkClient.CurrentUser.ModifyAsync(x => x.Avatar = new Image(ourFile)); await ourFile.FlushAsync(); }

            foreach (var file in new DirectoryInfo(@"Data/Temp/").GetFiles())
                file.Delete();

            await ReplyReactionMessage(Context.Message, new Emoji("\U00002705"), "I've changed my avatar :dog:");
        }

        [Command("eval")]
        [Summary("Have the bot evaluate some code")]
        [Remarks("eval <code>")]
        public async Task EvalAsync([Remainder] string code)
        {
            await Context.Message.AddReactionAsync(new Emoji("\U00002705"));
            //
            await Context.Channel.SendMessageAsync(await Evaluation.EvalutateAsync(Context, code));
        }
    }
}

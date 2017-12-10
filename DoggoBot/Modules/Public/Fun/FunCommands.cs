using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Discord;
using Discord.Commands;

using Newtonsoft.Json.Linq;

using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Common.Colors;
using DoggoBot.Core.Models.UbranJson;

namespace DoggoBot.Modules.Public.Fun
{
    [Name("Fun")]
    [Summary("Contains the fun commands for users")]
    public class FunCommands : DoggoModuleBase
    {
        private readonly HttpClient ourHttp = new HttpClient();
        private readonly Colors ourColors = new Colors();

        [Command("crush")]
        [Summary("Say who your crush is in chat ;)")]
        [Remarks("crush @user")]
        public async Task CrushAsync(IGuildUser user)
            => await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.EmbedLilac).WithDescription($"Ooooo, How interesting~\nSeems that {Context.Message.Author.Mention} has a crush on you {user.Mention}!").Build());

        [Command("love")]
        [Summary("Say who you love in chat ;)")]
        [Remarks("love @user")]
        public async Task LoveAsync(IGuildUser user)
            => await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.EmbedPink).WithDescription($"Awwww, How sweet~\n{Context.Message.Author.Mention} loves you {user.Mention}!").Build());

        [Command("dog")]
        [Alias("doggo", "borker")]
        [Summary("Send a random dog picture to chat")]
        [Remarks("dog")]
        public async Task DogAsync()
        {
            var url = await ourHttp.GetStringAsync("http://random.dog/woof");

            if (url == null)
                await ReplyReactionMessage(Context.Message, new Emoji("\U0000274c"), "I didn't find any doggos :cry:");
            if (Regex.IsMatch(url, ".gif|.GIF"))
                await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.RandomColor())
                .WithTitle("Check out this adorable doggo")
                .WithDescription("[Click me for gif!](http://random.dog/" + url + ")").Build());
            else if (Regex.IsMatch(url, ".webm|.WEBM|.mp4|.MP4"))
                await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.RandomColor())
                .WithTitle("Check out this adorable doggo")
                .WithDescription("[Click me for video!](http://random.dog/" + url + ")").Build());
            else
                await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.RandomColor())
                .WithTitle("Check out this adorable doggo")
                .WithImageUrl("http://random.dog/" + url).Build());
        }

        [Command("cat")]
        [Alias("catto", "kate")]
        [Summary("Send a random cat picture to chat")]
        [Remarks("cat")]
        public async Task CatAsync()
        {
            var url = JObject.Parse(await ourHttp.GetStringAsync("http://www.random.cat/meow"))["file"].ToString();

            if (url == null)
                await ReplyReactionMessage(Context.Message, new Emoji("\U0000274c"), "I didn't find any doggos :cry:");
            if (Regex.IsMatch(url, ".gif|.GIF"))
                await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.RandomColor())
                .WithTitle("Check out this cute catto")
                .WithDescription($"[Click me for gif!]({url})").Build());
            else if (Regex.IsMatch(url, ".webm|.WEBM|.mp4|.MP4"))
                await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.RandomColor())
                .WithTitle("Check out this cute catto")
                .WithDescription($"[Click me for video!]({url})").Build());
            else
                await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.RandomColor())
                .WithTitle("Check out this cute catto")
                .WithImageUrl(url).Build());
        }

        [Command("urban")]
        [Summary("Search something up on urban dictionary")]
        [Remarks("urban <search term>")]
        public async Task UrbanAsync([Remainder] string query)
        {
            var res = (await ourHttp.GetAsync("http://api.urbandictionary.com/v0/define?term=" + query.Replace(' ', '+')));

            if (!res.IsSuccessStatusCode)
                await ReplyAsync("Oh no! I couldn't chat with the Urban API :question:");
            else
            {
                var data = JToken.Parse(await res.Content.ReadAsStringAsync()).ToObject<UrbanJson>();

                if (data.ResultType == "no_results")
                    await ReplyAsync($"Welp, I'm sorry, but I didn't find anything on UrbanDictionary related to: `{query}` :mag:");
                else if (data.List[0].Definition.Count(char.IsLetter) > 2048)
                    await ReplyAsync("Darn! That definition is too big for discord! :wrench:");
                else
                {
                    var info = data.List[0];

                    await ReplyEmbed(new EmbedBuilder()
                        .WithTitle($"Urban Dictionary - [{info.Word}]")
                        .WithThumbnailUrl("https://i.imgur.com/FmQQkSp.png")
                        .WithDescription(info.Definition)
                        .AddField(x => { x.Name = "Word Example"; x.Value = info.Example ?? "No Word Example"; x.IsInline = true; })
                        .AddField(x => { x.Name = "Word Author"; x.Value = info.Author ?? "No Word Author Available"; x.IsInline = true; })
                        .AddField(x => { x.Name = "Word Rating"; x.Value = $":thumbup: {info.ThumbsUp} - :thumbdown: {info.ThumbsDown}"; x.IsInline = true; })
                        .WithColor(ourColors.EmbedOrange).Build());
                }
            }
        }
    }
}

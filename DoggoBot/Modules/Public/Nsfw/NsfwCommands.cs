using System;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Discord;
using Discord.Commands;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;

namespace DoggoBot.Modules.Public.Nsfw
{
    [Name("Nsfw")]
    [Summary("Contains the nsfw commands for users")]
    [RequiredChannelType(TypeOfChannel.Nsfw)]
    public class NsfwCommands : DoggoModuleBase
    {
        [Command("e621")]
        [Alias("yiff")]
        [Summary("Search up something random or specific tags on e621")]
        [Remarks("e621 <tags>")]
        public async Task e621Async([Remainder] string tags = null)
        {
            string url = await RetrieveImageUrl(tags, 0);

            if (url == null)
                await ReplyAsync("I didn't find any results matching those tags! :pencil:");
            else if (Regex.IsMatch(url, ".gif|.GIF"))
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"e621 - [{tags ?? "Random Result"}]")
                    .WithDescription($"[View GIF]({url})")
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(0, 45, 85)).Build());
            else if (Regex.IsMatch(url, ".webm|.WEBM"))
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"e621 - [{tags ?? "Random Result"}]")
                    .WithDescription($"[View Video]({url})")
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(0, 45, 85)).Build());
            else if (Regex.IsMatch(url, ".swf|.SWF"))
                await ReplyAsync("Result contained a flash file! Unsupported :x:");
            else
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"e621 - [{tags ?? "Random Result"}]")
                    .WithImageUrl(url)
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(0, 45, 85)).Build());
        }

        [Command("rule34")]
        [Summary("Search up something random or specific tags on Rule34")]
        [Remarks("rule34 <tags>")]
        public async Task Rule34Async([Remainder] string tags = null)
        {
            string url = await RetrieveImageUrl(tags, 1);

            if (url == null)
                await ReplyAsync("I didn't find any results matching those tags! :pencil:");
            else if (Regex.IsMatch(url, ".gif|.GIF"))
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"Rule34 - [{tags ?? "Random Result"}]")
                    .WithDescription($"[View GIF]({url})")
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(0, 45, 85)).Build());
            else if (Regex.IsMatch(url, ".webm|.WEBM"))
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"Rule34 - [{tags ?? "Random Result"}]")
                    .WithDescription($"[View Video]({url})")
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(0, 45, 85)).Build());
            else if (Regex.IsMatch(url, ".swf|.SWF"))
                await ReplyAsync("Result contained a flash file! Unsupported :x:");
            else
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"Rule34 - [{tags ?? "Random Result"}]")
                    .WithImageUrl(url)
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(172, 228, 163)).Build());
        }

        [Command("yandere")]
        [Alias("hentai")]
        [Summary("Search up something random or specific tags on Yande.re")]
        [Remarks("yandere <tags>")]
        public async Task YandereAsync([Remainder] string tags = null)
        {
            string url = await RetrieveImageUrl(tags, 2);

            if (url == null)
                await ReplyAsync("I didn't find any results matching those tags! :pencil:");
            else if (Regex.IsMatch(url, ".gif|.GIF"))
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"Yande.re - [{tags ?? "Random Result"}]")
                    .WithDescription($"[View GIF]({url})")
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(0, 45, 85)).Build());
            else if (Regex.IsMatch(url, ".webm|.WEBM"))
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"Yande.re - [{tags ?? "Random Result"}]")
                    .WithDescription($"[View Video]({url})")
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(0, 45, 85)).Build());
            else if (Regex.IsMatch(url, ".swf|.SWF"))
                await ReplyAsync("Result contained a flash file! Unsupported :x:");
            else
                await ReplyEmbed(new EmbedBuilder()
                    .WithTitle($"Yande.re - [{tags ?? "Random Result"}]")
                    .WithImageUrl(url)
                    .WithFooter(x => { x.Text = $"Requested by: {Context.User.Username}"; x.IconUrl = Context.User.GetAvatarUrl(); })
                    .WithColor(new Color(34, 34, 34)).Build());
        }


        private async Task<string> RetrieveImageUrl (string tags, int index)
        {
            string search = null;
            string toReturn = null;

            HttpClient ourHttp = new HttpClient();
            XmlDocument ourXml = new XmlDocument();

            ourHttp.DefaultRequestHeaders.Clear();
            ourHttp.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1");
            ourHttp.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            switch (index)
            {
                case 0: // e621
                    tags = tags?.Trim() ?? "";
                    search = $"http://e621.net/post/index.xml?tags={tags}";
                    break;
                case 1: // Rule34
                    tags = tags?.Replace(' ', '_');
                    search = $"https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=100&tags={tags}";
                    break;
                case 2: // Yande.re
                    tags = tags?.Replace(' ', '_');
                    search = $"https://yande.re/post.xml?limit=100&tags={tags}";
                    break;
            }

            if (index == 0)
            {
                var data = await ourHttp.GetStreamAsync(search);

                ourXml.Load(data);
                XmlNodeList nodes = ourXml.GetElementsByTagName("file_url");

                try { toReturn = nodes[new Random().Next(0, nodes.Count)].InnerText; }
                catch { ourHttp.Dispose(); return null; }

                ourHttp.Dispose();
                return toReturn;
            }
            else if (index == 1 || index == 2)
            {
                var data = await ourHttp.GetStreamAsync(search);

                ourXml.Load(data);
                XmlNode node = ourXml.LastChild.ChildNodes[new Random().Next(0, ourXml.LastChild.ChildNodes.Count)];

                try { toReturn = node.Attributes["file_url"].Value; }
                catch { ourHttp.Dispose(); return null; }

                if (!toReturn.StartsWith("http") || !toReturn.StartsWith("https"))
                    toReturn = string.Format("https:{0}", toReturn);

                ourHttp.Dispose();
                return toReturn;
            }
            else
                return null;
        }
    }
}

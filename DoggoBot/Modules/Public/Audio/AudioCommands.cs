using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using YoutubeSearch;
using YoutubeExplode;
using YoutubeExplode.Models;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Services.Audio;
using DoggoBot.Core.Common.Colors;
using DoggoBot.Core.Models.SongQueue;

namespace DoggoBot.Modules.Public.Audio
{
    [Name("Audio")]
    [Summary("Contains the audio commands for users")]
    [RequiredChannelType(TypeOfChannel.Guild)]
    public class AudioCommands : DoggoModuleBase
    {
        private static Dictionary<ulong, List<SongQueueItem>> guildQueue =
            new Dictionary<ulong, List<SongQueueItem>>();

        private readonly YoutubeClient ourYoutube = new YoutubeClient();
        private readonly Colors ourColors = new Colors();
        private readonly AudioService ourAudio;

        public AudioCommands(AudioService serv)
            => ourAudio = serv;

        [Command("join", RunMode = RunMode.Async)]
        [Alias("connect")]
        [Summary("Have the bot join your current voice channel")]
        [Remarks("join")]
        public async Task JoinAsync()
        {
            if ((Context.User as IVoiceState) == null)
                await ReplyAsync("You must be in a voice channel first!");
            else
                await ourAudio.ConnectAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, Context.Channel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Alias("disconnect")]
        [Summary("Have the bot leave your current voice channel")]
        [Remarks("leave")]
        public async Task LeaveAsync()
            => await ourAudio.DisconnectAsync(Context.Guild, Context.Channel);

        [Command("play")]
        [Summary("Play songs from your queue")]
        [Remarks("play")]
        public async Task PlayAsync()
        {
            List<SongQueueItem> ourList = new List<SongQueueItem>();

            if (guildQueue.ContainsKey(Context.Guild.Id)) { guildQueue.TryGetValue(Context.Guild.Id, out ourList); }

            if ((await Context.Guild.GetCurrentUserAsync()).GuildPermissions.DeafenMembers != true)
                await ReplyAsync("**I do not have permission to deafen myself, please change before I can play music!**");
            else if ((Context.User as IVoiceState).VoiceChannel == null)
                await ReplyAsync("You must be in a voice channel first!");
            else
            {
                await ourAudio.ConnectAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
                await (await Context.Guild.GetCurrentUserAsync() as SocketGuildUser).ModifyAsync(x => x.Deaf = true);

                while (ourList.Count > 0)
                {
                    SongQueueItem playingSong = ourList.First();
                    IGuildUser requester = await Context.Guild.GetUserAsync(playingSong.DiscordRequester);

                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                        .WithColor(ourColors.EmbedYellow)
                        .WithAuthor("Now Playing", playingSong.DiscordRequesterAvatar)
                        .WithDescription($"[{playingSong.SongTitle}]({playingSong.SongURL})")
                        .AddField(x => { x.Name = "Request By"; x.Value = requester.Mention; x.IsInline = true; })
                        .AddField(x => { x.Name = "Duration"; x.Value = playingSong.SongDuration; x.IsInline = true; })
                        .WithFooter(x => { x.Text = ourList.Count != 1 ? $"Next Song: {ourList.ElementAt(1).SongTitle}" : "This is the last song!"; }).Build());

                    await ourAudio.SendAudioAsync(Context.Guild, playingSong.SongId);

                    ourList.RemoveAt(0);
                    guildQueue.Remove(Context.Guild.Id);
                    guildQueue.Add(Context.Guild.Id, ourList);
                    guildQueue.TryGetValue(Context.Guild.Id, out ourList);
                }
            }

            if (ourList.Count == 0)
                await ReplyAsync("**Your song queue is empty, you should add some songs!** :dog:");
        }

        [Command("queue")]
        [Alias("list", "songs")]
        [Summary("List all the songs in your queue")]
        [Remarks("queue <page number>")]
        public async Task QueueAsync(int page = 0)
        {
            List<SongQueueItem> ourList = new List<SongQueueItem>();

            if (guildQueue.ContainsKey(Context.Guild.Id)) { guildQueue.TryGetValue(Context.Guild.Id, out ourList); }

            EmbedBuilder eb = new EmbedBuilder().WithColor(ourColors.EmbedYellow).WithTitle($"{Context.Guild.Name} Song Queue • [Total Songs: {ourList.Count()}]");
            if (ourList.Count > 0)
            {
                if (page <= 0)
                {
                    foreach (SongQueueItem song in ourList.Take(6))
                        eb.AddField(x =>
                        {
                            x.Name = $"{ourList.IndexOf(song)}. {song.SongTitle}";
                            x.Value = $"**Requested By:** {(Context.Guild.GetUserAsync(song.DiscordRequester).GetAwaiter().GetResult().Mention)}\n" +
                            $"**Duration:** {song.SongDuration}\n" +
                            $"[View on YouTube]({song.SongURL})";
                            x.IsInline = true;
                        });
                }
                else
                {
                    if (string.Join("\n", ourList.Skip(6 * page).Take(6)) != "")
                        foreach (SongQueueItem song in ourList.Skip(6 * page).Take(6))
                            eb.AddField(x =>
                            {
                                x.Name = $"{ourList.IndexOf(song)}. {song.SongTitle}";
                                x.Value = $"**Requested By:** {(Context.Guild.GetUserAsync(song.DiscordRequester).GetAwaiter().GetResult().Mention)}\n" +
                                $"**Duration:** {song.SongDuration}\n" +
                                $"[View on YouTube]({song.SongURL})";
                                x.IsInline = true;
                            });
                    else { await ReplyAsync("**That Song Queue page is empty!**"); return; }
                }
            }
            else { await ReplyAsync("**You Song Queue is empty!** :x:"); return; }

            await ReplyEmbed(eb.Build());
        }

        [Command("add")]
        [Alias("submit")]
        [Summary("Add a song to your queue")]
        [Remarks("add <song name or youtube url>")]
        public async Task AddSongQueue([Remainder] string song)
        {
            List<SongQueueItem> ourList = new List<SongQueueItem>();
            Video vidInfo = await GrabVideoInfo(song);

            if (guildQueue.ContainsKey(Context.Guild.Id)) { guildQueue.TryGetValue(Context.Guild.Id, out ourList); }

            ourList.Add(new SongQueueItem()
            {
                SongTitle = vidInfo.Title,
                SongId = vidInfo.Id,
                SongURL = $"https://www.youtube.com/watch?v={vidInfo.Id}",
                SongDuration = vidInfo.Duration,

                DiscordRequester = Context.User.Id,
                DiscordRequesterAvatar = Context.User.GetAvatarUrl()
            });

            guildQueue.Remove(Context.Guild.Id);
            guildQueue.Add(Context.Guild.Id, ourList);

            await ReplyAsync($"`{vidInfo.Title}` **has been added to your queue!**");
        }

        [Command("remove")]
        [Alias("delete")]
        [Summary("Remove a song from your queue")]
        [Remarks("remove <song number>")]
        public async Task RemoveQueueAsync(int element)
        {
            List<SongQueueItem> ourList = new List<SongQueueItem>();

            if (guildQueue.ContainsKey(Context.Guild.Id)) { guildQueue.TryGetValue(Context.Guild.Id, out ourList); }

            if (ourList.Count > 0)
            {
                await Context.Channel.SendMessageAsync($"**Okay, I'll remove** `{ourList.ElementAt(element).SongTitle}` **from the queue**");

                ourList.RemoveAt(element);
                guildQueue.Remove(Context.Guild.Id);
                guildQueue.Add(Context.Guild.Id, ourList);
            }
            else
                await ReplyAsync("**You Song Queue is empty, noting to remove!** :x:");
        }

        /// <summary>
        /// Grab the 'Video' information from a specified url or name
        /// </summary>
        /// <param name="grab">Url or Name to get song from</param>
        /// <returns>Video Data</returns>
        private async Task<Video> GrabVideoInfo(string grab)
        {
            Video returnData = null;

            if (grab.ToLower().Contains("youtube.com"))
                returnData = await ourYoutube.GetVideoAsync(YoutubeClient.ParseVideoId(grab));
            else
                returnData = await ourYoutube.GetVideoAsync(YoutubeClient.ParseVideoId(new VideoSearch().SearchQuery(grab, 1).FirstOrDefault().Url));

            return returnData;
        }
    }
}

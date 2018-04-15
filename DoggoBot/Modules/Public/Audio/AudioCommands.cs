using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.InteractiveCommands;

using YoutubeSearch;
using YoutubeExplode;
using YoutubeExplode.Models;

using DoggoBot.Core.Preconditions;
using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Services.Audio;
using DoggoBot.Core.Common.Colors;
using DoggoBot.Core.Models.GuildSongQueue;

namespace DoggoBot.Modules.Public.Audio
{
    [Name("Audio")]
    [Summary("Contains the audio commands for users")]
    [RequiredChannelType(TypeOfChannel.Guild)]
    public class AudioCommands : DoggoModuleBase
    {
        private static Dictionary<ulong, QueueInfo> guildQueue =
            new Dictionary<ulong, QueueInfo>();

        private readonly Colors ourColors = new Colors();

        private readonly YoutubeClient borkTube = new YoutubeClient();
        private readonly InteractiveService borkInteract;
        private readonly AudioService borkAudio;

        public AudioCommands(AudioService audio, InteractiveService interactive)
        {
            borkAudio = audio;
            borkInteract = interactive;
        }

        [Command("join", RunMode = RunMode.Async)]
        [Alias("connect")]
        [Summary("Have the bot join your current voice channel")]
        [Remarks("join")]
        public async Task JoinAsync()
        {
            if ((Context.User as IVoiceState) == null)
                await ReplyAsync("You must be in a voice channel first!");
            else
                await borkAudio.ConnectAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel, Context.Channel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Alias("disconnect")]
        [Summary("Have the bot leave your current voice channel")]
        [Remarks("leave")]
        public async Task LeaveAsync()
            => await borkAudio.DisconnectAsync(Context.Guild, Context.Channel);

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play songs from your queue")]
        [Remarks("play")]
        public async Task PlayAsync()
        {
            QueueInfo curQueue = new QueueInfo();

            if (guildQueue.ContainsKey(Context.Guild.Id))
                guildQueue.TryGetValue(Context.Guild.Id, out curQueue);

            if (curQueue.Songs.Count == 0) { await ReplyAsync("**Your queue is empty, you should add some songs!**"); return; }

            if ((await Context.Guild.GetCurrentUserAsync()).GuildPermissions.DeafenMembers != true)
                await ReplyAsync("**I do not have permission to deafen myself, please change before I can play music!**");
            else if ((Context.User as IVoiceState).VoiceChannel == null)
                await ReplyAsync("**You must be in a voice channel first!**");
            else
            {
                await borkAudio.ConnectAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
                await (await Context.Guild.GetCurrentUserAsync() as SocketGuildUser).ModifyAsync(x => x.Deaf = true);

                while (curQueue.Songs.Count > 0)
                {
                    curQueue.NowPlaying = curQueue.Songs.First();

                    await borkAudio.SendAudioAsync(Context.Guild, curQueue.Songs.First().SongId);

                    if (curQueue.loopSongs == true)
                    {
                        curQueue.NowPlaying = null;

                        Song toMove = curQueue.Songs.ElementAt(0);
                        curQueue.Songs.RemoveAt(0);
                        curQueue.Songs.Add(toMove);

                        guildQueue.Remove(Context.Guild.Id);
                        guildQueue.Add(Context.Guild.Id, curQueue);
                        guildQueue.TryGetValue(Context.Guild.Id, out curQueue);
                    }
                    else if (curQueue.loopSongs == false)
                    {
                        curQueue.NowPlaying = null;

                        curQueue.Songs.RemoveAt(0);

                        guildQueue.Remove(Context.Guild.Id);
                        guildQueue.Add(Context.Guild.Id, curQueue);
                        guildQueue.TryGetValue(Context.Guild.Id, out curQueue);
                    }
                }
            }
        }

        [Command("nowplaying")]
        [Alias("np")]
        [Summary("Display the currently playing song")]
        [Remarks("nowplaying")]
        public async Task NowPlayingAsync()
        {
            QueueInfo curQueue = new QueueInfo();

            if (guildQueue.ContainsKey(Context.Guild.Id))
                guildQueue.TryGetValue(Context.Guild.Id, out curQueue);

            if (curQueue.NowPlaying == null)
                await ReplyAsync($"**There is currently no song playing!**");
            else if (curQueue.NowPlaying != null)
                await ReplyEmbed(
                    new EmbedBuilder()
                    .WithThumbnailUrl("https://i.imgur.com/uLfl1pB.png")
                    .WithColor(ourColors.EmbedPurple)
                    .WithTitle("Now Playing")
                    .WithDescription($"**{curQueue.NowPlaying.SongTitle}**\n\n*Duration: {curQueue.NowPlaying.SongDuration}*\n*Requested by: {curQueue.NowPlaying.DiscordRequester.Mention}*\n\n[View on YouTube]({curQueue.NowPlaying.SongURL})")
                    .Build());
        }

        [Command("loop")]
        [Alias("repeat")]
        [Summary("Enable or Disable repeating the songs in your queue")]
        [Remarks("loop")]
        public async Task LoopAsync()
        {
            QueueInfo curQueue = new QueueInfo();

            if (guildQueue.ContainsKey(Context.Guild.Id))
                guildQueue.TryGetValue(Context.Guild.Id, out curQueue);

            if (curQueue.loopSongs == false)
            {
                curQueue.loopSongs = true;
                await ReplyAsync("**Neato, I've enabled looping for your queue!**");
            }
            else if (curQueue.loopSongs == true)
            {
                curQueue.loopSongs = false;
                await ReplyAsync("**Neato, I've disabled looping for your queue!**");
            }

            guildQueue.Remove(Context.Guild.Id);
            guildQueue.Add(Context.Guild.Id, curQueue);
        }

        [Command("queue")]
        [Alias("list", "songs")]
        [Summary("List all the songs in your queue")]
        [Remarks("queue <page number>")]
        public async Task QueueAsync(int page = 0)
        {
            QueueInfo curQueue = new QueueInfo();
            string niceStuff = "";

            if (guildQueue.ContainsKey(Context.Guild.Id))
                guildQueue.TryGetValue(Context.Guild.Id, out curQueue);

            if (curQueue.Songs.Count > 0)
            {
                if (page <= 0)
                    foreach (Song song in curQueue.Songs.Take(6))
                        niceStuff += $"**{curQueue.Songs.IndexOf(song)}.** [{song.SongTitle}]({song.SongURL})\n*• Duration: {song.SongDuration} | Request by: {song.DiscordRequester.Mention}*\n\n";
                else
                {
                    if (string.Join("\n", curQueue.Songs.Skip(6 * page).Take(6)) != "")
                        foreach (Song song in curQueue.Songs.Take(6))
                            niceStuff += $"**{curQueue.Songs.IndexOf(song)}.** [{song.SongTitle}]({song.SongURL})\n*• Duration: {song.SongDuration} | Request by: {song.DiscordRequester.Mention}*\n\n";
                    else { await ReplyAsync("**That page is empty!**"); return; }
                }
            }
            else { await ReplyAsync("**Your queue is empty!**"); return; }

            await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.EmbedLilac).WithTitle($"{Context.Guild.Name} Song Queue").WithDescription(niceStuff).Build());
        }

        [Command("add")]
        [Alias("submit")]
        [Summary("Add a song to your queue")]
        [Remarks("add <song name or youtube url>")]
        public async Task AddSongQueue([Remainder] string query)
        {
            QueueInfo curQueue = new QueueInfo();

            if (guildQueue.ContainsKey(Context.Guild.Id))
                guildQueue.TryGetValue(Context.Guild.Id, out curQueue);

            if (query.ToLower().Contains("youtube.com"))
            {
                Video grab = await borkTube.GetVideoAsync(YoutubeClient.ParseVideoId(query));

                curQueue.Songs.Add(new Song()
                {
                    SongTitle = grab.Title,
                    SongId = grab.Id,
                    SongURL = grab.GetUrl(),
                    SongDuration = grab.Duration,
                    DiscordRequester = Context.Message.Author
                });

                guildQueue.Remove(Context.Guild.Id);
                guildQueue.Add(Context.Guild.Id, curQueue);

                await ReplyAsync($"Awesome, I added `{grab.Title}` to your queue");
            }
            else
            {
                IEnumerable<VideoInformation> searchResults = new VideoSearch().SearchQuery(query, 1).Take(10);

                var borkMsg = await Context.Channel.SendMessageAsync($"***- Please respond with the song number you want -***\n\n{OrganizeList(searchResults)}");
                var uRes = await borkInteract.WaitForMessage(Context.Message.Author, borkMsg.Channel, TimeSpan.FromSeconds(60));

                if (int.TryParse(uRes.Content, out int result))
                {
                    if (result >= 10)
                        await DoMessages(borkMsg.Channel, borkMsg, "**The song number you gave was not listed, please try again.**");
                    else
                    {
                        Video chosen = await borkTube.GetVideoAsync(YoutubeClient.ParseVideoId(searchResults.ElementAt(result).Url));

                        curQueue.Songs.Add(new Song()
                        {
                            SongTitle = chosen.Title,
                            SongId = chosen.Id,
                            SongURL = chosen.GetUrl(),
                            SongDuration = chosen.Duration,
                            DiscordRequester = uRes.Author
                        });

                        guildQueue.Remove(Context.Guild.Id);
                        guildQueue.Add(Context.Guild.Id, curQueue);

                        await DoMessages(borkMsg.Channel, borkMsg, $"Awesome, I added `{chosen.Title}` to your queue");
                    }
                }
                else
                    await DoMessages(borkMsg.Channel, borkMsg, "**The response you gave was not valid, please try again.**");
            }
        }

        [Command("remove")]
        [Alias("delete")]
        [Summary("Remove a song from your queue")]
        [Remarks("remove <song number>")]
        public async Task RemoveQueueAsync(int element)
        {
            QueueInfo curQueue = new QueueInfo();

            if (guildQueue.ContainsKey(Context.Guild.Id))
                guildQueue.TryGetValue(Context.Guild.Id, out curQueue);

            if (curQueue.Songs.Count > 0)
            {
                await Context.Channel.SendMessageAsync($"**Cool, I'll go ahead and remove** `{curQueue.Songs.ElementAt(element).SongTitle}` **from your queue**");

                curQueue.Songs.RemoveAt(element);
                guildQueue.Remove(Context.Guild.Id);
                guildQueue.Add(Context.Guild.Id, curQueue);
            }
            else
                await ReplyAsync("**You queue is empty, nothing to remove!**");
        }

        private string OrganizeList(IEnumerable<VideoInformation> videos)
        {
            string toReturn = "";

            foreach (var vid in videos.ToList())
                toReturn += $"{videos.ToList().IndexOf(vid)}. {vid.Title}\n";

            return toReturn;
        }
    }
}

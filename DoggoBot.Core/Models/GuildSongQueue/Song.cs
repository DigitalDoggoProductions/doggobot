using System;

using Discord;

namespace DoggoBot.Core.Models.GuildSongQueue
{
    public class Song
    {
        // Song Information
        public string SongTitle { get; set; }
        public string SongId { get; set; }
        public string SongURL { get; set; }
        public TimeSpan SongDuration { get; set; }

        // Discord Information
        public IUser DiscordRequester { get; set; }
    }
}

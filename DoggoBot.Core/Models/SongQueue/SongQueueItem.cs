using System;

namespace DoggoBot.Core.Models.SongQueue
{
    public class SongQueueItem
    {
        // Song Information
        public string SongTitle { get; set; }
        public string SongId { get; set; }
        public string SongURL { get; set; }
        public TimeSpan SongDuration { get; set; }

        // Discord Information
        public ulong DiscordRequester { get; set; }
        public string DiscordRequesterAvatar { get; set; }
    }
}

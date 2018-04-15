using System.Collections.Generic;

namespace DoggoBot.Core.Models.GuildSongQueue
{
    public class QueueInfo
    {
        // Queue Information
        public bool loopSongs { get; set; } = false;
        public Song NowPlaying { get; set; }

        // Song List
        public List<Song> Songs { get; set; } = new List<Song>();
    }
}

using System;

namespace DoggoBot.Core.Models.BlockedUser
{
    public class BlockedUserModel
    {
        public string Reason { get; set; }
        public bool Permanent { get; set; }
        public DateTime BlockedTime { get; set; }
    }
}

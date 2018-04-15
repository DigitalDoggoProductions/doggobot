using System.Collections.Generic;

using Newtonsoft.Json;

namespace DoggoBot.Core.Models.Secrets
{
    public class SecretsModel
    {
        [JsonProperty("token")]
        public string DiscordToken { get; set; }

        [JsonProperty("game")]
        public string DiscordGame { get; set; }

        [JsonProperty("prefix")]
        public string BotPrefix { get; set; }

        [JsonProperty("bot-perms")]
        public int BotPermissions { get; set; }

        [JsonProperty("owner-ids")]
        public List<ulong> OwnerIDs { get; set; }
    }
}

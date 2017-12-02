using Newtonsoft.Json;

namespace DoggoBot.Core.Models.Credentials
{
    public class CredentialsModel
    {
        [JsonProperty("token")]
        public string DiscordToken { get; set; }

        [JsonProperty("game")]
        public string DiscordGame { get; set; }

        [JsonProperty("prefix")]
        public string BotPrefix { get; set; }
    }
}

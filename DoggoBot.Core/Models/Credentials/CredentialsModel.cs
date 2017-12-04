using System.Collections.Generic;

using Newtonsoft.Json;

namespace DoggoBot.Core.Models.Credentials
{
    public class CredentialsModel
    {
        // Discord Section

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

        // Twitter Section

        [JsonProperty("consumer_key")]
        public string ConsumerKey { get; set; }
        [JsonProperty("consumer_secret")]
        public string ConsumerSecret { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("access_token_secret")]
        public string AccessTokenSecret { get; set; }

        [JsonProperty("tweet_count")]
        public int TweetCount { get; set; }
    }
}

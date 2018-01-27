using System.Collections.Generic;

using Newtonsoft.Json;

namespace DoggoBot.Core.Models.UbranJson
{
    public class ListOfThings
    {
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("word")]
        public string Word { get; set; }
        [JsonProperty("definition")]
        public string Definition { get; set; }
        [JsonProperty("example")]
        public string Example { get; set; }
        [JsonProperty("thumbs_up")]
        public int ThumbsUp { get; set; }
        [JsonProperty("thumbs_down")]
        public int ThumbsDown { get; set; }
    }

    public class UrbanJson
    {
        [JsonProperty("result_type")]
        public string ResultType { get; set; }
        [JsonProperty("list")]
        public List<ListOfThings> List { get; set; }
    }
}

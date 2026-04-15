using Newtonsoft.Json;

namespace PlayMaker.Models
{
    public class TopScorer
    {
        [JsonProperty("rank")]
        public string Id { get; set; }

        [JsonProperty("play")]
        public string play { get; set; }

        [JsonProperty("goals")]
        public string goals { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }
}

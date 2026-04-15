using Newtonsoft.Json;

namespace PlayMaker.Models
{
    public class Standings
    {
        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonProperty("lose")]
        public string Lose { get; set; }

        [JsonProperty("win")]
        public string Win { get; set; }

        [JsonProperty("play")]
        public string Play { get; set; }

        [JsonProperty("point")]
        public string Point { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }
    }
}

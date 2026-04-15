using Newtonsoft.Json;

namespace PlayMaker.Models.Top10
{
    public class ApiResponse
    {
        [JsonProperty("data")]
        public Dictionary<int, Player> Data { get; set; }
    }
}

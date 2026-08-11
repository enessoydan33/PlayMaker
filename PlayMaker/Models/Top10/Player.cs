using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PlayMaker.Models.Top10
{
    public class Player
    {
        // Legacy JSON property name kept for Market deserialization compatibility.
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("Path")]
        public string Path { get; set; }

        [JsonProperty("Count")]
        public int Count { get; set; }

        // Yeni eklenen futbolcu bilgileri
        [JsonProperty("playerImage")]
        public string PlayerImage { get; set; }

        [JsonProperty("playerName")]
        public string PlayerName { get; set; }

        [JsonProperty("playerFullName")]
        public string PlayerFullName { get; set; }

        [JsonProperty("birthplace")]
        public string Birthplace { get; set; }

        [JsonProperty("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("playerShirtNumber")]
        public string PlayerShirtNumber { get; set; }

        [JsonProperty("birthplaceCountry")]
        public string BirthplaceCountry { get; set; }

        [JsonProperty("birthplaceCountryImage")]
        public string BirthplaceCountryImage { get; set; }

        [JsonProperty("age")]
        public string Age { get; set; }

        [JsonProperty("playerMainPosition")]
        public string playerMainPosition{ get; set; }

        [JsonProperty("club")]
        public string Club { get; set; }

        [JsonProperty("marketValue")]
        public string MarketValue { get; set; }
    }
}

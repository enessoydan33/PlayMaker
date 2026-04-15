using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;


namespace PlayMaker.Api
{
    public class PlayerSearchServices
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiHost;
        private readonly string _apiKey;

        public PlayerSearchServices(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiHost = configuration["TransfermarktApi:Host"] ?? "transfermarkt-db.p.rapidapi.com";
            _apiKey = configuration["TransfermarktApi:Key"] ?? "";
        }

        public async Task<JToken> SearchPlayer(string query)
        {
            var safeQuery = Uri.EscapeDataString(query ?? "");
            string[] candidateUrls =
            {
                $"https://{_apiHost}/v1/search/quick-search?locale=US&query={safeQuery}",
                $"https://{_apiHost}/search/quick-search?locale=US&query={safeQuery}",
            };
            
            HttpResponseMessage? response = null;
            string? lastError = null;

            foreach (var url in candidateUrls)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("x-rapidapi-host", _apiHost);
                request.Headers.Add("x-rapidapi-key", _apiKey);

                response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    lastError = null;
                    break;
                }

                var body = await response.Content.ReadAsStringAsync();
                var snippet = body == null ? "" : (body.Length > 300 ? body.Substring(0, 300) : body);
                lastError = $"HTTP {(int)response.StatusCode} {response.StatusCode} - {response.ReasonPhrase}. Body: {snippet}";
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Search API failed. Last: {lastError}");
                return null;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var jObj = JObject.Parse(jsonResponse);
            var players = jObj["data"]?["players"];
          
            return players;
        }



        public async Task<string> GetPlayerIdByName(string playerName)
        {
            var players = await SearchPlayer(playerName);

            if (players == null || !players.HasValues)
            {
                Console.WriteLine("⚠️ Oyuncu bulunamadı.");
                return null;
            }

            // İlk eşleşen oyuncunun id'si
            var firstPlayer = players.First;
            string playerId = firstPlayer?["id"]?.ToString();

            return playerId;
        }



    }

}

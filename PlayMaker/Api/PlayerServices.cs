using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PlayMaker.Models.Top10;
using System.Net.Http;

namespace PlayMaker.Api
{
    public class PlayerServices
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _cache;

        // Cache keys
        private const string RateLimitedKey = "market:rapidapi:rate_limited";

        public PlayerServices(HttpClient client, IMemoryCache cache, IConfiguration configuration)
        {
            _client = client;
            _cache = cache;

            // Ensure headers exist (avoid duplicates)
            if (!_client.DefaultRequestHeaders.Contains("x-rapidapi-key"))
                _client.DefaultRequestHeaders.Add("x-rapidapi-key", configuration["TransfermarktApi:Key"] ?? "");
            if (!_client.DefaultRequestHeaders.Contains("x-rapidapi-host"))
                _client.DefaultRequestHeaders.Add("x-rapidapi-host", configuration["TransfermarktApi:Host"] ?? "transfermarkt-db.p.rapidapi.com");
        }

        // Returns debug errors so UI can show whether RapidAPI is failing (401/429/etc.)
        public async Task<List<string>> UpdatePlayerData(ApiResponse response)
        {
            var errors = new List<string>();
            if (response == null || response.Data == null || response.Data.Count == 0)
            {
                Console.WriteLine("[PlayerServices] ApiResponse boş, güncellenecek oyuncu yok.");
                errors.Add("[PlayerServices] ApiResponse boş, güncellenecek oyuncu yok.");
                return errors;
            }

            // If we recently got 429, don't spam the API on every page load.
            if (_cache.TryGetValue(RateLimitedKey, out _))
            {
                errors.Add("RapidAPI rate limited (429). Biraz bekleyip tekrar dene (cache throttling aktif).");
                return errors;
            }

            foreach (var player in response.Data.Values.Take(5))
            {
                try
                {
                    var cacheKey = $"market:playerProfile:{player.ID}";
                    if (!_cache.TryGetValue(cacheKey, out Player? updatedPlayer) || updatedPlayer == null)
                    {
                        Console.WriteLine($"[PlayerServices] Fetching player profile, playerId={player.ID}");
                        var result = await GetPlayerData(player.ID);
                        updatedPlayer = result.player;

                        if (updatedPlayer != null)
                        {
                            // Cache player profile for 6 hours to reduce API calls.
                            _cache.Set(cacheKey, updatedPlayer, TimeSpan.FromHours(6));
                        }
                        else
                        {
                            // If rate limited, throttle further calls for 2 minutes.
                            if (result.error != null && result.error.Contains("429"))
                            {
                                _cache.Set(RateLimitedKey, true, TimeSpan.FromMinutes(2));
                                // Stop the loop immediately; don't spam more requests.
                                errors.Add("RapidAPI 429 alındı; kalan oyuncular için istek durduruldu.");
                                break;
                            }
                            Console.WriteLine($"[PlayerServices] API'den veri gelmedi, playerId={player.ID}");
                            errors.Add($"playerId={player.ID}: {result.error ?? "API'den veri gelmedi"}");
                            continue;
                        }
                    }

                    if (updatedPlayer != null)
                    {
                        player.PlayerName = updatedPlayer.PlayerName;
                        player.playerMainPosition = updatedPlayer.playerMainPosition;
                        player.Club = updatedPlayer.Club;
                        player.Age = updatedPlayer.Age;
                        player.MarketValue = updatedPlayer.MarketValue;
                        player.PlayerImage = updatedPlayer.PlayerImage;
                        player.BirthplaceCountry = updatedPlayer.BirthplaceCountry;
                        player.BirthplaceCountryImage = updatedPlayer.BirthplaceCountryImage;
                    }
                    else
                    {
                        Console.WriteLine($"[PlayerServices] API'den veri gelmedi, playerId={player.ID}");
                        errors.Add($"playerId={player.ID}: API'den veri gelmedi");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PlayerServices] Player {player.ID} güncellenirken hata: {ex.Message}");
                    errors.Add($"playerId={player.ID}: exception: {ex.Message}");
                    // Hata olsa bile diğer oyuncular için devam et
                }
            }

            return errors;
        }


        private async Task<(Player? player, string? error)> GetPlayerData(int playerId)
        {
            try
            {
                // Providers vary: some have /v1, some don't. Try a tiny set of candidates.
                // IMPORTANT: keep this list small to avoid triggering 429.
                var candidateUrls = new[]
                {
                    $"https://transfermarkt-db.p.rapidapi.com/v1/players/profile?locale=US&player_id={playerId}",
                    $"https://transfermarkt-db.p.rapidapi.com/players/profile?locale=US&player_id={playerId}",
                };

                HttpResponseMessage? response = null;
                string? lastMsg = null;

                foreach (var url in candidateUrls)
                {
                    Console.WriteLine($"[PlayerServices] GET {url}");
                    response = await _client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        lastMsg = null;
                        break;
                    }

                    var body = await response.Content.ReadAsStringAsync();
                    var snippet = body == null ? "" : (body.Length > 300 ? body.Substring(0, 300) : body);
                    lastMsg = $"HTTP {(int)response.StatusCode} {response.StatusCode}. Body: {snippet}";
                    Console.WriteLine($"[PlayerServices] {lastMsg}");

                    // If rate limited, stop immediately.
                    if ((int)response.StatusCode == 429)
                    {
                        _cache.Set(RateLimitedKey, true, TimeSpan.FromMinutes(2));
                        return (null, lastMsg);
                    }

                    // If endpoint doesn't exist, try next candidate; otherwise don't spam.
                    if ((int)response.StatusCode != 404)
                    {
                        return (null, lastMsg);
                    }
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    return (null, lastMsg ?? "Profile endpoint başarısız.");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject jsonData = JObject.Parse(jsonResponse);
                var playerToken = jsonData["data"]?["playerProfile"];

                if (playerToken == null || playerToken.Type == JTokenType.Null)
                {
                    Console.WriteLine("[PlayerServices] playerProfile alanı bulunamadı.");
                    return (null, "playerProfile alanı bulunamadı.");
                }

                return (JsonConvert.DeserializeObject<Player>(playerToken.ToString()), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayerServices] GetPlayerData hata: {ex.Message}");
                return (null, ex.Message);
            }
        }
    }
}

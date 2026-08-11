using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using PlayMaker.Models;
using PlayMaker.Models.Top10;

namespace PlayMaker.Api
{
    public class SofaScoreService
    {
        public const int SuperLigTournamentId = 52;

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SofaScoreService> _logger;
        private readonly string _host;
        private readonly string _apiKey;

        private static readonly TimeSpan TopPlayersCacheTtl = TimeSpan.FromHours(3);
        private static readonly TimeSpan StandingsCacheTtl = TimeSpan.FromMinutes(45);
        private static readonly TimeSpan SeasonsCacheTtl = TimeSpan.FromHours(12);
        private static readonly TimeSpan PlayerDetailCacheTtl = TimeSpan.FromHours(6);
        private static readonly TimeSpan SearchCacheTtl = TimeSpan.FromMinutes(20);

        public SofaScoreService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<SofaScoreService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _host = configuration["SofaScoreApi:Host"] ?? "sofascore.p.rapidapi.com";
            _apiKey = configuration["SofaScoreApi:Key"] ?? "";
        }

        public async Task<int?> GetCurrentSeasonIdAsync(int tournamentId = SuperLigTournamentId)
        {
            var cacheKey = $"sofascore:season:{tournamentId}";
            if (_cache.TryGetValue(cacheKey, out int cached) && cached > 0)
                return cached;

            var seasonIds = await GetRecentSeasonIdsAsync(tournamentId, take: 1);
            var id = seasonIds.FirstOrDefault();
            if (id > 0)
            {
                _cache.Set(cacheKey, id, SeasonsCacheTtl);
                return id;
            }

            return null;
        }

        /// <summary>
        /// Newest seasons first. Upcoming/empty seasons stay first in API order,
        /// so callers that need stats should try a few ids.
        /// </summary>
        private async Task<List<int>> GetRecentSeasonIdsAsync(int tournamentId, int take = 3)
        {
            var cacheKey = $"sofascore:seasonids:{tournamentId}:{take}";
            if (_cache.TryGetValue(cacheKey, out List<int>? cached) && cached != null && cached.Count > 0)
                return cached;

            var json = await GetStringAsync($"/tournaments/get-seasons?tournamentId={tournamentId}", $"seasons:{tournamentId}");
            if (string.IsNullOrWhiteSpace(json))
                return new List<int>();

            try
            {
                var root = JObject.Parse(json);
                var seasons = root["seasons"] as JArray;
                var ids = seasons?
                    .Select(s => s["id"]?.Value<int?>())
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => id!.Value)
                    .Take(take)
                    .ToList() ?? new List<int>();

                if (ids.Count > 0)
                    _cache.Set(cacheKey, ids, SeasonsCacheTtl);

                return ids;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SofaScore seasons parse failed for tournament {TournamentId}", tournamentId);
                return new List<int>();
            }
        }

        public async Task<List<TopScorer>> GetTopScorersAsync(int tournamentId = SuperLigTournamentId)
        {
            var cacheKey = $"sofascore:topscorers:{tournamentId}";
            if (_cache.TryGetValue(cacheKey, out List<TopScorer>? cached) && cached != null)
                return cached;

            // Newest season can be published before stats exist; try a few recent seasons.
            var seasonIds = await GetRecentSeasonIdsAsync(tournamentId, take: 3);
            foreach (var seasonId in seasonIds)
            {
                var list = await TryMapTopScorersAsync(tournamentId, seasonId);
                if (list.Count > 0)
                {
                    _cache.Set(cacheKey, list, TopPlayersCacheTtl);
                    _cache.Set($"sofascore:season:{tournamentId}", seasonId, SeasonsCacheTtl);
                    return list;
                }
            }

            return new List<TopScorer>();
        }

        private async Task<List<TopScorer>> TryMapTopScorersAsync(int tournamentId, int seasonId)
        {
            var json = await GetStringAsync(
                $"/tournaments/get-top-players?tournamentId={tournamentId}&seasonId={seasonId}",
                $"topplayers:{tournamentId}:{seasonId}");

            if (string.IsNullOrWhiteSpace(json))
                return new List<TopScorer>();

            try
            {
                var root = JObject.Parse(json);
                var goals = root["topPlayers"]?["goals"] as JArray;
                if (goals == null || goals.Count == 0)
                    return new List<TopScorer>();

                var list = new List<TopScorer>();
                var rank = 1;
                foreach (var item in goals.Take(50))
                {
                    var player = item["player"];
                    var stats = item["statistics"];
                    var name = player?["name"]?.ToString();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    list.Add(new TopScorer
                    {
                        Id = rank.ToString(CultureInfo.InvariantCulture),
                        name = name,
                        goals = stats?["goals"]?.ToString() ?? "0",
                        play = stats?["appearances"]?.ToString()
                               ?? stats?["matches"]?.ToString()
                               ?? stats?["minutesPlayed"]?.ToString()
                               ?? ""
                    });
                    rank++;
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SofaScore top scorers parse failed for season {SeasonId}", seasonId);
                return new List<TopScorer>();
            }
        }

        /// <summary>
        /// CollectAPI-compatible JSON: { "result": [ { rank, team, play, win, lose, point } ] }
        /// </summary>
        public async Task<string?> GetStandingsCollectApiShapeAsync(int tournamentId = SuperLigTournamentId)
        {
            var cacheKey = $"sofascore:standings:collectshape:{tournamentId}";
            if (_cache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
                return cached;

            var seasonIds = await GetRecentSeasonIdsAsync(tournamentId, take: 3);
            foreach (var seasonId in seasonIds)
            {
                var json = await GetStringAsync(
                    $"/tournaments/get-standings?tournamentId={tournamentId}&seasonId={seasonId}",
                    $"standings:{tournamentId}:{seasonId}");

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                try
                {
                    var root = JObject.Parse(json);
                    var rows = root["standings"]?.FirstOrDefault()?["rows"] as JArray;
                    if (rows == null || rows.Count == 0)
                        continue;

                    var result = new JArray();
                    foreach (var row in rows)
                    {
                        result.Add(new JObject
                        {
                            ["rank"] = row["position"]?.ToString() ?? "",
                            ["team"] = row["team"]?["name"]?.ToString() ?? "",
                            ["play"] = row["matches"]?.ToString() ?? "",
                            ["win"] = row["wins"]?.ToString() ?? "",
                            ["lose"] = row["losses"]?.ToString() ?? "",
                            ["point"] = row["points"]?.ToString() ?? ""
                        });
                    }

                    var shaped = new JObject { ["result"] = result }.ToString();
                    _cache.Set(cacheKey, shaped, StandingsCacheTtl);
                    _cache.Set($"sofascore:season:{tournamentId}", seasonId, SeasonsCacheTtl);
                    return shaped;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SofaScore standings parse failed for season {SeasonId}", seasonId);
                }
            }

            return null;
        }

        public async Task<List<Player>> SearchPlayersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Player>();

            var cacheKey = $"sofascore:search:{query.Trim().ToLowerInvariant()}";
            if (_cache.TryGetValue(cacheKey, out List<Player>? cached) && cached != null)
                return cached;

            var safe = Uri.EscapeDataString(query.Trim());
            var json = await GetStringAsync($"/search?q={safe}", $"search:{safe}");
            if (string.IsNullOrWhiteSpace(json))
                return new List<Player>();

            try
            {
                var root = JObject.Parse(json);
                var results = root["results"] as JArray;
                if (results == null)
                    return new List<Player>();

                var players = new List<Player>();
                foreach (var item in results)
                {
                    var type = item["type"]?.ToString();
                    if (!string.Equals(type, "player", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var entity = item["entity"];
                    var id = entity?["id"]?.Value<int?>();
                    if (!id.HasValue)
                        continue;

                    players.Add(new Player
                    {
                        ID = id.Value,
                        PlayerName = entity?["name"]?.ToString() ?? "",
                        PlayerFullName = entity?["name"]?.ToString() ?? "",
                        Club = entity?["team"]?["name"]?.ToString() ?? "",
                        playerMainPosition = entity?["position"]?.ToString() ?? "",
                        BirthplaceCountry = entity?["country"]?["name"]?.ToString() ?? "",
                        PlayerShirtNumber = entity?["jerseyNumber"]?.ToString() ?? "",
                        PlayerImage = "",
                        MarketValue = "-"
                    });
                }

                _cache.Set(cacheKey, players, SearchCacheTtl);
                return players;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SofaScore search parse failed");
                return new List<Player>();
            }
        }

        public async Task<Player?> GetPlayerDetailAsync(int playerId)
        {
            var cacheKey = $"sofascore:player:{playerId}";
            if (_cache.TryGetValue(cacheKey, out Player? cached) && cached != null)
                return cached;

            var json = await GetStringAsync($"/players/detail?playerId={playerId}", $"player:{playerId}");
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                var root = JObject.Parse(json);
                var p = root["player"];
                if (p == null)
                    return null;

                var dobTs = p["dateOfBirthTimestamp"]?.Value<long?>();
                var age = "";
                var dob = "";
                if (dobTs.HasValue && dobTs.Value > 0)
                {
                    var birth = DateTimeOffset.FromUnixTimeSeconds(dobTs.Value).UtcDateTime;
                    dob = birth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    age = GetAge(birth).ToString(CultureInfo.InvariantCulture);
                }

                var market = FormatMarketValue(p["proposedMarketValue"], p["proposedMarketValueRaw"]);

                var player = new Player
                {
                    ID = p["id"]?.Value<int?>() ?? playerId,
                    PlayerName = p["name"]?.ToString() ?? "",
                    PlayerFullName = p["name"]?.ToString() ?? "",
                    Club = p["team"]?["name"]?.ToString() ?? "",
                    playerMainPosition = p["position"]?.ToString() ?? "",
                    BirthplaceCountry = p["country"]?["name"]?.ToString() ?? "",
                    PlayerShirtNumber = p["jerseyNumber"]?.ToString() ?? p["shirtNumber"]?.ToString() ?? "",
                    DateOfBirth = dob,
                    Age = age,
                    MarketValue = market,
                    PlayerImage = "" // no documented image URL in response; UI uses placeholder
                };

                _cache.Set(cacheKey, player, PlayerDetailCacheTtl);
                return player;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SofaScore player detail parse failed for {PlayerId}", playerId);
                return null;
            }
        }

        private async Task<string?> GetStringAsync(string pathAndQuery, string logLabel)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("SofaScore API key missing ({Label})", logLabel);
                return null;
            }

            try
            {
                var url = $"https://{_host}{pathAndQuery}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("x-rapidapi-host", _host);
                request.Headers.TryAddWithoutValidation("x-rapidapi-key", _apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("SofaScore rate limited (429) for {Label}", logLabel);
                    _cache.Set("market:sofascore:rate_limited", true, TimeSpan.FromMinutes(10));
                    return null;
                }

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("SofaScore auth/subscription failed ({Status}) for {Label}", (int)response.StatusCode, logLabel);
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("SofaScore endpoint not found (404) for {Label}", logLabel);
                    return null;
                }

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body))
                {
                    _logger.LogWarning("SofaScore non-success {Status} for {Label}", (int)response.StatusCode, logLabel);
                    return null;
                }

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SofaScore request failed for {Label}", logLabel);
                return null;
            }
        }

        private static int GetAge(DateTime birthUtc)
        {
            var today = DateTime.UtcNow.Date;
            var age = today.Year - birthUtc.Year;
            if (birthUtc.Date > today.AddYears(-age))
                age--;
            return Math.Max(age, 0);
        }

        private static string FormatMarketValue(JToken? proposed, JToken? proposedRaw)
        {
            // Prefer raw numeric value (EUR), format as millions for existing Market UI ("X.Y M €")
            var rawValue = proposedRaw?["value"]?.Value<decimal?>();
            if (rawValue.HasValue && rawValue.Value > 0)
            {
                var millions = rawValue.Value / 1_000_000m;
                return millions.ToString("0.#", CultureInfo.InvariantCulture);
            }

            if (proposed != null && proposed.Type != JTokenType.Null)
            {
                if (decimal.TryParse(proposed.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var n) && n > 0)
                {
                    if (n >= 1_000_000)
                        return (n / 1_000_000m).ToString("0.#", CultureInfo.InvariantCulture);
                    return n.ToString("0.#", CultureInfo.InvariantCulture);
                }
            }

            return "-";
        }
    }
}

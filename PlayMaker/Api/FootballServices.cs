using Microsoft.Extensions.Caching.Memory;

namespace PlayMaker.Api
{
    public class FootballService
    {
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api.collectapi.com/football/league?league=";
        private readonly string _apiKey;
        private readonly IMemoryCache _memoryCache;
        private readonly SofaScoreService _sofaScore;
        private readonly ILogger<FootballService> _logger;

        public FootballService(
            HttpClient httpClient,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            SofaScoreService sofaScore,
            ILogger<FootballService> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _apiKey = configuration["CollectApi:Key"] ?? "";
            _sofaScore = sofaScore;
            _logger = logger;
        }

        public async Task<string?> GetLeaguesAsync(string url)
        {
            var cacheKey = $"football_leagues_{url}";
            if (_memoryCache.TryGetValue(cacheKey, out string? cachedResult) && !string.IsNullOrWhiteSpace(cachedResult))
                return cachedResult;

            var collectResult = await TryCollectApiAsync(url);
            if (!string.IsNullOrWhiteSpace(collectResult))
            {
                _memoryCache.Set(cacheKey, collectResult, TimeSpan.FromMinutes(10));
                return collectResult;
            }

            // Fallback only when CollectAPI fails/empty — and only for Super Lig mapping.
            if (string.Equals(url, "super-lig", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(url))
            {
                _logger.LogInformation("CollectAPI standings unavailable; trying SofaScore fallback for Super Lig");
                var fallback = await _sofaScore.GetStandingsCollectApiShapeAsync(SofaScoreService.SuperLigTournamentId);
                if (!string.IsNullOrWhiteSpace(fallback))
                {
                    _memoryCache.Set(cacheKey, fallback, TimeSpan.FromMinutes(45));
                    return fallback;
                }
            }

            return null;
        }

        private async Task<string?> TryCollectApiAsync(string leagueKey)
        {
            try
            {
                var apiurl = $"{API_URL}{leagueKey}";
                var request = new HttpRequestMessage(HttpMethod.Get, apiurl);
                request.Headers.TryAddWithoutValidation("authorization", _apiKey);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CollectAPI league HTTP {Status}", (int)response.StatusCode);
                    return null;
                }

                var result = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(result))
                    return null;

                if (result.Contains("\"success\": false", StringComparison.Ordinal) ||
                    result.Contains("\"success\":false", StringComparison.Ordinal))
                    return null;

                // Treat empty result array as failure so fallback can run.
                try
                {
                    var token = Newtonsoft.Json.Linq.JToken.Parse(result);
                    if (token is Newtonsoft.Json.Linq.JObject obj)
                    {
                        var arr = obj["result"] as Newtonsoft.Json.Linq.JArray;
                        if (arr != null && arr.Count == 0)
                            return null;
                    }
                    else if (token is Newtonsoft.Json.Linq.JArray rootArr && rootArr.Count == 0)
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CollectAPI league request failed");
                return null;
            }
        }
    }
}

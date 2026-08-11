using Microsoft.Extensions.Caching.Memory;
using PlayMaker.Models.Top10;

namespace PlayMaker.Api
{
    public class PlayerServices
    {
        private readonly SofaScoreService _sofaScore;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PlayerServices> _logger;

        private const string RateLimitedKey = "market:sofascore:rate_limited";

        public PlayerServices(SofaScoreService sofaScore, IMemoryCache cache, ILogger<PlayerServices> logger)
        {
            _sofaScore = sofaScore;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<string>> UpdatePlayerData(ApiResponse response)
        {
            var errors = new List<string>();
            if (response?.Data == null || response.Data.Count == 0)
            {
                errors.Add("No players to enrich.");
                return errors;
            }

            if (_cache.TryGetValue(RateLimitedKey, out _))
            {
                errors.Add("SofaScore temporarily throttled. Showing search results without full profiles.");
                return errors;
            }

            foreach (var player in response.Data.Values.Take(5))
            {
                try
                {
                    var detailed = await _sofaScore.GetPlayerDetailAsync(player.ID);
                    if (detailed == null)
                    {
                        errors.Add($"playerId={player.ID}: detail unavailable");
                        continue;
                    }

                    player.PlayerName = detailed.PlayerName;
                    player.PlayerFullName = detailed.PlayerFullName;
                    player.playerMainPosition = detailed.playerMainPosition;
                    player.Club = detailed.Club;
                    player.Age = detailed.Age;
                    player.DateOfBirth = detailed.DateOfBirth;
                    player.MarketValue = detailed.MarketValue;
                    player.BirthplaceCountry = detailed.BirthplaceCountry;
                    player.PlayerShirtNumber = detailed.PlayerShirtNumber;
                    player.PlayerImage = string.IsNullOrWhiteSpace(detailed.PlayerImage)
                        ? ""
                        : detailed.PlayerImage;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Player enrich failed for {PlayerId}", player.ID);
                    errors.Add($"playerId={player.ID}: enrich failed");
                }
            }

            return errors;
        }
    }
}

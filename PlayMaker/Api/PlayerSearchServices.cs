using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayMaker.Models.Top10;

namespace PlayMaker.Api
{
    public class PlayerSearchServices
    {
        private readonly SofaScoreService _sofaScore;
        private readonly ILogger<PlayerSearchServices> _logger;

        public PlayerSearchServices(SofaScoreService sofaScore, ILogger<PlayerSearchServices> logger)
        {
            _sofaScore = sofaScore;
            _logger = logger;
        }

        public async Task<JToken?> SearchPlayer(string query)
        {
            try
            {
                var players = await _sofaScore.SearchPlayersAsync(query);
                if (players == null || players.Count == 0)
                    return null;

                // MarketController: playerData.ToObject<List<Player>>()
                var json = JsonConvert.SerializeObject(players);
                return JArray.Parse(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Player search failed");
                return null;
            }
        }

        public async Task<string?> GetPlayerIdByName(string playerName)
        {
            var players = await _sofaScore.SearchPlayersAsync(playerName);
            var first = players.FirstOrDefault();
            return first == null ? null : first.ID.ToString();
        }
    }
}

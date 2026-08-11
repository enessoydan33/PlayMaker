using PlayMaker.Models;

namespace PlayMaker.Api
{
    /// <summary>
    /// Goal kings provider. CollectAPI /goalKings currently fails (500);
    /// this service uses SofaScore top-players and returns CollectAPI-shaped JSON
    /// so LeagueController mapping stays unchanged.
    /// </summary>
    public class GoalServices
    {
        private readonly SofaScoreService _sofaScore;
        private readonly ILogger<GoalServices> _logger;

        public GoalServices(SofaScoreService sofaScore, ILogger<GoalServices> logger)
        {
            _sofaScore = sofaScore;
            _logger = logger;
        }

        public async Task<string?> GetLeaguesAsync(string key)
        {
            try
            {
                // PlayMaker league keys are CollectAPI slugs; SofaScore Super Lig is tournament 52.
                // For other leagues we still attempt Super Lig scorers when key is super-lig / empty,
                // otherwise return empty to avoid wrong data.
                var isSuperLig = string.IsNullOrWhiteSpace(key)
                                 || key.Equals("super-lig", StringComparison.OrdinalIgnoreCase);

                if (!isSuperLig)
                {
                    _logger.LogInformation("Goal kings via SofaScore currently mapped for Super Lig only (key={Key})", key);
                    return "{\"result\":[]}";
                }

                var scorers = await _sofaScore.GetTopScorersAsync(SofaScoreService.SuperLigTournamentId);
                var result = scorers.Select(s => new
                {
                    rank = s.Id,
                    name = s.name,
                    goals = s.goals,
                    play = s.play
                });

                return System.Text.Json.JsonSerializer.Serialize(new { result });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GoalServices SofaScore mapping failed");
                return null;
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayMaker.Api;
using PlayMaker.Models.LiveScoreModel;

namespace PlayMaker.ViewComponents
{
    public class ScoreViewComponent : ViewComponent
    {
        private readonly LiveScoreServices _liveScoreServices;
        private readonly ILogger<ScoreViewComponent> _logger;

        public ScoreViewComponent(LiveScoreServices liveScoreServices, ILogger<ScoreViewComponent> logger)
        {
            _liveScoreServices = liveScoreServices;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var apiResponse = await _liveScoreServices.GetTeamsAsync();
                if (string.IsNullOrWhiteSpace(apiResponse))
                {
                    ViewBag.ErrorMessage = "Live scores unavailable.";
                    return View(null);
                }

                var matchData = JsonConvert.DeserializeObject<MatchData>(apiResponse);
                return View(matchData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ScoreViewComponent failed");
                ViewBag.ErrorMessage = "Live scores unavailable.";
                return View(null);
            }
        }
    }
}

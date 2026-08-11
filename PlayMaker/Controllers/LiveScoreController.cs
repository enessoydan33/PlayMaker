using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PlayMaker.Api;
using PlayMaker.Data;
using PlayMaker.Data.Concrete.EfCore;
using PlayMaker.Entity;
using PlayMaker.Models.LiveScoreModel;
using System.Security.Claims;

namespace PlayMaker.Controllers
{
    public class LiveScoreController : Controller
    {
        private readonly LiveScoreServices _liveScoreServices;
        private readonly PlaymakerContext _context;
        private readonly PollService _pollService;
        private readonly IUserVoteRepository _uservote;
        private readonly ILogger<LiveScoreController> _logger;

        public LiveScoreController(
            LiveScoreServices liveScoreServices,
            PlaymakerContext context,
            PollService pollService,
            IUserVoteRepository userVote,
            ILogger<LiveScoreController> logger)
        {
            _liveScoreServices = liveScoreServices;
            _context = context;
            _pollService = pollService;
            _uservote = userVote;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var apiResponse = await _liveScoreServices.GetTeamsAsync();
                if (string.IsNullOrWhiteSpace(apiResponse))
                {
                    ViewBag.ErrorMessage = "Live scores are temporarily unavailable. An active third-party API subscription is required.";
                    return View(null);
                }

                var matchData = JsonConvert.DeserializeObject<MatchData>(apiResponse);
                if (matchData?.Stages == null || matchData.Stages.Count == 0)
                {
                    ViewBag.ErrorMessage = "No live matches are available right now.";
                    return View(null);
                }

                await _pollService.CreatePollsIfNotExistAsync(matchData);
                var polls = _context.Polls.ToList();

                foreach (var stage in matchData.Stages)
                {
                    if (stage?.Events == null)
                        continue;

                    foreach (var match in stage.Events)
                    {
                        if (match?.T1 == null || match.T1.Count == 0 || match.T2 == null || match.T2.Count == 0)
                            continue;

                        string matchKey = $"{match.T1[0].Nm} vs {match.T2[0].Nm}";
                        var poll = polls.FirstOrDefault(p => p.MatchId == matchKey);
                        if (poll == null)
                            continue;

                        match.PollId = poll.Id;
                        match.Vote1 = await _context.UserVotes.CountAsync(v => v.PollId == poll.Id && v.SelectedOption == VoteOption.HomeWin);
                        match.VoteX = await _context.UserVotes.CountAsync(v => v.PollId == poll.Id && v.SelectedOption == VoteOption.Draw);
                        match.Vote2 = await _context.UserVotes.CountAsync(v => v.PollId == poll.Id && v.SelectedOption == VoteOption.AwayWin);
                    }
                }

                return View(matchData);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "LiveScore JSON parse failed");
                ViewBag.ErrorMessage = "Live scores are temporarily unavailable.";
                return View(null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LiveScore page failed");
                ViewBag.ErrorMessage = "Live scores are temporarily unavailable.";
                return View(null);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Vote(int pollId, string vote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index");

            VoteOption option;
            switch (vote)
            {
                case "1":
                    option = VoteOption.HomeWin;
                    break;
                case "X":
                    option = VoteOption.Draw;
                    break;
                case "2":
                    option = VoteOption.AwayWin;
                    break;
                default:
                    return RedirectToAction("Index");
            }

            bool alreadyVoted = await _context.UserVotes.AnyAsync(v => v.UserId == userId && v.PollId == pollId);
            if (!alreadyVoted)
            {
                var userVote = new UserVote
                {
                    UserId = userId,
                    PollId = pollId,
                    SelectedOption = option,
                    Date = DateTime.UtcNow
                };

                await _uservote.CreateUserVoteAsync(userVote);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}

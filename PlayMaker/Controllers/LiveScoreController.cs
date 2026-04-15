using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayMaker.Api;
using PlayMaker.Data;
using PlayMaker.Data.Concrete.EfCore;
using PlayMaker.Entity;
using PlayMaker.Models;
using PlayMaker.Models.LiveScoreModel;
using System;
using System.Security.Claims;


namespace PlayMaker.Controllers
{
    public class LiveScoreController:Controller
    {

        private readonly LiveScoreServices _liveScoreServices;
        private readonly PlaymakerContext _context;
        private readonly PollService _pollService;
        private readonly IUserVoteRepository _uservote;

        public LiveScoreController(LiveScoreServices liveScoreServices,PlaymakerContext context, PollService pollService,IUserVoteRepository userVote)
        {
            _liveScoreServices = liveScoreServices;
            _context = context;
            _pollService = pollService;
            _uservote = userVote;
        }

        public async Task<IActionResult> Index()
        {
            string apiResponse = await _liveScoreServices.GetTeamsAsync();
            Console.WriteLine(apiResponse);
            if (string.IsNullOrEmpty(apiResponse))
            {
                ViewBag.ErrorMessage = "API'den veri alınamadı.";
                return View();
            }

            try
            {
                var matchData = JsonConvert.DeserializeObject<MatchData>(apiResponse);
                await _pollService.CreatePollsIfNotExistAsync(matchData);
                var polls = _context.Polls.ToList();
                foreach (var stage in matchData.Stages)
                {
                    foreach (var match in stage.Events)
                    {
                        string matchKey = $"{match.T1[0].Nm} vs {match.T2[0].Nm}";
                        var poll = polls.FirstOrDefault(p => p.MatchId == matchKey);

                        match.PollId = poll.Id;
                        match.Vote1 = await _context.UserVotes.CountAsync(v => v.PollId == poll.Id && v.SelectedOption == VoteOption.HomeWin);
                        match.VoteX = await _context.UserVotes.CountAsync(v => v.PollId == poll.Id && v.SelectedOption == VoteOption.Draw);
                        match.Vote2 = await _context.UserVotes.CountAsync(v => v.PollId == poll.Id && v.SelectedOption == VoteOption.AwayWin);
                    }
                }


                return View(matchData);

            }
            catch (JsonReaderException ex)
            {
                ViewBag.ErrorMessage = "API'den dönen JSON hatalı: " + ex.Message;
                return View();
            }
            

        }

        [HttpPost]
        public async Task<IActionResult> Vote(int pollId, string vote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index");

            var option = vote switch
            {
                "1" => VoteOption.HomeWin,
                "X" => VoteOption.Draw,
                "2" => VoteOption.AwayWin,
                _ => throw new ArgumentException("Geçersiz oy.")
            };



            // Aynı kullanıcı aynı ankete tekrar oy vermesin
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

              await  _uservote.CreateUserVoteAsync(userVote);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



    }
}

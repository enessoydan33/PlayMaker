using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PlayMaker.Models;
using PlayMaker.Api;
using PlayMaker.Models.LigSecimi;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlayMaker.Entity;
using Microsoft.AspNetCore.Identity;
using PlayMaker.ViewsModel;
using System.Threading.Tasks;

namespace PlayMaker.Controllers
{
    public class LeagueController : Controller
    {
        private readonly GoalServices _goalServices;
        private readonly FootballService _footballService;
        private readonly UserManager<User> _userManager;
        private readonly LigIsimler _ligIsimler;
        private readonly PlayerSearchServices _playersearch;
        private readonly ICommentRepository _commentrepo;

        public LeagueController(
            FootballService footballService,
            GoalServices goalServices,
            UserManager<User> userManager,
            LigIsimler ligIsimler,
            ICommentRepository commentrepo,PlayerSearchServices playerSearch)
        {
            _footballService = footballService;
            _goalServices = goalServices;
            _userManager = userManager;
            _ligIsimler = ligIsimler;
            _commentrepo = commentrepo;
            _playersearch = playerSearch;
        }

        // GET: Sayfa ilk yüklendiğinde verileri getir
        public async Task<IActionResult> Index(string SelectedLeague)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var photoPath = user.ProfilePictureUrl;
                ViewBag.ProfilePhoto = photoPath;
            }

            if (string.IsNullOrEmpty(SelectedLeague))
                SelectedLeague = "super-lig";

            List<TopScorer> goalk = new List<TopScorer>();
            List<Standings> list = new List<Standings>();

            string g_apiResponse = await _goalServices.GetLeaguesAsync(SelectedLeague);
            if (!string.IsNullOrEmpty(g_apiResponse))
            {
                try
                {
                    var token = JToken.Parse(g_apiResponse);
                    if (token is JObject obj && obj["result"] != null)
                    {
                        goalk = obj["result"].ToObject<List<TopScorer>>();
                    }
                    else if (token is JArray arr)
                    {
                        goalk = arr.ToObject<List<TopScorer>>();
                    }
                }
                catch (Exception) { /* Log error */ }
            }

            string Ligs = await _footballService.GetLeaguesAsync(SelectedLeague);
            if (!string.IsNullOrEmpty(Ligs))
            {
                try
                {
                    var token = JToken.Parse(Ligs);
                    if (token is JArray arr)
                    {
                        list = arr.ToObject<List<Standings>>();
                    }
                    else if (token is JObject obj && obj["result"] != null)
                    {
                        list = obj["result"].ToObject<List<Standings>>();
                    }
                }
                catch (Exception) { /* Log error */ }
            }

            goalk = goalk ?? new List<TopScorer>();
            list = list ?? new List<Standings>();

            ViewBag.YorumGolk = new SelectList(goalk.Where(g => !string.IsNullOrEmpty(g.name)).Select(g => new { Key = g.name, Name = g.name }), "Key", "Name", SelectedLeague);
            ViewBag.TeamList = new SelectList(list.Where(g => !string.IsNullOrEmpty(g.Team)).Select(g => new { Key = g.Team, Name = g.Team }), "Key", "Name", SelectedLeague);
            ViewBag.Lig = _ligIsimler.GetLeagueNameByKey(SelectedLeague);

            return View(goalk);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index"); // Model tipi uyuşmazlığını önlemek için redirect
            }

            

            var userId = _userManager.GetUserId(User);

            Comment comment = null;

            switch (model.CommentType?.ToLower())
            {
                case "player":
                    string plyId=await _playersearch.GetPlayerIdByName(model.CommentTargetGol);
                    if (string.IsNullOrEmpty(plyId) || !int.TryParse(plyId, out var pid) || pid == 0)
                    {
                        TempData["ErrorMessage"] = "Player not found. Please try again.";
                        return RedirectToAction("Index", new { SelectedLeague = "super-lig" });
                    }
                    comment = new PlayerComment
                    {
                        UserId = userId,
                        Text = model.Text,
                        Date = DateTime.UtcNow,
                        PlayerId = pid,
                        Likes = new List<Like>(),
                        Dislikes = new List<Dislike>()
                    };
                    break;

                case "team":
                    comment = new TeamComment
                    {
                        UserId = userId,
                        Text = model.Text,
                        Date = DateTime.UtcNow,
                        TeamName = model.CommentTargetGol,
                        Likes = new List<Like>(),
                        Dislikes = new List<Dislike>()
                    };
                    break;
             
                case "league":
                    comment = new LeagueComment
                    {
                        UserId = userId,
                        Text = model.Text,
                        Date = DateTime.UtcNow,
                        LeagueName = model.CommentTargetGol,
                        Likes = new List<Like>(),
                        Dislikes = new List<Dislike>()
                    };
                    break;

                default:
                    ModelState.AddModelError("", "Invalid comment type.");
                    return View(model);
            }

            await _commentrepo.CreateCommentAsync(comment);
            return RedirectToAction("Index", new { SelectedLeague = "super-lig" });
        }


        public IActionResult LoadComments(string type)
        {
            
            return ViewComponent("Comment", new { type = type });
        }
    }
}

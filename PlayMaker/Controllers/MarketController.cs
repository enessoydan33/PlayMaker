using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayMaker.Api;
using PlayMaker.Models;
using PlayMaker.Models.Top10;
using System;
using System.Threading.Tasks;

namespace PlayMaker.Controllers
{
    public class MarketController : Controller
    {
        PlayerSearchServices playerSearchServices;
        PlayerServices playerServices;
        public MarketController(PlayerSearchServices _PlayerSearchService, PlayerServices _playerServices)
        {
            playerSearchServices = _PlayerSearchService;
            playerServices = _playerServices;
        }
        // Default route expects action name "Index"
        public IActionResult Index()
        {
            try
            {
                // Don't call RapidAPI on initial page load.
                // This avoids 429 and shows an empty table until user searches.
                return View(new ApiResponse { Data = new Dictionary<int, Player>() });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ViewBag.MarketApiErrors = new List<string> { ex.Message };
                return View(new ApiResponse { Data = new Dictionary<int, Player>() }); // Hata durumunda boş görünüm döndür
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string playerName)
        {
            try
            {

                var playerData = await playerSearchServices.SearchPlayer(playerName);

                if (playerData == null)
                {
                    Console.WriteLine("Hata: API'den gelen veri null.");
                    ViewBag.MarketApiErrors = new List<string> { "Search API'den veri gelmedi (null). Console logunu kontrol et." };
                    return View("Index", new ApiResponse { Data = new Dictionary<int, Player>() });
                }

                var playersList = playerData.ToObject<List<Player>>();
                var dict = playersList.ToDictionary(p => p.ID, p => p);

                // JSON verisini deserialize et
                var topPlayers = new ApiResponse
                {
                    Data = dict
                };

                var errors = await playerServices.UpdatePlayerData(topPlayers);
                ViewBag.MarketApiErrors = errors;


                if (topPlayers == null)
                {
                    Console.WriteLine("Hata: JSON deserialization başarısız!");
                    return View("Index", new ApiResponse { Data = new Dictionary<int, Player>() });
                }

                return View("Index",topPlayers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ViewBag.MarketApiErrors = new List<string> { ex.Message };
                return View("Index", new ApiResponse { Data = new Dictionary<int, Player>() });
            }
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using PlayMaker.Api;
using PlayMaker.Models;


namespace PlayMaker.ViewComponents
{
    public class LeagueViewComponent: ViewComponent
    {
         private readonly FootballService _footballService;
        private readonly LigIsimler _ligler;

        public LeagueViewComponent(FootballService footballService,LigIsimler ligler)
        {
            _footballService = footballService;
            _ligler = ligler;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? SelectedLeague = null)
        {
            if (string.IsNullOrEmpty(SelectedLeague))
            {
                SelectedLeague = "super-lig";
            }

            Console.WriteLine($"[LeagueVC] SelectedLeague = {SelectedLeague}");

            ViewBag.LiglerStd = new SelectList(_ligler.GetAllLeagues(), "key", "Name", SelectedLeague);

            string? apiResponse = await _footballService.GetLeaguesAsync(SelectedLeague);
            Console.WriteLine($"[LeagueVC] apiResponse null? = {apiResponse == null}, length = {apiResponse?.Length}");

            if (!string.IsNullOrEmpty(apiResponse))
            {
                try
                {
                    var token = JToken.Parse(apiResponse);
                    List<Standings> standingsData = new List<Standings>();

                    if (token is JArray arr)
                    {
                        standingsData = arr.ToObject<List<Standings>>();
                    }
                    else if (token is JObject obj && obj["result"] != null)
                    {
                        standingsData = obj["result"].ToObject<List<Standings>>();
                    }

                    return View(standingsData ?? new List<Standings>()); // Eğer null ise boş liste döndür
                }
                catch (Exception)
                {
                   // Log error if needed
                   return View(new List<Standings>());
                }
            }

            return View(new List<Standings>()); // Null kontrolü eklendi
        }




    }
}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayMaker.Api;
using PlayMaker.Models.LiveScoreModel;
using System.ComponentModel;

namespace PlayMaker.ViewComponents
{
    public class ScoreViewComponent:ViewComponent
    {

        private readonly LiveScoreServices _liveScoreServices;

        public ScoreViewComponent(LiveScoreServices liveScoreServices)
        {
            _liveScoreServices = liveScoreServices;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            string apiResponse = await _liveScoreServices.GetTeamsAsync();
            if (string.IsNullOrEmpty(apiResponse))
            {
                ViewBag.ErrorMessage = "API'den veri alınamadı.";
                return View();
            }

            try
            {
                var matchData = JsonConvert.DeserializeObject<MatchData>(apiResponse);
                
                // View'e gönder
                return View(matchData);

            }
            catch (JsonReaderException ex)
            {
                ViewBag.ErrorMessage = "API'den dönen JSON hatalı: " + ex.Message;
                Console.WriteLine("API Yanıtı: " + apiResponse);
                return View();
            }



           
        }


    }
}

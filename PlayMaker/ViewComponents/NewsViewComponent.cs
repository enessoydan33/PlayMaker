using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using PlayMaker.Api;
using PlayMaker.Models.NewsModel;

namespace PlayMaker.ViewComponents
{
    public class NewsViewComponent:ViewComponent
    {
        FootballNewsServices _footballNewsServices;
        public NewsViewComponent( FootballNewsServices footballNewsServices)
        {
            _footballNewsServices = footballNewsServices;

        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {

                string SNews = await _footballNewsServices.GetNewsAsync();

                if (SNews == null)
                {
                    Console.WriteLine("Hata: API'den gelen veri null.");
                    return View(null); // Eğer veri boşsa, boş bir görünüm döndür
                }

                var jObject = JObject.Parse(SNews);




                var newsList = jObject["response"]?["news"]
                        .Select(n => new NewsItem
                        {
                            Id = n["id"]?.ToString(),
                            ImageUrl = n["imageUrl"]?.ToString(),
                            Title = n["title"]?.ToString(),
                            GmtTime = n["gmtTime"]?.ToString(),
                            SourceStr = n["sourceStr"]?.ToString(),
                            SourceIconUrl = n["sourceIconUrl"]?.ToString(),
                            PageUrl = $"https://www.{n["sourceStr"]?.ToString()?.ToLower()}.com{n["page"]?["url"]?.ToString()}"
                        })
                        .ToList();


                if (newsList == null)
                {
                    Console.WriteLine("Hata: JSON deserialization başarısız!");
                    return View(null);
                }

                return View(newsList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                return View(null); // Hata durumunda boş görünüm döndür
            }
        }


    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlayMaker.Api;
using PlayMaker.Models;
using PlayMaker.Models.Top10;

namespace PlayMaker.Controllers
{
    public class WonderkidsController:Controller
    {

        public IActionResult Index( int page = 1)
        {
            var service = new WonderkidsService();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "excels", "WonderkisPlayers.xlsx");
            var allPlayers = service.ReadExcel(path);
           
            var filter = new WonderkidsFilter(); // Boş filtre

            // Paging
            int pageSize = 20;
            int totalPlayers = allPlayers.Count;
            int totalPages = (int)Math.Ceiling((double)totalPlayers / pageSize);
            var pagedPlayers = allPlayers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            SetSelectLists();

            return View(Tuple.Create(filter, pagedPlayers));
        }




        [HttpPost]
        public IActionResult Index(WonderkidsFilter filter, int page = 1)
        {
            var service = new WonderkidsService();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "excels", "WonderkisPlayers.xlsx");
            var allPlayers = service.ReadExcel(path);

            // Filtreleme
            if (!string.IsNullOrEmpty(filter.Position))
                allPlayers = allPlayers.Where(p => p.Role == filter.Position).ToList();

            if (!string.IsNullOrEmpty(filter.AgeGroup))
            {
                if (filter.AgeGroup == "U20")
                    allPlayers = allPlayers.Where(p => p.Age_2024 <= 20).ToList();
                else if (filter.AgeGroup == "U23")
                    allPlayers = allPlayers.Where(p => p.Age_2024 <= 23).ToList();
                else if (filter.AgeGroup == "U25")
                    allPlayers = allPlayers.Where(p => p.Age_2024 <= 25).ToList();
            }

            if (!string.IsNullOrEmpty(filter.Potential))
            {
                if (filter.Potential == "High") allPlayers = allPlayers.Where(p => p.Feature_Score_2023 >= 14).ToList();
                else if (filter.Potential == "Medium") allPlayers = allPlayers.Where(p => p.Feature_Score_2023 >= 12).ToList();
                else if (filter.Potential == "Low") allPlayers = allPlayers.Where(p => p.Feature_Score_2023 < 12).ToList();
            }

            if (!string.IsNullOrEmpty(filter.Order))
            {
                if (filter.Order == "overall_desc")
                    allPlayers = allPlayers.OrderByDescending(p => p.Feature_Score_2023).ToList();
                else if (filter.Order == "overall_asc")
                    allPlayers = allPlayers.OrderBy(p => p.Feature_Score_2023).ToList();
                else if (filter.Order == "potential_desc")
                    allPlayers = allPlayers.OrderByDescending(p => p.Predicted_2024_Growth).ToList();
                else if (filter.Order == "potential_asc")
                    allPlayers = allPlayers.OrderBy(p => p.Predicted_2024_Growth).ToList();
            }

            // Sayfalama
            int pageSize = 20;
            int totalPlayers = allPlayers.Count;
            int totalPages = (int)Math.Ceiling((double)totalPlayers / pageSize);
            var pagedPlayers = allPlayers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            SetSelectLists();

            return View(Tuple.Create(filter, pagedPlayers));
        }




        private void SetSelectLists()
        {
            ViewBag.Positions = new List<SelectListItem>
    {
        new SelectListItem { Text = "Fark etmez", Value = "" },
        new SelectListItem { Text = "Kaleci", Value = "GK" },
        new SelectListItem { Text = "Defans", Value = "DEF" },
        new SelectListItem { Text = "Orta Saha", Value = "MID" },
        new SelectListItem { Text = "Forvet", Value = "ATT" }
    };

            ViewBag.AgeGroups = new List<SelectListItem>
    {
        new SelectListItem { Text = "Fark etmez", Value = "" },
        new SelectListItem { Text = "U20", Value = "U20" },
        new SelectListItem { Text = "U23", Value = "U23" },
        new SelectListItem { Text = "U25", Value = "U25" }
    };

            ViewBag.Potentials = new List<SelectListItem>
    {
        new SelectListItem { Text = "Fark etmez", Value = "" },
        new SelectListItem { Text = "High", Value = "High" },
        new SelectListItem { Text = "Medium", Value = "Medium" },
        new SelectListItem { Text = "Low", Value = "Low" }
    };

            ViewBag.Orders = new List<SelectListItem>
    {
        new SelectListItem { Text = "Fark etmez", Value = "" },
        new SelectListItem { Text = "Overall ↓", Value = "overall_desc" },
        new SelectListItem { Text = "Overall ↑", Value = "overall_asc" },
        new SelectListItem { Text = "Potential ↓", Value = "potential_desc" },
        new SelectListItem { Text = "Potential ↑", Value = "potential_asc" }
    };
        }






    }
}

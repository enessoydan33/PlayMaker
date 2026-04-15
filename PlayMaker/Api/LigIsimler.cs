using PlayMaker.Models.LigSecimi;

namespace PlayMaker.Api
{
    public class LigIsimler
    {
        public List<Ligler> GetAllLeagues()
        {
            return new List<Ligler>
        {
            new Ligler{ key="super-lig" , Name="Süper lig"},
            new Ligler{ key="ingiltere-premier-ligi" , Name="Premier Lig"},
            new Ligler{ key="almanya-bundesliga" , Name="Bundesliga"},
            new Ligler{ key="italya-serie-a-ligi" , Name="Serie A"},
            new Ligler{ key="fransa-ligue-1", Name="Ligue 1"},
            new Ligler{ key="fransa-ligue-2", Name="Ligue 2"},
            new Ligler{ key="ispanya-la-liga", Name="La Liga"},
            new Ligler{ key="ingiltere-sampiyonluk-ligi" , Name="Championship"},
            new Ligler{ key="uefa-konferans-ligi" , Name="Konferans Ligi"},
        };
        }

        public string GetLeagueNameByKey(string key)
        {
            var lig = GetAllLeagues().FirstOrDefault(l => l.key == key);
            return lig?.Name ?? key; // eşleşme yoksa key'i döner
        }


    }
}

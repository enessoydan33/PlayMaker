using PlayMaker.Models.Top10;

namespace PlayMaker.Api
{
    public class Top10Players
    {
        
        public ApiResponse Get10player()
        {
        
            Dictionary<int, Player> allPlayers = new Dictionary<int, Player>
            {
             
              { 1, new Player {  ID= 418560, PlayerName = "Haaland" } },
              { 2, new Player { ID = 371998, PlayerName = "Vinicius" } },
              { 3, new Player { ID = 937958, PlayerName = "Yamal" } },
              { 4, new Player {  ID= 581678, PlayerName = "Bellingham" } },
              { 5, new Player { ID = 342229, PlayerName = "Mbappe" } },
             
            };

            var APlayer = new ApiResponse
            {
                Data = allPlayers
            };

            return APlayer;
        }


    }
}

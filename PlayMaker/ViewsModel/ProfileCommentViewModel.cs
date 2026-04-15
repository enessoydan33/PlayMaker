namespace PlayMaker.ViewsModel
{
    public class ProfileCommentViewModel
    {
     
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string Type { get; set; } // "Team", "Player", "League"

        public string? TeamName { get; set; }
        public int? PlayerId { get; set; }
        public string? LeagueName { get; set; }

    }
}

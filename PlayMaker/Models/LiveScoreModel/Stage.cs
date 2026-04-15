namespace PlayMaker.Models.LiveScoreModel
{
    public class Stage
    {
        public string Snm { get; set; }  // Lig İsmi
        public string Cnm { get; set; }  // Ülke İsmi
        public List<Event> Events { get; set; }
    }
}

namespace PlayMaker.Entity
{
    public class Option
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string Name { get; set; }
        public Poll Poll { get; set; }

    }
}

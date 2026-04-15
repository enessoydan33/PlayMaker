using PlayMaker.Entity;

public class UserVote
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int PollId { get; set; }
    public VoteOption SelectedOption { get; set; }  // Enum olarak geliyor
    public DateTime Date { get; set; }
    public User User { get; set; }
    public Poll Poll { get; set; }
}

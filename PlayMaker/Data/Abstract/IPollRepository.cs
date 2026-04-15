using PlayMaker.Entity;

public interface IPollRepository
{
    IQueryable<Poll> Polls { get; }
    Task CreatePollAsync(Poll poll);
    Task DeletePollAsync(Poll poll);
}

using PlayMaker.Entity;

public interface IUserVoteRepository
{
    IQueryable<UserVote> UserVotes { get; }
    Task CreateUserVoteAsync(UserVote userVote);
  
}

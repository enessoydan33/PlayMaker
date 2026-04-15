using PlayMaker.Entity;

namespace PlayMaker.Data.Concrete.EfCore
{
    public class UserVoteRepository : IUserVoteRepository
    {
        private readonly PlaymakerContext _context;


        public UserVoteRepository(PlaymakerContext context)
        {
            _context = context;
        }
       public IQueryable<UserVote> UserVotes => throw new NotImplementedException();

        public  async Task CreateUserVoteAsync(UserVote userVote )
        {
            _context.Add(userVote);
            _context.SaveChanges();
        }

       

    }
}

using PlayMaker.Entity;

namespace PlayMaker.Data.Concrete.EfCore
{
    public class PollRepository : IPollRepository
    {
        private readonly PlaymakerContext _context;

        public PollRepository(PlaymakerContext context)
        {
            _context = context;
        }



        public IQueryable<Poll> Poll { get; }

        public IQueryable<Poll> Polls => throw new NotImplementedException();

        public  async Task CreatePollAsync(Poll poll)
        {
            _context.Add(poll);
            _context.SaveChanges();
        }

        public async Task DeletePollAsync(Poll poll)
        {
            _context.Remove(poll);
            _context.SaveChanges();
        }
    }
}

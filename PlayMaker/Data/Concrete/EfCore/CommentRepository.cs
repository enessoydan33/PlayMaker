using PlayMaker.Data;
using PlayMaker.Entity;

public class CommentRepository : ICommentRepository
{
    private readonly PlaymakerContext _context;

    public CommentRepository(PlaymakerContext playmakerContext)
    {
        _context = playmakerContext;
    }

    public IQueryable<Comment> Comments => _context.Comments;

    public async Task CreateCommentAsync(Comment comment)
    {
        await _context.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(Comment comment)
    {
        _context.Remove(comment);
        await _context.SaveChangesAsync();
    }
}

using PlayMaker.Entity;

public interface ICommentRepository
{
    IQueryable<Comment> Comments { get; }
    Task CreateCommentAsync(Comment comment);
    Task DeleteCommentAsync(Comment comment);
}

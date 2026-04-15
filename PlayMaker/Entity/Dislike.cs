namespace PlayMaker.Entity
{
    public class Dislike
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CommentId { get; set; }
        public User User { get; set; }
        public Comment Comment { get; set; }
    }
}

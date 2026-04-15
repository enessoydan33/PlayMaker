using System.ComponentModel.DataAnnotations.Schema;

namespace PlayMaker.Entity
{
    public abstract class Comment
    {
        public int Id { get; set; }
       
        public string UserId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        public User User { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Dislike> Dislikes { get; set; }


    }
}

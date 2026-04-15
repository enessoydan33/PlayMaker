using Microsoft.AspNetCore.Identity;

namespace PlayMaker.Entity
{
    public class User : IdentityUser
    {
        public string ProfilePictureUrl { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<UserVote> UserVotes { get; set; }


    }




}

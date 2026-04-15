using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlayMaker.Entity;


namespace PlayMaker.Data
{
    public class PlaymakerContext : IdentityDbContext<User>
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Dislike> Dislikes { get; set; }
        public DbSet<PlayerComment> PlayerComments { get; set; }
        public DbSet<TeamComment> TeamComments { get; set; }
        public DbSet<LeagueComment> LeagueComments { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<UserVote> UserVotes { get; set; }

        public PlaymakerContext(DbContextOptions<PlaymakerContext> options) : base(options)
        {


        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            // TPT için her türetilmiş sınıfa ayrı tablo oluşturuyoruz
            modelBuilder.Entity<PlayerComment>().ToTable("PlayerComment");
            modelBuilder.Entity<TeamComment>().ToTable("TeamComment");
            modelBuilder.Entity<LeagueComment>().ToTable("LeagueComment");
        }









    }
}

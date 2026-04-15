using Microsoft.AspNetCore.Mvc;
using PlayMaker.Data;
using PlayMaker.Entity;
using PlayMaker.ViewsModel;
using Microsoft.EntityFrameworkCore;


namespace PlayMaker.ViewComponents
{
    public class CommentViewComponent : ViewComponent
    {
        private readonly PlaymakerContext _context;
        public CommentViewComponent(PlaymakerContext context)
        {
            _context = context;
        }


        public async Task<IViewComponentResult> InvokeAsync(string type)
        {
            var model = new GetCommentTarget
            {
                Type = type.ToLower(),
                Comments = new List<Comment>()
            };

            switch (type.ToLower())
            {
                case "league":
                    model.Comments = await _context.LeagueComments.Include(c => c.User).Include(c => c.Likes).Include(c => c.Dislikes).ToListAsync<Comment>();
                    break;
                case "team":
                    model.Comments = await _context.TeamComments.Include(c => c.User).Include(c => c.Likes).Include(c => c.Dislikes).ToListAsync<Comment>();
                    break;
                case "player":
                    model.Comments = await _context.PlayerComments.Include(c => c.User).Include(c => c.Likes).Include(c => c.Dislikes).ToListAsync<Comment>();
                    break;
            }

            return View( model); // sadece yorum kısmını render eden view
        }


    }


}

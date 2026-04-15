using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlayMaker.ViewsModel;
using PlayMaker.Entity;
using System.Threading.Tasks;
using PlayMaker.Data;
using Microsoft.EntityFrameworkCore;
using PlayMaker.Api;
using System.ComponentModel.Design;
namespace PlayMaker.Controllers
{
    public class UserController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signinmanager;
        private readonly IWebHostEnvironment _environment;
        private PlaymakerContext _context;
    
        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment environment,PlaymakerContext context )
        {
            _userManager = userManager;
            _signinmanager = signInManager;
            _environment = environment;
            _context = context;
            
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserIndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    ProfilePictureUrl = "/uploads/dfd.jpg"

                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signinmanager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var a in result.Errors)
                {
                    ModelState.AddModelError("", a.Description);
                }

                return View(model);



            }



            return View(model);

        }

        public IActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _signinmanager.SignOutAsync();

                    var result = await _signinmanager.PasswordSignInAsync(user, model.Password, true, false);

                    if (result.Succeeded)
                    {

                        return RedirectToAction("Index", "Home");
                    }

                    else
                    {
                        ModelState.AddModelError("", "Parola hatalı");
                        return View(model);
                    }

                }

                else
                {
                    ModelState.AddModelError("", "Mail hatalı");
                    return View(model);

                }

            }


            return View(model);
        }

        public async Task<IActionResult> Logout()
        {

            await _signinmanager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new UserEditprofileViewModel
            {
                UserName = user.UserName,
                ExistingProfilePhotoPath = user.ProfilePictureUrl,

                CurrentPassword = string.Empty,
                NewPassword = string.Empty,
                ConfirmPassword = string.Empty
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditprofileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            user.UserName = model.UserName;

            // PROFİL RESMİ YÜKLEME
            if (model.ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = "/uploads/" + uniqueFileName;
            }

            // ŞİFRE DEĞİŞTİRME
            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) &&
                !string.IsNullOrWhiteSpace(model.NewPassword) &&
                !string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Yeni şifre ve tekrar şifresi aynı olmalı.");
                    return View(model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View(model); // Şifre hatası varsa göster
                }
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profil başarıyla güncellendi!";
            return RedirectToAction("EditProfile");
        }


        public async Task<IActionResult> ProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var userıd = user.Id;
            if (user != null)
            {
                var photoPath = user.ProfilePictureUrl;
                ViewBag.ProfilePhoto = photoPath;
            }

            var teamComments = await _context.TeamComments.Where(c => c.UserId == userıd).Include(c => c.Likes)
       .Include(c => c.Dislikes)
       .Select(c => new ProfileCommentViewModel
       {
           
           Text = c.Text,
           Date = c.Date,
           LikeCount = c.Likes.Count,
           DislikeCount = c.Dislikes.Count,
           Type = "Team",
           TeamName = c.TeamName
       }).ToListAsync();

            var playerComments = await _context.PlayerComments
                .Where(c => c.UserId == userıd)
                .Include(c => c.Likes)
                .Include(c => c.Dislikes)
                .Select(c => new ProfileCommentViewModel
                {
                    Text = c.Text,
                    Date = c.Date,
                    LikeCount = c.Likes.Count,
                    DislikeCount = c.Dislikes.Count,
                    Type = "Player",
                    PlayerId =c.PlayerId
                }).ToListAsync();

            var leagueComments = await _context.Set<LeagueComment>()
               .Where(c => c.UserId == userıd)
                .Include(c => c.Likes)
                .Include(c => c.Dislikes)
                .Select(c => new ProfileCommentViewModel
                {
                    Text = c.Text,
                    Date = c.Date,
                    LikeCount = c.Likes.Count,
                    DislikeCount = c.Dislikes.Count,
                    Type = "League",
                    LeagueName = c.LeagueName
                }).ToListAsync();

            var allComments = teamComments
                .Concat(playerComments)
                .Concat(leagueComments)
                .OrderByDescending(c => c.Date)
                .ToList();

            return View(allComments);
        }

        [HttpPost]
        public IActionResult Like(int id)
        { 
            var userId = _userManager.GetUserId(User);

            var existingLike = _context.Likes.FirstOrDefault(l => l.CommentId == id && l.UserId == userId);
            var existingDislike = _context.Dislikes.FirstOrDefault(d => d.CommentId == id && d.UserId == userId);

            if (existingLike != null)
            {
                // Like zaten varsa geri çek
                _context.Likes.Remove(existingLike);
            }
            else
            {
                // Dislike varsa önce onu kaldır
                if (existingDislike != null)
                {
                    _context.Dislikes.Remove(existingDislike);
                }
                // Like ekle
                _context.Likes.Add(new Like { UserId = userId, CommentId = id });
            }
            _context.SaveChanges();
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index","League");
        }




        [HttpPost]
        public IActionResult DissLike(int id)
        {
            var userId = _userManager.GetUserId(User);
            var existingDislike = _context.Dislikes.FirstOrDefault(d => d.CommentId == id && d.UserId == userId);
            var existingLike = _context.Likes.FirstOrDefault(l => l.CommentId == id && l.UserId == userId);

            if (existingDislike != null)
            {
                // Dislike zaten varsa geri çek
                _context.Dislikes.Remove(existingDislike);
            }
            else
            {
                // Like varsa önce onu kaldır
                if (existingLike != null)
                {
                    _context.Likes.Remove(existingLike);
                }
                // Dislike ekle
                _context.Dislikes.Add(new Dislike { UserId = userId, CommentId = id });
            }

            _context.SaveChanges();
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index", "League");
        }



    }
    
}

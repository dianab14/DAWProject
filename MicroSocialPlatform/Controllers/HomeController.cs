using MicroSocialPlatform.Data;
using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MicroSocialPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.Identity.IsAuthenticated
                ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;

            IQueryable<Post> postsQuery = db.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt);

            // GUEST – vede doar postari publice
            if (currentUserId == null)
            {
                postsQuery = postsQuery.Where(p => !p.User.IsPrivate);
            }
            else
            {
                // id-urile userilor pe care ii urmareste (Accepted)
                var followingIds = db.Follows
                    .Where(f =>
                        f.FollowerId == currentUserId &&
                        f.Status == "Accepted")
                    .Select(f => f.FollowedId);

                postsQuery = postsQuery.Where(p =>
                    // propriile postari
                    p.UserId == currentUserId ||

                    // user public
                    !p.User.IsPrivate ||

                    // user privat dar urmarit
                    followingIds.Contains(p.UserId)
                );
            }

            var posts = await postsQuery.ToListAsync();
            return View(posts);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

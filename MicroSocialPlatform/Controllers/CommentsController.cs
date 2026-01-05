using MicroSocialPlatform.Data;
using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroSocialPlatform.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public CommentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        private string CurrentUserId() => userManager.GetUserId(User);

        // CREATE comment (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Show", "Posts", new { id = postId });

            // verificam ca postarea exista
            var postExists = await db.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) return NotFound();

            var comment = new Comment
            {
                PostId = postId,
                Content = content.Trim(),
                UserId = CurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            // evitam validari pe User/Post (nu vin din form)
            ModelState.Remove("User");
            ModelState.Remove("Post");

            if (!ModelState.IsValid)
                return RedirectToAction("Show", "Posts", new { id = postId });

            db.Comments.Add(comment);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", "Posts", new { id = postId });
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            if (comment.UserId != CurrentUserId()) return Forbid();

            return View(comment);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            if (comment.UserId != CurrentUserId()) return Forbid();

            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Content is required.");
                return View(comment);
            }

            comment.Content = content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return RedirectToAction("Show", "Posts", new { id = comment.PostId });
        }

        // DELETE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            bool isAdmin = User.IsInRole("Admin");

            if (comment.UserId != CurrentUserId() && !isAdmin) return Forbid();

            var postId = comment.PostId;

            db.Comments.Remove(comment);
            await db.SaveChangesAsync();

            return RedirectToAction("Show", "Posts", new { id = postId });
        }
    }
}

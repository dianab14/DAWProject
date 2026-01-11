using MicroSocialPlatform.Data;
using MicroSocialPlatform.Migrations;
using MicroSocialPlatform.Models;
using MicroSocialPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroSocialPlatform.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment env;
        private readonly IAiModerationService aiModerationService;
        private static readonly List<string> AllowedReactions = new()
        {
            "Like",
            "Haha",
            "Love",
            "Dislike"
        };


        public PostsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, IAiModerationService aiModerationService)
        {
            this.db = db;
            this.userManager = userManager;
            this.env = env;
            this.aiModerationService = aiModerationService;
        }

        private string CurrentUserId() => userManager.GetUserId(User);

        // FEED - descrescator dupa data
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "Home");
        }

        // SHOW - post + comentarii descrescator
        public async Task<IActionResult> Show(int id)
        {
            var post = await db.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && !p.User.IsDeleted);

            if (post == null) return NotFound();

            ViewBag.ReactionCounts = await db.Reactions
            .Where(r => r.PostId == id)
            .GroupBy(r => r.Type)
            .Select(g => new
            {
                Type = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

            ViewBag.MyReaction = await db.Reactions
                .FirstOrDefaultAsync(r =>
                    r.PostId == id &&
                    r.UserId == CurrentUserId());


            var comments = await db.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Post = post;
            ViewBag.Comments = comments;

            return View();
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // CREATE (POST) - Text/Imagine/Video
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? content, IFormFile? image, IFormFile? video)
        {
            var userId = CurrentUserId();

            // user activ
            var me = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (me == null || me.IsDeleted)
            {
                TempData["message"] = "Your account is deactivated. You cannot create posts.";
                TempData["messageType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            bool hasText = !string.IsNullOrWhiteSpace(content);
            bool hasImage = image != null && image.Length > 0;
            bool hasVideo = video != null && video.Length > 0;

            if (!hasText && !hasImage && !hasVideo)
            {
                ModelState.AddModelError("", "Post must contain text, an image, or a video.");
                return View();
            }

            if (hasImage && hasVideo)
            {
                ModelState.AddModelError("", "You can upload either an image or a video, not both.");
                return View();
            }

            if (hasText)
            {
                var result = await aiModerationService.AnalyzeAsync(content!.Trim());

                db.AiModerationLogs.Add(new AiModerationLog
                {
                    Content = content!.Trim(),
                    IsAppropriate = result.IsAppropriate,
                    Confidence = result.Confidence,
                    ContentType = "Post",
                    UserId = userId
                });

                await db.SaveChangesAsync();

                if (!result.Success || !result.IsAppropriate)
                {
                    TempData["message"] = "Your content contains inappropriate terms. Please rephrase and try again.";
                    TempData["messageType"] = "danger";

                    return View();
                }
            }

            string? imagePath = null;
            string? videoPath = null;

            // upload imagine
            if (hasImage)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(image!.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Image must be JPG, JPEG or PNG.");
                    return View();
                }

                var fileName = Guid.NewGuid().ToString() + ext;
                var storagePath = Path.Combine(env.WebRootPath, "uploads", "posts", "images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(storagePath)!);

                using (var fs = new FileStream(storagePath, FileMode.Create))
                    await image.CopyToAsync(fs);

                imagePath = "/uploads/posts/images/" + fileName;
            }

            // upload video
            if (hasVideo)
            {
                var allowed = new[] { ".mp4", ".webm", ".mov" };
                var ext = Path.GetExtension(video!.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Video must be MP4, WEBM or MOV.");
                    return View();
                }

                var fileName = Guid.NewGuid().ToString() + ext;
                var storagePath = Path.Combine(env.WebRootPath, "uploads", "posts", "videos", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(storagePath)!);

                using (var fs = new FileStream(storagePath, FileMode.Create))
                    await video.CopyToAsync(fs);

                videoPath = "/uploads/posts/videos/" + fileName;
            }

            var post = new Post
            {
                Content = content?.Trim(),
                ImagePath = imagePath,
                VideoPath = videoPath,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // evitam validari pe User (nu vine din form)
            ModelState.Remove("User");

            if (!ModelState.IsValid) return View();

            db.Posts.Add(post);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // EDIT (GET) - doar owner
        public async Task<IActionResult> Edit(int id)
        {
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();

            var me = await userManager.Users
                .FirstOrDefaultAsync(u => u.Id == CurrentUserId());

            if (me == null || me.IsDeleted)
            {
                TempData["message"] = "Your account is deactivated. You cannot edit posts.";
                TempData["messageType"] = "warning";
                return RedirectToAction("Index", "Home");
            }
            if (post.UserId != CurrentUserId()) return Forbid();

            return View(post);
        }

        // EDIT (POST) - doar owner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string? content)
        {
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();

            var me = await userManager.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId());
            if (me == null || me.IsDeleted)
            {
                TempData["message"] = "Your account is deactivated. You cannot edit posts.";
                TempData["messageType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            if (post.UserId != CurrentUserId()) return Forbid();

            bool hasText = !string.IsNullOrWhiteSpace(content);

            if (hasText)
            {
                var result = await aiModerationService.AnalyzeAsync(content!.Trim());

                // LOGGING
                db.AiModerationLogs.Add(new AiModerationLog
                {
                    Content = content!.Trim(),
                    IsAppropriate = result.IsAppropriate,
                    Confidence = result.Confidence,
                    ContentType = "Post (Edit)",
                    UserId = CurrentUserId()
                });

                await db.SaveChangesAsync();

                if (!result.Success || !result.IsAppropriate)
                {
                    TempData["message"] = "Your content contains inappropriate terms. Please rephrase and try again.";
                    TempData["messageType"] = "danger";

                    return View(post);
                }
            }

            post.Content = content?.Trim();
            post.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return RedirectToAction("Show", new { id = post.Id });
        }

        // DELETE (POST) - doar owner + admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool isAdmin = User.IsInRole("Admin");

            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();

            if (post.UserId != CurrentUserId() && !isAdmin) return Forbid();

            var me = await userManager.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId());
            if (me == null || me.IsDeleted)
            {
                TempData["message"] = "Your account is deactivated. You cannot delete posts.";
                TempData["messageType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            // stergem fisierele daca exista
            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var path = Path.Combine(env.WebRootPath, post.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            if (!string.IsNullOrEmpty(post.VideoPath))
            {
                var path = Path.Combine(env.WebRootPath, post.VideoPath.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            db.Posts.Remove(post);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> React(int postId, string type)
        {
            var userId = CurrentUserId();

            var me = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (me == null || me.IsDeleted)
            {
                TempData["message"] = "Your account is deactivated. You cannot react to posts.";
                TempData["messageType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            if (!AllowedReactions.Contains(type))
                return BadRequest("Invalid reaction type");

            var postExists = await db.Posts
                .AnyAsync(p => p.Id == postId && !p.User.IsDeleted);

            if (!postExists) return NotFound();

            var reaction = await db.Reactions
                .FirstOrDefaultAsync(r =>
                    r.PostId == postId &&
                    r.UserId == userId);

            if (reaction == null)
            {
                db.Reactions.Add(new Reaction
                {
                    PostId = postId,
                    UserId = userId,
                    Type = type
                });
            }
            else if (reaction.Type == type)
            {
                // toggle off
                db.Reactions.Remove(reaction);
            }
            else
            {
                // schimbare reactie
                reaction.Type = type;
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Show", new { id = postId });
        }

    }
}

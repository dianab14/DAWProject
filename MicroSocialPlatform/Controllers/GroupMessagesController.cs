using MicroSocialPlatform.Data;
using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MicroSocialPlatform.Controllers
{
    [Authorize]
    public class GroupMessagesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public GroupMessagesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        private string CurrentUserId() => userManager.GetUserId(User);

        private async Task<bool> IsAcceptedMember(int groupId, string userId)
        {
            return await db.GroupMemberships.AnyAsync(m =>
                m.GroupId == groupId &&
                m.UserId == userId &&
                m.Status == "Accepted");
        }

        // CREATE MESSAGE (POST) - din pagina Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int groupId, string content)
        {
            var userId = CurrentUserId();

            // validare minima
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", "Groups", new { id = groupId });

            // doar membri Accepted pot posta
            if (!await IsAcceptedMember(groupId, userId))
                return Forbid();

            var msg = new GroupMessage
            {
                GroupId = groupId,
                UserId = userId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };

            db.GroupMessages.Add(msg);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", "Groups", new { id = groupId });
        }

        // EDIT MESSAGE (GET) - afiseaza formular
        public async Task<IActionResult> Edit(int id)
        {
            var msg = await db.GroupMessages.FirstOrDefaultAsync(m => m.Id == id);
            if (msg == null) return NotFound();

            // doar autorul mesajului
            if (msg.UserId != CurrentUserId())
                return Forbid();

            // daca nu mai este membru Accepted, NU mai poate edita
            if (!await IsAcceptedMember(msg.GroupId, CurrentUserId()))
                return Forbid();

            return View(msg);
        }

        // EDIT MESSAGE (POST) - salveaza
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GroupMessage formMsg)
        {
            var msg = await db.GroupMessages.FirstOrDefaultAsync(m => m.Id == id);
            if (msg == null) return NotFound();

            if (msg.UserId != CurrentUserId())
                return Forbid();

            if (!await IsAcceptedMember(msg.GroupId, CurrentUserId()))
                return Forbid();

            if (string.IsNullOrWhiteSpace(formMsg.Content))
            {
                ModelState.AddModelError("Content", "Content is required.");
                return View(msg);
            }

            msg.Content = formMsg.Content.Trim();
            msg.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return RedirectToAction("Details", "Groups", new { id = msg.GroupId });
        }

        // DELETE MESSAGE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await db.GroupMessages.FirstOrDefaultAsync(m => m.Id == id);
            if (msg == null) return NotFound();

            var userId = CurrentUserId();

            bool isAdmin = User.IsInRole("Admin");

            // trebuie sa fii membru Accepted ca sa poti sterge
            if (!isAdmin && !await IsAcceptedMember(msg.GroupId, CurrentUserId()))
                return Forbid();

            // verific daca userul curent este moderatorul grupului
            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == msg.GroupId);
            if (group == null) return NotFound();

            bool isModerator = (group.ModeratorId == userId);
            bool isOwner = (msg.UserId == userId);


            if (!isOwner && !isModerator && !isAdmin)
                return Forbid();

            var groupId = msg.GroupId;

            db.GroupMessages.Remove(msg);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", "Groups", new { id = groupId });
        }
    }
}

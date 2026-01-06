using MicroSocialPlatform.Data;
using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroSocialPlatform.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public GroupsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        // LISTA GRUPURI
        public async Task<IActionResult> Index()
        {
            //var userId = userManager.GetUserId(User);

            //var groups = await db.Groups
            //    .Include(g => g.Moderator)
            //    .OrderByDescending(g => g.CreatedAt)
            //    .ToListAsync();

            //// luam status-ul userului pentru fiecare grup (daca exista)
            //var myMemberships = await db.GroupMemberships
            //    .Where(m => m.UserId == userId)
            //    .Select(m => new { m.GroupId, m.Status })
            //    .ToListAsync();

            ////GroupId -> Status ("Accepted", "Pending", "Rejected")
            //ViewBag.MyGroupStatus = myMemberships.ToDictionary(x => x.GroupId, x => x.Status);

            //return View(groups);

            var userId = userManager.GetUserId(User);

            // 1) Query de baza (NU ToListAsync inca)
            IQueryable<Group> groupsQuery = db.Groups
                .Include(g => g.Moderator)
                .OrderByDescending(g => g.CreatedAt);

            // 2) Paginare
            int perPage = 6; // alege cat vrei (ex: 6 carduri / pagina)
            int totalItems = await groupsQuery.CountAsync();

            int currentPage = 1;
            if (!string.IsNullOrEmpty(HttpContext.Request.Query["page"]))
            {
                int.TryParse(HttpContext.Request.Query["page"], out currentPage);
                if (currentPage < 1) currentPage = 1;
            }

            int lastPage = (int)Math.Ceiling((double)totalItems / perPage);
            if (lastPage < 1) lastPage = 1;
            if (currentPage > lastPage) currentPage = lastPage;

            int offset = (currentPage - 1) * perPage;

            var groups = await groupsQuery
                .Skip(offset)
                .Take(perPage)
                .ToListAsync();

            // 3) Status-urile userului (ca inainte)
            var myMemberships = await db.GroupMemberships
                .Where(m => m.UserId == userId)
                .Select(m => new { m.GroupId, m.Status })
                .ToListAsync();

            ViewBag.MyGroupStatus = myMemberships.ToDictionary(x => x.GroupId, x => x.Status);

            // 4) Date pentru paginare (pentru view)
            ViewBag.CurrentPage = currentPage;
            ViewBag.LastPage = lastPage;
            ViewBag.PaginationBaseUrl = "/Groups/Index?page";

            return View(groups);

        }


        // DETALII GRUP + membri + mesaje + status-ul meu
        public async Task<IActionResult> Details(int id)
        {
            var group = await db.Groups
                .Include(g => g.Moderator)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            var userId = userManager.GetUserId(User);

            // status-ul meu in grup (null daca nu am cerere / nu sunt membru)
            var myMembership = await db.GroupMemberships
                .FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);

            // membri acceptati
            var members = await db.GroupMemberships
                .Include(m => m.User)
                .Where(m => m.GroupId == id && m.Status == "Accepted")
                .ToListAsync();

            // cereri pending (doar moderatorul le vede)
            var pending = new List<GroupMembership>();
            if (group.ModeratorId == userId)
            {
                pending = await db.GroupMemberships
                    .Include(m => m.User)
                    .Where(m => m.GroupId == id && m.Status == "Pending")
                    .OrderBy(m => m.RequestedAt)
                    .ToListAsync();
            }

            // mesaje
            var messages = await db.GroupMessages
                .Include(m => m.User)
                .Where(m => m.GroupId == id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // trimit in ViewBag ca sa nu folosim ViewModels
            ViewBag.Group = group;
            ViewBag.MyMembership = myMembership;
            ViewBag.Members = members;
            ViewBag.Pending = pending;
            ViewBag.Messages = messages;

            return View();
        }

        // CREATE GROUP (GET)
        public IActionResult Create()
        {
            return View(new Group());
        }

        // CREATE GROUP (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group group)
        {
            var userId = userManager.GetUserId(User);

            var me = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (me == null || me.IsDeleted)
                return Forbid();

            //setam ModeratorId înainte de validare
            group.ModeratorId = userId;

            //scoatem validarea pentru câmpurile care NU vin din form
            ModelState.Remove("ModeratorId");
            ModelState.Remove("Moderator"); // uneori apare și asta

            if (!ModelState.IsValid)
                return View(group);

            group.ModeratorId = userId;
            group.CreatedAt = DateTime.UtcNow;

            db.Groups.Add(group);
            await db.SaveChangesAsync();

            db.GroupMemberships.Add(new GroupMembership
            {
                GroupId = group.Id,
                UserId = userId,
                Status = "Accepted",
                RequestedAt = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = group.Id });
        }

        // JOIN -> creeaza cerere Pending
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int groupId)
        {
            var userId = userManager.GetUserId(User);

            var me = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (me == null || me.IsDeleted) return Forbid();

            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound();

            // daca exista deja membership (Pending/Accepted/Rejected) nu mai creez altul
            var existing = await db.GroupMemberships
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (existing != null)
                return RedirectToAction("Details", new { id = groupId });

            db.GroupMemberships.Add(new GroupMembership
            {
                GroupId = groupId,
                UserId = userId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = groupId });
        }

        // MODERATOR: ACCEPTA cerere
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int groupId, int membershipId)
        {
            var userId = userManager.GetUserId(User);

            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound();
            if (group.ModeratorId != userId) return Forbid();

            var membership = await db.GroupMemberships
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.GroupId == groupId);

            if (membership == null) return NotFound();

            membership.Status = "Accepted";
            membership.JoinedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = groupId });
        }

        // MODERATOR: RESPINGE cerere
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int groupId, int membershipId)
        {
            var userId = userManager.GetUserId(User);

            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound();
            if (group.ModeratorId != userId) return Forbid();

            var membership = await db.GroupMemberships
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.GroupId == groupId);

            if (membership == null) return NotFound();

            membership.Status = "Rejected";

            await db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = groupId });
        }

        // USER: LEAVE grup (sterge membership-ul lui)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int groupId)
        {
            var userId = userManager.GetUserId(User);

            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound();

            // moderatorul nu "pleaca", el sterge grupul
            if (group.ModeratorId == userId) return Forbid();

            var membership = await db.GroupMemberships
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (membership == null) return NotFound();

            db.GroupMemberships.Remove(membership);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // MODERATOR: ELIMINA un membru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int groupId, int membershipId)
        {
            var userId = userManager.GetUserId(User);

            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound();
            if (group.ModeratorId != userId) return Forbid();

            var membership = await db.GroupMemberships
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.GroupId == groupId);

            if (membership == null) return NotFound();

            // nu permit sa se elimine pe sine (moderator)
            if (membership.UserId == group.ModeratorId) return Forbid();

            db.GroupMemberships.Remove(membership);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = groupId });
        }

        // MODERATOR: STERGE GRUP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");

            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();
            if (group.ModeratorId != userId && !isAdmin) return Forbid();

            db.Groups.Remove(group);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        //// MODERATOR: EDITEAZA GRUP (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var userId = userManager.GetUserId(User);
            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();
            if (group.ModeratorId != userId) return Forbid();
            return View(group);
        }

        // MODERATOR: EDITEAZA GRUP (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Group formGroup)
        {
            var userId = userManager.GetUserId(User);

            var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();

            if (group.ModeratorId != userId) return Forbid();

            // scoatem validarea pentru campuri care nu vin din form
            ModelState.Remove("ModeratorId");
            ModelState.Remove("Moderator");
            ModelState.Remove("GroupMembers");
            ModelState.Remove("GroupMessages");

            if (!ModelState.IsValid)
                return View(formGroup);

            // actualizam doar ce are voie sa schimbe
            group.Name = formGroup.Name;
            group.Description = formGroup.Description;

            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = group.Id });
        }

    }
}

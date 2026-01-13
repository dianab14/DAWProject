using MicroSocialPlatform.Data;
using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var users = db.Users
                // .Where(u => !u.IsDeleted)
                .OrderBy(u => u.UserName);

            ViewBag.UsersList = users;

            var currentUserId = _userManager.GetUserId(User);
            ViewBag.CurrentUserId = currentUserId;


            return View();
        }

        public async Task<IActionResult> Show(string id)
        {
            var user = await _userManager.FindByIdAsync(id);


            if (user is null)
            {
                return NotFound();
            }

            if (user.IsDeleted)
            {
                TempData["message"] = "This user account is deactivated and cannot be viewed.";
                TempData["messageType"] = "error";
                return RedirectToAction("Index");
            }

            else
            {
                var roles = await _userManager.GetRolesAsync(user);

                ViewBag.Roles = roles;

                ViewBag.UserCurrent = await _userManager.GetUserAsync(User);

                return View(user);
            }
        }

        public async Task<ActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);


            if (user == null)
                return NotFound();

            if (user.IsDeleted)
            {
                TempData["message"] = "You cannot edit a deactivated user account.";
                TempData["messageType"] = "error";
                return RedirectToAction("Index");
            }

            ViewBag.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user); // lista de nume de roluri

            ViewBag.UserRole = _roleManager.Roles // cautam rolul in vaza de date
                .Where(r => roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            if (user.IsDeleted)
            {
                TempData["message"] = "You cannot modify a deactivated user account.";
                TempData["messageType"] = "error";
                return RedirectToAction("Index");
            }


            // VALIDARE DOAR CE CONTEAZA
            if (string.IsNullOrWhiteSpace(newData.FirstName) ||
                string.IsNullOrWhiteSpace(newData.LastName))
            {
                ModelState.AddModelError("", "First name and last name are required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AllRoles = GetAllRoles();
                ViewBag.UserRole = newRole;
                return View(user);
            }

            // ✅ DOAR CAMPURI PERMISE
            user.FirstName = newData.FirstName.Trim();
            user.LastName = newData.LastName.Trim();

            // 🔁 UPDATE ROLE
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var role = await _roleManager.FindByIdAsync(newRole);
            if (role != null)
            {
                await _userManager.AddToRoleAsync(user, role.Name!);
            }

            await db.SaveChangesAsync();

            TempData["message"] = "User updated successfully.";
            TempData["messageType"] = "success";

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["message"] = "You cannot deactivate your own account.";
                TempData["messageType"] = "error";
                return RedirectToAction("Index");
            }

            // soft delete
            user.IsDeleted = true;

            // blocam si login-ul
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);
            TempData["message"] = "User deactivated successfully.";
            TempData["messageType"] = "success";
            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> Reactivate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["message"] = "You cannot reactivate your own account.";
                TempData["messageType"] = "error";
                return RedirectToAction("Index");
            }


            user.IsDeleted = false;
            user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);

            TempData["message"] = "User reactivated successfully.";
            TempData["messageType"] = "success";

            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}

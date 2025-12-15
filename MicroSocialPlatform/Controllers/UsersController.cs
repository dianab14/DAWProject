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
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.UserName);

            ViewBag.UsersList = users;

            return View();
        }

        public async Task<IActionResult> ShowAsync(string id)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }

            if (user.IsDeleted)
            {
                return NotFound();
            }

            else
            {
                var roles = await _userManager.GetRolesAsync(user);

                ViewBag.Roles = roles;

                ViewBag.UserCurent = await _userManager.GetUserAsync(User);

                return View(user);
            }
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }

            if (user.IsDeleted)
            {
                return NotFound();
            }

            else
            {
                ViewBag.AllRoles = GetAllRoles();

                var roleNames = await _userManager.GetRolesAsync(user); // Lista de nume de roluri

                // Cautam ID-ul rolului in baza de date
                ViewBag.UserRole = _roleManager.Roles
                                                  .Where(r => roleNames.Contains(r.Name))
                                                  .Select(r => r.Id)
                                                  .First(); // Selectam 1 singur rol

                return View(user);
            }

        }

        [HttpPost]
        public async Task<ActionResult> EditAsync(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }

            else
            {

                if (ModelState.IsValid)
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.FirstName = newData.FirstName;
                    user.LastName = newData.LastName;
                    user.PhoneNumber = newData.PhoneNumber;

                    // Cautam toate rolurile din baza de date
                    var roles = db.Roles.ToList();

                    foreach (var role in roles)
                    {
                        // Scoatem userul din rolurile anterioare
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }

                    // Adaugam noul rol selectat
                    var roleName = await _roleManager.FindByIdAsync(newRole);
                    await _userManager.AddToRoleAsync(user, roleName.ToString());

                    db.SaveChanges();

                }

                user.AllRoles = GetAllRoles();
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteAsync(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // soft delete
            user.IsDeleted = true;

            // blocam si login-ul
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);

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

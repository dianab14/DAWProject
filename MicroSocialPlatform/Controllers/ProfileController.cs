using MicroSocialPlatform.Data;
using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _db = db;
        _env = env;
    }

    // profilul meu
    [Authorize] // doar pt useri inregistrati (logati)
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User); // obtin userul curent

        if (user == null || user.IsDeleted) // verific daca userul exista si nu e sters (soft delete)
            return NotFound();

        ViewBag.IsMyProfile = true;

        var posts = await _db.Posts
        .Include(p => p.User)
        .Where(p => p.UserId == user.Id)
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();

        ViewBag.Posts = posts;

        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User); // obtin userul curent

        if (user == null || user.IsDeleted) // verific daca userul exista si nu e sters (soft delete)
            return NotFound();

        return View(user); // formular de editare pre-populat
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ApplicationUser input, IFormFile? ProfileImage, bool RemoveProfilePhoto)
    {
        var user = await _userManager.GetUserAsync(User); // obtin userul curent

        if (user == null || user.IsDeleted) // verific daca userul exista si nu e sters (soft delete)
            return NotFound();

        if (ProfileImage != null && ProfileImage.Length > 0)
        {
            // RemoveProfilePhoto = false; // daca se incarca o poza noua, se va sterge oricum mai jos poza veche
                                        // deci nu mai are sens sa o inlocuim cu cea default

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(ProfileImage.FileName).ToLower();

            // verificam extensia
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(
                    "ProfileImage",
                    "Profile image must be a JPG, JPEG or PNG file."
                );
                return View(input);
            }

            // nume unic fisier
            /*var fileName = user.Id + fileExtension; // o poza de profil unica pentru fiecare user; se va suprascrie poza anterioara
                                                    // daca a avut o poza adaugata ce nu era default
            
             genera un pop up in visual studio la fiecare suprascriere de fisier*/

            var fileName = Guid.NewGuid().ToString() + fileExtension; // practic 0 coliziuni

            var storagePath = Path.Combine(
                _env.WebRootPath,
                "images",
                "profiles",
                fileName
            );

            var databasePath = "/images/profiles/" + fileName;

            // salvare fisier pe disc
            using (var fileStream = new FileStream(storagePath, FileMode.Create))
            {
                await ProfileImage.CopyToAsync(fileStream);
            }

            // stergem poza veche daca nu e default
            if (user.ProfileImagePath != "/images/profiles/default-profile.png")
            {
                var oldPath = Path.Combine(
                    _env.WebRootPath,
                    user.ProfileImagePath.TrimStart('/')
                );

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }


            user.ProfileImagePath = databasePath;
        }
        else if (RemoveProfilePhoto)
        {
            if (user.ProfileImagePath != "/images/profiles/default-profile.png")
            {
                var oldPath = Path.Combine(
                    _env.WebRootPath,
                    user.ProfileImagePath.TrimStart('/')
                );

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            user.ProfileImagePath = "/images/profiles/default-profile.png";
        }

        // dam update la celelalte campuri
        user.FirstName = input.FirstName;
        user.LastName = input.LastName;
        user.Description = input.Description;
        user.IsPrivate = input.IsPrivate;

        if (!TryValidateModel(user)) // validare model
            return View(input);

        await _userManager.UpdateAsync(user);

        return RedirectToAction("Index", "Profile");
    }


    // profilul altui utilizator

    // cautare profil dupa string 
    [AllowAnonymous]
    public IActionResult Search()
    {
        // Lista initiala de utilizatori (nu afisam useri stersi)
        var users = _db.Users
            .Where(u => !u.IsDeleted);


        // MOTOR DE CAUTARE
        var text = "";

        if (Convert.ToString(HttpContext.Request.Query["text"]) != null)
        {
            text = Convert.ToString(HttpContext.Request.Query["text"]).Trim();

            // cautare dupa:
            // FirstName + " " + LastName
            // SAU
            // LastName + " " + FirstName
            users = users.Where(u =>
                (u.FirstName + " " + u.LastName).ToLower().Contains(text.ToLower()) ||
                (u.LastName + " " + u.FirstName).ToLower().Contains(text.ToLower())
            );
        }

        ViewBag.SearchString = text;

        // ordonam dupa ce am cautat

        users = users.OrderBy(u => u.FirstName);

        // AFISARE PAGINATA

        int _perPage = 2;

        int totalItems = users.Count();

        var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

        var offset = 0;

        if (!currentPage.Equals(0))
        {
            offset = (currentPage - 1) * _perPage;
        }

        var paginatedUsers = users.Skip(offset).Take(_perPage);

        ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

        ViewBag.Users = paginatedUsers;

        if (text != "")
        {
            ViewBag.PaginationBaseUrl =
                "/Profile/Search?text=" + text + "&page";
        }
        else
        {
            ViewBag.PaginationBaseUrl =
                "/Profile/Search?page";
        }

        return View();
    }

    [AllowAnonymous] // si utilizatorii neinregistrati pot vedea profiluri publice, sau limitat pe cele private
    public async Task<IActionResult> Show(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User); // obtin id-ul userului logat

        bool isMyProfile =
            User.Identity?.IsAuthenticated == true &&
            currentUserId == user.Id;

        ViewBag.IsMyProfile = isMyProfile;

        // logica pentru profil privat
        if (user.IsPrivate && !isMyProfile)
        {
            ViewBag.IsPrivateView = true;   // affisare limitata
            return View(user);
        }

        ViewBag.IsPrivateView = false;

        // profil public sau e al meu -> incarc postari
        var posts = await _db.Posts
            .Include(p => p.User)
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.Posts = posts;

        return View(user);

        /*
            // profil privat
            if (user.IsPrivate)
            {
                if (!User.Identity.IsAuthenticated)
                    return Forbid();

                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null || currentUser.Id != user.Id)
                {
                    // aici vei extinde cu follow logic în Sprint 3
                    return Forbid();
                }
            }
        */
    }
}

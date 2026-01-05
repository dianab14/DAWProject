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

        var posts = await _db.Posts
            .Include(p => p.User)
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.IsMyProfile = true;
        ViewBag.Posts = posts;
        ViewBag.FollowersCount = GetFollowersCount(user.Id);
        ViewBag.FollowingCount = GetFollowingCount(user.Id);
        ViewBag.PendingRequestsCount = GetPendingRequestsCount(user.Id);


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

        bool wasPrivate = user.IsPrivate; // verific daca schimb vizibilitatea profilului de la privat la public

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

        // daca userul a trecut de la Private la Public accept toate request-urile
        if (wasPrivate && !user.IsPrivate)
        {
            var pendingRequests = _db.Follows.Where(f =>
                f.FollowedId == user.Id &&
                f.Status == "Pending");

            foreach (var req in pendingRequests)
            {
                req.Status = "Accepted";
                req.AcceptedAt = DateTime.UtcNow;
            }

            _db.SaveChanges();
        }


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

        ViewBag.FollowersCount = GetFollowersCount(user.Id);
        ViewBag.FollowingCount = GetFollowingCount(user.Id);

        if (ViewBag.IsMyProfile)
        {
            ViewBag.PendingRequestsCount = GetPendingRequestsCount(user.Id);
        }

        ViewBag.Follow = _db.Follows.FirstOrDefault(f =>
            f.FollowerId == currentUserId &&
            f.FollowedId == user.Id
        );

        // Followers = cine il urmareste
        ViewBag.FollowersCount = _db.Follows.Count(f =>
            f.FollowedId == user.Id &&
            f.Status == "Accepted");

        // Following = pe cine urmareste
        ViewBag.FollowingCount = _db.Follows.Count(f =>
            f.FollowerId == user.Id &&
            f.Status == "Accepted");

        // logica pentru profil privat
        /* if (user.IsPrivate && !isMyProfile)
            {
                ViewBag.IsPrivateView = true;   // afisare limitata 
                return View(user);
            }
        */

        bool canViewFullProfile = CanViewFullProfile(user, currentUserId);

        ViewBag.IsPrivateView = !canViewFullProfile;

        // profil public sau e al meu -> incarc postari
        var posts = await _db.Posts
            .Include(p => p.User)
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.Posts = posts;

        return View(user);
    }
    [Authorize]
    [HttpPost]
    public IActionResult Follow(string id)
    {
        var currentUserId = _userManager.GetUserId(User); // obtin id-ul userului logat

        if (currentUserId == id) // nu pot sa imi dau follow mie insumi
            return RedirectToAction("Show", new { id });

        var existingFollow = _db.Follows // verific daca am deja la follow
            .FirstOrDefault(f =>
                f.FollowerId == currentUserId &&
                f.FollowedId == id);

        if (existingFollow != null) // deja am la follow / am dat request
            return RedirectToAction("Show", new { id });

        var followedUser = _db.Users.First(u => u.Id == id); // obtin userul care urmeaza sa fie urmarit

        var follow = new Follow
        {
            FollowerId = currentUserId,
            FollowedId = id,
            Status = followedUser.IsPrivate ? "Pending" : "Accepted", // daca e privat, ramane pending pana la acceptare
            AcceptedAt = followedUser.IsPrivate ? null : DateTime.UtcNow // daca nu e privat, e acceptat imediat
        };

        _db.Follows.Add(follow);
        _db.SaveChanges();

        TempData["message"] = followedUser.IsPrivate
            ? "Follow request sent."
            : "You are now following this user.";

        TempData["messageType"] = "success";

        return RedirectToAction("Show", new { id });
    }

    [Authorize]
    [HttpPost]
    public IActionResult Unfollow(string id)
    {
        var currentUserId = _userManager.GetUserId(User); // obtin id-ul userului logat

        var follow = _db.Follows // verific daca am la follow (request sau acceptat)
            .FirstOrDefault(f =>
                f.FollowerId == currentUserId &&
                f.FollowedId == id);

        if (follow == null) // nu am la follow / nu am dat request
            return RedirectToAction("Show", new { id });

        if (follow.Status == "Accepted") // daca era acceptat
        {
            TempData["message"] = "You have unfollowed this user.";
        }
        else if (follow.Status == "Pending") // daca era request in asteptare
        {
            TempData["message"] = "Follow request cancelled.";
        }

        TempData["messageType"] = "success";

        _db.Follows.Remove(follow);
        _db.SaveChanges();

        return RedirectToAction("Show", new { id });
    }

    [Authorize]
    public IActionResult Requests()
    {
        var currentUserId = _userManager.GetUserId(User); // obtin id-ul userului logat

        var requests = _db.Follows // obtin cererile de follow primite (si neacceptate inca)
            .Where(f =>
                f.FollowedId == currentUserId &&
                f.Status == "Pending")
            .Include(f => f.Follower)
            .OrderByDescending(f => f.RequestedAt)
            .ToList();

        return View(requests);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Accept(int id)
    {
        var currentUserId = _userManager.GetUserId(User);

        var follow = _db.Follows.FirstOrDefault(f =>
            f.Id == id &&
            f.FollowedId == currentUserId &&
            f.Status == "Pending");

        if (follow == null)
            return NotFound();

        follow.Status = "Accepted";
        follow.AcceptedAt = DateTime.UtcNow;
        _db.SaveChanges();

        return RedirectToAction("Requests");
    }

    [Authorize]
    [HttpPost]
    public IActionResult Decline(int id)
    {
        var currentUserId = _userManager.GetUserId(User);

        var follow = _db.Follows.FirstOrDefault(f =>
            f.Id == id &&
            f.FollowedId == currentUserId &&
            f.Status == "Pending");

        if (follow == null)
            return NotFound();

        _db.Follows.Remove(follow);
        _db.SaveChanges();

        return RedirectToAction("Requests");
    }

    [Authorize]
    public IActionResult Followers(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        ViewBag.IsMyProfile = (currentUserId == id);
        ViewBag.ShowRemoveFollower = ViewBag.IsMyProfile;
        ViewBag.ShowUnfollow = false;

        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null || user.IsDeleted)
            return NotFound();

        if (!CanViewFullProfile(user, currentUserId))
            return Forbid();

        var followers = _db.Follows
            .Where(f => f.FollowedId == user.Id && f.Status == "Accepted")
            .Include(f => f.Follower)
            .Select(f => f.Follower)
            .ToList();

        string possessive =
        user.FirstName.EndsWith("s")
        ? $"{user.LastName} {user.FirstName}'"
        : $"{user.LastName} {user.FirstName}'s";

        ViewBag.Title = $"{possessive} Followers";
        return View("FollowList", followers);
    }


    [Authorize]
    public IActionResult Following(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        ViewBag.IsMyProfile = (currentUserId == id);
        ViewBag.ShowRemoveFollower = false;
        ViewBag.ShowUnfollow = ViewBag.IsMyProfile;

        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null || user.IsDeleted)
            return NotFound();

        if (!CanViewFullProfile(user, currentUserId))
            return Forbid();

        var following = _db.Follows
            .Where(f => f.FollowerId == user.Id && f.Status == "Accepted")
            .Include(f => f.Followed)
            .Select(f => f.Followed)
            .ToList();

        string possessive =
        user.FirstName.EndsWith("s")
        ? $"{user.LastName} {user.FirstName}'"
        : $"{user.LastName} {user.FirstName}'s";

        ViewBag.Title = $"{possessive} Following";
        return View("FollowList", following);
    }

    [Authorize]
    [HttpPost]
    public IActionResult RemoveFollower(string id)
    {
        var currentUserId = _userManager.GetUserId(User);

        // id = userul care MA urmareste
        var follow = _db.Follows.FirstOrDefault(f =>
            f.FollowerId == id &&
            f.FollowedId == currentUserId &&
            f.Status == "Accepted");

        if (follow == null)
            return NotFound();

        _db.Follows.Remove(follow);
        _db.SaveChanges();

        TempData["message"] = "Follower removed.";
        TempData["messageType"] = "success";

        return RedirectToAction("Followers", new { id = currentUserId });
    }

    [Authorize]
    [HttpPost]
    public IActionResult UnfollowFromList(string id)
    {
        var currentUserId = _userManager.GetUserId(User);

        // id = userul pe care il urmaresc
        var follow = _db.Follows.FirstOrDefault(f =>
            f.FollowerId == currentUserId &&
            f.FollowedId == id &&
            f.Status == "Accepted");

        if (follow == null)
            return NotFound();

        _db.Follows.Remove(follow);
        _db.SaveChanges();

        TempData["message"] = "Unfollowed successfully.";
        TempData["messageType"] = "success";

        return RedirectToAction("Following", new { id = currentUserId });
    }


    [NonAction]
    private int GetFollowersCount(string userId)
    {
        return _db.Follows.Count(f =>
            f.FollowedId == userId &&
            f.Status == "Accepted");
    }

    [NonAction]
    private int GetFollowingCount(string userId)
    {
        return _db.Follows.Count(f =>
            f.FollowerId == userId &&
            f.Status == "Accepted");
    }

    [NonAction]
    private int GetPendingRequestsCount(string userId)
    {
        return _db.Follows.Count(f =>
            f.FollowedId == userId &&
            f.Status == "Pending");
    }

    [NonAction]
    private bool CanViewFullProfile(ApplicationUser profileUser, string currentUserId)
    {
        // not logged in
        if (string.IsNullOrEmpty(currentUserId))
        {
            return !profileUser.IsPrivate;
        }
        // owner
        if (profileUser.Id == currentUserId)
            return true;

        // public profile
        if (!profileUser.IsPrivate)
            return true;

        // accepted follower
        return _db.Follows.Any(f =>
            f.FollowerId == currentUserId &&
            f.FollowedId == profileUser.Id &&
            f.Status == "Accepted");
    }


}


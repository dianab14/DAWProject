using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroSocialPlatform.Controllers
{
    public class ApplicationUsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private readonly AppDbContext db;

        public ApplicationUsersController(AppDbContext context)
        {
            db = context;
        }
    }
}

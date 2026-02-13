using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Forked.Models.Domains;

namespace Forked.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<User> _signInManager;

        public HomeController(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Recipes");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using SmartClinic.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace SmartClinic.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Already logged in — go to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        public IActionResult Login(string code, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Code == code);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserCode", user.Code);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("UserName", user.Name);
                return RedirectToAction("Index", "Dashboard");
            }

            TempData["Error"] = "Invalid code or password. Please try again.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

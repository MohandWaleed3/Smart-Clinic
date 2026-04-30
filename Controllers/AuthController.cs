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
            return View();
        }

        [HttpPost]
        public IActionResult Login(string code, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Code == code && u.Password == password);
            if (user != null)
            {
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
    }
}

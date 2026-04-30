using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace SmartClinic.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Role = role;

            return View();
        }
    }
}

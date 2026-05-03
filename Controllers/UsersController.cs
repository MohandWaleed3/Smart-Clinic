using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartClinic.Data;
using SmartClinic.Models;
using SmartClinic.Filters;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SmartClinic.Controllers
{
    [RoleAuthorize("Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var users = from u in _context.Users select u;

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.Name.Contains(searchString)
                                      || s.Code.Contains(searchString)
                                      || s.Role.Contains(searchString));
            }

            return View(await users.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,Role,Password")] User user)
        {
            // Check for duplicate code
            if (_context.Users.Any(u => u.Code == user.Code))
            {
                ModelState.AddModelError("Code", "A user with this code already exists.");
            }

            if (ModelState.IsValid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,Role,Password")] User user)
        {
            if (id != user.Id) return NotFound();

            if (_context.Users.Any(u => u.Code == user.Code && u.Id != user.Id))
            {
                ModelState.AddModelError("Code", "Another user already has this code.");
            }

            if (ModelState.IsValid)
            {
                if (!user.Password.StartsWith("$2"))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Prevent deleting yourself
            var currentId = HttpContext.Session.GetInt32("UserId");
            if (currentId == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

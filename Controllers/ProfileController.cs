using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartClinic.Data;
using SmartClinic.Models;
using SmartClinic.Filters;
using System.Threading.Tasks;

namespace SmartClinic.Controllers
{
    [RoleAuthorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SmartClinic.Services.EmailService _emailService;

        public ProfileController(ApplicationDbContext context, SmartClinic.Services.EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string name)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Name cannot be empty.");
                return View(user);
            }

            user.Name = name;
            await _context.SaveChangesAsync();

            // Update session name
            HttpContext.Session.SetString("UserName", user.Name);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestPasswordReset()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.PasswordResetToken = System.Guid.NewGuid().ToString();
            user.ResetTokenExpiry = System.DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = Url.Action("VerifyReset", "Auth", new { token = user.PasswordResetToken }, Request.Scheme);
            var emailBody = $"<h3>Password Reset Request</h3><p>Click the link below to complete authentication and reset your password:</p><p><a href='{resetLink}'>Complete Authentication</a></p><p>This link will expire in 1 hour.</p>";
            
            await _emailService.SendEmailAsync(user.Email, "Reset Your Password - SmartClinic", emailBody);

            TempData["Success"] = "A password reset link has been sent to your email.";
            return RedirectToAction(nameof(Index));
        }
    }
}

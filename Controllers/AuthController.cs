using Microsoft.AspNetCore.Mvc;
using SmartClinic.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SmartClinic.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SmartClinic.Services.EmailService _emailService;

        public AuthController(ApplicationDbContext context, SmartClinic.Services.EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserCode", user.Code);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("UserName", user.Name);
                return RedirectToAction("Index", "Dashboard");
            }

            TempData["Error"] = "Invalid email or password. Please try again.";
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.PasswordResetToken = Guid.NewGuid().ToString();
                user.ResetTokenExpiry = System.DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                var resetLink = Url.Action("VerifyReset", "Auth", new { token = user.PasswordResetToken }, Request.Scheme);
                var emailBody = $"<h3>Password Reset Request</h3><p>Click the link below to complete authentication and reset your password:</p><p><a href='{resetLink}'>Complete Authentication</a></p><p>This link will expire in 1 hour.</p>";
                
                await _emailService.SendEmailAsync(user.Email, "Reset Your Password - SmartClinic", emailBody);
            }

            // Always show the same success message to prevent email enumeration
            TempData["Success"] = "If an account with that email exists, a password reset link has been sent.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerifyReset(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.ResetTokenExpiry > System.DateTime.UtcNow);
            if (user == null)
            {
                TempData["Error"] = "Invalid or expired reset token.";
                return RedirectToAction("Login");
            }

            // Mark session as verified for password reset
            HttpContext.Session.SetString("VerifiedResetToken", token);
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var token = HttpContext.Session.GetString("VerifiedResetToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            var token = HttpContext.Session.GetString("VerifiedResetToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters long.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.ResetTokenExpiry > System.DateTime.UtcNow);
            if (user == null)
            {
                TempData["Error"] = "Session expired or invalid. Please request a new reset link.";
                return RedirectToAction("Login");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("VerifiedResetToken");
            TempData["Success"] = "Password has been successfully reset. You can now log in.";
            return RedirectToAction("Login");
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartClinic.Data;
using SmartClinic.Models;
using SmartClinic.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SmartClinic.Controllers
{
    [RoleAuthorize("Admin", "Doctor", "Reception", "Patient")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments — filtered by role, with optional search
        public async Task<IActionResult> Index(string searchString, string dateFilter, string statusFilter)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");
            var userCode = HttpContext.Session.GetString("UserCode");

            ViewData["CurrentFilter"] = searchString;
            ViewData["DateFilter"]    = dateFilter;
            ViewData["StatusFilter"]  = statusFilter;

            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (role == "Doctor")
            {
                appointments = appointments.Where(a => a.DoctorId == userId);
                ViewData["IsDoctor"] = true;
            }
            else if (role == "Patient")
            {
                appointments = appointments.Where(a => a.Patient.Code == userCode);
                ViewData["IsPatient"] = true;
            }
            else
            {
                ViewData["IsAdmin"] = (role == "Admin");
            }

            // Search by patient name or doctor name
            if (!string.IsNullOrEmpty(searchString))
            {
                appointments = appointments.Where(a =>
                    a.Patient.Name.Contains(searchString) ||
                    a.Doctor.Name.Contains(searchString));
            }

            // Filter by date
            if (!string.IsNullOrEmpty(dateFilter) && DateTime.TryParse(dateFilter, out var apptDate))
            {
                appointments = appointments.Where(a => a.AppointmentDate.Date == apptDate.Date);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                appointments = appointments.Where(a => a.Status == statusFilter);
            }

            return View(await appointments.OrderBy(a => a.AppointmentDate).ToListAsync());
        }

        // GET: Appointments/Create — Admin & Reception only
        [RoleAuthorize("Admin", "Reception")]
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(
                _context.Users.Where(u => u.Role == "Doctor"), "Id", "Name");
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name");
            return View();
        }

        // POST: Appointments/Create — Admin & Reception only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Reception")]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,AppointmentDate,Status")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment scheduled successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(
                _context.Users.Where(u => u.Role == "Doctor"), "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
            return View(appointment);
        }

        // POST: Appointments/UpdateStatus — Doctor only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Doctor")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null) return NotFound();

            // Ensure doctor can only update their own appointments
            if (appointment.DoctorId != userId)
                return RedirectToAction("AccessDenied", "Auth");

            var allowed = new[] { "Scheduled", "Completed", "Cancelled" };
            if (!allowed.Contains(status))
                return BadRequest();

            appointment.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Appointment marked as {status}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Appointments/Delete/5 — Admin & Reception only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Reception")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Appointment cancelled successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartClinic.Data;
using SmartClinic.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace SmartClinic.Controllers
{
    [RoleAuthorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");
            var userCode = HttpContext.Session.GetString("UserCode");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Role = role;

            if (role == "Admin")
            {
                ViewBag.TotalPatients = await _context.Patients.CountAsync();
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
                ViewBag.ScheduledAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Scheduled");
            }
            else if (role == "Doctor")
            {
                ViewBag.MyAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == userId);
                ViewBag.MyScheduled = await _context.Appointments
                    .CountAsync(a => a.DoctorId == userId && a.Status == "Scheduled");
                ViewBag.MyCompleted = await _context.Appointments
                    .CountAsync(a => a.DoctorId == userId && a.Status == "Completed");
            }
            else if (role == "Reception")
            {
                ViewBag.TotalPatients = await _context.Patients.CountAsync();
                ViewBag.TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == System.DateTime.Today);
                ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            }
            else if (role == "Patient")
            {
                var myAppointments = _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.Patient.Code == userCode);

                ViewBag.MyTotal = await myAppointments.CountAsync();
                ViewBag.MyUpcoming = await myAppointments
                    .CountAsync(a => a.Status == "Scheduled" && a.AppointmentDate >= System.DateTime.Today);
                ViewBag.NextAppointment = await myAppointments
                    .Where(a => a.Status == "Scheduled" && a.AppointmentDate >= System.DateTime.Today)
                    .OrderBy(a => a.AppointmentDate)
                    .FirstOrDefaultAsync();
            }

            return View();
        }
    }
}

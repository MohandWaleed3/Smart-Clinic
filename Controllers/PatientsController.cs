using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartClinic.Data;
using SmartClinic.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace SmartClinic.Controllers
{
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index(string searchString)
        {
            // Security Check: Only Admin and Reception can access
            var role = HttpContext.Session.GetString("Role");
            if (role != "Reception" && role != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["CurrentFilter"] = searchString;

            var patients = from p in _context.Patients
                           select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(s => s.Name.Contains(searchString)
                                       || s.Code.Contains(searchString));
            }

            return View(await patients.ToListAsync());
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Reception" && role != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Security mechanism: CSRF Protection
        public async Task<IActionResult> Create([Bind("Id,Name,Code,DateOfBirth,Phone")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient record created securely!";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient record deleted permanently.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

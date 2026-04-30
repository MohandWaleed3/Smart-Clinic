using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartClinic.Data;
using SmartClinic.Models;
using SmartClinic.Filters;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace SmartClinic.Controllers
{
    [RoleAuthorize("Admin", "Reception", "Doctor")]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index(string searchString, string dateFilter)
        {
            var role = HttpContext.Session.GetString("Role");
            ViewData["CurrentFilter"] = searchString;
            ViewData["DateFilter"] = dateFilter;
            ViewData["IsReadOnly"] = (role == "Admin" || role == "Doctor");

            var patients = from p in _context.Patients select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(s => s.Name.Contains(searchString)
                                            || s.Code.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(dateFilter) && DateTime.TryParse(dateFilter, out var dob))
            {
                patients = patients.Where(s => s.DateOfBirth.Date == dob.Date);
            }

            return View(await patients.ToListAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // GET: Patients/Create — Reception only
        [RoleAuthorize("Reception")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create — Reception only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Reception")]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,DateOfBirth,Phone")] Patient patient)
        {
            if (_context.Patients.Any(p => p.Code == patient.Code))
            {
                ModelState.AddModelError("Code", "A patient with this code already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5 — Reception only
        [RoleAuthorize("Reception")]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: Patients/Edit/5 — Reception only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Reception")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,DateOfBirth,Phone")] Patient patient)
        {
            if (id != patient.Id) return NotFound();

            if (_context.Patients.Any(p => p.Code == patient.Code && p.Id != patient.Id))
            {
                ModelState.AddModelError("Code", "Another patient already has this code.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // POST: Patients/Delete/5 — Reception only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Reception")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Patient record deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

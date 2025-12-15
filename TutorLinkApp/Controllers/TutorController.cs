using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;

namespace TutorLinkApp.Controllers
{
    public class TutorController : Controller
    {
        private readonly TutorLinkContext _context;
        public TutorController(TutorLinkContext context)
        {
            _context = context;
        }

        // Helper method: check if user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        // Helper method: check if user is admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // GET: Tutor
        public async Task<IActionResult> Index()
        {
            // Everyone can see tutors
            var tutors = await _context.Tutors
                .Include(t => t.User)
                .Where(t => t.DeletedAt == null)
                .ToListAsync();

            return View(tutors);
        }

        // GET: Tutor/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);

            if (tutor == null)
                return NotFound();

            return View(tutor);
        }

        // GET: Tutor/Create
        public IActionResult Create()
        {
            if (!IsLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to create a tutor profile.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Tutor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tutor model)
        {
            if (!IsLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to create a tutor profile.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                model.UserId = HttpContext.Session.GetInt32("UserId").Value;
                model.CreatedAt = DateTime.Now;

                _context.Tutors.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tutor profile created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Tutor/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to edit a tutor profile.";
                return RedirectToAction("Login", "Account");
            }

            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null)
                return NotFound();

            // Optional: allow only owner or admin to edit
            if (tutor.UserId != HttpContext.Session.GetInt32("UserId") && !IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction(nameof(Index));
            }

            return View(tutor);
        }

        // POST: Tutor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tutor model)
        {
            if (!IsLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to edit a tutor profile.";
                return RedirectToAction("Login", "Account");
            }

            if (id != model.Id)
                return NotFound();

            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null)
                return NotFound();

            if (tutor.UserId != HttpContext.Session.GetInt32("UserId") && !IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                tutor.Skill = model.Skill;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tutor profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Tutor/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to delete a tutor profile.";
                return RedirectToAction("Login", "Account");
            }

            var tutor = await _context.Tutors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);

            if (tutor == null)
                return NotFound();

            if (tutor.UserId != HttpContext.Session.GetInt32("UserId") && !IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction(nameof(Index));
            }

            return View(tutor);
        }

        // POST: Tutor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn())
            {
                TempData["ErrorMessage"] = "You must be logged in to delete a tutor profile.";
                return RedirectToAction("Login", "Account");
            }

            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null)
                return NotFound();

            if (tutor.UserId != HttpContext.Session.GetInt32("UserId") && !IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete
            tutor.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tutor profile deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

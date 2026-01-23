using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Controllers
{
    public class TutorController : Controller
    {
        private readonly ITutorService _tutorService;

        public TutorController(ITutorService tutorService)
        {
            _tutorService = tutorService;
        }

        public async Task<IActionResult> Index(TutorSearchViewModel filters)
        {
            // Provjera ModelState
            if (!ModelState.IsValid)
            {
                return View(filters);
            }

            var result = await _tutorService.SearchTutors(filters);
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Provjera ModelState
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction(nameof(Index));
            }

            var tutor = await _tutorService.GetTutorDetails(id);
            if (tutor == null)
            {
                TempData["ErrorMessage"] = "Tutor not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(tutor);
        }

        // Helper method: check if user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
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
    }
}

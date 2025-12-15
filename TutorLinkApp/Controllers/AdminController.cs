using Microsoft.AspNetCore.Mvc;
using TutorLinkApp.Models;

namespace TutorLinkApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ISessionManager _sessionManager;

        public AdminController(IAdminService adminService, ISessionManager sessionManager)
        {
            _adminService = adminService;
            _sessionManager = sessionManager;
        }

        private bool IsAdmin() => _sessionManager.GetUserSession(HttpContext)?.RoleName == "Admin";

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return RedirectToHomeWithError();

            ViewBag.TotalUsers = await _adminService.GetTotalUsers();
            ViewBag.TotalTutors = await _adminService.GetTotalTutors();
            ViewBag.TotalStudents = await _adminService.GetTotalStudents();

            return View();
        }

        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToHomeWithError();

            var users = await _adminService.GetAllUsers();
            return View(users);
        }

        public IActionResult CreateUser() => IsAdmin() ? View() : RedirectToHomeWithError();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!IsAdmin()) return RedirectToHomeWithError();

            if (!ModelState.IsValid) return View(model);

            await _adminService.CreateUser(model);
            TempData["SuccessMessage"] = $"User {model.Username} created successfully!";
            return RedirectToAction("Users");
        }

        private IActionResult RedirectToHomeWithError()
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToAction("Index", "Home");
        }

        // Ostale metode (EditUser, DeleteUser) se mogu refaktorisati na isti način
    }

}
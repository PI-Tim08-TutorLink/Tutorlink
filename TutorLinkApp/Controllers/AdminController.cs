using Microsoft.AspNetCore.Mvc;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ISessionManager _sessionManager;
        private readonly IAdminUserCreationService _adminUserCreationService;

        public AdminController(IAdminService adminService, ISessionManager sessionManager,IAdminUserCreationService adminUserCreationService)
        {
            _adminService = adminService;
            _adminUserCreationService = adminUserCreationService;
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

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int roleId = model.Role switch
            {
                "Admin" => 1,
                "Student" => 2,
                "Tutor" => 3,
                _ => 0
            };

            if (roleId == 0)
            {
                ModelState.AddModelError(nameof(model.Role), "Please select a valid role.");
                return View(model);
            }

            await _adminUserCreationService.CreateUser(model, roleId);
            return RedirectToAction(nameof(Users));
        }

        private IActionResult RedirectToHomeWithError()
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToAction("Index", "Home");
        }
    }
}
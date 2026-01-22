using Microsoft.AspNetCore.Mvc;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ISessionManager _sessionManager;
        private readonly IUserService _userService;

        public ProfileController(ISessionManager sessionManager, IUserService userService)
        {
            _sessionManager = sessionManager;
            _userService = userService;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var session = _sessionManager.GetUserSession(HttpContext);
            if (session == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to view your profile.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserById(session.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TutorLinkApp.DTO;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;
using TutorLinkApp.VM;

namespace TutorLinkApp.Controllers
{
    public class AccountController : Controller
    {
        private const string SuccessMessageKey = "SuccessMessage";

        private readonly IUserService _userService;
        private readonly ISessionManager _sessionManager;

        public AccountController(IUserService userService, ISessionManager sessionManager)
        {
            _userService = userService;
            _sessionManager = sessionManager;
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _userService.IsEmailTaken(model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered");
                return View(model);
            }

            if (await _userService.IsUsernameTaken(model.Username))
            {
                ModelState.AddModelError("Username", "Username already taken");
                return View(model);
            }

            await _userService.CreateUser(model);

            TempData[SuccessMessageKey] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            _sessionManager.ClearSession(HttpContext);
            TempData.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.AuthenticateUser(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(model);
            }

            _sessionManager.SetUserSession(HttpContext, new UserSession
            {
                UserId = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                RoleId = user.RoleId,
                RoleName = user.Role?.Role1 ?? "Student" // dohvat role iz navigacije Role
            });

            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";

            return user.Role?.Role1 == "Admin"
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            _sessionManager.ClearSession(HttpContext);
            TempData[SuccessMessageKey] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model,
            [FromServices] IResetPasswordFacade facade)
        {
            if (!ModelState.IsValid)
                return View(model);

            var link = await facade.SendResetLink(
                model.Email,
                Url.Action("ResetPassword", "Account", null, Request.Scheme)!
            );

            if (link == null)
            {
                TempData["ErrorMessage"] = "Email address does not exist.";
            }
            else
            {
                TempData[SuccessMessageKey] = "Reset link sent successfully.";
                TempData["ResetLink"] = link;
            }

            return RedirectToAction("ForgotPassword");
        }

        public IActionResult ResetPassword(string token)
        {
            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model,
            [FromServices] IResetPasswordFacade facade)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await facade.ResetPassword(model.Token, model.NewPassword);

            if (success)
            {
                TempData[SuccessMessageKey] = "Password reset successful!";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Invalid or expired token");
            return View(model);
        }
    }
}

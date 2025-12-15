using Microsoft.AspNetCore.Mvc;
using TutorLinkApp.DTO;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;

public class AccountController : Controller
{
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
        if (!ModelState.IsValid) return View(model);

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

        var user = await _userService.CreateUser(model);
        TempData["SuccessMessage"] = "Registration successful! Please log in.";
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
        if (!ModelState.IsValid) return View(model);

        var userWithRole = await _userService.AuthenticateUserWithRole(model.Email, model.Password);
        if (userWithRole == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(model);
        }

        _sessionManager.SetUserSession(HttpContext, new UserSession
        {
            UserId = userWithRole.User.Id,
            Username = userWithRole.User.Username,
            FirstName = userWithRole.User.FirstName,
            RoleId = userWithRole.User.RoleId,
            RoleName = userWithRole.RoleName
        });

        TempData["SuccessMessage"] = $"Welcome back, {userWithRole.User.FirstName}!";

        return userWithRole.RoleName == "Admin"
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        _sessionManager.ClearSession(HttpContext);
        TempData["SuccessMessage"] = "You have been logged out successfully.";
        return RedirectToAction("Login");
    }
}

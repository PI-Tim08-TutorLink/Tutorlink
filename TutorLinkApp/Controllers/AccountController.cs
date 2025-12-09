using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TutorLinkApp.Models;

namespace TutorLinkApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly TutorLinkContext _context;

        public AccountController(TutorLinkContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered");
                    return View(model);
                }

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already taken");
                    return View(model);
                }

                // Generate salt and hash password
                var salt = GenerateSalt();
                var hash = HashPassword(model.Password, salt);

                // Determine role ID (assuming: 1 = Admin, 2 = Student, 3 = Tutor)
                int roleId = model.Role.ToLower() == "tutor" ? 3 : 2;

                // Create new user
                var user = new User
                {
                    Email = model.Email,
                    Username = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PwdHash = hash,
                    PwdSalt = salt,
                    RoleId = roleId,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // If registering as tutor, create tutor record
                if (model.Role.ToLower() == "tutor" && !string.IsNullOrWhiteSpace(model.Skills))
                {
                    var tutor = new Tutor
                    {
                        UserId = user.Id,
                        Skill = model.Skills,
                        CreatedAt = DateTime.Now
                    };
                    _context.Tutors.Add(tutor);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            TempData.Clear();

            return View();
        }


        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.DeletedAt == null);

                if (user != null && VerifyPassword(model.Password, user.PwdHash, user.PwdSalt))
                {
                    // Get role name
                    var role = await _context.Roles.FindAsync(user.RoleId);
                    var roleName = role?.Role1 ?? "Student";

                    // Set session
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("UserRole", roleName);
                    HttpContext.Session.SetInt32("RoleId", user.RoleId);

                    TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";

                    // Redirect based on role
                    if (roleName == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "You have been logged out successfully.";

            return RedirectToAction("Login", "Account");
        }


        // Helper methods for password hashing
        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combined = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string hash = HashPassword(password, storedSalt);
            return hash == storedHash;
        }
    }
}
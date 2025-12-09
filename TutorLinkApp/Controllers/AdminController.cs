using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TutorLinkApp.Models;

namespace TutorLinkApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly TutorLinkContext _context;

        public AdminController(TutorLinkContext context)
        {
            _context = context;
        }

        // Check if user is admin
        private bool IsAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == "Admin";
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalUsers = await _context.Users.CountAsync(u => u.DeletedAt == null);
            ViewBag.TotalTutors = await _context.Tutors.CountAsync(t => t.DeletedAt == null);
            ViewBag.TotalStudents = await _context.Users.CountAsync(u => u.RoleId == 2 && u.DeletedAt == null);

            return View("~/Views/Admin/Dashboard.cshtml");
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            var users = await _context.Users
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Tutors)
                .ToListAsync();

            return View(users);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Role1");
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered");
                    ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Role1");
                    return View(model);
                }

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already taken");
                    ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Role1");
                    return View(model);
                }

                // Generate salt and hash password
                var salt = GenerateSalt();
                var hash = HashPassword(model.Password, salt);

                // Determine role ID based on role name
                int roleId = model.Role.ToLower() == "tutor" ? 3 :
                             model.Role.ToLower() == "admin" ? 1 : 2;

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

                // If creating as tutor, create tutor record
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

                TempData["SuccessMessage"] = $"User {user.Username} created successfully!";
                return RedirectToAction("Users");
            }

            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Role1");
            return View(model);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Role1", user.RoleId);
            return View(user);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User user)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser != null)
                    {
                        existingUser.FirstName = user.FirstName;
                        existingUser.LastName = user.LastName;
                        existingUser.Email = user.Email;
                        existingUser.Username = user.Username;
                        existingUser.RoleId = user.RoleId;

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "User updated successfully!";
                        return RedirectToAction("Users");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "Id", "Role1", user.RoleId);
            return View(user);
        }

        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Soft delete
                user.DeletedAt = DateTime.Now;

                // Also soft delete associated tutor profiles
                var tutors = await _context.Tutors.Where(t => t.UserId == id).ToListAsync();
                foreach (var tutor in tutors)
                {
                    tutor.DeletedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User {user.Username} deleted successfully!";
            }
            return RedirectToAction("Users");
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id && e.DeletedAt == null);
        }

        // Helper methods
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
    }
}
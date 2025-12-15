using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;

namespace TutorLinkApp.Services.Implementations
{
    public interface IUserService
    {
        Task<bool> IsEmailTaken(string email);
        Task<bool> IsUsernameTaken(string username);
        Task<User> CreateUser(RegisterViewModel model);
        Task<UserWithRole?> AuthenticateUserWithRole(string email, string password);
    }

    public class UserService : IUserService
    {
        private readonly TutorLinkContext _context;
        private readonly IPasswordHasher _hasher;

        public UserService(TutorLinkContext context, IPasswordHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<bool> IsEmailTaken(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

        public async Task<bool> IsUsernameTaken(string username)
            => await _context.Users.AnyAsync(u => u.Username == username);

        public async Task<User> CreateUser(RegisterViewModel model)
        {
            var salt = _hasher.GenerateSalt();
            var hash = _hasher.Hash(model.Password, salt);

            int roleId = model.Role.ToLower() == "tutor" ? 3 : 2;

            var user = new User
            {
                Email = model.Email,
                Username = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PwdSalt = salt,
                PwdHash = hash,
                RoleId = roleId,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (roleId == 3 && !string.IsNullOrWhiteSpace(model.Skills))
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

            return user;
        }

        public async Task<UserWithRole?> AuthenticateUserWithRole(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);

            if (user == null) return null;

            if (!_hasher.Verify(password, user.PwdHash, user.PwdSalt)) return null;

            var role = await _context.Roles.FindAsync(user.RoleId);
            return new UserWithRole
            {
                User = user,
                RoleName = role?.Role1 ?? "Student"
            };
        }
    }

}

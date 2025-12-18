using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private readonly TutorLinkContext _context;
        public UserManagementService(TutorLinkContext context) => _context = context;

        public Task<List<User>> GetAllUsers()
            => _context.Users.Where(u => u.DeletedAt == null).ToListAsync();

        public Task<User?> GetUserById(int id)
            => _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

        public async Task UpdateUser(User user)
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null) throw new KeyNotFoundException();

            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.Username = user.Username;
            existing.RoleId = user.RoleId;

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteUser(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return;

            u.DeletedAt = DateTime.Now;

            var tutors = await _context.Tutors.Where(t => t.UserId == id).ToListAsync();
            foreach (var t in tutors) t.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}

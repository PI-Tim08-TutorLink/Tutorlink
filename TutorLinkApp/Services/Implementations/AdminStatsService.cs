using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Implementations
{
    public class AdminStatsService : IAdminStatsService
    {
        private readonly TutorLinkContext _context;
        public AdminStatsService(TutorLinkContext context) => _context = context;

        public Task<int> GetTotalUsers()
            => _context.Users.CountAsync(u => u.DeletedAt == null);

        public Task<int> GetTotalTutors()
            => _context.Tutors.CountAsync(t => t.DeletedAt == null);

        public Task<int> GetTotalStudents()
            => _context.Users.CountAsync(u => u.DeletedAt == null && u.RoleId == RoleIds.Student);
    }
}

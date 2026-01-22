using TutorLinkApp.Models;
using TutorLinkApp.VM;

namespace TutorLinkApp.Services.Interfaces
{
    public interface IAdminService
    {
        Task<int> GetTotalUsers();
        Task<int> GetTotalTutors();
        Task<int> GetTotalStudents();
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(int id);
        Task CreateUser(RegisterViewModel model);
        Task UpdateUser(User user);
        Task SoftDeleteUser(int id);
        Task<List<Tutor>> GetTutorsByUserId(int userId);
    }
}

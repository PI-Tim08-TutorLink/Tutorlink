using TutorLinkApp.Models;

namespace TutorLinkApp.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(int id);
        Task UpdateUser(User user);
        Task SoftDeleteUser(int id);
    }
}

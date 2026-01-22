using TutorLinkApp.Models;
using TutorLinkApp.DTO;
using TutorLinkApp.VM;

namespace TutorLinkApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IsEmailTaken(string email);
        Task<bool> IsUsernameTaken(string username);
        Task<User> CreateUser(RegisterViewModel model);
        Task<User?> AuthenticateUser(string email, string password);
        Task<User?> GetUserById(int id);
    }
}

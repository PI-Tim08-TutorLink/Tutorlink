using TutorLinkApp.Models;

namespace TutorLinkApp.Services.Interfaces
{
    public interface IAdminUserCreationService
    {
        Task CreateUser(RegisterViewModel model, int roleId);
    }
}

using TutorLinkApp.Models;

namespace TutorLinkApp.Services.Interfaces
{
    public interface ITutorService
    {
        Task<TutorSearchViewModel> SearchTutors(TutorSearchViewModel filters);
        Task<TutorCardViewModel?> GetTutorDetails(int tutorId);
        Task<List<string>> GetAllSkills();
    }
}

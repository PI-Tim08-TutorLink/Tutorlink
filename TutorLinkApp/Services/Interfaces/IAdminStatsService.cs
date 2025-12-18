namespace TutorLinkApp.Services.Interfaces
{
    public interface IAdminStatsService
    {
        Task<int> GetTotalUsers();
        Task<int> GetTotalTutors();
        Task<int> GetTotalStudents();
    }
}

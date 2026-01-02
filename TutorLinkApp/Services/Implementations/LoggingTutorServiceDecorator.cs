using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;
using ILogger = TutorLinkApp.Services.Interfaces.ILogger;

namespace TutorLinkApp.Services.Implementations
{
    public class LoggingTutorServiceDecorator : ITutorService
    {
        private readonly ITutorService _inner;
        private readonly ILogger _logger;

        public LoggingTutorServiceDecorator(ITutorService inner, ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<List<string>> GetAllSkills()
        {
            _logger.LogInfo("GetAllSkills called");
            return await _inner.GetAllSkills();
        }

        public async Task<TutorCardViewModel?> GetTutorDetails(int tutorId)
        {
            _logger.LogInfo("GetTutorDetails called");
            return await _inner.GetTutorDetails(tutorId);
        }

        public async Task<TutorSearchViewModel> SearchTutors(TutorSearchViewModel filters)
        {
            _logger.LogInfo("SearchTutors called");
            return await _inner.SearchTutors(filters);
        }

    }
}

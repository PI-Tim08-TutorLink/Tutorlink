using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;
using ILogger = TutorLinkApp.Services.Interfaces.ILogger;

namespace TutorLinkApp.Services.Implementations
{
    public class TutorService : ITutorService
    {
        ILogger logger = AppLogger.GetInstance();
        private readonly TutorLinkContext _context;
        public TutorService(TutorLinkContext context)
        {
            _context = context;
        }

        private ITutorSortStrategy GetSortStrategy(string sortBy)
        {
            return sortBy switch
            {
                "rating" => new SortByRatingStrategy(),
                "price_asc" => new SortByPriceAscStrategy(),
                "price_desc" => new SortByPriceDescStrategy(),
                "newest" => new SortByNewestStrategy(),
                _ => new SortByRatingStrategy(), // Default: best rated first
            };
        }

        public async Task<TutorSearchViewModel> SearchTutors(TutorSearchViewModel filters)
        {
            var query = _context.Tutors
                .Include(t => t.User)
                .Where(t => t.DeletedAt == null && t.User.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.SearchSkill))
            {
                query = query.Where(t => t.Skill.ToLower().Contains(filters.SearchSkill.ToLower()));
            }

            if (filters.MinPrice.HasValue && filters.MinPrice.Value > 0)
            {
                query = query.Where(t => t.HourlyRate.HasValue && t.HourlyRate >= filters.MinPrice.Value);
            }
            if (filters.MaxPrice.HasValue && filters.MaxPrice.Value > 0)
            {
                query = query.Where(t => t.HourlyRate.HasValue && t.HourlyRate <= filters.MaxPrice.Value);
            }
            if (filters.MinRating.HasValue && filters.MinRating.Value > 0)
            {
                query = query.Where(t =>
                    t.AverageRating.HasValue &&
                    t.AverageRating >= filters.MinRating.Value);
            }

            var sortStrategy = GetSortStrategy(filters.SortBy ?? "");
            query = sortStrategy.ApplySort(query);

            var tutors = await query.ToListAsync();

            var result = new TutorSearchViewModel
            {
                SearchSkill = filters.SearchSkill,
                MinPrice = filters.MinPrice,
                MaxPrice = filters.MaxPrice,
                MinRating = filters.MinRating,
                SortBy = filters.SortBy,
                Tutors = tutors.Select(t => new TutorCardViewModel
                {
                    Id = t.Id,
                    FullName = $"{t.User.FirstName} {t.User.LastName}",
                    Username = t.User.Username,
                    Email = t.User.Email,
                    Skills = t.Skill.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim())
                                   .ToList(),
                    HourlyRate = t.HourlyRate,
                    AverageRating = t.AverageRating,
                    TotalReviews = t.TotalReviews,
                    Bio = t.Bio,
                    Availability = t.Availability,
                    IsAvailable = true
                }).ToList(),
                AvailableSkills = await GetAllSkills()
            };

            return result;
        }
        public async Task<List<string>> GetAllSkills()
        {
            var allSkills = await _context.Tutors
                .Where(t => t.DeletedAt == null && !string.IsNullOrEmpty(t.Skill))
                .Select(t => t.Skill)
                .ToListAsync();

            logger.LogInfo("All skills fetched for filtering tutors.");

            return allSkills
                .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        public async Task<TutorCardViewModel?> GetTutorDetails(int tutorId)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == tutorId && t.DeletedAt == null);

            if (tutor == null) return null;

            return new TutorCardViewModel
            {
                Id = tutor.Id,
                FullName = $"{tutor.User.FirstName} {tutor.User.LastName}",
                Username = tutor.User.Username,
                Email = tutor.User.Email,
                Skills = tutor.Skill.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim())
                                   .ToList(),
                HourlyRate = tutor.HourlyRate,
                AverageRating = tutor.AverageRating,
                TotalReviews = tutor.TotalReviews,
                Bio = tutor.Bio,
                Availability = tutor.Availability,
                IsAvailable = true
            };
        }

    }
}

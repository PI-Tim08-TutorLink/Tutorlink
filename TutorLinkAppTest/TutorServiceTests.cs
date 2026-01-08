using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;

namespace TutorLinkAppTest
{
    public class TutorServiceTests : IDisposable
    {
        private readonly TutorLinkContext _context;
        private readonly TutorService _tutorService;

        public TutorServiceTests()
        {
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TutorLinkContext(options);
            _tutorService = new TutorService(_context);

            SeedTestData();
        }

        // helper metode za kreiranje usera i tutor-a
        private User CreateUser(
            int id,
            string firstName = "Test",
            string lastName = "User",
            string? username = null,
            string? email = null,
            DateTime? deletedAt = null)
        {
            username ??= $"{firstName.ToLower()}{lastName.ToLower()}";
            email ??= $"{firstName.ToLower()}@test.com";

            return new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                Email = email,
                PwdHash = $"hash{id}",
                PwdSalt = $"salt{id}",
                DeletedAt = deletedAt
            };
        }

        private Tutor CreateTutor(
            int id,
            User user,
            string skill = "Math",
            decimal? hourlyRate = 50,
            decimal? averageRating = 4.5m,
            int totalReviews = 10,
            string? bio = null,
            string availability = "Flexible",
            DateTime? deletedAt = null)
        {
            bio ??= $"{user.FirstName}'s bio";

            return new Tutor
            {
                Id = id,
                UserId = user.Id,
                User = user,
                Skill = skill,
                HourlyRate = hourlyRate,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                Bio = bio,
                Availability = availability,
                DeletedAt = deletedAt
            };
        }

        private void AddUserAndTutor(User user, Tutor tutor)
        {
            _context.Users.Add(user);
            _context.Tutors.Add(tutor);
            _context.SaveChanges();
        }

        private void SeedTestData()
        {
            var user1 = CreateUser(1, "John", "Doe");
            var tutor1 = CreateTutor(1, user1, "Math, Physics", 50, 4.5m, 10, availability: "Monday-Friday");

            var user2 = CreateUser(2, "Jane", "Smith");
            var tutor2 = CreateTutor(2, user2, "English, History", 40, 4.8m, 15, availability: "Weekends");

            _context.Users.AddRange(user1, user2);
            _context.Tutors.AddRange(tutor1, tutor2);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task SearchTutors_NoFilters_ReturnsAllTutors()
        {
            var filters = new TutorSearchViewModel();

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_FilterBySkill_ReturnsMatchingTutors()
        {
            var filters = new TutorSearchViewModel { SearchSkill = "Math" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Single(result.Tutors);
            Assert.Contains("Math", result.Tutors[0].Skills);
        }

        [Fact]
        public async Task SearchTutors_FilterBySkill_CaseInsensitive()
        {
            var filters = new TutorSearchViewModel { SearchSkill = "math" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Single(result.Tutors);
            Assert.Contains("Math", result.Tutors[0].Skills);
        }

        [Fact]
        public async Task SearchTutors_FilterByMinPrice_ReturnsOnlyTutorsAbovePrice()
        {
            var filters = new TutorSearchViewModel { MinPrice = 45 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Single(result.Tutors);
            Assert.Equal(50, result.Tutors[0].HourlyRate);
        }

        [Fact]
        public async Task SearchTutors_FilterByMaxPrice_ReturnsOnlyTutorsBelowPrice()
        {
            var filters = new TutorSearchViewModel { MaxPrice = 45 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Single(result.Tutors);
            Assert.Equal(40, result.Tutors[0].HourlyRate);
        }

        [Fact]
        public async Task SearchTutors_FilterByPriceRange_ReturnsMatchingTutors()
        {
            var filters = new TutorSearchViewModel { MinPrice = 35, MaxPrice = 55 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_FilterByMinRating_ReturnsOnlyHighRatedTutors()
        {
            var filters = new TutorSearchViewModel { MinRating = 4.7m };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Single(result.Tutors);
            Assert.True(result.Tutors[0].AverageRating >= 4.7m);
        }

        [Fact]
        public async Task SearchTutors_SortByRating_ReturnsHighestRatedFirst()
        {
            var filters = new TutorSearchViewModel { SortBy = "rating" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
            Assert.True(result.Tutors[0].AverageRating >= result.Tutors[1].AverageRating);
        }

        [Fact]
        public async Task SearchTutors_SortByPriceAsc_ReturnsLowestPriceFirst()
        {
            var filters = new TutorSearchViewModel { SortBy = "price_asc" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
            Assert.True(result.Tutors[0].HourlyRate <= result.Tutors[1].HourlyRate);
        }

        [Fact]
        public async Task SearchTutors_SortByPriceDesc_ReturnsHighestPriceFirst()
        {
            var filters = new TutorSearchViewModel { SortBy = "price_desc" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
            Assert.True(result.Tutors[0].HourlyRate >= result.Tutors[1].HourlyRate);
        }

        [Fact]
        public async Task SearchTutors_CombinedFilters_ReturnsCorrectResults()
        {
            var filters = new TutorSearchViewModel
            {
                SearchSkill = "Math",
                MinPrice = 40,
                MinRating = 4.0m,
                SortBy = "rating"
            };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Single(result.Tutors);
            Assert.Contains("Math", result.Tutors[0].Skills);
            Assert.True(result.Tutors[0].HourlyRate >= 40);
            Assert.True(result.Tutors[0].AverageRating >= 4.0m);
        }

        [Fact]
        public async Task SearchTutors_ExcludesDeletedTutors()
        {
            var deletedUser = CreateUser(3, "Deleted", "User");
            var deletedTutor = CreateTutor(3, deletedUser, "Chemistry", 60, deletedAt: DateTime.UtcNow);

            AddUserAndTutor(deletedUser, deletedTutor);

            var filters = new TutorSearchViewModel();

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_SortByNewest_ReturnsNewestFirst()
        {
            var filters = new TutorSearchViewModel { SortBy = "newest" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_SortByUnknown_UsesDefaultRatingSorting()
        {
            var filters = new TutorSearchViewModel { SortBy = "invalid_sort" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.True(result.Tutors[0].AverageRating >= result.Tutors[1].AverageRating);
        }

        [Fact]
        public async Task SearchTutors_SortByNull_UsesDefaultRatingSorting()
        {
            var filters = new TutorSearchViewModel { SortBy = null };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_MinPriceZero_IgnoresFilter()
        {
            var filters = new TutorSearchViewModel { MinPrice = 0 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Equal(2, result.Tutors.Count); // Vraća sve tutore
        }

        [Fact]
        public async Task SearchTutors_MinPriceNegative_IgnoresFilter()
        {
            var filters = new TutorSearchViewModel { MinPrice = -10 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_MaxPriceZero_IgnoresFilter()
        {
            var filters = new TutorSearchViewModel { MaxPrice = 0 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_MinRatingZero_IgnoresFilter()
        {
            var filters = new TutorSearchViewModel { MinRating = 0 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_AllFiltersNull_ReturnsAllTutors()
        {
            var filters = new TutorSearchViewModel
            {
                MinPrice = null,
                MaxPrice = null,
                MinRating = null,
                SearchSkill = null
            };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_TutorWithNullHourlyRate_ExcludedFromPriceFilter()
        {
            var user = CreateUser(10, "No", "Price");
            var tutor = CreateTutor(10, user, "Math", hourlyRate: null);
            AddUserAndTutor(user, tutor);

            var filters = new TutorSearchViewModel { MinPrice = 10 };

            var result = await _tutorService.SearchTutors(filters);

            Assert.DoesNotContain(result.Tutors, t => t.Id == 10);
        }

        [Fact]
        public async Task SearchTutors_TutorWithNullRating_ExcludedFromRatingFilter()
        {
            var user = CreateUser(11, "No", "Rating");
            var tutor = CreateTutor(11, user, "Math", averageRating: null);
            AddUserAndTutor(user, tutor);

            var filters = new TutorSearchViewModel { MinRating = 4.0m };

            var result = await _tutorService.SearchTutors(filters);

            Assert.DoesNotContain(result.Tutors, t => t.Id == 11);
        }

        [Fact]
        public async Task SearchTutors_TutorWithNullHourlyRate_IncludedWithoutPriceFilter()
        {
            var user = CreateUser(12, "Free", "Tutor");
            var tutor = CreateTutor(12, user, "Math", hourlyRate: null);
            AddUserAndTutor(user, tutor);

            var filters = new TutorSearchViewModel();

            var result = await _tutorService.SearchTutors(filters);

            Assert.Contains(result.Tutors, t => t.Id == 12);
        }

        [Fact]
        public async Task SearchTutors_EmptySearchSkill_ReturnsAllTutors()
        {
            var filters = new TutorSearchViewModel { SearchSkill = "" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_WhitespaceSearchSkill_ReturnsAllTutors()
        {
            var filters = new TutorSearchViewModel { SearchSkill = "   " };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Equal(2, result.Tutors.Count);
        }

        [Fact]
        public async Task SearchTutors_PartialSkillMatch_ReturnsTutor()
        {
            var filters = new TutorSearchViewModel { SearchSkill = "Mat" };

            var result = await _tutorService.SearchTutors(filters);

            Assert.Single(result.Tutors);
            Assert.Contains("Math", result.Tutors[0].Skills);
        }

        [Fact]
        public async Task SearchTutors_NoMatchingResults_ReturnsEmptyList()
        {
            var filters = new TutorSearchViewModel
            {
                SearchSkill = "NonExistentSkill",
                MinPrice = 1000
            };

            var result = await _tutorService.SearchTutors(filters);

            Assert.NotNull(result);
            Assert.Empty(result.Tutors);
        }

        [Fact]
        public async Task GetAllSkills_ReturnsDistinctSkills()
        {
            var skills = await _tutorService.GetAllSkills();

            Assert.NotNull(skills);
            Assert.Contains("Math", skills);
            Assert.Contains("Physics", skills);
            Assert.Contains("English", skills);
            Assert.Contains("History", skills);
        }

        [Fact]
        public async Task GetAllSkills_ReturnsSortedSkills()
        {
            var skills = await _tutorService.GetAllSkills();

            Assert.NotNull(skills);
            var sortedSkills = skills.OrderBy(s => s).ToList();
            Assert.Equal(sortedSkills, skills);
        }

        [Fact]
        public async Task GetAllSkills_TrimsWhitespace()
        {
            var user = CreateUser(3, "Test", "User");
            var tutor = CreateTutor(3, user, "  Biology  , Chemistry  ", 30);

            AddUserAndTutor(user, tutor);

            var skills = await _tutorService.GetAllSkills();

            Assert.Contains("Biology", skills);
            Assert.Contains("Chemistry", skills);
            Assert.DoesNotContain("  Biology  ", skills);
        }

        [Fact]
        public async Task GetAllSkills_ExcludesDeletedTutors()
        {
            var deletedUser = CreateUser(4, "Deleted", "Tutor", deletedAt: DateTime.UtcNow);
            var deletedTutor = CreateTutor(4, deletedUser, "DeletedSkill", 100, deletedAt: DateTime.UtcNow);

            AddUserAndTutor(deletedUser, deletedTutor);

            var skills = await _tutorService.GetAllSkills();

            Assert.DoesNotContain("DeletedSkill", skills);
        }

        [Fact]
        public async Task GetAllSkills_EmptySkillString_ExcludesFromResults()
        {
            var user = CreateUser(13, "Empty", "Skill");
            var tutor = CreateTutor(13, user, "Math, , English", 30);
            AddUserAndTutor(user, tutor);

            var skills = await _tutorService.GetAllSkills();

            Assert.Contains("Math", skills);
            Assert.Contains("English", skills);
            // Provjerava da nema praznih string-ova
            Assert.DoesNotContain("", skills);
        }

        [Fact]
        public async Task GetAllSkills_TutorWithEmptySkillString_ExcludesFromResults()
        {
            var user = CreateUser(14, "Empty", "Skill");
            var tutor = CreateTutor(14, user, skill: "", hourlyRate: 30);
            _context.Users.Add(user);
            _context.Tutors.Add(tutor);
            _context.SaveChanges();

            var skills = await _tutorService.GetAllSkills();

            // Ne bi trebalo sadržavati prazne skillove
            Assert.NotNull(skills);
            Assert.DoesNotContain("", skills);
        }

        [Fact]
        public async Task GetTutorDetails_ValidId_ReturnsTutorDetails()
        {
            var result = await _tutorService.GetTutorDetails(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("johndoe", result.Username);
            Assert.Equal("john@test.com", result.Email);
            Assert.Contains("Math", result.Skills);
            Assert.Equal(50, result.HourlyRate);
            Assert.Equal(4.5m, result.AverageRating);
        }

        [Fact]
        public async Task GetTutorDetails_InvalidId_ReturnsNull()
        {
            var result = await _tutorService.GetTutorDetails(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTutorDetails_DeletedTutor_ReturnsNull()
        {
            var deletedUser = CreateUser(5, "Deleted", "User");
            var deletedTutor = CreateTutor(5, deletedUser, "Test", 50, deletedAt: DateTime.UtcNow);

            AddUserAndTutor(deletedUser, deletedTutor);

            var result = await _tutorService.GetTutorDetails(5);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTutorDetails_ParsesSkillsCorrectly()
        {
            var result = await _tutorService.GetTutorDetails(1);

            Assert.NotNull(result);
            Assert.Equal(2, result.Skills.Count);
            Assert.Contains("Math", result.Skills);
            Assert.Contains("Physics", result.Skills);
        }
    }
}

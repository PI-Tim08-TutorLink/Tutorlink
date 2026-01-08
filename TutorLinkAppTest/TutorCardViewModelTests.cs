using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;

namespace TutorLinkAppTest
{
    public class TutorCardViewModelTests
    {
        [Fact]
        public void TutorCardViewModel_DefaultValues_AreCorrect()
        {
            var model = new TutorCardViewModel();

            Assert.Equal(0, model.Id);
            Assert.NotNull(model.Skills);
            Assert.Empty(model.Skills);
            Assert.Equal(0, model.TotalReviews);
            Assert.False(model.IsAvailable);
        }

        [Fact]
        public void TutorCardViewModel_CanSetAndGetAllProperties()
        {
            var model = new TutorCardViewModel
            {
                Id = 1,
                FullName = "John Doe",
                Username = "johndoe",
                Email = "john@test.com",
                HourlyRate = 50,
                AverageRating = 4.5m,
                TotalReviews = 10,
                Bio = "Experienced tutor",
                Availability = "Monday-Friday",
                IsAvailable = true
            };

            Assert.Equal(1, model.Id);
            Assert.Equal("John Doe", model.FullName);
            Assert.Equal("johndoe", model.Username);
            Assert.Equal("john@test.com", model.Email);
            Assert.Equal(50, model.HourlyRate);
            Assert.Equal(4.5m, model.AverageRating);
            Assert.Equal(10, model.TotalReviews);
            Assert.Equal("Experienced tutor", model.Bio);
            Assert.Equal("Monday-Friday", model.Availability);
            Assert.True(model.IsAvailable);
        }

        [Fact]
        public void TutorCardViewModel_Skills_CanAddItems()
        {
            var model = new TutorCardViewModel();

            model.Skills.Add("Math");
            model.Skills.Add("Physics");

            Assert.Equal(2, model.Skills.Count);
            Assert.Contains("Math", model.Skills);
            Assert.Contains("Physics", model.Skills);
        }

        [Fact]
        public void TutorCardViewModel_NullableFields_CanBeNull()
        {
            var model = new TutorCardViewModel
            {
                HourlyRate = null,
                AverageRating = null,
                Bio = null,
                Availability = null
            };

            Assert.Null(model.HourlyRate);
            Assert.Null(model.AverageRating);
            Assert.Null(model.Bio);
            Assert.Null(model.Availability);
        }
    }
}

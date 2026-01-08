using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;

namespace TutorLinkAppTest
{
    public class TutorSearchViewModelTests
    {
        [Fact]
        public void TutorSearchViewModel_DefaultValues_AreCorrect()
        {
            var model = new TutorSearchViewModel();

            Assert.Null(model.SearchSkill);
            Assert.Null(model.MinPrice);
            Assert.Null(model.MaxPrice);
            Assert.Null(model.MinRating);
            Assert.Null(model.SortBy);
            Assert.NotNull(model.Tutors);
            Assert.Empty(model.Tutors);
            Assert.NotNull(model.AvailableSkills);
            Assert.Empty(model.AvailableSkills);
        }

        [Fact]
        public void TutorSearchViewModel_CanSetAndGetAllProperties()
        {
            var model = new TutorSearchViewModel
            {
                SearchSkill = "Math",
                MinPrice = 30,
                MaxPrice = 100,
                MinRating = 4.0m,
                SortBy = "rating"
            };

            Assert.Equal("Math", model.SearchSkill);
            Assert.Equal(30, model.MinPrice);
            Assert.Equal(100, model.MaxPrice);
            Assert.Equal(4.0m, model.MinRating);
            Assert.Equal("rating", model.SortBy);
        }

        [Fact]
        public void TutorSearchViewModel_Tutors_CanAddItems()
        {
            var model = new TutorSearchViewModel();
            var tutor = new TutorCardViewModel { Id = 1, FullName = "John Doe" };

            model.Tutors.Add(tutor);

            Assert.Single(model.Tutors);
            Assert.Contains(tutor, model.Tutors);
        }

        [Fact]
        public void TutorSearchViewModel_AvailableSkills_CanAddItems()
        {
            var model = new TutorSearchViewModel();

            model.AvailableSkills.Add("Math");
            model.AvailableSkills.Add("Physics");

            Assert.Equal(2, model.AvailableSkills.Count);
            Assert.Contains("Math", model.AvailableSkills);
            Assert.Contains("Physics", model.AvailableSkills);
        }
    }
}

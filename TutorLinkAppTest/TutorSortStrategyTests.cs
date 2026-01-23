using System;
using System.Collections.Generic;
using System.Linq;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;

namespace TutorLinkAppTest
{
    public class TutorSortStrategyTests
    {
        private List<Tutor> CreateTestTutors()
        {
            return new List<Tutor>
            {
                new Tutor
                {
                    Id = 1,
                    HourlyRate = 50,
                    AverageRating = 4.5m,
                    TotalReviews = 10,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Tutor
                {
                    Id = 2,
                    HourlyRate = 60,
                    AverageRating = 4.8m,
                    TotalReviews = 20,
                    CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Tutor
                {
                    Id = 3,
                    HourlyRate = 70,
                    AverageRating = 4.2m,
                    TotalReviews = 5,
                    CreatedAt = new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Tutor
                {
                    Id = 4,
                    HourlyRate = null,
                    AverageRating = null,
                    TotalReviews = 0,
                    CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            };
        }

        [Fact]
        public void SortByRatingStrategy_SortsDescendingByRating()
        {
            var tutors = CreateTestTutors();
            var query = tutors.AsQueryable();
            var strategy = new SortByRatingStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(4, result.Count);
            Assert.Equal(4.8m, result[0].AverageRating);
            Assert.Equal(4.5m, result[1].AverageRating);
            Assert.Equal(4.2m, result[2].AverageRating);
            Assert.Null(result[3].AverageRating);
        }

        [Fact]
        public void SortByRatingStrategy_TieBreaksByTotalReviews()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, AverageRating = 4.5m, TotalReviews = 5 },
                new Tutor { Id = 2, AverageRating = 4.5m, TotalReviews = 20 },
                new Tutor { Id = 3, AverageRating = 4.5m, TotalReviews = 10 }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByRatingStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(20, result[0].TotalReviews);
            Assert.Equal(10, result[1].TotalReviews);
            Assert.Equal(5, result[2].TotalReviews);
        }

        [Fact]
        public void SortByRatingStrategy_HandlesNullRatings()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, AverageRating = null, TotalReviews = 10 },
                new Tutor { Id = 2, AverageRating = 4.5m, TotalReviews = 5 },
                new Tutor { Id = 3, AverageRating = null, TotalReviews = 3 }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByRatingStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(2, result[0].Id);
            Assert.Null(result[1].AverageRating);
            Assert.Null(result[2].AverageRating);
        }

        [Fact]
        public void SortByRatingStrategy_AllNullRatings_SortsByReviews()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, AverageRating = null, TotalReviews = 5 },
                new Tutor { Id = 2, AverageRating = null, TotalReviews = 20 },
                new Tutor { Id = 3, AverageRating = null, TotalReviews = 10 }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByRatingStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(20, result[0].TotalReviews);
            Assert.Equal(10, result[1].TotalReviews);
            Assert.Equal(5, result[2].TotalReviews);
        }

        [Fact]
        public void SortByRatingStrategy_EmptyList_ReturnsEmptyList()
        {
            var tutors = new List<Tutor>();
            var query = tutors.AsQueryable();
            var strategy = new SortByRatingStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void SortByRatingStrategy_SingleTutor_ReturnsSameTutor()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, AverageRating = 4.5m, TotalReviews = 10 }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByRatingStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void SortByPriceAscStrategy_SortsAscendingByPrice()
        {
            var tutors = CreateTestTutors();
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceAscStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(4, result.Count);
            Assert.Equal(50, result[0].HourlyRate);
            Assert.Equal(60, result[1].HourlyRate);
            Assert.Equal(70, result[2].HourlyRate);
            Assert.Null(result[3].HourlyRate);
        }

        [Fact]
        public void SortByPriceAscStrategy_HandlesNullPrices()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, HourlyRate = 50 },
                new Tutor { Id = 2, HourlyRate = null },
                new Tutor { Id = 3, HourlyRate = 30 },
                new Tutor { Id = 4, HourlyRate = null }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceAscStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(30, result[0].HourlyRate);
            Assert.Equal(50, result[1].HourlyRate);
            Assert.Null(result[2].HourlyRate);
            Assert.Null(result[3].HourlyRate);
        }

        [Fact]
        public void SortByPriceAscStrategy_AllSamePrice_MaintainsOrder()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, HourlyRate = 50 },
                new Tutor { Id = 2, HourlyRate = 50 },
                new Tutor { Id = 3, HourlyRate = 50 }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceAscStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(3, result.Count);
            Assert.All(result, t => Assert.Equal(50, t.HourlyRate));
        }

        [Fact]
        public void SortByPriceAscStrategy_EmptyList_ReturnsEmptyList()
        {
            var tutors = new List<Tutor>();
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceAscStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void SortByPriceDescStrategy_SortsDescendingByPrice()
        {
            var tutors = CreateTestTutors();
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceDescStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(4, result.Count);
            Assert.Equal(70, result[0].HourlyRate);
            Assert.Equal(60, result[1].HourlyRate);
            Assert.Equal(50, result[2].HourlyRate);
            Assert.Null(result[3].HourlyRate);
        }

        [Fact]
        public void SortByPriceDescStrategy_HandlesNullPrices()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, HourlyRate = null },
                new Tutor { Id = 2, HourlyRate = 50 },
                new Tutor { Id = 3, HourlyRate = null },
                new Tutor { Id = 4, HourlyRate = 70 }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceDescStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(70, result[0].HourlyRate);
            Assert.Equal(50, result[1].HourlyRate);
            Assert.Null(result[2].HourlyRate);
            Assert.Null(result[3].HourlyRate);
        }

        [Fact]
        public void SortByPriceDescStrategy_AllNullPrices_MaintainsOrder()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, HourlyRate = null },
                new Tutor { Id = 2, HourlyRate = null },
                new Tutor { Id = 3, HourlyRate = null }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceDescStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(3, result.Count);
            Assert.All(result, t => Assert.Null(t.HourlyRate));
        }

        [Fact]
        public void SortByPriceDescStrategy_EmptyList_ReturnsEmptyList()
        {
            var tutors = new List<Tutor>();
            var query = tutors.AsQueryable();
            var strategy = new SortByPriceDescStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void SortByNewestStrategy_SortsDescendingByCreatedAt()
        {
            var tutors = CreateTestTutors();
            var query = tutors.AsQueryable();
            var strategy = new SortByNewestStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(4, result.Count);
            Assert.Equal(new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc), result[0].CreatedAt);
            Assert.Equal(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), result[1].CreatedAt);
            Assert.Equal(new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), result[2].CreatedAt);
            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), result[3].CreatedAt);
        }

        [Fact]
        public void SortByNewestStrategy_SameDate_MaintainsOrder()
        {
            var sameDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, CreatedAt = sameDate },
                new Tutor { Id = 2, CreatedAt = sameDate },
                new Tutor { Id = 3, CreatedAt = sameDate }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByNewestStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(3, result.Count);
            Assert.All(result, t => Assert.Equal(sameDate, t.CreatedAt));
        }

        [Fact]
        public void SortByNewestStrategy_DifferentTimes_SortsCorrectly()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, CreatedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc) },
                new Tutor { Id = 2, CreatedAt = new DateTime(2024, 6, 1, 14, 30, 0, DateTimeKind.Utc) },
                new Tutor { Id = 3, CreatedAt = new DateTime(2024, 6, 1, 9, 15, 0, DateTimeKind.Utc) }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByNewestStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
            Assert.Equal(3, result[2].Id);
        }

        [Fact]
        public void SortByNewestStrategy_EmptyList_ReturnsEmptyList()
        {
            var tutors = new List<Tutor>();
            var query = tutors.AsQueryable();
            var strategy = new SortByNewestStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void SortByNewestStrategy_SingleTutor_ReturnsSameTutor()
        {
            var tutors = new List<Tutor>
            {
                new Tutor { Id = 1, CreatedAt = DateTime.UtcNow }
            };
            var query = tutors.AsQueryable();
            var strategy = new SortByNewestStrategy();

            var result = strategy.ApplySort(query).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void DifferentStrategies_ProduceDifferentResults()
        {
            var tutors = CreateTestTutors();
            var query = tutors.AsQueryable();

            var ratingStrategy = new SortByRatingStrategy();
            var priceAscStrategy = new SortByPriceAscStrategy();
            var priceDescStrategy = new SortByPriceDescStrategy();
            var newestStrategy = new SortByNewestStrategy();

            var resultRating = ratingStrategy.ApplySort(query).ToList();
            var resultPriceAsc = priceAscStrategy.ApplySort(query).ToList();
            var resultPriceDesc = priceDescStrategy.ApplySort(query).ToList();
            var resultNewest = newestStrategy.ApplySort(query).ToList();

            Assert.NotEqual(resultRating[0].Id, resultPriceAsc[0].Id);
            Assert.NotEqual(resultRating[0].Id, resultPriceDesc[0].Id);
            Assert.NotEqual(resultRating[0].Id, resultNewest[0].Id);
        }
    }
}
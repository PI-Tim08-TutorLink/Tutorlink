using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Implementations
{
    public class SortByRatingStrategy : ITutorSortStrategy
    {
        public IQueryable<Tutor> ApplySort(IQueryable<Tutor> query)
        {
            return query.OrderByDescending(t => t.AverageRating ?? 0)
                        .ThenByDescending(t => t.TotalReviews);
        }
    }
}

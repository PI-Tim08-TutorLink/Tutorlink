using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Implementations
{
    public class SortByNewestStrategy : ITutorSortStrategy
    {
        public IQueryable<Tutor> ApplySort(IQueryable<Tutor> query)
        {
            return query.OrderByDescending(t => t.CreatedAt);
        }
    }
}

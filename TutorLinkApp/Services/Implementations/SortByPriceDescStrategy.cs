using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Implementations
{
    public class SortByPriceDescStrategy : ITutorSortStrategy
    {
        public IQueryable<Tutor> ApplySort(IQueryable<Tutor> query)
        {
            return query.OrderByDescending(t => t.HourlyRate ?? 0);
        }
    }
}

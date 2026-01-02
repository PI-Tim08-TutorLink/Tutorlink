using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Implementations
{
    public class SortByPriceAscStrategy : ITutorSortStrategy
    {
        public IQueryable<Tutor> ApplySort(IQueryable<Tutor> query)
        {
            return query.OrderBy(t => t.HourlyRate ?? decimal.MaxValue);
        }
    }
}
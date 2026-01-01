using TutorLinkApp.Models;

namespace TutorLinkApp.Services.Interfaces
{
    public interface ITutorSortStrategy
    {
        IQueryable<Tutor> ApplySort(IQueryable<Tutor> query);
    }
}

namespace TutorLinkApp.Models
{
    public class TutorSearchViewModel
    {
        public string? SearchSkill { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public string? SortBy { get; set; }

        public List<TutorCardViewModel> Tutors { get; set; } = new List<TutorCardViewModel>();

        public List<string> AvailableSkills { get; set; } = new List<string>();
    }
}

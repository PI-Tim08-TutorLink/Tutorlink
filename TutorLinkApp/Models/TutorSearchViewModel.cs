namespace TutorLinkApp.Models
{
    public class TutorSearchViewModel
    {
        public string? SearchSkill { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public string? SortBy { get; set; } // "rating", "price_asc", "price_desc", "newest"

        // Results
        public List<TutorCardViewModel> Tutors { get; set; } = new List<TutorCardViewModel>();

        // For UI (dropdowns, etc.)
        public List<string> AvailableSkills { get; set; } = new List<string>();
    }
}

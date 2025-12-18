namespace TutorLinkApp.Models
{
    public class TutorCardViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<string> Skills { get; set; } = new List<string>();
        public decimal? HourlyRate { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string? Bio { get; set; }
        public string? Availability { get; set; }
        public bool IsAvailable { get; set; } // Will implement later with TimeSlots
    }
}

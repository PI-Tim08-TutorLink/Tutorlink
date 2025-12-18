using System;
using System.Collections.Generic;

namespace TutorLinkApp.Models;

public partial class Tutor
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Skill { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public decimal? HourlyRate { get; set; }

    public decimal? AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public string? Bio { get; set; }

    public string? Availability { get; set; }

    public virtual User User { get; set; } = null!;
}

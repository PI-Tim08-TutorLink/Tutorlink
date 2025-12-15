using TutorLinkApp.Models;

public class UserWithRole
{
    public User User { get; set; } = default!;
    public string RoleName { get; set; } = "Student";
}
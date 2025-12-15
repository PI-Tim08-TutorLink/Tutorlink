namespace TutorLinkApp.DTO
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string RoleName { get; set; } = "Student";
        public int RoleId { get; set; }
    }
}

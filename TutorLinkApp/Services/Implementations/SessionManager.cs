using TutorLinkApp.DTO;

namespace TutorLinkApp.Services.Implementations
{
    public class SessionManager : ISessionManager
    {
        public void SetUserSession(HttpContext httpContext, UserSession session)
        {
            httpContext.Session.SetInt32("UserId", session.UserId);
            httpContext.Session.SetString("Username", session.Username);
            httpContext.Session.SetString("FirstName", session.FirstName);
            httpContext.Session.SetString("UserRole", session.RoleName);
            httpContext.Session.SetInt32("RoleId", session.RoleId);
        }

        public void ClearSession(HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }

        public UserSession? GetUserSession(HttpContext httpContext)
        {
            var userId = httpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return null;

            return new UserSession
            {
                UserId = userId.Value,
                Username = httpContext.Session.GetString("Username") ?? string.Empty,
                FirstName = httpContext.Session.GetString("FirstName") ?? string.Empty,
                RoleName = httpContext.Session.GetString("UserRole") ?? "Student",
                RoleId = httpContext.Session.GetInt32("RoleId") ?? 0
            };
        }
    }
}

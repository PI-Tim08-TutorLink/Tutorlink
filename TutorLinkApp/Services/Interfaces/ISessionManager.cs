using TutorLinkApp.DTO;

public interface ISessionManager
{
    void SetUserSession(HttpContext httpContext, UserSession session);
    void ClearSession(HttpContext httpContext);
    UserSession? GetUserSession(HttpContext httpContext);
}
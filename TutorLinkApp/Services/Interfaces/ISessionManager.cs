using Microsoft.AspNetCore.Http;
using TutorLinkApp.DTO;

namespace TutorLinkApp.Services.Interfaces
{
    public interface ISessionManager
    {
        void SetUserSession(HttpContext httpContext, UserSession session);
        void ClearSession(HttpContext httpContext);
        UserSession? GetUserSession(HttpContext httpContext);
    }
}

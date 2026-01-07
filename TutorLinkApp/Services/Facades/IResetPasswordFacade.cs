namespace TutorLinkApp.Services.Implementations
{
    public interface IResetPasswordFacade
    {
        Task<string?> SendResetLink(string email, string resetUrlBase);
        Task<bool> ResetPassword(string token, string newPassword);
    }
}

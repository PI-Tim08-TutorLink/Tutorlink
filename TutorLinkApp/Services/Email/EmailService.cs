using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Email
{
    public class EmailService
    {
        private readonly IEmailSender _sender;

        public EmailService(IEmailSender sender)
        {
            _sender = sender;
        }

        public Task SendResetPasswordEmail(string email, string link)
        {
            return _sender.SendAsync(
                email,
                "Reset password",
                $"Click the link to reset your password:\n{link}"
            );
        }
    }
}

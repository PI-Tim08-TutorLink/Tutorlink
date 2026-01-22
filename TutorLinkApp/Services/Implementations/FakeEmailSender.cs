using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Implementations
{
    public class FakeEmailSender : IEmailSender
    {
        private readonly ILogger<FakeEmailSender> _logger;

        public FakeEmailSender(ILogger<FakeEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            _logger.LogInformation("FAKE EMAIL SENT");

            _logger.LogInformation(
                "Email details | To: {To} | Subject: {Subject} | Body: {Body}",
                to,
                subject,
                body
            );

            return Task.CompletedTask;
        }
    }
}

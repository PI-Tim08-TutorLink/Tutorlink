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
            _logger.LogInformation($"TO: {to}");
            _logger.LogInformation($"SUBJECT: {subject}");
            _logger.LogInformation($"BODY:\n{body}");

            return Task.CompletedTask;
        }
    }
}

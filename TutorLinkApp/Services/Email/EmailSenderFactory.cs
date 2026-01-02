using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Services.Email
{
    public static class EmailSenderFactory
    {
        public static IEmailSender Create(IServiceProvider services)
        {
            return services.GetRequiredService<FakeEmailSender>();
        }
    }
}

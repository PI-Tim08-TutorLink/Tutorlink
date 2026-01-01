using ILogger = TutorLinkApp.Services.Interfaces.ILogger;

namespace TutorLinkApp.Services.Implementations
{
    public class AppLogger : ILogger
    {
        private static AppLogger? instance;

        private AppLogger() { }

        public static AppLogger GetInstance()
        {
            if(instance == null)
            {
                instance = new AppLogger();
            }
            return instance;
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now}: {message}");
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now}: {message}");
        }
    }
}

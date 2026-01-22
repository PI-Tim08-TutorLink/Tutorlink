using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TutorLinkApp.Models;

namespace TutorLinkApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in Error action");
            }

            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogError(
                "Error occurred. RequestId: {RequestId}, StatusCode: {StatusCode}",
                requestId,
                statusCode
            );

            if (statusCode.HasValue)
            {
                ViewBag.StatusCode = statusCode.Value;
                ViewBag.ErrorMessage = statusCode.Value switch
                {
                    400 => "Bad Request - The request could not be understood.",
                    401 => "Unauthorized - Please log in to access this page.",
                    403 => "Forbidden - You don't have permission to access this page.",
                    404 => "Page Not Found - The page you're looking for doesn't exist.",
                    500 => "Internal Server Error - Something went wrong on our end.",
                    503 => "Service Unavailable - Please try again later.",
                    _ => "An error occurred while processing your request."
                };
            }
            else
            {
                ViewBag.ErrorMessage = "An unexpected error occurred.";
            }

            return View(new ErrorViewModel
            {
                RequestId = requestId
            });
        }
    }
}
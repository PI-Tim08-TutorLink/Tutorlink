using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TutorLinkApp.Controllers;
using TutorLinkApp.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace TutorLinkApp.Tests.Controllers
{
    public class HomeControllerTests
    {
        private HomeController CreateController()
        {
            var mockLogger = new Mock<ILogger<HomeController>>();
            return new HomeController(mockLogger.Object);
        }

        private HomeController CreateControllerWithContext()
        {
            var mockLogger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(mockLogger.Object);

            // Setup HttpContext za Error action
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-id";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ReturnsNonNullViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Index_ReturnsDefaultView()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // Default view name (Index.cshtml)
        }

        // ========== PRIVACY ACTION TESTS ==========

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsNonNullViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Privacy_ReturnsDefaultView()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // Default view name (Privacy.cshtml)
        }

        // ========== ERROR ACTION TESTS ==========

        [Fact]
        public void Error_ReturnsViewResult()
        {
            // Arrange
            var controller = CreateControllerWithContext();

            // Act
            var result = controller.Error();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            // Arrange
            var controller = CreateControllerWithContext();

            // Act
            var result = controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ErrorViewModel>(result.Model);
        }

        [Fact]
        public void Error_ModelContainsRequestId()
        {
            // Arrange
            var controller = CreateControllerWithContext();

            // Act
            var result = controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.NotNull(model.RequestId);
            Assert.NotEmpty(model.RequestId);
        }

        [Fact]
        public void Error_UsesHttpContextTraceIdentifier_WhenActivityCurrentIsNull()
        {
            // Arrange
            var controller = CreateControllerWithContext();
            Activity.Current = null; // Ensure no current activity

            // Act
            var result = controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.Equal("test-trace-id", model.RequestId);
        }

        [Fact]
        public void Error_UsesActivityCurrentId_WhenAvailable()
        {
            // Arrange
            var controller = CreateControllerWithContext();
            var activity = new Activity("TestActivity");
            activity.Start();

            try
            {
                // Act
                var result = controller.Error() as ViewResult;
                var model = result?.Model as ErrorViewModel;

                // Assert
                Assert.NotNull(model);
                Assert.Equal(activity.Id, model.RequestId);
            }
            finally
            {
                activity.Stop();
            }
        }

        [Fact]
        public void Error_HasCorrectResponseCacheAttribute()
        {
            // Arrange
            var controller = CreateController();
            var methodInfo = typeof(HomeController).GetMethod("Error");

            // Act
            var attributes = methodInfo?.GetCustomAttributes(typeof(ResponseCacheAttribute), false);

            // Assert
            Assert.NotNull(attributes);
            Assert.NotEmpty(attributes);

            var cacheAttribute = attributes[0] as ResponseCacheAttribute;
            Assert.NotNull(cacheAttribute);
            Assert.Equal(0, cacheAttribute.Duration);
            Assert.Equal(ResponseCacheLocation.None, cacheAttribute.Location);
            Assert.True(cacheAttribute.NoStore);
        }

        // ========== CONTROLLER INITIALIZATION TESTS ==========

        [Fact]
        public void Constructor_InitializesWithLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HomeController>>();

            // Act
            var controller = new HomeController(mockLogger.Object);

            // Assert
            Assert.NotNull(controller);
        }

        // ========== INTEGRATION-STYLE TESTS ==========

        [Fact]
        public void AllActions_ReturnViewResult()
        {
            // Arrange
            var controller = CreateControllerWithContext();

            // Act
            var indexResult = controller.Index();
            var privacyResult = controller.Privacy();
            var errorResult = controller.Error();

            // Assert
            Assert.IsType<ViewResult>(indexResult);
            Assert.IsType<ViewResult>(privacyResult);
            Assert.IsType<ViewResult>(errorResult);
        }

        [Fact]
        public void ViewResults_AreNotNull()
        {
            // Arrange
            var controller = CreateControllerWithContext();

            // Act & Assert
            Assert.NotNull(controller.Index());
            Assert.NotNull(controller.Privacy());
            Assert.NotNull(controller.Error());
        }
    }
}
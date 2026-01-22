using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using TutorLinkApp.VM;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.DTO;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;
using TutorLinkApp.Controllers;

namespace TutorLinkApp.Tests.Controllers
{
    public class AccountControllerTests
    {
        private static TempDataDictionary CreateTempData()
            => new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        private static IUrlHelper CreateMockUrlHelper(string? returnUrl = null)
        {
            var mockUrlHelper = new Mock<IUrlHelper>();

            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(returnUrl ?? "https://localhost:7142/Account/ResetPassword");

            return mockUrlHelper.Object;
        }

        private static HttpContext CreateMockHttpContext()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(r => r.Scheme).Returns("https");

            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

            return mockHttpContext.Object;
        }

        private static AccountController CreateController(
            Mock<IUserService> mockUserService,
            Mock<ISessionManager> mockSessionManager,
            IUrlHelper? urlHelper = null,
            HttpContext? httpContext = null)
        {
            var controller = new AccountController(mockUserService.Object, mockSessionManager.Object)
            {
                TempData = CreateTempData(),
                Url = urlHelper ?? CreateMockUrlHelper(),
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext ?? CreateMockHttpContext()
                }
            };

            return controller;
        }

        // ========== FORGOT PASSWORD TESTS ==========

        [Fact]
        public void ForgotPassword_Get_ReturnsView()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);

            // Act
            var result = controller.ForgotPassword() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async void ForgotPassword_Post_EmailExists_SetsSuccessTempData()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            mockFacade
                .Setup(f => f.SendResetLink(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("http://resetlink.com/token123");

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new ForgotPasswordViewModel { Email = "test@example.com" };

            // Act
            var result = await controller.ForgotPassword(model, mockFacade.Object) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ForgotPassword", result.ActionName);
            Assert.Equal("Reset link sent successfully.", controller.TempData["SuccessMessage"]);
            Assert.Equal("http://resetlink.com/token123", controller.TempData["ResetLink"]);

            mockFacade.Verify(
                f => f.SendResetLink("test@example.com", It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async void ForgotPassword_Post_EmailDoesNotExist_SetsErrorTempData()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            mockFacade
                .Setup(f => f.SendResetLink(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new ForgotPasswordViewModel { Email = "nonexistent@example.com" };

            // Act
            var result = await controller.ForgotPassword(model, mockFacade.Object) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ForgotPassword", result.ActionName);
            Assert.Equal("Email address does not exist.", controller.TempData["ErrorMessage"]);
            Assert.Null(controller.TempData["ResetLink"]);

            mockFacade.Verify(
                f => f.SendResetLink("nonexistent@example.com", It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async void ForgotPassword_Post_InvalidModelState_ReturnsView()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            var controller = CreateController(mockUserService, mockSessionManager);
            controller.ModelState.AddModelError("Email", "Email is required");

            var model = new ForgotPasswordViewModel { Email = "" };

            // Act
            var result = await controller.ForgotPassword(model, mockFacade.Object) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);

            mockFacade.Verify(
                f => f.SendResetLink(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        // ========== RESET PASSWORD TESTS ==========

        [Fact]
        public void ResetPassword_Get_WithValidToken_ReturnsViewWithModel()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);
            var token = "test-token-123";

            // Act
            var result = controller.ResetPassword(token) as ViewResult;
            var model = result?.Model as ResetPasswordViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(token, model.Token);
        }

        [Fact]
        public void ResetPassword_Get_WithEmptyToken_ReturnsViewWithEmptyToken()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);
            var token = "";

            // Act
            var result = controller.ResetPassword(token) as ViewResult;
            var model = result?.Model as ResetPasswordViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("", model.Token);
        }

        [Fact]
        public void ResetPassword_Get_WithWhitespaceToken_ReturnsViewWithWhitespaceToken()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);
            var token = " ";

            // Act
            var result = controller.ResetPassword(token) as ViewResult;
            var model = result?.Model as ResetPasswordViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(" ", model.Token);
        }

        [Fact]
        public void ResetPassword_Get_WithNullToken_ReturnsViewWithNullToken()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);
            string? token = null;

            // Act
            var result = controller.ResetPassword(token!) as ViewResult;
            var model = result?.Model as ResetPasswordViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Null(model.Token);
        }

        [Fact]
        public async void ResetPassword_Post_ValidToken_RedirectsToLogin()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            mockFacade
                .Setup(f => f.ResetPassword(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new ResetPasswordViewModel
            {
                Token = "valid-token",
                NewPassword = "NewPass123",
                ConfirmPassword = "NewPass123"
            };

            // Act
            var result = await controller.ResetPassword(model, mockFacade.Object) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Password reset successful!", controller.TempData["SuccessMessage"]);

            mockFacade.Verify(
                f => f.ResetPassword("valid-token", "NewPass123"),
                Times.Once
            );
        }

        [Fact]
        public async void ResetPassword_Post_InvalidToken_ReturnsViewWithError()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            mockFacade
                .Setup(f => f.ResetPassword(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new ResetPasswordViewModel
            {
                Token = "invalid-token",
                NewPassword = "NewPass123",
                ConfirmPassword = "NewPass123"
            };

            // Act
            var result = await controller.ResetPassword(model, mockFacade.Object) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(""));

            mockFacade.Verify(
                f => f.ResetPassword("invalid-token", "NewPass123"),
                Times.Once
            );
        }

        [Fact]
        public async void ResetPassword_Post_InvalidModelState_ReturnsView()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            var controller = CreateController(mockUserService, mockSessionManager);
            controller.ModelState.AddModelError("NewPassword", "Password is required");

            var model = new ResetPasswordViewModel
            {
                Token = "some-token",
                NewPassword = "",
                ConfirmPassword = ""
            };

            // Act
            var result = await controller.ResetPassword(model, mockFacade.Object) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);

            mockFacade.Verify(
                f => f.ResetPassword(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async void ResetPassword_Post_PasswordsDoNotMatch_ValidationFails()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            var controller = CreateController(mockUserService, mockSessionManager);
            controller.ModelState.AddModelError("ConfirmPassword", "Passwords do not match");

            var model = new ResetPasswordViewModel
            {
                Token = "some-token",
                NewPassword = "NewPass123",
                ConfirmPassword = "DifferentPass123"
            };

            // Act
            var result = await controller.ResetPassword(model, mockFacade.Object) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);

            mockFacade.Verify(
                f => f.ResetPassword(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async void ForgotPassword_Post_FacadeThrowsException_PropagatesException()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var mockFacade = new Mock<IResetPasswordFacade>();

            mockFacade
                .Setup(f => f.SendResetLink(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new ForgotPasswordViewModel { Email = "test@example.com" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await controller.ForgotPassword(model, mockFacade.Object));
        }

        // ========== LOGIN TESTS ==========

        [Fact]
        public void Login_Get_ReturnsView()
        {
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);

            var result = controller.Login() as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Login_Post_InvalidModel_ReturnsView()
        {
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);
            controller.ModelState.AddModelError("Email", "Required");

            var model = new LoginViewModel { Email = "", Password = "" };

            var result = await controller.Login(model) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
        }

        // ========== REGISTER TESTS ==========

        [Fact]
        public void Register_Get_ReturnsView()
        {
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);

            var result = controller.Register() as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Register_Post_InvalidModel_ReturnsView()
        {
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            var controller = CreateController(mockUserService, mockSessionManager);
            controller.ModelState.AddModelError("Email", "Required");

            var model = new RegisterViewModel();

            var result = await controller.Register(model) as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Register_Post_EmailTaken_ReturnsViewWithError()
        {
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            mockUserService.Setup(s => s.IsEmailTaken(It.IsAny<string>())).ReturnsAsync(true);

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new RegisterViewModel
            {
                Email = "taken@test.com",
                Username = "newuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Student"
            };

            var result = await controller.Register(model) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Register_Post_UsernameTaken_ReturnsViewWithError()
        {
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            mockUserService.Setup(s => s.IsEmailTaken(It.IsAny<string>())).ReturnsAsync(false);
            mockUserService.Setup(s => s.IsUsernameTaken(It.IsAny<string>())).ReturnsAsync(true);

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new RegisterViewModel
            {
                Email = "new@test.com",
                Username = "takenuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Student"
            };

            var result = await controller.Register(model) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Register_Post_ValidData_RedirectsToLogin()
        {
            var mockUserService = new Mock<IUserService>();
            var mockSessionManager = new Mock<ISessionManager>();
            mockUserService.Setup(s => s.IsEmailTaken(It.IsAny<string>())).ReturnsAsync(false);
            mockUserService.Setup(s => s.IsUsernameTaken(It.IsAny<string>())).ReturnsAsync(false);

            var controller = CreateController(mockUserService, mockSessionManager);
            var model = new RegisterViewModel
            {
                Email = "new@test.com",
                Username = "newuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Student"
            };

            var result = await controller.Register(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            mockUserService.Verify(s => s.CreateUser(It.IsAny<RegisterViewModel>()), Times.Once);
        }
    }
}
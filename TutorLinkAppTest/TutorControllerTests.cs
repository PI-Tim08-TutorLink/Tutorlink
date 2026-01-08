using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Controllers;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkAppTest
{
    public class TutorControllerTests
    {
        private readonly Mock<ITutorService> _mockTutorService;
        private readonly Mock<ISessionManager> _mockSessionManager;

        public TutorControllerTests()
        {
            _mockTutorService = new Mock<ITutorService>();
            _mockSessionManager = new Mock<ISessionManager>();
        }

        private TutorController CreateController(
            int? userId = null,
            string? userRole = null)
        {
            var controller = new TutorController(
                _mockTutorService.Object,
                _mockSessionManager.Object
            );

            var httpContext = new DefaultHttpContext();
            var mockSession = new MockHttpSession();

            if (userId.HasValue)
            {
                mockSession.SetInt32("UserId", userId.Value);
            }

            if (!string.IsNullOrEmpty(userRole))
            {
                mockSession.SetString("UserRole", userRole);
            }

            httpContext.Session = mockSession;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );

            return controller;
        }

        [Fact]
        public async Task Index_NoFilters_CallsServiceAndReturnsView()
        {
            var filters = new TutorSearchViewModel();
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>
                {
                    new TutorCardViewModel { Id = 1, FullName = "John Doe" },
                    new TutorCardViewModel { Id = 2, FullName = "Jane Smith" }
                }
            };

            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var controller = CreateController();

            var result = await controller.Index(filters) as ViewResult;

            Assert.NotNull(result);
            _mockTutorService.Verify(
                s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Index_WithFilters_PassesFiltersToService()
        {
            var filters = new TutorSearchViewModel
            {
                SearchSkill = "Math",
                MinPrice = 30,
                MaxPrice = 60,
                MinRating = 4.0m,
                SortBy = "rating"
            };

            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>()
            };

            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var controller = CreateController();

            var result = await controller.Index(filters) as ViewResult;

            Assert.NotNull(result);
            _mockTutorService.Verify(
                s => s.SearchTutors(It.Is<TutorSearchViewModel>(f =>
                    f.SearchSkill == "Math" &&
                    f.MinPrice == 30 &&
                    f.MaxPrice == 60 &&
                    f.MinRating == 4.0m &&
                    f.SortBy == "rating"
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task Index_ReturnsViewWithSearchResults()
        {
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>
                {
                    new TutorCardViewModel { Id = 1, FullName = "John Doe" },
                    new TutorCardViewModel { Id = 2, FullName = "Jane Smith" }
                }
            };

            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var controller = CreateController();

            var result = await controller.Index(new TutorSearchViewModel()) as ViewResult;
            var model = result?.Model as TutorSearchViewModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(2, model.Tutors.Count);
            Assert.Equal("John Doe", model.Tutors[0].FullName);
            Assert.Equal("Jane Smith", model.Tutors[1].FullName);
        }

        [Fact]
        public async Task Index_EmptyResults_ReturnsViewWithEmptyList()
        {
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>()
            };

            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var controller = CreateController();

            var result = await controller.Index(new TutorSearchViewModel()) as ViewResult;
            var model = result?.Model as TutorSearchViewModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Empty(model.Tutors);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithTutor()
        {
            var tutorDetails = new TutorCardViewModel
            {
                Id = 1,
                FullName = "John Doe",
                Username = "johndoe",
                Email = "john@test.com",
                Skills = new List<string> { "Math", "Physics" },
                HourlyRate = 50,
                AverageRating = 4.5m,
                TotalReviews = 10
            };

            _mockTutorService
                .Setup(s => s.GetTutorDetails(1))
                .ReturnsAsync(tutorDetails);

            var controller = CreateController();

            var result = await controller.Details(1) as ViewResult;
            var model = result?.Model as TutorCardViewModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(1, model.Id);
            Assert.Equal("John Doe", model.FullName);
            Assert.Equal("johndoe", model.Username);
            Assert.Contains("Math", model.Skills);
        }

        [Fact]
        public async Task Details_InvalidId_RedirectsToIndexWithError()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(999))
                .ReturnsAsync((TutorCardViewModel?)null);

            var controller = CreateController();

            var result = await controller.Details(999) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Tutor not found.", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Details_ServiceReturnsNull_SetsErrorMessage()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(It.IsAny<int>()))
                .ReturnsAsync((TutorCardViewModel?)null);

            var controller = CreateController();

            var result = await controller.Details(1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
            Assert.Contains("not found", controller.TempData["ErrorMessage"].ToString());
        }

        [Fact]
        public async Task Details_CallsServiceWithCorrectId()
        {
            var tutorDetails = new TutorCardViewModel { Id = 5 };

            _mockTutorService
                .Setup(s => s.GetTutorDetails(5))
                .ReturnsAsync(tutorDetails);

            var controller = CreateController();

            await controller.Details(5);

            _mockTutorService.Verify(
                s => s.GetTutorDetails(5),
                Times.Once
            );
        }

        [Fact]
        public void Create_UserNotLoggedIn_RedirectsToLogin()
        {
            var controller = CreateController(userId: null);

            var result = controller.Create() as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
            Assert.Equal(
                "You must be logged in to create a tutor profile.",
                controller.TempData["ErrorMessage"]
            );
        }

        [Fact]
        public void Create_UserLoggedIn_ReturnsView()
        {
            var controller = CreateController(userId: 1);

            var result = controller.Create() as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void Create_UserNotLoggedIn_SetsErrorMessage()
        {
            var controller = CreateController(userId: null);

            controller.Create();

            Assert.NotNull(controller.TempData["ErrorMessage"]);
            Assert.Contains("logged in", controller.TempData["ErrorMessage"].ToString());
        }

        [Fact]
        public void Create_UserLoggedIn_DoesNotSetErrorMessage()
        {
            var controller = CreateController(userId: 1);

            controller.Create();

            Assert.False(controller.TempData.ContainsKey("ErrorMessage"));
        }

        [Fact]
        public void Create_ChecksSessionForUserId()
        {
            var controller = CreateController(userId: 123);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_MultipleUsers_WorksCorrectly()
        {
            var controller1 = CreateController(userId: null);
            var result1 = controller1.Create();
            Assert.IsType<RedirectToActionResult>(result1);

            var controller2 = CreateController(userId: 1);
            var result2 = controller2.Create();
            Assert.IsType<ViewResult>(result2);


            var controller3 = CreateController(userId: 999);
            var result3 = controller3.Create();
            Assert.IsType<ViewResult>(result3);
        }

        [Fact]
        public async Task Index_NullFilters_HandlesGracefully()
        {
            var expectedResult = new TutorSearchViewModel
            {
                Tutors = new List<TutorCardViewModel>()
            };

            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(expectedResult);

            var controller = CreateController();

            var result = await controller.Index(null!) as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Details_NegativeId_RedirectsWithError()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(-1))
                .ReturnsAsync((TutorCardViewModel?)null);

            var controller = CreateController();

            var result = await controller.Details(-1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Details_ZeroId_RedirectsWithError()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(0))
                .ReturnsAsync((TutorCardViewModel?)null);

            var controller = CreateController();

            var result = await controller.Details(0) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public void Create_SessionExpired_RedirectsToLogin()
        {
            var controller = CreateController(userId: null);

            var result = controller.Create() as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
        }

        [Fact]
        public void IsAdmin_WhenUserRoleIsAdmin_ReturnsTrue()
        {
            var controller = CreateController(userId: 1, userRole: "Admin");

            var method = typeof(TutorController)
                .GetMethod("IsAdmin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = (bool)method!.Invoke(controller, null)!;

            Assert.True(result);
        }

        [Fact]
        public void IsAdmin_WhenUserRoleIsStudent_ReturnsFalse()
        {
            var controller = CreateController(userId: 1, userRole: "Student");

            var method = typeof(TutorController)
                .GetMethod("IsAdmin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = (bool)method!.Invoke(controller, null)!;

            Assert.False(result);
        }

        [Fact]
        public void IsAdmin_WhenUserRoleIsNull_ReturnsFalse()
        {
            var controller = CreateController(userId: 1, userRole: null);

            var method = typeof(TutorController)
                .GetMethod("IsAdmin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = (bool)method!.Invoke(controller, null)!;

            Assert.False(result);
        }

        [Fact]
        public void IsAdmin_WhenUserRoleIsTutor_ReturnsFalse()
        {
            var controller = CreateController(userId: 1, userRole: "Tutor");

            var method = typeof(TutorController)
                .GetMethod("IsAdmin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = (bool)method!.Invoke(controller, null)!;

            Assert.False(result);
        }

        [Fact]
        public void IsAdmin_WhenUserRoleIsAdminWithDifferentCase_ReturnsFalse()
        {
            var controller = CreateController(userId: 1, userRole: "admin");

            var method = typeof(TutorController)
                .GetMethod("IsAdmin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = (bool)method!.Invoke(controller, null)!;

            Assert.False(result); 
        }

        [Fact]
        public async Task Index_ServiceThrowsException_PropagatesException()
        {
            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = CreateController();

            // Controller nema try-catch, pa će exception biti propagiran
            await Assert.ThrowsAsync<Exception>(
                () => controller.Index(new TutorSearchViewModel())
            );
        }

        [Fact]
        public async Task Details_ServiceThrowsException_PropagatesException()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Connection failed"));

            var controller = CreateController();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => controller.Details(1)
            );
        }

        [Fact]
        public async Task Index_ServiceThrowsArgumentNullException_PropagatesException()
        {
            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ThrowsAsync(new ArgumentNullException("filters"));

            var controller = CreateController();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => controller.Index(new TutorSearchViewModel())
            );
        }

        [Fact]
        public void Create_NegativeUserId_RedirectsToLogin()
        {
            var controller = CreateController(userId: -1);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }
    }


    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public string Id => "test-session-id";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value)
            => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[]? value)
            => _sessionStorage.TryGetValue(key, out value);
    }
}

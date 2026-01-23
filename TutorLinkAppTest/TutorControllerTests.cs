using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using TutorLinkApp.Controllers;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;
using Xunit;

namespace TutorLinkAppTest
{
    public class TutorControllerTests
    {
        private readonly Mock<ITutorService> _mockTutorService;

        public TutorControllerTests()
        {
            _mockTutorService = new Mock<ITutorService>();
        }

        private TutorController CreateController(int? userId = null)
        {
            var controller = new TutorController(_mockTutorService.Object);

            var httpContext = new DefaultHttpContext();
            var session = new MockHttpSession();

            if (userId.HasValue)
            {
                session.SetInt32("UserId", userId.Value);
            }

            httpContext.Session = session;

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

        // -------------------- INDEX --------------------

        [Fact]
        public async Task Index_NoFilters_ReturnsView()
        {
            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(new TutorSearchViewModel());

            var controller = CreateController();

            var result = await controller.Index(null) as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Index_WithFilters_PassesFiltersToService()
        {
            var filters = new TutorSearchViewModel
            {
                SearchSkill = "Math",
                MinPrice = 20,
                MaxPrice = 60,
                MinRating = 4,
                SortBy = "rating"
            };

            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(new TutorSearchViewModel());

            var controller = CreateController();

            await controller.Index(filters);

            _mockTutorService.Verify(
                s => s.SearchTutors(It.Is<TutorSearchViewModel>(f =>
                    f.SearchSkill == "Math" &&
                    f.MinPrice == 20 &&
                    f.MaxPrice == 60 &&
                    f.MinRating == 4 &&
                    f.SortBy == "rating"
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task Index_EmptyResults_ReturnsEmptyList()
        {
            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ReturnsAsync(new TutorSearchViewModel
                {
                    Tutors = new List<TutorCardViewModel>()
                });

            var controller = CreateController();

            var result = await controller.Index(new TutorSearchViewModel()) as ViewResult;
            var model = result?.Model as TutorSearchViewModel;

            Assert.NotNull(model);
            Assert.Empty(model.Tutors);
        }

        // -------------------- DETAILS --------------------

        [Fact]
        public async Task Details_ValidId_ReturnsView()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(1))
                .ReturnsAsync(new TutorCardViewModel { Id = 1 });

            var controller = CreateController();

            var result = await controller.Details(1) as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Details_InvalidId_RedirectsToIndex()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(It.IsAny<int>()))
                .ReturnsAsync((TutorCardViewModel?)null);

            var controller = CreateController();

            var result = await controller.Details(999) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
        }

        // -------------------- CREATE --------------------

        [Fact]
        public void Create_UserNotLoggedIn_RedirectsToLogin()
        {
            var controller = CreateController(null);

            var result = controller.Create() as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public void Create_UserLoggedIn_ReturnsView()
        {
            var controller = CreateController(1);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        // -------------------- EXCEPTIONS --------------------

        [Fact]
        public async Task Index_ServiceThrows_ExceptionPropagates()
        {
            _mockTutorService
                .Setup(s => s.SearchTutors(It.IsAny<TutorSearchViewModel>()))
                .ThrowsAsync(new Exception("DB error"));

            var controller = CreateController();

            await Assert.ThrowsAsync<Exception>(() =>
                controller.Index(new TutorSearchViewModel()));
        }

        [Fact]
        public async Task Details_ServiceThrows_ExceptionPropagates()
        {
            _mockTutorService
                .Setup(s => s.GetTutorDetails(It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException());

            var controller = CreateController();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                controller.Details(1));
        }
    }

    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new();

        public string Id => "test-session";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _storage.Keys;

        public void Clear() => _storage.Clear();
        public void Remove(string key) => _storage.Remove(key);
        public void Set(string key, byte[] value) => _storage[key] = value;

        public bool TryGetValue(string key, out byte[]? value)
            => _storage.TryGetValue(key, out value);

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorLinkApp.Controllers;
using TutorLinkApp.DTO;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;
using Xunit;

namespace TutorLinkAppTest.Integration
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _adminServiceMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<IAdminUserCreationService> _userCreationMock;

        public AdminControllerTests()
        {
            _adminServiceMock = new Mock<IAdminService>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _userCreationMock = new Mock<IAdminUserCreationService>();
        }

        private AdminController CreateController(bool isAdmin = false)
        {
            var controller = new AdminController(
                _adminServiceMock.Object,
                _sessionManagerMock.Object,
                _userCreationMock.Object
            );

            // Setup HttpContext
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Setup TempData
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            // Setup session - admin or not
            if (isAdmin)
            {
                _sessionManagerMock
                    .Setup(s => s.GetUserSession(It.IsAny<HttpContext>()))
                    .Returns(new UserSession { RoleName = "Admin" });
            }
            else
            {
                _sessionManagerMock
                    .Setup(s => s.GetUserSession(It.IsAny<HttpContext>()))
                    .Returns((UserSession?)null);
            }

            return controller;
        }

        // ========== CreateUser GET ==========
        [Fact]
        public void CreateUser_Get_ReturnsView()
        {
            var controller = CreateController();

            var result = controller.CreateUser();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<RegisterViewModel>(viewResult.Model);
        }

        // ========== CreateUser POST ==========
        [Fact]
        public async Task CreateUser_Post_InvalidModel_ReturnsView()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Email", "Required");
            var model = new RegisterViewModel();

            var result = await controller.CreateUser(model);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateUser_Post_ValidAdmin_RedirectsToUsers()
        {
            var controller = CreateController(isAdmin: true);
            var model = new RegisterViewModel
            {
                Username = "newadmin",
                Email = "admin@test.com",
                FirstName = "New",
                LastName = "Admin",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = "Admin"
            };

            var result = await controller.CreateUser(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Users", redirect.ActionName);
            _userCreationMock.Verify(s => s.CreateUser(model, 1), Times.Once);
        }

        [Fact]
        public async Task CreateUser_Post_ValidStudent_CallsServiceWithRoleId2()
        {
            var controller = CreateController(isAdmin: true);
            var model = new RegisterViewModel
            {
                Username = "newstudent",
                Email = "student@test.com",
                FirstName = "New",
                LastName = "Student",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = "Student"
            };

            var result = await controller.CreateUser(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Users", redirect.ActionName);
            _userCreationMock.Verify(s => s.CreateUser(model, 2), Times.Once);
        }

        [Fact]
        public async Task CreateUser_Post_ValidTutor_CallsServiceWithRoleId3()
        {
            var controller = CreateController(isAdmin: true);
            var model = new RegisterViewModel
            {
                Username = "newtutor",
                Email = "tutor@test.com",
                FirstName = "New",
                LastName = "Tutor",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = "Tutor",
                Skills = "Math"
            };

            var result = await controller.CreateUser(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Users", redirect.ActionName);
            _userCreationMock.Verify(s => s.CreateUser(model, 3), Times.Once);
        }

        [Fact]
        public async Task CreateUser_Post_InvalidRole_ReturnsViewWithError()
        {
            var controller = CreateController(isAdmin: true);
            var model = new RegisterViewModel
            {
                Username = "user",
                Email = "user@test.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = "InvalidRole"
            };
            
            var result = await controller.CreateUser(model);

            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Role"));
        }

        // ========== Dashboard ==========
        [Fact]
        public async Task Dashboard_WhenAdmin_ReturnsViewWithData()
        {
            _adminServiceMock.Setup(s => s.GetTotalUsers()).ReturnsAsync(10);
            _adminServiceMock.Setup(s => s.GetTotalTutors()).ReturnsAsync(3);
            _adminServiceMock.Setup(s => s.GetTotalStudents()).ReturnsAsync(7);
            var controller = CreateController(isAdmin: true);

            var result = await controller.Dashboard();

            Assert.IsType<ViewResult>(result);
            Assert.Equal(10, controller.ViewBag.TotalUsers);
            Assert.Equal(3, controller.ViewBag.TotalTutors);
            Assert.Equal(7, controller.ViewBag.TotalStudents);
        }

        [Fact]
        public async Task Dashboard_WhenNotAdmin_RedirectsToHome()
        {
            var controller = CreateController(isAdmin: false);

            var result = await controller.Dashboard();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("Access denied. Admin privileges required.", controller.TempData["ErrorMessage"]);
        }

        // ========== Users ==========
        [Fact]
        public async Task Users_WhenAdmin_ReturnsViewWithUsers()
        {
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1", Email = "user1@test.com" },
                new User { Id = 2, Username = "user2", Email = "user2@test.com" }
            };
            _adminServiceMock.Setup(s => s.GetAllUsers()).ReturnsAsync(users);
            var controller = CreateController(isAdmin: true);

            var result = await controller.Users();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Users_WhenNotAdmin_RedirectsToHome()
        {
            var controller = CreateController(isAdmin: false);

            var result = await controller.Users();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Users_WhenAdmin_CallsGetAllUsers()
        {
            _adminServiceMock.Setup(s => s.GetAllUsers()).ReturnsAsync(new List<User>());
            var controller = CreateController(isAdmin: true);

            await controller.Users();

            _adminServiceMock.Verify(s => s.GetAllUsers(), Times.Once);
        }
    }
}
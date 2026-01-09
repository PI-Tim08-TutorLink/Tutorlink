using Microsoft.AspNetCore.Http;
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

namespace TutorLinkAppTest.Integration
{
    public class AdminControllerTests
    {
        private AdminController CreateController()
        {
            var adminServiceMock = new Mock<IAdminService>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var userCreationMock = new Mock<IAdminUserCreationService>();

            // default ponašanje mockova
            adminServiceMock.Setup(s => s.GetTotalUsers()).ReturnsAsync(1);
            adminServiceMock.Setup(s => s.GetTotalTutors()).ReturnsAsync(0);
            adminServiceMock.Setup(s => s.GetTotalStudents()).ReturnsAsync(1);
            adminServiceMock.Setup(s => s.GetAllUsers()).ReturnsAsync(new List<User>());

            return new AdminController(
                adminServiceMock.Object,
                sessionManagerMock.Object,
                userCreationMock.Object
            );
        }

        [Fact]
        public async Task CreateUser_Get_ReturnsView()
        {
            var controller = CreateController();

            var result = controller.CreateUser();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateUser_Post_InvalidModel_ReturnsView()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Email", "Required");

            var model = new RegisterViewModel();

            var result = await controller.CreateUser(model);

            Assert.IsType<ViewResult>(result);
        }
    }
}
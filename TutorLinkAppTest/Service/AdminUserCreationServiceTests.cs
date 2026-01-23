using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;
using Xunit;

namespace TutorLinkAppTest.Service
{
    public class AdminUserCreationServiceTests
    {
        [Fact]
        public async Task CreateUser_ShouldInsertUserIntoDatabase()
        {
            // ARRANGE
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase("CreateUserDb")
                .Options;

            using var context = new TutorLinkContext(options);

            var hasherMock = new Mock<IPasswordHasher>();
            hasherMock.Setup(h => h.GenerateSalt()).Returns("salt");
            hasherMock.Setup(h => h.Hash(It.IsAny<string>(), "salt")).Returns("hash");

            var service = new AdminUserCreationService(context, hasherMock.Object);

            var model = new RegisterViewModel
            {
                Email = "admin@test.com",
                Username = "admin",
                Password = "Password123",
                ConfirmPassword = "Password123",
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin"
            };

            int roleId = 1;

            // ACT
            await service.CreateUser(model, roleId);

            // ASSERT
            var created = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
            Assert.NotNull(created);
            Assert.Equal("admin", created!.Username);
            Assert.Equal(roleId, created.RoleId);

            Assert.Equal("hash", created.PwdHash);
            Assert.Equal("salt", created.PwdSalt);
        }
    }
}

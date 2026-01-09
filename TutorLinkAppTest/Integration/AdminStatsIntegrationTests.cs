using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;

namespace TutorLinkAppTest.Integration
{

    public class AdminStatsIntegrationTests
    {
        [Fact]
        public async Task GetTotalUsers_ReturnsCorrectCount()
        {
            // ARRANGE
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase(databaseName: "AdminStatsDb")
                .Options;

            using var context = new TutorLinkContext(options);

            var user1 = new User
            {
                CreatedAt = DateTime.UtcNow,
                DeletedAt = null,

                Username = "u1",
                FirstName = "Test",
                LastName = "User",
                Email = "u1@test.com",

                PwdHash = "HASH",
                PwdSalt = "SALT",

                RoleId = 2 // Student npr.
            };

            var user2 = new User
            {
                CreatedAt = DateTime.UtcNow,
                DeletedAt = null,

                Username = "u2",
                FirstName = "Test2",
                LastName = "User2",
                Email = "u2@test.com",

                PwdHash = "HASH2",
                PwdSalt = "SALT2",

                RoleId = 3 // Tutor npr.
            };

            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var service = new AdminStatsService(context);

            // ACT
            var result = await service.GetTotalUsers();

            // ASSERT
            Assert.Equal(2, result);
        }
    }
}

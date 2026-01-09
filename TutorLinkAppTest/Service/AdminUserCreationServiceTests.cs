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
            // ARRANGE - InMemory DbContext
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase("CreateUserDb")
                .Options;

            using var context = new TutorLinkContext(options);

            // Mock PasswordHasher (da ne ovisis o stvarnoj implementaciji)
            var hasherMock = new Mock<IPasswordHasher>();
            hasherMock.Setup(h => h.GenerateSalt()).Returns("salt");
            hasherMock.Setup(h => h.Hash(It.IsAny<string>(), "salt")).Returns("hash");

            // ✅ OVO je ključno: instanciraj servis onako kako ga ti imaš
            // Ako tvoj konstruktor izgleda: AdminUserCreationService(TutorLinkContext ctx, IPasswordHasher hasher)
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

            int roleId = 1; // Admin

            // ACT
            await service.CreateUser(model, roleId);

            // ASSERT - provjeri da je user upisan
            var created = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
            Assert.NotNull(created);
            Assert.Equal("admin", created!.Username);
            Assert.Equal(roleId, created.RoleId);

            // i hash/salt
            Assert.Equal("hash", created.PwdHash);
            Assert.Equal("salt", created.PwdSalt);
        }
    }
}

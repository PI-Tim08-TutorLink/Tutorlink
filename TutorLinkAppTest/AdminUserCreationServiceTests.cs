using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;
using TutorLinkApp.VM;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace TutorLinkApp.Tests.Services
{
    public sealed class AdminUserCreationServiceTests : IDisposable
    {
        private readonly TutorLinkContext _context;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly AdminUserCreationService _service;

        public AdminUserCreationServiceTests()
        {
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TutorLinkContext(options);
            _mockHasher = new Mock<IPasswordHasher>();

            _mockHasher.Setup(h => h.GenerateSalt()).Returns("test-salt");
            _mockHasher.Setup(h => h.Hash(It.IsAny<string>(), It.IsAny<string>())).Returns("test-hash");

            _service = new AdminUserCreationService(_context, _mockHasher.Object);
        }



        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ========== CREATE USER TESTS ==========

        [Fact]
        public async Task CreateUser_ValidStudentData_CreatesUserSuccessfully()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "newstudent",
                Email = "student@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!"
            };
            int roleId = 2;

            // Act
            await _service.CreateUser(model, roleId);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newstudent");
            Assert.NotNull(user);
            Assert.Equal("student@example.com", user.Email);
            Assert.Equal("John", user.FirstName);
            Assert.Equal("Doe", user.LastName);
            Assert.Equal(2, user.RoleId);
            Assert.Equal("test-salt", user.PwdSalt);
            Assert.Equal("test-hash", user.PwdHash);
            Assert.NotEqual(default(DateTime), user.CreatedAt);

            _mockHasher.Verify(h => h.GenerateSalt(), Times.Once);
            _mockHasher.Verify(h => h.Hash("Password123!", "test-salt"), Times.Once);
        }

        [Fact]
        public async Task CreateUser_ValidAdminData_CreatesUserSuccessfully()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "newadmin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                Password = "AdminPass123!"
            };
            int roleId = 1;

            // Act
            await _service.CreateUser(model, roleId);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newadmin");
            Assert.NotNull(user);
            Assert.Equal(1, user.RoleId);
            Assert.Equal("admin@example.com", user.Email);
        }

        [Fact]
        public async Task CreateUser_ValidTutorData_CreatesUserSuccessfully()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "newtutor",
                Email = "tutor@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "TutorPass123!",
                Skills = "Math"
            };
            int roleId = 3;

            // Act
            await _service.CreateUser(model, roleId);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newtutor");
            Assert.NotNull(user);
            Assert.Equal(3, user.RoleId);
        }

        [Fact]
        public async Task CreateUser_UsernameTaken_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "takenuser",
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var model = new RegisterViewModel
            {
                Username = "takenuser",
                Email = "new@example.com",
                FirstName = "New",
                LastName = "User",
                Password = "Password123!"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.CreateUser(model, 2)
            );

            Assert.Equal("Username already exists.", exception.Message);


            var userCount = await _context.Users.CountAsync();
            Assert.Equal(1, userCount);
        }

        [Fact]
        public async Task CreateUser_UsernameTaken_DoesNotCallHasher()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "takenuser",
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var model = new RegisterViewModel
            {
                Username = "takenuser",
                Email = "new@example.com",
                FirstName = "New",
                LastName = "User",
                Password = "Password123!"
            };

            // Act
            try
            {
                await _service.CreateUser(model, 2);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Caught expected exception.");
            }

            // Assert
            _mockHasher.Verify(h => h.GenerateSalt(), Times.Never);
            _mockHasher.Verify(h => h.Hash(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateUser_CaseSensitiveUsername_TreatedAsDifferent()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "TestUser",
                Email = "test1@example.com",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test2@example.com",
                FirstName = "Test",
                LastName = "User2",
                Password = "Password123!"
            };

            // Act
            await _service.CreateUser(model, 2);

            // Assert
            var userCount = await _context.Users.CountAsync();
            Assert.Equal(2, userCount);
        }

        [Fact]
        public async Task CreateUser_CallsPasswordHasherInCorrectOrder()
        {
            // Arrange
            var callSequence = new List<string>();

            _mockHasher.Setup(h => h.GenerateSalt())
                .Returns("test-salt")
                .Callback(() => callSequence.Add("GenerateSalt"));

            _mockHasher.Setup(h => h.Hash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("test-hash")
                .Callback(() => callSequence.Add("Hash"));

            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123!"
            };

            // Act
            await _service.CreateUser(model, 2);

            // Assert
            Assert.Equal(2, callSequence.Count);
            Assert.Equal("GenerateSalt", callSequence[0]);
            Assert.Equal("Hash", callSequence[1]);
        }

        [Fact]
        public async Task CreateUser_WithSpecialCharactersInName_SavesCorrectly()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "specialuser",
                Email = "special@example.com",
                FirstName = "Jöhn-Dåvid",
                LastName = "O'Døe-Smith",
                Password = "Password123!"
            };

            // Act
            await _service.CreateUser(model, 2);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "specialuser");
            Assert.NotNull(user);
            Assert.Equal("Jöhn-Dåvid", user.FirstName);
            Assert.Equal("O'Døe-Smith", user.LastName);
        }

        [Fact]
        public async Task CreateUser_WithLongPassword_HashesCorrectly()
        {
            // Arrange
            var longPassword = new string('a', 100);
            var model = new RegisterViewModel
            {
                Username = "longpassuser",
                Email = "long@example.com",
                FirstName = "Long",
                LastName = "Password",
                Password = longPassword
            };

            _mockHasher.Setup(h => h.Hash(longPassword, "test-salt")).Returns("long-hash");

            // Act
            await _service.CreateUser(model, 2);

            // Assert
            _mockHasher.Verify(h => h.Hash(longPassword, "test-salt"), Times.Once);
        }

        [Fact]
        public async Task CreateUser_MultipleUsers_CreatesAllSuccessfully()
        {
            // Arrange
            var models = new[]
            {
                new RegisterViewModel { Username = "user1", Email = "user1@test.com", FirstName = "User", LastName = "One", Password = "Pass1" },
                new RegisterViewModel { Username = "user2", Email = "user2@test.com", FirstName = "User", LastName = "Two", Password = "Pass2" },
                new RegisterViewModel { Username = "user3", Email = "user3@test.com", FirstName = "User", LastName = "Three", Password = "Pass3" }
            };

            // Act
            foreach (var model in models)
            {
                await _service.CreateUser(model, 2);
            }

            // Assert
            var userCount = await _context.Users.CountAsync();
            Assert.Equal(3, userCount);
        }

        [Fact]
        public async Task CreateUser_TutorRole_ExecutesTutorBranch()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "tutoruser",
                Email = "tutor@example.com",
                FirstName = "Tutor",
                LastName = "User",
                Password = "Password123!",
                Skills = "Math"
            };

            int tutorRoleId = 3;

            // Act
            await _service.CreateUser(model, tutorRoleId);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "tutoruser");
            Assert.NotNull(user);
            Assert.Equal(3, user.RoleId);
        }

        [Fact]
        public async Task CreateUser_NonTutorRole_SkipsTutorBranch()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "studentuser",
                Email = "student@example.com",
                FirstName = "Student",
                LastName = "User",
                Password = "Password123!"
            };

            int studentRoleId = 2;

            // Act
            await _service.CreateUser(model, studentRoleId);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "studentuser");
            Assert.NotNull(user);
            Assert.Equal(2, user.RoleId);
        }

        [Fact]
        public async Task CreateUser_WithMinimalData_CreatesUserSuccessfully()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "a",
                Email = "a@b.c",
                FirstName = "A",
                LastName = "B",
                Password = "p"
            };

            // Act
            await _service.CreateUser(model, 2);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "a");
            Assert.NotNull(user);
        }

        [Fact]
        public async Task CreateUser_SavesAllPropertiesCorrectly()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "completeuser",
                Email = "complete@example.com",
                FirstName = "Complete",
                LastName = "User",
                Password = "CompletePass123!"
            };

            var expectedSalt = "expected-salt";
            var expectedHash = "expected-hash";

            _mockHasher.Setup(h => h.GenerateSalt()).Returns(expectedSalt);
            _mockHasher.Setup(h => h.Hash("CompletePass123!", expectedSalt)).Returns(expectedHash);

            // Act
            await _service.CreateUser(model, 2);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "completeuser");
            Assert.NotNull(user);
            Assert.Equal("completeuser", user.Username);
            Assert.Equal("complete@example.com", user.Email);
            Assert.Equal("Complete", user.FirstName);
            Assert.Equal("User", user.LastName);
            Assert.Equal(2, user.RoleId);
            Assert.Equal(expectedSalt, user.PwdSalt);
            Assert.Equal(expectedHash, user.PwdHash);
        }
    }
}
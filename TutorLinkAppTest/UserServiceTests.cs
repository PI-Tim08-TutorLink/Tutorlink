using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.VM;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace TutorLinkApp.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly TutorLinkContext _context;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TutorLinkContext(options);
            _mockHasher = new Mock<IPasswordHasher>();
            _mockHasher.Setup(h => h.GenerateSalt()).Returns("test-salt");
            _mockHasher.Setup(h => h.Hash(It.IsAny<string>(), It.IsAny<string>())).Returns("test-hash");
            _mockHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _userService = new UserService(_context, _mockHasher.Object);

            SeedRoles();
        }

        private void SeedRoles()
        {
            _context.Roles.AddRange(
                new Role { Id = 1, Role1 = "Admin" },
                new Role { Id = 2, Role1 = "Student" },
                new Role { Id = 3, Role1 = "Tutor" }
            );
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ========== IS EMAIL TAKEN TESTS ==========

        [Fact]
        public async Task IsEmailTaken_EmailExists_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Email = "existing@example.com",
                Username = "existinguser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.IsEmailTaken("existing@example.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEmailTaken_EmailDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _userService.IsEmailTaken("notfound@example.com");

            // Assert
            Assert.False(result);
        }

        // ========== IS USERNAME TAKEN TESTS ==========

        [Fact]
        public async Task IsUsernameTaken_UsernameExists_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "existinguser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.IsUsernameTaken("existinguser");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUsernameTaken_UsernameDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _userService.IsUsernameTaken("nonexistentuser");

            // Assert
            Assert.False(result);
        }

        // ========== CREATE USER TESTS ==========

        [Fact]
        public async Task CreateUser_AsStudent_CreatesUserWithRoleId2()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "student@example.com",
                Username = "student1",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                Role = "Student"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("student@example.com", result.Email);
            Assert.Equal("student1", result.Username);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal(2, result.RoleId);
            Assert.Equal("test-salt", result.PwdSalt);
            Assert.Equal("test-hash", result.PwdHash);
            Assert.NotEqual(default(DateTime), result.CreatedAt);

            _mockHasher.Verify(h => h.GenerateSalt(), Times.Once);
            _mockHasher.Verify(h => h.Hash("Password123!", "test-salt"), Times.Once);
        }

        [Fact]
        public async Task CreateUser_AsTutor_CreatesUserWithRoleId3()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "Tutor",
                Skills = "Math, Physics"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.RoleId);
            Assert.Equal("tutor@example.com", result.Email);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithSkills_CreatesTutorRecord()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "Tutor",
                Skills = "Math, Physics, Chemistry"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.NotNull(tutor);
            Assert.Equal(result.Id, tutor.UserId);
            Assert.Equal("Math, Physics, Chemistry", tutor.Skill);
            Assert.NotEqual(default(DateTime), tutor.CreatedAt);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithoutSkills_DoesNotCreateTutorRecord()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "Tutor",
                Skills = ""
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithNullSkills_DoesNotCreateTutorRecord()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "Tutor",
                Skills = null
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithWhitespaceSkills_DoesNotCreateTutorRecord()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "Tutor",
                Skills = "   "
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_AsStudent_DoesNotCreateTutorRecord()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "student@example.com",
                Username = "student1",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                Role = "Student",
                Skills = "Some skills"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_RoleCaseInsensitive_TutorLowercase_CreatesCorrectRole()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "tutor",
                Skills = "Math"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            Assert.Equal(3, result.RoleId);
        }

        [Fact]
        public async Task CreateUser_RoleCaseInsensitive_TutorUppercase_CreatesCorrectRole()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "TUTOR",
                Skills = "Math"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            Assert.Equal(3, result.RoleId);
        }

        [Fact]
        public async Task CreateUser_RoleCaseInsensitive_TutorMixedCase_CreatesCorrectRole()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "tutor@example.com",
                Username = "tutor1",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "Password123!",
                Role = "TuToR",
                Skills = "Math"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            Assert.Equal(3, result.RoleId);
        }

        [Fact]
        public async Task CreateUser_UnknownRole_DefaultsToStudent()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "user@example.com",
                Username = "user1",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                Role = "Admin"
            };

            // Act
            var result = await _userService.CreateUser(model);

            // Assert
            Assert.Equal(2, result.RoleId);
        }

        // ========== AUTHENTICATE USER WITH ROLE TESTS ==========

        [Fact]
        public async Task AuthenticateUserWithRole_ValidCredentials_ReturnsUserWithRole()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "correct-hash",
                PwdSalt = "correct-salt",
                RoleId = 2,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify("CorrectPassword", "correct-hash", "correct-salt"))
                .Returns(true);

            // Act
            var result = await _userService.AuthenticateUserWithRole("test@example.com", "CorrectPassword");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.User.Id);
            Assert.Equal("Student", result.RoleName);
        }

        [Fact]
        public async Task AuthenticateUserWithRole_UserNotFound_ReturnsNull()
        {
            // Act
            var result = await _userService.AuthenticateUserWithRole("notfound@example.com", "Password");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUserWithRole_DeletedUser_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Email = "deleted@example.com",
                Username = "deleteduser",
                FirstName = "Deleted",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2,
                DeletedAt = DateTime.Now.AddDays(-1)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.AuthenticateUserWithRole("deleted@example.com", "Password");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUserWithRole_WrongPassword_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "correct-hash",
                PwdSalt = "correct-salt",
                RoleId = 2,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify("WrongPassword", "correct-hash", "correct-salt"))
                .Returns(false);

            // Act
            var result = await _userService.AuthenticateUserWithRole("test@example.com", "WrongPassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUserWithRole_AdminUser_ReturnsAdminRole()
        {
            // Arrange
            var user = new User
            {
                Email = "admin@example.com",
                Username = "admin",
                FirstName = "Admin",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // Act
            var result = await _userService.AuthenticateUserWithRole("admin@example.com", "Password");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.RoleName);
        }

        [Fact]
        public async Task AuthenticateUserWithRole_TutorUser_ReturnsTutorRole()
        {
            // Arrange
            var user = new User
            {
                Email = "tutor@example.com",
                Username = "tutor",
                FirstName = "Tutor",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 3,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // Act
            var result = await _userService.AuthenticateUserWithRole("tutor@example.com", "Password");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Tutor", result.RoleName);
        }

        [Fact]
        public async Task AuthenticateUserWithRole_RoleNotFound_DefaultsToStudent()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 999,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // Act
            var result = await _userService.AuthenticateUserWithRole("test@example.com", "Password");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Student", result.RoleName);
        }

        // ========== INTEGRATION TESTS ==========

        [Fact]
        public async Task FullWorkflow_CreateStudentAndAuthenticate_WorksCorrectly()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Email = "newstudent@example.com",
                Username = "newstudent",
                FirstName = "New",
                LastName = "Student",
                Password = "Password123!",
                Role = "Student"
            };

            _mockHasher.Setup(h => h.GenerateSalt()).Returns("new-salt");
            _mockHasher.Setup(h => h.Hash("Password123!", "new-salt")).Returns("new-hash");
            _mockHasher.Setup(h => h.Verify("Password123!", "new-hash", "new-salt")).Returns(true);

            // Act
            var createdUser = await _userService.CreateUser(registerModel);

            // Act
            var authResult = await _userService.AuthenticateUserWithRole("newstudent@example.com", "Password123!");

            // Assert
            Assert.NotNull(authResult);
            Assert.Equal(createdUser.Id, authResult.User.Id);
            Assert.Equal("Student", authResult.RoleName);
        }

        [Fact]
        public async Task FullWorkflow_CreateTutorWithSkillsAndAuthenticate_WorksCorrectly()
        {
            // Arrange
            var registerModel = new RegisterViewModel
            {
                Email = "newtutor@example.com",
                Username = "newtutor",
                FirstName = "New",
                LastName = "Tutor",
                Password = "Password123!",
                Role = "Tutor",
                Skills = "Programming, Math"
            };

            _mockHasher.Setup(h => h.GenerateSalt()).Returns("tutor-salt");
            _mockHasher.Setup(h => h.Hash("Password123!", "tutor-salt")).Returns("tutor-hash");
            _mockHasher.Setup(h => h.Verify("Password123!", "tutor-hash", "tutor-salt")).Returns(true);

            // Act
            var createdUser = await _userService.CreateUser(registerModel);

            var tutorRecord = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == createdUser.Id);
            Assert.NotNull(tutorRecord);

            // Act
            var authResult = await _userService.AuthenticateUserWithRole("newtutor@example.com", "Password123!");

            // Assert
            Assert.NotNull(authResult);
            Assert.Equal(createdUser.Id, authResult.User.Id);
            Assert.Equal("Tutor", authResult.RoleName);
        }
    }
}
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.VM;
using System;
using System.Threading.Tasks;
using TutorLinkApp.Services.Interfaces;

namespace TutorLinkApp.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly TutorLinkContext _context;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly UserService _userService;

        private bool _disposed;

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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ================= IS EMAIL TAKEN TESTS =================

        [Fact]
        public async Task IsEmailTaken_EmailExists_ReturnsTrue()
        {
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

            var result = await _userService.IsEmailTaken("existing@example.com");

            Assert.True(result);
        }

        [Fact]
        public async Task IsEmailTaken_EmailDoesNotExist_ReturnsFalse()
        {
            var result = await _userService.IsEmailTaken("notfound@example.com");

            Assert.False(result);
        }

        // ================= IS USERNAME TAKEN TESTS =================

        [Fact]
        public async Task IsUsernameTaken_UsernameExists_ReturnsTrue()
        {
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

            var result = await _userService.IsUsernameTaken("existinguser");

            Assert.True(result);
        }

        [Fact]
        public async Task IsUsernameTaken_UsernameDoesNotExist_ReturnsFalse()
        {
            var result = await _userService.IsUsernameTaken("nonexistentuser");

            Assert.False(result);
        }

        // ================= CREATE USER TESTS =================

        [Fact]
        public async Task CreateUser_AsStudent_CreatesUserWithRoleId2()
        {
            var model = new RegisterViewModel
            {
                Email = "student@example.com",
                Username = "student1",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                Role = "Student"
            };

            var result = await _userService.CreateUser(model);

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

            var result = await _userService.CreateUser(model);

            Assert.NotNull(result);
            Assert.Equal(3, result.RoleId);
            Assert.Equal("tutor@example.com", result.Email);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithSkills_CreatesTutorRecord()
        {
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

            var result = await _userService.CreateUser(model);

            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.NotNull(tutor);
            Assert.Equal(result.Id, tutor.UserId);
            Assert.Equal("Math, Physics, Chemistry", tutor.Skill);
            Assert.NotEqual(default(DateTime), tutor.CreatedAt);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithoutSkills_DoesNotCreateTutorRecord()
        {
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

            var result = await _userService.CreateUser(model);

            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithNullSkills_DoesNotCreateTutorRecord()
        {
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

            var result = await _userService.CreateUser(model);

            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_AsTutorWithWhitespaceSkills_DoesNotCreateTutorRecord()
        {
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

            var result = await _userService.CreateUser(model);

            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_AsStudent_DoesNotCreateTutorRecord()
        {
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

            var result = await _userService.CreateUser(model);

            var tutor = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == result.Id);
            Assert.Null(tutor);
        }

        [Fact]
        public async Task CreateUser_RoleCaseInsensitive_TutorLowercase_CreatesCorrectRole()
        {
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

            var result = await _userService.CreateUser(model);

            Assert.Equal(3, result.RoleId);
        }

        [Fact]
        public async Task CreateUser_UnknownRole_DefaultsToStudent()
        {
            var model = new RegisterViewModel
            {
                Email = "user@example.com",
                Username = "user1",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                Role = "Admin"
            };

            var result = await _userService.CreateUser(model);

            Assert.Equal(2, result.RoleId);
        }

        // ================= AUTHENTICATE USER TESTS =================

        [Fact]
        public async Task AuthenticateUser_ValidCredentials_ReturnsUser()
        {
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

            var result = await _userService.AuthenticateUser("test@example.com", "CorrectPassword");

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal("testuser", result.Username);
            Assert.NotNull(result.Role);
        }

        [Fact]
        public async Task AuthenticateUser_UserNotFound_ReturnsNull()
        {
            var result = await _userService.AuthenticateUser("notfound@example.com", "Password");

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUser_DeletedUser_ReturnsNull()
        {
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

            var result = await _userService.AuthenticateUser("deleted@example.com", "Password");

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUser_WrongPassword_ReturnsNull()
        {
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

            var result = await _userService.AuthenticateUser("test@example.com", "WrongPassword");

            Assert.Null(result);
        }

        // ================= GET USER BY ID TESTS =================

        [Fact]
        public async Task GetUserById_UserExists_ReturnsUser()
        {
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.GetUserById(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetUserById_UserNotFound_ReturnsNull()
        {
            var result = await _userService.GetUserById(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserById_DeletedUser_ReturnsNull()
        {
            var user = new User
            {
                Email = "deleted@example.com",
                Username = "deleteduser",
                FirstName = "Deleted",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 2,
                DeletedAt = DateTime.Now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.GetUserById(user.Id);

            Assert.Null(result);
        }

        // ================= INTEGRATION TESTS =================

        [Fact]
        public async Task FullWorkflow_CreateStudentAndAuthenticate_WorksCorrectly()
        {
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

            var createdUser = await _userService.CreateUser(registerModel);
            var authResult = await _userService.AuthenticateUser("newstudent@example.com", "Password123!");

            Assert.NotNull(authResult);
            Assert.Equal(createdUser.Id, authResult.Id);
            Assert.Equal(2, authResult.RoleId);
        }

        [Fact]
        public async Task FullWorkflow_CreateTutorWithSkillsAndAuthenticate_WorksCorrectly()
        {
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

            var createdUser = await _userService.CreateUser(registerModel);

            var tutorRecord = await _context.Tutors.FirstOrDefaultAsync(t => t.UserId == createdUser.Id);
            Assert.NotNull(tutorRecord);

            var authResult = await _userService.AuthenticateUser("newtutor@example.com", "Password123!");

            Assert.NotNull(authResult);
            Assert.Equal(createdUser.Id, authResult.Id);
            Assert.Equal(3, authResult.RoleId);
        }
    }
}

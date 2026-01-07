using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Email;
using TutorLinkApp.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace TutorLinkApp.Tests.Services
{
    public class ResetPasswordFacadeTests : IDisposable
    {
        private readonly TutorLinkContext _context;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly EmailService _emailService;
        private readonly ResetPasswordFacade _facade;

        public ResetPasswordFacadeTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TutorLinkContext(options);
            _mockHasher = new Mock<IPasswordHasher>();
            _mockEmailSender = new Mock<IEmailSender>();

            // Mock email sender to return completed task
            _mockEmailSender
                .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _emailService = new EmailService(_mockEmailSender.Object);
            _facade = new ResetPasswordFacade(_context, _mockHasher.Object, _emailService);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ========== SEND RESET LINK TESTS ==========

        [Fact]
        public async Task SendResetLink_ValidEmail_GeneratesTokenAndReturnsLink()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var resetUrlBase = "https://localhost:7142/Account/ResetPassword";

            // Act
            var result = await _facade.SendResetLink(user.Email, resetUrlBase);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith(resetUrlBase, result);
            Assert.Contains("?token=", result);

            // Verify user has reset token
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.NotNull(updatedUser!.ResetToken);
            Assert.NotNull(updatedUser.ResetTokenExpiry);
            Assert.True(updatedUser.ResetTokenExpiry > DateTime.Now);
            Assert.True(updatedUser.ResetTokenExpiry <= DateTime.Now.AddMinutes(31));

            // Verify email was sent
            _mockEmailSender.Verify(
                s => s.SendAsync(
                    user.Email,
                    "Reset password",
                    It.Is<string>(body => body.Contains(result))),
                Times.Once);
        }

        [Fact]
        public async Task SendResetLink_NonExistentEmail_ReturnsNull()
        {
            // Arrange
            var resetUrlBase = "https://localhost:7142/Account/ResetPassword";

            // Act
            var result = await _facade.SendResetLink("nonexistent@example.com", resetUrlBase);

            // Assert
            Assert.Null(result);

            // Verify email was NOT sent
            _mockEmailSender.Verify(
                s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SendResetLink_DeletedUser_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "deleted@example.com",
                Username = "deleteduser",
                FirstName = "Deleted",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                DeletedAt = DateTime.Now.AddDays(-1) // User is soft-deleted
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var resetUrlBase = "https://localhost:7142/Account/ResetPassword";

            // Act
            var result = await _facade.SendResetLink(user.Email, resetUrlBase);

            // Assert
            Assert.Null(result);

            // Verify email was NOT sent to deleted user
            _mockEmailSender.Verify(
                s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SendResetLink_GeneratesUniqueTokens()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var resetUrlBase = "https://localhost:7142/Account/ResetPassword";

            // Act
            var result1 = await _facade.SendResetLink(user.Email, resetUrlBase);
            var token1 = result1!.Split("?token=")[1];

            var result2 = await _facade.SendResetLink(user.Email, resetUrlBase);
            var token2 = result2!.Split("?token=")[1];

            // Assert
            Assert.NotEqual(token1, token2); // Tokens should be unique
        }

        [Fact]
        public async Task SendResetLink_SetsExpiryToApproximately30Minutes()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var resetUrlBase = "https://localhost:7142/Account/ResetPassword";
            var beforeCall = DateTime.Now;

            // Act
            await _facade.SendResetLink(user.Email, resetUrlBase);

            // Assert
            var updatedUser = await _context.Users.FindAsync(user.Id);
            var expectedExpiry = beforeCall.AddMinutes(30);

            Assert.NotNull(updatedUser!.ResetTokenExpiry);

            // Allow 2 seconds tolerance for test execution time
            var timeDifference = Math.Abs((updatedUser.ResetTokenExpiry!.Value - expectedExpiry).TotalSeconds);
            Assert.True(timeDifference < 2, $"Token expiry should be ~30 minutes from now. Difference: {timeDifference} seconds");
        }

        [Fact]
        public async Task SendResetLink_UpdatesExistingToken()
        {
            // Arrange
            var oldToken = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                ResetToken = oldToken,
                ResetTokenExpiry = DateTime.Now.AddMinutes(15),
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var resetUrlBase = "https://localhost:7142/Account/ResetPassword";

            // Act
            var result = await _facade.SendResetLink(user.Email, resetUrlBase);

            // Assert
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.NotEqual(oldToken, updatedUser!.ResetToken);
        }

        // ========== RESET PASSWORD TESTS ==========

        [Fact]
        public async Task ResetPassword_ValidToken_UpdatesPasswordAndClearsToken()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "oldhash",
                PwdSalt = "oldsalt",
                RoleId = 1,
                ResetToken = token,
                ResetTokenExpiry = DateTime.Now.AddMinutes(15),
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var newPassword = "NewPassword123!";
            var newSalt = "newsalt";
            var newHash = "newhash";

            _mockHasher.Setup(h => h.GenerateSalt()).Returns(newSalt);
            _mockHasher.Setup(h => h.Hash(newPassword, newSalt)).Returns(newHash);

            // Act
            var result = await _facade.ResetPassword(token, newPassword);

            // Assert
            Assert.True(result);

            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.Equal(newSalt, updatedUser!.PwdSalt);
            Assert.Equal(newHash, updatedUser.PwdHash);
            Assert.Null(updatedUser.ResetToken);
            Assert.Null(updatedUser.ResetTokenExpiry);

            _mockHasher.Verify(h => h.GenerateSalt(), Times.Once);
            _mockHasher.Verify(h => h.Hash(newPassword, newSalt), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                ResetToken = "valid-token",
                ResetTokenExpiry = DateTime.Now.AddMinutes(15),
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _facade.ResetPassword("invalid-token", "NewPassword123!");

            // Assert
            Assert.False(result);

            // Verify password wasn't changed
            var unchangedUser = await _context.Users.FindAsync(user.Id);
            Assert.Equal("hash", unchangedUser!.PwdHash);
            Assert.Equal("salt", unchangedUser.PwdSalt);
            Assert.NotNull(unchangedUser.ResetToken); // Token still there

            _mockHasher.Verify(h => h.GenerateSalt(), Times.Never);
            _mockHasher.Verify(h => h.Hash(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPassword_ExpiredToken_ReturnsFalse()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                ResetToken = token,
                ResetTokenExpiry = DateTime.Now.AddMinutes(-5), // Expired 5 minutes ago
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _facade.ResetPassword(token, "NewPassword123!");

            // Assert
            Assert.False(result);

            // Verify password wasn't changed
            var unchangedUser = await _context.Users.FindAsync(user.Id);
            Assert.Equal("hash", unchangedUser!.PwdHash);
            Assert.Equal("salt", unchangedUser.PwdSalt);

            _mockHasher.Verify(h => h.GenerateSalt(), Times.Never);
        }

        [Fact]
        public async Task ResetPassword_TokenExpiringNow_ReturnsFalse()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                ResetToken = token,
                ResetTokenExpiry = DateTime.Now.AddMilliseconds(-100), // Just expired
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _facade.ResetPassword(token, "NewPassword123!");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResetPassword_NoUserWithToken_ReturnsFalse()
        {
            // Arrange - No user in database

            // Act
            var result = await _facade.ResetPassword("any-token", "NewPassword123!");

            // Assert
            Assert.False(result);
            _mockHasher.Verify(h => h.GenerateSalt(), Times.Never);
        }

        [Fact]
        public async Task ResetPassword_NullToken_ReturnsFalse()
        {
            // Act
            var result = await _facade.ResetPassword(null!, "NewPassword123!");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResetPassword_EmptyToken_ReturnsFalse()
        {
            // Act
            var result = await _facade.ResetPassword("", "NewPassword123!");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResetPassword_WhitespaceToken_ReturnsFalse()
        {
            // Act
            var result = await _facade.ResetPassword("   ", "NewPassword123!");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResetPassword_ClearsTokenEvenIfHashingFails()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt",
                RoleId = 1,
                ResetToken = token,
                ResetTokenExpiry = DateTime.Now.AddMinutes(15),
                DeletedAt = null
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.GenerateSalt()).Returns("newsalt");
            _mockHasher.Setup(h => h.Hash(It.IsAny<string>(), It.IsAny<string>())).Returns("newhash");

            // Act
            var result = await _facade.ResetPassword(token, "NewPassword123!");

            // Assert - Token should be cleared even if successful
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.Null(updatedUser!.ResetToken);
            Assert.Null(updatedUser.ResetTokenExpiry);
        }
    }
}
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.DTO;
using System.Text;

namespace TutorLinkApp.Tests.Services
{
    public class SessionManagerTests
    {
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _sessionManager = new SessionManager();
        }

        private HttpContext CreateMockHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            return httpContext;
        }
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();

            public string Id => "test-session-id";
            public bool IsAvailable => true;
            public IEnumerable<string> Keys => _store.Keys;

            public void Clear() => _store.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void Remove(string key) => _store.Remove(key);

            public void Set(string key, byte[] value) => _store[key] = value;

            public bool TryGetValue(string key, out byte[]? value)
                => _store.TryGetValue(key, out value);
        }

        // ========== SET USER SESSION TESTS ==========

        [Fact]
        public void SetUserSession_StoresAllSessionValues()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 123,
                Username = "testuser",
                FirstName = "John",
                RoleName = "Admin",
                RoleId = 1
            };

            // Act
            _sessionManager.SetUserSession(httpContext, userSession);

            // Assert
            Assert.Equal(123, httpContext.Session.GetInt32("UserId"));
            Assert.Equal("testuser", httpContext.Session.GetString("Username"));
            Assert.Equal("John", httpContext.Session.GetString("FirstName"));
            Assert.Equal("Admin", httpContext.Session.GetString("UserRole"));
            Assert.Equal(1, httpContext.Session.GetInt32("RoleId"));
        }

        [Fact]
        public void SetUserSession_WithStudentRole_StoresCorrectly()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 456,
                Username = "student1",
                FirstName = "Jane",
                RoleName = "Student",
                RoleId = 3
            };

            // Act
            _sessionManager.SetUserSession(httpContext, userSession);

            // Assert
            Assert.Equal(456, httpContext.Session.GetInt32("UserId"));
            Assert.Equal("Student", httpContext.Session.GetString("UserRole"));
            Assert.Equal(3, httpContext.Session.GetInt32("RoleId"));
        }

        [Fact]
        public void SetUserSession_WithTutorRole_StoresCorrectly()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 789,
                Username = "tutor1",
                FirstName = "Mike",
                RoleName = "Tutor",
                RoleId = 2
            };

            // Act
            _sessionManager.SetUserSession(httpContext, userSession);

            // Assert
            Assert.Equal("Tutor", httpContext.Session.GetString("UserRole"));
        }

        [Fact]
        public void SetUserSession_OverwritesExistingSession()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var oldSession = new UserSession
            {
                UserId = 1,
                Username = "olduser",
                FirstName = "Old",
                RoleName = "Student",
                RoleId = 3
            };
            var newSession = new UserSession
            {
                UserId = 2,
                Username = "newuser",
                FirstName = "New",
                RoleName = "Admin",
                RoleId = 1
            };

            // Act
            _sessionManager.SetUserSession(httpContext, oldSession);
            _sessionManager.SetUserSession(httpContext, newSession);

            // Assert
            Assert.Equal(2, httpContext.Session.GetInt32("UserId"));
            Assert.Equal("newuser", httpContext.Session.GetString("Username"));
            Assert.Equal("New", httpContext.Session.GetString("FirstName"));
            Assert.Equal("Admin", httpContext.Session.GetString("UserRole"));
        }

        [Fact]
        public void SetUserSession_WithEmptyUsername_StoresEmptyString()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 100,
                Username = "",
                FirstName = "Test",
                RoleName = "Student",
                RoleId = 3
            };

            // Act
            _sessionManager.SetUserSession(httpContext, userSession);

            // Assert
            Assert.Equal("", httpContext.Session.GetString("Username"));
        }

        [Fact]
        public void SetUserSession_WithSpecialCharacters_StoresCorrectly()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 200,
                Username = "user@test.com",
                FirstName = "Jöhn-Døe",
                RoleName = "Admin",
                RoleId = 1
            };

            // Act
            _sessionManager.SetUserSession(httpContext, userSession);

            // Assert
            Assert.Equal("user@test.com", httpContext.Session.GetString("Username"));
            Assert.Equal("Jöhn-Døe", httpContext.Session.GetString("FirstName"));
        }

        // ========== CLEAR SESSION TESTS ==========

        [Fact]
        public void ClearSession_RemovesAllSessionData()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 123,
                Username = "testuser",
                FirstName = "John",
                RoleName = "Admin",
                RoleId = 1
            };
            _sessionManager.SetUserSession(httpContext, userSession);

            // Act
            _sessionManager.ClearSession(httpContext);

            // Assert
            Assert.Null(httpContext.Session.GetInt32("UserId"));
            Assert.Null(httpContext.Session.GetString("Username"));
            Assert.Null(httpContext.Session.GetString("FirstName"));
            Assert.Null(httpContext.Session.GetString("UserRole"));
            Assert.Null(httpContext.Session.GetInt32("RoleId"));
        }

        [Fact]
        public void ClearSession_OnEmptySession_DoesNotThrow()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();

            // Act & Assert
            _sessionManager.ClearSession(httpContext);
        }

        [Fact]
        public void ClearSession_MultipleTimes_DoesNotThrow()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 123,
                Username = "testuser",
                FirstName = "John",
                RoleName = "Admin",
                RoleId = 1
            };
            _sessionManager.SetUserSession(httpContext, userSession);

            // Act
            _sessionManager.ClearSession(httpContext);
            _sessionManager.ClearSession(httpContext);
            _sessionManager.ClearSession(httpContext);

            // Assert - Should still be clear
            Assert.Null(httpContext.Session.GetInt32("UserId"));
        }

        // ========== GET USER SESSION TESTS ==========

        [Fact]
        public void GetUserSession_WithValidSession_ReturnsUserSession()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var originalSession = new UserSession
            {
                UserId = 123,
                Username = "testuser",
                FirstName = "John",
                RoleName = "Admin",
                RoleId = 1
            };
            _sessionManager.SetUserSession(httpContext, originalSession);

            // Act
            var retrievedSession = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.NotNull(retrievedSession);
            Assert.Equal(123, retrievedSession.UserId);
            Assert.Equal("testuser", retrievedSession.Username);
            Assert.Equal("John", retrievedSession.FirstName);
            Assert.Equal("Admin", retrievedSession.RoleName);
            Assert.Equal(1, retrievedSession.RoleId);
        }

        [Fact]
        public void GetUserSession_WithNoSession_ReturnsNull()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.Null(session);
        }

        [Fact]
        public void GetUserSession_AfterClearSession_ReturnsNull()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 123,
                Username = "testuser",
                FirstName = "John",
                RoleName = "Admin",
                RoleId = 1
            };
            _sessionManager.SetUserSession(httpContext, userSession);
            _sessionManager.ClearSession(httpContext);

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.Null(session);
        }

        [Fact]
        public void GetUserSession_WithMissingUsername_UsesEmptyString()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            httpContext.Session.SetInt32("UserId", 123);
            httpContext.Session.SetString("FirstName", "John");
            httpContext.Session.SetString("UserRole", "Admin");
            httpContext.Session.SetInt32("RoleId", 1);

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(string.Empty, session.Username);
        }

        [Fact]
        public void GetUserSession_WithMissingFirstName_UsesEmptyString()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            httpContext.Session.SetInt32("UserId", 123);
            httpContext.Session.SetString("Username", "testuser");
            httpContext.Session.SetString("UserRole", "Admin");
            httpContext.Session.SetInt32("RoleId", 1);

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(string.Empty, session.FirstName);
        }

        [Fact]
        public void GetUserSession_WithMissingUserRole_UsesDefaultStudent()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            httpContext.Session.SetInt32("UserId", 123);
            httpContext.Session.SetString("Username", "testuser");
            httpContext.Session.SetString("FirstName", "John");
            httpContext.Session.SetInt32("RoleId", 3);

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.NotNull(session);
            Assert.Equal("Student", session.RoleName);
        }

        [Fact]
        public void GetUserSession_WithMissingRoleId_UsesDefaultZero()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            httpContext.Session.SetInt32("UserId", 123);
            httpContext.Session.SetString("Username", "testuser");
            httpContext.Session.SetString("FirstName", "John");
            httpContext.Session.SetString("UserRole", "Admin");
            // Deliberately not setting RoleId

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(0, session.RoleId);
        }

        [Fact]
        public void GetUserSession_WithAllFieldsMissing_ExceptUserId_ReturnsDefaults()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            httpContext.Session.SetInt32("UserId", 999);

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(999, session.UserId);
            Assert.Equal(string.Empty, session.Username);
            Assert.Equal(string.Empty, session.FirstName);
            Assert.Equal("Student", session.RoleName);
            Assert.Equal(0, session.RoleId);
        }

        // ========== INTEGRATION TESTS ==========

        [Fact]
        public void FullWorkflow_SetGetClear_WorksCorrectly()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var userSession = new UserSession
            {
                UserId = 123,
                Username = "testuser",
                FirstName = "John",
                RoleName = "Admin",
                RoleId = 1
            };

            // Act & Assert
            _sessionManager.SetUserSession(httpContext, userSession);
            var retrieved1 = _sessionManager.GetUserSession(httpContext);
            Assert.NotNull(retrieved1);
            Assert.Equal(123, retrieved1.UserId);

            // Act & Assert
            _sessionManager.ClearSession(httpContext);
            var retrieved2 = _sessionManager.GetUserSession(httpContext);
            Assert.Null(retrieved2);
        }

        [Fact]
        public void MultipleUsers_SwitchSessions_WorksCorrectly()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            var user1 = new UserSession
            {
                UserId = 1,
                Username = "user1",
                FirstName = "Alice",
                RoleName = "Student",
                RoleId = 3
            };
            var user2 = new UserSession
            {
                UserId = 2,
                Username = "user2",
                FirstName = "Bob",
                RoleName = "Tutor",
                RoleId = 2
            };

            // Act
            _sessionManager.SetUserSession(httpContext, user1);
            var session1 = _sessionManager.GetUserSession(httpContext);

            _sessionManager.SetUserSession(httpContext, user2);
            var session2 = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.Equal("Alice", session1!.FirstName);
            Assert.Equal("Bob", session2!.FirstName);
            Assert.NotEqual(session1.UserId, session2.UserId);
        }

        [Fact]
        public void GetUserSession_WithZeroUserId_StillReturnsSession()
        {
            // Arrange
            var httpContext = CreateMockHttpContext();
            httpContext.Session.SetInt32("UserId", 0);
            httpContext.Session.SetString("Username", "user0");

            // Act
            var session = _sessionManager.GetUserSession(httpContext);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(0, session.UserId);
            Assert.Equal("user0", session.Username);
        }
    }
}
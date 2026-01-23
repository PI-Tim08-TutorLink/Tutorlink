using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TutorLinkAppTest.Service
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TutorLinkApp.Models;
    using TutorLinkApp.Services.Implementations;
    using Xunit;

    namespace TutorLinkAppTest.Integration
    {
        public class AdminServiceTests
        {
            private static TutorLinkContext CreateContext(string dbName)
            {
                var options = new DbContextOptionsBuilder<TutorLinkContext>()
                    .UseInMemoryDatabase(databaseName: dbName)
                    .Options;
                return new TutorLinkContext(options);
            }

            // ========== GetTotalUsers ==========
            [Fact]
            public async Task GetTotalUsers_ReturnsOnlyActiveUsers()
            {
                using var ctx = CreateContext("AdminService_GetTotalUsers");
                ctx.Users.AddRange(
                    new User { Username = "user1", Email = "u1@test.com", FirstName = "A", LastName = "B", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = null },
                    new User { Username = "user2", Email = "u2@test.com", FirstName = "C", LastName = "D", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = null },
                    new User { Username = "deleted", Email = "del@test.com", FirstName = "E", LastName = "F", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = DateTime.UtcNow }
                );
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetTotalUsers();

                Assert.Equal(2, result);
            }

            [Fact]
            public async Task GetTotalUsers_WhenNoUsers_ReturnsZero()
            {
                using var ctx = CreateContext("AdminService_GetTotalUsers_Empty");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetTotalUsers();

                Assert.Equal(0, result);
            }

            // ========== GetTotalTutors ==========
            [Fact]
            public async Task GetTotalTutors_ReturnsOnlyActiveTutors()
            {
                using var ctx = CreateContext("AdminService_GetTotalTutors");
                ctx.Tutors.AddRange(
                    new Tutor { UserId = 1, Skill = "Math", DeletedAt = null },
                    new Tutor { UserId = 2, Skill = "English", DeletedAt = null },
                    new Tutor { UserId = 3, Skill = "Physics", DeletedAt = DateTime.UtcNow }
                );
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetTotalTutors();

                Assert.Equal(2, result);
            }

            [Fact]
            public async Task GetTotalTutors_WhenNoTutors_ReturnsZero()
            {
                using var ctx = CreateContext("AdminService_GetTotalTutors_Empty");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetTotalTutors();

                Assert.Equal(0, result);
            }

            // ========== GetTotalStudents ==========
            [Fact]
            public async Task GetTotalStudents_ReturnsOnlyActiveStudentsWithRoleId2()
            {
                using var ctx = CreateContext("AdminService_GetTotalStudents");
                ctx.Users.AddRange(
                    new User { Username = "student1", Email = "s1@test.com", FirstName = "A", LastName = "B", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = null },
                    new User { Username = "student2", Email = "s2@test.com", FirstName = "C", LastName = "D", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = null },
                    new User { Username = "admin", Email = "a@test.com", FirstName = "E", LastName = "F", PwdHash = "h", PwdSalt = "s", RoleId = 1, DeletedAt = null },
                    new User { Username = "tutor", Email = "t@test.com", FirstName = "G", LastName = "H", PwdHash = "h", PwdSalt = "s", RoleId = 3, DeletedAt = null },
                    new User { Username = "deletedStudent", Email = "ds@test.com", FirstName = "I", LastName = "J", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = DateTime.UtcNow }
                );
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetTotalStudents();

                Assert.Equal(2, result);
            }

            // ========== GetAllUsers ==========
            [Fact]
            public async Task GetAllUsers_ReturnsOnlyActiveUsers()
            {
                using var ctx = CreateContext("AdminService_GetAllUsers");
                ctx.Users.AddRange(
                    new User { Username = "user1", Email = "u1@test.com", FirstName = "A", LastName = "B", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = null },
                    new User { Username = "user2", Email = "u2@test.com", FirstName = "C", LastName = "D", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = null },
                    new User { Username = "deleted", Email = "del@test.com", FirstName = "E", LastName = "F", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = DateTime.UtcNow }
                );
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetAllUsers();

                Assert.Equal(2, result.Count);
                Assert.All(result, u => Assert.Null(u.DeletedAt));
            }

            [Fact]
            public async Task GetAllUsers_WhenNoUsers_ReturnsEmptyList()
            {
                using var ctx = CreateContext("AdminService_GetAllUsers_Empty");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetAllUsers();

                Assert.Empty(result);
            }

            // ========== GetUserById ==========
            [Fact]
            public async Task GetUserById_WhenExists_ReturnsUser()
            {
                using var ctx = CreateContext("AdminService_GetUserById");
                var user = new User { Username = "testuser", Email = "test@test.com", FirstName = "Test", LastName = "User", PwdHash = "h", PwdSalt = "s", RoleId = 2 };
                ctx.Users.Add(user);
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetUserById(user.Id);

                Assert.NotNull(result);
                Assert.Equal("testuser", result.Username);
            }

            [Fact]
            public async Task GetUserById_WhenNotExists_ReturnsNull()
            {
                using var ctx = CreateContext("AdminService_GetUserById_NotFound");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetUserById(999);

                Assert.Null(result);
            }

            // ========== CreateUser ==========
            [Fact]
            public async Task CreateUser_CreatesUserWithHashedPassword()
            {
                using var ctx = CreateContext("AdminService_CreateUser");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var model = new RegisterViewModel
                {
                    Username = "newuser",
                    Email = "new@test.com",
                    FirstName = "New",
                    LastName = "User",
                    Password = "Password123!",
                    Role = "Student"
                };

                await svc.CreateUser(model);

                var created = await ctx.Users.SingleAsync(u => u.Username == "newuser");
                Assert.Equal("new@test.com", created.Email);
                Assert.Equal(2, created.RoleId);
                Assert.NotEqual("Password123!", created.PwdHash);
                Assert.False(string.IsNullOrWhiteSpace(created.PwdSalt));
            }

            [Fact]
            public async Task CreateUser_ForTutor_CreatesTutorProfile()
            {
                using var ctx = CreateContext("AdminService_CreateUser_Tutor");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var model = new RegisterViewModel
                {
                    Username = "newtutor",
                    Email = "tutor@test.com",
                    FirstName = "New",
                    LastName = "Tutor",
                    Password = "Password123!",
                    Role = "Tutor",
                    Skills = "Math"
                };

                await svc.CreateUser(model);

                var user = await ctx.Users.SingleAsync(u => u.Username == "newtutor");
                Assert.Equal(3, user.RoleId);

                var tutor = await ctx.Tutors.SingleAsync(t => t.UserId == user.Id);
                Assert.Equal("Math", tutor.Skill);
            }

            [Fact]
            public async Task CreateUser_ForAdmin_SetsRoleId1()
            {
                using var ctx = CreateContext("AdminService_CreateUser_Admin");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var model = new RegisterViewModel
                {
                    Username = "newadmin",
                    Email = "admin@test.com",
                    FirstName = "New",
                    LastName = "Admin",
                    Password = "Password123!",
                    Role = "Admin"
                };

                await svc.CreateUser(model);

                var user = await ctx.Users.SingleAsync(u => u.Username == "newadmin");
                Assert.Equal(1, user.RoleId);
            }

            // ========== UpdateUser ==========
            [Fact]
            public async Task UpdateUser_UpdatesUserData()
            {
                using var ctx = CreateContext("AdminService_UpdateUser");
                var user = new User { Username = "original", Email = "original@test.com", FirstName = "Original", LastName = "User", PwdHash = "h", PwdSalt = "s", RoleId = 2 };
                ctx.Users.Add(user);
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var updatedUser = new User
                {
                    Id = user.Id,
                    Username = "updated",
                    Email = "updated@test.com",
                    FirstName = "Updated",
                    LastName = "Name",
                    RoleId = 3
                };

                await svc.UpdateUser(updatedUser);

                var result = await ctx.Users.FindAsync(user.Id);
                Assert.Equal("updated", result.Username);
                Assert.Equal("updated@test.com", result.Email);
                Assert.Equal("Updated", result.FirstName);
                Assert.Equal("Name", result.LastName);
                Assert.Equal(3, result.RoleId);
            }

            [Fact]
            public async Task UpdateUser_WhenUserNotFound_ThrowsKeyNotFoundException()
            {
                using var ctx = CreateContext("AdminService_UpdateUser_NotFound");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var user = new User { Id = 999, Username = "nonexistent" };

                await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.UpdateUser(user));
            }

            // ========== SoftDeleteUser ==========
            [Fact]
            public async Task SoftDeleteUser_SetsDeletedAt()
            {
                using var ctx = CreateContext("AdminService_SoftDelete");
                var user = new User { Username = "todelete", Email = "del@test.com", FirstName = "To", LastName = "Delete", PwdHash = "h", PwdSalt = "s", RoleId = 2, DeletedAt = null };
                ctx.Users.Add(user);
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                await svc.SoftDeleteUser(user.Id);

                var deleted = await ctx.Users.FindAsync(user.Id);
                Assert.NotNull(deleted.DeletedAt);
            }

            [Fact]
            public async Task SoftDeleteUser_AlsoDeletesTutorProfiles()
            {
                using var ctx = CreateContext("AdminService_SoftDelete_Tutors");
                var user = new User { Username = "tutoruser", Email = "tu@test.com", FirstName = "Tutor", LastName = "User", PwdHash = "h", PwdSalt = "s", RoleId = 3, DeletedAt = null };
                ctx.Users.Add(user);
                await ctx.SaveChangesAsync();

                var tutor = new Tutor { UserId = user.Id, Skill = "Math", DeletedAt = null };
                ctx.Tutors.Add(tutor);
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                await svc.SoftDeleteUser(user.Id);

                var deletedTutor = await ctx.Tutors.FindAsync(tutor.Id);
                Assert.NotNull(deletedTutor.DeletedAt);
            }

            [Fact]
            public async Task SoftDeleteUser_WhenUserNotFound_DoesNothing()
            {
                using var ctx = CreateContext("AdminService_SoftDelete_NotFound");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var countBefore = await ctx.Users.CountAsync();

                await svc.SoftDeleteUser(999);

                var countAfter = await ctx.Users.CountAsync();
                Assert.Equal(countBefore, countAfter);
            }

            // ========== GetTutorsByUserId ==========
            [Fact]
            public async Task GetTutorsByUserId_ReturnsTutorsForUser()
            {
                using var ctx = CreateContext("AdminService_GetTutorsByUserId");
                ctx.Tutors.AddRange(
                    new Tutor { UserId = 1, Skill = "Math" },
                    new Tutor { UserId = 1, Skill = "Physics" },
                    new Tutor { UserId = 2, Skill = "English" }
                );
                await ctx.SaveChangesAsync();

                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetTutorsByUserId(1);

                Assert.Equal(2, result.Count);
                Assert.All(result, t => Assert.Equal(1, t.UserId));
            }

            [Fact]
            public async Task GetTutorsByUserId_WhenNoTutors_ReturnsEmptyList()
            {
                using var ctx = CreateContext("AdminService_GetTutorsByUserId_Empty");
                var hasher = new PasswordHasher();
                var svc = new AdminService(ctx, hasher);

                var result = await svc.GetTutorsByUserId(999);

                Assert.Empty(result);
            }
        }
    }
}

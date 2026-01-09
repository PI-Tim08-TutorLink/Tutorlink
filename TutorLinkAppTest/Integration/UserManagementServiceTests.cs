using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;

namespace TutorLinkAppTest.Integration
{
    public class UserManagementServiceTests
    {
        [Fact]
        public async Task GetAllUsers_ReturnsOnlyNotDeleted_AndIncludesTutorProfiles()
        {
            using var ctx = AdminTestDbFactory.CreateContext("UserManagement_GetAllUsers");

            var adminRole = await AdminTestDbFactory.GetRoleAsync(ctx, RoleIds.Admin);
            var tutorRole = await AdminTestDbFactory.GetRoleAsync(ctx, RoleIds.Tutor);

            var tutorUser = new User
            {
                Username = "tutor1",
                Email = "t1@test.com",
                FirstName = "T",
                LastName = "One",
                PwdHash = "h",
                PwdSalt = "s",
                RoleId = tutorRole.Id,
                Role = tutorRole,
                CreatedAt = DateTime.UtcNow,
            };
            tutorUser.Tutors.Add(new Tutor { Skill = "Math", CreatedAt = DateTime.UtcNow, User = tutorUser });

            ctx.Users.Add(tutorUser);

            ctx.Users.Add(new User
            {
                Username = "admin1",
                Email = "a1@test.com",
                FirstName = "A",
                LastName = "One",
                PwdHash = "h",
                PwdSalt = "s",
                RoleId = adminRole.Id,
                Role = adminRole,
                CreatedAt = DateTime.UtcNow,
            });

            ctx.Users.Add(new User
            {
                Username = "deleted",
                Email = "d@test.com",
                FirstName = "D",
                LastName = "One",
                PwdHash = "h",
                PwdSalt = "s",
                RoleId = adminRole.Id,
                Role = adminRole,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = DateTime.UtcNow,
            });

            await ctx.SaveChangesAsync();

            var sut = new UserManagementService(ctx);
            var users = await sut.GetAllUsers();

            Assert.Equal(2, users.Count);
            Assert.Contains(users, u => u.Username == "tutor1");
            Assert.Contains(users, u => u.Username == "admin1");

            var tutorFromResult = Assert.Single(users, u => u.Username == "tutor1");
            Assert.NotNull(tutorFromResult.Role);
            Assert.NotEmpty(tutorFromResult.Tutors);
        }

        [Fact]
        public async Task GetUserById_ReturnsNull_WhenDeleted()
        {
            using var ctx = AdminTestDbFactory.CreateContext("UserManagement_GetUserById_Deleted");
            var adminRole = await AdminTestDbFactory.GetRoleAsync(ctx, RoleIds.Admin);

            var u = new User
            {
                Username = "x",
                Email = "x@test.com",
                FirstName = "X",
                LastName = "X",
                PwdHash = "h",
                PwdSalt = "s",
                RoleId = adminRole.Id,
                Role = adminRole,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = DateTime.UtcNow,
            };
            ctx.Users.Add(u);
            await ctx.SaveChangesAsync();

            var sut = new UserManagementService(ctx);
            var result = await sut.GetUserById(u.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUser_PersistsChanges()
        {
            using var ctx = AdminTestDbFactory.CreateContext("UserManagement_UpdateUser");
            var adminRole = await AdminTestDbFactory.GetRoleAsync(ctx, RoleIds.Admin);

            var u = new User
            {
                Username = "before",
                Email = "before@test.com",
                FirstName = "Before",
                LastName = "User",
                PwdHash = "h",
                PwdSalt = "s",
                RoleId = adminRole.Id,
                Role = adminRole,
                CreatedAt = DateTime.UtcNow,
            };
            ctx.Users.Add(u);
            await ctx.SaveChangesAsync();

            u.Username = "after";
            u.Email = "after@test.com";

            var sut = new UserManagementService(ctx);
            await sut.UpdateUser(u);

            var fromDb = await ctx.Users.FindAsync(u.Id);
            Assert.NotNull(fromDb);
            Assert.Equal("after", fromDb!.Username);
            Assert.Equal("after@test.com", fromDb.Email);
        }

        [Fact]
        public async Task SoftDeleteUser_SetsDeletedAt()
        {
            using var ctx = AdminTestDbFactory.CreateContext("UserManagement_SoftDeleteUser");
            var adminRole = await AdminTestDbFactory.GetRoleAsync(ctx, RoleIds.Admin);

            var u = new User
            {
                Username = "del",
                Email = "del@test.com",
                FirstName = "Del",
                LastName = "User",
                PwdHash = "h",
                PwdSalt = "s",
                RoleId = adminRole.Id,
                Role = adminRole,
                CreatedAt = DateTime.UtcNow,
            };
            ctx.Users.Add(u);
            await ctx.SaveChangesAsync();

            var sut = new UserManagementService(ctx);
            await sut.SoftDeleteUser(u.Id);

            var fromDb = await ctx.Users.FindAsync(u.Id);
            Assert.NotNull(fromDb);
            Assert.NotNull(fromDb!.DeletedAt);
        }
    }
}

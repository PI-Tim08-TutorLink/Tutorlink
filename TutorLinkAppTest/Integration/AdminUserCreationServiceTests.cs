using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;
using Xunit;

namespace TutorLinkAppTest.Integration
{
    public class AdminUserCreationServiceTests
    {
        [Fact]
        public async Task CreateUser_CreatesAdminUser_WithSaltAndHash()
        {
            using var ctx = AdminTestDbFactory.CreateContext("AdminUserCreation_CreateAdmin");

            var hasher = new PasswordHasher();
            var svc = new AdminUserCreationService(ctx, hasher);

            var model = new RegisterViewModel
            {
                Username = "admin2",
                FirstName = "Admin",
                LastName = "Two",
                Email = "admin2@admin.com",
                Password = "Secret123!",
                ConfirmPassword = "Secret123!",
                Role = "Admin"
            };

            await svc.CreateUser(model, RoleIds.Admin);

            var created = ctx.Users.Single(u => u.Email == "admin2@admin.com");
            Assert.Equal(RoleIds.Admin, created.RoleId);
            Assert.False(string.IsNullOrWhiteSpace(created.PwdSalt));
            Assert.False(string.IsNullOrWhiteSpace(created.PwdHash));
            Assert.NotEqual(model.Password, created.PwdHash);
            Assert.True(hasher.Verify(model.Password, created.PwdHash, created.PwdSalt));
        }

        [Fact]
        public async Task CreateUser_ForTutor_CreatesTutorProfile()
        {
            using var ctx = AdminTestDbFactory.CreateContext("AdminUserCreation_CreateTutor");

            var hasher = new PasswordHasher();
            var svc = new AdminUserCreationService(ctx, hasher);

            var model = new RegisterViewModel
            {
                Username = "tutor99",
                FirstName = "Tutor",
                LastName = "Ninety",
                Email = "tutor99@tutor.com",
                Password = "Secret123!",
                ConfirmPassword = "Secret123!",
                Role = "Tutor",
                Skills = "Math"
            };

            await svc.CreateUser(model, RoleIds.Tutor);

            var created = ctx.Users.Single(u => u.Email == "tutor99@tutor.com");
            Assert.Equal(RoleIds.Tutor, created.RoleId);

            var tutor = ctx.Tutors.Single(t => t.UserId == created.Id && t.DeletedAt == null);
            Assert.Equal("Math", tutor.Skill);
        }

        [Fact]
        public async Task CreateUser_WithInvalidRoleId_Throws()
        {
            using var ctx = AdminTestDbFactory.CreateContext("AdminUserCreation_InvalidRole");
            var hasher = new PasswordHasher();
            var svc = new AdminUserCreationService(ctx, hasher);

            var model = new RegisterViewModel
            {
                Username = "u1",
                FirstName = "U",
                LastName = "One",
                Email = "u1@test.com",
                Password = "Secret123!",
                ConfirmPassword = "Secret123!",
                Role = "NotARole"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateUser(model, 999));
        }
    }
}
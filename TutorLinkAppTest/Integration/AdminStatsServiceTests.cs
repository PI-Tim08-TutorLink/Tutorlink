using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Implementations;

namespace TutorLinkAppTest.Integration
{
    public class AdminStatsServiceTests
    {
        [Fact]
        public async Task GetTotalUsers_ReturnsOnlyNotDeleted()
        {
            using var ctx = AdminTestDbFactory.CreateContext(nameof(GetTotalUsers_ReturnsOnlyNotDeleted));

            ctx.Users.AddRange(
                AdminTestDbFactory.NewUser(1, RoleIds.Admin, "a@a.com"),
                AdminTestDbFactory.NewUser(2, RoleIds.Student, "b@b.com"),
                AdminTestDbFactory.NewUser(3, RoleIds.Student, "c@c.com", deleted: true)
            );
            await ctx.SaveChangesAsync();

            var svc = new AdminStatsService(ctx);

            var total = await svc.GetTotalUsers();

            Assert.Equal(2, total);
        }

        [Fact]
        public async Task GetTotalTutors_ReturnsOnlyNotDeletedTutors()
        {
            using var ctx = AdminTestDbFactory.CreateContext(nameof(GetTotalTutors_ReturnsOnlyNotDeletedTutors));

            ctx.Tutors.AddRange(
                new Tutor { Id = 10, UserId = 1, Skill = "Math", CreatedAt = DateTime.UtcNow, DeletedAt = null },
                new Tutor { Id = 11, UserId = 2, Skill = "Phys", CreatedAt = DateTime.UtcNow, DeletedAt = DateTime.UtcNow }
            );
            await ctx.SaveChangesAsync();

            var svc = new AdminStatsService(ctx);

            var total = await svc.GetTotalTutors();

            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetTotalStudents_ReturnsOnlyRoleStudentsAndNotDeleted()
        {
            using var ctx = AdminTestDbFactory.CreateContext(nameof(GetTotalStudents_ReturnsOnlyRoleStudentsAndNotDeleted));

            ctx.Users.AddRange(
                AdminTestDbFactory.NewUser(1, RoleIds.Admin, "admin@x.com"),
                AdminTestDbFactory.NewUser(2, RoleIds.Student, "s1@x.com"),
                AdminTestDbFactory.NewUser(3, RoleIds.Student, "s2@x.com", deleted: true),
                AdminTestDbFactory.NewUser(4, RoleIds.Tutor, "t@x.com")
            );
            await ctx.SaveChangesAsync();

            var svc = new AdminStatsService(ctx);

            var total = await svc.GetTotalStudents();

            Assert.Equal(1, total);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TutorLinkApp.Models;

namespace TutorLinkAppTest.Integration
{
    internal static class AdminTestDbFactory
    {
        public static TutorLinkContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TutorLinkContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new TutorLinkContext(options);

            // Ensure model is created and roles exist.
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            if (!ctx.Roles.Any())
            {
                ctx.Roles.AddRange(
                    new Role { Id = RoleIds.Admin, Role1 = "Admin" },
                    new Role { Id = RoleIds.Student, Role1 = "Student" },
                    new Role { Id = RoleIds.Tutor, Role1 = "Tutor" }
                );
                ctx.SaveChanges();
            }

            return ctx;
        }

        public static async Task<Role> GetRoleAsync(TutorLinkContext ctx, int roleId)
        {
            var role = await ctx.Roles.SingleAsync(r => r.Id == roleId);
            return role;
        }

        public static User NewUser(int id, int roleId, string email, bool deleted = false)
        {
            return new User
            {
                Id = id,
                Username = $"user{id}",
                FirstName = "First",
                LastName = "Last",
                Email = email,
                PwdSalt = "salt",
                PwdHash = "hash",
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = deleted ? DateTime.UtcNow : null
            };
        }
    }
}

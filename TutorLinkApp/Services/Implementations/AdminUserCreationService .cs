using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Interfaces;


public class AdminUserCreationService : IAdminUserCreationService
{
    private readonly TutorLinkContext _context;
    private readonly IPasswordHasher _hasher;

    public AdminUserCreationService(TutorLinkContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task CreateUser(RegisterViewModel model, int roleId)
    {
        // 1) Role validation
        if (roleId != RoleIds.Admin && roleId != RoleIds.Student && roleId != RoleIds.Tutor)
            throw new InvalidOperationException("Invalid roleId.");

        // 2) Username validation
        var usernameTaken = await _context.Users.AnyAsync(u => u.Username == model.Username);
        if (usernameTaken)
            throw new InvalidOperationException("Username already exists.");

        // 3) Email validation - DODAJ OVO!
        var emailTaken = await _context.Users.AnyAsync(u => u.Email == model.Email);
        if (emailTaken)
            throw new InvalidOperationException("Email already exists.");

        var salt = _hasher.GenerateSalt();
        var hash = _hasher.Hash(model.Password, salt);

        // 4) Create user
        var user = new User
        {
            Username = model.Username,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PwdSalt = salt,
            PwdHash = hash,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            DeletedAt = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 5) Create Tutor profile
        if (roleId == RoleIds.Tutor)
        {
            var tutor = new Tutor
            {
                UserId = user.Id,
                Skill = model.Skills,
                DeletedAt = null
            };
            _context.Tutors.Add(tutor);
            await _context.SaveChangesAsync();
        }
    }
}
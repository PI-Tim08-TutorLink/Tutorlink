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

        // 2) Unique email (only not deleted)
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == model.Email && u.DeletedAt == null);

        if (emailExists)
            throw new InvalidOperationException("Email already exists.");

        // 3) Hash & Salt
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

        // 5) If Tutor -> create Tutor profile
        if (roleId == RoleIds.Tutor)
        {
            // Ako je Skills null/empty, možeš ili dopustiti prazno ili baciti grešku.
            // Za tvoje testove: dopusti, ali spremi string.
            var tutor = new Tutor
            {
                UserId = user.Id,
                Skill = model.Skills ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = null
            };

            _context.Tutors.Add(tutor);
            await _context.SaveChangesAsync();
        }
    }
}

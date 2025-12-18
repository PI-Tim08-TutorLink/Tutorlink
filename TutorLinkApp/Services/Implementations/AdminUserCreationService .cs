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

        Console.WriteLine(">>> CreateUser POST HIT");
        Console.WriteLine($">>> Username={model.Username}, Email={model.Email}, roleId={roleId}");
        // basic check
        var usernameTaken = await _context.Users.AnyAsync(u => u.Username == model.Username);
        if (usernameTaken)
            throw new InvalidOperationException("Username already exists.");

        // var salt = Guid.NewGuid().ToString("N");
        // var hash = _hasher.Hash(model.Password, salt);
        var salt = _hasher.GenerateSalt();
        var hash = _hasher.Hash(model.Password, salt);
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            RoleId = roleId,
            PwdSalt = salt,
            PwdHash = hash,
            CreatedAt = DateTime.Now
        };
      

        user.PwdSalt = salt;
        user.PwdHash = hash;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Tutor profil samo ako je tutor (ako imaš takvu logiku)
        if (roleId == RoleIds.Tutor)
        {
            // ako u modelu ima Skills ili sl. - dodaj samo ako postoji u tvom VM-u
            // _context.Tutors.Add(new Tutor { UserId = user.Id, Skill = model.Skills, CreatedAt = DateTime.Now });
            // await _context.SaveChangesAsync();
        }
    }


}

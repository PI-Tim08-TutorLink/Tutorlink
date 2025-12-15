using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;

public class AdminService : IAdminService
{
    private readonly TutorLinkContext _context;
    private readonly IPasswordHasher _hasher;

    public AdminService(TutorLinkContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<int> GetTotalUsers() => await _context.Users.CountAsync(u => u.DeletedAt == null);
    public async Task<int> GetTotalTutors() => await _context.Tutors.CountAsync(t => t.DeletedAt == null);
    public async Task<int> GetTotalStudents() => await _context.Users.CountAsync(u => u.RoleId == 2 && u.DeletedAt == null);
    public async Task<List<User>> GetAllUsers() => await _context.Users.Where(u => u.DeletedAt == null).Include(u => u.Tutors).ToListAsync();
    public async Task<User?> GetUserById(int id) => await _context.Users.FindAsync(id);

    public async Task CreateUser(RegisterViewModel model)
    {
        var salt = _hasher.GenerateSalt();
        var hash = _hasher.Hash(model.Password, salt);

        int roleId = model.Role.ToLower() == "tutor" ? 3 :
                     model.Role.ToLower() == "admin" ? 1 : 2;

        var user = new User
        {
            Email = model.Email,
            Username = model.Username,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PwdSalt = salt,
            PwdHash = hash,
            RoleId = roleId,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (roleId == 3 && !string.IsNullOrWhiteSpace(model.Skills))
        {
            var tutor = new Tutor
            {
                UserId = user.Id,
                Skill = model.Skills,
                CreatedAt = DateTime.Now
            };
            _context.Tutors.Add(tutor);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateUser(User user)
    {
        var existingUser = await _context.Users.FindAsync(user.Id);
        if (existingUser == null) throw new KeyNotFoundException();

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.Username = user.Username;
        existingUser.RoleId = user.RoleId;

        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return;

        user.DeletedAt = DateTime.Now;
        var tutors = await _context.Tutors.Where(t => t.UserId == id).ToListAsync();
        foreach (var tutor in tutors) tutor.DeletedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task<List<Tutor>> GetTutorsByUserId(int userId)
        => await _context.Tutors.Where(t => t.UserId == userId).ToListAsync();
}

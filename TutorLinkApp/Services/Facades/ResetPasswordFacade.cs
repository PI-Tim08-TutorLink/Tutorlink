using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;
using TutorLinkApp.Services.Email;
using TutorLinkApp.Services.Implementations;
using TutorLinkApp.Services.Interfaces;

public class ResetPasswordFacade : IResetPasswordFacade
{
    private readonly TutorLinkContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly EmailService _emailService;

    public ResetPasswordFacade(
        TutorLinkContext context,
        IPasswordHasher hasher,
        EmailService emailService)
    {
        _context = context;
        _hasher = hasher;
        _emailService = emailService;
    }

    public async Task<string?> SendResetLink(string email, string resetUrlBase)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);

        if (user == null)
            return null;

        user.ResetToken = Guid.NewGuid().ToString();
        user.ResetTokenExpiry = DateTime.Now.AddMinutes(30);

        await _context.SaveChangesAsync();

        var link = $"{resetUrlBase}?token={user.ResetToken}";

        await _emailService.SendResetPasswordEmail(email, link);

        return link;
    }

    public async Task<bool> ResetPassword(string token, string newPassword)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.ResetToken == token &&
                u.ResetTokenExpiry > DateTime.Now);

        if (user == null) return false;

        var salt = _hasher.GenerateSalt();
        var hash = _hasher.Hash(newPassword, salt);

        user.PwdSalt = salt;
        user.PwdHash = hash;
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _context.SaveChangesAsync();
        return true;
    }
}

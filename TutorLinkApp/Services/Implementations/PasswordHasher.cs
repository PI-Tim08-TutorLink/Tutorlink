using System.Security.Cryptography;
using System.Text;
using TutorLinkApp.Services.Interfaces;

public class PasswordHasher : IPasswordHasher
{
    public string GenerateSalt()
    {
        byte[] saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public string Hash(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] combined = new byte[saltBytes.Length + passwordBytes.Length];

        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(combined);
        return Convert.ToBase64String(hash);
    }

    public bool Verify(string password, string storedHash, string storedSalt)
    {
        return Hash(password, storedSalt) == storedHash;
    }
}
using System.Security.Cryptography;
using LeaveMgmt.Application.Abstractions.Security;

namespace LeaveMgmt.Infrastructure.Security;

internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;   // 128-bit
    private const int KeySize = 32;   // 256-bit
    private const int Iterations = 100_000;

    public (string Hash, string Salt) Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);
        return (Convert.ToBase64String(key), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);
        return CryptographicOperations.FixedTimeEquals(key, Convert.FromBase64String(hash));
    }
}

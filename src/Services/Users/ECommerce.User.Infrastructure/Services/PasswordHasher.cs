using System.Security.Cryptography;
using System.Text;
using ECommerce.User.Application.Interfaces;
using Konscious.Security.Cryptography;

namespace ECommerce.User.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 2;

    public string HashPassword(string password)
    {
        // Generate random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash password with Argon2id
        var hash = HashPasswordWithSalt(password, salt);

        // Combine salt and hash, then convert to Base64
        var hashBytes = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            // Decode the Base64 hash
            var hashBytes = Convert.FromBase64String(passwordHash);

            // Extract salt and hash
            var salt = new byte[SaltSize];
            var hash = new byte[HashSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(hashBytes, SaltSize, hash, 0, HashSize);

            // Hash the input password with the extracted salt
            var testHash = HashPasswordWithSalt(password, salt);

            // Compare hashes
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch
        {
            return false;
        }
    }

    private byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }
}

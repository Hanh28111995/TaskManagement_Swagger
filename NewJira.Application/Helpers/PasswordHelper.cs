using System.Security.Cryptography;

namespace NewJira.Application.Helpers;

public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algo = HashAlgorithmName.SHA256;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algo, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string hashed)
    {
        try
        {
            var parts = hashed.Split('.', 3);
            if (parts.Length != 3 || !int.TryParse(parts[0], out int iters)) return false;

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] key = Convert.FromBase64String(parts[2]);
            byte[] attempt = Rfc2898DeriveBytes.Pbkdf2(password, salt, iters, Algo, key.Length);

            return CryptographicOperations.FixedTimeEquals(key, attempt);
        }
        catch
        {
            return false;
        }
    }
}
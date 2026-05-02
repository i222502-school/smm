using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SocietiesManagementSystem.Security;

/// <summary>
/// SHA-256 over UTF-16 bytes of (password + salt string), matching SQL Server HASHBYTES('SHA2_256', nvarchar).
/// </summary>
public static class PasswordHasher
{
    public static byte[] HashPassword(string password, Guid salt)
    {
        var saltStr = salt.ToString("D", CultureInfo.InvariantCulture).ToUpperInvariant();
        var plain = password + saltStr;
        return SHA256.HashData(Encoding.Unicode.GetBytes(plain));
    }

    public static bool Verify(string password, Guid salt, byte[] storedHash)
    {
        var computed = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(computed, storedHash);
    }
}

using System.Security.Cryptography;
using System.Text;

namespace PersonalFinance.Services
{
    /// <summary>Утилиты хеширования паролей (SHA-256).</summary>
    public static class AuthService
    {
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty));
            return Convert.ToHexString(bytes);
        }

        public static bool Verify(string password, string hash)
            => string.Equals(HashPassword(password), hash, StringComparison.OrdinalIgnoreCase);
    }
}

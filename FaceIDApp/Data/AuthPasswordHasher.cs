using System;
using System.Linq;
using System.Security.Cryptography;

namespace FaceIDApp.Data
{
    internal static class AuthPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string Hash(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
                {
                    var hash = deriveBytes.GetBytes(HashSize);
                    return string.Join("$", new[]
                    {
                        "pbkdf2",
                        Iterations.ToString(),
                        Convert.ToBase64String(salt),
                        Convert.ToBase64String(hash)
                    });
                }
            }
        }

        public static bool Verify(string password, string storedHash)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            var parts = storedHash.Split('$');
            if (parts.Length != 4 || !string.Equals(parts[0], "pbkdf2", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!int.TryParse(parts[1], out var iterations))
                return false;

            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                var actualHash = deriveBytes.GetBytes(expectedHash.Length);
                return FixedTimeEquals(actualHash, expectedHash);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;

            var result = 0;
            for (var i = 0; i < left.Length; i++)
                result |= left[i] ^ right[i];

            return result == 0;
        }
    }
}

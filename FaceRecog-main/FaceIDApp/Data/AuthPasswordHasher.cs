using System;

namespace FaceIDApp.Data
{
    internal static class AuthPasswordHasher
    {
        private const int WorkFactor = 12;

        public static string Hash(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

        public static bool Verify(string password, string storedHash)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

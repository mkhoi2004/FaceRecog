using System;
using System.Configuration;

namespace FaceIDApp.Data
{
    internal sealed class DatabaseConfig
    {
        public DatabaseConfig(string applicationConnectionString)
        {
            if (string.IsNullOrWhiteSpace(applicationConnectionString))
                throw new ArgumentException("Application connection string is required.", nameof(applicationConnectionString));

            this.ApplicationConnectionString = applicationConnectionString;
        }

        public string ApplicationConnectionString { get; }

        public static DatabaseConfig LoadFromConfiguration()
        {
            var sqliteConnectionString = ConfigurationManager.ConnectionStrings["SqliteAdmin"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(sqliteConnectionString))
            {
                sqliteConnectionString = ReadSetting("DATABASE_URL");
            }

            if (string.IsNullOrWhiteSpace(sqliteConnectionString))
            {
                // Fallback to a default
                sqliteConnectionString = "Data Source=face_attendance.db;Version=3;";
            }

            return new DatabaseConfig(sqliteConnectionString);
        }

        private static string ReadSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}

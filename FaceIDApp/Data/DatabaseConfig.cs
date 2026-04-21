using System;
using System.Configuration;
using Npgsql;

namespace FaceIDApp.Data
{
    internal sealed class DatabaseConfig
    {
        public DatabaseConfig(string adminConnectionString, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(adminConnectionString))
                throw new ArgumentException("Admin connection string is required.", nameof(adminConnectionString));

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name is required.", nameof(databaseName));

            this.AdminConnectionString = adminConnectionString;
            this.DatabaseName = databaseName;
        }

        public string AdminConnectionString { get; }

        public string DatabaseName { get; }

        public string ApplicationConnectionString
        {
            get
            {
                var builder = new NpgsqlConnectionStringBuilder(this.AdminConnectionString)
                {
                    Database = this.DatabaseName
                };
                return builder.ConnectionString;
            }
        }

        public static DatabaseConfig LoadFromConfiguration()
        {
            var databaseUrl = ReadSetting("DATABASE_URL");
            var adminConnectionString = ConfigurationManager.ConnectionStrings["PostgresAdmin"]?.ConnectionString;
            var databaseName = ReadSetting("PostgresDatabaseName");

            if (string.IsNullOrWhiteSpace(adminConnectionString) && !string.IsNullOrWhiteSpace(databaseUrl))
            {
                var parsed = ParseDatabaseUrl(databaseUrl);
                adminConnectionString = parsed.Item1;
                if (!string.IsNullOrWhiteSpace(parsed.Item2))
                    databaseName = parsed.Item2;
            }

            if (string.IsNullOrWhiteSpace(adminConnectionString))
                throw new ConfigurationErrorsException("Missing connection string 'PostgresAdmin'.");

            return new DatabaseConfig(
                adminConnectionString,
                string.IsNullOrWhiteSpace(databaseName) ? "face_attendance" : databaseName.Trim());
        }

        private static string ReadSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static Tuple<string, string> ParseDatabaseUrl(string databaseUrl)
        {
            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
                throw new ConfigurationErrorsException($"Invalid DATABASE_URL: '{databaseUrl}'.");

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]),
                Password = uri.UserInfo.Contains(":")
                    ? Uri.UnescapeDataString(uri.UserInfo.Substring(uri.UserInfo.IndexOf(':') + 1))
                    : string.Empty,
                Pooling = true
            };

            var path = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(path))
                builder.Database = path;

            return Tuple.Create(builder.ConnectionString, string.IsNullOrWhiteSpace(path) ? null : path);
        }
    }
}

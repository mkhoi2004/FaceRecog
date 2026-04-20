using System;
using System.Configuration;
using System.Collections.Specialized;
using Npgsql;

namespace FaceRecog.WinForms.Data
{
    internal sealed class PostgresOptions
    {
        public PostgresOptions(string adminConnectionString, string databaseName)
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

        public static PostgresOptions LoadFromConfiguration()
        {
            var databaseUrl = ReadSetting("DATABASE_URL");
            var adminConnectionString = ConfigurationManager.ConnectionStrings["PostgresAdmin"]?.ConnectionString;
            var databaseName = ReadSetting("PostgresDatabaseName");

            if (string.IsNullOrWhiteSpace(adminConnectionString) && !string.IsNullOrWhiteSpace(databaseUrl))
            {
                var parsed = ParseDatabaseUrl(databaseUrl);
                adminConnectionString = parsed.AdminConnectionString;
                if (!string.IsNullOrWhiteSpace(parsed.DatabaseName))
                    databaseName = parsed.DatabaseName;
            }

            return new PostgresOptions(
                string.IsNullOrWhiteSpace(adminConnectionString) ? throw new ConfigurationErrorsException("Missing connection string 'PostgresAdmin'.") : adminConnectionString,
                string.IsNullOrWhiteSpace(databaseName) ? "face_recognition_winforms" : databaseName.Trim());
        }

        private static string ReadSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static ParsedDatabaseUrl ParseDatabaseUrl(string databaseUrl)
        {
            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
                throw new ConfigurationErrorsException($"Invalid DATABASE_URL value: '{databaseUrl}'.");

            if (!uri.Scheme.Equals("postgres", StringComparison.OrdinalIgnoreCase) &&
                !uri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationErrorsException("DATABASE_URL must use the postgres or postgresql scheme.");
            }

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]),
                Password = uri.UserInfo.Contains(":") ? Uri.UnescapeDataString(uri.UserInfo.Substring(uri.UserInfo.IndexOf(':') + 1)) : string.Empty,
                Pooling = true
            };

            var path = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(path))
                builder.Database = path;

            return new ParsedDatabaseUrl(builder.ConnectionString, string.IsNullOrWhiteSpace(path) ? null : path);
        }

        private readonly struct ParsedDatabaseUrl
        {
            public ParsedDatabaseUrl(string adminConnectionString, string databaseName)
            {
                this.AdminConnectionString = adminConnectionString;
                this.DatabaseName = databaseName;
            }

            public string AdminConnectionString { get; }

            public string DatabaseName { get; }
        }
    }
}

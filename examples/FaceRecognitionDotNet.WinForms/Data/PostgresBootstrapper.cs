using System;
using System.Threading.Tasks;
using Npgsql;

namespace FaceRecognitionDotNet.WinForms.Data
{
    internal sealed class PostgresBootstrapper
    {
        private readonly PostgresOptions _options;

        public PostgresBootstrapper(PostgresOptions options)
        {
            this._options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InitializeAsync()
        {
            await this.EnsureDatabaseExistsAsync().ConfigureAwait(false);
            await this.EnsureSchemaExistsAsync().ConfigureAwait(false);
            await this.EnsureDefaultUsersAsync().ConfigureAwait(false);
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            var builder = new NpgsqlConnectionStringBuilder(this._options.AdminConnectionString)
            {
                Database = "postgres"
            };

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var existsCommand = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection))
                {
                    existsCommand.Parameters.AddWithValue("name", this._options.DatabaseName);
                    var existing = await existsCommand.ExecuteScalarAsync().ConfigureAwait(false);
                    if (existing != null)
                        return;
                }

                using (var createCommand = new NpgsqlCommand($"CREATE DATABASE {QuoteIdentifier(this._options.DatabaseName)}", connection))
                {
                    await createCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task EnsureSchemaExistsAsync()
        {
            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                const string createSessionsTable = @"
CREATE TABLE IF NOT EXISTS scan_sessions (
    id uuid PRIMARY KEY,
    scan_type varchar(30) NOT NULL,
    source_path text NOT NULL,
    model_name varchar(30) NOT NULL,
    cpu_count integer NULL,
    status varchar(30) NOT NULL DEFAULT 'Completed',
    result_count integer NOT NULL DEFAULT 0,
    started_at timestamptz NOT NULL DEFAULT now(),
    completed_at timestamptz NULL
);";

                const string createImagesTable = @"
CREATE TABLE IF NOT EXISTS images (
    id uuid PRIMARY KEY,
    file_path text NOT NULL UNIQUE,
    file_name text NOT NULL,
    file_extension varchar(16) NOT NULL,
    file_size bigint NULL,
    modified_at timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);";

                const string createDetectionsTable = @"
CREATE TABLE IF NOT EXISTS detections (
    id uuid PRIMARY KEY,
    session_id uuid NOT NULL REFERENCES scan_sessions(id) ON DELETE CASCADE,
    image_id uuid NOT NULL REFERENCES images(id) ON DELETE CASCADE,
    top integer NOT NULL,
    ""right"" integer NOT NULL,
    bottom integer NOT NULL,
    ""left"" integer NOT NULL,
    confidence numeric(8,5) NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);";

                const string createUsersTable = @"
CREATE TABLE IF NOT EXISTS app_users (
    id uuid PRIMARY KEY,
    username text NOT NULL UNIQUE,
    full_name text NOT NULL,
    role text NOT NULL DEFAULT 'User',
    password_hash text NOT NULL,
    face_encoding_data text NULL,
    face_image_path text NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    last_login_at timestamptz NULL
);";

                const string updateUsersTable = @"
ALTER TABLE app_users
    ADD COLUMN IF NOT EXISTS role text NOT NULL DEFAULT 'User';

ALTER TABLE app_users
    ADD COLUMN IF NOT EXISTS face_image_path text NULL;";

                const string createLoginLogsTable = @"
CREATE TABLE IF NOT EXISTS login_logs (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    logged_in_at timestamptz NOT NULL DEFAULT now()
);";

                const string createLoginLogsIndex = @"
CREATE INDEX IF NOT EXISTS ix_login_logs_user_id ON login_logs(user_id);
CREATE INDEX IF NOT EXISTS ix_login_logs_logged_in_at ON login_logs(logged_in_at);";

                const string createAttendanceTable = @"
CREATE TABLE IF NOT EXISTS attendance_logs (
    id uuid PRIMARY KEY,
    user_id uuid NULL REFERENCES app_users(id) ON DELETE SET NULL,
    captured_image_path text NULL,
    model_name varchar(30) NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'Present',
    match_distance numeric(10,6) NULL,
    attended_at timestamptz NOT NULL DEFAULT now()
);";

                const string createIndexes = @"
CREATE INDEX IF NOT EXISTS ix_images_file_path ON images(file_path);
CREATE INDEX IF NOT EXISTS ix_detections_session_id ON detections(session_id);
CREATE INDEX IF NOT EXISTS ix_app_users_username ON app_users(username);
CREATE INDEX IF NOT EXISTS ix_attendance_logs_user_id ON attendance_logs(user_id);
CREATE INDEX IF NOT EXISTS ix_attendance_logs_attended_at ON attendance_logs(attended_at);";

                foreach (var commandText in new[] { createSessionsTable, createImagesTable, createDetectionsTable, createUsersTable, updateUsersTable, createLoginLogsTable, createAttendanceTable, createIndexes, createLoginLogsIndex })
                {
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task EnsureDefaultUsersAsync()
        {
            var defaultUsers = new[]
            {
                new DefaultUser("user123", "User", "user123", "User"),
                new DefaultUser("admin123", "Admin", "admin123", "Admin")
            };

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                foreach (var defaultUser in defaultUsers)
                {
                    using (var existsCommand = new NpgsqlCommand(@"
SELECT 1
FROM app_users
WHERE lower(username) = lower(@username)
LIMIT 1;", connection))
                    {
                        existsCommand.Parameters.AddWithValue("username", defaultUser.Username);

                        var existing = await existsCommand.ExecuteScalarAsync().ConfigureAwait(false);
                        if (existing != null)
                            continue;
                    }

                    using (var insertCommand = new NpgsqlCommand(@"
INSERT INTO app_users (id, username, full_name, role, password_hash, created_at)
VALUES (@id, @username, @full_name, @role, @password_hash, now());", connection))
                    {
                        insertCommand.Parameters.AddWithValue("id", Guid.NewGuid());
                        insertCommand.Parameters.AddWithValue("username", defaultUser.Username);
                        insertCommand.Parameters.AddWithValue("full_name", defaultUser.FullName);
                        insertCommand.Parameters.AddWithValue("role", defaultUser.Role);
                        insertCommand.Parameters.AddWithValue("password_hash", AuthPasswordHasher.Hash(defaultUser.Password));
                        await insertCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        private static string QuoteIdentifier(string identifier)
        {
            return '"' + identifier.Replace("\"", "\"\"") + '"';
        }

        private sealed class DefaultUser
        {
            public DefaultUser(string username, string fullName, string password, string role)
            {
                this.Username = username;
                this.FullName = fullName;
                this.Password = password;
                this.Role = role;
            }

            public string Username { get; }

            public string FullName { get; }

            public string Password { get; }

            public string Role { get; }
        }
    }
}

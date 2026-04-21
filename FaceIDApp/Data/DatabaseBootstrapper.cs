using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

namespace FaceIDApp.Data
{
    internal sealed class DatabaseBootstrapper
    {
        private readonly DatabaseConfig _config;

        public DatabaseBootstrapper(DatabaseConfig config)
        {
            this._config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task InitializeAsync()
        {
            await EnsureDatabaseExistsAsync();
            await EnsureSchemaExistsAsync();
            await EnsureDefaultAdminAsync();
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            var builder = new NpgsqlConnectionStringBuilder(this._config.AdminConnectionString)
            {
                Database = "postgres"
            };

            using (var conn = new NpgsqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", conn))
                {
                    cmd.Parameters.AddWithValue("name", this._config.DatabaseName);
                    if (await cmd.ExecuteScalarAsync() != null)
                        return;
                }

                using (var cmd = new NpgsqlCommand($"CREATE DATABASE \"{this._config.DatabaseName.Replace("\"", "\"\"")}\"", conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task EnsureSchemaExistsAsync()
        {
            using (var conn = new NpgsqlConnection(this._config.ApplicationConnectionString))
            {
                await conn.OpenAsync();

                // Check if schema already exists
                using (var cmd = new NpgsqlCommand(
                    "SELECT 1 FROM information_schema.tables WHERE table_name = 'employees' AND table_schema = 'public' LIMIT 1", conn))
                {
                    if (await cmd.ExecuteScalarAsync() != null)
                        return; // Schema already exists
                }

                // Try to load SQL file
                var sqlPath = FindSqlFile();
                if (sqlPath != null)
                {
                    var sql = File.ReadAllText(sqlPath);
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 120;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    return;
                }

                // Fallback: create minimal schema inline
                await CreateMinimalSchemaAsync(conn);
            }
        }

        private async Task EnsureDefaultAdminAsync()
        {
            using (var conn = new NpgsqlConnection(this._config.ApplicationConnectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT 1 FROM users WHERE lower(username) = 'admin' LIMIT 1", conn))
                {
                    if (await cmd.ExecuteScalarAsync() != null)
                        return;
                }

                var passwordHash = AuthPasswordHasher.Hash("admin123");
                using (var cmd = new NpgsqlCommand(@"
INSERT INTO users (username, password_hash, role, must_change_password)
VALUES ('admin', @hash, 'Admin', FALSE)", conn))
                {
                    cmd.Parameters.AddWithValue("hash", passwordHash);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static string FindSqlFile()
        {
            var candidates = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "face_attendance_v3.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "face_attendance_v3.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Database", "face_attendance_v3.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Database", "face_attendance_v3.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Database", "face_attendance_v3.sql"),
            };

            foreach (var path in candidates)
            {
                try
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                        return fullPath;
                }
                catch { }
            }

            return null;
        }

        private static async Task CreateMinimalSchemaAsync(NpgsqlConnection conn)
        {
            const string sql = @"
SET timezone = 'Asia/Ho_Chi_Minh';

CREATE TABLE IF NOT EXISTS departments (
    id SERIAL PRIMARY KEY, code VARCHAR(20) NOT NULL UNIQUE, name VARCHAR(100) NOT NULL,
    description TEXT, parent_id INT REFERENCES departments(id) ON DELETE SET NULL,
    manager_id INT, is_active BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order SMALLINT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS positions (
    id SERIAL PRIMARY KEY, code VARCHAR(20) NOT NULL UNIQUE, name VARCHAR(100) NOT NULL,
    level SMALLINT NOT NULL DEFAULT 1, description TEXT, is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS work_shifts (
    id SERIAL PRIMARY KEY, code VARCHAR(20) NOT NULL UNIQUE, name VARCHAR(100) NOT NULL,
    shift_type VARCHAR(20) NOT NULL DEFAULT 'Fixed',
    start_time TIME NOT NULL, end_time TIME NOT NULL,
    break_minutes SMALLINT NOT NULL DEFAULT 60, standard_hours DECIMAL(4,2) NOT NULL DEFAULT 8,
    late_threshold SMALLINT NOT NULL DEFAULT 15, early_threshold SMALLINT NOT NULL DEFAULT 15,
    is_overnight BOOLEAN NOT NULL DEFAULT FALSE, color_code VARCHAR(7),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS employees (
    id SERIAL PRIMARY KEY, code VARCHAR(20) NOT NULL UNIQUE, full_name VARCHAR(100) NOT NULL,
    gender CHAR(1), date_of_birth DATE, phone VARCHAR(15), email VARCHAR(100) UNIQUE,
    identity_card VARCHAR(20) UNIQUE,
    department_id INT REFERENCES departments(id) ON DELETE SET NULL,
    position_id INT REFERENCES positions(id) ON DELETE SET NULL,
    default_shift_id INT REFERENCES work_shifts(id) ON DELETE SET NULL,
    manager_id INT REFERENCES employees(id) ON DELETE SET NULL,
    hire_date DATE NOT NULL DEFAULT CURRENT_DATE, termination_date DATE,
    employment_type VARCHAR(20) NOT NULL DEFAULT 'FullTime',
    work_location VARCHAR(100), avatar_path TEXT,
    is_face_registered BOOLEAN NOT NULL DEFAULT FALSE, face_registered_at TIMESTAMPTZ,
    annual_leave_days DECIMAL(5,1) NOT NULL DEFAULT 12,
    used_leave_days DECIMAL(5,1) NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY, username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    employee_id INT UNIQUE REFERENCES employees(id) ON DELETE SET NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'Employee',
    is_active BOOLEAN NOT NULL DEFAULT TRUE, last_login TIMESTAMPTZ,
    failed_login_count SMALLINT NOT NULL DEFAULT 0, locked_until TIMESTAMPTZ,
    refresh_token_hash VARCHAR(500), refresh_token_expiry TIMESTAMPTZ,
    must_change_password BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS face_data (
    id SERIAL PRIMARY KEY, employee_id INT NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    encoding TEXT NOT NULL, image_path TEXT NOT NULL, thumbnail_path TEXT,
    image_index SMALLINT NOT NULL DEFAULT 1, angle VARCHAR(10),
    quality_score REAL NOT NULL DEFAULT 0, brightness REAL, sharpness REAL, face_bbox JSONB,
    is_active BOOLEAN NOT NULL DEFAULT TRUE, is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    verified_by INT REFERENCES users(id) ON DELETE SET NULL, verified_at TIMESTAMPTZ,
    registered_by INT REFERENCES users(id) ON DELETE SET NULL, note TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ,
    CONSTRAINT uq_face_emp_index UNIQUE (employee_id, image_index)
);

CREATE TABLE IF NOT EXISTS holidays (
    id SERIAL PRIMARY KEY, holiday_date DATE NOT NULL,
    name VARCHAR(100) NOT NULL, holiday_type VARCHAR(20) NOT NULL DEFAULT 'National',
    description TEXT, is_recurring BOOLEAN NOT NULL DEFAULT FALSE,
    year SMALLINT NOT NULL DEFAULT EXTRACT(YEAR FROM CURRENT_DATE)::SMALLINT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_holiday_date_year UNIQUE (holiday_date, year)
);

CREATE TABLE IF NOT EXISTS attendance_records (
    id BIGSERIAL PRIMARY KEY,
    employee_id INT NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    attendance_date DATE NOT NULL DEFAULT CURRENT_DATE,
    shift_id INT REFERENCES work_shifts(id) ON DELETE SET NULL,
    check_in TIMESTAMPTZ, check_in_device_id INT,
    check_in_image_path TEXT, check_in_method VARCHAR(20) DEFAULT 'Face',
    check_in_confidence REAL,
    check_in_latitude DECIMAL(10,8), check_in_longitude DECIMAL(11,8),
    check_out TIMESTAMPTZ, check_out_device_id INT,
    check_out_image_path TEXT, check_out_method VARCHAR(20),
    check_out_confidence REAL,
    check_out_latitude DECIMAL(10,8), check_out_longitude DECIMAL(11,8),
    status VARCHAR(20) NOT NULL DEFAULT 'NotYet',
    late_minutes SMALLINT NOT NULL DEFAULT 0,
    early_minutes SMALLINT NOT NULL DEFAULT 0,
    working_minutes INT NOT NULL DEFAULT 0,
    is_manual_edit BOOLEAN NOT NULL DEFAULT FALSE,
    manual_edit_by INT REFERENCES users(id) ON DELETE SET NULL,
    manual_edit_at TIMESTAMPTZ, manual_edit_reason TEXT,
    note TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ,
    CONSTRAINT uq_attendance_emp_date UNIQUE (employee_id, attendance_date)
);

CREATE TABLE IF NOT EXISTS attendance_logs (
    id BIGSERIAL PRIMARY KEY,
    attendance_id BIGINT REFERENCES attendance_records(id) ON DELETE SET NULL,
    employee_id INT REFERENCES employees(id) ON DELETE SET NULL,
    device_id INT,
    log_time TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    log_type VARCHAR(20) NOT NULL, method VARCHAR(20) NOT NULL DEFAULT 'Face',
    matched_face_id INT REFERENCES face_data(id) ON DELETE SET NULL,
    confidence REAL, face_distance REAL,
    image_path TEXT, latitude DECIMAL(10,8), longitude DECIMAL(11,8),
    ip_address VARCHAR(45),
    result VARCHAR(20) NOT NULL DEFAULT 'Success',
    fail_reason TEXT, raw_payload JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS leave_requests (
    id SERIAL PRIMARY KEY,
    employee_id INT NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    leave_type VARCHAR(20) NOT NULL,
    start_date DATE NOT NULL, end_date DATE NOT NULL,
    total_days DECIMAL(5,1) NOT NULL, is_half_day BOOLEAN NOT NULL DEFAULT FALSE,
    half_day_period VARCHAR(10), reason TEXT NOT NULL, document_path TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    approved_by INT REFERENCES employees(id) ON DELETE SET NULL,
    approved_at TIMESTAMPTZ, reject_reason TEXT, note TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at TIMESTAMPTZ
);
";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 120;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}

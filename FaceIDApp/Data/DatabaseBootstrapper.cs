using System;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;

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
            await EnsureViewsUpToDateAsync();
            await EnsureDefaultAdminAsync();
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            // SQLite creates the database file automatically when opened if it doesn't exist.
            // We can just verify the directory exists.
            var builder = new SQLiteConnectionStringBuilder(this._config.ApplicationConnectionString);
            var dbPath = builder.DataSource;
            if (!string.IsNullOrWhiteSpace(dbPath) && dbPath != ":memory:")
            {
                var dir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                if (!File.Exists(dbPath))
                {
                    SQLiteConnection.CreateFile(dbPath);
                }
            }
            await Task.CompletedTask;
        }

        private async Task EnsureSchemaExistsAsync()
        {
            using (var conn = new SQLiteConnection(this._config.ApplicationConnectionString))
            {
                await conn.OpenAsync();

                // Check if schema already exists
                using (var cmd = new SQLiteCommand(
                    "SELECT 1 FROM sqlite_master WHERE type='table' AND name='employees' LIMIT 1", conn))
                {
                    if (await cmd.ExecuteScalarAsync() != null)
                        return; // Schema already exists
                }

                // Try to load SQL file
                var sqlPath = FindSqlFile();
                if (sqlPath != null)
                {
                    var sql = File.ReadAllText(sqlPath);
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 120;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    
                    // Re-hash all seed user passwords with BCrypt (the SQL file may have static/test hashes)
                    await RehashSeedPasswordsAsync(conn);
                    return;
                }

                // Fallback: create minimal schema inline
                await CreateMinimalSchemaAsync(conn);
            }
        }

        private async Task EnsureDefaultAdminAsync()
        {
            using (var conn = new SQLiteConnection(this._config.ApplicationConnectionString))
            {
                await conn.OpenAsync();

                long? adminId = null;
                string adminHash = null;
                using (var cmd = new SQLiteCommand("SELECT id, password_hash FROM users WHERE lower(username) = 'admin' LIMIT 1", conn))
                using (var r = (SQLiteDataReader)await cmd.ExecuteReaderAsync())
                {
                    if (await r.ReadAsync())
                    {
                        adminId = r.GetInt64(0);
                        adminHash = r.IsDBNull(1) ? null : r.GetString(1);
                    }
                }

                if (adminId.HasValue)
                {
                    // One-time migration from legacy PBKDF2 format to BCrypt.
                    if (!string.IsNullOrWhiteSpace(adminHash) &&
                        adminHash.StartsWith("pbkdf2$", StringComparison.OrdinalIgnoreCase))
                    {
                        var migratedHash = AuthPasswordHasher.Hash("admin123");
                        using (var cmd = new SQLiteCommand(@"
UPDATE users
SET password_hash = @hash,
    must_change_password = 1,
    failed_login_count = 0,
    locked_until = NULL
WHERE id = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@hash", migratedHash);
                            cmd.Parameters.AddWithValue("@id", adminId.Value);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    return;
                }

                var passwordHash = AuthPasswordHasher.Hash("admin123");
                using (var cmd = new SQLiteCommand(@"
INSERT INTO users (username, password_hash, role, must_change_password)
VALUES ('admin', @hash, 'Admin', 0)", conn))
                {
                    cmd.Parameters.AddWithValue("@hash", passwordHash);
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

        /// <summary>
        /// After running seed SQL, re-hash user passwords to valid BCrypt.
        /// admin → admin123  |  user → user123
        /// </summary>
        private static async Task RehashSeedPasswordsAsync(SQLiteConnection conn)
        {
            // Hash admin password
            var adminHash = AuthPasswordHasher.Hash("admin123");
            using (var cmd = new SQLiteCommand(
                "UPDATE users SET password_hash = @hash WHERE lower(username) = 'admin'", conn))
            {
                cmd.Parameters.AddWithValue("@hash", adminHash);
                await cmd.ExecuteNonQueryAsync();
            }

            // Hash user password
            var userHash = AuthPasswordHasher.Hash("user123");
            using (var cmd = new SQLiteCommand(
                "UPDATE users SET password_hash = @hash WHERE lower(username) = 'user'", conn))
            {
                cmd.Parameters.AddWithValue("@hash", userHash);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task CreateMinimalSchemaAsync(SQLiteConnection conn)
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS departments (
    id INTEGER PRIMARY KEY AUTOINCREMENT, code TEXT NOT NULL UNIQUE, name TEXT NOT NULL,
    description TEXT, parent_id INTEGER REFERENCES departments(id) ON DELETE SET NULL,
    manager_id INTEGER, is_active INTEGER NOT NULL DEFAULT 1,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME
);

CREATE TABLE IF NOT EXISTS positions (
    id INTEGER PRIMARY KEY AUTOINCREMENT, code TEXT NOT NULL UNIQUE, name TEXT NOT NULL,
    level INTEGER NOT NULL DEFAULT 1, description TEXT, is_active INTEGER NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME
);

CREATE TABLE IF NOT EXISTS work_shifts (
    id INTEGER PRIMARY KEY AUTOINCREMENT, code TEXT NOT NULL UNIQUE, name TEXT NOT NULL,
    shift_type TEXT NOT NULL DEFAULT 'Fixed',
    start_time TEXT NOT NULL, end_time TEXT NOT NULL,
    break_minutes INTEGER NOT NULL DEFAULT 60, standard_hours REAL NOT NULL DEFAULT 8,
    late_threshold INTEGER NOT NULL DEFAULT 15, early_threshold INTEGER NOT NULL DEFAULT 15,
    is_overnight INTEGER NOT NULL DEFAULT 0, color_code TEXT,
    is_active INTEGER NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME
);

CREATE TABLE IF NOT EXISTS employees (
    id INTEGER PRIMARY KEY AUTOINCREMENT, code TEXT NOT NULL UNIQUE, full_name TEXT NOT NULL,
    gender TEXT, date_of_birth TEXT, phone TEXT, email TEXT UNIQUE,
    identity_card TEXT UNIQUE,
    department_id INTEGER REFERENCES departments(id) ON DELETE SET NULL,
    position_id INTEGER REFERENCES positions(id) ON DELETE SET NULL,
    default_shift_id INTEGER REFERENCES work_shifts(id) ON DELETE SET NULL,
    manager_id INTEGER REFERENCES employees(id) ON DELETE SET NULL,
    hire_date TEXT NOT NULL DEFAULT CURRENT_DATE, termination_date TEXT,
    employment_type TEXT NOT NULL DEFAULT 'FullTime',
    work_location TEXT, avatar_path TEXT,
    is_face_registered INTEGER NOT NULL DEFAULT 0, face_registered_at DATETIME,
    annual_leave_days REAL NOT NULL DEFAULT 12,
    used_leave_days REAL NOT NULL DEFAULT 0,
    is_active INTEGER NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME
);

CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT, username TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    employee_id INTEGER UNIQUE REFERENCES employees(id) ON DELETE SET NULL,
    role TEXT NOT NULL DEFAULT 'Employee',
    is_active INTEGER NOT NULL DEFAULT 1, last_login DATETIME,
    failed_login_count INTEGER NOT NULL DEFAULT 0, locked_until DATETIME,
    refresh_token_hash TEXT, refresh_token_expiry DATETIME,
    must_change_password INTEGER NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME
);

CREATE TABLE IF NOT EXISTS face_data (
    id INTEGER PRIMARY KEY AUTOINCREMENT, employee_id INTEGER NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    encoding TEXT NOT NULL, image_path TEXT NOT NULL, thumbnail_path TEXT,
    image_index INTEGER NOT NULL DEFAULT 1, angle TEXT,
    quality_score REAL NOT NULL DEFAULT 0, brightness REAL, sharpness REAL, face_bbox TEXT,
    is_active INTEGER NOT NULL DEFAULT 1, is_verified INTEGER NOT NULL DEFAULT 0,
    verified_by INTEGER REFERENCES users(id) ON DELETE SET NULL, verified_at DATETIME,
    registered_by INTEGER REFERENCES users(id) ON DELETE SET NULL, note TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME,
    UNIQUE (employee_id, image_index)
);

CREATE TABLE IF NOT EXISTS holidays (
    id INTEGER PRIMARY KEY AUTOINCREMENT, holiday_date TEXT NOT NULL,
    name TEXT NOT NULL, holiday_type TEXT NOT NULL DEFAULT 'National',
    description TEXT, is_recurring INTEGER NOT NULL DEFAULT 0,
    year INTEGER NOT NULL DEFAULT (strftime('%Y', CURRENT_DATE)),
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (holiday_date, year)
);

CREATE TABLE IF NOT EXISTS attendance_records (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id INTEGER NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    attendance_date TEXT NOT NULL DEFAULT CURRENT_DATE,
    shift_id INTEGER REFERENCES work_shifts(id) ON DELETE SET NULL,
    check_in DATETIME, check_in_device_id INTEGER,
    check_in_image_path TEXT, check_in_method TEXT DEFAULT 'Face',
    check_in_confidence REAL,
    check_in_latitude REAL, check_in_longitude REAL,
    check_out DATETIME, check_out_device_id INTEGER,
    check_out_image_path TEXT, check_out_method TEXT,
    check_out_confidence REAL,
    check_out_latitude REAL, check_out_longitude REAL,
    status TEXT NOT NULL DEFAULT 'NotYet',
    late_minutes INTEGER NOT NULL DEFAULT 0,
    early_minutes INTEGER NOT NULL DEFAULT 0,
    working_minutes INTEGER NOT NULL DEFAULT 0,
    is_manual_edit INTEGER NOT NULL DEFAULT 0,
    manual_edit_by INTEGER REFERENCES users(id) ON DELETE SET NULL,
    manual_edit_at DATETIME, manual_edit_reason TEXT,
    note TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME,
    UNIQUE (employee_id, attendance_date)
);

CREATE TABLE IF NOT EXISTS attendance_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    attendance_id INTEGER REFERENCES attendance_records(id) ON DELETE SET NULL,
    employee_id INTEGER REFERENCES employees(id) ON DELETE SET NULL,
    device_id INTEGER,
    log_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    log_type TEXT NOT NULL, method TEXT NOT NULL DEFAULT 'Face',
    matched_face_id INTEGER REFERENCES face_data(id) ON DELETE SET NULL,
    confidence REAL, face_distance REAL,
    image_path TEXT, latitude REAL, longitude REAL,
    ip_address TEXT,
    result TEXT NOT NULL DEFAULT 'Success',
    fail_reason TEXT, raw_payload TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS leave_requests (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id INTEGER NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
    leave_type TEXT NOT NULL,
    start_date TEXT NOT NULL, end_date TEXT NOT NULL,
    total_days REAL NOT NULL, is_half_day INTEGER NOT NULL DEFAULT 0,
    half_day_period TEXT, reason TEXT NOT NULL, document_path TEXT,
    status TEXT NOT NULL DEFAULT 'Pending',
    approved_by INTEGER REFERENCES employees(id) ON DELETE SET NULL,
    approved_at DATETIME, reject_reason TEXT, note TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, updated_at DATETIME
);
";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.CommandTimeout = 120;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        private async Task EnsureViewsUpToDateAsync()
        {
            var sqlPath = FindSqlFile();
            if (sqlPath == null) return;

            try
            {
                var sql = File.ReadAllText(sqlPath);
                
                using (var conn = new SQLiteConnection(this._config.ApplicationConnectionString))
                {
                    await conn.OpenAsync();
                    
                    var viewNames = new[] { 
                        "V_TODAY_ATTENDANCE", 
                        "V_MONTHLY_SUMMARY", 
                        "V_FACE_STATUS", 
                        "V_ATTENDANCE_ANOMALIES",
                        "V_PENDING_LEAVES",
                        "V_LEAVE_BALANCE",
                        "V_SUSPICIOUS_RECOGNITION"
                    };
                    
                    foreach (var viewName in viewNames)
                    {
                        using (var cmd = new SQLiteCommand($"DROP VIEW IF EXISTS {viewName}", conn))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // Extract only CREATE VIEW statements from the SQL file
                    var lines = sql.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var currentViewSql = new System.Text.StringBuilder();
                    bool inView = false;

                    foreach (var line in lines)
                    {
                        if (line.Trim().StartsWith("CREATE VIEW", StringComparison.OrdinalIgnoreCase))
                        {
                            inView = true;
                            currentViewSql.Clear();
                        }

                        if (inView)
                        {
                            currentViewSql.AppendLine(line);
                            if (line.Trim().EndsWith(";"))
                            {
                                using (var cmd = new SQLiteCommand(currentViewSql.ToString(), conn))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                }
                                inView = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"View update error: {ex.Message}");
            }
        }
    }
}

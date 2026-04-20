using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace FaceRecog.WinForms.Data
{
    internal sealed class PostgresRepository
    {
        private readonly PostgresOptions _options;

        public PostgresRepository(PostgresOptions options)
        {
            this._options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<Guid> SaveScanResultsAsync(
            string scanType,
            string sourcePath,
            string modelName,
            int? cpuCount,
            IEnumerable<string> resultLines)
        {
            if (string.IsNullOrWhiteSpace(scanType))
                throw new ArgumentException("Scan type is required.", nameof(scanType));

            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));

            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Model name is required.", nameof(modelName));

            var sessionId = Guid.NewGuid();
            var lines = (resultLines ?? Enumerable.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var transaction = connection.BeginTransaction())
                {
                    await this.InsertSessionAsync(connection, transaction, sessionId, scanType, sourcePath, modelName, cpuCount, lines.Length).ConfigureAwait(false);

                    foreach (var line in lines)
                    {
                        var parsed = ParseDetectionLine(line);
                        if (parsed == null)
                            continue;

                        var imageId = await UpsertImageAsync(connection, transaction, parsed.Value.ImagePath).ConfigureAwait(false);
                        await InsertDetectionAsync(connection, transaction, sessionId, imageId, parsed.Value).ConfigureAwait(false);
                    }

                    await this.UpdateSessionCompletionAsync(connection, transaction, sessionId, lines.Length).ConfigureAwait(false);
                    transaction.Commit();
                }
            }

            return sessionId;
        }

        public async Task<IReadOnlyList<AppUserItem>> GetUsersAsync()
        {
            var users = new List<AppUserItem>();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT id, username, full_name, role, password_hash, face_encoding_data, face_image_path, created_at, last_login_at
FROM app_users
ORDER BY created_at DESC;", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            users.Add(new AppUserItem
                            {
                                Id = reader.GetGuid(0),
                                Username = reader.GetString(1),
                                FullName = reader.GetString(2),
                                Role = reader.GetString(3),
                                PasswordHash = reader.GetString(4),
                                FaceEncodingData = reader.IsDBNull(5) ? null : reader.GetString(5),
                                FaceImagePath = reader.IsDBNull(6) ? null : reader.GetString(6),
                                CreatedAt = reader.GetDateTime(7),
                                LastLoginAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return users;
        }

        public async Task<AppUserItem> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT id, username, full_name, role, password_hash, face_encoding_data, face_image_path, created_at, last_login_at
FROM app_users
WHERE lower(username) = lower(@username)
LIMIT 1;", connection))
                {
                    command.Parameters.AddWithValue("username", username.Trim());

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (!await reader.ReadAsync().ConfigureAwait(false))
                            return null;

                        return new AppUserItem
                        {
                            Id = reader.GetGuid(0),
                            Username = reader.GetString(1),
                            FullName = reader.GetString(2),
                            Role = reader.GetString(3),
                            PasswordHash = reader.GetString(4),
                            FaceEncodingData = reader.IsDBNull(5) ? null : reader.GetString(5),
                            FaceImagePath = reader.IsDBNull(6) ? null : reader.GetString(6),
                            CreatedAt = reader.GetDateTime(7),
                            LastLoginAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                        };
                    }
                }
            }
        }

        public async Task<Guid> CreateUserAsync(string username, string fullName, string passwordHash, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.", nameof(username));

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));

            var userId = Guid.NewGuid();
            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
INSERT INTO app_users (id, username, full_name, role, password_hash, created_at)
VALUES (@id, @username, @full_name, @role, @password_hash, now());", connection))
                {
                    command.Parameters.AddWithValue("id", userId);
                    command.Parameters.AddWithValue("username", username.Trim());
                    command.Parameters.AddWithValue("full_name", fullName.Trim());
                    command.Parameters.AddWithValue("role", string.IsNullOrWhiteSpace(role) ? "User" : role.Trim());
                    command.Parameters.AddWithValue("password_hash", passwordHash);
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            return userId;
        }

        public async Task<bool> HasUserFaceEnrollmentAsync(Guid userId)
        {
            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT 1
FROM app_users
WHERE id = @id
  AND COALESCE(face_encoding_data, '') <> ''
  AND COALESCE(face_image_path, '') <> ''
LIMIT 1;", connection))
                {
                    command.Parameters.AddWithValue("id", userId);
                    var existing = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    return existing != null;
                }
            }
        }

        public async Task<bool> TryUpdateUserFaceEncodingAsync(Guid userId, string faceEncodingData, string faceImagePath = null)
        {
            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
UPDATE app_users
SET face_encoding_data = @face_encoding_data,
    face_image_path = @face_image_path
WHERE id = @id
  AND COALESCE(face_encoding_data, '') = ''
  AND COALESCE(face_image_path, '') = ''
RETURNING id;", connection))
                {
                    command.Parameters.AddWithValue("id", userId);
                    command.Parameters.AddWithValue("face_encoding_data", string.IsNullOrWhiteSpace(faceEncodingData) ? (object)DBNull.Value : faceEncodingData);
                    command.Parameters.AddWithValue("face_image_path", string.IsNullOrWhiteSpace(faceImagePath) ? (object)DBNull.Value : faceImagePath);
                    var inserted = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    return inserted != null;
                }
            }
        }

        public async Task UpdateUserLastLoginAsync(Guid userId)
        {
            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
UPDATE app_users
SET last_login_at = now()
WHERE id = @id;", connection))
                {
                    command.Parameters.AddWithValue("id", userId);
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task LogUserLoginAsync(Guid userId)
        {
            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
INSERT INTO login_logs (id, user_id, logged_in_at)
VALUES (@id, @user_id, now());", connection))
                {
                    command.Parameters.AddWithValue("id", Guid.NewGuid());
                    command.Parameters.AddWithValue("user_id", userId);
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<IReadOnlyList<LoginHistoryItem>> GetRecentLoginHistoryAsync(int limit = 200)
        {
            var history = new List<LoginHistoryItem>();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT l.id, l.user_id, u.username, u.full_name, u.role, l.logged_in_at
FROM login_logs l
LEFT JOIN app_users u ON u.id = l.user_id
ORDER BY l.logged_in_at DESC
LIMIT @limit;", connection))
                {
                    command.Parameters.AddWithValue("limit", limit);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            history.Add(new LoginHistoryItem
                            {
                                Id = reader.GetGuid(0),
                                UserId = reader.GetGuid(1),
                                Username = reader.IsDBNull(2) ? null : reader.GetString(2),
                                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Role = reader.IsDBNull(4) ? null : reader.GetString(4),
                                LoggedInAt = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }

            return history;
        }

        public async Task<bool> HasAttendanceAsync(Guid userId, string status, DateTimeOffset attendanceAt)
        {
            if (userId == Guid.Empty)
                return false;

            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required.", nameof(status));

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT 1
FROM attendance_logs
WHERE user_id = @user_id
  AND status = @status
  AND (attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh')::date = @attendance_day
LIMIT 1;", connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    command.Parameters.AddWithValue("status", status);
                    command.Parameters.AddWithValue("attendance_day", attendanceAt.Date);

                    var existing = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    return existing != null;
                }
            }
        }

        public async Task<bool> SaveAttendanceAsync(Guid? userId, string capturedImagePath, string modelName, double? matchDistance, string status, DateTimeOffset attendedAt)
        {
            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
INSERT INTO attendance_logs (id, user_id, captured_image_path, model_name, status, match_distance, attended_at)
VALUES (@id, @user_id, @captured_image_path, @model_name, @status, @match_distance, @attended_at)
ON CONFLICT DO NOTHING
RETURNING id;", connection))
                {
                    command.Parameters.AddWithValue("id", Guid.NewGuid());
                    command.Parameters.AddWithValue("user_id", (object)userId ?? DBNull.Value);
                    command.Parameters.AddWithValue("captured_image_path", string.IsNullOrWhiteSpace(capturedImagePath) ? (object)DBNull.Value : capturedImagePath);
                    command.Parameters.AddWithValue("model_name", modelName);
                    command.Parameters.AddWithValue("status", status);
                    command.Parameters.AddWithValue("match_distance", (object)matchDistance ?? DBNull.Value);
                    command.Parameters.AddWithValue("attended_at", attendedAt.UtcDateTime);

                    var inserted = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    return inserted != null;
                }
            }
        }

        public async Task<IReadOnlyList<AttendanceItem>> GetRecentAttendanceAsync(int limit = 50)
        {
            var attendance = new List<AttendanceItem>();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT a.id, a.user_id, u.username, u.full_name, a.captured_image_path, a.model_name, a.status, a.match_distance, a.attended_at
FROM attendance_logs a
LEFT JOIN app_users u ON u.id = a.user_id
ORDER BY a.attended_at DESC
LIMIT @limit;", connection))
                {
                    command.Parameters.AddWithValue("limit", limit);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            attendance.Add(new AttendanceItem
                            {
                                Id = reader.GetGuid(0),
                                UserId = reader.IsDBNull(1) ? (Guid?)null : reader.GetGuid(1),
                                Username = reader.IsDBNull(2) ? null : reader.GetString(2),
                                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CapturedImagePath = reader.IsDBNull(4) ? null : reader.GetString(4),
                                ModelName = reader.GetString(5),
                                Status = reader.GetString(6),
                                MatchDistance = reader.IsDBNull(7) ? (double?)null : Convert.ToDouble(reader.GetValue(7)),
                                AttendedAt = reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return attendance;
        }

        public async Task<IReadOnlyList<AttendanceSummaryItem>> GetAttendanceSummaryByDayAsync(DateTime attendanceDay)
        {
            var summaries = new List<AttendanceSummaryItem>();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT
    a.user_id,
    COALESCE(u.username, 'Unknown') AS username,
    COALESCE(u.full_name, 'Unknown') AS full_name,
    (a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh')::date AS attendance_day,
    MIN(CASE WHEN a.status = 'CheckIn' THEN a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh' END) AS check_in_at,
    MAX(CASE WHEN a.status = 'CheckOut' THEN a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh' END) AS check_out_at,
    COUNT(*) AS record_count
FROM attendance_logs a
LEFT JOIN app_users u ON u.id = a.user_id
WHERE (a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh')::date = @attendance_day
GROUP BY a.user_id, u.username, u.full_name, (a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh')::date
ORDER BY full_name, username;", connection))
                {
                    command.Parameters.AddWithValue("attendance_day", attendanceDay.Date);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var checkInAt = reader.IsDBNull(4) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Unspecified);
                            var checkOutAt = reader.IsDBNull(5) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(5), DateTimeKind.Unspecified);
                            var recordCount = reader.GetInt32(6);

                            summaries.Add(new AttendanceSummaryItem
                            {
                                UserId = reader.IsDBNull(0) ? (Guid?)null : reader.GetGuid(0),
                                Username = reader.GetString(1),
                                FullName = reader.GetString(2),
                                AttendanceDay = reader.GetDateTime(3),
                                CheckInAt = checkInAt,
                                CheckOutAt = checkOutAt,
                                RecordCount = recordCount,
                                WorkState = checkInAt.HasValue && checkOutAt.HasValue
                                    ? "Completed"
                                    : checkInAt.HasValue
                                        ? "In progress"
                                        : "Not started"
                            });
                        }
                    }
                }
            }

            return summaries;
        }

        public async Task<IReadOnlyList<AttendanceSummaryItem>> GetAttendanceSummaryAsync(int limit = 200)
        {
            var summaries = new List<AttendanceSummaryItem>();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT
    a.user_id,
    COALESCE(u.username, 'Unknown') AS username,
    COALESCE(u.full_name, 'Unknown') AS full_name,
    (a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh')::date AS attendance_day,
    MIN(CASE WHEN a.status = 'CheckIn' THEN a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh' END) AS check_in_at,
    MAX(CASE WHEN a.status = 'CheckOut' THEN a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh' END) AS check_out_at,
    COUNT(*) AS record_count
FROM attendance_logs a
LEFT JOIN app_users u ON u.id = a.user_id
GROUP BY a.user_id, u.username, u.full_name, (a.attended_at AT TIME ZONE 'Asia/Ho_Chi_Minh')::date
ORDER BY attendance_day DESC, full_name, username
LIMIT @limit;", connection))
                {
                    command.Parameters.AddWithValue("limit", limit);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var checkInAt = reader.IsDBNull(4) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Unspecified);
                            var checkOutAt = reader.IsDBNull(5) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(5), DateTimeKind.Unspecified);
                            var recordCount = reader.GetInt32(6);

                            summaries.Add(new AttendanceSummaryItem
                            {
                                UserId = reader.IsDBNull(0) ? (Guid?)null : reader.GetGuid(0),
                                Username = reader.GetString(1),
                                FullName = reader.GetString(2),
                                AttendanceDay = reader.GetDateTime(3),
                                CheckInAt = checkInAt,
                                CheckOutAt = checkOutAt,
                                RecordCount = recordCount,
                                WorkState = checkInAt.HasValue && checkOutAt.HasValue
                                    ? "Completed"
                                    : checkInAt.HasValue
                                        ? "In progress"
                                        : "Not started"
                            });
                        }
                    }
                }
            }

            return summaries;
        }

        public async Task<IReadOnlyList<ScanSessionItem>> GetRecentSessionsAsync(int limit = 50)
        {
            var sessions = new List<ScanSessionItem>();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT id, scan_type, source_path, model_name, cpu_count, status, result_count, started_at, completed_at
FROM scan_sessions
ORDER BY started_at DESC
LIMIT @limit;", connection))
                {
                    command.Parameters.AddWithValue("limit", limit);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            sessions.Add(new ScanSessionItem
                            {
                                Id = reader.GetGuid(0),
                                ScanType = reader.GetString(1),
                                SourcePath = reader.GetString(2),
                                ModelName = reader.GetString(3),
                                CpuCount = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                Status = reader.GetString(5),
                                ResultCount = reader.GetInt32(6),
                                StartedAt = reader.GetDateTime(7),
                                CompletedAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return sessions;
        }

        public async Task<IReadOnlyList<DetectionItem>> GetDetectionsBySessionAsync(Guid sessionId)
        {
            var detections = new List<DetectionItem>();

            using (var connection = new NpgsqlConnection(this._options.ApplicationConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = new NpgsqlCommand(@"
SELECT d.id, d.session_id, i.file_path, i.file_name, d.top, d.""right"", d.bottom, d.""left"", d.created_at
FROM detections d
INNER JOIN images i ON i.id = d.image_id
WHERE d.session_id = @session_id
ORDER BY i.file_name, d.created_at;", connection))
                {
                    command.Parameters.AddWithValue("session_id", sessionId);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            detections.Add(new DetectionItem
                            {
                                Id = reader.GetGuid(0),
                                SessionId = reader.GetGuid(1),
                                ImagePath = reader.GetString(2),
                                FileName = reader.GetString(3),
                                Top = reader.GetInt32(4),
                                Right = reader.GetInt32(5),
                                Bottom = reader.GetInt32(6),
                                Left = reader.GetInt32(7),
                                CreatedAt = reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return detections;
        }

        private static ParsedDetection? ParseDetectionLine(string line)
        {
            var last = line.LastIndexOf(',');
            if (last < 0)
                return null;

            var left = TryParseInt(line.Substring(last + 1));
            line = line.Substring(0, last);

            last = line.LastIndexOf(',');
            if (last < 0)
                return null;

            var bottom = TryParseInt(line.Substring(last + 1));
            line = line.Substring(0, last);

            last = line.LastIndexOf(',');
            if (last < 0)
                return null;

            var right = TryParseInt(line.Substring(last + 1));
            line = line.Substring(0, last);

            last = line.LastIndexOf(',');
            if (last < 0)
                return null;

            var top = TryParseInt(line.Substring(last + 1));
            var imagePath = line.Substring(0, last);

            if (!top.HasValue || !right.HasValue || !bottom.HasValue || !left.HasValue)
                return null;

            return new ParsedDetection(imagePath, top.Value, right.Value, bottom.Value, left.Value);
        }

        private static int? TryParseInt(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : (int?)null;
        }

        private async Task InsertSessionAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid sessionId, string scanType, string sourcePath, string modelName, int? cpuCount, int resultCount)
        {
            using (var command = new NpgsqlCommand(@"
INSERT INTO scan_sessions (id, scan_type, source_path, model_name, cpu_count, status, result_count, started_at, completed_at)
VALUES (@id, @scan_type, @source_path, @model_name, @cpu_count, 'Completed', @result_count, now(), now());", connection, transaction))
            {
                command.Parameters.AddWithValue("id", sessionId);
                command.Parameters.AddWithValue("scan_type", scanType);
                command.Parameters.AddWithValue("source_path", sourcePath);
                command.Parameters.AddWithValue("model_name", modelName);
                command.Parameters.AddWithValue("cpu_count", (object)cpuCount ?? DBNull.Value);
                command.Parameters.AddWithValue("result_count", resultCount);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private async Task UpdateSessionCompletionAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid sessionId, int resultCount)
        {
            using (var command = new NpgsqlCommand(@"
UPDATE scan_sessions
SET result_count = @result_count,
    completed_at = now(),
    status = 'Completed'
WHERE id = @id;", connection, transaction))
            {
                command.Parameters.AddWithValue("id", sessionId);
                command.Parameters.AddWithValue("result_count", resultCount);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private static async Task<Guid> UpsertImageAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string imagePath)
        {
            var storedImagePath = ImageStorage.StoreLocalCopy(imagePath);
            var fileInfo = new FileInfo(storedImagePath);
            var imageId = Guid.NewGuid();
            using (var command = new NpgsqlCommand(@"
INSERT INTO images (id, file_path, file_name, file_extension, file_size, modified_at)
VALUES (@id, @file_path, @file_name, @file_extension, @file_size, @modified_at)
ON CONFLICT (file_path)
DO UPDATE SET
    file_name = EXCLUDED.file_name,
    file_extension = EXCLUDED.file_extension,
    file_size = EXCLUDED.file_size,
    modified_at = EXCLUDED.modified_at
RETURNING id;", connection, transaction))
            {
                command.Parameters.AddWithValue("id", imageId);
                command.Parameters.AddWithValue("file_path", storedImagePath);
                command.Parameters.AddWithValue("file_name", fileInfo.Exists ? fileInfo.Name : Path.GetFileName(storedImagePath));
                command.Parameters.AddWithValue("file_extension", fileInfo.Exists ? fileInfo.Extension : Path.GetExtension(storedImagePath));
                command.Parameters.AddWithValue("file_size", fileInfo.Exists ? (object)fileInfo.Length : DBNull.Value);
                command.Parameters.AddWithValue("modified_at", fileInfo.Exists ? (object)fileInfo.LastWriteTimeUtc : DBNull.Value);

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                return result is Guid guid ? guid : imageId;
            }
        }

        private static async Task InsertDetectionAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid sessionId, Guid imageId, ParsedDetection detection)
        {
            using (var command = new NpgsqlCommand(@"
INSERT INTO detections (id, session_id, image_id, top, ""right"", bottom, ""left"", confidence)
VALUES (@id, @session_id, @image_id, @top, @right, @bottom, @left, @confidence);", connection, transaction))
            {
                command.Parameters.AddWithValue("id", Guid.NewGuid());
                command.Parameters.AddWithValue("session_id", sessionId);
                command.Parameters.AddWithValue("image_id", imageId);
                command.Parameters.AddWithValue("top", detection.Top);
                command.Parameters.AddWithValue("right", detection.Right);
                command.Parameters.AddWithValue("bottom", detection.Bottom);
                command.Parameters.AddWithValue("left", detection.Left);
                command.Parameters.AddWithValue("confidence", DBNull.Value);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private readonly struct ParsedDetection
        {
            public ParsedDetection(string imagePath, int top, int right, int bottom, int left)
            {
                this.ImagePath = imagePath;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
                this.Left = left;
            }

            public string ImagePath { get; }

            public int Top { get; }

            public int Right { get; }

            public int Bottom { get; }

            public int Left { get; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace FaceIDApp.Data
{
    internal sealed class Repository
    {
        private readonly DatabaseConfig _config;

        public Repository(DatabaseConfig config)
        {
            this._config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(this._config.ApplicationConnectionString);
        }

        private static async Task<bool> ColumnExistsAsync(SQLiteConnection conn, string tableName, string columnName)
        {
            using (var cmd = new SQLiteCommand($"PRAGMA table_info({tableName})", conn))
            using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    if (string.Equals(r.GetString(1), columnName, System.StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        // =============================================
        // DEPARTMENTS
        // =============================================
        public async Task<List<DepartmentDto>> GetDepartmentsAsync()
        {
            var list = new List<DepartmentDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("SELECT id, code, name, description, is_active FROM departments WHERE is_active = 1 ORDER BY sort_order, name", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new DepartmentDto
                        {
                            Id = r.GetInt32(0),
                            Code = r.GetString(1),
                            Name = r.GetString(2),
                            Description = r.IsDBNull(3) ? null : r.GetString(3),
                            IsActive = Convert.ToBoolean(r.GetValue(4))
                        });
                }
            }
            return list;
        }

        // =============================================
        // POSITIONS
        // =============================================
        public async Task<List<PositionDto>> GetPositionsAsync()
        {
            var list = new List<PositionDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("SELECT id, code, name, level, is_active FROM positions WHERE is_active = 1 ORDER BY level DESC, name", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new PositionDto
                        {
                            Id = r.GetInt32(0),
                            Code = r.GetString(1),
                            Name = r.GetString(2),
                            Level = r.GetInt32(3),
                            IsActive = Convert.ToBoolean(r.GetValue(4))
                        });
                }
            }
            return list;
        }

        // =============================================
        // WORK SHIFTS
        // =============================================
        public async Task<List<WorkShiftDto>> GetWorkShiftsAsync()
        {
            var list = new List<WorkShiftDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT id, code, name, shift_type, start_time, end_time, break_minutes,
       standard_hours, late_threshold, early_threshold, is_overnight, color_code, is_active
FROM work_shifts WHERE is_active = 1 ORDER BY start_time", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new WorkShiftDto
                        {
                            Id = r.GetInt32(0),
                            Code = r.GetString(1),
                            Name = r.GetString(2),
                            ShiftType = r.GetString(3),
                            StartTime = TimeSpan.Parse(r.GetString(4)),
                            EndTime = TimeSpan.Parse(r.GetString(5)),
                            BreakMinutes = Convert.ToInt16(r.GetValue(6)),
                            StandardHours = Convert.ToDecimal(r.GetValue(7)),
                            LateThreshold = Convert.ToInt16(r.GetValue(8)),
                            EarlyThreshold = Convert.ToInt16(r.GetValue(9)),
                            IsOvernight = Convert.ToBoolean(r.GetValue(10)),
                            ColorCode = r.IsDBNull(11) ? null : r.GetString(11),
                            IsActive = Convert.ToBoolean(r.GetValue(12))
                        });
                }
            }
            return list;
        }

        public async Task<WorkShiftDto> GetWorkShiftByIdAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT id, code, name, shift_type, start_time, end_time, break_minutes,
       standard_hours, late_threshold, early_threshold, is_overnight, color_code, is_active
FROM work_shifts WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (!await r.ReadAsync()) return null;
                        return new WorkShiftDto
                        {
                            Id = r.GetInt32(0), Code = r.GetString(1), Name = r.GetString(2),
                            ShiftType = r.GetString(3), StartTime = TimeSpan.Parse(r.GetString(4)), EndTime = TimeSpan.Parse(r.GetString(5)),
                            BreakMinutes = Convert.ToInt16(r.GetValue(6)), StandardHours = Convert.ToDecimal(r.GetValue(7)),
                            LateThreshold = Convert.ToInt16(r.GetValue(8)), EarlyThreshold = Convert.ToInt16(r.GetValue(9)),
                            IsOvernight = Convert.ToBoolean(r.GetValue(10)),
                            ColorCode = r.IsDBNull(11) ? null : r.GetString(11), IsActive = Convert.ToBoolean(r.GetValue(12))
                        };
                    }
                }
            }
        }

        // =============================================
        // EMPLOYEES
        // =============================================
        public async Task<List<EmployeeDto>> GetEmployeesAsync(bool activeOnly = true)
        {
            var list = new List<EmployeeDto>();
            var whereClause = activeOnly ? "WHERE e.is_active = 1" : "";
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand($@"
SELECT e.id, e.code, e.full_name, e.gender, e.date_of_birth, e.phone, e.email,
       e.identity_card, e.department_id, e.position_id, e.default_shift_id,
       e.hire_date, e.termination_date, e.employment_type, e.avatar_path,
       e.is_face_registered, e.face_registered_at, e.annual_leave_days, e.used_leave_days, e.is_active,
       e.manager_id,
       d.name AS dept_name, p.name AS pos_name, ws.name AS shift_name
FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
LEFT JOIN positions p ON e.position_id = p.id
LEFT JOIN work_shifts ws ON e.default_shift_id = ws.id
{whereClause}
ORDER BY e.code", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(ReadEmployee(r));
                }
            }
            return list;
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT e.id, e.code, e.full_name, e.gender, e.date_of_birth, e.phone, e.email,
       e.identity_card, e.department_id, e.position_id, e.default_shift_id,
       e.hire_date, e.termination_date, e.employment_type, e.avatar_path,
       e.is_face_registered, e.face_registered_at, e.annual_leave_days, e.used_leave_days, e.is_active,
       e.manager_id,
       d.name AS dept_name, p.name AS pos_name, ws.name AS shift_name
FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
LEFT JOIN positions p ON e.position_id = p.id
LEFT JOIN work_shifts ws ON e.default_shift_id = ws.id
WHERE e.id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (!await r.ReadAsync()) return null;
                        return ReadEmployee(r);
                    }
                }
            }
        }

        public async Task<int> CreateEmployeeAsync(EmployeeDto emp)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO employees (code, full_name, gender, date_of_birth, phone, email, identity_card,
                       department_id, position_id, default_shift_id, manager_id, hire_date, employment_type, avatar_path, annual_leave_days)
VALUES (@code, @full_name, @gender, @dob, @phone, @email, @idcard,
        @dept_id, @pos_id, @shift_id, @mgr_id, @hire_date, @emp_type, @avatar, @annual_leave)
; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("code", emp.Code);
                    cmd.Parameters.AddWithValue("full_name", emp.FullName);
                    cmd.Parameters.AddWithValue("gender", (object)emp.Gender ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("dob", (object)emp.DateOfBirth ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("phone", (object)emp.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("email", (object)emp.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("idcard", (object)emp.IdentityCard ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("dept_id", (object)emp.DepartmentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("pos_id", (object)emp.PositionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("shift_id", (object)emp.DefaultShiftId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("mgr_id", (object)emp.ManagerId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("hire_date", emp.HireDate);
                    cmd.Parameters.AddWithValue("emp_type", emp.EmploymentType ?? "FullTime");
                    cmd.Parameters.AddWithValue("avatar", (object)emp.AvatarPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("annual_leave", emp.AnnualLeaveDays);
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task UpdateEmployeeAsync(EmployeeDto emp)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE employees SET code=@code, full_name=@full_name, gender=@gender, date_of_birth=@dob,
    phone=@phone, email=@email, identity_card=@idcard,
    department_id=@dept_id, position_id=@pos_id, default_shift_id=@shift_id, manager_id=@mgr_id,
    hire_date=@hire_date, termination_date=@term_date, employment_type=@emp_type,
    avatar_path=@avatar, annual_leave_days=@annual_leave, is_active=@is_active
WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", emp.Id);
                    cmd.Parameters.AddWithValue("code", emp.Code);
                    cmd.Parameters.AddWithValue("full_name", emp.FullName);
                    cmd.Parameters.AddWithValue("gender", (object)emp.Gender ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("dob", (object)emp.DateOfBirth ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("phone", (object)emp.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("email", (object)emp.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("idcard", (object)emp.IdentityCard ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("dept_id", (object)emp.DepartmentId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("pos_id", (object)emp.PositionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("shift_id", (object)emp.DefaultShiftId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("mgr_id", (object)emp.ManagerId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("hire_date", emp.HireDate);
                    cmd.Parameters.AddWithValue("term_date", (object)emp.TerminationDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("emp_type", emp.EmploymentType ?? "FullTime");
                    cmd.Parameters.AddWithValue("avatar", (object)emp.AvatarPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("annual_leave", emp.AnnualLeaveDays);
                    cmd.Parameters.AddWithValue("is_active", emp.IsActive);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE employees SET is_active = 0, termination_date = CURRENT_DATE WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static EmployeeDto ReadEmployee(System.Data.Common.DbDataReader r)
        {
            return new EmployeeDto
            {
                Id = r.GetInt32(0),
                Code = r.GetString(1),
                FullName = r.GetString(2),
                Gender = r.IsDBNull(3) ? null : r.GetString(3),
                DateOfBirth = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4),
                Phone = r.IsDBNull(5) ? null : r.GetString(5),
                Email = r.IsDBNull(6) ? null : r.GetString(6),
                IdentityCard = r.IsDBNull(7) ? null : r.GetString(7),
                DepartmentId = r.IsDBNull(8) ? (int?)null : r.GetInt32(8),
                PositionId = r.IsDBNull(9) ? (int?)null : r.GetInt32(9),
                DefaultShiftId = r.IsDBNull(10) ? (int?)null : r.GetInt32(10),
                HireDate = r.GetDateTime(11),
                TerminationDate = r.IsDBNull(12) ? (DateTime?)null : r.GetDateTime(12),
                EmploymentType = r.GetString(13),
                AvatarPath = r.IsDBNull(14) ? null : r.GetString(14),
                IsFaceRegistered = Convert.ToBoolean(r.GetValue(15)),
                FaceRegisteredAt = r.IsDBNull(16) ? (DateTime?)null : r.GetDateTime(16),
                AnnualLeaveDays = Convert.ToDecimal(r.GetValue(17)),
                UsedLeaveDays = Convert.ToDecimal(r.GetValue(18)),
                IsActive = Convert.ToBoolean(r.GetValue(19)),
                ManagerId = r.IsDBNull(20) ? (int?)null : r.GetInt32(20),
                DepartmentName = r.IsDBNull(21) ? null : r.GetString(21),
                PositionName = r.IsDBNull(22) ? null : r.GetString(22),
                ShiftName = r.IsDBNull(23) ? null : r.GetString(23)
            };
        }

        // =============================================
        // USERS (Login)
        // =============================================
        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT u.id, u.username, u.password_hash, u.employee_id, u.role, u.is_active,
       u.last_login, u.failed_login_count, u.locked_until, u.must_change_password,
       e.full_name, e.code
FROM users u
LEFT JOIN employees e ON u.employee_id = e.id
WHERE lower(u.username) = lower(@username) LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("username", username.Trim());
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (!await r.ReadAsync()) return null;
                        return new UserDto
                        {
                            Id = r.GetInt32(0), Username = r.GetString(1), PasswordHash = r.GetString(2),
                            EmployeeId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3),
                            Role = r.GetString(4), IsActive = Convert.ToBoolean(r.GetValue(5)),
                            LastLogin = r.IsDBNull(6) ? (DateTime?)null : r.GetDateTime(6),
                            FailedLoginCount = Convert.ToInt16(r.GetValue(7)),
                            LockedUntil = r.IsDBNull(8) ? (DateTime?)null : r.GetDateTime(8),
                            MustChangePassword = Convert.ToBoolean(r.GetValue(9)),
                            EmployeeName = r.IsDBNull(10) ? null : r.GetString(10),
                            EmployeeCode = r.IsDBNull(11) ? null : r.GetString(11)
                        };
                    }
                }
            }
        }

        public async Task UpdateUserLastLoginAsync(int userId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE users SET last_login = CURRENT_TIMESTAMP, failed_login_count = 0 WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task IncrementFailedLoginAsync(int userId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE users SET failed_login_count = failed_login_count + 1,
    locked_until = CASE WHEN failed_login_count + 1 >= 5
        THEN datetime(CURRENT_TIMESTAMP, '+30 minutes') ELSE locked_until END
WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // FACE DATA
        // =============================================
        public async Task<List<FaceDataDto>> GetAllActiveFaceDataAsync()
        {
            var list = new List<FaceDataDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT fd.id, fd.employee_id, fd.encoding, fd.image_path, fd.thumbnail_path,
       fd.image_index, fd.angle, fd.quality_score, fd.is_active, fd.is_verified, fd.created_at,
       e.full_name, e.code
FROM face_data fd
JOIN employees e ON fd.employee_id = e.id
WHERE fd.is_active = 1 AND e.is_active = TRUE
ORDER BY fd.employee_id, fd.image_index", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(ReadFaceData(r));
                }
            }
            return list;
        }

        public async Task<List<FaceDataDto>> GetFaceDataByEmployeeAsync(int employeeId)
        {
            var list = new List<FaceDataDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT fd.id, fd.employee_id, fd.encoding, fd.image_path, fd.thumbnail_path,
       fd.image_index, fd.angle, fd.quality_score, fd.is_active, fd.is_verified, fd.created_at,
       e.full_name, e.code
FROM face_data fd
JOIN employees e ON fd.employee_id = e.id
WHERE fd.employee_id = @emp_id ORDER BY fd.image_index", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(ReadFaceData(r));
                    }
                }
            }
            return list;
        }

        public async Task<int> InsertFaceDataAsync(int employeeId, string encoding, string imagePath, int imageIndex, string angle, float qualityScore, int? registeredBy)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO face_data (employee_id, encoding, image_path, image_index, angle, quality_score, registered_by, is_active, is_verified)
VALUES (@emp_id, @encoding, @image_path, @image_index, @angle, @quality_score, @registered_by, 1, 0)
ON CONFLICT (employee_id, image_index) DO UPDATE SET
    encoding = EXCLUDED.encoding, image_path = EXCLUDED.image_path,
    angle = EXCLUDED.angle, quality_score = EXCLUDED.quality_score,
    is_active = 1, registered_by = EXCLUDED.registered_by
; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    cmd.Parameters.AddWithValue("encoding", encoding);
                    cmd.Parameters.AddWithValue("image_path", imagePath);
                    cmd.Parameters.AddWithValue("image_index", (short)imageIndex);
                    cmd.Parameters.AddWithValue("angle", (object)angle ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("quality_score", qualityScore);
                    cmd.Parameters.AddWithValue("registered_by", (object)registeredBy ?? DBNull.Value);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task DeleteFaceDataAsync(int faceDataId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE face_data SET is_active = 0 WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", faceDataId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static FaceDataDto ReadFaceData(System.Data.Common.DbDataReader r)
        {
            return new FaceDataDto
            {
                Id = r.GetInt32(0), EmployeeId = r.GetInt32(1),
                Encoding = r.GetString(2), ImagePath = r.GetString(3),
                ThumbnailPath = r.IsDBNull(4) ? null : r.GetString(4),
                ImageIndex = Convert.ToInt16(r.GetValue(5)),
                Angle = r.IsDBNull(6) ? null : r.GetString(6),
                QualityScore = r.GetFloat(7), IsActive = Convert.ToBoolean(r.GetValue(8)),
                IsVerified = Convert.ToBoolean(r.GetValue(9)), CreatedAt = r.GetDateTime(10),
                EmployeeName = r.IsDBNull(11) ? null : r.GetString(11),
                EmployeeCode = r.IsDBNull(12) ? null : r.GetString(12)
            };
        }



        // =============================================
        // HOLIDAYS
        // =============================================
        public async Task<bool> IsHolidayAsync(DateTime date)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("SELECT 1 FROM holidays WHERE holiday_date = @date LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("date", date.Date);
                    return await cmd.ExecuteScalarAsync() != null;
                }
            }
        }

        // =============================================
        // ATTENDANCE RECORDS
        // =============================================
        public async Task<AttendanceRecordDto> GetTodayAttendanceAsync(int employeeId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT id, employee_id, attendance_date, shift_id, check_in, check_out,
       check_in_image_path, check_out_image_path, check_in_method, check_out_method,
       check_in_confidence, check_out_confidence,
       status, late_minutes, early_minutes, working_minutes, is_manual_edit, note
FROM attendance_records
WHERE employee_id = @emp_id AND attendance_date = CURRENT_DATE LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        if (!await r.ReadAsync()) return null;
                        return ReadAttendance(r);
                    }
                }
            }
        }

        public async Task<long> CheckInAsync(int employeeId, int? shiftId, DateTime checkInTime,
            string imagePath, float? confidence, string method = "Face")
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO attendance_records
    (employee_id, attendance_date, shift_id, check_in, check_in_image_path,
     check_in_method, check_in_confidence, status)
VALUES (@emp_id, CURRENT_DATE, @shift_id, @check_in, @image_path,
        @method, @confidence, 'NotYet')
ON CONFLICT (employee_id, attendance_date) DO UPDATE SET
    check_in = EXCLUDED.check_in, check_in_image_path = EXCLUDED.check_in_image_path,
    check_in_method = EXCLUDED.check_in_method, check_in_confidence = EXCLUDED.check_in_confidence
; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    cmd.Parameters.AddWithValue("shift_id", (object)shiftId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("check_in", checkInTime);
                    cmd.Parameters.AddWithValue("image_path", (object)imagePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("method", method);
                    cmd.Parameters.AddWithValue("confidence", (object)confidence ?? DBNull.Value);
                    return Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task CheckOutAsync(int employeeId, DateTime checkOutTime, string imagePath,
            float? confidence, string status, int lateMinutes, int earlyMinutes, int workingMinutes, string method = "Face")
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE attendance_records SET
    check_out = @check_out, check_out_image_path = @image_path,
    check_out_method = @method, check_out_confidence = @confidence,
    status = @status, late_minutes = @late_min, early_minutes = @early_min,
    working_minutes = @work_min
WHERE employee_id = @emp_id AND attendance_date = CURRENT_DATE", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    cmd.Parameters.AddWithValue("check_out", checkOutTime);
                    cmd.Parameters.AddWithValue("image_path", (object)imagePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("method", method);
                    cmd.Parameters.AddWithValue("confidence", (object)confidence ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("status", status);
                    cmd.Parameters.AddWithValue("late_min", (short)lateMinutes);
                    cmd.Parameters.AddWithValue("early_min", (short)earlyMinutes);
                    cmd.Parameters.AddWithValue("work_min", workingMinutes);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Cập nhật status cho bản ghi check-in (khi chưa có check-out)
        /// </summary>
        public async Task UpdateCheckInStatusAsync(int employeeId, string status, int lateMinutes)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE attendance_records SET status = @status, late_minutes = @late_min
WHERE employee_id = @emp_id AND attendance_date = CURRENT_DATE", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    cmd.Parameters.AddWithValue("status", status);
                    cmd.Parameters.AddWithValue("late_min", (short)lateMinutes);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static AttendanceRecordDto ReadAttendance(System.Data.Common.DbDataReader r)
        {
            return new AttendanceRecordDto
            {
                Id = r.GetInt64(0), EmployeeId = r.GetInt32(1), AttendanceDate = r.GetDateTime(2),
                ShiftId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3),
                CheckIn = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4),
                CheckOut = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5),
                CheckInImagePath = r.IsDBNull(6) ? null : r.GetString(6),
                CheckOutImagePath = r.IsDBNull(7) ? null : r.GetString(7),
                CheckInMethod = r.IsDBNull(8) ? null : r.GetString(8),
                CheckOutMethod = r.IsDBNull(9) ? null : r.GetString(9),
                CheckInConfidence = r.IsDBNull(10) ? (float?)null : r.GetFloat(10),
                CheckOutConfidence = r.IsDBNull(11) ? (float?)null : r.GetFloat(11),
                Status = r.GetString(12), LateMinutes = Convert.ToInt16(r.GetValue(13)), EarlyMinutes = Convert.ToInt16(r.GetValue(14)),
                WorkingMinutes = r.GetInt32(15), IsManualEdit = Convert.ToBoolean(r.GetValue(16)),
                Note = r.IsDBNull(17) ? null : r.GetString(17)
            };
        }

        // =============================================
        // VIEW: v_today_attendance
        // =============================================
        public async Task<List<TodayAttendanceDto>> GetTodayAttendanceViewAsync()
        {
            var list = new List<TodayAttendanceDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("SELECT * FROM v_today_attendance ORDER BY full_name", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new TodayAttendanceDto
                        {
                            EmployeeId = r.GetInt32(r.GetOrdinal("employee_id")),
                            EmployeeCode = r.GetString(r.GetOrdinal("employee_code")),
                            FullName = r.GetString(r.GetOrdinal("full_name")),
                            DepartmentName = r.IsDBNull(r.GetOrdinal("department_name")) ? null : r.GetString(r.GetOrdinal("department_name")),
                            PositionName = r.IsDBNull(r.GetOrdinal("position_name")) ? null : r.GetString(r.GetOrdinal("position_name")),
                            ShiftName = r.IsDBNull(r.GetOrdinal("shift_name")) ? null : r.GetString(r.GetOrdinal("shift_name")),
                            ShiftStart = r.IsDBNull(r.GetOrdinal("start_time")) ? (TimeSpan?)null : TimeSpan.Parse(r.GetString(r.GetOrdinal("start_time"))),
                            ShiftEnd = r.IsDBNull(r.GetOrdinal("end_time")) ? (TimeSpan?)null : TimeSpan.Parse(r.GetString(r.GetOrdinal("end_time"))),
                            CheckIn = r.IsDBNull(r.GetOrdinal("check_in")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("check_in")),
                            CheckOut = r.IsDBNull(r.GetOrdinal("check_out")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("check_out")),
                            Status = r.IsDBNull(r.GetOrdinal("status")) ? null : r.GetString(r.GetOrdinal("status")),
                            LateMinutes = r.IsDBNull(r.GetOrdinal("late_minutes")) ? (int?)null : r.GetInt16(r.GetOrdinal("late_minutes")),
                            WorkingHours = r.IsDBNull(r.GetOrdinal("working_hours")) ? (decimal?)null : Convert.ToDecimal(r.GetValue(r.GetOrdinal("working_hours"))),
                            IsFaceRegistered = Convert.ToBoolean(r.GetValue(r.GetOrdinal("is_face_registered")))
                        });
                    }
                }
            }
            return list;
        }

        // =============================================
        // VIEW: v_monthly_summary
        // =============================================
        public async Task<List<MonthlySummaryDto>> GetMonthlySummaryAsync(DateTime month)
        {
            var list = new List<MonthlySummaryDto>();
            var firstOfMonth = new DateTime(month.Year, month.Month, 1);
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT * FROM v_monthly_summary WHERE month = @month ORDER BY full_name", conn))
                {
                    cmd.Parameters.AddWithValue("month", firstOfMonth.ToString("yyyy-MM-dd"));
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new MonthlySummaryDto
                            {
                                EmployeeId = r.GetInt32(r.GetOrdinal("employee_id")),
                                EmployeeCode = r.GetString(r.GetOrdinal("code")),
                                FullName = r.GetString(r.GetOrdinal("full_name")),
                                DepartmentName = r.IsDBNull(r.GetOrdinal("department_name")) ? null : r.GetString(r.GetOrdinal("department_name")),
                                Month = r.GetDateTime(r.GetOrdinal("month")),
                                TotalRecords = r.GetInt32(r.GetOrdinal("total_records")),
                                PresentDays = r.GetInt32(r.GetOrdinal("present_days")),
                                LateDays = r.GetInt32(r.GetOrdinal("late_days")),
                                EarlyLeaveDays = r.GetInt32(r.GetOrdinal("early_leave_days")),
                                LateAndEarlyDays = r.GetInt32(r.GetOrdinal("late_and_early_days")),
                                AbsentDays = r.GetInt32(r.GetOrdinal("absent_days")),
                                LeaveDays = r.GetInt32(r.GetOrdinal("leave_days")),
                                HolidayDays = r.GetInt32(r.GetOrdinal("holiday_days")),
                                DayOffDays = r.GetInt32(r.GetOrdinal("day_off_days")),
                                ActualWorkDays = r.GetInt32(r.GetOrdinal("actual_work_days")),
                                TotalLateMinutes = r.GetInt64(r.GetOrdinal("total_late_minutes")),
                                TotalWorkingHours = Convert.ToDecimal(r.GetValue(r.GetOrdinal("total_working_hours"))),
                                ManualEditCount = r.GetInt32(r.GetOrdinal("manual_edit_count"))
                            });
                        }
                    }
                }
            }
            return list;
        }

        // =============================================
        // DASHBOARD STATS
        // =============================================
        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var stats = new DashboardStats();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                // Total active employees
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM employees WHERE is_active = 1", conn))
                    stats.TotalEmployees = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // Face registered
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM employees WHERE is_active=1 AND is_face_registered=TRUE", conn))
                    stats.FaceRegistered = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                stats.FaceNotRegistered = stats.TotalEmployees - stats.FaceRegistered;

                // Today attendance stats
                using (var cmd = new SQLiteCommand(@"
SELECT
    COALESCE(SUM(CASE WHEN status='Present' THEN 1 ELSE 0 END), 0),
    COALESCE(SUM(CASE WHEN status='Late' OR status='LateAndEarly' OR status='EarlyLeave' THEN 1 ELSE 0 END), 0),
    COALESCE(SUM(CASE WHEN status='Absent' THEN 1 ELSE 0 END), 0),
    COALESCE(SUM(CASE WHEN status='Leave' THEN 1 ELSE 0 END), 0),
    COALESCE(SUM(CASE WHEN status='NotYet' THEN 1 ELSE 0 END), 0)
FROM attendance_records WHERE attendance_date = CURRENT_DATE", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    if (await r.ReadAsync())
                    {
                        stats.PresentCount = r.GetInt32(0);
                        stats.LateCount = r.GetInt32(1);
                        stats.AbsentCount = r.GetInt32(2);
                        stats.LeaveCount = r.GetInt32(3);
                        stats.NotYetCount = r.GetInt32(4);
                    }
                }
            }
            return stats;
        }

        // =============================================
        // LEAVE REQUESTS
        // =============================================
        public async Task<List<LeaveRequestDto>> GetLeaveRequestsAsync(int? employeeId = null, string status = null)
        {
            var list = new List<LeaveRequestDto>();
            var conditions = new List<string>();
            if (employeeId.HasValue) conditions.Add("lr.employee_id = @emp_id");
            if (!string.IsNullOrWhiteSpace(status)) conditions.Add("lr.status = @status");
            var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand($@"
SELECT lr.id, lr.employee_id, lr.leave_type, lr.start_date, lr.end_date, lr.total_days,
       lr.is_half_day, lr.half_day_period, lr.reason, lr.status,
       lr.approved_by, lr.approved_at, lr.reject_reason,
       e.full_name, e.code, approver.full_name
FROM leave_requests lr
JOIN employees e ON lr.employee_id = e.id
LEFT JOIN employees approver ON lr.approved_by = approver.id
{where}
ORDER BY lr.created_at DESC", conn))
                {
                    if (employeeId.HasValue) cmd.Parameters.AddWithValue("emp_id", employeeId.Value);
                    if (!string.IsNullOrWhiteSpace(status)) cmd.Parameters.AddWithValue("status", status);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                        {
                            list.Add(new LeaveRequestDto
                            {
                                Id = r.GetInt32(0), EmployeeId = r.GetInt32(1), LeaveType = r.GetString(2),
                                StartDate = r.GetDateTime(3), EndDate = r.GetDateTime(4),
                                TotalDays = Convert.ToDecimal(r.GetValue(5)), IsHalfDay = Convert.ToBoolean(r.GetValue(6)),
                                HalfDayPeriod = r.IsDBNull(7) ? null : r.GetString(7),
                                Reason = r.GetString(8), Status = r.GetString(9),
                                ApprovedBy = r.IsDBNull(10) ? (int?)null : r.GetInt32(10),
                                ApprovedAt = r.IsDBNull(11) ? (DateTime?)null : r.GetDateTime(11),
                                RejectReason = r.IsDBNull(12) ? null : r.GetString(12),
                                EmployeeName = r.GetString(13), EmployeeCode = r.GetString(14),
                                ApprovedByName = r.IsDBNull(15) ? null : r.GetString(15)
                            });
                        }
                    }
                }
            }
            return list;
        }

        // =============================================
        // ATTENDANCE LOGS (audit trail)
        // =============================================
        public async Task InsertAttendanceLogAsync(long? attendanceId, int? employeeId, int? deviceId,
            string logType, string method, int? matchedFaceId, float? confidence, float? faceDistance,
            string imagePath, string result, string failReason = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO attendance_logs
    (attendance_id, employee_id, device_id, log_type, method,
     matched_face_id, confidence, face_distance, image_path, result, fail_reason)
VALUES (@att_id, @emp_id, @dev_id, @log_type, @method,
        @face_id, @conf, @dist, @img, @result, @fail)", conn))
                {
                    cmd.Parameters.AddWithValue("att_id", (object)attendanceId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("emp_id", (object)employeeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("dev_id", (object)deviceId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("log_type", logType);
                    cmd.Parameters.AddWithValue("method", method);
                    cmd.Parameters.AddWithValue("face_id", (object)matchedFaceId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("conf", (object)confidence ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("dist", (object)faceDistance ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("img", (object)imagePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("result", result);
                    cmd.Parameters.AddWithValue("fail", (object)failReason ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // DEPARTMENTS CRUD
        // =============================================
        public async Task<int> CreateDepartmentAsync(DepartmentDto dept)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO departments (code, name, description, is_active) VALUES (@code, @name, @desc, 1) ; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("code", dept.Code);
                    cmd.Parameters.AddWithValue("name", dept.Name);
                    cmd.Parameters.AddWithValue("desc", (object)dept.Description ?? DBNull.Value);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateDepartmentAsync(DepartmentDto dept)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE departments SET code=@code, name=@name, description=@desc, is_active=@active WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", dept.Id);
                    cmd.Parameters.AddWithValue("code", dept.Code);
                    cmd.Parameters.AddWithValue("name", dept.Name);
                    cmd.Parameters.AddWithValue("desc", (object)dept.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("active", dept.IsActive);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE departments SET is_active = 0 WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // POSITIONS CRUD
        // =============================================
        public async Task<int> CreatePositionAsync(PositionDto pos)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO positions (code, name, level, is_active) VALUES (@code, @name, @level, 1) ; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("code", pos.Code);
                    cmd.Parameters.AddWithValue("name", pos.Name);
                    cmd.Parameters.AddWithValue("level", (short)pos.Level);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdatePositionAsync(PositionDto pos)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE positions SET code=@code, name=@name, level=@level, is_active=@active WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", pos.Id);
                    cmd.Parameters.AddWithValue("code", pos.Code);
                    cmd.Parameters.AddWithValue("name", pos.Name);
                    cmd.Parameters.AddWithValue("level", (short)pos.Level);
                    cmd.Parameters.AddWithValue("active", pos.IsActive);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeletePositionAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE positions SET is_active = 0 WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // WORK SHIFTS CRUD
        // =============================================
        public async Task<int> CreateWorkShiftAsync(WorkShiftDto ws)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO work_shifts (code, name, shift_type, start_time, end_time, break_minutes, standard_hours,
    late_threshold, early_threshold, is_overnight, color_code, is_active)
VALUES (@code, @name, @type, @start, @end, @break, @hours, @late, @early, @overnight, @color, 1)
; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("code", ws.Code);
                    cmd.Parameters.AddWithValue("name", ws.Name);
                    cmd.Parameters.AddWithValue("type", ws.ShiftType ?? "Fixed");
                    cmd.Parameters.AddWithValue("start", ws.StartTime);
                    cmd.Parameters.AddWithValue("end", ws.EndTime);
                    cmd.Parameters.AddWithValue("break", (short)ws.BreakMinutes);
                    cmd.Parameters.AddWithValue("hours", ws.StandardHours);
                    cmd.Parameters.AddWithValue("late", (short)ws.LateThreshold);
                    cmd.Parameters.AddWithValue("early", (short)ws.EarlyThreshold);
                    cmd.Parameters.AddWithValue("overnight", ws.IsOvernight);
                    cmd.Parameters.AddWithValue("color", (object)ws.ColorCode ?? DBNull.Value);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateWorkShiftAsync(WorkShiftDto ws)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE work_shifts SET code=@code, name=@name, shift_type=@type, start_time=@start, end_time=@end,
    break_minutes=@break, standard_hours=@hours, late_threshold=@late, early_threshold=@early,
    is_overnight=@overnight, color_code=@color, is_active=@active
WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", ws.Id);
                    cmd.Parameters.AddWithValue("code", ws.Code);
                    cmd.Parameters.AddWithValue("name", ws.Name);
                    cmd.Parameters.AddWithValue("type", ws.ShiftType ?? "Fixed");
                    cmd.Parameters.AddWithValue("start", ws.StartTime);
                    cmd.Parameters.AddWithValue("end", ws.EndTime);
                    cmd.Parameters.AddWithValue("break", (short)ws.BreakMinutes);
                    cmd.Parameters.AddWithValue("hours", ws.StandardHours);
                    cmd.Parameters.AddWithValue("late", (short)ws.LateThreshold);
                    cmd.Parameters.AddWithValue("early", (short)ws.EarlyThreshold);
                    cmd.Parameters.AddWithValue("overnight", ws.IsOvernight);
                    cmd.Parameters.AddWithValue("color", (object)ws.ColorCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("active", ws.IsActive);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteWorkShiftAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE work_shifts SET is_active = 0 WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // HOLIDAYS CRUD
        // =============================================
        public async Task<List<HolidayDto>> GetHolidaysAsync(int? year = null)
        {
            var list = new List<HolidayDto>();
            var where = year.HasValue ? "WHERE year = @year" : "";
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand($"SELECT id, holiday_date, name, holiday_type, is_recurring FROM holidays {where} ORDER BY holiday_date", conn))
                {
                    if (year.HasValue) cmd.Parameters.AddWithValue("year", (short)year.Value);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(new HolidayDto
                            {
                                Id = r.GetInt32(0), HolidayDate = r.GetDateTime(1),
                                Name = r.GetString(2), HolidayType = r.GetString(3),
                                IsRecurring = Convert.ToBoolean(r.GetValue(4))
                            });
                    }
                }
            }
            return list;
        }

        public async Task<int> CreateHolidayAsync(HolidayDto h)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO holidays (holiday_date, name, holiday_type, is_recurring, year)
VALUES (@date, @name, @type, @recurring, @year) ; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("date", h.HolidayDate.Date);
                    cmd.Parameters.AddWithValue("name", h.Name);
                    cmd.Parameters.AddWithValue("type", h.HolidayType ?? "National");
                    cmd.Parameters.AddWithValue("recurring", h.IsRecurring);
                    cmd.Parameters.AddWithValue("year", (short)h.HolidayDate.Year);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateHolidayAsync(HolidayDto h)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE holidays SET holiday_date=@date, name=@name, holiday_type=@type, is_recurring=@recurring WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", h.Id);
                    cmd.Parameters.AddWithValue("date", h.HolidayDate.Date);
                    cmd.Parameters.AddWithValue("name", h.Name);
                    cmd.Parameters.AddWithValue("type", h.HolidayType ?? "National");
                    cmd.Parameters.AddWithValue("recurring", h.IsRecurring);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteHolidayAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("DELETE FROM holidays WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // LEAVE REQUESTS CRUD
        // =============================================
        public async Task<int> CreateLeaveRequestAsync(LeaveRequestDto lr)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO leave_requests (employee_id, leave_type, start_date, end_date, total_days, is_half_day, half_day_period, reason, status)
VALUES (@emp_id, @type, @start, @end, @days, @half, @period, @reason, 'Pending') ; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", lr.EmployeeId);
                    cmd.Parameters.AddWithValue("type", lr.LeaveType);
                    cmd.Parameters.AddWithValue("start", lr.StartDate);
                    cmd.Parameters.AddWithValue("end", lr.EndDate);
                    cmd.Parameters.AddWithValue("days", lr.TotalDays);
                    cmd.Parameters.AddWithValue("half", lr.IsHalfDay);
                    cmd.Parameters.AddWithValue("period", (object)lr.HalfDayPeriod ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("reason", lr.Reason);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task ApproveLeaveRequestAsync(int requestId, int approvedBy)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE leave_requests SET status='Approved', approved_by=@by, approved_at=CURRENT_TIMESTAMP WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", requestId);
                    cmd.Parameters.AddWithValue("by", approvedBy);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RejectLeaveRequestAsync(int requestId, int rejectedBy, string reason)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE leave_requests SET status='Rejected', approved_by=@by, approved_at=CURRENT_TIMESTAMP, reject_reason=@reason WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", requestId);
                    cmd.Parameters.AddWithValue("by", rejectedBy);
                    cmd.Parameters.AddWithValue("reason", (object)reason ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // USERS CRUD
        // =============================================
        public async Task<List<UserDto>> GetUsersAsync()
        {
            var list = new List<UserDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT u.id, u.username, u.password_hash, u.employee_id, u.role, u.is_active,
       u.last_login, u.failed_login_count, u.locked_until, u.must_change_password,
       e.full_name, e.code
FROM users u LEFT JOIN employees e ON u.employee_id = e.id ORDER BY u.username", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new UserDto
                        {
                            Id = r.GetInt32(0), Username = r.GetString(1), PasswordHash = r.GetString(2),
                            EmployeeId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3),
                            Role = r.GetString(4), IsActive = Convert.ToBoolean(r.GetValue(5)),
                            LastLogin = r.IsDBNull(6) ? (DateTime?)null : r.GetDateTime(6),
                            FailedLoginCount = Convert.ToInt16(r.GetValue(7)),
                            LockedUntil = r.IsDBNull(8) ? (DateTime?)null : r.GetDateTime(8),
                            MustChangePassword = Convert.ToBoolean(r.GetValue(9)),
                            EmployeeName = r.IsDBNull(10) ? null : r.GetString(10),
                            EmployeeCode = r.IsDBNull(11) ? null : r.GetString(11)
                        });
                }
            }
            return list;
        }

        public async Task<int> CreateUserAsync(string username, string passwordHash, int? employeeId, string role)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO users (username, password_hash, employee_id, role, is_active) VALUES (@user, @hash, @emp_id, @role, 1) ; SELECT last_insert_rowid()", conn))
                {
                    cmd.Parameters.AddWithValue("user", username);
                    cmd.Parameters.AddWithValue("hash", passwordHash);
                    cmd.Parameters.AddWithValue("emp_id", (object)employeeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("role", role);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateUserAsync(int userId, string role, bool isActive, int? employeeId)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE users SET role=@role, is_active=@active, employee_id=@emp_id WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", userId);
                    cmd.Parameters.AddWithValue("role", role);
                    cmd.Parameters.AddWithValue("active", isActive);
                    cmd.Parameters.AddWithValue("emp_id", (object)employeeId ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ResetUserPasswordAsync(int userId, string newPasswordHash)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE users SET password_hash=@hash, failed_login_count=0, locked_until=NULL, must_change_password=1 WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", userId);
                    cmd.Parameters.AddWithValue("hash", newPasswordHash);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE users SET is_active = 0 WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // ATTENDANCE DEVICES CRUD
        // =============================================
        public async Task<List<AttendanceDeviceDto>> GetDevicesAsync()
        {
            var list = new List<AttendanceDeviceDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var legacyColumns = await ColumnExistsAsync(conn, "attendance_devices", "code");
                var sql = legacyColumns
                    ? "SELECT id, code, name, device_type, location, ip_address, is_active, last_heartbeat FROM attendance_devices ORDER BY code"
                    : "SELECT id, device_code, device_name, device_type, location_name, ip_address, is_active, last_heartbeat FROM attendance_devices ORDER BY device_code";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new AttendanceDeviceDto
                        {
                            Id = r.GetInt32(0), Code = r.GetString(1), Name = r.GetString(2),
                            DeviceType = r.GetString(3),
                            Location = r.IsDBNull(4) ? null : r.GetString(4),
                            IpAddress = r.IsDBNull(5) ? null : r.GetString(5),
                            IsActive = Convert.ToBoolean(r.GetValue(6)),
                            LastHeartbeat = r.IsDBNull(7) ? (DateTime?)null : r.GetDateTime(7)
                        });
                }
            }
            return list;
        }

        public async Task<int> CreateDeviceAsync(AttendanceDeviceDto d)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var legacyColumns = await ColumnExistsAsync(conn, "attendance_devices", "code");
                var sql = legacyColumns
                    ? @"
INSERT INTO attendance_devices (code, name, device_type, location, ip_address, is_active)
VALUES (@code, @name, @type, @loc, @ip, 1) ; SELECT last_insert_rowid()"
                    : @"
INSERT INTO attendance_devices (device_code, device_name, device_type, location_name, ip_address, is_active)
VALUES (@code, @name, @type, @loc, @ip, 1) ; SELECT last_insert_rowid()";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("code", d.Code);
                    cmd.Parameters.AddWithValue("name", d.Name);
                    cmd.Parameters.AddWithValue("type", d.DeviceType ?? "Camera");
                    cmd.Parameters.AddWithValue("loc", (object)d.Location ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("ip", (object)d.IpAddress ?? DBNull.Value);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateDeviceAsync(AttendanceDeviceDto d)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var legacyColumns = await ColumnExistsAsync(conn, "attendance_devices", "code");
                var sql = legacyColumns
                    ? "UPDATE attendance_devices SET code=@code, name=@name, device_type=@type, location=@loc, ip_address=@ip, is_active=@active WHERE id=@id"
                    : "UPDATE attendance_devices SET device_code=@code, device_name=@name, device_type=@type, location_name=@loc, ip_address=@ip, is_active=@active WHERE id=@id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", d.Id);
                    cmd.Parameters.AddWithValue("code", d.Code);
                    cmd.Parameters.AddWithValue("name", d.Name);
                    cmd.Parameters.AddWithValue("type", d.DeviceType ?? "Camera");
                    cmd.Parameters.AddWithValue("loc", (object)d.Location ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("ip", (object)d.IpAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("active", d.IsActive);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteDeviceAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE attendance_devices SET is_active = 0 WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // WORK CALENDARS CRUD
        // =============================================
        public async Task<List<WorkCalendarDto>> GetWorkCalendarsAsync()
        {
            var list = new List<WorkCalendarDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var hasYear = await ColumnExistsAsync(conn, "work_calendars", "year");
                var hasIsActive = await ColumnExistsAsync(conn, "work_calendars", "is_active");

                var sql = hasYear
                    ? @"
SELECT id, name, year, monday, tuesday, wednesday, thursday, friday, saturday, sunday, is_default,
       " + (hasIsActive ? "is_active" : "1 AS is_active") + @"
FROM work_calendars
" + (hasIsActive ? "WHERE is_active = 1" : string.Empty) + @"
ORDER BY year DESC, name"
                    : @"
SELECT id, name,
       CAST(strftime('%Y', COALESCE(effective_from, date('now'))) AS INTEGER) AS year,
       monday, tuesday, wednesday, thursday, friday, saturday, sunday, is_default,
       1 AS is_active
FROM work_calendars
ORDER BY effective_from DESC, name";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new WorkCalendarDto
                        {
                            Id = r.GetInt32(0), Name = r.GetString(1), Year = Convert.ToInt32(r.GetValue(2)),
                            Monday = Convert.ToBoolean(r.GetValue(3)), Tuesday = Convert.ToBoolean(r.GetValue(4)), Wednesday = Convert.ToBoolean(r.GetValue(5)),
                            Thursday = Convert.ToBoolean(r.GetValue(6)), Friday = Convert.ToBoolean(r.GetValue(7)), Saturday = Convert.ToBoolean(r.GetValue(8)),
                            Sunday = Convert.ToBoolean(r.GetValue(9)), IsDefault = Convert.ToBoolean(r.GetValue(10)), IsActive = Convert.ToBoolean(r.GetValue(11))
                        });
                }
            }
            return list;
        }

        public async Task<int> CreateWorkCalendarAsync(WorkCalendarDto wc)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var hasYear = await ColumnExistsAsync(conn, "work_calendars", "year");
                var hasIsActive = await ColumnExistsAsync(conn, "work_calendars", "is_active");
                var effectiveFrom = new DateTime(Math.Max(2000, wc.Year), 1, 1);

                var sql = hasYear
                    ? (hasIsActive
                        ? @"
INSERT INTO work_calendars (name, year, monday, tuesday, wednesday, thursday, friday, saturday, sunday, is_default, is_active)
VALUES (@name, @year, @mon, @tue, @wed, @thu, @fri, @sat, @sun, @default, 1) ; SELECT last_insert_rowid()"
                        : @"
INSERT INTO work_calendars (name, year, monday, tuesday, wednesday, thursday, friday, saturday, sunday, is_default)
VALUES (@name, @year, @mon, @tue, @wed, @thu, @fri, @sat, @sun, @default) ; SELECT last_insert_rowid()")
                    : @"
INSERT INTO work_calendars (name, monday, tuesday, wednesday, thursday, friday, saturday, sunday, is_default, effective_from)
VALUES (@name, @mon, @tue, @wed, @thu, @fri, @sat, @sun, @default, @effective_from) ; SELECT last_insert_rowid()";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("name", wc.Name);
                    if (hasYear)
                    {
                        cmd.Parameters.AddWithValue("year", (short)wc.Year);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("effective_from", effectiveFrom);
                    }
                    cmd.Parameters.AddWithValue("mon", wc.Monday);
                    cmd.Parameters.AddWithValue("tue", wc.Tuesday);
                    cmd.Parameters.AddWithValue("wed", wc.Wednesday);
                    cmd.Parameters.AddWithValue("thu", wc.Thursday);
                    cmd.Parameters.AddWithValue("fri", wc.Friday);
                    cmd.Parameters.AddWithValue("sat", wc.Saturday);
                    cmd.Parameters.AddWithValue("sun", wc.Sunday);
                    cmd.Parameters.AddWithValue("default", wc.IsDefault);
                    return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateWorkCalendarAsync(WorkCalendarDto wc)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var hasYear = await ColumnExistsAsync(conn, "work_calendars", "year");
                var hasIsActive = await ColumnExistsAsync(conn, "work_calendars", "is_active");
                var effectiveFrom = new DateTime(Math.Max(2000, wc.Year), 1, 1);

                var sql = hasYear
                    ? (hasIsActive
                        ? @"
UPDATE work_calendars SET name=@name, year=@year, monday=@mon, tuesday=@tue, wednesday=@wed,
    thursday=@thu, friday=@fri, saturday=@sat, sunday=@sun, is_default=@default, is_active=@active
WHERE id=@id"
                        : @"
UPDATE work_calendars SET name=@name, year=@year, monday=@mon, tuesday=@tue, wednesday=@wed,
    thursday=@thu, friday=@fri, saturday=@sat, sunday=@sun, is_default=@default
WHERE id=@id")
                    : @"
UPDATE work_calendars SET name=@name, monday=@mon, tuesday=@tue, wednesday=@wed,
    thursday=@thu, friday=@fri, saturday=@sat, sunday=@sun, is_default=@default,
    effective_from=@effective_from
WHERE id=@id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", wc.Id);
                    cmd.Parameters.AddWithValue("name", wc.Name);
                    if (hasYear)
                    {
                        cmd.Parameters.AddWithValue("year", (short)wc.Year);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("effective_from", effectiveFrom);
                    }
                    cmd.Parameters.AddWithValue("mon", wc.Monday);
                    cmd.Parameters.AddWithValue("tue", wc.Tuesday);
                    cmd.Parameters.AddWithValue("wed", wc.Wednesday);
                    cmd.Parameters.AddWithValue("thu", wc.Thursday);
                    cmd.Parameters.AddWithValue("fri", wc.Friday);
                    cmd.Parameters.AddWithValue("sat", wc.Saturday);
                    cmd.Parameters.AddWithValue("sun", wc.Sunday);
                    cmd.Parameters.AddWithValue("default", wc.IsDefault);
                    if (hasIsActive)
                    {
                        cmd.Parameters.AddWithValue("active", wc.IsActive);
                    }
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteWorkCalendarAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var hasIsActive = await ColumnExistsAsync(conn, "work_calendars", "is_active");
                var sql = hasIsActive
                    ? "UPDATE work_calendars SET is_active = 0 WHERE id = @id"
                    : "DELETE FROM work_calendars WHERE id = @id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // ATTENDANCE LOGS QUERY
        // =============================================
        public async Task<List<AttendanceLogDto>> GetAttendanceLogsAsync(DateTime date)
        {
            var list = new List<AttendanceLogDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var legacyDeviceColumns = await ColumnExistsAsync(conn, "attendance_devices", "name");
                var deviceNameColumn = legacyDeviceColumns ? "d.name" : "d.device_name";
                using (var cmd = new SQLiteCommand(@"
SELECT al.id, al.attendance_id, al.employee_id, al.device_id, al.log_time,
       al.log_type, al.method, al.matched_face_id, al.confidence, al.face_distance,
       al.image_path, al.result, al.fail_reason,
       e.full_name, e.code, " + deviceNameColumn + @" AS device_name
FROM attendance_logs al
LEFT JOIN employees e ON al.employee_id = e.id
LEFT JOIN attendance_devices d ON al.device_id = d.id
WHERE DATE(al.log_time) = @date
ORDER BY al.log_time DESC", conn))
                {
                    cmd.Parameters.AddWithValue("date", date.Date);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(new AttendanceLogDto
                            {
                                Id = r.GetInt64(0),
                                AttendanceId = r.IsDBNull(1) ? (long?)null : r.GetInt64(1),
                                EmployeeId = r.IsDBNull(2) ? (int?)null : r.GetInt32(2),
                                DeviceId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3),
                                LogTime = r.GetDateTime(4),
                                LogType = r.GetString(5), Method = r.GetString(6),
                                MatchedFaceId = r.IsDBNull(7) ? (int?)null : r.GetInt32(7),
                                Confidence = r.IsDBNull(8) ? (float?)null : r.GetFloat(8),
                                FaceDistance = r.IsDBNull(9) ? (float?)null : r.GetFloat(9),
                                ImagePath = r.IsDBNull(10) ? null : r.GetString(10),
                                Result = r.GetString(11),
                                FailReason = r.IsDBNull(12) ? null : r.GetString(12),
                                EmployeeName = r.IsDBNull(13) ? null : r.GetString(13),
                                EmployeeCode = r.IsDBNull(14) ? null : r.GetString(14),
                                DeviceName = r.IsDBNull(15) ? null : r.GetString(15)
                            });
                    }
                }
            }
            return list;
        }

        // =============================================
        // ATTENDANCE LOGS QUERY (date range overload)
        // =============================================
        public async Task<List<AttendanceLogDto>> GetAttendanceLogsAsync(DateTime from, DateTime to, int limit = 500)
        {
            var list = new List<AttendanceLogDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                var legacyDeviceColumns = await ColumnExistsAsync(conn, "attendance_devices", "name");
                var deviceNameColumn = legacyDeviceColumns ? "d.name" : "d.device_name";
                using (var cmd = new SQLiteCommand(@"
SELECT al.id, al.attendance_id, al.employee_id, al.device_id, al.log_time,
       al.log_type, al.method, al.matched_face_id, al.confidence, al.face_distance,
       al.image_path, al.result, al.fail_reason,
       e.full_name, e.code, " + deviceNameColumn + @" AS device_name
FROM attendance_logs al
LEFT JOIN employees e ON al.employee_id = e.id
LEFT JOIN attendance_devices d ON al.device_id = d.id
WHERE DATE(al.log_time) BETWEEN @from AND @to
ORDER BY al.log_time DESC
LIMIT @limit", conn))
                {
                    cmd.Parameters.AddWithValue("from", from.Date);
                    cmd.Parameters.AddWithValue("to", to.Date);
                    cmd.Parameters.AddWithValue("limit", limit);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(new AttendanceLogDto
                            {
                                Id = r.GetInt64(0),
                                AttendanceId = r.IsDBNull(1) ? (long?)null : r.GetInt64(1),
                                EmployeeId = r.IsDBNull(2) ? (int?)null : r.GetInt32(2),
                                DeviceId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3),
                                LogTime = r.GetDateTime(4),
                                LogType = r.GetString(5), Method = r.GetString(6),
                                MatchedFaceId = r.IsDBNull(7) ? (int?)null : r.GetInt32(7),
                                Confidence = r.IsDBNull(8) ? (float?)null : r.GetFloat(8),
                                FaceDistance = r.IsDBNull(9) ? (float?)null : r.GetFloat(9),
                                ImagePath = r.IsDBNull(10) ? null : r.GetString(10),
                                Result = r.GetString(11),
                                FailReason = r.IsDBNull(12) ? null : r.GetString(12),
                                EmployeeName = r.IsDBNull(13) ? null : r.GetString(13),
                                EmployeeCode = r.IsDBNull(14) ? null : r.GetString(14),
                                DeviceName = r.IsDBNull(15) ? null : r.GetString(15)
                            });
                    }
                }
            }
            return list;
        }

        // =============================================
        // CONNECTION TEST
        // =============================================
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new SQLiteCommand("SELECT 1", conn))
                    {
                        await cmd.ExecuteScalarAsync();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // =============================================
        // ATTENDANCE RECORDS - by employee & date range
        // =============================================
        public async Task<List<AttendanceRecordDto>> GetAttendanceByEmployeeAsync(int employeeId, DateTime from, DateTime to)
        {
            var list = new List<AttendanceRecordDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT ar.id, ar.employee_id, ar.attendance_date, ar.shift_id, ar.check_in, ar.check_out,
       ar.check_in_image_path, ar.check_out_image_path, ar.check_in_method, ar.check_out_method,
       ar.check_in_confidence, ar.check_out_confidence,
       ar.status, ar.late_minutes, ar.early_minutes, ar.working_minutes, ar.is_manual_edit, ar.note
FROM attendance_records ar
WHERE ar.employee_id = @emp_id
  AND ar.attendance_date BETWEEN @from AND @to
ORDER BY ar.attendance_date DESC", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    cmd.Parameters.AddWithValue("from", from.Date);
                    cmd.Parameters.AddWithValue("to", to.Date);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(ReadAttendance(r));
                    }
                }
            }
            return list;
        }

        // =============================================
        // SYSTEM SETTINGS
        // =============================================
        public async Task<List<SystemSettingDto>> GetSystemSettingsAsync()
        {
            var list = new List<SystemSettingDto>();
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(
                    "SELECT key, value, description, data_type FROM system_settings ORDER BY key", conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new SystemSettingDto
                        {
                            Key = r.GetString(0),
                            Value = r.IsDBNull(1) ? null : r.GetString(1),
                            Description = r.IsDBNull(2) ? null : r.GetString(2),
                            DataType = r.IsDBNull(3) ? null : r.GetString(3)
                        });
                }
            }
            return list;
        }

        public async Task<string> GetSystemSettingAsync(string key, string defaultValue = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(
                    "SELECT value FROM system_settings WHERE key = @key", conn))
                {
                    cmd.Parameters.AddWithValue("key", key);
                    var obj = await cmd.ExecuteScalarAsync();
                    if (obj == null || obj == DBNull.Value) return defaultValue;
                    return Convert.ToString(obj);
                }
            }
        }

        public async Task<double> GetSystemSettingDoubleAsync(string key, double defaultValue)
        {
            var raw = await GetSystemSettingAsync(key, null);
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;
            return double.TryParse(raw, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        public async Task<int> GetSystemSettingIntAsync(string key, int defaultValue)
        {
            var raw = await GetSystemSettingAsync(key, null);
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;
            return int.TryParse(raw, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        public async Task<DateTime?> GetLastSuccessLogTimeAsync(int employeeId, string logType)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
SELECT MAX(created_at)
FROM attendance_logs
WHERE employee_id = @emp AND log_type = @lt AND result = 'Success'", conn))
                {
                    cmd.Parameters.AddWithValue("emp", employeeId);
                    cmd.Parameters.AddWithValue("lt", logType);
                    var obj = await cmd.ExecuteScalarAsync();
                    if (obj == null || obj == DBNull.Value) return null;
                    if (DateTime.TryParse(Convert.ToString(obj), System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeLocal, out var dt))
                        return dt;
                    return null;
                }
            }
        }

        // =============================================
        // AUDIT LOGS (write)
        // =============================================
        public async Task InsertAuditLogAsync(int? userId, int? employeeId, string action,
            string tableName, string recordId, string description,
            string oldValues = null, string newValues = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO audit_logs
    (user_id, employee_id, action, table_name, record_id,
     old_values, new_values, description)
VALUES (@uid, @eid, @action, @table, @rid, @old, @new, @desc)", conn))
                {
                    cmd.Parameters.AddWithValue("uid",    (object)userId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("eid",    (object)employeeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("action", action);
                    cmd.Parameters.AddWithValue("table",  (object)tableName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("rid",    (object)recordId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("old",    (object)oldValues ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("new",    (object)newValues ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("desc",   (object)description ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // LEAVE REQUESTS — Update & Cancel
        // =============================================
        public async Task UpdateLeaveRequestAsync(LeaveRequestDto dto)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE leave_requests
SET leave_type   = @lt,
    start_date   = @sd,
    end_date     = @ed,
    total_days   = @td,
    reason       = @reason,
    updated_at   = CURRENT_TIMESTAMP
WHERE id = @id AND status = 'Pending'", conn))
                {
                    cmd.Parameters.AddWithValue("lt",     dto.LeaveType);
                    cmd.Parameters.AddWithValue("sd",     dto.StartDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("ed",     dto.EndDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("td",     dto.TotalDays);
                    cmd.Parameters.AddWithValue("reason", (object)dto.Reason ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("id",     dto.Id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task CancelLeaveRequestAsync(int id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE leave_requests
SET status = 'Cancelled', updated_at = CURRENT_TIMESTAMP
WHERE id = @id AND status = 'Pending'", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // ATTENDANCE RECORDS — Manual edit
        // =============================================
        public async Task UpdateAttendanceRecordManualAsync(long id,
            DateTime? checkIn, DateTime? checkOut, string status,
            int lateMin, int earlyMin, int workMin,
            string reason, int? editByUserId)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Bắt buộc nhập lý do khi sửa thủ công.");

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE attendance_records
SET check_in            = @ci,
    check_out           = @co,
    status              = @status,
    late_minutes        = @late,
    early_minutes       = @early,
    working_minutes     = @work,
    is_manual_edit      = 1,
    manual_edit_by      = @editby,
    manual_edit_at      = CURRENT_TIMESTAMP,
    manual_edit_reason  = @reason,
    updated_at          = CURRENT_TIMESTAMP
WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("ci",     (object)checkIn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("co",     (object)checkOut ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("status", status);
                    cmd.Parameters.AddWithValue("late",   lateMin);
                    cmd.Parameters.AddWithValue("early",  earlyMin);
                    cmd.Parameters.AddWithValue("work",   workMin);
                    cmd.Parameters.AddWithValue("editby", (object)editByUserId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("reason", reason);
                    cmd.Parameters.AddWithValue("id",     id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // FACE DATA — Verification
        // =============================================
        public async Task UpdateFaceDataVerificationAsync(int faceDataId, bool isVerified, int? verifiedBy)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE face_data
SET is_verified = @v,
    verified_by = @vby,
    verified_at = CASE WHEN @v = 1 THEN CURRENT_TIMESTAMP ELSE NULL END,
    updated_at  = CURRENT_TIMESTAMP
WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("v",   isVerified ? 1 : 0);
                    cmd.Parameters.AddWithValue("vby", (object)verifiedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("id",  faceDataId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpsertSystemSettingAsync(string key, string value)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO system_settings (key, value) VALUES (@key, @val)
ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value, updated_at = CURRENT_TIMESTAMP", conn))
                {
                    cmd.Parameters.AddWithValue("key", key);
                    cmd.Parameters.AddWithValue("val", (object)value ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // AUDIT LOGS (read-only)
        // =============================================
        public async Task<List<AuditLogDto>> GetAuditLogsAsync(int limit = 500, string tableName = null)
        {
            var list = new List<AuditLogDto>();
            var where = string.IsNullOrWhiteSpace(tableName) ? "" : "WHERE al.table_name = @table";
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand($@"
SELECT al.id, al.user_id, al.action, al.table_name, al.record_id,
       al.old_values, al.new_values, al.ip_address, al.created_at,
       u.username
FROM audit_logs al
LEFT JOIN users u ON al.user_id = u.id
{where}
ORDER BY al.created_at DESC LIMIT @limit", conn))
                {
                    cmd.Parameters.AddWithValue("limit", limit);
                    if (!string.IsNullOrWhiteSpace(tableName))
                        cmd.Parameters.AddWithValue("table", tableName);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(new AuditLogDto
                            {
                                Id = r.GetInt64(0),
                                UserId = r.IsDBNull(1) ? (int?)null : r.GetInt32(1),
                                Action = r.GetString(2),
                                TableName = r.GetString(3),
                                RecordId = r.IsDBNull(4) ? null : r.GetString(4),
                                OldValues = r.IsDBNull(5) ? null : r.GetString(5),
                                NewValues = r.IsDBNull(6) ? null : r.GetString(6),
                                IpAddress = r.IsDBNull(7) ? null : r.GetString(7),
                                CreatedAt = r.GetDateTime(8),
                                Username = r.IsDBNull(9) ? null : r.GetString(9)
                            });
                    }
                }
            }
            return list;
        }

        // =============================================
        // FACE REGISTRATION LOGS
        // =============================================
        public async Task<List<FaceRegistrationLogDto>> GetFaceRegistrationLogsAsync(int? employeeId = null)
        {
            var list = new List<FaceRegistrationLogDto>();
            var where = employeeId.HasValue ? "WHERE frl.employee_id = @emp_id" : "";
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand($@"
SELECT frl.id, frl.face_data_id, frl.employee_id, frl.action,
       frl.performed_by, frl.reason, frl.created_at,
       e.full_name AS emp_name, pe.full_name AS by_name
FROM face_registration_logs frl
JOIN employees e ON frl.employee_id = e.id
LEFT JOIN employees pe ON frl.performed_by = pe.id
{where}
ORDER BY frl.created_at DESC LIMIT 1000", conn))
                {
                    if (employeeId.HasValue)
                        cmd.Parameters.AddWithValue("emp_id", employeeId.Value);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(new FaceRegistrationLogDto
                            {
                                Id = r.GetInt64(0),
                                FaceDataId = r.IsDBNull(1) ? (int?)null : r.GetInt32(1),
                                EmployeeId = r.GetInt32(2),
                                Action = r.GetString(3),
                                PerformedBy = r.IsDBNull(4) ? (int?)null : r.GetInt32(4),
                                Reason = r.IsDBNull(5) ? null : r.GetString(5),
                                CreatedAt = r.GetDateTime(6),
                                EmployeeName = r.IsDBNull(7) ? null : r.GetString(7),
                                PerformedByName = r.IsDBNull(8) ? null : r.GetString(8)
                            });
                    }
                }
            }
            return list;
        }

        public async Task InsertFaceRegistrationLogAsync(int employeeId, string action, int? performedBy, string reason)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO face_registration_logs (employee_id, action, performed_by, reason)
VALUES (@emp_id, @action, @performed_by, @reason)", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    cmd.Parameters.AddWithValue("action", action);
                    cmd.Parameters.AddWithValue("performed_by", (object)performedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("reason", (object)reason ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateEmployeeFaceStatusAsync(int employeeId, bool isRegistered)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
UPDATE employees SET is_face_registered = @registered,
    face_registered_at = CASE WHEN @registered THEN CURRENT_TIMESTAMP ELSE NULL END
WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", employeeId);
                    cmd.Parameters.AddWithValue("registered", isRegistered);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateEmployeeAvatarAsync(int employeeId, string avatarPath)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("UPDATE employees SET avatar_path = @path WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", employeeId);
                    cmd.Parameters.AddWithValue("path", (object)avatarPath ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<string> GetNextEmployeeCodeAsync(string prefix = "NV")
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                // Sắp xếp theo chiều dài và sau đó theo mã để xử lý đúng thứ tự số (ví dụ NV9 < NV10)
                using (var cmd = new SQLiteCommand(@"
SELECT code FROM employees 
WHERE code LIKE @prefix || '%' 
ORDER BY length(code) DESC, code DESC 
LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("prefix", prefix);
                    var lastCode = await cmd.ExecuteScalarAsync() as string;
                    if (string.IsNullOrEmpty(lastCode)) return prefix + "001";

                    // Trích xuất phần số từ mã cuối cùng
                    var numericPart = lastCode.Substring(prefix.Length);
                    if (int.TryParse(numericPart, out int number))
                    {
                        return prefix + (number + 1).ToString("D3");
                    }
                    return prefix + "001";
                }
            }
        }

        // =============================================
        // EMPLOYEE SHIFT SCHEDULES
        // =============================================
        public async Task<List<EmployeeShiftScheduleDto>> GetEmployeeShiftSchedulesAsync(
            int? employeeId = null, DateTime? date = null)
        {
            var list = new List<EmployeeShiftScheduleDto>();
            var conditions = new System.Collections.Generic.List<string>();
            if (employeeId.HasValue) conditions.Add("ess.employee_id = @emp_id");
            if (date.HasValue)      conditions.Add("ess.schedule_date = @date");
            var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand($@"
SELECT ess.id, ess.employee_id, ess.schedule_date, ess.shift_id, ess.is_override, ess.note,
       e.full_name AS emp_name, ws.name AS shift_name
FROM employee_shift_schedules ess
JOIN employees e ON ess.employee_id = e.id
JOIN work_shifts ws ON ess.shift_id = ws.id
{where}
ORDER BY ess.schedule_date DESC, e.full_name
LIMIT 2000", conn))
                {
                    if (employeeId.HasValue) cmd.Parameters.AddWithValue("emp_id", employeeId.Value);
                    if (date.HasValue)      cmd.Parameters.AddWithValue("date", date.Value.Date);
                    using (var r = await cmd.ExecuteReaderAsync())
                    {
                        while (await r.ReadAsync())
                            list.Add(new EmployeeShiftScheduleDto
                            {
                                Id = r.GetInt64(0),
                                EmployeeId = r.GetInt32(1),
                                ScheduleDate = r.GetDateTime(2),
                                ShiftId = r.GetInt32(3),
                                IsOverride = Convert.ToBoolean(r.GetValue(4)),
                                Note = r.IsDBNull(5) ? null : r.GetString(5),
                                EmployeeName = r.IsDBNull(6) ? null : r.GetString(6),
                                ShiftName = r.IsDBNull(7) ? null : r.GetString(7)
                            });
                    }
                }
            }
            return list;
        }

        public async Task UpsertShiftScheduleAsync(int employeeId, DateTime date, int shiftId, bool isOverride, string note)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand(@"
INSERT INTO employee_shift_schedules (employee_id, schedule_date, shift_id, is_override, note)
VALUES (@emp_id, @date, @shift_id, @override, @note)
ON CONFLICT (employee_id, schedule_date)
DO UPDATE SET shift_id = EXCLUDED.shift_id, is_override = EXCLUDED.is_override, note = EXCLUDED.note", conn))
                {
                    cmd.Parameters.AddWithValue("emp_id", employeeId);
                    cmd.Parameters.AddWithValue("date", date.Date);
                    cmd.Parameters.AddWithValue("shift_id", shiftId);
                    cmd.Parameters.AddWithValue("override", isOverride);
                    cmd.Parameters.AddWithValue("note", (object)note ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteShiftScheduleAsync(long id)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("DELETE FROM employee_shift_schedules WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =============================================
        // PENDING LEAVE COUNT (for badge notification)
        // =============================================
        public async Task<int> GetPendingLeaveCountAsync()
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM leave_requests WHERE status = 'Pending'", conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
    }
}

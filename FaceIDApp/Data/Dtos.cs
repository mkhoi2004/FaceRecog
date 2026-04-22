using System;
using System.Collections.Generic;

namespace FaceIDApp.Data
{
    // =============================================
    // DTOs matching face_attendance_v3 schema
    // =============================================

    internal class DepartmentDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    internal class PositionDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
    }

    internal class WorkShiftDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ShiftType { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakMinutes { get; set; }
        public decimal StandardHours { get; set; }
        public int LateThreshold { get; set; }
        public int EarlyThreshold { get; set; }
        public bool IsOvernight { get; set; }
        public string ColorCode { get; set; }
        public bool IsActive { get; set; }
    }

    internal class EmployeeDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string IdentityCard { get; set; }
        public int? DepartmentId { get; set; }
        public int? PositionId { get; set; }
        public int? DefaultShiftId { get; set; }
        public int? ManagerId { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string EmploymentType { get; set; }
        public string AvatarPath { get; set; }
        public bool IsFaceRegistered { get; set; }
        public DateTime? FaceRegisteredAt { get; set; }
        public decimal AnnualLeaveDays { get; set; }
        public decimal UsedLeaveDays { get; set; }
        public bool IsActive { get; set; }

        // JOIN fields
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string ShiftName { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int? EmployeeId { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool MustChangePassword { get; set; }

        // JOIN fields
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
    }

    internal class FaceDataDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Encoding { get; set; }
        public string ImagePath { get; set; }
        public string ThumbnailPath { get; set; }
        public int ImageIndex { get; set; }
        public string Angle { get; set; }
        public float QualityScore { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        // JOIN fields
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
    }

    internal class AttendanceRecordDto
    {
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int? ShiftId { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string CheckInImagePath { get; set; }
        public string CheckOutImagePath { get; set; }
        public string CheckInMethod { get; set; }
        public string CheckOutMethod { get; set; }
        public float? CheckInConfidence { get; set; }
        public float? CheckOutConfidence { get; set; }
        public string Status { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public int WorkingMinutes { get; set; }
        public bool IsManualEdit { get; set; }
        public string Note { get; set; }

        // JOIN fields
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DepartmentName { get; set; }
        public string ShiftName { get; set; }
    }

    internal class HolidayDto
    {
        public int Id { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Name { get; set; }
        public string HolidayType { get; set; }
        public bool IsRecurring { get; set; }
    }

    internal class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public bool IsHalfDay { get; set; }
        public string HalfDayPeriod { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string RejectReason { get; set; }

        // JOIN fields
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string ApprovedByName { get; set; }
    }

    // =============================================
    // View DTOs
    // =============================================

    internal class TodayAttendanceDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan? ShiftStart { get; set; }
        public TimeSpan? ShiftEnd { get; set; }
        public int? LateThreshold { get; set; }
        public int? EarlyThreshold { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string CheckInMethod { get; set; }
        public string CheckOutMethod { get; set; }
        public float? CheckInConfidence { get; set; }
        public float? CheckOutConfidence { get; set; }
        public string Status { get; set; }
        public int? LateMinutes { get; set; }
        public int? EarlyMinutes { get; set; }
        public decimal? WorkingHours { get; set; }
        public bool? IsManualEdit { get; set; }
        public bool IsFaceRegistered { get; set; }
    }

    internal class MonthlySummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime Month { get; set; }
        public int TotalRecords { get; set; }
        public int PresentDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyLeaveDays { get; set; }
        public int LateAndEarlyDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HolidayDays { get; set; }
        public int DayOffDays { get; set; }
        public int ActualWorkDays { get; set; }
        public long TotalLateMinutes { get; set; }
        public long TotalEarlyMinutes { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public int ManualEditCount { get; set; }
    }

    internal class FaceStatusDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public bool IsFaceRegistered { get; set; }
        public DateTime? FaceRegisteredAt { get; set; }
        public int TotalFaces { get; set; }
        public int ActiveFaces { get; set; }
        public int VerifiedFaces { get; set; }
        public float? AvgQuality { get; set; }
        public float? MinQuality { get; set; }
    }

    // =============================================
    // Dashboard Stats
    // =============================================

    internal class DashboardStats
    {
        public int TotalEmployees { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int LeaveCount { get; set; }
        public int NotYetCount { get; set; }
        public int FaceRegistered { get; set; }
        public int FaceNotRegistered { get; set; }
    }

    // =============================================
    // Additional DTOs for full DB coverage
    // =============================================

    internal class AttendanceDeviceDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
        public string IpAddress { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastHeartbeat { get; set; }
    }

    internal class WorkCalendarDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    internal class EmployeeShiftScheduleDto
    {
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int ShiftId { get; set; }
        public bool IsOverride { get; set; }
        public string Note { get; set; }

        // JOIN fields
        public string EmployeeName { get; set; }
        public string ShiftName { get; set; }
    }

    internal class AttendanceLogDto
    {
        public long Id { get; set; }
        public long? AttendanceId { get; set; }
        public int? EmployeeId { get; set; }
        public int? DeviceId { get; set; }
        public DateTime LogTime { get; set; }
        public string LogType { get; set; }
        public string Method { get; set; }
        public int? MatchedFaceId { get; set; }
        public float? Confidence { get; set; }
        public float? FaceDistance { get; set; }
        public string ImagePath { get; set; }
        public string Result { get; set; }
        public string FailReason { get; set; }

        // JOIN fields
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DeviceName { get; set; }
    }

    internal class AuditLogDto
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public int? EmployeeId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string IpAddress { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // JOIN fields
        public string Username { get; set; }
    }

    internal class SystemSettingDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
    }

    internal class FaceRegistrationLogDto
    {
        public long Id { get; set; }
        public int? FaceDataId { get; set; }
        public int EmployeeId { get; set; }
        public string Action { get; set; }
        public int? PerformedBy { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }

        // JOIN fields
        public string EmployeeName { get; set; }
        public string PerformedByName { get; set; }
    }

}

using System;

namespace FaceRecog.WinForms.Data
{
    internal sealed class ScanSessionItem
    {
        public Guid Id { get; set; }

        public string ScanType { get; set; }

        public string SourcePath { get; set; }

        public string ModelName { get; set; }

        public int? CpuCount { get; set; }

        public string Status { get; set; }

        public int ResultCount { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    internal sealed class DetectionItem
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public string ImagePath { get; set; }

        public string FileName { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }

        public int Left { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public sealed class AppUserItem
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }

        public string PasswordHash { get; set; }

        public string FaceEncodingData { get; set; }

        public string FaceImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool HasFaceEnrollment
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.FaceEncodingData) && !string.IsNullOrWhiteSpace(this.FaceImagePath);
            }
        }

        public string FaceStatus
        {
            get
            {
                return this.HasFaceEnrollment ? "Đã đăng ký" : "Chưa đăng ký";
            }
        }
    }

    internal sealed class LoginHistoryItem
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }

        public DateTime LoggedInAt { get; set; }
    }

    internal sealed class AttendanceItem
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string CapturedImagePath { get; set; }

        public string ModelName { get; set; }

        public string Status { get; set; }

        public double? MatchDistance { get; set; }

        public DateTime AttendedAt { get; set; }
    }

    internal sealed class AttendanceSummaryItem
    {
        public Guid? UserId { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public DateTime AttendanceDay { get; set; }

        public DateTime? CheckInAt { get; set; }

        public DateTime? CheckOutAt { get; set; }

        public int RecordCount { get; set; }

        public string WorkState { get; set; }
    }

    internal sealed class FaceMatchItem
    {
        public int FaceIndex { get; set; }

        public string Box { get; set; }

        public Guid? UserId { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string Status { get; set; }

        public double Distance { get; set; }

        public string EncodingData { get; set; }
    }
}

using System;

namespace FaceRecognitionDotNet.WinForms.Data
{
    internal static class AttendanceSchedule
    {
        private static readonly TimeSpan CheckInStart = new TimeSpan(7, 0, 0);
        private static readonly TimeSpan CheckOutStart = new TimeSpan(17, 0, 0);

        public static DateTimeOffset GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, GetVietnamTimeZone());
        }

        public static string GetAttendanceStatus(DateTimeOffset vietnamTime)
        {
            return vietnamTime.TimeOfDay < CheckOutStart ? "CheckIn" : "CheckOut";
        }

        public static string GetAttendanceLabel(string attendanceStatus)
        {
            return string.Equals(attendanceStatus, "CheckOut", StringComparison.OrdinalIgnoreCase)
                ? "Tan làm"
                : "Vào làm";
        }

        public static string GetScheduleSummary(DateTimeOffset vietnamTime)
        {
            var status = GetAttendanceStatus(vietnamTime);
            return $"Giờ VN hiện tại {vietnamTime:HH:mm}. Vào làm từ 07:00, tan làm từ 17:00. Chế độ ghi nhận: {GetAttendanceLabel(status)}.";
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }
    }
}
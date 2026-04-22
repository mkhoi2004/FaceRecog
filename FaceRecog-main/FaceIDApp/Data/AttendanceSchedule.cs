using System;

namespace FaceIDApp.Data
{
    /// <summary>
    /// Tính toán trạng thái chấm công dựa trên giờ Việt Nam và thông tin ca làm việc.
    /// </summary>
    internal static class AttendanceSchedule
    {
        public static DateTimeOffset GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, GetVietnamTimeZone());
        }

        /// <summary>
        /// Xác định nên CheckIn hay CheckOut dựa trên thời gian hiện tại và ca làm việc.
        /// </summary>
        public static string GetAttendanceAction(DateTimeOffset vietnamTime, TimeSpan shiftStart, TimeSpan shiftEnd)
        {
            var midPoint = shiftStart + TimeSpan.FromTicks((shiftEnd - shiftStart).Ticks / 2);
            return vietnamTime.TimeOfDay < midPoint ? "CheckIn" : "CheckOut";
        }

        /// <summary>
        /// Fallback khi không có thông tin ca: dùng 08:00-17:00
        /// </summary>
        public static string GetAttendanceAction(DateTimeOffset vietnamTime)
        {
            return GetAttendanceAction(vietnamTime, new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
        }

        public static string GetActionLabel(string action)
        {
            return string.Equals(action, "CheckOut", StringComparison.OrdinalIgnoreCase)
                ? "Tan làm"
                : "Vào làm";
        }

        /// <summary>
        /// Tính status chấm công: Present / Late / EarlyLeave / LateAndEarly
        /// </summary>
        public static string CalculateStatus(
            TimeSpan checkInTime, TimeSpan? checkOutTime,
            TimeSpan shiftStart, TimeSpan shiftEnd,
            int lateThreshold, int earlyThreshold)
        {
            var allowedLateTime = shiftStart + TimeSpan.FromMinutes(lateThreshold);
            var allowedEarlyTime = shiftEnd - TimeSpan.FromMinutes(earlyThreshold);

            bool isLate = checkInTime > allowedLateTime;
            bool isEarly = checkOutTime.HasValue && checkOutTime.Value < allowedEarlyTime;

            if (isLate && isEarly) return "LateAndEarly";
            if (isLate) return "Late";
            if (isEarly) return "EarlyLeave";
            return "Present";
        }

        /// <summary>
        /// Tính số phút đi muộn (sau khi trừ ngưỡng ân hạn)
        /// </summary>
        public static int CalculateLateMinutes(TimeSpan checkInTime, TimeSpan shiftStart, int lateThreshold)
        {
            var allowedTime = shiftStart + TimeSpan.FromMinutes(lateThreshold);
            if (checkInTime <= allowedTime)
                return 0;

            return (int)(checkInTime - allowedTime).TotalMinutes;
        }

        /// <summary>
        /// Tính số phút về sớm (sau khi trừ ngưỡng ân hạn)
        /// </summary>
        public static int CalculateEarlyMinutes(TimeSpan checkOutTime, TimeSpan shiftEnd, int earlyThreshold)
        {
            var allowedTime = shiftEnd - TimeSpan.FromMinutes(earlyThreshold);
            if (checkOutTime >= allowedTime)
                return 0;

            return (int)(allowedTime - checkOutTime).TotalMinutes;
        }

        /// <summary>
        /// Tính số phút làm việc thực tế
        /// </summary>
        public static int CalculateWorkingMinutes(DateTimeOffset checkIn, DateTimeOffset checkOut, int breakMinutes)
        {
            var totalMinutes = (int)(checkOut - checkIn).TotalMinutes;
            return Math.Max(0, totalMinutes - breakMinutes);
        }

        public static string GetScheduleSummary(DateTimeOffset vietnamTime)
        {
            var action = GetAttendanceAction(vietnamTime);
            return $"Giờ VN: {vietnamTime:HH:mm}. Chế độ: {GetActionLabel(action)}.";
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }
    }
}

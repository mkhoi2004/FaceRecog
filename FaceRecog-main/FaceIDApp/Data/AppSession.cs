namespace FaceIDApp.Data
{
    /// <summary>
    /// Lưu thông tin phiên đăng nhập hiện tại (singleton tĩnh).
    /// Được set tại LoginForm sau khi đăng nhập thành công.
    /// </summary>
    internal static class AppSession
    {
        /// <summary>Người dùng đang đăng nhập</summary>
        public static UserDto CurrentUser { get; set; }

        /// <summary>Kiểm tra có phải Admin không</summary>
        public static bool IsAdmin => CurrentUser?.Role == "Admin";

        /// <summary>ID nhân viên của user hiện tại (nullable)</summary>
        public static int? CurrentEmployeeId => CurrentUser?.EmployeeId;

        /// <summary>Xóa session khi đăng xuất</summary>
        public static void Clear() => CurrentUser = null;
    }
}

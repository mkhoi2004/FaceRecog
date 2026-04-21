using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            EnableDpiAwareness();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize database
            try
            {
                AppDatabase.InitializeAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể kết nối PostgreSQL!\n\n{ex.Message}\n\nVui lòng kiểm tra:\n- PostgreSQL đã chạy chưa?\n- Connection string trong App.config đúng chưa?",
                    "Khởi tạo Database thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Login
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                    return;

                Application.Run(new MainForm(loginForm.LoggedInUser));
            }
        }

        private static void EnableDpiAwareness()
        {
            try
            {
                // Prefer per-monitor DPI awareness to avoid blurry UI on scaled displays.
                SetProcessDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware);
            }
            catch
            {
                try
                {
                    SetProcessDPIAware();
                }
                catch
                {
                    // Best effort only.
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("Shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDpiAwareness value);

        private enum ProcessDpiAwareness
        {
            ProcessDpiUnaware = 0,
            ProcessSystemDpiAware = 1,
            ProcessPerMonitorDpiAware = 2
        }
    }
}

using System;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
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

                Application.Run(new MainForm(loginForm.AuthenticatedUser));
            }
        }
    }
}

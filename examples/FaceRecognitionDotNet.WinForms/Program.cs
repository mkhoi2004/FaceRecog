using System;
using System.Windows.Forms;
using FaceRecognitionDotNet.WinForms.Data;

namespace FaceRecognitionDotNet.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                AppDatabase.InitializeAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Khởi tạo PostgreSQL thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                        return;

                    Application.Run(new MainForm(loginForm.AuthenticatedUser));
                }
        }
    }
}

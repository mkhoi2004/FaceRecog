using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using FaceRecog.WinForms.Data;

namespace FaceRecog.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
            ShowStartupDiagnostics();
#endif

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

        [Conditional("DEBUG")]
        private static void ShowStartupDiagnostics()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var diagnosticText =
                "StartupPath: " + Application.StartupPath + Environment.NewLine +
                "BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory + Environment.NewLine +
                "CurrentDirectory: " + Environment.CurrentDirectory + Environment.NewLine +
                "AssemblyLocation: " + assemblyLocation + Environment.NewLine +
                "ExeDirectory: " + Path.GetDirectoryName(assemblyLocation);

            Debug.WriteLine(diagnosticText);
            MessageBox.Show(diagnosticText, "FaceRecog debug startup diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

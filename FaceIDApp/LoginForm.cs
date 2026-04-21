using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblError;
        private Panel pnlForm;
        private PictureBox picLogo;
        private CheckBox chkShowPassword;

        public UserDto LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "FaceID — Đăng nhập";
            this.Size = new Size(480, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.DoubleBuffered = true;

            // Enable drag
            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { NativeMethods.ReleaseCapture(); NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0); } };

            // Close button
            var btnClose = new Label
            {
                Text = "✕", Font = new Font("Segoe UI", 14F),
                ForeColor = Color.FromArgb(148, 163, 184), Size = new Size(40, 40),
                Location = new Point(this.Width - 45, 5), TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(239, 68, 68);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.FromArgb(148, 163, 184);
            this.Controls.Add(btnClose);

            // Form panel (center card)
            pnlForm = new Panel
            {
                Size = new Size(400, 480),
                Location = new Point(40, 80),
                BackColor = Color.FromArgb(30, 41, 59)
            };
            pnlForm.Paint += PnlForm_Paint;
            this.Controls.Add(pnlForm);

            // Icon
            picLogo = new PictureBox
            {
                Size = new Size(72, 72),
                Location = new Point(164, 20),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };
            picLogo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new LinearGradientBrush(picLogo.ClientRectangle, Color.FromArgb(56, 189, 248), Color.FromArgb(59, 130, 246), 45F))
                    g.FillEllipse(brush, 4, 4, 64, 64);
                using (var font = new Font("Segoe UI", 28F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("🛡", font, Brushes.White, new RectangleF(4, 4, 64, 64), sf);
            };
            pnlForm.Controls.Add(picLogo);

            // Title
            lblTitle = new Label
            {
                Text = "FACEID SYSTEM",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 40), Location = new Point(0, 100),
                BackColor = Color.Transparent
            };
            pnlForm.Controls.Add(lblTitle);

            // Subtitle
            lblSubtitle = new Label
            {
                Text = "Hệ thống chấm công nhận diện khuôn mặt",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 25), Location = new Point(0, 140),
                BackColor = Color.Transparent
            };
            pnlForm.Controls.Add(lblSubtitle);

            // Username label
            var lblUser = new Label
            {
                Text = "TÀI KHOẢN", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184), Size = new Size(320, 20),
                Location = new Point(40, 190), BackColor = Color.Transparent
            };
            pnlForm.Controls.Add(lblUser);

            // Username
            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.FromArgb(51, 65, 85),
                BorderStyle = BorderStyle.None,
                Size = new Size(320, 28), Location = new Point(40, 215)
            };
            var pnlUser = CreateInputPanel(txtUsername, 40, 208);
            pnlForm.Controls.Add(pnlUser);

            // Password label
            var lblPass = new Label
            {
                Text = "MẬT KHẨU", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184), Size = new Size(320, 20),
                Location = new Point(40, 265), BackColor = Color.Transparent
            };
            pnlForm.Controls.Add(lblPass);

            // Password
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.FromArgb(51, 65, 85),
                BorderStyle = BorderStyle.None,
                Size = new Size(290, 28), Location = new Point(40, 290),
                UseSystemPasswordChar = true
            };
            var pnlPass = CreateInputPanel(txtPassword, 40, 283);
            pnlForm.Controls.Add(pnlPass);

            // Show password
            chkShowPassword = new CheckBox
            {
                Text = "Hiện mật khẩu", Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(148, 163, 184),
                Size = new Size(150, 25), Location = new Point(40, 330),
                BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat
            };
            chkShowPassword.CheckedChanged += (s, e) => txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            pnlForm.Controls.Add(chkShowPassword);

            // Login button
            btnLogin = new Button
            {
                Text = "ĐĂNG NHẬP", Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Size = new Size(320, 48), Location = new Point(40, 370),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Paint += BtnLogin_Paint;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.MouseEnter += (s, e) => btnLogin.Invalidate();
            btnLogin.MouseLeave += (s, e) => btnLogin.Invalidate();
            pnlForm.Controls.Add(btnLogin);

            // Error label
            lblError = new Label
            {
                Text = "", Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(248, 113, 113),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(320, 40), Location = new Point(40, 425),
                BackColor = Color.Transparent
            };
            pnlForm.Controls.Add(lblError);

            // Enter key
            txtPassword.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) { BtnLogin_Click(null, null); e.Handled = true; } };
            txtUsername.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) { txtPassword.Focus(); e.Handled = true; } };

            this.AcceptButton = btnLogin;
        }

        private Panel CreateInputPanel(TextBox txt, int x, int y)
        {
            var pnl = new Panel
            {
                Size = new Size(320, 38),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(51, 65, 85)
            };
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(txt.Focused ? Color.FromArgb(56, 189, 248) : Color.FromArgb(71, 85, 105), 1))
                {
                    var rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                    using (var path = RoundedRect(rect, 6))
                        g.DrawPath(pen, path);
                }
            };
            txt.Location = new Point(10, 7);
            txt.Width = 298;
            txt.GotFocus += (s, e) => pnl.Invalidate();
            txt.LostFocus += (s, e) => pnl.Invalidate();
            pnl.Controls.Add(txt);
            return pnl;
        }

        private void PnlForm_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, pnlForm.Width - 1, pnlForm.Height - 1);
            using (var path = RoundedRect(rect, 16))
            {
                using (var brush = new SolidBrush(Color.FromArgb(30, 41, 59)))
                    g.FillPath(brush, path);
                using (var pen = new Pen(Color.FromArgb(51, 65, 85), 1))
                    g.DrawPath(pen, path);
            }
        }

        private void BtnLogin_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, btnLogin.Width, btnLogin.Height);
            var hover = btnLogin.ClientRectangle.Contains(btnLogin.PointToClient(Cursor.Position));
            using (var path = RoundedRect(rect, 10))
            using (var brush = new LinearGradientBrush(rect,
                hover ? Color.FromArgb(14, 165, 233) : Color.FromArgb(56, 189, 248),
                hover ? Color.FromArgb(37, 99, 235) : Color.FromArgb(59, 130, 246), 90F))
            {
                g.FillPath(brush, path);
            }
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var font = new Font("Segoe UI", 12F, FontStyle.Bold))
                g.DrawString("ĐĂNG NHẬP", font, Brushes.White, rect, sf);
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Vui lòng nhập tài khoản và mật khẩu!";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";

            try
            {
                var user = await AppDatabase.Repository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    lblError.Text = "Tài khoản không tồn tại!";
                    return;
                }

                if (!user.IsActive)
                {
                    lblError.Text = "Tài khoản đã bị khóa!";
                    return;
                }

                if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.Now)
                {
                    lblError.Text = $"Tài khoản bị khóa đến {user.LockedUntil.Value:HH:mm}!";
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    await AppDatabase.Repository.IncrementFailedLoginAsync(user.Id);
                    lblError.Text = $"Sai mật khẩu! ({user.FailedLoginCount + 1}/5 lần)";
                    return;
                }

                await AppDatabase.Repository.UpdateUserLastLoginAsync(user.Id);
                LoggedInUser = user;
                AppSession.CurrentUser = user;   // set global session
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblError.Text = $"Lỗi: {ex.Message}";
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "ĐĂNG NHẬP";
                btnLogin.Invalidate();
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceRecog.WinForms.Data;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FaceRecog.WinForms
{
    public sealed class LoginForm : Form
    {
        private readonly TextBox _LoginUsernameTextBox;
        private readonly TextBox _LoginPasswordTextBox;
        private readonly TextBox _RegisterUsernameTextBox;
        private readonly TextBox _RegisterFullNameTextBox;
        private readonly TextBox _RegisterPasswordTextBox;
        private readonly TextBox _RegisterConfirmPasswordTextBox;
        private readonly Button _LoginButton;
        private readonly Button _RegisterButton;
        private readonly Label _StatusLabel;
        private readonly TabControl _TabControl;

        public LoginForm()
        {
            this._LoginUsernameTextBox = new TextBox();
            this._LoginPasswordTextBox = new TextBox();
            this._RegisterUsernameTextBox = new TextBox();
            this._RegisterFullNameTextBox = new TextBox();
            this._RegisterPasswordTextBox = new TextBox();
            this._RegisterConfirmPasswordTextBox = new TextBox();
            this._LoginButton = new Button();
            this._RegisterButton = new Button();
            this._StatusLabel = new Label();
            this._TabControl = new TabControl();
            this.InitializeComponent();
        }

        public AppUserItem AuthenticatedUser { get; private set; }

        private void InitializeComponent()
        {
            var loginPage = new TabPage("Đăng nhập");
            var registerPage = new TabPage("Tạo tài khoản");
            var loginPanel = new Panel();
            var registerPanel = new Panel();
            var loginUsernameLabel = new Label();
            var loginPasswordLabel = new Label();
            var registerUsernameLabel = new Label();
            var registerFullNameLabel = new Label();
            var registerPasswordLabel = new Label();
            var registerConfirmLabel = new Label();

            this.Text = "FaceRecog - Xác thực";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(520, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            this._TabControl.Dock = DockStyle.Fill;
            this._TabControl.ItemSize = new System.Drawing.Size(160, 30);
            this._TabControl.SizeMode = TabSizeMode.Fixed;
            this._TabControl.Padding = new System.Drawing.Point(16, 4);

            loginPanel.Dock = DockStyle.Fill;
            loginPanel.Padding = new Padding(24);
            registerPanel.Dock = DockStyle.Fill;
            registerPanel.Padding = new Padding(24);

            loginUsernameLabel.AutoSize = true;
            loginUsernameLabel.Location = new System.Drawing.Point(24, 28);
            loginUsernameLabel.Text = "Tên đăng nhập";

            this._LoginUsernameTextBox.Location = new System.Drawing.Point(24, 50);
            this._LoginUsernameTextBox.Width = 420;

            loginPasswordLabel.AutoSize = true;
            loginPasswordLabel.Location = new System.Drawing.Point(24, 88);
            loginPasswordLabel.Text = "Mật khẩu";

            this._LoginPasswordTextBox.Location = new System.Drawing.Point(24, 110);
            this._LoginPasswordTextBox.Width = 420;
            this._LoginPasswordTextBox.UseSystemPasswordChar = true;

            this._LoginButton.Location = new System.Drawing.Point(24, 150);
            this._LoginButton.Size = new System.Drawing.Size(120, 32);
            this._LoginButton.Text = "Đăng nhập";
            this._LoginButton.Click += this.LoginButtonOnClick;

            loginPanel.Controls.Add(loginUsernameLabel);
            loginPanel.Controls.Add(this._LoginUsernameTextBox);
            loginPanel.Controls.Add(loginPasswordLabel);
            loginPanel.Controls.Add(this._LoginPasswordTextBox);
            loginPanel.Controls.Add(this._LoginButton);

            registerUsernameLabel.AutoSize = true;
            registerUsernameLabel.Location = new System.Drawing.Point(24, 20);
            registerUsernameLabel.Text = "Tên đăng nhập";

            this._RegisterUsernameTextBox.Location = new System.Drawing.Point(24, 42);
            this._RegisterUsernameTextBox.Width = 420;

            registerFullNameLabel.AutoSize = true;
            registerFullNameLabel.Location = new System.Drawing.Point(24, 80);
            registerFullNameLabel.Text = "Họ và tên";

            this._RegisterFullNameTextBox.Location = new System.Drawing.Point(24, 102);
            this._RegisterFullNameTextBox.Width = 420;

            registerPasswordLabel.AutoSize = true;
            registerPasswordLabel.Location = new System.Drawing.Point(24, 140);
            registerPasswordLabel.Text = "Mật khẩu";

            this._RegisterPasswordTextBox.Location = new System.Drawing.Point(24, 162);
            this._RegisterPasswordTextBox.Width = 420;
            this._RegisterPasswordTextBox.UseSystemPasswordChar = true;

            registerConfirmLabel.AutoSize = true;
            registerConfirmLabel.Location = new System.Drawing.Point(24, 200);
            registerConfirmLabel.Text = "Nhập lại mật khẩu";

            this._RegisterConfirmPasswordTextBox.Location = new System.Drawing.Point(24, 222);
            this._RegisterConfirmPasswordTextBox.Width = 420;
            this._RegisterConfirmPasswordTextBox.UseSystemPasswordChar = true;

            this._RegisterButton.Location = new System.Drawing.Point(24, 260);
            this._RegisterButton.Size = new System.Drawing.Size(120, 32);
            this._RegisterButton.Text = "Tạo tài khoản";
            this._RegisterButton.Click += this.RegisterButtonOnClick;

            registerPanel.Controls.Add(registerUsernameLabel);
            registerPanel.Controls.Add(this._RegisterUsernameTextBox);
            registerPanel.Controls.Add(registerFullNameLabel);
            registerPanel.Controls.Add(this._RegisterFullNameTextBox);
            registerPanel.Controls.Add(registerPasswordLabel);
            registerPanel.Controls.Add(this._RegisterPasswordTextBox);
            registerPanel.Controls.Add(registerConfirmLabel);
            registerPanel.Controls.Add(this._RegisterConfirmPasswordTextBox);
            registerPanel.Controls.Add(this._RegisterButton);

            loginPage.Controls.Add(loginPanel);
            registerPage.Controls.Add(registerPanel);
            this._TabControl.TabPages.Add(loginPage);
            this._TabControl.TabPages.Add(registerPage);

            this._StatusLabel.Dock = DockStyle.Bottom;
            this._StatusLabel.Height = 26;
            this._StatusLabel.Padding = new Padding(24, 0, 24, 0);
            this._StatusLabel.Text = "Sẵn sàng.";

            this.Controls.Add(this._TabControl);
            this.Controls.Add(this._StatusLabel);
        }

        private async void LoginButtonOnClick(object sender, EventArgs e)
        {
            await this.LoginAsync().ConfigureAwait(true);
        }

        private async Task LoginAsync()
        {
            var username = this._LoginUsernameTextBox.Text.Trim();
            var password = this._LoginPasswordTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                this._StatusLabel.Text = "Nhập tên đăng nhập và mật khẩu.";
                return;
            }

            var user = await AppDatabase.Repository.GetUserByUsernameAsync(username).ConfigureAwait(true);
            if (user == null || !AuthPasswordHasher.Verify(password, user.PasswordHash))
            {
                this._StatusLabel.Text = "Sai tên đăng nhập hoặc mật khẩu.";
                return;
            }

            await AppDatabase.Repository.LogUserLoginAsync(user.Id).ConfigureAwait(true);
            await AppDatabase.Repository.UpdateUserLastLoginAsync(user.Id).ConfigureAwait(true);
            this.AuthenticatedUser = user;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void RegisterButtonOnClick(object sender, EventArgs e)
        {
            await this.RegisterAsync().ConfigureAwait(true);
        }

        private async Task RegisterAsync()
        {
            var username = this._RegisterUsernameTextBox.Text.Trim();
            var fullName = this._RegisterFullNameTextBox.Text.Trim();
            var password = this._RegisterPasswordTextBox.Text;
            var confirmPassword = this._RegisterConfirmPasswordTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
            {
                this._StatusLabel.Text = "Điền đầy đủ thông tin tài khoản.";
                return;
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                this._StatusLabel.Text = "Mật khẩu nhập lại không khớp.";
                return;
            }

            var existing = await AppDatabase.Repository.GetUserByUsernameAsync(username).ConfigureAwait(true);
            if (existing != null)
            {
                this._StatusLabel.Text = "Tên đăng nhập đã tồn tại.";
                return;
            }

            var userId = await AppDatabase.Repository.CreateUserAsync(username, fullName, AuthPasswordHasher.Hash(password), "User").ConfigureAwait(true);
            var user = await AppDatabase.Repository.GetUserByUsernameAsync(username).ConfigureAwait(true);
            if (user != null)
                await AppDatabase.Repository.UpdateUserLastLoginAsync(user.Id).ConfigureAwait(true);

            this.AuthenticatedUser = user ?? new AppUserItem { Id = userId, Username = username, FullName = fullName };
            MessageBox.Show(this, "Tạo tài khoản thành công. Bạn có thể đăng nhập ngay.", "Tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this._TabControl.SelectedIndex = 0;
            this._LoginUsernameTextBox.Text = username;
            this._LoginPasswordTextBox.Clear();
            this._StatusLabel.Text = "Đã tạo tài khoản.";
        }
    }
}


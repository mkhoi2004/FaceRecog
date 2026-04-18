using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceRecognitionDotNet;
using FaceRecognitionDotNet.WinForms.Data;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FaceRecognitionDotNet.WinForms
{
    public sealed class AccountsView : UserControl
    {
        private readonly BindingList<AppUserItem> _Users;
        private readonly DataGridView _UsersGrid;
        private readonly TextBox _ImagePathTextBox;
        private readonly ComboBox _ModelComboBox;
        private readonly Button _BrowseButton;
        private readonly Button _EnrollButton;
        private readonly Button _RefreshButton;
        private readonly Label _StatusLabel;
        private readonly Label _FaceStatusLabel;
        private readonly FaceRecognitionService _RecognitionService;

        public AccountsView()
        {
            this._Users = new BindingList<AppUserItem>();
            this._UsersGrid = new DataGridView();
            this._ImagePathTextBox = new TextBox();
            this._ModelComboBox = new ComboBox();
            this._BrowseButton = new Button();
            this._EnrollButton = new Button();
            this._RefreshButton = new Button();
            this._StatusLabel = new Label();
            this._FaceStatusLabel = new Label();
            this._RecognitionService = new FaceRecognitionService();
            this.InitializeComponent();
        }

        private async void AccountsViewOnLoad(object sender, EventArgs e)
        {
            await this.LoadUsersAsync().ConfigureAwait(true);
        }

        private void InitializeComponent()
        {
            var root = new Panel();
            var header = new Panel();
            var title = new Label();
            var subtitle = new Label();
            var leftPanel = new Panel();
            var rightPanel = new Panel();
            var enrollTitle = new Label();
            var usersTitle = new Label();
            var imageLabel = new Label();
            var modelLabel = new Label();
            var faceStatusTitle = new Label();

            this.Dock = DockStyle.Fill;
            this.Load += this.AccountsViewOnLoad;

            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(18);
            root.BackColor = Color.FromArgb(245, 247, 250);

            header.Dock = DockStyle.Top;
            header.Height = 92;
            header.BackColor = Color.White;
            header.Padding = new Padding(20, 18, 20, 16);

            title.AutoSize = true;
            title.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point);
            title.Location = new System.Drawing.Point(24, 14);
            title.Text = "Tài khoản và đăng ký khuôn mặt";

            subtitle.AutoSize = true;
            subtitle.ForeColor = Color.FromArgb(100, 116, 139);
            subtitle.Location = new System.Drawing.Point(26, 48);
            subtitle.Text = "Tạo và quản lý người dùng, sau đó đăng ký ảnh khuôn mặt để ứng dụng nhận diện về sau.";

            this._RefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._RefreshButton.Location = new System.Drawing.Point(820, 26);
            this._RefreshButton.Size = new System.Drawing.Size(110, 30);
            this._RefreshButton.Text = "Tải lại";
            this._RefreshButton.Click += async (s, e) => await this.LoadUsersAsync().ConfigureAwait(true);

            this._StatusLabel.AutoSize = false;
            this._StatusLabel.Location = new System.Drawing.Point(430, 30);
            this._StatusLabel.Size = new System.Drawing.Size(380, 24);
            this._StatusLabel.Text = "Sẵn sàng.";

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(this._RefreshButton);
            header.Controls.Add(this._StatusLabel);

            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 480;
            leftPanel.BackColor = Color.White;
            leftPanel.Padding = new Padding(18);
            leftPanel.BorderStyle = BorderStyle.FixedSingle;

            usersTitle.AutoSize = true;
            usersTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            usersTitle.Location = new System.Drawing.Point(18, 14);
            usersTitle.Text = "Người dùng";

            this._UsersGrid.Dock = DockStyle.Fill;
            this._UsersGrid.Location = new System.Drawing.Point(18, 44);
            this._UsersGrid.ReadOnly = true;
            this._UsersGrid.AllowUserToAddRows = false;
            this._UsersGrid.AllowUserToDeleteRows = false;
            this._UsersGrid.AutoGenerateColumns = false;
            this._UsersGrid.MultiSelect = false;
            this._UsersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._UsersGrid.RowHeadersVisible = false;
            this._UsersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._UsersGrid.DataSource = this._Users;

            this._UsersGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Tên đăng nhập", FillWeight = 90 });
            this._UsersGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ và tên", FillWeight = 110 });
            this._UsersGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FaceStatus", HeaderText = "Khuôn mặt", FillWeight = 70 });
            this._UsersGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreatedAt", HeaderText = "Ngày tạo", FillWeight = 80 });
            this._UsersGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LastLoginAt", HeaderText = "Đăng nhập cuối", FillWeight = 80 });
            this._UsersGrid.SelectionChanged += this.UsersGridOnSelectionChanged;

            leftPanel.Controls.Add(this._UsersGrid);
            leftPanel.Controls.Add(usersTitle);

            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.White;
            rightPanel.Padding = new Padding(18);
            rightPanel.BorderStyle = BorderStyle.FixedSingle;

            enrollTitle.AutoSize = true;
            enrollTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            enrollTitle.Location = new System.Drawing.Point(18, 14);
            enrollTitle.Text = "Đăng ký khuôn mặt";

            imageLabel.AutoSize = true;
            imageLabel.Location = new System.Drawing.Point(20, 52);
            imageLabel.Text = "Ảnh";

            this._ImagePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this._ImagePathTextBox.Location = new System.Drawing.Point(92, 48);
            this._ImagePathTextBox.Width = 560;

            this._BrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._BrowseButton.Location = new System.Drawing.Point(674, 46);
            this._BrowseButton.Size = new System.Drawing.Size(96, 28);
            this._BrowseButton.Text = "Chọn ảnh";
            this._BrowseButton.Click += this.BrowseButtonOnClick;

            modelLabel.AutoSize = true;
            modelLabel.Location = new System.Drawing.Point(20, 90);
            modelLabel.Text = "Model";

            this._ModelComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._ModelComboBox.Items.AddRange(new object[] { Model.Hog.ToString(), Model.Cnn.ToString() });
            this._ModelComboBox.Location = new System.Drawing.Point(92, 86);
            this._ModelComboBox.Width = 120;

            this._EnrollButton.Location = new System.Drawing.Point(92, 130);
            this._EnrollButton.Size = new System.Drawing.Size(220, 32);
            this._EnrollButton.Text = "Đăng ký khuôn mặt cho user đã chọn";
            this._EnrollButton.Click += this.EnrollButtonOnClick;

            faceStatusTitle.AutoSize = true;
            faceStatusTitle.Location = new System.Drawing.Point(20, 176);
            faceStatusTitle.Text = "Trạng thái";

            this._FaceStatusLabel.AutoSize = false;
            this._FaceStatusLabel.Location = new System.Drawing.Point(92, 174);
            this._FaceStatusLabel.Size = new System.Drawing.Size(320, 24);
            this._FaceStatusLabel.ForeColor = Color.FromArgb(71, 85, 105);
            this._FaceStatusLabel.Text = "Chưa chọn tài khoản.";

            this._StatusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this._StatusLabel.Location = new System.Drawing.Point(18, 410);
            this._StatusLabel.Size = new System.Drawing.Size(760, 26);
            this._StatusLabel.Text = "Chọn một người dùng và một ảnh khuôn mặt để lưu mã khuôn mặt.";

            rightPanel.Controls.Add(enrollTitle);
            rightPanel.Controls.Add(imageLabel);
            rightPanel.Controls.Add(this._ImagePathTextBox);
            rightPanel.Controls.Add(this._BrowseButton);
            rightPanel.Controls.Add(modelLabel);
            rightPanel.Controls.Add(this._ModelComboBox);
            rightPanel.Controls.Add(this._EnrollButton);
            rightPanel.Controls.Add(faceStatusTitle);
            rightPanel.Controls.Add(this._FaceStatusLabel);
            rightPanel.Controls.Add(this._StatusLabel);

            root.Controls.Add(rightPanel);
            root.Controls.Add(leftPanel);
            root.Controls.Add(header);
            this.Controls.Add(root);

            this._ModelComboBox.SelectedIndex = 0;
            this.UpdateEnrollmentUiState();
        }

        private void BrowseButtonOnClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Images|*.jpg;*.jpeg;*.png|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    this._ImagePathTextBox.Text = dialog.FileName;
            }
        }

        private async void EnrollButtonOnClick(object sender, EventArgs e)
        {
            await this.EnrollAsync().ConfigureAwait(true);
        }

        private async Task EnrollAsync()
        {
            if (this._UsersGrid.CurrentRow == null || !(this._UsersGrid.CurrentRow.DataBoundItem is AppUserItem selectedUser))
            {
                this._StatusLabel.Text = "Chọn một tài khoản trước.";
                return;
            }

            var imagePath = this._ImagePathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                this._StatusLabel.Text = "Chọn file ảnh khuôn mặt hợp lệ.";
                return;
            }

            if (!Enum.TryParse<Model>(this._ModelComboBox.SelectedItem?.ToString() ?? Model.Hog.ToString(), true, out var model))
            {
                this._StatusLabel.Text = "Model không hợp lệ.";
                return;
            }

            var modelsDirectory = Path.GetFullPath("models");
            if (!Directory.Exists(modelsDirectory))
            {
                this._StatusLabel.Text = $"Không tìm thấy thư mục model '{modelsDirectory}'.";
                return;
            }

            try
            {
                if (await AppDatabase.Repository.HasUserFaceEnrollmentAsync(selectedUser.Id).ConfigureAwait(true))
                {
                    this._StatusLabel.Text = "Tài khoản này đã có 1 ảnh khuôn mặt. Không thể đăng ký lại.";
                    this.UpdateEnrollmentUiState();
                    return;
                }

                this.UseWaitCursor = true;
                var encodingData = await Task.Run(() => this._RecognitionService.BuildEncodingData(imagePath, modelsDirectory, model)).ConfigureAwait(true);
                if (string.IsNullOrWhiteSpace(encodingData))
                {
                    this._StatusLabel.Text = "Không phát hiện được khuôn mặt trong ảnh.";
                    return;
                }

                var storedFaceImagePath = ImageStorage.StoreFaceImage(imagePath, selectedUser.Id);
                var updated = await AppDatabase.Repository.TryUpdateUserFaceEncodingAsync(selectedUser.Id, encodingData, storedFaceImagePath).ConfigureAwait(true);
                if (!updated)
                {
                    this._StatusLabel.Text = "Tài khoản này đã có 1 ảnh khuôn mặt. Không thể ghi đè.";
                    this.UpdateEnrollmentUiState();
                    return;
                }

                this._StatusLabel.Text = $"Đã lưu 1 ảnh khuôn mặt cho {selectedUser.Username} tại thư mục face.";
                await this.LoadUsersAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                this._StatusLabel.Text = ex.Message;
            }
            finally
            {
                this.UseWaitCursor = false;
            }
        }

        private void UsersGridOnSelectionChanged(object sender, EventArgs e)
        {
            this.UpdateEnrollmentUiState();
        }

        private void UpdateEnrollmentUiState()
        {
            var selectedUser = this._UsersGrid.CurrentRow != null ? this._UsersGrid.CurrentRow.DataBoundItem as AppUserItem : null;
            if (selectedUser == null)
            {
                this._BrowseButton.Enabled = false;
                this._EnrollButton.Enabled = false;
                this._FaceStatusLabel.Text = "Chưa chọn tài khoản.";
                this._EnrollButton.Text = "Đăng ký khuôn mặt cho user đã chọn";
                return;
            }

            this._FaceStatusLabel.Text = $"Trạng thái khuôn mặt: {selectedUser.FaceStatus}";
            if (selectedUser.HasFaceEnrollment)
            {
                this._BrowseButton.Enabled = false;
                this._ImagePathTextBox.ReadOnly = true;
                this._ModelComboBox.Enabled = false;
                this._EnrollButton.Enabled = false;
                this._EnrollButton.Text = "Đã đăng ký";
            }
            else
            {
                this._BrowseButton.Enabled = true;
                this._ImagePathTextBox.ReadOnly = false;
                this._ModelComboBox.Enabled = true;
                this._EnrollButton.Enabled = true;
                this._EnrollButton.Text = "Đăng ký khuôn mặt cho user đã chọn";
            }
        }

        private async Task LoadUsersAsync()
        {
            if (AppDatabase.Repository == null)
                return;

            var users = await AppDatabase.Repository.GetUsersAsync().ConfigureAwait(true);
            this._Users.RaiseListChangedEvents = false;
            this._Users.Clear();
            foreach (var user in users)
                this._Users.Add(user);
            this._Users.RaiseListChangedEvents = true;
            this._Users.ResetBindings();
            this._StatusLabel.Text = $"Đã tải {this._Users.Count} tài khoản.";
            this.UpdateEnrollmentUiState();
        }
    }
}


using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceRecognitionDotNet.WinForms.Data;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FaceRecognitionDotNet.WinForms
{
    public sealed class AdminDashboardView : UserControl
    {
        private readonly BindingList<LoginHistoryItem> _LoginHistory;
        private readonly BindingList<AttendanceSummaryItem> _AttendanceSummaries;
        private readonly DataGridView _LoginGrid;
        private readonly DataGridView _AttendanceGrid;
        private readonly Button _RefreshButton;
        private readonly Label _StatusLabel;
        private readonly Label _LoginSummaryLabel;
        private readonly Label _AttendanceSummaryLabel;

        public AdminDashboardView()
        {
            this._LoginHistory = new BindingList<LoginHistoryItem>();
            this._AttendanceSummaries = new BindingList<AttendanceSummaryItem>();
            this._LoginGrid = new DataGridView();
            this._AttendanceGrid = new DataGridView();
            this._RefreshButton = new Button();
            this._StatusLabel = new Label();
            this._LoginSummaryLabel = new Label();
            this._AttendanceSummaryLabel = new Label();
            this.InitializeComponent();
        }

        private async void AdminDashboardViewOnLoad(object sender, EventArgs e)
        {
            await this.RefreshAsync().ConfigureAwait(true);
        }

        private void InitializeComponent()
        {
            var root = new Panel();
            var header = new Panel();
            var title = new Label();
            var subtitle = new Label();
            var loginCard = new Panel();
            var attendanceCard = new Panel();
            var loginTitle = new Label();
            var attendanceTitle = new Label();
            var split = new SplitContainer();

            this.Dock = DockStyle.Fill;
            this.Load += this.AdminDashboardViewOnLoad;

            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(18);
            root.BackColor = Color.FromArgb(245, 247, 250);

            header.Dock = DockStyle.Top;
            header.Height = 96;
            header.BackColor = Color.White;
            header.Padding = new Padding(20, 18, 20, 16);

            title.AutoSize = true;
            title.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point);
            title.Location = new System.Drawing.Point(24, 14);
            title.Text = "Bảng điều khiển quản trị";

            subtitle.AutoSize = true;
            subtitle.ForeColor = Color.FromArgb(100, 116, 139);
            subtitle.Location = new System.Drawing.Point(26, 48);
            subtitle.Text = "Xem toàn bộ lịch sử đăng nhập và lịch sử chấm công theo ngày của tất cả tài khoản.";

            this._RefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._RefreshButton.Location = new System.Drawing.Point(820, 26);
            this._RefreshButton.Size = new System.Drawing.Size(110, 30);
            this._RefreshButton.Text = "Tải lại";
            this._RefreshButton.Click += async (s, e) => await this.RefreshAsync().ConfigureAwait(true);

            this._StatusLabel.AutoSize = false;
            this._StatusLabel.Location = new System.Drawing.Point(430, 30);
            this._StatusLabel.Size = new System.Drawing.Size(360, 24);
            this._StatusLabel.Text = "Sẵn sàng.";

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(this._RefreshButton);
            header.Controls.Add(this._StatusLabel);

            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Horizontal;
            split.SplitterDistance = 260;
            split.Panel1.Padding = new Padding(0, 14, 0, 8);
            split.Panel2.Padding = new Padding(0, 8, 0, 0);

            loginCard.Dock = DockStyle.Left;
            loginCard.Width = 500;
            loginCard.BackColor = Color.White;
            loginCard.Padding = new Padding(18);
            loginCard.BorderStyle = BorderStyle.FixedSingle;

            loginTitle.AutoSize = true;
            loginTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            loginTitle.Location = new System.Drawing.Point(18, 14);
            loginTitle.Text = "Lịch sử đăng nhập";

            this._LoginSummaryLabel.AutoSize = false;
            this._LoginSummaryLabel.Location = new System.Drawing.Point(170, 15);
            this._LoginSummaryLabel.Size = new System.Drawing.Size(270, 20);
            this._LoginSummaryLabel.ForeColor = Color.FromArgb(100, 116, 139);
            this._LoginSummaryLabel.Text = "0 bản ghi";

            this._LoginGrid.Dock = DockStyle.Fill;
            this._LoginGrid.Location = new System.Drawing.Point(18, 44);
            this._LoginGrid.ReadOnly = true;
            this._LoginGrid.AllowUserToAddRows = false;
            this._LoginGrid.AllowUserToDeleteRows = false;
            this._LoginGrid.AutoGenerateColumns = false;
            this._LoginGrid.RowHeadersVisible = false;
            this._LoginGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._LoginGrid.DataSource = this._LoginHistory;
            this._LoginGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LoggedInAt", HeaderText = "Đăng nhập lúc", FillWeight = 120 });
            this._LoginGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Tên đăng nhập", FillWeight = 70 });
            this._LoginGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ và tên", FillWeight = 100 });
            this._LoginGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Role", HeaderText = "Vai trò", FillWeight = 50 });

            loginCard.Controls.Add(this._LoginGrid);
            loginCard.Controls.Add(loginTitle);
            loginCard.Controls.Add(this._LoginSummaryLabel);

            attendanceCard.Dock = DockStyle.Fill;
            attendanceCard.BackColor = Color.White;
            attendanceCard.Padding = new Padding(18);
            attendanceCard.BorderStyle = BorderStyle.FixedSingle;

            attendanceTitle.AutoSize = true;
            attendanceTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            attendanceTitle.Location = new System.Drawing.Point(18, 14);
            attendanceTitle.Text = "Lịch sử chấm công";

            this._AttendanceSummaryLabel.AutoSize = false;
            this._AttendanceSummaryLabel.Location = new System.Drawing.Point(182, 15);
            this._AttendanceSummaryLabel.Size = new System.Drawing.Size(280, 20);
            this._AttendanceSummaryLabel.ForeColor = Color.FromArgb(100, 116, 139);
            this._AttendanceSummaryLabel.Text = "0 bản ghi";

            this._AttendanceGrid.Dock = DockStyle.Fill;
            this._AttendanceGrid.Location = new System.Drawing.Point(18, 44);
            this._AttendanceGrid.ReadOnly = true;
            this._AttendanceGrid.AllowUserToAddRows = false;
            this._AttendanceGrid.AllowUserToDeleteRows = false;
            this._AttendanceGrid.AutoGenerateColumns = false;
            this._AttendanceGrid.RowHeadersVisible = false;
            this._AttendanceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._AttendanceGrid.DataSource = this._AttendanceSummaries;
            this._AttendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AttendanceDay", HeaderText = "Ngày", FillWeight = 70 });
            this._AttendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Tên đăng nhập", FillWeight = 70 });
            this._AttendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ và tên", FillWeight = 100 });
            this._AttendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckInAt", HeaderText = "Vào làm", FillWeight = 70 });
            this._AttendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckOutAt", HeaderText = "Tan làm", FillWeight = 70 });
            this._AttendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "WorkState", HeaderText = "Trạng thái", FillWeight = 60 });
            this._AttendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RecordCount", HeaderText = "Bản ghi", FillWeight = 45 });

            attendanceCard.Controls.Add(this._AttendanceGrid);
            attendanceCard.Controls.Add(attendanceTitle);
            attendanceCard.Controls.Add(this._AttendanceSummaryLabel);

            split.Panel1.Controls.Add(loginCard);
            split.Panel2.Controls.Add(attendanceCard);

            root.Controls.Add(split);
            root.Controls.Add(header);
            this.Controls.Add(root);
        }

        private async Task RefreshAsync()
        {
            if (AppDatabase.Repository == null)
            {
                this._StatusLabel.Text = "Cơ sở dữ liệu chưa được khởi tạo.";
                return;
            }

            this._StatusLabel.Text = "Đang tải...";
            this.Enabled = false;

            try
            {
                var loginHistory = await AppDatabase.Repository.GetRecentLoginHistoryAsync(200).ConfigureAwait(true);
                this._LoginHistory.RaiseListChangedEvents = false;
                this._LoginHistory.Clear();
                foreach (var item in loginHistory)
                    this._LoginHistory.Add(item);
                this._LoginHistory.RaiseListChangedEvents = true;
                this._LoginHistory.ResetBindings();
                this._LoginSummaryLabel.Text = $"{this._LoginHistory.Count} records";

                var attendance = await AppDatabase.Repository.GetAttendanceSummaryAsync(200).ConfigureAwait(true);
                this._AttendanceSummaries.RaiseListChangedEvents = false;
                this._AttendanceSummaries.Clear();
                foreach (var item in attendance)
                    this._AttendanceSummaries.Add(item);
                this._AttendanceSummaries.RaiseListChangedEvents = true;
                this._AttendanceSummaries.ResetBindings();
                this._AttendanceSummaryLabel.Text = $"{this._AttendanceSummaries.Count} records";

                this._StatusLabel.Text = "Đã tải xong.";
            }
            catch (Exception ex)
            {
                this._StatusLabel.Text = ex.Message;
            }
            finally
            {
                this.Enabled = true;
            }
        }
    }
}

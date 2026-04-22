using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp.UserControls
{
    public partial class UCSettings : UserControl
    {
        // ─── Controls ────────────────────────────────────────────────────────
        // Tab Users
        private DataGridView dgvUsers;
        private Button btnAddUser, btnResetPwd, btnToggleActive, btnRefreshUsers;
        private TextBox txtNewUsername, txtNewPassword;
        private ComboBox cboNewRole, cboUserEmployee;
        private Label lblUserInfo;

        // Tab System
        private Label lblDbStatus, lblAppVersion, lblDbServer, lblUserCount, lblCurrentUser;
        private Button btnTestConn, btnRefreshSys;

        // Tab Config (system_settings)
        private DataGridView dgvConfig;
        private Button btnSaveConfig, btnRefreshConfig;

        // Tab Audit Log
        private DataGridView dgvAudit;
        private ComboBox cboAuditTable;
        private Button btnRefreshAudit;

        // Tab Face Log
        private DataGridView dgvFaceLog;
        private ComboBox cboFaceLogEmp;
        private Button btnRefreshFaceLog;

        public UCSettings()
        {
            InitializeComponent();
            ConfigureTabHeaderStyle();
            BuildTabUsers();
            BuildTabSystem();
            BuildTabConfig();
            BuildTabAudit();
            BuildTabFaceLog();

            tabMain.SelectedIndexChanged += async (s, e) => await LoadCurrentTabAsync();
            tabMain.SelectedIndex = 0;

            // Async load
            LoadUsersAsync();
        }

        private void ConfigureTabHeaderStyle()
        {
            tabMain.Appearance = TabAppearance.FlatButtons;
            tabMain.SizeMode = TabSizeMode.Fixed;
            tabMain.ItemSize = new Size(170, 34);
            tabMain.Padding = new Point(18, 6);
        }

        // ─────────────────────────────────────────────────────────────────────
        // TAB: USERS
        // ─────────────────────────────────────────────────────────────────────
        private void BuildTabUsers()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };

            // Toolbar
            var toolbar = MakePanel(DockStyle.Top, 50);
            var flpToolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5, 7, 5, 5),
                BackColor = Color.Transparent
            };
            
            btnAddUser      = MakeBtn("➕ Thêm TK", Color.FromArgb(34, 197, 94)); btnAddUser.Width = 110;
            btnResetPwd     = MakeBtn("🔑 Reset PW", Color.FromArgb(59, 130, 246)); btnResetPwd.Width = 115;
            btnToggleActive = MakeBtn("🔒 Kích hoạt", Color.FromArgb(234, 179, 8)); btnToggleActive.Width = 115;
            btnRefreshUsers = MakeBtn("🔄 Làm mới", Color.FromArgb(107, 114, 128)); btnRefreshUsers.Width = 100;
            
            flpToolbar.Controls.AddRange(new Control[] { btnAddUser, btnResetPwd, btnToggleActive, btnRefreshUsers });
            toolbar.Controls.Add(flpToolbar);

            // Form thêm
            var pnlForm = MakePanel(DockStyle.Top, 100);
            var tlpForm = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(5)
            };
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            
            txtNewUsername = MakeTxt(); txtNewUsername.Dock = DockStyle.Top;
            SetWatermark(txtNewUsername, "Tên đăng nhập");
            
            txtNewPassword = MakeTxt(); txtNewPassword.Dock = DockStyle.Top; txtNewPassword.UseSystemPasswordChar = true;
            SetWatermark(txtNewPassword, "Mật khẩu");
            
            cboNewRole = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            cboNewRole.Items.AddRange(new[] { "Admin", "Manager", "Employee" });
            cboNewRole.SelectedIndex = 2;
            
            cboUserEmployee = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
            
            lblUserInfo = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = Color.FromArgb(71, 85, 105), TextAlign = ContentAlignment.MiddleLeft };
            
            tlpForm.Controls.Add(MakeLbl("Tên TK:", Point.Empty), 0, 0);
            tlpForm.Controls.Add(MakeLbl("Mật khẩu:", Point.Empty), 1, 0);
            tlpForm.Controls.Add(MakeLbl("Vai trò:", Point.Empty), 2, 0);
            tlpForm.Controls.Add(MakeLbl("Nhân viên:", Point.Empty), 3, 0);
            
            tlpForm.Controls.Add(txtNewUsername, 0, 1);
            tlpForm.Controls.Add(txtNewPassword, 1, 1);
            tlpForm.Controls.Add(cboNewRole, 2, 1);
            tlpForm.Controls.Add(cboUserEmployee, 3, 1);
            
            var pnlInfo = new Panel { Dock = DockStyle.Bottom, Height = 30, Padding = new Padding(10, 0, 0, 0) };
            pnlInfo.Controls.Add(lblUserInfo);

            pnlForm.Controls.Clear();
            pnlForm.Controls.Add(tlpForm);
            pnlForm.Controls.Add(pnlInfo);

            // Grid
            dgvUsers = MakeGrid();
            dgvUsers.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="UId",       HeaderText="ID",         Width=45  },
                new DataGridViewTextBoxColumn { Name="UName",     HeaderText="Tên TK",     Width=130 },
                new DataGridViewTextBoxColumn { Name="URole",     HeaderText="Vai trò",    Width=90  },
                new DataGridViewTextBoxColumn { Name="UEmpId",    HeaderText="NV ID",      Width=60  },
                new DataGridViewTextBoxColumn { Name="ULastLogin",HeaderText="Đăng nhập cuối",Width=140},
                new DataGridViewTextBoxColumn { Name="UActive",   HeaderText="Trạng thái", Width=90  },
                new DataGridViewTextBoxColumn { Name="UFailed",   HeaderText="Lỗi đăng nhập",Width=80}
            });
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.SelectionChanged += async (s, e) =>
            {
                if (dgvUsers.SelectedRows.Count == 0) return;
                var uid = dgvUsers.SelectedRows[0].Cells["UId"].Value?.ToString();
                var uname = dgvUsers.SelectedRows[0].Cells["UName"].Value?.ToString();
                var urole = dgvUsers.SelectedRows[0].Cells["URole"].Value?.ToString();
                lblUserInfo.Text = $"Đã chọn: [{uid}] {uname} – Vai trò: {urole}";
            };

            // Events
            btnAddUser.Click      += BtnAddUser_Click;
            btnResetPwd.Click     += BtnResetPwd_Click;
            btnToggleActive.Click += BtnToggleActive_Click;
            btnRefreshUsers.Click += async (s, e) => await LoadUsersAsync();

            pnl.Controls.Add(dgvUsers);
            pnl.Controls.Add(pnlForm);
            pnl.Controls.Add(toolbar);
            tabUsers.Controls.Add(pnl);
        }

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            try
            {
                // Load employee dropdown
                var emps = await AppDatabase.Repository.GetEmployeesAsync(true);
                cboUserEmployee.Items.Clear();
                cboUserEmployee.Items.Add("(Không gán NV)");
                foreach (var e in emps)
                    cboUserEmployee.Items.Add($"{e.Id}: {e.Code} {e.FullName}");
                cboUserEmployee.SelectedIndex = 0;

                var users = await AppDatabase.Repository.GetUsersAsync();
                dgvUsers.Rows.Clear();
                foreach (var u in users)
                {
                    var rowIdx = dgvUsers.Rows.Add(
                        u.Id, u.Username, u.Role,
                        u.EmployeeId.HasValue ? u.EmployeeId.ToString() : "—",
                        u.LastLogin?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                        u.IsActive ? "✅ Hoạt động" : "🔒 Bị khóa",
                        u.FailedLoginCount);
                    if (!u.IsActive)
                        dgvUsers.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(160, 160, 160);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadUsers error: {ex.Message}");
            }
        }

        private async void BtnAddUser_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewUsername.Text) || string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Nhập đủ tên tài khoản và mật khẩu!", "Thông báo");
                return;
            }
            try
            {
                int? empId = null;
                if (cboUserEmployee.SelectedIndex > 0)
                    empId = int.Parse(cboUserEmployee.SelectedItem.ToString().Split(':')[0].Trim());

                string hash = AuthPasswordHasher.Hash(txtNewPassword.Text);
                string username = txtNewUsername.Text.Trim();
                string role = cboNewRole.SelectedItem?.ToString() ?? "Employee";
                await AppDatabase.Repository.CreateUserAsync(username, hash, empId, role);
                txtNewUsername.Text = "";
                txtNewPassword.Text = "";
                MessageBox.Show("✅ Tạo tài khoản thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadUsersAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        private async void BtnResetPwd_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) { MessageBox.Show("Chọn tài khoản trước!"); return; }
            var uid = (int)dgvUsers.SelectedRows[0].Cells["UId"].Value;
            var name = dgvUsers.SelectedRows[0].Cells["UName"].Value?.ToString();
            var newPwd = Microsoft.VisualBasic.Interaction.InputBox($"Nhập mật khẩu mới cho [{name}]:", "Reset mật khẩu", "");
            if (string.IsNullOrWhiteSpace(newPwd)) return;
            try
            {
                await AppDatabase.Repository.ResetUserPasswordAsync(uid, AuthPasswordHasher.Hash(newPwd));
                MessageBox.Show("✅ Reset mật khẩu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        private async void BtnToggleActive_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) { MessageBox.Show("Chọn tài khoản trước!"); return; }
            var uid = (int)dgvUsers.SelectedRows[0].Cells["UId"].Value;
            var isActive = dgvUsers.SelectedRows[0].Cells["UActive"].Value?.ToString().Contains("Hoạt động") == true;
            var role = dgvUsers.SelectedRows[0].Cells["URole"].Value?.ToString() ?? "Employee";
            var empIdStr = dgvUsers.SelectedRows[0].Cells["UEmpId"].Value?.ToString();
            int? empId = int.TryParse(empIdStr, out var eid) ? eid : (int?)null;
            try
            {
                await AppDatabase.Repository.UpdateUserAsync(uid, role, !isActive, empId);
                MessageBox.Show(isActive ? "Đã khóa tài khoản." : "Đã kích hoạt tài khoản.", "Thành công");
                await LoadUsersAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // TAB: SYSTEM INFO
        // ─────────────────────────────────────────────────────────────────────
        private void BuildTabSystem()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var infoPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(5),
                ColumnCount = 2, RowCount = 6
            };
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            lblDbStatus    = MakeSysLabel("—");
            lblAppVersion  = MakeSysLabel("—");
            lblDbServer    = MakeSysLabel("—");
            lblUserCount   = MakeSysLabel("—");
            lblCurrentUser = MakeSysLabel("—");

            infoPanel.Controls.Add(MakeSysKey("Phiên bản App:"),  0, 0); infoPanel.Controls.Add(lblAppVersion,  1, 0);
            infoPanel.Controls.Add(MakeSysKey("Server DB:"),      0, 1); infoPanel.Controls.Add(lblDbServer,    1, 1);
            infoPanel.Controls.Add(MakeSysKey("Kết nối DB:"),     0, 2); infoPanel.Controls.Add(lblDbStatus,    1, 2);
            infoPanel.Controls.Add(MakeSysKey("Tổng TK:"),        0, 3); infoPanel.Controls.Add(lblUserCount,   1, 3);
            infoPanel.Controls.Add(MakeSysKey("Người dùng:"),     0, 4); infoPanel.Controls.Add(lblCurrentUser, 1, 4);

            btnTestConn   = MakeBtn("🔌 Kiểm tra kết nối", Color.FromArgb(59, 130, 246));
            btnRefreshSys = MakeBtn("🔄 Làm mới", Color.FromArgb(107, 114, 128));
            btnTestConn.Location   = new Point(5, 220); btnTestConn.Width = 180;
            btnRefreshSys.Location = new Point(195, 220);

            btnTestConn.Click   += BtnTestConn_Click;
            btnRefreshSys.Click += async (s, e) => await LoadSystemInfoAsync();

            pnl.Controls.Add(infoPanel);
            pnl.Controls.Add(btnRefreshSys);
            pnl.Controls.Add(btnTestConn);
            tabSystem.Controls.Add(pnl);
        }

        private async System.Threading.Tasks.Task LoadSystemInfoAsync()
        {
            lblAppVersion.Text  = "FaceIDApp v1.0 (.NET 4.8 / WinForms)";
            lblDbServer.Text    = AppDatabase.Config?.ApplicationConnectionString?.Split(';')[0] ?? "—";
            lblCurrentUser.Text = AppSession.CurrentUser != null
                ? $"{AppSession.CurrentUser.Username} ({AppSession.CurrentUser.Role})"
                : "—";
            try
            {
                var users = await AppDatabase.Repository.GetUsersAsync();
                lblUserCount.Text = users.Count.ToString();

                bool ok = await AppDatabase.Repository.TestConnectionAsync();
                lblDbStatus.Text      = ok ? "✅ Kết nối thành công" : "❌ Không kết nối";
                lblDbStatus.ForeColor = ok ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 68, 68);
            }
            catch (Exception ex)
            {
                lblDbStatus.Text      = $"❌ {ex.Message}";
                lblDbStatus.ForeColor = Color.FromArgb(239, 68, 68);
            }
        }

        private async void BtnTestConn_Click(object sender, EventArgs e)
        {
            btnTestConn.Enabled = false;
            lblDbStatus.Text = "⏳ Đang kiểm tra...";
            await LoadSystemInfoAsync();
            btnTestConn.Enabled = true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // TAB: CONFIG (system_settings)
        // ─────────────────────────────────────────────────────────────────────
        private void BuildTabConfig()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };

            var toolbar = MakePanel(DockStyle.Top, 50);
            var flpToolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5, 7, 5, 5),
                BackColor = Color.Transparent
            };

            btnSaveConfig   = MakeBtn("💾 Lưu thay đổi", Color.FromArgb(34, 197, 94)); btnSaveConfig.Width = 140;
            btnRefreshConfig = MakeBtn("🔄 Làm mới", Color.FromArgb(107, 114, 128)); btnRefreshConfig.Width = 100;
            
            var lblHint = new Label { Text = "Nhấp đúp vào ô Giá trị để sửa trực tiếp. Nhấn Lưu để cập nhật DB.", Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = Color.FromArgb(107, 114, 128), AutoSize = true, Margin = new Padding(10, 8, 0, 0) };
            
            flpToolbar.Controls.AddRange(new Control[] { btnSaveConfig, btnRefreshConfig, lblHint });
            toolbar.Controls.Add(flpToolbar);

            dgvConfig = MakeGrid();
            dgvConfig.ReadOnly = false;
            dgvConfig.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="CfgKey",   HeaderText="Khóa",           Width=200, ReadOnly=true  },
                new DataGridViewTextBoxColumn { Name="CfgVal",   HeaderText="Giá trị",         Width=250 },
                new DataGridViewTextBoxColumn { Name="CfgType",  HeaderText="Kiểu",            Width=80,  ReadOnly=true },
                new DataGridViewTextBoxColumn { Name="CfgDesc",  HeaderText="Mô tả",           Width=300, ReadOnly=true}
            });
            dgvConfig.Dock = DockStyle.Fill;
            dgvConfig.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(71, 85, 105);
            dgvConfig.EditingControlShowing += (s, e) =>
            {
                // Highlight khi đang sửa
                if (e.Control is TextBox tb)
                    tb.BackColor = Color.FromArgb(255, 251, 210);
            };

            btnSaveConfig.Click   += BtnSaveConfig_Click;
            btnRefreshConfig.Click += async (s, e) => await LoadConfigAsync();

            pnl.Controls.Add(dgvConfig);
            pnl.Controls.Add(toolbar);
            tabConfig.Controls.Add(pnl);
        }

        private async System.Threading.Tasks.Task LoadConfigAsync()
        {
            try
            {
                var settings = await AppDatabase.Repository.GetSystemSettingsAsync();
                dgvConfig.Rows.Clear();
                foreach (var s in settings)
                    dgvConfig.Rows.Add(s.Key, s.Value ?? "", s.DataType ?? "string", s.Description ?? "");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadConfig error: {ex.Message}");
            }
        }

        private async void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dgvConfig.Rows)
                {
                    var key = row.Cells["CfgKey"].Value?.ToString();
                    var val = row.Cells["CfgVal"].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(key))
                        await AppDatabase.Repository.UpsertSystemSettingAsync(key, val);
                }
                MessageBox.Show("✅ Đã lưu tất cả cài đặt!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // TAB: AUDIT LOG
        // ─────────────────────────────────────────────────────────────────────
        private DateTimePicker dtpAuditFrom, dtpAuditTo;
        private ComboBox cboAuditAction;

        private void BuildTabAudit()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };

            var toolbar = MakePanel(DockStyle.Top, 85);
            var flpTop = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(5, 5, 5, 0), BackColor = Color.Transparent };
            var flpBottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(5, 0, 5, 5), BackColor = Color.Transparent };

            btnRefreshAudit = MakeBtn("🔄 Tải nhật ký", Color.FromArgb(59, 130, 246)); btnRefreshAudit.Width = 130;
            
            cboAuditTable = new ComboBox { Size = new Size(150, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(5, 5, 5, 0) };
            cboAuditTable.Items.AddRange(new[] { "(Tất cả bảng)", "employees", "attendance_records", "leave_requests", "users", "work_shifts", "departments" });
            cboAuditTable.SelectedIndex = 0;
            
            cboAuditAction = new ComboBox { Size = new Size(110, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(5, 5, 5, 0) };
            cboAuditAction.Items.AddRange(new[] { "(Tất cả)", "INSERT", "UPDATE", "DELETE" });
            cboAuditAction.SelectedIndex = 0;
            
            var lblLimit = new Label { Text = "Hiển thị tối đa 500 bản ghi", ForeColor = Color.FromArgb(107, 114, 128), Font = new Font("Segoe UI", 9F, FontStyle.Italic), AutoSize = true, Margin = new Padding(10, 10, 0, 0) };

            flpTop.Controls.AddRange(new Control[] { btnRefreshAudit, cboAuditTable, cboAuditAction, lblLimit });

            // Date range
            var lblFrom = new Label { Text = "Từ:", Font = new Font("Segoe UI", 9F), AutoSize = true, Margin = new Padding(5, 10, 0, 0) };
            dtpAuditFrom = new DateTimePicker { Size = new Size(145, 26), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Font = new Font("Segoe UI", 9F), Margin = new Padding(5, 5, 5, 0) };
            var lblTo = new Label { Text = "Đến:", Font = new Font("Segoe UI", 9F), AutoSize = true, Margin = new Padding(15, 10, 0, 0) };
            dtpAuditTo = new DateTimePicker { Size = new Size(145, 26), Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = new Font("Segoe UI", 9F), Margin = new Padding(5, 5, 5, 0) };
            
            flpBottom.Controls.AddRange(new Control[] { lblFrom, dtpAuditFrom, lblTo, dtpAuditTo });
            
            toolbar.Controls.Add(flpBottom);
            toolbar.Controls.Add(flpTop);

            dgvAudit = MakeGrid();
            dgvAudit.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvAudit.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="ATime",   HeaderText="Thời gian",  Width=140 },
                new DataGridViewTextBoxColumn { Name="AUser",   HeaderText="Người dùng", Width=110 },
                new DataGridViewTextBoxColumn { Name="AAction", HeaderText="Hành động",  Width=80  },
                new DataGridViewTextBoxColumn { Name="ATable",  HeaderText="Bảng",       Width=120 },
                new DataGridViewTextBoxColumn { Name="ARec",    HeaderText="Record ID",  Width=70  },
                new DataGridViewTextBoxColumn { Name="AOld",    HeaderText="Giá trị cũ", Width=200 },
                new DataGridViewTextBoxColumn { Name="ANew",    HeaderText="Giá trị mới",Width=200 },
                new DataGridViewTextBoxColumn { Name="AIp",     HeaderText="IP",         Width=110 }
            });
            dgvAudit.Dock = DockStyle.Fill;
            btnRefreshAudit.Click += async (s, e) => await LoadAuditAsync();

            pnl.Controls.Add(dgvAudit);
            pnl.Controls.Add(toolbar);
            tabAudit.Controls.Add(pnl);
        }

        private async System.Threading.Tasks.Task LoadAuditAsync()
        {
            try
            {
                var tableName = cboAuditTable.SelectedIndex > 0 ? cboAuditTable.SelectedItem.ToString() : null;
                var logs = await AppDatabase.Repository.GetAuditLogsAsync(500, tableName);
                dgvAudit.Rows.Clear();
                foreach (var l in logs)
                {
                    var rowIdx = dgvAudit.Rows.Add(
                        l.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        l.Username ?? (l.UserId.HasValue ? $"#{l.UserId}" : "System"),
                        l.Action, l.TableName,
                        l.RecordId != null ? l.RecordId : "—",
                        Truncate(l.OldValues, 60),
                        Truncate(l.NewValues, 60),
                        l.IpAddress ?? "—");

                    // Tô màu
                    var cell = dgvAudit.Rows[rowIdx].Cells["AAction"];
                    switch (l.Action?.ToUpper())
                    {
                        case "INSERT": cell.Style.ForeColor = Color.FromArgb(34, 197, 94);  break;
                        case "UPDATE": cell.Style.ForeColor = Color.FromArgb(234, 179, 8);  break;
                        case "DELETE": cell.Style.ForeColor = Color.FromArgb(239, 68, 68);  break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải audit log: {ex.Message}", "Lỗi");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // TAB: FACE REGISTRATION LOG
        // ─────────────────────────────────────────────────────────────────────
        private void BuildTabFaceLog()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };

            var toolbar = MakePanel(DockStyle.Top, 50);
            var flpToolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5, 7, 5, 5),
                BackColor = Color.Transparent
            };
            
            btnRefreshFaceLog = MakeBtn("🔄 Tải nhật ký", Color.FromArgb(59, 130, 246)); btnRefreshFaceLog.Width = 130;
            cboFaceLogEmp = new ComboBox { Size = new Size(220, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(5, 5, 5, 0) };
            cboFaceLogEmp.Items.Add("(Tất cả nhân viên)");
            cboFaceLogEmp.SelectedIndex = 0;
            
            flpToolbar.Controls.AddRange(new Control[] { btnRefreshFaceLog, cboFaceLogEmp });
            toolbar.Controls.Add(flpToolbar);

            dgvFaceLog = MakeGrid();
            dgvFaceLog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(168, 85, 247);
            dgvFaceLog.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="FLTime",    HeaderText="Thời gian",     Width=140 },
                new DataGridViewTextBoxColumn { Name="FLEmp",     HeaderText="Nhân viên",     Width=160 },
                new DataGridViewTextBoxColumn { Name="FLAction",  HeaderText="Hành động",     Width=90  },
                new DataGridViewTextBoxColumn { Name="FLFaceId",  HeaderText="Face Data ID",  Width=90  },
                new DataGridViewTextBoxColumn { Name="FLBy",      HeaderText="Thực hiện bởi", Width=140 },
                new DataGridViewTextBoxColumn { Name="FLReason",  HeaderText="Lý do",         Width=200 }
            });
            dgvFaceLog.Dock = DockStyle.Fill;

            btnRefreshFaceLog.Click += async (s, e) => await LoadFaceLogAsync();

            pnl.Controls.Add(dgvFaceLog);
            pnl.Controls.Add(toolbar);
            tabFaceLog.Controls.Add(pnl);
        }

        private async System.Threading.Tasks.Task LoadFaceLogAsync()
        {
            try
            {
                // Load employees cho filter nếu chưa có
                if (cboFaceLogEmp.Items.Count == 1)
                {
                    var emps = await AppDatabase.Repository.GetEmployeesAsync(true);
                    foreach (var emp in emps)
                        cboFaceLogEmp.Items.Add($"{emp.Id}: {emp.Code} – {emp.FullName}");
                }

                int? empId = null;
                if (cboFaceLogEmp.SelectedIndex > 0)
                    empId = int.Parse(cboFaceLogEmp.SelectedItem.ToString().Split(':')[0].Trim());

                var logs = await AppDatabase.Repository.GetFaceRegistrationLogsAsync(empId);
                dgvFaceLog.Rows.Clear();
                foreach (var l in logs)
                {
                    var rowIdx = dgvFaceLog.Rows.Add(
                        l.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        l.EmployeeName ?? l.EmployeeId.ToString(),
                        l.Action,
                        l.FaceDataId.HasValue ? l.FaceDataId.ToString() : "—",
                        l.PerformedByName ?? (l.PerformedBy.HasValue ? $"#{l.PerformedBy}" : "System"),
                        l.Reason ?? "");

                    var cell = dgvFaceLog.Rows[rowIdx].Cells["FLAction"];
                    switch (l.Action?.ToUpper())
                    {
                        case "REGISTER": cell.Style.ForeColor = Color.FromArgb(34, 197, 94);  break;
                        case "DELETE":   cell.Style.ForeColor = Color.FromArgb(239, 68, 68);  break;
                        case "UPDATE":   cell.Style.ForeColor = Color.FromArgb(234, 179, 8);  break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải face log: {ex.Message}", "Lỗi");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tab change → lazy load
        // ─────────────────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadCurrentTabAsync()
        {
            switch (tabMain.SelectedIndex)
            {
                case 0: await LoadUsersAsync();    break;
                case 1: await LoadSystemInfoAsync(); break;
                case 2: await LoadConfigAsync();   break;
                case 3: await LoadAuditAsync();    break;
                case 4: await LoadFaceLogAsync();  break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper factories
        // ─────────────────────────────────────────────────────────────────────
        private static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 32, RowTemplate = { Height = 28 },
                Font = new Font("Segoe UI", 9F),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(226, 232, 240)
            };
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            return g;
        }

        private static Button MakeBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = bg,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Size = new Size(110, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static Panel MakePanel(DockStyle dock, int height)
        {
            return new Panel
            {
                Dock = dock, Height = height,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(5, 5, 5, 5)
            };
        }

        private static TextBox MakeTxt() => new TextBox { Font = new Font("Segoe UI", 9.5F) };

        private static Label MakeLbl(string text, Point loc)
            => new Label { Text = text, Location = loc, AutoSize = true, Font = new Font("Segoe UI", 9F) };

        private static Label MakeSysKey(string text) => new Label
        {
            Text = text, AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            Padding = new Padding(0, 6, 0, 6)
        };

        private static Label MakeSysLabel(string text) => new Label
        {
            Text = text, AutoSize = true,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(15, 23, 42),
            Padding = new Padding(0, 6, 0, 6)
        };

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length > max ? s.Substring(0, max) + "…" : s;
        }

        private static void SetWatermark(TextBox txt, string watermark)
        {
            txt.Text = watermark;
            txt.ForeColor = Color.Gray;
            txt.GotFocus += (s, e) => { if (txt.ForeColor == Color.Gray) { txt.Text = ""; txt.ForeColor = SystemColors.WindowText; } };
            txt.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = watermark; txt.ForeColor = Color.Gray; } };
        }
    }
}

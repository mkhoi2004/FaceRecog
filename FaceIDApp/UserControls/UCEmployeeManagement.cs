using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp.UserControls
{
    public partial class UCEmployeeManagement : UserControl
    {
        // =============================================
        // State
        // =============================================
        private int? _editingEmployeeId = null;
        private List<DepartmentDto> _departments = new List<DepartmentDto>();
        private List<PositionDto> _positions = new List<PositionDto>();
        private List<WorkShiftDto> _shifts = new List<WorkShiftDto>();
        private List<EmployeeDto> _employees = new List<EmployeeDto>();

        // Extra form controls added in code (vì Designer chỉ có txtPosition)
        private ComboBox cboPosition;
        private ComboBox cboShift;
        private ComboBox cboEmploymentType;
        private NumericUpDown nudAnnualLeave;
        private TextBox txtIdentityCard;
        private ComboBox cboGender;
        private DataGridView dgvAttHistory;
        private ComboBox cboManager;
        private Label lblLeaveBalance;

        public UCEmployeeManagement()
        {
            InitializeComponent();
            ExtendDetailPanel();
            SetupUI();
            RefreshData();

            // Responsive layout for variable host sizes.
            this.Resize += (s, e) => ApplyResponsiveLayout();
            pnlContent.Resize += (s, e) => ApplyResponsiveLayout();
            pnlToolbar.Resize += (s, e) => ApplyToolbarLayout();
            pnlEmployeeDetail.Resize += (s, e) => ApplyDetailPanelLayout();
            ApplyResponsiveLayout();
        }

        // =============================================
        // Mở rộng panel chi tiết với các trường còn thiếu
        // =============================================
        private void ExtendDetailPanel()
        {
            // Ẩn txtPosition cũ, thay bằng cboPosition
            txtPosition.Visible = false;

            // ComboBox chức vụ
            var lblPos2 = new Label { Text = "Chức vụ:", Font = new Font("Segoe UI", 9.5F), Location = new Point(15, 265), AutoSize = true };
            cboPosition = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(100, 262),
                Size = new Size(210, 25),
                Name = "cboPosition"
            };
            pnlEmployeeDetail.Controls.Add(lblPos2);
            pnlEmployeeDetail.Controls.Add(cboPosition);

            // Ca mặc định
            var lblShift = new Label { Text = "Ca làm:", Font = new Font("Segoe UI", 9.5F), Location = new Point(15, 295), AutoSize = true };
            cboShift = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(100, 292),
                Size = new Size(210, 25),
                Name = "cboShift"
            };
            pnlEmployeeDetail.Controls.Add(lblShift);
            pnlEmployeeDetail.Controls.Add(cboShift);

            // Loại hợp đồng
            var lblEmpType = new Label { Text = "Loại HĐ:", Font = new Font("Segoe UI", 9.5F), Location = new Point(15, 325), AutoSize = true };
            cboEmploymentType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(100, 322),
                Size = new Size(210, 25)
            };
            cboEmploymentType.Items.AddRange(new[] { "FullTime", "PartTime", "Contract", "Intern", "Probation" });
            cboEmploymentType.SelectedIndex = 0;
            pnlEmployeeDetail.Controls.Add(lblEmpType);
            pnlEmployeeDetail.Controls.Add(cboEmploymentType);

            // Số ngày phép năm
            var lblLeave = new Label { Text = "Phép/năm:", Font = new Font("Segoe UI", 9.5F), Location = new Point(15, 355), AutoSize = true };
            nudAnnualLeave = new NumericUpDown
            {
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(100, 352),
                Size = new Size(80, 24),
                Minimum = 0, Maximum = 365,
                Value = 12
            };
            pnlEmployeeDetail.Controls.Add(lblLeave);
            pnlEmployeeDetail.Controls.Add(nudAnnualLeave);

            // CMND/CCCD
            var lblId = new Label { Text = "CMND/CC:", Font = new Font("Segoe UI", 9.5F), Location = new Point(15, 385), AutoSize = true };
            txtIdentityCard = new TextBox { Font = new Font("Segoe UI", 9.5F), Location = new Point(100, 382), Size = new Size(210, 24) };
            pnlEmployeeDetail.Controls.Add(lblId);
            pnlEmployeeDetail.Controls.Add(txtIdentityCard);

            // Giới tính
            var lblGender = new Label { Text = "Giới tính:", Font = new Font("Segoe UI", 9.5F), Location = new Point(15, 413), AutoSize = true };
            cboGender = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(100, 410),
                Size = new Size(120, 25)
            };
            cboGender.Items.Add(new ComboItem<string>("M", "Nam"));
            cboGender.Items.Add(new ComboItem<string>("F", "Nữ"));
            cboGender.Items.Add(new ComboItem<string>("O", "Khác"));
            cboGender.SelectedIndex = 0;
            pnlEmployeeDetail.Controls.Add(lblGender);
            pnlEmployeeDetail.Controls.Add(cboGender);

            // Quản lý trực tiếp
            var lblMgr = new Label { Text = "Quản lý:", Font = new Font("Segoe UI", 9.5F), Location = new Point(15, 441), AutoSize = true };
            cboManager = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(100, 438),
                Size = new Size(210, 25),
                Name = "cboManager"
            };
            pnlEmployeeDetail.Controls.Add(lblMgr);
            pnlEmployeeDetail.Controls.Add(cboManager);

            // Số ngày phép còn lại
            lblLeaveBalance = new Label
            {
                Text = "Phép còn: — ngày",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(190, 355),
                AutoSize = true
            };
            pnlEmployeeDetail.Controls.Add(lblLeaveBalance);

            // Dời chkIsActive + Email + Phone + DatePicker xuống thấp hơn
            lblEmail.Location    = new Point(lblEmail.Location.X, 471);
            txtEmail.Location    = new Point(txtEmail.Location.X, 468);
            lblPhone.Location    = new Point(lblPhone.Location.X, 498);
            txtPhone.Location    = new Point(txtPhone.Location.X, 495);
            lblDateOfBirth.Location = new Point(lblDateOfBirth.Location.X, 525);
            dtpDateOfBirth.Location = new Point(dtpDateOfBirth.Location.X, 522);
            lblHireDate.Location = new Point(lblHireDate.Location.X, 551);
            dtpHireDate.Location = new Point(dtpHireDate.Location.X, 548);
            chkIsActive.Location  = new Point(chkIsActive.Location.X, 576);
            btnSave.Location      = new Point(btnSave.Location.X, 605);
            btnCancel.Location    = new Point(btnCancel.Location.X, 605);
            btnRegisterFace.Location = new Point(btnRegisterFace.Location.X, 605);

            pnlEmployeeDetail.AutoScroll = true;

            // Tab xem lịch sử chấm công bên dưới dgvEmployees
            dgvAttHistory = new DataGridView
            {
                Dock = DockStyle.Bottom, Height = 180,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 32, RowTemplate = { Height = 28 },
                Font = new Font("Segoe UI", 9F),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(226, 232, 240)
            };
            dgvAttHistory.EnableHeadersVisualStyles = false;
            dgvAttHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvAttHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvAttHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvAttHistory.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 41, 59);
            dgvAttHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvAttHistory.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvAttHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvAttHistory.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "AttDate",   HeaderText = "Ngày",     Width = 90  },
                new DataGridViewTextBoxColumn { Name = "CheckIn",   HeaderText = "Vào",      Width = 70  },
                new DataGridViewTextBoxColumn { Name = "CheckOut",  HeaderText = "Ra",       Width = 70  },
                new DataGridViewTextBoxColumn { Name = "AttStatus", HeaderText = "Trạng thái", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "LateMin",   HeaderText = "Trễ(ph)",  Width = 60  },
                new DataGridViewTextBoxColumn { Name = "WorkMin",   HeaderText = "Làm(ph)",  Width = 65  },
                new DataGridViewTextBoxColumn { Name = "Method",    HeaderText = "PP",       Width = 60  },
                new DataGridViewTextBoxColumn { Name = "Note",      HeaderText = "Ghi chú",  Width = 120 }
            });

            // Label tiêu đề lịch sử
            var lblHist = new Label
            {
                Text = "📅 Lịch sử chấm công (30 ngày gần nhất)",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Bottom, Height = 22,
                BackColor = Color.FromArgb(241, 245, 249),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            pnlContent.Panel1.Controls.Add(dgvAttHistory);
            pnlContent.Panel1.Controls.Add(lblHist);
        }

        // =============================================
        // Setup UI events
        // =============================================
        private void SetupUI()
        {
            // Fix Title Spacing Issue
            lblTitle.AutoSize = false;
            lblTitle.Size = new Size(800, 45);
            lblTitle.Text = "👥 Quản lý nhân viên";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(30, 41, 59); // Slate 800
            lblTitle.UseCompatibleTextRendering = true;

            // Standardize Toolbar Buttons
            var toolbarButtons = new[] { btnAdd, btnEdit, btnDelete, btnRefresh };
            foreach (var btn in toolbarButtons)
            {
                btn.Height = 36;
                btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Cursor = Cursors.Hand;
            }

            btnAdd.Text = "➕ Thêm";
            btnAdd.Width = 90;
            
            btnEdit.Text = "✏️ Sửa";
            btnEdit.Width = 80;
            
            btnDelete.Text = "🗑️ Xóa";
            btnDelete.Width = 80;
            
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.Width = 110;

            // Grid styling
            dgvEmployees.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvEmployees.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvEmployees.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvEmployees.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; dgvEmployees.ColumnHeadersHeight = 35;
            dgvEmployees.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvEmployees.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvEmployees.RowTemplate.Height = 30;
            dgvEmployees.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 250);

            btnAdd.Click           += BtnAdd_Click;
            btnEdit.Click          += BtnEdit_Click;
            btnDelete.Click        += BtnDelete_Click;
            btnRefresh.Click       += (s, e) => RefreshData();
            btnSearch.Click        += BtnSearch_Click;
            txtSearch.KeyPress     += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnSearch_Click(null, null); };
            btnSave.Click          += BtnSave_Click;
            btnCancel.Click        += BtnCancel_Click;
            btnRegisterFace.Click  += BtnRegisterFace_Click;
            dgvEmployees.SelectionChanged += DgvEmployees_SelectionChanged;
            cboFilterDepartment.SelectedIndexChanged += async (s, e) => await FilterByDepartmentAsync();

            pnlContent.Panel1MinSize = 520;
            pnlContent.Panel2MinSize = 360;
        }

        private void ApplyResponsiveLayout()
        {
            ApplyToolbarLayout();

            if (pnlContent.Width > 0)
            {
                var preferredRightWidth = Math.Max(380, Math.Min(460, pnlContent.Width / 2));
                var splitter = pnlContent.Width - preferredRightWidth - pnlContent.SplitterWidth;

                splitter = Math.Max(pnlContent.Panel1MinSize, splitter);
                splitter = Math.Min(pnlContent.Width - pnlContent.Panel2MinSize - pnlContent.SplitterWidth, splitter);

                if (splitter > 0)
                    pnlContent.SplitterDistance = splitter;
            }

            ApplyDetailPanelLayout();
        }

        private void ApplyToolbarLayout()
        {
            // Left buttons alignment
            var x = 15;
            btnAdd.Left = x; x += btnAdd.Width + 8;
            btnEdit.Left = x; x += btnEdit.Width + 8;
            btnDelete.Left = x; x += btnDelete.Width + 8;
            btnRefresh.Left = x;

            // Right controls alignment
            var rightPadding = 15;
            cboFilterDepartment.Left = pnlToolbar.ClientSize.Width - cboFilterDepartment.Width - rightPadding;
            btnSearch.Left = cboFilterDepartment.Left - btnSearch.Width - 8;
            txtSearch.Left = btnSearch.Left - txtSearch.Width - 8;

            var minSearchLeft = btnRefresh.Right + 15;
            if (txtSearch.Left < minSearchLeft)
            {
                txtSearch.Width = Math.Max(120, txtSearch.Width - (minSearchLeft - txtSearch.Left));
                txtSearch.Left = minSearchLeft;
            }
        }

        private void ApplyDetailPanelLayout()
        {
            var fieldLeft = 100;
            var rightPadding = 20;
            var available = pnlEmployeeDetail.ClientSize.Width - fieldLeft - rightPadding;
            if (available < 170)
                available = 170;

            SetFieldWidth(txtEmployeeCode, available);
            SetFieldWidth(txtFullName, available);
            SetFieldWidth(cboDepartment, available);
            SetFieldWidth(cboPosition, available);
            SetFieldWidth(cboShift, available);
            SetFieldWidth(cboEmploymentType, available);
            SetFieldWidth(txtIdentityCard, available);
            SetFieldWidth(cboManager, available);
            SetFieldWidth(txtEmail, available);
            SetFieldWidth(txtPhone, available);
            SetFieldWidth(dtpDateOfBirth, available);
            SetFieldWidth(dtpHireDate, available);

            if (cboGender != null)
                cboGender.Width = Math.Min(120, available);

            if (nudAnnualLeave != null)
                nudAnnualLeave.Width = Math.Min(90, available);

            // Keep avatar centered relative to form field column.
            picEmployeePhoto.Left = fieldLeft + Math.Max(0, (available - picEmployeePhoto.Width) / 2);

            // Keep action buttons visible on narrow widths.
            const int saveDefaultWidth = 90;
            const int cancelDefaultWidth = 90;
            const int registerDefaultWidth = 100;
            var gap = 8;
            var rowWidth = saveDefaultWidth + cancelDefaultWidth + registerDefaultWidth + (gap * 2);
            if (rowWidth <= available)
            {
                btnSave.Width = saveDefaultWidth;
                btnCancel.Width = cancelDefaultWidth;
                btnRegisterFace.Width = registerDefaultWidth;

                btnSave.Left = fieldLeft;
                btnCancel.Left = btnSave.Right + gap;
                btnRegisterFace.Left = btnCancel.Right + gap;

                var top = Math.Max(btnSave.Top, btnCancel.Top);
                btnSave.Top = top;
                btnCancel.Top = top;
                btnRegisterFace.Top = top;
            }
            else
            {
                btnSave.Left = fieldLeft;
                btnCancel.Left = fieldLeft;
                btnRegisterFace.Left = fieldLeft;

                btnSave.Width = available;
                btnCancel.Width = available;
                btnRegisterFace.Width = available;

                btnCancel.Top = btnSave.Bottom + 6;
                btnRegisterFace.Top = btnCancel.Bottom + 6;
            }
        }

        private static void SetFieldWidth(Control control, int width)
        {
            if (control == null)
                return;

            control.Width = Math.Max(120, width);
        }

        // =============================================
        // Load tất cả dữ liệu
        // =============================================
        public async void RefreshData()
        {
            try
            {
                // Load lookup tables
                _departments = await AppDatabase.Repository.GetDepartmentsAsync();
                _positions   = await AppDatabase.Repository.GetPositionsAsync();
                _shifts      = await AppDatabase.Repository.GetWorkShiftsAsync();

                // Populate dropdowns
                PopulateComboBox(cboDepartment, _departments, d => d.Name, "(-- Phòng ban --)");
                PopulateComboBox(cboPosition,   _positions,   p => p.Name, "(-- Chức vụ --)");
                PopulateComboBox(cboShift,      _shifts,      s => $"{s.Name} ({s.StartTime:hh\\:mm}–{s.EndTime:hh\\:mm})", "(-- Ca mặc định --)");

                // Populate Manager dropdown
                _employees = await AppDatabase.Repository.GetEmployeesAsync(true);
                PopulateComboBox(cboManager, _employees, e => $"{e.Code} - {e.FullName}", "(-- Không có --)");

                // Filter combobox
                cboFilterDepartment.Items.Clear();
                cboFilterDepartment.Items.Add("Tất cả phòng ban");
                foreach (var d in _departments) cboFilterDepartment.Items.Add(d.Name);
                cboFilterDepartment.SelectedIndex = 0;

                // Load employee list
                await LoadEmployeeListAsync(true);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void PopulateComboBox<T>(ComboBox cbo, List<T> items, Func<T, string> display, string defaultText)
        {
            cbo.Items.Clear();
            cbo.Items.Add(new ComboItem<T>(default, defaultText));
            foreach (var item in items)
                cbo.Items.Add(new ComboItem<T>(item, display(item)));
            cbo.SelectedIndex = 0;
        }

        private async System.Threading.Tasks.Task LoadEmployeeListAsync(bool activeOnly = true)
        {
            var employees = await AppDatabase.Repository.GetEmployeesAsync(activeOnly);
            dgvEmployees.Rows.Clear();
            int idx = 1;
            foreach (var emp in employees)
            {
                var faceStatus = emp.IsFaceRegistered ? "✅ ĐK rồi" : "❌ Chưa ĐK";
                var workStatus = emp.IsActive ? "Đang LV" : "Nghỉ việc";
                var row = dgvEmployees.Rows.Add(
                    idx++, emp.Code, emp.FullName,
                    emp.DepartmentName ?? "—", emp.PositionName ?? "—",
                    emp.Phone ?? "—", faceStatus, workStatus);
                dgvEmployees.Rows[row].Tag = emp;

                // Color inactive
                if (!emp.IsActive)
                    dgvEmployees.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(160, 160, 160);
                // Highlight not face registered
                if (!emp.IsFaceRegistered)
                    dgvEmployees.Rows[row].Cells["colFaceStatus"].Style.ForeColor = Color.FromArgb(231, 76, 60);
                else
                    dgvEmployees.Rows[row].Cells["colFaceStatus"].Style.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        private async System.Threading.Tasks.Task FilterByDepartmentAsync()
        {
            if (_departments == null) return;
            try
            {
                var all = await AppDatabase.Repository.GetEmployeesAsync(true);
                if (cboFilterDepartment.SelectedIndex > 0)
                {
                    var deptName = cboFilterDepartment.SelectedItem.ToString();
                    all = all.Where(e => e.DepartmentName == deptName).ToList();
                }
                dgvEmployees.Rows.Clear();
                int idx = 1;
                foreach (var emp in all)
                {
                    var row = dgvEmployees.Rows.Add(idx++, emp.Code, emp.FullName,
                        emp.DepartmentName ?? "—", emp.PositionName ?? "—",
                        emp.Phone ?? "—",
                        emp.IsFaceRegistered ? "✅ ĐK rồi" : "❌ Chưa ĐK",
                        emp.IsActive ? "Đang LV" : "Nghỉ việc");
                    dgvEmployees.Rows[row].Tag = emp;
                }
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        // =============================================
        // Chọn nhân viên → populate form + lịch sử CC
        // =============================================
        private async void DgvEmployees_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0) return;
            var emp = dgvEmployees.SelectedRows[0].Tag as EmployeeDto;
            if (emp == null) return;

            _editingEmployeeId = emp.Id;

            // Populate basic fields
            txtEmployeeCode.Text  = emp.Code;
            txtFullName.Text      = emp.FullName;
            txtEmail.Text         = emp.Email ?? "";
            txtPhone.Text         = emp.Phone ?? "";
            txtIdentityCard.Text  = emp.IdentityCard ?? "";
            
            // Gender dropdown
            int gIdx = 0;
            for (int i = 0; i < cboGender.Items.Count; i++)
            {
                var item = cboGender.Items[i] as ComboItem<string>;
                if (item != null && item.Value == emp.Gender) { gIdx = i; break; }
            }
            cboGender.SelectedIndex = gIdx;

            if (emp.DateOfBirth.HasValue) dtpDateOfBirth.Value = emp.DateOfBirth.Value;
            dtpHireDate.Value = emp.HireDate;
            chkIsActive.Checked   = emp.IsActive;
            nudAnnualLeave.Value  = (decimal)emp.AnnualLeaveDays;

            // Set Department combobox by ID
            SetComboById(cboDepartment, _departments, d => d.Id == emp.DepartmentId, d => d.Name);
            SetComboById(cboPosition,   _positions,   p => p.Id == emp.PositionId,   p => p.Name);
            SetComboById(cboShift,      _shifts,      s => s.Id == emp.DefaultShiftId,
                s => $"{s.Name} ({s.StartTime:hh\\:mm}–{s.EndTime:hh\\:mm})");

            // EmploymentType
            int etIdx = cboEmploymentType.FindStringExact(emp.EmploymentType ?? "FullTime");
            cboEmploymentType.SelectedIndex = etIdx >= 0 ? etIdx : 0;

            // Manager
            if (emp.ManagerId.HasValue)
                SetComboById(cboManager, _employees, m => m.Id == emp.ManagerId.Value, m => m.FullName);
            else
                cboManager.SelectedIndex = 0;

            // Leave Balance
            var remaining = emp.AnnualLeaveDays - emp.UsedLeaveDays;
            lblLeaveBalance.Text = $"Phép còn: {remaining} ngày";
            lblLeaveBalance.ForeColor = remaining > 0
                ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 68, 68);

            // Load attendance history
            await LoadAttendanceHistoryAsync(emp.Id);

            // Avatar
            if (!string.IsNullOrEmpty(emp.AvatarPath) && System.IO.File.Exists(emp.AvatarPath))
            {
                try { picEmployeePhoto.Image = Image.FromFile(emp.AvatarPath); } catch { picEmployeePhoto.Image = null; }
            }
            else
            {
                picEmployeePhoto.Image = null;
            }
        }

        private void SetComboById<T>(ComboBox cbo, List<T> items, Func<T, bool> match, Func<T, string> display)
        {
            int found = -1;
            for (int i = 1; i < cbo.Items.Count; i++) // skip default [0]
            {
                var item = cbo.Items[i] as ComboItem<T>;
                if (item != null && match(item.Value))
                {
                    found = i;
                    break;
                }
            }
            cbo.SelectedIndex = found >= 0 ? found : 0;
        }

        private async System.Threading.Tasks.Task LoadAttendanceHistoryAsync(int empId)
        {
            try
            {
                var to   = DateTime.Today;
                var from = to.AddDays(-30);
                var records = await AppDatabase.Repository.GetAttendanceByEmployeeAsync(empId, from, to);
                dgvAttHistory.Rows.Clear();
                foreach (var r in records)
                {
                    var rowIdx = dgvAttHistory.Rows.Add(
                        r.AttendanceDate.ToString("dd/MM/yyyy"),
                        r.CheckIn?.ToString("HH:mm")  ?? "—",
                        r.CheckOut?.ToString("HH:mm") ?? "—",
                        TranslateStatus(r.Status),
                        r.LateMinutes,
                        r.WorkingMinutes,
                        r.CheckInMethod ?? "—",
                        r.Note ?? "");

                    // Color status
                    var cell = dgvAttHistory.Rows[rowIdx].Cells["AttStatus"];
                    switch (r.Status)
                    {
                        case "Present":     cell.Style.ForeColor = Color.FromArgb(34, 197, 94);  break;
                        case "Late":
                        case "LateAndEarly":cell.Style.ForeColor = Color.FromArgb(234, 179, 8);  break;
                        case "EarlyLeave":  cell.Style.ForeColor = Color.FromArgb(249, 115, 22); break;
                        case "Absent":      cell.Style.ForeColor = Color.FromArgb(239, 68, 68);  break;
                        default:            cell.Style.ForeColor = Color.FromArgb(107, 114, 128); break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Att history error: {ex.Message}");
            }
        }

        // =============================================
        // CRUD
        // =============================================
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            _editingEmployeeId = null;
            ClearForm();
            lblDetailTitle.Text = "📝 Thêm nhân viên mới";
            txtEmployeeCode.Focus();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var emp = dgvEmployees.SelectedRows[0].Tag as EmployeeDto;
            if (emp == null) return;
            lblDetailTitle.Text = $"✏️ Sửa: {emp.FullName}";
            txtEmployeeCode.Focus();
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var emp = dgvEmployees.SelectedRows[0].Tag as EmployeeDto;
            if (emp == null) return;

            var r = MessageBox.Show(
                $"Vô hiệu hóa nhân viên '{emp.FullName}'?\n(Dữ liệu chấm công sẽ được giữ nguyên)",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            try
            {
                await AppDatabase.Repository.DeleteEmployeeAsync(emp.Id);
                MessageBox.Show("Đã vô hiệu hóa nhân viên!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                RefreshData();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            var keyword = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(keyword)) { await LoadEmployeeListAsync(true); return; }

            try
            {
                var all = await AppDatabase.Repository.GetEmployeesAsync(true);
                var filtered = all.Where(emp =>
                    emp.Code.ToLower().Contains(keyword) ||
                    emp.FullName.ToLower().Contains(keyword) ||
                    (emp.Phone ?? "").Contains(keyword) ||
                    (emp.Email ?? "").ToLower().Contains(keyword) ||
                    (emp.IdentityCard ?? "").Contains(keyword)
                ).ToList();

                dgvEmployees.Rows.Clear();
                int idx = 1;
                foreach (var emp in filtered)
                {
                    var row = dgvEmployees.Rows.Add(idx++, emp.Code, emp.FullName,
                        emp.DepartmentName ?? "—", emp.PositionName ?? "—",
                        emp.Phone ?? "—",
                        emp.IsFaceRegistered ? "✅ ĐK rồi" : "❌ Chưa ĐK",
                        emp.IsActive ? "Đang LV" : "Nghỉ việc");
                    dgvEmployees.Rows[row].Tag = emp;
                }
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmployeeCode.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã NV và Họ tên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Lấy ID từ combobox
                int? deptId  = (cboDepartment.SelectedItem  as ComboItem<DepartmentDto>)?.Value?.Id;
                int? posId   = (cboPosition.SelectedItem    as ComboItem<PositionDto>)?.Value?.Id;
                int? shiftId = (cboShift.SelectedItem       as ComboItem<WorkShiftDto>)?.Value?.Id;

                var emp = new EmployeeDto
                {
                    Code             = txtEmployeeCode.Text.Trim(),
                    FullName         = txtFullName.Text.Trim(),
                    DepartmentId     = deptId,
                    PositionId       = posId,
                    DefaultShiftId   = shiftId,
                    ManagerId        = (cboManager.SelectedItem as ComboItem<EmployeeDto>)?.Value?.Id,
                    EmploymentType   = cboEmploymentType.SelectedItem?.ToString() ?? "FullTime",
                    Phone            = NullIfEmpty(txtPhone.Text),
                    Email            = NullIfEmpty(txtEmail.Text),
                    IdentityCard     = NullIfEmpty(txtIdentityCard.Text),
                    Gender           = (cboGender.SelectedItem as ComboItem<string>)?.Value ?? "M",
                    DateOfBirth      = dtpDateOfBirth.Value.Date,
                    HireDate         = dtpHireDate.Value.Date,
                    IsActive         = chkIsActive.Checked,
                    AnnualLeaveDays  = nudAnnualLeave.Value
                };

                if (_editingEmployeeId.HasValue)
                {
                    emp.Id = _editingEmployeeId.Value;
                    await AppDatabase.Repository.UpdateEmployeeAsync(emp);
                    MessageBox.Show("Cập nhật nhân viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int newId = await AppDatabase.Repository.CreateEmployeeAsync(emp);
                    MessageBox.Show($"Thêm nhân viên thành công! (ID: {newId})", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearForm();
                RefreshData();
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _editingEmployeeId = null;
            ClearForm();
            lblDetailTitle.Text = "📝 Thông tin chi tiết";
        }

        private void BtnRegisterFace_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0 || _editingEmployeeId == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show("Vui lòng chuyển sang tab '📸 Đăng ký khuôn mặt' để tiến hành đăng ký Face ID.", "Hướng dẫn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // =============================================
        // Helpers
        // =============================================
        private void ClearForm()
        {
            _editingEmployeeId = null;
            txtEmployeeCode.Text  = "";
            txtFullName.Text      = "";
            txtEmail.Text         = "";
            txtPhone.Text         = "";
            txtIdentityCard.Text  = "";
            cboGender.SelectedIndex = 0;
            cboDepartment.SelectedIndex  = 0;
            cboPosition.SelectedIndex    = 0;
            cboShift.SelectedIndex       = 0;
            cboEmploymentType.SelectedIndex = 0;
            if (cboManager.Items.Count > 0) cboManager.SelectedIndex = 0;
            nudAnnualLeave.Value  = 12;
            lblLeaveBalance.Text = "Phép còn: — ngày";
            lblLeaveBalance.ForeColor = Color.FromArgb(107, 114, 128);
            dtpDateOfBirth.Value  = DateTime.Today.AddYears(-25);
            dtpHireDate.Value     = DateTime.Today;
            chkIsActive.Checked   = true;
            picEmployeePhoto.Image = null;
            dgvAttHistory.Rows.Clear();
        }

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string TranslateStatus(string status)
        {
            switch (status)
            {
                case "Present":      return "Đúng giờ";
                case "Late":         return "Đi trễ";
                case "EarlyLeave":   return "Về sớm";
                case "LateAndEarly": return "Trễ+Sớm";
                case "Absent":       return "Vắng mặt";
                case "Leave":        return "Nghỉ phép";
                case "Holiday":      return "Ngày lễ";
                case "DayOff":       return "Ngày nghỉ";
                case "NotYet":       return "Chưa CC";
                default:             return status ?? "—";
            }
        }

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

        // Generic wrapper giữ giá trị kèm tên hiển thị
        private class ComboItem<T>
        {
            public T Value { get; }
            private readonly string _display;
            public ComboItem(T value, string display) { Value = value; _display = display; }
            public override string ToString() => _display;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp.UserControls
{
    public partial class UCCatalog : UserControl
    {
        // Grids
        private DataGridView dgvDept, dgvPos, dgvShift, dgvDevice, dgvCalendar, dgvShiftSched;
        private List<EmployeeDto> _empListSched;
        private List<WorkShiftDto> _shiftListSched;

        public UCCatalog()
        {
            InitializeComponent();
            BuildDepartmentsTab();
            BuildPositionsTab();
            BuildShiftsTab();
            BuildDevicesTab();
            BuildCalendarsTab();
            BuildShiftScheduleTab();
            this.Load += async (s, e) => await RefreshAllAsync();
        }

        public void RefreshData()
        {
            _ = RefreshAllAsync();
        }

        private async System.Threading.Tasks.Task RefreshAllAsync()
        {
            await LoadDepartmentsAsync();
            await LoadPositionsAsync();
            await LoadShiftsAsync();
            await LoadDevicesAsync();
            await LoadCalendarsAsync();
            await LoadShiftScheduleAsync();
        }

        #region Departments
        private void BuildDepartmentsTab()
        {
            var toolbar = CreateToolbar(tabDepartments, "Thêm phòng ban", BtnAddDept_Click, BtnDeleteDept_Click);
            dgvDept = CreateGrid(tabDepartments, toolbar.Bottom + 5);
            dgvDept.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã PB", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên phòng ban", Width = 250 },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Mô tả", Width = 300 },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Hoạt động", Width = 80 }
            });
            dgvDept.CellEndEdit += DgvDept_CellEndEdit;
        }

        private async System.Threading.Tasks.Task LoadDepartmentsAsync()
        {
            try
            {
                var list = await AppDatabase.Repository.GetDepartmentsAsync();
                dgvDept.Rows.Clear();
                foreach (var d in list)
                    dgvDept.Rows.Add(d.Id, d.Code, d.Name, d.Description, d.IsActive);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnAddDept_Click(object sender, EventArgs e)
        {
            try
            {
                var code = "PB_" + DateTime.Now.ToString("HHmmss");
                var id = await AppDatabase.Repository.CreateDepartmentAsync(new DepartmentDto { Code = code, Name = "Phòng ban mới", IsActive = true });
                await LoadDepartmentsAsync();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnDeleteDept_Click(object sender, EventArgs e)
        {
            if (dgvDept.CurrentRow == null) return;
            var id = (int)dgvDept.CurrentRow.Cells["Id"].Value;
            if (MessageBox.Show("Xóa phòng ban này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await AppDatabase.Repository.DeleteDepartmentAsync(id);
                await LoadDepartmentsAsync();
            }
        }

        private async void DgvDept_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvDept.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;
            var dto = new DepartmentDto
            {
                Id = (int)row.Cells["Id"].Value,
                Code = row.Cells["Code"].Value?.ToString() ?? "",
                Name = row.Cells["Name"].Value?.ToString() ?? "",
                Description = row.Cells["Description"].Value?.ToString(),
                IsActive = row.Cells["IsActive"].Value is bool b && b
            };
            try { await AppDatabase.Repository.UpdateDepartmentAsync(dto); }
            catch (Exception ex) { ShowError(ex); await LoadDepartmentsAsync(); }
        }
        #endregion

        #region Positions
        private void BuildPositionsTab()
        {
            var toolbar = CreateToolbar(tabPositions, "Thêm chức vụ", BtnAddPos_Click, BtnDeletePos_Click);
            dgvPos = CreateGrid(tabPositions, toolbar.Bottom + 5);
            dgvPos.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã CV", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên chức vụ", Width = 250 },
                new DataGridViewTextBoxColumn { Name = "Level", HeaderText = "Cấp bậc", Width = 80 },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Hoạt động", Width = 80 }
            });
            dgvPos.CellEndEdit += DgvPos_CellEndEdit;
        }

        private async System.Threading.Tasks.Task LoadPositionsAsync()
        {
            try
            {
                var list = await AppDatabase.Repository.GetPositionsAsync();
                dgvPos.Rows.Clear();
                foreach (var p in list)
                    dgvPos.Rows.Add(p.Id, p.Code, p.Name, p.Level, p.IsActive);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnAddPos_Click(object sender, EventArgs e)
        {
            try
            {
                var code = "CV_" + DateTime.Now.ToString("HHmmss");
                await AppDatabase.Repository.CreatePositionAsync(new PositionDto { Code = code, Name = "Chức vụ mới", Level = 1, IsActive = true });
                await LoadPositionsAsync();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnDeletePos_Click(object sender, EventArgs e)
        {
            if (dgvPos.CurrentRow == null) return;
            var id = (int)dgvPos.CurrentRow.Cells["Id"].Value;
            if (MessageBox.Show("Xóa chức vụ này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await AppDatabase.Repository.DeletePositionAsync(id);
                await LoadPositionsAsync();
            }
        }

        private async void DgvPos_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvPos.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;
            var dto = new PositionDto
            {
                Id = (int)row.Cells["Id"].Value,
                Code = row.Cells["Code"].Value?.ToString() ?? "",
                Name = row.Cells["Name"].Value?.ToString() ?? "",
                Level = int.TryParse(row.Cells["Level"].Value?.ToString(), out var lv) ? lv : 1,
                IsActive = row.Cells["IsActive"].Value is bool b && b
            };
            try { await AppDatabase.Repository.UpdatePositionAsync(dto); }
            catch (Exception ex) { ShowError(ex); await LoadPositionsAsync(); }
        }
        #endregion

        #region WorkShifts
        private void BuildShiftsTab()
        {
            var toolbar = CreateToolbar(tabShifts, "Thêm ca", BtnAddShift_Click, BtnDeleteShift_Click);
            dgvShift = CreateGrid(tabShifts, toolbar.Bottom + 5);
            dgvShift.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 40, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên ca", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "ShiftType", HeaderText = "Loại", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "StartTime", HeaderText = "Bắt đầu", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "EndTime", HeaderText = "Kết thúc", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "BreakMin", HeaderText = "Nghỉ (phút)", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "StdHours", HeaderText = "Giờ chuẩn", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "LateTh", HeaderText = "Trễ (phút)", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "EarlyTh", HeaderText = "Sớm (phút)", Width = 80 },
                new DataGridViewCheckBoxColumn { Name = "IsOvernight", HeaderText = "Ca đêm", Width = 70 },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "HĐ", Width = 50 }
            });
            dgvShift.CellEndEdit += DgvShift_CellEndEdit;
        }

        private async System.Threading.Tasks.Task LoadShiftsAsync()
        {
            try
            {
                var list = await AppDatabase.Repository.GetWorkShiftsAsync();
                dgvShift.Rows.Clear();
                foreach (var s in list)
                    dgvShift.Rows.Add(s.Id, s.Code, s.Name, s.ShiftType,
                        s.StartTime.ToString(@"hh\:mm"), s.EndTime.ToString(@"hh\:mm"),
                        s.BreakMinutes, s.StandardHours, s.LateThreshold, s.EarlyThreshold,
                        s.IsOvernight, s.IsActive);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnAddShift_Click(object sender, EventArgs e)
        {
            try
            {
                var code = "CA_" + DateTime.Now.ToString("HHmmss");
                await AppDatabase.Repository.CreateWorkShiftAsync(new WorkShiftDto
                {
                    Code = code, Name = "Ca mới", ShiftType = "Fixed",
                    StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(17),
                    BreakMinutes = 60, StandardHours = 8, LateThreshold = 15, EarlyThreshold = 15
                });
                await LoadShiftsAsync();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnDeleteShift_Click(object sender, EventArgs e)
        {
            if (dgvShift.CurrentRow == null) return;
            var id = (int)dgvShift.CurrentRow.Cells["Id"].Value;
            if (MessageBox.Show("Xóa ca này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await AppDatabase.Repository.DeleteWorkShiftAsync(id);
                await LoadShiftsAsync();
            }
        }

        private async void DgvShift_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvShift.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;
            TimeSpan.TryParse(row.Cells["StartTime"].Value?.ToString(), out var start);
            TimeSpan.TryParse(row.Cells["EndTime"].Value?.ToString(), out var end);
            var dto = new WorkShiftDto
            {
                Id = (int)row.Cells["Id"].Value,
                Code = row.Cells["Code"].Value?.ToString() ?? "",
                Name = row.Cells["Name"].Value?.ToString() ?? "",
                ShiftType = row.Cells["ShiftType"].Value?.ToString() ?? "Fixed",
                StartTime = start, EndTime = end,
                BreakMinutes = int.TryParse(row.Cells["BreakMin"].Value?.ToString(), out var br) ? br : 0,
                StandardHours = decimal.TryParse(row.Cells["StdHours"].Value?.ToString(), out var sh) ? sh : 8,
                LateThreshold = int.TryParse(row.Cells["LateTh"].Value?.ToString(), out var lt) ? lt : 15,
                EarlyThreshold = int.TryParse(row.Cells["EarlyTh"].Value?.ToString(), out var et) ? et : 15,
                IsOvernight = row.Cells["IsOvernight"].Value is bool bo && bo,
                IsActive = row.Cells["IsActive"].Value is bool ba && ba
            };
            try { await AppDatabase.Repository.UpdateWorkShiftAsync(dto); }
            catch (Exception ex) { ShowError(ex); await LoadShiftsAsync(); }
        }
        #endregion

        #region Devices
        private void BuildDevicesTab()
        {
            var toolbar = CreateToolbar(tabDevices, "Thêm thiết bị", BtnAddDevice_Click, BtnDeleteDevice_Click);
            dgvDevice = CreateGrid(tabDevices, toolbar.Bottom + 5);
            dgvDevice.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã TB", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên thiết bị", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "DeviceType", HeaderText = "Loại", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Location", HeaderText = "Vị trí", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "IpAddress", HeaderText = "IP", Width = 130 },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "HĐ", Width = 60 }
            });
            dgvDevice.CellEndEdit += DgvDevice_CellEndEdit;
        }

        private async System.Threading.Tasks.Task LoadDevicesAsync()
        {
            try
            {
                var list = await AppDatabase.Repository.GetDevicesAsync();
                dgvDevice.Rows.Clear();
                foreach (var d in list)
                    dgvDevice.Rows.Add(d.Id, d.Code, d.Name, d.DeviceType, d.Location, d.IpAddress, d.IsActive);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnAddDevice_Click(object sender, EventArgs e)
        {
            try
            {
                var code = "DEV_" + DateTime.Now.ToString("HHmmss");
                await AppDatabase.Repository.CreateDeviceAsync(new AttendanceDeviceDto { Code = code, Name = "Thiết bị mới", DeviceType = "Camera" });
                await LoadDevicesAsync();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnDeleteDevice_Click(object sender, EventArgs e)
        {
            if (dgvDevice.CurrentRow == null) return;
            var id = (int)dgvDevice.CurrentRow.Cells["Id"].Value;
            if (MessageBox.Show("Xóa thiết bị này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await AppDatabase.Repository.DeleteDeviceAsync(id);
                await LoadDevicesAsync();
            }
        }

        private async void DgvDevice_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvDevice.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;
            var dto = new AttendanceDeviceDto
            {
                Id = (int)row.Cells["Id"].Value,
                Code = row.Cells["Code"].Value?.ToString() ?? "",
                Name = row.Cells["Name"].Value?.ToString() ?? "",
                DeviceType = row.Cells["DeviceType"].Value?.ToString() ?? "Camera",
                Location = row.Cells["Location"].Value?.ToString(),
                IpAddress = row.Cells["IpAddress"].Value?.ToString(),
                IsActive = row.Cells["IsActive"].Value is bool b && b
            };
            try { await AppDatabase.Repository.UpdateDeviceAsync(dto); }
            catch (Exception ex) { ShowError(ex); await LoadDevicesAsync(); }
        }
        #endregion

        #region Calendars
        private void BuildCalendarsTab()
        {
            var toolbar = CreateToolbar(tabCalendars, "Thêm lịch", BtnAddCal_Click, BtnDeleteCal_Click);
            dgvCalendar = CreateGrid(tabCalendars, toolbar.Bottom + 5);
            dgvCalendar.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 40, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Year", HeaderText = "Năm", Width = 60 },
                new DataGridViewCheckBoxColumn { Name = "Mon", HeaderText = "T2", Width = 45 },
                new DataGridViewCheckBoxColumn { Name = "Tue", HeaderText = "T3", Width = 45 },
                new DataGridViewCheckBoxColumn { Name = "Wed", HeaderText = "T4", Width = 45 },
                new DataGridViewCheckBoxColumn { Name = "Thu", HeaderText = "T5", Width = 45 },
                new DataGridViewCheckBoxColumn { Name = "Fri", HeaderText = "T6", Width = 45 },
                new DataGridViewCheckBoxColumn { Name = "Sat", HeaderText = "T7", Width = 45 },
                new DataGridViewCheckBoxColumn { Name = "Sun", HeaderText = "CN", Width = 45 },
                new DataGridViewCheckBoxColumn { Name = "IsDefault", HeaderText = "Mặc định", Width = 70 }
            });
            dgvCalendar.CellEndEdit += DgvCal_CellEndEdit;
        }

        private async System.Threading.Tasks.Task LoadCalendarsAsync()
        {
            try
            {
                var list = await AppDatabase.Repository.GetWorkCalendarsAsync();
                dgvCalendar.Rows.Clear();
                foreach (var c in list)
                    dgvCalendar.Rows.Add(c.Id, c.Name, c.Year, c.Monday, c.Tuesday, c.Wednesday, c.Thursday, c.Friday, c.Saturday, c.Sunday, c.IsDefault);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnAddCal_Click(object sender, EventArgs e)
        {
            try
            {
                await AppDatabase.Repository.CreateWorkCalendarAsync(new WorkCalendarDto
                {
                    Name = "Lịch mới", Year = DateTime.Now.Year,
                    Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true
                });
                await LoadCalendarsAsync();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private async void BtnDeleteCal_Click(object sender, EventArgs e)
        {
            if (dgvCalendar.CurrentRow == null) return;
            var id = (int)dgvCalendar.CurrentRow.Cells["Id"].Value;
            if (MessageBox.Show("Xóa lịch này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await AppDatabase.Repository.DeleteWorkCalendarAsync(id);
                await LoadCalendarsAsync();
            }
        }

        private async void DgvCal_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvCalendar.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;
            var dto = new WorkCalendarDto
            {
                Id = (int)row.Cells["Id"].Value,
                Name = row.Cells["Name"].Value?.ToString() ?? "",
                Year = int.TryParse(row.Cells["Year"].Value?.ToString(), out var yr) ? yr : DateTime.Now.Year,
                Monday = row.Cells["Mon"].Value is bool m && m,
                Tuesday = row.Cells["Tue"].Value is bool tu && tu,
                Wednesday = row.Cells["Wed"].Value is bool w && w,
                Thursday = row.Cells["Thu"].Value is bool th && th,
                Friday = row.Cells["Fri"].Value is bool f && f,
                Saturday = row.Cells["Sat"].Value is bool sa && sa,
                Sunday = row.Cells["Sun"].Value is bool su && su,
                IsDefault = row.Cells["IsDefault"].Value is bool d && d,
                IsActive = true
            };
            try { await AppDatabase.Repository.UpdateWorkCalendarAsync(dto); }
            catch (Exception ex) { ShowError(ex); await LoadCalendarsAsync(); }
        }
        #endregion

        #region Helpers
        private Panel CreateToolbar(TabPage tab, string addText, EventHandler onAdd, EventHandler onDelete)
        {
            var pnl = new Panel { Height = 45, Dock = DockStyle.Top, BackColor = Color.Transparent };

            var btnAdd = new Button
            {
                Text = $"＋ {addText}", 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White, 
                BackColor = Color.FromArgb(34, 197, 94),
                FlatStyle = FlatStyle.Flat, 
                Size = new Size(180, 36), 
                Location = new Point(5, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += onAdd;
            pnl.Controls.Add(btnAdd);

            var btnDel = new Button
            {
                Text = "🗑 Xóa", 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White, 
                BackColor = Color.FromArgb(239, 68, 68),
                FlatStyle = FlatStyle.Flat, 
                Size = new Size(100, 36), 
                Location = new Point(btnAdd.Right + 8, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += onDelete;
            pnl.Controls.Add(btnDel);

            tab.Controls.Add(pnl);
            return pnl;
        }

        private DataGridView CreateGrid(TabPage tab, int top)
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 42, RowTemplate = { Height = 32 },
                Font = new Font("Segoe UI", 9.5F),
                GridColor = Color.FromArgb(226, 232, 240)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 41, 59);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.EnableHeadersVisualStyles = false;
            tab.Controls.Add(dgv);
            dgv.BringToFront();
            return dgv;
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region ShiftSchedule
        private void BuildShiftScheduleTab()
        {
            var pnl = new Panel { Dock = DockStyle.Fill };

            // ─── Toolbar ─────────────────────────────────────────
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(5) };

            var lblEmp = new Label { Text = "Nhân viên:", Location = new Point(5, 16), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            var cboEmp = new ComboBox { Location = new Point(80, 12), Size = new Size(220, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), Name = "cboSchedEmp" };

            var lblDate = new Label { Text = "Ngày:", Location = new Point(312, 16), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            var dtpDate = new DateTimePicker { Location = new Point(360, 12), Size = new Size(120, 26), CustomFormat = "dd/MM/yyyy", Format = DateTimePickerFormat.Custom, Font = new Font("Segoe UI", 9.5F), Name = "dtpSchedDate", Value = DateTime.Today };

            var lblShiftSel = new Label { Text = "Ca:", Location = new Point(493, 16), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            var cboShiftSel = new ComboBox { Location = new Point(515, 12), Size = new Size(180, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), Name = "cboSchedShift" };

            var chkOverride = new CheckBox { Text = "Ghi đè ca mặc định", Location = new Point(708, 14), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };

            var btnAssign = new Button { Text = "✅ Gán ca", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(34, 197, 94), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Size = new Size(90, 32), Location = new Point(850, 11) };
            btnAssign.FlatAppearance.BorderSize = 0;

            var btnDelSched = new Button { Text = "🗑️ Xóa", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(239, 68, 68), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Size = new Size(75, 32), Location = new Point(945, 11) };
            btnDelSched.FlatAppearance.BorderSize = 0;

            toolbar.Controls.AddRange(new Control[] { lblEmp, cboEmp, lblDate, dtpDate, lblShiftSel, cboShiftSel, chkOverride, btnAssign, btnDelSched });

            // ─── Grid ────────────────────────────────────────────
            dgvShiftSched = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 35, RowTemplate = { Height = 30 },
                Font = new Font("Segoe UI", 9F), GridColor = Color.FromArgb(226, 232, 240)
            };
            dgvShiftSched.EnableHeadersVisualStyles = false;
            dgvShiftSched.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvShiftSched.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvShiftSched.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvShiftSched.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvShiftSched.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvShiftSched.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvShiftSched.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="SSId",     HeaderText="ID",          Width=60,  Visible=false },
                new DataGridViewTextBoxColumn { Name="SSDate",   HeaderText="Ngày",         Width=110 },
                new DataGridViewTextBoxColumn { Name="SSEmp",    HeaderText="Nhân viên",    Width=180 },
                new DataGridViewTextBoxColumn { Name="SSShift",  HeaderText="Ca",           Width=160 },
                new DataGridViewTextBoxColumn { Name="SSOverride",HeaderText="Ghi đè",      Width=80  },
                new DataGridViewTextBoxColumn { Name="SSNote",   HeaderText="Ghi chú",      Width=200 }
            });

            // Khi chọn row → fill form
            dgvShiftSched.SelectionChanged += (s, e) =>
            {
                if (dgvShiftSched.SelectedRows.Count == 0) return;
                var row = dgvShiftSched.SelectedRows[0];
                var dateStr = row.Cells["SSDate"].Value?.ToString();
                if (DateTime.TryParse(dateStr, out var dt)) dtpDate.Value = dt;

                var empName = row.Cells["SSEmp"].Value?.ToString() ?? "";
                for (int i = 0; i < cboEmp.Items.Count; i++)
                    if (cboEmp.Items[i].ToString().Contains(empName)) { cboEmp.SelectedIndex = i; break; }
            };

            // Events
            btnAssign.Click += async (s, e) =>
            {
                if (cboEmp.SelectedIndex == 0 || cboShiftSel.SelectedIndex == 0) { MessageBox.Show("Chọn nhân viên và ca!"); return; }
                if (_empListSched == null || _shiftListSched == null) { MessageBox.Show("Dữ liệu chưa tải xong, vui lòng thử lại!"); return; }
                int empIdx = cboEmp.SelectedIndex - 1;
                int shiftIdx = cboShiftSel.SelectedIndex - 1;
                if (empIdx < 0 || empIdx >= _empListSched.Count || shiftIdx < 0 || shiftIdx >= _shiftListSched.Count) { MessageBox.Show("Lỗi lựa chọn, vui lòng tải lại trang!"); return; }
                try
                {
                    var emp   = _empListSched[empIdx];
                    var shift = _shiftListSched[shiftIdx];
                    await AppDatabase.Repository.UpsertShiftScheduleAsync(emp.Id, dtpDate.Value, shift.Id, chkOverride.Checked, null);
                    MessageBox.Show($"✅ Đã gán ca '{shift.Name}' cho {emp.FullName} ngày {dtpDate.Value:dd/MM/yyyy}", "Thành công");
                    await LoadShiftScheduleAsync();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
            };

            btnDelSched.Click += async (s, e) =>
            {
                if (dgvShiftSched.SelectedRows.Count == 0) { MessageBox.Show("Chọn dòng cần xóa!"); return; }
                var id = Convert.ToInt64(dgvShiftSched.SelectedRows[0].Cells["SSId"].Value);
                if (MessageBox.Show("Xóa lịch ca này?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                try
                {
                    await AppDatabase.Repository.DeleteShiftScheduleAsync(id);
                    await LoadShiftScheduleAsync();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
            };

            pnl.Controls.Add(dgvShiftSched);
            pnl.Controls.Add(toolbar);
            tabShiftSched.Controls.Add(pnl);

            // Async load dropdowns
            tabShiftSched.Enter += async (s, e) =>
            {
                await LoadShiftSchedDropdownsAsync(cboEmp, cboShiftSel);
            };
        }

        private async System.Threading.Tasks.Task LoadShiftSchedDropdownsAsync(ComboBox cboEmp, ComboBox cboShiftSel)
        {
            if (_empListSched == null)
            {
                _empListSched = await AppDatabase.Repository.GetEmployeesAsync(true);
                cboEmp.Items.Clear();
                cboEmp.Items.Add("-- Chọn NV --");
                foreach (var e in _empListSched)
                    cboEmp.Items.Add($"{e.Code} – {e.FullName}");
                cboEmp.SelectedIndex = 0;
            }
            if (_shiftListSched == null)
            {
                _shiftListSched = await AppDatabase.Repository.GetWorkShiftsAsync();
                cboShiftSel.Items.Clear();
                cboShiftSel.Items.Add("-- Chọn ca --");
                foreach (var s in _shiftListSched)
                    cboShiftSel.Items.Add($"{s.Name} ({s.StartTime:hh\\:mm}–{s.EndTime:hh\\:mm})");
                cboShiftSel.SelectedIndex = 0;
            }
        }

        private async System.Threading.Tasks.Task LoadShiftScheduleAsync()
        {
            try
            {
                var list = await AppDatabase.Repository.GetEmployeeShiftSchedulesAsync();
                dgvShiftSched.Rows.Clear();
                foreach (var ss in list)
                    dgvShiftSched.Rows.Add(
                        ss.Id,
                        ss.ScheduleDate.ToString("dd/MM/yyyy"),
                        ss.EmployeeName,
                        ss.ShiftName,
                        ss.IsOverride ? "Có" : "Không",
                        ss.Note ?? "");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadShiftSched error: {ex.Message}");
            }
        }
        #endregion
    }
}

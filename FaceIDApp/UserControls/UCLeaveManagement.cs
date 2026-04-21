using System;
using System.Drawing;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp.UserControls
{
    public partial class UCLeaveManagement : UserControl
    {
        private DataGridView dgvLeave, dgvHolidays;
        private ComboBox cboFilter;

        public UCLeaveManagement()
        {
            InitializeComponent();
            BuildLeaveTab();
            BuildHolidaysTab();
            this.Load += async (s, e) =>
            {
                await LoadLeaveAsync();
                await LoadHolidaysAsync();
            };
        }

        #region Leave Requests
        private void BuildLeaveTab()
        {
            var toolbar = new Panel { Height = 45, Dock = DockStyle.Top, BackColor = Color.Transparent };

            var btnAdd = new Button
            {
                Text = "＋ Tạo đơn nghỉ phép", Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(34, 197, 94),
                FlatStyle = FlatStyle.Flat, Size = new Size(200, 35), Location = new Point(5, 5), Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAddLeave_Click;
            toolbar.Controls.Add(btnAdd);

            var btnApprove = new Button
            {
                Text = "✓ Duyệt", Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(59, 130, 246),
                FlatStyle = FlatStyle.Flat, Size = new Size(100, 35), Location = new Point(215, 5), Cursor = Cursors.Hand
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += BtnApprove_Click;
            toolbar.Controls.Add(btnApprove);

            var btnReject = new Button
            {
                Text = "✗ Từ chối", Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White, BackColor = Color.FromArgb(239, 68, 68),
                FlatStyle = FlatStyle.Flat, Size = new Size(110, 35), Location = new Point(325, 5), Cursor = Cursors.Hand
            };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += BtnReject_Click;
            toolbar.Controls.Add(btnReject);

            var lblFilter = new Label { Text = "Lọc:", Font = new Font("Segoe UI", 10F), Location = new Point(460, 10), AutoSize = true };
            toolbar.Controls.Add(lblFilter);

            cboFilter = new ComboBox
            {
                Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(130, 30), Location = new Point(500, 6)
            };
            cboFilter.Items.AddRange(new[] { "Tất cả", "Pending", "Approved", "Rejected" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += async (s, e) => await LoadLeaveAsync();
            toolbar.Controls.Add(cboFilter);

            dgvLeave = CreateGrid(tabLeave);
            dgvLeave.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 40 },
                new DataGridViewTextBoxColumn { Name = "EmpCode", HeaderText = "Mã NV", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "EmpName", HeaderText = "Họ tên", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "LeaveType", HeaderText = "Loại nghỉ", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "StartDate", HeaderText = "Từ ngày", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "EndDate", HeaderText = "Đến ngày", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "TotalDays", HeaderText = "Số ngày", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Reason", HeaderText = "Lý do", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "ApprovedBy", HeaderText = "Người duyệt", Width = 120 }
            });
            dgvLeave.ReadOnly = true;

            // Add toolbar after grid so DockStyle.Top is respected and not hidden by DockStyle.Fill.
            tabLeave.Controls.Add(toolbar);
            tabLeave.Controls.SetChildIndex(toolbar, 0);
        }

        private async System.Threading.Tasks.Task LoadLeaveAsync()
        {
            try
            {
                string status = null;
                if (cboFilter.SelectedIndex > 0) status = cboFilter.SelectedItem.ToString();
                var list = await AppDatabase.Repository.GetLeaveRequestsAsync(null, status);
                dgvLeave.Rows.Clear();
                foreach (var lr in list)
                {
                    var row = dgvLeave.Rows.Add(lr.Id, lr.EmployeeCode, lr.EmployeeName, TranslateLeaveType(lr.LeaveType),
                        lr.StartDate.ToString("dd/MM/yyyy"), lr.EndDate.ToString("dd/MM/yyyy"),
                        lr.TotalDays, lr.Reason, TranslateStatus(lr.Status), lr.ApprovedByName ?? "");
                    // Color code status
                    var statusCell = dgvLeave.Rows[row].Cells["Status"];
                    switch (lr.Status)
                    {
                        case "Pending": statusCell.Style.ForeColor = Color.FromArgb(234, 179, 8); break;
                        case "Approved": statusCell.Style.ForeColor = Color.FromArgb(34, 197, 94); break;
                        case "Rejected": statusCell.Style.ForeColor = Color.FromArgb(239, 68, 68); break;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        private async void BtnAddLeave_Click(object sender, EventArgs e)
        {
            using (var dlg = new LeaveRequestDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await AppDatabase.Repository.CreateLeaveRequestAsync(dlg.Result);
                        await LoadLeaveAsync();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
                }
            }
        }

        private async void BtnApprove_Click(object sender, EventArgs e)
        {
            if (dgvLeave.CurrentRow == null) return;
            var id = (int)dgvLeave.CurrentRow.Cells["Id"].Value;
            var statusVal = dgvLeave.CurrentRow.Cells["Status"].Value?.ToString();
            if (statusVal == "Đã duyệt") { MessageBox.Show("Đơn này đã được duyệt rồi!", "Thông báo"); return; }

            // Lấy approver từ session (fallback về nhân viên đầu tiên nếu chưa có)
            int approverId = AppSession.CurrentEmployeeId ?? 1;
            try
            {
                await AppDatabase.Repository.ApproveLeaveRequestAsync(id, approverId);
                await LoadLeaveAsync();
                MessageBox.Show("✅ Đã duyệt đơn nghỉ phép!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        private async void BtnReject_Click(object sender, EventArgs e)
        {
            if (dgvLeave.CurrentRow == null) return;
            var id = (int)dgvLeave.CurrentRow.Cells["Id"].Value;
            var reason = Microsoft.VisualBasic.Interaction.InputBox("Lý do từ chối:", "Từ chối đơn", "");
            if (string.IsNullOrWhiteSpace(reason)) return;

            int approverId = AppSession.CurrentEmployeeId ?? 1;
            try
            {
                await AppDatabase.Repository.RejectLeaveRequestAsync(id, approverId, reason);
                await LoadLeaveAsync();
                MessageBox.Show("Đã từ chối đơn.", "Thông báo");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }
        #endregion

        #region Holidays
        private void BuildHolidaysTab()
        {
            var toolbar = new Panel { Height = 45, Dock = DockStyle.Top, BackColor = Color.Transparent };

            var btnAdd = new Button
            {
                Text = "＋ Thêm ngày lễ", Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(34, 197, 94),
                FlatStyle = FlatStyle.Flat, Size = new Size(180, 35), Location = new Point(5, 5), Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAddHoliday_Click;
            toolbar.Controls.Add(btnAdd);

            var btnDel = new Button
            {
                Text = "🗑 Xóa", Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White, BackColor = Color.FromArgb(239, 68, 68),
                FlatStyle = FlatStyle.Flat, Size = new Size(100, 35), Location = new Point(195, 5), Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += BtnDeleteHoliday_Click;
            toolbar.Controls.Add(btnDel);

            dgvHolidays = CreateGrid(tabHolidays);
            dgvHolidays.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "HolidayDate", HeaderText = "Ngày", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên ngày lễ", Width = 300 },
                new DataGridViewTextBoxColumn { Name = "HolidayType", HeaderText = "Loại", Width = 120 },
                new DataGridViewCheckBoxColumn { Name = "IsRecurring", HeaderText = "Hàng năm", Width = 80 }
            });
            dgvHolidays.CellEndEdit += DgvHoliday_CellEndEdit;

            // Add toolbar after grid so DockStyle.Top is respected and not hidden by DockStyle.Fill.
            tabHolidays.Controls.Add(toolbar);
            tabHolidays.Controls.SetChildIndex(toolbar, 0);
        }

        private async System.Threading.Tasks.Task LoadHolidaysAsync()
        {
            try
            {
                var list = await AppDatabase.Repository.GetHolidaysAsync();
                dgvHolidays.Rows.Clear();
                foreach (var h in list)
                    dgvHolidays.Rows.Add(h.Id, h.HolidayDate.ToString("dd/MM/yyyy"), h.Name, h.HolidayType, h.IsRecurring);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        private async void BtnAddHoliday_Click(object sender, EventArgs e)
        {
            try
            {
                await AppDatabase.Repository.CreateHolidayAsync(new HolidayDto
                {
                    HolidayDate = DateTime.Today,
                    Name = "Ngày lễ mới",
                    HolidayType = "National",
                    IsRecurring = false
                });
                await LoadHolidaysAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        private async void BtnDeleteHoliday_Click(object sender, EventArgs e)
        {
            if (dgvHolidays.CurrentRow == null) return;
            var id = (int)dgvHolidays.CurrentRow.Cells["Id"].Value;
            if (MessageBox.Show("Xóa ngày lễ này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await AppDatabase.Repository.DeleteHolidayAsync(id);
                await LoadHolidaysAsync();
            }
        }

        private async void DgvHoliday_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvHolidays.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;
            DateTime.TryParse(row.Cells["HolidayDate"].Value?.ToString(), out var dt);
            var dto = new HolidayDto
            {
                Id = (int)row.Cells["Id"].Value,
                HolidayDate = dt,
                Name = row.Cells["Name"].Value?.ToString() ?? "",
                HolidayType = row.Cells["HolidayType"].Value?.ToString() ?? "National",
                IsRecurring = row.Cells["IsRecurring"].Value is bool b && b
            };
            try { await AppDatabase.Repository.UpdateHolidayAsync(dto); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); await LoadHolidaysAsync(); }
        }
        #endregion

        #region Helpers
        private DataGridView CreateGrid(TabPage tab)
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 38, RowTemplate = { Height = 32 },
                Font = new Font("Segoe UI", 9.5F), GridColor = Color.FromArgb(226, 232, 240)
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
            return dgv;
        }

        private string TranslateLeaveType(string type)
        {
            switch (type)
            {
                case "Annual": return "Phép năm";
                case "Sick": return "Ốm đau";
                case "Personal": return "Việc riêng";
                case "Maternity": return "Thai sản";
                case "Unpaid": return "Không lương";
                default: return type;
            }
        }

        private string TranslateStatus(string status)
        {
            switch (status)
            {
                case "Pending": return "Chờ duyệt";
                case "Approved": return "Đã duyệt";
                case "Rejected": return "Từ chối";
                default: return status;
            }
        }
        #endregion
    }

    /// <summary>
    /// Dialog tạo đơn nghỉ phép
    /// </summary>
    internal class LeaveRequestDialog : Form
    {
        public LeaveRequestDto Result { get; private set; }
        private ComboBox cboEmployee, cboType;
        private DateTimePicker dtpStart, dtpEnd;
        private TextBox txtReason;

        public LeaveRequestDialog()
        {
            this.Text = "Tạo đơn nghỉ phép";
            this.Size = new Size(450, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 15;
            AddLabel("Nhân viên:", 15, y); y += 5;
            cboEmployee = new ComboBox { Location = new Point(15, y += 22), Size = new Size(400, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cboEmployee);

            AddLabel("Loại nghỉ:", 15, y += 40);
            cboType = new ComboBox { Location = new Point(15, y += 22), Size = new Size(400, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.Items.AddRange(new[] { "Annual", "Sick", "Personal", "Maternity", "Unpaid" });
            cboType.SelectedIndex = 0;
            this.Controls.Add(cboType);

            AddLabel("Từ ngày:", 15, y += 40);
            dtpStart = new DateTimePicker { Location = new Point(15, y += 22), Size = new Size(190, 28), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpStart);

            AddLabel("Đến ngày:", 220, y - 22);
            dtpEnd = new DateTimePicker { Location = new Point(220, y), Size = new Size(195, 28), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpEnd);

            AddLabel("Lý do:", 15, y += 40);
            txtReason = new TextBox { Location = new Point(15, y += 22), Size = new Size(400, 50), Multiline = true };
            this.Controls.Add(txtReason);

            var btnOk = new Button
            {
                Text = "Tạo đơn", DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(59, 130, 246),
                FlatStyle = FlatStyle.Flat, Size = new Size(130, 38), Location = new Point(155, y += 65), Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (cboEmployee.SelectedItem == null) { MessageBox.Show("Chọn nhân viên!"); this.DialogResult = DialogResult.None; return; }
                var emp = (EmployeeComboItem)cboEmployee.SelectedItem;
                Result = new LeaveRequestDto
                {
                    EmployeeId = emp.Id, LeaveType = cboType.SelectedItem.ToString(),
                    StartDate = dtpStart.Value.Date, EndDate = dtpEnd.Value.Date,
                    TotalDays = (decimal)(dtpEnd.Value.Date - dtpStart.Value.Date).TotalDays + 1,
                    Reason = txtReason.Text
                };
            };
            this.Controls.Add(btnOk);
            this.AcceptButton = btnOk;

            this.Load += async (s, e) =>
            {
                var emps = await AppDatabase.Repository.GetEmployeesAsync();
                foreach (var emp in emps)
                    cboEmployee.Items.Add(new EmployeeComboItem { Id = emp.Id, Display = $"{emp.Code} — {emp.FullName}" });
            };
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) });
        }
    }

    internal class EmployeeComboItem
    {
        public int Id { get; set; }
        public string Display { get; set; }
        public override string ToString() => Display;
    }
}

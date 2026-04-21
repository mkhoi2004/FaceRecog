using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp.UserControls
{
    public partial class UCAttendanceReport : UserControl
    {
        private DataGridView _dgvLog;
        private Button _btnViewLog;
        private Button _btnToggleView;
        private bool _showingLog;

        public UCAttendanceReport()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // ─── Style cho dgvReport (tab báo cáo tổng hợp) ──────────────────
            ConfigureReportColumns();
            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvReport.ColumnHeadersHeight = 35;
            dgvReport.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvReport.RowTemplate.Height = 30;
            dgvReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 250);

            // Dành hàng thứ hai cho nhóm nút nhật ký để tránh chồng lên filter cũ.
            pnlFilter.Height = 108;

            // Mặc định ngày lọc
            dtpFromDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpToDate.Value   = DateTime.Now;

            // ─── Tạo nút Log bên phải thanh filter ───────────────────────────
            _btnViewLog = new Button
            {
                Text = "📋 Nhật ký CC", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(107, 114, 128),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Size = new Size(120, 32)
            };
            _btnViewLog.FlatAppearance.BorderSize = 0;
            _btnViewLog.Click += (s, e) => ShowLogView(true);
            pnlFilter.Controls.Add(_btnViewLog);

            // ─── Panel nhật ký (ẩn mặc định) ─────────────────────────────────
            _dgvLog = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 32, RowTemplate = { Height = 28 },
                Font = new Font("Segoe UI", 9F),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(226, 232, 240), Visible = false
            };
            _dgvLog.EnableHeadersVisualStyles = false;
            _dgvLog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(107, 114, 128);
            _dgvLog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvLog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvLog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            _dgvLog.DefaultCellStyle.SelectionForeColor = Color.Black;
            _dgvLog.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            _dgvLog.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="LogTime",   HeaderText="Thời gian",    Width=130 },
                new DataGridViewTextBoxColumn { Name="LogEmp",    HeaderText="Mã NV",        Width=80  },
                new DataGridViewTextBoxColumn { Name="LogAttId",  HeaderText="Att#",         Width=55  },
                new DataGridViewTextBoxColumn { Name="LogAction", HeaderText="Hành động",    Width=90  },
                new DataGridViewTextBoxColumn { Name="LogMethod", HeaderText="Phương thức",  Width=90  },
                new DataGridViewTextBoxColumn { Name="LogConf",   HeaderText="Độ tin cậy",   Width=90  },
                new DataGridViewTextBoxColumn { Name="LogResult", HeaderText="Kết quả",      Width=80  },
                new DataGridViewTextBoxColumn { Name="LogReason", HeaderText="Lý do lỗi",   Width=120 }
            });

            pnlData.Controls.Add(_dgvLog);

            // Toggle view mode
            _btnToggleView = new Button
            {
                Text = "⟺", Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White, BackColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Size = new Size(32, 32)
            };
            _btnToggleView.FlatAppearance.BorderSize = 0;
            _btnToggleView.Click += (s, e) =>
            {
                ShowLogView(!_showingLog);
            };
            pnlFilter.Controls.Add(_btnToggleView);

            pnlFilter.Resize += (s, e) => LayoutFilterExtras();
            LayoutFilterExtras();
            ShowLogView(false);

            // ─── Event handlers ───────────────────────────────────────────────
            LoadFiltersAsync();
            btnSearch.Click      += BtnSearch_Click;
            btnExportExcel.Click += BtnExportExcel_Click;
            btnPrint.Click       += BtnPrint_Click;
        }

        private void ConfigureReportColumns()
        {
            dgvReport.Columns.Clear();
            dgvReport.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "RMonth",   HeaderText = "Tháng",      FillWeight = 70F  },
                new DataGridViewTextBoxColumn { Name = "REmpCode", HeaderText = "Mã NV",      FillWeight = 75F  },
                new DataGridViewTextBoxColumn { Name = "REmpName", HeaderText = "Họ tên",     FillWeight = 130F },
                new DataGridViewTextBoxColumn { Name = "RDept",    HeaderText = "Phòng ban",  FillWeight = 110F },
                new DataGridViewTextBoxColumn { Name = "RPresent", HeaderText = "Ngày đi làm", FillWeight = 80F  },
                new DataGridViewTextBoxColumn { Name = "RLate",    HeaderText = "Ngày đi trễ", FillWeight = 80F  },
                new DataGridViewTextBoxColumn { Name = "RAbsent",  HeaderText = "Ngày vắng",  FillWeight = 80F  },
                new DataGridViewTextBoxColumn { Name = "RLeave",   HeaderText = "Ngày nghỉ",  FillWeight = 80F  },
                new DataGridViewTextBoxColumn { Name = "RHours",   HeaderText = "Tổng giờ",   FillWeight = 75F  },
                new DataGridViewTextBoxColumn { Name = "RLateMin", HeaderText = "Trễ (ph)",   FillWeight = 70F  }
            });
        }

        private void LayoutFilterExtras()
        {
            if (_btnViewLog == null || _btnToggleView == null)
                return;

            const int y = 58;
            _btnToggleView.Location = new Point(Math.Max(20, pnlFilter.ClientSize.Width - _btnToggleView.Width - 18), y);
            _btnViewLog.Location = new Point(Math.Max(20, _btnToggleView.Left - _btnViewLog.Width - 8), y);
        }

        private void ShowLogView(bool showLog)
        {
            _showingLog = showLog;
            dgvReport.Visible = !showLog;
            _dgvLog.Visible = showLog;
            _btnToggleView.Text = showLog ? "📊" : "⟺";

            if (showLog)
                LoadAttendanceLogsAsync();
            else
                dgvReport.BringToFront();
        }

        // ─── Tải filter dropdowns từ DB ─────────────────────────────────────
        private async void LoadFiltersAsync()
        {
            try
            {
                var employees = await AppDatabase.Repository.GetEmployeesAsync(true);
                cboEmployee.Items.Clear();
                cboEmployee.Items.Add("Tất cả nhân viên");
                foreach (var emp in employees)
                    cboEmployee.Items.Add($"{emp.Code} - {emp.FullName}");
                cboEmployee.SelectedIndex = 0;

                var departments = await AppDatabase.Repository.GetDepartmentsAsync();
                cboDepartment.Items.Clear();
                cboDepartment.Items.Add("Tất cả phòng ban");
                foreach (var d in departments)
                    cboDepartment.Items.Add(d.Name);
                cboDepartment.SelectedIndex = 0;

                // Auto-load báo cáo tháng hiện tại khi mở tab
                BtnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Report filter error: {ex.Message}");
            }
        }

        // ─── Tìm kiếm báo cáo tổng hợp tháng ───────────────────────────────
        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                btnSearch.Enabled = false;
                var month = dtpFromDate.Value;
                var summaries = await AppDatabase.Repository.GetMonthlySummaryAsync(month);

                // Lọc phòng ban
                if (cboDepartment.SelectedIndex > 0)
                {
                    var deptName = cboDepartment.SelectedItem.ToString();
                    summaries = summaries.Where(s => s.DepartmentName == deptName).ToList();
                }
                // Lọc nhân viên
                if (cboEmployee.SelectedIndex > 0)
                {
                    var empCode = cboEmployee.SelectedItem.ToString().Split('-')[0].Trim();
                    summaries = summaries.Where(s => s.EmployeeCode == empCode).ToList();
                }

                dgvReport.Rows.Clear();
                int totalP = 0, totalL = 0, totalA = 0, totalD = 0;

                foreach (var s in summaries)
                {
                    dgvReport.Rows.Add(
                        s.Month.ToString("MM/yyyy"),
                        s.EmployeeCode, s.FullName,
                        s.DepartmentName ?? "—",
                        s.PresentDays, s.LateDays,
                        s.AbsentDays, s.LeaveDays,
                        $"{s.TotalWorkingHours:F1}h",
                        $"{s.TotalLateMinutes} ph");

                    totalP += s.PresentDays; totalL += s.LateDays;
                    totalA += s.AbsentDays;  totalD += s.TotalRecords;
                }

                UpdateSummary(totalD, totalP, totalL, totalA);

                // Tô màu các ô Late/Absent > 0
                foreach (DataGridViewRow row in dgvReport.Rows)
                {
                    if (int.TryParse(row.Cells["RLate"].Value?.ToString(), out int ld) && ld > 0)
                        row.Cells["RLate"].Style.ForeColor = Color.FromArgb(234, 179, 8);

                    if (int.TryParse(row.Cells["RAbsent"].Value?.ToString(), out int ad) && ad > 0)
                        row.Cells["RAbsent"].Style.ForeColor = Color.FromArgb(239, 68, 68);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải báo cáo:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { btnSearch.Enabled = true; }
        }

        // ─── Tải nhật ký attendance_logs ────────────────────────────────────
        private async void LoadAttendanceLogsAsync()
        {
            try
            {
                var from = dtpFromDate.Value.Date;
                var to   = dtpToDate.Value.Date;
                var logs = await AppDatabase.Repository.GetAttendanceLogsAsync(from, to, limit: 1000);
                _dgvLog.Rows.Clear();
                foreach (var log in logs)
                {
                    var rowIdx = _dgvLog.Rows.Add(
                        log.LogTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        log.EmployeeId.HasValue ? log.EmployeeId.ToString() : "?",
                        log.AttendanceId.HasValue ? log.AttendanceId.ToString() : "—",
                        log.LogType,
                        log.Method,
                        log.Confidence.HasValue ? $"{log.Confidence:P1}" : "—",
                        log.Result ?? "—",
                        log.FailReason ?? "");

                    // Màu theo kết quả
                    var resultCell = _dgvLog.Rows[rowIdx].Cells["LogResult"];
                    if (log.Result == "Success")
                        resultCell.Style.ForeColor = Color.FromArgb(34, 197, 94);
                    else if (log.Result == "Fail")
                        resultCell.Style.ForeColor = Color.FromArgb(239, 68, 68);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải nhật ký:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Xuất CSV ────────────────────────────────────────────────────────
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            var grid = _dgvLog.Visible ? _dgvLog : dgvReport;
            if (grid.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var suffix = _dgvLog.Visible ? "NhatKy" : "BaoCao";
            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV Files|*.csv",
                Title = "Xuất báo cáo",
                FileName = $"FaceID_{suffix}_{DateTime.Now:yyyyMMdd}.csv"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var writer = new System.IO.StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        var headers = new List<string>();
                        foreach (DataGridViewColumn col in grid.Columns)
                            headers.Add(col.HeaderText);
                        writer.WriteLine(string.Join(",", headers));

                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            var cells = new List<string>();
                            foreach (DataGridViewCell cell in row.Cells)
                                cells.Add($"\"{cell.Value}\"");
                            writer.WriteLine(string.Join(",", cells));
                        }
                    }
                    MessageBox.Show($"Xuất thành công!\n{sfd.FileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng in sẽ được phát triển thêm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void UpdateSummary(int totalDays, int presentDays, int lateDays, int absentDays)
        {
            lblTotalDaysValue.Text   = totalDays.ToString();
            lblPresentDaysValue.Text = presentDays.ToString();
            lblLateDaysValue.Text    = lateDays.ToString();
            lblAbsentDaysValue.Text  = absentDays.ToString();
        }
    }
}

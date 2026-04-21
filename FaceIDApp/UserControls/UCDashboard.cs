using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FaceIDApp.Data;

namespace FaceIDApp.UserControls
{
    public partial class UCDashboard : UserControl
    {
        private Label[] cardValues;
        private DataGridView _dgvPending;
        private static readonly (string title, string icon, Color c1, Color c2)[] CardDefs = new[]
        {
            ("Tổng NV",    "👥", Color.FromArgb( 59, 130, 246), Color.FromArgb( 37,  99, 235)),
            ("Có mặt",     "✅", Color.FromArgb( 34, 197,  94), Color.FromArgb( 22, 163,  74)),
            ("Đi trễ",     "⏰", Color.FromArgb(234, 179,   8), Color.FromArgb(202, 138,   4)),
            ("Vắng mặt",   "❌", Color.FromArgb(239,  68,  68), Color.FromArgb(220,  38,  38)),
            ("Nghỉ phép",  "📋", Color.FromArgb(168, 85, 247), Color.FromArgb(147, 51, 234)),
            ("Chưa CC",    "⏳", Color.FromArgb(107, 114, 128), Color.FromArgb( 75,  85,  99)),
            ("Đã ĐK mặt",  "📸", Color.FromArgb( 56, 189, 248), Color.FromArgb( 14, 165, 233)),
            ("Chưa ĐK mặt","🚫", Color.FromArgb(251, 146, 60), Color.FromArgb(249, 115, 22))
        };

        public UCDashboard()
        {
            InitializeComponent();
            SetupCards();
            SetupGrid();
            SetupPendingLeavePanel();
        }

        private void SetupCards()
        {
            pnlCards.Controls.Clear();
            cardValues = new Label[CardDefs.Length];

            for (int i = 0; i < CardDefs.Length; i++)
            {
                var def = CardDefs[i];
                var card = new Panel
                {
                    Size = new Size(145, 100),
                    Margin = new Padding(8),
                    BackColor = Color.Transparent,
                    Tag = i
                };

                var valLbl = new Label
                {
                    Text = "0", Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                    ForeColor = Color.White, TextAlign = ContentAlignment.BottomLeft,
                    Location = new Point(14, 10), Size = new Size(120, 40),
                    BackColor = Color.Transparent
                };
                cardValues[i] = valLbl;

                var titleLbl = new Label
                {
                    Text = $"{def.icon} {def.title}",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(220, 220, 255),
                    Location = new Point(14, 58), AutoSize = true,
                    BackColor = Color.Transparent
                };

                int idx = i;
                card.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    using (var path = RoundedRect(rect, 12))
                    using (var brush = new LinearGradientBrush(rect, CardDefs[idx].c1, CardDefs[idx].c2, 135F))
                    {
                        g.FillPath(brush, path);
                    }
                };

                card.Controls.Add(valLbl);
                card.Controls.Add(titleLbl);
                pnlCards.Controls.Add(card);
            }
        }

        private void SetupGrid()
        {
            dgvToday.Columns.Clear();
            dgvToday.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "STT", HeaderText = "#", Width = 40 },
                new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "Mã NV", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Họ tên", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Dept", HeaderText = "Phòng ban", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Shift", HeaderText = "Ca", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "CheckIn", HeaderText = "Giờ vào", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "CheckOut", HeaderText = "Giờ ra", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Late", HeaderText = "Trễ (phút)", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Hours", HeaderText = "Giờ làm", Width = 70 }
            });
            dgvToday.EnableHeadersVisualStyles = false;
            dgvToday.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvToday.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvToday.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvToday.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 41, 59);
            dgvToday.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvToday.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvToday.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvToday.GridColor = Color.FromArgb(226, 232, 240);
        }

        public async void RefreshData()
        {
            try
            {
                // Stats
                var stats = await AppDatabase.Repository.GetDashboardStatsAsync();
                cardValues[0].Text = stats.TotalEmployees.ToString();
                cardValues[1].Text = stats.PresentCount.ToString();
                cardValues[2].Text = stats.LateCount.ToString();
                cardValues[3].Text = stats.AbsentCount.ToString();
                cardValues[4].Text = stats.LeaveCount.ToString();
                cardValues[5].Text = stats.NotYetCount.ToString();
                cardValues[6].Text = stats.FaceRegistered.ToString();
                cardValues[7].Text = stats.FaceNotRegistered.ToString();

                // Today attendance
                var todayList = await AppDatabase.Repository.GetTodayAttendanceViewAsync();
                dgvToday.Rows.Clear();
                int idx = 1;
                foreach (var att in todayList)
                {
                    var checkIn  = att.CheckIn?.ToString("HH:mm:ss")  ?? "--:--:--";
                    var checkOut = att.CheckOut?.ToString("HH:mm:ss") ?? "--:--:--";
                    var status   = TranslateStatus(att.Status);
                    var hours    = att.WorkingHours?.ToString("F1") ?? "-";

                    var rowIdx = dgvToday.Rows.Add(idx++, att.EmployeeCode, att.FullName,
                        att.DepartmentName ?? "-", att.ShiftName ?? "-",
                        checkIn, checkOut, status, att.LateMinutes ?? 0, hours);

                    var cell = dgvToday.Rows[rowIdx].Cells["Status"];
                    switch (att.Status)
                    {
                        case "Present":      cell.Style.ForeColor = Color.FromArgb(34, 197, 94);   break;
                        case "Late":
                        case "LateAndEarly": cell.Style.ForeColor = Color.FromArgb(234, 179, 8);   break;
                        case "Absent":       cell.Style.ForeColor = Color.FromArgb(239, 68, 68);   break;
                        case "NotYet":       cell.Style.ForeColor = Color.FromArgb(107, 114, 128); break;
                    }
                }

                // Pending leave requests
                await LoadPendingLeavesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard error: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadPendingLeavesAsync()
        {
            if (_dgvPending == null) return;
            try
            {
                var pending = await AppDatabase.Repository.GetLeaveRequestsAsync(null, "Pending");
                _dgvPending.Rows.Clear();
                foreach (var lr in pending)
                    _dgvPending.Rows.Add(
                        lr.EmployeeCode, lr.EmployeeName,
                        TranslateLeaveType(lr.LeaveType),
                        lr.StartDate.ToString("dd/MM/yyyy"),
                        lr.EndDate.ToString("dd/MM/yyyy"),
                        lr.TotalDays);
            }
            catch { /* không block dashboard */ }
        }

        private static string TranslateLeaveType(string t)
        {
            switch (t)
            {
                case "Annual":    return "Phép năm";
                case "Sick":      return "Ốm đau";
                case "Personal":  return "Việc riêng";
                case "Maternity": return "Thai sản";
                case "Unpaid":    return "Không lương";
                default:          return t;
            }
        }


        private string TranslateStatus(string status)
        {
            switch (status)
            {
                case "Present": return "Có mặt";
                case "Late": return "Đi trễ";
                case "EarlyLeave": return "Về sớm";
                case "LateAndEarly": return "Trễ+Sớm";
                case "Absent": return "Vắng";
                case "Leave": return "Nghỉ phép";
                case "NotYet": return "Chưa CC";
                case "Holiday": return "Ngày lễ";
                case "DayOff": return "Ngày nghỉ";
                default: return status ?? "-";
            }
        }

        private void SetupPendingLeavePanel()
        {
            // Label tiêu đề
            var lblPending = new Label
            {
                Text = "📋 Đơn nghỉ phép đang chờ duyệt",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Bottom, Height = 26,
                BackColor = Color.FromArgb(255, 251, 235),
                Padding = new Padding(8, 4, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _dgvPending = new DataGridView
            {
                Dock = DockStyle.Bottom, Height = 140,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 30, RowTemplate = { Height = 26 },
                Font = new Font("Segoe UI", 9F),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(226, 232, 240)
            };
            _dgvPending.EnableHeadersVisualStyles = false;
            _dgvPending.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(234, 179, 8);
            _dgvPending.ColumnHeadersDefaultCellStyle.ForeColor  = Color.FromArgb(15, 23, 42);
            _dgvPending.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvPending.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(234, 179, 8);
            _dgvPending.DefaultCellStyle.SelectionBackColor      = Color.FromArgb(255, 243, 195);
            _dgvPending.DefaultCellStyle.SelectionForeColor      = Color.Black;
            _dgvPending.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 253, 230);

            _dgvPending.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "PCode",  HeaderText = "Mã NV",     Width = 80  },
                new DataGridViewTextBoxColumn { Name = "PName",  HeaderText = "Họ tên",    Width = 140 },
                new DataGridViewTextBoxColumn { Name = "PType",  HeaderText = "Loại nghỉ", Width = 90  },
                new DataGridViewTextBoxColumn { Name = "PFrom",  HeaderText = "Từ ngày",   Width = 90  },
                new DataGridViewTextBoxColumn { Name = "PTo",    HeaderText = "Đến ngày",  Width = 90  },
                new DataGridViewTextBoxColumn { Name = "PDays",  HeaderText = "Số ngày",   Width = 70  }
            });

            this.Controls.Add(_dgvPending);
            this.Controls.Add(lblPending);
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
}

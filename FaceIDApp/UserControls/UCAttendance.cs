using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceIDApp.Data;


namespace FaceIDApp.UserControls
{
    public partial class UCAttendance : UserControl
    {
        private Timer timerDateTime;
        private Timer timerCamera;
        private WebcamCaptureService _webcam;
        private FaceRecognitionService _faceService;
        private List<FaceDataDto> _faceDataCache;
        private string _modelsDirectory;
        private string _lastCapturedPath;
        private double _faceMaxDistance = 0.45;
        private int _duplicateWindowMinutes = 5;
        private List<TodayAttendanceDto> _todayAttendanceData = new List<TodayAttendanceDto>();

        public UCAttendance()
        {
            InitializeComponent();
            SetupUI();
            SetupTimer();
            _webcam = new WebcamCaptureService();
            _faceService = new FaceRecognitionService();

            try { _modelsDirectory = ModelsDirectoryResolver.Resolve(); }
            catch { _modelsDirectory = null; }

            LoadTodayAttendanceAsync();
        }

        private void SetupUI()
        {
            dgvTodayAttendance.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvTodayAttendance.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTodayAttendance.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvTodayAttendance.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; dgvTodayAttendance.ColumnHeadersHeight = 35;
            dgvTodayAttendance.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvTodayAttendance.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvTodayAttendance.RowTemplate.Height = 30;
            dgvTodayAttendance.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 250);

            btnStartCamera.Click += BtnStartCamera_Click;
            btnStopCamera.Click += BtnStopCamera_Click;
            btnCapture.Click += BtnCapture_Click;
            btnCheckIn.Click += BtnCheckIn_Click;
            btnCheckOut.Click += BtnCheckOut_Click;

            // ─── Manual Check-in Panel ──────────────────────────────────────
            var pnlManual = new Panel
            {
                Height = 45, Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(10, 6, 10, 6)
            };
            var lblManual = new Label { Text = "📝 Thủ công:", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Location = new Point(10, 12) };
            var txtManualCode = new TextBox { Font = new Font("Segoe UI", 9.5F), Location = new System.Drawing.Point(110, 8),
                Size = new Size(120, 24), Text = "Mã NV", ForeColor = Color.Gray };
            txtManualCode.GotFocus += (s2, e2) => { if (txtManualCode.ForeColor == Color.Gray) { txtManualCode.Text = ""; txtManualCode.ForeColor = SystemColors.WindowText; } };
            txtManualCode.LostFocus += (s2, e2) => { if (string.IsNullOrWhiteSpace(txtManualCode.Text)) { txtManualCode.Text = "Mã NV"; txtManualCode.ForeColor = Color.Gray; } };
            var cboManualAction = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F), Location = new Point(240, 8), Size = new Size(110, 24) };
            cboManualAction.Items.AddRange(new[] { "Check-in", "Check-out" });
            cboManualAction.SelectedIndex = 0;
            var btnManualSubmit = new Button { Text = "Xác nhận", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(59, 130, 246),
                FlatStyle = FlatStyle.Flat, Size = new Size(90, 28), Location = new Point(360, 7), Cursor = Cursors.Hand };
            btnManualSubmit.FlatAppearance.BorderSize = 0;
            btnManualSubmit.Click += async (s, ev) =>
            {
                var code = txtManualCode.Text.Trim();
                if (string.IsNullOrWhiteSpace(code)) { MessageBox.Show("Nhập mã NV!", "Thông báo"); return; }
                try
                {
                    var employees = await AppDatabase.Repository.GetEmployeesAsync(true);
                    var emp = employees.FirstOrDefault(e => e.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                    if (emp == null) { MessageBox.Show($"Không tìm thấy NV mã '{code}'!", "Lỗi"); return; }
                    var now = DateTime.Now;
                    if (cboManualAction.SelectedIndex == 0) // Check-in
                    {
                        var existing = await AppDatabase.Repository.GetTodayAttendanceAsync(emp.Id);
                        if (existing?.CheckIn != null) { MessageBox.Show("NV đã check-in rồi!"); return; }
                        var cd = await GetCooldownRemainingSecondsAsync(emp.Id, "CheckIn");
                        if (cd > 0) { MessageBox.Show($"Vui lòng đợi {cd}s trước khi check-in lại!", "Thông báo"); return; }
                        var attId = await AppDatabase.Repository.CheckInAsync(emp.Id, emp.DefaultShiftId, now, null, null, "Manual");
                        if (emp.DefaultShiftId.HasValue)
                        {
                            var shift = await AppDatabase.Repository.GetWorkShiftByIdAsync(emp.DefaultShiftId.Value);
                            if (shift != null)
                            {
                                var status = AttendanceSchedule.CalculateStatus(now.TimeOfDay, null, shift.StartTime, shift.EndTime, shift.LateThreshold, shift.EarlyThreshold);
                                var lateMin = AttendanceSchedule.CalculateLateMinutes(now.TimeOfDay, shift.StartTime, shift.LateThreshold);
                                await AppDatabase.Repository.UpdateCheckInStatusAsync(emp.Id, status, lateMin);
                            }
                        }
                        await AppDatabase.Repository.InsertAttendanceLogAsync(attId, emp.Id, null, "CheckIn", "Manual", null, null, null, null, "Success");
                        UpdateStatus($"✅ Manual check-in: {emp.FullName} lúc {now:HH:mm:ss}", Color.FromArgb(46, 204, 113));
                    }
                    else // Check-out
                    {
                        var existing = await AppDatabase.Repository.GetTodayAttendanceAsync(emp.Id);
                        if (existing == null || !existing.CheckIn.HasValue) { MessageBox.Show("NV chưa check-in!"); return; }
                        if (existing.CheckOut.HasValue) { MessageBox.Show("NV đã check-out rồi!"); return; }
                        var cd = await GetCooldownRemainingSecondsAsync(emp.Id, "CheckOut");
                        if (cd > 0) { MessageBox.Show($"Vui lòng đợi {cd}s trước khi check-out lại!", "Thông báo"); return; }
                        WorkShiftDto shift = null;
                        if (emp.DefaultShiftId.HasValue) shift = await AppDatabase.Repository.GetWorkShiftByIdAsync(emp.DefaultShiftId.Value);
                        string st = "Present"; int late = 0, early = 0, work = (int)(now - existing.CheckIn.Value).TotalMinutes;
                        if (shift != null)
                        {
                            st = AttendanceSchedule.CalculateStatus(existing.CheckIn.Value.TimeOfDay, now.TimeOfDay, shift.StartTime, shift.EndTime, shift.LateThreshold, shift.EarlyThreshold);
                            late = AttendanceSchedule.CalculateLateMinutes(existing.CheckIn.Value.TimeOfDay, shift.StartTime, shift.LateThreshold);
                            early = AttendanceSchedule.CalculateEarlyMinutes(now.TimeOfDay, shift.EndTime, shift.EarlyThreshold);
                            work = AttendanceSchedule.CalculateWorkingMinutes(existing.CheckIn.Value, now, shift.BreakMinutes);
                        }
                        await AppDatabase.Repository.CheckOutAsync(emp.Id, now, null, null, st, late, early, work, "Manual");
                        await AppDatabase.Repository.InsertAttendanceLogAsync(existing.Id, emp.Id, null, "CheckOut", "Manual", null, null, null, null, "Success");
                        UpdateStatus($"✅ Manual check-out: {emp.FullName} lúc {now:HH:mm:ss}", Color.FromArgb(46, 204, 113));
                    }
                    txtManualCode.Clear();
                    LoadTodayAttendanceAsync();
                }
                catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            var btnEditAttendance = new Button
            {
                Text = "✎ Sửa chấm công", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(245, 158, 11),
                FlatStyle = FlatStyle.Flat, Size = new Size(130, 28), Location = new Point(470, 7), Cursor = Cursors.Hand
            };
            btnEditAttendance.FlatAppearance.BorderSize = 0;
            btnEditAttendance.Click += BtnEditAttendance_Click;

            pnlManual.Controls.AddRange(new Control[] { lblManual, txtManualCode, cboManualAction, btnManualSubmit, btnEditAttendance });
            pnlTodayList.Controls.Add(pnlManual);
        }

        private void SetupTimer()
        {
            timerDateTime = new Timer { Interval = 1000 };
            timerDateTime.Tick += (s, e) => lblDateTime.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            timerDateTime.Start();
        }

        public void RefreshData()
        {
            LoadTodayAttendanceAsync();
        }

        private async void LoadTodayAttendanceAsync()
        {
            try
            {
                var todayList = await AppDatabase.Repository.GetTodayAttendanceViewAsync();
                _todayAttendanceData = todayList ?? new List<TodayAttendanceDto>();
                dgvTodayAttendance.Rows.Clear();
                int idx = 1;
                foreach (var att in todayList)
                {
                    var checkIn = att.CheckIn.HasValue ? att.CheckIn.Value.ToString("HH:mm:ss") : "--:--:--";
                    var checkOut = att.CheckOut.HasValue ? att.CheckOut.Value.ToString("HH:mm:ss") : "--:--:--";
                    var status = TranslateStatus(att.Status);
                    var soGio = att.WorkingHours.HasValue ? $"{att.WorkingHours:F1}h" : "—";
                    var rowIdx = dgvTodayAttendance.Rows.Add(idx++, att.EmployeeCode, att.FullName,
                        att.ShiftName ?? "—", checkIn, checkOut, soGio, status);

                    // Tô màu trạng thái
                    var cell = dgvTodayAttendance.Rows[rowIdx].Cells["colTrangThai"];
                    switch (att.Status)
                    {
                        case "Present":     cell.Style.BackColor = Color.FromArgb(212, 237, 218); cell.Style.ForeColor = Color.FromArgb(21, 87, 36);  break;
                        case "Late":
                        case "LateAndEarly":cell.Style.BackColor = Color.FromArgb(255, 243, 205); cell.Style.ForeColor = Color.FromArgb(133, 100, 4); break;
                        case "Absent":      cell.Style.BackColor = Color.FromArgb(248, 215, 218); cell.Style.ForeColor = Color.FromArgb(114, 28, 36); break;
                    }
                }

                // Cache face data for recognition
                _faceDataCache = await AppDatabase.Repository.GetAllActiveFaceDataAsync();

                // Load recognition config from system_settings (fallbacks used if missing)
                _faceMaxDistance = await AppDatabase.Repository.GetSystemSettingDoubleAsync("face.max_distance", 0.45);
                _duplicateWindowMinutes = await AppDatabase.Repository.GetSystemSettingIntAsync("face.duplicate_window_minutes", 5);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Attendance load error: {ex.Message}");
            }
        }

        // Returns remaining cooldown seconds; 0 if not in cooldown.
        private async Task<int> GetCooldownRemainingSecondsAsync(int employeeId, string logType)
        {
            if (_duplicateWindowMinutes <= 0) return 0;
            var last = await AppDatabase.Repository.GetLastSuccessLogTimeAsync(employeeId, logType);
            if (!last.HasValue) return 0;
            var elapsed = DateTime.Now - last.Value;
            var remaining = TimeSpan.FromMinutes(_duplicateWindowMinutes) - elapsed;
            return remaining.TotalSeconds > 0 ? (int)Math.Ceiling(remaining.TotalSeconds) : 0;
        }

        private void BtnStartCamera_Click(object sender, EventArgs e)
        {
            try
            {
                _webcam.Start(0);
                timerCamera = new Timer { Interval = 50 };
                timerCamera.Tick += (s, ev) =>
                {
                    var frame = _webcam.CaptureFrame();
                    if (frame != null)
                    {
                        picCamera.Image?.Dispose();
                        picCamera.Image = frame;
                    }
                };
                timerCamera.Start();
                btnStartCamera.Enabled = false;
                btnStopCamera.Enabled = true;
                UpdateStatus("📷 Camera đang hoạt động", Color.FromArgb(46, 204, 113));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không mở được camera!\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStopCamera_Click(object sender, EventArgs e)
        {
            StopCamera();
        }

        private void StopCamera()
        {
            timerCamera?.Stop();
            timerCamera?.Dispose();
            timerCamera = null;
            _webcam.Stop();

            picCamera.Image?.Dispose();
            picCamera.Image = null;
            picCamera.BackColor = Color.Black;

            btnStartCamera.Enabled = true;
            btnStopCamera.Enabled = false;
            UpdateStatus("📷 Camera đã tắt", Color.FromArgb(149, 165, 166));
        }

        private async void BtnCapture_Click(object sender, EventArgs e)
        {
            if (!_webcam.IsStarted)
            {
                MessageBox.Show("Vui lòng bật camera trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_modelsDirectory))
            {
                MessageBox.Show("Không tìm thấy thư mục model AI!\nKiểm tra lại thư mục models/", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var frame = _webcam.CaptureFrame();
            if (frame == null) return;

            _lastCapturedPath = ImageStorage.StoreBitmap(frame, "attendance");
            UpdateStatus("⏳ Đang nhận diện...", Color.FromArgb(243, 156, 18));
            Application.DoEvents();

            try
            {
                // Refresh face data cache from DB every time
                _faceDataCache = await AppDatabase.Repository.GetAllActiveFaceDataAsync();

                if (_faceDataCache == null || _faceDataCache.Count == 0)
                {
                    UpdateStatus("⚠ Chưa có dữ liệu khuôn mặt nào trong hệ thống!", Color.FromArgb(243, 156, 18));
                    return;
                }

                var results = _faceService.RecognizeFaces(_lastCapturedPath, _modelsDirectory, FaceRecog.Model.Hog, _faceDataCache, _faceMaxDistance);

                if (results.Count == 0)
                {
                    UpdateStatus("❌ Không phát hiện khuôn mặt!", Color.FromArgb(231, 76, 60));
                    return;
                }

                var match = results.FirstOrDefault(r => r.Status == "Matched");
                if (match == null)
                {
                    var bestResult = results.OrderBy(r => r.Distance).First();
                    UpdateStatus($"❌ Không nhận dạng được — Khuôn mặt chưa đăng ký! (distance: {bestResult.Distance:F3})", Color.FromArgb(231, 76, 60));
                    // Log failed attempt
                    _ = AppDatabase.Repository.InsertAttendanceLogAsync(null, null, null, "Attempt", "Face", null,
                        null, null, _lastCapturedPath, "Fail", "UnknownFace");
                    return;
                }

                var confidence = (float)match.Confidence;
                UpdateEmployeeInfo(match.EmployeeName, match.EmployeeCode, "", "");
                UpdateStatus($"✅ Nhận dạng: {match.EmployeeName} ({confidence:P0})", Color.FromArgb(46, 204, 113));

                // Load employee photo if exists (Prefer AvatarPath, fallback to face image)
                var emp = await AppDatabase.Repository.GetEmployeeByIdAsync(match.EmployeeId.Value);
                string photoPath = null;

                if (emp != null && !string.IsNullOrEmpty(emp.AvatarPath) && File.Exists(emp.AvatarPath))
                    photoPath = emp.AvatarPath;
                else
                {
                    var faceData = _faceDataCache.FirstOrDefault(f => f.EmployeeId == match.EmployeeId);
                    if (faceData != null && File.Exists(faceData.ImagePath))
                        photoPath = faceData.ImagePath;
                }

                if (!string.IsNullOrEmpty(photoPath))
                {
                    try 
                    {
                        using (var img = System.Drawing.Image.FromFile(photoPath))
                        {
                            picEmployeePhoto.Image = new Bitmap(img);
                        }
                    } 
                    catch { picEmployeePhoto.Image = null; }
                }
                else
                {
                    picEmployeePhoto.Image = null;
                }

                // Store match info for CheckIn/CheckOut
                btnCheckIn.Tag = match;
                btnCheckOut.Tag = match;
                btnCheckIn.Enabled = true;
                btnCheckOut.Enabled = true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Lỗi nhận diện: {ex.Message}", Color.FromArgb(231, 76, 60));
                System.Diagnostics.Debug.WriteLine($"Recognition error: {ex}");
            }
        }

        private async void BtnCheckIn_Click(object sender, EventArgs e)
        {
            var match = btnCheckIn.Tag as FaceMatchItem;
            if (match == null || !match.EmployeeId.HasValue)
            {
                MessageBox.Show("Vui lòng chụp và nhận diện trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int empId = match.EmployeeId.Value;
                var existing = await AppDatabase.Repository.GetTodayAttendanceAsync(empId);
                if (existing != null && existing.CheckIn.HasValue)
                {
                    MessageBox.Show("Nhân viên đã check-in hôm nay rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Cooldown guard — chặn quẹt liên tục trong khoảng face.duplicate_window_minutes
                var cooldown = await GetCooldownRemainingSecondsAsync(empId, "CheckIn");
                if (cooldown > 0)
                {
                    UpdateStatus($"⏳ Vui lòng đợi {cooldown}s trước khi check-in lại", Color.FromArgb(243, 156, 18));
                    return;
                }

                // Get employee's shift
                var emp = await AppDatabase.Repository.GetEmployeeByIdAsync(empId);
                int? shiftId = emp?.DefaultShiftId;
                WorkShiftDto shift = null;
                if (shiftId.HasValue)
                    shift = await AppDatabase.Repository.GetWorkShiftByIdAsync(shiftId.Value);

                var now = DateTime.Now;
                var attId = await AppDatabase.Repository.CheckInAsync(empId, shiftId, now,
                    _lastCapturedPath, (float)match.Confidence, "Face");

                // Calculate and update status
                if (shift != null)
                {
                    var status = AttendanceSchedule.CalculateStatus(now.TimeOfDay, null, shift.StartTime, shift.EndTime, shift.LateThreshold, shift.EarlyThreshold);
                    var lateMin = AttendanceSchedule.CalculateLateMinutes(now.TimeOfDay, shift.StartTime, shift.LateThreshold);
                    await AppDatabase.Repository.UpdateCheckInStatusAsync(empId, status, lateMin);
                }

                // Log
                await AppDatabase.Repository.InsertAttendanceLogAsync(attId, empId, null, "CheckIn", "Face",
                    match.MatchedFaceDataId, (float)match.Confidence, (float)match.Distance,
                    _lastCapturedPath, "Success");

                UpdateCheckInTime(now);
                UpdateStatus($"✅ Check-in thành công: {match.EmployeeName} lúc {now:HH:mm:ss}", Color.FromArgb(46, 204, 113));
                LoadTodayAttendanceAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi check-in:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCheckOut_Click(object sender, EventArgs e)
        {
            var match = btnCheckOut.Tag as FaceMatchItem;
            if (match == null || !match.EmployeeId.HasValue)
            {
                MessageBox.Show("Vui lòng chụp và nhận diện trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int empId = match.EmployeeId.Value;
                var existing = await AppDatabase.Repository.GetTodayAttendanceAsync(empId);
                if (existing == null || !existing.CheckIn.HasValue)
                {
                    MessageBox.Show("Nhân viên chưa check-in hôm nay!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (existing.CheckOut.HasValue)
                {
                    MessageBox.Show("Nhân viên đã check-out rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Cooldown guard — chặn quẹt liên tục trong khoảng face.duplicate_window_minutes
                var cooldown = await GetCooldownRemainingSecondsAsync(empId, "CheckOut");
                if (cooldown > 0)
                {
                    UpdateStatus($"⏳ Vui lòng đợi {cooldown}s trước khi check-out lại", Color.FromArgb(243, 156, 18));
                    return;
                }

                var emp = await AppDatabase.Repository.GetEmployeeByIdAsync(empId);
                WorkShiftDto shift = null;
                if (emp?.DefaultShiftId != null)
                    shift = await AppDatabase.Repository.GetWorkShiftByIdAsync(emp.DefaultShiftId.Value);

                var now = DateTime.Now;
                var checkInTime = existing.CheckIn.Value;

                string status = "Present";
                int lateMin = 0, earlyMin = 0, workMin = 0;

                if (shift != null)
                {
                    status = AttendanceSchedule.CalculateStatus(checkInTime.TimeOfDay, now.TimeOfDay, shift.StartTime, shift.EndTime, shift.LateThreshold, shift.EarlyThreshold);
                    lateMin = AttendanceSchedule.CalculateLateMinutes(checkInTime.TimeOfDay, shift.StartTime, shift.LateThreshold);
                    earlyMin = AttendanceSchedule.CalculateEarlyMinutes(now.TimeOfDay, shift.EndTime, shift.EarlyThreshold);
                    workMin = AttendanceSchedule.CalculateWorkingMinutes(checkInTime, now, shift.BreakMinutes);
                }
                else
                {
                    workMin = (int)(now - checkInTime).TotalMinutes;
                }

                await AppDatabase.Repository.CheckOutAsync(empId, now, _lastCapturedPath,
                    (float)match.Confidence, status, lateMin, earlyMin, workMin, "Face");

                await AppDatabase.Repository.InsertAttendanceLogAsync(existing.Id, empId, null, "CheckOut", "Face",
                    match.MatchedFaceDataId, (float)match.Confidence, (float)match.Distance,
                    _lastCapturedPath, "Success");

                UpdateStatus($"✅ Check-out thành công: {match.EmployeeName} lúc {now:HH:mm:ss}", Color.FromArgb(46, 204, 113));
                LoadTodayAttendanceAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi check-out:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnEditAttendance_Click(object sender, EventArgs e)
        {
            if (dgvTodayAttendance.CurrentRow == null)
            {
                MessageBox.Show("Chọn một dòng chấm công để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rowIdx = dgvTodayAttendance.CurrentRow.Index;
            if (rowIdx < 0 || rowIdx >= _todayAttendanceData.Count) return;
            var att = _todayAttendanceData[rowIdx];

            using (var dlg = new AttendanceEditDialog(att))
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var r = dlg.Result;
                try
                {
                    // Tính lại working_minutes
                    int workMin = 0;
                    if (r.CheckIn.HasValue && r.CheckOut.HasValue)
                        workMin = (int)(r.CheckOut.Value - r.CheckIn.Value).TotalMinutes;

                    // Lấy record ID hiện tại qua GetTodayAttendanceAsync
                    var rec = await AppDatabase.Repository.GetTodayAttendanceAsync(att.EmployeeId);
                    if (rec == null) { MessageBox.Show("Không tìm thấy bản ghi chấm công!", "Lỗi"); return; }

                    await AppDatabase.Repository.UpdateAttendanceRecordManualAsync(
                        rec.Id, r.CheckIn, r.CheckOut, r.Status,
                        r.LateMinutes, r.EarlyMinutes, workMin,
                        r.ManualEditReason, AppSession.CurrentUser?.Id);

                    await AppDatabase.Repository.InsertAuditLogAsync(
                        AppSession.CurrentUser?.Id, att.EmployeeId,
                        "ATTENDANCE_EDIT", "attendance_records", rec.Id.ToString(),
                        $"Sửa thủ công chấm công {att.FullName} ngày {DateTime.Today:dd/MM/yyyy}: {r.ManualEditReason}");

                    await AppDatabase.Repository.InsertAttendanceLogAsync(
                        rec.Id, att.EmployeeId, null, "CheckIn", "Manual",
                        null, null, null, null, "Success", "ManualEdit");

                    UpdateStatus($"✅ Đã sửa chấm công: {att.FullName}", Color.FromArgb(46, 204, 113));
                    LoadTodayAttendanceAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi sửa chấm công:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string TranslateStatus(string status)
        {
            switch (status)
            {
                case "Present": return "Đúng giờ";
                case "Late":
                case "LateAndEarly": return "Đi trễ";
                case "EarlyLeave": return "Về sớm";
                case "Absent": return "Vắng";
                case "Leave": return "Nghỉ phép";
                case "NotYet": return "Chờ xử lý";
                default: return status ?? "—";
            }
        }

        // Public methods
        public void UpdateEmployeeInfo(string name, string code, string department, string position)
        {
            lblEmployeeName.Text = name ?? "—";
            lblEmployeeCode.Text = $"Mã NV: {code}";
            lblDepartment.Text = $"🏢 Phòng ban: {department}";
            lblPosition.Text = $"💼 Chức vụ: {position}";
        }

        public void UpdateStatus(string status, Color color)
        {
            lblStatus.Text = status;
            lblStatus.ForeColor = color;
        }

        public void UpdateCheckInTime(DateTime time)
        {
            lblCheckInTime.Text = $"⏰ Giờ vào: {time:HH:mm:ss}";
        }

        public void SetCameraImage(System.Drawing.Image image)
        {
            picCamera.Image?.Dispose();
            picCamera.Image = image;
            if (image != null)
            {
                picCamera.BackColor = Color.FromArgb(44, 62, 80);
            }
        }
        public void SetEmployeePhoto(System.Drawing.Image image) { picEmployeePhoto.Image = image; }

        // Dispose is handled in Designer.cs
        // Call StopCamera when control is removed from parent
        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopCamera();
            _webcam?.Dispose();
            base.OnHandleDestroyed(e);
        }
    }

    internal class AttendanceEditResult
    {
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Status { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public string ManualEditReason { get; set; }
    }

    internal class AttendanceEditDialog : Form
    {
        public AttendanceEditResult Result { get; private set; }
        private DateTimePicker dtpCheckIn, dtpCheckOut;
        private CheckBox chkHasCheckIn, chkHasCheckOut;
        private ComboBox cboStatus;
        private TextBox txtReason;

        public AttendanceEditDialog(TodayAttendanceDto att)
        {
            this.Text = $"Sửa thủ công — {att.FullName} ({DateTime.Today:dd/MM/yyyy})";
            this.Size = new Size(420, 340);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 15;
            AddLabel("Giờ vào (Check-in):", 15, y);
            chkHasCheckIn = new CheckBox { Text = "Có", Location = new Point(200, y), AutoSize = true, Checked = att.CheckIn.HasValue };
            dtpCheckIn = new DateTimePicker { Location = new Point(15, y += 22), Size = new Size(370, 28), Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm:ss dd/MM/yyyy", Value = att.CheckIn ?? DateTime.Now, Enabled = att.CheckIn.HasValue };
            chkHasCheckIn.CheckedChanged += (s, e) => dtpCheckIn.Enabled = chkHasCheckIn.Checked;
            this.Controls.AddRange(new Control[] { chkHasCheckIn, dtpCheckIn });

            AddLabel("Giờ ra (Check-out):", 15, y += 35);
            chkHasCheckOut = new CheckBox { Text = "Có", Location = new Point(200, y), AutoSize = true, Checked = att.CheckOut.HasValue };
            dtpCheckOut = new DateTimePicker { Location = new Point(15, y += 22), Size = new Size(370, 28), Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm:ss dd/MM/yyyy", Value = att.CheckOut ?? DateTime.Now, Enabled = att.CheckOut.HasValue };
            chkHasCheckOut.CheckedChanged += (s, e) => dtpCheckOut.Enabled = chkHasCheckOut.Checked;
            this.Controls.AddRange(new Control[] { chkHasCheckOut, dtpCheckOut });

            AddLabel("Trạng thái:", 15, y += 35);
            cboStatus = new ComboBox { Location = new Point(15, y += 22), Size = new Size(370, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new[] { "Present", "Late", "EarlyLeave", "LateAndEarly", "Absent", "Leave" });
            cboStatus.SelectedItem = att.Status ?? "Present";
            if (cboStatus.SelectedIndex < 0) cboStatus.SelectedIndex = 0;
            this.Controls.Add(cboStatus);

            AddLabel("Lý do sửa (bắt buộc):", 15, y += 35);
            txtReason = new TextBox { Location = new Point(15, y += 22), Size = new Size(370, 50), Multiline = true };
            this.Controls.Add(txtReason);

            var btnOk = new Button
            {
                Text = "Lưu", DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.FromArgb(245, 158, 11),
                FlatStyle = FlatStyle.Flat, Size = new Size(120, 36), Location = new Point(135, y + 60), Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtReason.Text))
                {
                    MessageBox.Show("Vui lòng nhập lý do sửa!", "Thông báo");
                    this.DialogResult = DialogResult.None;
                    return;
                }
                Result = new AttendanceEditResult
                {
                    CheckIn  = chkHasCheckIn.Checked  ? dtpCheckIn.Value  : (DateTime?)null,
                    CheckOut = chkHasCheckOut.Checked ? dtpCheckOut.Value : (DateTime?)null,
                    Status   = cboStatus.SelectedItem?.ToString() ?? "Present",
                    ManualEditReason = txtReason.Text.Trim()
                };
            };
            this.Controls.Add(btnOk);
            this.AcceptButton = btnOk;
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) });
        }
    }
}

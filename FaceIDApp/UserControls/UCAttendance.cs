using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceIDApp.Data;
using FaceRecog;

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
            dgvTodayAttendance.ColumnHeadersHeight = 35;
            dgvTodayAttendance.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgvTodayAttendance.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvTodayAttendance.RowTemplate.Height = 30;
            dgvTodayAttendance.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 250);

            btnStartCamera.Click += BtnStartCamera_Click;
            btnStopCamera.Click += BtnStopCamera_Click;
            btnCapture.Click += BtnCapture_Click;
            btnCheckIn.Click += BtnCheckIn_Click;
            btnCheckOut.Click += BtnCheckOut_Click;
        }

        private void SetupTimer()
        {
            timerDateTime = new Timer { Interval = 1000 };
            timerDateTime.Tick += (s, e) => lblDateTime.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            timerDateTime.Start();
        }

        private async void LoadTodayAttendanceAsync()
        {
            try
            {
                var todayList = await AppDatabase.Repository.GetTodayAttendanceViewAsync();
                dgvTodayAttendance.Rows.Clear();
                int idx = 1;
                foreach (var att in todayList)
                {
                    var checkIn = att.CheckIn.HasValue ? att.CheckIn.Value.ToString("HH:mm:ss") : "--:--:--";
                    var checkOut = att.CheckOut.HasValue ? att.CheckOut.Value.ToString("HH:mm:ss") : "--:--:--";
                    var status = TranslateStatus(att.Status);
                    dgvTodayAttendance.Rows.Add(idx++, att.EmployeeCode, att.FullName, checkIn, checkOut, status);
                }

                // Cache face data for recognition
                _faceDataCache = await AppDatabase.Repository.GetAllActiveFaceDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Attendance load error: {ex.Message}");
            }
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

                var results = _faceService.RecognizeFaces(_lastCapturedPath, _modelsDirectory, Model.Hog, _faceDataCache, 0.45);

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

                // Load employee photo if exists
                var faceData = _faceDataCache.FirstOrDefault(f => f.EmployeeId == match.EmployeeId);
                if (faceData != null && File.Exists(faceData.ImagePath))
                {
                    try { picEmployeePhoto.Image = System.Drawing.Image.FromFile(faceData.ImagePath); } catch { }
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

        public void SetCameraImage(System.Drawing.Image image) { picCamera.Image = image; }
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
}

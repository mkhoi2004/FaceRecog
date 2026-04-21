using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FaceIDApp.Data;
using FaceRecog;

namespace FaceIDApp.UserControls
{
    public partial class UCFaceRegistration : UserControl
    {
        private int capturedCount = 0;
        private List<PictureBox> capturedPictures;
        private WebcamCaptureService _webcam;
        private FaceRecognitionService _faceService;
        private Timer timerCamera;
        private string _modelsDirectory;

        // Store captured image paths and encodings
        private readonly string[] _capturedPaths = new string[5];
        private readonly string[] _capturedEncodings = new string[5];
        private readonly string[] _angles = { "Front", "Left", "Right", "Up", "Down" };

        // Employee list for dropdown
        private List<EmployeeDto> _employees;

        public UCFaceRegistration()
        {
            InitializeComponent();
            _webcam = new WebcamCaptureService();
            _faceService = new FaceRecognitionService();

            try { _modelsDirectory = ModelsDirectoryResolver.Resolve(); }
            catch { _modelsDirectory = null; }

            SetupUI();
            RefreshData();
        }

        private void SetupUI()
        {
            capturedPictures = new List<PictureBox> { pic1, pic2, pic3, pic4, pic5 };

            cboSelectEmployee.SelectedIndexChanged += CboSelectEmployee_SelectedIndexChanged;
            btnStartCamera.Click += BtnStartCamera_Click;
            btnStopCamera.Click += BtnStopCamera_Click;
            btnCapture.Click += BtnCapture_Click;
            btnClearAll.Click += BtnClearAll_Click;
            btnRegister.Click += BtnRegister_Click;
        }

        public async void RefreshData()
        {
            try
            {
                _employees = await AppDatabase.Repository.GetEmployeesAsync(true);
                cboSelectEmployee.Items.Clear();
                cboSelectEmployee.Items.Add("-- Chọn nhân viên --");
                foreach (var emp in _employees)
                    cboSelectEmployee.Items.Add($"{emp.Code} - {emp.FullName}");
                cboSelectEmployee.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Face reg refresh error: {ex.Message}");
            }
        }

        private void CboSelectEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSelectEmployee.SelectedIndex > 0 && _employees != null)
            {
                var emp = _employees[cboSelectEmployee.SelectedIndex - 1];
                var faceStatus = emp.IsFaceRegistered ? "✅ Đã đăng ký" : "❌ Chưa đăng ký";
                lblSelectedInfo.Text = $"Phòng ban: {emp.DepartmentName ?? "—"}\nTrạng thái Face ID: {faceStatus}";
                lblSelectedInfo.ForeColor = emp.IsFaceRegistered
                    ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
            }
            else
            {
                lblSelectedInfo.Text = "Phòng ban: ---\nTrạng thái Face ID: ---";
                lblSelectedInfo.ForeColor = Color.FromArgb(127, 140, 141);
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
        }

        private void BtnCapture_Click(object sender, EventArgs e)
        {
            if (cboSelectEmployee.SelectedIndex == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên trước khi chụp ảnh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (capturedCount >= 5)
            {
                MessageBox.Show("Đã chụp đủ 5 ảnh. Vui lòng đăng ký hoặc xóa để chụp lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_webcam.IsStarted)
            {
                MessageBox.Show("Vui lòng bật camera trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_modelsDirectory))
            {
                MessageBox.Show("Không tìm thấy thư mục model AI!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var frame = _webcam.CaptureFrame();
            if (frame == null) return;

            var imgPath = ImageStorage.StoreBitmap(frame, "face_register");

            // Extract encoding
            var encoding = _faceService.BuildEncodingData(imgPath, _modelsDirectory, Model.Hog);
            if (encoding == null)
            {
                MessageBox.Show($"Không phát hiện khuôn mặt (hoặc có nhiều hơn 1 khuôn mặt) trong ảnh!\nHãy chụp lại với góc: {_angles[capturedCount]}",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _capturedPaths[capturedCount] = imgPath;
            _capturedEncodings[capturedCount] = encoding;

            // Show in picture box
            capturedPictures[capturedCount].Image = System.Drawing.Image.FromFile(imgPath);
            capturedPictures[capturedCount].BackColor = Color.FromArgb(46, 204, 113);
            capturedCount++;
            UpdateProgress();

            MessageBox.Show($"✅ Đã chụp ảnh {capturedCount}/5 (góc: {_angles[capturedCount - 1]})", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa tất cả ảnh đã chụp?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                ClearAllCaptures();
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            if (cboSelectEmployee.SelectedIndex == 0)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (capturedCount < 1)
            {
                MessageBox.Show("Vui lòng chụp ít nhất 1 ảnh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var emp = _employees[cboSelectEmployee.SelectedIndex - 1];
            var confirmResult = MessageBox.Show($"Xác nhận đăng ký Face ID cho {emp.FullName}?\n({capturedCount} ảnh)",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes) return;

            try
            {
                for (int i = 0; i < capturedCount; i++)
                {
                    var storedPath = ImageStorage.StoreFaceImage(_capturedPaths[i], emp.Id, i + 1);
                    await AppDatabase.Repository.InsertFaceDataAsync(
                        emp.Id, _capturedEncodings[i], storedPath,
                        i + 1, _angles[i], 0.8f, null);
                }

                MessageBox.Show($"✅ Đăng ký Face ID thành công cho {emp.FullName}!\n{capturedCount} ảnh đã được lưu.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearAllCaptures();
                cboSelectEmployee.SelectedIndex = 0;
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng ký:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearAllCaptures()
        {
            foreach (var pic in capturedPictures)
            {
                pic.Image = null;
                pic.BackColor = Color.FromArgb(236, 240, 241);
            }
            capturedCount = 0;
            Array.Clear(_capturedPaths, 0, _capturedPaths.Length);
            Array.Clear(_capturedEncodings, 0, _capturedEncodings.Length);
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            lblProgress.Text = $"Đã chụp: {capturedCount}/5 ảnh";
            progressBar.Value = capturedCount;
        }

        public void SetCameraImage(System.Drawing.Image image) { picCamera.Image = image; }
        public void SetCapturedImage(int index, System.Drawing.Image image)
        {
            if (index >= 0 && index < capturedPictures.Count) capturedPictures[index].Image = image;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopCamera();
            _webcam?.Dispose();
            base.OnHandleDestroyed(e);
        }
    }
}

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

            // Nút xem ảnh đã đăng ký
            var btnViewRegistered = new System.Windows.Forms.Button
            {
                Text = "👁 Xem ảnh ĐK",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, 116, 139),
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(110, 30),
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnViewRegistered.FlatAppearance.BorderSize = 0;
            btnViewRegistered.Click += BtnViewRegistered_Click;
            // Đặt bên phải lblSelectedInfo
            btnViewRegistered.Location = new System.Drawing.Point(lblSelectedInfo.Right + 10, lblSelectedInfo.Top);
            btnViewRegistered.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            lblSelectedInfo.Parent.Controls.Add(btnViewRegistered);
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
            var pic = capturedPictures[capturedCount];
            pic.Image = System.Drawing.Image.FromFile(imgPath);
            pic.BackColor = Color.FromArgb(46, 204, 113);

            // Quality score overlay (ước lượng từ encoding length)
            float quality = 0.8f; // base quality nếu có face
            var qualityLabel = new Label
            {
                AutoSize = false,
                Size = new System.Drawing.Size(pic.Width, 18),
                Location = new System.Drawing.Point(0, pic.Height - 18),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold)
            };
            if (quality >= 0.8f)
            {
                qualityLabel.Text = $"✓ {_angles[capturedCount]}";
                qualityLabel.BackColor = Color.FromArgb(180, 34, 197, 94);
                qualityLabel.ForeColor = Color.White;
            }
            else if (quality >= 0.5f)
            {
                qualityLabel.Text = $"⚠ {_angles[capturedCount]}";
                qualityLabel.BackColor = Color.FromArgb(180, 234, 179, 8);
                qualityLabel.ForeColor = Color.Black;
            }
            else
            {
                qualityLabel.Text = $"✗ {_angles[capturedCount]}";
                qualityLabel.BackColor = Color.FromArgb(180, 239, 68, 68);
                qualityLabel.ForeColor = Color.White;
            }
            pic.Controls.Clear();
            pic.Controls.Add(qualityLabel);

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
                        i + 1, _angles[i], 0.8f, AppSession.CurrentUser?.EmployeeId);
                }

                // Ghi face_registration_logs
                await AppDatabase.Repository.InsertFaceRegistrationLogAsync(
                    emp.Id, "Register", AppSession.CurrentUser?.EmployeeId,
                    $"Registered {capturedCount} face images");

                // Cập nhật trạng thái is_face_registered
                await AppDatabase.Repository.UpdateEmployeeFaceStatusAsync(emp.Id, true);

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
                pic.Controls.Clear();
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

        private async void BtnViewRegistered_Click(object sender, EventArgs e)
        {
            if (cboSelectEmployee.SelectedIndex <= 0 || _employees == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var emp = _employees[cboSelectEmployee.SelectedIndex - 1];
            try
            {
                var faceList = await AppDatabase.Repository.GetFaceDataByEmployeeAsync(emp.Id);
                if (faceList == null || faceList.Count == 0)
                {
                    MessageBox.Show($"{emp.FullName} chưa có ảnh Face ID nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using (var dlg = new Form
                {
                    Text = $"Ảnh Face ID — {emp.FullName} ({faceList.Count} ảnh)",
                    Size = new System.Drawing.Size(650, 220),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, MinimizeBox = false, BackColor = Color.White
                })
                {
                    int x = 10;
                    foreach (var fd in faceList)
                    {
                        var pic = new PictureBox
                        {
                            Size = new System.Drawing.Size(110, 140),
                            Location = new System.Drawing.Point(x, 15),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            BorderStyle = BorderStyle.FixedSingle,
                            BackColor = Color.FromArgb(241, 245, 249)
                        };
                        if (!string.IsNullOrEmpty(fd.ImagePath) && System.IO.File.Exists(fd.ImagePath))
                        {
                            try { pic.Image = System.Drawing.Image.FromFile(fd.ImagePath); } catch { }
                        }
                        var lbl = new Label
                        {
                            Text = $"#{fd.ImageIndex} {fd.Angle ?? ""}",
                            Location = new System.Drawing.Point(x, 160),
                            Size = new System.Drawing.Size(110, 18),
                            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                            Font = new Font("Segoe UI", 8F),
                            ForeColor = Color.FromArgb(71, 85, 105)
                        };
                        dlg.Controls.Add(pic);
                        dlg.Controls.Add(lbl);
                        x += 120;
                    }
                    dlg.ShowDialog(this.FindForm());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopCamera();
            _webcam?.Dispose();
            base.OnHandleDestroyed(e);
        }
    }
}

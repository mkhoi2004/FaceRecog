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

            // --- Fix Title Spacing Issue ---
            lblTitle.AutoSize = false;
            lblTitle.Size = new Size(800, 45);
            lblTitle.Text = "📷 Đăng ký khuôn mặt";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(30, 41, 59); // Slate 800
            lblTitle.UseCompatibleTextRendering = true;

            lblInstruction.ForeColor = Color.FromArgb(71, 85, 105); // Slate 600
            lblInstruction.Font = new Font("Segoe UI", 9F, FontStyle.Italic);

            // --- Fix the "separated weirdly" layout with TableLayoutPanel ---
            pnlLeft.Dock = DockStyle.None;
            pnlRight.Dock = DockStyle.None;
            
            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Margin = new Padding(0, 0, 5, 0);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Margin = new Padding(5, 0, 0, 0);
            
            tlpMain.Controls.Add(pnlLeft, 0, 0);
            tlpMain.Controls.Add(pnlRight, 1, 0);
            
            pnlMain.Controls.Clear();
            pnlMain.Controls.Add(tlpMain);

            // --- Fix Camera Buttons to be uniform ---
            var tlpCamBtns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            tlpCamBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCamBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            
            btnStartCamera.Dock = DockStyle.Fill;
            btnStartCamera.Margin = new Padding(0, 5, 5, 5);
            btnStopCamera.Dock = DockStyle.Fill;
            btnStopCamera.Margin = new Padding(5, 5, 0, 5);
            
            tlpCamBtns.Controls.Add(btnStartCamera, 0, 0);
            tlpCamBtns.Controls.Add(btnStopCamera, 1, 0);
            
            pnlCameraButtons.Controls.Clear();
            pnlCameraButtons.Controls.Add(tlpCamBtns);

            // --- Fix Action Buttons to be uniform ---
            var tlpBtns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0)
            };
            tlpBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            
            btnCapture.Dock = DockStyle.Fill;
            btnCapture.Margin = new Padding(0, 5, 5, 5);
            btnClearAll.Dock = DockStyle.Fill;
            btnClearAll.Margin = new Padding(5);
            btnRegister.Dock = DockStyle.Fill;
            btnRegister.Margin = new Padding(5, 5, 0, 5);
            
            tlpBtns.Controls.Add(btnCapture, 0, 0);
            tlpBtns.Controls.Add(btnClearAll, 1, 0);
            tlpBtns.Controls.Add(btnRegister, 2, 0);
            
            pnlButtons.Controls.Clear();
            pnlButtons.Controls.Add(tlpBtns);

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
            
            // Neo nút vào góc phải trên của GroupBox
            btnViewRegistered.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnViewRegistered.Location = new System.Drawing.Point(grpEmployeeSelect.Width - btnViewRegistered.Width - 15, 30);
            grpEmployeeSelect.Controls.Add(btnViewRegistered);
            btnViewRegistered.BringToFront();

            // Narrow the combobox to make room for the button
            cboSelectEmployee.Width = grpEmployeeSelect.Width - 250; 
            cboSelectEmployee.DropDownWidth = 350;

            // Fix Progress Panel
            pnlProgress.Dock = DockStyle.Bottom;
            pnlProgress.Height = 55;
            pnlProgress.Padding = new Padding(10, 5, 10, 5);
            lblProgress.Dock = DockStyle.Top;
            lblProgress.Height = 20;
            progressBar.Dock = DockStyle.Fill;
            progressBar.Margin = new Padding(0, 5, 0, 5);

            // Fix Instruction
            lblInstruction.AutoSize = false;
            lblInstruction.Height = 55;
            lblInstruction.Padding = new Padding(5);

            // Fix FlowLayoutPanel for images
            flpCapturedImages.Dock = DockStyle.Fill;
            flpCapturedImages.AutoScroll = true;
            flpCapturedImages.BackColor = Color.FromArgb(248, 250, 252);
            flpCapturedImages.Padding = new Padding(10);

            // Improve PictureBoxes in FlowLayoutPanel
            foreach (var pb in capturedPictures)
            {
                pb.Margin = new Padding(8);
                pb.BackColor = Color.White;
                pb.BorderStyle = BorderStyle.FixedSingle;
                pb.Width = 105; // Slightly smaller to ensure 3 per row if possible
                pb.Height = 105;
            }
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

                await AppDatabase.Repository.InsertAuditLogAsync(
                    AppSession.CurrentUser?.Id, emp.Id,
                    "FACE_REGISTER", "face_data", emp.Id.ToString(),
                    $"Đăng ký Face ID cho {emp.FullName} — {capturedCount} ảnh");

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
                int dlgWidth = Math.Max(550, faceList.Count * 130 + 30);
                using (var dlg = new Form
                {
                    Text = $"Ảnh Face ID — {emp.FullName} ({faceList.Count} ảnh)",
                    Size = new System.Drawing.Size(dlgWidth, 260),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, MinimizeBox = false, BackColor = Color.White
                })
                {
                    int x = 10;
                    foreach (var fd in faceList)
                    {
                        var fdRef = fd; // capture for async lambda
                        bool verified = fdRef.IsVerified;

                        var pic = new PictureBox
                        {
                            Size = new System.Drawing.Size(110, 130),
                            Location = new System.Drawing.Point(x, 15),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            BorderStyle = BorderStyle.FixedSingle,
                            BackColor = Color.FromArgb(241, 245, 249)
                        };
                        if (!string.IsNullOrEmpty(fdRef.ImagePath) && System.IO.File.Exists(fdRef.ImagePath))
                        {
                            try { pic.Image = System.Drawing.Image.FromFile(fdRef.ImagePath); } catch { }
                        }

                        var lblInfo = new Label
                        {
                            Text = $"#{fdRef.ImageIndex} {fdRef.Angle ?? ""}",
                            Location = new System.Drawing.Point(x, 150),
                            Size = new System.Drawing.Size(110, 16),
                            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                            Font = new Font("Segoe UI", 7.5F),
                            ForeColor = Color.FromArgb(71, 85, 105)
                        };

                        var btnVerify = new Button
                        {
                            Text = verified ? "✓ Đã xác nhận" : "? Xác nhận",
                            Size = new System.Drawing.Size(110, 26),
                            Location = new System.Drawing.Point(x, 170),
                            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                            ForeColor = Color.White,
                            BackColor = verified ? Color.FromArgb(34, 197, 94) : Color.FromArgb(59, 130, 246),
                            FlatStyle = FlatStyle.Flat,
                            Cursor = Cursors.Hand,
                            Tag = fdRef.Id
                        };
                        btnVerify.FlatAppearance.BorderSize = 0;
                        btnVerify.Click += async (s2, e2) =>
                        {
                            var btn = (Button)s2;
                            int faceId = (int)btn.Tag;
                            bool nowVerified = btn.BackColor != Color.FromArgb(34, 197, 94); // toggle
                            try
                            {
                                await AppDatabase.Repository.UpdateFaceDataVerificationAsync(
                                    faceId, nowVerified, AppSession.CurrentUser?.Id);
                                await AppDatabase.Repository.InsertFaceRegistrationLogAsync(
                                    emp.Id, nowVerified ? "Verify" : "Deactivate",
                                    AppSession.CurrentUser?.EmployeeId,
                                    nowVerified ? "HR xác nhận ảnh đủ chất lượng" : "HR huỷ xác nhận ảnh");
                                await AppDatabase.Repository.InsertAuditLogAsync(
                                    AppSession.CurrentUser?.Id, emp.Id,
                                    nowVerified ? "FACE_VERIFY" : "FACE_UNVERIFY",
                                    "face_data", faceId.ToString(),
                                    $"{(nowVerified ? "Xác nhận" : "Huỷ xác nhận")} ảnh Face ID #{faceId} của {emp.FullName}");

                                btn.Text = nowVerified ? "✓ Đã xác nhận" : "? Xác nhận";
                                btn.BackColor = nowVerified ? Color.FromArgb(34, 197, 94) : Color.FromArgb(59, 130, 246);
                            }
                            catch (Exception ex2)
                            {
                                MessageBox.Show($"Lỗi xác nhận: {ex2.Message}", "Lỗi");
                            }
                        };

                        dlg.Controls.AddRange(new Control[] { pic, lblInfo, btnVerify });
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

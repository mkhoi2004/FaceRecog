namespace FaceIDApp.UserControls
{
    partial class UCFaceRegistration
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.grpEmployeeSelect = new System.Windows.Forms.GroupBox();
            this.lblSelectEmployee = new System.Windows.Forms.Label();
            this.cboSelectEmployee = new System.Windows.Forms.ComboBox();
            this.lblSelectedInfo = new System.Windows.Forms.Label();
            this.grpCamera = new System.Windows.Forms.GroupBox();
            this.picCamera = new System.Windows.Forms.PictureBox();
            this.pnlCameraButtons = new System.Windows.Forms.Panel();
            this.btnStartCamera = new System.Windows.Forms.Button();
            this.btnStopCamera = new System.Windows.Forms.Button();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.grpCapturedImages = new System.Windows.Forms.GroupBox();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.flpCapturedImages = new System.Windows.Forms.FlowLayoutPanel();
            this.pic1 = new System.Windows.Forms.PictureBox();
            this.pic2 = new System.Windows.Forms.PictureBox();
            this.pic3 = new System.Windows.Forms.PictureBox();
            this.pic4 = new System.Windows.Forms.PictureBox();
            this.pic5 = new System.Windows.Forms.PictureBox();
            this.pnlProgress = new System.Windows.Forms.Panel();
            this.lblProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnCapture = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnRegister = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.grpEmployeeSelect.SuspendLayout();
            this.grpCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).BeginInit();
            this.pnlCameraButtons.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.grpCapturedImages.SuspendLayout();
            this.flpCapturedImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic5)).BeginInit();
            this.pnlProgress.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20);
            this.pnlHeader.Size = new System.Drawing.Size(900, 70);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.lblTitle.Location = new System.Drawing.Point(20, 17);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(309, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "📷 Đăng ký khuôn mặt";
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.pnlLeft);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 70);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(15);
            this.pnlMain.Size = new System.Drawing.Size(900, 530);
            this.pnlMain.TabIndex = 1;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.grpCamera);
            this.pnlLeft.Controls.Add(this.grpEmployeeSelect);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(15, 15);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.pnlLeft.Size = new System.Drawing.Size(450, 500);
            this.pnlLeft.TabIndex = 0;
            // 
            // grpEmployeeSelect
            // 
            this.grpEmployeeSelect.BackColor = System.Drawing.Color.White;
            this.grpEmployeeSelect.Controls.Add(this.lblSelectedInfo);
            this.grpEmployeeSelect.Controls.Add(this.cboSelectEmployee);
            this.grpEmployeeSelect.Controls.Add(this.lblSelectEmployee);
            this.grpEmployeeSelect.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpEmployeeSelect.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpEmployeeSelect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.grpEmployeeSelect.Location = new System.Drawing.Point(0, 0);
            this.grpEmployeeSelect.Name = "grpEmployeeSelect";
            this.grpEmployeeSelect.Padding = new System.Windows.Forms.Padding(15);
            this.grpEmployeeSelect.Size = new System.Drawing.Size(440, 120);
            this.grpEmployeeSelect.TabIndex = 0;
            this.grpEmployeeSelect.TabStop = false;
            this.grpEmployeeSelect.Text = "👤 Chọn nhân viên";
            // 
            // lblSelectEmployee
            // 
            this.lblSelectEmployee.AutoSize = true;
            this.lblSelectEmployee.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSelectEmployee.Location = new System.Drawing.Point(15, 35);
            this.lblSelectEmployee.Name = "lblSelectEmployee";
            this.lblSelectEmployee.Size = new System.Drawing.Size(66, 17);
            this.lblSelectEmployee.TabIndex = 0;
            this.lblSelectEmployee.Text = "Nhân viên:";
            // 
            // cboSelectEmployee
            // 
            this.cboSelectEmployee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSelectEmployee.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboSelectEmployee.FormattingEnabled = true;
            this.cboSelectEmployee.Items.AddRange(new object[] {
            "-- Chọn nhân viên --",
            "NV001 - Nguyễn Văn An",
            "NV002 - Trần Thị Bình",
            "NV003 - Lê Văn Cường",
            "NV004 - Phạm Thị Dung",
            "NV005 - Hoàng Văn Em"});
            this.cboSelectEmployee.Location = new System.Drawing.Point(100, 32);
            this.cboSelectEmployee.Name = "cboSelectEmployee";
            this.cboSelectEmployee.Size = new System.Drawing.Size(320, 25);
            this.cboSelectEmployee.TabIndex = 1;
            // 
            // lblSelectedInfo
            // 
            this.lblSelectedInfo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSelectedInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblSelectedInfo.Location = new System.Drawing.Point(15, 65);
            this.lblSelectedInfo.Name = "lblSelectedInfo";
            this.lblSelectedInfo.Size = new System.Drawing.Size(405, 45);
            this.lblSelectedInfo.TabIndex = 2;
            this.lblSelectedInfo.Text = "Phòng ban: ---\r\nTrạng thái Face ID: ---";
            // 
            // grpCamera
            // 
            this.grpCamera.BackColor = System.Drawing.Color.White;
            this.grpCamera.Controls.Add(this.picCamera);
            this.grpCamera.Controls.Add(this.pnlCameraButtons);
            this.grpCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCamera.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.grpCamera.Location = new System.Drawing.Point(0, 120);
            this.grpCamera.Name = "grpCamera";
            this.grpCamera.Padding = new System.Windows.Forms.Padding(15);
            this.grpCamera.Size = new System.Drawing.Size(440, 380);
            this.grpCamera.TabIndex = 1;
            this.grpCamera.TabStop = false;
            this.grpCamera.Text = "📹 Camera";
            // 
            // picCamera
            // 
            this.picCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.picCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picCamera.Location = new System.Drawing.Point(15, 25);
            this.picCamera.Name = "picCamera";
            this.picCamera.Size = new System.Drawing.Size(410, 290);
            this.picCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCamera.TabIndex = 0;
            this.picCamera.TabStop = false;
            // 
            // pnlCameraButtons
            // 
            this.pnlCameraButtons.Controls.Add(this.btnStopCamera);
            this.pnlCameraButtons.Controls.Add(this.btnStartCamera);
            this.pnlCameraButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCameraButtons.Location = new System.Drawing.Point(15, 315);
            this.pnlCameraButtons.Name = "pnlCameraButtons";
            this.pnlCameraButtons.Size = new System.Drawing.Size(410, 50);
            this.pnlCameraButtons.TabIndex = 1;
            // 
            // btnStartCamera
            // 
            this.btnStartCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnStartCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartCamera.FlatAppearance.BorderSize = 0;
            this.btnStartCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartCamera.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartCamera.ForeColor = System.Drawing.Color.White;
            this.btnStartCamera.Location = new System.Drawing.Point(50, 8);
            this.btnStartCamera.Name = "btnStartCamera";
            this.btnStartCamera.Size = new System.Drawing.Size(140, 35);
            this.btnStartCamera.TabIndex = 0;
            this.btnStartCamera.Text = "▶ Bật Camera";
            this.btnStartCamera.UseVisualStyleBackColor = false;
            // 
            // btnStopCamera
            // 
            this.btnStopCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnStopCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStopCamera.FlatAppearance.BorderSize = 0;
            this.btnStopCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopCamera.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopCamera.ForeColor = System.Drawing.Color.White;
            this.btnStopCamera.Location = new System.Drawing.Point(210, 8);
            this.btnStopCamera.Name = "btnStopCamera";
            this.btnStopCamera.Size = new System.Drawing.Size(140, 35);
            this.btnStopCamera.TabIndex = 1;
            this.btnStopCamera.Text = "⏹ Tắt Camera";
            this.btnStopCamera.UseVisualStyleBackColor = false;
            // 
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.grpCapturedImages);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(465, 15);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(420, 500);
            this.pnlRight.TabIndex = 1;
            // 
            // grpCapturedImages
            // 
            this.grpCapturedImages.BackColor = System.Drawing.Color.White;
            this.grpCapturedImages.Controls.Add(this.flpCapturedImages);
            this.grpCapturedImages.Controls.Add(this.pnlProgress);
            this.grpCapturedImages.Controls.Add(this.pnlButtons);
            this.grpCapturedImages.Controls.Add(this.lblInstruction);
            this.grpCapturedImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCapturedImages.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpCapturedImages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.grpCapturedImages.Location = new System.Drawing.Point(0, 0);
            this.grpCapturedImages.Name = "grpCapturedImages";
            this.grpCapturedImages.Padding = new System.Windows.Forms.Padding(15);
            this.grpCapturedImages.Size = new System.Drawing.Size(420, 500);
            this.grpCapturedImages.TabIndex = 0;
            this.grpCapturedImages.TabStop = false;
            this.grpCapturedImages.Text = "🖼️ Ảnh đã chụp";
            // 
            // lblInstruction
            // 
            this.lblInstruction.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInstruction.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblInstruction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.lblInstruction.Location = new System.Drawing.Point(15, 25);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Padding = new System.Windows.Forms.Padding(5);
            this.lblInstruction.Size = new System.Drawing.Size(390, 60);
            this.lblInstruction.TabIndex = 0;
            this.lblInstruction.Text = "📌 Hướng dẫn: Chụp ít nhất 5 ảnh khuôn mặt ở các góc khác nhau (thẳng, nghiêng tr" +
    "ái, nghiêng phải, ngửa lên, cúi xuống) để đảm bảo độ chính xác nhận diện.";
            // 
            // flpCapturedImages
            // 
            this.flpCapturedImages.Controls.Add(this.pic1);
            this.flpCapturedImages.Controls.Add(this.pic2);
            this.flpCapturedImages.Controls.Add(this.pic3);
            this.flpCapturedImages.Controls.Add(this.pic4);
            this.flpCapturedImages.Controls.Add(this.pic5);
            this.flpCapturedImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCapturedImages.Location = new System.Drawing.Point(15, 85);
            this.flpCapturedImages.Name = "flpCapturedImages";
            this.flpCapturedImages.Padding = new System.Windows.Forms.Padding(10);
            this.flpCapturedImages.Size = new System.Drawing.Size(390, 280);
            this.flpCapturedImages.TabIndex = 1;
            // 
            // pic1
            // 
            this.pic1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pic1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic1.Location = new System.Drawing.Point(13, 13);
            this.pic1.Name = "pic1";
            this.pic1.Size = new System.Drawing.Size(110, 110);
            this.pic1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic1.TabIndex = 0;
            this.pic1.TabStop = false;
            // 
            // pic2
            // 
            this.pic2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pic2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic2.Location = new System.Drawing.Point(129, 13);
            this.pic2.Name = "pic2";
            this.pic2.Size = new System.Drawing.Size(110, 110);
            this.pic2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic2.TabIndex = 1;
            this.pic2.TabStop = false;
            // 
            // pic3
            // 
            this.pic3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pic3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic3.Location = new System.Drawing.Point(245, 13);
            this.pic3.Name = "pic3";
            this.pic3.Size = new System.Drawing.Size(110, 110);
            this.pic3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic3.TabIndex = 2;
            this.pic3.TabStop = false;
            // 
            // pic4
            // 
            this.pic4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pic4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic4.Location = new System.Drawing.Point(13, 129);
            this.pic4.Name = "pic4";
            this.pic4.Size = new System.Drawing.Size(110, 110);
            this.pic4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic4.TabIndex = 3;
            this.pic4.TabStop = false;
            // 
            // pic5
            // 
            this.pic5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pic5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic5.Location = new System.Drawing.Point(129, 129);
            this.pic5.Name = "pic5";
            this.pic5.Size = new System.Drawing.Size(110, 110);
            this.pic5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pic5.TabIndex = 4;
            this.pic5.TabStop = false;
            // 
            // pnlProgress
            // 
            this.pnlProgress.Controls.Add(this.progressBar);
            this.pnlProgress.Controls.Add(this.lblProgress);
            this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlProgress.Location = new System.Drawing.Point(15, 365);
            this.pnlProgress.Name = "pnlProgress";
            this.pnlProgress.Size = new System.Drawing.Size(390, 50);
            this.pnlProgress.TabIndex = 2;
            // 
            // lblProgress
            // 
            this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblProgress.Location = new System.Drawing.Point(10, 5);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(370, 20);
            this.lblProgress.TabIndex = 0;
            this.lblProgress.Text = "Đã chụp: 0/5 ảnh";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(10, 28);
            this.progressBar.Maximum = 5;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(370, 15);
            this.progressBar.TabIndex = 1;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnRegister);
            this.pnlButtons.Controls.Add(this.btnClearAll);
            this.pnlButtons.Controls.Add(this.btnCapture);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(15, 415);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(390, 70);
            this.pnlButtons.TabIndex = 3;
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnCapture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCapture.FlatAppearance.BorderSize = 0;
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCapture.ForeColor = System.Drawing.Color.White;
            this.btnCapture.Location = new System.Drawing.Point(10, 15);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(115, 40);
            this.btnCapture.TabIndex = 0;
            this.btnCapture.Text = "📸 Chụp ảnh";
            this.btnCapture.UseVisualStyleBackColor = false;
            // 
            // btnClearAll
            // 
            this.btnClearAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnClearAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearAll.FlatAppearance.BorderSize = 0;
            this.btnClearAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearAll.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClearAll.ForeColor = System.Drawing.Color.White;
            this.btnClearAll.Location = new System.Drawing.Point(135, 15);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(115, 40);
            this.btnClearAll.TabIndex = 1;
            this.btnClearAll.Text = "🗑️ Xóa tất cả";
            this.btnClearAll.UseVisualStyleBackColor = false;
            // 
            // btnRegister
            // 
            this.btnRegister.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.Location = new System.Drawing.Point(260, 15);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(120, 40);
            this.btnRegister.TabIndex = 2;
            this.btnRegister.Text = "✅ Đăng ký";
            this.btnRegister.UseVisualStyleBackColor = false;
            // 
            // UCFaceRegistration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Name = "UCFaceRegistration";
            this.Size = new System.Drawing.Size(900, 600);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.grpEmployeeSelect.ResumeLayout(false);
            this.grpEmployeeSelect.PerformLayout();
            this.grpCamera.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).EndInit();
            this.pnlCameraButtons.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.grpCapturedImages.ResumeLayout(false);
            this.flpCapturedImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic5)).EndInit();
            this.pnlProgress.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.GroupBox grpEmployeeSelect;
        private System.Windows.Forms.Label lblSelectEmployee;
        private System.Windows.Forms.ComboBox cboSelectEmployee;
        private System.Windows.Forms.Label lblSelectedInfo;
        private System.Windows.Forms.GroupBox grpCamera;
        private System.Windows.Forms.PictureBox picCamera;
        private System.Windows.Forms.Panel pnlCameraButtons;
        private System.Windows.Forms.Button btnStartCamera;
        private System.Windows.Forms.Button btnStopCamera;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.GroupBox grpCapturedImages;
        private System.Windows.Forms.Label lblInstruction;
        private System.Windows.Forms.FlowLayoutPanel flpCapturedImages;
        private System.Windows.Forms.PictureBox pic1;
        private System.Windows.Forms.PictureBox pic2;
        private System.Windows.Forms.PictureBox pic3;
        private System.Windows.Forms.PictureBox pic4;
        private System.Windows.Forms.PictureBox pic5;
        private System.Windows.Forms.Panel pnlProgress;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Button btnRegister;
    }
}

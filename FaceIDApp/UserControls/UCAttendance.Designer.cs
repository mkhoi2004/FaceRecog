namespace FaceIDApp.UserControls
{
    partial class UCAttendance
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
            this.lblDateTime = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.btnCheckOut = new System.Windows.Forms.Button();
            this.btnCheckIn = new System.Windows.Forms.Button();
            this.pnlEmployeeInfo = new System.Windows.Forms.Panel();
            this.lblCheckInTime = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPosition = new System.Windows.Forms.Label();
            this.lblDepartment = new System.Windows.Forms.Label();
            this.lblEmployeeCode = new System.Windows.Forms.Label();
            this.lblEmployeeName = new System.Windows.Forms.Label();
            this.picEmployeePhoto = new System.Windows.Forms.PictureBox();
            this.pnlCamera = new System.Windows.Forms.Panel();
            this.picCamera = new System.Windows.Forms.PictureBox();
            this.pnlCameraControls = new System.Windows.Forms.Panel();
            this.btnCapture = new System.Windows.Forms.Button();
            this.btnStopCamera = new System.Windows.Forms.Button();
            this.btnStartCamera = new System.Windows.Forms.Button();
            this.pnlTodayList = new System.Windows.Forms.Panel();
            this.dgvTodayAttendance = new System.Windows.Forms.DataGridView();
            this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMaNV = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHoTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGioVao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGioRa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblTodayListTitle = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlEmployeeInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picEmployeePhoto)).BeginInit();
            this.pnlCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).BeginInit();
            this.pnlCameraControls.SuspendLayout();
            this.pnlTodayList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTodayAttendance)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.lblDateTime);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(30, 31, 30, 31);
            this.pnlHeader.Size = new System.Drawing.Size(1350, 123);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblDateTime
            // 
            this.lblDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDateTime.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblDateTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblDateTime.Location = new System.Drawing.Point(825, 31);
            this.lblDateTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(495, 49);
            this.lblDateTime.TabIndex = 1;
            this.lblDateTime.Text = "08:30:45 - 28/01/2026";
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.lblTitle.Location = new System.Drawing.Point(30, 31);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(400, 48);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "📷 Chấm công Face ID";
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pnlMain.Controls.Add(this.pnlInfo);
            this.pnlMain.Controls.Add(this.pnlCamera);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMain.Location = new System.Drawing.Point(0, 123);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(22, 23, 22, 23);
            this.pnlMain.Size = new System.Drawing.Size(1350, 538);
            this.pnlMain.TabIndex = 1;
            // 
            // pnlInfo
            // 
            this.pnlInfo.BackColor = System.Drawing.Color.White;
            this.pnlInfo.Controls.Add(this.pnlActions);
            this.pnlInfo.Controls.Add(this.pnlEmployeeInfo);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInfo.Location = new System.Drawing.Point(652, 23);
            this.pnlInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Padding = new System.Windows.Forms.Padding(22, 23, 22, 23);
            this.pnlInfo.Size = new System.Drawing.Size(676, 492);
            this.pnlInfo.TabIndex = 1;
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.btnCheckOut);
            this.pnlActions.Controls.Add(this.btnCheckIn);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActions.Location = new System.Drawing.Point(22, 361);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new System.Drawing.Size(632, 108);
            this.pnlActions.TabIndex = 1;
            // 
            // btnCheckOut
            // 
            this.btnCheckOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnCheckOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCheckOut.FlatAppearance.BorderSize = 0;
            this.btnCheckOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCheckOut.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCheckOut.ForeColor = System.Drawing.Color.White;
            this.btnCheckOut.Location = new System.Drawing.Point(322, 15);
            this.btnCheckOut.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCheckOut.Name = "btnCheckOut";
            this.btnCheckOut.Size = new System.Drawing.Size(285, 77);
            this.btnCheckOut.TabIndex = 1;
            this.btnCheckOut.Text = "🚪 CHECK-OUT";
            this.btnCheckOut.UseVisualStyleBackColor = false;
            // 
            // btnCheckIn
            // 
            this.btnCheckIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnCheckIn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCheckIn.FlatAppearance.BorderSize = 0;
            this.btnCheckIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCheckIn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCheckIn.ForeColor = System.Drawing.Color.White;
            this.btnCheckIn.Location = new System.Drawing.Point(15, 15);
            this.btnCheckIn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCheckIn.Name = "btnCheckIn";
            this.btnCheckIn.Size = new System.Drawing.Size(285, 77);
            this.btnCheckIn.TabIndex = 0;
            this.btnCheckIn.Text = "✅ CHECK-IN";
            this.btnCheckIn.UseVisualStyleBackColor = false;
            // 
            // pnlEmployeeInfo
            // 
            this.pnlEmployeeInfo.Controls.Add(this.lblCheckInTime);
            this.pnlEmployeeInfo.Controls.Add(this.lblStatus);
            this.pnlEmployeeInfo.Controls.Add(this.lblPosition);
            this.pnlEmployeeInfo.Controls.Add(this.lblDepartment);
            this.pnlEmployeeInfo.Controls.Add(this.lblEmployeeCode);
            this.pnlEmployeeInfo.Controls.Add(this.lblEmployeeName);
            this.pnlEmployeeInfo.Controls.Add(this.picEmployeePhoto);
            this.pnlEmployeeInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEmployeeInfo.Location = new System.Drawing.Point(22, 23);
            this.pnlEmployeeInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlEmployeeInfo.Name = "pnlEmployeeInfo";
            this.pnlEmployeeInfo.Size = new System.Drawing.Size(632, 446);
            this.pnlEmployeeInfo.TabIndex = 0;
            // 
            // lblCheckInTime
            // 
            this.lblCheckInTime.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblCheckInTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblCheckInTime.Location = new System.Drawing.Point(219, 246);
            this.lblCheckInTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCheckInTime.Name = "lblCheckInTime";
            this.lblCheckInTime.Size = new System.Drawing.Size(375, 38);
            this.lblCheckInTime.TabIndex = 6;
            this.lblCheckInTime.Text = "⏰ Giờ vào: --:--:--";
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblStatus.Location = new System.Drawing.Point(219, 200);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(375, 38);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "⏳ Trạng thái: Chờ nhận diện";
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblPosition.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblPosition.Location = new System.Drawing.Point(219, 154);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(138, 30);
            this.lblPosition.TabIndex = 4;
            this.lblPosition.Text = "💼 Chức vụ: ";
            // 
            // lblDepartment
            // 
            this.lblDepartment.AutoSize = true;
            this.lblDepartment.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDepartment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblDepartment.Location = new System.Drawing.Point(219, 108);
            this.lblDepartment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDepartment.Name = "lblDepartment";
            this.lblDepartment.Size = new System.Drawing.Size(164, 30);
            this.lblDepartment.TabIndex = 3;
            this.lblDepartment.Text = "🏢 Phòng ban: ";
            // 
            // lblEmployeeCode
            // 
            this.lblEmployeeCode.AutoSize = true;
            this.lblEmployeeCode.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblEmployeeCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblEmployeeCode.Location = new System.Drawing.Point(219, 62);
            this.lblEmployeeCode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEmployeeCode.Name = "lblEmployeeCode";
            this.lblEmployeeCode.Size = new System.Drawing.Size(118, 30);
            this.lblEmployeeCode.TabIndex = 2;
            this.lblEmployeeCode.Text = "Mã NV: ---";
            // 
            // lblEmployeeName
            // 
            this.lblEmployeeName.AutoSize = true;
            this.lblEmployeeName.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblEmployeeName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblEmployeeName.Location = new System.Drawing.Point(218, 15);
            this.lblEmployeeName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEmployeeName.Name = "lblEmployeeName";
            this.lblEmployeeName.Size = new System.Drawing.Size(232, 38);
            this.lblEmployeeName.TabIndex = 1;
            this.lblEmployeeName.Text = "Chờ nhận diện...";
            // 
            // picEmployeePhoto
            // 
            this.picEmployeePhoto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.picEmployeePhoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picEmployeePhoto.Location = new System.Drawing.Point(15, 15);
            this.picEmployeePhoto.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picEmployeePhoto.Name = "picEmployeePhoto";
            this.picEmployeePhoto.Size = new System.Drawing.Size(179, 214);
            this.picEmployeePhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picEmployeePhoto.TabIndex = 0;
            this.picEmployeePhoto.TabStop = false;
            // 
            // pnlCamera
            // 
            this.pnlCamera.BackColor = System.Drawing.Color.White;
            this.pnlCamera.Controls.Add(this.picCamera);
            this.pnlCamera.Controls.Add(this.pnlCameraControls);
            this.pnlCamera.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlCamera.Location = new System.Drawing.Point(22, 23);
            this.pnlCamera.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlCamera.Name = "pnlCamera";
            this.pnlCamera.Padding = new System.Windows.Forms.Padding(15, 15, 15, 15);
            this.pnlCamera.Size = new System.Drawing.Size(630, 492);
            this.pnlCamera.TabIndex = 0;
            // 
            // picCamera
            // 
            this.picCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.picCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picCamera.Location = new System.Drawing.Point(15, 15);
            this.picCamera.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picCamera.Name = "picCamera";
            this.picCamera.Size = new System.Drawing.Size(600, 385);
            this.picCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCamera.TabIndex = 0;
            this.picCamera.TabStop = false;
            // 
            // pnlCameraControls
            // 
            this.pnlCameraControls.Controls.Add(this.btnCapture);
            this.pnlCameraControls.Controls.Add(this.btnStopCamera);
            this.pnlCameraControls.Controls.Add(this.btnStartCamera);
            this.pnlCameraControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCameraControls.Location = new System.Drawing.Point(15, 400);
            this.pnlCameraControls.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlCameraControls.Name = "pnlCameraControls";
            this.pnlCameraControls.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.pnlCameraControls.Size = new System.Drawing.Size(600, 77);
            this.pnlCameraControls.TabIndex = 1;
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnCapture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCapture.FlatAppearance.BorderSize = 0;
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCapture.ForeColor = System.Drawing.Color.White;
            this.btnCapture.Location = new System.Drawing.Point(402, 12);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(180, 54);
            this.btnCapture.TabIndex = 2;
            this.btnCapture.Text = "📸 Chụp ảnh";
            this.btnCapture.UseVisualStyleBackColor = false;
            // 
            // btnStopCamera
            // 
            this.btnStopCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnStopCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStopCamera.FlatAppearance.BorderSize = 0;
            this.btnStopCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopCamera.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnStopCamera.ForeColor = System.Drawing.Color.White;
            this.btnStopCamera.Location = new System.Drawing.Point(207, 12);
            this.btnStopCamera.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnStopCamera.Name = "btnStopCamera";
            this.btnStopCamera.Size = new System.Drawing.Size(180, 54);
            this.btnStopCamera.TabIndex = 1;
            this.btnStopCamera.Text = "⏹ Tắt Camera";
            this.btnStopCamera.UseVisualStyleBackColor = false;
            // 
            // btnStartCamera
            // 
            this.btnStartCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnStartCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartCamera.FlatAppearance.BorderSize = 0;
            this.btnStartCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartCamera.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnStartCamera.ForeColor = System.Drawing.Color.White;
            this.btnStartCamera.Location = new System.Drawing.Point(12, 12);
            this.btnStartCamera.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnStartCamera.Name = "btnStartCamera";
            this.btnStartCamera.Size = new System.Drawing.Size(180, 54);
            this.btnStartCamera.TabIndex = 0;
            this.btnStartCamera.Text = "▶ Bật Camera";
            this.btnStartCamera.UseVisualStyleBackColor = false;
            // 
            // pnlTodayList
            // 
            this.pnlTodayList.BackColor = System.Drawing.Color.White;
            this.pnlTodayList.Controls.Add(this.dgvTodayAttendance);
            this.pnlTodayList.Controls.Add(this.lblTodayListTitle);
            this.pnlTodayList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTodayList.Location = new System.Drawing.Point(0, 661);
            this.pnlTodayList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlTodayList.Name = "pnlTodayList";
            this.pnlTodayList.Padding = new System.Windows.Forms.Padding(30, 31, 30, 31);
            this.pnlTodayList.Size = new System.Drawing.Size(1350, 262);
            this.pnlTodayList.TabIndex = 2;
            // 
            // dgvTodayAttendance
            // 
            this.dgvTodayAttendance.AllowUserToAddRows = false;
            this.dgvTodayAttendance.AllowUserToDeleteRows = false;
            this.dgvTodayAttendance.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTodayAttendance.BackgroundColor = System.Drawing.Color.White;
            this.dgvTodayAttendance.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvTodayAttendance.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvTodayAttendance.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvTodayAttendance.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTodayAttendance.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSTT,
            this.colMaNV,
            this.colHoTen,
            this.colGioVao,
            this.colGioRa,
            this.colTrangThai});
            this.dgvTodayAttendance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTodayAttendance.EnableHeadersVisualStyles = false;
            this.dgvTodayAttendance.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.dgvTodayAttendance.Location = new System.Drawing.Point(30, 78);
            this.dgvTodayAttendance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dgvTodayAttendance.Name = "dgvTodayAttendance";
            this.dgvTodayAttendance.ReadOnly = true;
            this.dgvTodayAttendance.RowHeadersVisible = false;
            this.dgvTodayAttendance.RowHeadersWidth = 62;
            this.dgvTodayAttendance.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTodayAttendance.Size = new System.Drawing.Size(1290, 153);
            this.dgvTodayAttendance.TabIndex = 1;
            // 
            // colSTT
            // 
            this.colSTT.FillWeight = 50F;
            this.colSTT.HeaderText = "STT";
            this.colSTT.MinimumWidth = 8;
            this.colSTT.Name = "colSTT";
            this.colSTT.ReadOnly = true;
            // 
            // colMaNV
            // 
            this.colMaNV.HeaderText = "Mã NV";
            this.colMaNV.MinimumWidth = 8;
            this.colMaNV.Name = "colMaNV";
            this.colMaNV.ReadOnly = true;
            // 
            // colHoTen
            // 
            this.colHoTen.FillWeight = 150F;
            this.colHoTen.HeaderText = "Họ tên";
            this.colHoTen.MinimumWidth = 8;
            this.colHoTen.Name = "colHoTen";
            this.colHoTen.ReadOnly = true;
            // 
            // colGioVao
            // 
            this.colGioVao.HeaderText = "Giờ vào";
            this.colGioVao.MinimumWidth = 8;
            this.colGioVao.Name = "colGioVao";
            this.colGioVao.ReadOnly = true;
            // 
            // colGioRa
            // 
            this.colGioRa.HeaderText = "Giờ ra";
            this.colGioRa.MinimumWidth = 8;
            this.colGioRa.Name = "colGioRa";
            this.colGioRa.ReadOnly = true;
            // 
            // colTrangThai
            // 
            this.colTrangThai.HeaderText = "Trạng thái";
            this.colTrangThai.MinimumWidth = 8;
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.ReadOnly = true;
            // 
            // lblTodayListTitle
            // 
            this.lblTodayListTitle.AutoSize = true;
            this.lblTodayListTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTodayListTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTodayListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTodayListTitle.Location = new System.Drawing.Point(30, 31);
            this.lblTodayListTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTodayListTitle.Name = "lblTodayListTitle";
            this.lblTodayListTitle.Padding = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.lblTodayListTitle.Size = new System.Drawing.Size(410, 47);
            this.lblTodayListTitle.TabIndex = 0;
            this.lblTodayListTitle.Text = "📋 Danh sách chấm công hôm nay";
            // 
            // UCAttendance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Controls.Add(this.pnlTodayList);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "UCAttendance";
            this.Size = new System.Drawing.Size(1350, 923);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlInfo.ResumeLayout(false);
            this.pnlActions.ResumeLayout(false);
            this.pnlEmployeeInfo.ResumeLayout(false);
            this.pnlEmployeeInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picEmployeePhoto)).EndInit();
            this.pnlCamera.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).EndInit();
            this.pnlCameraControls.ResumeLayout(false);
            this.pnlTodayList.ResumeLayout(false);
            this.pnlTodayList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTodayAttendance)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDateTime;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlCamera;
        private System.Windows.Forms.PictureBox picCamera;
        private System.Windows.Forms.Panel pnlCameraControls;
        private System.Windows.Forms.Button btnStartCamera;
        private System.Windows.Forms.Button btnStopCamera;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Panel pnlEmployeeInfo;
        private System.Windows.Forms.PictureBox picEmployeePhoto;
        private System.Windows.Forms.Label lblEmployeeName;
        private System.Windows.Forms.Label lblEmployeeCode;
        private System.Windows.Forms.Label lblDepartment;
        private System.Windows.Forms.Label lblPosition;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblCheckInTime;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.Button btnCheckIn;
        private System.Windows.Forms.Button btnCheckOut;
        private System.Windows.Forms.Panel pnlTodayList;
        private System.Windows.Forms.Label lblTodayListTitle;
        private System.Windows.Forms.DataGridView dgvTodayAttendance;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMaNV;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHoTen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGioVao;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGioRa;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrangThai;
    }
}

namespace FaceIDApp.UserControls
{
    partial class UCAttendanceReport
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
            this.pnlFilter = new System.Windows.Forms.Panel();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.lblToDate = new System.Windows.Forms.Label();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.lblEmployee = new System.Windows.Forms.Label();
            this.cboEmployee = new System.Windows.Forms.ComboBox();
            this.lblDepartment = new System.Windows.Forms.Label();
            this.cboDepartment = new System.Windows.Forms.ComboBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.pnlSummary = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlTotalDays = new System.Windows.Forms.Panel();
            this.lblTotalDaysValue = new System.Windows.Forms.Label();
            this.lblTotalDaysTitle = new System.Windows.Forms.Label();
            this.pnlPresentDays = new System.Windows.Forms.Panel();
            this.lblPresentDaysValue = new System.Windows.Forms.Label();
            this.lblPresentDaysTitle = new System.Windows.Forms.Label();
            this.pnlLateDays = new System.Windows.Forms.Panel();
            this.lblLateDaysValue = new System.Windows.Forms.Label();
            this.lblLateDaysTitle = new System.Windows.Forms.Label();
            this.pnlAbsentDays = new System.Windows.Forms.Panel();
            this.lblAbsentDaysValue = new System.Windows.Forms.Label();
            this.lblAbsentDaysTitle = new System.Windows.Forms.Label();
            this.pnlData = new System.Windows.Forms.Panel();
            this.dgvReport = new System.Windows.Forms.DataGridView();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmployeeCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmployeeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDepartment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCheckIn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCheckOut = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWorkHours = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlHeader.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.pnlSummary.SuspendLayout();
            this.pnlTotalDays.SuspendLayout();
            this.pnlPresentDays.SuspendLayout();
            this.pnlLateDays.SuspendLayout();
            this.pnlAbsentDays.SuspendLayout();
            this.pnlData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReport)).BeginInit();
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
            this.lblTitle.Size = new System.Drawing.Size(281, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "📊 Báo cáo chấm công";
            // 
            // pnlFilter
            // 
            this.pnlFilter.BackColor = System.Drawing.Color.White;
            this.pnlFilter.Controls.Add(this.btnPrint);
            this.pnlFilter.Controls.Add(this.btnExportExcel);
            this.pnlFilter.Controls.Add(this.btnSearch);
            this.pnlFilter.Controls.Add(this.cboDepartment);
            this.pnlFilter.Controls.Add(this.lblDepartment);
            this.pnlFilter.Controls.Add(this.cboEmployee);
            this.pnlFilter.Controls.Add(this.lblEmployee);
            this.pnlFilter.Controls.Add(this.dtpToDate);
            this.pnlFilter.Controls.Add(this.lblToDate);
            this.pnlFilter.Controls.Add(this.dtpFromDate);
            this.pnlFilter.Controls.Add(this.lblFromDate);
            this.pnlFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Location = new System.Drawing.Point(0, 70);
            this.pnlFilter.Name = "pnlFilter";
            this.pnlFilter.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.pnlFilter.Size = new System.Drawing.Size(900, 70);
            this.pnlFilter.TabIndex = 1;
            // 
            // lblFromDate
            // 
            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblFromDate.Location = new System.Drawing.Point(20, 25);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(55, 17);
            this.lblFromDate.TabIndex = 0;
            this.lblFromDate.Text = "Từ ngày:";
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.CustomFormat = "dd/MM/yyyy";
            this.dtpFromDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFromDate.Location = new System.Drawing.Point(80, 22);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(110, 24);
            this.dtpFromDate.TabIndex = 1;
            // 
            // lblToDate
            // 
            this.lblToDate.AutoSize = true;
            this.lblToDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblToDate.Location = new System.Drawing.Point(200, 25);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(64, 17);
            this.lblToDate.TabIndex = 2;
            this.lblToDate.Text = "Đến ngày:";
            // 
            // dtpToDate
            // 
            this.dtpToDate.CustomFormat = "dd/MM/yyyy";
            this.dtpToDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpToDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpToDate.Location = new System.Drawing.Point(270, 22);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(110, 24);
            this.dtpToDate.TabIndex = 3;
            // 
            // lblEmployee
            // 
            this.lblEmployee.AutoSize = true;
            this.lblEmployee.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEmployee.Location = new System.Drawing.Point(395, 25);
            this.lblEmployee.Name = "lblEmployee";
            this.lblEmployee.Size = new System.Drawing.Size(22, 17);
            this.lblEmployee.TabIndex = 4;
            this.lblEmployee.Text = "NV:";
            // 
            // cboEmployee
            // 
            this.cboEmployee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEmployee.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboEmployee.FormattingEnabled = true;
            this.cboEmployee.Items.AddRange(new object[] {
            "-- Tất cả --",
            "NV001 - Nguyễn Văn An",
            "NV002 - Trần Thị Bình",
            "NV003 - Lê Văn Cường"});
            this.cboEmployee.Location = new System.Drawing.Point(420, 22);
            this.cboEmployee.Name = "cboEmployee";
            this.cboEmployee.Size = new System.Drawing.Size(150, 25);
            this.cboEmployee.TabIndex = 5;
            // 
            // lblDepartment
            // 
            this.lblDepartment.AutoSize = true;
            this.lblDepartment.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDepartment.Location = new System.Drawing.Point(580, 25);
            this.lblDepartment.Name = "lblDepartment";
            this.lblDepartment.Size = new System.Drawing.Size(32, 17);
            this.lblDepartment.TabIndex = 6;
            this.lblDepartment.Text = "P.B:";
            // 
            // cboDepartment
            // 
            this.cboDepartment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDepartment.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboDepartment.FormattingEnabled = true;
            this.cboDepartment.Items.AddRange(new object[] {
            "-- Tất cả --",
            "Phòng IT",
            "Phòng Kế toán",
            "Phòng Nhân sự",
            "Phòng Marketing"});
            this.cboDepartment.Location = new System.Drawing.Point(615, 22);
            this.cboDepartment.Name = "cboDepartment";
            this.cboDepartment.Size = new System.Drawing.Size(120, 25);
            this.cboDepartment.TabIndex = 7;
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(745, 18);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(45, 32);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "🔍";
            this.btnSearch.UseVisualStyleBackColor = false;
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnExportExcel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportExcel.FlatAppearance.BorderSize = 0;
            this.btnExportExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportExcel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportExcel.ForeColor = System.Drawing.Color.White;
            this.btnExportExcel.Location = new System.Drawing.Point(795, 18);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(45, 32);
            this.btnExportExcel.TabIndex = 9;
            this.btnExportExcel.Text = "📥";
            this.btnExportExcel.UseVisualStyleBackColor = false;
            // 
            // btnPrint
            // 
            this.btnPrint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnPrint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrint.FlatAppearance.BorderSize = 0;
            this.btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPrint.ForeColor = System.Drawing.Color.White;
            this.btnPrint.Location = new System.Drawing.Point(845, 18);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(45, 32);
            this.btnPrint.TabIndex = 10;
            this.btnPrint.Text = "🖨️";
            this.btnPrint.UseVisualStyleBackColor = false;
            // 
            // pnlSummary
            // 
            this.pnlSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pnlSummary.Controls.Add(this.pnlTotalDays);
            this.pnlSummary.Controls.Add(this.pnlPresentDays);
            this.pnlSummary.Controls.Add(this.pnlLateDays);
            this.pnlSummary.Controls.Add(this.pnlAbsentDays);
            this.pnlSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSummary.Location = new System.Drawing.Point(0, 140);
            this.pnlSummary.Name = "pnlSummary";
            this.pnlSummary.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.pnlSummary.Size = new System.Drawing.Size(900, 90);
            this.pnlSummary.TabIndex = 2;
            // 
            // pnlTotalDays
            // 
            this.pnlTotalDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.pnlTotalDays.Controls.Add(this.lblTotalDaysValue);
            this.pnlTotalDays.Controls.Add(this.lblTotalDaysTitle);
            this.pnlTotalDays.Location = new System.Drawing.Point(18, 13);
            this.pnlTotalDays.Margin = new System.Windows.Forms.Padding(3);
            this.pnlTotalDays.Name = "pnlTotalDays";
            this.pnlTotalDays.Size = new System.Drawing.Size(200, 65);
            this.pnlTotalDays.TabIndex = 0;
            // 
            // lblTotalDaysValue
            // 
            this.lblTotalDaysValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalDaysValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblTotalDaysValue.ForeColor = System.Drawing.Color.White;
            this.lblTotalDaysValue.Location = new System.Drawing.Point(0, 0);
            this.lblTotalDaysValue.Name = "lblTotalDaysValue";
            this.lblTotalDaysValue.Size = new System.Drawing.Size(200, 40);
            this.lblTotalDaysValue.TabIndex = 0;
            this.lblTotalDaysValue.Text = "22";
            this.lblTotalDaysValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTotalDaysTitle
            // 
            this.lblTotalDaysTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTotalDaysTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotalDaysTitle.ForeColor = System.Drawing.Color.White;
            this.lblTotalDaysTitle.Location = new System.Drawing.Point(0, 40);
            this.lblTotalDaysTitle.Name = "lblTotalDaysTitle";
            this.lblTotalDaysTitle.Size = new System.Drawing.Size(200, 25);
            this.lblTotalDaysTitle.TabIndex = 1;
            this.lblTotalDaysTitle.Text = "📅 Tổng ngày làm việc";
            this.lblTotalDaysTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlPresentDays
            // 
            this.pnlPresentDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.pnlPresentDays.Controls.Add(this.lblPresentDaysValue);
            this.pnlPresentDays.Controls.Add(this.lblPresentDaysTitle);
            this.pnlPresentDays.Location = new System.Drawing.Point(224, 13);
            this.pnlPresentDays.Margin = new System.Windows.Forms.Padding(3);
            this.pnlPresentDays.Name = "pnlPresentDays";
            this.pnlPresentDays.Size = new System.Drawing.Size(200, 65);
            this.pnlPresentDays.TabIndex = 1;
            // 
            // lblPresentDaysValue
            // 
            this.lblPresentDaysValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPresentDaysValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblPresentDaysValue.ForeColor = System.Drawing.Color.White;
            this.lblPresentDaysValue.Location = new System.Drawing.Point(0, 0);
            this.lblPresentDaysValue.Name = "lblPresentDaysValue";
            this.lblPresentDaysValue.Size = new System.Drawing.Size(200, 40);
            this.lblPresentDaysValue.TabIndex = 0;
            this.lblPresentDaysValue.Text = "20";
            this.lblPresentDaysValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPresentDaysTitle
            // 
            this.lblPresentDaysTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblPresentDaysTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPresentDaysTitle.ForeColor = System.Drawing.Color.White;
            this.lblPresentDaysTitle.Location = new System.Drawing.Point(0, 40);
            this.lblPresentDaysTitle.Name = "lblPresentDaysTitle";
            this.lblPresentDaysTitle.Size = new System.Drawing.Size(200, 25);
            this.lblPresentDaysTitle.TabIndex = 1;
            this.lblPresentDaysTitle.Text = "✅ Ngày đi làm";
            this.lblPresentDaysTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlLateDays
            // 
            this.pnlLateDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.pnlLateDays.Controls.Add(this.lblLateDaysValue);
            this.pnlLateDays.Controls.Add(this.lblLateDaysTitle);
            this.pnlLateDays.Location = new System.Drawing.Point(430, 13);
            this.pnlLateDays.Margin = new System.Windows.Forms.Padding(3);
            this.pnlLateDays.Name = "pnlLateDays";
            this.pnlLateDays.Size = new System.Drawing.Size(200, 65);
            this.pnlLateDays.TabIndex = 2;
            // 
            // lblLateDaysValue
            // 
            this.lblLateDaysValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLateDaysValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblLateDaysValue.ForeColor = System.Drawing.Color.White;
            this.lblLateDaysValue.Location = new System.Drawing.Point(0, 0);
            this.lblLateDaysValue.Name = "lblLateDaysValue";
            this.lblLateDaysValue.Size = new System.Drawing.Size(200, 40);
            this.lblLateDaysValue.TabIndex = 0;
            this.lblLateDaysValue.Text = "2";
            this.lblLateDaysValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLateDaysTitle
            // 
            this.lblLateDaysTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblLateDaysTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLateDaysTitle.ForeColor = System.Drawing.Color.White;
            this.lblLateDaysTitle.Location = new System.Drawing.Point(0, 40);
            this.lblLateDaysTitle.Name = "lblLateDaysTitle";
            this.lblLateDaysTitle.Size = new System.Drawing.Size(200, 25);
            this.lblLateDaysTitle.TabIndex = 1;
            this.lblLateDaysTitle.Text = "⚠️ Ngày đi trễ";
            this.lblLateDaysTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlAbsentDays
            // 
            this.pnlAbsentDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.pnlAbsentDays.Controls.Add(this.lblAbsentDaysValue);
            this.pnlAbsentDays.Controls.Add(this.lblAbsentDaysTitle);
            this.pnlAbsentDays.Location = new System.Drawing.Point(636, 13);
            this.pnlAbsentDays.Margin = new System.Windows.Forms.Padding(3);
            this.pnlAbsentDays.Name = "pnlAbsentDays";
            this.pnlAbsentDays.Size = new System.Drawing.Size(200, 65);
            this.pnlAbsentDays.TabIndex = 3;
            // 
            // lblAbsentDaysValue
            // 
            this.lblAbsentDaysValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAbsentDaysValue.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblAbsentDaysValue.ForeColor = System.Drawing.Color.White;
            this.lblAbsentDaysValue.Location = new System.Drawing.Point(0, 0);
            this.lblAbsentDaysValue.Name = "lblAbsentDaysValue";
            this.lblAbsentDaysValue.Size = new System.Drawing.Size(200, 40);
            this.lblAbsentDaysValue.TabIndex = 0;
            this.lblAbsentDaysValue.Text = "0";
            this.lblAbsentDaysValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAbsentDaysTitle
            // 
            this.lblAbsentDaysTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAbsentDaysTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAbsentDaysTitle.ForeColor = System.Drawing.Color.White;
            this.lblAbsentDaysTitle.Location = new System.Drawing.Point(0, 40);
            this.lblAbsentDaysTitle.Name = "lblAbsentDaysTitle";
            this.lblAbsentDaysTitle.Size = new System.Drawing.Size(200, 25);
            this.lblAbsentDaysTitle.TabIndex = 1;
            this.lblAbsentDaysTitle.Text = "❌ Ngày vắng";
            this.lblAbsentDaysTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlData
            // 
            this.pnlData.BackColor = System.Drawing.Color.White;
            this.pnlData.Controls.Add(this.dgvReport);
            this.pnlData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlData.Location = new System.Drawing.Point(0, 230);
            this.pnlData.Name = "pnlData";
            this.pnlData.Padding = new System.Windows.Forms.Padding(15);
            this.pnlData.Size = new System.Drawing.Size(900, 370);
            this.pnlData.TabIndex = 3;
            // 
            // dgvReport
            // 
            this.dgvReport.AllowUserToAddRows = false;
            this.dgvReport.AllowUserToDeleteRows = false;
            this.dgvReport.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvReport.BackgroundColor = System.Drawing.Color.White;
            this.dgvReport.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvReport.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvReport.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvReport.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReport.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDate,
            this.colEmployeeCode,
            this.colEmployeeName,
            this.colDepartment,
            this.colCheckIn,
            this.colCheckOut,
            this.colWorkHours,
            this.colStatus,
            this.colNote});
            this.dgvReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvReport.EnableHeadersVisualStyles = false;
            this.dgvReport.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.dgvReport.Location = new System.Drawing.Point(15, 15);
            this.dgvReport.Name = "dgvReport";
            this.dgvReport.ReadOnly = true;
            this.dgvReport.RowHeadersVisible = false;
            this.dgvReport.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvReport.Size = new System.Drawing.Size(870, 340);
            this.dgvReport.TabIndex = 0;
            // 
            // colDate
            // 
            this.colDate.FillWeight = 80F;
            this.colDate.HeaderText = "Ngày";
            this.colDate.Name = "colDate";
            this.colDate.ReadOnly = true;
            // 
            // colEmployeeCode
            // 
            this.colEmployeeCode.FillWeight = 60F;
            this.colEmployeeCode.HeaderText = "Mã NV";
            this.colEmployeeCode.Name = "colEmployeeCode";
            this.colEmployeeCode.ReadOnly = true;
            // 
            // colEmployeeName
            // 
            this.colEmployeeName.FillWeight = 120F;
            this.colEmployeeName.HeaderText = "Họ tên";
            this.colEmployeeName.Name = "colEmployeeName";
            this.colEmployeeName.ReadOnly = true;
            // 
            // colDepartment
            // 
            this.colDepartment.FillWeight = 90F;
            this.colDepartment.HeaderText = "Phòng ban";
            this.colDepartment.Name = "colDepartment";
            this.colDepartment.ReadOnly = true;
            // 
            // colCheckIn
            // 
            this.colCheckIn.FillWeight = 70F;
            this.colCheckIn.HeaderText = "Giờ vào";
            this.colCheckIn.Name = "colCheckIn";
            this.colCheckIn.ReadOnly = true;
            // 
            // colCheckOut
            // 
            this.colCheckOut.FillWeight = 70F;
            this.colCheckOut.HeaderText = "Giờ ra";
            this.colCheckOut.Name = "colCheckOut";
            this.colCheckOut.ReadOnly = true;
            // 
            // colWorkHours
            // 
            this.colWorkHours.FillWeight = 60F;
            this.colWorkHours.HeaderText = "Số giờ";
            this.colWorkHours.Name = "colWorkHours";
            this.colWorkHours.ReadOnly = true;
            // 
            // colStatus
            // 
            this.colStatus.FillWeight = 70F;
            this.colStatus.HeaderText = "Trạng thái";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // colNote
            // 
            this.colNote.FillWeight = 80F;
            this.colNote.HeaderText = "Ghi chú";
            this.colNote.Name = "colNote";
            this.colNote.ReadOnly = true;
            // 
            // UCAttendanceReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Controls.Add(this.pnlData);
            this.Controls.Add(this.pnlSummary);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.pnlHeader);
            this.Name = "UCAttendanceReport";
            this.Size = new System.Drawing.Size(900, 600);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            this.pnlSummary.ResumeLayout(false);
            this.pnlTotalDays.ResumeLayout(false);
            this.pnlPresentDays.ResumeLayout(false);
            this.pnlLateDays.ResumeLayout(false);
            this.pnlAbsentDays.ResumeLayout(false);
            this.pnlData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvReport)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlFilter;
        private System.Windows.Forms.Label lblFromDate;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.Label lblToDate;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label lblEmployee;
        private System.Windows.Forms.ComboBox cboEmployee;
        private System.Windows.Forms.Label lblDepartment;
        private System.Windows.Forms.ComboBox cboDepartment;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.FlowLayoutPanel pnlSummary;
        private System.Windows.Forms.Panel pnlTotalDays;
        private System.Windows.Forms.Label lblTotalDaysValue;
        private System.Windows.Forms.Label lblTotalDaysTitle;
        private System.Windows.Forms.Panel pnlPresentDays;
        private System.Windows.Forms.Label lblPresentDaysValue;
        private System.Windows.Forms.Label lblPresentDaysTitle;
        private System.Windows.Forms.Panel pnlLateDays;
        private System.Windows.Forms.Label lblLateDaysValue;
        private System.Windows.Forms.Label lblLateDaysTitle;
        private System.Windows.Forms.Panel pnlAbsentDays;
        private System.Windows.Forms.Label lblAbsentDaysValue;
        private System.Windows.Forms.Label lblAbsentDaysTitle;
        private System.Windows.Forms.Panel pnlData;
        private System.Windows.Forms.DataGridView dgvReport;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmployeeCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmployeeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDepartment;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCheckIn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCheckOut;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWorkHours;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNote;
    }
}

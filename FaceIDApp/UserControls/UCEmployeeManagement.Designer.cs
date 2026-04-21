namespace FaceIDApp.UserControls
{
    partial class UCEmployeeManagement
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
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.cboFilterDepartment = new System.Windows.Forms.ComboBox();
            this.pnlContent = new System.Windows.Forms.SplitContainer();
            this.dgvEmployees = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDepartment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPhone = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFaceStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlEmployeeDetail = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.picEmployeePhoto = new System.Windows.Forms.PictureBox();
            this.lblEmployeeCode = new System.Windows.Forms.Label();
            this.txtEmployeeCode = new System.Windows.Forms.TextBox();
            this.lblFullName = new System.Windows.Forms.Label();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.lblDepartment = new System.Windows.Forms.Label();
            this.cboDepartment = new System.Windows.Forms.ComboBox();
            this.lblPosition = new System.Windows.Forms.Label();
            this.txtPosition = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblPhone = new System.Windows.Forms.Label();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.lblDateOfBirth = new System.Windows.Forms.Label();
            this.dtpDateOfBirth = new System.Windows.Forms.DateTimePicker();
            this.lblHireDate = new System.Windows.Forms.Label();
            this.dtpHireDate = new System.Windows.Forms.DateTimePicker();
            this.chkIsActive = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRegisterFace = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlContent)).BeginInit();
            this.pnlContent.Panel1.SuspendLayout();
            this.pnlContent.Panel2.SuspendLayout();
            this.pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmployees)).BeginInit();
            this.pnlEmployeeDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picEmployeePhoto)).BeginInit();
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
            this.lblTitle.Size = new System.Drawing.Size(268, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "👥 Quản lý nhân viên";
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pnlToolbar.Controls.Add(this.cboFilterDepartment);
            this.pnlToolbar.Controls.Add(this.btnSearch);
            this.pnlToolbar.Controls.Add(this.txtSearch);
            this.pnlToolbar.Controls.Add(this.btnRefresh);
            this.pnlToolbar.Controls.Add(this.btnDelete);
            this.pnlToolbar.Controls.Add(this.btnEdit);
            this.pnlToolbar.Controls.Add(this.btnAdd);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 70);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.pnlToolbar.Size = new System.Drawing.Size(900, 60);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(18, 13);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 35);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "➕ Thêm mới";
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(124, 13);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(80, 35);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "✏️ Sửa";
            this.btnEdit.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(210, 13);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 35);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "🗑️ Xóa";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(296, 13);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 35);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "🔄 Làm mới";
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Location = new System.Drawing.Point(500, 17);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(180, 25);
            this.txtSearch.TabIndex = 4;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(686, 13);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(80, 35);
            this.btnSearch.TabIndex = 5;
            this.btnSearch.Text = "🔍 Tìm";
            this.btnSearch.UseVisualStyleBackColor = false;
            // 
            // cboFilterDepartment
            // 
            this.cboFilterDepartment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFilterDepartment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilterDepartment.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboFilterDepartment.FormattingEnabled = true;
            this.cboFilterDepartment.Items.AddRange(new object[] {
            "-- Tất cả phòng ban --",
            "Phòng IT",
            "Phòng Kế toán",
            "Phòng Nhân sự",
            "Phòng Marketing",
            "Phòng Kinh doanh"});
            this.cboFilterDepartment.Location = new System.Drawing.Point(772, 16);
            this.cboFilterDepartment.Name = "cboFilterDepartment";
            this.cboFilterDepartment.Size = new System.Drawing.Size(113, 25);
            this.cboFilterDepartment.TabIndex = 6;
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 130);
            this.pnlContent.Name = "pnlContent";
            // 
            // pnlContent.Panel1
            // 
            this.pnlContent.Panel1.Controls.Add(this.dgvEmployees);
            this.pnlContent.Panel1.Padding = new System.Windows.Forms.Padding(15, 10, 5, 15);
            // 
            // pnlContent.Panel2
            // 
            this.pnlContent.Panel2.Controls.Add(this.pnlEmployeeDetail);
            this.pnlContent.Panel2.Padding = new System.Windows.Forms.Padding(5, 10, 15, 15);
            this.pnlContent.Size = new System.Drawing.Size(900, 470);
            this.pnlContent.SplitterDistance = 550;
            this.pnlContent.TabIndex = 2;
            // 
            // dgvEmployees
            // 
            this.dgvEmployees.AllowUserToAddRows = false;
            this.dgvEmployees.AllowUserToDeleteRows = false;
            this.dgvEmployees.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvEmployees.BackgroundColor = System.Drawing.Color.White;
            this.dgvEmployees.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvEmployees.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvEmployees.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvEmployees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEmployees.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colCode,
            this.colFullName,
            this.colDepartment,
            this.colPosition,
            this.colPhone,
            this.colFaceStatus,
            this.colStatus});
            this.dgvEmployees.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEmployees.EnableHeadersVisualStyles = false;
            this.dgvEmployees.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.dgvEmployees.Location = new System.Drawing.Point(15, 10);
            this.dgvEmployees.MultiSelect = false;
            this.dgvEmployees.Name = "dgvEmployees";
            this.dgvEmployees.ReadOnly = true;
            this.dgvEmployees.RowHeadersVisible = false;
            this.dgvEmployees.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvEmployees.Size = new System.Drawing.Size(530, 445);
            this.dgvEmployees.TabIndex = 0;
            // 
            // colId
            // 
            this.colId.HeaderText = "ID";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Visible = false;
            // 
            // colCode
            // 
            this.colCode.FillWeight = 70F;
            this.colCode.HeaderText = "Mã NV";
            this.colCode.Name = "colCode";
            this.colCode.ReadOnly = true;
            // 
            // colFullName
            // 
            this.colFullName.FillWeight = 120F;
            this.colFullName.HeaderText = "Họ tên";
            this.colFullName.Name = "colFullName";
            this.colFullName.ReadOnly = true;
            // 
            // colDepartment
            // 
            this.colDepartment.HeaderText = "Phòng ban";
            this.colDepartment.Name = "colDepartment";
            this.colDepartment.ReadOnly = true;
            // 
            // colPosition
            // 
            this.colPosition.HeaderText = "Chức vụ";
            this.colPosition.Name = "colPosition";
            this.colPosition.ReadOnly = true;
            // 
            // colPhone
            // 
            this.colPhone.FillWeight = 90F;
            this.colPhone.HeaderText = "SĐT";
            this.colPhone.Name = "colPhone";
            this.colPhone.ReadOnly = true;
            // 
            // colFaceStatus
            // 
            this.colFaceStatus.FillWeight = 70F;
            this.colFaceStatus.HeaderText = "Face ID";
            this.colFaceStatus.Name = "colFaceStatus";
            this.colFaceStatus.ReadOnly = true;
            // 
            // colStatus
            // 
            this.colStatus.FillWeight = 70F;
            this.colStatus.HeaderText = "Trạng thái";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // pnlEmployeeDetail
            // 
            this.pnlEmployeeDetail.AutoScroll = true;
            this.pnlEmployeeDetail.BackColor = System.Drawing.Color.White;
            this.pnlEmployeeDetail.Controls.Add(this.btnRegisterFace);
            this.pnlEmployeeDetail.Controls.Add(this.btnCancel);
            this.pnlEmployeeDetail.Controls.Add(this.btnSave);
            this.pnlEmployeeDetail.Controls.Add(this.chkIsActive);
            this.pnlEmployeeDetail.Controls.Add(this.dtpHireDate);
            this.pnlEmployeeDetail.Controls.Add(this.lblHireDate);
            this.pnlEmployeeDetail.Controls.Add(this.dtpDateOfBirth);
            this.pnlEmployeeDetail.Controls.Add(this.lblDateOfBirth);
            this.pnlEmployeeDetail.Controls.Add(this.txtPhone);
            this.pnlEmployeeDetail.Controls.Add(this.lblPhone);
            this.pnlEmployeeDetail.Controls.Add(this.txtEmail);
            this.pnlEmployeeDetail.Controls.Add(this.lblEmail);
            this.pnlEmployeeDetail.Controls.Add(this.txtPosition);
            this.pnlEmployeeDetail.Controls.Add(this.lblPosition);
            this.pnlEmployeeDetail.Controls.Add(this.cboDepartment);
            this.pnlEmployeeDetail.Controls.Add(this.lblDepartment);
            this.pnlEmployeeDetail.Controls.Add(this.txtFullName);
            this.pnlEmployeeDetail.Controls.Add(this.lblFullName);
            this.pnlEmployeeDetail.Controls.Add(this.txtEmployeeCode);
            this.pnlEmployeeDetail.Controls.Add(this.lblEmployeeCode);
            this.pnlEmployeeDetail.Controls.Add(this.picEmployeePhoto);
            this.pnlEmployeeDetail.Controls.Add(this.lblDetailTitle);
            this.pnlEmployeeDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEmployeeDetail.Location = new System.Drawing.Point(5, 10);
            this.pnlEmployeeDetail.Name = "pnlEmployeeDetail";
            this.pnlEmployeeDetail.Padding = new System.Windows.Forms.Padding(15);
            this.pnlEmployeeDetail.Size = new System.Drawing.Size(330, 445);
            this.pnlEmployeeDetail.TabIndex = 0;
            // 
            // lblDetailTitle
            // 
            this.lblDetailTitle.AutoSize = true;
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblDetailTitle.Location = new System.Drawing.Point(15, 15);
            this.lblDetailTitle.Name = "lblDetailTitle";
            this.lblDetailTitle.Size = new System.Drawing.Size(153, 21);
            this.lblDetailTitle.TabIndex = 0;
            this.lblDetailTitle.Text = "📝 Thông tin chi tiết";
            // 
            // picEmployeePhoto
            // 
            this.picEmployeePhoto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.picEmployeePhoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picEmployeePhoto.Location = new System.Drawing.Point(100, 45);
            this.picEmployeePhoto.Name = "picEmployeePhoto";
            this.picEmployeePhoto.Size = new System.Drawing.Size(100, 120);
            this.picEmployeePhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picEmployeePhoto.TabIndex = 1;
            this.picEmployeePhoto.TabStop = false;
            // 
            // lblEmployeeCode
            // 
            this.lblEmployeeCode.AutoSize = true;
            this.lblEmployeeCode.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEmployeeCode.Location = new System.Drawing.Point(15, 175);
            this.lblEmployeeCode.Name = "lblEmployeeCode";
            this.lblEmployeeCode.Size = new System.Drawing.Size(51, 17);
            this.lblEmployeeCode.TabIndex = 2;
            this.lblEmployeeCode.Text = "Mã NV:";
            // 
            // txtEmployeeCode
            // 
            this.txtEmployeeCode.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtEmployeeCode.Location = new System.Drawing.Point(100, 172);
            this.txtEmployeeCode.Name = "txtEmployeeCode";
            this.txtEmployeeCode.Size = new System.Drawing.Size(210, 24);
            this.txtEmployeeCode.TabIndex = 3;
            // 
            // lblFullName
            // 
            this.lblFullName.AutoSize = true;
            this.lblFullName.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblFullName.Location = new System.Drawing.Point(15, 205);
            this.lblFullName.Name = "lblFullName";
            this.lblFullName.Size = new System.Drawing.Size(52, 17);
            this.lblFullName.TabIndex = 4;
            this.lblFullName.Text = "Họ tên:";
            // 
            // txtFullName
            // 
            this.txtFullName.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtFullName.Location = new System.Drawing.Point(100, 202);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Size = new System.Drawing.Size(210, 24);
            this.txtFullName.TabIndex = 5;
            // 
            // lblDepartment
            // 
            this.lblDepartment.AutoSize = true;
            this.lblDepartment.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDepartment.Location = new System.Drawing.Point(15, 235);
            this.lblDepartment.Name = "lblDepartment";
            this.lblDepartment.Size = new System.Drawing.Size(75, 17);
            this.lblDepartment.TabIndex = 6;
            this.lblDepartment.Text = "Phòng ban:";
            // 
            // cboDepartment
            // 
            this.cboDepartment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDepartment.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboDepartment.FormattingEnabled = true;
            this.cboDepartment.Items.AddRange(new object[] {
            "Phòng IT",
            "Phòng Kế toán",
            "Phòng Nhân sự",
            "Phòng Marketing",
            "Phòng Kinh doanh"});
            this.cboDepartment.Location = new System.Drawing.Point(100, 232);
            this.cboDepartment.Name = "cboDepartment";
            this.cboDepartment.Size = new System.Drawing.Size(210, 25);
            this.cboDepartment.TabIndex = 7;
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblPosition.Location = new System.Drawing.Point(15, 265);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(58, 17);
            this.lblPosition.TabIndex = 8;
            this.lblPosition.Text = "Chức vụ:";
            // 
            // txtPosition
            // 
            this.txtPosition.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtPosition.Location = new System.Drawing.Point(100, 262);
            this.txtPosition.Name = "txtPosition";
            this.txtPosition.Size = new System.Drawing.Size(210, 24);
            this.txtPosition.TabIndex = 9;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEmail.Location = new System.Drawing.Point(15, 295);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(42, 17);
            this.lblEmail.TabIndex = 10;
            this.lblEmail.Text = "Email:";
            // 
            // txtEmail
            // 
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtEmail.Location = new System.Drawing.Point(100, 292);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(210, 24);
            this.txtEmail.TabIndex = 11;
            // 
            // lblPhone
            // 
            this.lblPhone.AutoSize = true;
            this.lblPhone.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblPhone.Location = new System.Drawing.Point(15, 325);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(33, 17);
            this.lblPhone.TabIndex = 12;
            this.lblPhone.Text = "SĐT:";
            // 
            // txtPhone
            // 
            this.txtPhone.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtPhone.Location = new System.Drawing.Point(100, 322);
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(210, 24);
            this.txtPhone.TabIndex = 13;
            // 
            // lblDateOfBirth
            // 
            this.lblDateOfBirth.AutoSize = true;
            this.lblDateOfBirth.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDateOfBirth.Location = new System.Drawing.Point(15, 355);
            this.lblDateOfBirth.Name = "lblDateOfBirth";
            this.lblDateOfBirth.Size = new System.Drawing.Size(71, 17);
            this.lblDateOfBirth.TabIndex = 14;
            this.lblDateOfBirth.Text = "Ngày sinh:";
            // 
            // dtpDateOfBirth
            // 
            this.dtpDateOfBirth.CustomFormat = "dd/MM/yyyy";
            this.dtpDateOfBirth.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpDateOfBirth.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDateOfBirth.Location = new System.Drawing.Point(100, 352);
            this.dtpDateOfBirth.Name = "dtpDateOfBirth";
            this.dtpDateOfBirth.Size = new System.Drawing.Size(210, 24);
            this.dtpDateOfBirth.TabIndex = 15;
            // 
            // lblHireDate
            // 
            this.lblHireDate.AutoSize = true;
            this.lblHireDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblHireDate.Location = new System.Drawing.Point(15, 385);
            this.lblHireDate.Name = "lblHireDate";
            this.lblHireDate.Size = new System.Drawing.Size(85, 17);
            this.lblHireDate.TabIndex = 16;
            this.lblHireDate.Text = "Ngày vào LV:";
            // 
            // dtpHireDate
            // 
            this.dtpHireDate.CustomFormat = "dd/MM/yyyy";
            this.dtpHireDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpHireDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpHireDate.Location = new System.Drawing.Point(100, 382);
            this.dtpHireDate.Name = "dtpHireDate";
            this.dtpHireDate.Size = new System.Drawing.Size(210, 24);
            this.dtpHireDate.TabIndex = 17;
            // 
            // chkIsActive
            // 
            this.chkIsActive.AutoSize = true;
            this.chkIsActive.Checked = true;
            this.chkIsActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIsActive.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkIsActive.Location = new System.Drawing.Point(100, 412);
            this.chkIsActive.Name = "chkIsActive";
            this.chkIsActive.Size = new System.Drawing.Size(104, 21);
            this.chkIsActive.TabIndex = 18;
            this.chkIsActive.Text = "Đang làm việc";
            this.chkIsActive.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(18, 440);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 19;
            this.btnSave.Text = "💾 Lưu";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(114, 440);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "❌ Hủy";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnRegisterFace
            // 
            this.btnRegisterFace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.btnRegisterFace.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegisterFace.FlatAppearance.BorderSize = 0;
            this.btnRegisterFace.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegisterFace.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRegisterFace.ForeColor = System.Drawing.Color.White;
            this.btnRegisterFace.Location = new System.Drawing.Point(210, 440);
            this.btnRegisterFace.Name = "btnRegisterFace";
            this.btnRegisterFace.Size = new System.Drawing.Size(100, 35);
            this.btnRegisterFace.TabIndex = 21;
            this.btnRegisterFace.Text = "📷 Đăng ký Face";
            this.btnRegisterFace.UseVisualStyleBackColor = false;
            // 
            // UCEmployeeManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Name = "UCEmployeeManagement";
            this.Size = new System.Drawing.Size(900, 600);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlContent.Panel1.ResumeLayout(false);
            this.pnlContent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlContent)).EndInit();
            this.pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmployees)).EndInit();
            this.pnlEmployeeDetail.ResumeLayout(false);
            this.pnlEmployeeDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picEmployeePhoto)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ComboBox cboFilterDepartment;
        private System.Windows.Forms.SplitContainer pnlContent;
        private System.Windows.Forms.DataGridView dgvEmployees;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFullName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDepartment;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPhone;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFaceStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.Panel pnlEmployeeDetail;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.PictureBox picEmployeePhoto;
        private System.Windows.Forms.Label lblEmployeeCode;
        private System.Windows.Forms.TextBox txtEmployeeCode;
        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.Label lblDepartment;
        private System.Windows.Forms.ComboBox cboDepartment;
        private System.Windows.Forms.Label lblPosition;
        private System.Windows.Forms.TextBox txtPosition;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblDateOfBirth;
        private System.Windows.Forms.DateTimePicker dtpDateOfBirth;
        private System.Windows.Forms.Label lblHireDate;
        private System.Windows.Forms.DateTimePicker dtpHireDate;
        private System.Windows.Forms.CheckBox chkIsActive;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRegisterFace;
    }
}

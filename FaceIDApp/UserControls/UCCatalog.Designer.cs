namespace FaceIDApp.UserControls
{
    partial class UCCatalog
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.tabMain        = new System.Windows.Forms.TabControl();
            this.tabDepartments = new System.Windows.Forms.TabPage();
            this.tabPositions   = new System.Windows.Forms.TabPage();
            this.tabShifts      = new System.Windows.Forms.TabPage();
            this.tabDevices     = new System.Windows.Forms.TabPage();
            this.tabCalendars   = new System.Windows.Forms.TabPage();
            this.tabShiftSched  = new System.Windows.Forms.TabPage();
            this.tabMain.SuspendLayout();
            this.SuspendLayout();
            // tabMain
            this.tabMain.Controls.Add(this.tabDepartments);
            this.tabMain.Controls.Add(this.tabPositions);
            this.tabMain.Controls.Add(this.tabShifts);
            this.tabMain.Controls.Add(this.tabDevices);
            this.tabMain.Controls.Add(this.tabCalendars);
            this.tabMain.Controls.Add(this.tabShiftSched);
            this.tabMain.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.tabMain.Location      = new System.Drawing.Point(16, 16);
            this.tabMain.Name          = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size          = new System.Drawing.Size(968, 632);
            // tabDepartments
            this.tabDepartments.Text      = "🏢 Phòng ban";
            this.tabDepartments.BackColor = System.Drawing.Color.White;
            this.tabDepartments.Padding   = new System.Windows.Forms.Padding(10);
            // tabPositions
            this.tabPositions.Text      = "💼 Chức vụ";
            this.tabPositions.BackColor = System.Drawing.Color.White;
            this.tabPositions.Padding   = new System.Windows.Forms.Padding(10);
            // tabShifts
            this.tabShifts.Text      = "🕐 Ca làm việc";
            this.tabShifts.BackColor = System.Drawing.Color.White;
            this.tabShifts.Padding   = new System.Windows.Forms.Padding(10);
            // tabDevices
            this.tabDevices.Text      = "📱 Thiết bị";
            this.tabDevices.BackColor = System.Drawing.Color.White;
            this.tabDevices.Padding   = new System.Windows.Forms.Padding(10);
            // tabCalendars
            this.tabCalendars.Text      = "📅 Lịch làm việc";
            this.tabCalendars.BackColor = System.Drawing.Color.White;
            this.tabCalendars.Padding   = new System.Windows.Forms.Padding(10);
            // tabShiftSched
            this.tabShiftSched.Text      = "🗓️ Lịch ca NV";
            this.tabShiftSched.BackColor = System.Drawing.Color.White;
            this.tabShiftSched.Padding   = new System.Windows.Forms.Padding(10);
            // UCCatalog
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(248, 250, 252);
            this.Controls.Add(this.tabMain);
            this.Font    = new System.Drawing.Font("Segoe UI", 9F);
            this.Name    = "UCCatalog";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.Size    = new System.Drawing.Size(1000, 664);
            this.tabMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabDepartments;
        private System.Windows.Forms.TabPage tabPositions;
        private System.Windows.Forms.TabPage tabShifts;
        private System.Windows.Forms.TabPage tabDevices;
        private System.Windows.Forms.TabPage tabCalendars;
        private System.Windows.Forms.TabPage tabShiftSched;
    }
}

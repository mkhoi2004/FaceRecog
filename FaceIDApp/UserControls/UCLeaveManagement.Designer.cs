namespace FaceIDApp.UserControls
{
    partial class UCLeaveManagement
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabLeave = new System.Windows.Forms.TabPage();
            this.tabHolidays = new System.Windows.Forms.TabPage();
            this.tabMain.SuspendLayout();
            this.SuspendLayout();
            // tabMain
            this.tabMain.Controls.Add(this.tabLeave);
            this.tabMain.Controls.Add(this.tabHolidays);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabMain.Location = new System.Drawing.Point(16, 16);
            this.tabMain.Size = new System.Drawing.Size(968, 632);
            // tabLeave
            this.tabLeave.Text = "📋 Đơn nghỉ phép";
            this.tabLeave.BackColor = System.Drawing.Color.White;
            this.tabLeave.Padding = new System.Windows.Forms.Padding(10);
            // tabHolidays
            this.tabHolidays.Text = "🎌 Ngày lễ";
            this.tabHolidays.BackColor = System.Drawing.Color.White;
            this.tabHolidays.Padding = new System.Windows.Forms.Padding(10);
            // UCLeaveManagement
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.Controls.Add(this.tabMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Padding = new System.Windows.Forms.Padding(16);
            this.Size = new System.Drawing.Size(1000, 664);
            this.tabMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabLeave;
        private System.Windows.Forms.TabPage tabHolidays;
    }
}

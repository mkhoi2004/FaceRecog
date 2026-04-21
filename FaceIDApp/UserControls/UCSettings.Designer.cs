namespace FaceIDApp.UserControls
{
    partial class UCSettings
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.tabMain    = new System.Windows.Forms.TabControl();
            this.tabUsers   = new System.Windows.Forms.TabPage();
            this.tabSystem  = new System.Windows.Forms.TabPage();
            this.tabConfig  = new System.Windows.Forms.TabPage();
            this.tabAudit   = new System.Windows.Forms.TabPage();
            this.tabFaceLog = new System.Windows.Forms.TabPage();
            this.tabMain.SuspendLayout();
            this.SuspendLayout();
            // tabMain
            this.tabMain.Controls.Add(this.tabUsers);
            this.tabMain.Controls.Add(this.tabSystem);
            this.tabMain.Controls.Add(this.tabConfig);
            this.tabMain.Controls.Add(this.tabAudit);
            this.tabMain.Controls.Add(this.tabFaceLog);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabMain.Location = new System.Drawing.Point(16, 16);
            this.tabMain.Size = new System.Drawing.Size(968, 632);
            // tabUsers
            this.tabUsers.Text      = "👤 Tài khoản";
            this.tabUsers.BackColor = System.Drawing.Color.White;
            this.tabUsers.Padding   = new System.Windows.Forms.Padding(10);
            // tabSystem
            this.tabSystem.Text      = "⚙️ Hệ thống";
            this.tabSystem.BackColor = System.Drawing.Color.White;
            this.tabSystem.Padding   = new System.Windows.Forms.Padding(10);
            // tabConfig
            this.tabConfig.Text      = "🔧 Cài đặt";
            this.tabConfig.BackColor = System.Drawing.Color.White;
            this.tabConfig.Padding   = new System.Windows.Forms.Padding(10);
            // tabAudit
            this.tabAudit.Text      = "📝 Audit Log";
            this.tabAudit.BackColor = System.Drawing.Color.White;
            this.tabAudit.Padding   = new System.Windows.Forms.Padding(10);
            // tabFaceLog
            this.tabFaceLog.Text      = "📸 Nhật ký Face";
            this.tabFaceLog.BackColor = System.Drawing.Color.White;
            this.tabFaceLog.Padding   = new System.Windows.Forms.Padding(10);
            // UCSettings
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.Controls.Add(this.tabMain);
            this.Font    = new System.Drawing.Font("Segoe UI", 9F);
            this.Padding = new System.Windows.Forms.Padding(16);
            this.Size    = new System.Drawing.Size(1000, 664);
            this.tabMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabUsers;
        private System.Windows.Forms.TabPage tabSystem;
        private System.Windows.Forms.TabPage tabConfig;
        private System.Windows.Forms.TabPage tabAudit;
        private System.Windows.Forms.TabPage tabFaceLog;
    }
}

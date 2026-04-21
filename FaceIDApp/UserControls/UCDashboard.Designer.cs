namespace FaceIDApp.UserControls
{
    partial class UCDashboard
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.pnlCards = new System.Windows.Forms.FlowLayoutPanel();
            this.dgvToday = new System.Windows.Forms.DataGridView();
            this.lblTableTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvToday)).BeginInit();
            this.SuspendLayout();
            // pnlCards
            this.pnlCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCards.AutoSize = true;
            this.pnlCards.Padding = new System.Windows.Forms.Padding(10);
            this.pnlCards.Location = new System.Drawing.Point(16, 16);
            this.pnlCards.Size = new System.Drawing.Size(968, 130);
            // lblTableTitle
            this.lblTableTitle.Text = "📋 Chấm công hôm nay";
            this.lblTableTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTableTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTableTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTableTitle.Height = 40;
            this.lblTableTitle.Padding = new System.Windows.Forms.Padding(16, 10, 0, 0);
            // dgvToday
            this.dgvToday.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvToday.BackgroundColor = System.Drawing.Color.White;
            this.dgvToday.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvToday.AllowUserToAddRows = false;
            this.dgvToday.AllowUserToDeleteRows = false;
            this.dgvToday.ReadOnly = true;
            this.dgvToday.RowHeadersVisible = false;
            this.dgvToday.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvToday.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvToday.ColumnHeadersHeight = 38;
            this.dgvToday.RowTemplate.Height = 32;
            // UCDashboard
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.Controls.Add(this.dgvToday);
            this.Controls.Add(this.lblTableTitle);
            this.Controls.Add(this.pnlCards);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Padding = new System.Windows.Forms.Padding(16);
            this.Size = new System.Drawing.Size(1000, 664);
            ((System.ComponentModel.ISupportInitialize)(this.dgvToday)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.FlowLayoutPanel pnlCards;
        private System.Windows.Forms.DataGridView dgvToday;
        private System.Windows.Forms.Label lblTableTitle;
    }
}

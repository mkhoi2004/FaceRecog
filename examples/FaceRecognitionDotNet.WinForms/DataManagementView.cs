using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceRecognitionDotNet.WinForms.Data;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FaceRecognitionDotNet.WinForms
{
    public sealed class DataManagementView : UserControl
    {
        private readonly Panel _HeaderPanel;
        private readonly Panel _SessionsCard;
        private readonly Panel _DetectionsCard;
        private readonly DataGridView _SessionsGrid;
        private readonly DataGridView _DetectionsGrid;
        private readonly Label _StatusLabel;
        private readonly Button _RefreshButton;
        private readonly Label _SessionSummaryLabel;
        private readonly Label _DetectionSummaryLabel;
        private readonly BindingList<ScanSessionItem> _Sessions;
        private readonly BindingList<DetectionItem> _Detections;
        private Guid? _SelectedSessionId;

        public DataManagementView()
        {
            this._HeaderPanel = new Panel();
            this._SessionsCard = new Panel();
            this._DetectionsCard = new Panel();
            this._SessionsGrid = new DataGridView();
            this._DetectionsGrid = new DataGridView();
            this._StatusLabel = new Label();
            this._RefreshButton = new Button();
            this._SessionSummaryLabel = new Label();
            this._DetectionSummaryLabel = new Label();
            this._Sessions = new BindingList<ScanSessionItem>();
            this._Detections = new BindingList<DetectionItem>();
            this.InitializeComponent();
        }

        private async void DataManagementViewOnLoad(object sender, EventArgs e)
        {
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        private void InitializeComponent()
        {
            var rootPanel = new Panel();
            var accentBar = new Panel();
            var headerTitle = new Label();
            var headerSubtitle = new Label();
            var sessionsTitle = new Label();
            var detectionsTitle = new Label();

            this.Dock = DockStyle.Fill;
            this.Load += this.DataManagementViewOnLoad;

            rootPanel.Dock = DockStyle.Fill;
            rootPanel.Padding = new Padding(18);
            rootPanel.BackColor = Color.FromArgb(245, 247, 250);

            this._HeaderPanel.Dock = DockStyle.Top;
            this._HeaderPanel.Height = 100;
            this._HeaderPanel.BackColor = Color.White;
            this._HeaderPanel.Padding = new Padding(20, 18, 20, 16);

            accentBar.Dock = DockStyle.Left;
            accentBar.Width = 6;
            accentBar.BackColor = Color.FromArgb(37, 99, 235);

            headerTitle.AutoSize = true;
            headerTitle.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point);
            headerTitle.Location = new System.Drawing.Point(70, 14);
            headerTitle.Text = "Khám phá cơ sở dữ liệu";

            headerSubtitle.AutoSize = true;
            headerSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            headerSubtitle.Location = new System.Drawing.Point(72, 50);
            headerSubtitle.Text = "Duyệt các phiên quét PostgreSQL và dữ liệu phát hiện khuôn mặt do ứng dụng WinForms lưu lại.";

            this._RefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._RefreshButton.Location = new System.Drawing.Point(820, 30);
            this._RefreshButton.Size = new System.Drawing.Size(110, 30);
            this._RefreshButton.Text = "Tải lại";
            this._RefreshButton.Click += async (s, e) => await this.LoadDataAsync().ConfigureAwait(true);

            this._StatusLabel.AutoSize = false;
            this._StatusLabel.Location = new System.Drawing.Point(430, 34);
            this._StatusLabel.Size = new System.Drawing.Size(360, 24);
            this._StatusLabel.Text = "Sẵn sàng.";

            this._HeaderPanel.Controls.Add(headerTitle);
            this._HeaderPanel.Controls.Add(headerSubtitle);
            this._HeaderPanel.Controls.Add(this._RefreshButton);
            this._HeaderPanel.Controls.Add(this._StatusLabel);
            this._HeaderPanel.Controls.Add(accentBar);

            this._SessionsCard.Dock = DockStyle.Left;
            this._SessionsCard.Width = 480;
            this._SessionsCard.BackColor = Color.White;
            this._SessionsCard.Padding = new Padding(18);
            this._SessionsCard.BorderStyle = BorderStyle.FixedSingle;

            sessionsTitle.AutoSize = true;
            sessionsTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            sessionsTitle.Location = new System.Drawing.Point(18, 14);
            sessionsTitle.Text = "Phiên quét";

            this._SessionSummaryLabel.AutoSize = false;
            this._SessionSummaryLabel.Location = new System.Drawing.Point(160, 15);
            this._SessionSummaryLabel.Size = new System.Drawing.Size(280, 20);
            this._SessionSummaryLabel.ForeColor = Color.FromArgb(100, 116, 139);
            this._SessionSummaryLabel.Text = "0 bản ghi";

            this._SessionsGrid.Dock = DockStyle.Fill;
            this._SessionsGrid.Location = new System.Drawing.Point(18, 44);
            this._SessionsGrid.ReadOnly = true;
            this._SessionsGrid.AllowUserToAddRows = false;
            this._SessionsGrid.AllowUserToDeleteRows = false;
            this._SessionsGrid.AutoGenerateColumns = false;
            this._SessionsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._SessionsGrid.MultiSelect = false;
            this._SessionsGrid.RowHeadersVisible = false;
            this._SessionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._SessionsGrid.DataSource = this._Sessions;
            this._SessionsGrid.SelectionChanged += this.SessionsGridOnSelectionChanged;

            this._SessionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StartedAt", HeaderText = "Bắt đầu", FillWeight = 125 });
            this._SessionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ScanType", HeaderText = "Loại", FillWeight = 70 });
            this._SessionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ModelName", HeaderText = "Model", FillWeight = 75 });
            this._SessionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ResultCount", HeaderText = "Kết quả", FillWeight = 55 });
            this._SessionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", FillWeight = 75 });

            this._SessionsCard.Controls.Add(this._SessionsGrid);
            this._SessionsCard.Controls.Add(sessionsTitle);
            this._SessionsCard.Controls.Add(this._SessionSummaryLabel);

            this._DetectionsCard.Dock = DockStyle.Fill;
            this._DetectionsCard.BackColor = Color.White;
            this._DetectionsCard.Padding = new Padding(18);
            this._DetectionsCard.BorderStyle = BorderStyle.FixedSingle;

            detectionsTitle.AutoSize = true;
            detectionsTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            detectionsTitle.Location = new System.Drawing.Point(18, 14);
            detectionsTitle.Text = "Khuôn mặt phát hiện";

            this._DetectionSummaryLabel.AutoSize = false;
            this._DetectionSummaryLabel.Location = new System.Drawing.Point(130, 15);
            this._DetectionSummaryLabel.Size = new System.Drawing.Size(320, 20);
            this._DetectionSummaryLabel.ForeColor = Color.FromArgb(100, 116, 139);
            this._DetectionSummaryLabel.Text = "0 bản ghi";

            this._DetectionsGrid.Dock = DockStyle.Fill;
            this._DetectionsGrid.Location = new System.Drawing.Point(18, 44);
            this._DetectionsGrid.ReadOnly = true;
            this._DetectionsGrid.AllowUserToAddRows = false;
            this._DetectionsGrid.AllowUserToDeleteRows = false;
            this._DetectionsGrid.AutoGenerateColumns = false;
            this._DetectionsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._DetectionsGrid.MultiSelect = false;
            this._DetectionsGrid.RowHeadersVisible = false;
            this._DetectionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._DetectionsGrid.DataSource = this._Detections;

            this._DetectionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FileName", HeaderText = "Ảnh", FillWeight = 140 });
            this._DetectionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Top", HeaderText = "Trên", FillWeight = 40 });
            this._DetectionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Right", HeaderText = "Phải", FillWeight = 40 });
            this._DetectionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Bottom", HeaderText = "Dưới", FillWeight = 40 });
            this._DetectionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Left", HeaderText = "Trái", FillWeight = 40 });
            this._DetectionsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreatedAt", HeaderText = "Tạo lúc", FillWeight = 110 });

            this._DetectionsCard.Controls.Add(this._DetectionsGrid);
            this._DetectionsCard.Controls.Add(detectionsTitle);
            this._DetectionsCard.Controls.Add(this._DetectionSummaryLabel);

            var split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Vertical;
            split.SplitterDistance = 500;
            split.Panel1.Padding = new Padding(0, 14, 8, 0);
            split.Panel2.Padding = new Padding(8, 14, 0, 0);
            split.Panel1.Controls.Add(this._SessionsCard);
            split.Panel2.Controls.Add(this._DetectionsCard);

            rootPanel.Controls.Add(split);
            rootPanel.Controls.Add(this._HeaderPanel);

            this.Controls.Add(rootPanel);
        }

        private async Task LoadDataAsync()
        {
            if (AppDatabase.Repository == null)
            {
                this._StatusLabel.Text = "Cơ sở dữ liệu chưa được khởi tạo.";
                return;
            }

            this._StatusLabel.Text = "Đang tải...";
            this.Enabled = false;
            this._SelectedSessionId = null;

            try
            {
                var sessions = await AppDatabase.Repository.GetRecentSessionsAsync(100).ConfigureAwait(true);
                this._Sessions.RaiseListChangedEvents = false;
                this._Sessions.Clear();
                foreach (var session in sessions)
                    this._Sessions.Add(session);
                this._Sessions.RaiseListChangedEvents = true;
                this._Sessions.ResetBindings();
                this._SessionSummaryLabel.Text = $"{this._Sessions.Count} records";

                if (this._Sessions.Count > 0)
                {
                    this._SessionsGrid.ClearSelection();
                    this._SessionsGrid.Rows[0].Selected = true;
                    await this.LoadDetectionsForSelectedSessionAsync().ConfigureAwait(true);
                }
                else
                {
                    this._Detections.Clear();
                    this._DetectionSummaryLabel.Text = "0 bản ghi";
                }

                this._StatusLabel.Text = "Đã tải xong.";
            }
            catch (Exception ex)
            {
                this._StatusLabel.Text = ex.Message;
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private async void SessionsGridOnSelectionChanged(object sender, EventArgs e)
        {
            await this.LoadDetectionsForSelectedSessionAsync().ConfigureAwait(true);
        }

        private async Task LoadDetectionsForSelectedSessionAsync()
        {
            if (AppDatabase.Repository == null)
                return;

            if (this._SessionsGrid.CurrentRow == null)
                return;

            if (!(this._SessionsGrid.CurrentRow.DataBoundItem is ScanSessionItem selectedSession))
                return;

            if (this._SelectedSessionId == selectedSession.Id && this._Detections.Count > 0)
                return;

            this._SelectedSessionId = selectedSession.Id;
            this._StatusLabel.Text = "Đang tải dữ liệu phát hiện...";

            var detections = await AppDatabase.Repository.GetDetectionsBySessionAsync(selectedSession.Id).ConfigureAwait(true);
            this._Detections.RaiseListChangedEvents = false;
            this._Detections.Clear();
            foreach (var detection in detections)
                this._Detections.Add(detection);
            this._Detections.RaiseListChangedEvents = true;
            this._Detections.ResetBindings();

            this._DetectionSummaryLabel.Text = $"{this._Detections.Count} records for {selectedSession.ModelName}/{selectedSession.ScanType}";
            this._StatusLabel.Text = $"Session {selectedSession.Id} loaded.";
        }
    }
}


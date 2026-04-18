using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceRecognitionDotNet;
using FaceRecognitionDotNet.WinForms.Data;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FaceRecognitionDotNet.WinForms
{
    public sealed class AttendanceView : UserControl
    {
        private readonly AppUserItem _CurrentUser;
        private readonly FaceRecognitionService _RecognitionService;
        private readonly BindingList<FaceMatchItem> _Results;
        private readonly BindingList<AttendanceItem> _History;
        private readonly BindingList<AttendanceSummaryItem> _TodaySummary;
        private readonly DataGridView _ResultsGrid;
        private readonly DataGridView _TodaySummaryGrid;
        private readonly DataGridView _HistoryGrid;
        private readonly PictureBox _CameraPreview;
        private readonly TextBox _ImagePathTextBox;
        private readonly ComboBox _ModelComboBox;
        private readonly NumericUpDown _ToleranceNumericUpDown;
        private readonly Button _BrowseButton;
        private readonly Button _RunButton;
        private readonly Button _EnrollFaceButton;
        private readonly Button _RefreshButton;
        private readonly Button _StartCameraButton;
        private readonly Button _StopCameraButton;
        private readonly CheckBox _LiveRecognitionCheckBox;
        private readonly Label _CameraStatusLabel;
        private readonly Label _StatusLabel;
        private readonly Label _FaceStateLabel;
        private readonly Panel _HeaderActionPanel;
        private readonly Timer _CameraTimer;
        private readonly WebcamCaptureService _CameraService;
        private bool _ProcessingCameraFrame;
        private bool _EnrollmentMode;

        public AttendanceView(AppUserItem currentUser)
        {
            this._CurrentUser = currentUser;
            this._RecognitionService = new FaceRecognitionService();
            this._Results = new BindingList<FaceMatchItem>();
            this._History = new BindingList<AttendanceItem>();
            this._TodaySummary = new BindingList<AttendanceSummaryItem>();
            this._ResultsGrid = new DataGridView();
            this._TodaySummaryGrid = new DataGridView();
            this._HistoryGrid = new DataGridView();
            this._CameraPreview = new PictureBox();
            this._ImagePathTextBox = new TextBox();
            this._ModelComboBox = new ComboBox();
            this._ToleranceNumericUpDown = new NumericUpDown();
            this._BrowseButton = new Button();
            this._RunButton = new Button();
            this._EnrollFaceButton = new Button();
            this._RefreshButton = new Button();
            this._StartCameraButton = new Button();
            this._StopCameraButton = new Button();
            this._LiveRecognitionCheckBox = new CheckBox();
            this._CameraStatusLabel = new Label();
            this._StatusLabel = new Label();
            this._FaceStateLabel = new Label();
            this._HeaderActionPanel = new Panel();
            this._CameraTimer = new Timer();
            this._CameraService = new WebcamCaptureService();
            this.InitializeComponent();
        }

        private async void AttendanceViewOnLoad(object sender, EventArgs e)
        {
            this.UpdateEnrollmentState();
            await this.RefreshHistoryAsync().ConfigureAwait(true);
            await this.RefreshTodaySummaryAsync().ConfigureAwait(true);
        }

        private void UpdateEnrollmentState()
        {
            if (this.HasCurrentUserFaceEnrollment())
            {
                this._EnrollmentMode = false;
                this._EnrollFaceButton.Enabled = false;
                this._EnrollFaceButton.Text = "Đã đăng ký";
                this._BrowseButton.Enabled = false;
                this._ImagePathTextBox.ReadOnly = true;
                this._ModelComboBox.Enabled = false;
                this._RunButton.Enabled = true;
                this._StartCameraButton.Enabled = true;
                this._StopCameraButton.Enabled = this._CameraService.IsStarted;
                this._LiveRecognitionCheckBox.Enabled = true;
                this._FaceStateLabel.Text = "Trạng thái khuôn mặt: Đã đăng ký";
                this._CameraStatusLabel.Text = "Khuôn mặt đã đăng ký. Bạn có thể chấm công.";
                this._StatusLabel.Text = this._CameraStatusLabel.Text;
                return;
            }

            this._RunButton.Enabled = false;
            this._StartCameraButton.Enabled = true;
            this._StopCameraButton.Enabled = this._CameraService.IsStarted;
            this._LiveRecognitionCheckBox.Enabled = false;
            this._EnrollFaceButton.Enabled = true;
            this._EnrollFaceButton.Text = "Đăng ký khuôn mặt";
            this._BrowseButton.Enabled = true;
            this._ImagePathTextBox.ReadOnly = false;
            this._ModelComboBox.Enabled = true;
            this._FaceStateLabel.Text = "Trạng thái khuôn mặt: Chưa đăng ký";
            this._CameraStatusLabel.Text = "Tài khoản này chưa đăng ký khuôn mặt. Bấm Đăng ký khuôn mặt để tự lưu 1 ảnh face.";
            this._StatusLabel.Text = this._CameraStatusLabel.Text;
        }

        private bool HasCurrentUserFaceEnrollment()
        {
            if (this._CurrentUser == null)
                return false;

            if (string.IsNullOrWhiteSpace(this._CurrentUser.FaceEncodingData) || string.IsNullOrWhiteSpace(this._CurrentUser.FaceImagePath))
                return false;

            return File.Exists(this._CurrentUser.FaceImagePath);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopCamera();
                if (this._CameraPreview.Image != null)
                {
                    this._CameraPreview.Image.Dispose();
                    this._CameraPreview.Image = null;
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var root = new Panel();
            var header = new Panel();
            var cameraCard = new Panel();
            var title = new Label();
            var subtitle = new Label();
            var cameraTitle = new Label();
            var cameraSubtitle = new Label();
            var inputCard = new Panel();
            var resultCard = new Panel();
            var historyCard = new Panel();
            var inputTitle = new Label();
            var resultTitle = new Label();
            var todaySummaryTitle = new Label();
            var historyTitle = new Label();
            var imageLabel = new Label();
            var modelLabel = new Label();
            var toleranceLabel = new Label();
            var cameraControlsPanel = new Panel();
            var enrollActionPanel = new Panel();
            var resultSplit = new SplitContainer();
            var split = new SplitContainer();

            this.Dock = DockStyle.Fill;
            this.Load += this.AttendanceViewOnLoad;

            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(18);
            root.BackColor = Color.FromArgb(245, 247, 250);

            header.Dock = DockStyle.Top;
            header.Height = 132;
            header.BackColor = Color.White;
            header.Padding = new Padding(20, 18, 20, 16);

            title.AutoSize = true;
            title.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point);
            title.Location = new System.Drawing.Point(24, 14);
            title.Text = "Chấm công";

            subtitle.AutoSize = true;
            subtitle.ForeColor = Color.FromArgb(100, 116, 139);
            subtitle.Location = new System.Drawing.Point(26, 48);
            subtitle.Text = "Tải ảnh hoặc dùng webcam để nhận diện khuôn mặt và ghi nhận vào làm hoặc tan làm theo giờ Việt Nam.";

            this._RefreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._RefreshButton.Location = new System.Drawing.Point(822, 24);
            this._RefreshButton.Size = new System.Drawing.Size(110, 30);
            this._RefreshButton.Text = "Tải lại";
            this._RefreshButton.Click += async (s, e) => await this.RefreshHistoryAsync().ConfigureAwait(true);

            this._HeaderActionPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this._HeaderActionPanel.Location = new System.Drawing.Point(430, 58);
            this._HeaderActionPanel.Size = new System.Drawing.Size(510, 40);

            this._StatusLabel.AutoSize = false;
            this._StatusLabel.Location = new System.Drawing.Point(430, 100);
            this._StatusLabel.Size = new System.Drawing.Size(500, 20);
            this._StatusLabel.Text = "Sẵn sàng.";

            this._FaceStateLabel.AutoSize = false;
            this._FaceStateLabel.Location = new System.Drawing.Point(430, 26);
            this._FaceStateLabel.Size = new System.Drawing.Size(220, 20);
            this._FaceStateLabel.ForeColor = Color.FromArgb(71, 85, 105);
            this._FaceStateLabel.Text = "Trạng thái khuôn mặt: Chưa đăng ký";

            this._EnrollFaceButton.AutoSize = true;
            this._EnrollFaceButton.Text = "Đăng ký khuôn mặt";
            this._EnrollFaceButton.Click += this.EnrollFaceButtonOnClick;
            this._EnrollFaceButton.Location = new System.Drawing.Point(0, 4);
            this._EnrollFaceButton.Size = new System.Drawing.Size(144, 30);

            this._HeaderActionPanel.Controls.Add(this._EnrollFaceButton);

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(this._RefreshButton);
            header.Controls.Add(this._HeaderActionPanel);
            header.Controls.Add(this._FaceStateLabel);
            header.Controls.Add(this._StatusLabel);

            cameraCard.Dock = DockStyle.Top;
            cameraCard.Height = 210;
            cameraCard.BackColor = Color.White;
            cameraCard.Padding = new Padding(18);
            cameraCard.BorderStyle = BorderStyle.FixedSingle;

            cameraTitle.AutoSize = true;
            cameraTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            cameraTitle.Location = new System.Drawing.Point(18, 14);
            cameraTitle.Text = "Camera trực tiếp";

            cameraSubtitle.AutoSize = true;
            cameraSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            cameraSubtitle.Location = new System.Drawing.Point(18, 38);
            cameraSubtitle.Text = "Khởi động webcam để xem trước và tự động nhận diện khuôn mặt theo thời gian thực.";

            this._CameraPreview.BorderStyle = BorderStyle.FixedSingle;
            this._CameraPreview.Location = new System.Drawing.Point(18, 64);
            this._CameraPreview.Size = new System.Drawing.Size(300, 120);
            this._CameraPreview.SizeMode = PictureBoxSizeMode.Zoom;

            cameraControlsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cameraControlsPanel.Location = new System.Drawing.Point(336, 64);
            cameraControlsPanel.Size = new System.Drawing.Size(550, 120);

            this._StartCameraButton.Location = new System.Drawing.Point(0, 0);
            this._StartCameraButton.Size = new System.Drawing.Size(110, 30);
            this._StartCameraButton.Text = "Bật camera";
            this._StartCameraButton.Click += this.StartCameraButtonOnClick;

            this._StopCameraButton.Location = new System.Drawing.Point(120, 0);
            this._StopCameraButton.Size = new System.Drawing.Size(110, 30);
            this._StopCameraButton.Text = "Tắt camera";
            this._StopCameraButton.Click += this.StopCameraButtonOnClick;

            this._LiveRecognitionCheckBox.AutoSize = true;
            this._LiveRecognitionCheckBox.Location = new System.Drawing.Point(0, 46);
            this._LiveRecognitionCheckBox.Text = "Tự nhận diện và lưu chấm công";
            this._LiveRecognitionCheckBox.Checked = true;

            this._CameraStatusLabel.AutoSize = false;
            this._CameraStatusLabel.Location = new System.Drawing.Point(0, 76);
            this._CameraStatusLabel.Size = new System.Drawing.Size(520, 36);
            this._CameraStatusLabel.Text = "Camera đang dừng.";

            cameraControlsPanel.Controls.Add(this._StartCameraButton);
            cameraControlsPanel.Controls.Add(this._StopCameraButton);
            cameraControlsPanel.Controls.Add(this._LiveRecognitionCheckBox);
            cameraControlsPanel.Controls.Add(this._CameraStatusLabel);

            cameraCard.Controls.Add(cameraTitle);
            cameraCard.Controls.Add(cameraSubtitle);
            cameraCard.Controls.Add(this._CameraPreview);
            cameraCard.Controls.Add(cameraControlsPanel);

            inputCard.Dock = DockStyle.Top;
            inputCard.Height = 156;
            inputCard.BackColor = Color.White;
            inputCard.Padding = new Padding(18);
            inputCard.BorderStyle = BorderStyle.FixedSingle;

            inputTitle.AutoSize = true;
            inputTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            inputTitle.Location = new System.Drawing.Point(18, 14);
            inputTitle.Text = "Dữ liệu nhận diện";

            imageLabel.AutoSize = true;
            imageLabel.Location = new System.Drawing.Point(20, 48);
            imageLabel.Text = "Ảnh";

            this._ImagePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this._ImagePathTextBox.Location = new System.Drawing.Point(92, 44);
            this._ImagePathTextBox.Width = 580;

            this._BrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._BrowseButton.Location = new System.Drawing.Point(690, 42);
            this._BrowseButton.Size = new System.Drawing.Size(96, 28);
            this._BrowseButton.Text = "Chọn ảnh";
            this._BrowseButton.Click += this.BrowseButtonOnClick;

            modelLabel.AutoSize = true;
            modelLabel.Location = new System.Drawing.Point(20, 84);
            modelLabel.Text = "Mô hình";

            this._ModelComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._ModelComboBox.Items.AddRange(new object[] { Model.Hog.ToString(), Model.Cnn.ToString() });
            this._ModelComboBox.Location = new System.Drawing.Point(92, 80);
            this._ModelComboBox.Width = 120;

            toleranceLabel.AutoSize = true;
            toleranceLabel.Location = new System.Drawing.Point(240, 84);
            toleranceLabel.Text = "Ngưỡng";

            this._ToleranceNumericUpDown.DecimalPlaces = 2;
            this._ToleranceNumericUpDown.Increment = 0.01M;
            this._ToleranceNumericUpDown.Minimum = 0.1M;
            this._ToleranceNumericUpDown.Maximum = 1.5M;
            this._ToleranceNumericUpDown.Value = 0.6M;
            this._ToleranceNumericUpDown.Location = new System.Drawing.Point(312, 80);
            this._ToleranceNumericUpDown.Width = 80;

            this._RunButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._RunButton.Location = new System.Drawing.Point(690, 78);
            this._RunButton.Size = new System.Drawing.Size(96, 28);
            this._RunButton.Text = "Ghi nhận";
            this._RunButton.Click += this.RunButtonOnClick;

            inputCard.Controls.Add(inputTitle);
            inputCard.Controls.Add(imageLabel);
            inputCard.Controls.Add(this._ImagePathTextBox);
            inputCard.Controls.Add(this._BrowseButton);
            inputCard.Controls.Add(modelLabel);
            inputCard.Controls.Add(this._ModelComboBox);
            inputCard.Controls.Add(toleranceLabel);
            inputCard.Controls.Add(this._ToleranceNumericUpDown);
            inputCard.Controls.Add(this._RunButton);

            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Horizontal;
            split.SplitterDistance = 380;
            split.Panel1.Padding = new Padding(0, 14, 0, 8);
            split.Panel2.Padding = new Padding(0, 8, 0, 0);

            resultCard.Dock = DockStyle.Left;
            resultCard.Width = 500;
            resultCard.BackColor = Color.White;
            resultCard.Padding = new Padding(18);
            resultCard.BorderStyle = BorderStyle.FixedSingle;

            resultTitle.AutoSize = true;
            resultTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            resultTitle.Location = new System.Drawing.Point(18, 14);
            resultTitle.Text = "Kết quả nhận diện";

            this._ResultsGrid.Dock = DockStyle.Fill;
            this._ResultsGrid.Location = new System.Drawing.Point(18, 44);
            this._ResultsGrid.ReadOnly = true;
            this._ResultsGrid.AllowUserToAddRows = false;
            this._ResultsGrid.AllowUserToDeleteRows = false;
            this._ResultsGrid.AutoGenerateColumns = false;
            this._ResultsGrid.RowHeadersVisible = false;
            this._ResultsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._ResultsGrid.DataSource = this._Results;

            this._ResultsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FaceIndex", HeaderText = "Khuôn mặt", FillWeight = 40 });
            this._ResultsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Tên đăng nhập", FillWeight = 80 });
            this._ResultsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ và tên", FillWeight = 100 });
            this._ResultsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Distance", HeaderText = "Khoảng cách", FillWeight = 60 });
            this._ResultsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", FillWeight = 60 });
            this._ResultsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Box", HeaderText = "Khung", FillWeight = 110 });

            resultSplit.Dock = DockStyle.Fill;
            resultSplit.Orientation = Orientation.Horizontal;
            resultSplit.SplitterDistance = 220;

            var resultsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18), BackColor = Color.White };
            resultsPanel.BorderStyle = BorderStyle.FixedSingle;
            resultTitle.Location = new System.Drawing.Point(18, 14);
            resultsPanel.Controls.Add(this._ResultsGrid);
            resultsPanel.Controls.Add(resultTitle);

            todaySummaryTitle.AutoSize = true;
            todaySummaryTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            todaySummaryTitle.Location = new System.Drawing.Point(18, 14);
            todaySummaryTitle.Text = "Tổng hợp hôm nay";

            this._TodaySummaryGrid.Dock = DockStyle.Fill;
            this._TodaySummaryGrid.Location = new System.Drawing.Point(18, 44);
            this._TodaySummaryGrid.ReadOnly = true;
            this._TodaySummaryGrid.AllowUserToAddRows = false;
            this._TodaySummaryGrid.AllowUserToDeleteRows = false;
            this._TodaySummaryGrid.AutoGenerateColumns = false;
            this._TodaySummaryGrid.RowHeadersVisible = false;
            this._TodaySummaryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._TodaySummaryGrid.DataSource = this._TodaySummary;
            this._TodaySummaryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Tên đăng nhập", FillWeight = 70 });
            this._TodaySummaryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ và tên", FillWeight = 100 });
            this._TodaySummaryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckInAt", HeaderText = "Vào làm", FillWeight = 70 });
            this._TodaySummaryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CheckOutAt", HeaderText = "Tan làm", FillWeight = 70 });
            this._TodaySummaryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "WorkState", HeaderText = "Trạng thái", FillWeight = 60 });
            this._TodaySummaryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RecordCount", HeaderText = "Bản ghi", FillWeight = 45 });

            var summaryPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18), BackColor = Color.White };
            summaryPanel.BorderStyle = BorderStyle.FixedSingle;
            summaryPanel.Controls.Add(this._TodaySummaryGrid);
            summaryPanel.Controls.Add(todaySummaryTitle);

            resultSplit.Panel1.Controls.Add(resultsPanel);
            resultSplit.Panel2.Controls.Add(summaryPanel);

            resultCard.Controls.Add(resultSplit);

            historyCard.Dock = DockStyle.Fill;
            historyCard.BackColor = Color.White;
            historyCard.Padding = new Padding(18);
            historyCard.BorderStyle = BorderStyle.FixedSingle;

            historyTitle.AutoSize = true;
            historyTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            historyTitle.Location = new System.Drawing.Point(18, 14);
            historyTitle.Text = "Lịch sử chấm công";

            this._HistoryGrid.Dock = DockStyle.Fill;
            this._HistoryGrid.Location = new System.Drawing.Point(18, 44);
            this._HistoryGrid.ReadOnly = true;
            this._HistoryGrid.AllowUserToAddRows = false;
            this._HistoryGrid.AllowUserToDeleteRows = false;
            this._HistoryGrid.AutoGenerateColumns = false;
            this._HistoryGrid.RowHeadersVisible = false;
            this._HistoryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._HistoryGrid.DataSource = this._History;

            this._HistoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AttendedAt", HeaderText = "Lúc", FillWeight = 80 });
            this._HistoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Tên đăng nhập", FillWeight = 70 });
            this._HistoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ và tên", FillWeight = 90 });
            this._HistoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", FillWeight = 55 });
            this._HistoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MatchDistance", HeaderText = "Khoảng cách", FillWeight = 55 });
            this._HistoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ModelName", HeaderText = "Mô hình", FillWeight = 60 });

            historyCard.Controls.Add(this._HistoryGrid);
            historyCard.Controls.Add(historyTitle);

            split.Panel1.Controls.Add(resultCard);
            split.Panel2.Controls.Add(historyCard);

            root.Controls.Add(split);
            root.Controls.Add(cameraCard);
            root.Controls.Add(inputCard);
            root.Controls.Add(header);
            this.Controls.Add(root);

            this._ModelComboBox.SelectedIndex = 0;
            this._CameraTimer.Interval = 1000;
            this._CameraTimer.Tick += this.CameraTimerOnTick;
        }

        private void BrowseButtonOnClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Images|*.jpg;*.jpeg;*.png|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    this._ImagePathTextBox.Text = dialog.FileName;
            }
        }

        private void StartCameraButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                this._CameraService.Start();
                this._CameraTimer.Start();
                this._CameraStatusLabel.Text = "Webcam đang chạy.";
                this._StopCameraButton.Enabled = true;
            }
            catch (Exception ex)
            {
                this._CameraStatusLabel.Text = ex.Message;
            }
        }

        private void StopCameraButtonOnClick(object sender, EventArgs e)
        {
            this.StopCamera();
            this._CameraStatusLabel.Text = "Camera đã dừng.";
            this._StopCameraButton.Enabled = false;
        }

        private void StopCamera()
        {
            this._CameraTimer.Stop();
            this._CameraService.Stop();
        }

        private async void CameraTimerOnTick(object sender, EventArgs e)
        {
            if (this._ProcessingCameraFrame || !this._CameraService.IsStarted)
                return;

            this._ProcessingCameraFrame = true;
            try
            {
                using (var frame = await Task.Run(() => this._CameraService.CaptureFrame()).ConfigureAwait(true))
                {
                    if (frame == null)
                    {
                        this._CameraStatusLabel.Text = "Đang chờ khung hình từ webcam...";
                        return;
                    }

                    this.UpdateCameraPreview(frame);

                    if (this._EnrollmentMode)
                    {
                        await this.TryEnrollFromCameraFrameAsync(frame).ConfigureAwait(true);
                        return;
                    }

                    if (!this._LiveRecognitionCheckBox.Checked)
                    {
                        this._CameraStatusLabel.Text = "Chỉ hiển thị xem trước webcam.";
                        return;
                    }

                    var attendanceAt = AttendanceSchedule.GetVietnamNow();
                    var attendanceStatus = AttendanceSchedule.GetAttendanceStatus(attendanceAt);
                    var attendanceLabel = AttendanceSchedule.GetAttendanceLabel(attendanceStatus);

                    var modelsDirectory = Path.GetFullPath("models");
                    if (!Directory.Exists(modelsDirectory))
                    {
                        this._CameraStatusLabel.Text = $"Không tìm thấy thư mục model '{modelsDirectory}'.";
                        return;
                    }

                    var selectedModel = this._ModelComboBox.SelectedItem?.ToString() ?? Model.Hog.ToString();
                    if (!Enum.TryParse<Model>(selectedModel, true, out var model))
                        model = Model.Hog;

                    var storedImagePath = ImageStorage.StoreBitmap(frame, "live");
                    var users = await AppDatabase.Repository.GetUsersAsync().ConfigureAwait(true);
                    var matches = await Task.Run(() => this._RecognitionService.RecognizeFaces(storedImagePath, modelsDirectory, model, users, (double)this._ToleranceNumericUpDown.Value)).ConfigureAwait(true);

                    var savedAny = false;
                    foreach (var match in matches)
                    {
                        if (!match.UserId.HasValue)
                            continue;

                        if (await AppDatabase.Repository.HasAttendanceAsync(match.UserId.Value, attendanceStatus, attendanceAt).ConfigureAwait(true))
                            continue;

                        savedAny = await AppDatabase.Repository.SaveAttendanceAsync(
                            match.UserId,
                            storedImagePath,
                            model.ToString(),
                            match.Distance,
                            attendanceStatus,
                            attendanceAt).ConfigureAwait(true) || savedAny;
                    }

                    if (matches.Count > 0)
                    {
                        this._CameraStatusLabel.Text = $"{attendanceLabel}: đã phát hiện {matches.Count} khuôn mặt.";
                        this._StatusLabel.Text = this._CameraStatusLabel.Text;
                    }
                    else
                    {
                        this._CameraStatusLabel.Text = "Không phát hiện khuôn mặt trong khung hình trực tiếp.";
                    }

                    if (!savedAny)
                    {
                        try
                        {
                            if (File.Exists(storedImagePath))
                                File.Delete(storedImagePath);
                        }
                        catch
                        {
                        }
                    }

                    if (savedAny)
                    {
                        await this.RefreshHistoryAsync().ConfigureAwait(true);
                        await this.RefreshTodaySummaryAsync().ConfigureAwait(true);
                    }
                }
            }
            catch (Exception ex)
            {
                this._CameraStatusLabel.Text = ex.Message;
            }
            finally
            {
                this._ProcessingCameraFrame = false;
            }
        }

        private void UpdateCameraPreview(Bitmap bitmap)
        {
            if (bitmap == null)
                return;

            this._CameraPreview.Image?.Dispose();
            this._CameraPreview.Image = new Bitmap(bitmap);
        }

        private async void RunButtonOnClick(object sender, EventArgs e)
        {
            await this.RunAttendanceAsync().ConfigureAwait(true);
        }

        private async void EnrollFaceButtonOnClick(object sender, EventArgs e)
        {
            await this.BeginCameraEnrollmentAsync().ConfigureAwait(true);
        }

        private async Task BeginCameraEnrollmentAsync()
        {
            if (this._CurrentUser == null)
            {
                this._StatusLabel.Text = "Không xác định được tài khoản hiện tại.";
                return;
            }

            if (await AppDatabase.Repository.HasUserFaceEnrollmentAsync(this._CurrentUser.Id).ConfigureAwait(true))
            {
                this._StatusLabel.Text = "Tài khoản này đã có 1 ảnh khuôn mặt. Không thể đăng ký lại.";
                this.UpdateEnrollmentState();
                return;
            }

            try
            {
                this.UseWaitCursor = true;

                this._EnrollmentMode = true;
                this._EnrollFaceButton.Enabled = false;
                this._EnrollFaceButton.Text = "Đang mở webcam...";
                this._BrowseButton.Enabled = false;
                this._ImagePathTextBox.ReadOnly = true;
                this._ModelComboBox.Enabled = false;
                this._RunButton.Enabled = false;
                this._LiveRecognitionCheckBox.Checked = false;
                this._LiveRecognitionCheckBox.Enabled = false;

                if (!this._CameraService.IsStarted)
                    this.StartCameraButtonOnClick(this, EventArgs.Empty);

                this._CameraStatusLabel.Text = "Webcam đang mở để đăng ký khuôn mặt. Đưa mặt vào khung hình, ứng dụng sẽ tự lưu khi nhận đúng 1 khuôn mặt.";
                this._StatusLabel.Text = this._CameraStatusLabel.Text;
                this._FaceStateLabel.Text = "Trạng thái khuôn mặt: Đang chờ đăng ký";
            }
            catch (Exception ex)
            {
                this._EnrollmentMode = false;
                this._StatusLabel.Text = ex.Message;
            }
            finally
            {
                this.UseWaitCursor = false;
            }
        }

        private async Task TryEnrollFromCameraFrameAsync(Bitmap frame)
        {
            if (!this._EnrollmentMode || this._CurrentUser == null)
                return;

            var modelsDirectory = Path.GetFullPath("models");
            if (!Directory.Exists(modelsDirectory))
            {
                this._CameraStatusLabel.Text = $"Không tìm thấy thư mục model '{modelsDirectory}'.";
                return;
            }

            if (!Enum.TryParse<Model>(this._ModelComboBox.SelectedItem?.ToString() ?? Model.Hog.ToString(), true, out var model))
                model = Model.Hog;

            var tempImagePath = ImageStorage.StoreBitmap(frame, "face_enroll");
            try
            {
                var encodingData = await Task.Run(() => this._RecognitionService.BuildEncodingData(tempImagePath, modelsDirectory, model)).ConfigureAwait(true);
                if (string.IsNullOrWhiteSpace(encodingData))
                {
                    this._CameraStatusLabel.Text = "Đưa khuôn mặt vào giữa khung hình. Ứng dụng cần đúng 1 khuôn mặt để đăng ký.";
                    this._StatusLabel.Text = this._CameraStatusLabel.Text;
                    return;
                }

                var storedFaceImagePath = ImageStorage.StoreFaceImage(tempImagePath, this._CurrentUser.Id);
                var updated = await AppDatabase.Repository.TryUpdateUserFaceEncodingAsync(this._CurrentUser.Id, encodingData, storedFaceImagePath).ConfigureAwait(true);
                if (!updated)
                {
                    this._CameraStatusLabel.Text = "Tài khoản này đã có 1 ảnh khuôn mặt. Không thể ghi đè.";
                    this._StatusLabel.Text = this._CameraStatusLabel.Text;
                    this._EnrollmentMode = false;
                    this.UpdateEnrollmentState();
                    return;
                }

                this._CurrentUser.FaceEncodingData = encodingData;
                this._CurrentUser.FaceImagePath = storedFaceImagePath;
                this._EnrollmentMode = false;
                this._CameraStatusLabel.Text = "Đã đăng ký khuôn mặt thành công.";
                this._StatusLabel.Text = this._CameraStatusLabel.Text;
                this._FaceStateLabel.Text = "Trạng thái khuôn mặt: Đã đăng ký";
                this.StopCamera();
                this.UpdateEnrollmentState();
                await this.RefreshTodaySummaryAsync().ConfigureAwait(true);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempImagePath))
                        File.Delete(tempImagePath);
                }
                catch
                {
                }
            }
        }

        private async Task RunAttendanceAsync()
        {
            var imagePath = this._ImagePathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                this._StatusLabel.Text = "Vui lòng chọn ảnh hợp lệ trước.";
                return;
            }

            if (!Enum.TryParse<Model>(this._ModelComboBox.SelectedItem?.ToString() ?? Model.Hog.ToString(), true, out var model))
            {
                this._StatusLabel.Text = "Mô hình không hợp lệ.";
                return;
            }

            var modelsDirectory = Path.GetFullPath("models");
            if (!Directory.Exists(modelsDirectory))
            {
                this._StatusLabel.Text = $"Không tìm thấy thư mục model '{modelsDirectory}'.";
                return;
            }

            string storedImagePath;
            try
            {
                storedImagePath = ImageStorage.StoreLocalCopy(imagePath);
            }
            catch (Exception ex)
            {
                this._StatusLabel.Text = ex.Message;
                return;
            }

            try
            {
                this.UseWaitCursor = true;
                this._RunButton.Enabled = false;
                this._Results.Clear();

                var attendanceAt = AttendanceSchedule.GetVietnamNow();
                var attendanceStatus = AttendanceSchedule.GetAttendanceStatus(attendanceAt);
                var attendanceLabel = AttendanceSchedule.GetAttendanceLabel(attendanceStatus);
                this._StatusLabel.Text = AttendanceSchedule.GetScheduleSummary(attendanceAt);

                var users = await AppDatabase.Repository.GetUsersAsync().ConfigureAwait(true);
                var matches = await Task.Run(() => this._RecognitionService.RecognizeFaces(storedImagePath, modelsDirectory, model, users, (double)this._ToleranceNumericUpDown.Value)).ConfigureAwait(true);

                foreach (var match in matches)
                {
                    this._Results.Add(match);

                    if (!match.UserId.HasValue)
                        continue;

                    if (await AppDatabase.Repository.HasAttendanceAsync(match.UserId.Value, attendanceStatus, attendanceAt).ConfigureAwait(true))
                        continue;

                    await AppDatabase.Repository.SaveAttendanceAsync(
                        match.UserId,
                        storedImagePath,
                        model.ToString(),
                        match.Distance,
                        attendanceStatus,
                        attendanceAt).ConfigureAwait(true);
                }

                if (matches.Count == 0)
                {
                    await AppDatabase.Repository.SaveAttendanceAsync(
                        null,
                        storedImagePath,
                        model.ToString(),
                        null,
                        "Không xác định",
                        attendanceAt).ConfigureAwait(true);
                    this._StatusLabel.Text = "Không nhận diện được khuôn mặt nào.";
                }
                else
                {
                    this._StatusLabel.Text = $"{attendanceLabel}: đã nhận diện {matches.Count} khuôn mặt theo giờ VN.";
                }

                await this.RefreshHistoryAsync().ConfigureAwait(true);
                await this.RefreshTodaySummaryAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                this._StatusLabel.Text = ex.Message;
            }
            finally
            {
                this.UseWaitCursor = false;
                this._RunButton.Enabled = true;
            }
        }

        private async Task RefreshHistoryAsync()
        {
            if (AppDatabase.Repository == null)
                return;

            var history = await AppDatabase.Repository.GetRecentAttendanceAsync(50).ConfigureAwait(true);
            this._History.RaiseListChangedEvents = false;
            this._History.Clear();
            foreach (var item in history)
                this._History.Add(item);
            this._History.RaiseListChangedEvents = true;
            this._History.ResetBindings();
        }

        private async Task RefreshTodaySummaryAsync()
        {
            if (AppDatabase.Repository == null)
                return;

            var today = AttendanceSchedule.GetVietnamNow().Date;
            var summaries = await AppDatabase.Repository.GetAttendanceSummaryByDayAsync(today).ConfigureAwait(true);
            this._TodaySummary.RaiseListChangedEvents = false;
            this._TodaySummary.Clear();
            foreach (var item in summaries)
                this._TodaySummary.Add(item);
            this._TodaySummary.RaiseListChangedEvents = true;
            this._TodaySummary.ResetBindings();
        }
    }
}


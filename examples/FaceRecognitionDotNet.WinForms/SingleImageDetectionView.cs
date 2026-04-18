using System;
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
    public sealed class SingleImageDetectionView : UserControl
    {
        private readonly FaceDetectionEngine _Engine;
        private readonly Panel _HeaderPanel;
        private readonly Panel _PreviewCard;
        private readonly Panel _OutputCard;
        private readonly Panel _HeaderBadge;
        private readonly TextBox _ImageTextBox;
        private readonly Button _BrowseButton;
        private readonly ComboBox _ModelComboBox;
        private readonly Button _RunButton;
        private readonly PictureBox _PreviewPictureBox;
        private readonly TextBox _OutputTextBox;
        private readonly Label _StatusLabel;

        public SingleImageDetectionView()
        {
            this._Engine = new FaceDetectionEngine();
            this._HeaderPanel = new Panel();
            this._PreviewCard = new Panel();
            this._OutputCard = new Panel();
            this._HeaderBadge = new Panel();
            this._ImageTextBox = new TextBox();
            this._BrowseButton = new Button();
            this._ModelComboBox = new ComboBox();
            this._RunButton = new Button();
            this._PreviewPictureBox = new PictureBox();
            this._OutputTextBox = new TextBox();
            this._StatusLabel = new Label();
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            var rootPanel = new Panel();
            var accentBar = new Panel();
            var headerTitle = new Label();
            var headerSubtitle = new Label();
            var settingsTitle = new Label();
            var previewTitle = new Label();
            var outputTitle = new Label();
            var imageLabel = new Label();
            var modelLabel = new Label();

            this.Dock = DockStyle.Fill;

            rootPanel.Dock = DockStyle.Fill;
            rootPanel.Padding = new Padding(18);
            rootPanel.BackColor = Color.FromArgb(245, 247, 250);

            this._HeaderPanel.Dock = DockStyle.Top;
            this._HeaderPanel.Height = 96;
            this._HeaderPanel.BackColor = Color.White;
            this._HeaderPanel.Padding = new Padding(20, 18, 20, 16);

            this._HeaderBadge.BackColor = Color.FromArgb(37, 99, 235);
            this._HeaderBadge.Location = new System.Drawing.Point(20, 18);
            this._HeaderBadge.Size = new System.Drawing.Size(38, 38);
            this._HeaderBadge.Paint += this.HeaderBadgeOnPaint;

            accentBar.Dock = DockStyle.Left;
            accentBar.Width = 6;
            accentBar.BackColor = Color.FromArgb(37, 99, 235);

            headerTitle.AutoSize = true;
            headerTitle.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point);
            headerTitle.Location = new System.Drawing.Point(70, 14);
            headerTitle.Text = "Ảnh đơn";

            headerSubtitle.AutoSize = true;
            headerSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            headerSubtitle.Location = new System.Drawing.Point(72, 50);
            headerSubtitle.Text = "Chọn một ảnh, xem trước và kiểm tra kết quả phát hiện khuôn mặt ở bên phải.";

            this._HeaderPanel.Controls.Add(headerTitle);
            this._HeaderPanel.Controls.Add(headerSubtitle);
            this._HeaderPanel.Controls.Add(this._HeaderBadge);
            this._HeaderPanel.Controls.Add(accentBar);

            settingsTitle.AutoSize = true;
            settingsTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            settingsTitle.Location = new System.Drawing.Point(18, 14);
            settingsTitle.Text = "Đầu vào";

            previewTitle.AutoSize = true;
            previewTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            previewTitle.Location = new System.Drawing.Point(18, 14);
            previewTitle.Text = "Xem trước";

            outputTitle.AutoSize = true;
            outputTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            outputTitle.Location = new System.Drawing.Point(18, 14);
            outputTitle.Text = "Kết quả";

            this._PreviewCard.Dock = DockStyle.Fill;
            this._PreviewCard.BackColor = Color.White;
            this._PreviewCard.Padding = new Padding(18);
            this._PreviewCard.BorderStyle = BorderStyle.FixedSingle;

            this._OutputCard.Dock = DockStyle.Fill;
            this._OutputCard.BackColor = Color.White;
            this._OutputCard.Padding = new Padding(18);
            this._OutputCard.BorderStyle = BorderStyle.FixedSingle;

            imageLabel.AutoSize = true;
            imageLabel.Location = new System.Drawing.Point(20, 48);
            imageLabel.Text = "Ảnh";

            this._ImageTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this._ImageTextBox.Location = new System.Drawing.Point(112, 44);
            this._ImageTextBox.Size = new System.Drawing.Size(628, 23);

            this._BrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._BrowseButton.Location = new System.Drawing.Point(758, 42);
            this._BrowseButton.Size = new System.Drawing.Size(110, 28);
            this._BrowseButton.Text = "Chọn ảnh";
            this._BrowseButton.Click += this.BrowseButtonOnClick;

            modelLabel.AutoSize = true;
            modelLabel.Location = new System.Drawing.Point(20, 84);
            modelLabel.Text = "Model";

            this._ModelComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._ModelComboBox.Items.AddRange(new object[] { Model.Hog.ToString(), Model.Cnn.ToString() });
            this._ModelComboBox.Location = new System.Drawing.Point(112, 80);
            this._ModelComboBox.Size = new System.Drawing.Size(150, 23);

            this._RunButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._RunButton.Location = new System.Drawing.Point(758, 78);
            this._RunButton.Size = new System.Drawing.Size(110, 28);
            this._RunButton.Text = "Phát hiện";
            this._RunButton.Click += this.RunButtonOnClick;

            this._StatusLabel.AutoSize = false;
            this._StatusLabel.Location = new System.Drawing.Point(286, 80);
            this._StatusLabel.Size = new System.Drawing.Size(470, 26);
            this._StatusLabel.Text = "Chọn một ảnh, chạy phát hiện và xem danh sách kết quả.";

            this._PreviewPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this._PreviewPictureBox.Location = new System.Drawing.Point(18, 44);
            this._PreviewPictureBox.Size = new System.Drawing.Size(506, 424);
            this._PreviewPictureBox.BorderStyle = BorderStyle.FixedSingle;
            this._PreviewPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            this._PreviewPictureBox.BackColor = Color.White;

            this._OutputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this._OutputTextBox.Location = new System.Drawing.Point(18, 44);
            this._OutputTextBox.Size = new System.Drawing.Size(334, 424);
            this._OutputTextBox.Multiline = true;
            this._OutputTextBox.ReadOnly = true;
            this._OutputTextBox.ScrollBars = ScrollBars.Both;
            this._OutputTextBox.WordWrap = false;
            this._OutputTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this._OutputTextBox.BackColor = Color.FromArgb(248, 250, 252);

            this._PreviewCard.Controls.Add(previewTitle);
            this._PreviewCard.Controls.Add(this._PreviewPictureBox);

            this._OutputCard.Controls.Add(outputTitle);
            this._OutputCard.Controls.Add(this._OutputTextBox);

            this._HeaderPanel.Controls.Add(settingsTitle);
            this._HeaderPanel.Controls.Add(imageLabel);
            this._HeaderPanel.Controls.Add(this._ImageTextBox);
            this._HeaderPanel.Controls.Add(this._BrowseButton);
            this._HeaderPanel.Controls.Add(modelLabel);
            this._HeaderPanel.Controls.Add(this._ModelComboBox);
            this._HeaderPanel.Controls.Add(this._RunButton);
            this._HeaderPanel.Controls.Add(this._StatusLabel);

            var contentSplit = new SplitContainer();
            contentSplit.Dock = DockStyle.Fill;
            contentSplit.Orientation = Orientation.Vertical;
            contentSplit.SplitterDistance = 536;
            contentSplit.Panel1.Padding = new Padding(0, 14, 8, 0);
            contentSplit.Panel2.Padding = new Padding(8, 14, 0, 0);
            contentSplit.Panel1.BackColor = Color.Transparent;
            contentSplit.Panel2.BackColor = Color.Transparent;
            contentSplit.IsSplitterFixed = false;

            contentSplit.Panel1.Controls.Add(this._PreviewCard);
            contentSplit.Panel2.Controls.Add(this._OutputCard);

            rootPanel.Controls.Add(contentSplit);
            rootPanel.Controls.Add(this._HeaderPanel);

            this.Controls.Add(rootPanel);

            this._ModelComboBox.SelectedIndex = 0;
            this._ImageTextBox.Text = string.Empty;
        }

        private async void RunButtonOnClick(object sender, EventArgs e)
        {
            var imagePath = this._ImageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                MessageBox.Show(this, "Vui lòng chọn một tệp ảnh hợp lệ.", "Ảnh đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Enum.TryParse<Model>(this._ModelComboBox.SelectedItem?.ToString() ?? Model.Hog.ToString(), true, out var model))
            {
                MessageBox.Show(this, "Vui lòng chọn model hợp lệ.", "Ảnh đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var modelsDirectory = Path.GetFullPath("models");
            if (!Directory.Exists(modelsDirectory))
            {
                MessageBox.Show(this, $"Please check whether model directory '{modelsDirectory}' exists.", "Single Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string storedImagePath;
            try
            {
                storedImagePath = ImageStorage.StoreLocalCopy(imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Single Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.SetBusyState(true, "Running single image detection...");
            this._OutputTextBox.Clear();

            try
            {
                using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(storedImagePath))
                {
                    this._PreviewPictureBox.Image?.Dispose();
                    this._PreviewPictureBox.Image = new Bitmap(bitmap);
                }

                var lines = await Task.Run(() => this._Engine.AnalyzeImage(storedImagePath, modelsDirectory, model));
                this._OutputTextBox.Lines = lines.ToArray();

                if (AppDatabase.Repository != null)
                {
                    await AppDatabase.Repository.SaveScanResultsAsync(
                        "single",
                        storedImagePath,
                        model.ToString(),
                        null,
                        lines);
                }

                this._StatusLabel.Text = lines.Count == 0 ? "Completed. No face detected." : $"Completed. {lines.Count} result line(s).";
            }
            catch (Exception ex)
            {
                this._StatusLabel.Text = "Thất bại.";
                MessageBox.Show(this, ex.Message, "Single Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.SetBusyState(false, this._StatusLabel.Text);
            }
        }

        private void BrowseButtonOnClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Images|*.jpg;*.jpeg;*.png|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    this._ImageTextBox.Text = dialog.FileName;
            }
        }

        private void SetBusyState(bool busy, string statusText)
        {
            this._BrowseButton.Enabled = !busy;
            this._RunButton.Enabled = !busy;
            this._ImageTextBox.Enabled = !busy;
            this._ModelComboBox.Enabled = !busy;
            this._HeaderPanel.Enabled = true;
            this.UseWaitCursor = busy;
            this._StatusLabel.Text = statusText;
        }

        private void HeaderBadgeOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var bg = new SolidBrush(Color.FromArgb(37, 99, 235)))
                e.Graphics.FillRectangle(bg, 0, 0, 38, 38);

            using (var pen = new Pen(Color.White, 1.8F))
            {
                e.Graphics.DrawRectangle(pen, 9, 10, 20, 16);
                e.Graphics.DrawLine(pen, 11, 22, 16, 16);
                e.Graphics.DrawLine(pen, 16, 16, 21, 20);
                using (var fill = new SolidBrush(Color.White))
                    e.Graphics.FillEllipse(fill, 22, 12, 3, 3);
            }
        }
    }
}

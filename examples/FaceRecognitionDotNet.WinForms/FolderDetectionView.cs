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
    public sealed class FolderDetectionView : UserControl
    {
        private readonly FaceDetectionEngine _Engine;
        private readonly Panel _HeaderPanel;
        private readonly Panel _InputCard;
        private readonly Panel _OutputCard;
        private readonly Panel _HeaderBadge;
        private readonly TextBox _DirectoryTextBox;
        private readonly Button _BrowseButton;
        private readonly ComboBox _ModelComboBox;
        private readonly NumericUpDown _CpusNumericUpDown;
        private readonly Button _RunButton;
        private readonly TextBox _OutputTextBox;
        private readonly Label _StatusLabel;

        public FolderDetectionView()
        {
            this._Engine = new FaceDetectionEngine();
            this._HeaderPanel = new Panel();
            this._InputCard = new Panel();
            this._OutputCard = new Panel();
            this._HeaderBadge = new Panel();
            this._DirectoryTextBox = new TextBox();
            this._BrowseButton = new Button();
            this._ModelComboBox = new ComboBox();
            this._CpusNumericUpDown = new NumericUpDown();
            this._RunButton = new Button();
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
            var inputTitle = new Label();
            var outputTitle = new Label();
            var settingsBadge = new Label();
            var directoryLabel = new Label();
            var modelLabel = new Label();
            var cpusLabel = new Label();

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
            headerTitle.Text = "Quét thư mục";

            headerSubtitle.AutoSize = true;
            headerSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            headerSubtitle.Location = new System.Drawing.Point(72, 50);
            headerSubtitle.Text = "Chọn một thư mục và quét toàn bộ ảnh JPG, JPEG hoặc PNG bằng model đã chọn.";

            this._HeaderPanel.Controls.Add(headerTitle);
            this._HeaderPanel.Controls.Add(headerSubtitle);
            this._HeaderPanel.Controls.Add(this._HeaderBadge);
            this._HeaderPanel.Controls.Add(accentBar);

            this._OutputCard.Dock = DockStyle.Fill;
            this._OutputCard.BackColor = Color.White;
            this._OutputCard.Padding = new Padding(18);
            this._OutputCard.Margin = new Padding(0, 14, 0, 0);
            this._OutputCard.BorderStyle = BorderStyle.FixedSingle;

            outputTitle.AutoSize = true;
            outputTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            outputTitle.Location = new System.Drawing.Point(18, 14);
            outputTitle.Text = "Kết quả";

            this._OutputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this._OutputTextBox.Location = new System.Drawing.Point(18, 42);
            this._OutputTextBox.Size = new System.Drawing.Size(872, 362);
            this._OutputTextBox.Multiline = true;
            this._OutputTextBox.ReadOnly = true;
            this._OutputTextBox.ScrollBars = ScrollBars.Both;
            this._OutputTextBox.WordWrap = false;
            this._OutputTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this._OutputTextBox.BackColor = Color.FromArgb(248, 250, 252);

            this._OutputCard.Controls.Add(outputTitle);
            this._OutputCard.Controls.Add(this._OutputTextBox);

            this._InputCard.Dock = DockStyle.Top;
            this._InputCard.Height = 156;
            this._InputCard.BackColor = Color.White;
            this._InputCard.Padding = new Padding(18);
            this._InputCard.Margin = new Padding(0, 14, 0, 0);
            this._InputCard.BorderStyle = BorderStyle.FixedSingle;

            inputTitle.AutoSize = true;
            inputTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            inputTitle.Location = new System.Drawing.Point(18, 14);
            inputTitle.Text = "Thiết lập quét";

            directoryLabel.AutoSize = true;
            directoryLabel.Location = new System.Drawing.Point(20, 48);
            directoryLabel.Text = "Thư mục";

            this._DirectoryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this._DirectoryTextBox.Location = new System.Drawing.Point(112, 44);
            this._DirectoryTextBox.Size = new System.Drawing.Size(626, 23);

            this._BrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._BrowseButton.Location = new System.Drawing.Point(758, 42);
            this._BrowseButton.Size = new System.Drawing.Size(110, 28);
            this._BrowseButton.Text = "Chọn thư mục";
            this._BrowseButton.Click += this.BrowseButtonOnClick;

            modelLabel.AutoSize = true;
            modelLabel.Location = new System.Drawing.Point(20, 84);
            modelLabel.Text = "Model";

            this._ModelComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._ModelComboBox.Items.AddRange(new object[] { Model.Hog.ToString(), Model.Cnn.ToString() });
            this._ModelComboBox.Location = new System.Drawing.Point(112, 80);
            this._ModelComboBox.Size = new System.Drawing.Size(150, 23);

            cpusLabel.AutoSize = true;
            cpusLabel.Location = new System.Drawing.Point(286, 84);
            cpusLabel.Text = "CPU";

            this._CpusNumericUpDown.Location = new System.Drawing.Point(344, 80);
            this._CpusNumericUpDown.Minimum = -1;
            this._CpusNumericUpDown.Maximum = 256;
            this._CpusNumericUpDown.Value = 1;
            this._CpusNumericUpDown.Width = 80;

            this._RunButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._RunButton.Location = new System.Drawing.Point(758, 78);
            this._RunButton.Size = new System.Drawing.Size(110, 28);
            this._RunButton.Text = "Chạy";
            this._RunButton.Click += this.RunButtonOnClick;

            this._StatusLabel.AutoSize = false;
            this._StatusLabel.Location = new System.Drawing.Point(428, 80);
            this._StatusLabel.Size = new System.Drawing.Size(320, 26);
            this._StatusLabel.Text = "Quét toàn bộ ảnh trong một thư mục và hiển thị các khung mặt tại đây.";

            this._InputCard.Controls.Add(inputTitle);
            this._InputCard.Controls.Add(directoryLabel);
            this._InputCard.Controls.Add(this._DirectoryTextBox);
            this._InputCard.Controls.Add(this._BrowseButton);
            this._InputCard.Controls.Add(modelLabel);
            this._InputCard.Controls.Add(this._ModelComboBox);
            this._InputCard.Controls.Add(cpusLabel);
            this._InputCard.Controls.Add(this._CpusNumericUpDown);
            this._InputCard.Controls.Add(this._RunButton);
            this._InputCard.Controls.Add(this._StatusLabel);

            rootPanel.Controls.Add(this._OutputCard);
            rootPanel.Controls.Add(this._InputCard);
            rootPanel.Controls.Add(this._HeaderPanel);

            this.Controls.Add(rootPanel);

            this._ModelComboBox.SelectedIndex = 0;
            this._DirectoryTextBox.Text = Path.GetFullPath(".");
            this._OutputTextBox.SelectionStart = 0;
        }

        private async void RunButtonOnClick(object sender, EventArgs e)
        {
            var folder = this._DirectoryTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show(this, "Vui lòng chọn một thư mục ảnh hợp lệ.", "Quét thư mục", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Enum.TryParse<Model>(this._ModelComboBox.SelectedItem?.ToString() ?? Model.Hog.ToString(), true, out var model))
            {
                MessageBox.Show(this, "Vui lòng chọn model hợp lệ.", "Quét thư mục", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var modelsDirectory = Path.GetFullPath("models");
            if (!Directory.Exists(modelsDirectory))
            {
                MessageBox.Show(this, $"Please check whether model directory '{modelsDirectory}' exists.", "Folder Scan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.SetBusyState(true, "Running folder scan...");
            this._OutputTextBox.Clear();

            try
            {
                var cpus = (int)this._CpusNumericUpDown.Value;
                var lines = await Task.Run(() => this._Engine.AnalyzeDirectory(folder, modelsDirectory, model, cpus));
                this._OutputTextBox.Lines = lines.ToArray();

                if (AppDatabase.Repository != null)
                {
                    await AppDatabase.Repository.SaveScanResultsAsync(
                        "folder",
                        folder,
                        model.ToString(),
                        cpus == -1 ? Environment.ProcessorCount : cpus,
                        lines);
                }

                this._StatusLabel.Text = $"Completed. {lines.Count} result line(s).";
            }
            catch (Exception ex)
            {
                this._StatusLabel.Text = "Thất bại.";
                MessageBox.Show(this, ex.Message, "Folder Scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.SetBusyState(false, this._StatusLabel.Text);
            }
        }

        private void BrowseButtonOnClick(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = this._DirectoryTextBox.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    this._DirectoryTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SetBusyState(bool busy, string statusText)
        {
            this._HeaderPanel.Enabled = true;
            this._InputCard.Enabled = true;
            this._BrowseButton.Enabled = !busy;
            this._RunButton.Enabled = !busy;
            this._DirectoryTextBox.Enabled = !busy;
            this._ModelComboBox.Enabled = !busy;
            this._CpusNumericUpDown.Enabled = !busy;
            this.UseWaitCursor = busy;
            this._StatusLabel.Text = statusText;
        }

        private void HeaderBadgeOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var bg = new SolidBrush(Color.FromArgb(37, 99, 235)))
                e.Graphics.FillRectangle(bg, 0, 0, 38, 38);

            using (var pen = new Pen(Color.White, 2F))
            {
                e.Graphics.DrawRectangle(pen, 9, 15, 20, 12);
                e.Graphics.DrawLine(pen, 9, 15, 14, 11);
                e.Graphics.DrawLine(pen, 14, 11, 20, 11);
                e.Graphics.DrawLine(pen, 20, 11, 23, 15);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FaceIDApp.Data;
using FaceIDApp.UserControls;

namespace FaceIDApp
{
    public partial class MainForm : Form
    {
        private readonly UserDto _currentUser;
        private Button _activeButton;
        private readonly List<Button> _menuButtons = new List<Button>();

        // User Controls
        private UCDashboard ucDashboard;
        private UCAttendance ucAttendance;
        private UCEmployeeManagement ucEmployeeManagement;
        private UCFaceRegistration ucFaceRegistration;
        private UCAttendanceReport ucAttendanceReport;
        private UCSettings ucSettings;
        private UCCatalog ucCatalog;
        private UCLeaveManagement ucLeaveManagement;

        // Colors
        private static readonly Color SidebarBg = Color.FromArgb(15, 23, 42);
        private static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);
        private static readonly Color SidebarActive = Color.FromArgb(56, 189, 248);
        private static readonly Color HeaderBg = Color.FromArgb(241, 245, 249);
        private static readonly Color ContentBg = Color.FromArgb(248, 250, 252);
        private static readonly Color TextLight = Color.FromArgb(203, 213, 225);
        private static readonly Color TextDim = Color.FromArgb(100, 116, 139);

        private Label lblHeaderTitle;
        private Label lblUserInfo;
        private Label lblClock;
        private Timer clockTimer;
        private Label _lblLeaveBadge;
        private Button _btnLeaveMenu;
        private readonly Dictionary<UserControl, Size> _viewMinSizes = new Dictionary<UserControl, Size>();
        private Button _btnLogout;

        public MainForm(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            BuildSidebar();
            BuildHeader();
            InitializeUserControls();
            ShowDashboard();
            LoadPendingBadgeAsync();

            // Refresh badge mỗi 5 phút
            var badgeTimer = new Timer { Interval = 300000 };
            badgeTimer.Tick += (s, e) => LoadPendingBadgeAsync();
            badgeTimer.Start();
        }

        public MainForm() : this(null) { }

        private void InitializeUserControls()
        {
            ucDashboard = RegisterResponsiveView(new UCDashboard());
            ucAttendance = RegisterResponsiveView(new UCAttendance());
            ucEmployeeManagement = RegisterResponsiveView(new UCEmployeeManagement());
            ucFaceRegistration = RegisterResponsiveView(new UCFaceRegistration());
            ucAttendanceReport = RegisterResponsiveView(new UCAttendanceReport());
            ucSettings = RegisterResponsiveView(new UCSettings());
            ucCatalog = RegisterResponsiveView(new UCCatalog());
            ucLeaveManagement = RegisterResponsiveView(new UCLeaveManagement());
        }

        private T RegisterResponsiveView<T>(T view) where T : UserControl
        {
            view.Dock = DockStyle.None;
            view.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            view.Margin = Padding.Empty;

            _viewMinSizes[view] = GetPreferredViewSize(view);
            return view;
        }

        private Size GetPreferredViewSize(UserControl view)
        {
            view.PerformLayout();

            var contentBounds = GetDescendantContentBounds(view);
            var minWidth = Math.Max(view.Width, contentBounds.Right + 16);
            var minHeight = Math.Max(view.Height, contentBounds.Bottom + 16);

            // Ngưỡng tối thiểu để UI không bị nén trên màn hình nhỏ.
            minWidth = Math.Max(minWidth, 1000);
            minHeight = Math.Max(minHeight, 640);

            return new Size(minWidth, minHeight);
        }

        private Rectangle GetDescendantContentBounds(Control parent)
        {
            var aggregate = Rectangle.Empty;

            foreach (Control child in parent.Controls)
            {
                var childBounds = child.Bounds;
                var nestedBounds = GetDescendantContentBounds(child);
                if (!nestedBounds.IsEmpty)
                {
                    childBounds = Rectangle.Union(
                        childBounds,
                        new Rectangle(
                            child.Left + nestedBounds.Left,
                            child.Top + nestedBounds.Top,
                            nestedBounds.Width,
                            nestedBounds.Height));
                }

                aggregate = aggregate.IsEmpty ? childBounds : Rectangle.Union(aggregate, childBounds);
            }

            return aggregate;
        }

        private void BuildSidebar()
        {
            pnlSidebar.BackColor = SidebarBg;
            pnlSidebar.AutoScroll = true;

            // Logo area
            var pnlLogo = new Panel { Height = 80, Dock = DockStyle.Top, BackColor = Color.Transparent };
            pnlLogo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new LinearGradientBrush(new Rectangle(20, 15, 44, 44), Color.FromArgb(56, 189, 248), Color.FromArgb(59, 130, 246), 45F))
                    g.FillEllipse(brush, 20, 18, 44, 44);
                using (var font = new Font("Segoe UI", 18F, FontStyle.Bold))
                    g.DrawString("🛡", font, Brushes.White, 24, 22);
                using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
                    g.DrawString("FaceID", font, new SolidBrush(Color.White), 72, 22);
                using (var font = new Font("Segoe UI", 8F))
                    g.DrawString("Attendance System", font, new SolidBrush(TextDim), 72, 47);
            };
            pnlSidebar.Controls.Add(pnlLogo);

            // Separator
            var sep1 = new Panel { Height = 1, Dock = DockStyle.Top, BackColor = Color.FromArgb(30, 41, 59) };
            pnlSidebar.Controls.Add(sep1);

            // Menu section label
            AddSectionLabel("MENU CHÍNH", 85);

            // Menu items
            var isAdmin = _currentUser?.Role == "Admin";

            int y = 110;
            AddMenuButton("🏠", "Trang chủ", y, () => ShowDashboard()); y += 44;
            AddMenuButton("📷", "Chấm công", y, () => ShowUC(ucAttendance, "Chấm công nhận diện")); y += 44;

            if (isAdmin)
            {
                AddMenuButton("👥", "Nhân viên", y, () => { ucEmployeeManagement.RefreshData(); ShowUC(ucEmployeeManagement, "Quản lý nhân viên"); }); y += 44;
                AddMenuButton("📸", "Đăng ký khuôn mặt", y, () => { ucFaceRegistration.RefreshData(); ShowUC(ucFaceRegistration, "Đăng ký khuôn mặt"); }); y += 44;

                AddSectionLabel("QUẢN LÝ", y); y += 28;
                AddMenuButton("🏢", "Danh mục", y, () => ShowUC(ucCatalog, "Quản lý danh mục")); y += 44;
                AddMenuButton("📋", "Nghỉ phép", y, () => ShowUC(ucLeaveManagement, "Quản lý nghỉ phép & Ngày lễ")); y += 44;
                _btnLeaveMenu = _menuButtons[_menuButtons.Count - 1];
                AddMenuButton("📊", "Báo cáo", y, () => ShowUC(ucAttendanceReport, "Báo cáo chấm công")); y += 44;

                AddSectionLabel("HỆ THỐNG", y); y += 28;
                AddMenuButton("⚙️", "Cài đặt", y, () => ShowUC(ucSettings, "Cài đặt hệ thống")); y += 44;
            }
            else
            {
                AddMenuButton("📋", "Nghỉ phép", y, () => ShowUC(ucLeaveManagement, "Đơn nghỉ phép")); y += 44;
            }

            // Logout at bottom
            _btnLogout = new Button
            {
                Text = "   🚪  Đăng xuất",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(248, 113, 113),
                FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(240, 44), Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Cursor = Cursors.Hand, BackColor = Color.Transparent
            };
            _btnLogout.Location = new Point(0, pnlSidebar.Height - 50);
            _btnLogout.FlatAppearance.BorderSize = 0;
            _btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 20, 20);
            _btnLogout.Click += (s, e) =>
            {
                var r = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.Retry;
                    this.Close();
                }
            };
            pnlSidebar.Controls.Add(_btnLogout);

            pnlSidebar.Resize += (s, e) => UpdateSidebarLayout();
            UpdateSidebarLayout();
        }

        private void UpdateSidebarLayout()
        {
            if (_btnLogout == null)
                return;

            _btnLogout.Width = pnlSidebar.ClientSize.Width;
            _btnLogout.Left = 0;
            _btnLogout.Top = Math.Max(0, pnlSidebar.ClientSize.Height - _btnLogout.Height - 8);
        }

        private void AddSectionLabel(string text, int y)
        {
            var lbl = new Label
            {
                Text = text, Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105), Size = new Size(200, 20),
                Location = new Point(20, y), BackColor = Color.Transparent
            };
            pnlSidebar.Controls.Add(lbl);
        }

        private void AddMenuButton(string icon, string text, int y, Action onClick)
        {
            var btn = new Button
            {
                Text = $"   {icon}  {text}",
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = TextLight,
                FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(230, 40), Location = new Point(5, y),
                Cursor = Cursors.Hand, BackColor = Color.Transparent, Tag = onClick
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = SidebarHover;
            btn.Click += (s, e) =>
            {
                SetActiveButton(btn);
                onClick?.Invoke();
            };
            btn.Paint += MenuButton_Paint;
            _menuButtons.Add(btn);
            pnlSidebar.Controls.Add(btn);
        }

        private void MenuButton_Paint(object sender, PaintEventArgs e)
        {
            var btn = (Button)sender;
            if (btn == _activeButton)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Left accent bar
                using (var brush = new LinearGradientBrush(new Rectangle(0, 0, 4, btn.Height), SidebarActive, Color.FromArgb(59, 130, 246), 90F))
                    g.FillRectangle(brush, 0, 4, 3, btn.Height - 8);
            }
        }

        private void SetActiveButton(Button btn)
        {
            if (_activeButton != null)
            {
                _activeButton.BackColor = Color.Transparent;
                _activeButton.ForeColor = TextLight;
                _activeButton.Invalidate();
            }
            _activeButton = btn;
            _activeButton.BackColor = SidebarHover;
            _activeButton.ForeColor = SidebarActive;
            _activeButton.Invalidate();
        }

        private void BuildHeader()
        {
            pnlHeader.BackColor = HeaderBg;
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240)))
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            lblHeaderTitle = new Label
            {
                Text = "🏠  Trang chủ",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true, Location = new Point(20, 14),
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblHeaderTitle);

            // Clock
            lblClock = new Label
            {
                Font = new Font("Segoe UI", 11F),
                ForeColor = TextDim,
                AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            lblClock.Location = new Point(pnlHeader.Width - 200, 18);
            pnlHeader.Controls.Add(lblClock);

            clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) =>
            {
                lblClock.Text = DateTime.Now.ToString("HH:mm:ss  •  dd/MM/yyyy");
                UpdateHeaderLayout();
            };
            clockTimer.Start();

            // User badge
            var displayName = _currentUser?.EmployeeName ?? _currentUser?.Username ?? "Admin";
            var role = _currentUser?.Role ?? "Admin";
            lblUserInfo = new Label
            {
                Text = $"👤 {displayName} ({role})",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextDim, AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            lblUserInfo.Location = new Point(pnlHeader.Width - 460, 20);
            pnlHeader.Controls.Add(lblUserInfo);

            pnlHeader.Resize += (s, e) => UpdateHeaderLayout();
            UpdateHeaderLayout();

            pnlMain.BackColor = ContentBg;
            pnlMain.AutoScroll = false;
        }

        private void UpdateHeaderLayout()
        {
            if (lblClock == null || lblUserInfo == null || lblHeaderTitle == null)
                return;

            const int rightPadding = 18;
            lblClock.Location = new Point(
                Math.Max(0, pnlHeader.Width - lblClock.Width - rightPadding),
                Math.Max(8, (pnlHeader.Height - lblClock.Height) / 2));

            var availableLeft = lblClock.Left - 18;
            var targetX = availableLeft - lblUserInfo.Width;
            var minX = lblHeaderTitle.Right + 14;
            lblUserInfo.Location = new Point(Math.Max(minX, targetX), Math.Max(8, (pnlHeader.Height - lblUserInfo.Height) / 2));

            // Khi không còn đủ chỗ, ẩn badge user để không chồng lên tiêu đề.
            lblUserInfo.Visible = (lblUserInfo.Right + 8) < lblClock.Left;
        }

        private void ShowDashboard()
        {
            if (_menuButtons.Count > 0)
                SetActiveButton(_menuButtons[0]);
            ucDashboard.RefreshData();
            ShowUC(ucDashboard, "Trang chủ");
        }

        private void ShowUC(UserControl uc, string title)
        {
            lblHeaderTitle.Text = $"  {title}";
            pnlMain.Controls.Clear();

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBg,
                AutoScroll = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            pnlMain.Controls.Add(host);

            if (!_viewMinSizes.TryGetValue(uc, out var minSize))
                minSize = GetPreferredViewSize(uc);

            uc.Dock = DockStyle.None;
            uc.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            uc.Location = Point.Empty;
            host.Controls.Add(uc);

            Action resizeView = () =>
            {
                host.AutoScrollMinSize = minSize;
                var targetWidth = Math.Max(host.ClientSize.Width, minSize.Width);
                var targetHeight = Math.Max(host.ClientSize.Height, minSize.Height);
                if (uc.Width != targetWidth || uc.Height != targetHeight)
                    uc.Size = new Size(targetWidth, targetHeight);
            };

            host.Resize += (s, e) => resizeView();
            resizeView();
            uc.BringToFront();
        }

        private async void LoadPendingBadgeAsync()
        {
            try
            {
                if (_currentUser?.Role != "Admin") return;
                var count = await AppDatabase.Repository.GetPendingLeaveCountAsync();
                if (_lblLeaveBadge == null && _btnLeaveMenu != null)
                {
                    _lblLeaveBadge = new Label
                    {
                        AutoSize = false,
                        Size = new Size(22, 22),
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(239, 68, 68)
                    };
                    _lblLeaveBadge.Location = new Point(_btnLeaveMenu.Right - 30, _btnLeaveMenu.Top + 2);
                    pnlSidebar.Controls.Add(_lblLeaveBadge);
                    _lblLeaveBadge.BringToFront();
                }
                if (_lblLeaveBadge != null)
                {
                    _lblLeaveBadge.Text = count > 99 ? "99+" : count.ToString();
                    _lblLeaveBadge.Visible = count > 0;
                }
            }
            catch { }
        }
    }
}

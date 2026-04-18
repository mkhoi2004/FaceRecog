using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FaceRecognitionDotNet.WinForms.Data;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FaceRecognitionDotNet.WinForms
{
    public sealed class MainForm : Form
    {
        private static readonly Color SidebarBaseColor = Color.FromArgb(30, 41, 59);
        private static readonly Color SidebarHoverColor = Color.FromArgb(51, 65, 85);
        private static readonly Color SidebarActiveColor = Color.FromArgb(37, 99, 235);

        private readonly Panel _NavigationPanel;
        private readonly Panel _BrandMark;
        private readonly Panel _ActiveIndicator;
        private readonly Panel _ContentPanel;
        private readonly Panel _WorkspaceFrame;
        private readonly Label _TitleLabel;
        private readonly Label _SubtitleLabel;
        private readonly Label _SectionLabel;
        private readonly Label _FooterLabel;
        private readonly Panel _FolderIconBadge;
        private readonly Panel _SingleIconBadge;
        private readonly Panel _DatabaseIconBadge;
        private readonly Button _FolderButton;
        private readonly Button _SingleButton;
        private readonly Button _AttendanceButton;
        private readonly Button _AccountsButton;
        private readonly Button _DatabaseButton;
        private readonly Timer _NavigationAnimationTimer;
        private readonly Dictionary<Button, UserControl> _Pages;
        private readonly FolderDetectionView _FolderView;
        private readonly SingleImageDetectionView _SingleView;
        private readonly AttendanceView _AttendanceView;
        private readonly AccountsView _AccountsView;
        private readonly AdminDashboardView _AdminView;
        private readonly AppUserItem _CurrentUser;
        private readonly bool _IsAdmin;
        private UserControl _ActivePage;
        private Button _ActiveButton;
        private Button _HoveredButton;
        private Color _FolderCurrentColor;
        private Color _FolderTargetColor;
        private Color _SingleCurrentColor;
        private Color _SingleTargetColor;
        private Color _AttendanceCurrentColor;
        private Color _AttendanceTargetColor;
        private Color _AccountsCurrentColor;
        private Color _AccountsTargetColor;
        private Color _DatabaseCurrentColor;
        private Color _DatabaseTargetColor;
        private float _IndicatorCurrentTop;
        private float _IndicatorTargetTop;
        private float _IndicatorCurrentHeight;
        private float _IndicatorTargetHeight;

        public MainForm(AppUserItem currentUser)
        {
            this._CurrentUser = currentUser;
            this._IsAdmin = currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            this._NavigationPanel = new Panel();
            this._BrandMark = new Panel();
            this._ActiveIndicator = new Panel();
            this._ContentPanel = new Panel();
            this._WorkspaceFrame = new Panel();
            this._TitleLabel = new Label();
            this._SubtitleLabel = new Label();
            this._SectionLabel = new Label();
            this._FooterLabel = new Label();
            this._FolderIconBadge = new Panel();
            this._SingleIconBadge = new Panel();
            this._DatabaseIconBadge = new Panel();
            this._FolderButton = new Button();
            this._SingleButton = new Button();
            this._AttendanceButton = new Button();
            this._AccountsButton = new Button();
            this._DatabaseButton = new Button();
            this._NavigationAnimationTimer = new Timer();
            this._Pages = new Dictionary<Button, UserControl>();
            this._FolderView = new FolderDetectionView();
            this._SingleView = new SingleImageDetectionView();
            this._AttendanceView = new AttendanceView(this._CurrentUser);
            this._AccountsView = new AccountsView();
            this._AdminView = new AdminDashboardView();
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this._NavigationPanel.SuspendLayout();
            this._ContentPanel.SuspendLayout();
            this._WorkspaceFrame.SuspendLayout();

            this.Text = "FaceRecognitionDotNet WinForms";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(1100, 760);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            this._NavigationPanel.Dock = DockStyle.Left;
            this._NavigationPanel.Width = 284;
            this._NavigationPanel.Padding = new Padding(18, 20, 18, 18);
            this._NavigationPanel.BackColor = Color.FromArgb(17, 24, 39);

            this._BrandMark.BackColor = Color.FromArgb(37, 99, 235);
            this._BrandMark.Location = new System.Drawing.Point(18, 18);
            this._BrandMark.Size = new System.Drawing.Size(14, 50);
            this._BrandMark.Paint += this.BrandMarkOnPaint;

            var brandGlow = new Panel();
            brandGlow.BackColor = Color.FromArgb(59, 130, 246);
            brandGlow.Location = new System.Drawing.Point(38, 18);
            brandGlow.Size = new System.Drawing.Size(48, 50);
            brandGlow.Paint += this.BrandGlowOnPaint;

            this._ActiveIndicator.BackColor = Color.FromArgb(96, 165, 250);
            this._ActiveIndicator.Width = 4;
            this._ActiveIndicator.Height = 44;
            this._ActiveIndicator.Left = 18;
            this._ActiveIndicator.Top = 190;

            this._TitleLabel.AutoSize = false;
            this._TitleLabel.Font = new Font("Segoe UI Semibold", 14.5F, FontStyle.Bold, GraphicsUnit.Point);
            this._TitleLabel.ForeColor = Color.White;
            this._TitleLabel.Location = new System.Drawing.Point(92, 14);
            this._TitleLabel.Size = new System.Drawing.Size(176, 28);
            this._TitleLabel.AutoEllipsis = true;
            this._TitleLabel.Text = "FaceRecognitionDotNet";

            this._SubtitleLabel.AutoSize = false;
            this._SubtitleLabel.ForeColor = Color.FromArgb(194, 202, 214);
            this._SubtitleLabel.Location = new System.Drawing.Point(92, 42);
            this._SubtitleLabel.Size = new System.Drawing.Size(176, 38);
            this._SubtitleLabel.AutoEllipsis = true;
            this._SubtitleLabel.Text = "Thanh điều hướng cho các ví dụ của thư viện, tách riêng khỏi dự án lõi.";

            var currentUserLabel = new Label();
            currentUserLabel.AutoSize = false;
            currentUserLabel.ForeColor = Color.FromArgb(226, 232, 240);
            currentUserLabel.Location = new System.Drawing.Point(18, 128);
            currentUserLabel.Size = new System.Drawing.Size(236, 26);
            currentUserLabel.AutoEllipsis = true;
            currentUserLabel.Text = this._CurrentUser == null
                ? "Đã đăng nhập: khách"
                : $"Đã đăng nhập: {this._CurrentUser.Username} ({this._CurrentUser.Role})";

            this._SectionLabel.AutoSize = false;
            this._SectionLabel.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            this._SectionLabel.ForeColor = Color.FromArgb(148, 163, 184);
            this._SectionLabel.Location = new System.Drawing.Point(18, 168);
            this._SectionLabel.Size = new System.Drawing.Size(120, 18);
            this._SectionLabel.Text = this._IsAdmin ? "QUẢN TRỊ" : "CHẤM CÔNG";

            this._FolderIconBadge.BackColor = Color.FromArgb(37, 99, 235);
            this._FolderIconBadge.Location = new System.Drawing.Point(22, 198);
            this._FolderIconBadge.Size = new System.Drawing.Size(26, 26);
            this._FolderIconBadge.Paint += this.FolderBadgeOnPaint;

            this._SingleIconBadge.BackColor = Color.FromArgb(71, 85, 105);
            this._SingleIconBadge.Location = new System.Drawing.Point(22, 250);
            this._SingleIconBadge.Size = new System.Drawing.Size(26, 26);
            this._SingleIconBadge.Paint += this.SingleBadgeOnPaint;

            this._DatabaseIconBadge.BackColor = Color.FromArgb(71, 85, 105);
            this._DatabaseIconBadge.Location = new System.Drawing.Point(22, 302);
            this._DatabaseIconBadge.Size = new System.Drawing.Size(26, 26);
            this._DatabaseIconBadge.Paint += this.DatabaseBadgeOnPaint;

            this._FolderButton.FlatStyle = FlatStyle.Flat;
            this._FolderButton.FlatAppearance.BorderSize = 0;
            this._FolderButton.BackColor = SidebarBaseColor;
            this._FolderButton.ForeColor = Color.White;
            this._FolderButton.TextAlign = ContentAlignment.MiddleLeft;
            this._FolderButton.Padding = new Padding(58, 0, 0, 0);
            this._FolderButton.Location = new System.Drawing.Point(22, 190);
            this._FolderButton.Size = new System.Drawing.Size(204, 44);
            this._FolderButton.Text = "Quét thư mục";
            this._FolderButton.Tag = this._FolderView;
            this._FolderButton.Click += this.NavigationButtonOnClick;
            this._FolderButton.MouseEnter += this.NavButtonMouseEnter;
            this._FolderButton.MouseLeave += this.NavButtonMouseLeave;
            this._FolderButton.GotFocus += this.NavButtonGotFocus;
            this._FolderButton.LostFocus += this.NavButtonLostFocus;

            this._SingleButton.FlatStyle = FlatStyle.Flat;
            this._SingleButton.FlatAppearance.BorderSize = 0;
            this._SingleButton.BackColor = SidebarBaseColor;
            this._SingleButton.ForeColor = Color.White;
            this._SingleButton.TextAlign = ContentAlignment.MiddleLeft;
            this._SingleButton.Padding = new Padding(58, 0, 0, 0);
            this._SingleButton.Location = new System.Drawing.Point(22, 242);
            this._SingleButton.Size = new System.Drawing.Size(204, 44);
            this._SingleButton.Text = "Ảnh đơn";
            this._SingleButton.Tag = this._SingleView;
            this._SingleButton.Click += this.NavigationButtonOnClick;
            this._SingleButton.MouseEnter += this.NavButtonMouseEnter;
            this._SingleButton.MouseLeave += this.NavButtonMouseLeave;
            this._SingleButton.GotFocus += this.NavButtonGotFocus;
            this._SingleButton.LostFocus += this.NavButtonLostFocus;

            this._AttendanceButton.FlatStyle = FlatStyle.Flat;
            this._AttendanceButton.FlatAppearance.BorderSize = 0;
            this._AttendanceButton.BackColor = SidebarBaseColor;
            this._AttendanceButton.ForeColor = Color.White;
            this._AttendanceButton.TextAlign = ContentAlignment.MiddleLeft;
            this._AttendanceButton.Padding = new Padding(58, 0, 0, 0);
            this._AttendanceButton.Location = new System.Drawing.Point(22, 294);
            this._AttendanceButton.Size = new System.Drawing.Size(204, 44);
            this._AttendanceButton.Text = "Chấm công";
            this._AttendanceButton.Tag = this._AttendanceView;
            this._AttendanceButton.Click += this.NavigationButtonOnClick;
            this._AttendanceButton.MouseEnter += this.NavButtonMouseEnter;
            this._AttendanceButton.MouseLeave += this.NavButtonMouseLeave;
            this._AttendanceButton.GotFocus += this.NavButtonGotFocus;
            this._AttendanceButton.LostFocus += this.NavButtonLostFocus;

            this._AccountsButton.FlatStyle = FlatStyle.Flat;
            this._AccountsButton.FlatAppearance.BorderSize = 0;
            this._AccountsButton.BackColor = SidebarBaseColor;
            this._AccountsButton.ForeColor = Color.White;
            this._AccountsButton.TextAlign = ContentAlignment.MiddleLeft;
            this._AccountsButton.Padding = new Padding(58, 0, 0, 0);
            this._AccountsButton.Location = new System.Drawing.Point(22, 346);
            this._AccountsButton.Size = new System.Drawing.Size(204, 44);
            this._AccountsButton.Text = "Tài khoản";
            this._AccountsButton.Tag = this._AccountsView;
            this._AccountsButton.Click += this.NavigationButtonOnClick;
            this._AccountsButton.MouseEnter += this.NavButtonMouseEnter;
            this._AccountsButton.MouseLeave += this.NavButtonMouseLeave;
            this._AccountsButton.GotFocus += this.NavButtonGotFocus;
            this._AccountsButton.LostFocus += this.NavButtonLostFocus;

            this._DatabaseButton.FlatStyle = FlatStyle.Flat;
            this._DatabaseButton.FlatAppearance.BorderSize = 0;
            this._DatabaseButton.BackColor = SidebarBaseColor;
            this._DatabaseButton.ForeColor = Color.White;
            this._DatabaseButton.TextAlign = ContentAlignment.MiddleLeft;
            this._DatabaseButton.Padding = new Padding(58, 0, 0, 0);
            this._DatabaseButton.Location = new System.Drawing.Point(22, 398);
            this._DatabaseButton.Size = new System.Drawing.Size(204, 44);
            this._DatabaseButton.Text = "Bảng điều khiển quản trị";
            this._DatabaseButton.Tag = this._AdminView;
            this._DatabaseButton.Click += this.NavigationButtonOnClick;
            this._DatabaseButton.MouseEnter += this.NavButtonMouseEnter;
            this._DatabaseButton.MouseLeave += this.NavButtonMouseLeave;
            this._DatabaseButton.GotFocus += this.NavButtonGotFocus;
            this._DatabaseButton.LostFocus += this.NavButtonLostFocus;

            this._FooterLabel.AutoSize = false;
            this._FooterLabel.ForeColor = Color.FromArgb(194, 202, 214);
            this._FooterLabel.Location = new System.Drawing.Point(18, 650);
            this._FooterLabel.Size = new System.Drawing.Size(214, 48);
            this._FooterLabel.Text = "Chỉ là host WinForms. Thư viện lõi không thay đổi.";

            this._NavigationPanel.Controls.Add(brandGlow);
            this._NavigationPanel.Controls.Add(this._BrandMark);
            this._NavigationPanel.Controls.Add(this._TitleLabel);
            this._NavigationPanel.Controls.Add(this._SubtitleLabel);
            this._NavigationPanel.Controls.Add(this._SectionLabel);
            this._NavigationPanel.Controls.Add(currentUserLabel);
            this._NavigationPanel.Controls.Add(this._ActiveIndicator);
            this._NavigationPanel.Controls.Add(this._FolderIconBadge);
            this._NavigationPanel.Controls.Add(this._SingleIconBadge);
            this._NavigationPanel.Controls.Add(this._DatabaseIconBadge);
            this._NavigationPanel.Controls.Add(this._FolderButton);
            this._NavigationPanel.Controls.Add(this._SingleButton);
            this._NavigationPanel.Controls.Add(this._AttendanceButton);
            this._NavigationPanel.Controls.Add(this._AccountsButton);
            this._NavigationPanel.Controls.Add(this._DatabaseButton);
            this._NavigationPanel.Controls.Add(this._FooterLabel);

            this._ContentPanel.Dock = DockStyle.Fill;
            this._ContentPanel.Padding = new Padding(18);
            this._ContentPanel.BackColor = Color.FromArgb(226, 232, 240);

            this._WorkspaceFrame.Dock = DockStyle.Fill;
            this._WorkspaceFrame.BackColor = Color.White;
            this._WorkspaceFrame.Padding = new Padding(0);

            this._ContentPanel.Controls.Add(this._WorkspaceFrame);
            this.Controls.Add(this._ContentPanel);
            this.Controls.Add(this._NavigationPanel);

            this._Pages.Add(this._FolderButton, this._FolderView);
            this._Pages.Add(this._SingleButton, this._SingleView);
            this._Pages.Add(this._AttendanceButton, this._AttendanceView);
            this._Pages.Add(this._AccountsButton, this._AccountsView);
            this._Pages.Add(this._DatabaseButton, this._AdminView);

            this.ConfigureRoleNavigation();

            this._FolderCurrentColor = SidebarBaseColor;
            this._FolderTargetColor = SidebarBaseColor;
            this._SingleCurrentColor = SidebarBaseColor;
            this._SingleTargetColor = SidebarBaseColor;
            this._AttendanceCurrentColor = SidebarBaseColor;
            this._AttendanceTargetColor = SidebarBaseColor;
            this._AccountsCurrentColor = SidebarBaseColor;
            this._AccountsTargetColor = SidebarBaseColor;
            this._DatabaseCurrentColor = SidebarBaseColor;
            this._DatabaseTargetColor = SidebarBaseColor;
            this._IndicatorCurrentTop = this._FolderButton.Top;
            this._IndicatorTargetTop = this._FolderButton.Top;
            this._IndicatorCurrentHeight = this._FolderButton.Height;
            this._IndicatorTargetHeight = this._FolderButton.Height;

            this._NavigationAnimationTimer.Interval = 12;
            this._NavigationAnimationTimer.Tick += this.NavigationAnimationTimerOnTick;

            this.ShowPage(this._AttendanceView, this._AttendanceButton);
            this._NavigationAnimationTimer.Start();

            this._ContentPanel.ResumeLayout(false);
            this._WorkspaceFrame.ResumeLayout(false);
            this._NavigationPanel.ResumeLayout(false);
            this._NavigationPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private void NavigationButtonOnClick(object sender, EventArgs e)
        {
            if (sender is Button button && this._Pages.TryGetValue(button, out var page))
                this.ShowPage(page, button);
        }

        private void ShowPage(UserControl page, Button activeButton)
        {
            if (this._ActivePage != null)
                this._WorkspaceFrame.Controls.Remove(this._ActivePage);

            this._ActiveButton = activeButton;
            this._ActivePage = page;
            this._ActivePage.Dock = DockStyle.Fill;
            this._WorkspaceFrame.Controls.Add(this._ActivePage);
            this._ActivePage.BringToFront();

            this.UpdateNavigationState(activeButton);
        }

        private void UpdateNavigationState(Button activeButton)
        {
            this._IndicatorTargetTop = activeButton.Top;
            this._IndicatorTargetHeight = activeButton.Height;

            this._FolderTargetColor = activeButton == this._FolderButton
                ? SidebarActiveColor
                : this._HoveredButton == this._FolderButton
                    ? SidebarHoverColor
                    : SidebarBaseColor;

            this._SingleTargetColor = activeButton == this._SingleButton
                ? SidebarActiveColor
                : this._HoveredButton == this._SingleButton
                    ? SidebarHoverColor
                    : SidebarBaseColor;

            this._AttendanceTargetColor = activeButton == this._AttendanceButton
                ? SidebarActiveColor
                : this._HoveredButton == this._AttendanceButton
                    ? SidebarHoverColor
                    : SidebarBaseColor;

            this._AccountsTargetColor = activeButton == this._AccountsButton
                ? SidebarActiveColor
                : this._HoveredButton == this._AccountsButton
                    ? SidebarHoverColor
                    : SidebarBaseColor;

            this._DatabaseTargetColor = activeButton == this._DatabaseButton
                ? SidebarActiveColor
                : this._HoveredButton == this._DatabaseButton
                    ? SidebarHoverColor
                    : SidebarBaseColor;

            this._FolderIconBadge.BackColor = activeButton == this._FolderButton ? SidebarActiveColor : Color.FromArgb(71, 85, 105);
            this._SingleIconBadge.BackColor = activeButton == this._SingleButton ? SidebarActiveColor : Color.FromArgb(71, 85, 105);
            this._DatabaseIconBadge.BackColor = activeButton == this._DatabaseButton ? SidebarActiveColor : Color.FromArgb(71, 85, 105);
            this._FolderIconBadge.Invalidate();
            this._SingleIconBadge.Invalidate();
            this._DatabaseIconBadge.Invalidate();
            this._BrandMark.Invalidate();
        }

        private void NavButtonMouseEnter(object sender, EventArgs e)
        {
            if (!(sender is Button hovered))
                return;

            this._HoveredButton = hovered;
            if (hovered != this._ActiveButton)
                this.SetTargetColor(hovered, SidebarHoverColor);
        }

        private void NavButtonMouseLeave(object sender, EventArgs e)
        {
            if (!(sender is Button hovered))
                return;

            if (this._HoveredButton == hovered)
                this._HoveredButton = null;

            this.SetTargetColor(hovered, hovered == this._ActiveButton ? SidebarActiveColor : SidebarBaseColor);
        }

        private void NavButtonGotFocus(object sender, EventArgs e)
        {
            if (sender is Button focused)
                this.SetTargetColor(focused, focused == this._ActiveButton ? SidebarActiveColor : SidebarHoverColor);
        }

        private void NavButtonLostFocus(object sender, EventArgs e)
        {
            if (sender is Button lost)
                this.SetTargetColor(lost, lost == this._ActiveButton ? SidebarActiveColor : SidebarBaseColor);
        }

        private void NavigationAnimationTimerOnTick(object sender, EventArgs e)
        {
            this._FolderCurrentColor = AnimateColor(this._FolderCurrentColor, this._FolderTargetColor, 0.12F);
            this._SingleCurrentColor = AnimateColor(this._SingleCurrentColor, this._SingleTargetColor, 0.12F);
            this._AttendanceCurrentColor = AnimateColor(this._AttendanceCurrentColor, this._AttendanceTargetColor, 0.12F);
            this._AccountsCurrentColor = AnimateColor(this._AccountsCurrentColor, this._AccountsTargetColor, 0.12F);
            this._DatabaseCurrentColor = AnimateColor(this._DatabaseCurrentColor, this._DatabaseTargetColor, 0.12F);
            this._FolderButton.BackColor = this._FolderCurrentColor;
            this._SingleButton.BackColor = this._SingleCurrentColor;
            this._AttendanceButton.BackColor = this._AttendanceCurrentColor;
            this._AccountsButton.BackColor = this._AccountsCurrentColor;
            this._DatabaseButton.BackColor = this._DatabaseCurrentColor;

            this._IndicatorCurrentTop = AnimateFloat(this._IndicatorCurrentTop, this._IndicatorTargetTop, 0.14F);
            this._IndicatorCurrentHeight = AnimateFloat(this._IndicatorCurrentHeight, this._IndicatorTargetHeight, 0.14F);
            this._ActiveIndicator.Top = (int)Math.Round(this._IndicatorCurrentTop);
            this._ActiveIndicator.Height = Math.Max(28, (int)Math.Round(this._IndicatorCurrentHeight));

            if (AreClose(this._FolderCurrentColor, this._FolderTargetColor) &&
                AreClose(this._SingleCurrentColor, this._SingleTargetColor) &&
                AreClose(this._AttendanceCurrentColor, this._AttendanceTargetColor) &&
                AreClose(this._AccountsCurrentColor, this._AccountsTargetColor) &&
                AreClose(this._DatabaseCurrentColor, this._DatabaseTargetColor) &&
                Math.Abs(this._IndicatorCurrentTop - this._IndicatorTargetTop) < 0.5F &&
                Math.Abs(this._IndicatorCurrentHeight - this._IndicatorTargetHeight) < 0.5F)
            {
                this._NavigationAnimationTimer.Stop();
                return;
            }

            this._FolderIconBadge.Invalidate();
            this._SingleIconBadge.Invalidate();
            this._DatabaseIconBadge.Invalidate();
            this._BrandMark.Invalidate();
        }

        private void ConfigureRoleNavigation()
        {
            this._FolderButton.Visible = false;
            this._SingleButton.Visible = false;
            this._AccountsButton.Visible = false;
            this._FolderIconBadge.Visible = false;
            this._SingleIconBadge.Visible = false;
            this._DatabaseIconBadge.Visible = false;

            this._AttendanceButton.Location = new System.Drawing.Point(22, 190);

            if (this._IsAdmin)
            {
                this._DatabaseButton.Text = "Bảng điều khiển quản trị";
                this._DatabaseButton.Location = new System.Drawing.Point(22, 242);
                this._DatabaseButton.Visible = true;
                this._FooterLabel.Text = "Chế độ quản trị: có lịch sử chấm công và lịch sử hệ thống.";
            }
            else
            {
                this._AttendanceButton.Visible = true;
                this._DatabaseButton.Visible = false;
                this._FooterLabel.Text = "Chế độ người dùng: chỉ chấm công bằng khuôn mặt.";
            }
        }

        private void SetTargetColor(Button button, Color targetColor)
        {
            if (button == this._FolderButton)
                this._FolderTargetColor = targetColor;
            else if (button == this._SingleButton)
                this._SingleTargetColor = targetColor;
            else if (button == this._AttendanceButton)
                this._AttendanceTargetColor = targetColor;
            else if (button == this._AccountsButton)
                this._AccountsTargetColor = targetColor;
            else if (button == this._DatabaseButton)
                this._DatabaseTargetColor = targetColor;

            if (!this._NavigationAnimationTimer.Enabled)
                this._NavigationAnimationTimer.Start();
        }

        private static Color AnimateColor(Color current, Color target, float step)
        {
            return Color.FromArgb(
                current.A,
                (int)Math.Round(current.R + ((target.R - current.R) * step)),
                (int)Math.Round(current.G + ((target.G - current.G) * step)),
                (int)Math.Round(current.B + ((target.B - current.B) * step)));
        }

        private static float AnimateFloat(float current, float target, float step)
        {
            return current + ((target - current) * step);
        }

        private static bool AreClose(Color current, Color target)
        {
            return Math.Abs(current.R - target.R) < 2 && Math.Abs(current.G - target.G) < 2 && Math.Abs(current.B - target.B) < 2;
        }

        private void BrandGlowOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(Color.FromArgb(96, 165, 250)))
                e.Graphics.FillEllipse(brush, 0, 0, 44, 44);

            using (var pen = new Pen(Color.White, 2F))
            {
                e.Graphics.DrawEllipse(pen, 10, 10, 24, 24);
                e.Graphics.DrawLine(pen, 14, 23, 30, 23);
                e.Graphics.DrawLine(pen, 22, 14, 22, 30);
            }
        }

        private void BrandMarkOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.White, 2.5F))
            {
                e.Graphics.DrawEllipse(pen, 2, 2, 8, 40);
                e.Graphics.DrawEllipse(pen, 4, 8, 4, 28);
            }
        }

        private void FolderBadgeOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = new SolidBrush(this._FolderIconBadge.BackColor))
                e.Graphics.FillRectangle(bg, 0, 0, 26, 26);

            using (var pen = new Pen(Color.White, 1.8F))
            {
                using (var path = CreateRoundedRectangle(5, 8, 16, 11, 2))
                    e.Graphics.DrawPath(pen, path);

                using (var fill = new SolidBrush(Color.White))
                    e.Graphics.FillRectangle(fill, 5, 8, 6, 4);

                e.Graphics.DrawLine(pen, 5, 12, 21, 12);
            }
        }

        private void SingleBadgeOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = new SolidBrush(this._SingleIconBadge.BackColor))
                e.Graphics.FillRectangle(bg, 0, 0, 26, 26);

            using (var pen = new Pen(Color.White, 1.7F))
            {
                e.Graphics.DrawRectangle(pen, 5, 5, 16, 16);
                e.Graphics.DrawLine(pen, 7, 17, 12, 12);
                e.Graphics.DrawLine(pen, 12, 12, 16, 15);
                using (var fill = new SolidBrush(Color.White))
                    e.Graphics.FillEllipse(fill, 15, 7, 3, 3);
            }
        }

        private void DatabaseBadgeOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = new SolidBrush(this._DatabaseIconBadge.BackColor))
                e.Graphics.FillRectangle(bg, 0, 0, 26, 26);

            using (var pen = new Pen(Color.White, 1.6F))
            {
                e.Graphics.DrawEllipse(pen, 6, 5, 14, 4);
                e.Graphics.DrawLine(pen, 6, 7, 6, 16);
                e.Graphics.DrawLine(pen, 20, 7, 20, 16);
                e.Graphics.DrawEllipse(pen, 6, 14, 14, 4);
                e.Graphics.DrawLine(pen, 6, 16, 20, 16);
                e.Graphics.DrawLine(pen, 6, 11, 20, 11);
            }
        }

        private static GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            var arc = new Rectangle(x, y, diameter, diameter);

            path.AddArc(arc, 180, 90);
            arc.X = x + width - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = y + height - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = x;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace FaceIDApp.Data
{
    /// <summary>
    /// Helper tĩnh áp dụng style nhất quán cho toàn bộ DataGridView trong app.
    /// Tuân thủ Fluent Slate design: header tối, row sáng, alternating, status color.
    /// </summary>
    internal static class GridStyleHelper
    {
        // Slate palette
        private static readonly Color HeaderBg = Color.FromArgb(30, 41, 59);
        private static readonly Color HeaderFg = Color.White;
        private static readonly Color RowAlt = Color.FromArgb(248, 250, 252);
        private static readonly Color GridLine = Color.FromArgb(226, 232, 240);
        private static readonly Color SelectBg = Color.FromArgb(219, 234, 254);
        private static readonly Color SelectFg = Color.Black;

        /// <summary>
        /// Áp dụng Fluent Slate style cho DataGridView.
        /// </summary>
        public static void ApplyStandard(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = HeaderFg;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBg;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 4, 0, 4);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.SelectionBackColor = SelectBg;
            dgv.DefaultCellStyle.SelectionForeColor = SelectFg;
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
            dgv.RowTemplate.Height = 30;

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = GridLine;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }

        /// <summary>
        /// Tô màu cell trạng thái chấm công qua CellFormatting event.
        /// Gọi: dgv.CellFormatting += GridStyleHelper.StatusCellFormatting("colTrangThai");
        /// </summary>
        public static DataGridViewCellFormattingEventHandler StatusCellFormatting(string statusColumnName)
        {
            return (sender, e) =>
            {
                var dgv = sender as DataGridView;
                if (dgv == null || e.ColumnIndex < 0 || e.RowIndex < 0) return;
                if (dgv.Columns[e.ColumnIndex].Name != statusColumnName) return;

                var value = e.Value?.ToString();
                if (string.IsNullOrEmpty(value)) return;

                switch (value)
                {
                    case "Present":
                    case "Đúng giờ":
                        e.CellStyle.BackColor = Color.FromArgb(212, 237, 218);
                        e.CellStyle.ForeColor = Color.FromArgb(21, 87, 36);
                        break;
                    case "Late":
                    case "Đi trễ":
                    case "LateAndEarly":
                    case "Trễ+Sớm":
                        e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                        e.CellStyle.ForeColor = Color.FromArgb(133, 100, 4);
                        break;
                    case "EarlyLeave":
                    case "Về sớm":
                        e.CellStyle.BackColor = Color.FromArgb(255, 237, 213);
                        e.CellStyle.ForeColor = Color.FromArgb(154, 52, 18);
                        break;
                    case "Absent":
                    case "Vắng mặt":
                        e.CellStyle.BackColor = Color.FromArgb(248, 215, 218);
                        e.CellStyle.ForeColor = Color.FromArgb(114, 28, 36);
                        break;
                    case "Leave":
                    case "Nghỉ phép":
                    case "Holiday":
                    case "Ngày lễ":
                        e.CellStyle.BackColor = Color.FromArgb(209, 233, 252);
                        e.CellStyle.ForeColor = Color.FromArgb(29, 78, 137);
                        break;
                }
            };
        }
    }
}

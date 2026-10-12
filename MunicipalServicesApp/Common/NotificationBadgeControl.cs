using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// A small red "unread count" badge, drawn as a filled rounded pill with white
    /// bold text (reusing <see cref="AppTheme.DrawPill"/> for a consistent look with
    /// the rest of the app's hand-drawn controls). Hides itself automatically when
    /// <see cref="Count"/> is zero, and shows "99+" rather than growing unbounded.
    /// </summary>
    public class NotificationBadgeControl : Control
    {
        private const float BadgeHeight = 20F;
        private int _count;

        /// <summary>Number of unread items to display. Setting this to 0 hides the badge.</summary>
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                UpdateSizeAndVisibility();
                Invalidate();
            }
        }

        public NotificationBadgeControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Height = (int)BadgeHeight;
            Visible = false;
        }

        private string DisplayText => _count > 99 ? "99+" : _count.ToString();

        private void UpdateSizeAndVisibility()
        {
            Visible = _count > 0;
            if (!Visible)
            {
                return;
            }

            using (var g = CreateGraphics())
            using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                var textSize = g.MeasureString(DisplayText, font);
                Width = Math.Max((int)(textSize.Width + 16), (int)BadgeHeight);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_count <= 0)
            {
                return;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var backBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(backBrush, ClientRectangle);
            }

            AppTheme.DrawPill(g, DisplayText, new PointF(0, 0), AppTheme.DangerColor, BadgeHeight);
        }
    }
}

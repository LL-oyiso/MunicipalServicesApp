using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalServicesApp.Common
{
    /// <summary>Which line-icon a <see cref="MenuCardControl"/> draws in its icon badge.</summary>
    public enum MenuCardIcon
    {
        Document,
        Calendar,
        Search,
        Shield
    }

    /// <summary>
    /// A clickable card tile for the main menu (icon, title, description), styled after
    /// the municipality's own online service-request portal. Everything is drawn with
    /// GDI+ rather than child controls, so the whole card - icon, text and all - is a
    /// single click target and reacts to hover/disabled state as one unit.
    /// </summary>
    public class MenuCardControl : Panel
    {
        private const int IconBadgeSize = 46;

        private string _title = string.Empty;
        private string _description = string.Empty;
        private MenuCardIcon _icon = MenuCardIcon.Document;
        private string _badgeText = string.Empty;
        private bool _isHovering;

        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value ?? string.Empty; Invalidate(); }
        }

        public MenuCardIcon Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        /// <summary>Small pill shown top-right while the card is disabled, e.g. "Coming soon".</summary>
        public string BadgeText
        {
            get => _badgeText;
            set { _badgeText = value ?? string.Empty; Invalidate(); }
        }

        public MenuCardControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            Margin = new Padding(12);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            _isHovering = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled)
            {
                _isHovering = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            var borderColor = !Enabled
                ? AppTheme.StepUpcomingColor
                : (_isHovering ? AppTheme.AccentColor : AppTheme.StepConnectorInactiveColor);
            var backColor = !Enabled
                ? Color.FromArgb(250, 250, 251)
                : (_isHovering ? Color.FromArgb(238, 246, 253) : Color.White);

            using (var path = RoundedRect(bounds, 10))
            {
                using (var backBrush = new SolidBrush(backColor))
                {
                    g.FillPath(backBrush, path);
                }
                using (var borderPen = new Pen(borderColor, _isHovering && Enabled ? 2F : 1F))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            var padding = 20;
            var iconRect = new Rectangle(padding, 18, IconBadgeSize, IconBadgeSize);
            var iconColor = Enabled ? AppTheme.AccentColor : Color.FromArgb(175, 181, 188);
            DrawIcon(g, iconRect, iconColor);

            var titleTop = iconRect.Bottom + 14;
            var titleColor = Enabled ? AppTheme.PrimaryColor : Color.FromArgb(150, 156, 163);
            using (var titleBrush = new SolidBrush(titleColor))
            {
                var titleRect = new RectangleF(padding, titleTop, Width - padding * 2, 42);
                g.DrawString(_title, AppTheme.HeadingFont, titleBrush, titleRect);
            }

            var descTop = titleTop + 42;
            var descColor = Enabled ? AppTheme.SecondaryColor : Color.FromArgb(175, 181, 188);
            using (var descBrush = new SolidBrush(descColor))
            {
                var descRect = new RectangleF(padding, descTop, Width - padding * 2, Math.Max(0, Height - descTop - 12));
                g.DrawString(_description, AppTheme.SmallFont, descBrush, descRect);
            }

            if (_badgeText.Length > 0)
            {
                // "Coming soon" (disabled cards) uses the warning colour; an enabled
                // card's badge (e.g. "Bonus" on Community Resources, flagging it as a
                // non-marked extra) uses the accent colour instead, so the two
                // meanings stay visually distinct.
                DrawBadge(g, Enabled ? AppTheme.AccentColor : AppTheme.WarningColor);
            }
        }

        private void DrawBadge(Graphics g, Color badgeColor)
        {
            using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                var textSize = g.MeasureString(_badgeText, font);
                var badgeWidth = textSize.Width + 18;
                var badgeHeight = 20F;
                var badgeRect = new RectangleF(Width - badgeWidth - 14, 14, badgeWidth, badgeHeight);

                using (var path = RoundedRect(Rectangle.Round(badgeRect), 10))
                {
                    using (var brush = new SolidBrush(badgeColor))
                    {
                        g.FillPath(brush, path);
                    }
                }

                using (var textBrush = new SolidBrush(Color.White))
                using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(_badgeText, font, textBrush, badgeRect, format);
                }
            }
        }

        private void DrawIcon(Graphics g, Rectangle rect, Color color)
        {
            using (var pen = new Pen(color, 2.2F) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                switch (_icon)
                {
                    case MenuCardIcon.Document:
                        DrawDocumentIcon(g, rect, pen, color);
                        break;
                    case MenuCardIcon.Calendar:
                        DrawCalendarIcon(g, rect, pen, color);
                        break;
                    case MenuCardIcon.Search:
                        DrawSearchIcon(g, rect, pen);
                        break;
                    case MenuCardIcon.Shield:
                        DrawShieldIcon(g, rect, pen, color);
                        break;
                }
            }
        }

        private static void DrawDocumentIcon(Graphics g, Rectangle rect, Pen pen, Color color)
        {
            float x = rect.X + 8;
            float y = rect.Y + 2;
            float w = rect.Width - 20;
            float h = rect.Height - 6;
            float fold = 10;

            var outline = new[]
            {
                new PointF(x, y),
                new PointF(x + w - fold, y),
                new PointF(x + w, y + fold),
                new PointF(x + w, y + h),
                new PointF(x, y + h),
                new PointF(x, y)
            };
            g.DrawLines(pen, outline);
            g.DrawLine(pen, x + w - fold, y, x + w - fold, y + fold);
            g.DrawLine(pen, x + w - fold, y + fold, x + w, y + fold);

            using (var thinPen = new Pen(color, 1.6F))
            {
                g.DrawLine(thinPen, x + 6, y + h - 20, x + w - 8, y + h - 20);
                g.DrawLine(thinPen, x + 6, y + h - 12, x + w - 14, y + h - 12);
            }

            using (var pencilPen = new Pen(color, 2.2F) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pencilPen, new PointF(x + w - 4, y + h - 2), new PointF(x + w + 8, y + h - 14));
            }
        }

        private static void DrawCalendarIcon(Graphics g, Rectangle rect, Pen pen, Color color)
        {
            float x = rect.X + 4;
            float y = rect.Y + 10;
            float w = rect.Width - 8;
            float h = rect.Height - 18;

            g.DrawRectangle(pen, x, y, w, h);
            g.DrawLine(pen, x, y + 11, x + w, y + 11);
            g.DrawLine(pen, x + w * 0.26F, y - 6, x + w * 0.26F, y + 6);
            g.DrawLine(pen, x + w * 0.74F, y - 6, x + w * 0.74F, y + 6);

            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, x + w * 0.20F, y + h * 0.48F, 5, 5);
                g.FillEllipse(brush, x + w * 0.46F, y + h * 0.48F, 5, 5);
                g.FillEllipse(brush, x + w * 0.72F, y + h * 0.48F, 5, 5);
            }
        }

        /// <summary>A simple shield with a cross, representing civic/community support services.</summary>
        private static void DrawShieldIcon(Graphics g, Rectangle rect, Pen pen, Color color)
        {
            float x = rect.X + 6;
            float y = rect.Y + 2;
            float w = rect.Width - 12;
            float h = rect.Height - 6;

            var outline = new[]
            {
                new PointF(x + w / 2F, y),
                new PointF(x + w, y + h * 0.22F),
                new PointF(x + w, y + h * 0.55F),
                new PointF(x + w / 2F, y + h),
                new PointF(x, y + h * 0.55F),
                new PointF(x, y + h * 0.22F),
                new PointF(x + w / 2F, y)
            };
            g.DrawLines(pen, outline);

            using (var crossPen = new Pen(color, 2.2F) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                var centerX = x + w / 2F;
                var centerY = y + h * 0.48F;
                g.DrawLine(crossPen, centerX, centerY - 9, centerX, centerY + 9);
                g.DrawLine(crossPen, centerX - 9, centerY, centerX + 9, centerY);
            }
        }

        private static void DrawSearchIcon(Graphics g, Rectangle rect, Pen pen)
        {
            float centerX = rect.X + rect.Width * 0.42F;
            float centerY = rect.Y + rect.Height * 0.42F;
            float radius = rect.Width * 0.28F;

            g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);

            var handleStart = new PointF(centerX + radius * 0.72F, centerY + radius * 0.72F);
            var handleEnd = new PointF(rect.Right - 4, rect.Bottom - 4);
            g.DrawLine(pen, handleStart, handleEnd);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

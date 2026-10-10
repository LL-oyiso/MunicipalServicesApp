using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// A custom-drawn card showing a single local event: a date tile, title, category
    /// badge, location and a short description. Modelled on <see cref="MenuCardControl"/>'s
    /// paint/hover pattern from Part 1, so the Local Events gallery matches the rest
    /// of the app's look and feel. Supports a "Recommended" accent variant with a
    /// plain-language reason line, for use in the recommendations panel.
    /// </summary>
    public class EventCardControl : Panel
    {
        private string _title = string.Empty;
        private string _category = string.Empty;
        private string _eventLocation = string.Empty;
        private string _description = string.Empty;
        private string _recommendationReason = string.Empty;
        private DateTime _eventDate = DateTime.Now;
        private bool _isRecommended;
        private bool _isHovering;

        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>Named EventLocation (not Location) to avoid hiding Control.Location, the control's on-screen position.</summary>
        public string EventLocation
        {
            get => _eventLocation;
            set { _eventLocation = value ?? string.Empty; Invalidate(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value ?? string.Empty; Invalidate(); }
        }

        public DateTime EventDate
        {
            get => _eventDate;
            set { _eventDate = value; Invalidate(); }
        }

        /// <summary>True to show the accent "Recommended" styling and reason line.</summary>
        public bool IsRecommended
        {
            get => _isRecommended;
            set { _isRecommended = value; Invalidate(); }
        }

        /// <summary>Plain-language reason shown at the bottom when <see cref="IsRecommended"/> is true.</summary>
        public string RecommendationReason
        {
            get => _recommendationReason;
            set { _recommendationReason = value ?? string.Empty; Invalidate(); }
        }

        public EventCardControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Cursor = Cursors.Default;
            Margin = new Padding(8);
            Size = new Size(300, 158);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;
            Invalidate();
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
            var borderColor = _isRecommended
                ? AppTheme.AccentColor
                : (_isHovering ? AppTheme.AccentColor : AppTheme.StepConnectorInactiveColor);
            var backColor = _isRecommended
                ? Color.FromArgb(238, 246, 253)
                : (_isHovering ? Color.FromArgb(248, 250, 252) : Color.White);

            using (var path = AppTheme.RoundedRect(bounds, 10))
            {
                using (var backBrush = new SolidBrush(backColor))
                {
                    g.FillPath(backBrush, path);
                }
                using (var borderPen = new Pen(borderColor, _isRecommended || _isHovering ? 2F : 1F))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            const int padding = 14;
            const int dateBadgeSize = 50;

            var dateBadgeRect = new Rectangle(padding, padding, dateBadgeSize, dateBadgeSize);
            DrawDateBadge(g, dateBadgeRect);

            if (_isRecommended)
            {
                AppTheme.DrawPill(g, "RECOMMENDED", new PointF(Width - padding - 92, padding - 4), AppTheme.AccentColor);
            }

            var textLeft = dateBadgeRect.Right + 12;
            var textWidth = Width - textLeft - padding;

            using (var titleBrush = new SolidBrush(AppTheme.PrimaryColor))
            {
                var titleRect = new RectangleF(textLeft, padding - 2, textWidth, 36);
                g.DrawString(_title, AppTheme.LabelFont, titleBrush, titleRect);
            }

            var categoryTop = padding + 24;
            AppTheme.DrawPill(g, _category, new PointF(textLeft, categoryTop), AppTheme.GetCategoryColor(_category));

            var locationTop = categoryTop + 26;
            using (var locationBrush = new SolidBrush(AppTheme.SecondaryColor))
            {
                var locationRect = new RectangleF(textLeft, locationTop, textWidth, 18);
                g.DrawString("Location: " + _eventLocation, AppTheme.SmallFont, locationBrush, locationRect);
            }

            var descTop = Math.Max(dateBadgeRect.Bottom + 10, locationTop + 20);
            var descReservedBottom = _isRecommended && _recommendationReason.Length > 0 ? 30 : 10;
            using (var descBrush = new SolidBrush(AppTheme.SecondaryColor))
            {
                var descRect = new RectangleF(padding, descTop, Width - padding * 2, Math.Max(0, Height - descTop - descReservedBottom));
                g.DrawString(_description, AppTheme.SmallFont, descBrush, descRect);
            }

            if (_isRecommended && _recommendationReason.Length > 0)
            {
                using (var reasonFont = new Font("Segoe UI", 8F, FontStyle.Italic))
                using (var reasonBrush = new SolidBrush(AppTheme.AccentColor))
                {
                    var reasonRect = new RectangleF(padding, Height - 22, Width - padding * 2, 18);
                    g.DrawString(_recommendationReason, reasonFont, reasonBrush, reasonRect);
                }
            }
        }

        /// <summary>Draws a small filled tile showing the event's day number over its abbreviated month.</summary>
        private void DrawDateBadge(Graphics g, Rectangle rect)
        {
            using (var path = AppTheme.RoundedRect(rect, 8))
            using (var backBrush = new SolidBrush(AppTheme.GetCategoryColor(_category)))
            {
                g.FillPath(backBrush, path);
            }

            using (var dayFont = new Font("Segoe UI", 15F, FontStyle.Bold))
            using (var monthFont = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                var dayRect = new RectangleF(rect.X, rect.Y + 2, rect.Width, rect.Height * 0.62F);
                g.DrawString(_eventDate.Day.ToString(), dayFont, textBrush, dayRect, format);

                var monthRect = new RectangleF(rect.X, rect.Y + rect.Height * 0.60F, rect.Width, rect.Height * 0.38F);
                g.DrawString(_eventDate.ToString("MMM").ToUpperInvariant(), monthFont, textBrush, monthRect, format);
            }
        }
    }
}

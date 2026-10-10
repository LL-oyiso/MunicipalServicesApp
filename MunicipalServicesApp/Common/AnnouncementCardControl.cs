using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// A custom-drawn card showing a single municipal announcement: title, message,
    /// category badge, posted date, and an "URGENT" badge when applicable. Modelled on
    /// <see cref="MenuCardControl"/>'s paint pattern from Part 1, consistent with
    /// <see cref="EventCardControl"/>.
    /// </summary>
    public class AnnouncementCardControl : Panel
    {
        private string _title = string.Empty;
        private string _message = string.Empty;
        private string _category = string.Empty;
        private DateTime _datePosted = DateTime.Now;
        private AnnouncementPriority _priority = AnnouncementPriority.Normal;
        private bool _isResidentSubmitted;

        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value ?? string.Empty; Invalidate(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value ?? string.Empty; Invalidate(); }
        }

        public DateTime DatePosted
        {
            get => _datePosted;
            set { _datePosted = value; Invalidate(); }
        }

        public AnnouncementPriority Priority
        {
            get => _priority;
            set { _priority = value; Invalidate(); }
        }

        /// <summary>True shows a muted "Resident request" tag in the footer, for transparency.</summary>
        public bool IsResidentSubmitted
        {
            get => _isResidentSubmitted;
            set { _isResidentSubmitted = value; Invalidate(); }
        }

        public AnnouncementCardControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Cursor = Cursors.Default;
            Margin = new Padding(8);
            Size = new Size(320, 140);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var isUrgent = _priority == AnnouncementPriority.Urgent;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            var borderColor = isUrgent ? AppTheme.DangerColor : AppTheme.StepConnectorInactiveColor;
            var backColor = isUrgent ? Color.FromArgb(253, 240, 240) : Color.White;

            using (var path = AppTheme.RoundedRect(bounds, 10))
            {
                using (var backBrush = new SolidBrush(backColor))
                {
                    g.FillPath(backBrush, path);
                }
                using (var borderPen = new Pen(borderColor, isUrgent ? 2F : 1F))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            const int padding = 14;
            var titleWidth = Width - padding * 2 - (isUrgent ? 70 : 0);

            using (var titleBrush = new SolidBrush(AppTheme.PrimaryColor))
            {
                var titleRect = new RectangleF(padding, padding - 2, titleWidth, 24);
                g.DrawString(_title, AppTheme.LabelFont, titleBrush, titleRect);
            }

            if (isUrgent)
            {
                AppTheme.DrawPill(g, "URGENT", new PointF(Width - padding - 62, padding - 2), AppTheme.DangerColor);
            }

            var categoryTop = padding + 26;
            AppTheme.DrawPill(g, _category, new PointF(padding, categoryTop), AppTheme.GetCategoryColor(_category));

            var messageTop = categoryTop + 26;
            using (var messageBrush = new SolidBrush(AppTheme.SecondaryColor))
            {
                var messageRect = new RectangleF(padding, messageTop, Width - padding * 2, Height - messageTop - 24);
                g.DrawString(_message, AppTheme.SmallFont, messageBrush, messageRect);
            }

            using (var footerBrush = new SolidBrush(Color.FromArgb(150, 156, 163)))
            using (var footerFont = new Font("Segoe UI", 8F))
            {
                var footerRect = new RectangleF(padding, Height - 20, Width - padding * 2, 16);
                var footerText = "Posted " + _datePosted.ToString("dd MMM yyyy");
                if (_isResidentSubmitted)
                {
                    footerText += "   -   Resident request";
                }
                g.DrawString(footerText, footerFont, footerBrush, footerRect);
            }
        }
    }
}

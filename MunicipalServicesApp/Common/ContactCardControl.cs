using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// A generic custom-drawn card showing a title, a coloured category/role pill, and
    /// up to three detail lines. Shared by all three "Community Resources" bonus
    /// sections (Emergency Services, Know Your Ward, Community Programs) instead of
    /// three near-identical bespoke controls, since their content all fits the same
    /// "title + badge + a few detail lines" shape. Modelled on
    /// <see cref="AnnouncementCardControl"/>'s paint pattern for visual consistency
    /// with the rest of the app.
    /// </summary>
    public class ContactCardControl : Panel
    {
        private string _title = string.Empty;
        private string _badgeText = string.Empty;
        private Color _badgeColor = AppTheme.AccentColor;
        private string _detailLine1 = string.Empty;
        private string _detailLine2 = string.Empty;
        private string _detailLine3 = string.Empty;

        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        public string BadgeText
        {
            get => _badgeText;
            set { _badgeText = value ?? string.Empty; Invalidate(); }
        }

        public Color BadgeColor
        {
            get => _badgeColor;
            set { _badgeColor = value; Invalidate(); }
        }

        public string DetailLine1
        {
            get => _detailLine1;
            set { _detailLine1 = value ?? string.Empty; Invalidate(); }
        }

        public string DetailLine2
        {
            get => _detailLine2;
            set { _detailLine2 = value ?? string.Empty; Invalidate(); }
        }

        public string DetailLine3
        {
            get => _detailLine3;
            set { _detailLine3 = value ?? string.Empty; Invalidate(); }
        }

        public ContactCardControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Cursor = Cursors.Default;
            Margin = new Padding(8);
            Size = new Size(320, 158);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var path = AppTheme.RoundedRect(bounds, 10))
            {
                using (var backBrush = new SolidBrush(Color.White))
                {
                    g.FillPath(backBrush, path);
                }
                using (var borderPen = new Pen(AppTheme.StepConnectorInactiveColor, 1F))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            const int padding = 14;
            var titleWidth = Width - padding * 2;

            // Measure the title's actual wrapped height (capped at two lines) rather
            // than assuming a fixed one-line offset, so a longer title pushes the
            // badge down instead of overlapping it.
            var singleLineHeight = AppTheme.LabelFont.GetHeight(g);
            var titleMaxHeight = singleLineHeight * 2 + 4;
            var titleSize = g.MeasureString(_title, AppTheme.LabelFont, Math.Max(1, (int)titleWidth));
            var titleHeight = Math.Min(Math.Max(titleSize.Height, singleLineHeight), titleMaxHeight);

            using (var titleBrush = new SolidBrush(AppTheme.PrimaryColor))
            {
                var titleRect = new RectangleF(padding, padding - 2, titleWidth, titleHeight);
                g.DrawString(_title, AppTheme.LabelFont, titleBrush, titleRect);
            }

            var badgeTop = padding - 2 + titleHeight + 4;
            if (_badgeText.Length > 0)
            {
                AppTheme.DrawPill(g, _badgeText, new PointF(padding, badgeTop), _badgeColor);
            }

            var detailTop = badgeTop + 26;
            using (var detailBrush = new SolidBrush(AppTheme.SecondaryColor))
            {
                DrawDetailLine(g, _detailLine1, padding, ref detailTop, titleWidth, detailBrush);
                DrawDetailLine(g, _detailLine2, padding, ref detailTop, titleWidth, detailBrush);
                DrawDetailLine(g, _detailLine3, padding, ref detailTop, titleWidth, detailBrush);
            }
        }

        /// <summary>
        /// Draws one detail line clipped to a single row with an ellipsis if it
        /// doesn't fit, rather than letting an unexpectedly long value wrap and
        /// overflow past the card's fixed height (the same category of bug fixed in
        /// EventCardControl/AnnouncementCardControl, avoided here up front since this
        /// control's seed content varies more in length across all three Community
        /// Resources sections).
        /// </summary>
        private void DrawDetailLine(Graphics g, string text, int left, ref float top, float width, Brush brush)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var rect = new RectangleF(left, top, width, 18);
            using (var format = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.LineLimit
            })
            {
                g.DrawString(text, AppTheme.SmallFont, brush, rect, format);
            }
            top += 19;
        }
    }
}

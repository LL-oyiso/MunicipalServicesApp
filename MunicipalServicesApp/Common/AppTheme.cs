using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// Shared colours, fonts and styling helpers used by every form, so the whole
    /// application keeps a single consistent look and feel.
    /// </summary>
    public static class AppTheme
    {
        public static readonly Color PrimaryColor = Color.FromArgb(0, 51, 102);
        public static readonly Color AccentColor = Color.FromArgb(0, 120, 215);
        public static readonly Color SecondaryColor = Color.FromArgb(96, 106, 117);
        public static readonly Color BackgroundColor = Color.FromArgb(245, 247, 250);
        public static readonly Color SuccessColor = Color.FromArgb(40, 140, 70);
        public static readonly Color WarningColor = Color.FromArgb(200, 130, 20);
        public static readonly Color DangerColor = Color.FromArgb(190, 40, 40);

        /// <summary>Dark teal used for the wizard's header/footer bars.</summary>
        public static readonly Color HeaderBarColor = Color.FromArgb(32, 92, 111);

        /// <summary>Light blue banner background used for instructional/info messages.</summary>
        public static readonly Color InfoBannerColor = Color.FromArgb(222, 240, 247);
        public static readonly Color InfoBannerTextColor = Color.FromArgb(20, 70, 90);

        /// <summary>Step indicator states.</summary>
        public static readonly Color StepCompletedColor = AccentColor;
        public static readonly Color StepCurrentColor = PrimaryColor;
        public static readonly Color StepUpcomingColor = Color.FromArgb(210, 214, 219);
        public static readonly Color StepConnectorActiveColor = AccentColor;
        public static readonly Color StepConnectorInactiveColor = Color.FromArgb(210, 214, 219);

        public static readonly Font TitleFont = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font HeadingFont = new Font("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font LabelFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10F);
        public static readonly Font ButtonFont = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 9F);

        /// <summary>Consistent accent colour per event/announcement category (Local Events and Announcements feature).</summary>
        private static readonly Dictionary<string, Color> CategoryColors = new Dictionary<string, Color>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Community", Color.FromArgb(0, 120, 215) },
            { "Sports and Recreation", Color.FromArgb(40, 140, 70) },
            { "Arts and Culture", Color.FromArgb(155, 89, 182) },
            { "Health and Safety", Color.FromArgb(190, 40, 40) },
            { "Municipal Notices", Color.FromArgb(200, 130, 20) },
            { "Youth and Education", Color.FromArgb(0, 150, 150) },
            { "Environment", Color.FromArgb(76, 153, 0) }
        };

        /// <summary>Looks up a category's accent colour, falling back to <see cref="AccentColor"/> for unknown ones.</summary>
        public static Color GetCategoryColor(string category)
        {
            return !string.IsNullOrWhiteSpace(category) && CategoryColors.TryGetValue(category, out var color)
                ? color
                : AccentColor;
        }

        /// <summary>Builds a rounded-rectangle path, shared by the custom-drawn card controls.</summary>
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
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

        /// <summary>
        /// Draws a small filled rounded "pill" badge (e.g. a category or priority tag)
        /// with white, bold, centred text at the given top-left location, and returns
        /// the size it occupied so callers can lay out whatever comes next.
        /// </summary>
        public static SizeF DrawPill(Graphics g, string text, PointF location, Color fillColor, float height = 20F)
        {
            using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                var textSize = g.MeasureString(text, font);
                var width = textSize.Width + 16;
                var rect = new RectangleF(location.X, location.Y, width, height);

                using (var path = RoundedRect(Rectangle.Round(rect), (int)(height / 2)))
                using (var brush = new SolidBrush(fillColor))
                {
                    g.FillPath(brush, path);
                }

                using (var textBrush = new SolidBrush(Color.White))
                using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(text, font, textBrush, rect, format);
                }

                return new SizeF(width, height);
            }
        }

        /// <summary>Applies the common background/font defaults every form should share.</summary>
        public static void ApplyFormDefaults(Form form)
        {
            form.BackColor = BackgroundColor;
            form.Font = BodyFont;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>Styles a primary/accent action button (e.g. Submit, Report Issues).</summary>
        public static void StylePrimaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = AccentColor;
            button.ForeColor = Color.White;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>Styles a secondary/neutral button (e.g. Back, Attach, Remove).</summary>
        public static void StyleSecondaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = SecondaryColor;
            button.BackColor = Color.White;
            button.ForeColor = PrimaryColor;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>Styles a disabled/coming-soon menu button so its state reads as intentional.</summary>
        public static void StyleDisabledButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.FromArgb(225, 228, 232);
            button.ForeColor = Color.FromArgb(130, 136, 143);
            button.Font = ButtonFont;
        }

        /// <summary>Styles a cancel/discard button (e.g. Cancel, back to Main Menu from mid-wizard).</summary>
        public static void StyleCancelButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = DangerColor;
            button.ForeColor = Color.White;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>Styles a light instructional banner label (info text above a step's main input).</summary>
        public static void StyleInfoBanner(Label label)
        {
            label.BackColor = InfoBannerColor;
            label.ForeColor = InfoBannerTextColor;
            label.Font = BodyFont;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Padding = new Padding(12, 0, 12, 0);
        }
    }
}

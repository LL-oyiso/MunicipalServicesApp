using System.Drawing;
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

        public static readonly Font TitleFont = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font HeadingFont = new Font("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font LabelFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10F);
        public static readonly Font ButtonFont = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 9F);

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
    }
}

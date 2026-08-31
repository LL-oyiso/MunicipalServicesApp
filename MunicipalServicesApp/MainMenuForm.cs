using System;
using System.Windows.Forms;
using MunicipalServicesApp.Common;

namespace MunicipalServicesApp
{
    /// <summary>
    /// The application's startup screen. Presents the three top-level tasks; only
    /// "Report Issues" is implemented in this phase, so the other two are visibly
    /// disabled (with an explanatory tooltip) rather than left looking broken.
    /// </summary>
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            AppTheme.ApplyFormDefaults(this);

            lblTitle.Font = AppTheme.TitleFont;
            lblTitle.ForeColor = AppTheme.PrimaryColor;

            lblSubtitle.Font = AppTheme.BodyFont;
            lblSubtitle.ForeColor = AppTheme.SecondaryColor;

            lblFooter.Font = AppTheme.SmallFont;
            lblFooter.ForeColor = AppTheme.SecondaryColor;

            AppTheme.StylePrimaryButton(btnReportIssues);
            AppTheme.StyleDisabledButton(btnLocalEvents);
            AppTheme.StyleDisabledButton(btnServiceStatus);
        }

        private void btnReportIssues_Click(object sender, EventArgs e)
        {
            try
            {
                using (var reportIssuesForm = new ReportIssuesForm())
                {
                    this.Hide();
                    reportIssuesForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Something went wrong opening the Report Issues screen:" + Environment.NewLine + ex.Message,
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Show();
            }
        }
    }
}

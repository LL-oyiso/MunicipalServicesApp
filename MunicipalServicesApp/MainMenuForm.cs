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

            pnlHeaderBar.BackColor = AppTheme.HeaderBarColor;
            lblAppTitle.Font = AppTheme.TitleFont;
            lblAppTitle.ForeColor = System.Drawing.Color.White;

            pnlContent.BackColor = AppTheme.BackgroundColor;

            lblSubtitle.Font = AppTheme.BodyFont;
            lblSubtitle.ForeColor = AppTheme.SecondaryColor;

            lblFooterNote.Font = AppTheme.SmallFont;
            lblFooterNote.ForeColor = AppTheme.SecondaryColor;
        }

        private void cardReportIssues_Click(object sender, EventArgs e)
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

        private void cardLocalEvents_Click(object sender, EventArgs e)
        {
            try
            {
                using (var localEventsForm = new LocalEventsForm())
                {
                    this.Hide();
                    localEventsForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Something went wrong opening the Local Events screen:" + Environment.NewLine + ex.Message,
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

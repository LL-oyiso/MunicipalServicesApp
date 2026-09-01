namespace MunicipalServicesApp
{
    partial class MainMenuForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeaderBar = new System.Windows.Forms.Panel();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.layoutContent = new System.Windows.Forms.TableLayoutPanel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.pnlCards = new System.Windows.Forms.TableLayoutPanel();
            this.cardReportIssues = new MunicipalServicesApp.Common.MenuCardControl();
            this.cardLocalEvents = new MunicipalServicesApp.Common.MenuCardControl();
            this.cardServiceStatus = new MunicipalServicesApp.Common.MenuCardControl();
            this.lblFooterNote = new System.Windows.Forms.Label();
            this.layoutRoot.SuspendLayout();
            this.pnlHeaderBar.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.layoutContent.SuspendLayout();
            this.pnlCards.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.pnlHeaderBar, 0, 0);
            this.layoutRoot.Controls.Add(this.pnlContent, 0, 1);
            this.layoutRoot.Name = "layoutRoot";
            //
            // pnlHeaderBar
            //
            this.pnlHeaderBar.Controls.Add(this.lblAppTitle);
            this.pnlHeaderBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeaderBar.Name = "pnlHeaderBar";
            //
            // lblAppTitle
            //
            this.lblAppTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Text = "Municipal Services App";
            this.lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlContent
            //
            this.pnlContent.Controls.Add(this.layoutContent);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";
            //
            // layoutContent
            //
            this.layoutContent.ColumnCount = 1;
            this.layoutContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutContent.Padding = new System.Windows.Forms.Padding(40, 24, 40, 20);
            this.layoutContent.RowCount = 3;
            this.layoutContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.layoutContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.layoutContent.Controls.Add(this.lblSubtitle, 0, 0);
            this.layoutContent.Controls.Add(this.pnlCards, 0, 1);
            this.layoutContent.Controls.Add(this.lblFooterNote, 0, 2);
            this.layoutContent.Name = "layoutContent";
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "How can we help you engage with your municipality today?";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlCards
            //
            this.pnlCards.ColumnCount = 3;
            this.pnlCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.pnlCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlCards.Controls.Add(this.cardReportIssues, 0, 0);
            this.pnlCards.Controls.Add(this.cardLocalEvents, 1, 0);
            this.pnlCards.Controls.Add(this.cardServiceStatus, 2, 0);
            this.pnlCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCards.Name = "pnlCards";
            this.pnlCards.RowCount = 1;
            this.pnlCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            //
            // cardReportIssues
            //
            this.cardReportIssues.Description = "Tell us about a municipal problem in your area, such as a pothole, water leak " +
    "or faulty streetlight.";
            this.cardReportIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardReportIssues.Icon = MunicipalServicesApp.Common.MenuCardIcon.Document;
            this.cardReportIssues.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardReportIssues.Name = "cardReportIssues";
            this.cardReportIssues.Title = "Report Issues";
            this.cardReportIssues.Click += new System.EventHandler(this.cardReportIssues_Click);
            //
            // cardLocalEvents
            //
            this.cardLocalEvents.BadgeText = "Coming soon";
            this.cardLocalEvents.Description = "Browse local events, notices and announcements shared by the municipality.";
            this.cardLocalEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardLocalEvents.Enabled = false;
            this.cardLocalEvents.Icon = MunicipalServicesApp.Common.MenuCardIcon.Calendar;
            this.cardLocalEvents.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.cardLocalEvents.Name = "cardLocalEvents";
            this.cardLocalEvents.Title = "Local Events and Announcements";
            this.toolTip1.SetToolTip(this.cardLocalEvents, "Coming soon in a future update.");
            //
            // cardServiceStatus
            //
            this.cardServiceStatus.BadgeText = "Coming soon";
            this.cardServiceStatus.Description = "Search for a service request you have already logged and follow its progress.";
            this.cardServiceStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardServiceStatus.Enabled = false;
            this.cardServiceStatus.Icon = MunicipalServicesApp.Common.MenuCardIcon.Search;
            this.cardServiceStatus.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.cardServiceStatus.Name = "cardServiceStatus";
            this.cardServiceStatus.Title = "Service Request Status";
            this.toolTip1.SetToolTip(this.cardServiceStatus, "Coming soon in a future update.");
            //
            // lblFooterNote
            //
            this.lblFooterNote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFooterNote.Name = "lblFooterNote";
            this.lblFooterNote.Text = "Local Events and Service Request Status will be available in a future update.";
            this.lblFooterNote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // MainMenuForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 520);
            this.Controls.Add(this.layoutRoot);
            this.MinimumSize = new System.Drawing.Size(720, 460);
            this.Name = "MainMenuForm";
            this.Text = "Municipal Services App - Main Menu";
            this.pnlHeaderBar.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.layoutContent.ResumeLayout(false);
            this.pnlCards.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel layoutRoot;
        private System.Windows.Forms.Panel pnlHeaderBar;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.TableLayoutPanel layoutContent;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.TableLayoutPanel pnlCards;
        private MunicipalServicesApp.Common.MenuCardControl cardReportIssues;
        private MunicipalServicesApp.Common.MenuCardControl cardLocalEvents;
        private MunicipalServicesApp.Common.MenuCardControl cardServiceStatus;
        private System.Windows.Forms.Label lblFooterNote;
    }
}

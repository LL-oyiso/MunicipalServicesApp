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
            this.layoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnReportIssues = new System.Windows.Forms.Button();
            this.btnLocalEvents = new System.Windows.Forms.Button();
            this.btnServiceStatus = new System.Windows.Forms.Button();
            this.lblFooter = new System.Windows.Forms.Label();
            this.layoutMain.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutMain
            //
            this.layoutMain.ColumnCount = 1;
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutMain.Padding = new System.Windows.Forms.Padding(50, 30, 50, 30);
            this.layoutMain.RowCount = 6;
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.layoutMain.Controls.Add(this.lblTitle, 0, 0);
            this.layoutMain.Controls.Add(this.lblSubtitle, 0, 1);
            this.layoutMain.Controls.Add(this.btnReportIssues, 0, 2);
            this.layoutMain.Controls.Add(this.btnLocalEvents, 0, 3);
            this.layoutMain.Controls.Add(this.btnServiceStatus, 0, 4);
            this.layoutMain.Controls.Add(this.lblFooter, 0, 5);
            this.layoutMain.Name = "layoutMain";
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Municipal Services App";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "How can we help you engage with your municipality today?";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnReportIssues
            //
            this.btnReportIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReportIssues.Margin = new System.Windows.Forms.Padding(60, 8, 60, 8);
            this.btnReportIssues.Name = "btnReportIssues";
            this.btnReportIssues.Text = "Report Issues";
            this.btnReportIssues.UseVisualStyleBackColor = false;
            this.btnReportIssues.Click += new System.EventHandler(this.btnReportIssues_Click);
            //
            // btnLocalEvents
            //
            this.btnLocalEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLocalEvents.Enabled = false;
            this.btnLocalEvents.Margin = new System.Windows.Forms.Padding(60, 8, 60, 8);
            this.btnLocalEvents.Name = "btnLocalEvents";
            this.btnLocalEvents.Text = "Local Events and Announcements";
            this.btnLocalEvents.UseVisualStyleBackColor = false;
            this.toolTip1.SetToolTip(this.btnLocalEvents, "Coming soon in a future update.");
            //
            // btnServiceStatus
            //
            this.btnServiceStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnServiceStatus.Enabled = false;
            this.btnServiceStatus.Margin = new System.Windows.Forms.Padding(60, 8, 60, 8);
            this.btnServiceStatus.Name = "btnServiceStatus";
            this.btnServiceStatus.Text = "Service Request Status";
            this.btnServiceStatus.UseVisualStyleBackColor = false;
            this.toolTip1.SetToolTip(this.btnServiceStatus, "Coming soon in a future update.");
            //
            // lblFooter
            //
            this.lblFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFooter.Name = "lblFooter";
            this.lblFooter.Text = "Local Events and Service Request Status will be available in a future update.";
            this.lblFooter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // MainMenuForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.layoutMain);
            this.MinimumSize = new System.Drawing.Size(520, 420);
            this.Name = "MainMenuForm";
            this.Text = "Municipal Services App - Main Menu";
            this.layoutMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel layoutMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Button btnReportIssues;
        private System.Windows.Forms.Button btnLocalEvents;
        private System.Windows.Forms.Button btnServiceStatus;
        private System.Windows.Forms.Label lblFooter;
    }
}

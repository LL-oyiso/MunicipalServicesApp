namespace MunicipalServicesApp
{
    partial class ReportIssuesForm
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
            this.layoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblLocation = new System.Windows.Forms.Label();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.rtbDescription = new System.Windows.Forms.RichTextBox();
            this.lblAttachments = new System.Windows.Forms.Label();
            this.pnlAttachmentButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnRemoveAttachment = new System.Windows.Forms.Button();
            this.lstAttachments = new System.Windows.Forms.ListBox();
            this.pnlProgress = new System.Windows.Forms.TableLayoutPanel();
            this.progressReporting = new System.Windows.Forms.ProgressBar();
            this.lblEngagement = new System.Windows.Forms.Label();
            this.pnlActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.layoutMain.SuspendLayout();
            this.pnlAttachmentButtons.SuspendLayout();
            this.pnlProgress.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutMain
            //
            this.layoutMain.ColumnCount = 1;
            this.layoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutMain.Padding = new System.Windows.Forms.Padding(24, 16, 24, 16);
            this.layoutMain.RowCount = 12;
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.layoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.layoutMain.Controls.Add(this.lblHeader, 0, 0);
            this.layoutMain.Controls.Add(this.lblLocation, 0, 1);
            this.layoutMain.Controls.Add(this.txtLocation, 0, 2);
            this.layoutMain.Controls.Add(this.lblCategory, 0, 3);
            this.layoutMain.Controls.Add(this.cmbCategory, 0, 4);
            this.layoutMain.Controls.Add(this.lblDescription, 0, 5);
            this.layoutMain.Controls.Add(this.rtbDescription, 0, 6);
            this.layoutMain.Controls.Add(this.lblAttachments, 0, 7);
            this.layoutMain.Controls.Add(this.pnlAttachmentButtons, 0, 8);
            this.layoutMain.Controls.Add(this.lstAttachments, 0, 9);
            this.layoutMain.Controls.Add(this.pnlProgress, 0, 10);
            this.layoutMain.Controls.Add(this.pnlActions, 0, 11);
            this.layoutMain.Name = "layoutMain";
            //
            // lblHeader
            //
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Text = "Report an Issue";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblLocation
            //
            this.lblLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Text = "Location *";
            this.lblLocation.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // txtLocation
            //
            this.txtLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLocation.MaxLength = 150;
            this.txtLocation.Name = "txtLocation";
            this.toolTip1.SetToolTip(this.txtLocation, "e.g. street address, suburb, or nearest landmark.");
            this.txtLocation.TextChanged += new System.EventHandler(this.InputChanged);
            //
            // lblCategory
            //
            this.lblCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Text = "Category *";
            this.lblCategory.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // cmbCategory
            //
            this.cmbCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Items.AddRange(new object[] {
            "Sanitation",
            "Roads",
            "Utilities",
            "Electricity",
            "Water",
            "Other"});
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.InputChanged);
            //
            // lblDescription
            //
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Text = "Description *";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // rtbDescription
            //
            this.rtbDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbDescription.Name = "rtbDescription";
            this.toolTip1.SetToolTip(this.rtbDescription, "Describe the issue in as much detail as possible.");
            this.rtbDescription.TextChanged += new System.EventHandler(this.InputChanged);
            //
            // lblAttachments
            //
            this.lblAttachments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAttachments.Name = "lblAttachments";
            this.lblAttachments.Text = "Attachments (optional)";
            this.lblAttachments.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // pnlAttachmentButtons
            //
            this.pnlAttachmentButtons.Controls.Add(this.btnAttach);
            this.pnlAttachmentButtons.Controls.Add(this.btnRemoveAttachment);
            this.pnlAttachmentButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAttachmentButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlAttachmentButtons.Name = "pnlAttachmentButtons";
            this.pnlAttachmentButtons.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.pnlAttachmentButtons.WrapContents = false;
            //
            // btnAttach
            //
            this.btnAttach.AutoSize = false;
            this.btnAttach.Height = 32;
            this.btnAttach.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Text = "Add Attachment";
            this.btnAttach.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnAttach.UseVisualStyleBackColor = false;
            this.btnAttach.Width = 150;
            this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);
            //
            // btnRemoveAttachment
            //
            this.btnRemoveAttachment.AutoSize = false;
            this.btnRemoveAttachment.Height = 32;
            this.btnRemoveAttachment.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.btnRemoveAttachment.Name = "btnRemoveAttachment";
            this.btnRemoveAttachment.Text = "Remove Selected";
            this.btnRemoveAttachment.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnRemoveAttachment.UseVisualStyleBackColor = false;
            this.btnRemoveAttachment.Width = 140;
            this.btnRemoveAttachment.Click += new System.EventHandler(this.btnRemoveAttachment_Click);
            //
            // lstAttachments
            //
            this.lstAttachments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstAttachments.Name = "lstAttachments";
            this.lstAttachments.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.toolTip1.SetToolTip(this.lstAttachments, "Files attached to this report. Select one or more and click Remove Selected to undo.");
            //
            // pnlProgress
            //
            this.pnlProgress.ColumnCount = 1;
            this.pnlProgress.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProgress.RowCount = 2;
            this.pnlProgress.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.pnlProgress.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlProgress.Controls.Add(this.progressReporting, 0, 0);
            this.pnlProgress.Controls.Add(this.lblEngagement, 0, 1);
            this.pnlProgress.Name = "pnlProgress";
            //
            // progressReporting
            //
            this.progressReporting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressReporting.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.progressReporting.Maximum = 100;
            this.progressReporting.Name = "progressReporting";
            //
            // lblEngagement
            //
            this.lblEngagement.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEngagement.Name = "lblEngagement";
            this.lblEngagement.Text = "Let\'s get started - tell us where the issue is.";
            this.lblEngagement.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlActions
            //
            this.pnlActions.Controls.Add(this.btnSubmit);
            this.pnlActions.Controls.Add(this.btnBack);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            //
            // btnSubmit
            //
            this.btnSubmit.Height = 38;
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Width = 140;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            //
            // btnBack
            //
            this.btnBack.Height = 38;
            this.btnBack.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Text = "Back to Main Menu";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Width = 150;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            //
            // ReportIssuesForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 640);
            this.Controls.Add(this.layoutMain);
            this.MinimumSize = new System.Drawing.Size(560, 560);
            this.Name = "ReportIssuesForm";
            this.Text = "Municipal Services App - Report Issues";
            this.pnlAttachmentButtons.ResumeLayout(false);
            this.pnlProgress.ResumeLayout(false);
            this.pnlActions.ResumeLayout(false);
            this.layoutMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel layoutMain;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblLocation;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.RichTextBox rtbDescription;
        private System.Windows.Forms.Label lblAttachments;
        private System.Windows.Forms.FlowLayoutPanel pnlAttachmentButtons;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.Button btnRemoveAttachment;
        private System.Windows.Forms.ListBox lstAttachments;
        private System.Windows.Forms.TableLayoutPanel pnlProgress;
        private System.Windows.Forms.ProgressBar progressReporting;
        private System.Windows.Forms.Label lblEngagement;
        private System.Windows.Forms.FlowLayoutPanel pnlActions;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Button btnBack;
    }
}

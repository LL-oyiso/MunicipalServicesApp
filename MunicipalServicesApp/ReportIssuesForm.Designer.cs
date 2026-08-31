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
        /// Required method for Designer support - do not modify the contents of this
        /// method with the code editor. This builds only the wizard's persistent
        /// "chrome" (header bar, step content container, footer bar) - the contents of
        /// each individual step are built programmatically in ReportIssuesForm.cs,
        /// since the Summary/Confirmation steps must be generated dynamically anyway.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeaderBar = new System.Windows.Forms.Panel();
            this.stepIndicator = new MunicipalServicesApp.Common.StepIndicatorControl();
            this.lblWizardTitle = new System.Windows.Forms.Label();
            this.pnlStepContent = new System.Windows.Forms.Panel();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.flpFooterRight = new System.Windows.Forms.FlowLayoutPanel();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnCloseWizard = new System.Windows.Forms.Button();
            this.btnSubmitAnother = new System.Windows.Forms.Button();
            this.flpFooterLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.layoutRoot.SuspendLayout();
            this.pnlHeaderBar.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.flpFooterRight.SuspendLayout();
            this.flpFooterLeft.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.RowCount = 3;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.layoutRoot.Controls.Add(this.pnlHeaderBar, 0, 0);
            this.layoutRoot.Controls.Add(this.pnlStepContent, 0, 1);
            this.layoutRoot.Controls.Add(this.pnlFooter, 0, 2);
            this.layoutRoot.Name = "layoutRoot";
            //
            // pnlHeaderBar
            //
            this.pnlHeaderBar.Controls.Add(this.stepIndicator);
            this.pnlHeaderBar.Controls.Add(this.lblWizardTitle);
            this.pnlHeaderBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeaderBar.Name = "pnlHeaderBar";
            //
            // stepIndicator
            //
            this.stepIndicator.CurrentStepIndex = 0;
            this.stepIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stepIndicator.Name = "stepIndicator";
            this.stepIndicator.StepLabels = new string[0];
            //
            // lblWizardTitle
            //
            this.lblWizardTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWizardTitle.Height = 34;
            this.lblWizardTitle.Name = "lblWizardTitle";
            this.lblWizardTitle.Text = "Report an Issue";
            this.lblWizardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlStepContent
            //
            this.pnlStepContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStepContent.Name = "pnlStepContent";
            //
            // pnlFooter
            //
            this.pnlFooter.Controls.Add(this.flpFooterRight);
            this.pnlFooter.Controls.Add(this.flpFooterLeft);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFooter.Name = "pnlFooter";
            //
            // flpFooterRight
            //
            this.flpFooterRight.Controls.Add(this.btnNext);
            this.flpFooterRight.Controls.Add(this.btnBack);
            this.flpFooterRight.Controls.Add(this.btnCloseWizard);
            this.flpFooterRight.Controls.Add(this.btnSubmitAnother);
            this.flpFooterRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flpFooterRight.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpFooterRight.Name = "flpFooterRight";
            this.flpFooterRight.Padding = new System.Windows.Forms.Padding(0, 14, 16, 14);
            this.flpFooterRight.WrapContents = false;
            //
            // btnNext
            //
            this.btnNext.Height = 36;
            this.btnNext.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Width = 110;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            //
            // btnBack
            //
            this.btnBack.Height = 36;
            this.btnBack.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Text = "Back";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Width = 100;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            //
            // btnCloseWizard
            //
            this.btnCloseWizard.Height = 36;
            this.btnCloseWizard.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnCloseWizard.Name = "btnCloseWizard";
            this.btnCloseWizard.Text = "Close";
            this.btnCloseWizard.UseVisualStyleBackColor = false;
            this.btnCloseWizard.Visible = false;
            this.btnCloseWizard.Width = 100;
            this.btnCloseWizard.Click += new System.EventHandler(this.btnCloseWizard_Click);
            //
            // btnSubmitAnother
            //
            this.btnSubmitAnother.Height = 36;
            this.btnSubmitAnother.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btnSubmitAnother.Name = "btnSubmitAnother";
            this.btnSubmitAnother.Text = "New Report";
            this.btnSubmitAnother.UseVisualStyleBackColor = false;
            this.btnSubmitAnother.Visible = false;
            this.btnSubmitAnother.Width = 120;
            this.btnSubmitAnother.Click += new System.EventHandler(this.btnSubmitAnother_Click);
            //
            // flpFooterLeft
            //
            this.flpFooterLeft.Controls.Add(this.btnCancel);
            this.flpFooterLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpFooterLeft.Name = "flpFooterLeft";
            this.flpFooterLeft.Padding = new System.Windows.Forms.Padding(16, 14, 0, 14);
            //
            // btnCancel
            //
            this.btnCancel.Height = 36;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Width = 100;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // ReportIssuesForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 680);
            this.Controls.Add(this.layoutRoot);
            this.MinimumSize = new System.Drawing.Size(680, 560);
            this.Name = "ReportIssuesForm";
            this.Text = "Municipal Services App - Report Issues";
            this.pnlHeaderBar.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.flpFooterRight.ResumeLayout(false);
            this.flpFooterLeft.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layoutRoot;
        private System.Windows.Forms.Panel pnlHeaderBar;
        private MunicipalServicesApp.Common.StepIndicatorControl stepIndicator;
        private System.Windows.Forms.Label lblWizardTitle;
        private System.Windows.Forms.Panel pnlStepContent;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.FlowLayoutPanel flpFooterRight;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnCloseWizard;
        private System.Windows.Forms.Button btnSubmitAnother;
        private System.Windows.Forms.FlowLayoutPanel flpFooterLeft;
        private System.Windows.Forms.Button btnCancel;
    }
}

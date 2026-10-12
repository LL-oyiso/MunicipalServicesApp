namespace MunicipalServicesApp
{
    partial class CommunityResourcesForm
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
        /// method with the code editor. This builds only the form's persistent chrome
        /// (header bar with title and Back to Main Menu button, plus the content
        /// container panel) - everything inside pnlContent (tab strip, filter bars and
        /// galleries for all three Community Resources sections) is built
        /// programmatically in CommunityResourcesForm.cs, the same approach used by
        /// LocalEventsForm.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeaderBar = new System.Windows.Forms.Panel();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.flpHeaderRight = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBackToMenu = new System.Windows.Forms.Button();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.layoutRoot.SuspendLayout();
            this.pnlHeaderBar.SuspendLayout();
            this.flpHeaderRight.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.pnlHeaderBar, 0, 0);
            this.layoutRoot.Controls.Add(this.pnlContent, 0, 1);
            this.layoutRoot.Name = "layoutRoot";
            //
            // pnlHeaderBar
            //
            this.pnlHeaderBar.Controls.Add(this.lblFormTitle);
            this.pnlHeaderBar.Controls.Add(this.flpHeaderRight);
            this.pnlHeaderBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeaderBar.Name = "pnlHeaderBar";
            //
            // lblFormTitle
            //
            this.lblFormTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.lblFormTitle.Text = "Community Resources";
            this.lblFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // flpHeaderRight
            //
            this.flpHeaderRight.AutoSize = true;
            this.flpHeaderRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpHeaderRight.Controls.Add(this.btnBackToMenu);
            this.flpHeaderRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flpHeaderRight.Name = "flpHeaderRight";
            this.flpHeaderRight.Padding = new System.Windows.Forms.Padding(0, 18, 24, 18);
            this.flpHeaderRight.WrapContents = false;
            //
            // btnBackToMenu
            //
            this.btnBackToMenu.Height = 36;
            this.btnBackToMenu.Name = "btnBackToMenu";
            this.btnBackToMenu.Text = "Back to Main Menu";
            this.btnBackToMenu.UseVisualStyleBackColor = false;
            this.btnBackToMenu.Width = 160;
            this.btnBackToMenu.Click += new System.EventHandler(this.btnBackToMenu_Click);
            //
            // pnlContent
            //
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";
            //
            // CommunityResourcesForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.layoutRoot);
            this.MinimumSize = new System.Drawing.Size(860, 600);
            this.Name = "CommunityResourcesForm";
            this.Text = "Municipal Services App - Community Resources";
            this.pnlHeaderBar.ResumeLayout(false);
            this.flpHeaderRight.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layoutRoot;
        private System.Windows.Forms.Panel pnlHeaderBar;
        private System.Windows.Forms.Label lblFormTitle;
        private System.Windows.Forms.FlowLayoutPanel flpHeaderRight;
        private System.Windows.Forms.Button btnBackToMenu;
        private System.Windows.Forms.Panel pnlContent;
    }
}

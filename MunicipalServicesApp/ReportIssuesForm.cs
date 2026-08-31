using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Lets a resident report a municipal issue: location, category, description and
    /// optional attachments. A progress bar plus a dynamic message give continuous
    /// feedback as the form is filled in - the concrete engagement feature backing the
    /// Task 1 "Participatory Co-Design" strategy: residents can see, in real time, that
    /// their input is being captured and valued.
    /// </summary>
    public partial class ReportIssuesForm : Form
    {
        private readonly System.Collections.Generic.List<string> _attachedFiles = new System.Collections.Generic.List<string>();

        public ReportIssuesForm()
        {
            InitializeComponent();
            ApplyTheme();
            UpdateProgress();
        }

        private void ApplyTheme()
        {
            AppTheme.ApplyFormDefaults(this);

            lblHeader.Font = AppTheme.HeadingFont;
            lblHeader.ForeColor = AppTheme.PrimaryColor;

            foreach (var label in new[] { lblLocation, lblCategory, lblDescription, lblAttachments })
            {
                label.Font = AppTheme.LabelFont;
                label.ForeColor = AppTheme.PrimaryColor;
            }

            AppTheme.StyleSecondaryButton(btnAttach);
            AppTheme.StyleSecondaryButton(btnRemoveAttachment);
            AppTheme.StyleSecondaryButton(btnBack);
            AppTheme.StylePrimaryButton(btnSubmit);
        }

        /// <summary>Shared handler for every input control that affects reporting progress.</summary>
        private void InputChanged(object sender, EventArgs e)
        {
            UpdateProgress();
        }

        private void btnAttach_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Attach images or documents related to the issue";
                    dialog.Filter =
                        "Supported files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.pdf;*.doc;*.docx;*.txt|" +
                        "All files (*.*)|*.*";
                    dialog.Multiselect = true;

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        var addedCount = 0;
                        foreach (var fileName in dialog.FileNames)
                        {
                            if (!_attachedFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                            {
                                _attachedFiles.Add(fileName);
                                lstAttachments.Items.Add(Path.GetFileName(fileName));
                                addedCount++;
                            }
                        }

                        if (addedCount == 0 && dialog.FileNames.Length > 0)
                        {
                            MessageBox.Show(
                                "The selected file(s) are already attached to this report.",
                                "Municipal Services App",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }

                        UpdateProgress();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "We couldn't attach that file:" + Environment.NewLine + ex.Message,
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnRemoveAttachment_Click(object sender, EventArgs e)
        {
            if (lstAttachments.SelectedIndices.Count == 0)
            {
                MessageBox.Show(
                    "Select one or more attachments in the list first, then click Remove Selected.",
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var selectedIndices = lstAttachments.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            foreach (var index in selectedIndices)
            {
                _attachedFiles.RemoveAt(index);
                lstAttachments.Items.RemoveAt(index);
            }

            UpdateProgress();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                var location = txtLocation.Text.Trim();
                var description = rtbDescription.Text.Trim();

                if (string.IsNullOrWhiteSpace(location))
                {
                    ShowValidationError("Please enter the location of the issue.", txtLocation);
                    return;
                }

                if (cmbCategory.SelectedItem == null)
                {
                    ShowValidationError("Please select a category for the issue.", cmbCategory);
                    return;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    ShowValidationError("Please provide a description of the issue.", rtbDescription);
                    return;
                }

                var issue = new Issue
                {
                    Location = location,
                    Category = cmbCategory.SelectedItem.ToString(),
                    Description = description
                };
                issue.AttachmentPaths.AddRange(_attachedFiles);

                IssueRepository.AddIssue(issue);

                MessageBox.Show(
                    $"Thank you - your issue has been reported successfully!{Environment.NewLine}{Environment.NewLine}" +
                    $"Your reference number is {issue.ReferenceNumber}. Please keep this for your records - " +
                    "you can quote it if you follow up on this report. Every report helps the municipality " +
                    "understand and respond to what residents need.",
                    "Report Submitted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Something went wrong while submitting your report:" + Environment.NewLine + ex.Message,
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowValidationError(string message, Control controlToFocus)
        {
            MessageBox.Show(message, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            controlToFocus.Focus();
        }

        /// <summary>
        /// Recalculates reporting progress (25% each for Location, Category, Description
        /// and at least one attachment) and updates the progress bar plus the dynamic
        /// encouraging message shown to the user.
        /// </summary>
        private void UpdateProgress()
        {
            var percent = 0;
            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) percent += 25;
            if (cmbCategory.SelectedItem != null) percent += 25;
            if (!string.IsNullOrWhiteSpace(rtbDescription.Text)) percent += 25;
            if (_attachedFiles.Count > 0) percent += 25;

            progressReporting.Value = percent;

            string message;
            System.Drawing.Color color;

            switch (percent)
            {
                case 0:
                    message = "Let's get started - tell us where the issue is.";
                    color = AppTheme.SecondaryColor;
                    break;
                case 25:
                    message = "Great start! Now pick a category for the issue.";
                    color = AppTheme.WarningColor;
                    break;
                case 50:
                    message = "You're halfway there - now describe what's happening.";
                    color = AppTheme.WarningColor;
                    break;
                case 75:
                    message = "Almost done! Add a photo or document if you have one, or submit now.";
                    color = AppTheme.AccentColor;
                    break;
                default:
                    message = "All set - thank you for helping your community. Click Submit!";
                    color = AppTheme.SuccessColor;
                    break;
            }

            lblEngagement.Text = message;
            lblEngagement.ForeColor = color;
        }

        private void ResetForm()
        {
            txtLocation.Clear();
            cmbCategory.SelectedIndex = -1;
            rtbDescription.Clear();
            _attachedFiles.Clear();
            lstAttachments.Items.Clear();
            UpdateProgress();
            txtLocation.Focus();
        }
    }
}

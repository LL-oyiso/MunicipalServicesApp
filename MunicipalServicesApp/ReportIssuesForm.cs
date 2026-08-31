using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Guides a resident through reporting a municipal issue as a multi-step wizard:
    /// Category, Description, Location, Attachment, Contact Details, Summary and a
    /// closing Confirmation screen. The step indicator in the header bar is the
    /// dynamic engagement feature backing the Task 1 "Participatory Co-Design"
    /// strategy - residents can always see how far through the process they are and
    /// how much is left, so the form never feels like an anonymous black box.
    /// </summary>
    public partial class ReportIssuesForm : Form
    {
        private const int StepCategory = 0;
        private const int StepDescription = 1;
        private const int StepLocation = 2;
        private const int StepAttachment = 3;
        private const int StepContact = 4;
        private const int StepSummary = 5;
        private const int StepConfirmation = 6;
        private const int StepCount = 7;

        private const int MinimumDescriptionLength = 10;

        /// <summary>
        /// Labels for the six chips shown in the header's step indicator. Confirmation
        /// is deliberately absent - it is an end screen rather than a step the user
        /// fills in, so once it is reached the indicator shows every chip completed.
        /// </summary>
        private static readonly string[] WizardStepLabels =
        {
            "Category", "Description", "Location", "Attachment", "Contact", "Summary"
        };

        /// <summary>Encouraging footer message shown for each step, indexed by step number.</summary>
        private static readonly string[] FooterMessages =
        {
            "Step 1 of 6 - tell us what kind of issue this is.",
            "Step 2 of 6 - detail helps us respond faster.",
            "Step 3 of 6 - where can we find the problem?",
            "Step 4 of 6 - photos help, but are optional.",
            "Step 5 of 6 - how should we update you?",
            "Step 6 of 6 - check it over, then submit.",
            "Thank you - your report has been logged."
        };

        private static readonly string[] CategoryOptions =
        {
            "Sanitation", "Roads", "Utilities", "Electricity", "Water", "Other"
        };

        private readonly List<string> _attachedFiles = new List<string>();
        private readonly Panel[] _stepPanels = new Panel[StepCount];

        private int _currentStep;
        private Issue _submittedIssue;

        private Label lblFooterMessage;

        private ComboBox cmbCategory;
        private RichTextBox rtbDescription;
        private Label lblDescriptionCount;
        private TextBox txtLocation;
        private TextBox txtAdditionalLocation;
        private ListBox lstAttachments;
        private Button btnAttach;
        private Button btnRemoveAttachment;
        private Label lblAttachmentHint;
        private TextBox txtFirstName;
        private TextBox txtSurname;
        private TextBox txtEmail;
        private TextBox txtMobile;
        private Label lblEmailField;
        private Label lblMobileField;
        private RadioButton rbEmail;
        private RadioButton rbMobile;
        private Label lblSummaryPlaceholder;
        private Label lblConfirmationDetail;

        public ReportIssuesForm()
        {
            InitializeComponent();
            ApplyChromeTheme();
            BuildStepPanels();
            GoToStep(StepCategory);
        }

        #region Chrome and layout helpers

        /// <summary>
        /// Themes the persistent wizard chrome built by the designer (header bar,
        /// content area, footer) and adds the footer's per-step message label.
        /// </summary>
        private void ApplyChromeTheme()
        {
            AppTheme.ApplyFormDefaults(this);

            pnlHeaderBar.BackColor = AppTheme.HeaderBarColor;
            lblWizardTitle.BackColor = AppTheme.HeaderBarColor;
            lblWizardTitle.ForeColor = Color.White;
            lblWizardTitle.Font = AppTheme.HeadingFont;

            stepIndicator.StepLabels = WizardStepLabels;

            pnlStepContent.BackColor = AppTheme.BackgroundColor;

            pnlFooter.BackColor = AppTheme.HeaderBarColor;
            flpFooterLeft.BackColor = AppTheme.HeaderBarColor;
            flpFooterRight.BackColor = AppTheme.HeaderBarColor;

            AppTheme.StyleCancelButton(btnCancel);
            AppTheme.StyleSecondaryButton(btnBack);
            AppTheme.StylePrimaryButton(btnNext);
            AppTheme.StyleSecondaryButton(btnCloseWizard);
            AppTheme.StylePrimaryButton(btnSubmitAnother);

            lblFooterMessage = new Label
            {
                AutoSize = false,
                BackColor = AppTheme.HeaderBarColor,
                Dock = DockStyle.Fill,
                Font = AppTheme.SmallFont,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlFooter.Controls.Add(lblFooterMessage);

            // Docked siblings are laid out in reverse z-order, so the Fill control has
            // to be front-most to receive only the space the left/right button strips
            // leave behind rather than the whole footer.
            lblFooterMessage.BringToFront();
        }

        /// <summary>
        /// Creates the standard frame every step shares - a heading, an instructional
        /// info banner and an empty body table the caller fills in - so all seven steps
        /// line up identically no matter what controls they hold.
        /// </summary>
        private static Panel CreateStepShell(string heading, string bannerText, out TableLayoutPanel body)
        {
            var panel = new Panel
            {
                BackColor = AppTheme.BackgroundColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 18, 28, 18),
                Visible = false
            };

            var layout = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 3
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblHeading = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor,
                Text = heading,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblBanner = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 16),
                Text = bannerText
            };
            AppTheme.StyleInfoBanner(lblBanner);

            body = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 0
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            layout.Controls.Add(lblHeading, 0, 0);
            layout.Controls.Add(lblBanner, 0, 1);
            layout.Controls.Add(body, 0, 2);

            panel.Controls.Add(layout);
            return panel;
        }

        private static Label CreateFieldLabel(string text)
        {
            return new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.LabelFont,
                ForeColor = AppTheme.PrimaryColor,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Label CreateHintLabel(string text)
        {
            return new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static void AddRow(TableLayoutPanel body, float absoluteHeight)
        {
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, absoluteHeight));
            body.RowCount = body.RowStyles.Count;
        }

        private static void AddFillRow(TableLayoutPanel body)
        {
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            body.RowCount = body.RowStyles.Count;
        }

        #endregion

        #region Step construction

        private void BuildStepPanels()
        {
            _stepPanels[StepCategory] = BuildCategoryStep();
            _stepPanels[StepDescription] = BuildDescriptionStep();
            _stepPanels[StepLocation] = BuildLocationStep();
            _stepPanels[StepAttachment] = BuildAttachmentStep();
            _stepPanels[StepContact] = BuildContactStep();
            _stepPanels[StepSummary] = BuildSummaryStep();
            _stepPanels[StepConfirmation] = BuildConfirmationStep();

            foreach (var panel in _stepPanels)
            {
                pnlStepContent.Controls.Add(panel);
            }
        }

        private Panel BuildCategoryStep()
        {
            TableLayoutPanel body;
            var panel = CreateStepShell(
                "What kind of issue are you reporting?",
                "Picking the right category sends your report straight to the team that handles it.",
                out body);

            cmbCategory = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.BodyFont
            };
            cmbCategory.Items.AddRange(CategoryOptions);

            AddRow(body, 26F);
            AddRow(body, 34F);
            AddRow(body, 26F);
            AddFillRow(body);

            body.Controls.Add(CreateFieldLabel("Category *"), 0, 0);
            body.Controls.Add(cmbCategory, 0, 1);
            body.Controls.Add(CreateHintLabel("Not sure which one fits? Choose \"Other\" and describe it on the next step."), 0, 2);

            return panel;
        }

        private Panel BuildDescriptionStep()
        {
            TableLayoutPanel body;
            var panel = CreateStepShell(
                "Describe the issue",
                "Tell us what you can see and how long it has been like this. Specifics help our teams arrive prepared.",
                out body);

            rtbDescription = new RichTextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont
            };
            rtbDescription.TextChanged += rtbDescription_TextChanged;

            lblDescriptionCount = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor,
                TextAlign = ContentAlignment.MiddleRight
            };

            AddRow(body, 26F);
            AddFillRow(body);
            AddRow(body, 24F);

            body.Controls.Add(CreateFieldLabel("Description *"), 0, 0);
            body.Controls.Add(rtbDescription, 0, 1);
            body.Controls.Add(lblDescriptionCount, 0, 2);

            UpdateDescriptionCount();

            return panel;
        }

        private Panel BuildLocationStep()
        {
            TableLayoutPanel body;
            var panel = CreateStepShell(
                "Where is the issue?",
                "A street name and the nearest house or block number is usually enough for our teams to find it.",
                out body);

            txtLocation = new TextBox
            {
                Dock = DockStyle.Top,
                Font = AppTheme.BodyFont
            };

            txtAdditionalLocation = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            AddRow(body, 26F);
            AddRow(body, 32F);
            AddRow(body, 26F);
            AddRow(body, 26F);
            AddRow(body, 84F);
            AddFillRow(body);

            body.Controls.Add(CreateFieldLabel("Street address or location *"), 0, 0);
            body.Controls.Add(txtLocation, 0, 1);
            body.Controls.Add(CreateHintLabel("For example: 42 Long Street, opposite the clinic."), 0, 2);
            body.Controls.Add(CreateFieldLabel("Additional location information"), 0, 3);
            body.Controls.Add(txtAdditionalLocation, 0, 4);

            return panel;
        }

        private Panel BuildAttachmentStep()
        {
            TableLayoutPanel body;
            var panel = CreateStepShell(
                "Add photos or documents",
                "Attachments are optional, but a photo of the problem often gets it resolved faster.",
                out body);

            btnAttach = new Button
            {
                Height = 32,
                Margin = new Padding(0, 0, 8, 0),
                Text = "Attach Files",
                Width = 140
            };
            btnAttach.Click += btnAttach_Click;
            AppTheme.StyleSecondaryButton(btnAttach);

            btnRemoveAttachment = new Button
            {
                Height = 32,
                Margin = new Padding(0, 0, 0, 0),
                Text = "Remove",
                Width = 120
            };
            btnRemoveAttachment.Click += btnRemoveAttachment_Click;
            AppTheme.StyleSecondaryButton(btnRemoveAttachment);

            var flpAttachmentButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0),
                WrapContents = false
            };
            flpAttachmentButtons.Controls.Add(btnAttach);
            flpAttachmentButtons.Controls.Add(btnRemoveAttachment);

            lstAttachments = new ListBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                SelectionMode = SelectionMode.MultiExtended
            };

            lblAttachmentHint = CreateHintLabel(string.Empty);

            AddRow(body, 40F);
            AddRow(body, 26F);
            AddFillRow(body);
            AddRow(body, 24F);

            body.Controls.Add(flpAttachmentButtons, 0, 0);
            body.Controls.Add(CreateFieldLabel("Attached files"), 0, 1);
            body.Controls.Add(lstAttachments, 0, 2);
            body.Controls.Add(lblAttachmentHint, 0, 3);

            UpdateAttachmentHint();

            return panel;
        }

        private Panel BuildContactStep()
        {
            TableLayoutPanel body;
            var panel = CreateStepShell(
                "Your contact details",
                "We use these details only to keep you updated on this report.",
                out body);

            var grid = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                RowCount = 6
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (var i = 0; i < 5; i++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            }
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtFirstName = CreateContactTextBox();
            txtSurname = CreateContactTextBox();
            txtEmail = CreateContactTextBox();
            txtMobile = CreateContactTextBox();

            lblEmailField = CreateFieldLabel("Email address");
            lblMobileField = CreateFieldLabel("Mobile number *");

            rbEmail = new RadioButton
            {
                AutoSize = true,
                Font = AppTheme.BodyFont,
                ForeColor = AppTheme.PrimaryColor,
                Margin = new Padding(0, 4, 24, 0),
                Text = "Email"
            };
            rbMobile = new RadioButton
            {
                AutoSize = true,
                Checked = true,
                Font = AppTheme.BodyFont,
                ForeColor = AppTheme.PrimaryColor,
                Margin = new Padding(0, 4, 0, 0),
                Text = "Mobile (SMS)"
            };
            rbEmail.CheckedChanged += PreferredContactMethodChanged;

            var flpMethods = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(0),
                WrapContents = false
            };
            flpMethods.Controls.Add(rbMobile);
            flpMethods.Controls.Add(rbEmail);

            grid.Controls.Add(CreateFieldLabel("First name *"), 0, 0);
            grid.Controls.Add(txtFirstName, 1, 0);
            grid.Controls.Add(CreateFieldLabel("Surname *"), 0, 1);
            grid.Controls.Add(txtSurname, 1, 1);
            grid.Controls.Add(lblEmailField, 0, 2);
            grid.Controls.Add(txtEmail, 1, 2);
            grid.Controls.Add(lblMobileField, 0, 3);
            grid.Controls.Add(txtMobile, 1, 3);
            grid.Controls.Add(CreateFieldLabel("Keep me updated by *"), 0, 4);
            grid.Controls.Add(flpMethods, 1, 4);

            AddFillRow(body);
            AddRow(body, 24F);

            body.Controls.Add(grid, 0, 0);
            body.Controls.Add(CreateHintLabel("Only the method you choose above is required - the other is optional."), 0, 1);

            return panel;
        }

        private static TextBox CreateContactTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Top,
                Font = AppTheme.BodyFont,
                Margin = new Padding(0, 4, 0, 4)
            };
        }

        /// <summary>
        /// Moves the required-field marker onto whichever contact field the resident's
        /// chosen feedback method actually needs, so the form asks for the minimum.
        /// </summary>
        private void PreferredContactMethodChanged(object sender, EventArgs e)
        {
            lblEmailField.Text = rbEmail.Checked ? "Email address *" : "Email address";
            lblMobileField.Text = rbEmail.Checked ? "Mobile number" : "Mobile number *";
        }

        private Panel BuildSummaryStep()
        {
            TableLayoutPanel body;
            var panel = CreateStepShell(
                "Check your report",
                "Please review everything below. You can go back and change any part before submitting.",
                out body);

            lblSummaryPlaceholder = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                ForeColor = AppTheme.PrimaryColor,
                TextAlign = ContentAlignment.TopLeft
            };

            AddFillRow(body);
            body.Controls.Add(lblSummaryPlaceholder, 0, 0);

            return panel;
        }

        private Panel BuildConfirmationStep()
        {
            TableLayoutPanel body;
            var panel = CreateStepShell(
                "Report submitted",
                "Thank you for helping your community. Please keep your reference number for any follow-up.",
                out body);

            lblConfirmationDetail = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                ForeColor = AppTheme.PrimaryColor,
                TextAlign = ContentAlignment.TopLeft
            };

            AddFillRow(body);
            body.Controls.Add(lblConfirmationDetail, 0, 0);

            return panel;
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Shows a single step, hides the rest, and brings the header indicator and
        /// footer buttons in line with where the user now is.
        /// </summary>
        private void GoToStep(int stepIndex)
        {
            if (stepIndex == StepSummary)
            {
                BuildSummaryContent();
            }

            for (var i = 0; i < StepCount; i++)
            {
                _stepPanels[i].Visible = i == stepIndex;
            }

            _currentStep = stepIndex;

            // On the Confirmation screen the index deliberately runs past the last
            // chip so every step in the indicator renders as completed.
            stepIndicator.CurrentStepIndex = stepIndex;

            UpdateFooterForStep(stepIndex);
            FocusFirstControl(stepIndex);
        }

        private void UpdateFooterForStep(int stepIndex)
        {
            var isConfirmation = stepIndex == StepConfirmation;

            btnCancel.Visible = !isConfirmation;
            btnBack.Visible = !isConfirmation && stepIndex > StepCategory;
            btnNext.Visible = !isConfirmation;
            btnCloseWizard.Visible = isConfirmation;
            btnSubmitAnother.Visible = isConfirmation;

            btnNext.Text = stepIndex == StepSummary ? "Submit" : "Next";
            lblFooterMessage.Text = FooterMessages[stepIndex];
        }

        private void FocusFirstControl(int stepIndex)
        {
            switch (stepIndex)
            {
                case StepCategory:
                    cmbCategory.Focus();
                    break;
                case StepDescription:
                    rtbDescription.Focus();
                    break;
                case StepLocation:
                    txtLocation.Focus();
                    break;
                case StepAttachment:
                    btnAttach.Focus();
                    break;
                case StepContact:
                    txtFirstName.Focus();
                    break;
            }
        }

        /// <summary>
        /// Checks the current step is complete before the user is allowed to move on,
        /// pointing them at the specific control that still needs attention.
        /// </summary>
        private bool ValidateStep(int stepIndex)
        {
            switch (stepIndex)
            {
                case StepCategory:
                    if (cmbCategory.SelectedItem == null)
                    {
                        ShowValidationError("Please select a category for the issue.", cmbCategory);
                        return false;
                    }
                    return true;

                case StepDescription:
                    var description = rtbDescription.Text.Trim();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        ShowValidationError("Please provide a description of the issue.", rtbDescription);
                        return false;
                    }
                    if (description.Length < MinimumDescriptionLength)
                    {
                        ShowValidationError(
                            "Please give us a little more detail - at least " + MinimumDescriptionLength + " characters.",
                            rtbDescription);
                        return false;
                    }
                    return true;

                case StepLocation:
                    if (string.IsNullOrWhiteSpace(txtLocation.Text))
                    {
                        ShowValidationError("Please enter the location of the issue.", txtLocation);
                        return false;
                    }
                    return true;

                case StepContact:
                    return ValidateContactStep();

                default:
                    // Attachments are optional, and the Summary step only reviews data
                    // that was already validated on the step it came from.
                    return true;
            }
        }

        private bool ValidateContactStep()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                ShowValidationError("Please enter your first name.", txtFirstName);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSurname.Text))
            {
                ShowValidationError("Please enter your surname.", txtSurname);
                return false;
            }

            if (rbEmail.Checked)
            {
                var email = txtEmail.Text.Trim();
                if (string.IsNullOrWhiteSpace(email))
                {
                    ShowValidationError("Please enter your email address, or choose to be updated by SMS instead.", txtEmail);
                    return false;
                }
                if (!LooksLikeEmailAddress(email))
                {
                    ShowValidationError("That email address doesn't look right. Please check it and try again.", txtEmail);
                    return false;
                }
                return true;
            }

            var mobileDigits = DigitsOnly(txtMobile.Text);
            if (mobileDigits.Length == 0)
            {
                ShowValidationError("Please enter your mobile number, or choose to be updated by email instead.", txtMobile);
                return false;
            }
            if (mobileDigits.Length != 10)
            {
                ShowValidationError("Please enter a 10-digit mobile number, for example 0821234567.", txtMobile);
                return false;
            }

            return true;
        }

        private static bool LooksLikeEmailAddress(string value)
        {
            var atIndex = value.IndexOf('@');
            if (atIndex <= 0 || atIndex == value.Length - 1)
            {
                return false;
            }

            var domain = value.Substring(atIndex + 1);
            return domain.Contains(".")
                && !domain.StartsWith(".")
                && !domain.EndsWith(".")
                && !domain.Contains("@")
                && !value.Contains(" ");
        }

        private static string DigitsOnly(string value)
        {
            return new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        }

        private void ShowValidationError(string message, Control controlToFocus)
        {
            MessageBox.Show(message, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            controlToFocus.Focus();
        }

        /// <summary>True if the user has typed or chosen anything at all, so Cancel can warn before discarding it.</summary>
        private bool HasAnyDataEntered()
        {
            return cmbCategory.SelectedItem != null
                || !string.IsNullOrWhiteSpace(rtbDescription.Text)
                || !string.IsNullOrWhiteSpace(txtLocation.Text)
                || !string.IsNullOrWhiteSpace(txtAdditionalLocation.Text)
                || _attachedFiles.Count > 0
                || !string.IsNullOrWhiteSpace(txtFirstName.Text)
                || !string.IsNullOrWhiteSpace(txtSurname.Text)
                || !string.IsNullOrWhiteSpace(txtEmail.Text)
                || !string.IsNullOrWhiteSpace(txtMobile.Text);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (!ValidateStep(_currentStep))
            {
                return;
            }

            if (_currentStep == StepSummary)
            {
                SubmitReport();
                return;
            }

            GoToStep(_currentStep + 1);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (_currentStep > StepCategory)
            {
                GoToStep(_currentStep - 1);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCloseWizard_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSubmitAnother_Click(object sender, EventArgs e)
        {
            ResetWizard();
            GoToStep(StepCategory);
        }

        #endregion

        #region Step behaviour

        private void rtbDescription_TextChanged(object sender, EventArgs e)
        {
            UpdateDescriptionCount();
        }

        private void UpdateDescriptionCount()
        {
            var length = rtbDescription.Text.Trim().Length;
            lblDescriptionCount.Text = length < MinimumDescriptionLength
                ? length + " characters - at least " + MinimumDescriptionLength + " needed"
                : length + " characters";
            lblDescriptionCount.ForeColor = length < MinimumDescriptionLength
                ? AppTheme.SecondaryColor
                : AppTheme.SuccessColor;
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

                        UpdateAttachmentHint();
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
                    "Select one or more attachments in the list first, then click Remove.",
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Removing from the highest index down keeps the remaining indices valid.
            var selectedIndices = lstAttachments.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            foreach (var index in selectedIndices)
            {
                _attachedFiles.RemoveAt(index);
                lstAttachments.Items.RemoveAt(index);
            }

            UpdateAttachmentHint();
        }

        private void UpdateAttachmentHint()
        {
            if (_attachedFiles.Count == 0)
            {
                lblAttachmentHint.Text = "No files attached yet - you can continue without any.";
                lblAttachmentHint.ForeColor = AppTheme.SecondaryColor;
                return;
            }

            lblAttachmentHint.Text = _attachedFiles.Count == 1
                ? "1 file attached."
                : _attachedFiles.Count + " files attached.";
            lblAttachmentHint.ForeColor = AppTheme.SuccessColor;
        }

        private void BuildSummaryContent()
        {
            var lines = new List<string>
            {
                "Category: " + (cmbCategory.SelectedItem == null ? "-" : cmbCategory.SelectedItem.ToString()),
                string.Empty,
                "Description: " + rtbDescription.Text.Trim(),
                string.Empty,
                "Location: " + txtLocation.Text.Trim()
            };

            if (!string.IsNullOrWhiteSpace(txtAdditionalLocation.Text))
            {
                lines.Add("Additional location information: " + txtAdditionalLocation.Text.Trim());
            }

            lines.Add(string.Empty);
            lines.Add("Attachments: " + (_attachedFiles.Count == 0
                ? "none"
                : string.Join(", ", _attachedFiles.Select(Path.GetFileName).ToArray())));

            lines.Add(string.Empty);
            lines.Add("Contact: " + (txtFirstName.Text.Trim() + " " + txtSurname.Text.Trim()).Trim());
            lines.Add("Preferred updates: " + (rbEmail.Checked
                ? "Email - " + txtEmail.Text.Trim()
                : "SMS - " + txtMobile.Text.Trim()));

            lblSummaryPlaceholder.Text = string.Join(Environment.NewLine, lines.ToArray());
        }

        #endregion

        #region Submission

        private void SubmitReport()
        {
            try
            {
                var issue = new Issue
                {
                    Category = cmbCategory.SelectedItem.ToString(),
                    Description = rtbDescription.Text.Trim(),
                    Location = txtLocation.Text.Trim(),
                    AdditionalLocationInfo = txtAdditionalLocation.Text.Trim()
                };
                issue.AttachmentPaths.AddRange(_attachedFiles);
                ApplyContactDetails(issue);

                IssueRepository.AddIssue(issue);
                _submittedIssue = issue;

                BuildConfirmationContent(issue);
                GoToStep(StepConfirmation);
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

        private void ApplyContactDetails(Issue issue)
        {
            issue.ContactFirstName = txtFirstName.Text.Trim();
            issue.ContactSurname = txtSurname.Text.Trim();
            issue.ContactEmail = txtEmail.Text.Trim();
            issue.ContactMobile = txtMobile.Text.Trim();
            issue.PreferredContactMethod = rbEmail.Checked ? ContactMethod.Email : ContactMethod.Mobile;
        }

        private void BuildConfirmationContent(Issue issue)
        {
            var lines = new[]
            {
                "Your reference number is " + issue.ReferenceNumber + ".",
                string.Empty,
                "Category: " + issue.Category,
                "Location: " + issue.Location,
                "Logged: " + issue.DateReported.ToString("dd MMMM yyyy HH:mm"),
                "Attachments: " + issue.AttachmentPaths.Count
            };

            lblConfirmationDetail.Text = string.Join(Environment.NewLine, lines);
        }

        private void ResetWizard()
        {
            cmbCategory.SelectedIndex = -1;
            rtbDescription.Clear();
            txtLocation.Clear();
            txtAdditionalLocation.Clear();
            txtFirstName.Clear();
            txtSurname.Clear();
            txtEmail.Clear();
            txtMobile.Clear();
            rbMobile.Checked = true;

            _attachedFiles.Clear();
            lstAttachments.Items.Clear();
            _submittedIssue = null;

            UpdateDescriptionCount();
            UpdateAttachmentHint();
        }

        #endregion
    }
}

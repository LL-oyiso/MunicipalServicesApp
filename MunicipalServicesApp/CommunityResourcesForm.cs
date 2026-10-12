using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// A bonus, non-marked section: Emergency Services, Know Your Ward and Community
    /// Programs, browsable as three tabs. This whole form is explicitly a "nice to
    /// have" extra that does not count toward the Local Events and Announcements
    /// rubric - it exists purely to be useful to a resident using the app, built the
    /// same way LocalEventsForm was (persistent chrome in the Designer, everything
    /// inside pnlContent built here programmatically, tab strip + AutoScroll panels
    /// using the shared FlowLayoutHelper fix for wrap-height).
    /// </summary>
    public partial class CommunityResourcesForm : Form
    {
        // Tab strip
        private Panel _pnlEmergencyTab;
        private Panel _pnlWardTab;
        private Panel _pnlProgramsTab;
        private Button _btnTabEmergency;
        private Button _btnTabWard;
        private Button _btnTabPrograms;

        private Panel _scrollEmergencyPanel;
        private Panel _scrollWardPanel;
        private Panel _scrollProgramsPanel;

        // Emergency Services tab
        private ComboBox _cmbServiceType;
        private TextBox _txtEmergencyArea;
        private FlowLayoutPanel _flpEmergencyGallery;
        private Label _lblEmergencyResultsSummary;

        // Know Your Ward tab
        private ComboBox _cmbWard;
        private FlowLayoutPanel _flpWardGallery;
        private Label _lblWardResultsSummary;

        // Community Programs tab
        private ComboBox _cmbProgramCategory;
        private FlowLayoutPanel _flpProgramsGallery;
        private Label _lblProgramsResultsSummary;

        public CommunityResourcesForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;

            ApplyTheme();
            BuildContent();
            RefreshEmergencyGallery();
            RefreshWardGallery();
            RefreshProgramsGallery();

            Shown += (s, e) =>
            {
                FlowLayoutHelper.ForceScrollableRelayout(_scrollEmergencyPanel, _flpEmergencyGallery);
                FlowLayoutHelper.ForceScrollableRelayout(_scrollWardPanel, _flpWardGallery);
                FlowLayoutHelper.ForceScrollableRelayout(_scrollProgramsPanel, _flpProgramsGallery);
            };
        }

        private void ApplyTheme()
        {
            AppTheme.ApplyFormDefaults(this);

            pnlHeaderBar.BackColor = AppTheme.HeaderBarColor;
            lblFormTitle.Font = AppTheme.TitleFont;
            lblFormTitle.ForeColor = Color.White;

            AppTheme.StyleSecondaryButton(btnBackToMenu);

            pnlContent.BackColor = AppTheme.BackgroundColor;
        }

        private void btnBackToMenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BuildContent()
        {
            var tabStrip = BuildTabStrip();
            _pnlEmergencyTab = BuildEmergencyTabPanel();
            _pnlWardTab = BuildWardTabPanel();
            _pnlProgramsTab = BuildProgramsTabPanel();
            _pnlWardTab.Visible = false;
            _pnlProgramsTab.Visible = false;

            pnlContent.Controls.Add(_pnlProgramsTab);
            pnlContent.Controls.Add(_pnlWardTab);
            pnlContent.Controls.Add(_pnlEmergencyTab);
            pnlContent.Controls.Add(tabStrip);
        }

        private Panel BuildTabStrip()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White
            };

            _btnTabEmergency = new Button
            {
                Text = "Emergency Services",
                Location = new Point(24, 8),
                Size = new Size(170, 36)
            };
            _btnTabEmergency.Click += (s, e) => SelectTab(0);

            _btnTabWard = new Button
            {
                Text = "Know Your Ward",
                Location = new Point(202, 8),
                Size = new Size(160, 36)
            };
            _btnTabWard.Click += (s, e) => SelectTab(1);

            _btnTabPrograms = new Button
            {
                Text = "Community Programs",
                Location = new Point(370, 8),
                Size = new Size(170, 36)
            };
            _btnTabPrograms.Click += (s, e) => SelectTab(2);

            panel.Controls.Add(_btnTabEmergency);
            panel.Controls.Add(_btnTabWard);
            panel.Controls.Add(_btnTabPrograms);

            ApplyTabButtonStyle(_btnTabEmergency, isSelected: true);
            ApplyTabButtonStyle(_btnTabWard, isSelected: false);
            ApplyTabButtonStyle(_btnTabPrograms, isSelected: false);

            return panel;
        }

        private static void ApplyTabButtonStyle(Button button, bool isSelected)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = AppTheme.ButtonFont;
            button.Cursor = Cursors.Hand;

            if (isSelected)
            {
                button.BackColor = AppTheme.AccentColor;
                button.ForeColor = Color.White;
            }
            else
            {
                button.BackColor = Color.FromArgb(238, 240, 243);
                button.ForeColor = AppTheme.SecondaryColor;
            }
        }

        private void SelectTab(int index)
        {
            _pnlEmergencyTab.Visible = index == 0;
            _pnlWardTab.Visible = index == 1;
            _pnlProgramsTab.Visible = index == 2;

            ApplyTabButtonStyle(_btnTabEmergency, index == 0);
            ApplyTabButtonStyle(_btnTabWard, index == 1);
            ApplyTabButtonStyle(_btnTabPrograms, index == 2);

            switch (index)
            {
                case 0:
                    FlowLayoutHelper.ForceScrollableRelayout(_scrollEmergencyPanel, _flpEmergencyGallery);
                    break;
                case 1:
                    FlowLayoutHelper.ForceScrollableRelayout(_scrollWardPanel, _flpWardGallery);
                    break;
                case 2:
                    FlowLayoutHelper.ForceScrollableRelayout(_scrollProgramsPanel, _flpProgramsGallery);
                    break;
            }
        }

        // ----------------------------------------------------------------
        // Emergency Services tab
        // ----------------------------------------------------------------

        private Panel BuildEmergencyTabPanel()
        {
            var tabPanel = new Panel { Dock = DockStyle.Fill };

            var filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(24, 14, 24, 14)
            };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            _cmbServiceType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 0, 8, 0) };
            _cmbServiceType.Items.Add(new ComboOption<EmergencyServiceType?>("All Service Types", null));
            foreach (EmergencyServiceType type in Enum.GetValues(typeof(EmergencyServiceType)))
            {
                _cmbServiceType.Items.Add(new ComboOption<EmergencyServiceType?>(CommunityResourcesRepository.GetServiceTypeDisplayName(type), type));
            }
            _cmbServiceType.SelectedIndex = 0;

            _txtEmergencyArea = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.BodyFont, Margin = new Padding(0, 0, 8, 0) };

            var btnSearch = new Button { Text = "Search", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0) };
            AppTheme.StylePrimaryButton(btnSearch);
            btnSearch.Click += (s, e) => RefreshEmergencyGallery();

            var btnClear = new Button { Text = "Clear filters", Dock = DockStyle.Fill };
            AppTheme.StyleSecondaryButton(btnClear);
            btnClear.Click += (s, e) =>
            {
                _cmbServiceType.SelectedIndex = 0;
                _txtEmergencyArea.Text = string.Empty;
                RefreshEmergencyGallery();
            };

            layout.Controls.Add(_cmbServiceType, 0, 0);
            layout.Controls.Add(_txtEmergencyArea, 1, 0);
            layout.Controls.Add(btnSearch, 2, 0);
            layout.Controls.Add(btnClear, 3, 0);
            filterBar.Controls.Add(layout);

            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.BackgroundColor, Padding = new Padding(24, 16, 8, 24) };
            var section = BuildSectionShell(
                "Emergency Services",
                "Search fire brigade, ambulance, police and mental health contacts by area or name.",
                out _lblEmergencyResultsSummary,
                out _flpEmergencyGallery);
            scrollPanel.Controls.Add(section);
            _scrollEmergencyPanel = scrollPanel;

            tabPanel.Controls.Add(scrollPanel);
            tabPanel.Controls.Add(filterBar);
            return tabPanel;
        }

        private void RefreshEmergencyGallery()
        {
            var serviceType = (_cmbServiceType.SelectedItem as ComboOption<EmergencyServiceType?>)?.Value;
            var area = _txtEmergencyArea.Text;

            var results = CommunityResourcesRepository.SearchEmergencyContacts(serviceType, area);

            _flpEmergencyGallery.SuspendLayout();
            foreach (Control control in _flpEmergencyGallery.Controls)
            {
                control.Dispose();
            }
            _flpEmergencyGallery.Controls.Clear();

            foreach (var contact in results)
            {
                _flpEmergencyGallery.Controls.Add(CreateEmergencyCard(contact));
            }
            _flpEmergencyGallery.ResumeLayout();

            _lblEmergencyResultsSummary.Text = results.Count == 0
                ? "No contacts match your filters. Try clearing them."
                : $"Showing {results.Count} contact{(results.Count == 1 ? string.Empty : "s")}.";

            FlowLayoutHelper.ForceScrollableRelayout(_scrollEmergencyPanel, _flpEmergencyGallery);
        }

        private static ContactCardControl CreateEmergencyCard(EmergencyContact contact)
        {
            return new ContactCardControl
            {
                Title = contact.Name,
                BadgeText = CommunityResourcesRepository.GetServiceTypeDisplayName(contact.ServiceType),
                BadgeColor = GetServiceTypeColor(contact.ServiceType),
                DetailLine1 = $"Area: {contact.Area}",
                DetailLine2 = $"Phone: {contact.Phone}",
                DetailLine3 = $"{contact.Address} ({contact.Hours})"
            };
        }

        private static Color GetServiceTypeColor(EmergencyServiceType type)
        {
            switch (type)
            {
                case EmergencyServiceType.FireBrigade: return AppTheme.DangerColor;
                case EmergencyServiceType.Ambulance: return AppTheme.WarningColor;
                case EmergencyServiceType.Police: return AppTheme.PrimaryColor;
                case EmergencyServiceType.MentalHealth: return Color.FromArgb(155, 89, 182);
                default: return AppTheme.AccentColor;
            }
        }

        // ----------------------------------------------------------------
        // Know Your Ward tab
        // ----------------------------------------------------------------

        private Panel BuildWardTabPanel()
        {
            var tabPanel = new Panel { Dock = DockStyle.Fill };

            var filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(24, 14, 24, 14)
            };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            _cmbWard = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 0, 8, 0) };
            _cmbWard.Items.Add(new ComboOption<int?>("All Wards", null));
            foreach (var ward in CommunityResourcesRepository.WardNumbers.OrderBy(w => w))
            {
                _cmbWard.Items.Add(new ComboOption<int?>($"Ward {ward}", ward));
            }
            _cmbWard.SelectedIndex = 0;
            _cmbWard.SelectedIndexChanged += (s, e) => RefreshWardGallery();

            layout.Controls.Add(_cmbWard, 0, 0);
            filterBar.Controls.Add(layout);

            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.BackgroundColor, Padding = new Padding(24, 16, 8, 24) };
            var section = BuildSectionShell(
                "Know Your Ward",
                "Find your ward councillor or community caretaker and how to reach them.",
                out _lblWardResultsSummary,
                out _flpWardGallery);
            scrollPanel.Controls.Add(section);
            _scrollWardPanel = scrollPanel;

            tabPanel.Controls.Add(scrollPanel);
            tabPanel.Controls.Add(filterBar);
            return tabPanel;
        }

        private void RefreshWardGallery()
        {
            var wardNumber = (_cmbWard.SelectedItem as ComboOption<int?>)?.Value;
            var results = CommunityResourcesRepository.GetWardMembers(wardNumber);

            _flpWardGallery.SuspendLayout();
            foreach (Control control in _flpWardGallery.Controls)
            {
                control.Dispose();
            }
            _flpWardGallery.Controls.Clear();

            foreach (var member in results)
            {
                _flpWardGallery.Controls.Add(CreateWardCard(member));
            }
            _flpWardGallery.ResumeLayout();

            _lblWardResultsSummary.Text = results.Count == 0
                ? "No ward representatives found."
                : $"Showing {results.Count} representative{(results.Count == 1 ? string.Empty : "s")}.";

            FlowLayoutHelper.ForceScrollableRelayout(_scrollWardPanel, _flpWardGallery);
        }

        private static ContactCardControl CreateWardCard(WardCouncilMember member)
        {
            return new ContactCardControl
            {
                Title = member.Name,
                BadgeText = member.Role,
                BadgeColor = member.Role == "Ward Councillor" ? AppTheme.AccentColor : AppTheme.SuccessColor,
                DetailLine1 = $"Ward {member.WardNumber}",
                DetailLine2 = $"{member.Phone}  -  {member.Email}",
                DetailLine3 = $"{member.OfficeAddress} ({member.OfficeHours})"
            };
        }

        // ----------------------------------------------------------------
        // Community Programs tab
        // ----------------------------------------------------------------

        private Panel BuildProgramsTabPanel()
        {
            var tabPanel = new Panel { Dock = DockStyle.Fill };

            var filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(24, 14, 24, 14)
            };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            _cmbProgramCategory = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 0, 8, 0) };
            _cmbProgramCategory.Items.Add(new ComboOption<ProgramCategory?>("All Categories", null));
            foreach (ProgramCategory category in Enum.GetValues(typeof(ProgramCategory)))
            {
                _cmbProgramCategory.Items.Add(new ComboOption<ProgramCategory?>(CommunityResourcesRepository.GetProgramCategoryDisplayName(category), category));
            }
            _cmbProgramCategory.SelectedIndex = 0;
            _cmbProgramCategory.SelectedIndexChanged += (s, e) => RefreshProgramsGallery();

            layout.Controls.Add(_cmbProgramCategory, 0, 0);
            filterBar.Controls.Add(layout);

            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.BackgroundColor, Padding = new Padding(24, 16, 8, 24) };
            var section = BuildSectionShell(
                "Community Programs",
                "Computer literacy, CV guidelines and interview preparation sessions near you.",
                out _lblProgramsResultsSummary,
                out _flpProgramsGallery);
            scrollPanel.Controls.Add(section);
            _scrollProgramsPanel = scrollPanel;

            tabPanel.Controls.Add(scrollPanel);
            tabPanel.Controls.Add(filterBar);
            return tabPanel;
        }

        private void RefreshProgramsGallery()
        {
            var category = (_cmbProgramCategory.SelectedItem as ComboOption<ProgramCategory?>)?.Value;
            var results = CommunityResourcesRepository.GetPrograms(category);

            _flpProgramsGallery.SuspendLayout();
            foreach (Control control in _flpProgramsGallery.Controls)
            {
                control.Dispose();
            }
            _flpProgramsGallery.Controls.Clear();

            foreach (var program in results)
            {
                _flpProgramsGallery.Controls.Add(CreateProgramCard(program));
            }
            _flpProgramsGallery.ResumeLayout();

            _lblProgramsResultsSummary.Text = results.Count == 0
                ? "No programmes found."
                : $"Showing {results.Count} programme{(results.Count == 1 ? string.Empty : "s")}.";

            FlowLayoutHelper.ForceScrollableRelayout(_scrollProgramsPanel, _flpProgramsGallery);
        }

        private static ContactCardControl CreateProgramCard(CommunityProgram program)
        {
            return new ContactCardControl
            {
                Title = program.Title,
                BadgeText = CommunityResourcesRepository.GetProgramCategoryDisplayName(program.Category),
                BadgeColor = GetProgramCategoryColor(program.Category),
                DetailLine1 = program.Description,
                DetailLine2 = $"When: {program.Schedule}",
                DetailLine3 = $"{program.Location}  -  {program.ContactInfo}"
            };
        }

        private static Color GetProgramCategoryColor(ProgramCategory category)
        {
            switch (category)
            {
                case ProgramCategory.ComputerLiteracy: return Color.FromArgb(0, 150, 150);
                case ProgramCategory.CvGuidelines: return Color.FromArgb(40, 140, 70);
                case ProgramCategory.InterviewPrep: return Color.FromArgb(155, 89, 182);
                default: return AppTheme.AccentColor;
            }
        }

        // ----------------------------------------------------------------
        // Shared helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Builds the common "heading + instructional subtitle + results summary +
        /// gallery" shell shared by all three tabs, matching the section headers used
        /// in LocalEventsForm.
        /// </summary>
        private static Panel BuildSectionShell(string heading, string subtitle, out Label resultsSummary, out FlowLayoutPanel gallery)
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 24)
            };

            var headingLabel = new Label
            {
                Text = heading,
                Dock = DockStyle.Top,
                Height = 30,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor
            };

            var subtitleLabel = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Top,
                Height = 20,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor
            };

            resultsSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor
            };

            gallery = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                // Intentionally not AutoSize - see FlowLayoutHelper for why.
                WrapContents = true,
                AutoScroll = false
            };

            // Added in reverse visual order (Dock=Top siblings stack with the
            // last-added control on top), same convention used throughout
            // LocalEventsForm: gallery first, then summary, then subtitle, then
            // heading, so it reads heading -> subtitle -> summary -> gallery.
            section.Controls.Add(gallery);
            section.Controls.Add(resultsSummary);
            section.Controls.Add(subtitleLabel);
            section.Controls.Add(headingLabel);

            return section;
        }

        /// <summary>A ComboBox item pairing a display string with a strongly-typed value.</summary>
        private class ComboOption<T>
        {
            public string Text { get; }
            public T Value { get; }

            public ComboOption(string text, T value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString() => Text;
        }
    }
}

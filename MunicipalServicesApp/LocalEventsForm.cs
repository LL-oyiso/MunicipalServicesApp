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
    /// Lets residents browse, search and filter local events and municipal
    /// announcements, and (in later checkpoints) view personalised recommendations
    /// and submit their own announcement requests. The persistent chrome (header bar,
    /// content container) is built in the Designer; everything inside pnlContent is
    /// built here programmatically, since it keeps growing across checkpoints and
    /// reacts to live search/filter state - the same approach ReportIssuesForm uses
    /// for its wizard steps.
    /// </summary>
    public partial class LocalEventsForm : Form
    {
        private readonly RecommendationEngine _recommendationEngine = new RecommendationEngine();

        // Filter bar controls
        private ComboBox _cmbCategory;
        private DateTimePicker _dtpFrom;
        private DateTimePicker _dtpTo;
        private TextBox _txtKeyword;
        private Button _btnSearch;
        private Button _btnClearFilters;
        private DateTime _defaultFromDate;
        private DateTime _defaultToDate;

        // Events gallery
        private FlowLayoutPanel _flpEventsGallery;
        private Label _lblEventsResultsSummary;

        public LocalEventsForm()
        {
            InitializeComponent();
            ApplyTheme();
            BuildContent();
            ExecuteSearch(recordSearch: false);
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

        /// <summary>Builds the filter bar (Dock=Top) and the scrollable body (Dock=Fill) inside pnlContent.</summary>
        private void BuildContent()
        {
            var filterBar = BuildFilterBar();
            var scrollBody = BuildScrollBody();

            // Added in this order so the Dock=Fill scroll body claims the remaining
            // space left over once the Dock=Top filter bar (added after it) has
            // staked its claim at the top - mirrors the Dock-order convention already
            // used for pnlHeaderBar's children in this file's Designer.
            pnlContent.Controls.Add(scrollBody);
            pnlContent.Controls.Add(filterBar);
        }

        private Panel BuildFilterBar()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(24, 12, 24, 12)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
            // Fixed (Absolute) row heights rather than AutoSize: TableLayoutPanel's
            // AutoSize calculation for a Dock=Fill Button can under-size on the very
            // first layout pass (it only corrects itself once something forces a
            // relayout, e.g. resizing the window) - which was silently clipping the
            // Search/Clear filters button text down to nothing. Fixed heights avoid
            // depending on that calculation at all.
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            layout.Controls.Add(BuildFieldLabel("Category"), 0, 0);
            layout.Controls.Add(BuildFieldLabel("From date"), 1, 0);
            layout.Controls.Add(BuildFieldLabel("To date"), 2, 0);
            layout.Controls.Add(BuildFieldLabel("Keyword"), 3, 0);

            _cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.BodyFont,
                Margin = new Padding(0, 0, 8, 0)
            };
            PopulateCategoryDropdown();

            layout.Controls.Add(_cmbCategory, 0, 1);

            // Default the range to span every seeded event, so showing both pickers
            // immediately (no separate "enable this filter" step) still starts out
            // displaying everything - narrowing either date simply narrows the range.
            _defaultFromDate = EventsRepository.EventDates.DefaultIfEmpty(DateTime.Now.Date).Min();
            _defaultToDate = EventsRepository.EventDates.DefaultIfEmpty(DateTime.Now.Date).Max();

            _dtpFrom = BuildDatePicker(_defaultFromDate);
            layout.Controls.Add(_dtpFrom, 1, 1);

            _dtpTo = BuildDatePicker(_defaultToDate);
            layout.Controls.Add(_dtpTo, 2, 1);

            _txtKeyword = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                Margin = new Padding(0, 0, 8, 0)
            };
            layout.Controls.Add(_txtKeyword, 3, 1);

            _btnSearch = new Button { Dock = DockStyle.Fill, Text = "Search", Margin = new Padding(0, 0, 6, 0) };
            AppTheme.StylePrimaryButton(_btnSearch);
            _btnSearch.Click += (s, e) => ExecuteSearch(recordSearch: true);
            layout.Controls.Add(_btnSearch, 4, 1);

            _btnClearFilters = new Button { Dock = DockStyle.Fill, Text = "Clear filters", Margin = new Padding(6, 0, 0, 0) };
            AppTheme.StyleSecondaryButton(_btnClearFilters);
            _btnClearFilters.Click += (s, e) => ClearFilters();
            layout.Controls.Add(_btnClearFilters, 5, 1);

            panel.Controls.Add(layout);
            return panel;
        }

        private static Label BuildFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 8, 2)
            };
        }

        /// <summary>
        /// Builds a date picker used directly as a filter boundary - no separate
        /// "enable this filter" checkbox. Whatever date is showing is the boundary
        /// the search applies, which is why the pickers default to the earliest and
        /// latest seeded event dates (so the initial view still shows everything).
        /// Margin matches the other filter-bar fields so every control's input row
        /// lines up on the same baseline.
        /// </summary>
        private static DateTimePicker BuildDatePicker(DateTime initialValue)
        {
            return new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Value = initialValue,
                Font = AppTheme.BodyFont,
                Margin = new Padding(0, 0, 8, 0)
            };
        }

        private void PopulateCategoryDropdown()
        {
            _cmbCategory.Items.Clear();
            _cmbCategory.Items.Add("All Categories");
            foreach (var category in EventsRepository.Categories.OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
            {
                _cmbCategory.Items.Add(category);
            }
            _cmbCategory.SelectedIndex = 0;
        }

        private Panel BuildScrollBody()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.BackgroundColor,
                Padding = new Padding(24, 16, 8, 24)
            };

            var sectionHeader = new Label
            {
                Text = "Upcoming Events",
                Dock = DockStyle.Top,
                Height = 30,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor
            };

            _lblEventsResultsSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor
            };

            _flpEventsGallery = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            // Added in reverse visual order: the Dock engine stacks the most recently
            // added Dock=Top control at the very top, so the gallery (added first)
            // ends up below the summary line, which ends up below the section header.
            scrollPanel.Controls.Add(_flpEventsGallery);
            scrollPanel.Controls.Add(_lblEventsResultsSummary);
            scrollPanel.Controls.Add(sectionHeader);

            return scrollPanel;
        }

        private void ClearFilters()
        {
            _cmbCategory.SelectedIndex = 0;
            _dtpFrom.Value = _defaultFromDate;
            _dtpTo.Value = _defaultToDate;
            _txtKeyword.Text = string.Empty;
            ExecuteSearch(recordSearch: false);
        }

        /// <summary>
        /// Reads the current filter bar state and runs the search. Only records the
        /// search onto the recommendation engine's history stack when it was
        /// explicitly triggered by the resident clicking "Search" - the initial
        /// page-load view and "Clear filters" aren't genuine expressions of interest.
        /// </summary>
        private void ExecuteSearch(bool recordSearch)
        {
            var category = _cmbCategory.SelectedIndex > 0 ? _cmbCategory.SelectedItem.ToString() : null;
            DateTime? fromDate = _dtpFrom.Value.Date;
            DateTime? toDate = _dtpTo.Value.Date;
            var keyword = string.IsNullOrWhiteSpace(_txtKeyword.Text) ? null : _txtKeyword.Text.Trim();

            var results = EventsRepository.SearchEvents(category, fromDate, toDate, keyword);

            if (recordSearch)
            {
                _recommendationEngine.RecordSearch(new SearchQuery
                {
                    Category = category,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Keyword = keyword
                });
            }

            RefreshEventsGallery(results);
        }

        private void RefreshEventsGallery(List<Event> events)
        {
            _flpEventsGallery.SuspendLayout();

            foreach (Control control in _flpEventsGallery.Controls)
            {
                control.Dispose();
            }
            _flpEventsGallery.Controls.Clear();

            foreach (var evt in events)
            {
                _flpEventsGallery.Controls.Add(CreateEventCard(evt));
            }

            _flpEventsGallery.ResumeLayout();

            _lblEventsResultsSummary.Text = events.Count == 0
                ? "No events match your filters. Try clearing them."
                : $"Showing {events.Count} event{(events.Count == 1 ? string.Empty : "s")}.";
        }

        private static EventCardControl CreateEventCard(Event evt, bool isRecommended = false, string reason = null)
        {
            return new EventCardControl
            {
                Title = evt.Title,
                Category = evt.Category,
                EventLocation = evt.Location,
                Description = evt.Description,
                EventDate = evt.EventDate,
                IsRecommended = isRecommended,
                RecommendationReason = reason ?? string.Empty
            };
        }
    }
}

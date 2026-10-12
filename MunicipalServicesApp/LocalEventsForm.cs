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
    ///
    /// Content is split across two tabs (Upcoming Events / Announcements) rather than
    /// one long scrolling page, with a notification badge on the Announcements tab
    /// showing how many published announcements the resident hasn't viewed yet.
    /// </summary>
    public partial class LocalEventsForm : Form
    {
        private readonly RecommendationEngine _recommendationEngine = new RecommendationEngine();

        // Announcement IDs the resident has already seen, tracked for this session so
        // the Announcements tab's notification badge can show a genuine "unread" count.
        private readonly HashSet<string> _readAnnouncementIds = new HashSet<string>();

        // Tab strip
        private Panel _pnlEventsTab;
        private Panel _pnlAnnouncementsTab;
        private Button _btnTabEvents;
        private Button _btnTabAnnouncements;
        private NotificationBadgeControl _announcementsBadge;

        // The AutoScroll panels inside each tab - kept as fields (rather than local
        // variables) so ForceScrollableRelayout can be called on them whenever their
        // content changes or the tab becomes visible. See ForceScrollableRelayout for
        // why this is necessary.
        private Panel _scrollEventsPanel;
        private Panel _scrollAnnouncementsPanel;

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

        // Recommendations (search-history driven)
        private Panel _pnlRecommendationsSection;
        private FlowLayoutPanel _flpRecommendationsGallery;
        private Label _lblRecommendationsSummary;
        private Button _btnPreviousSearch;

        // Announcements gallery
        private FlowLayoutPanel _flpAnnouncementsGallery;
        private Label _lblAnnouncementsResultsSummary;

        // Submit-a-request mini-form
        private TextBox _txtAnnouncementTitle;
        private ComboBox _cmbAnnouncementCategory;
        private TextBox _txtAnnouncementMessage;
        private Button _btnSubmitAnnouncement;

        // Pending review queue
        private ListBox _lstPendingAnnouncements;
        private Label _lblPendingSummary;
        private Button _btnPublishNext;

        public LocalEventsForm()
        {
            InitializeComponent();

            // Opens maximized rather than at the Designer's fixed 1000x700 - there's
            // simply too much content (filter bar, two full galleries, submit form,
            // pending review) to view comfortably at a fixed size, and the resident
            // shouldn't have to remember to maximize it by hand every time.
            WindowState = FormWindowState.Maximized;

            ApplyTheme();
            BuildContent();
            ExecuteSearch(recordSearch: false);
            RefreshAnnouncementsGallery();
            RefreshPendingList();

            // Belt-and-braces: recompute both galleries' heights once more after the
            // form has actually been shown on screen with its real size. See
            // FitFlowPanelHeight for why the gallery height has to be computed
            // manually at all.
            Shown += (s, e) =>
            {
                ForceScrollableRelayout(_scrollEventsPanel, _flpEventsGallery, _flpRecommendationsGallery);
                ForceScrollableRelayout(_scrollAnnouncementsPanel, _flpAnnouncementsGallery);
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

        /// <summary>
        /// Builds the tab strip (Dock=Top) and the two tab content panels (Dock=Fill)
        /// inside pnlContent. Only one tab panel is visible at a time; everything was
        /// previously one long scrolling page, which stopped being usable once the
        /// events gallery, announcements gallery, submission form and pending queue
        /// all had to share the same view.
        /// </summary>
        private void BuildContent()
        {
            var tabStrip = BuildTabStrip();
            _pnlEventsTab = BuildEventsTabPanel();
            _pnlAnnouncementsTab = BuildAnnouncementsTabPanel();
            _pnlAnnouncementsTab.Visible = false;

            // Both tab panels are Dock=Fill and occupy the same space; only one is
            // Visible at a time so their add order relative to each other doesn't
            // matter. The tab strip is added LAST so it claims the top strip, the
            // same Dock-order convention used throughout this form.
            pnlContent.Controls.Add(_pnlAnnouncementsTab);
            pnlContent.Controls.Add(_pnlEventsTab);
            pnlContent.Controls.Add(tabStrip);
        }

        /// <summary>
        /// Two plain buttons styled to look like tabs, plus a notification badge
        /// overlapping the Announcements button's top-right corner. A real TabControl
        /// doesn't support that badge without owner-draw gymnastics that would look
        /// inconsistent with the rest of the app's hand-drawn controls, so this simple
        /// button pair (absolutely positioned - no Dock/AutoSize pitfalls to worry
        /// about for a two-item fixed strip) keeps the same custom-drawn visual style.
        /// </summary>
        private Panel BuildTabStrip()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White
            };

            _btnTabEvents = new Button
            {
                Text = "Upcoming Events",
                Location = new Point(24, 8),
                Size = new Size(160, 36)
            };
            _btnTabEvents.Click += (s, e) => SelectTab(showEventsTab: true);

            _btnTabAnnouncements = new Button
            {
                Text = "Announcements",
                Location = new Point(192, 8),
                Size = new Size(160, 36)
            };
            _btnTabAnnouncements.Click += (s, e) => SelectTab(showEventsTab: false);

            _announcementsBadge = new NotificationBadgeControl
            {
                BackColor = Color.White,
                Location = new Point(_btnTabAnnouncements.Right - 12, _btnTabAnnouncements.Top - 8)
            };

            panel.Controls.Add(_btnTabEvents);
            panel.Controls.Add(_btnTabAnnouncements);
            panel.Controls.Add(_announcementsBadge);
            _announcementsBadge.BringToFront();

            ApplyTabButtonStyle(_btnTabEvents, isSelected: true);
            ApplyTabButtonStyle(_btnTabAnnouncements, isSelected: false);

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

        /// <summary>
        /// Switches which tab panel is visible and restyles the tab buttons to match.
        /// Switching into the Announcements tab is also the moment unread announcements
        /// become "read" - this mirrors how notification badges behave in most apps
        /// (the badge clears when you actually visit the place the notifications live,
        /// not just because new content happened to arrive while you were already
        /// looking at it).
        /// </summary>
        private void SelectTab(bool showEventsTab)
        {
            _pnlEventsTab.Visible = showEventsTab;
            _pnlAnnouncementsTab.Visible = !showEventsTab;

            ApplyTabButtonStyle(_btnTabEvents, showEventsTab);
            ApplyTabButtonStyle(_btnTabAnnouncements, !showEventsTab);

            if (!showEventsTab)
            {
                MarkAnnouncementsAsRead();
                ForceScrollableRelayout(_scrollAnnouncementsPanel, _flpAnnouncementsGallery);
            }
            else
            {
                ForceScrollableRelayout(_scrollEventsPanel, _flpEventsGallery, _flpRecommendationsGallery);
            }
        }

        /// <summary>
        /// Recomputes each given gallery's real wrapped height (see
        /// FitFlowPanelHeight) and then makes the containing AutoScroll panel
        /// re-layout against those now-correct heights. Safe to call repeatedly (on
        /// tab switch, on gallery refresh, and once more after the form is actually
        /// Shown) since it's cheap and idempotent. Accepts multiple galleries since
        /// the Events tab stacks two of them (events, then recommendations) inside
        /// the same scroll panel.
        /// </summary>
        private static void ForceScrollableRelayout(Panel scrollPanel, params FlowLayoutPanel[] galleries)
        {
            foreach (var gallery in galleries)
            {
                if (gallery != null)
                {
                    FitFlowPanelHeight(gallery);
                }
            }

            scrollPanel?.PerformLayout();
        }

        /// <summary>
        /// Manually computes and assigns a FlowLayoutPanel's height to fit every
        /// wrapped row of its children, based on the panel's *current* ClientSize.Width.
        ///
        /// This exists because FlowLayoutPanel's own AutoSize+WrapContents height
        /// calculation turned out to be unreliable once nested inside a Dock=Top
        /// AutoSize section Panel, itself inside a Dock=Fill AutoScroll Panel: it was
        /// self-reporting a height that only accounted for the first row, so every
        /// row after it got silently clipped - and no scrollbar ever appeared on the
        /// outer AutoScroll panel, because AutoScrollMinSize is computed FROM that
        /// already-wrong child height, not independently. The panel's WIDTH (and
        /// therefore how many cards fit per row) is assigned correctly by ordinary
        /// Dock=Top layout well before this runs, so replicating just the wrap-height
        /// arithmetic ourselves - using the panel's real current width and each
        /// child's real size/margin - sidesteps the unreliable built-in calculation
        /// entirely.
        /// </summary>
        private static void FitFlowPanelHeight(FlowLayoutPanel flp)
        {
            var availableWidth = flp.ClientSize.Width;
            if (availableWidth <= 0 || flp.Controls.Count == 0)
            {
                flp.Height = 0;
                return;
            }

            var totalHeight = 0;
            var rowHeight = 0;
            var rowWidthUsed = 0;
            var isFirstInRow = true;

            foreach (Control control in flp.Controls)
            {
                var controlWidth = control.Width + control.Margin.Left + control.Margin.Right;
                var controlHeight = control.Height + control.Margin.Top + control.Margin.Bottom;

                if (!isFirstInRow && rowWidthUsed + controlWidth > availableWidth)
                {
                    totalHeight += rowHeight;
                    rowWidthUsed = 0;
                    rowHeight = 0;
                    isFirstInRow = true;
                }

                rowWidthUsed += controlWidth;
                rowHeight = Math.Max(rowHeight, controlHeight);
                isFirstInRow = false;
            }

            totalHeight += rowHeight;
            flp.Height = totalHeight;
        }

        /// <summary>
        /// Events tab: the filter bar (Dock=Top) plus a scrollable panel containing
        /// the events gallery, then the search-history-driven recommendations
        /// section, in that reading order top to bottom.
        /// </summary>
        private Panel BuildEventsTabPanel()
        {
            var tabPanel = new Panel { Dock = DockStyle.Fill };

            var filterBar = BuildFilterBar();

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.BackgroundColor,
                Padding = new Padding(24, 16, 8, 24)
            };

            var recommendationsSection = BuildRecommendationsSection();
            var eventsSection = BuildEventsSection();

            // Added in reverse visual order (see BuildAnnouncementsTabPanel for why):
            // bottom-most section added first, top-most added last, so it reads
            // Upcoming Events -> Recommended For You from top to bottom.
            scrollPanel.Controls.Add(recommendationsSection);
            scrollPanel.Controls.Add(eventsSection);
            _scrollEventsPanel = scrollPanel;

            // scrollPanel (Fill) added first, filterBar (Top) added last - same
            // Dock-order convention used elsewhere in this form.
            tabPanel.Controls.Add(scrollPanel);
            tabPanel.Controls.Add(filterBar);

            return tabPanel;
        }

        /// <summary>
        /// Announcements tab: a scrollable panel containing the announcements gallery,
        /// the submit-a-request form, and the pending review queue, in that reading
        /// order top to bottom.
        /// </summary>
        private Panel BuildAnnouncementsTabPanel()
        {
            var tabPanel = new Panel { Dock = DockStyle.Fill };

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.BackgroundColor,
                Padding = new Padding(24, 16, 8, 24)
            };

            var pendingSection = BuildPendingReviewSection();
            var submitSection = BuildSubmitAnnouncementSection();
            var announcementsSection = BuildAnnouncementsSection();

            // Added in reverse visual order (see BuildEventsSection/BuildScrollBody
            // comments elsewhere in this file for why): bottom-most section added
            // first, top-most section added last, so it reads Announcements ->
            // Submit a Request -> Pending Review from top to bottom.
            scrollPanel.Controls.Add(pendingSection);
            scrollPanel.Controls.Add(submitSection);
            scrollPanel.Controls.Add(announcementsSection);
            _scrollAnnouncementsPanel = scrollPanel;

            tabPanel.Controls.Add(scrollPanel);

            return tabPanel;
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

        private Panel BuildEventsSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 24)
            };

            var header = new Label
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
                // Intentionally not AutoSize - see FitFlowPanelHeight for why.
                // Height is computed and assigned manually every time the card
                // list changes.
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            // Added in reverse visual order (see BuildAnnouncementsTabPanel for why):
            // gallery first, summary next, header last, so header ends up on top.
            section.Controls.Add(_flpEventsGallery);
            section.Controls.Add(_lblEventsResultsSummary);
            section.Controls.Add(header);

            return section;
        }

        /// <summary>
        /// Personalised recommendations, ranked by how often the resident has
        /// searched each category this session (see RecommendationEngine). Hidden
        /// entirely until there is at least one real search to base them on, and
        /// always excludes whatever the main gallery above is currently showing -
        /// otherwise, before any search, this would just be duplicating the same
        /// "popular upcoming events" already visible above it. Also hosts the
        /// "Previous Search" button, which pops the most recent search off the
        /// engine's history stack and re-applies it to the filter bar - a LIFO "step
        /// back through what I searched" action, which is why it lives here next to
        /// the feature it drives rather than in the filter bar itself.
        /// </summary>
        private Panel BuildRecommendationsSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 24),
                Visible = false
            };
            _pnlRecommendationsSection = section;

            var header = new Label
            {
                Text = "Recommended For You",
                Dock = DockStyle.Top,
                Height = 30,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor
            };

            _lblRecommendationsSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor
            };

            var actionRow = new Panel { Dock = DockStyle.Top, Height = 42 };
            _btnPreviousSearch = new Button
            {
                Text = "Previous Search",
                Width = 150,
                Height = 32,
                Location = new Point(0, 6),
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            AppTheme.StyleSecondaryButton(_btnPreviousSearch);
            _btnPreviousSearch.Click += btnPreviousSearch_Click;
            actionRow.Controls.Add(_btnPreviousSearch);

            _flpRecommendationsGallery = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                // Intentionally not AutoSize - see FitFlowPanelHeight for why.
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            // Added in reverse visual order (see BuildAnnouncementsTabPanel for why):
            // gallery first, action row next, summary next, header last.
            section.Controls.Add(_flpRecommendationsGallery);
            section.Controls.Add(actionRow);
            section.Controls.Add(_lblRecommendationsSummary);
            section.Controls.Add(header);

            return section;
        }

        private void btnPreviousSearch_Click(object sender, EventArgs e)
        {
            var previous = _recommendationEngine.PopLastSearch();
            if (previous == null)
            {
                MessageBox.Show(
                    "There's no previous search to go back to yet.",
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ApplySearchQueryToFilterBar(previous);
            ExecuteSearch(recordSearch: false);
        }

        /// <summary>
        /// Applies a recorded search's criteria back onto the filter bar controls.
        /// Deliberately does NOT re-record the search - the caller is meant to run
        /// ExecuteSearch(recordSearch: false) afterwards, since re-recording would
        /// just push the same query straight back onto the history stack and defeat
        /// the point of "Previous Search" stepping backwards through it.
        /// </summary>
        private void ApplySearchQueryToFilterBar(SearchQuery query)
        {
            var categoryIndex = string.IsNullOrWhiteSpace(query.Category)
                ? 0
                : _cmbCategory.Items.IndexOf(query.Category);
            _cmbCategory.SelectedIndex = categoryIndex >= 0 ? categoryIndex : 0;

            _dtpFrom.Value = query.FromDate ?? _defaultFromDate;
            _dtpTo.Value = query.ToDate ?? _defaultToDate;
            _txtKeyword.Text = query.Keyword ?? string.Empty;
        }

        /// <summary>
        /// Refreshes the recommendations section against whatever the main events
        /// gallery is currently showing (<paramref name="currentlyVisibleEvents"/>),
        /// so recommended events always exclude those already on screen. The whole
        /// section hides itself when there's no real search history yet, or when
        /// everything it would otherwise recommend is already visible above - both
        /// cases where showing it would just be a duplicate of the main gallery.
        /// </summary>
        private void RefreshRecommendationsGallery(List<Event> currentlyVisibleEvents)
        {
            if (_recommendationEngine.SearchCount == 0)
            {
                _pnlRecommendationsSection.Visible = false;
                ForceScrollableRelayout(_scrollEventsPanel, _flpEventsGallery, _flpRecommendationsGallery);
                return;
            }

            var excludeIds = (currentlyVisibleEvents ?? new List<Event>()).Select(e => e.Id);
            var recommendations = _recommendationEngine.GetRecommendations(6, excludeIds);

            _pnlRecommendationsSection.Visible = recommendations.Count > 0;

            _flpRecommendationsGallery.SuspendLayout();

            foreach (Control control in _flpRecommendationsGallery.Controls)
            {
                control.Dispose();
            }
            _flpRecommendationsGallery.Controls.Clear();

            foreach (var recommended in recommendations)
            {
                _flpRecommendationsGallery.Controls.Add(
                    CreateEventCard(recommended.Event, isRecommended: true, reason: recommended.Reason));
            }

            _flpRecommendationsGallery.ResumeLayout();

            _lblRecommendationsSummary.Text =
                $"Based on {_recommendationEngine.SearchCount} search{(_recommendationEngine.SearchCount == 1 ? string.Empty : "es")} this session - different from what's shown above.";

            ForceScrollableRelayout(_scrollEventsPanel, _flpEventsGallery, _flpRecommendationsGallery);
        }

        private Panel BuildAnnouncementsSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 24)
            };

            var header = new Label
            {
                Text = "Municipal Announcements",
                Dock = DockStyle.Top,
                Height = 30,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor
            };

            _lblAnnouncementsResultsSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor
            };

            _flpAnnouncementsGallery = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                // Intentionally not AutoSize - see FitFlowPanelHeight for why.
                // Height is computed and assigned manually every time the card
                // list changes.
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            section.Controls.Add(_flpAnnouncementsGallery);
            section.Controls.Add(_lblAnnouncementsResultsSummary);
            section.Controls.Add(header);

            return section;
        }

        private Panel BuildSubmitAnnouncementSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 24)
            };

            var header = new Label
            {
                Text = "Submit an Announcement Request",
                Dock = DockStyle.Top,
                Height = 30,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor
            };

            var hint = new Label
            {
                Text = "Suggest an announcement for the municipality to review. Submitted requests appear "
                     + "in Pending Review below until a municipal officer publishes them to the board.",
                Dock = DockStyle.Top,
                Height = 34,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor
            };

            var formCard = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(16),
                MaximumSize = new Size(640, 0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

            layout.Controls.Add(BuildFormLabel("Title"), 0, 0);
            _txtAnnouncementTitle = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.BodyFont, Margin = new Padding(0, 4, 0, 4) };
            layout.Controls.Add(_txtAnnouncementTitle, 1, 0);

            layout.Controls.Add(BuildFormLabel("Category"), 0, 1);
            _cmbAnnouncementCategory = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 4, 0, 4) };
            foreach (var category in EventsRepository.Categories.OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
            {
                _cmbAnnouncementCategory.Items.Add(category);
            }
            layout.Controls.Add(_cmbAnnouncementCategory, 1, 1);

            layout.Controls.Add(BuildFormLabel("Message"), 0, 2);
            _txtAnnouncementMessage = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.BodyFont, Multiline = true, Margin = new Padding(0, 4, 0, 4) };
            layout.Controls.Add(_txtAnnouncementMessage, 1, 2);

            _btnSubmitAnnouncement = new Button { Text = "Submit Request", Width = 160, Height = 34, Margin = new Padding(0, 6, 0, 0), Anchor = AnchorStyles.Left };
            AppTheme.StylePrimaryButton(_btnSubmitAnnouncement);
            _btnSubmitAnnouncement.Click += btnSubmitAnnouncement_Click;
            layout.Controls.Add(_btnSubmitAnnouncement, 1, 3);

            formCard.Controls.Add(layout);

            section.Controls.Add(formCard);
            section.Controls.Add(hint);
            section.Controls.Add(header);

            return section;
        }

        private Panel BuildPendingReviewSection()
        {
            var section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 0, 24)
            };

            var header = new Label
            {
                Text = "Pending Review",
                Dock = DockStyle.Top,
                Height = 30,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor
            };

            _lblPendingSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor
            };

            _lstPendingAnnouncements = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 110,
                Font = AppTheme.SmallFont,
                IntegralHeight = false
            };

            var buttonWrapper = new Panel { Dock = DockStyle.Top, Height = 46 };
            _btnPublishNext = new Button
            {
                Text = "Publish Next",
                Width = 160,
                Height = 34,
                Location = new Point(0, 8),
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            AppTheme.StyleSecondaryButton(_btnPublishNext);
            _btnPublishNext.Click += btnPublishNext_Click;
            buttonWrapper.Controls.Add(_btnPublishNext);

            section.Controls.Add(buttonWrapper);
            section.Controls.Add(_lstPendingAnnouncements);
            section.Controls.Add(_lblPendingSummary);
            section.Controls.Add(header);

            return section;
        }

        private static Label BuildFormLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = AppTheme.SmallFont,
                ForeColor = AppTheme.SecondaryColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void btnSubmitAnnouncement_Click(object sender, EventArgs e)
        {
            var title = _txtAnnouncementTitle.Text?.Trim();
            var message = _txtAnnouncementMessage.Text?.Trim();
            var category = _cmbAnnouncementCategory.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show(
                    "Please fill in a title, category and message before submitting.",
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            EventsRepository.SubmitAnnouncement(new Announcement
            {
                Title = title,
                Message = message,
                Category = category
            });

            _txtAnnouncementTitle.Clear();
            _txtAnnouncementMessage.Clear();
            _cmbAnnouncementCategory.SelectedIndex = -1;

            RefreshPendingList();

            MessageBox.Show(
                "Thank you. Your announcement request has been submitted for review.",
                "Municipal Services App",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnPublishNext_Click(object sender, EventArgs e)
        {
            var published = EventsRepository.PublishNextPendingAnnouncement();
            if (published == null)
            {
                MessageBox.Show(
                    "There are no pending announcement requests to publish.",
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            RefreshAnnouncementsGallery();
            RefreshPendingList();
        }

        private void RefreshAnnouncementsGallery()
        {
            var announcements = EventsRepository.GetRankedAnnouncements();

            _flpAnnouncementsGallery.SuspendLayout();

            foreach (Control control in _flpAnnouncementsGallery.Controls)
            {
                control.Dispose();
            }
            _flpAnnouncementsGallery.Controls.Clear();

            foreach (var announcement in announcements)
            {
                _flpAnnouncementsGallery.Controls.Add(CreateAnnouncementCard(announcement));
            }

            _flpAnnouncementsGallery.ResumeLayout();

            _lblAnnouncementsResultsSummary.Text = announcements.Count == 0
                ? "No announcements have been published yet."
                : $"{announcements.Count} published announcement{(announcements.Count == 1 ? string.Empty : "s")}.";

            UpdateAnnouncementsBadge(announcements);
            ForceScrollableRelayout(_scrollAnnouncementsPanel, _flpAnnouncementsGallery);
        }

        /// <summary>
        /// Recomputes how many published announcements the resident hasn't viewed yet
        /// and reflects that count on the Announcements tab's notification badge.
        /// </summary>
        private void UpdateAnnouncementsBadge(List<Announcement> announcements)
        {
            var unreadCount = announcements.Count(a => !_readAnnouncementIds.Contains(a.Id));
            _announcementsBadge.Count = unreadCount;
        }

        /// <summary>Marks every currently published announcement as read, clearing the notification badge.</summary>
        private void MarkAnnouncementsAsRead()
        {
            var announcements = EventsRepository.GetRankedAnnouncements();
            foreach (var announcement in announcements)
            {
                _readAnnouncementIds.Add(announcement.Id);
            }

            UpdateAnnouncementsBadge(announcements);
        }

        private void RefreshPendingList()
        {
            var pending = EventsRepository.PendingAnnouncements;

            _lstPendingAnnouncements.Items.Clear();
            foreach (var announcement in pending)
            {
                _lstPendingAnnouncements.Items.Add(FormatPendingAnnouncement(announcement));
            }

            _lblPendingSummary.Text = pending.Count == 0
                ? "No resident requests are waiting for review."
                : $"{pending.Count} request{(pending.Count == 1 ? string.Empty : "s")} awaiting review.";

            _btnPublishNext.Enabled = pending.Count > 0;
        }

        private static string FormatPendingAnnouncement(Announcement announcement)
        {
            return $"{announcement.Title}  -  {announcement.Category}  -  submitted {announcement.DatePosted:dd MMM yyyy HH:mm}";
        }

        private static AnnouncementCardControl CreateAnnouncementCard(Announcement announcement)
        {
            return new AnnouncementCardControl
            {
                Title = announcement.Title,
                Message = announcement.Message,
                Category = announcement.Category,
                DatePosted = announcement.DatePosted,
                Priority = announcement.Priority,
                IsResidentSubmitted = announcement.IsResidentSubmitted
            };
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

            // Recommendations depend on category search frequency, which just
            // changed above (when recordSearch is true) - and "Previous Search"
            // re-applying an older query (recordSearch: false) should still reflect
            // that query's results, so refresh unconditionally either way. Pass the
            // current results so recommendations can exclude whatever's already
            // visible in the main gallery above.
            RefreshRecommendationsGallery(results);
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

            ForceScrollableRelayout(_scrollEventsPanel, _flpEventsGallery);
        }

        private static EventCardControl CreateEventCard(Event evt, bool isRecommended = false, string reason = null)
        {
            var card = new EventCardControl
            {
                EventId = evt.Id,
                Title = evt.Title,
                Category = evt.Category,
                EventLocation = evt.Location,
                Description = evt.Description,
                EventDate = evt.EventDate,
                IsRecommended = isRecommended,
                RecommendationReason = reason ?? string.Empty
            };

            card.Click += (s, e) => ShowEventDetails(card.EventId);
            return card;
        }

        /// <summary>
        /// Looks the full event back up by id (demonstrating a genuine use of
        /// <see cref="EventsRepository.GetEventById"/>'s dictionary lookup, rather
        /// than the card just carrying its own separate copy of every field) and
        /// shows it in a simple detail popup.
        /// </summary>
        private static void ShowEventDetails(string eventId)
        {
            var evt = EventsRepository.GetEventById(eventId);
            if (evt == null)
            {
                return;
            }

            var message =
                $"{evt.Title}\n\n" +
                $"Category: {evt.Category}\n" +
                $"Date: {evt.EventDate:dddd, dd MMMM yyyy}\n" +
                $"Location: {evt.Location}\n\n" +
                evt.Description;

            MessageBox.Show(message, "Event Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.DataStructures;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// The resident-facing tabs: Track a Request (look one request up by its reference
    /// number in the AVL tree) and All Requests (every request, newest first, from the
    /// red-black date tree, filtered by status, ward and the service category tree).
    /// </summary>
    public partial class ServiceRequestStatusForm
    {
        private const int TrackTabIndex = 0;

        // Track a Request tab
        private TextBox _txtReference;
        private Label _lblTrackMessage;
        private TableLayoutPanel _trackResult;
        private Label _lblResultReference;
        private Label _lblResultStatus;
        private Label _lblResultPriority;
        private Label _lblSearchCost;
        private StepIndicatorControl _timeline;
        private Label _lblProgress;
        private Label _lblCategory;
        private Label _lblDepartment;
        private Label _lblWard;
        private Label _lblCrew;
        private Label _lblReported;
        private Label _lblLastUpdated;
        private Label _lblLocation;
        private Label _lblDescription;
        private ListView _lvHistory;
        private Issue _trackedRequest;

        // All Requests tab
        private ComboBox _cmbStatusFilter;
        private ComboBox _cmbDateFilter;
        private ComboBox _cmbWardFilter;
        private TreeView _tvCategories;
        private Label _lblAllSummary;
        private ListView _lvAllRequests;
        private bool _suppressFilterEvents;

        // ----------------------------------------------------------------
        // Track a Request tab
        // ----------------------------------------------------------------

        private Panel BuildTrackTabPanel()
        {
            var tabPanel = new Panel();

            var searchBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(24, 14, 24, 14)
            };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            _txtReference = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.BodyFont, Margin = new Padding(0, 3, 8, 0), CharacterCasing = CharacterCasing.Upper };
            _txtReference.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    TrackRequest(_txtReference.Text);
                }
            };

            var btnTrack = new Button { Text = "Track", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            AppTheme.StylePrimaryButton(btnTrack);
            btnTrack.Click += (s, e) => TrackRequest(_txtReference.Text);

            layout.Controls.Add(CreateFieldName("Reference number"), 0, 0);
            layout.Controls.Add(_txtReference, 1, 0);
            layout.Controls.Add(btnTrack, 2, 0);
            layout.Controls.Add(CreateHint("Use the number shown when you reported the issue, for example REQ-2026-0001."), 3, 0);
            searchBar.Controls.Add(layout);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 12, 24, 24), BackColor = AppTheme.BackgroundColor };

            _lblTrackMessage = new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                Font = AppTheme.BodyFont,
                Padding = new Padding(12, 0, 12, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            AppTheme.StyleInfoBanner(_lblTrackMessage);

            _trackResult = BuildTrackResult();
            _trackResult.Visible = false;

            body.Controls.Add(_trackResult);
            body.Controls.Add(_lblTrackMessage);

            tabPanel.Controls.Add(body);
            tabPanel.Controls.Add(searchBar);

            _refreshers.Add(() =>
            {
                if (_trackedRequest != null)
                {
                    ShowTrackedRequest(_trackedRequest, null);
                }
            });

            var lastReported = IssueRepository.LastReported;
            if (lastReported != null)
            {
                _txtReference.Text = lastReported.ReferenceNumber;
                TrackRequest(lastReported.ReferenceNumber);
            }
            else
            {
                ShowTrackMessage("Enter your reference number above and press Track to see where your request is.", isError: false);
            }

            return tabPanel;
        }

        private TableLayoutPanel BuildTrackResult()
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, BackColor = Color.White, Padding = new Padding(16, 8, 16, 12) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 144F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Row 0: reference number with status and priority pills.
            var header = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0) };
            _lblResultReference = new Label { AutoSize = true, Font = AppTheme.HeadingFont, ForeColor = AppTheme.PrimaryColor, Margin = new Padding(0, 8, 16, 0) };
            _lblResultStatus = CreatePill();
            _lblResultStatus.Margin = new Padding(0, 9, 8, 0);
            _lblResultPriority = CreatePill();
            _lblResultPriority.Margin = new Padding(0, 9, 8, 0);
            header.Controls.Add(_lblResultReference);
            header.Controls.Add(_lblResultStatus);
            header.Controls.Add(_lblResultPriority);

            // Row 1: how the lookup was done.
            _lblSearchCost = CreateHint(string.Empty);

            // Row 2: the workflow's main path as a timeline.
            var timelineFrame = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.HeaderBarColor, Padding = new Padding(8, 4, 8, 0), Margin = new Padding(0, 4, 0, 4) };
            _timeline = new StepIndicatorControl { Dock = DockStyle.Fill };
            timelineFrame.Controls.Add(_timeline);

            // Row 3: what is left before the request closes.
            _lblProgress = new Label { Dock = DockStyle.Fill, Font = AppTheme.LabelFont, ForeColor = AppTheme.PrimaryColor, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };

            // Row 4: request details in two label/value columns.
            var details = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 5, Margin = new Padding(0, 4, 0, 0) };
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (var i = 0; i < 4; i++)
            {
                details.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            }
            details.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _lblCategory = CreateFieldValue();
            _lblDepartment = CreateFieldValue();
            _lblWard = CreateFieldValue();
            _lblCrew = CreateFieldValue();
            _lblReported = CreateFieldValue();
            _lblLastUpdated = CreateFieldValue();
            _lblLocation = CreateFieldValue();
            _lblDescription = CreateFieldValue();
            _lblDescription.TextAlign = ContentAlignment.TopLeft;
            _lblDescription.Padding = new Padding(0, 5, 0, 0);

            AddDetail(details, 0, 0, "Category", _lblCategory);
            AddDetail(details, 0, 2, "Department", _lblDepartment);
            AddDetail(details, 1, 0, "Ward", _lblWard);
            AddDetail(details, 1, 2, "Assigned crew", _lblCrew);
            AddDetail(details, 2, 0, "Reported", _lblReported);
            AddDetail(details, 2, 2, "Last updated", _lblLastUpdated);
            AddDetail(details, 3, 0, "Location", _lblLocation);
            details.SetColumnSpan(_lblLocation, 3);
            var descriptionName = CreateFieldName("Description");
            descriptionName.TextAlign = ContentAlignment.TopLeft;
            descriptionName.Padding = new Padding(0, 5, 0, 0);
            details.Controls.Add(descriptionName, 0, 4);
            details.Controls.Add(_lblDescription, 1, 4);
            details.SetColumnSpan(_lblDescription, 3);

            // Rows 5 and 6: every status change so far.
            _lvHistory = CreateListView(Column("Date", 160), Column("Status", 120), Column("Update", 600));

            table.Controls.Add(header, 0, 0);
            table.Controls.Add(_lblSearchCost, 0, 1);
            table.Controls.Add(timelineFrame, 0, 2);
            table.Controls.Add(_lblProgress, 0, 3);
            table.Controls.Add(details, 0, 4);
            table.Controls.Add(CreateHeading("Status history"), 0, 5);
            table.Controls.Add(_lvHistory, 0, 6);

            return table;
        }

        private static void AddDetail(TableLayoutPanel details, int row, int column, string name, Label value)
        {
            details.Controls.Add(CreateFieldName(name), column, row);
            details.Controls.Add(value, column + 1, row);
        }

        private void ShowTrackMessage(string text, bool isError)
        {
            _lblTrackMessage.Text = text;
            _lblTrackMessage.BackColor = isError ? Color.FromArgb(251, 234, 234) : AppTheme.InfoBannerColor;
            _lblTrackMessage.ForeColor = isError ? AppTheme.DangerColor : AppTheme.InfoBannerTextColor;
            _lblTrackMessage.Visible = true;
        }

        private void TrackRequest(string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber))
            {
                _trackResult.Visible = false;
                _trackedRequest = null;
                ShowTrackMessage("Please enter a reference number, for example REQ-2026-0001.", isError: true);
                return;
            }

            var request = ServiceRequestRepository.FindByReference(referenceNumber, out var avlSteps, out var bstSteps);
            if (request == null)
            {
                _trackResult.Visible = false;
                _trackedRequest = null;
                ShowTrackMessage(
                    "No request matches \"" + referenceNumber.Trim() + "\". Check the number on your confirmation, or browse the All Requests tab.",
                    isError: true);
                return;
            }

            var searchCost = string.Format(
                "Found by the AVL tree in {0} comparison{1}. An unbalanced binary search tree holding the same {2} requests needed {3}.",
                avlSteps, avlSteps == 1 ? string.Empty : "s", ServiceRequestRepository.Count, bstSteps);
            ShowTrackedRequest(request, searchCost);
        }

        private void OpenInTrackTab(Issue request)
        {
            _txtReference.Text = request.ReferenceNumber;
            TrackRequest(request.ReferenceNumber);
            SelectTab(TrackTabIndex);
        }

        /// <summary>Shows a request's details; pass null for searchCost to keep the previous lookup text.</summary>
        private void ShowTrackedRequest(Issue request, string searchCost)
        {
            _trackedRequest = request;
            _lblTrackMessage.Visible = false;
            _trackResult.Visible = true;

            _lblResultReference.Text = request.ReferenceNumber;
            SetPill(_lblResultStatus, StatusWorkflow.DisplayName(request.Status), StatusColor(request.Status));
            SetPill(_lblResultPriority, request.Priority + " priority", PriorityColor(request.Priority));
            if (searchCost != null)
            {
                _lblSearchCost.Text = searchCost;
            }

            ShowTimeline(request.Status);
            _lblProgress.Text = DescribeProgress(request);

            _lblCategory.Text = string.IsNullOrWhiteSpace(request.SubCategory)
                ? request.Category + " (sub-type set when staff triage it)"
                : request.Category + " > " + request.SubCategory;
            _lblDepartment.Text = ServiceCategoryTree.GetDepartment(request.Category);
            _lblWard.Text = string.IsNullOrWhiteSpace(request.Area) ? ServiceRequestRepository.UnassignedArea : request.Area;
            _lblCrew.Text = string.IsNullOrWhiteSpace(request.AssignedCrew) ? "Not yet assigned" : request.AssignedCrew;
            _lblReported.Text = FormatDate(request.DateReported);
            _lblLastUpdated.Text = FormatDate(request.LastUpdated);
            _lblLocation.Text = string.IsNullOrWhiteSpace(request.AdditionalLocationInfo)
                ? request.Location
                : request.Location + " (" + request.AdditionalLocationInfo + ")";
            _lblDescription.Text = request.Description;

            _lvHistory.BeginUpdate();
            _lvHistory.Items.Clear();
            for (var i = request.StatusHistory.Count - 1; i >= 0; i--)
            {
                var change = request.StatusHistory[i];
                var item = new ListViewItem(FormatDate(change.ChangedAt)) { UseItemStyleForSubItems = false };
                item.SubItems.Add(StatusWorkflow.DisplayName(change.Status), StatusColor(change.Status), Color.White, AppTheme.LabelFont);
                item.SubItems.Add(change.Note ?? string.Empty);
                _lvHistory.Items.Add(item);
            }
            _lvHistory.EndUpdate();
        }

        /// <summary>
        /// Draws the workflow graph's main path as the timeline. On Hold sits in the In
        /// Progress slot (it returns there), and a rejected request shows a two-step path.
        /// </summary>
        private void ShowTimeline(ServiceRequestStatus status)
        {
            if (status == ServiceRequestStatus.Rejected)
            {
                _timeline.StepLabels = StatusWorkflow.DisplayNames(new[] { ServiceRequestStatus.Submitted, ServiceRequestStatus.Rejected });
                _timeline.CurrentStepIndex = 2;
                return;
            }

            var path = StatusWorkflow.MainPath();
            var labels = StatusWorkflow.DisplayNames(path);
            var slot = status == ServiceRequestStatus.OnHold ? ServiceRequestStatus.InProgress : status;
            var index = path.IndexOf(slot);

            if (status == ServiceRequestStatus.OnHold)
            {
                labels[index] = "On Hold";
            }

            // Closed is the last step, so mark it done rather than current.
            _timeline.StepLabels = labels;
            _timeline.CurrentStepIndex = status == ServiceRequestStatus.Closed ? path.Count : index;
        }

        private static string DescribeProgress(Issue request)
        {
            switch (request.Status)
            {
                case ServiceRequestStatus.Closed:
                    return request.Rating.HasValue
                        ? "This request is closed. You rated the service " + request.Rating.Value + " out of 5."
                        : "This request is closed. Thank you for reporting it.";
                case ServiceRequestStatus.Rejected:
                    var reason = request.StatusHistory.LastOrDefault()?.Note;
                    return "This request was not accepted." + (string.IsNullOrWhiteSpace(reason) ? string.Empty : " " + reason);
            }

            var path = StatusWorkflow.PathToClosed(request.Status);
            var steps = path.Count - 1;
            return string.Format(
                "{0} more step{1} until closed: {2}",
                steps, steps == 1 ? string.Empty : "s", string.Join(" > ", StatusWorkflow.DisplayNames(path.Skip(1))));
        }

        // ----------------------------------------------------------------
        // All Requests tab
        // ----------------------------------------------------------------

        private Panel BuildAllRequestsTabPanel()
        {
            var tabPanel = new Panel();

            var filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(24, 14, 24, 14)
            };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            _cmbStatusFilter = CreateFilterCombo();
            _cmbStatusFilter.Items.Add(new ComboOption<Func<Issue, bool>>("All statuses", r => true));
            _cmbStatusFilter.Items.Add(new ComboOption<Func<Issue, bool>>("Open requests only", r => r.IsOpen));
            foreach (var status in StatusWorkflow.AllStatusesInWorkflowOrder())
            {
                var captured = status;
                _cmbStatusFilter.Items.Add(new ComboOption<Func<Issue, bool>>(StatusWorkflow.DisplayName(status), r => r.Status == captured));
            }

            _cmbDateFilter = CreateFilterCombo();
            _cmbDateFilter.Items.Add(new ComboOption<int?>("Reported any time", null));
            _cmbDateFilter.Items.Add(new ComboOption<int?>("Reported in the last 7 days", 7));
            _cmbDateFilter.Items.Add(new ComboOption<int?>("Reported in the last 30 days", 30));

            _cmbWardFilter = CreateFilterCombo();
            _cmbWardFilter.Items.Add(new ComboOption<string>("All wards", null));
            foreach (var area in ServiceRequestRepository.Areas)
            {
                _cmbWardFilter.Items.Add(new ComboOption<string>(area, area));
            }
            _cmbWardFilter.Items.Add(new ComboOption<string>(ServiceRequestRepository.UnassignedArea, ServiceRequestRepository.UnassignedArea));

            var btnClear = new Button { Text = "Clear filters", Dock = DockStyle.Fill };
            AppTheme.StyleSecondaryButton(btnClear);
            btnClear.Click += (s, e) => ClearAllRequestFilters();

            layout.Controls.Add(_cmbStatusFilter, 0, 0);
            layout.Controls.Add(_cmbDateFilter, 1, 0);
            layout.Controls.Add(_cmbWardFilter, 2, 0);
            layout.Controls.Add(btnClear, 3, 0);
            filterBar.Controls.Add(layout);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(24, 12, 24, 24), BackColor = AppTheme.BackgroundColor };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _tvCategories = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = AppTheme.BodyFont,
                HideSelection = false,
                ItemHeight = 24,
                Margin = new Padding(0, 0, 16, 0)
            };
            _tvCategories.Nodes.Add(BuildCategoryNode(ServiceCategoryTree.Tree.Root));
            _tvCategories.ExpandAll();
            _tvCategories.SelectedNode = _tvCategories.Nodes[0];
            _tvCategories.AfterSelect += (s, e) =>
            {
                if (!_suppressFilterEvents)
                {
                    RefreshAllRequests();
                }
            };

            _lblAllSummary = new Label { Dock = DockStyle.Fill, Font = AppTheme.BodyFont, ForeColor = AppTheme.SecondaryColor, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };

            _lvAllRequests = CreateListView(
                Column("Reference", 115), Column("Reported", 135), Column("Category", 90), Column("Sub-type", 160),
                Column("Ward", 150), Column("Priority", 70), Column("Status", 95), Column("Last update", 130));
            _lvAllRequests.ItemActivate += (s, e) =>
            {
                if (_lvAllRequests.SelectedItems.Count > 0)
                {
                    OpenInTrackTab((Issue)_lvAllRequests.SelectedItems[0].Tag);
                }
            };

            body.Controls.Add(CreateHeading("Service category"), 0, 0);
            body.Controls.Add(_tvCategories, 0, 1);
            body.Controls.Add(_lblAllSummary, 1, 0);
            body.Controls.Add(_lvAllRequests, 1, 1);

            tabPanel.Controls.Add(body);
            tabPanel.Controls.Add(filterBar);

            _suppressFilterEvents = true;
            _cmbStatusFilter.SelectedIndex = 0;
            _cmbDateFilter.SelectedIndex = 0;
            _cmbWardFilter.SelectedIndex = 0;
            _suppressFilterEvents = false;

            _refreshers.Add(RefreshAllRequests);
            return tabPanel;
        }

        private ComboBox CreateFilterCombo()
        {
            var combo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 0, 8, 0) };
            combo.SelectedIndexChanged += (s, e) =>
            {
                if (!_suppressFilterEvents)
                {
                    RefreshAllRequests();
                }
            };
            return combo;
        }

        private static TreeNode BuildCategoryNode(GeneralTreeNode<string> categoryNode)
        {
            var treeNode = new TreeNode(categoryNode.Value) { Tag = categoryNode };
            foreach (var child in categoryNode.Children)
            {
                treeNode.Nodes.Add(BuildCategoryNode(child));
            }
            return treeNode;
        }

        private void ClearAllRequestFilters()
        {
            _suppressFilterEvents = true;
            _cmbStatusFilter.SelectedIndex = 0;
            _cmbDateFilter.SelectedIndex = 0;
            _cmbWardFilter.SelectedIndex = 0;
            _tvCategories.SelectedNode = _tvCategories.Nodes[0];
            _suppressFilterEvents = false;
            RefreshAllRequests();
        }

        /// <summary>
        /// Picks the best index for the filters chosen - the ward BST when a ward is
        /// picked, otherwise the red-black date tree (a range query when a date window is
        /// picked) - then applies the remaining filters to that smaller list.
        /// </summary>
        private void RefreshAllRequests()
        {
            var statusFilter = ((ComboOption<Func<Issue, bool>>)_cmbStatusFilter.SelectedItem).Value;
            var days = ((ComboOption<int?>)_cmbDateFilter.SelectedItem).Value;
            var ward = ((ComboOption<string>)_cmbWardFilter.SelectedItem).Value;
            var categoryNode = _tvCategories.SelectedNode?.Tag as GeneralTreeNode<string>;
            var since = days.HasValue ? DateTime.Today.AddDays(-days.Value) : DateTime.MinValue;

            IEnumerable<Issue> source;
            string indexUsed;
            if (ward != null)
            {
                source = ServiceRequestRepository.GetByArea(ward)
                    .Where(r => r.DateReported >= since)
                    .OrderByDescending(r => r.DateReported);
                indexUsed = "ward binary search tree lookup";
            }
            else if (days.HasValue)
            {
                source = ServiceRequestRepository.GetReportedBetween(since, DateTime.MaxValue);
                indexUsed = "red-black tree date range query";
            }
            else
            {
                source = ServiceRequestRepository.GetNewestFirst();
                indexUsed = "red-black tree reverse in-order walk";
            }

            var results = source
                .Where(statusFilter)
                .Where(r => ServiceCategoryTree.Matches(categoryNode, r.Category, r.SubCategory))
                .ToList();

            _lvAllRequests.BeginUpdate();
            _lvAllRequests.Items.Clear();
            foreach (var request in results)
            {
                var item = new ListViewItem(request.ReferenceNumber) { Tag = request, UseItemStyleForSubItems = false };
                item.SubItems.Add(FormatDate(request.DateReported));
                item.SubItems.Add(request.Category);
                item.SubItems.Add(request.SubCategory ?? "Not yet triaged");
                item.SubItems.Add(string.IsNullOrWhiteSpace(request.Area) ? ServiceRequestRepository.UnassignedArea : request.Area);
                item.SubItems.Add(request.Priority.ToString(), PriorityColor(request.Priority), Color.White, AppTheme.BodyFont);
                item.SubItems.Add(StatusWorkflow.DisplayName(request.Status), StatusColor(request.Status), Color.White, AppTheme.LabelFont);
                item.SubItems.Add(FormatDate(request.LastUpdated));
                _lvAllRequests.Items.Add(item);
            }
            _lvAllRequests.EndUpdate();

            _lblAllSummary.Text = string.Format(
                "Showing {0} of {1} requests, newest first ({2}). Double-click one to track it.",
                results.Count, ServiceRequestRepository.Count, indexUsed);

            UpdateCategoryCounts(_tvCategories.Nodes[0]);
        }

        /// <summary>Appends how many requests fall under each category tree node, e.g. "Roads (5)".</summary>
        private static void UpdateCategoryCounts(TreeNode treeNode)
        {
            var categoryNode = (GeneralTreeNode<string>)treeNode.Tag;
            var count = ServiceRequestRepository.All.Count(r => ServiceCategoryTree.Matches(categoryNode, r.Category, r.SubCategory));
            treeNode.Text = categoryNode.Value + " (" + count + ")";

            foreach (TreeNode child in treeNode.Nodes)
            {
                UpdateCategoryCounts(child);
            }
        }
    }
}

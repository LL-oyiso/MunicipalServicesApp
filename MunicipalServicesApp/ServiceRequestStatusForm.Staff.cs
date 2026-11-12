using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// The Staff tab: the open requests in heap order (most urgent first, then oldest),
    /// and an editor that triages the selected request and moves it to one of the
    /// statuses the workflow graph allows next.
    /// </summary>
    public partial class ServiceRequestStatusForm
    {
        private Label _lblNextJob;
        private ListView _lvWorkQueue;
        private Panel _pnlEditor;
        private TableLayoutPanel _editor;
        private Label _lblEditorEmpty;
        private Label _lblEditorReference;
        private Label _lblEditorStatus;
        private Label _lblEditorSummary;
        private ComboBox _cmbEditWard;
        private ComboBox _cmbEditSubCategory;
        private ComboBox _cmbEditPriority;
        private ComboBox _cmbEditCrew;
        private ComboBox _cmbNextStatus;
        private Label _lblAllowedMoves;
        private TextBox _txtStatusNote;
        private Label _lblStaffFeedback;
        private Issue _staffSelected;

        private Panel BuildStaffTabPanel()
        {
            var tabPanel = new Panel();

            var banner = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Font = AppTheme.BodyFont,
                Padding = new Padding(24, 0, 24, 0),
                Text = "Staff view for municipal officials. In a live system this tab would only be shown after a staff login.",
                TextAlign = ContentAlignment.MiddleLeft
            };
            AppTheme.StyleInfoBanner(banner);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(24, 8, 24, 24), BackColor = AppTheme.BackgroundColor };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _lblNextJob = new Label { Dock = DockStyle.Fill, Font = AppTheme.LabelFont, ForeColor = AppTheme.DangerColor, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };

            _lvWorkQueue = CreateListView(
                Column("#", 36), Column("Reference", 115), Column("Priority", 75), Column("Status", 95),
                Column("Category", 90), Column("Waiting", 90));
            _lvWorkQueue.Margin = new Padding(0, 0, 16, 0);
            _lvWorkQueue.SelectedIndexChanged += (s, e) =>
            {
                if (_lvWorkQueue.SelectedItems.Count > 0)
                {
                    var request = (Issue)_lvWorkQueue.SelectedItems[0].Tag;
                    if (request != _staffSelected)
                    {
                        ShowStaffFeedback(string.Empty, isError: false);
                    }
                    ShowInEditor(request);
                }
            };

            body.Controls.Add(CreateHeading("Work queue"), 0, 0);
            body.Controls.Add(_lblNextJob, 0, 1);
            body.Controls.Add(_lvWorkQueue, 0, 2);
            body.Controls.Add(CreateHeading("Update request"), 1, 0);
            body.Controls.Add(CreateHint("Open requests leave a binary min-heap by priority, then by how long they have waited."), 1, 1);

            _pnlEditor = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16, 12, 16, 12) };
            _lblEditorEmpty = new Label
            {
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                ForeColor = AppTheme.SecondaryColor,
                Text = "Select a request in the work queue to triage it or move it to its next status.",
                TextAlign = ContentAlignment.MiddleCenter
            };
            _editor = BuildEditor();
            _editor.Visible = false;
            _pnlEditor.Controls.Add(_editor);
            _pnlEditor.Controls.Add(_lblEditorEmpty);
            body.Controls.Add(_pnlEditor, 1, 2);

            tabPanel.Controls.Add(body);
            tabPanel.Controls.Add(banner);

            _refreshers.Add(RefreshWorkQueue);
            return tabPanel;
        }

        private TableLayoutPanel BuildEditor()
        {
            var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 12 };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));  // reference + status
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));  // summary
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));  // triage heading
            for (var i = 0; i < 4; i++)
            {
                editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F)); // ward, sub-type, priority, crew
            }
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));  // status heading
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));  // move to
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));  // allowed moves hint
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));  // note
            editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // save + feedback

            var header = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0) };
            _lblEditorReference = new Label { AutoSize = true, Font = AppTheme.HeadingFont, ForeColor = AppTheme.PrimaryColor, Margin = new Padding(0, 6, 16, 0) };
            _lblEditorStatus = CreatePill();
            _lblEditorStatus.Margin = new Padding(0, 7, 0, 0);
            header.Controls.Add(_lblEditorReference);
            header.Controls.Add(_lblEditorStatus);
            editor.Controls.Add(header, 0, 0);
            editor.SetColumnSpan(header, 2);

            _lblEditorSummary = CreateFieldValue();
            editor.Controls.Add(_lblEditorSummary, 0, 1);
            editor.SetColumnSpan(_lblEditorSummary, 2);

            var triageHeading = CreateFieldName("Triage");
            triageHeading.ForeColor = AppTheme.AccentColor;
            editor.Controls.Add(triageHeading, 0, 2);

            _cmbEditWard = CreateEditorCombo();
            _cmbEditWard.Items.Add(ServiceRequestRepository.UnassignedArea);
            _cmbEditWard.Items.AddRange(ServiceRequestRepository.Areas.Cast<object>().ToArray());

            _cmbEditSubCategory = CreateEditorCombo();

            _cmbEditPriority = CreateEditorCombo();
            foreach (RequestPriority priority in Enum.GetValues(typeof(RequestPriority)))
            {
                _cmbEditPriority.Items.Add(priority);
            }

            _cmbEditCrew = CreateEditorCombo();
            _cmbEditCrew.Items.Add("Not yet assigned");
            _cmbEditCrew.Items.AddRange(ServiceRequestRepository.Crews.Cast<object>().ToArray());

            AddEditorRow(editor, 3, "Ward", _cmbEditWard);
            AddEditorRow(editor, 4, "Sub-type", _cmbEditSubCategory);
            AddEditorRow(editor, 5, "Priority", _cmbEditPriority);
            AddEditorRow(editor, 6, "Crew", _cmbEditCrew);

            var statusHeading = CreateFieldName("Status");
            statusHeading.ForeColor = AppTheme.AccentColor;
            editor.Controls.Add(statusHeading, 0, 7);

            _cmbNextStatus = CreateEditorCombo();
            AddEditorRow(editor, 8, "Move to", _cmbNextStatus);

            _lblAllowedMoves = CreateHint(string.Empty);
            editor.Controls.Add(_lblAllowedMoves, 1, 9);

            _txtStatusNote = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.BodyFont, Multiline = true, Margin = new Padding(0, 3, 0, 3) };
            var noteName = CreateFieldName("Note to resident");
            noteName.TextAlign = ContentAlignment.TopLeft;
            noteName.Padding = new Padding(0, 6, 0, 0);
            editor.Controls.Add(noteName, 0, 10);
            editor.Controls.Add(_txtStatusNote, 1, 10);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Margin = new Padding(0, 8, 0, 0) };
            var btnSave = new Button { Text = "Save changes", Size = new Size(150, 38), Margin = new Padding(0, 0, 16, 0) };
            AppTheme.StylePrimaryButton(btnSave);
            btnSave.Click += (s, e) => SaveStaffChanges();
            _lblStaffFeedback = new Label { AutoSize = false, Size = new Size(380, 38), Font = AppTheme.LabelFont, TextAlign = ContentAlignment.MiddleLeft };
            actions.Controls.Add(btnSave);
            actions.Controls.Add(_lblStaffFeedback);
            editor.Controls.Add(actions, 1, 11);

            return editor;
        }

        private static ComboBox CreateEditorCombo()
        {
            return new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 3, 0, 3) };
        }

        private static void AddEditorRow(TableLayoutPanel editor, int row, string name, Control input)
        {
            editor.Controls.Add(CreateFieldName(name), 0, row);
            editor.Controls.Add(input, 1, row);
        }

        /// <summary>Rebuilds the queue from the heap, keeping the selected request selected.</summary>
        private void RefreshWorkQueue()
        {
            var queue = ServiceRequestRepository.GetWorkQueue();
            var selectedReference = _staffSelected?.ReferenceNumber;

            _lvWorkQueue.BeginUpdate();
            _lvWorkQueue.Items.Clear();
            ListViewItem toSelect = null;
            for (var i = 0; i < queue.Count; i++)
            {
                var request = queue[i];
                var item = new ListViewItem((i + 1).ToString()) { Tag = request, UseItemStyleForSubItems = false };
                item.SubItems.Add(request.ReferenceNumber);
                item.SubItems.Add(request.Priority.ToString(), PriorityColor(request.Priority), Color.White, AppTheme.LabelFont);
                item.SubItems.Add(StatusWorkflow.DisplayName(request.Status), StatusColor(request.Status), Color.White, AppTheme.LabelFont);
                item.SubItems.Add(request.Category);
                item.SubItems.Add(DescribeWaiting(request.DateReported));
                _lvWorkQueue.Items.Add(item);

                if (request.ReferenceNumber == selectedReference)
                {
                    toSelect = item;
                }
            }
            _lvWorkQueue.EndUpdate();

            var next = ServiceRequestRepository.PeekNextJob();
            _lblNextJob.Text = next == null
                ? "No open requests. Every request is closed or rejected."
                : string.Format("Next job: {0}, {1} priority, {2} in {3}.",
                    next.ReferenceNumber, next.Priority, next.SubCategory ?? next.Category,
                    string.IsNullOrWhiteSpace(next.Area) ? "an untriaged ward" : next.Area);

            if (toSelect != null)
            {
                toSelect.Selected = true;
                toSelect.EnsureVisible();
            }
            else if (_staffSelected != null)
            {
                // Closed or rejected, so it has left the queue; keep showing it in the editor.
                ShowInEditor(_staffSelected);
            }
        }

        private static string DescribeWaiting(DateTime reported)
        {
            var days = (int)(DateTime.Now - reported).TotalDays;
            return days == 0 ? "Today" : days == 1 ? "1 day" : days + " days";
        }

        private void ShowInEditor(Issue request)
        {
            _staffSelected = request;
            _lblEditorEmpty.Visible = false;
            _editor.Visible = true;

            _lblEditorReference.Text = request.ReferenceNumber;
            SetPill(_lblEditorStatus, StatusWorkflow.DisplayName(request.Status), StatusColor(request.Status));
            _lblEditorSummary.Text = request.Category + " at " + request.Location + ", reported " + FormatDate(request.DateReported);

            _cmbEditWard.SelectedItem = string.IsNullOrWhiteSpace(request.Area) ? ServiceRequestRepository.UnassignedArea : request.Area;

            _cmbEditSubCategory.Items.Clear();
            _cmbEditSubCategory.Items.Add("Not yet triaged");
            _cmbEditSubCategory.Items.AddRange(ServiceCategoryTree.GetSubCategories(request.Category).Cast<object>().ToArray());
            _cmbEditSubCategory.SelectedItem = request.SubCategory ?? "Not yet triaged";
            if (_cmbEditSubCategory.SelectedIndex < 0)
            {
                _cmbEditSubCategory.SelectedIndex = 0;
            }

            _cmbEditPriority.SelectedItem = request.Priority;
            _cmbEditCrew.SelectedItem = request.AssignedCrew ?? "Not yet assigned";

            // Only the workflow graph's outgoing edges from the current status are offered.
            var nextStatuses = StatusWorkflow.NextStatuses(request.Status);
            _cmbNextStatus.Items.Clear();
            _cmbNextStatus.Items.Add(new ComboOption<ServiceRequestStatus?>("Keep as " + StatusWorkflow.DisplayName(request.Status), null));
            foreach (var status in nextStatuses)
            {
                _cmbNextStatus.Items.Add(new ComboOption<ServiceRequestStatus?>(StatusWorkflow.DisplayName(status), status));
            }
            _cmbNextStatus.SelectedIndex = 0;

            _lblAllowedMoves.Text = nextStatuses.Count == 0
                ? StatusWorkflow.DisplayName(request.Status) + " is final, so the status can no longer change."
                : "The workflow graph allows " + StatusWorkflow.DisplayName(request.Status) + " to move to "
                    + string.Join(" or ", StatusWorkflow.DisplayNames(nextStatuses)) + ". From here it can still reach: "
                    + string.Join(", ", StatusWorkflow.DisplayNames(StatusWorkflow.ReachableStatuses(request.Status))) + ".";

            _txtStatusNote.Text = string.Empty;
        }

        private void SaveStaffChanges()
        {
            var request = _staffSelected;
            if (request == null)
            {
                return;
            }

            var ward = (string)_cmbEditWard.SelectedItem;
            var area = ward == ServiceRequestRepository.UnassignedArea ? null : ward;
            var subCategory = _cmbEditSubCategory.SelectedIndex <= 0 ? null : (string)_cmbEditSubCategory.SelectedItem;
            var crew = _cmbEditCrew.SelectedIndex <= 0 ? null : (string)_cmbEditCrew.SelectedItem;
            var priority = (RequestPriority)_cmbEditPriority.SelectedItem;
            var newStatus = ((ComboOption<ServiceRequestStatus?>)_cmbNextStatus.SelectedItem).Value;
            var note = _txtStatusNote.Text.Trim();

            if (newStatus == ServiceRequestStatus.Assigned && (crew == null || area == null))
            {
                ShowStaffFeedback("Choose a ward and a crew before assigning the request.", isError: true);
                return;
            }

            if (newStatus == ServiceRequestStatus.Rejected && note.Length == 0)
            {
                ShowStaffFeedback("Add a note telling the resident why the request is rejected.", isError: true);
                return;
            }

            ServiceRequestRepository.UpdateTriage(request, area, subCategory, crew, priority);

            if (newStatus.HasValue)
            {
                try
                {
                    ServiceRequestRepository.ChangeStatus(request, newStatus.Value, note.Length > 0 ? note : DefaultNote(newStatus.Value, crew));
                }
                catch (InvalidOperationException ex)
                {
                    ShowStaffFeedback(ex.Message, isError: true);
                    return;
                }
            }

            ShowStaffFeedback(
                newStatus.HasValue
                    ? "Saved. " + request.ReferenceNumber + " is now " + StatusWorkflow.DisplayName(newStatus.Value) + "."
                    : "Saved the triage details for " + request.ReferenceNumber + ".",
                isError: false);

            RefreshAllViews();
            ShowInEditor(request);
        }

        private static string DefaultNote(ServiceRequestStatus status, string crew)
        {
            switch (status)
            {
                case ServiceRequestStatus.Assigned: return "Assigned to " + crew + ".";
                case ServiceRequestStatus.InProgress: return "Crew on site and work has started.";
                case ServiceRequestStatus.OnHold: return "Work paused while we wait for parts or access.";
                case ServiceRequestStatus.Resolved: return "Work completed. Please let us know if the problem returns.";
                case ServiceRequestStatus.Closed: return "Request closed. Thank you for reporting it.";
                default: return null;
            }
        }

        private void ShowStaffFeedback(string text, bool isError)
        {
            _lblStaffFeedback.Text = text;
            _lblStaffFeedback.ForeColor = isError ? AppTheme.DangerColor : AppTheme.SuccessColor;
        }
    }
}

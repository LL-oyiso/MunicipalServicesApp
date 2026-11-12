using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Lets residents track their service requests by reference number and browse every
    /// request, and lets municipal staff move requests through the workflow. The
    /// persistent chrome is built in the Designer; each tab is built in its own partial
    /// class file (Track, Staff, Route) to keep this form readable.
    /// </summary>
    public partial class ServiceRequestStatusForm : Form
    {
        private sealed class TabEntry
        {
            public Button Button;
            public Panel Panel;
            public Action OnShown;
        }

        private readonly List<TabEntry> _tabs = new List<TabEntry>();
        private Panel _tabStrip;

        // Called after any request changes, so every tab shows current data.
        private readonly List<Action> _refreshers = new List<Action>();

        public ServiceRequestStatusForm()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;

            ApplyTheme();
            BuildContent();
            RefreshAllViews();
            SelectTab(0);
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
            _tabStrip = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };

            AddTab("Track a Request", BuildTrackTabPanel(), null);
            AddTab("All Requests", BuildAllRequestsTabPanel(), () => _tvCategories.Nodes[0].EnsureVisible());

            // Dock=Fill tab panels first, the Dock=Top strip last so it claims the top.
            foreach (var tab in _tabs)
            {
                pnlContent.Controls.Add(tab.Panel);
            }

            pnlContent.Controls.Add(_tabStrip);
        }

        private void AddTab(string text, Panel panel, Action onShown)
        {
            var index = _tabs.Count;
            var left = index == 0 ? 24 : _tabs[index - 1].Button.Right + 8;

            var button = new Button { Text = text, Location = new Point(left, 8), Size = new Size(190, 36) };
            button.Click += (s, e) => SelectTab(index);
            _tabStrip.Controls.Add(button);

            panel.Dock = DockStyle.Fill;
            panel.Visible = false;

            _tabs.Add(new TabEntry { Button = button, Panel = panel, OnShown = onShown });
        }

        private void SelectTab(int index)
        {
            for (var i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].Panel.Visible = i == index;
                ApplyTabButtonStyle(_tabs[i].Button, i == index);
            }

            _tabs[index].OnShown?.Invoke();
        }

        private static void ApplyTabButtonStyle(Button button, bool isSelected)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = AppTheme.ButtonFont;
            button.Cursor = Cursors.Hand;
            button.BackColor = isSelected ? AppTheme.AccentColor : Color.FromArgb(238, 240, 243);
            button.ForeColor = isSelected ? Color.White : AppTheme.SecondaryColor;
        }

        private void RefreshAllViews()
        {
            foreach (var refresh in _refreshers)
            {
                refresh();
            }
        }

        #region Shared helpers

        internal static Color StatusColor(ServiceRequestStatus status)
        {
            switch (status)
            {
                case ServiceRequestStatus.Submitted: return AppTheme.SecondaryColor;
                case ServiceRequestStatus.Assigned: return AppTheme.AccentColor;
                case ServiceRequestStatus.InProgress: return AppTheme.WarningColor;
                case ServiceRequestStatus.OnHold: return Color.FromArgb(155, 89, 182);
                case ServiceRequestStatus.Resolved: return AppTheme.SuccessColor;
                case ServiceRequestStatus.Closed: return AppTheme.PrimaryColor;
                case ServiceRequestStatus.Rejected: return AppTheme.DangerColor;
                default: return AppTheme.SecondaryColor;
            }
        }

        internal static Color PriorityColor(RequestPriority priority)
        {
            switch (priority)
            {
                case RequestPriority.Critical: return AppTheme.DangerColor;
                case RequestPriority.High: return AppTheme.WarningColor;
                case RequestPriority.Medium: return AppTheme.AccentColor;
                default: return AppTheme.SecondaryColor;
            }
        }

        private static Label CreateHeading(string text)
        {
            return new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.HeadingFont,
                ForeColor = AppTheme.PrimaryColor,
                Text = text,
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private static Label CreateHint(string text)
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

        private static Label CreateFieldName(string text)
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

        private static Label CreateFieldValue()
        {
            return new Label
            {
                AutoEllipsis = true,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                ForeColor = AppTheme.SecondaryColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        /// <summary>A coloured, white-text label used as a status or priority pill.</summary>
        private static Label CreatePill()
        {
            return new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0),
                Size = new Size(120, 26),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private static void SetPill(Label pill, string text, Color color)
        {
            pill.Text = text;
            pill.BackColor = color;
        }

        /// <summary>A details-view ListView with the app's fonts and full-row selection.</summary>
        private static ListView CreateListView(params KeyValuePair<string, int>[] columns)
        {
            var listView = new ListView
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = AppTheme.BodyFont,
                FullRowSelect = true,
                GridLines = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                HideSelection = false,
                MultiSelect = false,
                View = View.Details
            };

            foreach (var column in columns)
            {
                listView.Columns.Add(column.Key, column.Value);
            }

            // The last column takes whatever width is left, so there is no empty column.
            listView.Resize += (s, e) =>
            {
                var used = 0;
                for (var i = 0; i < listView.Columns.Count - 1; i++)
                {
                    used += listView.Columns[i].Width;
                }

                var last = listView.Columns[listView.Columns.Count - 1];
                last.Width = Math.Max(columns[columns.Length - 1].Value, listView.ClientSize.Width - used - 4);
            };

            return listView;
        }

        private static KeyValuePair<string, int> Column(string title, int width)
        {
            return new KeyValuePair<string, int>(title, width);
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("dd MMM yyyy HH:mm");
        }

        /// <summary>Wraps a list's items so the ComboBox shows friendly text but hands back typed values.</summary>
        private sealed class ComboOption<T>
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

        #endregion
    }
}

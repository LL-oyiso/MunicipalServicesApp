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
    /// The Crew Route and Nearby tab: the minimum spanning tree linking every ward with
    /// open work, open requests near a ward found by breadth-first search, and a
    /// side-by-side comparison of the request index trees' heights.
    /// </summary>
    public partial class ServiceRequestStatusForm
    {
        private ComboBox _cmbRouteCrew;
        private WardMapControl _wardMap;
        private Label _lblRouteSummary;
        private ListView _lvRouteLegs;
        private ComboBox _cmbNearbyWard;
        private ComboBox _cmbNearbyHops;
        private Label _lblNearbySummary;
        private ListView _lvNearby;
        private ListView _lvIndexStats;

        private Panel BuildRouteTabPanel()
        {
            var tabPanel = new Panel();

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(24, 12, 24, 24), BackColor = AppTheme.BackgroundColor };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            body.Controls.Add(BuildRouteColumn(), 0, 0);
            body.Controls.Add(BuildNearbyColumn(), 1, 0);
            tabPanel.Controls.Add(body);

            _refreshers.Add(RefreshRouteTab);
            return tabPanel;
        }

        private TableLayoutPanel BuildRouteColumn()
        {
            var column = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Margin = new Padding(0, 0, 16, 0) };
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            column.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));

            var options = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = new Padding(0) };
            var planFor = CreateFieldName("Plan the route for");
            planFor.AutoSize = true;
            planFor.Dock = DockStyle.None;
            planFor.Margin = new Padding(0, 9, 8, 0);
            _cmbRouteCrew = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Width = 260, Margin = new Padding(0, 5, 0, 0) };
            _cmbRouteCrew.Items.Add(new ComboOption<string>("Every open request", null));
            foreach (var crew in ServiceRequestRepository.Crews)
            {
                _cmbRouteCrew.Items.Add(new ComboOption<string>(crew + "'s open requests", crew));
            }
            _cmbRouteCrew.SelectedIndex = 0;
            _cmbRouteCrew.SelectedIndexChanged += (s, e) => RefreshRoute();
            options.Controls.Add(planFor);
            options.Controls.Add(_cmbRouteCrew);

            _wardMap = new WardMapControl { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) };
            _wardMap.WardClicked += (s, area) => SelectNearbyWard(area);

            _lblRouteSummary = new Label { Dock = DockStyle.Fill, Font = AppTheme.BodyFont, ForeColor = AppTheme.PrimaryColor, TextAlign = ContentAlignment.MiddleLeft };

            _lvRouteLegs = CreateListView(Column("Leg", 40), Column("From", 170), Column("To", 170), Column("Distance", 80), Column("Roads taken", 200));

            column.Controls.Add(CreateHeading("Crew route (minimum spanning tree)"), 0, 0);
            column.Controls.Add(options, 0, 1);
            column.Controls.Add(_wardMap, 0, 2);
            column.Controls.Add(_lblRouteSummary, 0, 3);
            column.Controls.Add(_lvRouteLegs, 0, 4);
            return column;
        }

        private TableLayoutPanel BuildNearbyColumn()
        {
            var column = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            column.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            column.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));

            var options = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            _cmbNearbyWard = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 5, 8, 0) };
            _cmbNearbyWard.Items.AddRange(ServiceRequestRepository.Areas.Cast<object>().ToArray());
            _cmbNearbyWard.SelectedIndex = 0;
            _cmbNearbyWard.SelectedIndexChanged += (s, e) => RefreshNearby();

            _cmbNearbyHops = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.BodyFont, Margin = new Padding(0, 5, 0, 0) };
            _cmbNearbyHops.Items.Add(new ComboOption<int>("This ward only", 0));
            _cmbNearbyHops.Items.Add(new ComboOption<int>("Up to 1 ward away", 1));
            _cmbNearbyHops.Items.Add(new ComboOption<int>("Up to 2 wards away", 2));
            _cmbNearbyHops.SelectedIndex = 1;
            _cmbNearbyHops.SelectedIndexChanged += (s, e) => RefreshNearby();

            options.Controls.Add(_cmbNearbyWard, 0, 0);
            options.Controls.Add(_cmbNearbyHops, 1, 0);

            _lblNearbySummary = CreateHint(string.Empty);

            _lvNearby = CreateListView(Column("Reference", 110), Column("Ward", 140), Column("Away", 45), Column("Priority", 65), Column("Status", 85), Column("Sub-type", 110));
            _lvNearby.ItemActivate += (s, e) =>
            {
                if (_lvNearby.SelectedItems.Count > 0)
                {
                    OpenInTrackTab((Issue)_lvNearby.SelectedItems[0].Tag);
                }
            };

            _lvIndexStats = CreateListView(Column("Used for", 140), Column("Structure", 125), Column("Height", 80), Column("Why", 190));

            column.Controls.Add(CreateHeading("Open requests nearby (breadth-first search)"), 0, 0);
            column.Controls.Add(options, 0, 1);
            column.Controls.Add(_lblNearbySummary, 0, 2);
            column.Controls.Add(_lvNearby, 0, 3);
            column.Controls.Add(CreateHeading("How the request trees compare"), 0, 4);
            column.Controls.Add(_lvIndexStats, 0, 5);
            return column;
        }

        private void RefreshRouteTab()
        {
            RefreshRoute();
            RefreshNearby();
            RefreshIndexStats();
        }

        private void RefreshRoute()
        {
            var crew = ((ComboOption<string>)_cmbRouteCrew.SelectedItem).Value;
            var wards = WardMap.AreasWithOpenRequests(crew);
            var route = WardMap.PlanCrewRoute(crew);
            var roads = WardMap.RoadsOnRoute(route);

            var openCounts = ServiceRequestRepository.Areas.ToDictionary(
                area => area,
                area => ServiceRequestRepository.GetByArea(area).Count(r => r.IsOpen && (crew == null || r.AssignedCrew == crew)),
                StringComparer.OrdinalIgnoreCase);
            _wardMap.SetData(roads, wards, openCounts);

            _lvRouteLegs.BeginUpdate();
            _lvRouteLegs.Items.Clear();
            for (var i = 0; i < route.Count; i++)
            {
                var leg = route[i];
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(leg.From);
                item.SubItems.Add(leg.To);
                item.SubItems.Add(leg.Weight.ToString("0.0") + " km");
                item.SubItems.Add(leg.Path.Count > 2
                    ? "Via " + string.Join(", ", leg.Path.Skip(1).Take(leg.Path.Count - 2))
                    : "Direct road");
                _lvRouteLegs.Items.Add(item);
            }
            _lvRouteLegs.EndUpdate();

            var who = crew ?? "the crews";
            if (wards.Count == 0)
            {
                _lblRouteSummary.Text = "No open requests for " + who + " with a ward set, so there is no route to plan.";
            }
            else if (wards.Count == 1)
            {
                _lblRouteSummary.Text = "All open work for " + who + " is in " + wards[0] + ", so no travel between wards is needed.";
            }
            else
            {
                _lblRouteSummary.Text = string.Format(
                    "Linking the {0} wards with open requests takes {1:0.0} km of road, the shortest possible (Prim's algorithm). The whole road network is {2:0.0} km.",
                    wards.Count, roads.Sum(r => r.Weight), WardMap.TotalRoadLength());
            }
        }

        private void SelectNearbyWard(string area)
        {
            _cmbNearbyWard.SelectedItem = area;
        }

        private void RefreshNearby()
        {
            var area = (string)_cmbNearbyWard.SelectedItem;
            var hops = ((ComboOption<int>)_cmbNearbyHops.SelectedItem).Value;
            var nearby = WardMap.FindOpenRequestsNear(area, hops)
                .OrderBy(n => n.WardsAway)
                .ThenBy(n => n.Request.Priority)
                .ToList();

            _wardMap.SelectedWard = area;

            _lvNearby.BeginUpdate();
            _lvNearby.Items.Clear();
            foreach (var match in nearby)
            {
                var request = match.Request;
                var item = new ListViewItem(request.ReferenceNumber) { Tag = request, UseItemStyleForSubItems = false };
                item.SubItems.Add(request.Area);
                item.SubItems.Add(match.WardsAway == 0 ? "Here" : match.WardsAway.ToString());
                item.SubItems.Add(request.Priority.ToString(), PriorityColor(request.Priority), Color.White, AppTheme.LabelFont);
                item.SubItems.Add(StatusWorkflow.DisplayName(request.Status), StatusColor(request.Status), Color.White, AppTheme.LabelFont);
                item.SubItems.Add(request.SubCategory ?? request.Category);
                _lvNearby.Items.Add(item);
            }
            _lvNearby.EndUpdate();

            var wardsSearched = WardMap.Graph.WithinHops(area, hops).Count;
            _lblNearbySummary.Text = string.Format(
                "{0} open request{1} across {2} ward{3}. Click a ward on the map to search from it.",
                nearby.Count, nearby.Count == 1 ? string.Empty : "s", wardsSearched, wardsSearched == 1 ? string.Empty : "s");
        }

        private void RefreshIndexStats()
        {
            var stats = ServiceRequestRepository.GetIndexStatistics();
            var best = (int)Math.Ceiling(Math.Log(stats.RequestCount + 1, 2));

            _lvIndexStats.BeginUpdate();
            _lvIndexStats.Items.Clear();
            AddIndexRow("Reference lookup", "AVL tree", stats.AvlHeight.ToString(), "Rotations keep it balanced");
            AddIndexRow("Same keys, no balancing", "Binary search tree", stats.UnbalancedBstHeight.ToString(), "Keys arrive in order: a list");
            AddIndexRow("Dates (newest first)", "Red-black tree", stats.RedBlackHeight + " (black " + stats.RedBlackBlackHeight + ")", "Stays under 2 x log2(n)");
            AddIndexRow("Ward filter", "Binary search tree", stats.AreaBstHeight.ToString(), stats.AreaCount + " keys incl. unassigned");
            AddIndexRow("Best possible", "Perfect balance", best.ToString(), "log2(" + stats.RequestCount + ") rounded up");
            _lvIndexStats.EndUpdate();
        }

        private void AddIndexRow(string usedFor, string structure, string height, string why)
        {
            var item = new ListViewItem(usedFor);
            item.SubItems.Add(structure);
            item.SubItems.Add(height);
            item.SubItems.Add(why);
            _lvIndexStats.Items.Add(item);
        }
    }
}

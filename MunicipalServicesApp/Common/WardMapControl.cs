using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using MunicipalServicesApp.Data;
using MunicipalServicesApp.DataStructures;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// Custom-drawn schematic of the ward graph: every road with its distance, the
    /// planned crew route drawn thick on top, and each ward as a circle showing how many
    /// open requests it has. Clicking a ward raises <see cref="WardClicked"/>.
    /// </summary>
    public class WardMapControl : Control
    {
        private const float WardRadius = 18F;

        // Schematic positions, as fractions of the drawing area, in ServiceRequestRepository.Areas order.
        private static readonly PointF[] WardPositions =
        {
            new PointF(0.56F, 0.30F), // Ward 1 - City Centre
            new PointF(0.30F, 0.44F), // Ward 3 - Civic Park
            new PointF(0.90F, 0.06F), // Ward 5 - Industrial Park
            new PointF(0.84F, 0.66F), // Ward 7 - Riverside
            new PointF(0.06F, 0.06F), // Ward 9 - Greenfield
            new PointF(0.52F, 0.56F), // Ward 12 - Hillcrest
            new PointF(0.10F, 0.92F)  // Ward 15 - Lakeside
        };

        private List<WeightedEdge<string>> _routeRoads = new List<WeightedEdge<string>>();
        private Dictionary<string, int> _openCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _routeWards = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private string _selectedWard;

        public event EventHandler<string> WardClicked;

        public WardMapControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Cursor = Cursors.Hand;
        }

        /// <summary>Shows a new route and open-request counts.</summary>
        public void SetData(List<WeightedEdge<string>> routeRoads, IEnumerable<string> routeWards, Dictionary<string, int> openCounts)
        {
            _routeRoads = routeRoads ?? new List<WeightedEdge<string>>();
            _routeWards = new HashSet<string>(routeWards ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            _openCounts = openCounts ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Invalidate();
        }

        public string SelectedWard
        {
            get => _selectedWard;
            set
            {
                _selectedWard = value;
                Invalidate();
            }
        }

        private PointF PositionOf(string area)
        {
            var index = IndexOf(area);
            var p = WardPositions[index];
            var marginX = 70F;
            var marginY = 36F;
            return new PointF(marginX + p.X * (Width - 2 * marginX), marginY + p.Y * (Height - 2 * marginY - 20F));
        }

        private static int IndexOf(string area)
        {
            var areas = ServiceRequestRepository.Areas;
            for (var i = 0; i < areas.Length; i++)
            {
                if (string.Equals(areas[i], area, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var roadPen = new Pen(Color.FromArgb(205, 210, 216), 2F))
            using (var routePen = new Pen(AppTheme.AccentColor, 6F) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var distanceFont = new Font("Segoe UI", 8F))
            using (var routeDistanceFont = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var nameFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var countFont = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (var centred = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Every road, then the route on top, each labelled with its distance.
                foreach (var road in WardMap.Graph.Edges)
                {
                    var onRoute = _routeRoads.Any(r => WardMap.IsSameRoad(r, road.From, road.To));
                    var a = PositionOf(road.From);
                    var b = PositionOf(road.To);
                    g.DrawLine(onRoute ? routePen : roadPen, a, b);

                    var mid = new PointF((a.X + b.X) / 2F, (a.Y + b.Y) / 2F);
                    var label = road.Weight.ToString("0.0") + " km";
                    var font = onRoute ? routeDistanceFont : distanceFont;
                    var size = g.MeasureString(label, font);
                    var labelRect = new RectangleF(mid.X - size.Width / 2F, mid.Y - size.Height / 2F, size.Width, size.Height);
                    using (var labelBack = new SolidBrush(Color.FromArgb(235, Color.White)))
                    using (var labelBrush = new SolidBrush(onRoute ? AppTheme.AccentColor : AppTheme.SecondaryColor))
                    {
                        g.FillRectangle(labelBack, labelRect);
                        g.DrawString(label, font, labelBrush, labelRect, centred);
                    }
                }

                foreach (var area in ServiceRequestRepository.Areas)
                {
                    var centre = PositionOf(area);
                    var circle = new RectangleF(centre.X - WardRadius, centre.Y - WardRadius, WardRadius * 2, WardRadius * 2);
                    var count = _openCounts.TryGetValue(area, out var c) ? c : 0;
                    var fill = _routeWards.Contains(area) ? AppTheme.PrimaryColor : count > 0 ? AppTheme.SecondaryColor : Color.FromArgb(205, 210, 216);

                    if (string.Equals(area, _selectedWard, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var ring = new Pen(AppTheme.WarningColor, 4F))
                        {
                            g.DrawEllipse(ring, RectangleF.Inflate(circle, 5F, 5F));
                        }
                    }

                    using (var brush = new SolidBrush(fill))
                    {
                        g.FillEllipse(brush, circle);
                    }

                    g.DrawString(count.ToString(), countFont, Brushes.White, circle, centred);

                    // "Ward 1 - City Centre" drawn as "Ward 1" over "City Centre".
                    var parts = area.Split(new[] { " - " }, 2, StringSplitOptions.None);
                    var name = parts.Length == 2 ? parts[0] + Environment.NewLine + parts[1] : area;
                    var nameSize = g.MeasureString(name, nameFont, 140, centred);
                    var nameRect = new RectangleF(centre.X - nameSize.Width / 2F - 3F, centre.Y + WardRadius + 3F, nameSize.Width + 6F, nameSize.Height + 2F);
                    using (var nameBack = new SolidBrush(Color.FromArgb(240, Color.White)))
                    using (var nameBrush = new SolidBrush(AppTheme.PrimaryColor))
                    {
                        g.FillRectangle(nameBack, nameRect);
                        g.DrawString(name, nameFont, nameBrush, nameRect, centred);
                    }
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            foreach (var area in ServiceRequestRepository.Areas)
            {
                var centre = PositionOf(area);
                var dx = e.X - centre.X;
                var dy = e.Y - centre.Y;
                if (dx * dx + dy * dy <= (WardRadius + 6F) * (WardRadius + 6F))
                {
                    WardClicked?.Invoke(this, area);
                    return;
                }
            }
        }
    }
}

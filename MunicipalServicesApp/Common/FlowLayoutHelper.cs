using System;
using System.Windows.Forms;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// Shared fix for a FlowLayoutPanel wrap-height bug hit while building
    /// LocalEventsForm: FlowLayoutPanel's own AutoSize+WrapContents height
    /// calculation is unreliable once nested inside a Dock=Top AutoSize section
    /// Panel, itself inside a Dock=Fill AutoScroll Panel - it self-reports a height
    /// that only accounts for the first row, silently clipping every row after it,
    /// with no scrollbar ever appearing (AutoScrollMinSize is computed FROM that
    /// already-wrong child height, not independently). Extracted here so any new
    /// scrollable card gallery (events, announcements, or the Community Resources
    /// bonus section) can reuse the fix instead of re-deriving it.
    /// </summary>
    public static class FlowLayoutHelper
    {
        /// <summary>
        /// Recomputes each given gallery's real wrapped height (see
        /// <see cref="FitFlowPanelHeight"/>) and then makes the containing AutoScroll
        /// panel re-layout against those now-correct heights. Safe to call repeatedly
        /// (on tab switch, on gallery refresh, after the form is Shown) since it's
        /// cheap and idempotent. Accepts multiple galleries for tabs that stack more
        /// than one FlowLayoutPanel inside the same scroll panel.
        /// </summary>
        public static void ForceScrollableRelayout(Panel scrollPanel, params FlowLayoutPanel[] galleries)
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
        /// wrapped row of its children, based on the panel's *current*
        /// ClientSize.Width. The panel's WIDTH (and therefore how many cards fit per
        /// row) is assigned correctly by ordinary Dock=Top layout well before this
        /// runs, so replicating just the wrap-height arithmetic ourselves - using the
        /// panel's real current width and each child's real size/margin - sidesteps
        /// the unreliable built-in calculation entirely.
        /// </summary>
        public static void FitFlowPanelHeight(FlowLayoutPanel flp)
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
    }
}

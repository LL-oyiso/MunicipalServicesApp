using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// Custom-drawn wizard step indicator: a row of numbered circles connected by
    /// lines, showing completed / current / upcoming steps. Doubles as the Report
    /// Issues wizard's dynamic engagement feature - residents can see, at a glance,
    /// how far through the reporting process they are.
    /// </summary>
    public class StepIndicatorControl : Panel
    {
        private const int CircleDiameter = 30;

        private string[] _stepLabels = new string[0];
        private int _currentStepIndex;

        public string[] StepLabels
        {
            get => _stepLabels;
            set
            {
                _stepLabels = value ?? new string[0];
                Invalidate();
            }
        }

        /// <summary>0-based index of the step currently shown to the user.</summary>
        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                _currentStepIndex = value;
                Invalidate();
            }
        }

        public StepIndicatorControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = AppTheme.HeaderBarColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var stepCount = _stepLabels.Length;
            if (stepCount == 0)
            {
                return;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var circleY = 14;
            var slotWidth = Width / (float)stepCount;

            using (var labelFont = new Font("Segoe UI", 8.5F, FontStyle.Regular))
            using (var numberFont = new Font("Segoe UI", 10F, FontStyle.Bold))
            {
                for (var i = 0; i < stepCount; i++)
                {
                    var centerX = slotWidth * i + slotWidth / 2F;

                    // Connector line to the next step.
                    if (i < stepCount - 1)
                    {
                        var nextCenterX = slotWidth * (i + 1) + slotWidth / 2F;
                        var connectorColor = i < _currentStepIndex ? AppTheme.StepConnectorActiveColor : AppTheme.StepConnectorInactiveColor;
                        using (var pen = new Pen(connectorColor, 2F))
                        {
                            g.DrawLine(pen, centerX + CircleDiameter / 2F, circleY + CircleDiameter / 2F, nextCenterX - CircleDiameter / 2F, circleY + CircleDiameter / 2F);
                        }
                    }

                    var circleRect = new RectangleF(centerX - CircleDiameter / 2F, circleY, CircleDiameter, CircleDiameter);

                    if (i < _currentStepIndex)
                    {
                        // Completed: filled accent circle with a checkmark.
                        using (var brush = new SolidBrush(AppTheme.StepCompletedColor))
                        {
                            g.FillEllipse(brush, circleRect);
                        }

                        DrawCheckmark(g, circleRect);
                    }
                    else if (i == _currentStepIndex)
                    {
                        // Current: filled primary circle with the step number.
                        using (var brush = new SolidBrush(AppTheme.StepCurrentColor))
                        {
                            g.FillEllipse(brush, circleRect);
                        }

                        DrawCenteredText(g, (i + 1).ToString(), numberFont, Brushes.White, circleRect);
                    }
                    else
                    {
                        // Upcoming: outlined circle with a muted step number.
                        using (var brush = new SolidBrush(Color.White))
                        using (var pen = new Pen(AppTheme.StepUpcomingColor, 2F))
                        {
                            g.FillEllipse(brush, circleRect);
                            g.DrawEllipse(pen, circleRect);
                        }

                        using (var textBrush = new SolidBrush(AppTheme.StepUpcomingColor))
                        {
                            DrawCenteredText(g, (i + 1).ToString(), numberFont, textBrush, circleRect);
                        }
                    }

                    var labelColor = i <= _currentStepIndex ? Color.White : Color.FromArgb(190, 205, 212);
                    var labelRect = new RectangleF(centerX - slotWidth / 2F + 2, circleY + CircleDiameter + 4, slotWidth - 4, 28);
                    using (var labelBrush = new SolidBrush(labelColor))
                    using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
                    {
                        g.DrawString(_stepLabels[i], labelFont, labelBrush, labelRect, format);
                    }
                }
            }
        }

        private static void DrawCenteredText(Graphics g, string text, Font font, Brush brush, RectangleF bounds)
        {
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(text, font, brush, bounds, format);
            }
        }

        private static void DrawCheckmark(Graphics g, RectangleF circleRect)
        {
            using (var pen = new Pen(Color.White, 2.2F) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                var x = circleRect.X;
                var y = circleRect.Y;
                var w = circleRect.Width;
                var h = circleRect.Height;

                var p1 = new PointF(x + w * 0.27F, y + h * 0.53F);
                var p2 = new PointF(x + w * 0.44F, y + h * 0.70F);
                var p3 = new PointF(x + w * 0.75F, y + h * 0.32F);

                g.DrawLines(pen, new[] { p1, p2, p3 });
            }
        }
    }
}

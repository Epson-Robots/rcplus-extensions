// -----------------------------------------------------------------------
// <copyright file="DropIndicatorAdorner.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace VanguardModelPFScrewdriver.Dialogs.EditProgram
{
    /// <summary>
    /// Adorner that draws a horizontal insertion line used as a drop indicator
    /// while dragging program steps within the editor.
    /// </summary>
    public class DropIndicatorAdorner : Adorner
    {
        /// <summary>
        /// Bounds of the item the indicator is drawn for.
        /// Used to determine the horizontal extent of the indicator line.
        /// </summary>
        public Rect Bounds;

        /// <summary>
        /// Gets or sets the brush used to draw the indicator line and its edge marks.
        /// </summary>
        public Brush Brush { get; set; } = Brushes.Gray;

        /// <summary>
        /// Gets or sets the thickness of the indicator line.
        /// </summary>
        public double Thickness { get; set; } = 2;

        /// <summary>
        /// Gets or sets the vertical position of the indicator line.
        /// Setting this value requests a redraw of the adorner.
        /// </summary>
        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Backing store for <see cref="Y"/>.
        /// </summary>
        private double _y;

        /// <summary>
        /// Radius of the circles drawn at both ends of the indicator line.
        /// </summary>
        private const int _edgeBallRadius = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropIndicatorAdorner"/> class.
        /// The adorner is hidden and excluded from hit testing by default.
        /// </summary>
        /// <param name="adornedElement">The element to be adorned.</param>
        public DropIndicatorAdorner(
            UIElement adornedElement
        )
        : base(adornedElement)
        {
            IsHitTestVisible = false;
            Hide();
        }

        /// <summary>
        /// Draws the indicator line together with a circle at each end.
        /// </summary>
        /// <param name="drawingContext">The drawing context used to render the adorner.</param>
        protected override void OnRender(
            DrawingContext drawingContext
        )
        {
            base.OnRender(drawingContext);

            Pen pen = new(Brush, Thickness);

            Point leftEdge = new(Bounds.X + _edgeBallRadius * 2, Y - Bounds.Height / 2);
            Point rightEdge = new(Bounds.X + Bounds.Width - _edgeBallRadius * 2, Y - Bounds.Height / 2);

            drawingContext.DrawLine(pen, leftEdge, rightEdge);
            drawingContext.DrawEllipse(Brush, null, leftEdge, _edgeBallRadius, _edgeBallRadius);
            drawingContext.DrawEllipse(Brush, null, rightEdge, _edgeBallRadius, _edgeBallRadius);
        }

        /// <summary>
        /// Makes the indicator visible.
        /// </summary>
        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Updates <see cref="Bounds"/> and makes the indicator visible.
        /// </summary>
        /// <param name="bounds">Bounds of the item the indicator is drawn for.</param>
        public void Show(
            Rect bounds
        )
        {
            Bounds = bounds;

            Show();
        }

        /// <summary>
        /// Hides the indicator.
        /// </summary>
        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Moves the indicator line to the specified vertical position.
        /// </summary>
        /// <param name="y">The new vertical position of the indicator line.</param>
        public void MoveTo(
            double y
        )
        {
            Y = y;
        }
    }
}

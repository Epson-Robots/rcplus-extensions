// -----------------------------------------------------------------------
// <copyright file="PlotControlWrapper.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using ScottPlot;
using ScottPlot.Plottable;
using System.Drawing;
using System.Windows.Input;
using VanguardModelPFScrewdriver.Utils;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Wraps a ScottPlot <see cref="WpfPlot"/> control and provides
    /// torque graph specific helper operations.
    /// </summary>
    public class PlotControlWrapper
    {
        /// <summary>
        /// Represents a torque waveform plot together with its original values in mN・m.
        /// </summary>
        public class TorquePlot
        {
            /// <summary>
            /// The underlying ScottPlot plottable.
            /// </summary>
            public SignalPlotXY Plot;

            // Original torque values in mN・m, kept for unit conversion.
            private readonly double[] _innerTorques;

            /// <summary>
            /// Initializes a new instance of the <see cref="TorquePlot"/> class.
            /// </summary>
            /// <param name="plot">Plottable that draws the torque waveform.</param>
            /// <param name="dataY">Torque values in mN・m.</param>
            /// <param name="torqueUnit">Torque unit to display initially.</param>
            public TorquePlot(
                SignalPlotXY plot,
                double[] dataY,
                TorqueUnitKind torqueUnit
            )
            {
                Plot = plot;
                _innerTorques = (double[])dataY.Clone();

                // Convert the displayed values only when a non-default unit is requested.
                if (torqueUnit != TorqueUnitKind.MNM)
                {
                    ChangeUnit(torqueUnit);
                }
            }

            /// <summary>
            /// Rewrites the displayed Y values according to the specified torque unit.
            /// </summary>
            /// <param name="torqueUnit">Torque unit to display.</param>
            public void ChangeUnit(
                TorqueUnitKind torqueUnit
            )
            {
                switch (torqueUnit)
                {
                    case TorqueUnitKind.MNM:
                        // Restore the original values.
                        Array.Copy(_innerTorques, Plot.Ys, _innerTorques.Length);
                        break;

                    case TorqueUnitKind.KGFCM:
                        // Convert every sample from mN・m to kgf・cm.
                        for (var i = 0; i < Plot.Ys.Length; i++)
                        {
                            Plot.Ys[i] = TorqueValueConverter.MNM2KGFCM((float)_innerTorques[i]);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the wrapped WPF plot control.
        /// </summary>
        public WpfPlot PlotControl { get; }

        /// <summary>
        /// Occurs when a draggable plottable has been dropped.
        /// </summary>
        public event EventHandler? Dropped;

        // Indicates whether axis ticks are currently visible.
        private bool _ticksShown = false;

        // Appearance of the measurement window polygon.
        private readonly static Color _windowLineColor = Color.Green;
        private readonly static Color _windowFillColor = Color.FromArgb(50, Color.Green);

        private const int _windowLineWidth = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotControlWrapper"/> class.
        /// </summary>
        public PlotControlWrapper()
        {
            PlotControl = new();

            // Hide ticks until at least one plottable is added.
            PlotControl.Plot.XAxis.Ticks(false);
            PlotControl.Plot.YAxis.Ticks(false);

            PlotControl.PlottableDropped += (_, _) =>
            {
                Dropped?.Invoke(this, new EventArgs());
            };
        }

        /// <summary>
        /// Updates the axis labels with the localized captions and the current torque unit.
        /// </summary>
        /// <param name="torqueUnit">Torque unit shown on the Y axis label.</param>
        public void UpdateTerms(
            TorqueUnitKind torqueUnit
        )
        {
            const int _AxisLabelFontSize = 12;

            // Get the localized expression of the torque unit.
            TorqueUnitLocalizer localizer = new();
            var expression = localizer.Convert(torqueUnit, typeof(string), null!, null!);
            var unit = (expression is string torqueUnitExpr) ? torqueUnitExpr : string.Empty;

            PlotControl.Plot.XAxis.Label(Main.Captions![Caption.NumTurns], size: _AxisLabelFontSize);
            PlotControl.Plot.YAxis.Label($"{Main.Captions![Caption.Torque]} ({unit})", size: _AxisLabelFontSize);
        }

        /// <summary>
        /// Removes all plottables, hides the axis ticks and refreshes the control.
        /// </summary>
        public void RefreshWithClear()
        {
            PlotControl.Plot.Clear();

            if (_ticksShown)
            {
                PlotControl.Plot.XAxis.Ticks(false);
                PlotControl.Plot.YAxis.Ticks(false);
                _ticksShown = false;
            }

            PlotControl.RefreshRequest();
        }

        /// <summary>
        /// Refreshes the control, showing the axis ticks once a plottable exists.
        /// </summary>
        public void Refresh()
        {
            if (!_ticksShown && PlotControl.Plot.GetPlottables().Length > 0)
            {
                PlotControl.Plot.XAxis.Ticks(true);
                PlotControl.Plot.YAxis.Ticks(true);
                _ticksShown = true;
            }

            PlotControl.RefreshRequest();
        }

        /// <summary>
        /// Calculates the maximum X and Y values over all plottables.
        /// Negative results are clamped to zero.
        /// </summary>
        /// <returns>The maximum X and Y coordinates.</returns>
        /// <exception cref="NotImplementedException">An unsupported plottable type is contained.</exception>
        public (double X, double Y) CalcMax()
        {
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (var plottable in PlotControl.Plot.GetPlottables())
            {
                double localMaxX;
                double localMaxY;

                // Each plottable type exposes its coordinates differently.
                if (plottable is SignalPlotXY graph)
                {
                    localMaxX = graph.Xs.Max();
                    localMaxY = graph.Ys.Max();
                }
                else if (plottable is Polygon rect)
                {
                    localMaxX = rect.Xs.Max();
                    localMaxY = rect.Ys.Max();
                }
                else if (plottable is DraggableMarkerPlot handle)
                {
                    localMaxX = handle.X;
                    localMaxY = handle.Y;
                }
                else
                {
                    throw new NotImplementedException($"Unexpected plotable type: {plottable.GetType()}");
                }
                maxX = Math.Max(maxX, localMaxX);
                maxY = Math.Max(maxY, localMaxY);
            }

            return (Math.Max(0, maxX), Math.Max(0, maxY));
        }

        /// <summary>
        /// Gets the current lower and upper limits of the X axis.
        /// </summary>
        public (double Lower, double Upper) GetAxisLimitsX()
        {
            var limits = PlotControl.Plot.GetAxisLimits();

            return (limits.XMin, limits.XMax);
        }

        /// <summary>
        /// Gets the current lower and upper limits of the Y axis.
        /// </summary>
        public (double Lower, double Upper) GetAxisLimitsY()
        {
            var limits = PlotControl.Plot.GetAxisLimits();

            return (limits.YMin, limits.YMax);
        }

        /// <summary>
        /// Adjusts both axes automatically so that all plottables fit.
        /// </summary>
        public void ScaleAuto()
        {
            PlotControl.Plot.AxisAuto();
        }

        /// <summary>
        /// Sets the X axis limits. The request is ignored when the range is invalid.
        /// </summary>
        /// <param name="lower">Lower limit.</param>
        /// <param name="upper">Upper limit.</param>
        public void SetAxisLimitsX(
            double lower,
            double upper
        )
        {
            if (lower >= upper)
            {
                return;
            }

            PlotControl.Plot.SetAxisLimitsX(lower, upper);
        }

        /// <summary>
        /// Sets the Y axis limits. The request is ignored when the range is invalid.
        /// </summary>
        /// <param name="lower">Lower limit.</param>
        /// <param name="upper">Upper limit.</param>
        public void SetAxisLimitsY(
            double lower,
            double upper
        )
        {
            if (lower >= upper)
            {
                return;
            }

            PlotControl.Plot.SetAxisLimitsY(lower, upper);
        }

        /// <summary>
        /// Removes the specified plottable from the plot.
        /// </summary>
        /// <param name="plottable">Plottable to remove.</param>
        public void Remove(
            IPlottable plottable
        )
        {
            PlotControl.Plot.Remove(plottable);
        }

        /// <summary>
        /// Adds a torque waveform to the plot.
        /// </summary>
        /// <param name="dataX">Number of turns.</param>
        /// <param name="dataY">Torque values in mN・m.</param>
        /// <param name="color">Line color.</param>
        /// <param name="torqueUnit">Torque unit to display.</param>
        /// <returns>The created torque plot.</returns>
        public TorquePlot Add(
            double[] dataX,
            double[] dataY,
            Color color,
            TorqueUnitKind torqueUnit
        )
        {
            var plot = PlotControl.Plot.AddSignalXY(dataX, dataY, color);

            return new TorquePlot(plot, dataY, torqueUnit);
        }

        /// <summary>
        /// Adds the measurement window as a filled polygon.
        /// </summary>
        /// <param name="xs">X coordinates of the polygon vertices.</param>
        /// <param name="ys">Y coordinates of the polygon vertices.</param>
        /// <returns>The created polygon.</returns>
        public ScottPlot.Plottable.Polygon AddWindow(
            double[] xs,
            double[] ys
        )
        {
            return PlotControl.Plot.AddPolygon(
                xs, ys,
                _windowFillColor,
                _windowLineWidth,
                _windowLineColor
            );
        }

        /// <summary>
        /// Adds a horizontal line at the specified Y coordinate.
        /// </summary>
        /// <param name="y">Y coordinate of the line.</param>
        /// <param name="color">Line color.</param>
        /// <returns>The created line.</returns>
        public HLine AddHorizontalLine(
            double y,
            Color color
        )
        {
            return PlotControl.Plot.AddHorizontalLine(y, color);
        }

        /// <summary>
        /// Adds a vertical line at the specified X coordinate.
        /// </summary>
        /// <param name="x">X coordinate of the line.</param>
        /// <param name="color">Line color.</param>
        /// <returns>The created line.</returns>
        public VLine AddVerticalLine(
            double x,
            Color color
        )
        {
            return PlotControl.Plot.AddVerticalLine(x, color);
        }

        /// <summary>
        /// Adds a draggable handle marker used to resize the measurement window.
        /// </summary>
        /// <param name="x">X coordinate of the handle.</param>
        /// <param name="y">Y coordinate of the handle.</param>
        /// <returns>The created draggable marker.</returns>
        public DraggableMarkerPlot AddHandle(
            double x,
            double y
        )
        {
            return PlotControl.Plot.AddMarkerDraggable(x, y, MarkerShape.filledSquare, color: _windowLineColor);
        }

        /// <summary>
        /// Gets the current mouse position converted into plot coordinates.
        /// </summary>
        /// <param name="_">Mouse event arguments (not used).</param>
        /// <returns>The mouse position in plot coordinates.</returns>
        public (double X, double Y) GetMouseCoordinate(
            MouseEventArgs _
        )
        {
            return PlotControl.GetMouseCoordinates();
        }
    }
}

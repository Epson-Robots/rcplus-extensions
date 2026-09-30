// -----------------------------------------------------------------------
// <copyright file="WindowItem.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ScottPlot.Plottable;
using System.Reactive.Disposables;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using VanguardModelPFScrewdriver.Utils;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

namespace VanguardModelPFScrewdriver.Dialogs.EditWindow
{
    /// <summary>
    /// Represents a single tightening judgement window, both as editable values
    /// and as a draggable rectangle drawn on the torque/turns plot.
    /// </summary>
    internal class WindowItem : IDisposable
    {
        /// <summary>Window number shown to the user (1-based).</summary>
        public int Number { get; }

        /// <summary>Indicates whether this window is used for judgement.</summary>
        public ReactivePropertySlim<bool> IsEnabled { get; } = new(false);

        /// <summary>Number of turns at the left edge of the window.</summary>
        public ReactiveProperty<float> StartTurn { get; } = new(0f);

        /// <summary>Number of turns at the right edge of the window.</summary>
        public ReactiveProperty<float> EndTurn { get; } = new(0f);

        /// <summary>Torque at the left edge (center of the window height).</summary>
        public TorqueProperty TorqueAtStart { get; } = new();

        /// <summary>Torque at the right edge (center of the window height).</summary>
        public TorqueProperty TorqueAtEnd { get; } = new();

        /// <summary>Half height of the window, i.e. the allowed torque deviation.</summary>
        public TorqueProperty TorqueWidth { get; } = new();

        /// <summary>Indicates whether this window is currently selected on the plot.</summary>
        public ReactivePropertySlim<bool> IsSelected { get; } = new(false);

        /// <summary>Torque unit shared by all window items.</summary>
        public static TorqueUnitKind TorqueUnit { get; set; } = TorqueUnitKind.MNM;

        /// <summary>Number of decimal places used to display torque values.</summary>
        public static int DecimalPlaces { get; set; } = 1;

        /// <summary>Tool type of the target program, used to resolve input ranges.</summary>
        public static ToolKind ToolType { get; set; } = ToolKind.S;

        /// <summary>Tightening mode of the target program, used to resolve input ranges.</summary>
        public static ModeKind Mode { get; set; } = ModeKind.Normal;

        /// <summary>Plot shared by all window items.</summary>
        public static PlotControlWrapper? Plot { get; set; }

        /// <summary>Owner view model, used for validation and axis adjustment.</summary>
        public EditWindowWindowViewModel ParentViewModel { get; }

        /// <summary>True once the initial values have been loaded and validation may run.</summary>
        private bool _validateReady = false;

        /// <summary>True while values are being written back from a drag operation, to avoid redraw loops.</summary>
        private bool _updating = false;

        /// <summary>Rectangle that visualizes the window.</summary>
        private ScottPlot.Plottable.Polygon? _windowRect;

        /// <summary>Drag handle on the left edge.</summary>
        private DraggableMarkerPlot? _startTurnHandle;

        /// <summary>Drag handle on the right edge.</summary>
        private DraggableMarkerPlot? _endTurnHandle;

        /// <summary>Drag handle on the top edge, used to change the torque width.</summary>
        private DraggableMarkerPlot? _torqueWidthHandle;

        /// <summary>Drag handle at the center, used to move the whole window.</summary>
        private DraggableMarkerPlot? _centerHandle;

        /// <summary>Center position before the current drag step (X), used to compute the delta.</summary>
        private double _prevCenterX;

        /// <summary>Center position before the current drag step (Y), used to compute the delta.</summary>
        private double _prevCenterY;

        /// <summary>Snapshot of the original data, used to detect modifications.</summary>
        private Window? _original;

        private readonly CompositeDisposable _disposables = [];

        // Vertex indices of the window rectangle.
        const int _topLeft = 0;

        const int _topRight = 1;

        const int _bottomRight = 2;

        const int _bottomLeft = 3;

        /// <summary>
        /// Validates a turn value and checks that the start turn does not exceed the end turn.
        /// </summary>
        /// <param name="target">Property being validated.</param>
        /// <param name="value">Entered value.</param>
        /// <returns>An error message key when invalid; otherwise null.</returns>
        private string? CheckNumTurns(
            object target,
            float value
         )
        {
            string? error = null;

            if (_validateReady)
            {
                // First check the allowed range.
                error = ParentViewModel.Validate(target, value);
                if (error == null)
                {
                    if (StartTurn.Value > EndTurn.Value)
                    {
                        // The window would have a negative width.
                        error = "StartEndInversion";

                        ParentViewModel.SetValid(Number, false);
                        ParentViewModel.UpdateCanOK();
                    }
                    else
                    {
                        // Re-validate both properties to clear a previous inversion error.
                        // Suppress re-entrancy while forcing the validation.
                        _validateReady = false;
                        StartTurn.ForceValidate();
                        EndTurn.ForceValidate();
                        _validateReady = true;

                        ParentViewModel.SetValid(Number, true);
                        ParentViewModel.UpdateCanOK();
                    }
                }
            }

            return error;
        }

        /// <summary>
        /// Updates the rectangle and the other handles while the left edge handle is dragged.
        /// </summary>
        public void StartTurnDragged(
            object? sender,
            EventArgs ev
        )
        {
            if (Plot != null)
            {
                // Move the left edge vertices to the handle position.
                _windowRect!.Xs[_topLeft] = _startTurnHandle!.X;
                _windowRect.Ys[_topLeft] = _startTurnHandle.Y + TorqueWidth.Value;
                _windowRect.Xs[_bottomLeft] = _startTurnHandle!.X;
                _windowRect.Ys[_bottomLeft] = _startTurnHandle.Y - TorqueWidth.Value;

                // Keep the width handle at the middle of the top edge.
                _torqueWidthHandle!.X = (_windowRect.Xs[_topLeft] + _windowRect.Xs[_topRight]) / 2;
                _torqueWidthHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_topRight]) / 2;

                // Keep the center handle at the center of the rectangle.
                _centerHandle!.X = (_windowRect.Xs[_topLeft] + _windowRect.Xs[_bottomRight]) / 2;
                _centerHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_bottomRight]) / 2;

                ParentViewModel.AdjustScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Updates the rectangle and the other handles while the right edge handle is dragged.
        /// </summary>
        public void EndTurnDragged(
            object? sender,
            EventArgs ev
        )
        {
            if (Plot != null)
            {
                // Move the right edge vertices to the handle position.
                _windowRect!.Xs[_topRight] = _endTurnHandle!.X;
                _windowRect.Ys[_topRight] = _endTurnHandle.Y + TorqueWidth.Value;
                _windowRect.Xs[_bottomRight] = _endTurnHandle!.X;
                _windowRect.Ys[_bottomRight] = _endTurnHandle.Y - TorqueWidth.Value;

                // Keep the width handle at the middle of the top edge.
                _torqueWidthHandle!.X = (_windowRect.Xs[_topLeft] + _windowRect.Xs[_topRight]) / 2;
                _torqueWidthHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_topRight]) / 2;

                // Keep the center handle at the center of the rectangle.
                _centerHandle!.X = (_windowRect.Xs[_topLeft] + _windowRect.Xs[_bottomRight]) / 2;
                _centerHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_bottomRight]) / 2;

                ParentViewModel.AdjustScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Changes the height of the window while the torque width handle is dragged.
        /// </summary>
        public void TorqueWidthDragged(
            object? sender,
            EventArgs ev
        )
        {
            if (Plot != null)
            {
                // The new width is the distance between the handle and the window center line.
                var centerY = (_windowRect!.Ys[_topLeft] + _windowRect.Ys[_bottomRight]) / 2;
                var newWidth = _torqueWidthHandle!.Y - centerY;

                // Expand or shrink the rectangle symmetrically around the center line.
                _windowRect.Ys[_topLeft] = _startTurnHandle!.Y + newWidth;
                _windowRect.Ys[_topRight] = _endTurnHandle!.Y + newWidth;
                _windowRect.Ys[_bottomRight] = _endTurnHandle.Y - newWidth;
                _windowRect.Ys[_bottomLeft] = _startTurnHandle.Y - newWidth;

                _centerHandle!.X = (_windowRect.Xs[_topLeft] + _windowRect.Xs[_bottomRight]) / 2;
                _centerHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_bottomRight]) / 2;

                ParentViewModel.AdjustScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Moves the whole window while the center handle is dragged.
        /// </summary>
        public void WholeDragged(
            object? sender,
            EventArgs ev
        )
        {
            if (Plot != null)
            {
                // Compute the movement since the last drag step.
                var xDiff = _centerHandle!.X - _prevCenterX;
                var yDiff = _centerHandle.Y - _prevCenterY;
                _prevCenterX = _centerHandle.X;
                _prevCenterY = _centerHandle.Y;

                // Translate every vertex of the rectangle.
                for (int i = 0; i < _windowRect!.Xs.Length; i++)
                {
                    _windowRect.Xs[i] += xDiff;
                }
                for (int i = 0; i < _windowRect.Ys.Length; i++)
                {
                    _windowRect.Ys[i] += yDiff;
                }

                // Translate the remaining handles by the same amount.
                _startTurnHandle!.X += xDiff;
                _startTurnHandle.Y += yDiff;

                _endTurnHandle!.X += xDiff;
                _endTurnHandle.Y += yDiff;

                _torqueWidthHandle!.X += xDiff;
                _torqueWidthHandle!.Y += yDiff;

                ParentViewModel.AdjustScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Redraws the window from the current property values.
        /// </summary>
        private void DrawWindow()
        {
            // Skip while values are being written back from a drag operation.
            if (Plot != null && !_updating)
            {
                UpdatePlot();

                // Handles are only visible for an enabled and selected window.
                ShowWindow(IsEnabled.Value);
                ShowHandles(IsEnabled.Value && IsSelected.Value);

                ParentViewModel.AdjustScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Shows or hides all drag handles.
        /// </summary>
        private void ShowHandles(
            bool show
        )
        {
            _startTurnHandle!.IsVisible = show;
            _endTurnHandle!.IsVisible = show;
            _torqueWidthHandle!.IsVisible = show;
            _centerHandle!.IsVisible = show;
        }

        /// <summary>
        /// Shows or hides the window rectangle.
        /// </summary>
        private void ShowWindow(
            bool show
        )
        {
            _windowRect!.IsVisible = show;
        }

        /// <summary>
        /// Gets the allowed value range for the specified hint, based on the current tool type and mode.
        /// </summary>
        /// <param name="id">Hint identifier of the target value.</param>
        /// <returns>The allowed range, or an unrestricted range when no hint is defined.</returns>
        private static FloatRange GetRange(
            HintId id
        )
        {
            FloatRange range = new(0, float.MaxValue);

            var entry = HintData.Instance.Find(id, ToolType, Mode);
            if (entry != null)
            {
                _ = entry.GetHint(ref range);
            }

            return range;
        }

        /// <summary>
        /// Recalculates the rectangle vertices, the handle positions and their drag limits.
        /// </summary>
        private void UpdatePlot()
        {
            // Rectangle vertices: the torque line +/- the torque width.
            _windowRect!.Xs[_topLeft] = StartTurn.Value;
            _windowRect.Ys[_topLeft] = TorqueAtStart.Value + TorqueWidth.Value;

            _windowRect.Xs[_topRight] = EndTurn.Value;
            _windowRect.Ys[_topRight] = TorqueAtEnd.Value + TorqueWidth.Value;

            _windowRect.Xs[_bottomRight] = EndTurn.Value;
            _windowRect.Ys[_bottomRight] = TorqueAtEnd.Value - TorqueWidth.Value;

            _windowRect.Xs[_bottomLeft] = StartTurn.Value;
            _windowRect.Ys[_bottomLeft] = TorqueAtStart.Value - TorqueWidth.Value;

            // Edge handles sit at the middle of the left/right edges.
            _startTurnHandle!.X = _windowRect.Xs[_topLeft];
            _startTurnHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_bottomLeft]) / 2;

            _endTurnHandle!.X = _windowRect.Xs[_topRight];
            _endTurnHandle.Y = (_windowRect.Ys[_topRight] + _windowRect.Ys[_bottomRight]) / 2;

            // Width handle sits at the middle of the top edge.
            _torqueWidthHandle!.X = (_windowRect.Xs[_topLeft] + _windowRect.Xs[_topRight]) / 2;
            _torqueWidthHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_topRight]) / 2;

            // Center handle sits at the center of the rectangle; remember it as the drag origin.
            _centerHandle!.X = (_windowRect.Xs[_topLeft] + _windowRect.Xs[_bottomRight]) / 2;
            _centerHandle.Y = (_windowRect.Ys[_topLeft] + _windowRect.Ys[_bottomRight]) / 2;
            _prevCenterX = _centerHandle.X;
            _prevCenterY = _centerHandle.Y;

            // Allowed ranges for each editable value.
            var startTurnRange = GetRange(HintId.WindowStartTurn);
            var endTurnRange = GetRange(HintId.WindowEndTurn);
            var torqueRange = GetRange(HintId.Torque);
            var torqueWidthRange = GetRange(HintId.WindowTorqueWidth);

            // The left edge may not pass the right edge, and the window must stay above zero torque.
            _startTurnHandle.DragXLimitMin = startTurnRange.Min;
            _startTurnHandle.DragXLimitMax = EndTurn.Value;
            _startTurnHandle.DragYLimitMin = Math.Max(torqueRange.Min, TorqueWidth.Value);
            _startTurnHandle.DragYLimitMax = torqueRange.Max;

            // The right edge may not pass the left edge.
            _endTurnHandle.DragXLimitMin = StartTurn.Value;
            _endTurnHandle.DragXLimitMax = endTurnRange.Max;
            _endTurnHandle.DragYLimitMin = Math.Max(torqueRange.Min, TorqueWidth.Value);
            _endTurnHandle.DragYLimitMax = torqueRange.Max;

            // The width handle only moves vertically, from the center line up to the allowed maximum.
            _torqueWidthHandle.DragXLimitMin = _torqueWidthHandle.X;
            _torqueWidthHandle.DragXLimitMax = _torqueWidthHandle.X;
            _torqueWidthHandle.DragYLimitMin = _centerHandle.Y;
            _torqueWidthHandle.DragYLimitMax = Math.Min(_centerHandle.Y + Math.Min(_startTurnHandle.Y, _endTurnHandle.Y), _centerHandle.Y + torqueWidthRange.Max);

            // The center handle is limited so that the whole rectangle stays inside the allowed ranges.
            _centerHandle.DragXLimitMin = Math.Max(
                startTurnRange.Min + (_windowRect.Xs[_topRight] - _windowRect.Xs[_topLeft]) / 2,
                endTurnRange.Min - (_windowRect.Xs[_topRight] - _windowRect.Xs[_topLeft]) / 2
            );
            _centerHandle.DragXLimitMax = Math.Max(
                endTurnRange.Max - (_windowRect.Xs[_topRight] - _windowRect.Xs[_topLeft]) / 2,
                startTurnRange.Min + (_windowRect.Xs[_topRight] - _windowRect.Xs[_topLeft]) / 2
            );
            _centerHandle.DragYLimitMin = Math.Max(
                TorqueWidth.Value + Math.Abs(_windowRect.Ys[_bottomLeft] - _windowRect.Ys[_bottomRight]) / 2,
                torqueRange.Min + Math.Abs(_windowRect.Ys[_bottomLeft] - _windowRect.Ys[_bottomRight]) / 2
            );
            _centerHandle.DragYLimitMax = torqueRange.Max - Math.Abs(_windowRect.Ys[_bottomLeft] - _windowRect.Ys[_bottomRight]) / 2;
        }

        /// <summary>
        /// Creates the rectangle and the drag handles on the plot (initially hidden).
        /// </summary>
        private void CreatePlot()
        {
            if (Plot != null)
            {
                double[] xs = [0, 0, 0, 0];
                double[] ys = [0, 0, 0, 0];
                _windowRect = Plot.AddWindow(xs, ys);
                _windowRect.IsVisible = false;

                _startTurnHandle = Plot.AddHandle(0, 0);
                _startTurnHandle.Dragged += StartTurnDragged;
                _startTurnHandle.IsVisible = false;

                _endTurnHandle = Plot.AddHandle(0, 0);
                _endTurnHandle.Dragged += EndTurnDragged;
                _endTurnHandle.IsVisible = false;

                _torqueWidthHandle = Plot.AddHandle(0, 0);
                _torqueWidthHandle.Dragged += TorqueWidthDragged;
                _torqueWidthHandle.IsVisible = false;

                _centerHandle = Plot.AddHandle(0, 0);
                _centerHandle.Dragged += WholeDragged;
                _centerHandle.IsVisible = false;
            }
        }

        /// <summary>
        /// Removes the rectangle and the handles from the plot and detaches their event handlers.
        /// </summary>
        private void DestroyPlot()
        {
            if (Plot != null)
            {
                if (_windowRect != null)
                {
                    Plot.Remove(_windowRect);
                    _windowRect = null;
                }

                if (_startTurnHandle != null)
                {
                    _startTurnHandle.Dragged -= StartTurnDragged;
                    Plot.Remove(_startTurnHandle);
                    _startTurnHandle = null;
                }
                if (_endTurnHandle != null)
                {
                    _endTurnHandle.Dragged -= EndTurnDragged;
                    Plot.Remove(_endTurnHandle);
                    _endTurnHandle = null;
                }
                if (_torqueWidthHandle != null)
                {
                    _torqueWidthHandle.Dragged -= TorqueWidthDragged;
                    Plot.Remove(_torqueWidthHandle);
                    _torqueWidthHandle = null;
                }
                if (_centerHandle != null)
                {
                    _centerHandle.Dragged -= WholeDragged;
                    Plot.Remove(_centerHandle);
                    _centerHandle = null;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified plot coordinate is inside this window.
        /// </summary>
        /// <param name="x">Number of turns.</param>
        /// <param name="y">Torque value.</param>
        /// <returns>True when the point lies inside an enabled window.</returns>
        public bool IsInWindow(
            double x,
            double y
        )
        {
            if (IsEnabled.Value)
            {
                if (StartTurn.Value <= x && x <= EndTurn.Value)
                {
                    // Interpolate the torque center line at the given number of turns.
                    var xRatio = (x - StartTurn.Value) / (EndTurn.Value - StartTurn.Value);
                    var torque = TorqueAtStart.Value + (TorqueAtEnd.Value - TorqueAtStart.Value) * xRatio;
                    if (torque - TorqueWidth.Value <= y && y <= torque + TorqueWidth.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Writes the current plot geometry back into the properties after a drag operation.
        /// </summary>
        public void Dropped()
        {
            // Suppress redrawing while the properties are updated one by one.
            _updating = true;

            StartTurn.Value = (float)_startTurnHandle!.X;
            EndTurn.Value = (float)_endTurnHandle!.X;
            TorqueAtStart.Value = (float)_startTurnHandle.Y;
            TorqueAtEnd.Value = (float)_endTurnHandle!.Y;
            TorqueWidth.Value = (float)((_windowRect!.Ys[_topLeft] - _windowRect.Ys[_bottomLeft]) / 2);

            _updating = false;
            DrawWindow();
        }

        /// <summary>
        /// Initializes a new empty window item.
        /// </summary>
        /// <param name="parentViewModel">Owner view model.</param>
        /// <param name="index">Zero-based window index.</param>
        public WindowItem(
            EditWindowWindowViewModel parentViewModel,
            int index
        )
        {
            Number = index + 1;

            ParentViewModel = parentViewModel;

            CreatePlot();

            // Hook up validation for the editable values.
            StartTurn.SetValidateNotifyError((value) => CheckNumTurns(StartTurn, value)).AddTo(_disposables);
            EndTurn.SetValidateNotifyError((value) => CheckNumTurns(EndTurn, value)).AddTo(_disposables);
            TorqueAtStart.SetValidator(parentViewModel);
            TorqueAtEnd.SetValidator(parentViewModel);
            TorqueWidth.SetValidator(parentViewModel);

            // Redraw the window whenever any displayed value changes.
            IsEnabled.Subscribe((_) => DrawWindow()).AddTo(_disposables);
            StartTurn.Subscribe((_) => DrawWindow()).AddTo(_disposables);
            EndTurn.Subscribe((_) => DrawWindow()).AddTo(_disposables);
            TorqueAtStart.PropertyChanged += (_, _) =>
            {
                DrawWindow();
            };
            TorqueAtEnd.PropertyChanged += (_, _) =>
            {
                DrawWindow();
            };
            TorqueWidth.PropertyChanged += (_, _) =>
            {
                DrawWindow();
            };
            IsSelected.Subscribe((_) => DrawWindow()).AddTo(_disposables);
        }

        /// <summary>
        /// Initializes a new window item from existing window data.
        /// </summary>
        /// <param name="parentViewModel">Owner view model.</param>
        /// <param name="window">Source window data.</param>
        public WindowItem(
            EditWindowWindowViewModel parentViewModel,
            ModelPFData.Program.Window window
        )
        : this(parentViewModel, window.Index)
        {
            Set(window);

            // Enable validation only after the initial values have been applied.
            _validateReady = true;
        }

        /// <summary>
        /// Removes the plot objects and releases the subscriptions.
        /// </summary>
        public void Dispose()
        {
            DestroyPlot();

            GC.SuppressFinalize(this);
            _disposables.Dispose();
        }

        /// <summary>
        /// Determines whether the current values differ from the originally loaded ones.
        /// </summary>
        public bool IsModified()
        {
            if (_original == null)
            {
                // Nothing was loaded, so there is nothing to compare against.
                return false;
            }
            else
            {
                // Compare a copy filled with the current values against the original.
                Window current = _original.Clone();
                SetBack(current);

                return !_original.Equals(current);
            }
        }

        /// <summary>
        /// Loads the values from the specified window data and keeps a copy as the original.
        /// </summary>
        /// <param name="window">Source window data.</param>
        public void Set(
            ModelPFData.Program.Window window
        )
        {
            _original = window.Clone();

            IsEnabled.Value = window.IsEnabled;
            StartTurn.Value = window.StartTurn;
            EndTurn.Value = window.EndTurn;
            // Torque values are stored internally and converted for display.
            TorqueAtStart.InnerValue = window.TorqueAtStart;
            TorqueAtStart.TorqueUnit = TorqueUnit;
            TorqueAtEnd.InnerValue = window.TorqueAtEnd;
            TorqueAtEnd.TorqueUnit = TorqueUnit;
            TorqueWidth.InnerValue = window.TorqueWidth;
            TorqueWidth.TorqueUnit = TorqueUnit;
        }

        /// <summary>
        /// Writes the current values back into the specified window data.
        /// </summary>
        /// <param name="window">Destination window data.</param>
        public void SetBack(
            ModelPFData.Program.Window window
        )
        {
            window.IsEnabled = IsEnabled.Value;
            window.StartTurn = StartTurn.Value;
            window.EndTurn = EndTurn.Value;
            window.TorqueAtStart = TorqueAtStart.InnerValue;
            window.TorqueAtEnd = TorqueAtEnd.InnerValue;
            window.TorqueWidth = TorqueWidth.InnerValue;
        }
    }
}

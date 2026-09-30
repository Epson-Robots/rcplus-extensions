// -----------------------------------------------------------------------
// <copyright file="EditWindowWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ScottPlot;
using System.Drawing;
using System.Windows.Input;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using VanguardModelPFScrewdriver.Utils;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;

namespace VanguardModelPFScrewdriver.Dialogs.EditWindow
{
    /// <summary>
    /// View model for the "Edit Window" dialog.
    /// Lets the user edit tightening judgement windows on a torque/turns plot.
    /// </summary>
    internal class EditWindowWindowViewModel : DialogViewModel, IValidator
    {
        /// <summary>Torque unit currently used for display and editing.</summary>
        public ReactivePropertySlim<TorqueUnitKind> TorqueUnit { get; } = new(TorqueUnitKind.MNM);

        /// <summary>Editable window items shown in the list and on the plot.</summary>
        public ReactiveCollection<WindowItem> WindowItems { get; } = [];

        /// <summary>Hint message shown for the focused input field.</summary>
        public ReactivePropertySlim<string> Hint { get; } = new();

        /// <summary>Valid value range associated with the current hint.</summary>
        public FloatRange HintRange { get; set; } = new();

        /// <summary>Window item currently selected by the user (null when nothing is selected).</summary>
        public ReactivePropertySlim<WindowItem?> SelectedWindowItem { get; } = new(null);

        /// <summary>Holds the plot control so that the view can host it.</summary>
        public ReactivePropertySlim<WpfPlot?> PlotControlHolder { get; } = new(null);

        /// <summary>Wrapper that encapsulates plot drawing and mouse interaction.</summary>
        public PlotControlWrapper Plot = new();

        /// <summary>Previously selected item, used to clear its selection state.</summary>
        private WindowItem? _lastSelectedItem;

        /// <summary>
        /// Updates the selection state when the selected window item changes.
        /// </summary>
        /// <param name="item">Newly selected item, or null if the selection was cleared.</param>
        private void SelectedWindowItemChanged(
            WindowItem? item
        )
        {
            // Deselect the previous item.
            if (_lastSelectedItem != null)
            {
                _lastSelectedItem.IsSelected.Value = false;
            }
            // Select the new item.
            if (item != null)
            {
                item.IsSelected.Value = true;
            }
            _lastSelectedItem = item;
        }

        /// <summary>
        /// Handles a mouse click on the plot and selects the window that contains the clicked point.
        /// </summary>
        /// <param name="ev">Mouse event supplied by the view.</param>
        public void Clicked(
            MouseEventArgs ev
        )
        {
            // Convert the mouse position into plot (data) coordinates.
            var (x, y) = Plot.GetMouseCoordinate(ev);

            foreach (var item in WindowItems)
            {
                if (item.IsInWindow(x, y))
                {
                    SelectedWindowItem.Value = item;
                    break;
                }
            }
        }

        /// <summary>
        /// Called when the dialog is confirmed. Writes the edited windows back to the parameters.
        /// </summary>
        protected override Task OnOK()
        {
            if (_parameters != null)
            {
                List<ModelPFData.Program.Window> windows = [];

                // Convert each view model item back into a data model window.
                foreach (var windowItem in WindowItems)
                {
                    ModelPFData.Program.Window window = new(windowItem.Number - 1);
                    windowItem.SetBack(window);
                    windows.Add(window);
                }

                _parameters["NewWindows"] = windows;
            }

            return base.OnOK();
        }

        /// <summary>
        /// Called when the dialog is cancelled. Asks for confirmation if there are unsaved changes.
        /// </summary>
        protected override void OnCancel()
        {
            // Check whether any window item has been modified.
            var isModified = false;
            foreach (var windowItem in WindowItems)
            {
                if (windowItem.IsModified())
                {
                    isModified = true;
                    break;
                }
            }

            if (isModified)
            {
                // Warn the user that the changes will be discarded.
                var response = Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, Caption.DiscardChangesWarning),
                    ButtonType.Yes_No,
                    IconType.Warning,
                    _window
                );
                if (response == ResponseType.No)
                {
                    // Keep the dialog open.
                    return;
                }
            }

            base.OnCancel();
        }

        /// <summary>
        /// Expands the plot axes so that every window and graph fits into the visible area.
        /// </summary>
        public void AdjustScale()
        {
            var (maxX, maxY) = Plot.CalcMax();

            var (_, curMaxX) = Plot.GetAxisLimitsX();
            var (_, curMaxY) = Plot.GetAxisLimitsY();

            // Only extend the axes; never shrink them.
            if (maxX > curMaxX)
            {
                Plot.SetAxisLimitsX(0, maxX);
            }
            if (maxY > curMaxY)
            {
                Plot.SetAxisLimitsY(0, maxY);
            }
        }

        /// <summary>
        /// Initializes the view model from the dialog parameters and the ModelPF data.
        /// </summary>
        protected override void Setup()
        {
            if (_proFuseData != null && _parameters!["TargetProgram"] is ProgramItemViewModel targetProgramItem)
            {
                // Apply the display settings shared by all window items.
                TorqueUnit.Value = _proFuseData.PreferenceData.TorqueUnit;
                WindowItem.TorqueUnit = _proFuseData.PreferenceData.TorqueUnit;
                WindowItem.DecimalPlaces = TorqueValueConverter.DecimalPlaces(WindowItem.TorqueUnit);
                WindowItem.ToolType = (ToolKind)_parameters["ToolType"]!;
                WindowItem.Mode = (ModeKind)_parameters["Mode"]!;

                // Set up the plot area using the upper limits of the target program.
                WindowItem.Plot = Plot;
                Plot.SetAxisLimitsY(0, (float)_parameters["TorqueUpperLimit"]!);
                Plot.SetAxisLimitsX(0, (float)_parameters["NumTurnsUpperLimit"]!);
                Plot.UpdateTerms(WindowItem.TorqueUnit);

                List<ModelPFData.Program.Window>? windows = null;

                // Prefer windows edited in a previous session of this dialog, if any.
                if (_parameters?.TryGetValue("NewWindows", out var list) == true)
                {
                    windows = list as List<ModelPFData.Program.Window>;
                }
                windows ??= _proFuseData.ProgramData[targetProgramItem.ProgramNo].Tightening.Windows;

                foreach (var window in windows)
                {
                    WindowItems.Add(new WindowItem(this, window));
                }

                // Draw the reference torque graphs (blue for OK results, red otherwise).
                if (
                    _parameters?.TryGetValue("Graphs", out var obj) == true
                    && obj is Dictionary<ResultLogEntry, PlotControlWrapper.TorquePlot> graphs
                )
                {
                    foreach (var (entry, graph) in graphs)
                    {
                        Plot.Add(graph.Plot.Xs, graph.Plot.Ys, entry.Result == "OK" ? Color.Blue : Color.Red, TorqueUnit.Value);
                    }
                }
            }

            PlotControlHolder.Value = Plot.PlotControl;

            // Notify the selected item when a drag operation on the plot has finished.
            Plot.Dropped += (_, _) =>
            {
                SelectedWindowItem.Value?.Dropped();
            };

            AdjustScale();
            Plot.Refresh();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditWindowWindowViewModel"/> class.
        /// </summary>
        public EditWindowWindowViewModel()
        {
            // Keep the selection state of the window items in sync.
            SelectedWindowItem
                .Subscribe(SelectedWindowItemChanged)
                .AddTo(_disposables);
        }

        // ======================================================================

        /// <summary>Targets (input fields) that currently hold an invalid value.</summary>
        private readonly HashSet<object> _invalidTargets = [];

        /// <summary>Gets a value indicating whether at least one input is invalid.</summary>
        public bool HasInvalidTarget => (_invalidTargets.Count > 0);

        /// <summary>
        /// Registers or clears the invalid state of the specified target.
        /// </summary>
        /// <param name="target">Validation target, typically an input control or item.</param>
        /// <param name="isValid">True when the target value is valid.</param>
        public void SetValid(
            object target,
            bool isValid
        )
        {
            if (isValid)
            {
                _invalidTargets.Remove(target);
            }
            else
            {
                _invalidTargets.Add(target);
            }
        }

        /// <summary>
        /// Validates the specified value against the currently active hint range.
        /// </summary>
        /// <param name="target">Validation target, typically the input control being edited.</param>
        /// <param name="value">Value entered by the user.</param>
        /// <returns>An error message key when the value is out of range; otherwise null.</returns>
        public string? Validate(
            object target,
            object value
        )
        {
            try
            {
                // Only numeric values are validated here.
                if (Convert.ChangeType(value, typeof(float)) is float floatValue)
                {
                    var isValid = HintRange.IsInRange(floatValue);

                    // Update the invalid target set and the OK button state.
                    SetValid(target, isValid);
                    CanOK.Value = !HasInvalidTarget;

                    if (!isValid)
                    {
                        return "ValueOutOfRange";
                    }
                }
            }
            catch (Exception)
            {
                // The value is not convertible to a number; treat it as no validation result.
                // EMPTY
            }

            return null;
        }

        /// <summary>
        /// Refreshes the enabled state of the OK button based on the current validation results.
        /// </summary>
        public void UpdateCanOK()
        {
            CanOK.Value = !HasInvalidTarget;
        }
    }
}

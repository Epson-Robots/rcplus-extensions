// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelGraph.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using ScottPlot;
using System.Drawing;
using System.IO;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.Utils;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ResultLogEntry;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that draws torque waveforms
    /// and controls the scaling of the graph axes.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Validates that the axis upper limit values entered by the user are positive.
        /// </summary>
        public class UpperLimitValidator : IValidator
        {
            /// <summary>
            /// Gets or sets the action invoked when a target becomes valid.
            /// </summary>
            public Action? ChangedAction { get; set; }

            // Targets that currently hold an invalid value.
            private readonly HashSet<object> _invalidTargets = [];

            /// <summary>
            /// Gets a value indicating whether at least one target holds an invalid value.
            /// </summary>
            public bool HasInvalidTarget => (_invalidTargets.Count > 0);

            /// <summary>
            /// Updates the validity state of the specified target.
            /// </summary>
            /// <param name="target">Target to update.</param>
            /// <param name="isValid">True if the target holds a valid value.</param>
            public void SetValid(
                object target,
                bool isValid
            )
            {
                if (isValid)
                {
                    _invalidTargets.Remove(target);
                    ChangedAction?.Invoke();
                }
                else
                {
                    _invalidTargets.Add(target);
                }
            }

            /// <summary>
            /// Validates the specified value of a target.
            /// </summary>
            /// <param name="target">Target being validated.</param>
            /// <param name="value">Value to validate.</param>
            /// <returns>An error key if the value is invalid; otherwise null.</returns>
            public string? Validate(
                object target,
                object value
            )
            {
                // Axis upper limits must be greater than zero.
                if (value is float floatValue && floatValue <= 0)
                {
                    SetValid(target, false);
                    return "ShouldBePositive";
                }
                else
                {
                    SetValid(target, true);
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the X axis is scaled automatically.
        /// </summary>
        public ReactivePropertySlim<bool> NumTurnsAutoScaling { get; } = new(true);

        /// <summary>
        /// Gets a value indicating whether the X axis uses a fixed upper limit.
        /// </summary>
        public ReactivePropertySlim<bool> NumTurnsFixedScaling { get; } = new(false);

        /// <summary>
        /// Gets the fixed upper limit of the X axis (number of turns).
        /// </summary>
        public ReactiveProperty<float> NumTurnsUpperLimit { get; } = new();

        /// <summary>
        /// Gets a value indicating whether the Y axis is scaled automatically.
        /// </summary>
        public ReactivePropertySlim<bool> TorqueAutoScaling { get; } = new(true);

        /// <summary>
        /// Gets a value indicating whether the Y axis uses a fixed upper limit.
        /// </summary>
        public ReactivePropertySlim<bool> TorqueFixedScaling { get; } = new(false);

        /// <summary>
        /// Gets the fixed upper limit of the Y axis (torque).
        /// </summary>
        public TorqueProperty TorqueUpperLimit { get; } = new();

        /// <summary>
        /// Gets the holder that exposes the plot control to the view.
        /// </summary>
        public ReactivePropertySlim<WpfPlot?> PlotControlHolder { get; } = new(null);

        /// <summary>
        /// Wrapper of the plot control used to draw the waveforms.
        /// </summary>
        public PlotControlWrapper Plot = new();

        // Waveform of the currently selected tightening result.
        private PlotControlWrapper.TorquePlot? _currentGraph;

        // Waveforms drawn additionally for comparison, keyed by their log entry.
        private readonly Dictionary<ResultLogEntry, PlotControlWrapper.TorquePlot> _overlayGraphs = [];

        // Bounding areas of all drawn waveforms.
        private readonly GraphAreaMap _areaMap = new();

        // Validator of the fixed axis upper limits.
        private readonly UpperLimitValidator _upperLimitValidator = new();

        /// <summary>
        /// Removes all waveforms from the graph.
        /// </summary>
        private void ClearGraph()
        {
            _areaMap.Clear();
            Plot.RefreshWithClear();
        }

        /// <summary>
        /// Builds the file name of a torque waveform file.
        /// </summary>
        /// <param name="resultKind">Tightening result kind ("OK" or "NG").</param>
        /// <param name="waveIndex">Index of the waveform in the ring buffer.</param>
        /// <returns>The waveform file name.</returns>
        private string GetWaveFileName(
            string resultKind,
            int waveIndex
        )
        {
            return $"VGD_PF_{SelectedToolType.Value}_{SelectedCTContsId.Value:D3}_{resultKind}_{waveIndex:D5}.tw";
        }

        /// <summary>
        /// Loads the waveform belonging to the specified log entry and adds it to the graph.
        /// </summary>
        /// <param name="entry">Result log entry whose waveform is drawn.</param>
        /// <param name="color">Line color of the waveform.</param>
        /// <returns>The created plot, or null if no waveform could be drawn.</returns>
        private PlotControlWrapper.TorquePlot? AddGraph(
            ResultLogEntry entry,
            Color color
        )
        {
            // OK and NG waveforms are stored in separate ring buffers.
            var lineNo = entry.LineNo;
            var waveIndex = entry.Result == "OK" ? RingMapOK.GetWaveIndex(lineNo) : RingMapNG.GetWaveIndex(lineNo);
            if (waveIndex >= 0)
            {
                var resultKind = entry.Result;

                var logFolder = GetLogFolder();
                if (logFolder == null)
                {
                    return null;
                }

                var waveFilePath = Path.Combine(logFolder, GetWaveFileName(resultKind, waveIndex));
                if (!File.Exists(waveFilePath))
                {
                    // The waveform file has already been overwritten or removed.
                    entry.CanDrawWave.Value = false;
                    return null;
                }

                var waveData = TorqueWaveformData.Load(waveFilePath);
                if (waveData != null)
                {
                    // Convert the sampling angle into the number of turns per sample.
                    double turnStep = (double)waveData.SamplingAngle / 360d;
                    if (turnStep == 0)
                    {
                        return null;
                    }
                    double[] dataX = [.. Enumerable.Range(0, waveData.Count).Select(x => (double)x * turnStep)];
                    double[] dataY = [.. waveData.Torques.Select(x => (double)x)];

                    if (dataX.Length == 0)
                    {
                        return null;
                    }
                    var graph =  Plot.Add(dataX, dataY, color, TorqueUnit.Value);

                    // Register the drawn area so that the axes can be scaled to all waveforms.
                    GraphAreaMap.Area area = new()
                    {
                        X = new(0, (float)dataX[^1]),
                        Y = waveData.TorqueRange,
                    };
                    _areaMap.Add(graph, area);

                    return graph;
                }
            }

            return null;
        }

        /// <summary>
        /// Applies the current scaling settings to both axes.
        /// </summary>
        private void SetScale()
        {
            if (_areaMap.IsEmpty)
            {
                return;
            }

            if (NumTurnsAutoScaling.Value)
            {
                Plot.SetAxisLimitsX(0, _areaMap.X.Max);
            }
            else
            {
                // Never cut off the waveform below its minimum range.
                Plot.SetAxisLimitsX(0, Math.Max(_areaMap.X.Min, NumTurnsUpperLimit.Value));
            }

            if (TorqueAutoScaling.Value)
            {
                Plot.SetAxisLimitsY(0, _areaMap.Y.Max);
            }
            else
            {
                Plot.SetAxisLimitsY(0, Math.Max(_areaMap.Y.Min, TorqueUpperLimit.Value));
            }
        }

        /// <summary>
        /// Redraws the graph when the selected tightening result has changed.
        /// </summary>
        /// <param name="entry">Newly selected result log entry, or null if nothing is selected.</param>
        private void OnSelectedTighteningResultChanged(
            ResultLogEntry? entry
        )
        {
            var refresh = false;

            // Remove the waveform of the previously selected entry.
            if (_currentGraph != null)
            {
                Plot.Remove(_currentGraph.Plot);
                _areaMap.Remove(_currentGraph);
                refresh = true;
            }

            if (entry != null && entry.CanDrawWave.Value)
            {
                _currentGraph = AddGraph(entry, Color.Red);

                if (_currentGraph != null)
                {
                    // Emphasize the selected waveform with a thicker line.
                    _currentGraph.Plot.LineWidth = 2;
                    refresh = true;
                }
            }

            if (refresh)
            {
                SetScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Adds or removes an overlay waveform when the draw state of a log entry has changed.
        /// </summary>
        /// <param name="sender">Result log entry that raised the event.</param>
        /// <param name="ev">Event data indicating whether the waveform should be drawn.</param>
        private void LogEntryChangedHandler(
            object? sender,
            ResultLogEntryEventArgs ev
        )
        {
            if (sender is ResultLogEntry entry)
            {
                if (ev.DrawWave)
                {
                    var graph = AddGraph(entry, Color.Blue);
                    if (graph == null)
                    {
                        return;
                    }
                    _overlayGraphs[entry] = graph;
                }
                else
                {
                    // Remove the overlay waveform of this entry.
                    if (_overlayGraphs.TryGetValue(entry, out var graph))
                    {
                        Plot.Remove(graph.Plot);
                        _overlayGraphs.Remove(entry);
                        _areaMap.Remove(graph);
                    }
                }

                SetScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Re-applies the scaling when the scaling settings have been changed by the user.
        /// </summary>
        private void OnScaleSettingsChanged()
        {
            // Ignore the change while an invalid upper limit is entered.
            if (
                !_upperLimitValidator.HasInvalidTarget
                && (_currentGraph != null || _overlayGraphs.Count > 0)
            )
            {
                SetScale();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Converts the values of all drawn waveforms to the specified torque unit.
        /// </summary>
        /// <param name="torqueUnit">Torque unit to display.</param>
        private void RescaleGraphs(
            TorqueUnitKind torqueUnit
        )
        {
            _currentGraph?.ChangeUnit(torqueUnit);

            foreach (var graph in _overlayGraphs.Values)
            {
                graph.ChangeUnit(torqueUnit);
            }
        }
    }
}

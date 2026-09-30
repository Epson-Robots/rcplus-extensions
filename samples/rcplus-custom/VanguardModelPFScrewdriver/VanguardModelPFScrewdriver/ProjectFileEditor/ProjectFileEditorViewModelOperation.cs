// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelOperation.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using VanguardModelPFScrewdriver.ModelPF;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFRegister;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that starts and stops the screwdriver
    /// operations (tightening, loosening, free run) and logs their results.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Gets a value indicating whether a tightening operation is running.
        /// </summary>
        public ReactivePropertySlim<bool> TightenGoing { get; } = new(false);

        /// <summary>
        /// Gets a value indicating whether a loosening operation is running.
        /// </summary>
        public ReactivePropertySlim<bool> LoosenGoing { get; } = new(false);

        /// <summary>
        /// Gets a value indicating whether a free run operation is running.
        /// </summary>
        public ReactivePropertySlim<bool> FreeRunGoing { get; } = new(false);

        /// <summary>
        /// Gets a value indicating whether any operation is running.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> Operating { get; }

        // ======================================================================

        /// <summary>
        /// Gets the caption of the tightening button (start or stop).
        /// </summary>
        public ReadOnlyReactivePropertySlim<IRCXLangRxCaption> TighteningCaption { get; }

        /// <summary>
        /// Gets the command currently assigned to the tightening button.
        /// </summary>
        public ReadOnlyReactivePropertySlim<AsyncReactiveCommand> TighteningCommand { get; }

        /// <summary>
        /// Command that starts a tightening operation.
        /// </summary>
        public AsyncReactiveCommand StartTighteningCommand { get; }

        /// <summary>
        /// Gets the caption of the loosening button (start or stop).
        /// </summary>
        public ReadOnlyReactivePropertySlim<IRCXLangRxCaption> LooseningCaption { get; }

        /// <summary>
        /// Gets the command currently assigned to the loosening button.
        /// </summary>
        public ReadOnlyReactivePropertySlim<AsyncReactiveCommand> LooseningCommand { get; }

        /// <summary>
        /// Command that starts a loosening operation.
        /// </summary>
        public AsyncReactiveCommand StartLooseningCommand { get; }

        /// <summary>
        /// Gets the caption of the free run button (start or stop).
        /// </summary>
        public ReadOnlyReactivePropertySlim<IRCXLangRxCaption> FreeRunCaption { get; }

        /// <summary>
        /// Gets the command currently assigned to the free run button.
        /// </summary>
        public ReadOnlyReactivePropertySlim<AsyncReactiveCommand> FreeRunCommand { get; }

        /// <summary>
        /// Command that starts a free run operation.
        /// </summary>
        public AsyncReactiveCommand StartFreeRunCommand { get; }

        /// <summary>
        /// Command that stops the running operation.
        /// </summary>
        public AsyncReactiveCommand StopOperationCommand { get; }

        /// <summary>
        /// Gets the END signal state of the screwdriver.
        /// </summary>
        public ReactivePropertySlim<bool> EndState { get; } = new(false);

        /// <summary>
        /// Gets the BUSY signal state of the screwdriver.
        /// </summary>
        public ReactivePropertySlim<bool> BusyState { get; } = new(false);

        /// <summary>
        /// Gets the ERR signal state of the screwdriver.
        /// </summary>
        public ReactivePropertySlim<bool> ErrState { get; } = new(false);

        // Polls the screwdriver signals while an operation is running.
        private readonly DispatcherTimer _operatingStatusWatcher = new();

        // Polling interval of the operating status watcher.
        private readonly TimeSpan _intervalForWatchingOperatingStatus = new(0, 0, 0, 0, 50);

        // True once the BUSY signal has been observed for the running operation.
        private bool _busyDetected = false;

        // True while a tightening result is being logged.
        private bool _loggingInProcess = false;

        // Guards access to _loggingInProcess.
        private readonly object _loggingLock = new();

        // True if the user intended to stop the operation.
        private bool _stopTriggered;

        // Semaphore for safe operation start
        private SemaphoreSlim _operationStartLock = new(1, 1);

        // ======================================================================

        /// <summary>
        /// Starts a tightening operation with the selected program.
        /// </summary>
        private async Task OnStartTighteningAsync()
        {
            if (_proFuseClient != null && SelectedProgramItem.Value != null)
            {
                await _operationStartLock.WaitAsync();

                _stopTriggered = false;

                // An empty program would not perform any motion.
                var steps = _proFuseData.ProgramData[SelectedProgramItem.Value.ProgramNo].Tightening.Steps;
                if (steps.Count == 0 || steps[0].NumTurns == 0)
                {
                    ErrorReporter.Message(Caption.NoProgramSteps);
                }
                else
                {
                    TightenGoing.Value = await _proFuseClient.StartTighteningAsync(SelectedProgramItem.Value.ProgramNo);
                }

                _operationStartLock.Release();
            }
        }

        /// <summary>
        /// Starts a loosening operation with the selected program.
        /// </summary>
        private async Task OnStartLooseningAsync()
        {
            if (_proFuseClient != null && SelectedProgramItem.Value != null)
            {
                await _operationStartLock.WaitAsync();

                _stopTriggered = false;

                // An empty program would not perform any motion.
                var steps = _proFuseData.ProgramData[SelectedProgramItem.Value.ProgramNo].Loosening.Steps;
                if (steps.Count == 0 || steps[0].NumTurns == 0)
                {
                    ErrorReporter.Message(Caption.NoProgramSteps);
                }
                else
                {
                    LoosenGoing.Value = await _proFuseClient.StartLooseningAsync(SelectedProgramItem.Value.ProgramNo);
                }

                _operationStartLock.Release();
            }
        }

        /// <summary>
        /// Starts a free run operation with the selected program.
        /// </summary>
        private async Task OnStartFreeRunAsync()
        {
            if (_proFuseClient != null && SelectedProgramItem.Value != null)
            {
                await _operationStartLock.WaitAsync();

                _stopTriggered = false;

                FreeRunGoing.Value = await _proFuseClient.StartFreeRunAsync(SelectedProgramItem.Value.ProgramNo);

                _operationStartLock.Release();
            }
        }

        /// <summary>
        /// Stops the operation that is currently running.
        /// </summary>
        private async Task OnStopOperationAsync()
        {
            if (_proFuseClient != null && Operating.Value)
            {
                await _operationStartLock.WaitAsync();

                if (!_stopTriggered)
                {
                    _stopTriggered = true;

                    _ = await _proFuseClient.StopOperationAsync();
                }

                _operationStartLock.Release();
            }
        }

        /// <summary>
        /// Retrieves the result of the finished tightening operation, stores it together with
        /// its torque waveform and adds it to the result list.
        /// </summary>
        private async Task LogTighteningAsync()
        {
            // Make sure only one logging sequence runs at a time.
            lock(_loggingLock) {
                if (_loggingInProcess)
                {
                    return;
                }
                _loggingInProcess = true;
            }

            if (_proFuseClient != null)
            {
                ResultLogEntry? resultLogEntry = null;
                if (!_stopTriggered)
                {
                    resultLogEntry = await _proFuseClient.GetTighteningResultAsync();
                }
                if (resultLogEntry != null)
                {
                    await AppendResultLogToFileAsync(resultLogEntry);

                    // Store the torque waveform belonging to this result.
                    var torqueWaveformData = await _proFuseClient.GetTorqueDataAsync(resultLogEntry.TimeStamp);
                    if (torqueWaveformData != null)
                    {
                        var logFolder = GetLogFolder();
                        if (logFolder != null)
                        {
                            var waveFilePath = Path.Combine(logFolder, GetWaveFileName(resultLogEntry.Result, resultLogEntry.WaveIndex));
                            if (waveFilePath != null)
                            {
                                torqueWaveformData.Save(waveFilePath);
                            }
                        }
                    }
                }

                lock (_loggingLock)
                {
                    _loggingInProcess = false;
                }

                // Update the bound properties on the UI thread.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TightenGoing.Value = false;

                    if (resultLogEntry != null)
                    {
                        TighteningResults.Add(resultLogEntry);
                        if (resultLogEntry.Result == "OK")
                        {
                            // The waveform slot is reused, so the previous owner loses its waveform.
                            var curLineNo = RingMapOK.GetLineNo(resultLogEntry.WaveIndex);
                            if (curLineNo >= 0)
                            {
                                var curLogEntry = TighteningResults.FirstOrDefault(x => x.LineNo == curLineNo);
                                if (curLogEntry != null)
                                {
                                    curLogEntry.CanDrawWave.Value = false;
                                }
                            }
                            RingMapOK.Add(resultLogEntry.LineNo, resultLogEntry.WaveIndex);
                            resultLogEntry.CanDrawWave.Value = true;
                        }
                        else
                        {
                            // The NG waveforms are managed by a separate ring buffer.
                            var curLineNo = RingMapNG.GetLineNo(resultLogEntry.WaveIndex);
                            if (curLineNo >= 0)
                            {
                                var curLogEntry = TighteningResults.FirstOrDefault(x => x.LineNo == curLineNo);
                                if (curLogEntry != null)
                                {
                                    curLogEntry.CanDrawWave.Value = false;
                                }
                            }
                            RingMapNG.Add(resultLogEntry.LineNo, resultLogEntry.WaveIndex);
                            resultLogEntry.CanDrawWave.Value = true;
                        }
                        SelectedTighteningResult.Value = resultLogEntry;
                    }
                });
            }
        }

        /// <summary>
        /// Starts watching the screwdriver signals when an operation has been started.
        /// </summary>
        /// <param name="operating">True if an operation is running.</param>
        private Task OperatingChangedAsync(
            bool operating
        )
        {
            if (operating)
            {
                _busyDetected = false;
                _operatingStatusWatcher.Start();
            }
            else
            {
                BusyState.Value = false;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Polls the screwdriver signals and detects the completion of the running operation.
        /// </summary>
        /// <param name="sender">Timer that raised the event.</param>
        /// <param name="ev">Event data.</param>
        private async Task OperatingStatusWatcherOnTickAsync(
            object? sender,
            EventArgs ev
        )
        {
            if (_proFuseClient != null)
            {
                _operatingStatusWatcher.Stop();

                // Read the current output signals of the screwdriver.
                DOValue? doValue = null;
                if (!_stopTriggered)
                {
                    doValue = await _proFuseClient.CheckStatusAsync();
                }

                if (doValue.HasValue)
                {
                    EndState.Value = (doValue & DOValue.END) != 0;
                    BusyState.Value = (doValue & DOValue.BUSY) != 0;
                    ErrState.Value = (doValue & DOValue.ERR) != 0;
                }

                if (BusyState.Value && !_busyDetected)
                {
                    _busyDetected = true;
                }

                // The operation has finished when BUSY falls again, or END / ERR is reported.
                if (
                    _stopTriggered ||
                    (
                        Operating.Value
                        && (
                            (_busyDetected && !BusyState.Value)
                            || EndState.Value
                            || ErrState.Value
                        )
                    )
                )
                {
                    // Only tightening operations produce a result to be logged.
                    if (TightenGoing.Value)
                    {
                        _ = Task.Run(LogTighteningAsync);
                    }
                    LoosenGoing.Value = false;
                    FreeRunGoing.Value = false;
                }
                else
                {
                    _operatingStatusWatcher.Start();
                }
            }
        }
    }
}

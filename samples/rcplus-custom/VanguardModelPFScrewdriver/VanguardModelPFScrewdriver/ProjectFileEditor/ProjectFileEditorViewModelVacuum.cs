// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelVacuum.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using VanguardModelPFScrewdriver.Dialogs;
using VanguardModelPFScrewdriver.Dialogs.VacuumSettings;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXIOAPI;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static VanguardModelPFScrewdriver.Constants;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Vacuum related part of the project file editor view model.
    /// Provides commands and states used to control the vacuum (suction) hardware.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Gets the command that opens the vacuum settings dialog.
        /// </summary>
        public ReactiveCommand VacuumSettingsCommand { get; }

        /// <summary>
        /// Gets the output I/O label used to turn the vacuum on or off.
        /// </summary>
        public ReactivePropertySlim<IRCXOutputIOLabel<bool>?> VacuumOnLabel { get; } = new(null);

        /// <summary>
        /// Gets the command that writes the vacuum on/off state to the output I/O label.
        /// </summary>
        public AsyncReactiveCommand<bool> VacuumOnCommand { get; } = new();

        /// <summary>
        /// Gets the output I/O label used to turn the vacuum break on or off.
        /// </summary>
        public ReactivePropertySlim<IRCXOutputIOLabel<bool>?> VacuumBreakOnLabel { get; } = new(null);

        /// <summary>
        /// Gets the command that writes the vacuum break on/off state to the output I/O label.
        /// </summary>
        public AsyncReactiveCommand<bool> VacuumBreakOnCommand { get; } = new();

        /// <summary>
        /// Gets the I/O watcher that monitors the suction detection input.
        /// </summary>
        public ReactivePropertySlim<IRCXIOWatcher?> SuctionStateLabel { get; } = new(null);

        /// <summary>
        /// Gets the current suction state; <see langword="true"/> when suction is detected.
        /// </summary>
        public ReactivePropertySlim<bool> SuctionState { get; } = new(false);

        // ======================================================================

        /// <summary>
        /// Shows the vacuum settings dialog and refreshes the I/O labels after it is closed.
        /// </summary>
        private void OnVacuumSettings()
        {
            _ = DialogViewModel.ShowDialog<VacuumSettingsWindow>(_proFuseData, _proFuseClient);

            // Re-resolve the I/O labels because the settings may have been changed.
            SetIOLabels();
        }

        /// <summary>
        /// Writes the requested vacuum state to the assigned output I/O label.
        /// </summary>
        /// <param name="isOn"><see langword="true"/> to turn the vacuum on; otherwise <see langword="false"/>.</param>
        /// <returns>A completed task, because the write operation is synchronous.</returns>
        private Task OnVacuumOn(
            bool isOn
        )
        {
            // Do nothing when no I/O label is assigned.
            var result = VacuumOnLabel.Value?.Write(isOn);
            if (result.HasValue && result.Value != RCXResult.Success)
            {
                ErrorReporter.Message(Caption.IOWritingError);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes the requested vacuum break state to the assigned output I/O label.
        /// </summary>
        /// <param name="isOn"><see langword="true"/> to turn the vacuum break on; otherwise <see langword="false"/>.</param>
        /// <returns>A completed task, because the write operation is synchronous.</returns>
        private Task OnVacuumBreakOn(
            bool isOn
        )
        {
            // Do nothing when no I/O label is assigned.
            var result = VacuumBreakOnLabel.Value?.Write(isOn);
            if (result.HasValue && result.Value != RCXResult.Success)
            {
                ErrorReporter.Message(Caption.IOWritingError);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the suction input change notification raised by the I/O watcher.
        /// </summary>
        /// <param name="watcher">The I/O watcher that raised the notification.</param>
        /// <param name="oldData">The previous input value.</param>
        /// <param name="newData">The new input value.</param>
        private void SuctionStateChanged(
            IRCXIOWatcher watcher,
            int oldData,
            int newData
        )
        {
            // Any non-zero value means that suction is detected.
            SuctionState.Value = (newData != 0);
        }
        // ======================================================================

        /// <summary>
        /// Resolves the configured I/O labels and (re)creates the watcher of the suction input.
        /// </summary>
        private void SetIOLabels()
        {
            // Resolve the output labels configured in the vacuum settings.
            VacuumOnLabel.Value = _ioAPI.IOLabels.FirstOrDefault(x => x.Label == _proFuseData.VacuumData.IOLabelVacuumOn) as IRCXOutputIOLabel<bool>;

            VacuumBreakOnLabel.Value = _ioAPI.IOLabels.FirstOrDefault(x => x.Label == _proFuseData.VacuumData.IOLabelVacuumBreakOn) as IRCXOutputIOLabel<bool>;

            // The suction input can only be watched while the controller is online.
            IRCXIOWatcher? watcher = null;
            if (
                _controllerConnectionAPI.IsOnline == true
                && _ioAPI.IOLabels.FirstOrDefault(x => x.Label == _proFuseData.VacuumData.IOLabelSuctionState) is IRCXInputIOLabel<bool> suctionStateLabel
            )
            {
                RCXResult result;
                (result, watcher) = suctionStateLabel.CreateWatcher(SuctionStateChanged);
                if (result != RCXResult.Success)
                {
                    ErrorReporter.Message(Caption.CannotCreateWatcher);
                    watcher = null;
                }
            }

            // A null value indicates that the suction state cannot be monitored.
            SuctionStateLabel.Value = watcher;
        }
    }
}

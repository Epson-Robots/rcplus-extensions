// -----------------------------------------------------------------------
// <copyright file="VacuumSettingsWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXIOAPI;

namespace VanguardModelPFScrewdriver.Dialogs.VacuumSettings
{
    /// <summary>
    /// View model for the vacuum settings dialog.
    /// Lets the user assign controller I/O labels to the vacuum ON, vacuum break ON
    /// and suction state signals.
    /// </summary>
    internal class VacuumSettingsWindowViewModel : DialogViewModel
    {
        /// <summary>
        /// Gets the selectable output I/O labels of type "Bit" (including an empty entry for "not assigned").
        /// </summary>
        public ReactiveCollection<string> BitOutputLabels { get; } = [];

        /// <summary>
        /// Gets or sets the output label used to turn the vacuum on.
        /// </summary>
        public ReactivePropertySlim<string> SelectedVacuumOnLabel { get; } = new(string.Empty);

        /// <summary>
        /// Gets or sets the output label used to turn the vacuum break on.
        /// </summary>
        public ReactivePropertySlim<string> SelectedVacuumBreakOnLabel { get; } = new(string.Empty);

        /// <summary>
        /// Gets the selectable input I/O labels of type "Bit" (including an empty entry for "not assigned").
        /// </summary>
        public ReactiveCollection<string> BitInputLabels { get; } = [];

        /// <summary>
        /// Gets or sets the input label used to monitor the suction state.
        /// </summary>
        public ReactivePropertySlim<string> SelectedSuctionStateLabel { get; } = new(string.Empty);

        /// <summary>
        /// Controller I/O API used to enumerate the available I/O labels.
        /// </summary>
        private readonly IRCXIOAPI _ioAPI;

        /// <summary>
        /// Applies the selected I/O labels to the ModelPF data when the dialog is confirmed,
        /// and marks the data as dirty if anything changed.
        /// </summary>
        protected override Task OnOK()
        {
            if (_proFuseData != null)
            {
                _proFuseData.VacuumData.IOLabelVacuumOn = SelectedVacuumOnLabel.Value;
                _proFuseData.VacuumData.IOLabelVacuumBreakOn = SelectedVacuumBreakOnLabel.Value;
                _proFuseData.VacuumData.IOLabelSuctionState = SelectedSuctionStateLabel.Value;

                _proFuseData.SetDirty();
            }

            return base.OnOK();
        }

        /// <summary>
        /// Updates the OK button availability.
        /// The same output label must not be assigned to both vacuum ON and vacuum break ON,
        /// unless no label is assigned at all.
        /// </summary>
        private void CheckCanOK()
        {
            CanOK.Value = (
                !string.Equals(SelectedVacuumOnLabel.Value, SelectedVacuumBreakOnLabel.Value, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty(SelectedVacuumOnLabel.Value)
            );
        }

        /// <summary>
        /// Builds the selectable label lists from the controller I/O definitions and
        /// loads the currently configured labels into the bindable properties.
        /// </summary>
        protected override void Setup()
        {
            // Empty entry represents "no label assigned".
            BitOutputLabels.Add(string.Empty);
            BitInputLabels.Add(string.Empty);

            // Only bit-type labels can be used for vacuum control/monitoring.
            foreach (var label in _ioAPI.IOLabels)
            {
                if (label.GetDataTypeName() == "Bit")
                {
                    switch (label.GetKind())
                    {
                        case RCXIOKind.Output:
                            BitOutputLabels.Add(label.Label);
                            break;

                        case RCXIOKind.Input:
                            BitInputLabels.Add(label.Label);
                            break;
                    }
                }
            }

            if (_proFuseData != null)
            {
                // Restore the current settings as the initial selection.
                SelectedVacuumOnLabel.Value = BitOutputLabels.FirstOrDefault(
                    x => string.Equals(x, _proFuseData.VacuumData.IOLabelVacuumOn, StringComparison.OrdinalIgnoreCase)
                ) ?? string.Empty;
                SelectedVacuumBreakOnLabel.Value = BitOutputLabels.FirstOrDefault(
                    x => string.Equals(x, _proFuseData.VacuumData.IOLabelVacuumBreakOn, StringComparison.OrdinalIgnoreCase)
                ) ?? string.Empty;
                SelectedSuctionStateLabel.Value = BitInputLabels.FirstOrDefault(
                    x => string.Equals(x, _proFuseData.VacuumData.IOLabelSuctionState, StringComparison.OrdinalIgnoreCase)
                ) ?? string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VacuumSettingsWindowViewModel"/> class,
        /// acquiring the I/O API and subscribing to selection changes for validation.
        /// </summary>
        public VacuumSettingsWindowViewModel()
        {
            _ioAPI = Main.GetAPI<IRCXIOAPI>();

            // Re-validate whenever an output label selection changes.
            SelectedVacuumOnLabel.Subscribe((_) => CheckCanOK()).AddTo(_disposables);
            SelectedVacuumBreakOnLabel.Subscribe((_) => CheckCanOK()).AddTo(_disposables);
        }
    }
}

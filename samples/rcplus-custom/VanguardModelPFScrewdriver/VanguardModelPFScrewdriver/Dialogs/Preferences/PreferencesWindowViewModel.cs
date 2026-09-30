// -----------------------------------------------------------------------
// <copyright file="PreferencesWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;

namespace VanguardModelPFScrewdriver.Dialogs.Preferences
{
    /// <summary>
    /// View model for the Preferences dialog.
    /// Provides bindable properties for torque unit, log storage destination,
    /// log folder and the maximum number of log files kept for pass/fail results.
    /// </summary>
    internal class PreferencesWindowViewModel : DialogViewModel
    {
        /// <summary>
        /// Gets a value indicating whether the torque unit is mN·m (true) or kgf·cm (false).
        /// </summary>
        public ReactivePropertySlim<bool> UseTorqueUnitMNM { get; } = new(true);

        /// <summary>
        /// Gets the list of selectable storage destinations for log files.
        /// The order must match the index order used by <see cref="MaxNumFilesOnPass"/> and <see cref="MaxNumFilesOnFail"/>.
        /// </summary>
        public ReactiveCollection<StorageKind> StorageKinds { get; } =
        [
            StorageKind.PC,
            StorageKind.ControllerUSB,
            StorageKind.ControllerFlash,
        ];

        /// <summary>
        /// Gets the storage destination currently selected by the user.
        /// </summary>
        public ReactivePropertySlim<StorageKind> SelectedStorageKind { get; } = new(StorageKind.PC);

        /// <summary>
        /// Gets the folder path on the PC where log files are stored.
        /// </summary>
        public ReactiveProperty<string> PCFolder { get; } = new(string.Empty);

        /// <summary>
        /// Gets the command that opens a folder picker to choose <see cref="PCFolder"/>.
        /// </summary>
        public ReactiveCommand ChoosePCFolderCommand { get; } = new();

        /// <summary>
        /// Gets the maximum number of "pass" log files per storage kind.
        /// The array is indexed by <see cref="StorageKind"/>.
        /// </summary>
        public ReactiveProperty<string>[] MaxNumFilesOnPass { get; } = [new("100"), new("100"), new("10")];

        /// <summary>
        /// Gets the maximum number of "fail" log files per storage kind.
        /// The array is indexed by <see cref="StorageKind"/>.
        /// </summary>
        public ReactiveProperty<string>[] MaxNumFilesOnFail { get; } = [new("100"), new("100"), new("10")];

        /// <summary>
        /// Gets the "pass" file-count property bound to the UI for the selected storage kind.
        /// </summary>
        public ReactivePropertySlim<ReactiveProperty<string>> OnPass { get; } = new();

        /// <summary>
        /// Gets the "fail" file-count property bound to the UI for the selected storage kind.
        /// </summary>
        public ReactivePropertySlim<ReactiveProperty<string>> OnFail { get; } = new();

        /// <summary>
        /// Upper bounds (exclusive) for the number of log files, indexed by <see cref="StorageKind"/>.
        /// </summary>
        private static readonly int[] _limits = [10000, 10000, 100];

        /// <summary>
        /// Indicates whether the current "pass" file-count input is valid.
        /// </summary>
        private bool _passIsValid = false;

        /// <summary>
        /// Indicates whether the current "fail" file-count input is valid.
        /// </summary>
        private bool _failIsValid = false;

        /// <summary>
        /// Indicates whether the current PC folder path is valid.
        /// </summary>
        private bool _pcFolderIsValid = false;

        /// <summary>
        /// Switches the bound file-count properties when the selected storage kind changes.
        /// </summary>
        /// <param name="kind">The newly selected storage kind.</param>
        private void OnSelectedStorageKindChanged(
            StorageKind kind
        )
        {
            OnPass.Value = MaxNumFilesOnPass[(int)kind];
            OnFail.Value = MaxNumFilesOnFail[(int)kind];
        }

        /// <summary>
        /// Determines whether the specified folder path can be handled by the controller.
        /// An empty path is treated as valid.
        /// </summary>
        /// <param name="path">The folder path to validate.</param>
        /// <returns><c>true</c> if the path is ASCII only and not longer than <see cref="Constants.MaxPathLength"/>; otherwise, <c>false</c>.</returns>
        private static bool PCFolderIsValid(
            string path
        )
        {
            if (!string.IsNullOrEmpty(path))
            {
                // Only ASCII characters are supported by the controller.
                if (!path.All(c => c <= 0x7f))
                {
                    return false;
                }

                // The controller cannot handle paths longer than the limit.
                if (path.Length > MaxPathLength)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Shows a folder picker and validates the selected path.
        /// The path must contain ASCII characters only and must not exceed <see cref="Constants.MaxPathLength"/>;
        /// otherwise an error message is shown and the picker is displayed again.
        /// </summary>
        private void OnChoosePCFolder()
        {
            OpenFolderDialog dialog = new();

            while (true)
            {
                // The user cancelled the folder picker.
                if (dialog.ShowDialog() != true)
                {
                    break;
                }

                // Non-ASCII characters and too long paths are not supported by the controller.
                if (PCFolderIsValid(dialog.FolderName))
                {
                    PCFolder.Value = dialog.FolderName;
                    break;
                }

                // Notify the user and let them pick another folder.
                ErrorReporter.Message(Caption.BadPathError, _window);
            }
        }

        /// <summary>
        /// Writes the edited values back to the ModelPF preference data and marks it as dirty.
        /// </summary>
        /// <returns>A task that completes when the base OK handling has finished.</returns>
        protected override Task OnOK()
        {
            if (_proFuseData != null)
            {
                _proFuseData.PreferenceData.TorqueUnit = UseTorqueUnitMNM.Value ? TorqueUnitKind.MNM : TorqueUnitKind.KGFCM;

                _proFuseData.PreferenceData.LogStorage = SelectedStorageKind.Value;
                _proFuseData.PreferenceData.LogFolder = PCFolder.Value;

                // Copy the per-storage file limits back to the model.
                for (int i = 0; i < MaxNumFilesOnPass.Length; i++)
                {
                    _proFuseData.PreferenceData.MaxNumPass[i] = int.Parse(MaxNumFilesOnPass[i].Value);
                }
                for (int i = 0; i < MaxNumFilesOnFail.Length; i++)
                {
                    _proFuseData.PreferenceData.MaxNumFail[i] = int.Parse(MaxNumFilesOnFail[i].Value);
                }

                // Mark the project data as modified so that it will be saved.
                _proFuseData.SetDirty();
            }

            return base.OnOK();
        }

        /// <summary>
        /// Initializes the bindable properties from the current ModelPF preference data
        /// when the dialog is opened.
        /// </summary>
        protected override void Setup()
        {
            if (_proFuseData != null)
            {
                UseTorqueUnitMNM.Value = (_proFuseData.PreferenceData.TorqueUnit == TorqueUnitKind.MNM);

                SelectedStorageKind.Value = _proFuseData.PreferenceData.LogStorage;
                PCFolder.Value = _proFuseData.PreferenceData.LogFolder;

                // Load the per-storage file limits from the model.
                for (int i = 0; i < MaxNumFilesOnPass.Length; i++)
                {
                    MaxNumFilesOnPass[i].Value = _proFuseData.PreferenceData.MaxNumPass[i].ToString();
                }
                for (int i = 0; i < MaxNumFilesOnFail.Length; i++)
                {
                    MaxNumFilesOnFail[i].Value = _proFuseData.PreferenceData.MaxNumFail[i].ToString();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreferencesWindowViewModel"/> class
        /// and wires up the reactive subscriptions.
        /// </summary>
        public PreferencesWindowViewModel()
        {
            // Keep the bound file-count properties in sync with the selected storage kind.
            SelectedStorageKind.Subscribe(OnSelectedStorageKindChanged).AddTo(_disposables);
            ChoosePCFolderCommand.Subscribe(OnChoosePCFolder).AddTo(_disposables);

            PCFolder.SetValidateNotifyError((path) =>
            {
                string? error = null;

                if (SelectedStorageKind.Value == StorageKind.PC && !PCFolderIsValid(path))
                {
                    error = "InvalidPCFolder";
                }

                _pcFolderIsValid = (error == null);
                CanOK.Value = (_passIsValid && _failIsValid && _pcFolderIsValid);

                return error;
            })
            .AddTo(_disposables);

            for (int i = 0; i < MaxNumFilesOnPass.Length; i++)
            {
                var limit = _limits[i];
                MaxNumFilesOnPass[i].SetValidateNotifyError((stringValue) =>
                {
                    _passIsValid = (int.TryParse(stringValue, out var value) && 0 <= value && value < limit);
                    CanOK.Value = (_passIsValid && _failIsValid && _pcFolderIsValid);
                    return _passIsValid ? null : "OutOfRange";
                })
                .AddTo(_disposables);
            }
            for (int i = 0; i < MaxNumFilesOnFail.Length; i++)
            {
                var limit = _limits[i];
                MaxNumFilesOnFail[i].SetValidateNotifyError((stringValue) =>
                {
                    _failIsValid = (int.TryParse(stringValue, out var value) && 0 <= value && value < limit);
                    CanOK.Value = (_passIsValid && _failIsValid && _pcFolderIsValid);
                    return _failIsValid ? null : "OutOfRange";
                })
                .AddTo(_disposables);
            }
        }
    }
}

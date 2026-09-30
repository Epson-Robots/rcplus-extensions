// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelMisc.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Win32;
using Reactive.Bindings;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using VanguardModelPFScrewdriver.Dialogs;
using VanguardModelPFScrewdriver.Dialogs.Preferences;
using VanguardModelPFScrewdriver.Dialogs.SystemInformation;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.Utils;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static Epson.RoboticsShared.ExtensionsAPI.V3.IRCXControllerAPI;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that handles preferences,
    /// log storage locations and the log file watchers.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Gets the torque unit used for display.
        /// </summary>
        public ReactivePropertySlim<TorqueUnitKind> TorqueUnit { get; } = new(TorqueUnitKind.MNM);

        /// <summary>
        /// Gets the number of decimal places used to display torque values.
        /// </summary>
        public ReactivePropertySlim<int> TorqueDecimalPlaces { get; } = new(1);

        // ======================================================================

        /// <summary>
        /// Gets the visibility of the panel used to fetch logs from the controller.
        /// </summary>
        public ReactivePropertySlim<Visibility> FetchLogPanelVisibility { get; } = new(Visibility.Collapsed);

        /// <summary>
        /// Gets the visibility of the USB drive selection button.
        /// </summary>
        public ReactivePropertySlim<Visibility> SelectUSBVisibility { get; } = new(Visibility.Hidden);

        /// <summary>
        /// Gets the USB drive selected as the log source.
        /// </summary>
        public ReactivePropertySlim<string?> SelectedUSBDrive { get; } = new(string.Empty, ReactivePropertyMode.DistinctUntilChanged);

        // ======================================================================

        /// <summary>
        /// "Preferences" command.
        /// </summary>
        public ReactiveCommand PreferencesCommand { get; }

        /// <summary>
        /// "System information" command.
        /// </summary>
        public ReactiveCommand SystemInformationCommand { get; }

        // ======================================================================

        /// <summary>
        /// Command that fetches the log files from the robot controller.
        /// </summary>
        public AsyncReactiveCommand FetchFromControllerCommand { get; } = new();

        /// <summary>
        /// Command that selects the USB drive holding the log files.
        /// </summary>
        public ReactiveCommand SelectUSBDriveCommand { get; } = new();

        // ======================================================================

        // Watches the log files for newly appended entries.
        private FileSystemWatcher? _logFileChangedWatcher;

        // Watches the log files for deletion.
        private FileSystemWatcher? _logFileDeletedWatcher;

        /// <summary>
        /// Applies the current preference values to the view state.
        /// </summary>
        private void SetPreferences()
        {
            TorqueUnit.Value = _proFuseData.PreferenceData.TorqueUnit;

            // Fetching is only meaningful when the logs are stored on the controller.
            FetchLogPanelVisibility.Value = _proFuseData.PreferenceData.LogStorage switch
            {
                StorageKind.PC => Visibility.Collapsed,
                _ => Visibility.Visible,
            };
            SelectUSBVisibility.Value = _proFuseData.PreferenceData.LogStorage switch
            {
                StorageKind.ControllerUSB => Visibility.Visible,
                _ => Visibility.Hidden,
            };
        }

        /// <summary>
        /// Opens the "Preferences" dialog and reloads the logs if the log settings have changed.
        /// </summary>
        private void OnPreferences()
        {
            var prevPreferenceData = _proFuseData.PreferenceData.Clone();

            _ = DialogViewModel.ShowDialog<PreferencesWindow>(_proFuseData, _proFuseClient);

            SetPreferences();

            if (!_proFuseData.PreferenceData.LoggingMatterEquals(prevPreferenceData))
            {
                SetupLogsAndGraph();
            }
        }

        /// <summary>
        /// Opens the "System information" dialog.
        /// </summary>
        private void OnSystemInformation()
        {
            _ = DialogViewModel.ShowDialog<SystemInformationWindow>(_proFuseData, _proFuseClient);
        }

        // ======================================================================

        /// <summary>
        /// Gets the local folder that mirrors the log files stored on the controller.
        /// The folder is created if it does not exist yet.
        /// </summary>
        /// <returns>The shadow log folder, or null if it could not be determined.</returns>
        private string? GetShadowLogFolder()
        {
            // The folder name is derived from the connection name and the controller serial number.
            var lastConnection = _controllerConnectionAPI.GetLastConnection();
            if (lastConnection == null)
            {
                return null;
            }
            var connectionName = lastConnection.Name;

            var (result, settings) = _controllerAPI.GetControllerSettings(null, "General");
            if (result != RCXResult.Success || settings == null)
            {
                return null;
            }
            var serialNumber = settings.GetValue("SerialNumber", typeof(string)) as string;
            if (string.IsNullOrEmpty(serialNumber))
            {
                serialNumber = "NA";
            }

            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EpsonRC80",
                "Extensions",
                Main.CommonId,
                $"{connectionName}_{serialNumber}"
            );
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return folder;
        }

        /// <summary>
        /// Gets the folder that currently holds the log files, depending on the log storage setting.
        /// </summary>
        /// <returns>The log folder, or null if it could not be determined.</returns>
        private string? GetLogFolder()
        {
            if (_projectAPI.ProjectFolder == null)
            {
                return null;
            }

            string? logFolder = null;
            switch (_proFuseData.PreferenceData.LogStorage)
            {
                case StorageKind.PC:
                    logFolder = _proFuseData.PreferenceData.LogFolder;

                    // A relative setting is resolved against the project folder.
                    if (logFolder == null || !Path.IsPathFullyQualified(logFolder))
                    {
                        logFolder = Path.Combine(_projectAPI.ProjectFolder, logFolder ?? string.Empty);
                    }
                    break;

                case StorageKind.ControllerUSB:
                    // Without a selected USB drive, the downloaded copies are used.
                    if (string.IsNullOrEmpty(SelectedUSBDrive.Value))
                    {
                        logFolder = GetShadowLogFolder();
                    }
                    else
                    {
                        logFolder = SelectedUSBDrive.Value;
                    }
                    break;

                case StorageKind.ControllerFlash:
                    logFolder = GetShadowLogFolder();
                    break;

                default:
                    break;
            }

            return logFolder;
        }

        /// <summary>
        /// Builds the log file name for the specified log type.
        /// </summary>
        /// <param name="typeChar">Log type character ('R' for results, 'E' for errors, '?' as wildcard).</param>
        /// <returns>The log file name.</returns>
        private string GetLogFileName(
            char typeChar
        )
        {
            return $"VGD_PF_{SelectedToolType.Value}_{SelectedCTContsId.Value:D3}_{typeChar}.csv";
        }

        /// <summary>
        /// Gets the controller path type that corresponds to the current log storage setting.
        /// </summary>
        /// <returns>The controller path type, or null if the logs are stored on the PC.</returns>
        private ControllerPathType? GetControllerPathType()
        {
            return _proFuseData.PreferenceData.LogStorage switch
            {
                StorageKind.ControllerUSB => ControllerPathType.ExternalUSB,
                StorageKind.ControllerFlash => ControllerPathType.ProjectFolder,
                _ => null,
            };
        }

        /// <summary>
        /// Downloads the log files and the waveform data from the robot controller,
        /// connecting to the controller first if required.
        /// </summary>
        private async Task OnFetchFromControllerAsync()
        {
            // Fetching always targets the downloaded copies, not a USB drive.
            SelectedUSBDrive.Value = null;

            if (_controllerConnectionAPI.IsOnline == false)
            {
                var isSuccess = await _controllerConnectionAPI.ConnectControllerAsync().ConfigureAwait(true);
                if (!isSuccess)
                {
                    // An error message has already been shown during the connection process.
                    return;
                }

                // Wait until the controller actually reports the online state.
                while (_controllerConnectionAPI.IsOnline != true)
                {
                    const int _waitMSec = 100;
                    await Task.Delay(_waitMSec).ConfigureAwait(true);
                }
                //const int _moreWaitMSec = 1000;
                //await Task.Delay(_moreWaitMSec).ConfigureAwait(true);
            }

            // The waveform files are only meaningful together with the result log.
            if (FetchResultLogFromController())
            {
                await FetchWaveformDataAsync();
            }
            _ = FetchErrorLogFromController();

            SetupLogsAndGraph();
        }

        /// <summary>
        /// Lets the user choose a folder and uses its root as the USB log drive.
        /// </summary>
        private void OnSelectUSBDrive()
        {
            OpenFolderDialog dialog = new();

            if (dialog.ShowDialog() == true)
            {
                SelectedUSBDrive.Value = Path.GetPathRoot(dialog.FolderName);
            }
        }

        // ======================================================================

        /// <summary>
        /// Adds tightening results to the list and selects the latest one.
        /// </summary>
        /// <param name="tightningResults">Result entries to add.</param>
        private void AddTighghteningResults(
            IEnumerable<ResultLogEntry> tightningResults
        )
        {
            TighteningResults.AddRangeOnScheduler(tightningResults);

            // Select the newest entry after the collection has been updated on the UI thread.
            Application.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    if (tightningResults.Any())
                    {
                        SelectedTighteningResult.Value = tightningResults.Last();
                    }
                },
                DispatcherPriority.Background
            );
        }

        /// <summary>
        /// Adds error entries to the list and selects the latest one.
        /// </summary>
        /// <param name="errors">Error entries to add.</param>
        private void AddErrors(
            IEnumerable<ErrorLogEntry> errors
        )
        {
            Errors.AddRangeOnScheduler(errors);

            // Select the newest entry after the collection has been updated on the UI thread.
            Application.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    if (errors.Any())
                    {
                        SelectedError.Value = errors.Last();
                    }
                },
                DispatcherPriority.Background
            );
        }

        /// <summary>
        /// Reloads the logs and the graph, and restarts the log file watchers
        /// when the logs are stored on the PC.
        /// </summary>
        private void SetupLogsAndGraph()
        {
            // Stop watching the previous log folder.
            _logFileChangedWatcher?.Dispose();
            _logFileChangedWatcher = null;

            _logFileDeletedWatcher?.Dispose();
            _logFileDeletedWatcher = null;

            TighteningResults.Clear();
            Errors.Clear();
            ClearGraph();

            _ = Task.Run(async () =>
            {
                // Read both log files from the beginning.
                var tightningResults = await ReloadResultLogAsync();
                AddTighghteningResults(tightningResults);

                var errors = await ReloadErrorLogAsync();
                AddErrors(errors);

                // Local log files can be monitored for live updates.
                if (_proFuseData.PreferenceData.LogStorage == StorageKind.PC)
                {
                    var logFolder = GetLogFolder();
                    if (logFolder != null)
                    {
                        // Watch for entries appended to the log files.
                        _logFileChangedWatcher = new(logFolder, GetLogFileName('?'))
                        {
                            NotifyFilter = NotifyFilters.LastWrite,
                        };
                        _logFileChangedWatcher.Changed += async (_, ev) =>
                        {
                            var fileName = Path.GetFileName(ev.Name);
                            if (string.Equals(fileName, GetResultLogFileName(), StringComparison.OrdinalIgnoreCase))
                            {
                                tightningResults = await UpdateResultLogAsync();
                                AddTighghteningResults(tightningResults);
                            }
                            else if (string.Equals(fileName, GetErrorLogFileName(), StringComparison.OrdinalIgnoreCase))
                            {
                                errors = await UpdateErrorLogAsync();
                                AddErrors(errors);
                            }
                        };

                        // Watch for deletion of the log files to clear the views.
                        _logFileDeletedWatcher = new(logFolder, GetLogFileName('?'))
                        {
                            NotifyFilter = NotifyFilters.FileName,
                        };
                        _logFileDeletedWatcher.Deleted += async (_, ev) =>
                        {
                            var fileName = Path.GetFileName(ev.Name);
                            if (string.Equals(fileName, GetResultLogFileName(), StringComparison.OrdinalIgnoreCase))
                            {
                                // Reset the read position, then clear the displayed entries.
                                _ = await ReloadResultLogAsync();
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    TighteningResults.Clear();
                                    ClearGraph();
                                });
                            }
                            else if (string.Equals(fileName, GetErrorLogFileName(), StringComparison.OrdinalIgnoreCase))
                            {
                                _ = await ReloadErrorLogAsync();
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Errors.Clear();
                                });
                            }
                        };
                        _logFileDeletedWatcher.EnableRaisingEvents = true;
                    }
                }
            });
        }
    }
}

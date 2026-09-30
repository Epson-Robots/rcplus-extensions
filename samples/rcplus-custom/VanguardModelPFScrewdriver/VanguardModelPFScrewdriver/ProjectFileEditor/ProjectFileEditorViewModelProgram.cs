// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelProgramMenu.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Microsoft.Win32;
using Reactive.Bindings;
using System.Windows;
using System.Windows.Threading;
using VanguardModelPFScrewdriver.Dialogs;
using VanguardModelPFScrewdriver.Dialogs.EditProgram;
using VanguardModelPFScrewdriver.Dialogs.Progress;
using VanguardModelPFScrewdriver.ModelPF;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Program.TighteningProgram;
using static VanguardModelPFScrewdriver.ProjectFileEditor.ProgramMenuItemViewModel;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that manages the screwdriver programs:
    /// editing, transferring to / from the device and importing / exporting them.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Gets the programs shown in the program list.
        /// </summary>
        public ReactiveCollection<ProgramItemViewModel> ProgramItems { get; } = [];

        /// <summary>
        /// Gets the program currently selected in the program list.
        /// </summary>
        public ReactivePropertySlim<ProgramItemViewModel?> SelectedProgramItem { get; } = new(null);

        /// <summary>
        /// "Edit program" command.
        /// </summary>
        public ReactiveCommand EditProgramCommand { get; }

        /// <summary>
        /// Gets the entries of the program context menu.
        /// </summary>
        public ReactiveCollection<ProgramMenuItemViewModel> ProgramMenuItems { get; } =
        [
            new(Caption.AllPrograms, true),
            new(Caption.DownloadProgram, CommandKind.DownloadAllPrograms),
            new(Caption.UploadProgram, CommandKind.UploadAllPrograms),
            new(Caption.SelectedProgram, true),
            new(Caption.DownloadProgram, CommandKind.DownloadSelectedProgram),
            new(Caption.UploadProgram, CommandKind.UploadSelectedProgram),
            new(),
            new(Caption.ImportProgram, CommandKind.ImportPrograms, true),
            new(Caption.ExportProgram, CommandKind.ExportPrograms, true),
        ];

        // ======================================================================

        /// <summary>
        /// Opens the "Edit program" dialog for the selected program.
        /// </summary>
        private void OnEditProgram()
        {
            if (SelectedProgramItem.Value != null)
            {
                // The dialog reuses the graph settings of the editor.
                Dictionary<string, object?> parameters = new()
                {
                    ["TargetProgram"] = SelectedProgramItem.Value,
                    ["ToolType"] = SelectedToolType.Value,
                    ["Mode"] = ModeKind.Normal,
                    ["TorqueUpperLimit"] = TorqueUpperLimit.Value,
                    ["NumTurnsUpperLimit"] = NumTurnsUpperLimit.Value,
                    ["Graphs"] = _overlayGraphs,
                    ["GraphAreaMap"] = _areaMap,
                };

                var result = DialogViewModel.ShowDialog<EditProgramWindow>(_proFuseData, _proFuseClient, parameters);
                if (result == true)
                {
                    // Refresh the list entry with the edited program data.
                    SelectedProgramItem.Value.Set(_proFuseData.ProgramData[SelectedProgramItem.Value.ProgramNo]);
                }
            }
        }

        // ======================================================================

        /// <summary>
        /// Downloads a single program from the device and stores it only if the transfer succeeded.
        /// </summary>
        /// <param name="programNo">Number of the program to download.</param>
        /// <param name="cancellationToken">Token used to cancel the transfer.</param>
        /// <returns>True if the program has been downloaded successfully.</returns>
        private async Task<bool> SafeDownloadProgramAsync(
            int programNo,
            CancellationToken cancellationToken = default
        )
        {
            if (_proFuseClient != null)
            {
                // Download into a temporary instance to keep the current data on failure.
                Program program = new(programNo);

                var isSuccess = await program.DownloadAsync(_proFuseClient, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                _proFuseData.ProgramData[programNo] = program;
                _proFuseData.SetDirty();
            }

            return true;
        }

        /// <summary>
        /// Asks the user to confirm that the existing data may be overwritten.
        /// </summary>
        /// <param name="captionId">Caption ID of the warning message.</param>
        /// <returns>True if the user confirmed the operation.</returns>
        private bool ConfirmOverwrite(
            int captionId
        )
        {
            var answer = _windowAPI.ShowMessageBox(
                new RCXCaption(Main.CommonId, Caption.ExtensionName),
                new RCXCaption(Main.CommonId, captionId),
                ButtonType.Yes_No,
                IconType.Warning
            );

            return answer == ResponseType.Yes;
        }

        /// <summary>
        /// Shows the "operation completed" message box on the UI thread.
        /// </summary>
        /// <remarks>
        /// The message box is dispatched with <see cref="DispatcherPriority.Background"/> so that
        /// the progress window is closed and the UI is updated before the notification appears.
        /// </remarks>
        private static void ReportCompleted()
        {
            Application.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    _ = Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.OperationCompleted),
                        ButtonType.OK,
                        IconType.Information
                    );
                },
                DispatcherPriority.Background
            );
        }

        /// <summary>
        /// Downloads the selected program from the device while showing a progress window.
        /// </summary>
        private async Task OnDownloadSelectedProgramAsync()
        {
            if (_proFuseData != null && _proFuseClient != null && SelectedProgramItem.Value != null)
            {
                if (!ConfirmOverwrite(Caption.DownloadOverwriteWarning))
                {
                    return;
                }

                // Show the progress window only if the transfer takes longer than this period.
                const int _quietWaitMSec = 2000;

                var isSuccess = false;
                await ProgressWindow.OpenAsync(
                    async (reporter) =>
                    {
                        // A single program has no meaningful progress count.
                        var format = Captions[Caption.DownloadingPrograms];
                        var message = string.Format(format, string.Empty).Replace("()", string.Empty);
                        reporter.SetMessage(message);
                        reporter.SetIsIndeterminate(true);
                        reporter.SetCancellable(true);

                        isSuccess = await SafeDownloadProgramAsync(SelectedProgramItem.Value.ProgramNo, reporter.CancellationToken);
                    },
                    _quietWaitMSec
                );
                if (!isSuccess)
                {
                    ErrorReporter.Message(Caption.ProgramDownloadError);
                }
                else
                {
                    ReportCompleted();
                }
            }
        }

        /// <summary>
        /// Downloads all programs from the device while showing a progress window.
        /// </summary>
        private async Task OnDownloadAllProgramsAsync()
        {
            if (_proFuseData != null && _proFuseClient != null)
            {
                if (!ConfirmOverwrite(Caption.DownloadOverwriteWarning))
                {
                    return;
                }

                var count = _proFuseData.ProgramData.Count;
                var isSuccess = true;
                await ProgressWindow.OpenAsync(
                    async (reporter) =>
                    {
                        reporter.SetMaximum(count);
                        reporter.SetCancellable(true);
                        var format = Captions[Caption.DownloadingPrograms];
                        for (var index = 0; index < count; index++)
                        {
                            var message = string.Format(format, $"{1 + index}/{count}");
                            reporter.SetMessage(message);

                            isSuccess = await SafeDownloadProgramAsync(_proFuseData.ProgramData[index].ProgramNo, reporter.CancellationToken);

                            // Stop on the first failure or when the user cancelled.
                            if (!isSuccess || reporter.CancellationToken.IsCancellationRequested)
                            {
                                break;
                            }
                            reporter.SetValue(1 + index);
                        }
                    }
                );
                if (!isSuccess)
                {
                    ErrorReporter.Message(Caption.ProgramDownloadError);
                }
                else
                {
                    ReportCompleted();
                }
            }
        }

        /// <summary>
        /// Uploads a single program to the device while showing a progress window.
        /// </summary>
        /// <param name="proFuseClient">Client connected to the device.</param>
        /// <param name="program">Program to upload.</param>
        /// <param name="reportCompleted">If true, report completion; if false, remain silent.</param>
        public static async Task UploadProgram(
            ModelPFClient proFuseClient,
            Program program,
            bool reportCompleted = true
        )
        {
            // Show the progress window only if the transfer takes longer than this period.
            const int _quietWaitMSec = 2000;

            var isSuccess = false;
            await ProgressWindow.OpenAsync(
                async (reporter) =>
                {
                    // A single program has no meaningful progress count.
                    var format = Captions[Caption.UploadingPrograms];
                    var message = string.Format(format, string.Empty).Replace("()", string.Empty);
                    reporter.SetMessage(message);
                    reporter.SetIsIndeterminate(true);
                    reporter.SetCancellable(true);

                    isSuccess = await program.UploadAsync(proFuseClient, reporter.CancellationToken);
                },
                _quietWaitMSec
            );
            if (!isSuccess)
            {
                ErrorReporter.Message(Caption.ProgramUploadError);
            }
            else if (reportCompleted)
            {
                ReportCompleted();
            }
        }

        /// <summary>
        /// Uploads the selected program to the device.
        /// </summary>
        private async Task OnUploadSelectedProgramAsync()
        {
            if (_proFuseData != null && _proFuseClient != null && SelectedProgramItem.Value != null)
            {
                await UploadProgram(_proFuseClient, _proFuseData.ProgramData[SelectedProgramItem.Value.ProgramNo]);
            }
        }

        /// <summary>
        /// Uploads all programs to the device while showing a progress window.
        /// </summary>
        private async Task OnUploadAllProgramsAsync()
        {
            if (_proFuseData != null && _proFuseClient != null)
            {
                var count = _proFuseData.ProgramData.Count;
                var isSuccess = true;
                await ProgressWindow.OpenAsync(
                    async (reporter) =>
                    {
                        reporter.SetMaximum(count);
                        reporter.SetCancellable(true);
                        var format = Captions[Caption.UploadingPrograms];
                        for (var index = 0; index < count; index++)
                        {
                            var message = string.Format(format, $"{1 + index}/{count}");
                            reporter.SetMessage(message);

                            isSuccess = await _proFuseData.ProgramData[index].UploadAsync(_proFuseClient, reporter.CancellationToken);

                            // Stop on the first failure or when the user cancelled.
                            if (!isSuccess || reporter.CancellationToken.IsCancellationRequested)
                            {
                                break;
                            }
                            reporter.SetValue(1 + index);
                        }
                    }
                );
                if (!isSuccess)
                {
                    ErrorReporter.Message(Caption.ProgramUploadError);
                }
                else
                {
                    ReportCompleted();
                }
            }
        }

        /// <summary>
        /// Imports the programs from a PRO-E Expert program file, replacing the current ones.
        /// </summary>
        private Task OnImportProgramsAsync()
        {
            if (!ConfirmOverwrite(Caption.ImportOverwriteWarning))
            {
                return Task.CompletedTask;
            }

            OpenFileDialog dialog = new()
            {
                Filter = Captions[Caption.PEProgramFilter],
            };

            if (dialog.ShowDialog() == true)
            {
                var proEExertProgram = ProEExpertProgram.Load(dialog.FileName);
                if (proEExertProgram != null)
                {
                    // Convert the loaded data into the internal program data.
                    ProgramDataBridge.Import(_proFuseData, proEExertProgram);
                    _proFuseData.SetDirty();

                    SetProgramItems();

                    ReportCompleted();
                }
                else
                {
                    ErrorReporter.Message(Caption.ImportProgramFailed);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Exports the current programs into a PRO-E Expert program file.
        /// </summary>
        private Task OnExportProgramsAsync()
        {
            SaveFileDialog dialog = new()
            {
                Filter = Captions[Caption.PEProgramFilter],
            };

            if (dialog.ShowDialog() == true)
            {
                // Convert the internal program data into the file format.
                ProEExpertProgram proEExpertProgram = new();
                ProgramDataBridge.Export(_proFuseData, proEExpertProgram);
                if (!proEExpertProgram.Save(dialog.FileName))
                {
                    ErrorReporter.Message(Caption.ExportProgramFailed);
                }
                else
                {
                    ReportCompleted();
                }
            }

            return Task.CompletedTask;
        }

        // ======================================================================

        /// <summary>
        /// Rebuilds the program list from the current program data and selects the first entry.
        /// </summary>
        private void SetProgramItems()
        {
            ProgramItems.Clear();
            foreach (var program in _proFuseData.ProgramData)
            {
                ProgramItemViewModel programItem = new();
                programItem.Set(program);

                ProgramItems.Add(programItem);
            }
            if (ProgramItems.Count > 0)
            {
                SelectedProgramItem.Value = ProgramItems[0];
            }
        }
    }
}

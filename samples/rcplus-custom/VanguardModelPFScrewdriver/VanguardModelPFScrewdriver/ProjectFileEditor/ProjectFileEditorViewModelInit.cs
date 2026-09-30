// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelInit.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.Utils;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXProgramExecutionAPI;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static VanguardModelPFScrewdriver.Constants;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.Preference;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;
using static VanguardModelPFScrewdriver.ProjectFileEditor.ProgramMenuItemViewModel;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that holds the common state
    /// and performs the initialization of commands and subscriptions.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Gets a value indicating whether the project file can be updated.
        /// </summary>
        /// <remarks>
        /// While any task is running the file may be in use, therefore updating is not allowed.
        /// </remarks>
        public ReactivePropertySlim<bool> CanSaveFile { get; } = new(false);

        /// <summary>
        /// Gets a value indicating whether a connection to the CT-CONTS is established.
        /// </summary>
        public ReactivePropertySlim<bool> HasCTContsConnection { get; } = new(false);

        /// <summary>
        /// Gets a value indicating whether the robot controller is online.
        /// </summary>
        public ReactivePropertySlim<bool> HasRobotControllerConnection { get; } = new(false, ReactivePropertyMode.DistinctUntilChanged);

        // ======================================================================

        /// <summary>
        /// Gets the selectable CT-CONTS IDs.
        /// </summary>
        public ReactiveCollection<int> CTContsIds { get; } = [.. Enumerable.Range(0, 251)];

        /// <summary>
        /// Gets the CT-CONTS ID currently selected.
        /// </summary>
        public ReactivePropertySlim<int> SelectedCTContsId { get; } = new(0);

        /// <summary>
        /// Gets a value indicating whether the CT-CONTS ID can be changed.
        /// </summary>
        public ReactivePropertySlim<bool> CanChangeCTContsId { get; } = new(true);

        /// <summary>
        /// Gets the selectable tool types.
        /// </summary>
        public ReactiveCollection<ToolKind> ToolTypes { get; } =
        [
            ToolKind.S,
            ToolKind.L,
            ToolKind.XL,
            ToolKind.GM,
            ToolKind.GX,
        ];

        /// <summary>
        /// Gets the tool type currently selected.
        /// </summary>
        public ReactivePropertySlim<ToolKind?> SelectedToolType { get; } = new(null);

        /// <summary>
        /// Gets a value indicating whether the tool type can be changed.
        /// </summary>
        public ReactivePropertySlim<bool> CanChangeToolType { get; } = new(true);

        // ======================================================================

        /// <summary>
        /// PRO-FUSE client.
        /// </summary>
        private ModelPFClient? _proFuseClient;

        /// <summary>
        /// PRO-FUSE data.
        /// </summary>
        private readonly ModelPFData _proFuseData = new();

        // ======================================================================

        /// <summary>
        /// Project API object
        /// </summary>
        private readonly IRCXProjectAPI _projectAPI;

        /// <summary>
        /// Window API object
        /// </summary>
        private readonly IRCXWindowAPI _windowAPI;

        /// <summary>
        /// I/O API object
        /// </summary>
        private readonly IRCXIOAPI _ioAPI;

        /// <summary>
        /// Controller connection API object
        /// </summary>
        private readonly IRCXControllerConnectionAPI _controllerConnectionAPI;

        /// <summary>
        /// Controller API object
        /// </summary>
        private readonly IRCXControllerAPI _controllerAPI;

        // ======================================================================

        /// <summary>
        /// Updates the menu items and the log file watcher when the file can / cannot be saved.
        /// </summary>
        /// <param name="canSave">True if the project file can be updated.</param>
        private void CanSaveFileChanged(
            bool canSave
        )
        {
            foreach (var menuItem in ProgramMenuItems)
            {
                switch (menuItem.CommandKindValue)
                {
                    // Transfer commands additionally require a CT-CONTS connection.
                    case CommandKind.DownloadAllPrograms:
                    case CommandKind.UploadAllPrograms:
                    case CommandKind.DownloadSelectedProgram:
                    case CommandKind.UploadSelectedProgram:
                        menuItem.IsEnabled.Value = canSave && HasCTContsConnection.Value;
                        break;

                    case CommandKind.ImportPrograms:
                        menuItem.IsEnabled.Value = canSave;
                        break;
                }
            }

            // Watch the log files only while no task is using them.
            if (_logFileChangedWatcher != null)
            {
                _logFileChangedWatcher.EnableRaisingEvents = !canSave;
            }
        }

        /// <summary>
        /// Updates the menu items and the device settings when the CT-CONTS connection state has changed.
        /// </summary>
        /// <param name="hasConnection">True if the CT-CONTS is connected.</param>
        private void HasCTContsConnectionChanged(
            bool hasConnection
        )
        {
            foreach (var menuItem in ProgramMenuItems)
            {
                switch (menuItem.CommandKindValue)
                {
                    case CommandKind.DownloadAllPrograms:
                    case CommandKind.DownloadSelectedProgram:
                    case CommandKind.UploadAllPrograms:
                    case CommandKind.UploadSelectedProgram:
                        menuItem.IsEnabled.Value = hasConnection && CanSaveFile.Value;
                        break;
                }
            }

            if (hasConnection)
            {
                // While connected, the values reported by the device are used and cannot be edited.
                SelectedCTContsId.Value = _proFuseData.SysInfoData.CTContsId;
                CanChangeCTContsId.Value = false;
                SelectedToolType.Value = _proFuseData.SysInfoData.ToolType;
                CanChangeToolType.Value = false;
            }
            else
            {
                CanChangeCTContsId.Value = true;
                CanChangeToolType.Value = true;
            }
        }

        /// <summary>
        /// Refreshes the I/O labels when the robot controller connection state has changed.
        /// </summary>
        /// <param name="hasConnection">True if the robot controller is online.</param>
        private void HasRobotControllerConnectionChanged(
            bool hasConnection
        )
        {
            SetIOLabels();
        }

        // ======================================================================

        /// <summary>
        /// Stores the newly selected CT-CONTS ID and reloads the logs and the graph.
        /// </summary>
        /// <param name="newCTContsId">Newly selected CT-CONTS ID.</param>
        private void SelectedCTContsIdChanged(
            int newCTContsId
        )
        {
            if (_proFuseData != null && _proFuseData.OtherData.LastCTContsId != newCTContsId)
            {
                _proFuseData.OtherData.LastCTContsId = newCTContsId;
                _proFuseData.SetDirty();

                SetupLogsAndGraph();
            }
        }

        /// <summary>
        /// Adjusts the program data to the newly selected tool type after user confirmation.
        /// </summary>
        /// <param name="newToolKind">Newly selected tool type.</param>
        private void SelectedToolTypeChanged(
            ToolKind? newToolKind
        )
        {
            if (newToolKind.HasValue && _proFuseData.OtherData.LastToolType != newToolKind.Value)
            {
                // Changing the tool type modifies the existing programs, so ask the user first.
                var response = _windowAPI.ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, Caption.ChangeProgramWarning),
                    ButtonType.Yes_No,
                    IconType.Warning
                );

                if (response == ResponseType.Yes)
                {
                    _proFuseData.Adjust(newToolKind.Value);
                    _proFuseData.SetDirty();
                }
                else
                {
                    // Restore the previous selection asynchronously to avoid a recursive change.
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SelectedToolType.Value = _proFuseData.OtherData.LastToolType;
                    },
                        DispatcherPriority.Background
                    );
                }
            }
        }

        // ======================================================================

        /// <summary>
        /// Applies the newly selected torque unit to the graph and to the value formatting.
        /// </summary>
        /// <param name="torqueUnit">Newly selected torque unit.</param>
        private void TorqueUnitChanged(
            TorqueUnitKind torqueUnit
        )
        {
            // Update the graph labels and redraw all waveforms in the new unit.
            Plot.UpdateTerms(torqueUnit);
            _areaMap.TorqueUnit = torqueUnit;
            RescaleGraphs(torqueUnit);
            SetScale();
            Plot.Refresh();

            // The number of decimal places depends on the unit.
            TorqueDecimalPlaces.Value = TorqueValueConverter.DecimalPlaces(torqueUnit);
            FloatToPersistentStringConverter.SpecialFormat["_T"] = $"f{TorqueDecimalPlaces.Value}";
            TorqueUpperLimit.TorqueUnit = torqueUnit;

            HintData.Entry.UnitInfo["Torque"] = torqueUnit;
        }

        // ======================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectFileEditorViewModel()
        {
            // ----------------------------------------------------------------------
            // File related part (ProjectFileEditorViewModelFile.cs)
            // ----------------------------------------------------------------------

            // ----------------------------------------------------------------------
            // Preferences / System Information related part (ProjectFileEditorViewModelMisc.cs)
            // ----------------------------------------------------------------------
            PreferencesCommand = CanSaveFile
                .ToReactiveCommand()
                .WithSubscribe(OnPreferences)
                .AddTo(_disposables);

            SystemInformationCommand = HasCTContsConnection
                .ToReactiveCommand()
                .WithSubscribe(OnSystemInformation)
                .AddTo(_disposables);

            TorqueUnit
                .Subscribe(TorqueUnitChanged)
                .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Connection related part (ProjectFileEditorViewModelConnect.cs)
            // ----------------------------------------------------------------------

            // The caption and the command of the connection button depend on the connection state.
            FromPCCaption = HasCTContsConnection
                .Select(x => x ? Captions["DisconnectFromPC"] : Captions["ConnectFromPC"])
                .ToReadOnlyReactivePropertySlim<IRCXLangRxCaption>()
                .AddTo(_disposables);
            ConnectFromPCCommand = CanSaveFile
                .ToReactiveCommand()
                .WithSubscribe(OnConnectFromPC)
                .AddTo(_disposables);
            DisconnectFromPCCommand
                .Subscribe(OnDisconnectFromPC)
                .AddTo(_disposables);
            FromPCCommand = HasCTContsConnection
                .Select(x => x ? DisconnectFromPCCommand : ConnectFromPCCommand)
                .ToReadOnlyReactivePropertySlim<ReactiveCommand>()
                .AddTo(_disposables);

            ControllerSettingsCommand = CanSaveFile
                .CombineLatest(
                    HasCTContsConnection,
                    (canSave, hasConection) => canSave && hasConection
                )
                .ToAsyncReactiveCommand()
                .WithSubscribe(OnControllerSettingsAsync)
                .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Program related part (ProjectFileEditorViewModelProgram.cs)
            // ----------------------------------------------------------------------

            // Dispatch every program menu command to its handler.
            ProgramMenuItemViewModel.Callback = async (menuItem) =>
            {
                switch (menuItem.CommandKindValue)
                {
                    case CommandKind.DownloadAllPrograms:
                        await OnDownloadAllProgramsAsync();
                        break;

                    case CommandKind.UploadAllPrograms:
                        await OnUploadAllProgramsAsync();
                        break;

                    case CommandKind.DownloadSelectedProgram:
                        await OnDownloadSelectedProgramAsync();
                        break;

                    case CommandKind.UploadSelectedProgram:
                        await OnUploadSelectedProgramAsync();
                        break;

                    case CommandKind.ImportPrograms:
                        await OnImportProgramsAsync();
                        break;

                    case CommandKind.ExportPrograms:
                        await OnExportProgramsAsync();
                        break;
                }
            };

            EditProgramCommand = CanSaveFile
                .ToReactiveCommand()
                .WithSubscribe(OnEditProgram)
                .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Operation related part (ProjectFileEditorViewModelOperation.cs)
            // ----------------------------------------------------------------------

            // The screwdriver is operating while any of the three operations is running.
            Operating = Observable.CombineLatest(
                TightenGoing,
                LoosenGoing,
                FreeRunGoing,
                (inTightening, inLoosenig, inFreeRun) => (inTightening || inLoosenig || inFreeRun)
            )
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposables);

            Operating
                .Subscribe(async (operating) => await OperatingChangedAsync(operating))
                .AddTo(_disposables);

            // Poll the operating status of the screwdriver periodically.
            _operatingStatusWatcher.Interval = _intervalForWatchingOperatingStatus;
            _operatingStatusWatcher.Tick += async (sender, ev) =>
            {
                await OperatingStatusWatcherOnTickAsync(sender, ev);
            };

            // Each operation button toggles between start and stop.
            TighteningCaption = TightenGoing
                .Select(x => x ? Captions["StopOperation"] : Captions["StartOperation"])
                .ToReadOnlyReactivePropertySlim<IRCXLangRxCaption>()
                .AddTo(_disposables);
            StartTighteningCommand = HasCTContsConnection
                .CombineLatest(
                    Operating,
                    CanSaveFile,
                    (hasConnection, operating, canSave) => (hasConnection && !operating && canSave)
                )
                .ToAsyncReactiveCommand()
                .WithSubscribe(OnStartTighteningAsync)
                .AddTo(_disposables);
            LooseningCaption = LoosenGoing
                .Select(x => x ? Captions["StopOperation"] : Captions["StartOperation"])
                .ToReadOnlyReactivePropertySlim<IRCXLangRxCaption>()
                .AddTo(_disposables);
            StartLooseningCommand = HasCTContsConnection
                .CombineLatest(
                    Operating,
                    CanSaveFile,
                    (hasConnection, operating, canSave) => (hasConnection && !operating && canSave)
                )
                .ToAsyncReactiveCommand()
                .WithSubscribe(OnStartLooseningAsync)
                .AddTo(_disposables);
            FreeRunCaption = FreeRunGoing
                .Select(x => x ? Captions["StopOperation"] : Captions["StartOperation"])
                .ToReadOnlyReactivePropertySlim<IRCXLangRxCaption>()
                .AddTo(_disposables);
            StartFreeRunCommand = HasCTContsConnection
                .CombineLatest(
                    Operating,
                    CanSaveFile,
                    (hasConnection, operating, canSave) => (hasConnection && !operating && canSave)
                )
                .ToAsyncReactiveCommand()
                .WithSubscribe(OnStartFreeRunAsync)
                .AddTo(_disposables);

            StopOperationCommand = HasCTContsConnection
                .CombineLatest(
                    BusyState,
                    (hasConnection, isBusy) => (hasConnection && isBusy)
                )
                .ToAsyncReactiveCommand()
                .WithSubscribe(OnStopOperationAsync)
                .AddTo(_disposables);

            TighteningCommand = TightenGoing
                .Select(x => x ? StopOperationCommand : StartTighteningCommand)
                .ToReadOnlyReactivePropertySlim<AsyncReactiveCommand>()
                .AddTo(_disposables);
            LooseningCommand = LoosenGoing
                .Select(x => x ? StopOperationCommand : StartLooseningCommand)
                .ToReadOnlyReactivePropertySlim<AsyncReactiveCommand>()
                .AddTo(_disposables);
            FreeRunCommand = FreeRunGoing
                .Select(x => x ? StopOperationCommand : StartFreeRunCommand)
                .ToReadOnlyReactivePropertySlim<AsyncReactiveCommand>()
                .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Vacuum related part (ProjectFileEditorViewModelVacuum.cs)
            // ----------------------------------------------------------------------
            VacuumSettingsCommand = CanSaveFile
                .ToReactiveCommand()
                .WithSubscribe(OnVacuumSettings)
                .AddTo(_disposables);
            VacuumOnCommand = HasRobotControllerConnection
                .ToAsyncReactiveCommand<bool>()
                .WithSubscribe(OnVacuumOn)
                .AddTo(_disposables);
            VacuumBreakOnCommand = HasRobotControllerConnection
                .ToAsyncReactiveCommand<bool>()
                .WithSubscribe(OnVacuumBreakOn)
                .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Log related part (ProjectFileEditorViewModelLog.cs)
            // ----------------------------------------------------------------------
            ResultLogEntry.LogEntryChanged += LogEntryChangedHandler;

            SelectedTighteningResult
                .Subscribe(OnSelectedTighteningResultChanged)
                .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Misc related part (ProjectFileEditorViewModelMisc.cs)
            // ----------------------------------------------------------------------
            FetchFromControllerCommand
                .Subscribe(OnFetchFromControllerAsync)
                .AddTo(_disposables);

            SelectUSBDriveCommand
                .Subscribe(OnSelectUSBDrive)
                .AddTo(_disposables);

            // The logs are read from the selected drive, so reload them on change.
            SelectedUSBDrive.Subscribe((_) =>
            {
                SetupLogsAndGraph();
            })
            .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Graph related part (ProjectFileEditorViewModelGraph.cs)
            // ----------------------------------------------------------------------

            // Re-apply the localized axis labels when the UI language is switched.
            Captions!.LanguageChanged += () =>
            {
                Plot.UpdateTerms(TorqueUnit.Value);
                Plot.Refresh();
            };

            const float _numTurnsUpperLimitInitial = 5f;
            const float _torqueUpperLimitInitial = 50f;

            NumTurnsUpperLimit.Value = _numTurnsUpperLimitInitial;
            TorqueUpperLimit.InnerValue = _torqueUpperLimitInitial;

            // The fixed axis upper limits must be positive values.
            NumTurnsUpperLimit
                .SetValidateNotifyError((value) => _upperLimitValidator.Validate(NumTurnsUpperLimit, value))
                .AddTo(_disposables);
            TorqueUpperLimit.SetValidator(_upperLimitValidator);
            _upperLimitValidator.ChangedAction = OnScaleSettingsChanged;

            NumTurnsAutoScaling
                .Subscribe((_) => OnScaleSettingsChanged())
                .AddTo(_disposables);
            TorqueAutoScaling
                .Subscribe((_) => OnScaleSettingsChanged())
                .AddTo(_disposables);

            // ----------------------------------------------------------------------
            // Common part (This file)
            // ----------------------------------------------------------------------
            CanSaveFile
                .Subscribe(CanSaveFileChanged)
                .AddTo(_disposables);

            HasCTContsConnection
                .Subscribe(HasCTContsConnectionChanged)
                .AddTo(_disposables);

            HasRobotControllerConnection
                .Subscribe(HasRobotControllerConnectionChanged)
                .AddTo(_disposables);

            SelectedCTContsId
                .Subscribe(SelectedCTContsIdChanged)
                .AddTo(_disposables);

            SelectedToolType
                .Subscribe(SelectedToolTypeChanged)
                .AddTo(_disposables);

            // Reflect the dirty state of the data in the state of the hosting content.
            _proFuseData.PropertyChanged += (_, _) =>
            {
                if (_windowAPI != null)
                {
                    var state = _windowAPI.GetContentState(this);

                    if (_proFuseData.IsDirty)
                    {
                        state |= IRCXWindowAPI.ContentState.IsDirty;
                    }
                    else
                    {
                        state &= ~IRCXWindowAPI.ContentState.IsDirty;
                    }

                    _windowAPI.SetContentState(this, state);
                }
            };

            // Get API objects
            _projectAPI = Main.GetAPI<IRCXProjectAPI>();
            _windowAPI = Main.GetAPI<IRCXWindowAPI>();
            _ioAPI = Main.GetAPI<IRCXIOAPI>();
            _controllerConnectionAPI = Main.GetAPI<IRCXControllerConnectionAPI>();
            _controllerAPI = Main.GetAPI<IRCXControllerAPI>();

            ErrorReporter.SetDelegate(this);
        }

        // ======================================================================

        /// <summary>
        /// Check the task is dead or alive.
        /// </summary>
        /// <param name="task">The task information.</param>
        /// <returns>True for dead task, false for other task.</returns>
        private static bool IsDeadTask(
            IRCXTask task
        )
        {
            return task.State switch
            {
                IRCXTask.RCXTaskState.Finished => true,
                IRCXTask.RCXTaskState.Aborted => true,
                IRCXTask.RCXTaskState.Error => true,
                _ => false,
            };
        }

        // ======================================================================

        /// <inheritdoc />
        public Task WindowCreated()
        {
            // The file may only be saved while no task is running.
            Main.GetAPI<IRCXProgramExecutionAPI>()
                .ObserveProperty(x => x.Tasks)
                .Subscribe((tasks) =>
                {
                    CanSaveFile.Value = tasks.All(x => IsDeadTask(x));
                })
                .AddTo(_disposables);

            _controllerConnectionAPI
                .ObserveProperty(x => x.IsOnline)
                .Subscribe((isOnline) =>
                {
                    HasRobotControllerConnection.Value = (isOnline == true);
                })
                .AddTo(_disposables);

            // Make sure the loaded data matches the tool type used last time.
            _proFuseData.Adjust(_proFuseData.OtherData.LastToolType, true);

            return Task.CompletedTask;
        }
    }
}

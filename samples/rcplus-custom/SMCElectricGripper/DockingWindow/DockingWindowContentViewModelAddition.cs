// -----------------------------------------------------------------------
// <copyright file="DockingWindowContentViewModelAddition.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using Epson.RoboticsShared.ExtensionsAPI;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SMCElectricGripper.Models;
using SMCElectricGripper.Spel;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static SMCElectricGripper.Constants;
using V2 = Epson.RoboticsShared.ExtensionsAPI.V2;

namespace SMCElectricGripper.DockingWindow
{
    /// <summary>
    /// Extension : Docking Window (Specific Part)
    /// </summary>
    internal partial class DockingWindowContentViewModel
    {
        /// <summary>
        /// Available COM ports for communication
        /// </summary>
        public ReactiveCollection<int> ComPorts { get; } = [];

        /// <summary>
        /// Selected COM port
        /// </summary>
        public ReactivePropertySlim<int> SelectedComPort { get; } = new(1);

        /// <summary>
        /// Command to connect to the gripper via the selected COM port
        /// </summary>
        public AsyncReactiveCommand ConnectCommand { get; } = new();

        /// <summary>
        /// Command to disconnect to the gripper via the selected COM port
        /// </summary>
        public AsyncReactiveCommand DisconnectCommand { get; } = new();

        /// <summary>
        /// Indicates whether the system is ready for gripper-related operations.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsSystemReady { get; }

        /// <summary>
        /// Indicates whether gripper operations can be executed.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsOperationReady { get; }

        /// <summary>
        /// Indicates whether the gripper is currently connected
        /// </summary>
        public ReactivePropertySlim<bool> IsGripperConnected { get; } = new(false);

        /// <summary>
        /// Indicates whether any operation is currently in progress.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsOperating { get; }

        /// <summary>
        /// Indicates whether the open jog operation can be executed.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> CanOpen { get; }

        /// <summary>
        /// Indicates whether the close jog operation can be executed.
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> CanClose { get; }

        /// <summary>
        /// Available recipe numbers
        /// </summary>
        public ReactiveCollection<int> Recipes { get; } = [];

        /// <summary>
        /// Selected recipe number
        /// </summary>
        public ReactivePropertySlim<int> SelectedRecipe { get; } = new(1);

        /// <summary>
        /// Indicates whether the positioning mode is selected
        /// </summary>
        public ReactivePropertySlim<bool> IsPositionMode { get; } = new(true);

        /// <summary>
        /// Target position in millimeters
        /// </summary>
        public ReactivePropertySlim<double> Position { get; } = new(0.0);

        /// <summary>
        /// Minimum allowed position value in millimeters
        /// </summary>
        public ReactivePropertySlim<double> PositionMin { get; } = new(0.0);

        /// <summary>
        /// Maximum allowed position value in millimeters
        /// </summary>
        public ReactivePropertySlim<double> PositionMax { get; } = new(50.0);

        /// <summary>
        /// Target speed in millimeters per second
        /// </summary>
        public ReactivePropertySlim<double> Speed { get; } = new(5.0);

        /// <summary>
        /// Minimum allowed speed value in millimeters per second
        /// </summary>
        public ReactivePropertySlim<double> SpeedMin { get; } = new(5.0);

        /// <summary>
        /// Maximum allowed speed value in millimeters per second
        /// </summary>
        public ReactivePropertySlim<double> SpeedMax { get; } = new(30.0);

        /// <summary>
        /// Gripping force in newtons
        /// </summary>
        public ReactivePropertySlim<double> Force { get; } = new(60.0);

        /// <summary>
        /// Minimum allowed force value in newtons
        /// </summary>
        public ReactivePropertySlim<double> ForceMin { get; } = new(60.0);

        /// <summary>
        /// Maximum allowed force value in newtons
        /// </summary>
        public ReactivePropertySlim<double> ForceMax { get; } = new(140.0);

        /// <summary>
        /// Command to apply the position setting to the gripper.
        /// </summary>
        public AsyncReactiveCommand PositionCommitCommand { get; } = new();

        /// <summary>
        /// Command to apply the speed setting to the gripper.
        /// </summary>
        public AsyncReactiveCommand SpeedCommitCommand { get; } = new();

        /// <summary>
        /// Command to apply the force setting to the gripper.
        /// </summary>
        public AsyncReactiveCommand ForceCommitCommand { get; } = new();

        /// <summary>
        /// Command to back up the current settings
        /// </summary>
        public AsyncReactiveCommand BackupSettingsCommand { get; } = new();

        /// <summary>
        /// Command to restore settings from a backup
        /// </summary>
        public AsyncReactiveCommand RestoreSettingsCommand { get; } = new();

        /// <summary>
        /// Indicates whether the servo is currently ON
        /// </summary>
        public ReactivePropertySlim<bool> IsServoOn { get; } = new(false);

        /// <summary>
        /// Command to turn the servo ON or OFF
        /// </summary>
        public AsyncReactiveCommand ServoCommand { get; } = new();

        /// <summary>
        /// Command to execute the current recipe.
        /// </summary>
        public AsyncReactiveCommand ExecuteCommand { get; }

        /// <summary>
        /// Command to abort the current operation
        /// </summary>
        public AsyncReactiveCommand AbortCommand { get; }

        /// <summary>
        /// Command to start the open operation while the button is pressed
        /// </summary>
        public ReactiveCommand OpenStartCommand { get; } = new();

        /// <summary>
        /// Command to start the close operation while the button is pressed
        /// </summary>
        public ReactiveCommand CloseStartCommand { get; } = new();

        /// <summary>
        /// Command to abort the current jog operation.
        /// </summary>
        public ReactiveCommand JogAbortCommand { get; } = new();

        /// <summary>
        /// Current system status text
        /// </summary>
        public ReactivePropertySlim<string> Status { get; } = new(string.Empty);

        /// <summary>
        /// Current position feedback in millimeters
        /// </summary>
        public ReactivePropertySlim<double> CurrentPosition { get; } = new(0.0);

        /// <summary>
        /// Command to refresh the current status.
        /// </summary>
        public AsyncReactiveCommand RefreshCommand { get; } = new();

        /// <summary>
        /// Command to reset the gripper.
        /// </summary>
        public AsyncReactiveCommand ResetCommand { get; } = new();

        /// <summary>
        /// Represents the type of operation currently being executed.
        /// </summary>
        private enum OperationKind
        {
            /// <summary>No operation.</summary>
            None,

            /// <summary>Updating settings.</summary>
            Setting,

            /// <summary>Executing a command.</summary>
            Execute,

            /// <summary>Aborting.</summary>
            Abort,

            /// <summary>Jog open.</summary>
            Open,

            /// <summary>Jog close.</summary>
            Close,
        }

        /// <summary>
        /// Controller API object
        /// </summary>
        private readonly IRCXControllerAPI _controllerAPI;

        /// <summary>
        /// Controller API (V2) object
        /// </summary>
        private readonly V2.IRCXControllerAPI _controllerAPIv2;

        /// <summary>
        /// Controller connection API object
        /// </summary>
        private readonly IRCXControllerConnectionAPI _connectionAPI;

        /// <summary>
        /// Window API object
        /// </summary>
        private readonly IRCXWindowAPI _windowAPI;

        /// <summary>
        /// Project API object
        /// </summary>
        private readonly IRCXProjectAPI _projectAPI;

        /// <summary>
        /// Program Execution API object
        /// </summary>
        private readonly IRCXProgramExecutionAPI _programExecutionAPI;

        /// <summary>
        /// Program Execution API (V2) object
        /// </summary>
        private readonly V2.IRCXProgramExecutionAPI _programExecutionAPIv2;

        /// <summary>
        /// Library API object.
        /// </summary>
        private readonly IRCXLibraryAPI _libraryAPI;

        /// <summary>
        /// Ensures that ExecuteFunctionAsync calls are executed sequentially to prevent concurrent access.
        /// </summary>
        private readonly SemaphoreSlim _executeLock = new(1, 1);

        /// <summary>
        /// Lock for synchronizing access to the global variable cache.
        /// </summary>
        private readonly object _globalVarLock = new();

        /// <summary>
        /// Cache of global variable handles.
        /// </summary>
        private readonly Dictionary<string, IRCXProgramExecutionAPI.IRCXGlobalVariable> _globalVariables = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether a connection operation is currently in progress.
        /// </summary>
        private readonly ReactivePropertySlim<bool> _isConnecting = new(false);
        
        /// <summary>
        /// Represents the type of operation currently being executed.
        /// </summary>
        private readonly ReactivePropertySlim<OperationKind> _executingOperation = new(OperationKind.None);

        /// <summary>
        /// Cancellation token source for the current execution
        /// </summary>
        private CancellationTokenSource? _executeCommandCts;

        /// <summary>
        /// Indicates whether properties are being updated from the gripper to prevent write-back loops.
        /// </summary>
        private bool _isUpdatingFromGripper;

        /// <summary>
        /// Constructor
        /// </summary>
        public DockingWindowContentViewModel()
        {
            // Initialize API instances
            _controllerAPI = Main.GetAPI<IRCXControllerAPI>();
            _controllerAPIv2 = Main.GetAPI<V2.IRCXControllerAPI>();
            _connectionAPI = Main.GetAPI<IRCXControllerConnectionAPI>();
            _windowAPI = Main.GetAPI<IRCXWindowAPI>();
            _projectAPI = Main.GetAPI<IRCXProjectAPI>();
            _programExecutionAPI = Main.GetAPI<IRCXProgramExecutionAPI>();
            _programExecutionAPIv2 = Main.GetAPI<V2.IRCXProgramExecutionAPI>();
            _libraryAPI = Main.GetAPI<IRCXLibraryAPI>();

            // Initialize UI-related resources
            IsSystemReady = new[]
            {
                _projectAPI.ObserveProperty(x => x.ProjectFolder).Select(x => !string.IsNullOrEmpty(x)),
                _libraryAPI.ObserveProperty(x => x.ProjectLibraries).Select(x => x?.Any(y => string.Equals(y, SpelConstants.LEHR_LIBRARY_NAME, StringComparison.OrdinalIgnoreCase)) ?? false),
                _connectionAPI.ObserveProperty(x => x.IsOnline).Select(x => x == true),
                _connectionAPI.ObserveProperty(x => x.IsMonitorMode).Select(x => !x),
                _controllerAPIv2.ObserveProperty(x => x.OperationMode).Select(x => x == V2.IRCXControllerAPI.RCXOperationMode.Program),
            }
            .CombineLatestValuesAreAllTrue()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposables);
            IsOperationReady = new IObservable<bool>[]
            {
                IsSystemReady,
                IsGripperConnected,
                _isConnecting.Select(x => !x),
            }
            .CombineLatestValuesAreAllTrue()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposables);
            IsOperating =
                _executingOperation.Select(x => x != OperationKind.None)
                .ToReadOnlyReactivePropertySlim()
                .AddTo(_disposables);
            ExecuteCommand = new (new []
            {
                IsServoOn,
                IsOperating.Select(x => !x),
            }
            .CombineLatestValuesAreAllTrue()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposables));
            AbortCommand = new (new []
            {
                IsServoOn,
                _executingOperation.Select(x => x != OperationKind.Setting),
            }
            .CombineLatestValuesAreAllTrue()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposables));
            CanOpen = new[]
            {
                IsServoOn,
                _executingOperation.Select(x => x == OperationKind.None || x == OperationKind.Open),
            }
            .CombineLatestValuesAreAllTrue()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposables);
            CanClose = new[]
            {
                IsServoOn,
                _executingOperation.Select(x => x == OperationKind.None || x == OperationKind.Close),
            }
            .CombineLatestValuesAreAllTrue()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposables);

            var config = LoadConfiguration();

            ComPorts.Clear();
            foreach (var port in Enumerable.Range(1, 8).Concat(Enumerable.Range(1001, 8)))
            {
                ComPorts.Add(port);
            }
            SelectedComPort.Value = (config?.LastUsedComPort != null && ComPorts.Contains(config.LastUsedComPort))
                    ? config.LastUsedComPort
                    : ComPorts.First();

            Recipes.Clear();
            for (var i = 1; i <= 10; i++)
            {
                Recipes.Add(i);
            }
            SelectedRecipe.Value = Recipes.First();

            PositionMin.Value = 0;
            PositionMax.Value = 50;
            SpeedMin.Value = 5;
            SpeedMax.Value = 30;
            ForceMin.Value = 60;
            ForceMax.Value = 140;
            Status.Value = GripperStatusTextProvider.ToDisplayText(GripperStatus.None);

            // Register handlers
            _programExecutionAPI
                .ObserveProperty(x => x.Tasks)
                .Subscribe(_ => UpdateGripperConnectionState())
                .AddTo(_disposables);
            IsSystemReady.Subscribe(_ => UpdateGripperConnectionState()).AddTo(_disposables);
            IsOperationReady.Subscribe(async _ => await OnOperationReadyChangedAsync().ConfigureAwait(false)).AddTo(_disposables);
            ConnectCommand.Subscribe(OnConnectAsync).AddTo(_disposables);
            DisconnectCommand.Subscribe(OnDisconnectAsync).AddTo(_disposables);
            BackupSettingsCommand.Subscribe(OnBackupSettingsAsync).AddTo(_disposables);
            RestoreSettingsCommand.Subscribe(OnRestoreSettingsAsync).AddTo(_disposables);
            ServoCommand.Subscribe(OnServoChangedAsync).AddTo(_disposables);
            ResetCommand.Subscribe(OnResetAsync).AddTo(_disposables);
            ExecuteCommand.Subscribe(OnExecuteAsync).AddTo(_disposables);
            AbortCommand.Subscribe(OnAbortAsync).AddTo(_disposables);
            OpenStartCommand.Subscribe(OnOpenStart).AddTo(_disposables);
            CloseStartCommand.Subscribe(OnCloseStart).AddTo(_disposables);
            JogAbortCommand.Subscribe(OnJogAbort).AddTo(_disposables);
            RefreshCommand.Subscribe(OnRefreshAsync).AddTo(_disposables);
            PositionCommitCommand
                .Subscribe(CommitPositionAsync)
                .AddTo(_disposables);
            SpeedCommitCommand
                .Subscribe(CommitSpeedAsync)
                .AddTo(_disposables);
            ForceCommitCommand
                .Subscribe(CommitForceAsync)
                .AddTo(_disposables);

            // Reactively executes asynchronous side-effect logic and ensures only the latest change is applied.
            SelectedRecipe
                .Select(recipeNo => Observable.FromAsync(async () => await OnSelectedRecipeAsync(recipeNo).ConfigureAwait(false)))
                .Switch()
                .Subscribe()
                .AddTo(_disposables);
            IsPositionMode
                .Select(isPositionMode => Observable.FromAsync(async () => await OnOperationModeChangedAsync(isPositionMode).ConfigureAwait(false)))
                .Switch()
                .Subscribe()
                .AddTo(_disposables);
            Position
                .DistinctUntilChanged()
                .Subscribe(OnPositionValueChanged)
                .AddTo(_disposables);
            Speed
                .DistinctUntilChanged()
                .Subscribe(OnSpeedValueChanged)
                .AddTo(_disposables);
            Force
                .DistinctUntilChanged()
                .Subscribe(OnForceValueChanged)
                .AddTo(_disposables);
        }

        /// <inheritdoc />
        public Task WindowCreated()
        {
            return Task.CompletedTask;
        }

        private async Task OnOperationReadyChangedAsync()
        {
            if (!IsOperationReady.Value)
            {
                return;
            }

            await OnSelectedRecipeAsync(SelectedRecipe.Value).ConfigureAwait(true);
            UpdateServoState();
            await UpdateAlarmStateAsync().ConfigureAwait(true);
            await UpdateCurrentPositionStateAsync().ConfigureAwait(true);
        }

        private void UpdateGripperConnectionState()
        {
            if (!IsSystemReady.Value)
            {
                IsGripperConnected.Value = false;
                lock (_globalVarLock)
                {
                    _globalVariables.Clear();
                }
                return;
            }
            
            var value = GetGlobalVariableValue(SpelConstants.GVar_LEHR_COMM_TASK_NO);
            IsGripperConnected.Value =
                value != null &&
                int.TryParse(value, out var commTaskNo) &&
                _programExecutionAPI.Tasks.Any(t =>
                    t.Number == commTaskNo &&
                    t.State != IRCXProgramExecutionAPI.IRCXTask.RCXTaskState.Aborted &&
                    t.State != IRCXProgramExecutionAPI.IRCXTask.RCXTaskState.Finished);
        }

        private void UpdateServoState()
        {
            var value = GetGlobalVariableValue(SpelConstants.GVar_LEHR_SERVO_ON_OFF);
            IsServoOn.Value =
                value != null &&
                int.TryParse(value, out var servo) &&
                servo != 0;
        }

        private async Task UpdateAlarmStateAsync()
        {
            var func = SpelConstants.FUNC_LEHR_STATUS;
            var result = await ExecuteFunctionAsync(func, SpelConstants.GVar_LEHR_STATUS).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value.ErrorCode != SpelErrorCode.NoError)
            {
                return;
            }

            var status = Enum.TryParse<GripperStatus>(result.Value.Value, out var s) ? s : GripperStatus.None;
            Status.Value = GripperStatusTextProvider.ToDisplayText(status);
        }

        private async Task UpdateCurrentPositionStateAsync()
        {
            var func = SpelConstants.FUNC_LEHR_GET_CURRENT_POSITION;
            var result = await ExecuteFunctionAsync(func, SpelConstants.GVar_LEHR_CURRENT_POSITION).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value.ErrorCode != SpelErrorCode.NoError ||
                !double.TryParse(result.Value.Value, out var curPos))
            {
                return;
            }

            CurrentPosition.Value = curPos;
        }

        private async Task<MotionSetting?> ReadMotionSettingAsync(int recipeNo)
        {
            var func = string.Format(SpelConstants.FUNC_LEHR_GET_MODE, recipeNo);
            var result = await ExecuteFunctionAsync(func, SpelConstants.GVar_LEHR_MODE).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value.ErrorCode != SpelErrorCode.NoError ||
                !int.TryParse(result.Value.Value, out var mode))
            {
                return null;
            }

            func = string.Format(SpelConstants.FUNC_LEHR_GET_POSITION, recipeNo);
            result = await ExecuteFunctionAsync(func, SpelConstants.GVar_LEHR_POSITION).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value.ErrorCode != SpelErrorCode.NoError ||
                !double.TryParse(result.Value.Value, out var position))
            {
                return null;
            }

            func = string.Format(SpelConstants.FUNC_LEHR_GET_SPEED, recipeNo);
            result = await ExecuteFunctionAsync(func, SpelConstants.GVar_LEHR_SPEED).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value.ErrorCode != SpelErrorCode.NoError ||
                !double.TryParse(result.Value.Value, out var speed))
            {
                return null;
            }

            var force = ForceMin.Value;
            if ((OperationMode)mode == OperationMode.Gripping)
            {
                func = string.Format(SpelConstants.FUNC_LEHR_GET_FORCE, recipeNo);
                result = await ExecuteFunctionAsync(func, SpelConstants.GVar_LEHR_FORCE).ConfigureAwait(false);
                if (!result.HasValue ||
                    result.Value.ErrorCode != SpelErrorCode.NoError ||
                    !double.TryParse(result.Value.Value, out force))
                {
                    return null;
                }
            }

            return new MotionSetting
            {
                RecipeNo = recipeNo,
                Mode = (OperationMode)mode,
                Position = position,
                Speed = speed,
                Force = force,
            };
        }

        private async Task<bool> WriteRecipeMotionSettingAsync(MotionSetting motionSetting)
        {
            var func = string.Format(SpelConstants.FUNC_LEHR_SET_MODE, motionSetting.RecipeNo, (int)motionSetting.Mode);
            var result = await ExecuteFunctionAsync(func).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value != SpelErrorCode.NoError)
            {
                return false;
            }

            func = string.Format(SpelConstants.FUNC_LEHR_SET_POSITION, motionSetting.RecipeNo, motionSetting.Position);
            result = await ExecuteFunctionAsync(func).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value != SpelErrorCode.NoError)
            {
                return false;
            }

            func = string.Format(SpelConstants.FUNC_LEHR_SET_SPEED, motionSetting.RecipeNo, motionSetting.Speed);
            result = await ExecuteFunctionAsync(func).ConfigureAwait(true);
            if (!result.HasValue ||
                result.Value != SpelErrorCode.NoError)
            {
                return false;
            }

            if (motionSetting.Mode == OperationMode.Gripping)
            {
                func = string.Format(SpelConstants.FUNC_LEHR_SET_FORCE, motionSetting.RecipeNo, motionSetting.Force);
                result = await ExecuteFunctionAsync(func).ConfigureAwait(true);
                if (!result.HasValue ||
                    result.Value != SpelErrorCode.NoError)
                {
                    return false;
                }

            }

            return true;
        }

        private async Task<SpelErrorCode?> ExecuteFunctionAsync(string function, OperationKind operationKind = OperationKind.Setting, bool asBackgroundTask = false)
        {
            await _executeLock.WaitAsync().ConfigureAwait(true);
            _executingOperation.Value = operationKind;
            try
            {
                var result = await _programExecutionAPIv2.StartLibraryFunctionAsync(function, asBackgroundTask, false, Main.CommonId).ConfigureAwait(true);
                if (result != RCXResult.Success)
                {
                    return null;
                }

                var cts = new CancellationTokenSource();
                _executeCommandCts = cts;
                var code = await WaitUntilFunctionCompletedAsync(TimeSpan.FromSeconds(10), cts.Token).ConfigureAwait(true);
                return (SpelErrorCode?)code;
            }
            catch
            {
                // Ignored: cancellation or any exception
                return null;
            }
            finally
            {
                _executingOperation.Value = OperationKind.None;
                _executeLock.Release();
            }
        }

        private async Task<(SpelErrorCode ErrorCode, string Value)?> ExecuteFunctionAsync(string function, string globalVariable)
        {
            var errorCode = await ExecuteFunctionAsync(function).ConfigureAwait(true);
            if (!errorCode.HasValue)
            {
                return null;
            }
            else if (errorCode.Value != SpelErrorCode.NoError)
            {
                return (errorCode.Value, string.Empty);
            }

            var value = GetGlobalVariableValue(globalVariable);
            return value != null ? (errorCode.Value, value) : null;
        }

        private string? GetGlobalVariableValue(string name)
        {
            lock (_globalVarLock)
            {
                string value;
                if (_globalVariables.TryGetValue(name, out var globalVar))
                {
                    if (globalVar.GetValue(out value) == RCXResult.Success)
                    {
                        return value;
                    }

                    _globalVariables.Remove(name);
                }

                var (result, globalVars) = _programExecutionAPI.GetGlobalVariables();
                if (result != RCXResult.Success || globalVars == null)
                {
                    return null;
                }

                globalVar = globalVars
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (globalVar == null ||
                    globalVar.GetValue(out value) != RCXResult.Success)
                {
                    return null;
                }

                _globalVariables[name] = globalVar;
                return value;
            }
        }

        private RCXResult SetGlobalVariableValue(string name, string value)
        {
            lock (_globalVarLock)
            {
                RCXResult result;
                if (_globalVariables.TryGetValue(name, out var globalVar))
                {
                    result = globalVar.SetValue(value);
                    if (result == RCXResult.Success)
                    {
                        return result;
                    }

                    _globalVariables.Remove(name);
                }

                (result, var globalVars) = _programExecutionAPI.GetGlobalVariables();
                if (result != RCXResult.Success)
                {
                    return result;
                }

                globalVar = globalVars?
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (globalVar == null)
                {
                    return RCXResult.InvalidValue;
                }

                result = globalVar.SetValue(value);
                if (result != RCXResult.Success)
                {
                    return result;
                }

                _globalVariables[name] = globalVar;
                return result;
            }
        }

        private async Task OnConnectAsync()
        {
            _isConnecting.Value = true;

            try
            {
                if (_libraryAPI.ProjectLibraries?.Any(y => string.Equals(y, SpelConstants.LEHR_LIBRARY_NAME, StringComparison.OrdinalIgnoreCase)) != true)
                {
                    _windowAPI.ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.LibraryProjectNotOpenMessage),
                        IRCXWindowAPI.ButtonType.OK,
                        IRCXWindowAPI.IconType.Error);
                    return;
                }

                await _connectionAPI.ConnectControllerAsync().ConfigureAwait(true);
                if (_connectionAPI.IsOnline != true || _connectionAPI.IsMonitorMode == true)
                {
                    _windowAPI.ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.ControllerConnectionFailedMessage),
                        IRCXWindowAPI.ButtonType.OK,
                        IRCXWindowAPI.IconType.Error);
                    return;
                }

                var (getResult, preference) = _controllerAPI.GetControllerSettings(null, "Preference");
                var allowBackgroundTasks = false;
                if (getResult == RCXResult.Success &&
                    preference != null &&
                    preference.TryGetValue("AllowBackgroundTasks", out var val) &&
                    val.Value is bool boolVal)
                {
                    allowBackgroundTasks = boolVal;
                }

                if (!allowBackgroundTasks)
                {
                    var response = _windowAPI.ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.BackgroundTaskDisabledMessage),
                        IRCXWindowAPI.ButtonType.Yes_No,
                        IRCXWindowAPI.IconType.Question);
                    if (response != IRCXWindowAPI.ResponseType.Yes)
                    {
                        return;
                    }
                }

                var func = string.Format(SpelConstants.FUNC_LEHR_CONNECT, SelectedComPort.Value);
                var result = await ExecuteFunctionAsync(func, asBackgroundTask: allowBackgroundTasks).ConfigureAwait(true);
                if (!result.HasValue || result.Value != SpelErrorCode.NoError)
                {
                    _windowAPI.ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.GripperConnectionFailedMessage),
                        IRCXWindowAPI.ButtonType.OK,
                        IRCXWindowAPI.IconType.Error);
                    return;
                }

                var config = new Configuration
                {
                    LastUsedComPort = SelectedComPort.Value,
                };
                SaveConfiguration(config);
            }
            finally
            {
                _isConnecting.Value = false;
            }
        }

        private async Task OnDisconnectAsync()
        {
            if (!IsGripperConnected.Value)
            {
                return;
            }

            var func = SpelConstants.FUNC_LEHR_DISCONNECT;
            await ExecuteFunctionAsync(func).ConfigureAwait(true);
        }

        private async Task OnSelectedRecipeAsync(int newRecipeNo)
        {
            if (!IsOperationReady.Value)
            {
                return;
            }

            var setting = await ReadMotionSettingAsync(newRecipeNo).ConfigureAwait(true);
            if (setting == null)
            {
                return;
            }

            _isUpdatingFromGripper = true;
            try
            {
                IsPositionMode.Value = setting.Mode == OperationMode.Positioning;
                Position.Value = setting.Position;
                Speed.Value = setting.Speed;
                Force.Value = setting.Force;
            }
            finally
            {
                _isUpdatingFromGripper = false;
            }
        }

        private static readonly JsonSerializerOptions _writeOptions = new() { WriteIndented = true };

        private async Task OnBackupSettingsAsync()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Title = Captions[Caption.BackupSettingsLabel],
                FileName = "SMCLEHRSettings.json",
                Filter = "JSON files (*.json)|*.json",
            };
            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            void ShowBackupFailedMessage()
            {
                _windowAPI.ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(string.Format(
                        Captions[Caption.MotionSettingsBackupFailedMessage],
                        Path.GetFileName(saveFileDialog.FileName))),
                    IRCXWindowAPI.ButtonType.OK,
                    IRCXWindowAPI.IconType.Error);
            }

            var settings = new Dictionary<int, MotionSetting>();
            foreach (var recipeNo in Recipes)
            {
                var setting = await ReadMotionSettingAsync(recipeNo).ConfigureAwait(true);
                if (setting == null)
                {
                    ShowBackupFailedMessage();
                    return;
                }

                settings[recipeNo] = setting;
            }

            try
            {
                var json = JsonSerializer.Serialize(settings, _writeOptions);
                File.WriteAllText(saveFileDialog.FileName, json, Encoding.UTF8);
            }
            catch
            {
                ShowBackupFailedMessage();
            }
        }

        private async Task OnRestoreSettingsAsync()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = Captions[Caption.RestoreSettingsLabel],
                Filter = "JSON files (*.json)|*.json",
            };
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            void ShowRestoreFailedMessage()
            {
                _windowAPI.ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(string.Format(
                        Captions[Caption.MotionSettingsLoadFailedMessage],
                        Path.GetFileName(openFileDialog.FileName))),
                    IRCXWindowAPI.ButtonType.OK,
                    IRCXWindowAPI.IconType.Error);
            }

            Dictionary<int, MotionSetting>? settings;
            try
            {
                var json = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);
                settings = JsonSerializer.Deserialize<Dictionary<int, MotionSetting>>(json);
            }
            catch
            {
                ShowRestoreFailedMessage();
                return;
            }

            if (settings == null || settings.Count == 0)
            {
                ShowRestoreFailedMessage();
                return;
            }

            foreach (var (_, setting) in settings)
            {
                var result = await WriteRecipeMotionSettingAsync(setting).ConfigureAwait(true);
                if (!result)
                {
                    ShowRestoreFailedMessage();
                    return;
                }
            }

            await OnSelectedRecipeAsync(SelectedRecipe.Value).ConfigureAwait(true);
        }

        private async Task OnServoChangedAsync()
        {
            var func = string.Format(SpelConstants.FUNC_LEHR_SERVO, IsServoOn.Value ? 1 : 0);
            var result = await ExecuteFunctionAsync(func).ConfigureAwait(true);
            if (!result.HasValue || result.Value != SpelErrorCode.NoError)
            {
                return;
            }

            UpdateServoState();
        }

        private async Task OnResetAsync()
        {
            var func = string.Format(SpelConstants.FUNC_LEHR_RESET);
            await ExecuteFunctionAsync(func).ConfigureAwait(true);
            await OnRefreshAsync().ConfigureAwait(false);
        }

        private async Task OnExecuteAsync()
        {
            await ExecuteRecipe(SelectedRecipe.Value, OperationKind.Execute).ConfigureAwait(true);
        }

        private async Task ExecuteRecipe(int recipeNo, OperationKind operationKind)
        {
            var func = string.Format(SpelConstants.FUNC_LEHR_EXECUTE, recipeNo, 0);
            await ExecuteFunctionAsync(func, operationKind).ConfigureAwait(false);
        }

        private async Task<int?> WaitUntilFunctionCompletedAsync(TimeSpan timeout, CancellationToken token)
        {
            var start = Environment.TickCount64;
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (Environment.TickCount64 - start > timeout.TotalMilliseconds)
                {
                    return null;
                }

                var value = GetGlobalVariableValue(SpelConstants.GVar_LEHR_RETURN);
                if (value != null &&
                    int.TryParse(value, out var result) &&
                    result >= 0) // If the value is negative, the function is currently executing
                {
                    return result;
                }

                await Task.Delay(100, token).ConfigureAwait(true);
            }
        }

        private async Task OnAbortAsync()
        {
            _executeCommandCts?.Cancel();
            _executeCommandCts = null;

            var func = string.Format(SpelConstants.FUNC_LEHR_ABORT);
            await ExecuteFunctionAsync(func, OperationKind.Abort).ConfigureAwait(true);
        }

        private void OnOpenStart()
        {
            _executingOperation.Value = OperationKind.Open;
            SetGlobalVariableValue(SpelConstants.GVar_LEHR_JOG, ((int)JogState.Opening).ToString());
        }

        private void OnCloseStart()
        {
            _executingOperation.Value = OperationKind.Close;
            SetGlobalVariableValue(SpelConstants.GVar_LEHR_JOG, ((int)JogState.Closing).ToString());
        }

        private void OnJogAbort()
        {
            SetGlobalVariableValue(SpelConstants.GVar_LEHR_JOG, ((int)JogState.Idle).ToString());
            _executingOperation.Value = OperationKind.None;
        }

        private async Task OnRefreshAsync()
        {
            if (!IsOperationReady.Value)
            {
                return;
            }

            await UpdateAlarmStateAsync().ConfigureAwait(true);
            await UpdateCurrentPositionStateAsync().ConfigureAwait(true);
        }

        private async Task OnOperationModeChangedAsync(bool isPositionMode)
        {
            if (!IsOperationReady.Value)
            {
                return;
            }

            if (_isUpdatingFromGripper)
            {
                return;
            }

            SpeedMax.Value = isPositionMode ? 100 : 30;

            if (Speed.Value > SpeedMax.Value)
            {
                Speed.Value = SpeedMax.Value;
                await CommitPositionAsync().ConfigureAwait(true);
            }

            var func = string.Format(SpelConstants.FUNC_LEHR_SET_MODE, SelectedRecipe.Value, (int)(isPositionMode ? OperationMode.Positioning : OperationMode.Gripping));
            await ExecuteFunctionAsync(func).ConfigureAwait(true);
        }

        private void OnPositionValueChanged(double x)
        {
            if (_isUpdatingFromGripper)
            {
                return;
            }

            if (x < PositionMin.Value)
            {
                Position.Value = PositionMin.Value;
            }
            else if (x > PositionMax.Value)
            {
                Position.Value = PositionMax.Value;
            }
        }

        private void OnSpeedValueChanged(double x)
        {
            if (_isUpdatingFromGripper)
            {
                return;
            }

            if (x < SpeedMin.Value)
            {
                Speed.Value = SpeedMin.Value;
            }
            else if (x > SpeedMax.Value)
            {
                Speed.Value = SpeedMax.Value;
            }
        }

        private void OnForceValueChanged(double x)
        {
            if (_isUpdatingFromGripper)
            {
                return;
            }

            if (x < ForceMin.Value)
            {
                Force.Value = ForceMin.Value;
            }
            else if (x > ForceMax.Value)
            {
                Force.Value = ForceMax.Value;
            }
        }

        private async Task CommitPositionAsync()
        {
            if (!IsOperationReady.Value)
            {
                return;
            }
            if (_isUpdatingFromGripper)
            {
                return;
            }

            var func = string.Format(SpelConstants.FUNC_LEHR_SET_POSITION, SelectedRecipe.Value, Math.Round(Position.Value, 1));
            await ExecuteFunctionAsync(func).ConfigureAwait(false);
        }

        private async Task CommitSpeedAsync()
        {
            if (!IsOperationReady.Value)
            {
                return;
            }
            if (_isUpdatingFromGripper)
            {
                return;
            }

            var func = string.Format(SpelConstants.FUNC_LEHR_SET_SPEED, SelectedRecipe.Value, Math.Round(Speed.Value, 1));
            await ExecuteFunctionAsync(func).ConfigureAwait(false);
        }

        private async Task CommitForceAsync()
        {
            if (!IsOperationReady.Value)
            {
                return;
            }
            if (_isUpdatingFromGripper)
            {
                return;
            }

            var func = string.Format(SpelConstants.FUNC_LEHR_SET_FORCE, SelectedRecipe.Value, Math.Round(Force.Value, 1));
            await ExecuteFunctionAsync(func).ConfigureAwait(false);
        }

        private static Configuration? LoadConfiguration()
        {
            try
            {
                var bytes = Main.Settings?.Read();
                if (bytes == null)
                {
                    return null;
                }

                var json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<Configuration>(json);
            }
            catch
            {
                // Ignore invalid or corrupted configuration and treat as not existing
                return null;
            }
        }

        private static void SaveConfiguration(Configuration configuration)
        {
            try
            {
                var json = JsonSerializer.Serialize(configuration);
                var bytes = Encoding.UTF8.GetBytes(json);

                Main.Settings?.Write(bytes);
            }
            catch
            {
                // Ignore
            }
        }
    }
}

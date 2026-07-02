// -----------------------------------------------------------------------
// <copyright file="ContentViewModelAddition.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using Epson.RoboticsShared.ExtensionsAPI;
    using Reactive.Bindings;
    using Reactive.Bindings.Extensions;
    using System.Windows;
    using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;

    /// <summary>
    /// Extension : Docking Window (Specific Part)
    /// </summary>
    internal partial class DockingWindowContentViewModel
    {
        /// <summary>
        /// The controller is online or not
        /// </summary>
        public ReactivePropertySlim<bool> IsOnline { get; } = new(false);

        /// <summary>
        /// Motors are powered or not
        /// </summary>
        public ReactivePropertySlim<bool> IsMotorOn { get; } = new(false);

        /// <summary>
        /// Motor state expression
        /// </summary>
        public ReactivePropertySlim<string> MotorState { get; } = new("Off");

        /// <summary>
        /// Toggle motor state command
        /// </summary>
        public AsyncReactiveCommand MotorToggleCommand { get; }

        /// <summary>
        /// Left stick position
        /// </summary>
        public ReactivePropertySlim<Vector> LeftStickPosition { get; } = new(new Vector(), ReactivePropertyMode.None);

        /// <summary>
        /// Right stick position
        /// </summary>
        public ReactivePropertySlim<Vector> RightStickPosition { get; } = new(new Vector(), ReactivePropertyMode.None);

        /// <summary>
        /// TeachCommand feasibility
        /// </summary>
        public ReactivePropertySlim<bool> CanTeach { get; } = new(false);

        /// <summary>
        /// Teach command
        /// </summary>
        public AsyncReactiveCommand TeachCommand { get; }

        /// <summary>
        /// Teached points information for log
        /// </summary>
        public ReactiveCollection<LogItem> LogItems { get; } = [];

        /// <summary>
        /// API result expression
        /// </summary>
        public ReactivePropertySlim<string> APIResult { get; } = new();

        /// <summary>
        /// Auxiliary information for API result (Error message etc.)
        /// </summary>
        public ReactivePropertySlim<string> APIResultAux { get; } = new();

        /// <summary>
        /// Controller connection API object
        /// </summary>
        private IRCXControllerConnectionAPI? _connectionAPI;

        /// <summary>
        /// Controller API object
        /// </summary>
        private IRCXControllerAPI? _controllerAPI;

        /// <summary>
        /// Robot manager API object
        /// </summary>
        private IRCXRobotManagerAPI? _robotManagerAPI;

        /// <summary>
        /// Point API object
        /// </summary>
        private IRCXPointAPI? _pointAPI;

        /// <summary>
        /// Jogger object
        /// </summary>
        private IRCXRobotManagerAPI.IRCXJogger? _jogger;

        /// <summary>
        /// Polling timer
        /// </summary>
        private PeriodicTimer? _pollingTimer;

        /// <summary>
        /// Polling task
        /// </summary>
        private Task? _pollingTask;

        /// <summary>
        /// Next axis to jog
        /// </summary>
        private IRCXRobotManagerAPI.RCXJogCartesianAxis _targetAxis = IRCXRobotManagerAPI.RCXJogCartesianAxis.Z;

        /// <summary>
        /// Logical dead zone for stick
        /// </summary>
        private const double _positionThreshold = 0.1;

        /// <summary>
        /// Polling interval
        /// </summary>
        private const long _pollingMSec = 10;

        /// <summary>
        /// Target point file for teaching
        /// </summary>
        private string? _targetPointFile;

        /// <summary>
        /// Toggles the motor state
        /// </summary>
        /// <returns>Task</returns>
        private async Task OnMotorToggleAsync()
        {
            if (_controllerAPI != null)
            {
                if (_controllerAPI.IsMotorOn == true)
                {
                    var result = await _controllerAPI.MotorOffAsync();
                    APIResult.Value = result.ToString();
                    APIResultAux.Value = string.Empty;
                }
                else if (_controllerAPI.IsMotorOn == false)
                {
                    var result = await _controllerAPI.MotorOnAsync();
                    APIResult.Value = result.ToString();
                    APIResultAux.Value = string.Empty;
                }
            }
        }

        /// <summary>
        /// Jog along specified axis
        /// </summary>
        /// <param name="axis">Axis</param>
        /// <param name="position">Stick position</param>
        /// <returns>Task</returns>
        private async Task Jog(
            IRCXRobotManagerAPI.RCXJogCartesianAxis axis,
            double position
        )
        {
            if (_jogger != null && _jogger.IsValid)
            {
                var oppositeDirection = (position > 0);
                var (result, message) = await _jogger.StartCartesianJogAsync(axis, oppositeDirection);
                APIResult.Value = result.ToString() + (string.IsNullOrEmpty(message) ? string.Empty : " *");
                APIResultAux.Value = message;
            }
        }

        /// <summary>
        /// Check the stick positions and jog
        /// </summary>
        /// <returns>Task</returns>
        private async Task CheckStickPosition()
        {
            switch (_targetAxis)
            {
                case IRCXRobotManagerAPI.RCXJogCartesianAxis.X:
                    if (Math.Abs(RightStickPosition.Value.X) >= _positionThreshold)
                    {
                        await Jog(_targetAxis, RightStickPosition.Value.X);
                    }
                    _targetAxis = IRCXRobotManagerAPI.RCXJogCartesianAxis.Y;
                    break;

                case IRCXRobotManagerAPI.RCXJogCartesianAxis.Y:
                    if (Math.Abs(RightStickPosition.Value.Y) >= _positionThreshold)
                    {
                        await Jog(_targetAxis, RightStickPosition.Value.Y);
                    }
                    _targetAxis = IRCXRobotManagerAPI.RCXJogCartesianAxis.Z;
                    break;

                case IRCXRobotManagerAPI.RCXJogCartesianAxis.Z:
                    if (Math.Abs(LeftStickPosition.Value.Y) >= _positionThreshold)
                    {
                        await Jog(_targetAxis, LeftStickPosition.Value.Y);
                    }
                    _targetAxis = IRCXRobotManagerAPI.RCXJogCartesianAxis.X;
                    break;
            }
        }

        /// <summary>
        /// Set target point file
        /// </summary>
        /// <param name="robotNumber">Robot number</param>
        private void SetTargetPointFile(
            int? robotNumber
        )
        {
            _targetPointFile = null;

            if (_pointAPI != null)
            {
                var descriptors = _pointAPI.PointFileDescriptors;

                _targetPointFile = descriptors
                    .Where(x => x.RobotNumber == robotNumber && x.IsDefault)
                    .Select(x => x.FileName)
                    .FirstOrDefault();

                if (_targetPointFile == null)
                {
                    _targetPointFile = descriptors
                        .Where(x => x.RobotNumber == null)
                        .Select(x => x.FileName)
                        .FirstOrDefault();
                }
            }

            CanTeach.Value = (IsOnline.Value && !string.IsNullOrEmpty(_targetPointFile));
        }

        /// <summary>
        /// Teach point
        /// </summary>
        /// <returns>Task</returns>
        private Task OnTeachAsync()
        {
            if (_pointAPI != null && _targetPointFile != null)
            {
                var (result, points) = _pointAPI.GetPoints(_targetPointFile);
                if (result == RCXResult.Success && points != null)
                {
                    var pointNumbers = points.Select(x => (int)x["Number"].Value).ToHashSet();
                    var pointNumberRange = Enumerable.Range(
                        _pointAPI.PointNumberMin,
                        _pointAPI.PointNumberMax - _pointAPI.PointNumberMin + 1
                    );
                    foreach (var number in pointNumberRange)
                    {
                        if (!pointNumbers.Contains(number))
                        {
                            var stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            var teachResult = _pointAPI.TeachPoint(
                                _targetPointFile,
                                number,
                                description: $"SimpleJog: {stamp}",
                                shouldSave: true
                            );
                            APIResult.Value = teachResult.ToString();
                            APIResultAux.Value = string.Empty;

                            if (teachResult == RCXResult.Success)
                            {
                                LogItems.Add(new(number, _robotManagerAPI?.WorldPosition));
                            }
                            break;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DockingWindowContentViewModel()
        {
            MotorToggleCommand = IsOnline
            .ToAsyncReactiveCommand()
            .WithSubscribe(OnMotorToggleAsync)
            .AddTo(_disposables);

            TeachCommand = CanTeach
            .ToAsyncReactiveCommand()
            .WithSubscribe(OnTeachAsync)
            .AddTo(_disposables);
        }

        /// <inheritdoc />
        public Task WindowCreated()
        {
            _connectionAPI = Main.GetAPI<IRCXControllerConnectionAPI>();

            _connectionAPI?.ObserveProperty(x => x.IsOnline).Subscribe((isOnline) =>
            {
                IsOnline.Value = (isOnline == true);

                CanTeach.Value = (IsOnline.Value && !string.IsNullOrWhiteSpace(_targetPointFile));
            })
            .AddTo(_disposables);

            _controllerAPI = Main.GetAPI<IRCXControllerAPI>();
            _robotManagerAPI = Main.GetAPI<IRCXRobotManagerAPI>();
            _pointAPI = Main.GetAPI<IRCXPointAPI>();

            _controllerAPI?.ObserveProperty(x => x.IsMotorOn).Subscribe(async (isMotorOn) =>
            {
                IsMotorOn.Value = (isMotorOn == true);
                MotorState.Value = (isMotorOn == true) ? "On" : "Off";

                if (_robotManagerAPI != null)
                {
                    if (isMotorOn == true)
                    {
                        _jogger = await _robotManagerAPI.CreateJoggerAsync();
                        _pollingTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(_pollingMSec));
                        _pollingTask = Task.Factory.StartNew(async () =>
                        {
                            while (await _pollingTimer.WaitForNextTickAsync())
                            {
                                await CheckStickPosition();
                            }
                        });
                    }
                    else
                    {
                        if (_jogger != null)
                        {
                            await _jogger.DisposeAsync();
                            _jogger = null;
                        }
                        _pollingTask?.Dispose();
                        _pollingTimer?.Dispose();
                    }

                }
            })
            .AddTo(_disposables);

            _robotManagerAPI?.ObserveProperty(x => x.CurrentRobotNumber).Subscribe((robotNumber) =>
            {
                SetTargetPointFile(robotNumber);
            })
            .AddTo(_disposables);

            return Task.CompletedTask;
        }
    }
}

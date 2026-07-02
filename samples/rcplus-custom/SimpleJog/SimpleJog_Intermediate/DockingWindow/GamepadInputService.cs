// -----------------------------------------------------------------------
// <copyright file="GamepadInputService.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using Reactive.Bindings;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Windows.Gaming.Input;

    /// <summary>
    /// Implementation of gamepad input service
    /// </summary>
    public class GamepadInputService : IGamepadInputService
    {
        /// <inheritdoc />
        public IReadOnlyReactiveProperty<GamepadReading> CurrentReading => _reading;

        /// <summary>
        /// The substance of CurrentReading
        /// </summary>
        private readonly ReactivePropertySlim<GamepadReading> _reading = new(mode: ReactivePropertyMode.None);

        /// <summary>
        /// Target gamepad
        /// </summary>
        private Gamepad? _gamepad;

        /// <summary>
        /// Timer for polling
        /// </summary>
        private DispatcherTimer _timer;

        /// <summary>
        /// Polling interval
        /// </summary>
        private const int _pollingIntervalMSec = 16;

        /// <summary>
        /// Constructor
        /// </summary>
        public GamepadInputService()
        {
            _timer = new()
            {
                Interval = TimeSpan.FromMilliseconds(_pollingIntervalMSec),
            };

            _timer.Tick += (_, _) =>
            {
                if (_gamepad != null)
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        return;
                    }

                    _reading.Value = _gamepad.GetCurrentReading();
                }
            };
        }

        /// <inheritdoc />
        public void SetGamepad(
            Gamepad? gamepad
        )
        {
            _gamepad = gamepad;
        }

        /// <inheritdoc />
        public void Start()
        {
            _timer.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _timer.Stop();
        }
    }
}

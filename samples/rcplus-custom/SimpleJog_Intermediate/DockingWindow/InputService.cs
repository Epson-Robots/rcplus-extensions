// -----------------------------------------------------------------------
// <copyright file="InputService.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using Reactive.Bindings;
    using Reactive.Bindings.Extensions;
    using System;
    using System.Reactive.Disposables;
    using System.Windows;
    using Windows.Gaming.Input;

    /// <summary>
    /// Input service
    /// </summary>
    public class InputService : IDisposable
    {
        /// <summary>
        /// State of gamepad buttons
        /// </summary>
        public ReactivePropertySlim<GamepadButtons> Buttons { get; } = new(GamepadButtons.None);

        /// <summary>
        /// Left stick position
        /// </summary>
        public ReactivePropertySlim<Vector> LeftStickPosition { get; } = new();

        /// <summary>
        /// Right stick position
        /// </summary>
        public ReactivePropertySlim<Vector> RightStickPosition { get; } = new();

        /// <summary>
        /// Stores the most recently calculated smoothed position for the left stick.
        /// </summary>
        private Vector _leftSmoothedPosition;

        /// <summary>
        /// Stores the most recently calculated smoothed position for the right stick.
        /// </summary>
        private Vector _rightSmoothedPosition;

        /// <summary>
        /// Dead zone definition
        /// </summary>
        private const double _deadZoneFactor = 0.05;

        /// <summary>
        /// Represents the smoothing factor used in calculations that require exponential smoothing.
        /// </summary>
        /// <remarks>This constant determines the weight given to new data points versus historical data
        /// in smoothing algorithms. A lower value results in smoother output but slower response to changes.</remarks>
        private const double _smoothingFactor = 0.2;

        /// <summary>
        /// Disposables
        /// </summary>
        private readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gamepadInputService">Gamepad input service</param>
        public InputService(
            IGamepadInputService gamepadInputService
        )
        {
            gamepadInputService.CurrentReading.Subscribe((reading) =>
            {
                Buttons.Value = reading.Buttons;

                _leftSmoothedPosition = AdjustPosition(
                    new Vector(reading.LeftThumbstickX, reading.LeftThumbstickY),
                    _leftSmoothedPosition
                );
                _rightSmoothedPosition = AdjustPosition(
                    new Vector(reading.RightThumbstickX, reading.RightThumbstickY),
                    _rightSmoothedPosition
                );

                LeftStickPosition.Value = _leftSmoothedPosition;
                RightStickPosition.Value = _rightSmoothedPosition;
            })
            .AddTo(_disposables);
        }

        /// <summary>
        /// Dead zone check and smoothing
        /// </summary>
        /// <param name="currentPosition">Current stick position</param>
        /// <param name="lastPosition">Last stick position</param>
        /// <returns>Adjusted stick position</returns>
        private Vector AdjustPosition(
            Vector currentPosition,
            Vector lastPosition
        )
        {
            var distance = Math.Sqrt(
                Math.Pow(currentPosition.X, 2.0)
                + Math.Pow(currentPosition.Y, 2.0)
            );

            if (distance < _deadZoneFactor)
            {
                return new Vector();
            }
            else
            {
                return new Vector(
                    lastPosition.X * (1.0 - _smoothingFactor) + currentPosition.X * _smoothingFactor,
                    lastPosition.Y * (1.0 - _smoothingFactor) + currentPosition.Y * _smoothingFactor
                );
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

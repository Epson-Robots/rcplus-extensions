// -----------------------------------------------------------------------
// <copyright file="IGamepadInputService.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using Reactive.Bindings;
    using Windows.Gaming.Input;

    /// <summary>
    /// Interface of gamepad input service
    /// </summary>
    public interface IGamepadInputService
    {
        /// <summary>
        /// Property for current reading
        /// </summary>
        public IReadOnlyReactiveProperty<GamepadReading> CurrentReading { get; }

        /// <summary>
        /// Set target gamepad
        /// </summary>
        /// <param name="gamepad">Gamepad object</param>
        public void SetGamepad(
            Gamepad gamepad
        );

        /// <summary>
        /// Start service
        /// </summary>
        public void Start();

        /// <summary>
        /// Stop service
        /// </summary>
        public void Stop();
    }
}

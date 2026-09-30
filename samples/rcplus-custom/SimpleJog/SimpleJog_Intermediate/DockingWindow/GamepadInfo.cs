// -----------------------------------------------------------------------
// <copyright file="GamepadInfo.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using Windows.Gaming.Input;

    /// <summary>
    /// Gamepad information
    /// </summary>
    public class GamepadInfo
    {
        /// <summary>
        /// Gamepad object
        /// </summary>
        public Gamepad Gamepad { get; }

        /// <summary>
        /// Gamepad number
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Gamepad name
        /// </summary>
        public string Name => $"Gamepad #{Number}";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gamepad">Gamepad object</param>
        /// <param name="number">Gamepad number</param>
        public GamepadInfo(
            Gamepad gamepad,
            int number
        )
        {
            Gamepad = gamepad;
            Number = number;
        }
    }
}

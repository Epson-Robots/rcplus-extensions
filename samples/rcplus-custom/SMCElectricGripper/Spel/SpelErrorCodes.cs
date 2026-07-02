// -----------------------------------------------------------------------
// <copyright file="SpelErrorCodes.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SMCElectricGripper.Spel
{
    /// <summary>
    /// Error codes returned by the SMC LEHR SPEL library functions.
    /// </summary>
    internal enum SpelErrorCode
    {
        /// <summary>
        /// No error.
        /// The library function completed successfully.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Invalid argument.
        /// An argument value is out of range or invalid.
        /// </summary>
        ArgumentError = 1,

        /// <summary>
        /// COM port open failure.
        /// The specified COM port could not be opened.
        /// </summary>
        ComOpenError = 2,

        /// <summary>
        /// Communication error.
        /// The gripper did not respond or communication failed.
        /// </summary>
        CommunicationError = 3,
    }
}

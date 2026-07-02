// -----------------------------------------------------------------------
// <copyright file="OperationMode.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SMCElectricGripper.Models
{
    /// <summary>
    /// Defines operation modes for the gripper.
    /// Numeric values are aligned with SPEL library specifications.
    /// </summary>
    public enum OperationMode
    {
        /// <summary>
        /// Positioning mode.
        /// Corresponds to SPEL value 0.
        /// </summary>
        Positioning = 0,

        /// <summary>
        /// Gripping mode.
        /// Corresponds to SPEL value 2.
        /// </summary>
        Gripping = 2,
    }
}

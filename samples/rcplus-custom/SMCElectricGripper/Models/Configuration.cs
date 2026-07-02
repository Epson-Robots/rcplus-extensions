// -----------------------------------------------------------------------
// <copyright file="Configuration.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SMCElectricGripper.Models
{
    /// <summary>
    /// Persistent configuration data for gripper.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The COM port number that was used most recently
        /// </summary>
        public int LastUsedComPort { get; set; }
    }
}

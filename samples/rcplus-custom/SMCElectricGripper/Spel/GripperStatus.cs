// -----------------------------------------------------------------------
// <copyright file="GripperStatus.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SMCElectricGripper.Spel
{
    /// <summary>
    /// Gripper status.
    /// </summary>
    internal enum GripperStatus
    {
        /// <summary>
        /// Normal state.
        /// Includes cases where the gripper is not connected
        /// or communication is not established.
        /// </summary>
        None = 0,

        // -----------------------------
        // Alarm statuses
        // -----------------------------

        /// <summary>
        /// Alarm: Overload alarm.
        /// Triggered when an overload condition persists
        /// for a specified period of time.
        /// </summary>
        OverLoadAlarm = 100,

        /// <summary>
        /// Alarm: Overcurrent alarm.
        /// Triggered when the motor current exceeds
        /// the rated value.
        /// </summary>
        OverCurrentAlarm = 101,

        /// <summary>
        /// Alarm: Overtemperature alarm.
        /// Triggered when the internal motor temperature
        /// exceeds the allowable limit.
        /// </summary>
        OverTemperatureAlarm = 102,

        /// <summary>
        /// Alarm: Overvoltage alarm.
        /// Triggered when the input voltage exceeds 30V.
        /// </summary>
        OverVoltageAlarm = 103,

        /// <summary>
        /// Alarm: Undervoltage alarm.
        /// Triggered when the input voltage drops below 18V.
        /// </summary>
        UnderVoltageAlarm = 104,

        /// <summary>
        /// Alarm: Position deviation overflow alarm.
        /// Triggered when the position deviation exceeds
        /// the allowable threshold.
        /// </summary>
        OverFlowAlarm = 105,

        // -----------------------------
        // Warning statuses
        // -----------------------------

        /// <summary>
        /// Warning: Gripper failed to grip the workpiece.
        /// Triggered when the gripper fails to hold the workpiece.
        /// </summary>
        GripperFailedWarning = 200,

        /// <summary>
        /// Warning: Workpiece lost.
        /// Triggered when the workpiece is dropped
        /// after gripping.
        /// </summary>
        WorkpieceLostWarning = 201,

        /// <summary>
        /// Warning: Overload warning.
        /// Triggered when the load exceeds the specified threshold
        /// during positioning.
        /// </summary>
        OverLoadWarning = 202,

        /// <summary>
        /// Warning: Temperature warning.
        /// Triggered when the internal temperature exceeds
        /// the warning threshold.
        /// </summary>
        TemperatureWarning = 203,
    }
}

// -----------------------------------------------------------------------
// <copyright file="GripperStatusTextProvider.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SMCElectricGripper.Spel
{
    /// <summary>
    /// Provides display text for gripper status codes.
    /// The status codes are defined in <see cref="GripperStatus"/>
    /// </summary>
    internal static class GripperStatusTextProvider
    {
        /// <summary>
        /// Converts a gripper status code to an display text.
        /// </summary>
        /// <param name="status">
        /// Gripper status code defined in <see cref="GripperStatus"/>.
        /// </param>
        /// <returns>
        /// Display text corresponding to the specified status.
        /// </returns>
        public static string ToDisplayText(GripperStatus status)
        {
            return status switch
            {
                GripperStatus.None => "No Alarm",
                GripperStatus.OverLoadAlarm => "OverLoad Alarm",
                GripperStatus.OverCurrentAlarm => "OverCurrent Alarm",
                GripperStatus.OverTemperatureAlarm => "OverTemperature Alarm",
                GripperStatus.OverVoltageAlarm => "OverVoltage Alarm",
                GripperStatus.UnderVoltageAlarm => "UnderVoltage Alarm",
                GripperStatus.OverFlowAlarm => "OverFlow Alarm",
                GripperStatus.GripperFailedWarning => "Gripper failed Warning",
                GripperStatus.WorkpieceLostWarning => "Workpiece lost Warning",
                GripperStatus.OverLoadWarning => "OverLoad Warning",
                GripperStatus.TemperatureWarning => "Temperature Warning",
                _ => $"Unknown ({(int)status})",
            };
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="LogItem.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleJog.DockingWindow
{
    using static Epson.RoboticsShared.ExtensionsAPI.IRCXRobotManagerAPI;

    /// <summary>
    /// Teaching log list box item
    /// </summary>
    public class LogItem
    {
        /// <summary>
        /// Point number
        /// </summary>
        public int PointNumber { get; }

        /// <summary>
        /// Point position
        /// </summary>
        public IDictionary<RCXJogCartesianAxis, double>? WorldPosition { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (WorldPosition == null)
            {
                return $"P{PointNumber}";
            }
            else
            {
                var x = WorldPosition[RCXJogCartesianAxis.X];
                var y = WorldPosition[RCXJogCartesianAxis.Y];
                var z = WorldPosition[RCXJogCartesianAxis.Z];

                return $"P{PointNumber}  X: {x:f2}, Y: {y:f2}, Z: {z:f2}";
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pointNumber">Point number</param>
        /// <param name="worldPosition">Point position</param>
        public LogItem(
            int pointNumber,
            IDictionary<RCXJogCartesianAxis, double>? worldPosition
        )
        {
            PointNumber = pointNumber;
            WorldPosition = worldPosition;
        }
    }
}

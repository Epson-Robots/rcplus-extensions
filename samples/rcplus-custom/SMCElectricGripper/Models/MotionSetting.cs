// -----------------------------------------------------------------------
// <copyright file="MotionSetting.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SMCElectricGripper.Models
{
    /// <summary>
    /// Represents the motion parameters of the gripper recipe.
    /// </summary>
    public sealed class MotionSetting
    {
        /// <summary>
        /// Recipe number.
        /// </summary>
        public int RecipeNo { get; set; }

        /// <summary>
        /// Operation mode (Positioning or Gripping).
        /// </summary>
        public OperationMode Mode { get; set; }

        /// <summary>
        /// Position in millimeters.
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// Speed in millimeters per second.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Gripping force in newtons.
        /// </summary>
        public double Force { get; set; }
    }
}

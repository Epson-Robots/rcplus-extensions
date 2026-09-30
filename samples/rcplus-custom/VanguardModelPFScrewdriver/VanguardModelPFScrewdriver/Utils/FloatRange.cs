// -----------------------------------------------------------------------
// <copyright file="FloatRange.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Represents an inclusive range of <see cref="float"/> values.
    /// </summary>
    public class FloatRange
    {
        /// <summary>
        /// Gets or sets the lower bound of the range (inclusive).
        /// </summary>
        public float Min { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the range (inclusive).
        /// </summary>
        public float Max { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatRange"/> class.
        /// </summary>
        /// <param name="min">The lower bound of the range. Defaults to <see cref="float.MinValue"/>.</param>
        /// <param name="max">The upper bound of the range. Defaults to <see cref="float.MaxValue"/>.</param>
        public FloatRange(
            float min = float.MinValue,
            float max = float.MaxValue
        )
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Determines whether the specified value falls within the range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns><c>true</c> if the value is between <see cref="Min"/> and <see cref="Max"/> inclusive; otherwise, <c>false</c>.</returns>
        public bool IsInRange(
            float value
        )
        {
            // Both bounds are treated as inclusive.
            return (Min <= value && value <= Max);
        }
    }
}

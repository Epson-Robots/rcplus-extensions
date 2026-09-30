// -----------------------------------------------------------------------
// <copyright file="IValidator.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VanguardModelPFScrewdriver.Utils
{
    /// <summary>
    /// Validates the values entered into the UI and keeps track of the targets
    /// that currently hold an invalid value.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Gets a value indicating whether at least one target is currently invalid.
        /// </summary>
        public bool HasInvalidTarget { get; }

        /// <summary>
        /// Registers or clears the invalid state of the specified target.
        /// </summary>
        /// <param name="target">The target the state belongs to, typically a property or a control.</param>
        /// <param name="isValid"><c>true</c> when the target holds a valid value; otherwise, <c>false</c>.</param>
        public void SetValid(
            object target,
            bool isValid
        );

        /// <summary>
        /// Validates the value of the specified target.
        /// </summary>
        /// <param name="target">The target the value belongs to.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>An error message when the value is invalid; otherwise, <c>null</c>.</returns>
        public string? Validate(
            object target,
            object value
        );
    }
}

// -----------------------------------------------------------------------
// <copyright file="IProgressReporter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VanguardModelPFScrewdriver.Dialogs.Progress
{
    /// <summary>
    /// Provides a way to report the state of a long-running operation to a progress UI
    /// and to observe cancellation requests raised from that UI.
    /// </summary>
    public interface IProgressReporter
    {
        /// <summary>
        /// Gets the token that is signaled when the user cancels the operation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Sets the message displayed to the user.
        /// </summary>
        /// <param name="message">The text describing the current step of the operation.</param>
        public void SetMessage(
            string message
        );

        /// <summary>
        /// Sets whether the progress is displayed in indeterminate (marquee) mode.
        /// </summary>
        /// <param name="isIndeterminate">
        /// <see langword="true"/> to show indeterminate progress; otherwise, <see langword="false"/>.
        /// </param>
        public void SetIsIndeterminate(
            bool isIndeterminate
        );

        /// <summary>
        /// Sets the lower bound of the progress range.
        /// </summary>
        /// <param name="minimum">The minimum progress value.</param>
        public void SetMinimum(
            double minimum
        );

        /// <summary>
        /// Sets the upper bound of the progress range.
        /// </summary>
        /// <param name="maximum">The maximum progress value.</param>
        public void SetMaximum(
            double maximum
        );

        /// <summary>
        /// Sets the current progress value.
        /// </summary>
        /// <param name="value">
        /// The progress value, expected to be between the configured minimum and maximum.
        /// </param>
        public void SetValue(
            double value
        );

        /// <summary>
        /// Sets whether the user is allowed to cancel the operation.
        /// </summary>
        /// <param name="cancellable">
        /// <see langword="true"/> to enable cancellation; otherwise, <see langword="false"/>.
        /// </param>
        public void SetCancellable(
            bool cancellable
        );
    }
}

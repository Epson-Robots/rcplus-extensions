// -----------------------------------------------------------------------
// <copyright file="ProgressReporter.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;

namespace VanguardModelPFScrewdriver.Dialogs.Progress
{
    /// <summary>
    /// Default <see cref="IProgressReporter"/> implementation that updates a
    /// <see cref="ProgressWindow"/> from any thread by marshaling calls to the UI dispatcher.
    /// </summary>
    public class ProgressReporter : IProgressReporter
    {
        /// <summary>
        /// The progress window that is being updated.
        /// </summary>
        private readonly ProgressWindow _window;

        /// <summary>
        /// The view model bound to <see cref="_window"/>.
        /// </summary>
        private readonly ProgressWindowViewModel _viewModel;

        /// <summary>
        /// Gets the token that is signaled when the user cancels the operation.
        /// </summary>
        public CancellationToken CancellationToken => _viewModel.CancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressReporter"/> class.
        /// </summary>
        /// <param name="window">The progress window whose data context is a <see cref="ProgressWindowViewModel"/>.</param>
        public ProgressReporter(
            ProgressWindow window
        )
        {
            _window = window;
            _viewModel = (ProgressWindowViewModel)window.DataContext;
        }

        /// <summary>
        /// Sets the message displayed to the user.
        /// </summary>
        /// <param name="message">The text describing the current step of the operation.</param>
        public void SetMessage(
            string message
        )
        {
            // Marshal the update to the UI thread because callers may run on a worker thread.
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.Message.Value = message;
            });
        }

        /// <summary>
        /// Sets whether the progress is displayed in indeterminate (marquee) mode.
        /// </summary>
        /// <param name="isIndeterminate">
        /// <see langword="true"/> to show indeterminate progress; otherwise, <see langword="false"/>.
        /// </param>
        public void SetIsIndeterminate(
            bool isIndeterminate
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.IsIndeterminate.Value = isIndeterminate;
            });
        }

        /// <summary>
        /// Sets the lower bound of the progress range.
        /// </summary>
        /// <param name="minimum">The minimum progress value.</param>
        public void SetMinimum(
            double minimum
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.Minimum.Value = minimum;
            });
        }

        /// <summary>
        /// Sets the upper bound of the progress range.
        /// </summary>
        /// <param name="maximum">The maximum progress value.</param>
        public void SetMaximum(
            double maximum
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.Maximum.Value = maximum;
            });
        }

        /// <summary>
        /// Sets the current progress value.
        /// </summary>
        /// <param name="value">The progress value between the configured minimum and maximum.</param>
        public void SetValue(
            double value
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.Value.Value = value;
            });
        }

        /// <summary>
        /// Sets whether the user is allowed to cancel the operation.
        /// </summary>
        /// <param name="cancellable">
        /// <see langword="true"/> to enable cancellation; otherwise, <see langword="false"/>.
        /// </param>
        public void SetCancellable(
            bool cancellable
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.Cancellable.Value = cancellable;
            });
        }
    }
}

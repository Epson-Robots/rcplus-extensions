// -----------------------------------------------------------------------
// <copyright file="ProgressWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;

namespace VanguardModelPFScrewdriver.Dialogs.Progress
{
    /// <summary>
    /// View model for <see cref="ProgressWindow"/> that exposes the bindable progress state
    /// and the cancellation mechanism of the running operation.
    /// </summary>
    public class ProgressWindowViewModel
    {
        /// <summary>
        /// Gets the caption provider used to localize the texts displayed in the window.
        /// </summary>
        public static IRCXCaptionGetter Captions { get; } = Main.Captions!;

        /// <summary>
        /// Gets the message describing the current step of the operation.
        /// </summary>
        public ReactivePropertySlim<string> Message { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the progress bar runs in indeterminate (marquee) mode.
        /// </summary>
        public ReactivePropertySlim<bool> IsIndeterminate { get; set; } = new(false);

        /// <summary>
        /// Gets or sets the lower bound of the progress range.
        /// </summary>
        public ReactivePropertySlim<double> Minimum { get; set; } = new(0);

        /// <summary>
        /// Gets or sets the upper bound of the progress range.
        /// </summary>
        public ReactivePropertySlim<double> Maximum { get; set; } = new(100);

        /// <summary>
        /// Gets the current progress value.
        /// </summary>
        public ReactivePropertySlim<double> Value { get; } = new(0);

        /// <summary>
        /// Gets a value indicating whether the user is allowed to cancel the operation.
        /// </summary>
        public ReactivePropertySlim<bool> Cancellable { get; } = new(false);

        /// <summary>
        /// Gets the command bound to the cancel button. It is enabled only while <see cref="Cancellable"/> is true.
        /// </summary>
        public ReactiveCommand CancelCommand { get; }

        /// <summary>
        /// Gets the token that is signaled when the user cancels the operation.
        /// </summary>
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// The source used to signal cancellation to the running operation.
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>
        /// Holds the reactive subscriptions owned by this view model.
        /// </summary>
        private readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressWindowViewModel"/> class.
        /// </summary>
        public ProgressWindowViewModel()
        {
            // Derive the command's executability from Cancellable and request cancellation when invoked.
            CancelCommand = Cancellable
                .ToReactiveCommand()
                .WithSubscribe(_cancellationTokenSource.Cancel)
                .AddTo(_disposables);
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="DialogViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows;
using VanguardModelPFScrewdriver.ModelPF;

namespace VanguardModelPFScrewdriver.Dialogs
{
    /// <summary>
    /// Base class for the view models of the plug-in dialogs.
    /// Provides OK/Cancel commands, dialog close handling and shared context data.
    /// </summary>
    public class DialogViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the caption (localized string) provider supplied by the host application.
        /// </summary>
        public static IRCXCaptionGetter Captions { get; } = Main.Captions!;

        /// <summary>
        /// Gets or sets the result used to close the dialog.
        /// Setting this value is bound to the window and closes it.
        /// </summary>
        public bool? CloseResult
        {
            get => _closeResult;
            set
            {
                _closeResult = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CloseResult)));
            }
        }

        /// <summary>
        /// Gets the command executed when the OK button is pressed.
        /// </summary>
        public AsyncReactiveCommand OKCommand { get; }

        /// <summary>
        /// Gets the flag indicating whether the OK command is currently enabled.
        /// </summary>
        public ReactivePropertySlim<bool> CanOK { get; } = new(true);

        /// <summary>
        /// Gets the command executed when the Cancel button is pressed.
        /// </summary>
        public ReactiveCommand CancelCommand { get; }

        /// <summary>
        /// Gets the flag indicating whether the Cancel command is currently enabled.
        /// </summary>
        public ReactivePropertySlim<bool> CanCancel { get; } = new(true);

        /// <summary>
        /// Gets the command executed when the Help button is pressed.
        /// Displays the plug-in help topic.
        /// </summary>
        public ReactiveCommand ShowHelpCommand { get; } = new();

        /// <summary>
        /// Gets or sets the docking window used as the owner of the dialogs.
        /// When <c>null</c>, the application main window is used instead.
        /// </summary>
        public static Window? DockingWindow { get; set; }

        /// <summary>
        /// Reference to the window hosting this view model.
        /// </summary>
        protected Window? _window;

        /// <summary>
        /// PRO-FUSE definition data shared with the dialog.
        /// </summary>
        protected ModelPFData? _proFuseData;

        /// <summary>
        /// Client used to communicate with the PRO-FUSE controller.
        /// </summary>
        protected ModelPFClient? _proFuseClient;

        /// <summary>
        /// Parameter dictionary exchanged between the dialog and its caller.
        /// </summary>
        protected IDictionary<string, object?>? _parameters;

        /// <summary>
        /// Container holding the subscriptions to be disposed with this view model.
        /// </summary>
        protected readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Backing store for <see cref="CloseResult"/>.
        /// </summary>
        private bool? _closeResult;

        /// <summary>
        /// Handles the OK action. Override to add validation or custom processing.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnOK()
        {
            CloseResult = true;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the Cancel action. Override to add custom processing.
        /// </summary>
        protected virtual void OnCancel()
        {
            CloseResult = false;
        }

        /// <summary>
        /// Performs initialization before the dialog is shown.
        /// </summary>
        protected virtual void Setup()
        {
        }

        /// <summary>
        /// Performs cleanup after the dialog has been closed.
        /// </summary>
        protected virtual void Cleanup()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewModel"/> class
        /// and wires up the OK and Cancel commands.
        /// </summary>
        public DialogViewModel()
        {
            OKCommand = CanOK
                .ToAsyncReactiveCommand()
                .WithSubscribe(OnOK)
                .AddTo(_disposables);

            CancelCommand = CanCancel
                .ToReactiveCommand()
                .WithSubscribe(OnCancel)
                .AddTo(_disposables);

            ShowHelpCommand
                .Subscribe(Main.ShowHelp)
                .AddTo(_disposables);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _disposables.Dispose();
        }

        /// <summary>
        /// Creates and shows the specified dialog window modally.
        /// </summary>
        /// <typeparam name="T">Type of the window to display.</typeparam>
        /// <param name="proFuseData">PRO-FUSE definition data passed to the dialog.</param>
        /// <param name="proFuseClient">Client used to communicate with the PRO-FUSE controller.</param>
        /// <param name="parameters">Parameter dictionary exchanged with the caller.</param>
        /// <returns><c>true</c> when closed with OK, <c>false</c> when cancelled, <c>null</c> otherwise.</returns>
        public static bool? ShowDialog<T>(
            ModelPFData? proFuseData = null,
            ModelPFClient? proFuseClient = null,
            IDictionary<string, object?>? parameters = null
        )
        where T : Window, new()
        {
            T dialog = new()
            {
                Owner = DockingWindow ?? Application.Current.MainWindow,
            };

            // The window must provide a DialogViewModel as its DataContext.
            if (dialog.DataContext is not DialogViewModel viewModel)
            {
                return null;
            }
            else
            {
                viewModel._window = dialog;
                viewModel._proFuseData = proFuseData;
                viewModel._proFuseClient = proFuseClient;
                viewModel._parameters = parameters;

                viewModel.Setup();

                var result = dialog.ShowDialog();

                viewModel.Cleanup();

                return result;
            }
        }
    }
}

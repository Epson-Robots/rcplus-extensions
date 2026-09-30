// -----------------------------------------------------------------------
// <copyright file="ProgramMenuItemViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// View model of a single entry of the program context menu.
    /// An entry can be a command item, a group title or a separator.
    /// </summary>
    public class ProgramMenuItemViewModel : IDisposable
    {
        /// <summary>
        /// Identifies the action invoked by a menu item.
        /// </summary>
        public enum CommandKind
        {
            /// <summary>No action (group title or separator).</summary>
            None,

            /// <summary>Download all programs to the screwdriver.</summary>
            DownloadAllPrograms,

            /// <summary>Upload all programs from the screwdriver.</summary>
            UploadAllPrograms,

            /// <summary>Download the selected program to the screwdriver.</summary>
            DownloadSelectedProgram,

            /// <summary>Upload the selected program from the screwdriver.</summary>
            UploadSelectedProgram,

            /// <summary>Import programs from a file.</summary>
            ImportPrograms,

            /// <summary>Export programs to a file.</summary>
            ExportPrograms,
        }

        /// <summary>
        /// Gets the caption getter used to localize menu titles.
        /// </summary>
        public static IRCXCaptionGetter Captions { get; } = Main.Captions!;

        /// <summary>
        /// Gets or sets the handler invoked when any menu item is executed.
        /// </summary>
        public static Func<ProgramMenuItemViewModel, Task>? Callback { get; set; } = null;

        /// <summary>
        /// Gets a value indicating whether this item is a separator.
        /// </summary>
        public bool IsSeparator { get; } = false;

        /// <summary>
        /// Gets the localized title of this item.
        /// </summary>
        public ReactivePropertySlim<string> Title { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether this item can be executed.
        /// </summary>
        public ReactivePropertySlim<bool> IsEnabled { get; set; } = new(false);

        /// <summary>
        /// Gets a value indicating whether this item is a non-clickable group title.
        /// </summary>
        public bool IsGroupTitle { get; } = false;

        /// <summary>
        /// Gets the command executed when this item is clicked, or null if not clickable.
        /// </summary>
        public ICommand? Command { get; }

        /// <summary>
        /// Gets the action assigned to this item.
        /// </summary>
        public CommandKind CommandKindValue { get; } = CommandKind.None;

        // Subscriptions released on disposal.
        private readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Initializes a new instance used as a localized title item.
        /// </summary>
        /// <param name="captionId">Caption ID of the title.</param>
        /// <param name="isGroupTitle">True to create a non-clickable group title.</param>
        public ProgramMenuItemViewModel(
            int captionId,
            bool isGroupTitle = false
        )
        {
            Title.Value = Captions[captionId];

            // Re-apply the caption whenever the UI language is switched.
            Captions.LanguageChanged += () =>
            {
                Title.Value = Captions[captionId];
            };

            IsGroupTitle = isGroupTitle;
        }

        /// <summary>
        /// Initializes a new instance used as a separator.
        /// </summary>
        public ProgramMenuItemViewModel()
        {
            IsSeparator = true;
        }

        /// <summary>
        /// Initializes a new instance used as a command item.
        /// </summary>
        /// <param name="captionId">Caption ID of the title.</param>
        /// <param name="commandKind">Action assigned to this item.</param>
        /// <param name="isEnabled">Initial enabled state.</param>
        public ProgramMenuItemViewModel(
            int captionId,
            CommandKind commandKind,
            bool isEnabled = false
        )
        : this(captionId)
        {
            CommandKindValue = commandKind;

            if (commandKind != CommandKind.None)
            {
                // The command availability is driven by IsEnabled.
                Command = IsEnabled
                    .ToAsyncReactiveCommand()
                    .WithSubscribe(() => Callback?.Invoke(this))
                    .AddTo(_disposables);
                IsEnabled.Value = isEnabled;
            }
        }

        /// <summary>
        /// Releases the subscriptions held by this view model.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _disposables.Dispose();
        }
    }
}

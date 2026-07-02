// -----------------------------------------------------------------------
// <copyright file="DockingWindowContentViewModel.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Media;
using Epson.RoboticsShared.ExtensionsAPI;
using static SimpleJog.Constants;

namespace SimpleJog.DockingWindow
{
    /// <summary>
    /// Extension : Docking Window (Typically Common Part)
    /// </summary>
    internal partial class DockingWindowContentViewModel : IRCXUserControlViewModel, IDisposable
    {
        /// <inheritdoc />
        public string Id => Main.CommonId;

        /// <inheritdoc />
        public string ViewModelId => $"{Id}.DockingWindow";

        /// <inheritdoc />
        public bool KeepOpenWhenProjectClosing => true;

        /// <summary>
        /// Captions
        /// </summary>
        public static IRCXCaptionGetter Captions { get; } = Main.Captions!;

        /// <inheritdoc />
        public RCXCaption WindowCaption { get; set; } = new(Main.CommonId, Caption.WindowTitle);

        /// <inheritdoc />
        public ImageSource? WindowIcon { get; set; } = Main.CommonIcon;

        /// <summary>
        /// Related disposables
        /// </summary>
        private readonly CompositeDisposable _disposables = [];

        /// <inheritdoc />
        public Task<bool> CloseAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SaveAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public void Reload()
        {
        }

        /// <inheritdoc />
        public void Copy()
        {
        }

        /// <inheritdoc />
        public void Cut()
        {
        }

        /// <inheritdoc />
        public void Paste()
        {
        }

        /// <inheritdoc />
        public void SelectAll()
        {
        }

        /// <inheritdoc />
        public void Undo()
        {
        }

        /// <inheritdoc />
        public void Redo()
        {
        }

        /// <inheritdoc />
        public void ShowHelp()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// Show docking window
        /// </summary>
        /// <returns>Task</returns>
        public static async Task Show()
        {
            DockingWindowContent control = new();

            if (control.DataContext is DockingWindowContentViewModel controlViewModel)
            {
                await Main.GetAPI<IRCXWindowAPI>().ShowDockingWindowAsync(controlViewModel, control);
            }
        }
    }
}

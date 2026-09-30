// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using System.IO;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static VanguardModelPFScrewdriver.Constants;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Extension : Project File Editor (Typically Common Part)
    /// </summary>
    internal partial class ProjectFileEditorViewModel : IRCXUserControlViewModel, IDisposable
    {
        /// <inheritdoc />
        public string Id => Main.CommonId;

        /// <inheritdoc />
        public string ViewModelId => $"{Id}.ProjectFileEditor.{FileName}";

        /// <inheritdoc />
        public bool KeepOpenWhenProjectClosing => false;

        /// <summary>
        /// Captions
        /// </summary>
        public static IRCXCaptionGetter Captions { get; } = Main.Captions!;

        /// <inheritdoc />
        public RCXCaption WindowCaption { get; set; } = new(string.Empty);

        /// <inheritdoc />
        public ImageSource? WindowIcon { get; set; } = Main.CommonIcon;

        /// <summary>
        /// File name of the project file
        /// </summary>
        internal string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Related disposables
        /// </summary>
        private readonly CompositeDisposable _disposables = [];

        /// <summary>
        /// Path name of the file or null if the project is not opened
        /// </summary>
        private string? FilePath
        {
            get
            {
                if (string.IsNullOrEmpty(FileName))
                {
                    return null;
                }
                if (_projectAPI.ProjectFolder == null)
                {
                    return null;
                }

                return Path.Combine(_projectAPI.ProjectFolder, FileName);
            }
        }

        /// <inheritdoc />
        public Task<bool> CloseAsync()
        {
            if (_proFuseData.IsDirty)
            {
                if (!CanSaveFile.Value)
                {
                    _ = _windowAPI.ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.CannotClose),
                        ButtonType.Yes_No_Cancel,
                        IconType.Warning,
                        Application.Current.MainWindow
                    );

                    return Task.FromResult(false);
                }

                var response = _windowAPI.ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, Caption.QueryClose),
                    ButtonType.Yes_No_Cancel,
                    IconType.Warning,
                    Application.Current.MainWindow
                );

                if (response == ResponseType.Cancel)
                {
                    return Task.FromResult(false);
                }
                else if (response == ResponseType.Yes)
                {
                    SaveContent();
                }
            }

            _logFileChangedWatcher?.Dispose();
            _logFileDeletedWatcher?.Dispose();

            _proFuseClient?.Disconnect();

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SaveAsync()
        {
            if (_proFuseData.IsDirty && !CanSaveFile.Value)
            {
                _ = _windowAPI.ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, Caption.CannotSave),
                    ButtonType.Yes_No_Cancel,
                    IconType.Warning,
                    Application.Current.MainWindow
                );

                return Task.FromResult(false);
            }

            SaveContent();

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public void Reload()
        {
            LoadContent();
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
            Main.ShowHelp();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}

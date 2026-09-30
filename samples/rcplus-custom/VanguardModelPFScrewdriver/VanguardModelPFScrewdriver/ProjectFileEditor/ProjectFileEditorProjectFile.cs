// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorProjectFile.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VanguardModelPFScrewdriver.ModelPF;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static VanguardModelPFScrewdriver.Constants;
using V2 = Epson.RoboticsShared.ExtensionsAPI.V2;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Extension : Project File
    /// </summary>
    [Export(typeof(IRCXProjectFileProvider))]
    public partial class ProjectFileEditorProjectFile : V2.IRCXProjectFileProvider
    {
        /// <inheritdoc />
        public string Id => Main.CommonId;

        /// <inheritdoc />
        public string FileTypeName => "VanguardModelPF";

        /// <inheritdoc />
        public string Extension => ".vgdpf";

        /// <inheritdoc />
        public bool UseDefaultProjectExplorerItem => true;

        /// <inheritdoc />
        public RCXCaption ProjectExplorerRootItemCaption => new(Main.CommonId, Caption.FileCategory);

        /// <inheritdoc />
        public ImageSource? ProjectExplorerRootItemIconData => Main.CommonIcon;

        /// <inheritdoc />
        public ImageSource? FileIcon => LoadIcon("item");

        /// <inheritdoc />
        public RCXCaption FileTypeNameCaption => new(Main.CommonId, Caption.FileTypeName);

        /// <summary>
        /// Command name raised by the "Add Library" context menu item.
        /// </summary>
        private const string _addLibraryCommand = "AddLibrary";

        /// <summary>
        /// Folder (relative to this assembly) that contains the library archive.
        /// </summary>
        private const string _libraryFolder = "Libraries";

        /// <summary>
        /// Base name of the library provided by this extension.
        /// </summary>
        private const string _libraryName = "VGD_PF";

        /// <summary>
        /// Loads a PNG image from the "Resources\Images" folder next to this assembly.
        /// </summary>
        /// <param name="iconName">Icon file name without extension.</param>
        /// <returns>The loaded image, or <see langword="null"/> if the file does not exist.</returns>
        private BitmapSource? LoadIcon(
            string iconName
        )
        {
            var parentDir = Directory.GetParent(GetType().Assembly.Location)!.FullName;
            var iconPath = Path.Combine(parentDir, "Resources", "Images", $"{iconName}.png");
            if (File.Exists(iconPath))
            {
                // Convert the GDI+ bitmap into a WPF image source.
                var image = new Bitmap(iconPath);
                return Imaging.CreateBitmapSourceFromHBitmap(
                    image.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }

            return null;
        }

        /// <summary>
        /// Reports the number of definition files in the current project as usage data.
        /// </summary>
        private void ReportNumDefinitionFiles()
        {
            var projectAPI = Main.GetAPI<IRCXProjectAPI>();
            if (projectAPI.ProjectFiles != null)
            {
                // Count only the files owned by this extension.
                var count = projectAPI.ProjectFiles.Count(x => string.Equals(Path.GetExtension(x.Name), Extension, StringComparison.OrdinalIgnoreCase));

                var generalAPI = Main.GetAPI<IRCXGeneralAPI>();
                generalAPI.AddDataToCollect($"{Extension}:{count}");
            }
        }

        /// <inheritdoc />
        public void CustomizeProjectExplorerItem(
            IRCXProjectExplorerItemProvider.Item item,
            string? fileName
        )
        {
            if (fileName != null || item.ContextMenuItems == null)
            {
                item.SelectedIconData = LoadIcon("item_inv");
                return;
            }

            item.Icon = LoadIcon("category");
            item.SelectedIconData = LoadIcon("category_inv");

            // Add the "Add Library" command to the root item context menu.
            item.ContextMenuItems.Add(
                new()
                {
                    Caption = new(Main.CommonId, Caption.AddLibraryMenuItem),
                    CommandName = _addLibraryCommand,
                }
            );

            ReportNumDefinitionFiles();
        }

        /// <summary>
        /// Handles context menu commands raised from the project explorer.
        /// </summary>
        /// <param name="command">Name of the invoked command.</param>
        /// <param name="commandParameter">Optional command parameter.</param>
        /// <returns><see langword="true"/> if the command was handled; otherwise <see langword="false"/>.</returns>
        public bool HandleCommand(
            string command,
            object? commandParameter
        )
        {
            if (command != _addLibraryCommand)
            {
                return false;
            }
            else
            {
                // The caller expects a synchronous result, so wait for the async operation.
                OnAddLibraryAsync().GetAwaiter().GetResult();

                return true;
            }
        }

        /// <summary>
        /// Imports the bundled library archive and adds the library to the current project.
        /// </summary>
        private static async Task OnAddLibraryAsync()
        {
            var windowAPI = Main.GetAPI<IRCXWindowAPI>();
            int captionId;
            IconType iconType;
            var alreadyExists = false;

            // Library archive shipped with this extension.
            var libraryArchivePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                _libraryFolder,
                $"{_libraryName}.zip"
            );

            RCXResult result;

            var libraryAPI = Main.GetAPI<IRCXLibraryAPI>();
            result = await libraryAPI.ImportAsync(libraryArchivePath, true);
            if (result != RCXResult.Success)
            {
                captionId = Caption.ImportLibraryFailed;
                iconType = IconType.Error;
            }
            else
            {
                var libraryFileName = $"{_libraryName}.lib";

                // Check whether the library has already been referenced by the project.
                var addedLibraries = libraryAPI.ProjectLibraries;
                if (addedLibraries != null && addedLibraries.Any(x => string.Equals(x, libraryFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    alreadyExists = true;
                }

                result = await libraryAPI.AddToProjectAsync(libraryFileName, Main.CommonId).ConfigureAwait(true);
                if (result == RCXResult.Success)
                {
                    captionId = Caption.AddLibraryCompleted;
                    iconType = IconType.Information;
                }
                else
                {
                    captionId = Caption.AddLibraryFailed;
                    iconType = IconType.Error;
                }
            }

            // Suppress the notification when the library was already added.
            if (!alreadyExists)
            {
                _ = windowAPI.ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, captionId),
                    ButtonType.OK,
                    iconType,
                    Application.Current.MainWindow
                );
            }
        }

        /// <inheritdoc />
        public async Task OpenAsync(
            string fileName
        )
        {
            // Open project file editor window
            ProjectFileEditor editorControl = new();

            if (editorControl.DataContext is ProjectFileEditorViewModel editorViewModel)
            {
                // Bind the target file and load its content before showing the window.
                editorViewModel.FileName = fileName;
                editorViewModel.LoadContent();
                await Main.GetAPI<V2.IRCXWindowAPI>().ShowDockingWindowWithJogAsync(editorViewModel, editorControl);
            }
        }

        /// <inheritdoc />
        public void WriteInitialContent(
            FileStream fileStream
        )
        {
            try
            {
                // Write the default definition (tool kind S) into a newly created file.
                ModelPFData content = new();
                content.Adjust(ModelPFData.SysInfo.ToolKind.S, true);
                content.WriteToFile(fileStream);
            }
            catch (Exception)
            {
                ErrorReporter.Message(Caption.CreateDefFileFailed);
            }
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="MainMenuItem.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Epson.RoboticsShared.ExtensionsAPI;
using SMCElectricGripper.DockingWindow;
using SMCElectricGripper.Spel;
using static SMCElectricGripper.Constants;

namespace SMCElectricGripper
{
    /// <summary>
    /// Extension : Menu Item
    /// </summary>
    [Export(typeof(IRCXMainMenuItemProvider))]
    public partial class MainMenuItem : IRCXMainMenuItemProvider
    {
        /// <inheritdoc />
        public string Id => Main.CommonId;

        /// <inheritdoc />
        public string MenuItemId => $"{Id}.MainMenuItem";

        /// <inheritdoc />
        public IRCXMainMenuItemProvider.MenuItem MainMenuRootItem
        {
            get
            {
                return new IRCXMainMenuItemProvider.MenuItem
                {
                    Caption = new RCXCaption(Main.CommonId, Caption.ParentMainMenu),
                    Icon = Main.CommonIcon,
                    CommandName = Caption.ParentMainMenu.ToString(),
                    ToolTip = new RCXCaption(Main.CommonId, Caption.ParentMainMenu),
                    Children =
                    [
                        new()
                        {
                            Caption = new RCXCaption(Main.CommonId, Caption.SettingsMenu),
                            Icon = Main.CommonIcon,
                            CommandName = Caption.SettingsMenu.ToString(),
                            ToolTip = new RCXCaption(Main.CommonId, Caption.SettingsMenu),
                        },
                        new()
                        {
                            Caption = new RCXCaption(Main.CommonId, Caption.ImportLibraryMenu),
                            Icon = Main.CommonIcon,
                            CommandName = Caption.ImportLibraryMenu.ToString(),
                            ToolTip = new RCXCaption(Main.CommonId, Caption.ImportLibraryMenu),
                        },
                    ]
                };
            }
        }

        /// <inheritdoc />
        public IRCXMainMenuItemProvider.TopLevelMenu TopLevel => IRCXMainMenuItemProvider.TopLevelMenu.Default;

        /// <inheritdoc />
        public async Task ExecuteMainMenuItemCommandAsync(
            string commandName,
            bool fromToolBar
        )
        {
            var rcVersion = Main.GetAPI<IRCXGeneralAPI>().RCPlusVersion;
            var match = Regex.Match(rcVersion, @"^\d+(\.\d+){1,3}");
            if (!match.Success ||
                !Version.TryParse(match.Value, out var version) ||
                version < new Version(8, 1, 4, 0))
            {
                Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, Caption.UnsupportedRcPlusVersionMessage),
                    IRCXWindowAPI.ButtonType.OK,
                    IRCXWindowAPI.IconType.Error);
                return;
            }

            if (commandName == Caption.SettingsMenu.ToString())
            {
                await DockingWindowContentViewModel.Show().ConfigureAwait(false);
            }
            else if (commandName == Caption.ImportLibraryMenu.ToString())
            {
                var libZipPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                    "Libraries",
                    "SMC_LEHR.zip"
                );

                var libraryAPI = Main.GetAPI<IRCXLibraryAPI>();
                var result = await libraryAPI.ImportAsync(libZipPath, true).ConfigureAwait(true);
                if (result != RCXCommon.RCXResult.Success)
                {
                    Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                        new RCXCaption(Main.CommonId, Caption.ExtensionName),
                        new RCXCaption(Main.CommonId, Caption.LibraryImportFailedMessage),
                        IRCXWindowAPI.ButtonType.OK,
                        IRCXWindowAPI.IconType.Error);
                    return;
                }

                if (libraryAPI.ProjectLibraries != null &&
                    libraryAPI.ProjectLibraries.Any(x => string.Equals(x, SpelConstants.LEHR_LIBRARY_NAME, StringComparison.OrdinalIgnoreCase)) != true)
                {
                    result = await libraryAPI.AddToProjectAsync(SpelConstants.LEHR_LIBRARY_NAME, Main.CommonId).ConfigureAwait(true);
                    if (result != RCXCommon.RCXResult.Success)
                    {
                        Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                            new RCXCaption(Main.CommonId, Caption.ExtensionName),
                            new RCXCaption(Main.CommonId, Caption.LibraryProjectAddFailedMessage),
                            IRCXWindowAPI.ButtonType.OK,
                            IRCXWindowAPI.IconType.Error);
                        return;
                    }
                }

                Main.GetAPI<IRCXWindowAPI>().ShowMessageBox(
                    new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    new RCXCaption(Main.CommonId, Caption.LibraryImportCompletedMessage),
                    IRCXWindowAPI.ButtonType.OK,
                    IRCXWindowAPI.IconType.Information);
            }
        }
    }
}

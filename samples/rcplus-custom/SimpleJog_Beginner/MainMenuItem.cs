// -----------------------------------------------------------------------
// <copyright file="MainMenuItem.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using SimpleJog.DockingWindow;
using System.ComponentModel.Composition;
using static SimpleJog.Constants;

namespace SimpleJog
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
                    Caption = new RCXCaption(Main.CommonId, Caption.MainMenu),
                    Icon = Main.CommonIcon,
                    CommandName = "Main",
                    ToolTip = new RCXCaption(Main.CommonId, Caption.MainMenu),
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
            if (fromToolBar)
            {
                var controllerConnectionAPI = Main.GetAPI<IRCXControllerConnectionAPI>();
                if (controllerConnectionAPI?.IsOnline == false)
                {
                    _ = await controllerConnectionAPI.ConnectControllerAsync().ConfigureAwait(true);
                }
            }

            await DockingWindowContentViewModel.Show();
        }
    }
}

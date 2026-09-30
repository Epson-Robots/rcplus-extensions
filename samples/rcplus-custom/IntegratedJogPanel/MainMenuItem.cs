// -----------------------------------------------------------------------
// <copyright file="MainMenuItem.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.ComponentModel.Composition;
using Epson.RoboticsShared.ExtensionsAPI;
using IntegratedJogPanel.DockingWindow;
using static IntegratedJogPanel.Constants;

namespace IntegratedJogPanel
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
            // (Code here)

            await DockingWindowContentViewModel.Show();
        }


    }
}

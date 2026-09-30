using Epson.RoboticsShared.ExtensionsAPI;
using SpelAnalysisTool.DockingWindow;
using System.ComponentModel.Composition;
using static SpelAnalysisTool.Constants;

namespace SpelAnalysisTool
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
                    Caption = new RCXCaption(Main.CommonId, Caption.ExtensionName),
                    Icon = Main.CommonIcon,
                    CommandName = "Main",
                    ToolTip = new RCXCaption(Main.CommonId, Caption.ExtensionName),
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

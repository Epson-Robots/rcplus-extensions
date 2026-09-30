// -----------------------------------------------------------------------
// <copyright file="ProjectFileEditorViewModelConnect.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using VanguardModelPFScrewdriver.Dialogs;
using VanguardModelPFScrewdriver.Dialogs.ConnectFromPC;
using VanguardModelPFScrewdriver.Dialogs.ControllerSettings;
using VanguardModelPFScrewdriver.ModelPF;

namespace VanguardModelPFScrewdriver.ProjectFileEditor
{
    /// <summary>
    /// Part of the project file editor view model that handles
    /// connecting to / disconnecting from the screwdriver controller.
    /// </summary>
    internal partial class ProjectFileEditorViewModel
    {
        /// <summary>
        /// Caption of the "Connect / Disconnect from PC" button.
        /// </summary>
        public ReadOnlyReactivePropertySlim<IRCXLangRxCaption> FromPCCaption { get; }

        /// <summary>
        /// Reference to the currently active "Connect / Disconnect from PC" command.
        /// </summary>
        public ReadOnlyReactivePropertySlim<ReactiveCommand> FromPCCommand { get; }

        /// <summary>
        /// "Connect from PC" command.
        /// </summary>
        public ReactiveCommand ConnectFromPCCommand { get; } = new();

        /// <summary>
        /// "Disconnect from PC" command.
        /// </summary>
        public ReactiveCommand DisconnectFromPCCommand { get; } = new();

        /// <summary>
        /// "Controller settings" command.
        /// </summary>
        public AsyncReactiveCommand ControllerSettingsCommand { get; }

        // ======================================================================

        /// <summary>
        /// Opens the "Connect from PC" dialog and stores the established client.
        /// </summary>
        private void OnConnectFromPC()
        {
            if (_proFuseClient?.IsConnected != true)
            {
                Dictionary<string, object?> parameters = [];

                _ = DialogViewModel.ShowDialog<ConnectFromPCWindow>(_proFuseData, _proFuseClient, parameters);

                // The dialog returns the connected client through the parameter dictionary.
                if (
                    parameters.TryGetValue("ModelPFClient", out var value)
                    && value is ModelPFClient client
                )
                {
                    _proFuseClient = client;
                    HasCTContsConnection.Value = true;
                }
            }
        }

        /// <summary>
        /// Disconnects the PC from the screwdriver controller.
        /// </summary>
        private void OnDisconnectFromPC()
        {
            if (_proFuseClient?.IsConnected == true)
            {
                _proFuseClient.Disconnect();
                _proFuseClient = null;
                HasCTContsConnection.Value = false;
            }
        }

        /// <summary>
        /// Opens the "Controller settings" dialog, connecting to the robot controller first if required.
        /// </summary>
        private async Task OnControllerSettingsAsync()
        {
            if (_controllerConnectionAPI.IsOnline == false)
            {
                var isSuccess = await _controllerConnectionAPI.ConnectControllerAsync().ConfigureAwait(true);
                if (!isSuccess)
                {
                    // An error message has already been shown during the connection process.
                    return;
                }

                // Wait until the controller actually reports the online state.
                while (_controllerConnectionAPI.IsOnline != true)
                {
                    const int _waitMSec = 100;
                    await Task.Delay(_waitMSec).ConfigureAwait(true);
                }

                // Give the controller some extra time to become ready for requests.
                const int _moreWaitMSec = 1000;
                await Task.Delay(_moreWaitMSec).ConfigureAwait(true);
            }

            _ = DialogViewModel.ShowDialog<ControllerSettingsWindow>(_proFuseData, _proFuseClient);
        }
    }
}

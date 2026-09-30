// -----------------------------------------------------------------------
// <copyright file="ConnectFromPCWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Net;
using VanguardModelPFScrewdriver.Dialogs.Progress;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXWindowAPI;
using static VanguardModelPFScrewdriver.Constants;

namespace VanguardModelPFScrewdriver.Dialogs.ConnectFromPC
{
    /// <summary>
    /// ViewModel for the "Connect from PC" dialog.
    /// </summary>
    internal class ConnectFromPCWindowViewModel : DialogViewModel
    {
        /// <summary>
        /// Gets the IP address of the ModelPF device to connect to.
        /// </summary>
        public ReactivePropertySlim<IPAddress?> IPAddress { get; } = new(_defaultAddress);

        /// <summary>
        /// Gets the command that restores the default connection settings.
        /// </summary>
        public ReactiveCommand DefaultSettingsCommand { get; } = new();

        /// <summary>
        /// Default IP address (192.168.0.1).
        /// </summary>
        private readonly static IPAddress _defaultAddress = new([192, 168, 0, 1]);

        /// <summary>
        /// Window API used to display message boxes.
        /// </summary>
        private readonly IRCXWindowAPI _windowAPI;

        /// <summary>
        /// Restores the IP address to its default value.
        /// </summary>
        private void OnDefaultSettings()
        {
            IPAddress.Value = _defaultAddress;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Connects to the ModelPF device with the specified IP address, verifies the tool type
        /// and stores the connected client so the caller can reuse it.
        /// </remarks>
        protected override async Task OnOK()
        {
            // Create a client and try to connect to the device.
            const int _quietWaitMSec = 1000;
            ModelPFClient client = new();
            await ProgressWindow.OpenAsync(
                async (reporter) =>
                {
                    reporter.SetMessage(Captions[Caption.WaitingConnection]);
                    reporter.SetIsIndeterminate(true);
                    reporter.SetCancellable(true);

                    await client.ConnectAsync(IPAddress.Value!, reporter.CancellationToken);
                },
                _quietWaitMSec
            );

            if (client.IsConnected)
            {
                // Download the system information from the connected device.
                var isSuccess = await _proFuseData!.SysInfoData.DownloadAsync(client);
                if (!isSuccess)
                {
                    ErrorReporter.Message(Caption.SysInfoDownloadError, _window);
                }
                else
                {
                    bool isOK = true;

                    // The connected tool type differs from the one used previously.
                    // Ask the user whether the stored tool type should be updated.
                    if (_proFuseData.OtherData.LastToolType != _proFuseData.SysInfoData.ToolType)
                    {
                        var response = _windowAPI.ShowMessageBox(
                            new RCXCaption(Main.CommonId, Caption.ExtensionName),
                            new RCXCaption(Main.CommonId, Caption.DifferentToolTypeWarning),
                            ButtonType.Yes_No,
                            IconType.Warning,
                            _window
                        );

                        if (response == ResponseType.Yes)
                        {
                            // Accept the new tool type and mark the project data as modified.
                            isOK = true;
                            _proFuseData.Adjust(_proFuseData.SysInfoData.ToolType);
                            _proFuseData.SetDirty();
                        }
                        else
                        {
                            // The user cancelled, so keep the dialog open.
                            isOK = false;
                        }
                    }

                    if (isOK)
                    {
                        // Hand the connected client over to the caller.
                        _parameters!["ModelPFClient"] = client;

                        // Persist the IP address only when it has actually changed.
                        if (!_proFuseData.NetworkData.IPAddressConnectFromPC.Equals(IPAddress.Value))
                        {
                            _proFuseData.NetworkData.IPAddressConnectFromPC = IPAddress.Value!;
                            _proFuseData.SetDirty();
                        }

                        await base.OnOK();
                    }
                }
            }
            else
            {
                // Connection failed: notify the user and keep the dialog open.
                ErrorReporter.Message(Caption.CannotConnectError, _window);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Loads the IP address that was stored in the project data.
        /// </remarks>
        protected override void Setup()
        {
            if (_proFuseData != null)
            {
                IPAddress.Value = _proFuseData.NetworkData.IPAddressConnectFromPC;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectFromPCWindowViewModel"/> class
        /// and sets up the reactive bindings and command subscriptions.
        /// </summary>
        public ConnectFromPCWindowViewModel()
        {
            _windowAPI = Main.GetAPI<IRCXWindowAPI>();

            // Enable the OK button only while a valid IP address is entered.
            IPAddress.Subscribe((address) =>
            {
                CanOK.Value = (address != null);
            })
            .AddTo(_disposables);

            DefaultSettingsCommand
                .Subscribe(OnDefaultSettings)
                .AddTo(_disposables);
        }
    }
}

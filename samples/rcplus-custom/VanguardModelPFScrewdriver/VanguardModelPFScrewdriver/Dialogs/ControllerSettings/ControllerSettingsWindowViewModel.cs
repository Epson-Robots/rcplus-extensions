// -----------------------------------------------------------------------
// <copyright file="ControllerSettingsWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.ExtensionsAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.ComponentModel;
using System.Net;
using System.Windows;
using VanguardModelPFScrewdriver.ModelPF;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using VanguardModelPFScrewdriver.Utils;
using static Epson.RoboticsShared.ExtensionsAPI.IRCXControllerAPI;
using static Epson.RoboticsShared.ExtensionsAPI.RCXCommon;
using static VanguardModelPFScrewdriver.Constants;

namespace VanguardModelPFScrewdriver.Dialogs.ControllerSettings
{
    /// <summary>
    /// View model for the controller settings dialog.
    /// It edits the network settings of the ModelPF CT controller (CTConts) and
    /// binds the selected robot controller TCP/IP port to the ModelPF client.
    /// </summary>
    internal class ControllerSettingsWindowViewModel : DialogViewModel
    {
        /// <summary>
        /// Gets the IP address of the CT controller.
        /// </summary>
        public ReactivePropertySlim<IPAddress?> CTContsIPAddress { get; } = new();

        /// <summary>
        /// Gets the subnet mask of the CT controller.
        /// </summary>
        public ReactivePropertySlim<IPAddress?> CTContsSubnetMask { get; } = new();

        /// <summary>
        /// Gets the default gateway of the CT controller.
        /// </summary>
        public ReactivePropertySlim<IPAddress?> CTContsGateway { get; } = new();

        /// <summary>
        /// Gets a value indicating whether downloading the settings from the device succeeded.
        /// </summary>
        public ReactivePropertySlim<bool> DownloadingIsSuccess { get; } = new(false);

        /// <summary>
        /// Gets the command that copies the settings downloaded from the device into the editors.
        /// It is enabled only when the download succeeded.
        /// </summary>
        public ReactiveCommand SetDownloadedValuesCommand { get; }

        /// <summary>
        /// Gets the command that copies the settings stored in the project file into the editors.
        /// </summary>
        public ReactiveCommand SetFileValuesCommand { get; } = new();

        /// <summary>
        /// Gets the list of TCP/IP port numbers available on the robot controller.
        /// </summary>
        public ReactiveCollection<int> TCPPorts { get; } = [];

        /// <summary>
        /// Gets the TCP/IP port number currently selected by the user.
        /// </summary>
        public ReactivePropertySlim<int?> SelectedTCPPort { get; } = new(-1);

        /// <summary>
        /// Gets the host IP address assigned to the selected TCP/IP port.
        /// </summary>
        public ReactivePropertySlim<IPAddress?> TCPPortIPAddress { get; } = new();

        /// <summary>
        /// Gets a counter used as a trigger to clear the TCP/IP port address input on the view.
        /// The value is incremented whenever the stored host name is not a valid IP address.
        /// </summary>
        public ReactivePropertySlim<int> TCPPortIPAddressClearToken { get; } = new(0);

        /// <summary>
        /// Gets the IP address of the robot controller (read-only information).
        /// </summary>
        public ReactivePropertySlim<string> ControllerIPAddress { get; } = new();

        /// <summary>
        /// Gets the subnet mask of the robot controller (read-only information).
        /// </summary>
        public ReactivePropertySlim<string> ControllerSubnetMask { get; } = new();

        /// <summary>
        /// Gets the default gateway of the robot controller (read-only information).
        /// </summary>
        public ReactivePropertySlim<string> ControllerGateway {  get; } = new();

        /// <summary>
        /// Network settings downloaded from the ModelPF device.
        /// </summary>
        private ModelPFData.Network? _downloadedSettings;

        /// <summary>
        /// API used to read and write the robot controller settings.
        /// </summary>
        private readonly IRCXControllerAPI _controllerAPI;

        /// <summary>
        /// Common prefix of the controller setting category names for TCP/IP ports.
        /// </summary>
        private const string _commonTCPPortPrefix = "TCPIP/Port";

        /// <summary>
        /// Setting values of the currently selected TCP/IP port category.
        /// </summary>
        private IDictionary<string, ControllerSettingValue>? _currentTCPPortSettings;

        private void OnSetDownloadedValues()
        {
            CTContsIPAddress.Value = _downloadedSettings!.CTContsIPAddress;
            CTContsSubnetMask.Value = _downloadedSettings.CTContsSubnetMask;
            CTContsGateway.Value = _downloadedSettings.CTContsGateway;
        }

        private void OnSetFileValues()
        {
            CTContsIPAddress.Value = _proFuseData!.NetworkData.CTContsIPAddress;
            CTContsSubnetMask.Value = _proFuseData.NetworkData.CTContsSubnetMask;
            CTContsGateway.Value = _proFuseData.NetworkData.CTContsGateway;
        }

        /// <summary>
        /// Reads the network configuration of the robot controller and shows it in the dialog.
        /// </summary>
        private void GetControllerConfiguration()
        {
            // Clear the previous values first, so that nothing stale remains when reading fails.
            ControllerIPAddress.Value = string.Empty;
            ControllerSubnetMask.Value = string.Empty;
            ControllerGateway.Value = string.Empty;

            var (result, settings) = _controllerAPI.GetControllerSettings(null, "Configuration");
            if (result == RCXResult.Success && settings != null)
            {
                ControllerIPAddress.Value = settings.GetValue("IpAddress", typeof(string)) as string ?? string.Empty;
                ControllerSubnetMask.Value = settings.GetValue("SubnetMask", typeof(string)) as string ?? string.Empty;
                ControllerGateway.Value = settings.GetValue("DefaultGateway", typeof(string)) as string ?? string.Empty;
            }
        }

        /// <summary>
        /// Collects the TCP/IP port numbers defined on the robot controller
        /// from the controller setting category names.
        /// </summary>
        private void GetTCPPortsList()
        {
            TCPPorts.Clear();
            SelectedTCPPort.Value = null;

            var categoryNames = _controllerAPI.GetControllerSettingsCategoryNames();
            if (categoryNames != null)
            {
                foreach (var name in categoryNames)
                {
                    // Category names of TCP/IP ports look like "TCPIP/Port201".
                    if (name.StartsWith(_commonTCPPortPrefix))
                    {
                        var portNo = int.Parse(name.Replace(_commonTCPPortPrefix, string.Empty));
                        TCPPorts.Add(portNo);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the settings of the newly selected TCP/IP port and updates the displayed host address.
        /// </summary>
        /// <param name="portNo">The selected TCP/IP port number, or null when no port is selected.</param>
        private void OnSelectedTCPPortChanged(
            int? portNo
        )
        {
            if (portNo.HasValue)
            {
                TCPPortIPAddress.Value = null;
                _currentTCPPortSettings = null;

                var categoryName = $"{_commonTCPPortPrefix}{portNo.Value}";

                var (result, settings) = _controllerAPI.GetControllerSettings(null, categoryName);
                if (result == RCXResult.Success && settings != null)
                {
                    _currentTCPPortSettings = settings;
                    var addressString = settings.GetValue("HostName", typeof(string)) as string;
                    if (IPAddress.TryParse(addressString, out var address))
                    {
                        TCPPortIPAddress.Value = address;
                    }
                    else
                    {
                        // The host name is not an IP address, so ask the view to clear the input.
                        TCPPortIPAddressClearToken.Value++;
                    }
                }
            }

            CheckCanOK();
        }

        /// <summary>
        /// Converts a string expression to the type required by a controller setting value.
        /// </summary>
        /// <param name="valueType">The target type of the setting value.</param>
        /// <param name="expression">The string representation of the value.</param>
        /// <returns>The converted value, or null when the conversion is not possible.</returns>
        private static object? GetTypedValue(
            Type valueType,
            string expression
        )
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(valueType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    return converter.ConvertFrom(expression);
                }
            }
            catch (Exception)
            {
                // EMPTY
            }

            return null;
        }

        /// <summary>
        /// Applies the TCP/IP port settings required for the ModelPF communication to the robot controller.
        /// </summary>
        /// <returns>True when all settings were written and committed successfully; otherwise false.</returns>
        public async Task<bool> SetTCPPort()
        {
            // Open an edit session for the controller settings.
            var (result, sessionId) = await _controllerAPI.StartSetControllerSettingsAsync();
            if (result != RCXResult.Success || sessionId == null)
            {
                return false;
            }

            int portNo = SelectedTCPPort.Value!.Value;
            var categoryName = $"{_commonTCPPortPrefix}{portNo}";

            // Configure the port as a real TCP port that connects to the ModelPF device.
            _currentTCPPortSettings!["IsVirtualPort"].Value = false;
            _currentTCPPortSettings!["HostName"].Value = TCPPortIPAddress.Value!.ToString();
            _currentTCPPortSettings!["TcpIpPortNumber"].Value = _proFuseClient!.Port;
            _currentTCPPortSettings!["Protocol"].Value = GetTypedValue(_currentTCPPortSettings!["Protocol"].ValueType, "TCP");
            _currentTCPPortSettings!["Terminator"].Value = GetTypedValue(_currentTCPPortSettings!["Terminator"].ValueType, "CRLF");
            _currentTCPPortSettings!["Timeout"].Value = 5.0;

            result = await _controllerAPI.SetControllerSettingsAsync(sessionId, categoryName, _currentTCPPortSettings);
            if (result != RCXResult.Success)
            {
                return false;
            }

            // Commit the session so that the changes are stored in the controller.
            result = await _controllerAPI.CommitSetControllerSettingsAsync(sessionId);
            if (result != RCXResult.Success)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uploads the edited network settings to the ModelPF device, applies the TCP/IP port
        /// settings to the robot controller and closes the dialog when everything succeeded.
        /// </summary>
        protected override async Task OnOK()
        {
            // Store the edited values into the project data (fall back to defaults when empty).
            _proFuseData!.NetworkData.TCPPort = SelectedTCPPort.Value!.Value;
            _proFuseData!.NetworkData.CTContsIPAddress = CTContsIPAddress.Value ?? new IPAddress([192, 168, 0, 1]);
            _proFuseData!.NetworkData.CTContsSubnetMask = CTContsSubnetMask.Value ?? new IPAddress([255, 255, 255, 0]);
            _proFuseData!.NetworkData.CTContsGateway = CTContsGateway.Value ?? new IPAddress([0, 0, 0, 0]);

            var isSuccess = await _proFuseData!.NetworkData.UploadAsync(_proFuseClient!);
            if (!isSuccess)
            {
                ErrorReporter.Message(Caption.CannotUploadError, _window);
            }
            else
            {
                isSuccess = await SetTCPPort();
                if (!isSuccess)
                {
                    ErrorReporter.Message(Caption.RobotControllerSettingsError, _window);
                }
                else
                {
                    _proFuseData.SetDirty();

                    await base.OnOK();
                }
            }
        }

        /// <summary>
        /// Updates the enabled state of the OK button according to the current input values.
        /// </summary>
        private void CheckCanOK()
        {
            CanOK.Value = (
                _proFuseData != null
                && _proFuseClient != null
                && CTContsIPAddress.Value != null
                && CTContsSubnetMask.Value != null
                && SelectedTCPPort.Value.HasValue
                && TCPPortIPAddress.Value != null
                && _currentTCPPortSettings != null
            );
        }

        /// <summary>
        /// Initializes the dialog contents: shows the saved settings, starts downloading the
        /// device settings in the background and reads the robot controller information.
        /// </summary>
        protected override void Setup()
        {
            if (_proFuseData != null)
            {
                OnSetFileValues();

                // Download the current settings from the device without blocking the UI thread.
                _ = Task.Run(async () =>
                {
                    _downloadedSettings = new();
                    var isSuccess = _proFuseClient != null && await _downloadedSettings.DownloadAsync(_proFuseClient);
                    if (isSuccess)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DownloadingIsSuccess.Value = true;
                        });
                    }
                    else
                    {
                        ErrorReporter.Message(Caption.NetConfDownloadError, _window);
                    }
                });

                GetControllerConfiguration();
                GetTCPPortsList();

                // Restore the TCP/IP port stored in the project data.
                var portNo = _proFuseData.NetworkData.TCPPort;
                SelectedTCPPort.Value = portNo;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerSettingsWindowViewModel"/> class
        /// and subscribes to the property changes that affect the dialog state.
        /// </summary>
        public ControllerSettingsWindowViewModel()
        {
            _controllerAPI = Main.GetAPI<IRCXControllerAPI>();

            SetDownloadedValuesCommand = DownloadingIsSuccess
                .ToReactiveCommand()
                .WithSubscribe(OnSetDownloadedValues)
                .AddTo(_disposables);
            SetFileValuesCommand
                .Subscribe(OnSetFileValues)
                .AddTo(_disposables);

            CTContsIPAddress.Subscribe((_) => CheckCanOK()).AddTo(_disposables);
            CTContsSubnetMask.Subscribe((_) => CheckCanOK()).AddTo(_disposables);

            SelectedTCPPort.Subscribe(OnSelectedTCPPortChanged).AddTo(_disposables);
            TCPPortIPAddress.Subscribe((_) => CheckCanOK()).AddTo(_disposables);
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="SystemInformationWindowViewModel.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Reactive.Bindings;
using VanguardModelPFScrewdriver.ModelPF;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFData.SysInfo;
using VersionType = (byte Major, byte Minor, byte Patch);

namespace VanguardModelPFScrewdriver.Dialogs.SystemInformation
{
    /// <summary>
    /// View model for the system information dialog.
    /// Exposes the ModelPF device's system information (serial numbers, firmware
    /// versions, MAC address, tightening count, etc.) as bindable properties.
    /// </summary>
    internal class SystemInformationWindowViewModel : DialogViewModel
    {
        /// <summary>
        /// Gets the serial number of the controller/console.
        /// </summary>
        public ReactivePropertySlim<string> CTContsSerial { get; } = new(string.Empty);

        /// <summary>
        /// Gets the total number of tightenings, formatted for display.
        /// This value is updated periodically while the watcher is running.
        /// </summary>
        public ReactivePropertySlim<string> TotalNumTighteningsExpr { get; } = new(string.Empty);

        /// <summary>
        /// Gets the TMC firmware version string (Major.Minor.Patch).
        /// </summary>
        public ReactivePropertySlim<string> TMCVersion { get; } = new(string.Empty);

        /// <summary>
        /// Gets the SDC firmware version string (Major.Minor.Patch).
        /// </summary>
        public ReactivePropertySlim<string> SDCVersion { get; } = new(string.Empty);

        /// <summary>
        /// Gets the MAC address of the device, formatted as dot-separated hexadecimal bytes.
        /// </summary>
        public ReactivePropertySlim<string> MACAddress { get; } = new(string.Empty);

        /// <summary>
        /// Gets the serial number of the screwdriver.
        /// </summary>
        public ReactivePropertySlim<string> DriverSeiral { get; } = new(string.Empty);

        /// <summary>
        /// Gets the kind of the connected tool.
        /// </summary>
        public ReactivePropertySlim<ToolKind> ToolType { get; } = new(ToolKind.Unknown);

        /// <summary>
        /// Formats a version tuple as "Major.Minor.Patch".
        /// </summary>
        /// <param name="version">The version to format.</param>
        /// <returns>The formatted version string.</returns>
        private static string ToString(
            VersionType version
        )
        {
            return $"{version.Major}.{version.Minor}.{version.Patch}";
        }

        /// <summary>
        /// Formats a MAC address as dot-separated uppercase hexadecimal bytes.
        /// </summary>
        /// <param name="macAddress">The raw MAC address bytes.</param>
        /// <returns>The formatted MAC address string.</returns>
        private static string ToString(
            byte[] macAddress
        )
        {
            return string.Join(".", macAddress.Select(x => $"{x:X2}"));
        }

        /// <summary>
        /// Initializes the bindable properties from the current system information
        /// and starts the periodic watcher that keeps the tightening count up to date.
        /// </summary>
        protected override void Setup()
        {
            if (_proFuseData != null)
            {
                ModelPFData.SysInfo sysInfo = _proFuseData.SysInfoData;

                // Copy the snapshot values into the bindable properties.
                CTContsSerial.Value = sysInfo.CTContsSerial;
                TotalNumTighteningsExpr.Value = sysInfo.TotalNumTightenings.ToString();
                TMCVersion.Value = ToString(sysInfo.TMCVersion);
                SDCVersion.Value = ToString(sysInfo.SDCVersion);
                MACAddress.Value = ToString(sysInfo.MACAddress);

                DriverSeiral.Value = sysInfo.DriverSerial;
                ToolType.Value = sysInfo.ToolType;

                // Reflect changes pushed by the watcher into the displayed value.
                sysInfo.PropertyChanged += (_, _) =>
                {
                    TotalNumTighteningsExpr.Value = sysInfo.TotalNumTightenings.ToString();
                };

                // Poll the device once per second.
                sysInfo.StartWatcher(_proFuseClient!, new TimeSpan(0, 0, 1));
            }
        }

        /// <summary>
        /// Stops the periodic watcher when the dialog is closed.
        /// </summary>
        protected override void Cleanup()
        {
            _proFuseData?.SysInfoData.EndWatcher();
        }
    }
}

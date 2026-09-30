// -----------------------------------------------------------------------
// <copyright file="ModelPFDataSysInfo.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using System.Text;
using System.Windows.Threading;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFRegister;
using VersionType = (byte Major, byte Minor, byte Patch);

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE data (system information).
    /// </summary>
    public partial class ModelPFData
    {
        /// <summary>
        /// Holds the system information reported by the PRO-FUSE controller
        /// and periodically refreshes the total number of tightenings.
        /// </summary>
        public class SysInfo : Base, INotifyPropertyChanged
        {
            /// <summary>
            /// Specifies the type of the connected tool.
            /// </summary>
            public enum ToolKind : ushort
            {
                /// <summary>S type tool.</summary>
                S = 0,

                /// <summary>L type tool.</summary>
                L = 1,

                /// <summary>XL type tool.</summary>
                XL = 2,

                // SO = 3,

                /// <summary>GM type tool.</summary>
                GM = 4,

                /// <summary>GX type tool.</summary>
                GX = 5,

                /// <summary>The tool type reported by the controller is not supported.</summary>
                Unknown = 9999,
            }

            /// <summary>
            /// Occurs when a monitored property value has changed.
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;

            /// <summary>
            /// Gets or sets the TMC firmware version.
            /// </summary>
            public VersionType TMCVersion;

            /// <summary>
            /// Gets or sets the SDC firmware version.
            /// </summary>
            public VersionType SDCVersion;

            /// <summary>
            /// Gets or sets the total number of tightenings performed by the driver.
            /// </summary>
            public uint TotalNumTightenings;

            /// <summary>
            /// Gets or sets the type of the connected tool.
            /// </summary>
            public ToolKind ToolType = ToolKind.S;

            /// <summary>
            /// Gets or sets the identifier of the CT-CONTS controller.
            /// </summary>
            public int CTContsId;

            /// <summary>
            /// Gets or sets the MAC address of the CT-CONTS controller.
            /// </summary>
            public byte[] MACAddress = [0, 0, 0, 0, 0, 0];

            /// <summary>
            /// Gets or sets the serial number of the CT-CONTS controller.
            /// </summary>
            public string CTContsSerial = string.Empty;

            /// <summary>
            /// Gets or sets the serial number of the screwdriver unit.
            /// </summary>
            public string DriverSerial = string.Empty;

            /// <summary>
            /// Timer used to poll the controller while the watcher is running.
            /// </summary>
            private DispatcherTimer? _watcher;

            /// <summary>
            /// Converts a register value encoded as MMmmpp (decimal) into a version tuple.
            /// </summary>
            /// <param name="registerValue">The raw register value.</param>
            /// <returns>The decoded major, minor and patch numbers.</returns>
            private static VersionType GetVersion(
                ushort registerValue
            )
            {
                int version = registerValue;
                int patch = version % 100;
                version /= 100;
                int minor = version % 100;
                version /= 100;
                int major = version % 100;
                return new VersionType((byte)major, (byte)minor, (byte)patch);
            }

            /// <summary>
            /// Combines two consecutive registers into a 32-bit value (big endian word order).
            /// </summary>
            /// <param name="registerValues">The register values read from the controller.</param>
            /// <param name="offset">The index of the high-order register.</param>
            /// <returns>The combined 32-bit value.</returns>
            private static uint GetUInt(
                ushort[] registerValues,
                int offset
            )
            {
                uint value = (uint)registerValues[offset];

                return (value << 16) + (uint)registerValues[offset + 1];
            }

            /// <summary>
            /// Converts a register value into a <see cref="ToolKind"/> value.
            /// </summary>
            /// <param name="registerValue">The raw register value.</param>
            /// <returns>
            /// The matching <see cref="ToolKind"/>, or <see cref="ToolKind.Unknown"/> when the value is not defined.
            /// </returns>
            private static ToolKind GetToolType(
                ushort registerValue
            )
            {
                if (Enum.IsDefined(typeof(ToolKind), registerValue))
                {
                    return (ToolKind)registerValue;
                }
                else
                {
                    return ToolKind.Unknown;
                }
            }

            /// <summary>
            /// Extracts a MAC address stored in three consecutive registers, two octets per register.
            /// </summary>
            /// <param name="registerValues">The register values read from the controller.</param>
            /// <param name="offset">The index of the first register of the MAC address.</param>
            /// <returns>The six octets of the MAC address.</returns>
            private static byte[] GetMACAddress(
                ushort[] registerValues,
                int offset
            )
            {
                var octets = new byte[6];

                octets[0] = (byte)((registerValues[offset] >> 8) & 0xff);
                octets[1] = (byte)(registerValues[offset] & 0xff);
                octets[2] = (byte)((registerValues[offset + 1] >> 8) & 0xff);
                octets[3] = (byte)(registerValues[offset + 1] & 0xff);
                octets[4] = (byte)((registerValues[offset + 2] >> 8) & 0xff);
                octets[5] = (byte)(registerValues[offset+ 2] & 0xff);

                return octets;
            }

            /// <summary>
            /// Extracts a serial number string stored as ASCII characters, two characters per register.
            /// Reading stops at the first register that contains zero.
            /// </summary>
            /// <param name="registerValues">The register values read from the controller.</param>
            /// <param name="offset">The index of the first register of the serial number.</param>
            /// <returns>The decoded serial number.</returns>
            private static string GetSerial(
                ushort[] registerValues,
                int offset
            )
            {
                List<byte> bytes = [];

                // Each register holds two ASCII characters: high byte first, then low byte.
                foreach (var registerValue in registerValues[offset..])
                {
                    if (registerValue == 0)
                    {
                        // A zero register terminates the string.
                        break;
                    }
                    bytes.Add((byte)((registerValue >> 8) & 0xff));
                    bytes.Add((byte)(registerValue & 0xff));
                }

                return Encoding.ASCII.GetString([.. bytes]);
            }

            /// <summary>
            /// Creates a copy of this instance.
            /// </summary>
            /// <returns>A new <see cref="SysInfo"/> instance with the same values.</returns>
            public SysInfo Clone()
            {
                return new()
                {
                    TMCVersion = TMCVersion,
                    SDCVersion = SDCVersion,
                    TotalNumTightenings = TotalNumTightenings,
                    ToolType = ToolType,
                    CTContsId = CTContsId,

                    // Copy the array so that the clone does not share the MAC address buffer.
                    MACAddress = (byte[])MACAddress.Clone(),
                    CTContsSerial = CTContsSerial,
                    DriverSerial = DriverSerial,
                };
            }

            /// <summary>
            /// Determines whether the specified instance has the same values as this instance.
            /// Only the values that affect the edited data are compared; the read-only
            /// information reported by the controller is ignored.
            /// </summary>
            /// <param name="other">Instance to compare with.</param>
            /// <returns><c>true</c> if the relevant values match; otherwise, <c>false</c>.</returns>
            public bool Equals(
                SysInfo other
            )
            {
                return (
                    other.ToolType == ToolType
                    && other.CTContsId == CTContsId
                );
            }

            /// <summary>
            /// Reads the whole system information from the controller.
            /// </summary>
            /// <param name="client">The client connected to the controller.</param>
            /// <param name="cancellationToken">Token used to cancel the operation.</param>
            /// <returns><c>true</c> when all registers were read successfully; otherwise, <c>false</c>.</returns>
            public override async Task<bool> DownloadAsync(
                ModelPFClient client,
                CancellationToken cancellationToken = default
            )
            {
                // Offsets within the input register block starting at TMCVersion.
                const int OffsetOrigin1 = Input.TMCVersion;
                const int OffsetTMCVersion = Input.TMCVersion - OffsetOrigin1;
                const int OffsetSDCVersion = Input.SDCVersion - OffsetOrigin1;
                const int OffsetTotalNumTightenings = Input.TotalNumTightenings - OffsetOrigin1;
                const int OffsetToolType = Input.ToolType - OffsetOrigin1;
                const int OffsetID = Input.ID - OffsetOrigin1;

                const ushort count1 = (ushort)(Input.Reserved1 - Input.TMCVersion);
                var registerValues1 = await client.ReadInputRegistersAsync(Input.TMCVersion, count1, cancellationToken);
                if (registerValues1 == null)
                {
                    return false;
                }

                // Offsets within the holding register block starting at MACAddress.
                const int OffsetOrigin2 = Holding.MACAddress;
                const int OffsetMACAddress = Holding.MACAddress - OffsetOrigin2;
                const int OffsetCTContsSerial = Holding.CTContsSerial - OffsetOrigin2;
                const int OffsetDriverSerial = Holding.DriverSerial - OffsetOrigin2;

                const ushort count2 = (ushort)(Holding.Reserved1 - Holding.MACAddress);
                var registerValues2 = await client.ReadHoldingRegistersAsync(Holding.MACAddress, count2, cancellationToken);
                if (registerValues2 == null)
                {
                    return false;
                }

                // Decode the raw register values into the properties.
                TMCVersion = GetVersion(registerValues1[OffsetTMCVersion]);
                SDCVersion = GetVersion(registerValues1[OffsetSDCVersion]);
                TotalNumTightenings = GetUInt(registerValues1, OffsetTotalNumTightenings);
                ToolType = GetToolType(registerValues1[OffsetToolType]);
                CTContsId = (int)registerValues1[OffsetID];

                MACAddress = GetMACAddress(registerValues2, OffsetMACAddress);
                CTContsSerial = GetSerial(registerValues2, OffsetCTContsSerial);
                DriverSerial = GetSerial(registerValues2, OffsetDriverSerial);

                return true;
            }

            /// <summary>
            /// Reads only the total number of tightenings from the controller.
            /// </summary>
            /// <param name="client">The client connected to the controller.</param>
            /// <returns><c>true</c> when the registers were read successfully; otherwise, <c>false</c>.</returns>
            public async Task<bool> UpdateTotalNumTighteningsAsync(
                ModelPFClient client
            )
            {
                const ushort count = (ushort)(Input.ToolType - Input.TotalNumTightenings);
                var registerValues = await client.ReadInputRegistersAsync(Input.TotalNumTightenings, count);
                if (registerValues == null)
                {
                    return false;
                }

                TotalNumTightenings = GetUInt(registerValues, 0);

                return true;
            }

            /// <summary>
            /// Handles the watcher timer tick and raises <see cref="PropertyChanged"/>
            /// when the total number of tightenings has changed.
            /// </summary>
            /// <param name="sender">The timer that raised the event.</param>
            /// <param name="ev">The event data.</param>
            private async void OnTick(
                object? sender, EventArgs ev
            )
            {
                if (sender is DispatcherTimer timer && timer.Tag is ModelPFClient client)
                {
                    var oldValue = TotalNumTightenings;
                    if (await UpdateTotalNumTighteningsAsync(client))
                    {
                        if (oldValue != TotalNumTightenings)
                        {
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalNumTightenings)));
                        }
                    }
                }
            }

            /// <summary>
            /// Starts polling the controller at the specified interval.
            /// The first read is performed immediately.
            /// </summary>
            /// <param name="client">The client connected to the controller.</param>
            /// <param name="interval">The polling interval.</param>
            public void StartWatcher(
                ModelPFClient client,
                TimeSpan interval
            )
            {
                _watcher = new()
                {
                    Tag = client,
                    Interval = interval
                };
                _watcher.Tick += OnTick;

                OnTick(_watcher, EventArgs.Empty);

                _watcher.Start();
            }

            /// <summary>
            /// Stops polling the controller and releases the timer.
            /// </summary>
            public void EndWatcher()
            {
                _watcher?.Stop();
                _watcher = null;
            }
        }
    }
}

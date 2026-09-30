// -----------------------------------------------------------------------
// <copyright file="ModelPFDataNetwork.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Net;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFRegister;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE definition data (network).
    /// </summary>
    public partial class ModelPFData
    {
        /// <summary>
        /// Network settings.
        /// </summary>
        public class Network : Base
        {
            /// <summary>
            /// IP address used when connecting from the PC.
            /// Stored in the file only; not transferred via Modbus registers.
            /// </summary>
            public IPAddress IPAddressConnectFromPC = new([192, 168, 0, 1]);

            /// <summary>
            /// TCP port number used when connecting from SPEL+.
            /// Stored in the file only; not transferred via Modbus registers.
            /// </summary>
            public int TCPPort = 201;

            /// <summary>
            /// CT-CONTS IP address.
            /// </summary>
            public IPAddress CTContsIPAddress = new([192, 168, 0, 1]);

            /// <summary>
            /// CT-CONTS subnet mask.
            /// </summary>
            public IPAddress CTContsSubnetMask = new([255, 255, 255, 0]);

            /// <summary>
            /// CT-CONTS default gateway.
            /// </summary>
            public IPAddress CTContsGateway = new([0, 0, 0, 0]);

            /// <summary>
            /// Creates a copy of this instance.
            /// </summary>
            /// <returns>A new <see cref="Network"/> instance with the same values.</returns>
            public Network Clone()
            {
                return new()
                {
                    IPAddressConnectFromPC = new(IPAddressConnectFromPC.GetAddressBytes()),
                    TCPPort = TCPPort,
                    CTContsIPAddress = new(CTContsIPAddress.GetAddressBytes()),
                    CTContsSubnetMask = new(CTContsSubnetMask.GetAddressBytes()),
                    CTContsGateway = new(CTContsGateway.GetAddressBytes()),
                };
            }

            /// <summary>
            /// Determines whether the specified instance has the same values as this instance.
            /// </summary>
            /// <param name="other">Instance to compare with.</param>
            /// <returns><c>true</c> if all the settings match; otherwise, <c>false</c>.</returns>
            public bool Equals(
                Network other
            )
            {
                return (
                    other.IPAddressConnectFromPC.Equals(IPAddressConnectFromPC)
                    && other.TCPPort == TCPPort
                    && other.CTContsIPAddress.Equals(CTContsIPAddress)
                    && other.CTContsSubnetMask.Equals(CTContsSubnetMask)
                    && other.CTContsGateway.Equals(CTContsGateway)
                );
            }

            /// <inheritdoc />
            public override void ReadFromFile(
                BinaryReader reader,
                int phase = 0
            )
            {
                // The fields must be read in the same order as they are written.
                IPAddressConnectFromPC = IPAddressExtension.ReadFromFile(reader);
                TCPPort = reader.ReadUInt16();
                CTContsIPAddress = IPAddressExtension.ReadFromFile(reader);
                CTContsSubnetMask = IPAddressExtension.ReadFromFile(reader);
                CTContsGateway = IPAddressExtension.ReadFromFile(reader);
            }

            /// <inheritdoc />
            public override void WriteToFile(
                BinaryWriter writer,
                int phase = 0
            )
            {
                // The fields must be written in the same order as they are read.
                IPAddressConnectFromPC.WriteToFile(writer);
                writer.Write((ushort)TCPPort);
                CTContsIPAddress.WriteToFile(writer);
                CTContsSubnetMask.WriteToFile(writer);
                CTContsGateway.WriteToFile(writer);
            }

            /// <summary>
            /// Builds an IP address from two consecutive 16-bit registers.
            /// </summary>
            /// <param name="registerValues">Register values read from the device.</param>
            /// <param name="offset">Index of the first register holding the address.</param>
            /// <returns>The decoded IP address.</returns>
            private static IPAddress GetIPAddress(
                ushort[] registerValues,
                int offset
            )
            {
                var octets = new byte[4];

                // Each register holds two octets: high byte first, then low byte.
                octets[0] = (byte)((registerValues[offset] >> 8) & 0xff);
                octets[1] = (byte)(registerValues[offset] & 0xff);
                octets[2] = (byte)((registerValues[offset + 1] >> 8) & 0xff);
                octets[3] = (byte)(registerValues[offset + 1] & 0xff);

                return new IPAddress(octets);
            }

            /// <summary>
            /// Stores an IP address into two consecutive 16-bit registers.
            /// </summary>
            /// <param name="registerValues">Register buffer to write to.</param>
            /// <param name="offset">Index of the first register to store the address.</param>
            /// <param name="address">The IP address to encode.</param>
            private static void SetIPAddress(
                ushort[] registerValues,
                int offset,
                IPAddress address
            )
            {
                var addressBytes = address.GetAddressBytes();

                // Pack two octets per register: high byte first, then low byte.
                registerValues[offset] = (ushort)(((ushort)addressBytes[0] << 8) + (ushort)addressBytes[1]);
                registerValues[offset + 1] = (ushort)(((ushort)addressBytes[2] << 8) + (ushort)addressBytes[3]);
            }

            /// <summary>
            /// Downloads the network settings from the device.
            /// </summary>
            /// <param name="client">Client used to communicate with the device.</param>
            /// <param name="cancellationToken">Token used to cancel the operation.</param>
            /// <returns><c>true</c> if the download succeeded; otherwise, <c>false</c>.</returns>
            public override async Task<bool> DownloadAsync(
                ModelPFClient client,
                CancellationToken cancellationToken = default
            )
            {
                // Offsets are relative to the first register of the network area.
                const int OffsetOrigin = Holding.IPAddress;
                const int OffsetIPAddress = Holding.IPAddress - OffsetOrigin;
                const int OffsetSubnetMask = Holding.SubnetMask - OffsetOrigin;
                const int OffsetGateway = Holding.Gateway - OffsetOrigin;

                // Read the whole network register block at once.
                const ushort count = (ushort)(Holding.MACAddress - Holding.IPAddress);
                var registerValues = await client.ReadHoldingRegistersAsync(Holding.IPAddress, count, cancellationToken);
                if (registerValues == null)
                {
                    return false;
                }

                CTContsIPAddress = GetIPAddress(registerValues, OffsetIPAddress);
                CTContsSubnetMask = GetIPAddress(registerValues, OffsetSubnetMask);
                CTContsGateway = GetIPAddress(registerValues, OffsetGateway);

                return true;
            }

            /// <summary>
            /// Uploads the network settings to the device.
            /// </summary>
            /// <param name="client">Client used to communicate with the device.</param>
            /// <param name="cancellationToken">Token used to cancel the operation.</param>
            /// <returns><c>true</c> if the upload succeeded; otherwise, <c>false</c>.</returns>
            public override async Task<bool> UploadAsync(
                ModelPFClient client,
                CancellationToken cancellationToken = default
            )
            {
                // Offsets are relative to the first register of the network area.
                const int OffsetOrigin = Holding.IPAddress;
                const int OffsetIPAddress = Holding.IPAddress - OffsetOrigin;
                const int OffsetSubnetMask = Holding.SubnetMask - OffsetOrigin;
                const int OffsetGateway = Holding.Gateway - OffsetOrigin;

                const ushort count = (ushort)(Holding.MACAddress - Holding.IPAddress);
                var registerValues = new ushort[count];

                SetIPAddress(registerValues, OffsetIPAddress, CTContsIPAddress);
                SetIPAddress(registerValues, OffsetSubnetMask, CTContsSubnetMask);
                SetIPAddress(registerValues, OffsetGateway, CTContsGateway);

                bool isSuccess;

                // Write the whole network register block at once.
                isSuccess = await client.WriteMultipleRegistersAsync(Holding.IPAddress, registerValues, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                // Wait until the device finishes applying the system parameters.
                isSuccess = await client.WaitReadyAsync(RequestCode.UPLOAD_SYSPARAM, cancellationToken);
                if (!isSuccess)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
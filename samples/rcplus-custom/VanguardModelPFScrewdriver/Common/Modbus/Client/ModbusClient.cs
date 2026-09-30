// -----------------------------------------------------------------------
// <copyright file="ModbusClient.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using static Epson.RoboticsShared.Modbus.Client.ModbusDefinitions;

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Base Class of Modbus Client
    /// </summary>
    internal class ModbusClient
    {
        /// <summary>
        /// Calculate PDU length.
        /// </summary>
        /// <param name="dataLength">Length of the data.</param>
        /// <returns>The length.</returns>
        internal static ushort PDULength(
            int dataLength
        )
        {
            return (ushort)(sizeof(FunctionCode) + dataLength);
        }

        /// <summary>
        /// Calculate body length.
        /// </summary>
        /// <param name="dataLength">Length of the data.</param>
        /// <returns>The length.</returns>
        internal static ushort BodyLength(
            int dataLength
        )
        {
            return (ushort)(sizeof(byte) + PDULength(dataLength));
        }

        /// <summary>
        /// Calculate byte length of a ushort array.
        /// </summary>
        /// <param name="data">A ushort array.</param>
        /// <returns>The length.</returns>
        internal static int ByteLength(
            ushort[] data
        )
        {
            return sizeof(ushort) * data.Length;
        }

        /// <summary>
        /// Create a byte array for ADU.
        /// </summary>
        /// <param name="dataLength">Length of the data.</param>
        /// <returns>A byte array for ADU.</returns>
        internal static byte[] CreateADU(
            int dataLength
        )
        {
            return new byte[MBAPHeaderLength + PDULength(dataLength)];
        }

        /// <summary>
        /// Create an ADU for request.
        /// </summary>
        /// <param name="unitId">Unit identity.</param>
        /// <param name="functionCode">Function code.</param>
        /// <param name="dataLength">Length of the data.</param>
        /// <returns>A byte array for ADU.</returns>
        internal static byte[] CreateRequestADU(
            byte unitId,
            FunctionCode functionCode,
            int dataLength
        )
        {
            var adu = CreateADU(dataLength);

            adu[OffsetUnitId] = unitId;
            BodyLength(dataLength).SetBE(adu, OffsetBodyLength);
            adu[OffsetFunctionCode] = (byte)functionCode;

            return adu;
        }

        /// <summary>
        /// Create an ADU for request.
        /// </summary>
        /// <param name="unitId">Unit identity.</param>
        /// <param name="functionCode">Function code.</param>
        /// <param name="data">Data (An array of ushort).</param>
        /// <returns>A byte array for ADU.</returns>
        internal static byte[] CreateRequestADU(
            byte unitId,
            FunctionCode functionCode,
            ushort[] data
        )
        {
            var adu = CreateRequestADU(unitId, functionCode, ByteLength(data));
            data.SetBE(adu, OffsetData);

            return adu;
        }
    }
}

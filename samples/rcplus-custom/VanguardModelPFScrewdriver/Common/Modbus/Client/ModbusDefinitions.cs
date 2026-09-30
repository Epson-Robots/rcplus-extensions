// -----------------------------------------------------------------------
// <copyright file="ModbusDefnitions.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Modbus Related Definitions
    /// </summary>
    public static class ModbusDefinitions
    {
        /// <summary>
        /// MBAP header length
        /// </summary>
        public const byte MBAPHeaderLength = 7;

        /// <summary>
        /// Offset position for body length field in MBAP header
        /// </summary>
        public const byte OffsetBodyLength = 4;

        /// <summary>
        /// Offset position for unit identifier field in MBAP header
        /// </summary>
        public const byte OffsetUnitId = 6;

        /// <summary>
        /// Offset position for function code field in ADU
        /// </summary>
        public const byte OffsetFunctionCode = 7;

        /// <summary>
        /// Offset position for data field in ADU
        /// </summary>
        public const byte OffsetData = 8;

        /// <summary>
        /// Function codes
        /// </summary>
        /// <remarks>
        /// Only the function codes supported by this module are defined.
        /// </remarks>
        public enum FunctionCode : byte
        {
            None = 0x00,

            ReadHoldingRegisters = 0x03,
            ReadInputRegisters = 0x04,

            WriteSingleRegister = 0x06,

            WriteMultipleRegisters = 0x10,
        }

        /// <summary>
        /// Bit that specifies error
        /// </summary>
        public const byte ErrorCodeBit = 0x80;

        /// <summary>
        /// Exception codes
        /// </summary>
        public enum ExceptionCode : byte
        {
            None = 0,

            IllegalFunction = 0x01,
            IllegalDataAddress = 0x02,
            IllegalDataValue = 0x03,
            ServerDeviceFailure = 0x04,
            Acknowledge = 0x05,
            ServerDeviceBusy = 0x06,
            MemoryParityError = 0x08,
            GatewayPathUnavailable = 0x0A,
            GatewayTargetDeviceFailedToResponse = 0x0B,
        }
    }
}

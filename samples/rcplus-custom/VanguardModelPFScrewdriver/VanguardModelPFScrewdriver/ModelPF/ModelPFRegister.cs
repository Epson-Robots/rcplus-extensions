// -----------------------------------------------------------------------
// <copyright file="ModelPFRegister.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// Defines the Modbus register map of the PRO-FUSE screwdriver and
    /// the helper types used to interpret the register values.
    /// </summary>
    public static class ModelPFRegister
    {
        /// <summary>
        /// Addresses of the input registers (read-only data reported by the driver).
        /// </summary>
        public static class Input
        {
            public const ushort TMCVersion = 0x0000;
            public const ushort SDCVersion = 0x0001;
            public const ushort TotalNumTightenings = 0x0002;
            public const ushort ToolType = 0x0004;
            public const ushort ID = 0x0005;
            public const ushort Reserved1 = 0x0006;

            public const ushort CanGetTighteningResult = 0x0030;
            public const ushort GetTightingResult = 0x0031;
            public const ushort GetTightingResult_ProgramNo = 0x0031;
            public const ushort GetTightingResult_NumTurns = 0x0032;
            public const ushort GetTightingResult_Torque = 0x0033;
            public const ushort GetTightingResult_ElapsedTime = 0x0034;
            public const ushort GetTightingResult_Result = 0x0035;
            public const ushort GetTightingResult_ErrorDetail = 0x0036;
            public const ushort Reserved4 = 0x0037;

            public const ushort NumTorques = 0x003f;
            public const ushort Torques = 0x0040;
        }

        /// <summary>
        /// Addresses of the holding registers (readable and writable settings and commands).
        /// Offset constants are relative to the start address of the corresponding block.
        /// </summary>
        public static class Holding
        {
            public const ushort IPAddress = 0x0000;
            public const ushort SubnetMask = 0x0002;
            public const ushort Gateway = 0x0004;
            public const ushort MACAddress = 0x0006;
            public const ushort CTContsSerial = 0x0009;
            public const ushort DriverSerial = 0x0011;
            public const ushort Reserved1 = 0x0019;

            public const ushort WindowDefinitionStart = 0x0040;
            public const ushort WindowDefinitionEnd = 0x007f;
            public const int NumRegsPerWindow = 8;
            public const int OffsetWindowIsEnabled = 0;
            public const int OffsetWindowStartTurn = 1;
            public const int OffsetWindowEndTurn = 2;
            public const int OffsetWindowTorqueAtStart = 3;
            public const int OffsetWindowTorqueAtEnd = 4;
            public const int OffsetWindowTorqueWidth = 5;

            public const int OffsetDirection = 0;
            public const int OffsetTargetTorque = 1;

            public const int NumRegsPerStep = 2;
            public const int SubOffsetNumTurns = 0;
            public const int SubOffsetSpeed = 1;

            public const ushort TighteningPartStart = 0x0080;
            public const ushort TighteningPartEnd = 0x009f;
            public const int OffsetSpecialFlags = 2;
            public const int OffsetMode = 3;
            public const int OffsetStartDetectionAmount = 4;
            public const int OffsetInitialTappingTorque = 5;
            public const int OffsetTorqueUpDetectionTime = 6;
            public const int OffsetPermissibleTurnsBelow = 7;
            public const int OffsetPermissibleTurnsAbove = 8;
            public const int OffsetFurtherTighteningAngle = 9;
            public const int OffsetFurtherTighteningAngleForProtruding = 10;
            public const int OffsetTorqueThresholdToChangeSpeed = 11;
            public const int OffsetSpeedAfterChanged = 12;
            public const int OffsetTightningSteps = 14;

            public const ushort LooseningPartStart = 0x00A0;
            public const ushort LooseningPartEnd = 0x00af;
            public const int OffsetLooseningSteps = 2;

            public const ushort FreeRunPartStart = 0x00b0;
            public const ushort FreeRunPartEnd = 0x00b7;
            public const int OffsetFreeRunSteps = 2;

            public const ushort SampleingAngle = 0x00b8;
            public const ushort TorqueOffset = 0x00b9;

            public const ushort SelectProgram = 0x00c0;
            public const ushort RequestExecute = 0x00c1;
            public const ushort DO = 0x00c2;
            public const ushort DI = 0x00c3;
            public const ushort EnableRemote = 0x00c4;
        }

        /// <summary>
        /// Request codes written to the <see cref="Holding.RequestExecute"/> register.
        /// </summary>
        public enum RequestCode : ushort
        {
            /// <summary>No request is pending; the previous request has been completed.</summary>
            DONE = 0,

            /// <summary>Clears the "tightening result available" flag of the driver.</summary>
            CLEAR_CAN_GET_TIGHTENING_RESULT = 1,

            /// <summary>Uploads a program from the driver to the host.</summary>
            UPLOAD_PROGRAM = 2,

            /// <summary>Downloads a program from the host to the driver.</summary>
            DOWNLOAD_PROGRAM = 3,

            /// <summary>Uploads the system parameters from the driver to the host.</summary>
            UPLOAD_SYSPARAM = 4,
        }

        /// <summary>
        /// Bit definitions of the digital output (<see cref="Holding.DO"/>) register.
        /// </summary>
        [Flags]
        public enum DOValue : ushort
        {
            /// <summary>The operation has finished.</summary>
            END = (1 << 0),

            /// <summary>The driver is currently operating.</summary>
            BUSY = (1 << 1),

            // FIT = (1 << 2),

            /// <summary>An error has occurred.</summary>
            ERR = (1 << 3),

            /// <summary>Bit 0 of the error code.</summary>
            ERR0 = (1 << 4),

            /// <summary>Bit 1 of the error code.</summary>
            ERR1 = (1 << 5),

            /// <summary>Bit 2 of the error code.</summary>
            ERR2 = (1 << 6),

            /// <summary>Bit 3 of the error code.</summary>
            ERR3 = (1 << 7),

            /// <summary>Mask that selects all error code bits.</summary>
            ERROR_CODE = (ERR0 | ERR1 | ERR2 | ERR3),

            /// <summary>Bit position of the least significant error code bit.</summary>
            ERROR_CODE_SHIFT = 4,
        }

        /// <summary>
        /// Extracts the error code stored in the error code bits of a digital output value.
        /// </summary>
        /// <param name="value">The digital output value read from the driver.</param>
        /// <returns>The error code as an unsigned value.</returns>
        public static ushort GetErrorCode(
            this DOValue value
        )
        {
            return (ushort)((int)(value & DOValue.ERROR_CODE) >> (int)DOValue.ERROR_CODE_SHIFT);
        }

        /// <summary>
        /// Bit definitions of the digital input (<see cref="Holding.DI"/>) register.
        /// </summary>
        [Flags]
        public enum DIValue
        {
            /// <summary>Bit 0 of the program number selector.</summary>
            SEL0 = (1 << 0),

            /// <summary>Bit 1 of the program number selector.</summary>
            SEL1 = (1 << 1),

            /// <summary>Bit 2 of the program number selector.</summary>
            SEL2 = (1 << 2),

            /// <summary>Bit 3 of the program number selector.</summary>
            SEL3 = (1 << 3),

            /// <summary>Mask that selects all program selector bits.</summary>
            PROGRAM_SELECTOR = (SEL0 | SEL1 | SEL2 | SEL3),

            /// <summary>Bit position of the least significant program selector bit.</summary>
            PROGRAM_SELECTOR_SHIFT = 0,

            /// <summary>Starts a tightening operation.</summary>
            TIGHTEN = (1 << 4),

            /// <summary>Starts a loosening operation.</summary>
            LOOSEN = (1 << 5),

            /// <summary>Starts a free run operation.</summary>
            FREERUN = (1 << 6),

            /// <summary>Mask that selects all operation bits.</summary>
            OPERATIONS = (TIGHTEN | LOOSEN | FREERUN),
        }

        /// <summary>
        /// Replaces the program selector bits of a digital input value with the specified program number.
        /// </summary>
        /// <param name="value">The digital input value to modify.</param>
        /// <param name="programNo">The program number to set into the selector bits.</param>
        public static void SetProgram(
            ref this DIValue value,
            int programNo
        )
        {
            value &= ~DIValue.PROGRAM_SELECTOR;
            value |= (DIValue)(programNo << (int)DIValue.PROGRAM_SELECTOR_SHIFT);
        }

        /// <summary>
        /// Bit definitions of the remote enable (<see cref="Holding.EnableRemote"/>) register.
        /// </summary>
        [Flags]
        public enum Remote : ushort
        {
            /// <summary>Enables remote control of the digital inputs.</summary>
            DI = (1 << 0),

            /// <summary>Enables remote monitoring of the digital outputs.</summary>
            DO = (1 << 1),
        }
    }
}

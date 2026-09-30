// -----------------------------------------------------------------------
// <copyright file="ModbusException.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using static Epson.RoboticsShared.Modbus.Client.ModbusDefinitions;

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Modbus exception
    /// </summary>
    public class ModbusException : Exception
    {
        /// <summary>
        /// Function code
        /// </summary>
        public FunctionCode FunctionCodeValue { get; } = FunctionCode.None;

        /// <summary>
        /// Exception code
        /// </summary>
        public ExceptionCode ExceptionCodeValue { get; } = ExceptionCode.None;

        /// <summary>
        /// Constructor
        /// </summary>
        public ModbusException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ModbusException(
            string message
        )
        : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">Inner exception instance.</param>
        public ModbusException(
            string message,
            Exception innerException
        )
        : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="functionCode">Modbus function code.</param>
        /// <param name="exceptionCode">Modbus exception code.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ModbusException(
            FunctionCode functionCode,
            ExceptionCode exceptionCode,
            string message
        )
        : base(message)
        {
            FunctionCodeValue = functionCode;
            ExceptionCodeValue = exceptionCode;
        }

        /// <summary>
        /// Create Modbus exception object.
        /// </summary>
        /// <param name="response">Response message.</param>
        /// <returns>An exception.</returns>
        public static ModbusException Create(
            byte[] response
        )
        {
            if (
                response.Length >= MBAPHeaderLength + 2
            )
            {
                var errorCode = response[OffsetFunctionCode];
                var exceptionCode = response[OffsetData];
                if ((errorCode & ErrorCodeBit) != 0)
                {
                    var functionCode = (errorCode & ~ErrorCodeBit);
                    if (
                        Enum.IsDefined(typeof(FunctionCode), functionCode)
                        && Enum.IsDefined(typeof(ExceptionCode), exceptionCode)
                    )
                    {
                        return new ModbusException(
                            (FunctionCode)functionCode,
                            (ExceptionCode)exceptionCode,
                            $"{exceptionCode}: {functionCode}"
                        );
                    }
                }
            }

            return new ModbusException("Communication Error");
        }
    }
}

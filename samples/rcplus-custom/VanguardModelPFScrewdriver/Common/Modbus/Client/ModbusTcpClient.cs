// -----------------------------------------------------------------------
// <copyright file="ModbusTcpClient.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Net.Sockets;
using static Epson.RoboticsShared.Modbus.Client.ModbusDefinitions;

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Modbus TCP Client Implementation
    /// </summary>
    internal class ModbusTcpClient : ModbusClient, IModbusClient
    {
        /// <summary>
        /// TcpClient instance
        /// </summary>
        private readonly TcpClient _tcpClient;

        /// <summary>
        /// NetworkStream of the TcpClient instance
        /// </summary>
        private readonly NetworkStream _stream;

        /// <summary>
        /// Default timeout for read/write operations in milliseconds
        /// </summary>
        private const int _defaultTimeout = 5000;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// The TcpClient should be connected to a server.
        /// </remarks>
        /// <param name="tcpClient">TcpClient instance</param>
        /// <param name="timeout">Timeout in milliseconds. Default is 5000ms.</param>
        public ModbusTcpClient(
            TcpClient tcpClient,
            int timeout = _defaultTimeout
        )
        {
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();
            _stream.ReadTimeout = timeout;
            _stream.WriteTimeout = timeout;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _stream.Dispose();
            _tcpClient.Dispose();
        }

        /// <summary>
        /// Read registers.
        /// </summary>
        /// <param name="functionCode">Function code.</param>
        /// <param name="address">Address.</param>
        /// <param name="count">Number of registers to read. 1 to 125.</param>
        /// <param name="unitId">Unit identity.</param>
        /// <param name="cancellationToken">A token to cancel the request while it is in progress.</param>
        /// <returns>An array of register values.</returns>
        /// <exception cref="ArgumentException">Unexpected count.</exception>
        /// <exception cref="ModbusException">Modbus error.</exception>
        /// <exception cref="OperationCanceledException">The request was canceled.</exception>
        private async Task<ushort[]> ReadRegistersAsync(
            FunctionCode functionCode,
            ushort address,
            ushort count,
            byte unitId,
            CancellationToken cancellationToken = default
        )
        {
            const int _maxCountOfThisOperation = 125;

            if (count == 0 || count > _maxCountOfThisOperation)
            {
                throw new ArgumentException($"Unexpected count: {count}");
            }

            // [Request Data]
            // Starting Address: 2 Bytes
            // Quantity of Registers: 2 Bytes
            ushort[] inputData = [address, count];
            var request = CreateRequestADU(unitId, functionCode, inputData);
            await _stream.WriteAsync(request, cancellationToken);

            // [Response Data]
            // Byte Count: 1 Byte
            // Registers Value: N * 2 Bytes
            var outputDataLength = sizeof(byte) + sizeof(ushort) * count;
            var response = CreateADU(outputDataLength);
            await _stream.ReadExactlyAsync(response, cancellationToken);
            if (
                response[OffsetFunctionCode] == (byte)functionCode
                && response[OffsetData] == (byte)(sizeof(ushort) * count)
            )
            {
                return response.GetBE(OffsetData + sizeof(byte), count);
            }
            else
            {
                throw ModbusException.Create(response);
            }
        }

        /// <inheritdoc />
        public async Task<ushort[]> ReadHoldingRegistersAsync(
            ushort address,
            ushort count,
            byte unitId,
            CancellationToken cancellationToken = default
        )
        {
            return await ReadRegistersAsync(FunctionCode.ReadHoldingRegisters, address, count, unitId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ushort[]> ReadInputRegistersAsync(
            ushort address,
            ushort count,
            byte unitId,
            CancellationToken cancellationToken = default
        )
        {
            return await ReadRegistersAsync(FunctionCode.ReadInputRegisters, address, count, unitId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task WriteSingleRegisterAsync(
            ushort address,
            ushort value,
            byte unitId,
            CancellationToken cancellationToken = default
        )
        {
            // [Request Data]
            // Register Address: 2 Bytes
            // Register Value: 2 Bytes
            ushort[] inputData = [address, value];
            var request = CreateRequestADU(unitId, FunctionCode.WriteSingleRegister, inputData);
            await _stream.WriteAsync(request, cancellationToken);

            // [Response Data]
            // Register Address: 2 Bytes
            // Register Value: 2 Bytes
            var outputDataLength = ByteLength(inputData);
            var response = CreateADU(outputDataLength);
            await _stream.ReadExactlyAsync(response, cancellationToken);
            if (
                response[OffsetFunctionCode] == (byte)FunctionCode.WriteSingleRegister
                && response.GetBE(OffsetData) == address
                && response.GetBE(OffsetData + sizeof(ushort)) == value
            )
            {
                // Write operation completed successfully
            }
            else
            {
                throw ModbusException.Create(response);
            }
        }

        /// <inheritdoc />
        public async Task WriteMultipleRegistersAsync(
            ushort address,
            ushort[] values,
            byte unitId,
            CancellationToken cancellationToken = default
        )
        {
            const int _maxCountOfThisOperation = 123;

            ushort count = (ushort)values.Length;
            if (count == 0 || count > _maxCountOfThisOperation)
            {
                throw new ArgumentException($"Unexpected length of values: {count}");
            }

            // [Request Data]
            // Starting Address: 2 Bytes
            // Quantity of Registers: 2 Bytes
            // Byte Count: 1 Byte
            // Registers Value: N * 2 Bytes
            var inputDataLength = sizeof(ushort) + sizeof(ushort) + sizeof(byte) + ByteLength(values);
            var request = CreateRequestADU(unitId, FunctionCode.WriteMultipleRegisters, inputDataLength);
            var offset = OffsetData;
            address.SetBE(request, offset);             offset += sizeof(ushort);
            count.SetBE(request, offset);               offset += sizeof(ushort);
            request[offset] = (byte)ByteLength(values); offset += sizeof(byte);
            values.SetBE(request, offset);
            await _stream.WriteAsync(request, cancellationToken);

            // [Response Data]
            // Starting Address: 2 Bytes
            // Quantity of Registers: 2 Bytes
            var outputDatalength = sizeof(ushort) + sizeof(ushort);
            var response = CreateADU(outputDatalength);
            await _stream.ReadExactlyAsync(response, cancellationToken);
            if (
                response[OffsetFunctionCode] == (byte)FunctionCode.WriteMultipleRegisters
                && response.GetBE(OffsetData) == address
                && response.GetBE(OffsetData + sizeof(ushort)) == values.Length
            )
            {
                // Write operation completed successfully
            }
            else
            {
                throw ModbusException.Create(response);
            }
        }
    }
}

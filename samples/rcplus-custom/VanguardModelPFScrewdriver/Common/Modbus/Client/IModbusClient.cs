// -----------------------------------------------------------------------
// <copyright file="IModbusClient.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Modbus client interface
    /// </summary>
    public interface IModbusClient : IDisposable
    {
        /// <summary>
        /// Function code 03 (0x03)
        /// Read Holding Registers
        /// </summary>
        /// <param name="address">Starting address.</param>
        /// <param name="count">Quantity of registers. 1 to 125.</param>
        /// <param name="unitId">The Modbus unit identifier (slave ID). Default is 0.</param>
        /// <param name="cancellationToken">A token to cancel the request while it is in progress.</param>
        /// <returns>A task returns an array of register values.</returns>
        /// <exception cref="ArgumentException">Unexpected count.</exception>
        /// <exception cref="ModbusException">Modbus error.</exception>
        /// <exception cref="IOException">Communication timeout or network error.</exception>
        /// <exception cref="OperationCanceledException">The request was canceled.</exception>
        public Task<ushort[]> ReadHoldingRegistersAsync(
            ushort address,
            ushort count,
            byte unitId = 0,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Function code 04 (0x04)
        /// Read Input Registers
        /// </summary>
        /// <param name="address">Starting address.</param>
        /// <param name="count">Quantity of registers.</param>
        /// <param name="unitId">The Modbus unit identifier (slave ID). Default is 0.</param>
        /// <param name="cancellationToken">A token to cancel the request while it is in progress.</param>
        /// <returns>A task returns an array of register values.</returns>
        /// <exception cref="ArgumentException">Unexpected count.</exception>
        /// <exception cref="ModbusException">Modbus error.</exception>
        /// <exception cref="IOException">Communication timeout or network error.</exception>
        /// <exception cref="OperationCanceledException">The request was canceled.</exception>
        public Task<ushort[]> ReadInputRegistersAsync(
            ushort address,
            ushort count,
            byte unitId = 0,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Function code 06 (0x06)
        /// Write Single Register
        /// </summary>
        /// <param name="address">Register address.</param>
        /// <param name="value">Register value.</param>
        /// <param name="unitId">The Modbus unit identifier (slave ID). Default is 0.</param>
        /// <param name="cancellationToken">A token to cancel the request while it is in progress.</param>
        /// <returns>A task.</returns>
        /// <exception cref="ModbusException">Modbus error.</exception>
        /// <exception cref="IOException">Communication timeout or network error.</exception>
        /// <exception cref="OperationCanceledException">The request was canceled.</exception>
        public Task WriteSingleRegisterAsync(
            ushort address,
            ushort value,
            byte unitId = 0,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Function code 16 (0x10)
        /// Write Multiple Registers
        /// </summary>
        /// <param name="address">Starting address.</param>
        /// <param name="values">An array of register values.</param>
        /// <param name="unitId">The Modbus unit identifier (slave ID). Default is 0.</param>
        /// <param name="cancellationToken">A token to cancel the request while it is in progress.</param>
        /// <returns>A task.</returns>
        /// <exception cref="ModbusException">Modbus error.</exception>
        /// <exception cref="IOException">Communication timeout or network error.</exception>
        /// <exception cref="OperationCanceledException">The request was canceled.</exception>
        public Task WriteMultipleRegistersAsync(
            ushort address,
            ushort[] values,
            byte unitId = 0,
            CancellationToken cancellationToken = default
        );
    }
}

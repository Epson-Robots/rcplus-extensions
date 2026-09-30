// -----------------------------------------------------------------------
// <copyright file="ModbusClientFactory.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Net.Sockets;

namespace Epson.RoboticsShared.Modbus.Client
{
    /// <summary>
    /// Modbus Client Factory
    /// </summary>
    public static class ModbusClientFactory
    {
        /// <summary>
        /// Create a new instance of Modbus TCP client.
        /// </summary>
        /// <param name="tcpClient">TcpClient instance.</param>
        /// <returns>Client object.</returns>
        public static IModbusClient Create(
            TcpClient tcpClient
        )
        {
            return new ModbusTcpClient(tcpClient);
        }
    }
}
